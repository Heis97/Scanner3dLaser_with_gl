using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.Structure;
using OpenGL;
using PathPlanning;

namespace opengl3
{
    public enum Ax { X,Y,Z};
    static class SurfaceReconstraction
    {
        static public Point3d_GL[] gen_random_cont_XY(double d, int n, double err,Point3d_GL p_cent)
        {
            var ps = new Point3d_GL[n];
            var alph = Math.PI;
            var d_alph = 2 * Math.PI / n;
            for (int i = 0; i < ps.Length; i++)
            {
                var rand = Accord.Math.Random.Generator.Random;
                var r = (d / 2) + err * (rand.NextDouble() - 0.5);
                var x = r * Math.Sin(alph);
                var y = r * Math.Cos(alph);
                ps[i] = new Point3d_GL(x, y)+p_cent;
                alph += d_alph;
            }


            return ps;
        }
        static public Point3d_GL fit_circle_xy(Point3d_GL[] ps)
        {
            var ps_ord_x = Point3d_GL.sortByX(ps);
            var ps_circ = new Point3d_GL[]
            {
                ps_ord_x[0],
                ps_ord_x[ps_ord_x.Length/2],
                ps_ord_x[ps_ord_x.Length-1]
            };
            var circ = Point3d_GL.calcCirc(ps_circ[0], ps_circ[1], ps_circ[2]);
            var ps_add = Point3d_GL.add_arr(ps, new Point3d_GL(-circ.x, -circ.y));
            var ps_pol = Point3d_GL.toPolar(ps_add);

            var ps_pol_gauss = Point3d_GL.gaussFilter_X(ps_pol,10);
            var ps_cart = Point3d_GL.toPolar(ps_pol_gauss);

            return new Point3d_GL(circ.x, circ.y, ps_pol_gauss[ps.Length/2].x);
        }
        static public Point3d_GL[] ps_fit_circ_XY(Point3d_GL[] ps)
        {
            var circ = fit_circle_xy(ps);
            Console.WriteLine(circ);
            var r_app = circ.z;
            var ps_circ = new Point3d_GL[ps.Length];
            var alph = Math.PI;
            var d_alph = 2 * Math.PI / ps_circ.Length;
            for (int i = 0; i < ps_circ.Length; i++)
            {
                var x = circ.x + r_app * Math.Sin(alph);
                var y = circ.y + r_app * Math.Cos(alph);
                ps_circ[i] = new Point3d_GL(x, y);
                alph += d_alph;
            }
            return ps_circ;
        }

        static public Point3d_GL[] ps_fit_circ_XY_mnk(Point3d_GL[] ps)
        {
            
            var sx = 0d;
            var sx2 = 0d;
            var sx3 = 0d;
            var sy = 0d;
            var sy2 = 0d;
            var sy3 = 0d;
            var sxy = 0d;
            var sx2y = 0d;
            var sxy2 = 0d;

            var n = ps.Length;
            for (int i = 0; i < ps.Length; i++)
            {
                sx += ps[i].x;
                sx2 += ps[i].x* ps[i].x;
                sx3 += ps[i].x* ps[i].x* ps[i].x;

                sy += ps[i].y;
                sy2 += ps[i].y * ps[i].y;
                sy3 += ps[i].y * ps[i].y * ps[i].y;

                sxy += ps[i].x* ps[i].y;
                sx2y += ps[i].x * ps[i].x * ps[i].y;
                sxy2 += ps[i].x * ps[i].y * ps[i].y;
            }
            var n1 = 1d / n;

            var n11 = 2 * (sx2 - n1 * sx * sx);
            var n12 = 2 * (sxy - n1 * sx * sy);
            var w1 = sx3 + sxy2 - n1 * sx2 * sx - n1 * sy2 * sx;

            var n21 = 2 * (sxy - n1 * sx * sy);
            var n22 = 2 * (sy2 - n1 * sy * sy);
            var w2 = sx2y + sy3 - n1 * sx2 * sy - n1 * sy2 * sy;

            var det_N = n11 * n22 - n12 * n21;
            var x0 = (w1 * n22 - w2 * n12) / det_N;
            var y0 = (w2 * n11 - w1 * n21) / det_N;

            var r2 = x0 * x0 + y0 * y0 + n1 * (sx2 + sy2 - 2 * (x0 * sx + y0 * sy));

            //var circ = fit_circle_xy(ps);
            //Console.WriteLine(circ);
            var circ = new Point3d_GL(x0, y0, Math.Sqrt(r2));
            var r_app = circ.z;
            var ps_circ = new Point3d_GL[ps.Length];
            var alph = Math.PI;
            var d_alph = 2 * Math.PI / ps_circ.Length;
            for (int i = 0; i < ps_circ.Length; i++)
            {
                var x = circ.x + r_app * Math.Sin(alph);
                var y = circ.y + r_app * Math.Cos(alph);
                ps_circ[i] = new Point3d_GL(x, y);
                alph += d_alph;
            }
            return ps_circ;
        }





