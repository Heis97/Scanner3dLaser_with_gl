using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;

namespace opengl3
{
    public struct Polygon3d_GL
    {
        public Point3d_GL[] ps;
        public Flat3d_GL flat3D;
        public Vector3d_GL v1, v2, v3;
        int special_point_ind;
        public double dim;
        public Point3d_GL centr;

        public Polygon3d_GL(Point3d_GL P1, Point3d_GL P2, Point3d_GL P3, int _special_point_ind = 0,bool nz_positive = false)
        {
            special_point_ind = _special_point_ind;
            ps = new Point3d_GL[] { P1, P2, P3 };
            v1 = new Vector3d_GL(P1, P2).normalize();
            v2 = new Vector3d_GL(P1, P3).normalize();
            v3 = v1 | v2;//vector multiply
            v3.normalize();
            centr = (P1 + P2 + P3) / 3;
            if(nz_positive && v3.z<0)
            {
                v1 = -v1;
                v2 = -v2;
                v3 = -v3;
                
            }

            dim = Math.Max(Math.Max((P3 - P1).magnitude(), (P2 - P1).magnitude()), (P3 - P2).magnitude());
            //Console.WriteLine(v3);
            
            flat3D = new Flat3d_GL(v3.x, v3.y, v3.z, -v3 * P1);

        }
        static public Polygon3d_GL from_ps(Point3d_GL[] ps)
        {
            if (ps == null) return new Polygon3d_GL();
            if (ps.Length > 2)
            {
                return new Polygon3d_GL(ps[0], ps[1], ps[2]);
            }
            else return new Polygon3d_GL();

        }

        static public Polygon3d_GL[] multMatr(Polygon3d_GL[] pols, Matrix<double> matrix)
        {
            var pols_mul = new Polygon3d_GL[pols.Length];
            for (int i = 0; i < pols.Length; i++)
            {
                var ps = Point3d_GL.multMatr(pols[i].ps, matrix);
                pols_mul[i] = new Polygon3d_GL(ps[0], ps[1], ps[2]);
            }
            return pols_mul;
        }

        public Polygon3d_GL set_color(Color3d_GL color)
        {
            ps[0].color = color;
            ps[1].color = color;
            ps[2].color = color;
            return this;
        }

