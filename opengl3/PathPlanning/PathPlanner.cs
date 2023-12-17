using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using opengl3;

namespace PathPlanning
{
    public class LinePath
    {
        public List<Point3d_GL> ps;
        public void add(Point3d_GL p)  { ps = Point3d_GL.add_arr(ps, p); }
        public void rotate(double angle)  {  ps = Point3d_GL.rotate_points(ps, angle);  }
        public void reverse_dotted()//for dotted color
        {
            var ps_rev = new List<Point3d_GL>();
            for (int i = 0; i < ps.Count-2; i+=2)
            {
                var p1 = ps[ps.Count - 1 - i];
                var p2 = ps[ps.Count - 2 - i];
                p1.color = ps[ps.Count - 2 - i].color;
                p2.color = ps[ps.Count - 1 - i].color;
                ps_rev.Add(p1); ps_rev.Add(p2);
            }
            ps = ps_rev;
        }
        public void reverse()
        {
            ps.Reverse();
        }
        public Point3d_GL get_first()
        {
            if (ps == null) return Point3d_GL.notExistP();
            if (ps.Count < 1) return Point3d_GL.notExistP();
            return ps[0];
        }

        public Point3d_GL get_last()
        {
            if (ps == null) return Point3d_GL.notExistP();
            if (ps.Count < 2) return Point3d_GL.notExistP();
            return ps[ps.Count-1];
        }

        static public int nearest_end_line(LinePath[] lines, LinePath line)
        {
            var min_d = double.MaxValue;
            var i_min = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                double dist;
                int j;
                (j,dist) = min_dist_ends(lines[i],line);
                if (dist < min_d)
                {
                    min_d = dist;
                    i_min = i;
                }
            }
            return i_min;
        }

        static public (int,double) min_dist_ends(LinePath line1, LinePath line2)
        {
            if (line1.ps == null || line2.ps == null) return (0,double.NaN);
            if (line1.ps.Count < 2 || line2.ps.Count < 2) return (0, double.NaN);
            var p1 = line1.ps[0];
            var p2 = line1.ps[line1.ps.Count - 1];
            var p3 = line2.ps[0];
            var p4 = line2.ps[line2.ps.Count - 1];
            var d1 = (p1 - p3).magnitude();
            var d2 = (p1 - p4).magnitude();
            var d3 = (p2 - p3).magnitude();
            var d4 = (p2 - p4).magnitude();

            var dists = new double[] { d1, d2, d3, d4 };
            var min_d = double.MaxValue;
            var i_min = 0;
            for (int i = 0; i < dists.Length; i++)
            {
                if (dists[i] < min_d)
                {
                    min_d = dists[i];
                    i_min = i;
                }
            }
            return (i_min,min_d);
        }

        static public (LinePath, LinePath,bool) try_connect_lines(LinePath line1, LinePath line2,double min_dist)
        {
            var i_min = 0;
            var min_d = double.NaN;
            (i_min, min_d) = min_dist_ends(line1, line2);
            if (min_d > min_dist) return (line1, line2, false);
            switch(i_min)
            {
                case 0: line1.reverse(); break;
                case 1: line1.reverse(); line2.reverse(); break;
                case 2: break;
                case 3: line2.reverse(); break;
            }

            return (line1, line2, true);
        }