        static public Polygon3d_GL[] get_conts_from_defect(Polygon3d_GL[] pols)
        {
            var pols_color = new Polygon3d_GL[pols.Length];
            for (int i = 0; i < pols.Length; i++)
            {
                pols_color[i] = pols[i].Clone();
                if (pols[i].flat3D.n.z < 0.95)
                {
                    pols_color[i].set_color(Color3d_GL.green());
                }
                else
                {
                    pols_color[i].set_color(new Color3d_GL(0.5f, 0.5f, 0.5f));
                }
            }
            return pols_color;
        }



        static public Point3d_GL[] order_ps_by_ax(Point3d_GL[] ps, Ax ax)
        {
            var ps_or = new Point3d_GL[ps.Length];
            if (ax == Ax.X)
            {
                ps_or = (from p in ps
                         orderby p.x
                         select p).ToArray();
            }
            if (ax == Ax.Y)
            {
                ps_or = (from p in ps
                         orderby p.y
                         select p).ToArray();
            }
            if (ax == Ax.Z)
            {
                ps_or = (from p in ps
                         orderby p.z
                         select p).ToArray();
            }
            return ps_or;
        }

        static public double[][] prep_for_regr(Point3d_GL[] ps, Ax ax_X, Ax ax_Y)
        {
            var data = new List<double[]>();
            for (int i = 0; i < ps.Length; i++)
            {
                var v_X = get_val_ax(ps, i, ax_X);
                var v_Y = get_val_ax(ps, i, ax_Y);
                data.Add(new double[] { v_X, v_Y });
            }
            return data.ToArray();
        }

        static public Point3d_GL[] reconstruct(Point3d_GL[] ps, Ax ax_X, Ax ax_Y, Ax const_ax, double dx)
        {
            var ps_or = order_ps_by_ax(ps, ax_X);
            var data = prep_for_regr(ps_or, ax_X, ax_Y);
            var koef = Regression.regression(data, 3);
            double v_X_min = get_val_ax(ps_or, 0, ax_X);
            double v_X_max = get_val_ax(ps_or, ps_or.Length - 1, ax_X);

            var ps_rec = new List<Point3d_GL>();
            var const_v = get_val_ax(ps, 0, const_ax);
            for (double i = v_X_min; i < v_X_max; i += Math.Abs(dx))
            {
                var p = new Point3d_GL(0, 0, 0);
                var v = Regression.calcPolynSolv(koef, i);
                p = set_val_ax(p, i, ax_X);
                p = set_val_ax(p, v, ax_Y);
                p = set_val_ax(p, const_v, const_ax);
                ps_rec.Add(p);
            }
            return ps_rec.ToArray();
        }
        static public Point3d_GL[] reconstruct_3d(Point3d_GL[] ps, Ax ax_X, double dx)
        {

            // var ps_rec = Regression.regress3DLine(ps, )

            //return ps_rec.ToArray();
            return null;
        }

        static double get_val_ax(Point3d_GL[] ps, int ind, Ax ax1)
        {
            double v = 0;
            switch (ax1)
            {
                case Ax.X: v = ps[ind].x; break;
                case Ax.Y: v = ps[ind].y; break;
                case Ax.Z: v = ps[ind].z; break;
            }
            return v;
        }
        static Point3d_GL set_val_ax(Point3d_GL p, double v, Ax ax1)
        {
            switch (ax1)
            {
                case Ax.X: p.x = v; break;
                case Ax.Y: p.y = v; break;
                case Ax.Z: p.z = v; break;
            }
            return p;
        }

