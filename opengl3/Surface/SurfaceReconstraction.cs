using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using PathPlanning;

namespace opengl3
{
    public enum Ax { X,Y,Z};
    static class SurfaceReconstraction
    {
        
        static public Point3d_GL[] order_ps_by_ax(Point3d_GL[] ps,Ax ax)
        {
            var ps_or = new Point3d_GL[ps.Length];
            if(ax == Ax.X)
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
            for(int i = 0; i < ps.Length; i++)
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
            for (double i = v_X_min; i < v_X_max; i+= Math.Abs(dx))
            {
                var p = new Point3d_GL(0,0,0);
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
        static Point3d_GL set_val_ax(Point3d_GL p, double v,  Ax ax1)
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
                        // GL1.addLineMeshTraj(ps[i], colors[i_c,0], colors[i_c, 1], colors[i_c, 2]);
                        ps_rec.Add(Regression.spline3DLine(ps[i], ds));
                    }
                }
            }



            //traj_i = GL1.addLinesMeshTraj(ps, 1);
            //GL1.addLinesMeshTraj(ps_rec.ToArray(), 0, 1);
            //GL1.SortObj();
            return ps_rec.ToArray();
        }

        static public double cross_obj_flats_find_ang_z(float[] mesh, int num, int ds,GraphicGL graphicGL)
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


        /*static Polygon3d_GL[] splines_to_mesh(CubicSpline[] splines)
        {
            return null;
        }*/
        static public Polygon3d_GL[] surf_from_rec_lines(Point3d_GL[][] rec_lines)
        {

            return null;
        }
        static public Point3d_GL[] points_from_rec_lines(Point3d_GL[][] rec_lines, int layers)
        {
            var ps_layer = new List<Point3d_GL>();
            for (int i = 0; i < rec_lines.Length; i++)
            {
                var ps_z = Point3d_GL.divide_sect(rec_lines[0][i], rec_lines[1][i], layers);
                ps_layer.AddRange(ps_z);
            }
            return ps_layer.ToArray();
        }
        static public Point3d_GL[][] find_rec_lines(Polygon3d_GL[] surf_up, Polygon3d_GL[] surf_down, double dist, GraphicGL graphicGL)
        {
            var lines = find_lines_for_surf(surf_down, dist);
            var ps_down = ps_from_lines(lines);
            lines = set_vec_lines(lines, new Point3d_GL(0, 0, 1));
            var ps_up = find_cross_surf_lines(surf_up, lines);
            var ps_rec = divide_ps(ps_up);
            //graphicGL.addLine3dMesh(lines, Color3d_GL.green());
            //graphicGL.addPointMesh(ps_up, Color3d_GL.blue(), "ps_up");
            graphicGL.addPointMesh(ps_down, Color3d_GL.red(), "ps_down");
            graphicGL.addLineMesh(ps_up, Color3d_GL.green());
            return null;
        }
        static public Point3d_GL[][] divide_ps(Point3d_GL[] ps_d)
        {
            var ps = new List<Point3d_GL>();
            var ps_st = new List<Point3d_GL>();
            for(int i = 0; i < ps_d.Length; i+=2)
            {
                ps.Add(ps_d[i]);
                ps_st.Add(ps_d[i+1]);
            }
            return new Point3d_GL[][] {ps.ToArray(), ps_st.ToArray()};  
        }
        static public Line3d_GL[] set_vec_lines(Line3d_GL[] lines, Point3d_GL vec)
        {
            for(int i = 0; i < lines.Length; i++)
            {
                lines[i].k = vec;
            }
            return lines;
        }
        static public Line3d_GL[] find_lines_for_surf(Polygon3d_GL[] surf,double dist)
        {
            var map_xy = new RasterMap(surf);
            var ps_mesh_xy = gen_mesh_ps_xy(map_xy.pt_minmax[0], map_xy.pt_minmax[1], dist);
            var proj_matrs = PathPlanner.project_layer(surf,ps_mesh_xy.ToList(),map_xy,new Vector3d_GL(1,0,0));
            var lines = lines_from_matrs(proj_matrs.ToArray());
           
            return lines;
        }
        static public Point3d_GL[] ps_from_lines(Line3d_GL[] lines)
        {
            var ps = new Point3d_GL[lines.Length];  
            for(int i = 0; i < lines.Length; i++)
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
                var p = new Point3d_GL(matrs[i][0,3], matrs[i][1, 3], matrs[i][2, 3]);
                var v_z = new Vector3d_GL(matrs[i][2, 0], matrs[i][2, 1], matrs[i][2, 2])*100;
                lines[i] = new Line3d_GL(v_z, p);
            }
            return lines;
        }

        static public Point3d_GL[] gen_mesh_ps_xy(Point3d_GL min, Point3d_GL max, double dist)
        {
            var ps_mesh = new List<Point3d_GL>();
            for(double x = min.x; x < max.x; x += dist)
                for (double y = min.y; y < max.y; y += dist)
                {
                    ps_mesh.Add(new Point3d_GL(x, y));
                }
            return ps_mesh.ToArray();
        }

        static public Point3d_GL[] find_cross_surf_lines(Polygon3d_GL[] surf,Line3d_GL[] lines)
        {
            var ps = new List<Point3d_GL>();
            for (int i = 0; i < lines.Length; i++)
                for (int j = 0; j < surf.Length; j++)
                {
                    var p = Polygon3d_GL.cross_line_triang( surf[j], lines[i]);
                    if(p.exist)
                    {
                        ps.Add(p);
                        ps.Add(lines[i].p);
                    }
                        
                }

           return  ps.ToArray();
        }
        static public Polygon3d_GL[] expand_surf(Polygon3d_GL[] surf)
        {

            return null;
        }
    }
}
