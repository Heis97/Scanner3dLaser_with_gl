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
using Emgu.CV.Aruco;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using System.Runtime.CompilerServices;
using Emgu.CV.Dnn;
using OpenGL;
using System.Threading;
using System.Text.Json;
using Emgu.CV.XFeatures2D;

namespace opengl3
{
    public class NavigTarget
    {
        public NavigTarget()
        {
            update_intrisic_param();
        }
        public float[] color = new float[3] {0.1f,0.1f,0.1f};
        [Description("Цвет")]
        [Category("Название")]
        [DisplayName("Цвет")]
        public float[] Color
        {
            get { return color; }
            set { color = value; }
        }

        public string name;
        [Description("Название")]
        [Category("Название")]
        [DisplayName("Name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        public double x;
        [Description("Координата X")]
        [Category("Положение")]
        [DisplayName("X")]
        [Editor(typeof(DoubleUpDownEditor), typeof(UITypeEditor))]
        public double X 
        {
            get { return x; }
            set { x = value; update_intrisic_param(); }
        }

        public double y;
        [Description("Координата Y")]
        [Category("Положение")]
        [DisplayName("Y")]
        [Editor(typeof(DoubleUpDownEditor), typeof(UITypeEditor))]
        public double Y
        {
            get { return y; }
            set { y = value; update_intrisic_param(); }
        }

        public double z;
        [Description("Координата Z")]
        [Category("Положение")]
        [DisplayName("Z")]
        [Editor(typeof(DoubleUpDownEditor), typeof(UITypeEditor))]
        public double Z
        {
            get { return z; }
            set { z = value; update_intrisic_param(); }
        }

        public double a;
        [Description("Координата A")]
        [Category("Положение")]
        [DisplayName("A")]
        [Editor(typeof(DoubleUpDownEditorAngle), typeof(UITypeEditor))]
        public double A
        {
            get { return a; }
            set { a = value; update_intrisic_param(); }
        }

        public double b;
        [Description("Координата B")]
        [Category("Положение")]
        [DisplayName("B")]
        public double B
        {
            get { return b; }
            set { b = value; update_intrisic_param(); }
        }

        public double c;
        [Description("Координата C")]
        [Category("Положение")]
        [DisplayName("C")]
        public double C
        {
            get { return c; }
            set { c = value; update_intrisic_param(); }
        }

        public double d = 3;
        [Description("размер D")]
        [Category("Размер")]
        [DisplayName("D")]
        public double D
        {
            get { return d; }
            set { d = value; }
        }

        public double l = 50;
        [Description("размер L")]
        [Category("Размер")]
        [DisplayName("L")]
        public double L
        {
            get { return l; }
            set { l = value; update_intrisic_param(); }
        }

        public Matrix<double> matr = UtilMatr.eye_matr(4);
        public Point3d_GL p1;
        public Point3d_GL p2;

        public void update_intrisic_param()
        {
            p1 = new Point3d_GL(x, y, z);   
            p2 = new Point3d_GL(0, 0, l);
            matr  = UtilMatr.matrix_cv(new Point3d_GL(x,y,z),new Point3d_GL(a,b,c));
            p2 = matr * p2;

        }
        public override string ToString()
        {
            return Name; // Это будет использоваться для отображения в TreeView
        }   

    }

    public class RobotClient : TcpClientWrapper
    {
        public event Action<string> FrameUpdated;  // теперь имя события отражает суть
        string cur_pos = "";
        private string _currentFrame;
        public string CurrentFrame => _currentFrame;
        public int _messageCounter = 0;
        private DateTime _lastStatsTime = DateTime.UtcNow;
        protected override void OnMessageReceived(string message)
        {
            //Interlocked.Increment(ref _messageCounter);
            if (message != null)
            {
                _currentFrame = message;
                FrameUpdated?.Invoke(message);

            }
            _currentFrame = message;// Console.WriteLine($"Получено: {message}");

           
           /* try
            {
                var frame = JsonSerializer.Deserialize<RobotFrame>(message);
                if (frame != null)
                {
                    _currentFrame = frame;
                    FrameUpdated?.Invoke(frame);
                }
            }
            catch {  }*/
        }

        public async Task SendFrameAsync(RobotFrame frame)
        {
            string json = JsonSerializer.Serialize(frame);
            await SendAsync(json);
        }

    }

    class NavigTool
    {
        public enum ToolType { tp4_v1, tp1_v1 };
        public Point3d_GL[] ps;
        public List<Point3d_GL> trace_tcp = new List<Point3d_GL>();
        int[] inds;
        int[] ind_aruco;
        int aruco_corner_num=4;
        int aruco_number = 0;
        Point3d_GL tcp;
        Point3d_GL rotate;
        NavigSys NavigSys;

        public Matrix<double> matrix_frame = new Matrix<double>(new double[,] {//frame markers
                {1,0,0,0 },
                {0,1,0,0 },
                {0,0,1,0 },
                {0,0,0,1 }});
        public Matrix<double> matrix_frame_inv;
        public Matrix<double> matrix_model;
        public Matrix<double> matrix_model_inv;

        public Matrix<double> matrix_model_debug = new Matrix<double>(new double[,] {//3d model markers
                {1,0,0,0 },
                {0,1,0,0 },
                {0,0,1,0 },
                {0,0,0,1 }});

        public string name_3d_model = "new_tool";
        public string name_3d_model_debug = "new_debug_tool";
        public string name_3d_model_trace_tcp = "new_trace_tcp";
        public string path_3d_model = null;
        public Matrix<double> matrix_tcp = new Matrix<double>(new double[,] {
                {1,0,0,0 },
                {0,1,0,0 },
                {0,0,1,0 },
                {0,0,0,1 }});
        public ToolType tool_type;
        public List<List<Point3d_GL>> ps_for_registr = new List<List<Point3d_GL>>();

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

        public NavigTool(int[] _inds, ToolType _tool_type,string _name_3d_model=null, string _path_3d_model = null, Matrix<double> _matrix_model_debug = null, string _name_3d_model_debug = null) 
        {
            if (_inds == null) { Console.WriteLine("NavigTool _inds == null"); return; }
            aruco_number = _inds.Length;
            inds = new int[aruco_number];
            for (int i =0; i<_inds.Length;i++)
            {
                inds[i] = _inds[i];
            }
            tool_type = _tool_type;
            if (_tool_type == ToolType.tp4_v1)
            {
                if (aruco_number != 4)  Console.WriteLine("NavigTool inds.Length != 4 for ToolType.tp4_v1");
            }
            if (_tool_type == ToolType.tp1_v1)
            {
                if (aruco_number != 1) Console.WriteLine("NavigTool inds.Length != 1 for ToolType.tp1_v1");
            }
            if (_name_3d_model != null)
            {
                name_3d_model = _name_3d_model;
            }
            if (_path_3d_model != null)
            {
                path_3d_model = _path_3d_model;
            }
            if (_matrix_model_debug != null && _name_3d_model_debug != null)
            {
                matrix_model_debug = _matrix_model_debug;
                name_3d_model_debug = _name_3d_model_debug;
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
        public Point3d_GL calibrate_tcp_4p(Point3d_GL[][][] ps_cal)//ps filterd
        {
            var ps = ps_cal;
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


            //Console.WriteLine("p_cenr: " + p_cenr);
            //Console.WriteLine("p_cenr2: " + p_cenr_2);

            var tcp_aver = new Matrix<double>(new double[,] {
                {0,0,0,0 },
                {0,0,0,0 },
                {0,0,0,0 },
                {0,0,0,0 }});
            int ps_count = 5;

            var tcp_list = new List<Matrix<double>>();
            for (int i = 0; i < ps_count; i++)
            {
                var m_inv = ms[i].Clone();

                CvInvoke.Invert(m_inv, m_inv, DecompMethod.Svd);

                var m_test = m_inv * ms[i];
                var tcp = new Matrix<double>(new double[,] {
                {1,0,0,p_cenr.x },
                {0,1,0,p_cenr.y },
                {0,0,1,p_cenr.z },
                {0,0,0,1 }});
                var m_tcp = m_inv * tcp;
                tcp_list.Add(m_tcp);
                tcp_aver += m_tcp;// m_inv* tcp
                //Console.WriteLine("m_inv * tcp");
                //prin.t(m_inv * tcp);

            }
            tcp_aver = tcp_aver / ps_count;
            var p_err = new Point3d_GL();

            for (int i = 0;i < ps_count;i++)
            {
                var p_i = new Point3d_GL(
                    Math.Abs(tcp_aver[0, 3] - tcp_list[i][0, 3]),
                    Math.Abs(tcp_aver[1, 3] - tcp_list[i][1, 3]),
                    Math.Abs(tcp_aver[2, 3] - tcp_list[i][2, 3]));
                p_err += p_i;
            }


            Console.WriteLine("Navig_calib p_err x y z: " + p_err/ps_count);



            this.matrix_tcp = tcp_aver;
            //Console.WriteLine("tcp_aver");
            //prin.t(tcp_aver);

            this.tcp = new Point3d_GL(tcp_aver[0, 3], tcp_aver[1, 3], tcp_aver[2, 3]);

            return this.tcp;
        }
        public Point3d_GL calibrate_tool_tcp_4p(Point3d_GL[][][] ps_cal)//ps filterd
        {
            var ps = ps_cal;
            if (ps == null) return Point3d_GL.notExistP();
            if (ps.Length < 5) return Point3d_GL.notExistP();
            var ms = new List<Matrix<double>>();
            var ps_calc = new List<Point3d_GL>();
            for (int i = 0; i < 5; i++)
            {
                var m = get_frame(ps[i]);
                if (m == null) continue;
                ms.Add(m);
                ps_calc.Add(new Point3d_GL(m[0, 3], m[1, 3], m[2, 3]));
            }


            var p_cenr = find_center_sphere_4p(ps_calc)[0];

            var p_cenr_2 = find_center_sphere_4p(ps_calc.GetRange(1, 4))[0];


            //Console.WriteLine("p_cenr: " + p_cenr);
            //Console.WriteLine("p_cenr2: " + p_cenr_2);

            var tcp_aver = new Matrix<double>(new double[,] {
                {0,0,0,0 },
                {0,0,0,0 },
                {0,0,0,0 },
                {0,0,0,0 }});
            int ps_count = 5;

            var tcp_list = new List<Matrix<double>>();
            for (int i = 0; i < ps_count; i++)
            {
                var m_inv = ms[i].Clone();

                CvInvoke.Invert(m_inv, m_inv, DecompMethod.Svd);

                var m_test = m_inv * ms[i];
                var tcp = new Matrix<double>(new double[,] {
                {1,0,0,p_cenr.x },
                {0,1,0,p_cenr.y },
                {0,0,1,p_cenr.z },
                {0,0,0,1 }});
                var m_tcp = m_inv * tcp;
                tcp_list.Add(m_tcp);
                tcp_aver += m_tcp;// m_inv* tcp
                //Console.WriteLine("m_inv * tcp");
                //prin.t(m_inv * tcp);

            }
            tcp_aver = tcp_aver / ps_count;
            var p_err = new Point3d_GL();

            for (int i = 0; i < ps_count; i++)
            {
                var p_i = new Point3d_GL(
                    Math.Abs(tcp_aver[0, 3] - tcp_list[i][0, 3]),
                    Math.Abs(tcp_aver[1, 3] - tcp_list[i][1, 3]),
                    Math.Abs(tcp_aver[2, 3] - tcp_list[i][2, 3]));
                p_err += p_i;
            }


            Console.WriteLine("Navig_calib p_err x y z: " + p_err / ps_count);



            this.matrix_tcp = tcp_aver;
            //Console.WriteLine("tcp_aver");
            //prin.t(tcp_aver);

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
            //if (!check_aruko_ps(ps)) { return null; } 
            if (tool_type == ToolType.tp4_v1)
            {
                return get_frame_tr4_v1(ps);
            }
            else if (tool_type == ToolType.tp1_v1)
            {
                return get_frame_tr1_v1(ps);
            }
            return null;
        }
        public Matrix<double> get_frame_tcp(Point3d_GL[][] ps_finded)//ps not filterd
        {
            var ps = filter_ps(ps_finded);
            if (!check_aruko_ps(ps)) { return null; }
            if (tool_type == ToolType.tp4_v1)
            {
                get_frame_tr4_v1(ps);
                return  matrix_model;
            }
            if (tool_type == ToolType.tp1_v1)
            {
                get_frame_tr1_v1(ps);
                return matrix_model;
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

            var vz = (vx | vyx).normalize();
            var vy = (vz | vx).normalize();
            matrix_frame = RobotFrame.matrix_basis_from_ps(new Point3d_GL[] { p0, p1, p2 });
            matrix_frame_inv = matrix_frame.Clone();
            CvInvoke.Invert(matrix_frame, matrix_frame_inv, DecompMethod.LU);

            if (tool_type == ToolType.tp4_v1)
            {
                matrix_model = matrix_frame * matrix_tcp ;
            }
            matrix_model_inv = matrix_model.Clone();
            CvInvoke.Invert(matrix_model, matrix_model_inv, DecompMethod.LU);

            ps = new Point3d_GL[] { p0, p1, p2, p3,new Point3d_GL(matrix_model[0,3], matrix_model[1, 3], matrix_model[2, 3]) };
            return matrix_frame;
        }

        public Matrix<double> get_frame_tr1_v1(Point3d_GL[][] ps_finded)//ps filterd
        {
            var p0 = ps_finded[0][0];
            var p1 = ps_finded[0][3];
            var p2 = ps_finded[0][2];
            var p3 = ps_finded[0][1];

            var vx = (p2 - p1).normalize();
            var vyx = (p0 - p1).normalize();

            var vz = (vx | vyx).normalize();
            var vy = (vz | vx).normalize();
            matrix_frame = RobotFrame.matrix_basis_from_ps(new Point3d_GL[] { p0, p1, p2 });
            matrix_frame_inv = matrix_frame.Clone();
            CvInvoke.Invert(matrix_frame, matrix_frame_inv, DecompMethod.LU);

            if (tool_type == ToolType.tp1_v1)
            {
                matrix_model = matrix_frame * matrix_tcp;
            }
            matrix_model_inv = matrix_model.Clone();
            CvInvoke.Invert(matrix_model, matrix_model_inv, DecompMethod.LU);
            var ps_ext = gen_3d_ps_other_corners(new Point3d_GL(9, 9, 0), 50, 100, matrix_frame);
            ps = new Point3d_GL[] { p0, p1, p2, p3, new Point3d_GL(matrix_model[0, 3], matrix_model[1, 3], matrix_model[2, 3]) };
            var ps_l = ps.ToList();
            ps_l.AddRange(ps_ext);
            ps = ps_l.ToArray();

            

            return matrix_frame;
        }

        static Point3d_GL[] gen_3d_ps_other_corners(Point3d_GL pc, double marker_size, double corner_lin_dist, Matrix<double> matrix_frame)
        {
            var ps_marker = new Point3d_GL[] { new Point3d_GL(0, 0, 0), new Point3d_GL(marker_size, 0, 0), new Point3d_GL(marker_size, marker_size, 0), new Point3d_GL(0, marker_size, 0) };
            var pc_marker = new Point3d_GL(marker_size/2, marker_size / 2, 0);
            var ps_corn = Point3d_GL.add_arr(ps_marker, pc - pc_marker);

            var ps_corn_x = Point3d_GL.add_arr(ps_corn, new Point3d_GL(corner_lin_dist,0,0));
            var ps_corn_y = Point3d_GL.add_arr(ps_corn, new Point3d_GL( 0, corner_lin_dist, 0));
            var ps_corn_xy = Point3d_GL.add_arr(ps_corn, new Point3d_GL(corner_lin_dist, corner_lin_dist, 0));

            var ps_l = new List<Point3d_GL>();
            ps_l.AddRange(ps_corn_x);
            ps_l.AddRange(ps_corn_y);
            ps_l.AddRange(ps_corn_xy);
            var ps_arr = Point3d_GL.multMatr( ps_l.ToArray() , matrix_frame);
            return ps_arr;
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
                if (ps[i]!=null && ps[i].Length!=0)
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


        public void add_point_for_registr(int ind_p,Point3d_GL p)
        {
            if(ind_p<ps_for_registr.Count)
            {
                ps_for_registr[ind_p].Add(p * matrix_frame_inv);
            }
        }

        public void init_points_for_registr(int count_p)
        {
            ps_for_registr = new List<List<Point3d_GL>>();
            for (int i = 0; i < count_p; i++)
            {
                ps_for_registr.Add(new List<Point3d_GL>());
            }
        }

        public Point3d_GL[] get_points_for_registr_draw()
        {
          return Point3d_GL.multMatr(Point3d_GL.unifPoints2d(ps_for_registr).ToArray(), matrix_model);
        }
    }
    class NavigSys
    {

        public NavigTool[] tools;
        public List<NavigTarget> targets;
        public Scanner stereo;
        int aruko_max_ind = 1;
        public NavigSys(Scanner _stereo, int _aruko_max_ind)
        {
            targets = new List<NavigTarget> ();
            stereo = _stereo;
            aruko_max_ind = _aruko_max_ind;
        }

        public Point3d_GL[][] navigation_processing_get_points3d(Mat _mat1, Mat _mat2,out Mat _mat_out1, out Mat _mat_out2)
        {

            System.Drawing.PointF[][] points_aruco1 = new System.Drawing.PointF[aruko_max_ind][];
            System.Drawing.PointF[][] points_aruco2 = new System.Drawing.PointF[aruko_max_ind][];

            Point3d_GL[][] points3d_aruco = new Point3d_GL[aruko_max_ind][];


            _mat_out1 = get_aruco_info(stereo.stereoCamera.cameraCVs[0].undist(_mat1.Clone()), ref points_aruco1);
            _mat_out2 = get_aruco_info(stereo.stereoCamera.cameraCVs[1].undist(_mat2.Clone()), ref points_aruco2);


            for (int i = 0; i < points_aruco1.Length; i++)
            {
                if (points_aruco1[i] != null && points_aruco2[i] != null)
                {
                    if (points_aruco1[i].Length != 0 && points_aruco2[i].Length != 0)
                    {
                        points3d_aruco[i] = stereo.stereoCamera.comp_points_3d(points_aruco1[i], points_aruco2[i], i);
                        //Console.WriteLine(i + " " + points3d_aruco[i][0].x + " " + points3d_aruco[i][0].z + " ");
                    }
                    else
                    {
                        points3d_aruco[i] = null;
                    }
                }
                else
                {
                    points3d_aruco[i] = null;
                }
            }
            return points3d_aruco;
        }
        public static Image<Bgr, byte> WarpArucoWithSurroundings(
        Image<Bgr, byte> image,
        int markerId,
        int markerOutputSize = 100,
        double marginFactor = 10,
        Dictionary.PredefinedDictionaryName dictionary = Dictionary.PredefinedDictionaryName.Dict4X4_50,
        Bgr backgroundColor = default)
        {
            if (marginFactor < 0)
                throw new ArgumentException("marginFactor должно быть >= 0.", nameof(marginFactor));

           // if (backgroundColor == default)
                backgroundColor = new Bgr(0, 0, 0);

            // 1. Детектируем маркеры
            var arucoDict = new Dictionary(dictionary);
            var detectorParams = DetectorParameters.GetDefault();

            var corners = new VectorOfVectorOfPointF();
            var ids = new VectorOfInt();
            var rejected = new VectorOfVectorOfPointF();

            ArucoInvoke.DetectMarkers(image, arucoDict, corners, ids, detectorParams, rejected);

            // 2. Ищем нужный ID
            int index = -1;
            for (int i = 0; i < ids.Size; i++)
            {
                if (ids[i] == markerId)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                throw new Exception($"Маркер с ID {markerId} не найден.");

            System.Drawing.PointF[] srcPoints = corners[index].ToArray();

            // 3. Вычисляем размер выходного изображения и координаты маркера в центре
            int outputSize = (int)Math.Round(markerOutputSize * (1 + 2 * marginFactor));
            int offset = (int)Math.Round(marginFactor * markerOutputSize);
            int markerSize = markerOutputSize; // маркер будет точно этого размера

            System.Drawing.PointF[] dstPoints = new System.Drawing.PointF[]
            {
            new System.Drawing.PointF(offset, offset),
            new System.Drawing.PointF(offset + markerSize - 1, offset),
            new System.Drawing.PointF(offset + markerSize - 1, offset + markerSize - 1),
            new System.Drawing.PointF(offset, offset + markerSize - 1)
            };

            // 4. Вычисляем гомографию и применяем перспективное преобразование
            Mat homography = CvInvoke.FindHomography(srcPoints, dstPoints, RobustEstimationAlgorithm.Ransac);

            Mat warpedMat = new Mat();
            MCvScalar borderColor = new MCvScalar(backgroundColor.Blue, backgroundColor.Green, backgroundColor.Red);
            CvInvoke.WarpPerspective(image, warpedMat, homography, new Size(outputSize, outputSize),
                Inter.Linear, Warp.Default, BorderType.Constant, borderColor);

            return warpedMat.ToImage<Bgr, byte>();
        }



        public void navigation_processing_draw_points3d(Point3d_GL[] ps, ref Mat _mat_out1, ref Mat _mat_out2)
        {
            

        }

        public NavigTool precise_frame_comp( NavigTool tool, ref Mat _mat_out1,ref Mat _mat_out2)
        {
            if (tool.tool_type != NavigTool.ToolType.tp1_v1) return tool;
            var ps1 = precise_points_circles(tool, _mat_out1, 0);
            var ps2 = precise_points_circles(tool, _mat_out2, 1);

            var ps3d  = stereo.stereoCamera.comp_points_3d(ps1, ps2);
            var ps_for_frame = new Point3d_GL[1][];
            if (ps3d == null) { return tool; }
            if(ps3d.Length != 3) { return tool; }
            var p4 = Point3d_GL.centr_mass(new Point3d_GL[] { tool.ps[0], tool.ps[1], tool.ps[2], tool.ps[3] });
            ps_for_frame[0] = new Point3d_GL[] { ps3d[1], ps3d[2],  ps3d[0], p4  };

            _mat_out1 = draw_points(ps_for_frame[0], _mat_out1, 0, new MCvScalar(255, 0, 0));
           // _mat_out2 = draw_points(ps_for_frame[0], _mat_out2, 1, new MCvScalar(0, 255, 0));
            tool.get_frame_tr1_v1(ps_for_frame);


            return tool;
        }
        public Mat draw_points(Point3d_GL[] ps, Mat _mat_out1, int ind, MCvScalar color)
        {
            var mat_orig = _mat_out1.Clone();
            if (ps == null || ps.Length == 0) return null;
            var ps_draw = new PointF[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {

                var p3d_cam_0 = stereo.stereoCamera.cameraCVs[0].matrixSC * ps[i];
                if (ind == 1)
                {
                    p3d_cam_0 = stereo.stereoCamera.R_inv * p3d_cam_0;
                }
                var p2d_0 = stereo.stereoCamera.cameraCVs[ind].point2DfromCam(p3d_cam_0);
                ps_draw[i] = new PointF(p2d_0.X, p2d_0.Y);
                

            }
            _mat_out1 = UtilOpenCV.drawPointsF(_mat_out1, ps_draw, (int)color.V2, (int)color.V1, (int)color.V0, 4, true);
            return _mat_out1;
        }
        public System.Drawing.PointF[] precise_points_circles(NavigTool tool, Mat _mat_out1, int ind)
        {
            var mat_orig = _mat_out1.Clone();
            var ps = tool.ps;
            if (ps == null || ps.Length == 0) return null;
            if (ps.Length != 17) return null;
            var ps2di_0 = new System.Drawing.Point[12];


            for (int i = 0; i < ps.Length; i++)
            {

                var p3d_cam_0 = stereo.stereoCamera.cameraCVs[0].matrixSC * ps[i];
                if(ind==1)
                {
                    p3d_cam_0 = stereo.stereoCamera.R_inv * p3d_cam_0;
                }
                var p2d_0 = stereo.stereoCamera.cameraCVs[ind].point2DfromCam(p3d_cam_0);
                var p2di_0 = new System.Drawing.Point((int)p2d_0.X, (int)p2d_0.Y);
                //CvInvoke.DrawMarker(_mat_out1, p2di_0, new MCvScalar(0, 0, 255), MarkerTypes.Cross, 5);

                if (i > 4)
                {
                    ps2di_0[i - 5] = p2di_0;
                }

            }

            var ps2di_0_arr = new System.Drawing.Point[3][];
            ps2di_0_arr[0] = new System.Drawing.Point[4] { ps2di_0[0], ps2di_0[1], ps2di_0[2], ps2di_0[3] };
            ps2di_0_arr[1] = new System.Drawing.Point[4] { ps2di_0[4], ps2di_0[5], ps2di_0[6], ps2di_0[7] };
            ps2di_0_arr[2] = new System.Drawing.Point[4] { ps2di_0[8], ps2di_0[9], ps2di_0[10], ps2di_0[11] };

            var ps2di_0_arr_c = new System.Drawing.PointF[] {
                new System.Drawing.PointF((ps2di_0_arr[0][0].X+ ps2di_0_arr[0][2].X)/2,( ps2di_0_arr[0][0].Y + ps2di_0_arr[0][2].Y)/2),
                new System.Drawing.PointF((ps2di_0_arr[1][0].X+ ps2di_0_arr[1][2].X)/2,( ps2di_0_arr[1][0].Y + ps2di_0_arr[1][2].Y)/2),
                new System.Drawing.PointF((ps2di_0_arr[2][0].X+ ps2di_0_arr[2][2].X)/2,( ps2di_0_arr[2][0].Y + ps2di_0_arr[2][2].Y)/2),};

            var _mat_out1n = KeepInsidePolygonsAndFillOutside(mat_orig, ps2di_0_arr, new Bgr(0, 0, 0));

            CvInvoke.CvtColor(_mat_out1n, _mat_out1n, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(_mat_out1n, _mat_out1n, new Size(5, 5), -1);
            CvInvoke.AdaptiveThreshold(_mat_out1n, _mat_out1n, 255, AdaptiveThresholdType.MeanC, ThresholdType.BinaryInv, 7, 7);


            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(_mat_out1n, contours, hier, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);
            //var conts = sameContours_cv(contours);

           // CvInvoke.CvtColor(_mat_out1, _mat_out1, ColorConversion.Gray2Bgr);

            contours = FindCircles.size_filter(contours, 20, 3800);
            contours = FindCircles.only_same_centres_illum(contours, 2);

            var conts = FindCircles.sameContours(contours, 0.3, 1);
            conts = FindCircles.illum_same_centres(conts, 3);


            var psc = PointF.toPointF(FindCircles.get_pc_conts(conts));


            var ps2_predict = PointF.toPointF(ps2di_0_arr_c);
            if (psc == null) return null;
            if (psc.Length==0) return null;

            if (ps2_predict == null) return null;
            if (ps2_predict.Length == 0) return null;

            //UtilOpenCV.drawPointsF(_mat_out1, PointF.toSystemPoint(ps2_predict), 128, 128, 128, 3);

            //UtilOpenCV.drawPointsF(_mat_out1, PointF.toSystemPoint(psc), 255, 255, 0, 3);

            for (int i = 0; i < ps2_predict.Length; i++)
            {
                var j_min = 0;
                var dist_min = double.MaxValue;
                for (int j = 0; j < psc.Length; j++)
                {
                    var cur_dist = (ps2_predict[i] - psc[j]).norm;
                    if (cur_dist < dist_min)
                    {
                        j_min = j;
                        dist_min = cur_dist;
                    }
                   
                }
                ps2_predict[i] = psc[j_min];
            }


            var ps_result = PointF.toSystemPoint(ps2_predict);

            //UtilOpenCV.drawPointsF(_mat_out1, ps_result, 255, 255, 255, 3, true);

            return ps_result;
        }

        public static Mat draw_conts_with_charachts(Mat mat, VectorOfVectorOfPoint conts)
        {
            Random random = new Random();
            if (conts == null) return mat;
            if (conts != null) CvInvoke.DrawContours(mat, conts, -1, new MCvScalar(0, 255, 0), 1, LineType.EightConnected);
            Console.WriteLine("__________");
            for (int i = 0; i<conts.Size;i++)
            {
                var area = CvInvoke.ContourArea(conts[i]);
                var cont_pc = FindCircles.findCentrCont(conts[i]);
                var cont_fig = FindCircles.sumHuMom(conts[i]);
                CvInvoke.PutText(mat, " f: " + Math.Round(10*cont_fig, 2), new System.Drawing.Point((int)cont_pc.X , (int)cont_pc.Y ), FontFace.HersheyTriplex, 0.5, new MCvScalar(0, 0, 255));
                Console.WriteLine(" f: " + Math.Round(10 * cont_fig, 2));
            }
            
            return mat;
        }

        public static  Mat KeepInsidePolygonsAndFillOutside(Mat image, System.Drawing.Point[][] polygons, Bgr bgColor)
        {
            if (image == null)
                return image;
            if (image.Depth != DepthType.Cv8U || image.NumberOfChannels != 3)
                return image;

            // Если многоугольников нет — закрашиваем всё изображение
            if (polygons == null || polygons.Length == 0)
            {
                image.SetTo(new MCvScalar(bgColor.Blue, bgColor.Green, bgColor.Red));
                return image;
            }

            int width = image.Width;
            int height = image.Height;

            using (Mat mask = new Mat(height, width, DepthType.Cv8U, 1))
            {
                mask.SetTo(new MCvScalar(0));

                // Создаём обёртку для массива контуров
                using (VectorOfVectorOfPoint vvop = new VectorOfVectorOfPoint(polygons))
                {
                    // Заливаем многоугольники белым (255) на маске
                    CvInvoke.FillPoly(mask, vvop, new MCvScalar(255));
                }

                // Создаём фоновое изображение, полностью заполненное заданным цветом
                using (Mat background = new Mat(height, width, image.Depth, image.NumberOfChannels))
                {
                    background.SetTo(new MCvScalar(bgColor.Blue, bgColor.Green, bgColor.Red));

                    // Инвертируем маску – выделяем области СНАРУЖИ многоугольников
                    using (Mat maskInv = new Mat())
                    {
                        CvInvoke.BitwiseNot(mask, maskInv);

                        // Копируем фон в исходное изображение только там, где maskInv != 0
                        background.CopyTo(image, maskInv);
                    }
                }
            }
            return image; 
        }
        public NavigSys navigation_processing_get_scene(Point3d_GL[][] ps, ref Mat _mat_out1, ref Mat _mat_out2)
        {
            for (int i = 0;i< tools.Length; i++)
            {
                tools[i].get_frame(ps);
                tools[i] = precise_frame_comp(tools[i],ref _mat_out1,  ref _mat_out2);
            }


            return this;
        }

        public static Mat get_aruco_info(Mat image, ref System.Drawing.PointF[][] points)
        {

            // 1. Инициализация: загружаем изображение и создаём словарь маркеров

            var dictionary = new Dictionary(Dictionary.PredefinedDictionaryName.Dict4X4_50);
            var detectorParams = DetectorParameters.GetDefault();
            detectorParams.CornerRefinementMethod = DetectorParameters.RefinementMethod.Contour;
            //detectorParams.CornerRefinementWinSize = 5;
            detectorParams.CornerRefinementMaxIterations = 30;
            detectorParams.CornerRefinementMinAccuracy = 0.1;
            Mat gray = new Mat();
            CvInvoke.CvtColor(image, gray, ColorConversion.Rgb2Gray);


            //ArucoDetector detector = new ArucoDetector(dictionary, parameters);
            // Контейнеры для результатов
            var corners = new VectorOfVectorOfPointF();
            var ids = new VectorOfInt();
            var rejectedPoints = new VectorOfVectorOfPointF();

            // ArucoInvoke.RefineDetectedMarkers()
            // 2. Основной шаг: детекция маркеров

            if (gray.IsEmpty) return null;
            ArucoInvoke.DetectMarkers(gray, dictionary, corners, ids, detectorParams, rejectedPoints);



            List<System.Drawing.PointF[]> cornersList = corners.ToArrayOfArray().ToList();


            List<System.Drawing.PointF[]> cornersList_2 = new List<System.Drawing.PointF[]>();
            // 2. Настраиваем критерии для CornerSubPix
            var criteria = new MCvTermCriteria(30, 0.01); // макс. итераций, точность

            // 3. Применяем уточнение к каждому углу каждого маркера
            foreach (var markerCorners in cornersList)
            {
                VectorOfPointF vec = new VectorOfPointF(markerCorners);
                // CornerSubPix работает с массивом PointF[] или VectorOfPointF

                CvInvoke.CornerSubPix(gray, vec, new System.Drawing.Size(3, 3),
                                      new System.Drawing.Size(-1, -1), criteria);
                // Обновляем исходный массив, если нужно

                cornersList_2.Add(vec.ToArray());
            }
            corners = new VectorOfVectorOfPointF(cornersList_2.ToArray());// cornersList.ToArray();
            //Console.WriteLine("--Aruco---");
            // 3. Обработка результатов: рисуем найденные маркеры, если они есть
            if (ids.Size > 0)
            {
                // Отрисовка границ и ID маркеров на изображении
                ArucoInvoke.DrawDetectedMarkers(image, corners, ids, new Bgr(Color.Green).MCvScalar);

                /*if (ids.Size > 0)
                {
                    for (int i = 0; i < ids.Size; i++)
                    {
                        int markerId = ids[i];
                       System.Drawing.PointF[] markerCorners = corners[i].ToArray(); // Четыре угла текущего маркера
                        Console.WriteLine($"Маркер ID: {markerId}");
                        Console.WriteLine($"  Углы: ({markerCorners[0].X:F2}, {markerCorners[0].Y:F2}), " +
                                          $"({markerCorners[1].X:F2}, {markerCorners[1].Y:F2}), " +
                                          $"({markerCorners[2].X:F2}, {markerCorners[2].Y:F2}), " +
                                          $"({markerCorners[3].X:F2}, {markerCorners[3].Y:F2})");
                    }
                }*/
                // Вывод информации в консоль
                for (int i = 0; i < ids.Size; i++)
                {
                    int id = ids[i];
                    System.Drawing.PointF[] cornersArray = corners[i].ToArray();
                    if (points.Length > id)
                    {
                        points[id] = new System.Drawing.PointF[cornersArray.Length];
                        for (int j = 0; j < cornersArray.Length; j++)
                        {
                            points[id][j] = new System.Drawing.PointF(cornersArray[j].X, cornersArray[j].Y);
                            var color = new MCvScalar(128,128,128);//bgr
                            /*if(j == 0) color = new MCvScalar(0,255,0);
                            if (j == 1) color = new MCvScalar(0, 0, 0);
                            if (j == 2) color = new MCvScalar(0, 0, 255);
                            if (j == 3) color = new MCvScalar(128, 128, 128);
                            CvInvoke.DrawMarker(image, new System.Drawing.Point((int) cornersArray[j].X, (int)cornersArray[j].Y), color, MarkerTypes.Cross, 3);*/
                        }
                    }
                    //Console.WriteLine("----------");
                }


            }
            else
            {
            }


            return image;
        }

    }
}