        public bool affilationPoint_xy(Point3d_GL p)
        {
            if (ps.Length < 3)
            {
                return false;
            }

            p = p - ps[0];
            var b = ps[1] - ps[0];
            var c = ps[2] - ps[0];

            if (b.x == 0)
            {
                var lam = b.Clone();
                b = c.Clone();
                c = lam.Clone();
            }
            if ((c.x * b.y - b.x * c.y) == 0) return false;
            var m = (p.x * b.y - b.x * p.y) / (c.x * b.y - b.x * c.y);

            if (m >= 0 && m <= 1)
            {
                var l = (p.x - m * c.x) / b.x;
                if (l >= 0 && m + l <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        public Point3d_GL crossLine(Line3d_GL p1)
        {
            var p_cross = p1.calcCrossFlat(flat3D);
            var v_c = new Vector3d_GL(ps[special_point_ind], p_cross);
            var a1 = v1 ^ v_c;
            var a2 = v2 ^ v_c;
            var b1 = v1 ^ v2;

            if (a1 <= b1 && a2 <= b1)
            {
                return p_cross;
            }
            return Point3d_GL.notExistP();
        }
        public Point3d_GL crossLine_deb(Line3d_GL p1)
        {
            var p_cross = p1.calcCrossFlat(flat3D);
            var v_c = new Vector3d_GL(ps[special_point_ind], p_cross);
            var a1 = v1 ^ v_c;
            var a2 = v2 ^ v_c;
            var b1 = v1 ^ v2;

            if (a1 <= b1 && a2 <= b1)
            {
                return new Point3d_GL(v_c.x, v_c.y, v_c.z);
            }
            return new Point3d_GL(v_c.x, v_c.y, v_c.z);
        }

        static public Point3d_GL[] createLightFlat(Polygon3d_GL[] polygons, Line3d_GL[] lines)
        {
            var ps_laser = new List<Point3d_GL>();
            int last_i = 0;
            for (int line_i = 0; line_i < lines.Length; line_i++)
            {
                for (int polygon_i = last_i; polygon_i < polygons.Length; polygon_i++)
                {
                    var p = polygons[polygon_i].crossLine(lines[line_i]);
                    if (p.exist)
                    {
                        ps_laser.Add(p);
                        //  last_i = polygon_i;
                    }
                }
            }
            //Console.WriteLine("intersect " + polygons.Length +" polyg and " + lines.Length + " lines: " + ps_laser.Count + " points");
            return ps_laser.ToArray();
        }

        static Point3d_GL[] sortByX(Point3d_GL[] ps)
        {
            var ps_sort = from p in ps
                          orderby p.x descending
                          select p;
            return ps_sort.ToArray();
        }
        static Point3d_GL[] sortByY(Point3d_GL[] ps)
        {
            var ps_sort = from p in ps
                          orderby p.y descending
                          select p;
            return ps_sort.ToArray();
        }
        static public Polygon3d_GL[] triangulate_two_lines_xy(Point3d_GL[] ps1, Point3d_GL[] ps2)
        {
            if (ps1 == null || ps2 == null) return null;
            if (ps1.Length < 2 || ps1.Length < 2) return null;
            var ps_or = Point3d_GL.order_points(ps1);
            var dp = ps_or[ps_or.Length - 1] - ps_or[0];
            Ax ax;
            if (Math.Abs(dp.x) > Math.Abs(dp.y)) ax = Ax.X;
            else ax = Ax.Y;

            if (ax == Ax.X)
            {
                ps1 = sortByX(ps1);
                ps2 = sortByX(ps2);
            }
            else
            {
                ps1 = sortByY(ps1);
                ps2 = sortByY(ps2);
            }

            var polygons = new List<Polygon3d_GL>();
            //var polygons_ind = new List<int[]>();
            int ind_2 = 0;
            //int ind_2_last = ind_2;
            List<int>[] ps1_connect = new List<int>[ps1.Length];
            List<int>[] ps2_connect = new List<int>[ps2.Length];
            for (int i = 1; i < ps1.Length; i++)
            {
                polygons.Add(new Polygon3d_GL(ps1[i - 1], ps1[i], ps2[ind_2]));
                //polygons_ind.Add(new int[] { i - 1, i, ind_2, 1 });
                if (ps1_connect[i - 1] == null)
                {
                    ps1_connect[i - 1] = new List<int>();
                }
                if (ps1_connect[i] == null)
                {
                    ps1_connect[i] = new List<int>();
                }

                if (ps2_connect[ind_2] == null)
                {
                    ps2_connect[ind_2] = new List<int>();
                }
                ps1_connect[i - 1].Add(ind_2); ps1_connect[i].Add(ind_2);

                ps2_connect[ind_2].Add(i - 1); ps2_connect[ind_2].Add(i);

                if (i < ps1.Length - 1)
                {
                    var min_dist = double.MaxValue;
                    for (int j = 0; j < ps2.Length; j++)
                    {
                        var dist = (ps1[i] - ps2[j]).magnitude_ax(ax);
                        if (dist < min_dist)
                        {
                            min_dist = dist;
                            ind_2 = j;
                        }
                    }
                }
                /* if(ind_2<ind_2_last)
                 {
                     Console.WriteLine("ind_2<ind_2_last");
                     Console.WriteLine(ind_2);
                 }*/
                //ind_2_last = ind_2;
            }

            for (int i = 1; i < ps2_connect.Length; i++)
            {
                if (ps2_connect[i] == null)
                {
                    ps2_connect[i] = new List<int>();
                    ps2_connect[i].Add(ps2_connect[i - 1][ps2_connect[i - 1].Count - 1]);

                }
            }

            for (int i = 1; i < ps2_connect.Length; i++)
            {
                for (int j = 0; j < ps1.Length; j++)
                {

                    if (ps2_connect[i - 1].Contains(j) && ps2_connect[i].Contains(j))
                    {
                        polygons.Add(new Polygon3d_GL(ps2[i], ps2[i - 1], ps1[j]));
                        //polygons_ind.Add(new int[] { i, i - 1, j, 2 });
                    }

                }
            }

            //Console.WriteLine(polygons_ind);

            return polygons.ToArray();
        }



        static public Point3d_GL[][] smooth_lines_xy(Point3d_GL[][] ps, double smooth)
        {
            if (ps == null) return null;
            if (ps.Length == 0 || ps.Length == 1) return null;

            double map_resol = 0.02;
            var p_minmax = lines_minmax(ps);
            var p_min = p_minmax[0]; var p_max = p_minmax[1];
            int smooth_rad = (int)(smooth / map_resol);
            var p_len = (p_max - p_min) / map_resol + new Point3d_GL(2 * smooth_rad + 1, 2 * smooth_rad + 1, 2);//comp_round
            var x_len = (int)p_len.x;
            var y_len = (int)p_len.y;

            var map_xy = new int[x_len, y_len][][];
            for (int i = 0; i < ps.Length; i++)
            {
                var x_max = 0;
                var y_max = 0;
                if (ps[i] != null)
                {
                    for (int j = 0; j < ps[i].Length; j++)
                    {
                        var p_cur = (ps[i][j] - p_min) / map_resol;

                        var x = (int)p_cur.x;
                        var y = (int)p_cur.y;
                        if (x > x_max) x_max = x;
                        if (y > y_max) y_max = y;
                        //Console.WriteLine(x + "  "+y+" " + map_xy.GetLength(0)+" "+ map_xy.GetLength(1));
                        //Console.WriteLine(ps[i][j] + "  " + p_min + " " + p_max);
                        if (map_xy[x, y] == null)
                        {
                            map_xy[x, y] = new int[0][];
                        }
                        var map_cur = map_xy[x, y];
                        var list = map_cur.ToList();
                        list.Add(new int[] { i, j });
                        map_xy[x, y] = list.ToArray();
                    }
                }

                //Console.WriteLine(x_max+ " "+y_max);
            }
            // Console.WriteLine(map_xy.GetLength(0) + " " + map_xy.GetLength(1));

            Point3d_GL[][] ps_smooth = (Point3d_GL[][])ps.Clone();
            for (int i = 0; i < ps.Length; i++)
            {
                ps_smooth[i] = (Point3d_GL[])ps[i].Clone();
            }


            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i] != null)
                {
                    for (int j = 0; j < ps[i].Length; j++)
                    {
                        var p_cur = (ps[i][j] - p_min) / map_resol;
                        var x = (int)p_cur.x;
                        var y = (int)p_cur.y;
                        ps_smooth[i][j] = comp_map_in_rad(map_xy, smooth_rad, ps, x, y, ps[i][j]);
                    }
                }
            }

            return ps_smooth;
        }

