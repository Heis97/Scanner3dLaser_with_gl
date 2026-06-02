using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.UI;
using Emgu.CV;
using Accord;
using Emgu.CV.Stitching;

namespace opengl3
{
    class NavigTool
    {
        public enum ToolType { tp4_v1 };
        Point3d_GL[] ps;
        int[] inds;
        int[] ind_aruco;
        int aruco_corner_num;
        int aruco_number = 0;
        Point3d_GL tcp;
        Point3d_GL rotate;
        public Matrix<double> matrix_frame;
        Matrix<double> matrix_tcp = new Matrix<double>(new double[,] {
                {1,0,0,0 },
                {0,1,0,0 },
                {0,0,1,0 },
                {0,0,0,1 }});
        ToolType tool_type;
        /*
         * x down
         * y right 
         * 
         * y-> 
         * p1----------p0
         * 
         * x
         * ||    
         * \/
         * 
         * p2-----------p3

         * */
        public NavigTool(int[] _inds, ToolType _tool_type) 
        {
            if (_inds == null) { Console.WriteLine("NavigTool _inds == null"); return ; }
            aruco_number = _inds.Length;
            inds = new int[aruco_number];
            for (int i =0; i<_inds.Length;i++)
            {
                inds[i] = _inds[i];
            }
            tool_type = _tool_type;
            if (_tool_type == ToolType.tp4_v1)
            {
                aruco_corner_num = 4;
                if (aruco_number != 4)  Console.WriteLine("NavigTool inds.Length != 4 for ToolType.tp4_v1");
            }
            ind_aruco = comp_ind_table(inds);
        }
        static int[] comp_ind_table(int[] inds)
        {
            var max_ind = int.MinValue;
            for (int i = 0; i < inds.Length; i++)
            {
                max_ind = Math.Max(max_ind, inds[i]);
            }
            var table = new int [max_ind+1];
            for (int i = 0; i < inds.Length; i++)
            {
                table[inds[i]] = i;
            }
            return table;
        }
        public double calibrate_markers(Point3d_GL[][][] ps_cal)//detect markerspos relative p1  //ps not filterd
        {
            var ps = filter_ps_array(ps_cal);
            if(ps == null ) return -1;
            if(ps.Length <4 ) return -1;


            return 0;
        }
        public Point3d_GL calibrate_tcp_4p(Point3d_GL[][][] ps_cal)//ps not filterd
        {
            var ps = filter_ps_array(ps_cal);
            if (ps == null) return Point3d_GL.notExistP();
            if (ps.Length < 5) return Point3d_GL.notExistP();
            var ms = new List<Matrix<double>>();
            var ps_calc = new List<Point3d_GL>();
            for (int i = 0; i < 5; i++)
            {
                var m = get_frame(ps[i]);
                ms.Add(m);
                ps_calc.Add(new Point3d_GL(m[0, 3], m[1, 3], m[2, 3]));
            }


            var p_cenr = find_center_sphere_4p(ps_calc)[0];

            var p_cenr_2 = find_center_sphere_4p(ps_calc.GetRange(1,4))[0];


            Console.WriteLine("p_cenr: " + p_cenr);
            Console.WriteLine("p_cenr2: " + p_cenr_2);

            var tcp_aver = new Matrix<double>(new double[,] {
                {1,0,0,0 },
                {0,1,0,0 },
                {0,0,1,0 },
                {0,0,0,1 }});

            for (int i = 0; i < 4; i++)
            {
                var m_inv = ms[i].Clone();
                CvInvoke.Invert(m_inv, m_inv, DecompMethod.Eig);
                var tcp = new Matrix<double>(new double[,] {
                {1,0,0,p_cenr.x },
                {0,1,0,p_cenr.y },
                {0,0,1,p_cenr.z },
                {0,0,0,1 }});

                tcp_aver += tcp* m_inv;// m_inv* tcp
            }

            tcp_aver = tcp_aver / 4;

            this.matrix_tcp = tcp_aver;

            this.tcp = new Point3d_GL(tcp_aver[0, 3], tcp_aver[1, 3], tcp_aver[2, 3]);

            return this.tcp;
        }

        public static Point3d_GL[] find_center_sphere_4p(List<Point3d_GL> points)
        {
            if (points == null || points.Count < 4)
                throw new ArgumentException("Необходимо минимум 4 точки.");

            double x1 = points[0].x, y1 = points[0].y, z1 = points[0].z;
            double x2 = points[1].x, y2 = points[1].y, z2 = points[1].z;
            double x3 = points[2].x, y3 = points[2].y, z3 = points[2].z;
            double x4 = points[3].x, y4 = points[3].y, z4 = points[3].z;

            // Вспомогательные величины U, V, W
            double U = (z1 - z2) * (x3 * y4 - x4 * y3) - (z2 - z3) * (x4 * y1 - x1 * y4);
            double V = (z3 - z4) * (x1 * y2 - x2 * y1) - (z4 - z1) * (x2 * y3 - x3 * y2);
            double W = (z1 - z3) * (x4 * y2 - x2 * y4) - (z2 - z4) * (x1 * y3 - x3 * y1);

            double denominator = U + V + W;
            if (Math.Abs(denominator) < 1e-12)
                throw new InvalidOperationException("Точки компланарны или сфера не определена.");

            // Вычисление компонент центра (Ax, Bx, Cx, Dx) и т.д.
            double squared1 = x1 * x1 + y1 * y1 + z1 * z1;
            double squared2 = x2 * x2 + y2 * y2 + z2 * z2;
            double squared3 = x3 * x3 + y3 * y3 + z3 * z3;
            double squared4 = x4 * x4 + y4 * y4 + z4 * z4;

            double Ax = squared1 * (y2 * (z3 - z4) + y3 * (z4 - z2) + y4 * (z2 - z3));
            double Bx = squared2 * (y3 * (z4 - z1) + y4 * (z1 - z3) + y1 * (z3 - z4));
            double Cx = squared3 * (y4 * (z1 - z2) + y1 * (z2 - z4) + y2 * (z4 - z1));
            double Dx = squared4 * (y1 * (z2 - z3) + y2 * (z3 - z1) + y3 * (z1 - z2));

            double Ay = squared1 * (x2 * (z3 - z4) + x3 * (z4 - z2) + x4 * (z2 - z3));
            double By = squared2 * (x3 * (z4 - z1) + x4 * (z1 - z3) + x1 * (z3 - z4));
            double Cy = squared3 * (x4 * (z1 - z2) + x1 * (z2 - z4) + x2 * (z4 - z1));
            double Dy = squared4 * (x1 * (z2 - z3) + x2 * (z3 - z1) + x3 * (z1 - z2));

            double Az = squared1 * (x2 * (y3 - y4) + x3 * (y4 - y2) + x4 * (y2 - y3));
            double Bz = squared2 * (x3 * (y4 - y1) + x4 * (y1 - y3) + x1 * (y3 - y4));
            double Cz = squared3 * (x4 * (y1 - y2) + x1 * (y2 - y4) + x2 * (y4 - y1));
            double Dz = squared4 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));