        static public Point3d_GL[][] cross_obj_flats(float[] mesh, double dx, double ds, GraphicGL graphicGL, double fi = -1)
        {
            var p_min = new Point3d_GL(double.MaxValue, double.MaxValue, double.MaxValue);
            var p_max = new Point3d_GL(double.MinValue, double.MinValue, double.MinValue);
            for (int i = 0; i < mesh.Length; i += 3)
            {
                if (mesh[i] > p_max.x) p_max.x = mesh[i];
                if (mesh[i] < p_min.x) p_min.x = mesh[i];

                if (mesh[i + 1] > p_max.y) p_max.y = mesh[i + 1];
                if (mesh[i + 1] < p_min.y) p_min.y = mesh[i + 1];

                if (mesh[i + 2] > p_max.z) p_max.z = mesh[i + 2];
                if (mesh[i + 2] < p_min.z) p_min.z = mesh[i + 2];
            }
            //var ps_x = cross_obj_flats_ax(mesh, dx, p_min.x, p_max.x, Ax.X);
            if (fi > 0)
            {
                return cross_obj_flats_ax(mesh, dx, ds, p_min.y, p_max.y, Ax.Y, graphicGL, fi);
            }
            else
            {
                return cross_obj_flats_ax(mesh, dx, ds, p_min.y, p_max.y, Ax.Y, graphicGL);
            }
        }
        static public Point3d_GL[][] cross_obj_flats_ax_2(float[] mesh, double dx, double min, double max, Ax ax, GraphicGL graphicGL)
        {
            var flats = new List<Flat3d_GL>();
            for (double i = min; i < max; i += dx)
            {
                if (ax == Ax.Y) flats.Add(new Flat3d_GL(0, 1, 0, -i));
                if (ax == Ax.X) flats.Add(new Flat3d_GL(1, 0, 0, -i));
            }
            var ps = graphicGL.cross_flat_gpu_mesh(mesh, flats.ToArray());
            var ps_rec = new List<Point3d_GL[]>();
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i] != null)
                {
                    if (ps[i].Length > 0)
                    {
                        if (ax == Ax.Y) ps_rec.Add(SurfaceReconstraction.reconstruct(ps[i], Ax.X, Ax.Z, Ax.Y, 1));
                        if (ax == Ax.X) ps_rec.Add(SurfaceReconstraction.reconstruct(ps[i], Ax.Y, Ax.Z, Ax.X, 1));
                    }
                }
            }
            return ps_rec.ToArray();
        }

        static public Point3d_GL[][] cross_obj_flats_ax(float[] mesh, double dx, double ds, double min, double max, Ax ax, GraphicGL graphicGL, double fi = 0)
        {
            var flats = new List<Flat3d_GL>();
            for (double i = min; i < max; i += dx)
            {
                if (fi < 0)
                {
                    if (ax == Ax.Y) fi = Math.PI / 2;
                    if (ax == Ax.X) fi = 0;
                }
                flats.Add(new Flat3d_GL(Math.Cos(fi), Math.Sin(fi), 0, -i));
            }
            var ps = graphicGL.cross_flat_gpu_mesh(mesh, flats.ToArray());

            var colors = new float[,]
            {
                {1,0,0 },{0,1,0 },{0,0,1 },{1,1,0 },{1,0,1 },{0,1,1 }
            };

            var ps_rec = new List<Point3d_GL[]>();
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i] != null)
                {
                    if (ps[i].Length > 0)
                    {
                        ps[i] = Point3d_GL.order_points(ps[i]);
                        // var i_c = i % (colors.GetLength(0));

                        var ps_spl = Regression.spline3DLine(ps[i], ds);
                        graphicGL.addLineMeshTraj(ps_spl, Color3d_GL.green());
                        ps_rec.Add(ps_spl);

                    }
                }
            }



            //traj_i = GL1.addLinesMeshTraj(ps, 1);
            //GL1.addLinesMeshTraj(ps_rec.ToArray(), 0, 1);
            //GL1.SortObj();
            return ps_rec.ToArray();
        }

        static public double cross_obj_flats_find_ang_z(float[] mesh, int num, int ds, GraphicGL graphicGL)
        {
            var flats = new List<Flat3d_GL>();
            var dfi = 0.5 * Math.PI / num;
            for (int i = 0; i < num; i++)
            {
                var fi = i * dfi;
                flats.Add(new Flat3d_GL(Math.Cos(fi), Math.Sin(fi), 0, 0));
                Console.WriteLine(fi);
            }
            var ps = graphicGL.cross_flat_gpu_mesh(mesh, flats.ToArray());

            var i_max = 0d;
            var cur_max = 0d;
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i] != null)
                {
                    if (ps[i].Length > 0)
                    {
                        Console.WriteLine("ps[i]1 " + ps[i].Length);
                        ps[i] = Point3d_GL.order_points(ps[i]);
                        Console.WriteLine("ps[i]2 " + ps[i].Length);
                        var curve = Point3d_GL.calc_curve_cm(Regression.spline3DLine(ps[i], ds));
                        Console.WriteLine(curve);
                        if (curve > cur_max)
                        {
                            cur_max = curve;
                            i_max = i;
                        }
                    }
                }
            }

            return i_max * dfi;
        }

        //---------------------------------------------------------------------------

        static public List<List<Point3d_GL[]>> divide_layer(List<List<Point3d_GL[]>> sort_lines, double dz)
        {
            var divide_layers = new List<List<Point3d_GL[]>>();
            var aver_num = aver_num_layer(sort_lines, dz);
            for (int i = 0; i < sort_lines.Count; i++)
            {
                divide_layers.Add(new List<Point3d_GL[]>());
                if (sort_lines[i] != null)
                {
                    for (int j = 0; j < sort_lines[i].Count; j++)
                    {
                        var ps_d = Point3d_GL.divide_sect_dz(sort_lines[i][j][0], sort_lines[i][j][1], aver_num, dz, dz * 0.2);
                        divide_layers[i].Add(ps_d);
                    }
                }
            }
            return divide_layers;
        }

        static public int aver_num_layer(List<List<Point3d_GL[]>> sort_lines, double dz)
        {
            var divide_layers = new List<List<Point3d_GL[]>>();
            double len = 0;
            int len_i = 0;
            for (int i = 0; i < sort_lines.Count; i++)
            {
                divide_layers.Add(new List<Point3d_GL[]>());
                if (sort_lines[i] != null)
                {
                    for (int j = 0; j < sort_lines[i].Count; j++)
                    {
                        len += (sort_lines[i][j][1] - sort_lines[i][j][0]).magnitude();
                        len_i++;
                    }
                }
            }
            return (int)(len / len_i);
        }
        static public int max_num_layer(List<List<Point3d_GL[]>> div_lines)
        {
            int max_i = 0;
            for (int i = 0; i < div_lines.Count; i++)
            {
                if (div_lines[i] != null)
                {
                    for (int j = 0; j < div_lines[i].Count; j++)
                    {
                        if (max_i < div_lines[i][j].Length)
                            max_i = div_lines[i][j].Length;
                    }
                }
            }
            return max_i;
        }
        static public Point3d_GL[][] get_layer(List<List<Point3d_GL[]>> div_l, int ind)
        {
            var layer = new List<Point3d_GL[]>();
            for (int i = 0; i < div_l.Count; i++)
            {
                var l = new List<Point3d_GL>();
                if (div_l[i] != null)
                {
                    for (int j = 0; j < div_l[i].Count; j++)
                    {
                        if (div_l[i][j] != null)
                            if (div_l[i][j].Length > ind)
                                l.Add(div_l[i][j][ind]);
                    }
                }
                layer.Add(l.ToArray());
            }
            return layer.ToArray();
        }
        static public Polygon3d_GL[][] get_layers(List<List<Point3d_GL[]>> sort_lines, double dz)
        {
            var div = divide_layer(sort_lines, dz);
            var max_num = max_num_layer(div);
            var layers = new List<Polygon3d_GL[]>();
            for (int i = 0; i < max_num; i++)
            {
                var layer_ps = get_layer(div, i);
                var layer = Polygon3d_GL.triangulate_lines_xy(layer_ps);
                layers.Add(layer);
            }
            return layers.ToArray();
        }
        static public int[,] analyse_layer(List<List<Point3d_GL[]>> sort_lines, double dz, GraphicGL graphicGL)
        {
            var map_xy_layers = new int[sort_lines.Count, max_sublen(sort_lines)];
            for (int i = 0; i < sort_lines.Count; i++)
            {
                if (sort_lines[i] != null)
                {
                    for (int j = 0; j < sort_lines[i].Count; j++)
                    {
                        map_xy_layers[i, j] = (int)((sort_lines[i][j][1] - sort_lines[i][j][0]).magnitude() / dz);
                    }
                }
            }

            var im = new Image<Gray, Byte>(map_xy_layers.GetLength(0), map_xy_layers.GetLength(1));
            for (int i = 0; i < im.Width; i++)
                for (int j = 0; j < im.Height; j++)
                    im.Data[j, i, 0] = (byte)map_xy_layers[i, j];

            //CvInvoke.Imshow("map", im);
            MainScanningForm.send_buffer_img(im, PrimitiveType.Triangles, graphicGL);

            return map_xy_layers;
        }
        static public int max_sublen(List<List<Point3d_GL[]>> ps)
        {
            int len = 0;
            for (int i = 0; i < ps.Count; i++)
            {
                if (ps[i].Count > len)
                    len = ps[i].Count;
            }
            return len;
        }

        static public List<List<Point3d_GL[]>> clasters_rec_lines(Point3d_GL[][] rec_lines, double dist)
        {
            var sort_l = (from l in rec_lines
                          orderby l[0].x
                          select l).ToArray();
            var clast = new List<List<Point3d_GL[]>>();
            var x_cur = double.MinValue;
            for (int i = 0; i < sort_l.Length; i++)
            {
                if (sort_l[i][0].x - x_cur > dist)
                {
                    clast.Add(new List<Point3d_GL[]>());
                    x_cur = sort_l[i][0].x;
                }
                clast[clast.Count - 1].Add(sort_l[i]);
            }
            return clast;
        }


        //-------------------------------------------------------------------
        static public Point3d_GL[][] find_rec_lines(Polygon3d_GL[] surf_up, Polygon3d_GL[] surf_down, double dist, double dz, GraphicGL graphicGL)
        {
            var lines = find_lines_for_surf(surf_down, dist);
            var ps_down = ps_from_lines(lines);
            lines = set_vec_lines(lines, new Point3d_GL(0, 0, 1));
            var ps_up = find_cross_surf_lines(surf_up, lines);
            var ps_rec = divide_ps(ps_up);

            var sort_lines = clasters_rec_lines(ps_rec, dist / 2);
            var layers = get_layers(sort_lines, dz);

            for (int i = 0; i < layers.Length; i++)
            {
                graphicGL.addMesh(Polygon3d_GL.toMesh(layers[i])[0], PrimitiveType.Triangles);
            }
            //graphicGL.addLine3dMesh(lines, Color3d_GL.green());
            //graphicGL.addPointMesh(ps_rec[0], Color3d_GL.blue(), "ps_up");
            //graphicGL.addLineMesh(ps_up, Color3d_GL.green());
            return null;
        }
        static public Point3d_GL[][] divide_ps(Point3d_GL[] ps_d)//[line1],[...
        {
            var ps = new List<Point3d_GL[]>();
            for (int i = 0; i < ps_d.Length; i += 2)
            {
                ps.Add(new Point3d_GL[] { ps_d[i], ps_d[i + 1] });
            }
            return ps.ToArray();
        }
        static public Line3d_GL[] set_vec_lines(Line3d_GL[] lines, Point3d_GL vec)
        {
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].k = vec;
            }
            return lines;
        }
        static public Line3d_GL[] find_lines_for_surf(Polygon3d_GL[] surf, double dist)
        {
            var map_xy = new RasterMap(surf);
            var ps_mesh_xy = gen_mesh_ps_xy(map_xy.pt_minmax[0], map_xy.pt_minmax[1], dist);
            var proj_matrs = PathPlanner.project_layer(surf, ps_mesh_xy.ToList(), map_xy, new Vector3d_GL(1, 0, 0));
            var lines = lines_from_matrs(proj_matrs.ToArray());

            return lines;
        }
        static public Point3d_GL[] ps_from_lines(Line3d_GL[] lines)
        {
            var ps = new Point3d_GL[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                ps[i] = lines[i].p.Clone();
            }
            return ps;
        }
        static public Line3d_GL[] lines_from_matrs(Matrix<double>[] matrs)
        {
            var lines = new Line3d_GL[matrs.Length];
            for (int i = 0; i < matrs.Length; i++)
            {
                var p = new Point3d_GL(matrs[i][0, 3], matrs[i][1, 3], matrs[i][2, 3]);
                var v_z = new Vector3d_GL(matrs[i][2, 0], matrs[i][2, 1], matrs[i][2, 2]) * 100;
                lines[i] = new Line3d_GL(v_z, p);
            }
            return lines;
        }

        static public Point3d_GL[] gen_mesh_ps_xy(Point3d_GL min, Point3d_GL max, double dist)
        {
            var ps_mesh = new List<Point3d_GL>();
            for (double x = min.x; x < max.x; x += dist)
                for (double y = min.y; y < max.y; y += dist)
                {
                    ps_mesh.Add(new Point3d_GL(x, y));
                }
            return ps_mesh.ToArray();
        }



        static public Point3d_GL[] find_cross_surf_lines(Polygon3d_GL[] surf, Line3d_GL[] lines)
        {
            var ps = new List<Point3d_GL>();
            for (int i = 0; i < lines.Length; i++)
                for (int j = 0; j < surf.Length; j++)
                {
                    var p = Polygon3d_GL.cross_line_triang(surf[j], lines[i]);
                    if (p.exist)
                    {
                        ps.Add(p);
                        ps.Add(lines[i].p);
                    }

                }

            return ps.ToArray();
        }
        static public Polygon3d_GL[] expand_surf(Polygon3d_GL[] surf)
        {

            return null;
        }

        //-------------------------------------------------------------------------
        static public Point3d_GL[,] gen_grid_ps_xy(Point3d_GL min, Point3d_GL max, double dist)
        {
            var len_x = (int)((max.x - min.x) / dist);
            var len_y = (int)((max.y - min.y) / dist);

            var ps_mesh = new Point3d_GL[len_x, len_y];
            for (int x = 0; x < len_x; x++)
                for (int y = 0; y < len_y; y++)
                {
                    ps_mesh[x, y] = new Point3d_GL(min.x + x * dist, min.y + y * dist);
                }
            return ps_mesh;
        }

        static public Polygon3d_GL[][] find_sub_surf_xy(Polygon3d_GL[] surf_up, Polygon3d_GL[] surf_down, double dz, double ddz, double res_xy, double max_ang)
        {
            Console.WriteLine("remesh_gridxy");
            var surfs = remesh_gridxy(surf_up, surf_down, res_xy);
            Console.WriteLine("surf_xy_simp");
            var surf_d_sm = surf_xy_simp(surfs[1], max_ang);
            Console.WriteLine("subsurf_betw_grids_dz");
            var sub = subsurf_betw_grids_dz(new Point3d_GL[][,] { surf_d_sm, surfs[0] }, dz, ddz);
            Console.WriteLine("triangl_subsurf");
            var layrs = triangl_subsurf(sub);
            return layrs;
        }
        static public Point3d_GL[][,] remesh_gridxy(Polygon3d_GL[] surf_up, Polygon3d_GL[] surf_down, double step)
        {
            var map_xy_up = new RasterMap(surf_up);
            var map_xy_down = new RasterMap(surf_down);
            var p_min = Point3d_GL.Min(map_xy_up.pt_minmax[0], map_xy_down.pt_minmax[0]);
            var p_max = Point3d_GL.Max(map_xy_up.pt_minmax[1], map_xy_down.pt_minmax[1]);
            var grid = gen_grid_ps_xy(p_min, p_max, step);

            var grid_up_proj = new Point3d_GL[grid.GetLength(0), grid.GetLength(1)];
            var grid_down_proj = new Point3d_GL[grid.GetLength(0), grid.GetLength(1)];
            for (int x = 0; x < grid.GetLength(0); x++)
                for (int y = 0; y < grid.GetLength(1); y++)
                {
                    grid_up_proj[x, y] = map_xy_up.proj_point_xy(grid[x, y], surf_up);
                    grid_down_proj[x, y] = map_xy_down.proj_point_xy(grid[x, y], surf_down);
                }


            return new Point3d_GL[][,] { grid_up_proj, grid_down_proj };
        }

        static public Point3d_GL[,] surf_xy_simp(Point3d_GL[,] surf_down, double max_ang)
        {
            var smooth = smooth_xy_iter(surf_down, max_ang);
            var up_sm = up_surf(smooth, surf_down);
            return up_sm;
        }
        static public Point3d_GL[,] up_surf(Point3d_GL[,] surf_up, Point3d_GL[,] surf_down)
        {
            var min_dz = double.MaxValue;
            var w = surf_up.GetLength(0);
            var h = surf_up.GetLength(1);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (surf_up[x, y].exist && surf_down[x, y].exist)
                    {
                        var dz = surf_up[x, y].z - surf_down[x, y].z;
                        if (dz < min_dz)
                        {
                            min_dz = dz;
                        }
                    }
                }
            var surf_up_off = new Point3d_GL[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (surf_up[x, y].exist)
                        surf_up_off[x, y] = surf_up[x, y] - new Point3d_GL(0, 0, min_dz);
                    else
                        surf_up_off[x, y] = Point3d_GL.notExistP();
                }


            return surf_up_off;
        }

        static public Point3d_GL[,] smooth_xy(Point3d_GL[,] surf, int rad)
        {
            var w = surf.GetLength(0);
            var h = surf.GetLength(1);
            var surf_smooth = new Point3d_GL[w, h];
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    surf_smooth[x, y] = Point3d_GL.notExistP();
                    if (surf[x, y].exist)
                    {
                        var beg_x = x - rad; if (beg_x < 0) beg_x = 0;
                        var beg_y = y - rad; if (beg_y < 0) beg_y = 0;

                        var end_x = x + rad; if (end_x > w - 1) end_x = w - 1;
                        var end_y = y + rad; if (end_y > h - 1) end_y = h - 1;

                        var ps = new List<Point3d_GL>();
                        for (int x_sub = beg_x; x_sub < end_x; x_sub++)
                            for (int y_sub = beg_y; y_sub < end_y; y_sub++)
                            {
                                ps.Add(surf[x_sub, y_sub]);
                            }
                        var az = aver_z(ps.ToArray());
                        //Console.WriteLine(az);
                        if (az != double.NaN)
                            surf_smooth[x, y] = new Point3d_GL(surf[x, y].x, surf[x, y].y, az);
                    }
                }
            return surf_smooth;
        }
        static public double aver_z(Point3d_GL[] ps)
        {
            if (ps == null) return double.NaN;
            var z_all = 0d;
            var len = 0;
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].exist)
                {
                    z_all += ps[i].z;
                    len++;
                }
            }
            if (len == 0) return double.NaN;
            return z_all / len;
        }
        static public Point3d_GL[,] smooth_xy_iter(Point3d_GL[,] surf_down, double max_ang)
        {
            int iter_max = 100;
            int i = 0;
            int rad = 10;
            var smooth_surf = surf_down;
            var ang = max_dz_neigh_ang(smooth_surf);
            while (ang > max_ang && i < iter_max)
            {
                max_ang = max_dz_neigh_ang(smooth_surf);
                smooth_surf = smooth_xy(smooth_surf, rad);
                i++;
            }
            return smooth_surf;
        }
        static public double max_dz_neigh(Point3d_GL[,] surf1, Point3d_GL[,] surf2)
        {
            var max_dz = 0d;
            var w = surf1.GetLength(0);
            var h = surf1.GetLength(1);
            for (int x = 1; x < w - 1; x++)
                for (int y = 1; y < h - 1; y++)
                {
                    if (surf1[x, y].exist && surf2[x, y].exist)
                    {
                        if (surf1[x - 1, y].exist && surf2[x - 1, y].exist)
                        {
                            var dz = Math.Abs((surf2[x, y].z - surf1[x, y].z) - (surf2[x - 1, y].z - surf1[x - 1, y].z));
                            if (dz > max_dz) max_dz = dz;
                        }
                        if (surf1[x, y - 1].exist && surf2[x, y - 1].exist)
                        {
                            var dz = Math.Abs((surf2[x, y].z - surf1[x, y].z) - (surf2[x, y - 1].z - surf1[x, y - 1].z));
                            if (dz > max_dz) max_dz = dz;
                        }
                    }
                }
            return max_dz;
        }

        static public double max_dz_neigh_ang(Point3d_GL[,] surf)
        {
            var max_dz = 0d;
            var w = surf.GetLength(0);
            var h = surf.GetLength(1);
            for (int x = 1; x < w - 2; x++)
                for (int y = 1; y < h - 2; y++)
                {
                    if (surf[x, y].exist && surf[x - 1, y].exist && surf[x + 1, y].exist)
                    {
                        var v1 = surf[x + 1, y] - surf[x, y];
                        var v2 = surf[x, y] - surf[x - 1, y];
                        var ang = RobotFrame.arccos(v1 ^ v2);
                        if (ang > max_dz) max_dz = ang;
                    }

                    if (surf[x, y].exist && surf[x, y - 1].exist && surf[x, y + 1].exist)
                    {
                        var v1 = surf[x, y + 1] - surf[x, y];
                        var v2 = surf[x, y] - surf[x, y - 1];
                        var ang = RobotFrame.arccos(v1 ^ v2);
                        if (ang > max_dz) max_dz = ang;
                    }
                    // Console.WriteLine(max_dz);
                }
            return max_dz;
        }


        static public Point3d_GL[] divide_nearest(Point3d_GL[,] surf_up, Point3d_GL[,][] grid_sub, Point p)
        {
            var w = grid_sub.GetLength(0);
            var h = grid_sub.GetLength(1);
            if (p.X >= w || p.X < 0 || p.Y >= h || p.Y < 0) return null;

            double min_dist = double.MaxValue;
            var p_min = new Point(0, 0);
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (grid_sub[x, y] != null)
                    {
                        var dist = Math.Abs(p.X - x) + Math.Abs(p.Y - y);
                        if (dist < min_dist)
                        {
                            min_dist = dist;
                            p_min = new Point(x, y);
                        }
                    }
                }

            var p_up = surf_up[p.X, p.Y];
            var dz = p_up - grid_sub[p_min.X, p_min.Y][0];
            var div_up = Point3d_GL.add_arr(grid_sub[p_min.X, p_min.Y], dz);
            //Console.WriteLine(p_up+"; "+grid_sub[p_min.X, p_min.Y][0] + "; " + dz + "; " +div_up[0]  + "; ");
            return div_up;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="grids">down,up(biggest)</param>
        /// <param name="dz"></param>
        /// <param name="ddz"></param>
        /// <returns></returns>
        static public Point3d_GL[][,] subsurf_betw_grids_dz(Point3d_GL[][,] grids, double dz, double ddz)
        {
            var w = grids[0].GetLength(0);
            var h = grids[0].GetLength(1);
            var grid_sub = new Point3d_GL[w, h][];
            Console.WriteLine("aver_num");
            var aver_num = aver_num_layer(grids, dz);
            int max_num = 0;
            Console.WriteLine("sectz");
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (grids[0][x, y].exist && grids[1][x, y].exist)
                    {
                        grid_sub[x, y] = Point3d_GL.divide_sect_dz(grids[1][x, y], grids[0][x, y], aver_num, dz, ddz);
                        if (grid_sub[x, y].Length > max_num) max_num = grid_sub[x, y].Length;
                    }
                }

            var grid_sub_c = (Point3d_GL[,][])grid_sub.Clone();
            Console.WriteLine("divide");
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (grid_sub[x, y] == null && grids[1][x, y].exist)
                    {
                        grid_sub_c[x, y] = divide_nearest(grids[1], grid_sub, new Point(x, y));
                        //Console.WriteLine(x + " " + y + " null");
                    }
                }
            Console.WriteLine("not_ex");
            var subsurfs = new Point3d_GL[max_num][,];
            for (int i = 0; i < subsurfs.Length; i++)
            {
                subsurfs[i] = new Point3d_GL[w, h];
                for (int x = 0; x < w; x++)
                    for (int y = 0; y < h; y++)
                    {
                        subsurfs[i][x, y] = Point3d_GL.notExistP();
                    }
            }
            Console.WriteLine("reshape");
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    if (grid_sub_c[x, y] != null)
                    {
                        for (int z = 0; z < grid_sub_c[x, y].Length; z++)
                        {
                            subsurfs[z][x, y] = grid_sub_c[x, y][z];
                        }
                    }
                    else
                    {
                        //Console.WriteLine(x + " " + y + " null");
                    }

                }

            return subsurfs;
        }
        static public Polygon3d_GL[][] triangl_subsurf(Point3d_GL[][,] subsurfs)
        {
            var surfs = new List<Polygon3d_GL[]>();
            for (int i = 0; i < subsurfs.Length; i++)
            {
                surfs.Add(triangl_grid(subsurfs[i]));
            }
            return surfs.ToArray();
        }
        static public Polygon3d_GL[] triangl_grid(Point3d_GL[,] grid)
        {
            var lines = new List<Point3d_GL[]>();
            for (int x = 0; x < grid.GetLength(0); x++)
            {
                var line = new List<Point3d_GL>();
                for (int y = 0; y < grid.GetLength(1); y++)
                    if (grid[x, y].exist)
                    {
                        line.Add(grid[x, y]);
                    }
                if (line.Count > 0) lines.Add(line.ToArray());

            }
            return Polygon3d_GL.triangulate_lines_xy(lines.ToArray());


        }

        static public int aver_num_layer(Point3d_GL[][,] grids, double dz)
        {
            double len = 0;
            int len_i = 0;
            for (int x = 0; x < grids[0].GetLength(0); x++)
                for (int y = 0; y < grids[0].GetLength(1); y++)
                {
                    if (grids[0][x, y].exist && grids[1][x, y].exist)
                    {
                        len += (grids[1][x, y] - grids[0][x, y]).magnitude();
                        len_i++;
                    }
                }

            return (int)(len / len_i);
        }

    }

}