        static Point3d_GL comp_map_in_rad(int[,][][] map, int smooth_rad, Point3d_GL[][] ps, int x, int y, Point3d_GL p)
        {
            var p_sm = p.Copy();
            int p_count = 1;
            if (x < smooth_rad || y < smooth_rad)
            {
                return p_sm;
            }
            for (int x_cur = -smooth_rad + x; x_cur < x + smooth_rad; x_cur++)
            {
                for (int y_cur = -smooth_rad + y; y_cur < y + smooth_rad; y_cur++)
                {
                    if (map[x_cur, y_cur] != null)
                    {
                        for (int k = 0; k < map[x_cur, y_cur].Length; k++)
                        {
                            if (map[x_cur, y_cur][k] != null)
                            {
                                if (map[x_cur, y_cur][k].Length > 1)
                                {
                                    var i = map[x_cur, y_cur][k][0];
                                    var j = map[x_cur, y_cur][k][1];
                                    p_sm += ps[i][j];
                                    p_count++;
                                }
                            }

                        }
                    }
                }
            }
            return p_sm / p_count;
        }

        static Point3d_GL[] lines_minmax(Point3d_GL[][] ps)
        {
            var p_min = new Point3d_GL(double.MaxValue, double.MaxValue, double.MaxValue);
            var p_max = new Point3d_GL(double.MinValue, double.MinValue, double.MinValue);
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i] != null)
                {
                    for (int j = 0; j < ps[i].Length; j++)
                    {
                        if (p_min.x > ps[i][j].x)
                        {
                            p_min.x = ps[i][j].x;
                        }
                        if (p_min.y > ps[i][j].y)
                        {
                            p_min.y = ps[i][j].y;
                        }
                        if (p_min.z > ps[i][j].z)
                        {
                            p_min.z = ps[i][j].z;
                        }

                        if (p_max.x < ps[i][j].x)
                        {
                            p_max.x = ps[i][j].x;
                        }
                        if (p_max.y < ps[i][j].y)
                        {
                            p_max.y = ps[i][j].y;
                        }
                        if (p_max.z < ps[i][j].z)
                        {
                            p_max.z = ps[i][j].z;
                        }
                    }
                }
            }
            return new Point3d_GL[] { p_min, p_max };
        }

        static public Polygon3d_GL[] triangulate_lines_xy(Point3d_GL[][] ps, double smooth = -1)
        {
            List<Polygon3d_GL> polygons = new List<Polygon3d_GL>();
            if (smooth > 0) ps = smooth_lines_xy(ps, smooth);
            if (ps == null) return null;

            var ps_f = new List<Point3d_GL[]>();
            for (int i = 1; i < ps.Length; i++)
            {
                if (ps[i].Length > 0) ps_f.Add(ps[i]);
            }
            ps = ps_f.ToArray();
            for (int i = 1; i < ps.Length; i++)
            {
                var line = triangulate_two_lines_xy(ps[i - 1], ps[i]);
                if (line == null) continue;
                polygons.AddRange(line);
            }
            return polygons.ToArray();
        }

        /// <summary>
        /// mesh,color,normal
        /// </summary>
        /// <param name="polygons"></param>
        /// <returns></returns>
        static public float[][] toMesh(Polygon3d_GL[] polygons)
        {
            if (polygons == null) return null;
            var mesh = new List<float>();
            var color = new List<float>();
            var normal = new List<float>();
            for (int i = 0; i < polygons.Length; i++)
            {
                if (polygons[i].ps.Length > 2)
                {
                    var mesh_sub = new float[9];
                    mesh_sub[0] = (float)polygons[i].ps[0].x;
                    mesh_sub[1] = (float)polygons[i].ps[0].y;
                    mesh_sub[2] = (float)polygons[i].ps[0].z;

                    mesh_sub[3] = (float)polygons[i].ps[1].x;
                    mesh_sub[4] = (float)polygons[i].ps[1].y;
                    mesh_sub[5] = (float)polygons[i].ps[1].z;

                    mesh_sub[6] = (float)polygons[i].ps[2].x;
                    mesh_sub[7] = (float)polygons[i].ps[2].y;
                    mesh_sub[8] = (float)polygons[i].ps[2].z;
                    mesh.AddRange(mesh_sub);


                    var color_sub = new float[9];
                    if (polygons[i].ps[0].color != null && polygons[i].ps[1].color != null && polygons[i].ps[2].color != null)
                    {
                        color_sub[0] = (float)polygons[i].ps[0].color.r;
                        color_sub[1] = (float)polygons[i].ps[0].color.g;
                        color_sub[2] = (float)polygons[i].ps[0].color.b;

                        color_sub[3] = (float)polygons[i].ps[1].color.r;
                        color_sub[4] = (float)polygons[i].ps[1].color.g;
                        color_sub[5] = (float)polygons[i].ps[1].color.b;

                        color_sub[6] = (float)polygons[i].ps[2].color.r;
                        color_sub[7] = (float)polygons[i].ps[2].color.g;
                        color_sub[8] = (float)polygons[i].ps[2].color.b;
                    }

                    color.AddRange(color_sub);

                    var normal_sub = new float[9];
                    normal_sub[0] = (float)polygons[i].flat3D.A;
                    normal_sub[1] = (float)polygons[i].flat3D.B;
                    normal_sub[2] = (float)polygons[i].flat3D.C;

                    normal_sub[3] = (float)polygons[i].flat3D.A;
                    normal_sub[4] = (float)polygons[i].flat3D.B;
                    normal_sub[5] = (float)polygons[i].flat3D.C;

                    normal_sub[6] = (float)polygons[i].flat3D.A;
                    normal_sub[7] = (float)polygons[i].flat3D.B;
                    normal_sub[8] = (float)polygons[i].flat3D.C;
                    normal.AddRange(normal_sub);
                }
            }
            return new float[][] { mesh.ToArray(), color.ToArray(), normal.ToArray() };
        }

        static public Polygon3d_GL[] polygs_from_mesh(float[] mesh, float[] color = null)
        {
            if (mesh.Length % 9 != 0) return null;
            List<Polygon3d_GL> polygs = new List<Polygon3d_GL>();
            for (int i = 0; i < mesh.Length; i += 9)
            {
                var p1 = new Point3d_GL(mesh[i], mesh[i + 1], mesh[i + 2]);
                var p2 = new Point3d_GL(mesh[i + 3], mesh[i + 4], mesh[i + 5]);
                var p3 = new Point3d_GL(mesh[i + 6], mesh[i + 7], mesh[i + 8]);
                if (color != null)
                {
                    p1.color = new Color3d_GL(color[i], color[i + 1], color[i + 2]);
                    p2.color = new Color3d_GL(color[i + 3], color[i + 4], color[i + 5]);
                    p3.color = new Color3d_GL(color[i + 6], color[i + 7], color[i + 8]);
                }
                polygs.Add(new Polygon3d_GL(p1, p2, p3));
            }
            return polygs.ToArray();
        }

        static public Polygon3d_GL[] triangulate_two_same_conts(Point3d_GL[] ps1, Point3d_GL[] ps2)
        {
            if (ps1.Length != ps2.Length) return null;
            var pols = new List<Polygon3d_GL>();
            for (int i = 0; i < ps1.Length; i++)
            {
                var ind = i - 1;
                if (i == 0) ind = ps1.Length - 1;
                pols.Add(new Polygon3d_GL(ps1[ind], ps2[i], ps2[ind]));
                pols.Add(new Polygon3d_GL(ps1[ind], ps1[i], ps2[i]));
            }
            return pols.ToArray();
        }

        public Point3d_GL[] get_dimens_minmax()
        {
            var p_min = new Point3d_GL(double.MaxValue, double.MaxValue, double.MaxValue);
            var p_max = new Point3d_GL(double.MinValue, double.MinValue, double.MinValue);
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].x < p_min.x) p_min.x = ps[i].x;
                if (ps[i].y < p_min.y) p_min.y = ps[i].y;
                if (ps[i].z < p_min.z) p_min.z = ps[i].z;

                if (ps[i].x > p_max.x) p_max.x = ps[i].x;
                if (ps[i].y > p_max.y) p_max.y = ps[i].y;
                if (ps[i].z > p_max.z) p_max.z = ps[i].z;
            }
            return new Point3d_GL[] { p_min, p_max };
        }
        static public Point3d_GL[] get_dimens_minmax_arr_full(Polygon3d_GL[] polygons)
        {
            var p_min = new Point3d_GL(double.MaxValue, double.MaxValue, double.MaxValue);
            var p_max = new Point3d_GL(double.MinValue, double.MinValue, double.MinValue);

            for (int j = 0; j < polygons.Length; j++)
            {

                for (int i = 0; i < polygons[j].ps.Length; i++)
                {
                    if (polygons[j].ps[i].x < p_min.x) p_min.x = polygons[j].ps[i].x;
                    if (polygons[j].ps[i].y < p_min.y) p_min.y = polygons[j].ps[i].y;
                    if (polygons[j].ps[i].z < p_min.z) p_min.z = polygons[j].ps[i].z;

                    if (polygons[j].ps[i].x > p_max.x) p_max.x = polygons[j].ps[i].x;
                    if (polygons[j].ps[i].y > p_max.y) p_max.y = polygons[j].ps[i].y;
                    if (polygons[j].ps[i].z > p_max.z) p_max.z = polygons[j].ps[i].z;
                }

            }
            return new Point3d_GL[] { p_min, p_max };
        }
        static public Point3d_GL[] get_dimens_minmax_arr(Polygon3d_GL[] polygons)
        {
            var p_min = new Point3d_GL(double.MaxValue, double.MaxValue, double.MaxValue);
            var p_max = new Point3d_GL(double.MinValue, double.MinValue, double.MinValue);

            for (int i = 0; i < polygons.Length; i++)
            {

                if (polygons[i].ps[0].x < p_min.x) p_min.x = polygons[i].ps[0].x;
                if (polygons[i].ps[0].y < p_min.y) p_min.y = polygons[i].ps[0].y;
                if (polygons[i].ps[0].z < p_min.z) p_min.z = polygons[i].ps[0].z;

                if (polygons[i].ps[0].x > p_max.x) p_max.x = polygons[i].ps[0].x;
                if (polygons[i].ps[0].y > p_max.y) p_max.y = polygons[i].ps[0].y;
                if (polygons[i].ps[0].z > p_max.z) p_max.z = polygons[i].ps[0].z;

            }
            return new Point3d_GL[] { p_min, p_max };
        }

        public Point3d_GL project_point_xy(Point3d_GL p)
        {
            if (flat3D.C == 0)
            {
                return new Point3d_GL(p.x, p.y, 0);
            }
            var z = (-flat3D.D - flat3D.A * p.x - flat3D.B * p.y) / flat3D.C;
            return new Point3d_GL(p.x, p.y, z);
        }

        static public double aver_dim(Polygon3d_GL[][] polygons)
        {
            int len = 0;
            double dim_all = 0;

            for (int i = 0; i < polygons.Length; i++)
            {
                for (int j = 0; j < polygons[i].Length; j++)
                {
                    dim_all += polygons[i][j].dim;
                }
                len += polygons[i].Length;
            }
            return dim_all / len;
        }


        public static Point3d_GL cross_line_triang(Polygon3d_GL polygon, Line3d_GL line)
        {
            var p_cross = line.calcCrossFlat(polygon.flat3D);
            var ps_in1 = ps_in_triang_3d(polygon, p_cross);
            if (ps_in1)
                return p_cross;
            else
                return Point3d_GL.notExistP();
        }

        public static Point3d_GL cross_line_triang_2(Polygon3d_GL polygon1, Polygon3d_GL polygon2, Line3d_GL line)
        {
            var p_cross = line.calcCrossFlat(polygon1.flat3D);
            var ps_in1 = ps_in_triang_3d(polygon1, p_cross);
            var ps_in2 = ps_in_triang_3d(polygon2, p_cross);
            if (ps_in1 && ps_in2)
                return p_cross;
            else
                return Point3d_GL.notExistP();
        }

        public static bool ps_in_triang_3d(Polygon3d_GL polygon, Point3d_GL p)
        {
            var v_c = p - polygon.ps[2];
            var a1 = (polygon.ps[0] - polygon.ps[2]) ^ v_c;
            var a2 = (polygon.ps[1] - polygon.ps[2]) ^ v_c;
            var b1 = (polygon.ps[0] - polygon.ps[2]) ^ (polygon.ps[1] - polygon.ps[2]);
            if (a1 < b1 || a2 < b1)
            {
                return false;
            }
            v_c = p - polygon.ps[1];
            a1 = (polygon.ps[0] - polygon.ps[1]) ^ v_c;
            a2 = (polygon.ps[2] - polygon.ps[1]) ^ v_c;
            b1 = (polygon.ps[0] - polygon.ps[1]) ^ (polygon.ps[2] - polygon.ps[1]);
            if (a1 < b1 || a2 < b1)
            {
                return false;
            }
            return true;
        }
        public static Point3d_GL cross_line_triang_arc(Polygon3d_GL polygon, Line3d_GL line)
        {
            var p_cross = line.calcCrossFlat(polygon.flat3D);
            var v_c = p_cross - polygon.ps[2];
            var v1 = (polygon.ps[0] - polygon.ps[2]);
            var v2 = (polygon.ps[1] - polygon.ps[2]);
            var a1 = v1 ^ v_c;
            var a2 = v2 ^ v_c;
            var b1 = v1 ^ v2;
            if (a1 < b1 || a2 < b1 || a1 == double.NaN || a2 == double.NaN || b1 == double.NaN)
            {
                return Point3d_GL.notExistP();
            }
            v_c = p_cross - polygon.ps[1];
            v1 = (polygon.ps[0] - polygon.ps[1]);
            v2 = (polygon.ps[2] - polygon.ps[1]);
            a1 = v1 ^ v_c;
            a2 = v2 ^ v_c;
            b1 = v1 ^ v2;
            if (a1 < b1 || a2 < b1 || a1 == double.NaN || a2 == double.NaN || b1 == double.NaN)
            {
                return Point3d_GL.notExistP();
            }
            return p_cross;
        }
        public static Point3d_GL[] cross_triang(Polygon3d_GL pn1, Polygon3d_GL pn2)
        {
            int ind = 0;
            var ps1 = new List<Point3d_GL>();
            Point3d_GL[] ps2 = new Point3d_GL[6];

            ps2[0] = cross_line_triang_2(pn1, pn2, new Line3d_GL(pn2.ps[0], pn2.ps[1]));
            ps2[1] = cross_line_triang_2(pn1, pn2, new Line3d_GL(pn2.ps[1], pn2.ps[2]));
            ps2[2] = cross_line_triang_2(pn1, pn2, new Line3d_GL(pn2.ps[2], pn2.ps[0]));

            ps2[3] = cross_line_triang_2(pn2, pn1, new Line3d_GL(pn1.ps[0], pn1.ps[1]));
            ps2[4] = cross_line_triang_2(pn2, pn1, new Line3d_GL(pn1.ps[1], pn1.ps[2]));
            ps2[5] = cross_line_triang_2(pn2, pn1, new Line3d_GL(pn1.ps[2], pn1.ps[0]));

            for (int i = 0; i < 6; i++)
            {
                if (ps2[i].exist)
                {
                    ps1.Add(ps2[i]);
                    ind++;
                }
            }
            //Console.WriteLine(ps1.Count);
            return ps1.ToArray();
        }


        public static Point3d_GL[] get_uniq_points(Polygon3d_GL[] pn)
        {
            var ind_mesh = new IndexedMesh(pn);
            return ind_mesh.ps_uniq;
        }
        public static Point3d_GL[] get_points(Polygon3d_GL[] pn)
        {
            var ps = new List<Point3d_GL>();
            for (int i = 0; i < pn.Length; i++)
            {
                ps.AddRange(pn[i].ps);
            }
            return ps.ToArray();
        }

        public static Point3d_GL[] get_centres(Polygon3d_GL[] pn)
        {
            var ps = new List<Point3d_GL>();
            for (int i = 0; i < pn.Length; i++)
            {
                ps.Add(pn[i].centr);
            }
            return ps.ToArray();
        }
        public static Polygon3d_GL[] from_points(Point3d_GL[] ps1)
        {
            return null;
        }


        static public Polygon3d_GL[] merge_surfs(Polygon3d_GL[][] surfs)
        {


            return null;
        }

        public Polygon3d_GL Clone( bool nz_positive = false)
        {
            return new Polygon3d_GL(ps[0].Clone(), ps[1].Clone(), ps[2].Clone(), special_point_ind,nz_positive);
        }

        static public Polygon3d_GL[] Clone_surf(Polygon3d_GL[] surf, bool nz_positive = false)
        {
            var surf_clone = new Polygon3d_GL[surf.Length];
            for(int i=0; i<surf.Length;i++)
            {
                surf_clone[i] = surf[i].Clone(nz_positive);
            }
            return surf_clone;
        }

        //public
        public static Polygon3d_GL[] add_arr(Polygon3d_GL[] pol, Point3d_GL p)
        {
            var pols = new List<Polygon3d_GL>();
            for (int i = 0; i < pol.Length; i++)
            {
                pols.Add(pol[i] + p);
            }
            return pols.ToArray();
        }
        public static Polygon3d_GL operator -(Polygon3d_GL pol, Point3d_GL p)
        {
            var p1 = pol.ps[0] - p;
            var p2 = pol.ps[1] - p;
            var p3 = pol.ps[2] - p;
            return new Polygon3d_GL(p1, p2, p3);
        }
        public static Polygon3d_GL operator +(Polygon3d_GL pol, Point3d_GL p)
        {
            var p1 = pol.ps[0] + p;
            var p2 = pol.ps[1] + p;
            var p3 = pol.ps[2] + p;
            return new Polygon3d_GL(p1, p2, p3);
        }
        public static Polygon3d_GL operator *(Polygon3d_GL pol, double k)
        {
            var p1 = pol.ps[0] * k;
            var p2 = pol.ps[1] * k;
            var p3 = pol.ps[2] * k;
            return new Polygon3d_GL(p1, p2, p3);
        }
        public static Polygon3d_GL operator /(Polygon3d_GL pol, double k)
        {
            var p1 = pol.ps[0] / k;
            var p2 = pol.ps[1] / k;
            var p3 = pol.ps[2] / k;
            return new Polygon3d_GL(p1, p2, p3);
        }
    }



    public class IndexedMesh
    {
        public Point3d_GL[] ps_uniq;
        public int[] inds_uniq;
        int[][] check_arr;
        int[][] pols_inds;
        List<int>[] ps_on_triang;
        int triangles_on_board;
        public IndexedMesh(Polygon3d_GL[] pns)
        {
            triangles_on_board = 0;
            var ps = new List<Point3d_GL>();
            var pn_l = new List<int>();
            for (int i = 0; i < pns.Length; i++)
            {
                ps.AddRange(pns[i].ps);
            }

            var map_ps = new RasterMap(ps.ToArray(), -1, RasterMap.type_map.XYZ);

            var inds = new int[ps.Count];
            var inds_ch = new bool[ps.Count];
            for (int i = 0; i < ps.Count; i++)
            {
                inds[i] = i;
                inds_ch[i] = false;
            }


            var ps_uniq = new List<Point3d_GL>();
            int ind_cur = 0;
            /*for (int i = 0; i < ps.Count; i++)
            {
                int ind_ref = 0;
                for (int j = i; j < ps.Count; j++)
                {                              
                    if(i!=j)
                        if ((ps[j] - ps[i]).magnitude() < 0.001 && inds_ch[j] != true)
                        {
                            inds[j] = ind_cur;
                            inds_ch[j] = true;
                            ind_ref++;
                        }                                                         
                }
                if(ind_ref>0)
                {
                    ps_uniq.Add(ps[i]);
                    inds[i] = ind_cur;
                    ind_cur++;
                    inds_ch[i] = true;
                    
                }
                Console.WriteLine("remesh: " + i + "/" + ps.Count);
            }*/

            for (int i = 0; i < ps.Count; i++)
            {
                int ind_ref = 0;

                //-------------------------------------

                var inds_loc = map_ps.get_local_ps(ps[i]);


                for (int k = 0; k < inds_loc.Length; k++)
                {
                    int j = inds_loc[k];
                    if (i != j)
                        if ((ps[j] - ps[i]).magnitude() < 0.001 && inds_ch[j] != true)
                        {
                            inds[j] = ind_cur;
                            inds_ch[j] = true;
                            ind_ref++;
                        }
                }


                //-------------------------------------
                if (ind_ref > 0)
                {
                    ps_uniq.Add(ps[i]);
                    inds[i] = ind_cur;
                    ind_cur++;
                    inds_ch[i] = true;

                }
                //Console.WriteLine("remesh: " + i + "/" + ps.Count);
            }
            Console.WriteLine(ps_uniq.Count);
            for (int i = 0; i < inds_ch.Length; i++)
            {
                if (!inds_ch[i])
                {
                    inds[i] = ind_cur;
                    ps_uniq.Add(ps[i]);

                    ind_cur++;
                }
            }

            this.ps_uniq = ps_uniq.ToArray();
            this.inds_uniq = inds.ToArray();
            Console.WriteLine(ps_uniq.Count);
            Console.WriteLine(inds.Length);
        }
        public Polygon3d_GL[] get_polygs()
        {
            var pns = new List<Polygon3d_GL>();
            for (int i = 0; i < inds_uniq.Length; i += 3)
            {
                pns.Add(new Polygon3d_GL(ps_uniq[inds_uniq[i]], ps_uniq[inds_uniq[i + 1]], ps_uniq[inds_uniq[i + 2]]));
            }
            return pns.ToArray();
        }
        int[][] get_poligs_inds()
        {
            var pns = new List<int[]>();
            for (int i = 0; i < inds_uniq.Length / 3; i++)
            {
                var pol = new int[3] { inds_uniq[3 * i], inds_uniq[3 * i + 1], inds_uniq[3 * i + 2] };
                pns.Add(pol);
            }

            return pns.ToArray();
        }
        List<int>[] get_points_triang_inds()
        {
            var points_on_triang = new List<int>[ps_uniq.Length];
            for (int i = 0; i < inds_uniq.Length / 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    var k = 3 * i + j;
                    if (points_on_triang[inds_uniq[k]] == null) points_on_triang[inds_uniq[k]] = new List<int>();
                    points_on_triang[inds_uniq[k]].Add(i);
                }
            }


            return points_on_triang;
        }

        public Point3d_GL[][] points_on_board()
        {
            pols_inds = get_poligs_inds();
            ps_on_triang = get_points_triang_inds();
            check_arr = check_board_all_poligs();

            var board_conts = get_ps_on_board();

            var pss = new List<Point3d_GL[]>();
            for (int i = 0; i < board_conts.Length; i ++)
            {
                if (board_conts[i]!=null)
                {
                    var ps = new List<Point3d_GL>();
                    for (int j = 0; j < board_conts[i].Length; j++)
                    {
                        ps.Add(ps_uniq[board_conts[i][j]]);
                    }
                    pss.Add(ps.ToArray());
                }
                
            }

            return pss.ToArray();
        }
        public int[][] triangs_on_board()
        {
            pols_inds = get_poligs_inds();
            ps_on_triang = get_points_triang_inds();
            check_arr = check_board_all_poligs();
            var board_conts = get_triangs_on_board();

            return board_conts.ToArray();
        }

        public Point3d_GL[][] normals_on_board()//normals
        {
            pols_inds = get_poligs_inds();
            ps_on_triang = get_points_triang_inds();
            check_arr = check_board_all_poligs();
            var pols = get_polygs();
            var board_conts = get_triangs_on_board();

            var pss = new List<Point3d_GL[]>();
            for (int i = 0; i < board_conts.Length; i++)
            {
                if (board_conts[i] != null)
                {
                    var ps = new List<Point3d_GL>();
                    for (int j = 0; j < board_conts[i].Length; j++)
                    {
                        ps.Add(pols[board_conts[i][j]].flat3D.n.toPoint());
                    }
                    pss.Add(ps.ToArray());
                }

            }

            return pss.ToArray();
        }
        int get_first_pol_on_board(int[][] check_arr)
        {
            for (int i = 0; i < check_arr.Length; i++)
            {
                if (check_arr[i] != null) return i;
            }
            return -1;
        }
        int[][] get_ps_on_board()
        {
            var board_p = new List<int>();
            var board_ps = new List<int[]>();
            var cur_ind = -1;
            int first = get_first_pol_on_board(check_arr);
            int cur_triang = first;
            var ps_c = check_arr[cur_triang];
            board_p.Add(ps_c[1]);
            board_p.Add(ps_c[0]);
            check_arr[cur_triang] = null;
            triangles_on_board--;
            while (triangles_on_board>0)
            {
                cur_ind = board_p[board_p.Count - 1];
                var triang = ps_on_triang[cur_ind];  
                var ps1 = new int[] { };
                for (int i= 0; i < triang.Count ; i++)
                {
                    ps1 = check_arr[triang[i]];
                     
                    if (ps1 != null)
                    {
                        cur_triang = triang[i];
                        break; 
                    } 
                };
                if(ps1!=null)
                {
                    if (ps1[0] == cur_ind) cur_ind = ps1[1];
                    else cur_ind = ps1[0];
                    board_p.Add(cur_ind);
                    check_arr[cur_triang] = null;
                    triangles_on_board--;
                }              
                if ((board_p.Count > 2 && cur_triang == first) || ps1 == null || triangles_on_board==0)
                {
                    board_ps.Add(board_p.ToArray());
                    board_p = new List<int>();
                    first = get_first_pol_on_board(check_arr);
                    if(first>0)
                    {
                        cur_triang = first;
                        ps_c = check_arr[cur_triang];  
                        board_p.Add(ps_c[1]);
                        board_p.Add(ps_c[0]);
                        check_arr[cur_triang] = null;
                        triangles_on_board--;
                    }
                    
                }
            }
            return board_ps.ToArray();
        }

        int[][] get_triangs_on_board()
        {
            var board_t = new List<int>();
            var board_ts = new List<int[]>();
            var board_p = new List<int>();
            var board_ps = new List<int[]>();
            var cur_ind = -1;
            int first = get_first_pol_on_board(check_arr);
            int cur_triang = first;
            var ps_c = check_arr[cur_triang];
            board_p.Add(ps_c[1]);
            board_p.Add(ps_c[0]);
            board_t.Add(cur_triang);
            check_arr[cur_triang] = null;
            triangles_on_board--;
            while (triangles_on_board > 0)
            {
                cur_ind = board_p[board_p.Count - 1];
                var triang = ps_on_triang[cur_ind];
                var ps1 = new int[] { };
                for (int i = 0; i < triang.Count; i++)
                {
                    ps1 = check_arr[triang[i]];

                    if (ps1 != null)
                    {
                        cur_triang = triang[i];
                        break;
                    }
                };
                if (ps1 != null)
                {
                    if (ps1[0] == cur_ind) cur_ind = ps1[1];
                    else cur_ind = ps1[0];
                    board_p.Add(cur_ind);
                    board_t.Add(cur_triang);
                    check_arr[cur_triang] = null;
                    triangles_on_board--;
                }
                if ((board_p.Count > 2 && cur_triang == first) || ps1 == null || triangles_on_board == 0)
                {
                    board_ps.Add(board_p.ToArray());
                    board_p = new List<int>();
                    board_ts.Add(board_t.ToArray());
                    board_t = new List<int>();
                    first = get_first_pol_on_board(check_arr);
                    if (first > 0)
                    {
                        cur_triang = first;
                        ps_c = check_arr[cur_triang];
                        board_p.Add(ps_c[1]);
                        board_p.Add(ps_c[0]);
                        board_t.Add(cur_triang);
                        check_arr[cur_triang] = null;
                        triangles_on_board--;
                    }

                }
            }
            return board_ts.ToArray();
        }

        int[][] check_board_all_poligs()
        {
            triangles_on_board = 0;
            var check_arr = new int[pols_inds.Length][];
            for (int i = 0; i < pols_inds.Length; i++)
            {
                var ps = check_polig_on_board(i);
                check_arr[i] = ps;
                if (ps != null) triangles_on_board++;
            }
            return check_arr;
        }

        int[] check_polig_on_board(int i)
        {
            if (pols_inds[i] == null) return null;
            if (pols_inds[i].Length < 3) return null;
            var inds = pols_inds[i];
            var l_0 = ps_on_triang[inds[0]];
            var l_1 = ps_on_triang[inds[1]];
            var l_2 = ps_on_triang[inds[2]];

            var l_cross_0 = l_0.Intersect(l_1);
            var l_cross_1 = l_1.Intersect(l_2);
            var l_cross_2 = l_2.Intersect(l_0);

            if (l_cross_0 != null && l_cross_1 != null && l_cross_2 != null)
            {
                if (l_cross_1.ToArray().Length < 2 && l_cross_2.ToArray().Length < 2) return new int[] { inds[0], inds[1] };
                if (l_cross_0.ToArray().Length < 2 && l_cross_2.ToArray().Length < 2) return new int[] { inds[1], inds[2] };
                if (l_cross_0.ToArray().Length < 2 && l_cross_1.ToArray().Length < 2) return new int[] { inds[2], inds[0] };

                if (l_cross_0.ToArray().Length < 2) return new int[] { inds[0], inds[1] };
                if (l_cross_1.ToArray().Length < 2) return new int[] { inds[1], inds[2] };
                if (l_cross_2.ToArray().Length < 2) return new int[] { inds[2], inds[0] };
            }

             return null;
        }
    }


}
