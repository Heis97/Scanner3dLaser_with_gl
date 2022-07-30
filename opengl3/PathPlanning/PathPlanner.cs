using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using opengl3;

namespace PathPlanning
{
    class PathPlanner
    {

        public static List<Point3d_GL> traj_to_matr(List<Matrix<double>> matrs)
        {
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

        static List<Matrix<double>> join_traj(List<List<Matrix<double>>> traj)
        {
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
                traj_j.AddRange(traj_to_matr(traj[i]));
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
                div_step = 0.3,
                h_transf = 3,
                layers = amount,
                layers_angle = Math.PI / 2,
                step = dz * 3,
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
        static Point3d_GL rotate_point(Point3d_GL p, double angle)
        {
            var x_r = p.x * Math.Cos(angle) - p.y * Math.Sin(angle);
            var y_r = p.x * Math.Sin(angle) + p.y * Math.Cos(angle);
            return new Point3d_GL(x_r,y_r,p.z);
        }
        static List<Point3d_GL> rotate_list_points (List<Point3d_GL>  traj, double angle)
        {
            var traj_rot = new List<Point3d_GL>();
            for (int i = 0; i < traj.Count; i++)
            {
                traj_rot.Add(rotate_point(traj[i], angle));
            }
            return traj_rot;
        }
        static List<Point3d_GL> GeneratePositionTrajectory_angle(List<Point3d_GL> contour, double step, double angle)
        {
            var contour_rotate = rotate_list_points(contour, angle);
            var traj = GeneratePositionTrajectory(contour_rotate, step);
            var traj_rotate = rotate_list_points(traj, -angle);
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
        public static List<List<Point3d_GL>> Generate_multiLayer2d_mesh(List<List<Point3d_GL>> contour, TrajParams trajParams)
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

        static List<Point3d_GL> filter_traj(List<Point3d_GL> layer, double fil_step)
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
        static List<Point3d_GL> divide_traj(List<Point3d_GL> layer,double div_step)
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


        static Matrix<double> proj_point(Polygon3d_GL polyg, Point3d_GL point)
        {
            var vec_x_dir = new Vector3d_GL(-1, 0, 0);
            var vec_y = (polyg.flat3D.n | vec_x_dir).normalize();
            var vec_x = (vec_y | polyg.flat3D.n).normalize();
            var vec_z = polyg.flat3D.n;
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
        static List<Matrix<double>> project_layer(Polygon3d_GL[] surface, List<Point3d_GL> layer, RasterMap map_xy)
        {
            var layer_3d = new List<Matrix<double>>();
            for (int i = 0; i < layer.Count; i++)
            {
                var polyg_ind = map_xy.get_polyg_ind_prec(layer[i], surface);
                var polyg = surface[polyg_ind];
                layer_3d.Add(proj_point(polyg, layer[i]));
            }
            return layer_3d;
        }

        static List<List<Matrix<double>>> add_transit(List<List<Matrix<double>>> traj, double trans_h)
        {
            for(int i=0; i< traj.Count;i++)
            {
                var last = traj[i][traj[i].Count - 1].Clone();
                last[2, 3] += trans_h;
                traj[i].Add(last);
                var first = traj[i][0].Clone();
                first[2, 3] += trans_h;
                traj[i].Insert(0, first);
            }
            return traj;
        }
        public static List<List<Matrix<double>>> Generate_multiLayer3d_mesh(Polygon3d_GL[] surface, List<List<Point3d_GL>> contour, TrajParams trajParams)
        {
            var traj_2d = Generate_multiLayer2d_mesh(contour, trajParams);
            var traj_3d = new List<List<Matrix<double>>>();
            double resolut = 0.2;
            var map_xy = new RasterMap(surface, resolut);
            for (int i=0; i<traj_2d.Count;i++)
            {
                var traj_df = filter_traj(divide_traj(traj_2d[i], trajParams.div_step), trajParams.div_step / 2);
                traj_3d.Add(project_layer(surface, traj_df, map_xy));
            }
            traj_3d = add_transit(traj_3d, trajParams.h_transf);
            return traj_3d;
        }



        public static string generate_robot_traj(List<Matrix<double>> traj)
        {
            var traj_rob = new StringBuilder();
            double v = 20;
            for(int i = 0; i<traj.Count; i++)
            {
                var x = traj[i][0, 3];
                var y = traj[i][1, 3];
                var z = traj[i][2, 3];
                double b = Math.Asin(traj[i][2, 0]);
                double a = 0;
                double c = 0;
                if( Math.Cos(b)!=0)
                {
                    c = -Math.Asin(traj[i][2, 1] / Math.Cos(b));
                    a =  Math.Asin(traj[i][1, 0] / Math.Cos(b));
                }
                int e = (int)traj[i][3, 3];

                traj_rob.Append(
                    " X" + round(x) + ", Y" + round(y) + ", Z" + round(z) +
                    ", A" + round(a) + ", B" + round(b) + ", C" + round(c) + 
                    ", V" + round(v) + ", D" + e+" \n"
                    );

            }
            return traj_rob.ToString();
        }

        static double round(double val)
        {
            return Math.Round(val,4);
        }
    }


    class TrajParams
    {
        public double step;
        public double layers;
        public double div_step;
        public double layers_angle;
        public double[] z;
        public double h_transf;
        public TrajParams(double _step, double _layers, double _div_step, double _layers_angle, double[] _z, double _h_transf)
        {
            step = _step;
            layers = _layers;
            div_step = _div_step;
            layers_angle = _layers_angle;
            z = _z;
            h_transf = _h_transf;
        }
        public TrajParams()
        {

        }
    }
}