        static public (LayerPath, LinePath, bool) try_connect_lines(LayerPath layer, LinePath line, double min_dist)
        {
            double d1, d2;
            int i1, i2;
            (i1,d1) = min_dist_ends(layer.lines[0], line);
            (i2, d2) = min_dist_ends(layer.lines[layer.lines.Count-1], line);
            var min_d = Math.Min(d1, d2);
            bool ret = false;
            if(d1 < d2)
            {
                switch (i1)
                {
                    case 0: layer.lines.Reverse(); line.reverse(); break;//!!!!!!!
                    case 1: layer.lines.Reverse(); break;//!!!!!!!
                    case 2: layer.lines.Reverse(); break;//!!!!!!!
                    case 3: layer.lines.Reverse(); line.reverse(); break;//!!!!!!!
                }
            }
            else
            {
                switch (i2)
                {
                    case 0: line.reverse(); break;
                    case 1:  break;
                    case 2:  break;
                    case 3:  line.reverse(); break;
                }
            }
            if (min_d < min_dist) ret = true;
            return (layer, line, ret);
        }
    }
    public class LayerPath
    {
        public List<LinePath> lines;
        public LayerPath() {
            lines = new List<LinePath>();
        }
        public LayerPath(TrajectoryPath traj)
        {
            var ls = new List<LinePath>();
            foreach(var layer in traj.layers)
            {
                ls.AddRange(layer.lines);
            }
            lines = ls;
        }
        public void ReverseLines()
        {
            for(int i = 0; i < lines.Count; i++)
                lines[i].reverse();
        }
        public Point3d_GL getPoint(int ind)
        {
            switch (ind)
            {
                case -2:
                    return lines[lines.Count - 1].ps[0];
                case -1:
                    return lines[lines.Count - 1].ps[lines[lines.Count - 1].ps.Count - 1];
                case 0:
                    return lines[0].ps[0];
                case 1:
                    return lines[0].ps[lines[0].ps.Count-1];
                default:
                    return Point3d_GL.notExistP();
            }
        }
        public void add(Point3d_GL p)
        {
            foreach(LinePath l in lines)
                l.add(p);
        }
        public void rotate(double angle)
        {
            foreach (LinePath l in lines)
                l.rotate(angle);
        }

        public List<Point3d_GL> to_ps()
        {
            var ps = new List<Point3d_GL>();
            foreach (var line in lines)
                ps.AddRange(line.ps);
            return ps;
        }
    }
    public class TrajectoryPath
    {
        public List<LayerPath> layers;
        public void add(Point3d_GL p)
        {
            foreach (var l in layers)
                l.add(p);
        }
        public void rotate(double angle)
        {
            foreach (var l in layers)
                l.rotate(angle);
        }
        public List<Point3d_GL> to_ps()
        {
            var ps = new List<Point3d_GL>();
            foreach (var layer in layers)
                foreach (var line in layer.lines)
                    ps.AddRange(line.ps);
            return ps;
        }

        public List<List<Point3d_GL>> to_ps_by_layers()
        {
            var ps_list = new List<List<Point3d_GL>>();
            var ps = new List<Point3d_GL>();

            foreach (var layer in layers)
            {
                ps = new List<Point3d_GL>();
                foreach (var line in layer.lines)
                {
                    ps.AddRange(line.ps);
                }
                ps_list = ps_list.Append(ps).ToList();
            }
               
            return ps_list;
        }

    }
    public class PathPlanner
    {




        public enum PatternType { Lines, Harmonic,Dotted}
        static double pi = 3.1415926535;
        static double cos(double ang)
        {
            return Math.Cos(ang);
        }
        static double sin(double ang)
        {
            return Math.Sin(ang);
        }
        public static LinePath gen_arc_sect_xy(Point3d_GL p1, Point3d_GL p2, double r, double min_dist, bool right = true)
        {
            var v1 = p2 - p1;
            if (v1.magnitude() > 2 * r) return new LinePath { ps = new List<Point3d_GL>(new Point3d_GL[] { p1, p2 }) };
            var v2 = v1 / 2;
            var v3_len = Math.Sqrt(r * r - (v2.magnitude() * v2.magnitude()));
            var v3 = Point3d_GL.vec_perpend_xy(v2).normalize()* v3_len;
            double sign = 1;
            if(right) sign = -1;
            var p3 = p1 + v2 + v3 * sign;
            var v_beg = p1 - p3;
            var v_end = p2 - p3;
            var ps = new List<Point3d_GL>();
            var alph = Point3d_GL.ang(v_beg, v_end);
            var min_alph = min_dist / r;
            var delim = (int)(alph/min_alph);
            ps.Add(p1);
            var v_beg_n = v_beg.normalize();
            var v_end_n = v_end.normalize();
            for (int i = 1; i < delim; i++)
            {
                var fi = (i/(double)delim)* (pi / 2);
                var v_med = (v_beg_n * cos(fi) + v_end_n * sin(fi)).normalize() *r ;
                ps.Add(p3+v_med);
            }
            ps.Add(p2);
            return new LinePath { ps = ps};
        }

        public static LinePath gen_smooth_arc(Point3d_GL[] ps, double r, double min_dist)
        {
            var ps_sm = new List<Point3d_GL>();
            ps_sm.Add(ps[0]);
            for (int i = 1; i < ps.Length-1; i++)
            {
                ps_sm.AddRange(gen_arc_sect(ps[i-1], ps[i], ps[i + 1],r,min_dist).ps);
            }
            ps_sm.Add(ps[ps.Length-1]);
            return new LinePath { ps = ps_sm };
        }

        public static List<RobotFrame> gen_smooth_arc(List<RobotFrame> ps, double r, double min_dist)
        {
            var ps_sm = new List<RobotFrame>();
            ps_sm.Add(ps[0]);
            for (int i = 1; i < ps.Count - 1; i++)
            {
                var ps_sec = gen_arc_sect(ps[i - 1].get_pos(), ps[i].get_pos(), ps[i + 1].get_pos(), r, min_dist).ps;
                for (int j = 0; j < ps_sec.Count; j++)
                {
                    var fr = ps[i].Clone();
                    fr.set_pos(ps_sec[j]);
                    ps_sm.Add(fr);
                }
            }
            ps_sm.Add(ps[ps.Count - 1]);
            return ps_sm;
        }

        public static LinePath gen_arc_sect(Point3d_GL p1, Point3d_GL p2, Point3d_GL p3, double r, double min_dist)
        {
            var v1 = p1 - p2;
            var v2 = p3 - p2;
            if (v1.magnitude() < 2 * r || v2.magnitude() < 2 * r) return new LinePath { ps = new List<Point3d_GL>(new Point3d_GL[] { p2 }) };
            var v1_n = v1.normalize();
            var v2_n = v2.normalize();
            var vc_n = ((v1_n+v2_n)/2).normalize();
            var alpha  = Point3d_GL.ang(v1_n, v2_n)/2;
            var d1 = r / Math.Sin(alpha);
            var d2 = r / Math.Tan(alpha);
            var p3_c = p2 + vc_n * d1;
            var p1_c = p2 + v1 * d2;
            var p2_c = p2 + v2 * d2;
            //----------------------------------
            var v_beg = p1_c - p3_c;
            var v_end = p2_c - p3_c;
            var ps = new List<Point3d_GL>();
            var alph = Point3d_GL.ang(v_beg, v_end);
            var min_alph = min_dist / r;
            var delim = (int)(alph / min_alph);
            ps.Add(p1_c);
            var v_beg_n = v_beg.normalize();
            var v_end_n = v_end.normalize();
            for (int i = 1; i < delim; i++)
            {
                var fi = (i / (double)delim) * (pi / 2);
                var v_med = (v_beg_n * cos(fi) + v_end_n * sin(fi)).normalize() * r;
                ps.Add(p3_c + v_med);
            }
            ps.Add(p2_c);
            return new LinePath { ps = ps };
        }

        public static LinePath gen_harmonic_line_xy(Point3d_GL p1, Point3d_GL p2, double r, double min_dist, double dist_arc, bool start_dir_is_right = true)
        {
            var ps = new List<Point3d_GL>();
            var dist = (p2 - p1).magnitude();
            var len_arc = (int)(dist / dist_arc);
            var p = p1.Clone();
            var pv = (p2 - p1).normalize() * dist_arc;
            bool dir = start_dir_is_right;
            for(int i=0; i<len_arc;i++)
            {
                ps.AddRange(gen_arc_sect_xy(p, p + pv, r, min_dist, dir).ps);
                p += pv;
                dir = !dir;
            }
            return new LinePath { ps = ps };
        }

        public static LinePath gen_dotted_line_xy(Point3d_GL p1, Point3d_GL p2, double step, double filling)
        {
            var ps = new List<Point3d_GL>();
            var dist = (p2 - p1).magnitude();
            var len_arc = (int)(dist / step);
            var p = p1.Clone();
            var pvn = (p2 - p1).normalize();
            var pv = pvn * step;
            var len_dot_2 = step * filling / 2;
            for (int i = 0; i <= len_arc; i++)
            {
                var p_a = p - pvn * len_dot_2;
                p_a.color = new Color3d_GL(1, 0, 0);
                var p_b = p + pvn * len_dot_2;
                p_b.color = new Color3d_GL(0, 0, 0);
                ps.Add(p_a); ps.Add(p_b);
                p += pv;
            }
            return new LinePath { ps = ps };
        }


        public static List<Point3d_GL> matr_to_traj(List<Matrix<double>> matrs)
        {
            if(matrs == null) return null;
            var traj_m = new List<Point3d_GL>();
            for (int i = 0; i < matrs.Count; i++)
            {
                traj_m.Add(new Point3d_GL(matrs[i][0, 3], matrs[i][1, 3], matrs[i][2, 3]));
            }
            return traj_m;
        }
        static List<Point3d_GL> join_traj(List<List<Point3d_GL>> traj)
        {
            var traj_j = new List<Point3d_GL>();
            for(int i=0; i<traj.Count;i++)
            {
                traj_j.AddRange(traj[i]);
            }
            return traj_j;
        }

        public static List<Matrix<double>> join_traj(List<List<Matrix<double>>> traj)
        {
            if (traj == null) return null;
            var traj_j = new List<Matrix<double>>();
            for (int i = 0; i < traj.Count; i++)
            {
                traj_j.AddRange(traj[i]);
            }
            return traj_j;
        }

        static List<Point3d_GL> join_traj_matr(List<List<Matrix<double>>> traj)
        {
            var traj_j = new List<Point3d_GL>();
            for (int i = 0; i < traj.Count; i++)
            {
                traj_j.AddRange(matr_to_traj(traj[i]));
            }
            return traj_j;
        }
        public static List<Point3d_GL> test2dcont(double rad,int amount,double dz)
        {
            List<List<Point3d_GL>> conts = new List<List<Point3d_GL>>();
            List<double> zs = new List<double>();
            for (int i =0; i<amount; i++)
            {
                conts.Add(GenerateContour(20, rad, rad * 0.2));
                zs.Add(dz * i);
            }
            var param_tr = new TrajParams
            {
                div_step = 0.3,
                h_transf = 3,
                layers = amount,
                layers_angle = Math.PI / 2,
                step = rad / 10,
                z = zs.ToArray()
            };
            var traj = Generate_multiLayer2d_mesh(conts, param_tr);

            return join_traj(traj);
        }

        public static List<Point3d_GL> test2dcont(List<Point3d_GL> cont, int amount, double dz)
        {
            List<List<Point3d_GL>> conts = new List<List<Point3d_GL>>();
            List<double> zs = new List<double>();
            for (int i = 0; i < amount; i++)
            {
                conts.Add(cont);
                zs.Add(dz * i);
            }
            var param_tr = new TrajParams
            {
                div_step = 0.3,
                h_transf = 3,
                layers = amount,
                layers_angle = Math.PI / 2,
                step = dz*3,
                z = zs.ToArray()
            };
            var traj = Generate_multiLayer2d_mesh(conts, param_tr);

            return join_traj(traj);
        }

        public static List<Matrix<double>> test3dcont(Polygon3d_GL[] surface, List<Point3d_GL> cont, int amount, double dz)
        {
            List<List<Point3d_GL>> conts = new List<List<Point3d_GL>>();
            List<double> zs = new List<double>();
            for (int i = 0; i < amount; i++)
            {
                conts.Add(cont);
                zs.Add(dz * (i+1));
            }
            var param_tr = new TrajParams
            {
                div_step = 1.3,
                h_transf = 3,
                layers = amount,
                layers_angle = Math.PI / 2,
                step = dz * 4,
                z = zs.ToArray()
            };
            var traj = Generate_multiLayer3d_mesh(surface,conts, param_tr);

            return join_traj(traj);
        }


        static List<Point3d_GL> GenerateContour(int n, double rad, double delt)
        {
            double step = 2 * Math.PI/n;
            var cont = new List<Point3d_GL>();
            Random rand = new Random();
            for(int i=0; i<=n; i++)
            {
                var l = rad + delt * rand.NextDouble();
                var x = l * Math.Cos(i * step);
                var y = l * Math.Sin(i * step);
                cont.Add(new Point3d_GL(x, y, 0));
            }
            return cont;
        }

        static List<Point3d_GL> FindCross_for_line(List<Point3d_GL> p, double y)
        {
            var x = p[0].x + (p[1].x - p[0].x) * (y - p[0].y) / (p[1].y - p[0].y);
            var p1 =new  Point3d_GL(x, y, 0);
            x = p[2].x + (p[3].x - p[2].x) * (y - p[2].y) / (p[3].y - p[2].y);
            var p2 = new Point3d_GL(x, y, 0);

            var ps = new List<Point3d_GL>();
            ps.Add(p1); ps.Add(p2);
            return ps;
        }
        static List<Point3d_GL> FindPoints_for_line(List<Point3d_GL> contour, double y)
        {
            var ps = new List<Point3d_GL>();
            for (int i = 1; i < contour.Count; i++)
            {
                if(((y>=contour[i].y) && (y < contour[i-1].y)) || ((y >= contour[i-1].y) && (y < contour[i].y)))
                {
                    ps.Add(contour[i - 1]);
                    ps.Add(contour[i]);
                }
            }
            if(ps.Count>3)
            {
                if (ps[0].x < ps[2].x)
                {
                    return ps;
                }
                else
                {
                    var ps_r = new List<Point3d_GL>();
                    ps_r.Add(ps[2]);
                    ps_r.Add(ps[3]);
                    ps_r.Add(ps[0]);
                    ps_r.Add(ps[1]);
                    return ps_r;
                }
            }
            return new List<Point3d_GL>();
        }


        public static LayerPath gen_pattern_in_square_xy(PatternSettings settings,Point3d_GL p_min, Point3d_GL p_max)
        {
            if (settings == null) return null;
            var pattern = new List<LinePath>();
            var p_cent = new Point3d_GL(p_min, p_max);
            p_min = p_cent + (p_min - p_cent) * 1;
            p_max = p_cent + (p_max - p_cent) * 1;
            switch (settings.patternType)
            {
                case PatternType.Lines:
                    {
                        bool dir = false;
                        for (double y = p_min.y; y <= p_max.y;y+=settings.step)
                        {
                            var p1 = new Point3d_GL(p_min.x, y);
                            var p2 = new Point3d_GL(p_max.x, y);
                            var line = new LinePath { ps = new List<Point3d_GL>(new Point3d_GL[] { p1, p2 }) };
                            if (dir) line.ps.Reverse();
                            dir = !dir;
                            pattern.Add(line);
                        }
                    }
                    break;
                case PatternType.Harmonic:
                    {
                        bool dir_r = settings.start_dir_r;
                        bool dir = false;
                        for (double y = p_min.y; y <= p_max.y; y += settings.step)
                        {
                            var p1 = new Point3d_GL(p_min.x, y);
                            var p2 = new Point3d_GL(p_max.x, y);
                            var line = gen_harmonic_line_xy(p1, p2, settings.r, settings.min_dist, settings.arc_dist, dir_r);
                            if (dir) line.ps.Reverse();
                            dir_r = !dir_r;
                            dir = !dir;
                            pattern.Add(line);
                        }
                    }
                    break;
                case PatternType.Dotted:
                    {
                        bool dir = false;

                        for (double y = p_min.y + settings.step/2; y <= p_max.y; y += settings.step)
                        {
                            var p1 = new Point3d_GL(p_min.x, y);
                            var p2 = new Point3d_GL(p_max.x, y);
                            var line = gen_dotted_line_xy(p1, p2, settings.step,settings.filling);
                            if (dir) line.reverse();
                            dir = !dir;
                            pattern.Add(line);
                        }
                    }
                    break;
                default: break;
            }
            var layer = new LayerPath { lines = pattern };
            p_cent.z = 0;
            layer.add(-p_cent);
            layer.rotate(settings.angle);

            //layer.add(p_cent);
            return layer;
        }

        public static LayerPath gen_pattern_in_contour_xy(PatternSettings settings, TrajParams trajParams, List<Point3d_GL> contour, Point3d_GL p_min, Point3d_GL p_max, GraphicGL gl = null)
        {
            var layer_sq = gen_pattern_in_square_xy(settings, p_min, p_max).to_ps();
            //Console.WriteLine("layer_sq z"+layer_sq[0].z + " " + layer_sq[1].z + " ");
            //gl.addLineMeshTraj(layer_sq.ToArray(), Color3d_GL.aqua());
            var ps_div = divide_traj(layer_sq, trajParams.div_step);
            var cont_layer = cut_pattern_in_contour_xy_cv(contour, ps_div);
            //gl.addLineMeshTraj(cont_layer.ToArray(), Color3d_GL.red());
            var layer = parse_layerpath_from_ps(cont_layer, trajParams.div_step,settings.step, gl);//step
            //gl.addLineMeshTraj(layer.to_ps().ToArray(), Color3d_GL.black());
            return layer;
        }


        public static LayerPath parse_layerpath_from_ps(List<Point3d_GL> ps, double div, double step, GraphicGL gl = null)
        {
            var lines = parse_linepath_from_ps(ps, div,gl);
            var areas = layerpaths_from_linepaths(lines, step);
            var traj = new TrajectoryPath{ layers = areas.ToList()};
            traj = Trajectory.OptimizeTranzitions2LayerPath(traj);
            return new LayerPath(traj);
        }
        public static LayerPath[] layerpaths_from_linepaths(LinePath[] ls, double step)
        {
            var ls_unioned = new List<LayerPath>();
            var ls_list = ls.ToList();
            ls_unioned.Add(new LayerPath());
            ls_unioned[ls_unioned.Count - 1].lines.Add(ls[0]);
            ls_list.RemoveAt(0);
            for (; ls_list.Count >0;)
            {
                var i_min = LinePath.nearest_end_line(ls_list.ToArray(), ls_unioned[ls_unioned.Count - 1].lines[ls_unioned[ls_unioned.Count - 1].lines.Count - 1]);
                LinePath line1, line2;
                LayerPath layerPath;
                bool ret;
                (layerPath, line2, ret) = LinePath.try_connect_lines(ls_unioned[ls_unioned.Count - 1], ls_list[i_min], step * 1.7);
                if (ret)
                {
                    ls_unioned[ls_unioned.Count - 1] = layerPath;
                    ls_unioned[ls_unioned.Count - 1].lines.Add(line2);
                }
                else
                {
                    ls_unioned.Add(new LayerPath());
                    ls_unioned[ls_unioned.Count - 1].lines.Add(line2);
                }
                ls_list.RemoveAt(i_min);
            }
            return ls_unioned.ToArray();
        }
        public static LinePath[] parse_linepath_from_ps(List<Point3d_GL> ps, double div, GraphicGL gl = null)
        {

            var dirty_ls = parse_linepath_from_ps_dirty(ps, div);
           /* foreach (var line in dirty_ls)
            {
                var color = Color3d_GL.random();
                gl.addLineMeshTraj(line.ps.ToArray(), color, "lines");
                gl.addPointMesh(new Point3d_GL[] { line.ps[0], line.ps[line.ps.Count-1] }, color, "ps");
            }*/
            var ls = linepath_from_dirty_linepath(dirty_ls, div);  
            return ls;
        }
        public static LinePath[] parse_linepath_from_ps_dirty(List<Point3d_GL> ps, double div)
        {
            var ps_ord = Point3d_GL.order_points_by_dist(ps.ToArray(),div*1.3).ToList();
            var lines = new List<LinePath>();
            var ps_loc = new List<Point3d_GL>();
            ps_loc.Add(ps_ord[0]);
            for (int i = 1; i < ps_ord.Count; i++)
            {                
                var dist =  (ps_ord[i] - ps_loc[ps_loc.Count-1]).magnitude();
                if(dist>1.3*div)
                {
                    lines.Add(new LinePath() { ps = ps_loc });
                    ps_loc = new List<Point3d_GL>();
                    ps_loc.Add(ps_ord[i]);
                    
                }
                else
                {
                    ps_loc.Add(ps_ord[i]);
                }
            }
            if(ps_loc.Count>0) lines.Add(new LinePath() { ps = ps_loc });
            return lines.ToArray();
        }


        public static LinePath[] linepath_from_dirty_linepath(LinePath[] ls, double div)//!!!
        {
            var ls_unioned = new List<LinePath>();
            ls_unioned.Add(ls[0]);
            for (int i = 0; i < ls.Length; i++)
            {
                LinePath line1, line2;
                bool ret;
                (line1, line2, ret) = LinePath.try_connect_lines(ls_unioned[ls_unioned.Count - 1], ls[i], div);
                if(ret)
                {
                    ls_unioned.RemoveAt(ls_unioned.Count - 1);
                    line1.ps.AddRange(line2.ps);
                    ls_unioned.Add(line1);
                }
                else
                {
                    if (line1 == null) continue;
                    ls_unioned.Add(line2);
                }
            }
            return ls_unioned.ToArray();
        }

        public static List<Point3d_GL> cut_pattern_in_contour_xy_cv(List<Point3d_GL> contour, List<Point3d_GL> pattern)
        {
            var size_im = new Size(1000, 1000);
            var min_p = Point3d_GL.Min(contour.ToArray());
            var cont_nz = Point3d_GL.add_arr(contour, -min_p+new Point3d_GL(1,1,1));
            var max_pz = Point3d_GL.Max(cont_nz.ToArray()) + new Point3d_GL(1, 1, 1);
            var k =  size_im.Width/Math.Max(max_pz.x, max_pz.y);

            var cont_im = new VectorOfPoint();
            for(int i=0; i<contour.Count;i++)
            {
                var p = cont_nz[i] * k;
                cont_im.Push(new Point[] { new Point((int)p.x, (int)p.y) });
            }
            var im = new Image<Gray, Byte>(size_im);
            var im_patt = new Image<Gray, Byte>(size_im);
            CvInvoke.FillPoly(im, cont_im, new MCvScalar(255));
            CvInvoke.FillPoly(im_patt, cont_im, new MCvScalar(127));
            //CvInvoke.Imshow("cont", im);
            var patt_cut = new List<Point3d_GL>();
            for (int i = 0; i < pattern.Count; i++)
            {
                var p = (pattern[i] - min_p + new Point3d_GL(1, 1, 1))*k;
                var p_i = new Point((int)p.x, (int)p.y);
                //CvInvoke.Circle(im_patt, p_i, 0, new MCvScalar(255));
                if (p_i.X < 0 || p_i.Y < 0 || p_i.X >= size_im.Width || p_i.Y >= size_im.Height) continue;
                if (im.Data[p_i.Y, p_i.X, 0] > 0) patt_cut.Add(pattern[i]);
            }

            //CvInvoke.Imshow("cont", im_patt);
            return patt_cut;
        }

        public static List<Point3d_GL> gen_traj_3d_pores(PatternSettings settings,TrajParams trajParams)
        {
            var traj_layers = new List<LayerPath>();
            var dim_cur = new Point3d_GL(settings.dim_x, settings.dim_y);
            var dim_const = new Point3d_GL(settings.dim_x, settings.dim_y);
            var A_filling = settings.filling;
            settings.filling = 0;
            var df = A_filling / (trajParams.layers / 3);
            for (int i=0; i<trajParams.layers;i++)
            {
                if (i > trajParams.layers / 2) settings.start_dir_r = false;
                else settings.start_dir_r = true;
                if (i % 2 == 0) { settings.angle = pi / 2; dim_cur = new Point3d_GL(settings.dim_y, settings.dim_x); }
                else { settings.angle = 0; dim_cur = new Point3d_GL(settings.dim_x, settings.dim_y); }

                if (i < trajParams.layers / 3) settings.filling += df;
                else if((i < 2*trajParams.layers / 3) && (i >=   trajParams.layers / 3)) settings.filling -= df;
                else settings.filling =0;

                var layer = gen_pattern_in_square_xy(settings, new Point3d_GL(), dim_cur);
                layer.add(new Point3d_GL(0,0,trajParams.dz * ((int)(i/2)+1))+ dim_const / 2);
                traj_layers.Add(layer);
                //Console.WriteLine(settings.filling);
                //settings.angle += pi / 2;
            }
            var traj = new TrajectoryPath { layers = traj_layers };
            traj = Trajectory.OptimizeTranzitions2LayerPath(traj);
            var traj_ps = traj.to_ps();
            return traj_ps;
        }

        public static List<List<Point3d_GL>> gen_traj_2d(List<List<Point3d_GL>> contour, TrajParams trajParams, PatternSettings settings,GraphicGL gl = null)
        {
            var traj_layers = new List<LayerPath>();
            var min_p = Point3d_GL.Min(contour) - new Point3d_GL(5 * trajParams.step, 5 * trajParams.step, 5 * trajParams.step);
            var max_p = Point3d_GL.Max(contour) + new Point3d_GL(5 * trajParams.step, 5 * trajParams.step, 5 * trajParams.step);
            for (int i = 0; i < contour.Count; i++)
            {
                var layer = gen_pattern_in_contour_xy(settings, trajParams, contour[i].ToList(), min_p, max_p, gl);
                layer.add(new Point3d_GL(0, 0, trajParams.dz * 0));
                traj_layers.Add(layer);
                settings.angle += settings.angle_layers;
                

            }
           
            var traj = new TrajectoryPath { layers = traj_layers };
            traj = Trajectory.OptimizeTranzitions2LayerPath(traj);
            var traj_ps = traj.to_ps_by_layers();
            return traj_ps;
        }
        static List<Point3d_GL> GeneratePositionTrajectory(List<Point3d_GL> contour, double step)
        {
            var traj = new List<Point3d_GL>();
            var y_min = double.MaxValue;
            var y_max = double.MinValue;
            var i_min = 0;
            var i_max = 0;
            for(int i=0; i<contour.Count; i++)
            {
                if(contour[i].y<y_min)
                {
                    y_min = contour[i].y;
                    i_min = i;
                }
                if (contour[i].y > y_max)
                {
                    y_max = contour[i].y;
                    i_max = i;
                }
            }
            var p_min = contour[i_min];
            var p_max = contour[i_max];
            var y = step * (int)(p_min.y / step);
            int flagRL = 0;
            while(y<p_max.y)
            {
                var ps = FindPoints_for_line(contour, y);
                if((ps.Count == 4) && (flagRL == 0))
                {
                    var p12 = FindCross_for_line(ps, y);
                    if((p12[1] - p12[0]).magnitude_xy()>0.1)
                    {
                        traj.Add(p12[1]); traj.Add(p12[0]);
                        flagRL = 1;
                    }
                }
                else if ((ps.Count == 4) && (flagRL == 1))
                {
                    var p12 = FindCross_for_line(ps, y);
                    if ((p12[1] - p12[0]).magnitude_xy() > 0.1)
                    {
                        traj.Add(p12[0]); traj.Add(p12[1]);
                        flagRL = 0;
                    }
                }
                y += step;
            }
            return traj;
        }
        
        static List<Point3d_GL> GeneratePositionTrajectory_angle(List<Point3d_GL> contour, double step, double angle)
        {
            var contour_rotate = Point3d_GL.rotate_points(contour, angle);
            var traj = GeneratePositionTrajectory(contour_rotate, step);
            var traj_rotate = Point3d_GL.rotate_points(traj, -angle);
            return traj_rotate;
        }
        static List<Point3d_GL> set_z_layer(List<Point3d_GL> traj, double z)
        {
            for (int i = 0; i < traj.Count; i++)
            {
                traj[i] =  new Point3d_GL(traj[i].x, traj[i].y, z);
            }
            return traj;
        }
        public static List<List<Point3d_GL>> Generate_multiLayer2d_mesh(List<List<Point3d_GL>> contour, TrajParams trajParams,double z = 0)
        {

            var traj = new List<List<Point3d_GL>>();
            for (int i=0;i < contour.Count; i++)
            {
                var alfa2 = trajParams.layers_angle;
                if (i % 2 == 0)
                {
                    alfa2 += Math.PI / 2;
                }
                
                var layer = set_z_layer(GeneratePositionTrajectory_angle(contour[i], trajParams.step, alfa2), trajParams.z[i]);
                traj.Add(layer);
            }
        

    //traj = Trajectory.optimize_tranzitions_2_layer(traj)
            for (int i=0; i<traj.Count; i++)
            {
                if (i != traj.Count - 1)
                {
                    if (traj[i].Count > 0)
                    {
                        var last_pos = traj[i][traj[i].Count-1].Copy();
                        last_pos.z = trajParams.z[i + 1];
                        traj[i].Add(last_pos); 
                    }                    
                }
                
            }
            

            return traj;
        }
        public static List<List<Point3d_GL>> generate_2d_traj(List<List<Point3d_GL>> contour, TrajParams trajParams)
        {

            var traj = new List<List<Point3d_GL>>();
            for (int i = 0; i < contour.Count; i++)
            {
                var alfa2 = trajParams.layers_angle;
                if (i % 2 == 0)
                {
                    alfa2 += Math.PI / 2;
                }

                var layer = GeneratePositionTrajectory_angle(contour[i], trajParams.step, alfa2);
                traj.Add(layer);
            }


            //traj = Trajectory.optimize_tranzitions_2_layer(traj)
            for (int i = 0; i < traj.Count; i++)
            {
                if (i != traj.Count - 1)
                {
                    if (traj[i].Count > 0)
                    {
                        traj[i].Add(traj[i][traj[i].Count - 1].Copy());
                    }
                }

            }

            traj = Trajectory.OptimizeTranzitions2Layer(traj);
            return traj;
        }

        public static List<Point3d_GL> filter_traj(List<Point3d_GL> layer, double fil_step)
        {
            var traj_fil = new List<Point3d_GL>();
            traj_fil.Add(layer[0]);
            for(int i=1; i<layer.Count;i++)
            {
                if((layer[i]-traj_fil[traj_fil.Count-1]).magnitude()>fil_step)
                {
                    traj_fil.Add(layer[i]);
                }
            }
            return traj_fil;
        }
        public static List<Point3d_GL> divide_traj(List<Point3d_GL> layer,double div_step)
        {
            var traj_div = new List<Point3d_GL>();
            for(int i=0; i<layer.Count-1;i++)
            {
                traj_div.Add(layer[i]);
                var dist = (layer[i + 1] - layer[i]).magnitude();
                if(2*dist>div_step)
                {
                    var n = (int)(dist / div_step);
                    for (int j = 0; j < n; j++)
                    {
                        traj_div.Add(
                            new Point3d_GL(
                            layer[i].x + (div_step * j * (layer[i + 1].x - layer[i].x)) / dist,
                            layer[i].y + (div_step * j * (layer[i + 1].y - layer[i].y)) / dist,
                            layer[i].z + (div_step * j * (layer[i + 1].z - layer[i].z)) / dist
                            ));
                    }
                }
            }
            return traj_div;
        }


        static Vector3d_GL comp_vecx(Vector3d_GL vec_x_dir, Vector3d_GL n)
        {
            var y = (n | vec_x_dir).normalize();
            var x = (y | n).normalize();
            return x;
        }
        static Vector3d_GL comp_vecz(Vector3d_GL vec_x, Vector3d_GL normal)
        {
            var y = (normal | vec_x).normalize();
            var n = (vec_x | y).normalize();
            return n;
        }


        static Matrix<double> proj_point(Polygon3d_GL polyg, Point3d_GL point,Vector3d_GL vec_x_dir)//only xy scans
        {

             /*var vec_y = (polyg.flat3D.n | vec_x_dir).normalize();
             //Console.WriteLine(vec_y);
             var vec_x = (vec_y | polyg.flat3D.n).normalize();
             //Console.WriteLine(vec_x);
             var vec_z = polyg.flat3D.n;
             //Console.WriteLine(vec_z);
             //Console.WriteLine("_________________");
             */
            var n = polyg.flat3D.n;
            if (n.z < 0) n *= -1;
            var vec_x = comp_vecx(vec_x_dir,n);
            var vec_z = comp_vecz(vec_x,n);
            var vec_y = (vec_z | vec_x).normalize();

            var p_proj = polyg.project_point_xy(point);
            
            //var p_proj = polyg.ps[0];
            return new Matrix<double>(new double[,]
            {
                { vec_x.x,vec_x.y,vec_x.z,p_proj.x},
                 { vec_y.x,vec_y.y,vec_y.z,p_proj.y},
                  { vec_z.x,vec_z.y,vec_z.z,p_proj.z+point.z},
                   { 0,0,0,1}
            });
        }

        public static Matrix<double> proj_point_test(Polygon3d_GL polyg, Vector3d_GL vec_x_dir)
        {

            var vec_y = (polyg.flat3D.n | vec_x_dir).normalize();
            //Console.WriteLine(vec_y);
            var vec_x = (vec_y | polyg.flat3D.n).normalize();
            //Console.WriteLine(vec_x);
            var vec_z = polyg.flat3D.n;
            //Console.WriteLine(vec_z);
            //Console.WriteLine("_________________");
            //var p_proj = polyg.project_point_xy(point);
            var p_proj = new Point3d_GL(10, 10, 0);
            //var p_proj = polyg.ps[0];
            return new Matrix<double>(new double[,]
            {
                { vec_x.x,vec_x.y,vec_x.z,p_proj.x},
                { vec_y.x,vec_y.y,vec_y.z,p_proj.y},
                { vec_z.x,vec_z.y,vec_z.z,p_proj.z},
                { 0      ,      0,      0,       1}
            });
        }

        public static List<Matrix<double>> project_layer(Polygon3d_GL[] surface, List<Point3d_GL> layer, RasterMap map_xy, Vector3d_GL vec_x_dir)
        {
            var layer_3d = new List<Matrix<double>>();
            for (int i = 0; i < layer.Count; i++)
            {
                var polyg_inds = map_xy.get_polyg_ind_prec_xy(layer[i], surface);
                if (polyg_inds == null) { 
                    layer_3d.Add(new Matrix<double>(new double[,]
                    {
                        {1, 0,0 , layer[i].x },
                        {0,1 , 0, layer[i].y },
                        {0,0 , 1, layer[i].z },
                        {0,0 ,0 ,  1}
                    }));
                    continue;
                }
                var polyg_ind =RasterMap.fing_high_polyg(polyg_inds, layer[i], surface);
                var proj_matr = proj_point(surface[polyg_ind], layer[i], vec_x_dir);
                if(proj_matr!=null)
                    layer_3d.Add(proj_matr);          
            }
            if (layer_3d.Count == 0) return null;
            return layer_3d;
        }

        

        static List<List<Matrix<double>>> add_transit(List<List<Matrix<double>>> traj, double trans_h)
        {
            if(traj.Count < 2) return traj;
            for(int i=0; i< traj.Count;i++)
            {
                traj[i][0][3, 3] = 0;
                var last = traj[i][traj[i].Count - 1].Clone();
                last[2, 3] += trans_h;
                last[3, 3] = 0;
                traj[i].Add(last);
                var first = traj[i][0].Clone();
                first[2, 3] += trans_h;
                first[3, 3] = 0;
                traj[i].Insert(0, first);
            }
            return traj;
        }

        static List<List<Matrix<double>>> add_transit_out(List<List<Matrix<double>>> traj, double trans_h)
        {
            traj[0][0][2, 3] +=trans_h;
            traj[traj.Count-1][traj[traj.Count - 1].Count-1][2, 3] += trans_h;
            return traj;
        }
        public static List<List<Matrix<double>>> Generate_multiLayer3d_mesh(Polygon3d_GL[] surface, List<List<Point3d_GL>> contour, TrajParams trajParams)
        {
            trajParams.comp_z();
            var traj_2d = Generate_multiLayer2d_mesh(contour, trajParams);
            traj_2d = Trajectory.OptimizeTranzitions2Layer(traj_2d);
            var traj_3d = new List<List<Matrix<double>>>();
            double resolut = 0.2;
            var map_xy = new RasterMap(surface, resolut,RasterMap.type_map.XY);
            var ang_x = trajParams.ang_x;
            var vec_x = new Vector3d_GL(Math.Cos(ang_x), Math.Sin(ang_x), 0);
            for (int i=0; i<traj_2d.Count;i++)
            {
                var traj_df = filter_traj(divide_traj(traj_2d[i], trajParams.div_step), trajParams.div_step / 2);
                traj_3d.Add(project_layer(surface, traj_df, map_xy, vec_x));
            }
            traj_3d = add_transit(traj_3d, trajParams.h_transf);
            traj_3d = add_transit_out(traj_3d, trajParams.h_transf_out);
            return traj_3d;
        }

        public static List<List<Matrix<double>>> generate_3d_traj_diff_surf(List<Polygon3d_GL[]> surface, List<List<Point3d_GL>> contour, TrajParams trajParams, PatternSettings patternSettings = null, GraphicGL gl = null)
        {
            trajParams.comp_z();
            var traj_2d = new List<List<Point3d_GL>>();
            if (patternSettings != null) traj_2d = gen_traj_2d(contour, trajParams, patternSettings,gl);
            else traj_2d = generate_2d_traj(contour, trajParams);

            //gl.addLineMeshTraj(traj_2d[0].ToArray(), Color3d_GL.blue());
            
            var traj_3d = new List<List<Matrix<double>>>();
            double resolut = -1;
            
            var ang_x = trajParams.ang_x;
            var vec_x = new Vector3d_GL(Math.Cos(ang_x), Math.Sin(ang_x), 0);
            for (int i = 0; i < traj_2d.Count; i++)
            {
                var map_xy = new RasterMap(surface[i], resolut, RasterMap.type_map.XY);
                var traj_df = filter_traj(divide_traj(traj_2d[i], trajParams.div_step), trajParams.div_step / 2);
                //if (imb != null) imb.Image = UtilOpenCV.draw_map_xy(map_xy, surface[i], traj_df.ToArray());
                var proj_layer = project_layer(surface[i], traj_df, map_xy, vec_x);
                //gl.addLineMeshTraj(matr_to_traj(proj_layer).ToArray(), Color3d_GL.purple());
                if (proj_layer == null) continue;
                traj_3d.Add(proj_layer);
            }
            
            traj_3d = add_transit(traj_3d, trajParams.h_transf);
            return traj_3d;
        }

        public static List<Point3d_GL> project_contour_on_surface(Polygon3d_GL[] surface, List<Point3d_GL> contour)
        {
            var cont_proj = new List<Point3d_GL>();
            for(int i=0; i< contour.Count; i++)
            {
                cont_proj.Add(new Point3d_GL() { x = contour[i].x, y = contour[i].y });
            }
            double resolut = 0.2;
            var map_xy = new RasterMap(surface, resolut, RasterMap.type_map.XY);

            var proj_c = project_layer(surface, cont_proj, map_xy, new Vector3d_GL(1,0,0));
            
            return matr_to_ps(proj_c);
        }

        public static string generate_robot_traj(List<Matrix<double>> traj, RobotFrame.RobotType type_robot,TrajParams trajParams = null)
        {
            var traj_rob = new List<RobotFrame>();
            var r_syr = 18.5/2;
            var v = trajParams.Vel;
            //s_syr*f = s_nos*v
            var f = ((trajParams.dz * trajParams.line_width) * v) / (3.1415 * r_syr * r_syr);
            for (int i = 0; i < traj.Count; i++)
            {
                var fr = new RobotFrame(traj[i],type_robot);
                fr.X += trajParams.off_x;
                fr.Y += trajParams.off_y;
                fr.V = v;
                fr.F = f;
                traj_rob.Add(fr);
            }
            traj_rob = RobotFrame.smooth_angle(traj_rob, trajParams.w_smooth_ang);
            traj_rob = RobotFrame.decrease_angle(traj_rob, trajParams.k_decr_ang);

            return RobotFrame.generate_string(traj_rob.ToArray());
        }
        public static string generate_printer_prog(List<Matrix<double>> traj,  TrajParams trajParams = null)
        {
            var traj_rob = new List<RobotFrame>();
            var f = trajParams.Vel;

            for (int i = 1; i < traj.Count; i++)
            {
                var fr1 = new RobotFrame(traj[i-1], RobotFrame.RobotType.FABION2);
                var fr2 = new RobotFrame(traj[i], RobotFrame.RobotType.FABION2);
                fr2.V = RobotFrame.dist(fr1,fr2)*trajParams.line_width*trajParams.dz;
                fr2.F = f;
                traj_rob.Add(fr2);
            }
            //traj_rob = RobotFrame.smooth_angle(traj_rob, 5);
            //traj_rob = RobotFrame.decrease_angle(traj_rob, 0.5);

            return RobotFrame.generate_string_fabion(traj_rob.ToArray());
        }
        
        public static List<Point3d_GL> matr_to_ps(List<Matrix<double>> traj)
        {
            var traj_rob = new List<Point3d_GL>();

            for (int i = 0; i < traj.Count; i++)
            {
                var f = new RobotFrame(traj[i]);
                traj_rob.Add(new Point3d_GL(f.X, f.Y, f.Z));
            }

            return traj_rob;
        }
        public static List<Matrix<double>> ps_to_matr(List<Point3d_GL> ps)
        {
            var traj_rob = new List<Matrix<double>>();

            for (int i = 0; i < ps.Count; i++)
            {
                var color_cur = new Color3d_GL(0,0,0);
                if (ps[i].color != null) color_cur = ps[i].color;
                traj_rob.Add(new Matrix<double>(new double[,]
                {
                    
                    { 1,0,0,ps[i].x},
                    { 0,1,0,ps[i].y},
                    { 0,0,1,ps[i].z},
                    { color_cur.r,color_cur.g,color_cur.b,1 }
                }));
            }

            return traj_rob;
        }
        public static string generate_robot_traj_old(List<Matrix<double>> traj)
        {
            var traj_rob = new StringBuilder();
            double v = 20;
            for(int i = 0; i<traj.Count; i++)
            {
                var x = traj[i][0, 3];
                var y = traj[i][1, 3];
                var z = traj[i][2, 3];
                double b = Math.Asin(-traj[i][2, 0]);
                double a = 0;
                double c = 0;
                if( Math.Cos(b)!=0)
                {
                    c =  Math.Asin(traj[i][2, 1] / Math.Cos(b));
                    a =  Math.Asin(traj[i][1, 0] / Math.Cos(b));
                }
                int e = (int)traj[i][3, 3];

                traj_rob.Append(
                    " X" + round(x) + ", Y" + round(y) + ", Z" + round(z) +
                    ", A" + round(a) + ", B" + round(0.5*b) + ", C" + round(0.5 * c) + 
                    ", V" + round(v) + ", D" + e+" \n"
                    );
                
            }

            traj_rob.Append("q\n");
            return traj_rob.ToString();
        }

        static double round(double val)
        {
            return Math.Round(val,4);
        }
    }
    public static class Trajectory
    {
        static public List<List<Point3d_GL>> OptimizeTranzitions(List<List<Point3d_GL>> traj)
        {
            List<int[][]> approach = new List<int[][]>();

            for (int i = 0; i < traj.Count; i++)
            {
                int s1 = 0;
                int s2 = 1;
                int e1 = -1;
                int e2 = -2;
                approach.Add(new int[][] { new int[] { s1, e1 }, new int[] { s2, e2 }, new int[] { e1, s1 }, new int[] { e2, s2 } });
            }

            List<double[][]> dists = new List<double[][]>();

            for (int i = 0; i < traj.Count - 1; i++)
            {
                double[][] distsBetween = new double[approach[i].Length][];

                for (int layer1 = 0; layer1 < approach[i].Length; layer1++)
                {
                    double[] distsLayer = new double[approach[i + 1].Length];

                    for (int layer2 = 0; layer2 < approach[i].Length; layer2++)
                    {
                        distsLayer[layer2] = Distance(traj[i][approach[i][layer1][layer2]], traj[i + 1][approach[i][layer1][layer2]]);
                    }

                    distsBetween[layer1] = distsLayer;
                }

                dists.Add(distsBetween);
            }

            return traj;
        }

        static int get_ind(int i, int len)
        {
            if (i<0)
                return len+i;
            else
                return i;
        }

        static public List<List<Point3d_GL>> OptimizeTranzitions2Layer(List<List<Point3d_GL>> traj)
        {
            List<int[][]> approach = new List<int[][]>();

            for (int i = 0; i < traj.Count; i++)
            {
                int s1 = 0;
                int s2 = 1;
                int e1 = -1;
                int e2 = -2;
                approach.Add(new int[][] { new int[] { s1, e1 }, new int[] { s2, e2 }, new int[] { e1, s1 }, new int[] { e2, s2 } });
            }

            List<double[][]> dists = new List<double[][]>();

            for (int i = 0; i < approach.Count; i++)
            {
                double[][] b = new double[approach[i].Length][];

                for (int j = 0; j < approach[i].Length; j++)
                {
                    double[] c = new double[approach[i].Length];
                    for (int k = 0; k < approach[i].Length; k++)
                    {
                        c[k] = 1000000000.0f;
                    }

                    b[j] = c;
                }

                dists.Add(b);
            }

            for (int i = 0; i < traj.Count - 1; i++)
            {
                for (int layer1 = 0; layer1 < approach[i].Length; layer1++)
                {
                    for (int layer2 = 0; layer2 < approach[i + 1].Length; layer2++)
                    {
                        if (traj[i] != null && traj[i + 1] != null)
                        {
                            var i1 = get_ind(approach[ i][layer1][1], traj[i].Count) ;
                            var i2 = get_ind(approach[i+1][layer2][0], traj[i+1].Count);
                            var p1 = traj[i][i1];
                            var p2 = traj[i+1][i2];
                            dists[i][layer1][layer2] = 
                                Distance(p1,p2);
                        }
                    }
                }
            }

            List<int> fastWay = new List<int>();
            List<List<Point3d_GL>> traj_opt = new List<List<Point3d_GL>>();
            List< List<int>> ways = new List<List<int>>();
            double min_dist = double.MaxValue;
            for (int f = 0; f < 4; f++)
            {
                var traj_cl = clone_traj(traj);
                for (int i = 0; i < dists.Count; i++)
                {
                    int low, up;
                    if (i == 0)
                    {
                        (low, up) = FindBestWayFirst(dists[i], f);
                        fastWay = new List<int> { low, up };
                    }
                    else
                    {
                        (low, up) = FindBestWayCont(dists[i], fastWay[fastWay.Count - 1]);
                        fastWay.Add(up);
                    }
                }
                ways.Add(fastWay);
                //Console.WriteLine("Traj after___________");
                var traj_opt_loc = OptimizeTrans(traj_cl.ToArray(), approach.ToList(), fastWay).ToList();
                var dist_opt = CompTrans(traj_opt_loc.ToArray());
                if(dist_opt < min_dist)
                {
                    min_dist = dist_opt;
                    traj_opt = clone_traj(traj_opt_loc);
                }
                Console.WriteLine("dist_opt: "+dist_opt);
            }

            //Console.WriteLine("Traj before___________");
            var dist_def = CompTrans(traj.ToArray());
            Console.WriteLine("dist_def: " + dist_def);

            return traj_opt;
        }
        static List<List<Point3d_GL>>  clone_traj(List<List<Point3d_GL>> tr)
        {
            var tr_cl = new List<List<Point3d_GL>>();
            for(int i = 0; i < tr.Count; i++)
            {
                var l_cl = new List<Point3d_GL>();
                for (int j = 0; j < tr[i].Count; j++)
                {
                    l_cl.Add(tr[i][j].Clone());
                }
                tr_cl.Add(l_cl);
            }
            return tr_cl;
        }

        static public TrajectoryPath OptimizeTranzitions2LayerPath(TrajectoryPath traj)
        {
            if (traj.layers.Count < 2) return traj;
            List<int[][]> approach = new List<int[][]>();

            for (int i = 0; i < traj.layers.Count; i++)
            {
                int s1 = 0;
                int s2 = 1;
                int e1 = -1;
                int e2 = -2;
                approach.Add(new int[][] { new int[] { s1, e1 }, new int[] { s2, e2 }, new int[] { e1, s1 }, new int[] { e2, s2 } });
            }

            List<double[][]> dists = new List<double[][]>();

            for (int i = 0; i < approach.Count; i++)
            {
                double[][] b = new double[approach[i].Length][];

                for (int j = 0; j < approach[i].Length; j++)
                {
                    double[] c = new double[approach[i].Length];
                    for (int k = 0; k < approach[i].Length; k++)
                    {
                        c[k] = 1000000000.0f;
                    }

                    b[j] = c;
                }

                dists.Add(b);
            }

            for (int i = 0; i < traj.layers.Count - 1; i++)
            {
                for (int layer1 = 0; layer1 < approach[i].Length; layer1++)
                {
                    for (int layer2 = 0; layer2 < approach[i + 1].Length; layer2++)
                    {
                        if (traj.layers[i] != null && traj.layers[i + 1] != null)
                        {
                            var p1 = traj.layers[i].getPoint(approach[i][layer1][1]);
                            var p2 = traj.layers[i+1].getPoint(approach[i + 1][layer2][0]);
                            dists[i][layer1][layer2] =
                                Distance(p1, p2);
                        }
                    }
                }
            }

            List<int> fastWay = new List<int>();

            /*for (int i = 0; i < dists.Count; i++)
            {
                int low, up;
                if (i == 0)
                {
                    (low, up) = FindBestWayFirst_old(dists[i]);
                    fastWay = new List<int> { low, up };
                }
                else
                {
                    (low, up) = FindBestWayCont(dists[i], fastWay[fastWay.Count - 1]);
                    fastWay.Add(up);
                }
            }*/

            for(int f = 0; f < 4; f++)
            {
                for (int i = 0; i < dists.Count; i++)
                {
                    int low, up;
                    if (i == 0)
                    {
                        (low, up) = FindBestWayFirst(dists[i], f);
                        fastWay = new List<int> { low, up };
                    }
                    else
                    {
                        (low, up) = FindBestWayCont(dists[i], fastWay[fastWay.Count - 1]);
                        fastWay.Add(up);
                    }
                }
                OptimizeTransPath(traj, approach.ToList(), fastWay);
            }
            
            return traj;
        }

        static  public (int,int) FindBestWayFirst(double[][] trans_map,int first)
        {
            int low = 0;
            int up = 0;
            double min_dist = 100000;
            
            for (int j = 0; j < trans_map[first].Length; j++)
            {
                if (trans_map[first][j] < min_dist)
                {
                    min_dist = trans_map[first][j];
                    low = first;
                    up = j;
                }
            }
            
            return (low, up);
        }

        static public (int, int) FindBestWayFirst_old(double[][] trans_map)
        {
            int low = 0;
            int up = 0;
            double min_dist = 100000;
            for (int i = 0; i < trans_map.Length; i++)
            {
                for (int j = 0; j < trans_map[i].Length; j++)
                {
                    if (trans_map[i][j] < min_dist)
                    {
                        min_dist = trans_map[i][j];
                        low = i;
                        up = j;
                    }
                }
            }
            return (low, up);
        }

        public static (int, int) FindBestWayCont(double[][] transMap, int prevL)
        {
            int low = prevL;
            int up = 0;
            double minDist = 100000;
            for (int j = 0; j < transMap[prevL].Length; j++)
            {
                if (transMap[prevL][j] < minDist)
                {
                    minDist = transMap[prevL][j];
                    up = j;
                }
            }
            return (low, up);
        }

        public static List<Point3d_GL>[] OptimizeTrans(List<Point3d_GL>[] traj, List<int[][]> approach, List<int> fastWay)
        {
            var optTraj = new List<Point3d_GL>[traj.Length];
            for (int i = 0; i < traj.Length; i++)
            {
                optTraj[i] = SetLayerDirection(traj[i], approach[i][fastWay[i]]);
            }
            return optTraj;
        }
        public static TrajectoryPath OptimizeTransPath(TrajectoryPath traj, List<int[][]> approach, List<int> fastWay)
        {
            var optTraj = new List<LayerPath>();
            for (int i = 0; i < traj.layers.Count; i++)
            {
                optTraj.Add( SetLayerDirectionPath(traj.layers[i], approach[i][fastWay[i]]));
            }
            return new TrajectoryPath { layers = optTraj };
        }
        public static double CompTrans(List<Point3d_GL>[] traj)
        {
            var trans = new List<double>();
            double allTR = 0;
            for (int i = 0; i < traj.Length - 1; i++)
            {
                if (traj[i] != null && traj[i + 1] != null)
                {
                    double dist = Distance(traj[i][traj[i].Count - 1], traj[i + 1][0]);
                    trans.Add(dist);
                    allTR += dist;
                }
            }
            return allTR;
        }

        public static List<Point3d_GL> SetLayerDirection(List<Point3d_GL> layer, int[] inds)
        {
            if (inds[0] == 0)
            {
                //nothing
            }
            else if (inds[0] == -1)
            {
                layer.Reverse();
            }
            else if (inds[0] == 1)
            {
                layer = ReverseLineDirect(layer);
            }
            else if (inds[0] == -2)
            {
                layer.Reverse();
                layer = ReverseLineDirect(layer);
            }
            return layer;
        }
        public static LayerPath SetLayerDirectionPath(LayerPath layer, int[] inds)
        {
            if (inds[0] == 0)
            {
                //nothing
            }
            else if (inds[0] == -1)
            {
                layer.lines.Reverse();
                layer.ReverseLines();
            }
            else if (inds[0] == 1)
            {
                layer.ReverseLines();
            }
            else if (inds[0] == -2)
            {
                layer.lines.Reverse();
            }
            return layer;
        }

        public static List<Point3d_GL> ReverseLineDirect(List<Point3d_GL> layer)
        {
            int i = 0;
            int stop = 0;
            if (layer.Count % 2 != 0)
            {
                stop = 1;
            }
            while (i < layer.Count - stop)
            {
                Point3d_GL lam = layer[i + 1].Clone();
                layer[i + 1] = layer[i].Clone();
                layer[i] = lam.Clone();
                i += 2;
            }
            return layer;
        }
        public static double Distance(Point3d_GL p1, Point3d_GL p2)
        {
            return (p1 - p2).magnitude();
        }
    }


    public  class PatternSettings
    {
        public double min_dist;
        public double Min_dist
        {
            get { return min_dist; }
            set { min_dist = value; }
        }
        public double dim_x;
        public double Dim_x
        {
            get { return dim_x; }
            set { dim_x = value; }
        }
        public double dim_y;
        public double Dim_y
        {
            get { return dim_y; }
            set { dim_y = value; }
        }
        public double arc_dist;
        public double Arc_dist
        {
            get { return arc_dist; }
            set { arc_dist = value; }
        }
        public double r;
        public double R
        {
            get { return r; }
            set { r = value; }
        }
        public double step;
        public double Step
        {
            get { return step; }
            set { step = value; }
        }
        public double angle;

        public double Angle
        {
            get { return angle; }
            set { angle = value; }
        }

        public double angle_layers;

        public double Angle_layers
        {
            get { return angle_layers; }
            set { angle_layers = value; }
        }
        public double filling;

        public double Filling
        {
            get { return filling; }
            set { filling = value; }
        }
        public PathPlanner.PatternType patternType;
        public PathPlanner.PatternType PatternType
        {
            get { return patternType; }
            set { patternType = value; }
        }
        public bool start_dir_r;
        public bool Start_dir_r
        {
            get { return start_dir_r; }
            set { start_dir_r = value; }
        }
    }


}
