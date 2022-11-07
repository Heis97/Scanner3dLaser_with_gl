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
        public Vector3d_GL v1,v2,v3;
        int special_point_ind;

        public Polygon3d_GL(Point3d_GL P1, Point3d_GL P2, Point3d_GL P3, int _special_point_ind = 0)
        {
            special_point_ind = _special_point_ind;
            ps = new Point3d_GL[] { P1, P2, P3 };
            v1 = new Vector3d_GL(P1, P2).normalize();
            v2 = new Vector3d_GL(P1, P3).normalize();
            v3 = v1 | v2;//vector multiply

            if(v3.z<0)
            {
                v3 = -v3;
            }
            //Console.WriteLine(v3);
            v3.normalize();
            flat3D = new Flat3d_GL(v3.x, v3.y, v3.z, -v3 * P1);
      
        }

        static public Polygon3d_GL[] multMatr(Polygon3d_GL[] pols, Matrix<double> matrix)
        {
            var pols_mul = new Polygon3d_GL[pols.Length];
            for(int i = 0; i < pols.Length; i++)
            {
                var ps = Point3d_GL.multMatr(pols[i].ps, matrix);
                pols_mul[i]= new Polygon3d_GL(ps[0],ps[1],ps[2]);
            }
            return pols_mul;
        }

        static public Flat3d_GL notExistFlat()
        {
            var flat = new Flat3d_GL();
            flat.exist = false;
            return flat;
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
            var v_c = new Vector3d_GL(ps[special_point_ind],p_cross);
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
            for(int line_i=0; line_i < lines.Length; line_i++)
            {
                for (int polygon_i = last_i; polygon_i < polygons.Length; polygon_i++)
                {
                    var p = polygons[polygon_i].crossLine(lines[line_i]);
                    if(p.exist)
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
                            orderby p.x
                            select p;
            return ps_sort.ToArray();
        }

        static public Polygon3d_GL[] triangulate_two_lines_xy(Point3d_GL[] _ps1, Point3d_GL[] _ps2)
        {
            var ps1 = sortByX(_ps1).Reverse().ToArray();
            var ps2 = sortByX(_ps2).Reverse().ToArray();
            var polygons = new List<Polygon3d_GL>();
            var polygons_ind = new List<int[]>();
            int ind_2 = 0;
            int ind_2_last = ind_2;
            List<int>[] ps1_connect = new List<int>[ps1.Length];
            List<int>[] ps2_connect = new List<int>[ps2.Length];
            for (int i=1; i < ps1.Length; i++)
            {                
                polygons.Add(new Polygon3d_GL(ps1[i-1],ps1[i],ps2[ind_2]));
                polygons_ind.Add(new int[] { i - 1, i, ind_2,1 });
                if (ps1_connect[i-1] == null)
                {
                    ps1_connect[i-1] = new List<int>();
                }
                if (ps1_connect[i]==null)
                {
                    ps1_connect[i] = new List<int>();
                }

                if (ps2_connect[ind_2] == null)
                {
                    ps2_connect[ind_2] = new List<int>();
                }
                ps1_connect[i-1].Add(ind_2); ps1_connect[i].Add(ind_2);

                ps2_connect[ind_2].Add(i-1); ps2_connect[ind_2].Add(i);

                if (i < ps1.Length - 1)
                {
                    var min_dist = double.MaxValue;
                    for (int j = 0; j < ps2.Length; j++)
                    {
                        var dist = (ps1[i] - ps2[j]).magnitude_x();
                        if (dist < min_dist)
                        {
                            min_dist = dist;
                            ind_2 = j;
                        }
                    }
                }
                if(ind_2<ind_2_last)
                {
                    Console.WriteLine("ind_2<ind_2_last");
                    Console.WriteLine(ind_2);
                }
                ind_2_last = ind_2;
            }

            for (int i = 1; i < ps2_connect.Length; i++)
            {
                if (ps2_connect[i] == null)
                {
                    ps2_connect[i] = new List<int>();
                    ps2_connect[i].Add(ps2_connect[i - 1][ps2_connect[i - 1].Count - 1]);
                    
                }
            }

            for (int i=1; i < ps2_connect.Length; i++)
            {
                for(int j = 0; j < ps1.Length; j++)
                {
                    
                    if (ps2_connect[i - 1].Contains(j) && ps2_connect[i].Contains(j))
                    {
                        polygons.Add(new Polygon3d_GL(ps2[i], ps2[i-1], ps1[j]));
                        polygons_ind.Add(new int[] { i, i-1, j, 2 });
                    }
                   
                }
            }

            //Console.WriteLine(polygons_ind);
            
            return polygons.ToArray();
        }
        static public Point3d_GL[][] smooth_lines_xy(Point3d_GL[][] ps, double smooth)
        {
            double map_resol = 0.02;
            var p_minmax = lines_minmax(ps);
            var p_min = p_minmax[0]; var p_max = p_minmax[1];
            int smooth_rad = (int)(smooth / map_resol);
            var p_len = (p_max - p_min)/map_resol+new Point3d_GL(2*smooth_rad+1, 2*smooth_rad + 1, 2);//comp_round
            var x_len = (int)p_len.x;
            var y_len = (int)p_len.y;

            var map_xy = new int[x_len, y_len][][];
            for(int i=0; i< ps.Length; i++)
            {
                var x_max = 0;
                var y_max = 0;
                if (ps[i]!=null)
                {
                    for (int j = 0; j < ps[i].Length; j++)
                    {
                        var p_cur = (ps[i][j] - p_min) / map_resol;
                        
                        var x = (int)p_cur.x;
                        var y = (int)p_cur.y;
                        if(x>x_max) x_max = x;
                        if(y>y_max) y_max = y;
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
            for(int i=0; i<ps.Length;i++)
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
                        ps_smooth[i][j] = comp_map_in_rad(map_xy,smooth_rad,ps,x,y, ps[i][j]) ;
                    }
                }
            }

            return ps_smooth;
        }

        static Point3d_GL comp_map_in_rad(int[,][][] map,int smooth_rad, Point3d_GL[][] ps,int x,int y,Point3d_GL p)
        {
            var p_sm = p.Copy();
            int p_count = 1;
            if(x< smooth_rad || y<smooth_rad)
            {
                return p_sm;
            }
            for (int x_cur = -smooth_rad + x; x_cur < x+ smooth_rad; x_cur++)
            {
                for (int y_cur = -smooth_rad + y; y_cur < y + smooth_rad; y_cur++)
                {
                    if(map[x_cur,y_cur]!=null)
                    {
                        for(int k=0; k< map[x_cur, y_cur].Length;k++)
                        {
                            if(map[x_cur, y_cur][k]!=null)
                            {
                                if (map[x_cur, y_cur][k].Length>1)
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
            for (int i=0; i < ps.Length;i++)
            {
                if(ps[i]!=null)
                {
                    for (int j = 0; j < ps[i].Length; j++)
                    { 
                        if(p_min.x > ps[i][j].x)
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

        static public Polygon3d_GL[] triangulate_lines_xy(Point3d_GL[][] ps)
        {
            List<Polygon3d_GL> polygons = new List<Polygon3d_GL>();
           // var ps = smooth_lines_xy(_ps,1.5);
            for (int i=1; i<ps.Length; i++)
            {
                polygons.AddRange(triangulate_two_lines_xy(ps[i - 1], ps[i]));
            }
            Console.WriteLine("triangulated.");
            return polygons.ToArray();
        }


        static public float[][] toMesh(Polygon3d_GL[] polygons)
        {
            var mesh =new  List<float>();
            var color = new List<float>();
            var normal = new List<float>();
            for (int i=0; i<polygons.Length;i++)
            {               
                if(polygons[i].ps.Length>2)
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
                    if (polygons[i].ps[0].color!= null && polygons[i].ps[1].color != null && polygons[i].ps[2].color != null)
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

        public Point3d_GL[] get_dimens_minmax()
        {
            var p_min = new Point3d_GL(double.MaxValue, double.MaxValue, double.MaxValue);
            var p_max = new Point3d_GL(double.MinValue, double.MinValue, double.MinValue);
            for(int i=0; i<ps.Length; i++)
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
    }
}
