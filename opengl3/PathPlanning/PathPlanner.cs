using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using Emgu.CV;
using opengl3;

namespace PathPlanning
{
    public class PathPlanner
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

        public static List<Matrix<double>> join_traj(List<List<Matrix<double>>> traj)
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
            for (int i=0;i < trajParams.layers; i++)
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


        static Matrix<double> proj_point(Polygon3d_GL polyg, Point3d_GL point,Vector3d_GL vec_x_dir)
        {

            /* var vec_y = (polyg.flat3D.n | vec_x_dir).normalize();
             //Console.WriteLine(vec_y);
             var vec_x = (vec_y | polyg.flat3D.n).normalize();
             //Console.WriteLine(vec_x);
             var vec_z = polyg.flat3D.n;
             //Console.WriteLine(vec_z);
             //Console.WriteLine("_________________");
             */
            var n = polyg.flat3D.n;
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

        static List<Matrix<double>> project_layer(Polygon3d_GL[] surface, List<Point3d_GL> layer, RasterMap map_xy, Vector3d_GL vec_x_dir)
        {
            var layer_3d = new List<Matrix<double>>();
            for (int i = 0; i < layer.Count; i++)
            {
               // if(i>0) vec_x_dir = layer[i] - 
                var polyg_ind = map_xy.get_polyg_ind_prec_xy(layer[i], surface);
                var polyg = surface[polyg_ind];
                layer_3d.Add(proj_point(polyg, layer[i],vec_x_dir));
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
            return traj_3d;
        }
        public static List<Point3d_GL> project_contour_on_surface(Polygon3d_GL[] surface, List<Point3d_GL> contour, TrajParams trajParams)
        {
            double resolut = 0.2;
            var map_xy = new RasterMap(surface, resolut, RasterMap.type_map.XY);
            var proj_c = project_layer(surface, contour, map_xy, new Vector3d_GL(1,0,0));
            
            return matr_to_ps(proj_c);
        }

        public static string generate_robot_traj(List<Matrix<double>> traj, RobotFrame.RobotType type_robot)
        {
            var traj_rob = new List<RobotFrame>();
            double v = 20;
            for (int i = 0; i < traj.Count; i++)
            {
                traj_rob.Add(new RobotFrame(traj[i], v,type_robot));
            }
            traj_rob = RobotFrame.smooth_angle(traj_rob, 5);

            return RobotFrame.generate_string(traj_rob.ToArray());
        }

        public static List<Point3d_GL> matr_to_ps(List<Matrix<double>> traj)
        {
            var traj_rob = new List<Point3d_GL>();

            for (int i = 0; i < traj.Count; i++)
            {
                var f = new RobotFrame(traj[i], 20);
                traj_rob.Add(new Point3d_GL(f.X, f.Y, f.Z));
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

            for (int i = 0; i < dists.Count; i++)
            {
                int low, up;
                if (i == 0)
                {
                    (low, up) = FindBestWayFirst(dists[i]);
                    fastWay = new List<int> { low, up };
                }
                else
                {
                    (low, up) = FindBestWayCont(dists[i], fastWay[fastWay.Count - 1]);
                    fastWay.Add(up);
                }
            }

            Console.WriteLine("Traj before___________");
            CompTrans(traj.ToArray());

            Console.WriteLine("Traj after___________");
            CompTrans(OptimizeTrans(traj.ToArray(), approach.ToList(), fastWay));
            return traj;
        }

        static  public (int,int) FindBestWayFirst(double[][] trans_map)
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

        public static void CompTrans(List<Point3d_GL>[] traj)
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
            Console.WriteLine(allTR);
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
}