            double x0 = 0.5 * (Ax - Bx + Cx - Dx) / denominator;
            double y0 = -0.5 * (Ay - By + Cy - Dy) / denominator;
            double z0 = 0.5 * (Az - Bz + Cz - Dz) / denominator;

            double radius = Math.Sqrt((x1 - x0) * (x1 - x0) +
                                      (y1 - y0) * (y1 - y0) +
                                      (z1 - z0) * (z1 - z0));

            return new Point3d_GL[]
            {
            new Point3d_GL(x0, y0, z0),
            new Point3d_GL(radius, radius, radius)
            };
        }

        public double calibrate_rot(Point3d_GL[][][] ps_cal)//ps not filterd
        {
            var ps = filter_ps_array(ps_cal);

            return 0;
        }

        public Matrix<double> get_frame(Point3d_GL[][] ps_finded)//ps not filterd
        {
            var ps = filter_ps( ps_finded);
            if (!check_aruko_ps(ps)) { return null; } 
            if (tool_type == ToolType.tp4_v1)
            {
                return get_frame_tr4_v1(ps);
            }

            return null;
        }
        public Matrix<double> get_frame_tcp(Point3d_GL[][] ps_finded)//ps not filterd
        {
            var ps = filter_ps(ps_finded);
            if (!check_aruko_ps(ps)) { return null; }
            if (tool_type == ToolType.tp4_v1)
            {

                return get_frame_tr4_v1(ps) * matrix_tcp ;
            }

            return null;
        }
        /*
         * x down
         * y right 
         * 
         * x - true axis
         * y-> 
         * p1----------p0
         * 
         * x
         * ||    
         * \/
         * 
         * p2-----------p3
         * */
        public Matrix<double> get_frame_tr4_v1(Point3d_GL[][] ps_finded)//ps filterd
        {
            var p0 = Point3d_GL.centr_mass(ps_finded[0]);
            var p1 = Point3d_GL.centr_mass(ps_finded[1]);
            var p2 = Point3d_GL.centr_mass(ps_finded[2]);
            var p3 = Point3d_GL.centr_mass(ps_finded[3]);

            var vx = (p2 - p1).normalize();
            var vyx = (p0 - p1).normalize();

            var vz = vx | vyx;
            var vy = vz | vx;
            matrix_frame = RobotFrame.matrix_assemble(vx, vy, vz, p1);
            return matrix_frame;
        }
        Point3d_GL[][][] filter_ps_array(Point3d_GL[][][] ps)
         {
            var ps_filtr = new List<Point3d_GL[][]>();
            for(int i = 0; i < ps.Length; i++)
            {
                var frame_ps_filtr = filter_ps(ps[i]);

                if(check_aruko_ps(frame_ps_filtr))
                {
                    ps_filtr.Add(frame_ps_filtr);
                }                
            }
            return ps_filtr.ToArray();
        }

        Point3d_GL[][] filter_ps(Point3d_GL[][] ps)
        {
            var ps_filtr = new Point3d_GL[aruco_number][];
            for (int i = 0; i < ps_filtr.Length; i++)
            {
                ps_filtr[i] = new Point3d_GL[aruco_corner_num];
                for (int j = 0; j < ps_filtr[i].Length; j++)
                {

                    ps_filtr[i][j] = Point3d_GL.notExistP();
                }

            }

            for (int i = 0; i< ps.Length; i++)
            {
                if (ps[i]!=null)
                {
                    var cur_aruco_ind = ps[i][0].ind;
                    if (inds.Contains(cur_aruco_ind))
                    {
                        for (int j = 0; j < ps[i].Length; j++)
                        {
                            if (ps[i][j].ind_sec < aruco_corner_num)
                            {
                                ps_filtr[ind_aruco[cur_aruco_ind]][ps[i][j].ind_sec] = ps[i][j];
                            }
                        }
                    }
                }
                
            }



            return ps_filtr;
        }

        static bool check_aruko_ps(Point3d_GL[][] ps)
        {
            bool check_done = true;
            for (int i = 0; i < ps.Length; i++)
            {
                if(ps[i]!=null)
                {
                    for (int j = 0; j < ps[i].Length; j++)
                    {

                        if (!ps[i][j].exist) check_done = false;
                    }
                }
                else
                {
                    check_done = false;
                }

            }

            return check_done;
        }



    }
    class NavigSys
    {

    }
}
