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



    class NavigTool
    {
        public enum ToolType { tp4_v1 };
        public Point3d_GL[] ps;
        public List<Point3d_GL> trace_tcp = new List<Point3d_GL>();
        int[] inds;
        int[] ind_aruco;
        int aruco_corner_num;
        int aruco_number = 0;
        Point3d_GL tcp;
        Point3d_GL rotate;

        public Matrix<double> matrix_frame;
        public Matrix<double> matrix_model;

        public Matrix<double> matrix_model_debug = new Matrix<double>(new double[,] {
                {1,0,0,0 },
                {0,1,0,0 },
                {0,0,1,0 },
                {0,0,0,1 }});

        public string name_3d_model = "new_tool";
        public string name_3d_model_debug = "new_debug_tool";
        public string name_3d_model_trace_tcp = "new_trace_tcp";
        public string path_3d_model = null;
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

        public NavigTool(int[] _inds, ToolType _tool_type,string _name_3d_model=null, string _path_3d_model = null, Matrix<double> _matrix_model_debug = null, string _name_3d_model_debug = null) 
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
                get_frame_tr4_v1(ps);
                return  matrix_model;
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
            matrix_frame = RobotFrame.matrix_assemble(vx, vy, vz, p1);
           
            if(tool_type == ToolType.tp4_v1)
            {
                matrix_model = matrix_frame * matrix_tcp ;
            }
            ps = new Point3d_GL[] { p0, p1, p2, p3,new Point3d_GL(matrix_model[0,3], matrix_model[1, 3], matrix_model[2, 3]) };
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

        public NavigSys navigation_processing_get_scene(Point3d_GL[][] ps)
        {
            for (int i = 0;i< tools.Length; i++)
            {
                tools[i].get_frame(ps);
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
