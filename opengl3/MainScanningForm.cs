using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Statistics.Models.Regression;
using Accord.Statistics.Models.Regression.Linear;
using Accord.Math;
using Accord.Math.Optimization.Losses;
using System.Threading;
using System.IO.Ports;
using PathPlanning;
using Accord.Statistics.Distributions.Univariate;

namespace opengl3
{
    public partial class MainScanningForm : Form
    {
        #region var
        bool scan_dist = false;
        bool scan_sync = false;
        int port_tcp = 30005;
        StringBuilder sb_enc = null;
        string video_scan_name = "1";
        string scan_i = "emp";
        string traj_i = "emp";
        TrajParams param_tr = new TrajParams();
        PatternSettings param_patt = new PatternSettings();
        FormSettings formSettings = new FormSettings();
        Polygon3d_GL[] mesh = null;
        List<Matrix<double>> rob_traj = null;
        Point3d_GL[] cont_traj = null;
        ImageBox[] imb_base = null;
        FrameType imProcType = FrameType.Test;
        LaserLine laserLine;
        DeviceMarlin linearPlatf;
        Point cam_calib_p1 = new Point(0, 0);
        Point cam_calib_p2 = new Point(0, 0);
        bool settingWindow = false;
        Mat[] patt;
        Matrix<double> persp_matr = new Matrix<double>(new double[3, 3] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } });
        TextBox[] textBoxes_Persp;
        int photo_number = 0;
        float markSize = 10f;
        Size chess_size = new Size(6, 7);
        Size chess_size_real = new Size(6, 7);
        StereoCameraCV stereocam = null;
        StereoCamera stereocam_scan = null;
        CameraCV cameraCVcommon;
        TCPclient con1;
        private const float PI = 3.14159265358979f;
        // private Size cameraSize = new Size(1280, 960);
         private Size cameraSize = new Size(1184, 656);
       // private Size cameraSize = new Size(1184, 656);
        //private Size cameraSize = new Size(1920, 1080);
         //private Size cameraSize = new Size(640, 480);
        public GraphicGL GL1 = new GraphicGL();
        private VideoCapture myCapture1 = null;
        VideoWriter writer = null;
        double fps1 = 0;

        VideoWriter[] video_writer = new VideoWriter[2];
        List<Mat>[] video_mats = new List<Mat>[2];


        private float z_mult_cam = 0.2f;
        private float[] vertex_buffer_data = { 0.0f };
        private float[] normal_buffer_data = { 0.0f };
        private float[] color_buffer_data = { 0.0f };
        volatile List<IntPtr> camera_ind = new List<IntPtr>();
        volatile List<long> camera_frame_time = new List<long>();
        List<float[]> im = new List<float[]>();
        public List<Mat> Ims;
        public List<Point> ints;
        private Point3d_GL offset_model;
        int fr_ind = 0;
        private List<Frame> frames;

        int res_min = 256 * 1;
        volatile Mat[] mat_global = new Mat[3];
        Mat matr = new Mat();
        int flag1 = 1;
        int flag2 = 1;
        int im_i = 100000;
        double red_c = 240;
        double blue_c = 250;
        double green_c = 250;
        const int scanning_len = 330;
        volatile int videoframe_count = 0;

        volatile int[] videoframe_counts = new int[5] { -1, -1,-1,-1,-1 };

        volatile int[] videoframe_counts_stop = new int[5] { 0, 0,0,0,0};

        double minArea = 1;
        double maxArea = 10;
        string name_scan = "test_1008_1";
        string openGl_folder = @"virtual_stereo/test6";
        Point3d_GL p1_scan = new Point3d_GL(548.0, -60.0, 225.0);//(655.35, -73.21, 80.40);
        Point3d_GL p2_scan = new Point3d_GL(548.0, 60.0, 225.0);
        RobotModel RobotModel_1;

        Matrix<double> cameraDistortionCoeffs = new Matrix<double>(5, 1);
        Matrix<double> cameraMatrix = new Matrix<double>(3, 3);

        Matrix<double> cameraDistortionCoeffs_dist = new Matrix<double>(5, 1);
        Matrix<double> cameraMatrix_dist = new Matrix<double>(3, 3);


        int k = 1;
        bool writ = false;
        int bin_pos = 40;

        List<Mat> cap_mats = new List<Mat>();
        Features features = new Features();
        MCvPoint3D32f[] points3D = new MCvPoint3D32f[]
            {
                new MCvPoint3D32f(0,0,0),
                new MCvPoint3D32f(60, 0, 0),
                new MCvPoint3D32f(0, 60, 0),
                new MCvPoint3D32f(60, 60, 0)
            };

        Scanner scanner;
        #endregion

        public MainScanningForm()
        {
            InitializeComponent();
            init_vars();

            //var im_las = new Image<Bgr, byte>("test_las_scan_table_model6.png");

            //CvInvoke.Imshow("im1", im_las);

            //UtilOpenCV.takeLineFromMat(im_las, 1);

            // test_basis();
            //UtilOpenCV.generateImage_chessboard_circle(10, 11, 100);
            //load_camers_v2();

            /* var path = @"D:\Project VS\scaner\opengl3\bin\x86\Debug\cam1";
             var paths = Directory.GetDirectories(path);

             var paths_sort = (from f in paths
                              orderby File.GetCreationTime(f) descending
                              select f).ToList().GetRange(0,27);


             foreach (string filename in paths_sort)
                 //File.GetCreationTime(filename);
                 Console.WriteLine(filename+" " +File.GetCreationTime(filename));*/
            // resize();
            //var pos = new PositionRob(new Point3d_GL(0.1, 0.1, 0.1), new Point3d_GL());
            //var q = RobotFrame.comp_inv_kinem_priv(pos, new int[] { -1, -1, 1 });
            //prin.t(q);

            //test_get_conts();
            //loadVideo_test_laser("test_sync_1\\v2810_2.avi");

            // frames_sync_from_file("enc_v1.txt");
        }
        static void analys_sph()
        {

            var ps1 = new PointF[]
            {
                new PointF(16,8),
                new PointF(6,34),
                new PointF(20,30),
                new PointF(56,1),
                new PointF(43,12),

                new PointF(45,17),
                new PointF(42,23),
                new PointF(30,36),
                new PointF(15,45),
                new PointF(17,51),

                new PointF(16,76),
                new PointF(28,85),
                new PointF(6,106),
                new PointF(63,37),
                new PointF(62,45),

                new PointF(84,10),
                new PointF(94,5),
                new PointF(109,9),
                new PointF(123,13),
                new PointF(116,21),

                new PointF(140,33),
                new PointF(161,24),
                new PointF(157,21),
                new PointF(187,24),
                new PointF(215,31),

                new PointF(171,59),
                new PointF(152,71),
                new PointF(95,59),
                new PointF(114,60),
                new PointF(123,74),

                new PointF(73,81),
                new PointF(87,89),
                new PointF(155,79),
                new PointF(40,112),
                new PointF(74,108),

                new PointF(158,93),
                new PointF(177,64),
                new PointF(212,53),
                new PointF(225,60),
                new PointF(221,79),

                new PointF(210,78),
                new PointF(192,85),
                new PointF(193,88),
                new PointF(212,102),
                new PointF(218,102),

                new PointF(202,115),
                new PointF(212,92)
            };

            var ps2 = new PointF[]
            {
                new PointF(69,35),
                new PointF(113,36),
                new PointF(154,34),
                new PointF(200,35),
                new PointF(244,34),

                new PointF(68,79),
                new PointF(114,79),
                new PointF(156,79),
                new PointF(202,77),
                new PointF(246,77),

                new PointF(66,124),
                new PointF(111,124),
                new PointF(157,123),
                new PointF(199,122),
                new PointF(247,120),

                new PointF(68,167),
                new PointF(114,168),
                new PointF(154,168),
                new PointF(200,166),
                new PointF(245,163),

                new PointF(70,214),
                new PointF(112,212),
                new PointF(159,210),
                new PointF(203,214),
                new PointF(246,209)
            };



            var vals1 = analyse_points_2(ps1, 3);
            var vals2 = analyse_points_2(ps2, 3);

            var max_v = Math.Max(vals1.Max(), vals2.Max());

            var gist1 = create_gist(vals1, 20, max_v);
            var gist2 = create_gist(vals2, 20, max_v);

            print_gists(gist1, gist2);
        }
        static int[] create_gist(double[] vals,int N, double max_v)//n - number of sections
        {
            var gist = new int[N+1];
            var k = max_v / N;
            Console.WriteLine("k: "+k);
            for (int i = 0; i < vals.Length; i++)
            {
                gist[(int)((vals[i]) / k)]++;
            }
            return gist;
        }

        static void print_gists(int[] gist1, int[] gist2)
        {
            var len1 = gist1.Length;
            var len2 = gist2.Length;
            if(len1!=len2)
            {
                if (len1 > len2)
                {
                    var d = len1 - len2;
                    var d_arr = new int[d];
                    var len2_l = new List<int>(len2);
                    len2_l.AddRange(d_arr);
                    gist2 = len2_l.ToArray();
                }
                else
                {
                    var d = len2 - len1;
                    var d_arr = new int[d];
                    var len1_l = new List<int>(len1);
                    len1_l.AddRange(d_arr);
                    gist1 = len1_l.ToArray();
                }
            }

            for(int i=0; i<gist1.Length;i++)
            {
                Console.WriteLine(i + " " + gist1[i] + " " + gist2[i]);
            }
            
        }


        static double[] analyse_points(PointF[] ps)//aver dist of every two points
        {

            var vals = new List<double>();
            for(int i=0; i<ps.Length;i++)
            {
                for (int j = i; j < ps.Length; j++)
                {
                    if(i!=j)
                    {
                        var dist = (ps[i] - ps[j]).norm;
                        vals.Add((double)dist);
                    }
                }
            }
            return vals.ToArray();
        }

        static double[] analyse_points_2(PointF[] ps,int n)//aver dist for n nearest points
        {
            var vals = new List<double>();
            
            for (int i = 0; i < ps.Length; i++)
            {
                var vals_1 = new List<double>();
                for (int j = 0; j < ps.Length; j++)
                {
                    if (i != j)
                    {
                        var dist = (ps[i] - ps[j]).norm;
                        vals_1.Add((double)dist);
                    }
                }
                vals_1.Sort();
                vals.AddRange(vals_1.GetRange(0, n));
            }
            return vals.ToArray();
        }

        static int[] frames_max(int[,] data)
        {
            int analyse_len = 10;
            var end_data = new List<int[]>();
            for(int i= data.GetLength(0)-analyse_len;i < data.GetLength(0) - 1;i++)
            {
                end_data.Add(new int[] { data[i, 1], data[i, 2] });
            }
            var ed_s = (from d in end_data
                        orderby d[0] descending
                        select d).ToArray();
            var i_min = 0;
            for(int i=1; i_min==0 && i<analyse_len-2;i++)
            {
                if (ed_s[i][1] != ed_s[0][1]) i_min = i;
            }
            var ed_l = ed_s[0].ToList();
            ed_l.AddRange(ed_s[i_min]);
            return  ed_l.ToArray();
        }
        static public double[][] frames_sync_from_file(string enc_path,Label label=null)
        {
            var data = analys_sync(enc_path);
            var frms_max = frames_max(data);

            var fr_max = frms_max[0];
            var cam_max = frms_max[1];
            var fr_min = frms_max[2];
            var cam_min = frms_max[3];


            Console.WriteLine("fr_cnt = "+fr_max);
            Console.WriteLine("fr_cnt_m = " + fr_min);
            var data_s = new int[3, fr_max + 1][];
            for(int i = 0; i < data.GetLength(0); i++)
            {
                var fr_n = data[i,1];
                var cam_n = data[i, 2];
                var time = data[i, 3];
                if( data_s[cam_n,fr_n]==null)
                {
                    data_s[cam_n, fr_n] = new int[0];
                }
                var l = data_s[cam_n, fr_n].ToList();
                l.Add(time);
                data_s[cam_n, fr_n] = l.ToArray();
            }

            var find_prec1 = new List<double>();
            var find_prec2 = new List<double>();

            for (int i = 1; i < data_s.GetLength(1); i++)
            {
                if (data_s[cam_max, i] != null && data_s[cam_min, i] != null && data_s[cam_max, i-1] != null && data_s[cam_min, i-1] != null)
                    if (data_s[cam_max, i].Length > 1 && data_s[cam_min, i].Length > 1 && data_s[cam_max, i-1].Length > 1 && data_s[cam_min, i-1].Length > 1)
                    { 
                       /* Console.WriteLine(i+" "+data_s[cam_max, i][0] + "  " + data_s[cam_min, i][0]+" "+ 
                            (data_s[cam_max, i][1]-data_s[cam_max, i][0]) + "  " + (data_s[cam_min, i][1]- data_s[cam_min, i][0]) + " "+
                            (data_s[cam_max, i][0] - data_s[cam_max, i-1][0]) + " "+ (data_s[cam_min, i][0] - data_s[cam_min, i - 1][0])+" "+
                            (data_s[cam_max, i][1] - data_s[cam_max, i - 1][1]) + " " + (data_s[cam_min, i][1] - data_s[cam_min, i - 1][1]) + " " );*/
                        find_prec1.Add(data_s[cam_max, i][0] - data_s[cam_max, i - 1][0]);
                        find_prec2.Add(data_s[cam_min, i][0] - data_s[cam_min, i - 1][0]);
                    }
            }
            var prec1 = find_aver_dev( find_prec1.ToArray());
            var prec2 = find_aver_dev(find_prec2.ToArray());
            if(label!=null) label.Text = Math.Round(prec1).ToString() + " " + Math.Round(prec2).ToString() + " PREC"; 

            var prs = compare_frames(data_s, fr_min, fr_max, cam_min, cam_max);
            return prs;
        }

        static double find_aver_dev(double[] vals)
        {
            double len = vals.Length;
            var aver = vals.Sum() / len;
            var sq_arr = new double[vals.Length];
            for (int i = 0; i < sq_arr.Length; i++)
            {
                sq_arr[i] = (vals[i] - aver) * (vals[i] - aver);
            }
            var sq_aver = sq_arr.Sum() / len;
            return Math.Sqrt(sq_aver);
        }

        static  double[][] compare_frames(int[,][] data, int frame_min, int frame_max, int cam_min, int cam_max)
        {
            int j = 1;
            var pairs = new double[frame_min][];
            for(int i=1; i<frame_min;i++)
            {
                if(data[cam_min, i]!=null && data[cam_max, j]!=null)
                {
                    while (data[cam_min, i][0] > data[cam_max, j][0] && j < frame_max)
                    {
                        j++;
                    }
                    double df = 0;//на какую часть нужно сместиться относительно j-1 кадра чтобы совпасть по времени
                    if(j>1)
                    {
                        df = (double)(data[cam_max, i][0] - data[cam_max, j - 1][0]) /(data[cam_max, j][0] - data[cam_max, j - 1][0]) ;

                        var d1 = data[cam_max, i][0];
                        var d2 = data[cam_max, j][0] - (data[cam_max, j][0] - data[cam_max, j - 1][0]) * (1 - df);
                        //Console.WriteLine(d1 + " " + d2);
                    }
                    pairs[i] = new double[] { j, df };
                   
                }
                //Console.WriteLine(i + " " + j);
            }
            pairs[0] = new double[] { cam_min, cam_max ,frame_min, frame_max};
            return pairs;
        }
        static int[,] analys_sync(string enc_path)
        {
            string enc;
            using (StreamReader sr = new StreamReader(enc_path))
            {
                enc = sr.ReadToEnd();
            }

            enc = enc.Replace("\r", "");
            var lines = enc.Split('\n'); 
            var enc_pos = new int[lines.Length, 8];
            int ind = 0;
            int st_time = 0;
            foreach (var line in lines)
            //for (int l = lines.Length-1; l>=0; l--)
            {
                //var line = lines[l];
                if (line.Length > 0)
                {
                    var vals = line.Trim().Split(' ');

                    if (vals.Length == 6)
                    {
                        for (int i = 0; i < vals.Length; i++)
                        //for (int i = vals.Length - 1; i >= 0; i--)
                        {
                            if (i == 3)
                            {
                                string time = "";
                                for (int j = vals[i].Length / 2; j < vals[i].Length; j++)
                                {
                                    time += vals[i][j];
                                }
                                vals[i] = time;
                            }
                            enc_pos[ind, i] = Convert.ToInt32(vals[i]);
                            if (i == 3)
                            {
                                enc_pos[ind, i] -= st_time;
                                if (ind == 0)
                                {
                                    st_time = enc_pos[ind, i];
                                    enc_pos[ind, i] = 0;
                                }

                            }
                        }
                       /*Console.Write(enc_pos[ind, 3] + ";");
                        if (enc_pos[ind, 5] == 1) Console.Write(enc_pos[ind, 4] + ";" + ";");
                        if (enc_pos[ind, 5] == 2) Console.Write(";" + enc_pos[ind, 4] + ";");


                        Console.WriteLine(' ');*/
                    }
                }
                ind++;
            }
            return enc_pos;
        }

        void python_c_sh()
        {
            var eng = IronPython.Hosting.Python.CreateEngine();
            var scope = eng.CreateScope();
            eng.Execute(@"def greetings(name): return 'Hello ' + name.title() + '!'", scope);
            dynamic greetings = scope.GetVariable("greetings");
            System.Console.WriteLine(greetings("world"));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            label_timer.Text = DateTime.Now.Second + " : " + DateTime.Now.Millisecond;// +" "+ DateTime.Now.Ticks/ TimeSpan.TicksPerMillisecond;
            lab_fps_cam1.Text = fps1.ToString();
        }
        void init_vars()
        {
            #region important
            combo_improc.Items.AddRange(new string[] { "Распознать шахматный паттерн", "Стерео Исп", "Паттерн круги", "Датчик расст", "Ничего" });

            cameraDistortionCoeffs_dist[0, 0] = -0.3;

            mat_global[0] = new Mat();
            mat_global[1] = new Mat();
            mat_global[2] = new Mat();

            if (comboImages.Items.Count > 0)
            {
                comboImages.SelectedIndex = 0;
            }

            textBoxes_Persp = new TextBox[]
            {
                textBoxK_0,textBoxK_1,textBoxK_2,
                textBoxK_3,textBoxK_4,textBoxK_5,
                textBoxK_6,textBoxK_7,textBoxK_8,
            };

           // patt = UtilOpenCV.generateImage_chessboard(chess_size.Width, chess_size.Height, 200);
            patt = UtilOpenCV.generateImage_chessboard(10, 11, 200);
            #endregion
            imb_base = new ImageBox[] { imBox_base_1, imBox_base_2 };
            minArea = 1.0 * k * k * 15;
            maxArea = 15 * k * k * 250;
            red_c = 252;
            //var model_mesh = STLmodel.parsingStl_GL4(@"curve_test_asc.STL");
            //GL1.addMesh(model_mesh, PrimitiveType.Triangles);

            param_tr = new TrajParams
            {
                dz = 0.4,
                div_step = 1.3,
                h_transf = 1,
                h_transf_out = 10,
                layers = 2,
                layers_angle = Math.PI / 2,// Math.PI / 4,
                step = 2.6,
                vel = 20,
                line_width = 0.4,
                k_decr_ang = 0.5,
                w_smooth_ang = 15               
            };
            propGrid_traj.SelectedObject = param_tr;

            param_patt = new PatternSettings
            {
                step = 2,
                angle = 0,
                angle_layers = PI/2,
                min_dist = 0.1,
                arc_dist = 2,
                r = 2,
                start_dir_r = true,
                patternType = PathPlanner.PatternType.Lines,
                dim_x = 10,
                dim_y = 10,
                filling = 0.7
            };
            propGrid_pattern.SelectedObject = param_patt;
            prop_gr_light.SelectedObject = GL1.lightSources_obj;
            debugBox.Text = "0.3 0.3 1";

            scan_dist = ch_b_dist.Checked;
            scan_sync = ch_b_sync.Checked;

            tree_models.CheckBoxes = true;
            //load_camers_v2();

            /*var m_test = new Mat("test_ph.jpg");
            var fr = new Frame(m_test, "sdf", FrameType.MarkBoard);
            CameraCV.findPoints(fr, new Size(9, 10));*/

        }

        void test_smooth()
        {
            var p1 = new Point3d_GL(0,0,0);
            var p2 = new Point3d_GL(10, 0, 0);
            var p3 = new Point3d_GL(10, 10, 10);
            var traj = new List<Point3d_GL>();
            traj.Add(p1);
            traj.Add(p2);
            traj.Add(p3);
            var div = PathPlanner.divide_traj(traj, 0.05);
            var line_s1 = Point3d_GL.line_aver(div.ToArray(), 20);
            var line_s2 = Point3d_GL.line_laplace(div.ToArray(), 200);
            GL1.addLineMeshTraj(line_s1,Color3d_GL.red(),"aver");
            GL1.addLineMeshTraj(line_s2, Color3d_GL.blue(), "lapl");

        }

        void test_remesh()
        {
           /* var p1 = new Point3d_GL(1, 1, 1);
            var p2 = new Point3d_GL(2, 2, 2);
            var p3 = new Point3d_GL(3, 3, 3);
            var p4 = new Point3d_GL(4, 4, 4);*/

            var p1 = new Point3d_GL(0, 0, 0);
            var p2 = new Point3d_GL(10, 0, 0);
            var p3 = new Point3d_GL(10, 10, 0);
            var p4 = new Point3d_GL(0, 10, 0);
            var polygs = new Polygon3d_GL[] { new Polygon3d_GL(p1, p2, p3), new Polygon3d_GL(p1, p4, p3) };
            var inds_m = new IndexedMesh(polygs);
            var polyg_re = inds_m.get_polygs();

            var mesh = Polygon3d_GL.toMesh(polygs);           
            GL1.add_buff_gl(mesh[0], mesh[1], mesh[2], PrimitiveType.Triangles, "re1");

            mesh = Polygon3d_GL.toMesh(polyg_re);
            GL1.add_buff_gl(mesh[0], mesh[1], mesh[2], PrimitiveType.Triangles, "re2");
        }
        void test_cross_line_triang()
        {
            /* var p1 = new Point3d_GL(1, 1, 1);
             var p2 = new Point3d_GL(2, 2, 2);
             var p3 = new Point3d_GL(3, 3, 3);
             var p4 = new Point3d_GL(4, 4, 4);*/

            var p1 = new Point3d_GL(0, 0, 0);
            var p2 = new Point3d_GL(10, 0, 0);
            var p3 = new Point3d_GL(10, 10, 0);
            var p4 = new Point3d_GL(0, 10, 0);
            var polygs = new Polygon3d_GL[] { new Polygon3d_GL(p1, p2, p3), new Polygon3d_GL(p1, p4, p3) };
            var inds_m = new IndexedMesh(polygs);
            var line = new Line3d_GL(new Point3d_GL(4, -0.5, 0), new Point3d_GL(4, -0.5, 10));
            var p_cross = Polygon3d_GL.cross_line_triang(polygs[0], line);
            Console.WriteLine(p_cross+" "+p_cross.exist);
        }
        void add_points_cal()
        {
            var ps = new Point3d_GL[] { 
                new Point3d_GL(-0.3801956,0.22838, 0.03581),
                new Point3d_GL(-0.380255,  0.278841, 0.0354566),
                new Point3d_GL(-0.4398407, 0.278541,0.0353042),

            };
            ps = Point3d_GL.mult(ps, 1000);
           // GL1.addPointMesh(ps,Color3d_GL.red());
        }

        void loadStereo()
        {
            var cam_cal_1 = new string[] { @"cam1\camera_cal_1006_1" };
            var cam_cal_2 = new string[] { @"cam2\camera_cal_1006_1" };

            var frms1 = FrameLoader.loadPathsDiff(cam_cal_1, FrameType.MarkBoard, PatternType.Chess);
            var frms2 = FrameLoader.loadPathsDiff(cam_cal_2, FrameType.MarkBoard, PatternType.Chess);
            var cam1 = new CameraCV(frms1, chess_size, markSize, null);
            var cam2 = new CameraCV(frms2, chess_size, markSize, null);

            var cam_cal_stereo = new string[] { @"camera_cal_1006_1" };
            var frms = FrameLoader.loadPathsDiffDouble(cam_cal_stereo, FrameType.MarkBoard);

            stereocam = new StereoCameraCV(new CameraCV[] { cam1, cam2 }, chess_size_real, markSize, frms);

            var frms3 = FrameLoader.loadImages_stereoCV(@"cam1\" + cam_cal_stereo[0], @"cam2\" + cam_cal_stereo[0], FrameType.MarkBoard);
            comboImages.Items.AddRange(frms3);

        }

        void oneCam(string[] cam_cal_paths, float mark_size)
        {
            var frms = FrameLoader.loadPathsDiff(cam_cal_paths, FrameType.Pattern, PatternType.Mesh);
            var cam1 = new CameraCV(frms, chess_size_real, mark_size, null);
            cameraCVcommon = cam1;
            comboImages.Items.AddRange(frms);

        }

        void test_cross_triag()
        {
            double l = 10;
            var p1 = new Point3d_GL(l, 0, 0);
            var p2 = new Point3d_GL(0, l/2, 0);
            var p3 = new Point3d_GL(0, 0, l/2);

            var p4 = new Point3d_GL(0, 0, 0);
            var p5 = new Point3d_GL(l/3, l, l);
            var p6 = new Point3d_GL(l, 0, l);

            var t1 = new Polygon3d_GL(p1, p2, p3);
            var t2 = new Polygon3d_GL(p4, p5, p6);

            var ps = Polygon3d_GL.cross_triang(t1, t2);

            GL1.addPointMesh(ps,new Color3d_GL(1));
            var triag = GL1.addMesh(Polygon3d_GL.toMesh(new Polygon3d_GL[] {t1,t2})[0],PrimitiveType.Triangles);
            GL1.buffersGl.setTranspobj(triag, 0.3f);
        }



        #region laserScanner
        void loadScanner()
        {
            var cam_cal_paths = new string[] { @"cam1\photo_9", @"cam1\photo_11", @"cam1\photo_13" };//, @"cam1\photo_10"
            var scan_path = @"cam1\scan_2002_1";
            //var scan_path = @"cam1\las_cal_2002_1\test";
            var las_cal_path = @"cam1\las_cal_2002_1\test";



            var frms_las_cal = FrameLoader.loadImages_diff(las_cal_path, FrameType.LasHand, PatternType.Chess);
            var frms_scan = FrameLoader.loadImages_diff(scan_path, FrameType.LasHand, PatternType.Chess);

            var frms = FrameLoader.loadPathsDiff(cam_cal_paths, FrameType.Pattern, PatternType.Chess);
            var cam1 = new CameraCV(frms, chess_size_real, markSize, null);
            cameraCVcommon = cam1;
            //comboImages.Items.AddRange(frms_las_cal);
            comboImages.Items.AddRange(frms_scan);
            //comboImages.Items.AddRange(frms);

            var scanner1 = new Scanner(cam1);
            if (scanner1.calibrateLaser(Frame.getMats(frms_las_cal), PatternType.Chess, GL1))
            {
                scanner1.addPoints(Frame.getMats(frms_scan));
                var p3d_scan_sc = scanner1.getPointsScene();
                var mesh_scan_sc = Point3d_GL.toMesh(p3d_scan_sc);
                GL1.addMeshWithoutNorm(mesh_scan_sc, PrimitiveType.Points, new Color3d_GL(0.9f));
            }
            else
            {
                Console.WriteLine("CalibLas FALSE");
            }
        }

        void loadScannerLin(string[] cam_cal_paths, string las_cal_path, string lin_cal_path, string scand_path, float[] normrgb)
        {
            Console.WriteLine("load frms lin cal");
            var frms_lin_cal = FrameLoader.loadImages_diff(lin_cal_path, FrameType.LasLin, PatternType.Chess);
            /*Console.WriteLine("load frms las cal");
            var frms_las_cal = FrameLoader.loadImages_diff(las_cal_path, FrameType.LasLin, PatternType.Chess);
            //var frms_scan = FrameLoader.loadImages_diff(scan_path, FrameType.LasLin, PatternType.Chess);
            Console.WriteLine("load frms scan");
            var frms_scan_diff = FrameLoader.loadImages_diff(scand_path, FrameType.LasDif, PatternType.Chess);
            Console.WriteLine("load frms cam cal");*/
            var frms = FrameLoader.loadPathsDiff(cam_cal_paths, FrameType.MarkBoard, PatternType.Chess);
            var cam1 = new CameraCV(frms, chess_size, markSize, null);

            cameraCVcommon = cam1;
            var scanner1 = new Scanner(cam1);
            scanner1.calibrateLinear(Frame.getMats(frms_lin_cal), Frame.getLinPos(frms_lin_cal), PatternType.Chess, GL1);

            /*comboImages.Items.AddRange(frms_lin_cal);
            comboImages.Items.AddRange(frms_scan_diff);
            comboImages.Items.AddRange(frms_las_cal);
            //comboImages.Items.AddRange(frms_scan);
            //comboImages.Items.AddRange(frms);

            var scanner1 = new Scanner(cam1);
            if (scanner1.calibrateLaser(Frame.getMats(frms_las_cal), PatternType.Chess, GL1))
            {
                Console.WriteLine("CalibLas Done________________");
                if (scanner1.calibrateLinear(Frame.getMats(frms_lin_cal), Frame.getLinPos(frms_lin_cal), PatternType.Chess, GL1))
                {
                    Console.WriteLine("CalibLin Done________________________");
                    var lins = scanner1.addPointsLin(Frame.getMats(frms_scan_diff), Frame.getLinPos(frms_scan_diff));
                    if (lins > 0)
                    {
                        Console.WriteLine("Load Points Done__" + lins+"_lins__________________");
                        var p3d_scan_sc = scanner1.getPointsScene();
                        var mesh_scan_sc = Point3d_GL.toMesh(p3d_scan_sc);
                        var sph_points_xy = new Point3d_GL[]
                        {
                            new Point3d_GL(-25,-38,0),
                            new Point3d_GL(-3,-81,0),
                            new Point3d_GL(-49,-81,0),
                            new Point3d_GL(-32,-58,0)
                        };
                        var sph_points = STLmodel.nearestPointsXY(p3d_scan_sc, sph_points_xy);
                        var sphere = UtilMatr.findSphereFrom4Points(sph_points);
                        prin.t("sph_points_xy");
                        prin.t(sph_points_xy);
                        prin.t("sph_points");
                        prin.t(sph_points);
                        Console.WriteLine(sphere[0] + "\n" + sphere[1]);
                        //GL1.addMeshWithoutNorm(mesh_scan_sc, PrimitiveType.Points, normrgb[0], normrgb[1], normrgb[2]);

                        var mesh_scan_stl = meshFromPoints(scanner1.getPointsLinesScene());
                        //STLmodel.saveMesh(mesh_scan_stl, "test_" + normrgb[0] + "_" + normrgb[1] + "_" + normrgb[2]);
                        GL1.addMesh(mesh_scan_stl, PrimitiveType.Triangles, normrgb[0], normrgb[1], normrgb[2]);
                    }
                    else
                    {
                        Console.WriteLine("Load Points FALSE________________");
                    }
                }
                else
                {
                    Console.WriteLine("CalibLin FALSE________________");
                }
            }
            else
            {
                Console.WriteLine("CalibLas FALSE________________");
            }*/
        }
        //_________________________________________________________
        void loadScannerLinLas(
            string[] cam_cal_paths,
            string[] las_cal_path,
            string[] las_cal_orig_path,
            string scand_path, string scand_orig_path,
            float[] normrgb, bool undist)
        {

            var frms = FrameLoader.loadPathsDiff(cam_cal_paths, FrameType.Pattern, PatternType.Mesh);
            comboImages.Items.AddRange(frms);
            
            var cam1 = new CameraCV(frms, chess_size, markSize, null);
            cameraCVcommon = cam1;

            var frms_scan_diff = FrameLoader.loadImages_diff(scand_path, FrameType.LasDif, PatternType.Chess, scand_orig_path, cam1, undist);
            comboImages.Items.AddRange(frms_scan_diff);

            var frms_las_cal_0 = FrameLoader.loadImages_diff(las_cal_path[0], FrameType.LasLin, PatternType.Chess, las_cal_orig_path[0], cam1, undist);
            //var frms_las_cal_1 = FrameLoader.loadImages_diff(las_cal_path[1], FrameType.LasLin, PatternType.Chess, las_cal_orig_path[1], cam1, undist);
            comboImages.Items.AddRange(frms_las_cal_0);

            
            var orig = FrameLoader.loadPathsDiff(las_cal_orig_path, FrameType.MarkBoard, PatternType.Chess, cam1, undist);
            var orig_scan = FrameLoader.loadPathsDiff(new string[] { scand_orig_path }, FrameType.MarkBoard, PatternType.Chess, cam1, undist);
            // GL1.addFlat3d_XY_zero(0);
            //GL1.addFlat3d_XY_zero(4);
            var frms_las_cal_scan = FrameLoader.loadPathsDiff(new string[] { las_cal_orig_path[0] }, FrameType.MarkBoard, PatternType.Chess, cam1, undist);
            //comboImages.Items.AddRange(frms_las_cal_0);
            var scanner1 = new Scanner(cam1);
            scanner1.linearAxis.GraphicGL = GL1;

            //if (scanner1.calibrateLinearLas(new Mat[][] { Frame.getMats(frms_las_cal_0), Frame.getMats(frms_las_cal_1) }, Frame.getMats(orig), Frame.getLinPos(frms_las_cal_0), PatternType.Chess, GL1))
            if (scanner1.calibrateLinearLas(
                new Mat[][] { Frame.getMats(frms_las_cal_0) },
                Frame.getMats(orig),
                Frame.getLinPos(frms_las_cal_0),
                PatternType.Chess, GL1))
            {
                //frms_las_cal_1 = null;
                Console.WriteLine("CalibLin Done________________________");
                var lins = scanner1.addPointsLinLas(Frame.getMats(frms_scan_diff), Frame.getLinPos(frms_scan_diff), Frame.getMats(orig_scan)[0], PatternType.Chess);
                /* var lins = scanner1.addPointsLinLas(
                     Frame.getMats(frms_las_cal_0),
                     Frame.getLinPos(frms_las_cal_0),
                     FrameLoader.loadImages_diff(las_cal_orig_path[0],FrameType.MarkBoard)[0].im,
                     PatternType.Chess);*/
                frms_las_cal_0 = null;
                GC.Collect();
                //var lins = 0;
                if (lins > 0)
                {
                    //frms_scan_diff = null;
                    Console.WriteLine("Load Points Done__" + lins + "_lins__________________");
                    var mesh_scan_stl = meshFromPoints(scanner1.getPointsLinesCam());
                    GL1.addMesh(mesh_scan_stl, PrimitiveType.Triangles, new Color3d_GL( normrgb[0], normrgb[1], normrgb[2]));
                    mesh_scan_stl = null;
                    scanner1 = null;

                    GC.Collect();
                }
                else
                {
                    Console.WriteLine("Load Points FALSE________________");
                }
            }
            else
            {
                Console.WriteLine("CalibLin FALSE________________");
            }
            GC.Collect();
        }

        void load_camers_v2()
        {
            markSize = 10f;//6.2273f
            chess_size = new Size(6, 7);//new Size(10, 11);
            var frms_1 = FrameLoader.loadImages_diff(@"cam1\virt_cam_cal_250823_3", FrameType.Pattern, PatternType.Mesh);
             var cam1 = new CameraCV(frms_1, chess_size, markSize, null);       
            cam1.save_camera("virt_cam_conf_cal_250823_3.txt");            
            comboImages.Items.AddRange(frms_1);
            cameraCVcommon = cam1;
           /* var frms_2 = FrameLoader.loadImages_diff(@"cam2\cam2_cal_130523_2", FrameType.Pattern, PatternType.Mesh);
            var cam2 = new CameraCV(frms_2, chess_size, markSize, null);
            cam2.save_camera("cam2_conf_130523_2.txt");
            comboImages.Items.AddRange(frms_2);*/
        }
        Scanner loadScanner_v2(string  conf1, string conf2, string stereo_cal, string bfs_file = null)
        {
            var cam1 = CameraCV.load_camera(conf1);
            var cam2 = CameraCV.load_camera(conf2);
            Scanner scanner;
            if(bfs_file==null)
            {
                scanner = new Scanner(new CameraCV[] { cam1, cam2 });
            }
            else
            {

                var stereo_cam = new StereoCamera(new CameraCV[] { cam1, cam2 },bfs_file);
                scanner = new Scanner(stereo_cam);
                stereocam_scan = stereo_cam;
            }
            chess_size = new Size(6, 7);
            var marksize = 10f;
            var stereo_cal_1 = stereo_cal.Split('\\').Reverse().ToArray()[0];
            var frms_stereo = FrameLoader.loadImages_stereoCV(@"cam1\" + stereo_cal_1, @"cam2\" + stereo_cal_1, FrameType.Pattern, true);
            scanner.initStereo(new Mat[] { frms_stereo[0].im, frms_stereo[0].im_sec }, PatternType.Mesh,chess_size,marksize);

            //comboImages.Items.AddRange(frms_stereo);

            return scanner;
        }
        void load_scan_v2(Scanner scanner,string scan_path, int strip = 1, double smooth = 0.8)
        {



            var scan_path_1 = scan_path.Split('\\').Reverse().ToArray()[0];
            //
            if(ch_b_sync.Checked)
                scanner = loadVideo_stereo(scan_path_1, scanner, strip);
            else
                scanner = loadVideo_stereo_not_sync(scan_path_1, scanner, strip);

            var mesh = Polygon3d_GL.triangulate_lines_xy(scanner.getPointsLinesScene(), smooth);

            //mesh = GL1.addNormals(mesh, 1);
            this.scanner = scanner;

            var scan_stl = Polygon3d_GL.toMesh(mesh);
            if(scan_stl != null) scan_i = GL1.add_buff_gl(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Triangles, scan_path_1);
           // if (scan_stl != null) scan_i = GL1.add_buff_gl_dyn(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Points);
            
            Console.WriteLine("Loading end.");
        }
        //----------------------------------------------------------------------------------------------------------
        Scanner loadScanner_sing(string conf1, string laser_line = null)
        {
            var cam1 = CameraCV.load_camera(conf1);
            LinearAxis line = null;
            if(laser_line != null)  line = LinearAxis.load(laser_line);
            var scanner = new Scanner( cam1,line);
            this.scanner = scanner;
            return scanner;
        }
        void load_scan_sing(Scanner scanner,string scan_path, int strip = 1, double smooth = 0.8)
        {
            var scan_path_1 = scan_path.Split('\\').Reverse().ToArray()[0];
            scanner = loadVideo_sing_cam(scan_path_1, scanner,strip);
            var ps = scanner.getPointsLinesScene();
            var mesh = Polygon3d_GL.triangulate_lines_xy(ps, smooth);
            var scan_stl = Polygon3d_GL.toMesh(mesh);
            scan_i = GL1.add_buff_gl(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Triangles,"scan_sing");
            

        }

        void load_calib_sing(Scanner scanner, string scan_path, int strip = 1, double smooth = 0.8)
        {

            var scan_path_1 = scan_path.Split('\\').Reverse().ToArray()[0];
            scanner = loadVideo_sing_cam(scan_path_1, scanner, strip, true);
            scanner.linearAxis.save("linear.txt");

        }

        //-------------------------------------------------------------------------------------------
        void loadScannerStereoLas(
            string[] cam_cal_path,
            string stereo_cal_path,
            string scand_path,
            float[] normrgb, bool undist,int strip)
        {
            #region calibrate
            /*var frms_1 = FrameLoader.loadImages_diff(@"cam1\" + cam_cal_path, FrameType.MarkBoard, PatternType.Chess);
            var cam1 = new CameraCV(frms_1, new Size(6, 7), markSize, null);
            var frms_2 = FrameLoader.loadImages_diff(@"cam2\" + cam_cal_path, FrameType.MarkBoard, PatternType.Chess);
            var cam2 = new CameraCV(frms_2, new Size(6, 7), markSize, null);*/

            var frms_1 = FrameLoader.loadImages_diff(@"cam1\" + cam_cal_path[0], FrameType.Pattern, PatternType.Mesh);
           // var cam1 = new CameraCV(frms_1, new Size(6, 7), markSize, null);
            var frms_2 = FrameLoader.loadImages_diff(@"cam2\" + cam_cal_path[1], FrameType.Pattern, PatternType.Mesh);
            //var cam2 = new CameraCV(frms_2, new Size(6, 7), markSize, null);
            var size = frms_1[0].size;

            var cam1 = new CameraCV(new Matrix<double>(new double[,]
            {
                { 1265.9413589664, 0, 522.784557645 },
                { 0, 1243.0250932118, 306.9685870958 },
                { 0, 0 ,1 } }), new Matrix<double>(new double[,]
            {{ -0.109168496, -0.4567725913, -0.0013216245, 0.0031954698, 1.5318467875  }}), size);



            var cam2 = new CameraCV(new Matrix<double>(new double[,]
            {
                { 1263.8882646257, 0, 592.877084689   },
                { 0, 1240.0422808291, 328.1683916855  },
                { 0, 0 ,1 } }), new Matrix<double>(new double[,]
            {{ -0.1176432326, -0.6806742511, -0.0011129391, 0.0003074201, 2.4070663543  }}), size);

            var frms_stereo = FrameLoader.loadImages_stereoCV(@"cam1\" + stereo_cal_path, @"cam2\" + stereo_cal_path, FrameType.Pattern, true);

            cam1.save_camera("cam1_conf_0408.txt");
            cam2.save_camera("cam2_conf_0408.txt");

            comboImages.Items.AddRange(frms_1);
           // comboImages.Items.AddRange(frms_2);
            comboImages.Items.AddRange(frms_stereo);
            cameraCVcommon = cam1;
            #endregion

        
            scanner = new Scanner(new CameraCV[] { cam1, cam2 });

            scanner.initStereo(new Mat[] { frms_stereo[0].im, frms_stereo[0].im_sec }, PatternType.Mesh,chess_size,10f);

            loadVideo_stereo(scand_path, scanner, strip);
            var mesh = Polygon3d_GL.triangulate_lines_xy(scanner.getPointsLinesScene());
            var scan_stl = Polygon3d_GL.toMesh(mesh);
             GL1.add_buff_gl(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Triangles);

           // GL1.addPointMesh(scanner.getPointsScene());
            //GL1.buffersGl.sortObj();
            //STLmodel.saveMesh(scan_stl[0], scand_path);
        }

        public float[] extract_delt(float[] mesh)
        {
            for(int i=0; i<mesh.Length; i+=3)
            {
                mesh[i + 2] =3* comp_delt(mesh[i + 2]);
            }
            return mesh;
        }

        static float comp_delt(float val)
        {
            return val - ((float)Math.Round(val / 10))*10;
        }

        string scan_fold_name = "test";
        string scan_fold_path = @"C:\";
        public void startScanLaser(int typeScan)//0 - defolt, 1 - dif,2 - marl,3 - stereo, 4 - sing
        {
            try
            {
                scan_fold_name = box_scanFolder.Text;
                box_scanFolder.Text += "_" + DateTime.Now.Month.ToString() + "_"
                + DateTime.Now.Day.ToString() + "_" +
                DateTime.Now.Hour.ToString() + "_"
                + DateTime.Now.Minute.ToString() + "_"
                + DateTime.Now.Second.ToString();
                
                Thread robot_thread = new Thread(scan_resLaser);
                robot_thread.Start(typeScan);
            }
            catch
            {
            }
        }

        private void scan_resLaser(object obj)
        {
            Thread.Sleep(100);
            int typescan = (int)obj;
            int counts = Convert.ToInt32(boxN.Text);
            
            string folder_scan = box_scanFolder.Text;
            var p1_cur_scan = robFrameFromTextBox(nameX, nameY, nameZ, nameA, nameB, nameC);
            var p2_cur_scan = robFrameFromTextBox(nameX2, nameY2, nameZ2, nameA, nameB, nameC);
            var fps = Convert.ToInt32(tB_fps_scan.Text);
            float x = (float)p1_cur_scan.x;
            
            var delx = (float)(p2_cur_scan.x - p1_cur_scan.x) / (float)counts;
            if (laserLine == null)
            {
                initLaserFast();
                Thread.Sleep(200);
            }
            laserLine?.laserOff();
            Thread.Sleep(300);
            var dir_scan =Path.Combine( Directory.GetCurrentDirectory(), "cam1\\" + folder_scan);
            scan_fold_path = dir_scan;
            //Console.WriteLine(dir_scan);
            makePhotoLaser(
                    new float[] { x },
                    new string[] { "cam1\\" + folder_scan+ "\\orig", "cam2\\" + folder_scan + "\\orig" },
                    new ImageBox[] { imageBox1, imageBox2 }
                    );
            Thread.Sleep(200);

            

            if (typescan == 3)
            {
                var t_video = (double)counts / fps;
                var v_laser = (p2_cur_scan.x - p1_cur_scan.x) / t_video;
                laserLine?.laserOn();
                Thread.Sleep(100);
                laserLine?.setShvpVel(200);
                Thread.Sleep(200);

                laserLine?.setShvpPos((int)p1_cur_scan.x);
                Thread.Sleep(2000);
                startWrite(1, counts);
                startWrite(2, counts);
                Console.WriteLine(v_laser + " v_las");
                laserLine?.setShvpVel(v_laser);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                sb_enc = new StringBuilder();

            }

            if (typescan == 4)
            {
                

                var t_video = (double)counts / fps;
                var v_laser = (p2_cur_scan.x - p1_cur_scan.x) / t_video;
                laserLine?.laserOn();
                Thread.Sleep(200);

                
                startWrite(1, counts);
                sb_enc = new StringBuilder();
                laserLine?.setShvpVel(v_laser);
                laserLine?.setShvpPos((int)p2_cur_scan.x);

            }

            for (int i = 0; i < counts; i++)
            {

                if (typescan==0)
                {
                    if (laserLine == null)
                    {
                        initLaserFast();
                        Thread.Sleep(200);
                    }
                    laserLine.laserOn();
                    makePhotoLaser(
                    new float[] { x },
                    new string[] { "cam1\\" + folder_scan , "cam2\\" + folder_scan },
                    new ImageBox[] { imageBox1 ,imageBox2 }
                    );
                }
                else if(typescan == 1)
                {
                    makeDoublePhotoLaser(
                    new float[] { x },
                    new string[] { "cam1\\" + folder_scan, "cam2\\" + folder_scan },
                    new ImageBox[] { imageBox1, imageBox2 }
                    );
                }
                else if(typescan == 2)
                {
                    if (laserLine == null)
                    {
                        initLaserFast();
                        Thread.Sleep(200);
                    }
                    laserLine.laserOn();
                    makePhotoMarlin(
                    new float[] { x },
                    new string[] { "cam1\\" + folder_scan, "cam2\\" + folder_scan },
                    new ImageBox[] { imageBox1, imageBox2 }
                    );
                }               
                x += delx;
            }
            
        }



        void initLaserFast()
        {
            find_ports();
            Thread.Sleep(100);
            laserLine = new LaserLine(portArd);
        }
        void makePhotoLaser(float[] pos, string[] folders, ImageBox[] imageBoxes)
        {
            laserLine?.setShvpPos((int)pos[0]);
            Console.WriteLine("cur_pos: " + (int)pos[0]);
            Thread.Sleep(300);
            if (folders.Length == imageBoxes.Length)
            {
                for (int i = 0; i < folders.Length; i++)
                {

                    UtilOpenCV.saveImage(imageBoxes[i], folders[i], pos[0].ToString());
                }
            }
        }

        void makePhotoMarlin(float[] pos, string[] folders, ImageBox[] imageBoxes)
        {
            linearPlatf.setShvpPos(pos[0]);
            Console.WriteLine("cur_pos: " + (int)pos[0]);
            Thread.Sleep(300);
            if (folders.Length == imageBoxes.Length)
            {
                for (int i = 0; i < folders.Length; i++)
                {

                    UtilOpenCV.saveImage(imageBoxes[i], folders[i], pos[0].ToString());
                }
            }
        }

        void makeDoublePhotoLaser(float[] pos, string[] folders, ImageBox[] imageBoxes)
        {
            if (laserLine == null)
            {
                initLaserFast();
                Thread.Sleep(200);
                laserLine.laserOn();
            }
            laserLine?.setShvpPos((int)pos[0]);
            //Console.WriteLine("cur_pos: " + (int)pos[0]);
            laserLine.laserOn();
            Thread.Sleep(500);
            var mats_def = new Mat[folders.Length];
            var mats_las = new Mat[folders.Length];
            var mats_dif = new Mat[folders.Length];
            if (folders.Length == imageBoxes.Length)
            {
                for (int i = 0; i < folders.Length; i++)
                {
                    //UtilOpenCV.saveImage(imageBoxes[i], folders[i], pos[0] + ".png");
                    mats_def[i] = ((Mat)imageBoxes[i].Image).Clone();
                }
            }
            laserLine.laserOff();
            Thread.Sleep(500);

            for (int i = 0; i < folders.Length; i++)
            {
                mats_las[i] = ((Mat)imageBoxes[i].Image).Clone();
                
                if (mats_def[i] !=null && mats_las[i] != null)
                {
                    mats_dif[i] = (mats_def[i] - mats_las[i]).Clone();
                    Directory.CreateDirectory(folders[i] + "\\def");
                    Directory.CreateDirectory(folders[i] + "\\las");
                    Directory.CreateDirectory(folders[i] + "\\dif");
                    
                    (mats_def[i]?.ToImage<Bgr, Byte>()).Save((folders[i] + "\\def") + "\\" + Convert.ToString(pos[0]) + ".png");
                    (mats_las[i]?.ToImage<Bgr, Byte>()).Save((folders[i] + "\\las") + "\\" + Convert.ToString(pos[0]) + ".png");
                    (mats_dif[i]?.ToImage<Bgr, Byte>()).Save((folders[i] + "\\dif") + "\\" + Convert.ToString(pos[0]) + ".png");
                }
                
            }
            GC.Collect();
        }
        #endregion

        #region robot
        public void startScan(object sender, EventArgs e)
        {

            try
            {
                Thread robot_thread = new Thread(scan_res);
                robot_thread.Start(con1);
            }
            catch
            {
            }
        }
        async void scan()
        {
            var p1_cur_scun = p1_scan;
            var p2_cur_scun = p2_scan;
            var x = p1_cur_scun.x;
            var y = p1_cur_scun.y;
            var z = p1_cur_scun.z;

            con1.send_mes(x + " " + y + " " + z + " \n");
            int counts = 200;
            var delx = (p2_cur_scun.x - p1_cur_scun.x) / (double)counts;
            var dely = (p2_cur_scun.y - p1_cur_scun.y) / (double)counts;
            var delz = (p2_cur_scun.z - p1_cur_scun.z) / (double)counts;

            await Task.Delay(4000);
            for (int i = 0; i < counts; i++)
            {
                string name = " " + x + " " + y + " " + z + " .png";
                UtilOpenCV.saveImage(imageBox1, imageBox2, name, name_scan);

                x += delx;
                y += dely;
                z += delz;
                con1.send_mes(x + " " + y + " " + z + " \n");
                await Task.Delay(500);
            }


        }
        Point3d_GL pointFromTextBox(TextBox textBox1, TextBox textBox2, TextBox textBox3)
        {
            return new Point3d_GL(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text), Convert.ToDouble(textBox3.Text));
        }

        robFrame robFrameFromTextBox(TextBox textBox1, TextBox textBox2, TextBox textBox3, TextBox textBox4, TextBox textBox5, TextBox textBox6)
        {
            return new robFrame(Convert.ToDouble(textBox1.Text), Convert.ToDouble(textBox2.Text), Convert.ToDouble(textBox3.Text),
                Convert.ToDouble(textBox4.Text), Convert.ToDouble(textBox5.Text), Convert.ToDouble(textBox6.Text));
        }

        private void scan_res(object obj)
        {
            var con = (TCPclient)obj;
            int counts = Convert.ToInt32(boxN.Text);
            //var p1_cur_scan = p1_scan;
            //var p2_cur_scan = p2_scan;
            string folder_scan = box_scanFolder.Text;
            var p1_cur_scan = robFrameFromTextBox(nameX, nameY, nameZ, nameA, nameB, nameC);
            var p2_cur_scan = robFrameFromTextBox(nameX2, nameY2, nameZ2, nameA, nameB, nameC);
            float x = (float)p1_cur_scan.x;
            float y = (float)p1_cur_scan.y;
            float z = (float)p1_cur_scan.z;
            float a = (float)p1_cur_scan.a;
            float b = (float)p1_cur_scan.b;
            float c = (float)p1_cur_scan.c;

            var delx = (float)(p2_cur_scan.x - p1_cur_scan.x) / (float)counts;
            var dely = (float)(p2_cur_scan.y - p1_cur_scan.y) / (float)counts;
            var delz = (float)(p2_cur_scan.z - p1_cur_scan.z) / (float)counts;

            for (int i = 0; i < counts; i++)
            {

                makePhoto(
                    new float[] { x, y, z, a, b, c },
                    i,
                    new string[] { "cam1\\" + folder_scan, "cam2\\" + folder_scan },
                    new ImageBox[] { imageBox1, imageBox2 }
                    , con
                    );
                x += delx;
                y += dely;
                z += delz;
            }

        }


        #endregion

        #region events
        private void glControl1_Resize(object sender, EventArgs e)
        {
            GL1.resize(sender, e);
        }
        Button addButton(TransRotZoom trz, string name, int ind, Size sizeControl, Point locatControl, Size offset)
        {
            var recGL = new Rectangle(trz.rect.X, sizeControl.Height - trz.rect.Y - trz.rect.Height, trz.rect.Width, sizeControl.Height - trz.rect.Y);
            var but1 = new Button();
            but1.Location = new Point(recGL.X + locatControl.X + offset.Width, recGL.Y + locatControl.Y + offset.Height);
            but1.Size = new Size(20, 20);
            but1.AccessibleName = ind.ToString();
            but1.Text = name;
            return but1;
        }

        void addButForMonitor(GraphicGL graphicGL, Size sizeControl, Point locatControl)
        {
            int ind = 0;
            foreach (var trz in graphicGL.transRotZooms)
            {
                var but1 = addButton(trz, "P", ind, sizeControl, locatControl, new Size(0, 0));
                but1.Click += opGl_but_changePers;
                var but2 = addButton(trz, "V", ind, sizeControl, locatControl, new Size(23, 0));
                but2.Click += opGl_but_changeVisib;
                var but3 = addButton(trz, "S", ind, sizeControl, locatControl, new Size(46, 0));
                but3.Click += opGl_but_savePic;
                tabOpenGl.Controls.Add(but1);
                tabOpenGl.Controls.Add(but2);
                tabOpenGl.Controls.Add(but3);
                ind++;
            }
            glControl1.SendToBack();
        }
        void opGl_but_changePers(object sender, EventArgs e)
        {
            int i = Convert.ToInt32(((Button)sender).AccessibleName);
            GL1.changeViewType(i);
        }

        void opGl_but_changeVisib(object sender, EventArgs e)
        {
            int i = Convert.ToInt32(((Button)sender).AccessibleName);
            GL1.changeVisible(i);
        }
        void opGl_but_savePic(object sender, EventArgs e)
        {
            int i = Convert.ToInt32(((Button)sender).AccessibleName);
            GL1.SaveToFolder(openGl_folder, i);
        }
        private void glControl1_ContextCreated(object sender, GlControlEventArgs e)
        {
            var send = (Control)sender;
            GL1.glControl_ContextCreated(sender, e);
            var w = send.Width;
            var h = send.Height;
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(10, 0, 0), new Point3d_GL(0, 10, 0), new Point3d_GL(0, 0, 10));
            //generateImage3D_BOARD(chess_size.Width, chess_size.Height, markSize, PatternType.Mesh);
            //GL1.addFlat3d_XY_zero_s(-0.1, Color3d_GL.white());
            //GL1.SortObj();
            int monitor_num = 1;
            if(monitor_num==4)
            {
                GL1.addMonitor(new Rectangle(w / 2, 0, w / 2, h / 2), 0);
                GL1.addMonitor(new Rectangle(0, 0, w / 2, h / 2), 1, new Vertex3d(0, 60,0), new Vertex3d(100, 0, -60), 0);               
                GL1.addMonitor(new Rectangle(w / 2, h / 2, w / 2, h / 2), 2);
                GL1.addMonitor(new Rectangle(0, h / 2, w / 2, h / 2), 3);
            }
            else if (monitor_num == 2)
            {
                GL1.addMonitor(new Rectangle(0 , 0, w , h / 2), 0);
                GL1.addMonitor(new Rectangle(0 , h / 2, w , h / 2), 1, new Vertex3d(0, 60, 0), new Vertex3d(100, 0, -60), 0);
            }
            else
            {
                GL1.addMonitor(new Rectangle(0, 0, w, h), 0);
            }
            
           /* */
            //GL1.transRotZooms[1].xRot = 33;
            //GL1.transRotZooms[1].off_x = -25;
            //GL1.transRotZooms[1].off_y = 31;
            //GL1.transRotZooms[1].zoom = 2.6699;
            
            addButForMonitor(GL1, send.Size, send.Location);
            GL1.add_Label(lab_kor, lab_curCor,lab_TRZ);

            GL1.add_TreeView(tree_models);
            //UtilOpenCV.distortFolder(@"virtual_stereo\test6\monitor_0", GL1.cameraCV);
            //UtilOpenCV.distortFolder(@"virtual_stereo\test6\monitor_1", GL1.cameraCV);
            /*var p1 = new Point3d_GL(0, 0, 20);
            var p2 = new Point3d_GL(30, 0, 0);
            var p3 = new Point3d_GL(30, 30, 0);
            var p4 = new Point3d_GL(0, 30, 0);
            var polygs = new Polygon3d_GL[] { new Polygon3d_GL(p1, p2, p3), new Polygon3d_GL(p1, p4, p3) };
            var m1 = PathPlanner.proj_point_test(polygs[0], new Vector3d_GL(1, 0, 0));
            var ps = new Point3d_GL[] { p1, p2, p3 };
            GL1.addFrame(m1);
            GL1.addLineMesh(ps);*/

            // startGenerate();
            //trB_SGBM_Enter();
            // test_cross_triag();
            //test_smooth();
            //test_remesh();
            //add_points_cal();
            //load_ps_from_pulse("settings_pulse.json", new string[] { "b_2806a", "b_2806b", "b_2806c" });
            //test_cross_line_triang();
            //test_surf_rec();
            //test_surf_cross();
            //test_get_conts_3d();
            //test_arc();
            //test_patt();
            //test_cut();
            //test_traj_3d_pores();

            //GL1.addFlat3d_XY_zero_s(0);
            //GL1.addFlat3d_XZ_zero_s(50);
            /*var scan_stla = new Model3d("def2.stl", false);
            scan_stla.mesh = GL1.translateMesh(scan_stla.mesh, 0, 0, 20);
            GL1.add_buff_gl(scan_stla.mesh, scan_stla.color, scan_stla.normale, PrimitiveType.Triangles, "def2");*/
            //test_arc();
            //test_traj_def();
            //test_reconstr();
            //test_surf_rec_2();
            //test_find_cont_1();
        }

        private void glControl1_Render(object sender, GlControlEventArgs e)
        {

            GL1.glControl_Render(sender, e);
            /* if (GL1.rendercout == 0)
             {
                 UtilOpenCV.SaveMonitor(GL1);
             }*/

            bool find_gl = true;
            if (find_gl && GL1.transRotZooms.Count>1)
            {
                var mat1_or = GL1.matFromMonitor(0);
                var mat2_or = GL1.matFromMonitor(1);

                var mat1 = new Mat();
                var mat2 = new Mat();
                CvInvoke.Flip(mat1_or, mat1, FlipType.Vertical);
                if (mat2_or != null)
                {
                    CvInvoke.Flip(mat2_or, mat2, FlipType.Vertical);
                    CvInvoke.Rotate(mat2, mat2, RotateFlags.Rotate180);
                }
                //Console.WriteLine
                mat1 = UtilOpenCV.remapDistImOpenCvCentr(mat1, cameraDistortionCoeffs_dist);
                mat2 = UtilOpenCV.remapDistImOpenCvCentr(mat2, cameraDistortionCoeffs_dist);
                imBox_mark1.Image = mat1;
                imBox_mark2.Image = mat2;

                imProcess_virt(mat1, 1);
                imProcess_virt(mat2, 2);

                

                var corn = new System.Drawing.PointF[0];
                //var mat3 = UtilOpenCV.remapDistImOpenCvCentr(mat2, new Matrix<double>(new double[] { -0.5, 0, 0, 0, 0 }));
                //imBox_mark1.Image = FindCircles.findCircles(mat1_or, ref corn, chess_size);
                //imBox_mark2.Image = FindCircles.findCircles(mat2_or, ref corn, chess_size);


            }

            prop_gr_light.Update();
            /*var mat1_or = GL1.matFromMonitor(0);
            CvInvoke.Flip(mat1_or, mat1_or, FlipType.Vertical);
            System.Drawing.PointF[] corn = null;
            var err = UtilOpenCV.calcSubpixelPrec(chess_size, GL1, markSize, 0,PatternType.Mesh);
            Console.WriteLine(err[0].X +" "+ err[0].Y);*/
            //imBox_mark2.Image = imBox_mark2.Image;



            //GL1.printDebug(debugBox);

            /* var mat1 = UtilOpenCV.remapDistImOpenCvCentr(UtilOpenCV.GLnoise(mat1_or, 0, 10), cameraDistortionCoeffs_dist);
             var mat2 = UtilOpenCV.GLnoise(mat2_or, 0, 10);
             imBox_mark1.Image = mat2;
             imBox_mark2.Image = UtilOpenCV.drawChessboard(mat2, new Size(6, 7));*/

            //imBox_disparity.Image = features.drawDescriptorsMatch(ref mat1_or, ref mat2_or);

        }


        void test_traj_def()
        {
            var scan_stla = new Model3d("leg_wound4.stl", false);
            var pols = scan_stla.pols;

            //var pols2 = SurfaceReconstraction.get_conts_from_defect(pols);
            var meshs = Polygon3d_GL.toMesh(pols);
            //scan_stla.mesh = GL1.translateMesh(scan_stla.mesh, 0, 0, 20);
            GL1.add_buff_gl(meshs[0], meshs[1], meshs[2], PrimitiveType.Triangles, "def2");
        }

        void test_reconstr()
        {
            var scan_stla = new Model3d("def1.stl", false);
            var pols = scan_stla.pols;
            
            var pols2 = SurfaceReconstraction.get_conts_from_defect(pols);
            var meshs = Polygon3d_GL.toMesh(pols2);
            //scan_stla.mesh = GL1.translateMesh(scan_stla.mesh, 0, 0, 20);
            GL1.add_buff_gl(meshs[0], meshs[1], meshs[2], PrimitiveType.Triangles, "def2");
           // scan_stla.pols
        }
        void test_traj_color()
        {
           // var ps = new Point3d_GL[] {new Point3d_GL(1,1,0), new Point3d_GL(1, 10, 0) , new Point3d_GL(10, 10, 0) , new Point3d_GL(1, 1, 0) };
            //GL1.addTraj()
            
        }
        void test_surf_rec()
        {
            var scan_stla = new Model3d("defects\\def1c.stl", false);
            GL1.add_buff_gl(scan_stla.mesh, scan_stla.color, scan_stla.normale, PrimitiveType.Triangles, "def1a");
            var scan_stlb = new Model3d("defects\\def1d.stl", false);
            GL1.add_buff_gl(scan_stlb.mesh, scan_stlb.color, scan_stlb.normale, PrimitiveType.Triangles, "def1b");

            //SurfaceReconstraction.find_rec_lines(scan_stlb.pols, scan_stla.pols, 0.5,0.4, GL1);            
            var layers = SurfaceReconstraction.find_sub_surf_xy(scan_stlb.pols, scan_stla.pols, 0.5,2.5, 0.2,0.1);
            


            for (int i = 0; i < layers.Length; i++)
            {
                GL1.addMesh(Polygon3d_GL.toMesh(layers[i])[0], PrimitiveType.Triangles);
            }
        }
        void test_find_cont_1()
        {
            Console.WriteLine("load models");
            var scan_stl_orig = new Model3d("models\\defects\\ring3.stl", false);
            GL1.add_buff_gl(scan_stl_orig.mesh, scan_stl_orig.color, scan_stl_orig.normale, PrimitiveType.Triangles, "def_orig");
            var mesh = new IndexedMesh(scan_stl_orig.pols);
            var board = mesh.points_on_board();
            foreach (var cont in board)
            {
                var color = Color3d_GL.random();
                GL1.addLineMeshTraj(cont,color );
            }
        }
        void test_surf_rec_2()
        {
            Console.WriteLine("load models");
            var scan_stl_up = new Model3d("models\\defects\\def_up2a.stl", false);
            //GL1.add_buff_gl(scan_stl_up.mesh, scan_stl_up.color, scan_stl_up.normale, PrimitiveType.Triangles, "def_up2a");
            var scan_stl_down = new Model3d("models\\defects\\def_down2a.stl", false);
            //GL1.add_buff_gl(scan_stl_down.mesh, scan_stl_down.color, scan_stl_down.normale, PrimitiveType.Triangles, "def_down2a");
            var scan_stl_orig = new Model3d("models\\defects\\def_orig.stl", false);
            GL1.add_buff_gl(scan_stl_orig.mesh, scan_stl_orig.color, scan_stl_orig.normale, PrimitiveType.Triangles, "def_orig");
            Console.WriteLine("find_sub_surf_xy");
            //SurfaceReconstraction.find_rec_lines(scan_stlb.pols, scan_stla.pols, 0.5,0.4, GL1);            
            var layers = SurfaceReconstraction.find_sub_surf_xy(scan_stl_up.pols, scan_stl_down.pols,1, 2.5, 0.5, 0.3);
            
            var cuts = new List<string>();

            var conts = new List<List<Point3d_GL>>();
            var surfs = new List<Polygon3d_GL[]>();
            for (int i = 0; i < layers.Length; i++)
            {
               // if(i==0)
                {
                    Console.WriteLine("intersect: " + i);
                    var meshs = Polygon3d_GL.toMesh(layers[i]);

                    var mesh_sm_tr = meshs[0];
                    if (i==0) mesh_sm_tr = GL1.translateMesh(mesh_sm_tr, 0, 0, -0.5f);
                    surfs.Add(Polygon3d_GL.polygs_from_mesh(mesh_sm_tr));
                    //var rec = GL1.add_buff_gl(mesh_sm_tr, meshs[1], meshs[2], PrimitiveType.Triangles, "_cut_" + i);
                    //cuts.Add(rec);
                    var ps_inter = RasterMap.intersec_line_of_two_mesh(Polygon3d_GL.toMesh(surfs[surfs.Count - 1])[0], scan_stl_orig.mesh);
                    if (ps_inter == null) continue;
                    GL1.addLineMeshTraj(ps_inter, new Color3d_GL(1, 0, 0), "intersec_cut_" + i);
                    conts.Add(ps_inter.ToList());
                }
            }
            Console.WriteLine("gen_traj3d");
            var _traj = PathPlanner.generate_3d_traj_diff_surf(surfs, conts, param_tr,param_patt,GL1);
            
            rob_traj = PathPlanner.join_traj(_traj);
            var ps = PathPlanner.matr_to_traj(rob_traj);

            if (GL1.buffersGl.objs.Keys.Contains(traj_i)) GL1.buffersGl.removeObj(traj_i);

            //for (int i = 0; i < rob_traj.Count; i++) GL1.addFrame(rob_traj[i],2);
            if (ps == null) return;
            traj_i = GL1.addLineMeshTraj( ps.ToArray(), new Color3d_GL(0.9f), "gen_traj");
            var traj_rob = PathPlanner.generate_robot_traj(rob_traj, RobotFrame.RobotType.PULSE, param_tr);
        }
        void test_surf_cross()
        {
            var scan_stla = new Model3d("flat.stl", false);
            GL1.add_buff_gl(scan_stla.mesh, scan_stla.color, scan_stla.normale, PrimitiveType.Triangles, "def1a");
            var scan_stlb = new Model3d("wall.stl", false);
            GL1.add_buff_gl(scan_stlb.mesh, scan_stlb.color, scan_stlb.normale, PrimitiveType.Triangles, "def1b");
            // SurfaceReconstraction.find_rec_lines(scan_stlb.pols, scan_stla.pols, 0.5,0.4, GL1);
            var ps = RasterMap.intersec_line_of_two_mesh(scan_stla.mesh, scan_stlb.mesh);
            GL1.addLineMeshTraj(ps, new Color3d_GL(1, 0, 0), "intersec");

        }
        void test_pr()
        {
            var ps_2 = FileManage.loadFromJson("settings_pulse.json");
            var ps = new List<Point3d_GL>();
            ps.Add(ps_2[3]["b_2806a"].to_point3dgl());
            ps.Add(ps_2[3]["cal_1_2"].to_point3dgl());
            ps.Add(ps_2[3]["cal_1_3"].to_point3dgl());
            
            var ps2 = Point3d_GL.mult(ps.ToArray(), 1000);
            GL1.addPointMesh(ps2);
        }

        void load_ps_from_pulse(string path, string[] ps_n)
        {
            var ps_2 = FileManage.loadFromJson(path);
            var ps = new List<Point3d_GL>();
            foreach (var p in ps_n)
                ps.Add(ps_2[3][p].to_point3dgl());

            var ps2 = Point3d_GL.mult(ps.ToArray(), 1000);
            var dist = Point3d_GL.dist_betw_ps(ps2);
            prin.t(dist);
            GL1.addPointMesh(ps2);
        }

        void test_basis()
        {

            var basis1 = new Point3d_GL[] {
                new Point3d_GL(-0.91, -0.88, 1),
                new Point3d_GL(-0.91, 0.94, 1),
                new Point3d_GL(0.8, 0.94, 1),
                new Point3d_GL(0.8, -0.82, 0.5)};
            var basis2 = new Point3d_GL[] { 
                new Point3d_GL(-6.33, -9.64, 3.82),
                new Point3d_GL(-6.3, -7.84,3.82),
                new Point3d_GL(-4.56, -7.87,3.82),
                new Point3d_GL(-4.56, -9.64, 3.32)};

           /* var basis1 = new Point3d_GL[] {
                new Point3d_GL(0, 0, 0),
                new Point3d_GL(0, 10, 0),
                new Point3d_GL(10, 10, 0),
                new Point3d_GL(10, 0, 10)};
            var basis2 = new Point3d_GL[] {
                new Point3d_GL(0, 0, 0),
                new Point3d_GL(0, 5,0),
                new Point3d_GL(5, 5, 0),
                new Point3d_GL(5, 0, 5)};*/
            var transf = UtilMatr.calcTransformMatr_cv(basis1, basis2);
            prin.t(transf);
        }

        void test_get_conts()
        {



            var cont1 = new Point3d_GL[] {
                new Point3d_GL(0, 0, 0),
                new Point3d_GL(1+10, 0, 0),
                new Point3d_GL(1+10, 1, 0),
                new Point3d_GL(0, 1, 0.5)};
            var cont2 = new Point3d_GL[] {
                 new Point3d_GL(0+10, 0, 0),
                new Point3d_GL(1, 0, 0),
                new Point3d_GL(1, 1, 0),
                new Point3d_GL(0+10, 1, 0.5)};

            var conts_or = cont1.ToList();
            conts_or.AddRange(cont2);
            prin.t(conts_or.ToArray());
            prin.t("+++++++++++++++++++++++++");
            var conts_det = Point3d_GL.get_contours(conts_or.ToArray());
            foreach(var cont in conts_det)
            {
                prin.t(cont);
                prin.t("______________");
            }

        }
        
        
        void test_arc()
        {
            var ps1 = new Point3d_GL[] { new Point3d_GL(0, 0, 0), new Point3d_GL(0, 20,3), new Point3d_GL(20, 20, 3), new Point3d_GL(20, 0), new Point3d_GL(0, 0) };
            var ps = PathPlanner.gen_smooth_arc(ps1, 1.91, 0.2).ps.ToArray();
            prin.t(Point3d_GL.dist_betw_ps(ps));
            //RobotFrame.comp_acs(frms.ToList(), model_res / 10);
            GL1.addLineMeshTraj(ps, new Color3d_GL(1, 0, 0), "arc_test");
        }

    /* void test_arc()
     {
         var ps = PathPlanner.gen_arc_sect_xy(new Point3d_GL(0, 0, 0), new Point3d_GL(2, 2), 1.91, 0.6,false).ToArray();
         prin.t(Point3d_GL.dist_betw_ps(ps));
         var ps_l = PathPlanner.gen_harmonic_line_xy(new Point3d_GL(0, 0, 0), new Point3d_GL(20, 2), 2.91, 0.1,2, false).ToArray();

         GL1.addLineMeshTraj(ps_l, new Color3d_GL(1, 0, 0), "arc_test");
     }

     void test_patt()
     {
         var sett = new PatternSettings
         {
             step = 1,
             angle = PI / 4,
             min_dist = 0.1,
             arc_dist = 1,
             r = 2,
             patternType = PathPlanner.PatternType.Harmonic
         };
         var ps = PathPlanner.gen_pattern_in_square_xy(sett,new Point3d_GL(10, 10, 0), new Point3d_GL(20, 20)).ToArray();

         GL1.addLineMeshTraj(ps, new Color3d_GL(1, 0, 0), "patt_test");
     }

     void test_cut()
     {
         var sett = new PatternSettings
         {
             step = 1,
             angle = PI / 6,
             min_dist = 0.1,
             arc_dist = 1,
             r = 2,
             patternType = PathPlanner.PatternType.Harmonic
         };
         var contour = new Point3d_GL[] { new Point3d_GL(0, 0), new Point3d_GL(10, 10), new Point3d_GL(0, 20), new Point3d_GL(-10, 10) };
         var pattern = PathPlanner.gen_pattern_in_square_xy(sett, new Point3d_GL(-10, 0, 0), new Point3d_GL(10, 20));

         var ps_cut = PathPlanner.cut_pattern_in_contour_xy_cv(contour.ToList(), pattern);


         GL1.addLineMeshTraj(ps_cut.ToArray(), new Color3d_GL(1, 0, 0), "cut_test");
     }
     void test_traj_3d_pores()
     {
         param_tr.layers = 10;
         var contour = new Point3d_GL[] { new Point3d_GL(0, 0), new Point3d_GL(10, 10), new Point3d_GL(0, 20), new Point3d_GL(-10, 10) };
         var pattern = PathPlanner.gen_traj_3d_pores(param_patt,param_tr);
         var traj = PathPlanner.ps_to_matr(pattern);
         var prog = PathPlanner.generate_printer_prog(traj,param_tr);
         debugBox.Text = prog;
         GL1.addLineMeshTraj(pattern.ToArray(), new Color3d_GL(1, 0, 0), "pores_test");
     }*/
    void test_get_conts_3d()
        {

            var scan_stla = new Model3d("test\\cont_2a.stl", false);
            GL1.add_buff_gl(scan_stla.mesh, scan_stla.color, scan_stla.normale, PrimitiveType.Triangles, "def1a");
            var scan_stlb = new Model3d("test\\cont_2b.stl", false);
            GL1.add_buff_gl(scan_stlb.mesh, scan_stlb.color, scan_stlb.normale, PrimitiveType.Triangles, "def1b");

            var conts_det = RasterMap.intersec_conts_of_two_mesh(scan_stla.mesh, scan_stlb.mesh);
            
            foreach (var cont in conts_det)
            {
                GL1.addLineMeshTraj(cont, new Color3d_GL(1, 0, 0), "intersec");
            }

        }

        private void but_cross_flat_Click(object sender, EventArgs e)
        {

            var v = debugBox.Text.Trim().Split(' ');
            var f = new List<double>();
            for(int i = 0; i < v.Length; i++)
            {
                f.Add(Convert.ToDouble(v[i]));
            }

            
           SurfaceReconstraction.cross_obj_flats(GL1.buffersGl.objs[scan_i].vertex_buffer_data, f[0], f[1],GL1, f[2]);

        }


        
        Mat toMat(Bitmap bitmap)
        {
            var data = new byte[bitmap.Height, bitmap.Width, 3];
            for (int i = 0; i < bitmap.Width; i++)
            {
                for (int j = 0; j < bitmap.Height; j++)
                {
                    data[j, i, 0] = bitmap.GetPixel(i, j).B;
                    data[j, i, 1] = bitmap.GetPixel(i, j).G;
                    data[j, i, 2] = bitmap.GetPixel(i, j).B;
                }
            }
            return new Image<Bgr, byte>(data).Mat;
        }
        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            GL1.glControl_MouseMove(sender, e);

        }
        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            GL1.glControl_MouseDown(sender, e);
        }
        void test4pointHomo()
        {
            int id_mon = 0;
            var mat1 = GL1.matFromMonitor(id_mon);

            var points2D = new System.Drawing.PointF[points3D.Length];
            for (int i = 0; i < points3D.Length; i++)
            {
                var p = GL1.calcPixel(new Vertex4f(points3D[i].X, points3D[i].Y, points3D[i].Z, 1), id_mon);
                points2D[i] = new System.Drawing.PointF(p.X, p.Y);
            }
            GL1.cameraCV.compPos(points3D, points2D);
            var mxCam = matrixFromCam(GL1.cameraCV);
            prin.t(mxCam);
            prin.t("-------------------");
            prin.t(GL1.Vs[id_mon]);
            prin.t("-------------------");
            imBox_mark2.Image = mat1;
           
        }
        CameraCV projMatr(CameraCV cameraCV, int id_mon)
        {

            var points2D = new System.Drawing.PointF[points3D.Length];
            for (int i = 0; i < points3D.Length; i++)
            {
                var p = GL1.calcPixel(new Vertex4f(points3D[i].X, points3D[i].Y, points3D[i].Z, 1), id_mon);
                points2D[i] = new System.Drawing.PointF(p.X, p.Y);
            }
            cameraCV.pos = cameraCV.compPos(points3D, points2D);
            var mxCam = matrixFromCam(cameraCV);
            var prjCam = cameraCV.cameramatrix * mxCam;
            cameraCV.prjmatrix = prjCam;
            return cameraCV;
        }
        


        void load_subpix()
        {
            var folders = Directory.GetDirectories("subpix");
            var frms = new List<Frame[]>();
            foreach (var s in folders)
            {
                var frm = FrameLoader.loadImages_chess(s);
                frms.Add(frm);
                //comboImages.Items.AddRange(frm);
            }

            var fr_ar = frms.ToArray();

            for(int i=0; i<fr_ar.Length;i++)
            {
                for (int j = 0; j < fr_ar[i].Length; j++)
                {
                    for(int k = 1; k<13;k++)
                    {
                        var err = UtilOpenCV.calcSubpixelPrec(chess_size, GL1, markSize, 1, PatternType.Chess, fr_ar[i][j].im, fr_ar[i][j].name,k);
                        if(err[0].X<100000000000)
                        {
                            Console.WriteLine(err[0] + " " + err[1].X + " " + k);
                        }                       
                    }
                    Console.WriteLine("__________________________");
                }
            }
        }
        async void calibr_make_photo()
        {
            var zoom = 0.1;
            for(int i=0; i<20; i++)
            {
                for(int j=0; j <5; j++)
                {
                    
                    GL1.transRotZooms[1].zoom = zoom * i;
                    await Task.Delay(100);
                    UtilOpenCV.saveImage(imBox_mark1, "subpix\\calib_graph_" + i, GL1.transRotZooms[1].ToString() + j);
                }
                
            }           
        }
        Matrix<double> matrixFromCam(CameraCV cam)
        {
            var rotateMatrix = new Matrix<double>(3, 3);
            CvInvoke.Rodrigues(cam.cur_r, rotateMatrix);
            var tvec = UtilMatr.toVertex3f(cam.cur_t);
            var mx = UtilMatr.assemblMatrix(rotateMatrix, tvec);
            var datam = new double[3, 4];
            for(int i=0; i<datam.GetLength(0);i++)
            {
                for (int j = 0; j < datam.GetLength(1); j++)
                {
                    datam[i, j] = mx[(uint)i, (uint)j];
                }
            }
            // var invMx = mx.Inverse;
            return new Matrix<double>(datam);
        }      
        private void Form1_mousewheel(object sender, MouseEventArgs e)
        {
            GL1.Form1_mousewheel(sender, e);
        }

        #endregion

        #region buttons
        private void comboImages_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Console.WriteLine(comboImages.SelectedItem);
            Frame fr2 = null;
            if(comboImages.SelectedIndex>1)
            {
                fr2 = (Frame)comboImages.Items[comboImages.SelectedIndex - 2];

            }

            var fr = (Frame)comboImages.SelectedItem;
            Console.WriteLine(fr.frameType);

            if (fr.stereo)
            {
                var mat1 = fr.im;
                var mat2 = fr.im_sec;
                imageBox1.Image = mat1;
                imageBox2.Image = mat2;
                // CvInvoke.Rotate(mat2, mat2, RotateFlags.Rotate180);
                if (fr.frameType == FrameType.MarkBoard)
                {
                    imBox_base_1.Image = stereocam.remapCam(fr.im, 1);
                    imBox_base_2.Image = stereocam.remapCam(fr.im_sec, 2);
                    imBox_base.Image = UtilOpenCV.drawMatches(fr.im, CameraCV.findPoints((Mat)imBox_base_1.Image, chess_size), CameraCV.findPoints((Mat)imBox_base_2.Image, chess_size), 255, 0, 0);
                }
                else if (fr.frameType == FrameType.Pattern)
                {
                    System.Drawing.PointF[] corn = null;
                    if (true)
                    {

                        //imageBox1.Image = UtilOpenCV.drawInsideRectCirc(fr.im.Clone(), chess_size);
                        //imageBox2.Image = UtilOpenCV.drawInsideRectCirc(fr.im_sec.Clone(), chess_size);
                        //imBox_base_1.Image = GeometryAnalyse.findCirclesIter(fr.im.Clone(), ref corn, chess_size);
                        //imBox_base_2.Image = GeometryAnalyse.findCirclesIter(fr.im_sec.Clone(), ref corn, chess_size);
                        if (stereocam_scan != null)
                        {
                            imBox_base_1.Image = GeometryAnalyse.findCirclesIter(stereocam_scan.cameraCVs[0].undist(mat1.Clone()), ref corn, chess_size);
                            imBox_base_2.Image = GeometryAnalyse.findCirclesIter(stereocam_scan.cameraCVs[1].undist(mat2.Clone()), ref corn, chess_size);
                        }

                    }
                    else
                    {
                        imBox_base_1.Image = FindCircles.findCircles(mat1, ref corn, chess_size);
                        imBox_base_2.Image = FindCircles.findCircles(mat2, ref corn, chess_size);
                    }
                }

                else if (fr.frameType == FrameType.LasDif)
                {
                    var fr_im_cl = scanner.stereoCamera.cameraCVs[0].undist(fr.im);
                    var fr_im_sec_cl = scanner.stereoCamera.cameraCVs[1].undist(fr.im_sec);

                    var ps1 = Detection.detectLineDiff(fr_im_cl);
                    var ps2 = Detection.detectLineDiff(fr_im_sec_cl);
                    var ps1_dr = PointF.toSystemPoint_d(ps1);
                    var ps2_dr = PointF.toSystemPoint_d(ps2);


                    //var fr_im_cl = fr.im.Clone();
                    //var fr_im_sec_cl = fr.im_sec.Clone();
                    if (ps1_dr == null || ps2_dr == null) return;
                    CvInvoke.Line(fr_im_cl, ps1_dr[0], ps1_dr[ps1_dr.Length - 1], new MCvScalar(255, 0, 0));
                    CvInvoke.Line(fr_im_sec_cl, ps2_dr[0], ps2_dr[ps2_dr.Length - 1], new MCvScalar(255, 0, 0));
                    imBox_base_1.Image = UtilOpenCV.drawPoints(fr_im_cl, ps1_dr, 0, 255, 0, 1);
                    imBox_base_2.Image = UtilOpenCV.drawPoints(fr_im_sec_cl, ps2_dr, 0, 255, 0, 1);
                    
                    
                }
                    

                
            }
            else
            {
                if (fr.frameType == FrameType.Pos)
                {
                    FindMark.finPointFsFromIm(fr.im, bin_pos, imageBox1, imageBox4, maxArea, minArea);
                }
                else if (fr.frameType == FrameType.LasRob)
                {
                   //ContourAnalyse.findContourZ(fr.im, imageBox1, (int)red_c, DirectionType.Down);
                    var mat1 = fr.im.Clone();
                    var mat2 = fr.im.Clone();
                    var mat3 = fr.im.Clone();
                    var rgb = mat1.Split();
                    var ps = Detection.detectLineDiff(mat2,5);
                    mat3= UtilOpenCV.drawChessboard(mat3, chess_size);
                    //CvInvoke.GaussianBlur(mat1, mat1, new Size(3, 3), 0);
                    var im2 = mat2.ToImage<Gray, Byte>();

                    UtilOpenCV.drawPointsF(mat2, ps, 0, 255, 0);
                    
                    var im1 = mat1.ToImage<Gray, Byte>();
                    
                    

                    var im1_r = im1.Clone();
                    /*CvInvoke.Rotate(im1_r, im1_r, RotateFlags.Rotate180);
                    CvInvoke.Sobel(im1_r, im1_r, DepthType.Cv8U, 0, 1);
                    CvInvoke.Rotate(im1_r, im1_r, RotateFlags.Rotate180);
                    CvInvoke.Sobel(im1, im1, DepthType.Cv8U, 0, 1);*/

                    im1 = (0.3 * rgb[0] + 0.3 * rgb[1] + 0.3 * rgb[2]).ToImage<Gray, Byte>();
                    imBox_base_1.Image = im1;
                    imageBox2.Image = mat2;

                    imageBox1.Image = im1;

                }

                else if (fr.frameType == FrameType.LasDif)
                {
                    /* var mat1 = fr.im.Clone();
                     var mat2 = fr.im.Clone();
                     var rgb = mat1.Split();
                     var im1 = (rgb[0] + rgb[1] + rgb[2]).ToImage<Gray, Byte>();
                     var im2 = im1.Clone();
                 */
                    var mat = fr.im.Clone();
                    var ps = Detection.detectLineDiff(mat,7);
                   
                    /* var ps_t = new List<PointF>();
                     for(int i=0; i<500;i++)
                     {
                         var p = new PointF(500 + i * 0.1, i);
                         if (i>100 && i < 300)
                         {
                             if (i > 130 && i < 270)
                             {
                                 if (i > 160 && i < 230)
                                 {
                                     p.X -= 60;
                                 }
                                 p.X -= 60;
                             }
                             p.X -= 60;
                         }
                         else
                         {
                             p.X -= 200;
                         }
                         ps_t.Add(p);

                     }


                     */
                   // prin.t(ps);
                    //Console.WriteLine("___________");

                    var ps_xy = Detection.x_max_claster(ps, 2);

                   // prin.t(ps_xy);

                    UtilOpenCV.drawPointsF(mat, ps, 0, 255, 0,2);

                    //UtilOpenCV.drawPointsF(mat, ps_xy, 255, 255, 0, 2);
                    //imBox_base.Image = im2;
                    imageBox2.Image = scanner.cameraCV.undist(fr.im);
                    imageBox1.Image = scanner.cameraCV.undist(mat);


                }

                else if (fr.frameType == FrameType.LasHand)
                {
                    var mat1 = fr.im.Clone();
                    if(cameraCVcommon!=null)
                    {

                        var ps = Detection.detectLine(cameraCVcommon.undist(fr.im));
                        mat1 = cameraCVcommon.undist(mat1);
                        //fr.im = cameraCVcommon.undist(fr.im);
                        UtilOpenCV.drawPointsF(mat1, ps, 0, 255, 0);
                    }
                    mat1 =  UtilOpenCV.drawChessboard(mat1, chess_size);
                    imageBox1.Image = mat1;

                }
                else if (fr.frameType == FrameType.Test)
                {
                    imageBox1.Image = fr.im;
                    //findLaserArea(fr.im, imageBox1, (int)red_c);
                    //imageBox_debug_cam_2.Image =  drawDescriptors(fr.im);
                    //findContourZ(fr.im, imageBox1, (int)red_c, DirectionType.Up);
                    //var gauss = ContourAnalyse.findContourZ(fr.im, null, 245, DirectionType.Down);
                    //UtilOpenCV.drawPointsF(mat1, UtilMatr.doubleToPointF(gauss), 0, 0, 255);
                }

                else if (fr.frameType == FrameType.MarkBoard)
                {
                    imBox_debug1.Image = UtilOpenCV.drawChessboard(fr.im, chess_size);
                    imageBox1.Image = UtilOpenCV.drawInsideRectChessboard(fr.im, chess_size);

                    cameraCVcommon.compPos(fr.im, PatternType.Chess,chess_size);


                }
                else if (fr.frameType == FrameType.Pattern)
                {

                    imageBox2.Image = cameraCVcommon.undist(fr.im.Clone());
                    var corn = new System.Drawing.PointF[0];
                    //imageBox1.Image = UtilOpenCV.drawInsideRectCirc(fr.im, chess_size);
                    imageBox1.Image = GeometryAnalyse.findCirclesIter(fr.im.Clone(), ref corn, chess_size);
                    //imageBox2.Image = FindCircles.findCircles(fr.im,ref corn, chess_size);
                }
            }
            if (fr.frameType == FrameType.LasLin || fr.frameType == FrameType.LasDif)
            {



            }

            //imageBox2.Image = cameraCVcommon.undist(fr.im);
        }
        private void but_resize_Click(object sender, EventArgs e)
        {
            resize();
        }

        void resize()
        {
            var cur_size = new Size(1920, 1080);
            var target_size = new Size(1300, 700);

            var kx = target_size.Width / (double)cur_size.Width;
            var ky = target_size.Height / (double)cur_size.Height;

            var k = Math.Min(kx, ky);

            for (int j = 0; j < windowsTabs.Controls.Count; j++)
            {
                var tab = (TabPage)windowsTabs.Controls[j];
                for (int i = 0; i < tab.Controls.Count; i++)
                {
                    tab.Controls[i].Location = new Point((int)(tab.Controls[i].Location.X * k), (int)(tab.Controls[i].Location.Y * k));
                    tab.Controls[i].Size = new Size((int)(tab.Controls[i].Size.Width * k), (int)(tab.Controls[i].Size.Height * k));
                    tab.Controls[i].Font = new Font(tab.Controls[i].Font.Name, (float)(tab.Controls[i].Font.Size * k));
                }
            }
        }

        void chekLines()
        {
            /*var ps_list = ps.ToList().GetRange((int)(ps.Length / 10), ps.Length  - (int)(ps.Length / 5));
                var ps_im = LineF.find2Points(LineF.calcLine(ps_list.ToArray()), imageBox1.Size).ToList().GetRange(0,2).ToArray();

                var line_left = LineF.calcLine(new PointF[]
                {
                    ps_list[0],ps_list[ps_list.Count / 2],
                }) ;

                var line_right = LineF.calcLine(new PointF[]
                 {
                    ps_list[ps_list.Count / 2],ps_list[ps_list.Count -1],
                 });
                Console.WriteLine(line_left + " " + line_right);

                var ps_im_left = LineF.find2Points(line_left, imageBox1.Size).ToList().GetRange(0, 2).ToArray();
                var ps_im_right = LineF.find2Points(line_right, imageBox1.Size).ToList().GetRange(0, 2).ToArray();
                var mat_dr = UtilOpenCV.drawLines(fr.im.Clone(), ps_im_left, 0, 255, 0);
                imageBox2.Image = UtilOpenCV.drawLines(mat_dr, ps_im_right, 0, 255, 0);
                
                
                var sob_im = FindCircles.sobel(fr.im_sec.ToImage<Gray, byte>()).Convert<Bgr,byte>().Mat;*/

            /*GL1.buffersGl.clearObj();
             GL1.addMesh(
                meshFromImage(
                    (fr.im-fr.im_sec-sob_im).Clone()
                    .ToImage<Gray, byte>(),2f)
                ,PrimitiveType.Triangles);*/
            //GL1.buffersGl.sortObj();
            // imageBox1.Image = fr.im - fr.im_sec;
            //imageBox2.Image = fr.im - fr.im_sec - sob_im;
        }

        private void but_set_wind_Click(object sender, EventArgs e)
        {
            var but = (Button)sender;
            if (settingWindow)
            {
                settingWindow = false;
                but.Text = "Установить окно";
                imBox_pattern.Location = cam_calib_p1;
                imBox_pattern.Size = new Size(cam_calib_p2.X - cam_calib_p1.X, cam_calib_p2.Y - cam_calib_p1.Y);
            }
            else
            {
                settingWindow = true;
                but.Text = "Выйти из режима";
                imBox_pattern.Location = new Point(0, 0);
                imBox_pattern.Size = new Size(10, 10);
            }
        }

        private void tabCalibMonit_MouseDown(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("down");

            if (e.Button == MouseButtons.Right)
            {
                cam_calib_p1.X = e.X;
                cam_calib_p1.Y = e.Y;
            }

        }

        private void tabCalibMonit_MouseMove(object sender, MouseEventArgs e)
        {
            // Console.WriteLine("move");
            if (e.Button == MouseButtons.Right)
            {
                cam_calib_p2.X = e.X;
                cam_calib_p2.Y = e.Y;
                lab_pos_mouse.Text = "p1: " + cam_calib_p1 + "\n p2: " + cam_calib_p2;
            }

        }

        private void tabCalibMonit_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void tabCalibMonit_Paint(object sender, PaintEventArgs e)
        {

            // var g = e.Graphics;
            var g = tabCalibMonit.CreateGraphics();
            Pen pen1 = new Pen(Color.Black);
            pen1.Width = 2;
            // Console.WriteLine("paint");
            //  g.DrawString()
            if (settingWindow)
            {

                if (cam_calib_p1 != null && cam_calib_p2 != null)
                {

                    //Console.WriteLine("notNull");
                    var w = cam_calib_p2.X - cam_calib_p1.X;
                    var h = cam_calib_p2.Y - cam_calib_p1.Y;
                    if (w > 0 && h > 0)
                    {
                        // Console.WriteLine("wh"+w+" "+h);
                        g.Clear(Color.White);
                        g.DrawRectangle(pen1, cam_calib_p1.X, cam_calib_p1.Y, w, h);

                    }

                }
            }
            // tabCalibMonit.CreateGraphics
            //this.InvokePaint((Control)tabCalibMonit, e);
            // this.Update();

        }
        private void but_gl_cam_calib_Click(object sender, EventArgs e)
        {
            CameraCV.calibrMonit(imBox_pattern, new ImageBox[] { imBox_mark1, imBox_mark2 }, patt, txBx_photoName.Text,  GL1);
            GL1.printDebug(debugBox);
        }
        private void but_calib_Start_Click(object sender, EventArgs e)
        {
            
            CameraCV.calibrMonit(imBox_pattern, new ImageBox[]{ imBox_input_1, imBox_input_2 }, patt, txBx_photoName.Text, null);
        }
        private void tr_Persp_Scroll(object sender, EventArgs e)
        {
            var trbar = (TrackBar)sender;
            var ind = Convert.ToInt32(trbar.AccessibleName);
            var txbox = textBoxes_Persp[ind];
            var mult = Convert.ToDouble(txbox.Text);
            var val = mult* (double)trbar.Value/100;
            var i = ind % 3;
            var j = (ind - i) / 3;
            persp_matr[i, j] = val;
            //prin.t("i " + i + " j " + j);
            
            
            //CvInvoke.WarpPerspective(patt, patt_warp, persp_matr, patt.Size+ patt.Size,Inter.Linear,Warp.Default);
            var ptt = UtilOpenCV.warpPerspNorm(patt, persp_matr, imBox_pattern.Size);
            var mat1 = ptt[0];
           // prin.t(ptt[1]);
            
            imBox_pattern.Image = mat1;
            //var ps = CvInvoke.FindCirclesGrid(mat1.ToImage<Gray,byte>(), new Size(6, 7), CalibCgType.SymmetricGrid,new Emgu.CV.Features2D.SimpleBlobDetector());
            
            //UtilOpenCV.drawPointsF(mat1, ps, 255, 0, 5, 10);
           //  imBox_input_1.Image = FindCircles.findCircles( mat1,null, new Size(6,7));
            UtilOpenCV.calcSubpixelPrecCircle(chess_size, ptt);
            //imBox_input_1.Image = UtilOpenCV.drawChessboard((Mat)imBox_pattern.Image, new Size(7, 6),false,true);
        }
        private void but_SubpixPrec_Click(object sender, EventArgs e)
        {
            // var mat1 = (Mat)imBox_mark1.Image;
            // var mat2 = (Mat)imBox_mark2.Image;
            /* var im_name = "tsukuba";//"tsukuba";//"venus"
             var mat1 = new Mat(@"datasets\" + im_name + @"\im2.png");
             var mat2 = new Mat(@"datasets\" + im_name + @"\im6.png");
             var disp = UtilOpenCV.PaintLines(mat1.ToImage<Gray, byte>(), mat2.ToImage<Gray, byte>(), mat1.Height / 2,features);
             var depth = new Mat();

             var hist = UtilOpenCV.histogram(disp[1].Mat);
             depth = UtilOpenCV.normalize(disp[1].Mat);
             imBox_3dDebug.Image = UtilOpenCV.normalize(disp[1].Mat);
             imBox_disparity.Image = UtilOpenCV.normalize(disp[2].Mat);*/

            /*var disp = features.disparMap((Mat)imBox_mark1.Image, (Mat)imBox_mark2.Image, 30, 3);
            imBox_3dDebug.Image = disp[0];
            var mesh = meshFromImage(disp[0].ToImage<Gray, byte>());
            mesh =  GL1.scaleMesh(mesh, 1f, 0.5f, 0.5f, 2f);
            mesh = GL1.translateMesh(mesh, 200f);
            GL1.addMesh(mesh, PrimitiveType.Triangles, 0.9f);*/


            var mat1_or = GL1.matFromMonitor(0);
            CvInvoke.Flip(mat1_or, mat1_or, FlipType.Vertical);
            System.Drawing.PointF[] corn = null;
            var err = UtilOpenCV.calcSubpixelPrec(chess_size, GL1, markSize, 0, PatternType.Mesh);
            Console.WriteLine(err[0].X + " " + err[0].Y);

            //calibr_make_photo();
        }
        private void trB_SGBM_Scroll(object sender, EventArgs e)
        {
            var trbar = (TrackBar)sender;
            var val = trbar.Value;
            var bs = stereocam.solver_param.blockSize;
            switch (Convert.ToInt32(trbar.AccessibleName))
            {
                case 1://minDisp
                    stereocam.solver_param.minDisparity = val;
                    break;
                case 2://numDisp
                    stereocam.solver_param.numDisparities = 16 * val;
                    break;
                case 3://blockSize
                    stereocam.solver_param.blockSize = val;
                    break;
                case 4://p1
                    stereocam.solver_param.p1 = bs * bs * val;
                    break;
                case 5://p2
                    stereocam.solver_param.p2 = bs * bs * val;
                    break;
                case 6://disp12Max
                    stereocam.solver_param.disp12MaxDiff = val;
                    break;
                case 7://prefilt
                    stereocam.solver_param.preFilterCap = val;
                    break;
                case 8://uniq
                    stereocam.solver_param.uniquenessRatio = val;
                    break;
                case 9://specleWindS
                    stereocam.solver_param.speckleWindowSize = 20 * val;
                    break;
                case 10://specleRange
                    stereocam.solver_param.speckleRange = val;
                    break;
                default:
                    break;
            }
            stereocam.setSGBM_parameters();
        }
        private void trB_SGBM_Enter()
        {
            var bs = stereocam.solver_param.blockSize;
            trackBar1.Value = stereocam.solver_param.minDisparity;
            trackBar2.Value = stereocam.solver_param.numDisparities / 16;
            trackBar3.Value = stereocam.solver_param.blockSize;
            trackBar4.Value = stereocam.solver_param.p1 / (bs * bs);
            trackBar5.Value = stereocam.solver_param.p2 / (bs * bs);
            trackBar6.Value = stereocam.solver_param.disp12MaxDiff;
            trackBar7.Value = stereocam.solver_param.preFilterCap;
            trackBar8.Value = stereocam.solver_param.uniquenessRatio;
            trackBar9.Value = stereocam.solver_param.speckleWindowSize / 20;
            trackBar10.Value = stereocam.solver_param.speckleRange;
        }

        private void but_imGen_Click(object sender, EventArgs e)
        {
            UtilOpenCV.generateImagesFromAnotherFolderStereo(new string[] { @"virtual_stereo\test1\monitor_0", @"virtual_stereo\test1\monitor_1" },GL1,
                new CameraCV(cameraMatrix_dist,cameraDistortionCoeffs_dist,new Size(400,400)));//size not right!!!!!!!
     
        }
        private void but_swapMonit_Click(object sender, EventArgs e)
        {
            GL1.swapTRZ(0, 1);
        }
        private void but_modeV_Click(object sender, EventArgs e)
        {
            var but = (Button)sender;
            if (GL1.modeGL == modeGL.Paint)
            {
                but.Text = "Обзор";
                GL1.modeGL = modeGL.View;
            }
            else
            {
                but.Text = "Рисование";
                GL1.modeGL = modeGL.Paint;
            }
        }
        private void trackX_light_Scroll(object sender, EventArgs e)
        {
            GL1.lightXscroll(trackX_light.Value);
        }

        private void trackY_light_Scroll(object sender, EventArgs e)
        {
            GL1.lightYscroll(trackY_light.Value);
        }

        private void trackZ_light_Scroll(object sender, EventArgs e)
        {
            GL1.lightZscroll(trackZ_light.Value);
        }
        private void rob_con_Click(object sender, EventArgs e)
        {
            con1 = new TCPclient();           
            string iiwa = "172.31.1.147";
            string pulse = "localhost";
            port_tcp = Convert.ToInt32(tb_port_tcp.Text);
            con1.Connection(port_tcp,pulse );

            Thread tcp_thread = new Thread(recieve_tcp);
            tcp_thread.Start(con1);

        }
        bool printing = false;
        void recieve_tcp(object obj)
        {
            var con = (TCPclient)obj;
            
            while (con.is_connect())
            {
                if (printing)
                {
                    var res = con.reseav();
                    //Console.WriteLine(res);
                    resend_rob_to_ard_extr(res, laserLine);
                }  
                Thread.Sleep(10);
            }
        }

        void resend_rob_to_ard_extr(string mes, LaserLine ard)
        {
            if (ard == null) return;
            if (mes == null) return;
            if (mes.Length < 2) return;
            var vals_str = mes.Split(' ');
            if (vals_str.Length != 2) return;
            var vel = Convert.ToDouble(vals_str[0]);
            var dir = Convert.ToDouble(vals_str[1]);
            Console.WriteLine(vel + " " + dir);
            ard.set_dir_disp((int)-dir);
            //ard.set_div_disp(vel);
        }

        private void but_res_pos1_Click(object sender, EventArgs e)
        {
            var posRob = positionFromRobot(con1);
            if (posRob != null)
            {
                nameX.Text = posRob.x.ToString();
                nameY.Text = posRob.y.ToString();
                nameZ.Text = posRob.z.ToString();
                nameA.Text = posRob.a.ToString();
                nameB.Text = posRob.b.ToString();
                nameC.Text = posRob.c.ToString();
            }
           
        }

        private void but_res_pos_2_Click(object sender, EventArgs e)
        {
            var posRob = positionFromRobot(con1);
            nameX2.Text = posRob.x.ToString();
            nameY2.Text = posRob.y.ToString();
            nameZ2.Text = posRob.z.ToString();
            nameA2.Text = posRob.a.ToString();
            nameB2.Text = posRob.b.ToString();
            nameC2.Text = posRob.c.ToString();
        }
        private void rob_res_Click(object sender, EventArgs e)
        {
            try
            {
                //con1.send_mes("a\n");
                var mes = con1.reseav();
                Console.WriteLine(mes);
            }
            catch
            {

            }
            //var points = points1.Split(new char[] { ',' });
        }
        private void but_photo_Click(object sender, EventArgs e)
        {
            try
            {
                con1.send_mes("f\n");
                Thread.Sleep(50);
                var res = con1.reseav();
                res = res.Replace('\n', ' ');
                res = res.Replace('\r', ' ');
                Console.WriteLine(res);
                Console.WriteLine("---");
                UtilOpenCV.saveImage(imageBox1, imageBox2, res + ".png", box_photoFolder.Text);
                Console.WriteLine(res);
            }
            catch
            {

            }
        }
        robFrame positionFromRobot(TCPclient con)
        {
            if (con != null)
            {
                try
                {
                    con1.send_mes("f\n");
                    Thread.Sleep(50);
                    var res = con1.reseav();
                    Console.WriteLine(res);
                    if (res == null || res.Length < 10)
                    {
                        return null;
                    }
                    res = res.Trim();
                    var res_s = res.Split(' ');
                    double x = Convert.ToDouble(res_s[0]);
                    double y = Convert.ToDouble(res_s[1]);
                    double z = Convert.ToDouble(res_s[2]);
                    double a = Convert.ToDouble(res_s[3]);
                    double b = Convert.ToDouble(res_s[4]);
                    double c = Convert.ToDouble(res_s[5]);
                    Console.WriteLine(res);
                    return new robFrame(x, y, z, a, b, c);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
        private void bet_res_pos_Click(object sender, EventArgs e)
        {
            var posRob = positionFromRobot(con1);
            if (posRob == null) return;
            nameX_in.Text = posRob.x.ToString();
            nameY_in.Text = posRob.y.ToString();
            nameZ_in.Text = posRob.z.ToString();
            nameA_in.Text = posRob.a.ToString();
            nameB_in.Text = posRob.b.ToString();
            nameC_in.Text = posRob.c.ToString();
        }
        private void rob_send_Click(object sender, EventArgs e)
        {
            try
            {
                var mes = nameX.Text + " " + nameY.Text + " " + nameZ.Text + " " + nameA.Text + " " + nameB.Text + " " + nameC.Text + " " + boxN.Text + " \n";
                con1.send_mes(mes);
                Console.WriteLine(mes);
            }
            catch
            {

            }

        }
        

        void makePhoto(float[] pos, int count, string[] folders, ImageBox[] imageBoxes, TCPclient con)
        {
            var text = "";
            foreach (var ps in pos)
            {
                if (ps.ToString().Contains('.') == false)
                {
                    text += ps.ToString() + ".0 ";
                }
                else
                {
                    text += ps.ToString() + " ";
                }

            }
            text += count.ToString() + ".0\n";
            con.send_mes(text);
            Console.WriteLine(text);

            const int ind_max = 200;
            int ind = 0;
            string res = null;
            bool right_num = false;
            var name_ph = "";
            while (right_num == false && ind < ind_max)
            {
                Thread.Sleep(100);
                con.send_mes("a\n");
                res = con.reseav();
                if (res != null)
                {
                    if (res.Length > 5)
                    {
                        res = res.Trim();
                        if (res.Contains('\n'))
                        {
                            var name_phs = res.Split('\n');
                            name_ph = name_phs[name_phs.Length - 1];
                        }
                        else
                        {
                            name_ph = res;
                        }

                        name_ph = name_ph.Replace('\n', ' ');
                        name_ph = name_ph.Replace('\r', ' ');
                        name_ph = name_ph.Trim();
                        var num_r1 = name_ph.Replace("   ", " ");
                        var num_r2 = num_r1.Replace("  ", " ");
                        var numb_s = num_r2.Split(new char[] { ' ' });
                        Console.WriteLine(numb_s[numb_s.Length - 1]);
                        if (numb_s.Length > 2)
                        {
                            if (count == (int)Convert.ToDouble(numb_s[numb_s.Length - 1]))
                            {
                                if (folders.Length == imageBoxes.Length)
                                {
                                    for (int i = 0; i < folders.Length; i++)
                                    {

                                        UtilOpenCV.saveImage(imageBoxes[i], folders[i], name_ph + ".png");
                                        right_num = true;
                                    }
                                }
                            }
                        }
                    }


                }
                ind++;
            }

        }
        private void rob_discon_Click(object sender, EventArgs e)
        {
            try
            {
                con1.send_mes("q\n");
                con1.close_con();
            }
            catch
            {

            }
        }

        private void glControl1_Load(object sender, EventArgs e)
        {

        }

       

        private void comboVideo_SelectedIndexChanged(object sender, EventArgs e)
        {
            var combo = (ComboBox)sender;
            var v = (VideoFrame)combo.SelectedItem;

            var mat = findMostWhite(v.im);
            var im_out = binVideo(mat, imageBox2, (int)red_c);
            imageBox1.Image = im_out;
            comboNumber.Items.Clear();
            for (int i = 0; i < v.im.Length; i++)
            {
                comboNumber.Items.Add(i);
            }
        }
        private void comboNumber_SelectedIndexChanged(object sender, EventArgs e)
        {
            var combo = (ComboBox)sender;
            var v = (VideoFrame)comboVideo.SelectedItem;
            int i = (int)combo.SelectedItem;
            if (i < v.im.Length)
            {
                var im = v.im[i];
                imageBox3.Image = im;
                var im_out = binVideo(im, imageBox2, (int)red_c);
                imageBox1.Image = im_out;

            }
        }
        private void trackBar1_Scroll(object sender, EventArgs e)
        {

        }
        

        private void butSave_Click(object sender, EventArgs e)
        {
            string name = " " + nameX.Text + " " + nameY.Text + " " + nameZ.Text + " " + boxN.Text + " .png";
            UtilOpenCV.saveImage(imageBox1, imageBox2, name, "single");
            Console.WriteLine(name);

        }
        private void butSaveIm_Click(object sender, EventArgs e)
        {
            UtilOpenCV.saveImage(imageBox1, imageBox2, txBx_photoName.Text +"_"+ photo_number.ToString()+ ".png", box_photoFolder.Text);
            photo_number++;
        }
        private void butSaveSing_Click(object sender, EventArgs e)
        {
            string name = " " + nameX.Text + " " + nameY.Text + " " + nameZ.Text + " " + boxN.Text + " .png";
            UtilOpenCV.saveImage(imageBox1, "saved", name);
            Console.WriteLine(name);
        }
        
        private void butFinPointFs_Click(object sender, EventArgs e)
        {
            var ids = textBox_monitor_id.Text.Trim().Split(' ');
            for(int i=0; i < ids.Length; i++)
            {
                GL1.SaveToFolder(openGl_folder, Convert.ToInt32(ids[i]));
            }           
        }
        private void butStart_Click(object sender, EventArgs e)
        {
            int fcc = VideoWriter.Fourcc('M', 'P', '4', 'V'); //'M', 'J', 'P', 'G'
            int fps = 15;
            string name = nameX.Text + " " + nameY.Text + " " + nameZ.Text + " .mp4";
            videoframe_count = 0;
            writer = new VideoWriter(name, fcc, fps, new Size(myCapture1.Width, myCapture1.Height), true);
            writ = true;
        }

        private void butStop_Click(object sender, EventArgs e)
        {
            writ = false;
            writer = null;

        }
        private void videoCapt_Click(object sender, EventArgs e)
        {
            im_i = 0;
            flag1 = 0;
        }
        private void trackBin_Scroll(object sender, EventArgs e)
        {

        }

        /* private void trackR_Scroll(object sender, EventArgs e)
         {
             red_c = trackR.Value;
             label4.Text = red_c.ToString();
              histogramBox1.ClearHistogram();
              histogramBox1.GenerateHistograms(imageBox4.Image, 256);
              histogramBox1.Refresh();
         }

         private void trackG_Scroll(object sender, EventArgs e)
         {
             green_c = trackG.Value;
         }

         private void trackB_Scroll(object sender, EventArgs e)
         {
             blue_c = trackB.Value;
         }


         private void trackMinArea_Scroll(object sender, EventArgs e)
         {
             minArea = 2.5 * k *k* trackMinArea.Value;
             Console.WriteLine("min = " + minArea);
         }

         private void trackMaxArea_Scroll(object sender, EventArgs e)
         {
             maxArea = 15 * k *k* trackMaxArea.Value;
             Console.WriteLine("max = " + maxArea);

         }*/

        private void butCalcIm_Click(object sender, EventArgs e)
        {
            binImage(imageBox1, red_c, green_c);
        }

        private void trackOx_Scroll(object sender, EventArgs e)
        {
            var track = (TrackBar)sender;
            GL1.orientXscroll(track.Value);
        }

        private void trackOz_Scroll(object sender, EventArgs e)
        {
            var track = (TrackBar)sender;
            GL1.orientYscroll(track.Value);
        }

        private void trackOy_Scroll(object sender, EventArgs e)
        {
            var track = (TrackBar)sender;
            GL1.orientZscroll(track.Value);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path);
            if (con1 != null)
            {
                
                con1.send_mes("q\n");//g
                con1.close_con();
            }
        }

        private void but_ProjV_Click(object sender, EventArgs e)
        {
            var but = (Button)sender;
            if (GL1.typeProj == viewType.Ortho)
            {
                but.Text = "Проецирование(текущая: перспектива)";
                GL1.typeProj = viewType.Perspective;
            }
            else
            {
                but.Text = "Проецирование(текущая: ортоганальная)";
                GL1.typeProj = viewType.Ortho;
            }
        }

        private void but_plane_Oxy_Click(object sender, EventArgs e)
        {
            GL1.planeXY();
        }

        private void but_plane_Oyz_Click(object sender, EventArgs e)
        {
            GL1.planeYZ();
        }

        private void butt_plane_Ozx_Click(object sender, EventArgs e)
        {
            GL1.planeZX();
        }
        private void but_robMod_Click(object sender, EventArgs e)
        {
            RobotModel_1 = new RobotModel(new RobotFrame(500, 520, 150, 0.3, 0.1, 1.4), 8888);
            //RobotModel_1.move(new RobotFrame(620, 120, 150, 0.3, 0.1, 1.4), 10, 5);
            //Thread.Sleep(5500);
            //RobotModel_1.move(new RobotFrame(620, 120, 150, 0.3, 0.5, 1.4), 10, 5,true);
            //RobotModel_1.move(new RobotFrame(620, 150, 150, 0.3, 0.5, 1.4), 10, 5, true);
            //var frms_2 = RobotFrame.divide_line(new RobotFrame(600, 120, 150, 0.3, 0.1, 1.4), new RobotFrame(620, 120, 150, 0.3, 0.1, 1.4),0.1);

            var frms = new RobotFrame[] { new RobotFrame(520, 520, 150, 0.3, 0.1, 1.4), new RobotFrame(520, 550, 150, 0.3, 0.5, 1.4), new RobotFrame(550, 550, 150, 0.3, 0.5, 1.4) };
            RobotModel_1.move(frms,10, 5,true,true);
            //RobotModel_1.move(new robFrame(590, 110, 150, 0.3, 0.1, 1.4), 30, 30);
            //Thread.Sleep(500);

            //RobotModel_1.move(new robFrame(620, 120, 150, 0.3, 0.1, 1.4), 30, 30);
            //con1 = new TCPclient();
            //con1.Connection(8888, "127.0.0.1");
            //Thread.Sleep(400);
            //var mes = con1.reseav();
            //Console.WriteLine(mes);
        }

        private void but_addBufRob_Click(object sender, EventArgs e)
        {
            RobotModel_1.sendMes(box_scanFolder.Text);
        }

        private void but_comDist_Click(object sender, EventArgs e)
        {
            int K1deg = Convert.ToInt32(textBox_K1deg.Text);
            int K2deg = Convert.ToInt32(textBox_K2deg.Text);
            int K3deg = Convert.ToInt32(textBox_K3deg.Text);
            int P1deg = Convert.ToInt32(textBox_P1deg.Text);
            int P2deg = Convert.ToInt32(textBox_P2deg.Text);
            double K1 = Convert.ToDouble(textBox_K1.Text);
            double K2 = Convert.ToDouble(textBox_K2.Text);
            double K3 = Convert.ToDouble(textBox_K3.Text);
            double P1 = Convert.ToDouble(textBox_P1.Text);
            double P2 = Convert.ToDouble(textBox_P2.Text);
            cameraDistortionCoeffs_dist[0, 0] = K1 * Math.Pow(10, K1deg);
            cameraDistortionCoeffs_dist[1, 0] = K2 * Math.Pow(10, K2deg);
            cameraDistortionCoeffs_dist[2, 0] = P1 * Math.Pow(10, P1deg);
            cameraDistortionCoeffs_dist[3, 0] = P2 * Math.Pow(10, P2deg);
            cameraDistortionCoeffs_dist[4, 0] = K3 * Math.Pow(10, K3deg);

            var frame = (Frame)comboImages.SelectedItem;
            // imageBox_cameraDist.Image = remapDistIm(frame.im  , cameraMatrix, cameraDistortionCoeffs);
            imageBox_cameraDist.Image = UtilOpenCV.remapDistImOpenCvCentr(frame.im, cameraDistortionCoeffs_dist);


            //imageBox_cameraDist.Image =  ;
        }

        private void imageBox_cameraDist_MouseMove(object sender, MouseEventArgs e)
        {
            label_corPic.Text = e.X + " " + e.Y;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            int K1deg = Convert.ToInt32(textBox_K1deg.Text);
            int K2deg = Convert.ToInt32(textBox_K2deg.Text);
            int K3deg = Convert.ToInt32(textBox_K3deg.Text);
            int P1deg = Convert.ToInt32(textBox_P1deg.Text);
            int P2deg = Convert.ToInt32(textBox_P2deg.Text);
            double K1 = Convert.ToDouble(textBox_K1.Text);
            double K2 = Convert.ToDouble(textBox_K2.Text);
            double K3 = Convert.ToDouble(textBox_K3.Text);
            double P1 = Convert.ToDouble(textBox_P1.Text);
            double P2 = Convert.ToDouble(textBox_P2.Text);
            cameraDistortionCoeffs[0, 0] = K1 * Math.Pow(10, K1deg);
            cameraDistortionCoeffs[1, 0] = K2 * Math.Pow(10, K2deg);
            cameraDistortionCoeffs[2, 0] = P1 * Math.Pow(10, P1deg);
            cameraDistortionCoeffs[3, 0] = P2 * Math.Pow(10, P2deg);
            cameraDistortionCoeffs[4, 0] = K3 * Math.Pow(10, K3deg);

            var frame = (Frame)comboImages.SelectedItem;
            imageBox_cameraDist.Image = UtilOpenCV.remapUnDistIm(frame.im, cameraMatrix, cameraDistortionCoeffs);
        }
        #endregion

        #region cameraUtil
        void capturingVideo(object sender, EventArgs e)
        {
            drawCameras((VideoCapture)sender);
        }

   
        void drawCameras(VideoCapture cap)
        {
            if (camera_ind != null)
            {

                if ((camera_ind.Count > 0) && (cap.Ptr == camera_ind[0]))
                {
                    /*var mat = new Mat();
                    cap.Retrieve(mat);
                    imProcess(mat, 1);*/
                    
                    cap.Retrieve(mat_global[0]);
                    
                   // CvInvoke.Imshow("im1", mat_global[0]);
                    camera_frame_time.Add(DateTime.Now.Ticks / 10000);
                    int fps_c = 100;
                    if(camera_frame_time.Count> fps_c)
                    {
                        camera_frame_time.RemoveAt(0);
                    }
                    if(camera_frame_time.Count== fps_c)
                    {
                        var dt = (int)(camera_frame_time[camera_frame_time.Count - 1] - camera_frame_time[0]);
                        fps1=Math.Round((double)fps_c*1000 / dt,1);
                    }
                    
                    imageBox1.Image = mat_global[0];  
                    
                    imProcess(mat_global[0],1);
                   

                }
                else if ((camera_ind.Count > 1) && (cap.Ptr == camera_ind[1]))
                {
                    /*var mat = new Mat();
                    cap.Retrieve(mat);
                    imProcess(mat, 2);*/
                    cap.Retrieve(mat_global[1]);                                      
                    imageBox2.Image = mat_global[1];

                    imProcess(mat_global[1],2);
                    
                    //imBox_base.Image = stereoProc(mat_global[0], mat_global[1]);
                }
            }
        }
         
        void imProcess(Mat mat,int ind)
        {

            switch (imProcType)
            {

                case FrameType.Test:
                    //laserLine?.offLaserSensor();
                    //imb_base[ind-1].Image = mat;
                    break;
                case FrameType.MarkBoard:
                    imb_base[ind - 1].Image = UtilOpenCV.drawChessboard(mat, chess_size, false,false,CalibCbType.FastCheck);
                    break;
                case FrameType.Undist:
                    imb_base[ind - 1].Image = stereocam.remapCam(mat, ind);
                    break;

                case FrameType.LasLin://laser sensor
                    try
                    {
                        var ps = Detection.detectLineSensor(mat);
                        Console.Write(ps[0].X+ " ");
                        laserLine?.setLaserCur((int)(10 * ps[0].X));
                       // Console.WriteLine((int)(10 * ps[0].X));
                        CvInvoke.Line(mat, new Point(350, 0), new Point(350, mat.Width - 1), new MCvScalar(0, 255, 0));
                        imb_base[ind - 1].Image = UtilOpenCV.drawPointsF(mat, ps, 255, 0, 0, 1);
                        //Console.Write(laserLine?.reseav());
                        
                        //imb_base[ind - 1].Image = Detection.detectLineSensor(mat);
                    }
                     catch
                    {

                    }
                    //imb_base[ind - 1].Image = Detection.detectLineSensor(mat);
                    break;
                case FrameType.Pattern:
                    System.Drawing.PointF[] points = null;
                    imb_base[ind - 1].Image = FindCircles.findCircles(mat,ref points, chess_size);
                    break;
                default:
                    break;
            }
            if (videoframe_counts[ind - 1] == 0)
            {
                //initWrite(ind,cameraSize.Width,cameraSize.Height);
                video_mats[ind - 1] = new List<Mat>();
                videoframe_counts[ind - 1]++;
            }

            if (videoframe_counts[ind - 1] > 0 && videoframe_counts[ind - 1] < videoframe_counts_stop[ind - 1])
            {
                // sb_enc?.Append(laserLine?.get_las_pos() + " " + videoframe_counts[ind - 1] + " " + ind + " ");
               // if (sb_enc == null) Console.WriteLine("NULL!");
                sb_enc?.Append("0" + " " + videoframe_counts[ind - 1] + " " + ind + " ");
                sb_enc?.Append(DateTime.Now.Ticks + " " + videoframe_counts[ind - 1] + " " + ind + " ");
                sb_enc?.Append("\n");
                //video_writer[ind - 1]?.Write(mat);
                video_mats[ind-1].Add(mat.Clone());
                //var p = Detection.detectLineSensor(mat)[0];
                //Console.WriteLine(ind + " " + video_mats[ind-1].Count+" "+p);
               // sb_enc?.Append(laserLine?.get_las_pos() + " " + videoframe_counts[ind - 1] + " " + ind + " ");
                sb_enc?.Append("0" + " " + videoframe_counts[ind - 1] + " " + ind + " ");
                sb_enc?.Append(DateTime.Now.Ticks + " " + videoframe_counts[ind - 1] + " " + ind + " ");
                sb_enc?.Append("\n");

                videoframe_counts[ind - 1]++;
            }
            else
            {
                if (video_mats[ind-1]!=null) save_video( ind, cameraSize.Width, cameraSize.Height);
                if (sb_enc!=null)
                {
                    laserLine?.laserOff();
                    
                    string path = "cam1" +  "\\" + box_scanFolder.Text + "\\enc.txt";
                    box_scanFolder.BeginInvoke((MethodInvoker)(() => box_scanFolder.Text = scan_fold_name));
                    textB_scan_path.BeginInvoke((MethodInvoker)(() => textB_scan_path.Text = scan_fold_path));
                    using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
                    {
                        if(sb_enc!=null)
                        {
                            sw.Write(sb_enc.ToString());
                            sb_enc = null;
                        }
                       
                    }
                   
                }


               // video_writer[ind - 1]?.Dispose();
            }

        }
        void imProcess_virt(Mat mat, int ind)
        {

            if (videoframe_counts[ind - 1] == 0)
            {
                initWrite(ind, GL1.transRotZooms[ind-1].rect.Width, GL1.transRotZooms[ind - 1].rect.Height);
                videoframe_counts[ind - 1]++;
            }

            if (videoframe_counts[ind - 1] > 0 && videoframe_counts[ind - 1] < videoframe_counts_stop[ind - 1])
            {
                //Console.WriteLine("mat" + " " + mat.Width + " " + mat.Height);
                video_writer[ind - 1]?.Write(mat);
                videoframe_counts[ind - 1]++;
                //Console.WriteLine(ind+" "+videoframe_counts[ind - 1]);
            }
            else
            {
                video_writer[ind - 1]?.Dispose();
            }
        }

        void initWrite(int ind,int w, int h)
        {
            int fcc = VideoWriter.Fourcc('m', 'p', '4', 'v'); //'M', 'J', 'P', 'G';'m', 'p', '4', 'v';'M', 'P', '4', 'V';'H', '2', '6', '4'
            
            int fps = 30;
            Directory.CreateDirectory("cam" + ind.ToString() + "\\" + box_scanFolder.Text);
            string name ="cam"+ind.ToString()+"\\"+ box_scanFolder.Text + "\\"+video_scan_name+".mp4";
            Console.WriteLine("wr" + " " + w + " " + h);
            video_writer[ind - 1] = new VideoWriter(name, fcc, fps, new Size(w, h), true);
            //var reswr = video_writer[ind - 1].Set(VideoWriter.WriterProperty.Quality, 1);
            //Console.WriteLine(reswr);
        }
        void save_video(int ind, int w, int h)
        {
            int fcc = VideoWriter.Fourcc('h', '2', '6', '4'); //'M', 'J', 'P', 'G';'m', 'p', '4', 'v';'M', 'P', '4', 'V';'H', '2', '6', '4'
            
            int fps = 30;
            Directory.CreateDirectory("cam" + ind.ToString() + "\\" + box_scanFolder.Text);
            string name = "cam" + ind.ToString() + "\\" + box_scanFolder.Text + "\\" + video_scan_name + ".mp4";
            Console.WriteLine("wr" + " " + w + " " + h);
            video_writer[ind - 1] = new VideoWriter(name, -1, fps, new Size(w, h), true);
            var reswr = video_writer[ind - 1].Set(VideoWriter.WriterProperty.Quality, 100);
            Console.WriteLine(reswr);
            for (int i=0;i< video_mats[ind-1].Count;i++)
            {
                video_writer[ind - 1].Write(video_mats[ind - 1][i]);
                //var p = Detection.detectLineSensor(video_mats[ind - 1][i])[0];
                //Console.WriteLine(ind + " "  + p);
            }
            video_mats[ind - 1] = null;
            video_writer[ind - 1].Dispose();

        }
        void startWrite(int ind,int count)
        {
            videoframe_counts_stop[ind - 1] = count;
            videoframe_counts[ind - 1] = 0;
        }

        Mat stereoProc(Mat mat1, Mat mat2)
        {

            return stereocam.epipolarTest(mat1, mat2);
            //return features.drawDescriptorsMatch(ref mat1, ref mat2);
            
            /*//imBox_input_1.Image = mat1;
            //imBox_input_2.Image = mat2;
            imBox_input_1.Image = UtilOpenCV.drawChessboard(mat1, new Size(6, 7));
            imBox_input_2.Image = UtilOpenCV.drawChessboard(mat2, new Size(6, 7));
            return null;*/
        }
        private void videoStart_Click(object sender, EventArgs e)
        {

            var contr = (TextBox)sender;
            videoStart(Convert.ToInt32(contr.Text));
        }
        private void videoStart(int number)
        {
            var capture = new VideoCapture(number);
           
            //capture.SetCaptureProperty(CapProp.
            capture.SetCaptureProperty(CapProp.FrameWidth, cameraSize.Width);

            // capture.SetCaptureProperty(CapProp.FrameHeight, cameraSize.Height);
            //capture.SetCaptureProperty(CapProp.Fps, 30);
            Console.WriteLine(capture.GetCaptureProperty(CapProp.FrameWidth) + " " + capture.GetCaptureProperty(CapProp.FrameHeight)+" "+ capture.GetCaptureProperty(CapProp.Fps));

            //capture.SetCaptureProperty(CapProp.Contrast, 30);
            camera_ind.Add(capture.Ptr);
            capture.ImageGrabbed += capturingVideo;
            capture.Start();
        }


        #endregion

        #region Mesh
        public void scanning(List<float[]> list_im)
        {
            int count = 0;
            Console.WriteLine("asda " + list_im.Count);
            int lenght = list_im[0].Length * scanning_len;
            if (vertex_buffer_data.Length != 6 * 3 * lenght)
            {
                vertex_buffer_data = new float[6 * 3 * lenght];
            }
            if (normal_buffer_data.Length != 6 * 3 * lenght)
            {
                normal_buffer_data = new float[6 * 3 * lenght];
            }
            if (color_buffer_data.Length != 6 * 3 * lenght)
            {
                color_buffer_data = new float[6 * 3 * lenght];
            }
            for (int x = 0; x < scanning_len - 1; x++)
            {
                for (int y = 0; y < list_im[0].Length - 1; y++)
                {
                    vertex_buffer_data[count] = 5 * x; count++;
                    vertex_buffer_data[count] = y; count++;
                    if (list_im[x][y] > 254)
                    {
                        vertex_buffer_data[count] = 0; count++;
                    }
                    else
                    {
                        vertex_buffer_data[count] = list_im[x][y]; count++;
                    }

                    /*Console.WriteLine(scanning_len + " x: " + x);
                    Console.WriteLine(list_im[0].Length + " y: " + y);
                    Console.WriteLine(count + " count: " + count);*/
                }

            }
            for (int i = 0; i < color_buffer_data.Length; i++)
            {
                color_buffer_data[i] = 0.1f;
            }
            for (int i = 0; i < normal_buffer_data.Length; i++)
            {
                normal_buffer_data[i] = 0.1f;
            }
            GL1.add_buff_gl(vertex_buffer_data, color_buffer_data, normal_buffer_data, PrimitiveType.Points);
        }
        static public void send_buffer_img(Image<Gray, Byte> im2, PrimitiveType type,GraphicGL graphicGL)
        {

            int lenght = im2.Width * im2.Height;
            var vertex_buffer_data = new float[6 * 3 * lenght];
            var normal_buffer_data = new float[6 * 3 * lenght];
            var color_buffer_data = new float[6 * 3 * lenght];
            if (vertex_buffer_data.Length != 6 * 3 * lenght)
            {
                vertex_buffer_data = new float[6 * 3 * lenght];
            }
            if (normal_buffer_data.Length != 6 * 3 * lenght)
            {
                normal_buffer_data = new float[6 * 3 * lenght];
            }
            if (color_buffer_data.Length != 6 * 3 * lenght)
            {
                color_buffer_data = new float[6 * 3 * lenght];
            }
            int i = 0;
            Console.WriteLine(vertex_buffer_data.Length);
            Console.WriteLine("-----------------------------------");
            var z_mult_cam = 5;
            for (int x = 0; x < im2.Width - 1; x++)
            {
                for (int y = 0; y < im2.Height - 1; y++)
                {
                    vertex_buffer_data[i] = x - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y, x, 0] - z_mult_cam / 2; i++;

                    vertex_buffer_data[i] = x + 1 - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y, x + 1, 0] - z_mult_cam / 2; i++;

                    vertex_buffer_data[i] = x - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y + 1 - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y + 1, x, 0] - z_mult_cam / 2; i++;

                    //-------------------------------------------------------------

                    vertex_buffer_data[i] = x - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y + 1 - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y + 1, x, 0] - z_mult_cam / 2; i++;

                    vertex_buffer_data[i] = x + 1 - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y, x + 1, 0] - z_mult_cam / 2; i++;

                    vertex_buffer_data[i] = x + 1 - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y + 1 - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y + 1, x + 1, 0] - z_mult_cam / 2; i++;
                }
            }
            graphicGL.addMesh(vertex_buffer_data, type);
        }
        private float[] meshFromImage(Image<Gray, float> im2, float dx,float dy, float z_mult_cam, float x_cut = 0, float y_cut = 0, float offx=0, float offy=0, float offz=0)
        {
            int lenght = im2.Width * im2.Height; 
            if (vertex_buffer_data.Length != 6 * 3 * lenght)
            {
                vertex_buffer_data = new float[6 * 3 * lenght];
            }
            int i = 0;
            Console.WriteLine(vertex_buffer_data.Length);
            Console.WriteLine("-----------------------------------");
            for (int x = (int)(x_cut * im2.Width); x < im2.Width - 1-(int)(x_cut * im2.Width); x++)
            {
                for (int y =(int)( y_cut* im2.Height); y < im2.Height - 1 - (int)(y_cut * im2.Height); y++)
                {
                    vertex_buffer_data[i] = dx * x  ; i++;
                    vertex_buffer_data[i] = dy * y ; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y, x, 0]; i++;

                    vertex_buffer_data[i] = dx * (x + 1) ; i++;
                    vertex_buffer_data[i] = dy * y; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y, x + 1, 0]; i++;

                    vertex_buffer_data[i] = dx * x ; i++;
                    vertex_buffer_data[i] = dy * (y + 1); i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y + 1, x, 0]; i++;

                    //-------------------------------------------------------------

                    vertex_buffer_data[i] = dx * x; i++;
                    vertex_buffer_data[i] = dy * (y + 1); i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y + 1, x, 0]; i++;

                    vertex_buffer_data[i] = dx * (x + 1); i++;
                    vertex_buffer_data[i] = dy * y; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y, x + 1, 0]; i++;

                    vertex_buffer_data[i] = dx * (x + 1); i++;
                    vertex_buffer_data[i] = dy * (y + 1) ; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y + 1, x + 1, 0]; i++;
                }
            }
            //GL1.translateMesh(vertex_buffer_data,)
            return vertex_buffer_data;
        }
        private float[] meshFromImage(Image<Gray, Byte> im2, float z_mult_cam)
        {
            int lenght = im2.Width * im2.Height;
            if (vertex_buffer_data.Length != 6 * 3 * lenght)
            {
                vertex_buffer_data = new float[6 * 3 * lenght];
            }
            int i = 0;
            Console.WriteLine(vertex_buffer_data.Length);
            Console.WriteLine("-----------------------------------");
            for (int x = 0; x < im2.Width - 1; x++)
            {
                for (int y = 0; y < im2.Height - 1; y++)
                {
                    vertex_buffer_data[i] = x - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y, x, 0] - z_mult_cam / 2; i++;

                    vertex_buffer_data[i] = x + 1 - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y, x + 1, 0] - z_mult_cam / 2; i++;

                    vertex_buffer_data[i] = x - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y + 1 - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y + 1, x, 0] - z_mult_cam / 2; i++;

                    //-------------------------------------------------------------

                    vertex_buffer_data[i] = x - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y + 1 - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y + 1, x, 0] - z_mult_cam / 2; i++;

                    vertex_buffer_data[i] = x + 1 - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y, x + 1, 0] - z_mult_cam / 2; i++;

                    vertex_buffer_data[i] = x + 1 - im2.Width / 2; i++;
                    vertex_buffer_data[i] = y + 1 - im2.Height / 2; i++;
                    vertex_buffer_data[i] = z_mult_cam * im2.Data[y + 1, x + 1, 0] - z_mult_cam / 2; i++;
                }
            }
            return vertex_buffer_data;
        }
        Point3d_GL[][] orderPoints(Point3d_GL[][] ps)
        {
            var ps_or = from p in ps
                        orderby p[0].x
                        select p;
            return ps_or.ToArray();
        }
        Point3d_GL[][] filterPoints(Point3d_GL[][] ps)
        {
            var ps_fil = new List<Point3d_GL[]>();
            for (int i=0; i<ps.Length;i++)
                if (ps[i]!=null)
                    if (ps[i].Length>0)
                        ps_fil.Add(ps[i]);
                
                
            
            return ps_fil.ToArray();
        }
        float[] meshFromPoints(Point3d_GL[][] points3d_in)
        {
            var points3d = orderPoints(filterPoints(points3d_in));


            var mesh = new List<float>();
            for (int i = 0; i < points3d.Length - 1; i++)
            {
                //Console.WriteLine(frames[0].points.Length - 2);
                for (int j = 0; j < points3d[0].Length - 1; j++)
                {
                    //Console.WriteLine(i + " " + j);
                    var p1 = points3d[i][j];
                    var p2 = points3d[i + 1][j];
                    var p3 = points3d[i][j + 1];
                    if (p1.exist & p2.exist & p3.exist)
                    {
                        mesh.Add((float)p1.x); mesh.Add((float)p1.y); mesh.Add((float)p1.z);
                        mesh.Add((float)p2.x); mesh.Add((float)p2.y); mesh.Add((float)p2.z);
                        mesh.Add((float)p3.x); mesh.Add((float)p3.y); mesh.Add((float)p3.z);
                    }

                    p1 = points3d[i + 1][j];
                    p2 = points3d[i + 1][j + 1];
                    p3 = points3d[i][j + 1];
                    if (p1.exist & p2.exist & p3.exist)
                    {
                        mesh.Add((float)p1.x); mesh.Add((float)p1.y); mesh.Add((float)p1.z);
                        mesh.Add((float)p2.x); mesh.Add((float)p2.y); mesh.Add((float)p2.z);
                        mesh.Add((float)p3.x); mesh.Add((float)p3.y); mesh.Add((float)p3.z);
                    }

                }
            }
            return mesh.ToArray();
        }
        void generateImage3D(int n, float k, float side)
        {
           
            float q_side = (k * side);
            Console.WriteLine("q_side " + q_side);

            float[] square_buf = {
                            0.0f,0.0f,0.0f, // triangle 1 : begin
                            0.0f,q_side, 0.0f,
                           q_side,q_side, 0.0f, // triangle 1 : end
                            q_side, q_side,0.0f, // triangle 2 : begin
                           q_side,0.0f,0.0f,
                            0.0f, 0.0f,0.0f // triangle 2 : end
            };

            for (float x = -side / 4; x >= -n*side; x -= side)
            {
                for (float y = -side / 4; y >= -n * side; y -= side)
                {
                    Console.WriteLine(x+" "+y);
                    GL1.addGLMesh(square_buf, PrimitiveType.Triangles, (float)x, (float)y);
                }
            }

        }

        void generateImage3D_BOARD(int n, int k, float sidef,PatternType patternType = PatternType.Chess)
        {
            float side = sidef * 2;
            if (patternType == PatternType.Chess)
            {
                n++;k++;
            }

            float w = sidef * (float)n;
            float h = sidef * (float)k;
            float offx = -sidef;
            float offy =  -sidef;
            float z = 0f;
            float[] pattern_mesh = {
                            0.0f,0.0f,0.0f, // triangle 1 : begin
                            0.0f,sidef, 0.0f,
                           sidef,sidef, 0.0f, // triangle 1 : end
                            sidef, sidef,0.0f, // triangle 2 : begin
                           sidef,0.0f,0.0f,
                            0.0f, 0.0f,0.0f};

            if(patternType == PatternType.Mesh)
            {
                pattern_mesh = circle_mesh(sidef / 4, 50);
                side = sidef;
                w += side;
                h += side;
            }

            if (patternType == PatternType.Chess)
            {
                for (float x = 0; x < w; x += side)
                {
                    for (float y = 0; y < h; y += side)
                    {
                        GL1.addGLMesh(pattern_mesh, PrimitiveType.Triangles, x + offx, y + offy, z);
                    }
                }
            }

            for (float x = sidef; x < w; x += side)
            {
                for (float y = sidef; y < h; y += side)
                {
                    GL1.addGLMesh(pattern_mesh, PrimitiveType.Triangles, x + offx, y + offy, z);
                }
            }
            
            

        }

        float[] circle_mesh(float rad,int count)
        {
            var mesh = new List<float>();
            var angle = 2*Math.PI / count;
            var cur_angle = 0d;
            for(int i=0; i < count; i++)
            {
                var p1 = new float[] { (float)(rad * Math.Cos(cur_angle)), (float)(rad * Math.Sin(cur_angle)), 0 };              
                cur_angle += angle;
                var p2 = new float[] { (float)(rad * Math.Cos(cur_angle)), (float)(rad * Math.Sin(cur_angle)), 0 };
                var p3 = new float[] { 0, 0, 0 };
                mesh.AddRange(p1);
                mesh.AddRange(p2);
                mesh.AddRange(p3);


            }
            return mesh.ToArray();
        }
        

        Image<Gray, float> MeshToIm(float[] mesh, int cols = 41, int rows = 91)
        {
            var im_grad = new Image<Gray, float>(cols, rows);
            var im_grad_data = new float[cols, rows, 1];
            for (int i = 0; i < cols - 1; i++)
            {
                for (int j = 0; j < rows - 1; j++)
                {
                    im_grad_data[i, j, 0] = mesh[3 * (i * rows + j) + 2];
                }
            }
            return new Image<Gray, float>(im_grad_data);
        }
        float[] coloring_mesh(float[] mesh)
        {
            var zmin = float.MaxValue;
            var zmax = float.MinValue;
            var color = new float[mesh.Length];
            for (int i = 0; i < mesh.Length; i += 3)
            {
                var z = mesh[i + 2];
                if (z > zmax)
                {
                    zmax = z;
                }
                if (z < zmin)
                {
                    zmin = z;
                }
            }
            var zs = (zmax - zmin) / 2;
            Console.WriteLine("zmin = " + zmin + "zmax = " + zmax);
            for (int i = 0; i < color.Length; i += 3)
            {
                var z = mesh[i + 2];
                float r = 0.0f;
                float g = 0.0f;
                float b = 0.0f;
                if (z < zs)
                {
                    b = 1f - (z - zmin) / (zs - zmin);
                    // Console.WriteLine("b = " + b);
                    g = 1f - b;
                }
                else
                {
                    r = (z - zs) / (zmax - zs);
                    g = 1f - r;
                }
                color[i] = r / 1;
                color[i + 1] = g / 1;
                color[i + 2] = b / 1;

            }
            return color;
        }
        #endregion

        #region video_leg
        public void binImage(ImageBox box, double bin, double points)
        {
            Mat im = new Mat();
            if (writ == true)
            {
                writer.Write(im);
                videoframe_count++;
                if (videoframe_count > 30)
                {
                    writ = false;
                }
            }
            imageBox2.Image = im;

            var frame_f = (Frame)comboImages.Items[fr_ind];
            var im_f = frame_f.im;
            if (frame_f.points != null)
            {
                //drawTours(im_f, frame_f.points, 255, 0, 255, 8);
            }

            box.Image = im_f;
            if (flag2 == 0)
            {
                var im1 = new Mat(im, new Rectangle(0, 0, im.Width, im.Height));
                Ims.Add(im1);
                if (Ims.Count > 3)
                {
                    var ps2 = FindMark.finPointFs(Ims, imageBox3, res_min);
                    flag2 = 1;
                }
            }
            im.Dispose();
        }
        public Mat binVideo(Mat im, ImageBox box, int bin)
        {
            var mat_im = new Mat();
            im.CopyTo(mat_im);

            var stroka = ContourAnalyse.findContour(mat_im, box, bin);
            var mat_out = Regression.paintRegression(mat_im, stroka,2);
            var points = Regression.regressionPoints(mat_im.Size, stroka);
            //drawTours(mat_out, points, 0, 255, 255);
            imageBox2.Image = im;
            return mat_out;
        }
        public void binVideo_Real(Mat im, ImageBox box, int bin)
        {
            var mat_im = new Mat();
            im.CopyTo(mat_im);
            var stroka = ContourAnalyse.findContourZ(mat_im, box, bin);
        }
        public List<VideoFrame> loadVideos(string path)
        {
            var files = Directory.GetFiles(path);
            List<VideoFrame> frames = new List<VideoFrame>();
            foreach (string file in files)
            {
                var frame = loadVideo(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                foreach (var fr in frames)
                {
                    comboVideo.Items.Add(fr);
                }
                return frames;
            }
            return null;
        }
        public VideoFrame loadVideo(string filepath)
        {
            videoframe_count = 0 ;
            Console.WriteLine(filepath + "   " );
            cap_mats = new List<Mat>();
            var orig_path = Directory.GetFiles( filepath + "\\orig")[0];
            var capture = new VideoCapture(Directory.GetFiles(filepath)[0]);
            var all_frames = capture.GetCaptureProperty(CapProp.FrameCount);
            capture.ImageGrabbed += loadingVideo;
            capture.Start();
           

            while (videoframe_count < all_frames)
            {
               
            }
            capture.ImageGrabbed -= loadingVideo;
            capture.Stop();
            string name = Path.GetFileName(filepath);

            return new VideoFrame(cap_mats.ToArray(), new Mat(orig_path), name);
        }

       
        Mat takeImage(VideoCapture capture)
        {
            Mat im = new Mat();
            capture.Retrieve(im);
            return im;
        }

        void loadingVideo(object sender, EventArgs e)
        {
            //Console.WriteLine(filepath + "   " + videoframe_count);
            Mat im = new Mat();
            var cap = (VideoCapture)sender;
            cap.Retrieve(im);
            var mat_c = new Mat();
            im.CopyTo(mat_c);
            cap_mats.Add(mat_c);
            videoframe_count++;
            Console.WriteLine("vframe: " + videoframe_count + "/" + cap.GetCaptureProperty(CapProp.FrameCount));
        }

        void streaming(object sender, EventArgs e)
        {
            binImage(imageBox1, red_c, green_c);
            while (im_i < scanning_len)
            {
                /* var res = binImage(imageBox1, red_c, green_c);
                 if (res != null)
                 {
                     im.Add(res);
                     im_i++;
                     Console.WriteLine(im_i);
                 }*/
            }
            if (im_i == scanning_len && flag1 == 0)
            {
                scanning(im);
                im = new List<float[]>();
                flag1 = 1;
            }

        }
        Image<Gray, Byte> toFlat(Image<Gray, Byte> im_res)
        {
            int prev_res = 0;
            int loc_res = 0;
            var im_ret = new Image<Gray, Byte>(im_res.Width, im_res.Height);
            for (int y = 1; y < im_res.Height - 1; y++)
            {
                for (int x = 1; x < im_res.Width - 1; x++)
                {

                    int res = (im_res.Data[y + 1, x - 1, 0] + im_res.Data[y + 1, x, 0] + im_res.Data[y + 1, x + 1, 0] +
                              im_res.Data[y, x - 1, 0] + im_res.Data[y, x, 0] + im_res.Data[y, x + 1, 0] +
                              im_res.Data[y - 1, x - 1, 0] + im_res.Data[y - 1, x, 0] + im_res.Data[y - 1, x + 1, 0]) / 9;
                    if (Math.Abs(prev_res - res) > 10)
                    {
                        loc_res = res;
                        im_ret.Data[y, x, 0] = (byte)res;
                        prev_res = res;

                    }
                    else
                    {
                        im_ret.Data[y, x, 0] = (byte)loc_res;
                        prev_res = res;
                    }

                }
            }

            return im_ret;
        }

        Mat findMostWhite(Mat[] mats)
        {
            var sort_mats = from mat in mats
                            orderby calcWhiteIm(mat) descending
                            select mat;
            return sort_mats.ToArray()[0];
        }
        int calcWhiteIm(Mat mat)
        {
            int ret = 0;
            var im = mat.ToImage<Gray, Byte>();
            for (int x = 0; x < mat.Width; x++)
            {
                for (int y = 0; y < mat.Height; y++)
                {
                    if (im.Data[y, x, 0] > 252)
                    {
                        ret++;
                    }
                }
            }
            return ret;
        }
        

        Frame findZeroFrame(List<VideoFrame> videoframes)
        {
            var fr = from f in videoframes
                     orderby f.pos_rob.z
                     select f;
            var vfrs = fr.ToList();
            return vfrs[vfrs.Count - 1].toFrame();
        }





        #endregion

        #region laser_but
        string portArd;

        void find_ports()
        {
            //comboBox_portsArd.Items.Add("COM3");
            comboBox_portsArd.Items.Clear();

            // Получаем список COM портов доступных в системе
            string[] portnames = SerialPort.GetPortNames();
            // Проверяем есть ли доступные
            if (portnames.Length == 0)
            {
                MessageBox.Show("COM PORT not found");
            }
            foreach (string portName in portnames)
            {
                //добавляем доступные COM порты в список           
                comboBox_portsArd.Items.Add(portName);
                //Console.WriteLine(portnames.Length);
                if (portnames[0] != null)
                {
                    comboBox_portsArd.SelectedItem = portnames[0];
                }
            }
        }
        private void but_find_ports_Click(object sender, EventArgs e)
        {
            find_ports();
        }
        private void but_close_Click(object sender, EventArgs e)
        {
            laserLine?.connectStop();
        }

        private void but_open_Click(object sender, EventArgs e)
        {
            laserLine = new LaserLine(portArd);
        }

        private void but_laserOn_Click(object sender, EventArgs e)
        {
            laserLine?.laserOn();
        }

        private void but_laserOff_Click(object sender, EventArgs e)
        {
            laserLine?.laserOff();
        }

        private void but_setPower_Click(object sender, EventArgs e)
        {
            laserLine?.setPower(Convert.ToInt32(textBox_powerLaser.Text));
        }

        private void but_laser_dest_Click(object sender, EventArgs e)
        {
            laserLine?.setLaserDest(Convert.ToInt32(textBox_laser_dest.Text));
        }

        private void but_set_kpp_Click(object sender, EventArgs e)
        {
            laserLine?.setK_p_p(Convert.ToInt32(textBox_set_kpp.Text));
        }

        private void butset_kvp_Click(object sender, EventArgs e)
        {
            laserLine?.setK_v_p(Convert.ToInt32(textBox_set_kvp.Text));
        }
        private void but_las_enc_Click(object sender, EventArgs e)
        {
            textBox_shvpPos.Text = laserLine?.get_las_pos().ToString();
        }
        private void comboBox_portsArd_SelectedIndexChanged(object sender, EventArgs e)
        {
            portArd = (string)((ComboBox)sender).SelectedItem;
        }
        #endregion

        #region scan_but
        private void but_setShvpVel_Click(object sender, EventArgs e)
        {
            laserLine?.setShvpVel(Convert.ToInt32(textBox_shvpVel.Text));
        }
        private void but_setShvpPos_Click(object sender, EventArgs e)
        {
            laserLine?.setShvpPos(Convert.ToInt32(textBox_shvpPos.Text));
        }

        private void combo_improc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(combo_improc.SelectedIndex == 0)
            {
                imProcType = FrameType.MarkBoard;
            }
            else if(combo_improc.SelectedIndex == 1)
            {
                imProcType = FrameType.Undist;
            }
            else if (combo_improc.SelectedIndex == 2)
            {
                imProcType = FrameType.Pattern;
            }
            else if (combo_improc.SelectedIndex == 3)
            {
                imProcType = FrameType.LasLin;
                laserLine?.onLaserSensor();
            }
            else
            {
                imProcType = FrameType.Test;
            }
        }

        private void but_scan_start_laser_Click(object sender, EventArgs e)
        {
            var but = (Button)sender;
            var tpScan = Convert.ToInt32( but.AccessibleName);
            startScanLaser(tpScan);
        }

        private void but_marl_setShvpPos_Click(object sender, EventArgs e)
        {
            linearPlatf?.setShvpPos(Convert.ToDouble(textBox_marl_shcpPos.Text));
        }

        private void but_marl_open_Click(object sender, EventArgs e)
        {
            linearPlatf = new DeviceMarlin(portArd);
        }

        private void but_marl_close_Click(object sender, EventArgs e)
        {
            linearPlatf?.connectStop();
        }

        private void but_marl_receav_Click(object sender, EventArgs e)
        {
            linearPlatf?.reseav();
        }

        private void but_scan_marl_Click(object sender, EventArgs e)
        {
            var but = (Button)sender;
            var tpScan = Convert.ToInt32(but.AccessibleName);
            startScanLaser(tpScan);
        }

        private void but_scan_stereolas_Click(object sender, EventArgs e)
        {
            var pos_rob = positionFromRobot(con1);
            if(pos_rob != null)
            {
                video_scan_name = pos_rob.ToString();
            }
            else
            {
                video_scan_name = "1";
            }
            startScanLaser(3);
        }
        private void but_scan_sing_las_Click(object sender, EventArgs e)
        {
            startScanLaser(4);
        }

        void show_frame(string filepath)
        {
            var capture1 = new VideoCapture(Directory.GetFiles("cam1\\" + filepath)[0]);
            var capture2 = new VideoCapture(Directory.GetFiles("cam2\\" + filepath)[0]);
            //capture1.SetCaptureProperty(CapProp.);
        }
        public Scanner loadVideo_stereo_not_sync(string filepath, Scanner scanner = null, int strip = 1)
        {

            videoframe_count = 0;
            var orig1 = new Mat(Directory.GetFiles("cam1\\" + filepath + "\\orig")[0]);
            var orig2 = new Mat(Directory.GetFiles("cam2\\" + filepath + "\\orig")[0]);
            Console.WriteLine(Directory.GetFiles("cam1\\" + filepath)[0]);
            Console.WriteLine(Directory.GetFiles("cam2\\" + filepath)[0]);

            var ve_paths1 = get_video_path(1,filepath);
            string video_path1 = ve_paths1[0];
           // string enc_path1 = ve_paths1[1];

            var ve_paths2 = get_video_path(2, filepath);
            string video_path2 = ve_paths2[0];
            // string enc_path2 = ve_paths2[1];

            scanner.set_coord_sys(StereoCamera.mode.model);
            var name_v1 = Path.GetFileNameWithoutExtension(video_path1);
            var name_v2 = Path.GetFileNameWithoutExtension(video_path2);
            if (name_v1.Length > 1 && name_v2.Length > 1)
            {
                scanner.set_rob_pos(name_v1);
                scanner.set_coord_sys(StereoCamera.mode.world);
            }
                


            var capture1 = new VideoCapture(video_path1);
            var capture2 = new VideoCapture(video_path2);
            var all_frames1 = capture1.GetCaptureProperty(CapProp.FrameCount);
            var all_frames2 = capture2.GetCaptureProperty(CapProp.FrameCount);
            var fr_st_vid = new Frame(orig1, orig2, "sd", FrameType.Test);
            var frames_show = new List<Frame>();
            fr_st_vid.stereo = true;
            //comboImages.Items.Add(fr_st_vid);

            
            int buff_diff = 9;
            int buff_len = buff_diff+1;
            var all_frames = Math.Min(all_frames1, all_frames2);
            if (scanner != null)
            {
                var orig2_im = orig2.ToImage<Bgr, byte>();
                CvInvoke.Rotate(orig2_im, orig2_im, RotateFlags.Rotate180);
                scanner.pointCloud.color_im = new Image<Bgr, byte>[] { orig1.ToImage<Bgr, byte>(), orig2_im };
                scanner.pointCloud.graphicGL = GL1;
            }
            var im1_buff = new Mat();
            var im2_buff = new Mat();

            var im1_buff_list = new List<Mat>();
            var im2_buff_list = new List<Mat>();

            //while (videoframe_count < 80)
            while (videoframe_count < all_frames)
            {
                Mat im1 = new Mat();
                Mat im2 = new Mat();
                
                while (!capture1.Read(im1)) { }
                while (!capture2.Read(im2)) { }
               // Console.WriteLine(videoframe_count+"____________________");
                
                if (scanner != null && im1!= null && im2!=null)
                {
                    var buffer_mat1 = im1.Clone();
                    var buffer_mat2 = im2.Clone();
                    if (videoframe_count % strip == 0 && videoframe_count> buff_len)
                    {
                        // im1 -= orig1;
                        // im2 -= orig2;

       
                        /* CvInvoke.Imshow("im1_or", im1);
                         CvInvoke.Imshow("buffer_mat1", im1_buff_list[1]);
                         CvInvoke.Imshow("buffer_mat8", im1_buff_list[8]);
                       */
                        
                        im1 -= im1_buff_list[buff_len - buff_diff];
                        im2 -= im2_buff_list[buff_len - buff_diff];
                       // CvInvoke.Imshow("im1_dif", im1);
                       // CvInvoke.WaitKey();
                        
                        CvInvoke.Rotate(im2, im2, RotateFlags.Rotate180);
                        if (ch_b_im_s.Checked)
                        {
                             var frame_d = new Frame(im1, im2, videoframe_count.ToString(), FrameType.LasDif);
                             frame_d.stereo = true;
                             frames_show.Add(frame_d);
                        }
                            //scanner.addPointsStereoLas(new Mat[] { im1, im2 },false);
                            /*var ps1 = Detection.detectLineDiff(im1, 7);
                            var ps2 = Detection.detectLineDiff(im2, 7);

                            imageBox1.Image = UtilOpenCV.drawPointsF(im1, ps1, 255, 0, 0);
                            imageBox2.Image = UtilOpenCV.drawPointsF(im2, ps2, 255, 0, 0);*/
                            //CvInvoke.Imshow("im2", im2);                       

                            scanner.addPointsStereoLas_2d(new Mat[] { im1, im2 }, ch_b_dist.Checked);
                    }
                    
                    im1_buff = buffer_mat1.Clone();
                    im2_buff = buffer_mat2.Clone();

                    im1_buff_list.Add(im1_buff);
                    im2_buff_list.Add(im2_buff);
                    if(im1_buff_list.Count>buff_len)
                    {
                        im1_buff_list.RemoveAt(0);
                        im2_buff_list.RemoveAt(0);
                    }
                    



                }                
                videoframe_count++;
                //Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames);
            }
            comboImages.Items.AddRange(frames_show.ToArray());
            scanner.compPointsStereoLas_2d();
            Console.WriteLine("Points computed.");
            return scanner;
        }


        static List<Mat>  read_frame(VideoCapture capture, List<Mat> buff,int len)
        {
            Mat im = new Mat();
            while (!capture.Read(im)){}
            buff.Add(im);
            if (buff.Count > len) buff.RemoveAt(0);
            return buff;
        }
        public Scanner loadVideo_stereo(string filepath, Scanner scanner = null, int strip = 1)
        {

            videoframe_count = 0;
            var orig1 = new Mat(Directory.GetFiles("cam1\\" + filepath + "\\orig")[0]);
            var orig2 = new Mat(Directory.GetFiles("cam2\\" + filepath + "\\orig")[0]);
            Console.WriteLine(Directory.GetFiles("cam1\\" + filepath)[0]);
            Console.WriteLine(Directory.GetFiles("cam2\\" + filepath)[0]);

            var ve_paths1 = get_video_path(1, filepath);
            string video_path1 = ve_paths1[0];
            
            // string enc_path1 = ve_paths1[1];

            var ve_paths2 = get_video_path(2, filepath);
            string video_path2 = ve_paths2[0];
            // string enc_path2 = ve_paths2[1];

            string enc_path = ve_paths1[1];
            var pairs = frames_sync_from_file(enc_path,lab_scan_pres);
            var cam_min = (int)pairs[0][0];
            var cam_max = (int)pairs[0][1];
            var frame_min = (int)pairs[0][2];
            var frame_max = (int)pairs[0][3];

            scanner.set_coord_sys(StereoCamera.mode.model);
            var name_v1 = Path.GetFileNameWithoutExtension(video_path1);
            var name_v2 = Path.GetFileNameWithoutExtension(video_path2);
            if (name_v1.Length > 1 && name_v2.Length > 1)
            {
                scanner.set_rob_pos(name_v1);
                scanner.set_coord_sys(StereoCamera.mode.world);
            }

            

            var capture1 = new VideoCapture(video_path1);
            var capture2 = new VideoCapture(video_path2);
            var captures = new VideoCapture[] { capture1, capture2 };
            var all_frames1 = capture1.GetCaptureProperty(CapProp.FrameCount);
            var all_frames2 = capture2.GetCaptureProperty(CapProp.FrameCount);
            var fr_st_vid = new Frame(orig1, orig2, "sd", FrameType.Test);
            var frames_show = new List<Frame>();
            fr_st_vid.stereo = true;
            //comboImages.Items.Add(fr_st_vid);

            int buff_diff = 10;
            int buff_len = buff_diff + 1;
            if(ch_b_dist.Checked)
            {
                orig1 = scanner.stereoCamera.cameraCVs[0].undist(orig1);
                orig2 = scanner.stereoCamera.cameraCVs[1].undist(orig2);
            }
            var all_frames = Math.Min(all_frames1, all_frames2);
            if (scanner != null)
            {
                var orig2_im = orig2.ToImage<Bgr, byte>();
                CvInvoke.Rotate(orig2_im, orig2_im, RotateFlags.Rotate180);
                scanner.pointCloud.color_im = new Image<Bgr, byte>[] { orig1.ToImage<Bgr, byte>(), orig2_im };
                scanner.pointCloud.graphicGL = GL1;
            }
            var im_min_buff_list = new List<Mat>();
            var im_max_buff_list = new List<Mat>();
            int f1 = 0;
            int f2 = 0;
            //f1 = 70;
            //while (f1 < frame_min-1)
            while (f1 < frame_min - 1 )
            //while (f1 < 200)
            {
                im_min_buff_list = read_frame(captures[cam_min-1], im_min_buff_list, buff_len); f1++;
                var f2_ind = (int)pairs[f1][0];
                var k = pairs[f1][1];
                while(f2 < f2_ind)
                //while (f2 != f2_ind)
                {
                    im_max_buff_list = read_frame(captures[cam_max - 1], im_max_buff_list, buff_len); f2++;
                } 
                if (scanner != null)
                {
                    if (f1% strip == 0 && f1 > buff_len)
                    {
                        var im_min = im_min_buff_list[buff_len - 1] - im_min_buff_list[buff_len - buff_diff];

                        var im_max = im_max_buff_list[buff_len - 1] - im_max_buff_list[buff_len - buff_diff];
                        var im_max_prev = im_max_buff_list[buff_len - 1 - 1] - im_max_buff_list[buff_len - buff_diff-1];
                        
                        if(cam_min == 2)
                        {
                            CvInvoke.Rotate(im_min, im_min, RotateFlags.Rotate180);
                        }
                        if (cam_max == 2)
                        {
                            CvInvoke.Rotate(im_max, im_max, RotateFlags.Rotate180);
                            CvInvoke.Rotate(im_max_prev, im_max_prev, RotateFlags.Rotate180);
                        }
                        //Console.WriteLine(f1 + " " + f2);

                        
                        //CvInvoke.Rotate(im2, im2, RotateFlags.Rotate180);
                        if(ch_b_im_s.Checked)
                        {
                            var frame_d = new Frame(im_min.Clone(), im_max.Clone(), videoframe_count.ToString(), FrameType.LasDif);
                            frame_d.stereo = true;
                            frames_show.Add(frame_d);
                        }
                        

                        scanner.addPointsStereoLas_2d_sync(new Mat[] { im_min,  im_max, im_max_prev }, k,cam_min, cam_max, ch_b_dist.Checked);
                    }
                }
                videoframe_count++;
                //Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames);
            }
            comboImages.Items.AddRange(frames_show.ToArray());
            scanner.compPointsStereoLas_2d();
            Console.WriteLine("Points computed.");
            return scanner;
        }
        string[] get_video_path(int ind,string filepath)//[ video, enc]
        {
            var files = Directory.GetFiles("cam"+ind+"\\" + filepath);
            string video_path = "";
            string enc_path = "";
            foreach (var path in files)
            {
                var ext = path.Split('.').Reverse().ToArray();

                if (ext[0] == "txt")
                {
                    enc_path = path;
                }
                if (ext[0] == "mp4")
                {
                    video_path = path;
                }
            }
            return new string[] { video_path, enc_path };
        }
        public Scanner loadVideo_sing_cam(string filepath, Scanner scanner = null, int strip = 1, bool calib = false)
        {
            videoframe_count = 0;
            var orig1 = new Mat(Directory.GetFiles("cam1\\" + filepath + "\\orig")[0]);
            //var mat_or_tr = new Mat();
            //CvInvoke.AdaptiveThreshold(orig1, mat_or_tr,  255, ThresholdType.);

            //CvInvoke.Imshow("thr", mat_or_tr);

            Console.WriteLine(Directory.GetFiles("cam1\\" + filepath)[0]);
            var ve_paths = get_video_path(1, filepath);
            string video_path = ve_paths[0];
            string enc_path = ve_paths[1];

            var capture1 = new VideoCapture(video_path);
            var all_frames1 = capture1.GetCaptureProperty(CapProp.FrameCount);
            var fr_st_vid = new Frame(orig1, "sd", FrameType.Test);
            var frames_show = new List<Frame>();
            var pos_inc_cal = new List<double>();
            comboImages.Items.Add(fr_st_vid);
            int buff_diff = 10;
            int buff_len = buff_diff + 1;
            var all_frames = all_frames1;
            if (scanner != null)
            {
                scanner.pointCloud.color_im = new Image<Bgr, byte>[] { orig1.ToImage<Bgr, byte>() };
                scanner.pointCloud.graphicGL = GL1;
            }
            var enc_file = "";
            using (StreamReader sr = new StreamReader(enc_path))
            {
                enc_file = sr.ReadToEnd();
            }
            var inc_pos = scanner.enc_pos(enc_file, (int)all_frames);
            var buffer_mat = new Mat();
            var im_orig = orig1.ToImage<Bgr, byte>();

            var im1_buff = new Mat();


            var im1_buff_list = new List<Mat>();
            foreach (var pos in inc_pos) Console.WriteLine(pos);

            while (videoframe_count < all_frames * 0.5)
            {
                Mat im1 = new Mat();
                while (!capture1.Read(im1)) { }
                if (scanner != null)
                {
                    var buffer_mat1 = im1.Clone();
                    //if (videoframe_count % strip == 0)
                    if (videoframe_count % strip == 0 && videoframe_count > 3)
                    {
                        var im1_or = im1.Clone();
                        im1 -= im1_buff_list[buff_len - buff_diff];

                        if(videoframe_count>20)         
                        {
                            var im1_or_un = scanner.cameraCV.undist(im1_or);
                            CvInvoke.Imshow("im1", im1_or_un);
                            CvInvoke.Imshow("buffer_mat", scanner.cameraCV.undist(im1_buff_list[buff_len - buff_diff]));
                            var im1_diff_un = scanner.cameraCV.undist(im1);
                            CvInvoke.Imshow("im1 diff", im1_diff_un);

                            var ps = Detection.detectLineDiff(im1_diff_un);
                            UtilOpenCV.drawPointsF(im1_or_un, ps, 0, 255, 0, 2);
                            CvInvoke.Imshow("im1-or_un", im1_or_un);
                            CvInvoke.WaitKey();
                        }
                        var frame_d = new Frame(im1, videoframe_count.ToString(), FrameType.LasDif);
                        frames_show.Add(frame_d);
                        if (calib)
                        {
                            //var frame_d = new Frame(im1, videoframe_count.ToString(), FrameType.LasDif);
                            // frames_show.Add(frame_d);
                            pos_inc_cal.Add(inc_pos[videoframe_count]);

                            scanner.addPointsSingLas_2d(im1, false, calib);
                        }
                        else scanner.addPointsLinLas_step(im1, im_orig, inc_pos[videoframe_count], PatternType.Mesh);

                    }
                    im1_buff = buffer_mat1.Clone();

                    im1_buff_list.Add(im1_buff);
                    if (im1_buff_list.Count > buff_len)
                    {
                        im1_buff_list.RemoveAt(0);
                    }
                }
                videoframe_count++;
                Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames);
            }
            comboImages.Items.AddRange(frames_show.ToArray());


            if (calib) scanner.calibrateLinearStep(Frame.getMats(frames_show.ToArray()), orig1, pos_inc_cal.ToArray(), PatternType.Mesh, GL1);

            //var mats = Frame.getMats(frames_show.ToArray());
            //var corn = Detection.detectLineDiff_corn_calibr(mats);

            //UtilOpenCV.drawPointsF(orig1, corn, 255, 0, 0, 2, true);
            //CvInvoke.Imshow("corn", orig1);
            return scanner;
        }


        public void loadVideo_test_laser(string filepath)
        {

            ImageViewer viewer = new ImageViewer(); //create an image viewer
            viewer.SetBounds(0, 0, 1620, 1080);
            VideoCapture capture = new VideoCapture(filepath); //create a camera captue
            var mat_f = new Mat();
            bool first = true;
            Application.Idle += new EventHandler(delegate (object sender, EventArgs e)
            {  //run this until application closed (close button click on image viewer)
                var mat = capture.QueryFrame();
                if (first) { mat_f = mat.Clone(); first = false; }

                var pf = Detection.detectLineSensor(mat)[0];
                var p = new System.Drawing.Point((int)pf.X, (int)pf.Y);
                Console.WriteLine(pf.X);
                CvInvoke.DrawMarker(mat, p, new MCvScalar(255, 0, 0), MarkerTypes.TiltedCross, 10);
                viewer.Image = mat; //draw the image obtained from camera
                
            });
            viewer.ShowDialog();


            /*string video_path = filepath;
            var capture1 = new VideoCapture(video_path);
            var all_frames1 = capture1.GetCaptureProperty(CapProp.FrameCount);
            Console.WriteLine(all_frames1);
            while (capture1.IsOpened)
            {
                Mat im1 = new Mat();
                var ok = capture1.Read(im1);
                if(ok)  CvInvoke.Imshow("v1", im1);
                
               
               // videoframe_count++;
                //Console.WriteLine("loading...      " + videoframe_count + "/" + all_frames1);
            }*/
        }
        #endregion

        #region graphic_util
        private void but_text_vis_Click(object sender, EventArgs e)
        {
            if (GL1.textureVis == 0)
            {
                GL1.textureVis = 1;
            }
               
            else
            {
                GL1.textureVis = 0;
            }
        }

        private void but_gl_light_Click(object sender, EventArgs e)
        {
            if (GL1.lightVis == 0)
            {
                GL1.lightVis = 1;
            }

            else
            {
                GL1.lightVis = 0;
            }
        }

        private void but_point_type_Click(object sender, EventArgs e)
        {
            if(GL1.buffersGl.objs[scan_i].tp == PrimitiveType.Points)
            {
                GL1.buffersGl.setPrType(scan_i,PrimitiveType.Lines);
            }
            else if (GL1.buffersGl.objs[scan_i].tp == PrimitiveType.Lines)
            {
                GL1.buffersGl.setPrType(scan_i, PrimitiveType.Triangles);
            }
            else if (GL1.buffersGl.objs[scan_i].tp == PrimitiveType.Triangles)
            {
                GL1.buffersGl.setPrType(scan_i, PrimitiveType.Points);
            }

        }

        private void but_end_cont_Click(object sender, EventArgs e)
        {
            debugBox.Text = gen_traj_rob(RobotFrame.RobotType.KUKA);
        }

        string gen_traj_rob(RobotFrame.RobotType robotType)
        {
            var selected_obj = selected_object(); if (selected_obj == null) return "";
            var mesh = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[selected_obj].vertex_buffer_data);
            var cont = GL1.get_contour()?.ToList();
            if (mesh != null && cont != null)
            {
                List<List<Point3d_GL>> conts = new List<List<Point3d_GL>>();
                for (int i = 0; i < param_tr.layers; i++)
                    conts.Add(cont);
                
                var _traj = PathPlanner.Generate_multiLayer3d_mesh(mesh, conts, param_tr);

                rob_traj = PathPlanner.join_traj(_traj);
                var ps = PathPlanner.matr_to_traj(rob_traj);

                if (GL1.buffersGl.objs.Keys.Contains(traj_i)) GL1.buffersGl.removeObj(traj_i);

                //for (int i = 0; i < rob_traj.Count; i++) GL1.addFrame(rob_traj[i],2);

                traj_i = GL1.addLineMeshTraj(ps.ToArray(),new Color3d_GL(0.9f),"gen_traj");
                var traj_rob = PathPlanner.generate_robot_traj(rob_traj,robotType,param_tr);
                return traj_rob;

            }
            return "";
        }


        private void but_dist_same_ps_Click(object sender, EventArgs e)
        {
            var selected_obj = selected_nodes();
            if (selected_obj == null) return;
            if (selected_obj.Length < 2) return;
            var ps1 = Point3d_GL.fromMesh(GL1.buffersGl.objs[selected_obj[0]].vertex_buffer_data);
            var ps2 = Point3d_GL.fromMesh(GL1.buffersGl.objs[selected_obj[1]].vertex_buffer_data);
            if (ps1 == null || ps2 == null) return;
            if (ps1.Length != ps2.Length) return;
            var dist  = Point3d_GL.dist_ps(ps1, ps2);

            prin.t(dist);

        }
        string cut_area(RasterMap.type_out type_cut,string selected_obj,string cut_obj = null)
        {

            var cont = GL1.get_contour();
            if (selected_obj != null && cont != null)
            {
                double resolut = -1;
                var polygs = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[selected_obj].vertex_buffer_data);
                var map_xy = new RasterMap(polygs, resolut, RasterMap.type_map.XY);
                var cut_surf = map_xy.get_polyg_contour_xy(cont, polygs, type_cut);

                //GL1.buffersGl.removeObj(selected_obj);
                if (cut_obj!= null) selected_obj = cut_obj;

                var scan_stl = Polygon3d_GL.toMesh(cut_surf);
                if (scan_stl != null)
                {
                    
                    GL1.add_buff_gl(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Triangles, selected_obj);
                    return selected_obj;
                }
                
            }
            return null;
        }

        private void but_reconstruc_area_Click(object sender, EventArgs e)
        {
            reconstrust_area();
        }
        private void reconstrust_area()
        {

            var selected_obj = selected_object(); if (selected_obj == null) return;//выбор объекта
            //cut_area(RasterMap.type_out.outside, selected_obj);
            //var fi = cross_obj_flats_find_ang_z(GL1.buffersGl.objs_dynamic[scan_i].vertex_buffer_data, 20, 1);
            var cut_obj = selected_obj + "_cut";
            var ret = cut_area(RasterMap.type_out.outside, selected_obj, cut_obj);//вырез отверстия раны
            if (ret == null) { Console.WriteLine("not cut"); return; };
            var polygs = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[cut_obj].vertex_buffer_data);
            var polyg_sm = RasterMap.smooth_mesh(polygs, 1);
            var mesh = Polygon3d_GL.toMesh(polyg_sm);
            var smooth_mesh = cut_obj + "_smooth";
            GL1.add_buff_gl(mesh[0], mesh[1], mesh[2], PrimitiveType.Triangles, smooth_mesh);//сгаживание окрестности


            var fi = 1.5;
            var df = 1;
            var ds = 1;
            var ps_cr = SurfaceReconstraction.cross_obj_flats(GL1.buffersGl.objs[smooth_mesh].vertex_buffer_data, df, ds, GL1, fi);//реконструирование отверстия
            //ps = Polygon3d_GL.smooth_lines_xy(ps, 2);
            var pols = Polygon3d_GL.triangulate_lines_xy(ps_cr);
            var scan_stl = Polygon3d_GL.toMesh(pols);
            var mesh_sm = scan_stl[0];
            GL1.add_buff_gl(mesh_sm, scan_stl[1], scan_stl[2], PrimitiveType.Triangles, selected_obj + "_rec");
            //GL1.buffersGl.removeObj(cut_obj);
            //GL1.buffersGl.removeObj(smooth_mesh);

             var cuts = new List<string>();

             var conts = new List<List<Point3d_GL>>();
             var surfs = new List<Polygon3d_GL[]>();
             for (int i = 0; i < 3; i++)
             {
                 var mesh_sm_tr = GL1.translateMesh(mesh_sm, 0, 0, -1f + (i * -1f));
                 surfs.Add(Polygon3d_GL.polygs_from_mesh(mesh_sm_tr));
                 //var rec = GL1.add_buff_gl(mesh_sm_tr, scan_stl[1], scan_stl[2], PrimitiveType.Triangles, selected_obj + "_cut_"+i);
                 //cuts.Add(rec);
                 var ps_inter = RasterMap.intersec_line_of_two_mesh(mesh_sm_tr, GL1.buffersGl.objs[selected_obj].vertex_buffer_data);
                 if (ps_inter == null) continue;
                 GL1.addLineMeshTraj(ps_inter, new Color3d_GL(1, 0, 0), "intersec_cut_" + i);
                 conts.Add(ps_inter.ToList());
             }

             var _traj = PathPlanner.generate_3d_traj_diff_surf(surfs, conts, param_tr);

             rob_traj = PathPlanner.join_traj(_traj);
             var ps = PathPlanner.matr_to_traj(rob_traj);

             if (GL1.buffersGl.objs.Keys.Contains(traj_i)) GL1.buffersGl.removeObj(traj_i);

             //for (int i = 0; i < rob_traj.Count; i++) GL1.addFrame(rob_traj[i],2);

             traj_i = GL1.addLineMeshTraj(ps.ToArray(), new Color3d_GL(0.9f), "gen_traj");
             var traj_rob = PathPlanner.generate_robot_traj(rob_traj, RobotFrame.RobotType.PULSE, param_tr);
            // return traj_rob;

        }

        private void reconstrust_up_surf()
        {

            var selected_obj = selected_object(); if (selected_obj == null) return;//выбор объекта
            //cut_area(RasterMap.type_out.outside, selected_obj);
            //var fi = cross_obj_flats_find_ang_z(GL1.buffersGl.objs_dynamic[scan_i].vertex_buffer_data, 20, 1);
            var cut_obj = selected_obj + "_cut";
            var ret = cut_area(RasterMap.type_out.outside, selected_obj, cut_obj);//вырез отверстия раны
            if (ret == null) { Console.WriteLine("not cut"); return; };
            var polygs = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[cut_obj].vertex_buffer_data);
            var polyg_sm = RasterMap.smooth_mesh(polygs, 1);
            var mesh = Polygon3d_GL.toMesh(polyg_sm);
            var smooth_mesh = cut_obj + "_smooth";
            GL1.add_buff_gl(mesh[0], mesh[1], mesh[2], PrimitiveType.Triangles, smooth_mesh);//сгаживание окрестности


            var fi = 1.5;
            var df = 1;
            var ds = 1;
            var ps_cr = SurfaceReconstraction.cross_obj_flats(GL1.buffersGl.objs[smooth_mesh].vertex_buffer_data, df, ds, GL1, fi);//реконструирование отверстия
            //ps = Polygon3d_GL.smooth_lines_xy(ps, 2);
            var pols = Polygon3d_GL.triangulate_lines_xy(ps_cr);
            var scan_stl = Polygon3d_GL.toMesh(pols);
            var mesh_sm = scan_stl[0];
            GL1.add_buff_gl(mesh_sm, scan_stl[1], scan_stl[2], PrimitiveType.Triangles, selected_obj + "_rec");

        }
        private void reconstrust_down_surf()
        {

            var selected_obj = selected_object(); if (selected_obj == null) return;//выбор объекта
            //cut_area(RasterMap.type_out.outside, selected_obj);
            //var fi = cross_obj_flats_find_ang_z(GL1.buffersGl.objs_dynamic[scan_i].vertex_buffer_data, 20, 1);
            var cut_obj = selected_obj + "_cut";
            var ret = cut_area(RasterMap.type_out.outside, selected_obj, cut_obj);//вырез отверстия раны
            if (ret == null) { Console.WriteLine("not cut"); return; };
            var polygs = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[cut_obj].vertex_buffer_data);
            var polyg_sm = RasterMap.smooth_mesh(polygs, 1);
            var mesh = Polygon3d_GL.toMesh(polyg_sm);
            var smooth_mesh = cut_obj + "_smooth";
            GL1.add_buff_gl(mesh[0], mesh[1], mesh[2], PrimitiveType.Triangles, smooth_mesh);//сгаживание окрестности


            var fi = 1.5;
            var df = 1;
            var ds = 1;
            var ps_cr = SurfaceReconstraction.cross_obj_flats(GL1.buffersGl.objs[smooth_mesh].vertex_buffer_data, df, ds, GL1, fi);//реконструирование отверстия
            //ps = Polygon3d_GL.smooth_lines_xy(ps, 2);
            var pols = Polygon3d_GL.triangulate_lines_xy(ps_cr);
            var scan_stl = Polygon3d_GL.toMesh(pols);
            var mesh_sm = scan_stl[0];
            GL1.add_buff_gl(mesh_sm, scan_stl[1], scan_stl[2], PrimitiveType.Triangles, selected_obj + "_rec");

        }
        private void reconstrust_area_first()
        {
            /*var selected_obj = selected_object(); if (selected_obj == null) return;
           //cut_area(RasterMap.type_out.outside, selected_obj);
           //var fi = cross_obj_flats_find_ang_z(GL1.buffersGl.objs_dynamic[scan_i].vertex_buffer_data, 20, 1);
           var fi = 1.5;
           var df = 1;
           var ds = 1;
           var ps = cross_obj_flats(GL1.buffersGl.objs[selected_obj].vertex_buffer_data, df,ds,fi);
           //ps = Polygon3d_GL.smooth_lines_xy(ps, 2);
           var pols = Polygon3d_GL.triangulate_lines_xy(ps);
           var scan_stl = Polygon3d_GL.toMesh(pols);
           var rec = GL1.add_buff_gl(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Triangles,"reconstruct");*/



            var selected_obj = selected_object(); if (selected_obj == null) return;//выбор объекта
            //cut_area(RasterMap.type_out.outside, selected_obj);
            //var fi = cross_obj_flats_find_ang_z(GL1.buffersGl.objs_dynamic[scan_i].vertex_buffer_data, 20, 1);
            var cut_obj = selected_obj + "_cut";
            var ret = cut_area(RasterMap.type_out.outside, selected_obj, cut_obj);//вырез отверстия раны
            if (ret == null) { Console.WriteLine("not cut"); return; };
            var polygs = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[cut_obj].vertex_buffer_data);
            var polyg_sm = RasterMap.smooth_mesh(polygs, 1);
            var mesh = Polygon3d_GL.toMesh(polyg_sm);
            var smooth_mesh = cut_obj + "_smooth";
            GL1.add_buff_gl(mesh[0], mesh[1], mesh[2], PrimitiveType.Triangles, smooth_mesh);//сгаживание окрестности


            var fi = 1.5;
            var df = 1;
            var ds = 1;
            var ps_cr = SurfaceReconstraction.cross_obj_flats(GL1.buffersGl.objs[smooth_mesh].vertex_buffer_data, df, ds, GL1, fi);//реконструирование отверстия
            //ps = Polygon3d_GL.smooth_lines_xy(ps, 2);
            var pols = Polygon3d_GL.triangulate_lines_xy(ps_cr);
            var scan_stl = Polygon3d_GL.toMesh(pols);
            var mesh_sm = scan_stl[0];

            //GL1.buffersGl.removeObj(cut_obj);
            //GL1.buffersGl.removeObj(smooth_mesh);

            var cuts = new List<string>();

            var conts = new List<List<Point3d_GL>>();
            var surfs = new List<Polygon3d_GL[]>();
            for (int i = 0; i < 3; i++)
            {
                var mesh_sm_tr = GL1.translateMesh(mesh_sm, 0, 0, -1 + (i * -1f));
                surfs.Add(Polygon3d_GL.polygs_from_mesh(mesh_sm_tr));
                //var rec = GL1.add_buff_gl(mesh_sm_tr, scan_stl[1], scan_stl[2], PrimitiveType.Triangles, selected_obj + "_cut_"+i);
                //cuts.Add(rec);
                var ps_inter = RasterMap.intersec_line_of_two_mesh(mesh_sm_tr, GL1.buffersGl.objs[selected_obj].vertex_buffer_data);
                GL1.addLineMeshTraj(ps_inter, new Color3d_GL(1, 0, 0), "intersec_cut_" + i);
                conts.Add(ps_inter.ToList());
            }

            var _traj = PathPlanner.generate_3d_traj_diff_surf(surfs, conts, param_tr);

            rob_traj = PathPlanner.join_traj(_traj);
            var ps = PathPlanner.matr_to_traj(rob_traj);

            if (GL1.buffersGl.objs.Keys.Contains(traj_i)) GL1.buffersGl.removeObj(traj_i);

            //for (int i = 0; i < rob_traj.Count; i++) GL1.addFrame(rob_traj[i],2);

            traj_i = GL1.addLineMeshTraj(ps.ToArray(), new Color3d_GL(0.9f), "gen_traj");
            var traj_rob = PathPlanner.generate_robot_traj(rob_traj, RobotFrame.RobotType.PULSE, param_tr);
            // return traj_rob;

        }

        private void but_remesh_test_Click(object sender, EventArgs e)
        {
            var selected_obj = selected_object(); if (selected_obj == null) return;

            var polygs = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[selected_obj].vertex_buffer_data, GL1.buffersGl.objs[selected_obj].color_buffer_data);
            var polyg_sm = RasterMap.smooth_mesh(polygs, Convert.ToDouble(tp_smooth_scan.Text));
            //polyg_sm = GL1.addNormals(polyg_sm, 0.5);
            var mesh = Polygon3d_GL.toMesh(polyg_sm);
            GL1.remove_buff_gl_id(selected_obj);
            GL1.add_buff_gl(mesh[0], mesh[1], mesh[2], PrimitiveType.Triangles, selected_obj);
        }
        private void but_intersec_obj_Click(object sender, EventArgs e)
        {
            var selected_obj = selected_nodes();
            if (selected_obj == null) return;
            if (selected_obj.Length < 2) return;
            var ps = RasterMap.intersec_line_of_two_mesh(GL1.buffersGl.objs[selected_obj[0]].vertex_buffer_data, GL1.buffersGl.objs[selected_obj[1]].vertex_buffer_data);
            GL1.addLineMeshTraj(ps, new Color3d_GL(1, 0, 0), "intersec");
        }
        private void but_delete_area_Click(object sender, EventArgs e)
        {
            var sel_ob = selected_object(); if (sel_ob == null) return;
            cut_area(RasterMap.type_out.outside, sel_ob);
        }

        private void but_keep_area_Click(object sender, EventArgs e)
        {
            var sel_ob = selected_object(); if (sel_ob == null) return;
            cut_area(RasterMap.type_out.inside, sel_ob);
        }
        private void but_set_model_matr_Click(object sender, EventArgs e)
        {
            var selected_obj = selected_nodes();
            if (selected_obj == null) return;
            if (selected_obj.Length < 2) return;
            var matr = GL1.buffersGl.objs[selected_obj[1]].trsc[0].matr;
            GL1.buffersGl.setMatrobj(selected_obj[0], 0, matr);
        }
        private void but_comp_basis_Click(object sender, EventArgs e)
        {
            var selected_obj = selected_nodes();
            if (selected_obj == null) return;
            if (selected_obj.Length < 2) return;
            var ps1 = Point3d_GL.fromMesh(GL1.buffersGl.objs[selected_obj[0]].vertex_buffer_data);
            var ps2 = Point3d_GL.fromMesh(GL1.buffersGl.objs[selected_obj[1]].vertex_buffer_data);
            if (ps1 == null || ps2 == null) return;
            if (ps1.Length < 4 || ps2.Length < 4) return;
            var matr =  UtilMatr.calcTransformMatr_cv(ps1, ps2);
            var matr_obj = GL1.addPointMesh(new Point3d_GL[] {new Point3d_GL(0,0,0)},Color3d_GL.blue(),"matr");
            GL1.buffersGl.setMatrobj(matr_obj,0,trsc.toGLmatrix(matr));
        }
        private void but_ps_cal_save_Click(object sender, EventArgs e)
        {
            var selected_obj = selected_object(); if (selected_obj == null) return;
            var mesh = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[selected_obj].vertex_buffer_data);
            var cont = GL1.get_contour()?.ToList();
            if (mesh != null && cont != null)
            {
                var ps_proj =  PathPlanner.project_contour_on_surface(mesh, cont);
                GL1.addPointMesh(ps_proj.ToArray(), Color3d_GL.green(), "proj_ps");
            }
               
        }
        private void but_send_traj_Click(object sender, EventArgs e)
        {            
            //var traj_rob = PathPlanner.generate_robot_traj(rob_traj);
            con1?.send_mes(debugBox.Text);
        }

        #endregion

        #region load_but
        private void but_stereo_3dp_Click(object sender, EventArgs e)
        {
            var scan_path = textB_scan_path.Text;
            var cam1_conf_path = textB_cam1_conf.Text;
            var cam2_conf_path = textB_cam2_conf.Text;
            var stereo_cal_path = textB_stereo_cal_path.Text;
            scan_path = scan_path.Split('\\').Reverse().ToArray()[0];
            string bfs_path = "bfs_cal.txt";
            var scanner = loadScanner_v2(cam1_conf_path, cam2_conf_path, stereo_cal_path,bfs_path);
            this.scanner = scanner;
            var fr = new Frame();

            (fr, this.scanner) = load_orig_scan_path(scan_path);
            //(fr, this.scanner) = load_photo_path(scan_path);

            comboImages.Items.Add(fr);
            //var stereo_cal_1 = scan_path.Split('\\').Reverse().ToArray()[0];
            //var cams_path = new string[] { @"cam1\" + stereo_cal_1, @"cam2\" + stereo_cal_1 }; var reverse = true;
            //var frms_stereo1 = FrameLoader.loadImages_stereoCV(cams_path[0], cams_path[1], FrameType.Pattern, reverse);
            //comboImages.Items.AddRange(frms_stereo1);
            chess_size = new Size(6, 7);
            var ps = ps3d_frame(fr, scanner);
            //ps3d_frame_sq(fr, scanner);
            GL1.addPointMesh(ps,Color3d_GL.red());



            var dist = Point3d_GL.dist_betw_ps(ps);
            prin.t(dist);
            
        }
        
        Point3d_GL[] ps3d_frame(Frame fr,Scanner scanner)
        {
            var corn1 = new System.Drawing.PointF[0];
            var corn2 = new System.Drawing.PointF[0];
            var mat1 = fr.im;
            var mat2 = fr.im_sec;
            if(ch_b_dist.Checked)
            {
                mat1 = scanner.stereoCamera.cameraCVs[0].undist(mat1);
                mat2 = scanner.stereoCamera.cameraCVs[1].undist(mat2);
            }
            
            imBox_base_1.Image = FindCircles.findCircles(mat1, ref corn1, chess_size);
            imBox_base_2.Image = FindCircles.findCircles(mat2, ref corn2, chess_size);
            //prin.t("p3d");
            //prin.t(corn1);
            var mat_p1 = UtilOpenCV.drawPointsF(mat1, corn1, 255, 0, 0);
            //CvInvoke.Imshow("p3d", mat_p1);
            //CvInvoke.WaitKey();
            var ps = PointCloud.comp_stereo_ps(PointF.toPointF(corn1), PointF.toPointF(corn2), scanner.stereoCamera,GL1);
            //GL1.addPointMesh(ps);
            return ps;
        }

        void ps3d_frame_sq(Frame fr, Scanner scanner)
        {
            var corn1 = new System.Drawing.PointF[0];
            var corn2 = new System.Drawing.PointF[0];
            var mat1 = fr.im;
            var mat2 = fr.im_sec;
            if (ch_b_dist.Checked)
            {
                mat1 = scanner.stereoCamera.cameraCVs[0].undist(mat1);
                mat2 = scanner.stereoCamera.cameraCVs[1].undist(mat2);
            }

            imBox_base_1.Image = FindCircles.findCircles(mat1, ref corn1, chess_size);
            imBox_base_2.Image = FindCircles.findCircles(mat2, ref corn2, chess_size);

            var marksize = 10;
            var size_patt = new Size(6, 7);
            var x = (size_patt.Width - 1) * marksize;
            var y = (size_patt.Height - 1) * marksize;

            var points3d = new Point3d_GL[]
                {
                    new Point3d_GL(x,y,0),
                    new Point3d_GL(0,y,0),
                    new Point3d_GL(x,0,0),
                    new Point3d_GL(0,0,0)
                };

            var ps_2d = UtilOpenCV.generatePoints(size_patt, marksize);
            points3d = Point3d_GL.toPoints(ps_2d);
            var pos1 = scanner.stereoCamera.cameraCVs[0].compPos(fr.im, PatternType.Mesh, size_patt, marksize);
            var pos2 = scanner.stereoCamera.cameraCVs[1].compPos(fr.im_sec, PatternType.Mesh, size_patt, marksize);
            var ps1_c = points3d;
            var ps2_c = points3d;
            ps1_c = Point3d_GL.multMatr(points3d, scanner.stereoCamera.cameraCVs[0].matrixSC);//
            ps2_c = Point3d_GL.multMatr(points3d, scanner.stereoCamera.cameraCVs[1].matrixSC);
            //var ps2d_1 = PointCloud.computePointsCam3d_to2d(ps1_c, scanner.stereoCamera.cameraCVs[0]);

            (ps1_c,ps2_c) = PointCloud.comp_stereo_ps_from_cam(ps1_c, ps2_c, scanner.stereoCamera, GL1);
            //var d1 = Point3d_GL.dist_betw_ps(ps1_c);
            //prin.t(d1);
            //GL1.addPointMesh(ps1_c,Color3d_GL.blue(),"cam1_p");
            //GL1.addPointMesh(ps2_c, Color3d_GL.aqua(), "cam2_p");
        }
        (Frame, Scanner) load_photo_path(string filepath)
        {
            var stereo_cal_1 = filepath.Split('\\').Reverse().ToArray()[0];
            var cams_path = new string[] { @"cam1\" + stereo_cal_1, @"cam2\" + stereo_cal_1 }; var reverse = true;
            var frms_stereo1 = FrameLoader.loadImages_stereoCV(cams_path[0], cams_path[1], FrameType.Pattern, reverse);

            return (frms_stereo1[0], scanner);
        }
        (Frame,Scanner) load_orig_scan_path(string filepath)
        {
            var orig1 = new Mat(Directory.GetFiles("cam1\\" + filepath + "\\orig")[0]);
            var orig2 = new Mat(Directory.GetFiles("cam2\\" + filepath + "\\orig")[0]);
            Console.WriteLine(Directory.GetFiles("cam1\\" + filepath)[0]);
            Console.WriteLine(Directory.GetFiles("cam2\\" + filepath)[0]);

            var ve_paths1 = get_video_path(1, filepath);
            string video_path1 = ve_paths1[0];
            var ve_paths2 = get_video_path(2, filepath);
            string video_path2 = ve_paths2[0];


            scanner.set_coord_sys(StereoCamera.mode.model);
            var name_v1 = Path.GetFileNameWithoutExtension(video_path1);
            var name_v2 = Path.GetFileNameWithoutExtension(video_path2);
            if (name_v1.Length > 1 && name_v2.Length > 1)
            {
                scanner.set_rob_pos(name_v1);
                scanner.set_coord_sys(StereoCamera.mode.world);
            }

            CvInvoke.Rotate(orig2, orig2, RotateFlags.Rotate180);
            var fr_st_vid = new Frame(orig1, orig2, "sd", FrameType.Test);
            fr_st_vid.stereo = true;

            return (fr_st_vid,scanner);
        }
        private void but_scan_load_ex_Click(object sender, EventArgs e)
        {

            var scan_path = textB_scan_path.Text;
            var cam1_conf_path = textB_cam1_conf.Text;
            var cam2_conf_path = textB_cam2_conf.Text;
            var stereo_cal_path = textB_stereo_cal_path.Text;

            int strip = Convert.ToInt32(tb_strip_scan.Text);
            double smooth = Convert.ToDouble(tp_smooth_scan.Text);

            var scanner = loadScanner_v2(cam1_conf_path, cam2_conf_path, stereo_cal_path);
            this.scanner = scanner;
            load_scan_v2(scanner,scan_path, strip,smooth);

        }

        private void but_scan_stereo_rob_Click(object sender, EventArgs e)
        {
            var scan_path = textB_scan_path.Text;
            var cam1_conf_path = textB_cam1_conf.Text;
            var cam2_conf_path = textB_cam2_conf.Text;
            var stereo_cal_path = textB_stereo_cal_path.Text;

            int strip = Convert.ToInt32(tb_strip_scan.Text);
            //double smooth = Convert.ToDouble(tp_smooth_scan.Text);
            double smooth = -1;
            string bfs_path = "bfs_cal.txt";

            var scanner = loadScanner_v2(cam1_conf_path, cam2_conf_path, stereo_cal_path,bfs_path);
            load_scan_v2(scanner, scan_path, strip, smooth);
        }

        private void but_scan_load_sing_Click(object sender, EventArgs e)
        {
            var scan_path = textB_scan_path.Text;
            var cam1_conf_path = textB_cam1_conf.Text;
            var laser_line_path = textB_cam2_conf.Text;

            int strip = Convert.ToInt32(tb_strip_scan.Text);
            double smooth = Convert.ToDouble(tp_smooth_scan.Text);

            var scanner = loadScanner_sing(cam1_conf_path,laser_line_path);
            load_scan_sing(scanner, scan_path, strip, smooth);
        }
        private void but_load_sing_calib_Click(object sender, EventArgs e)
        {
            var scan_path = textB_scan_path.Text;
            var cam1_conf_path = textB_cam1_conf.Text;

            int strip = Convert.ToInt32(tb_strip_scan.Text);
            double smooth = Convert.ToDouble(tp_smooth_scan.Text);

            var scanner = loadScanner_sing(cam1_conf_path);
            load_calib_sing(scanner,scan_path, strip, smooth);
        }

        string save_file_name(string init_direct,string init_name, string extns)
        {
            var filePath = string.Empty;
            using (SaveFileDialog openFileDialog = new SaveFileDialog())
            {
                openFileDialog.InitialDirectory = init_direct;
                //openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.Filter = extns + " files (*." + extns + ")|*." + extns;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.FileName = init_name;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                }
            }
            return filePath;
        }
        string get_file_name(string init_direct,string extns)
        {
            var filePath = string.Empty;
            using (  OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = init_direct;
                //openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.Filter = extns+" files (*."+ extns + ")|*." + extns ;
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                }
            }
            return filePath;
        }

        string get_folder_name(string init_direct)
        {
            OpenFileDialog folderBrowser = new OpenFileDialog();
            // Set validate names and check file exists to false otherwise windows will
            // not let you select "Folder Selection."
            folderBrowser.InitialDirectory = init_direct;
            folderBrowser.ValidateNames = false;
            folderBrowser.CheckFileExists = false;
            folderBrowser.CheckPathExists = true;
            // Always default to Folder Selection.
            folderBrowser.FileName = "Folder Selection.";
            if (folderBrowser.ShowDialog() == DialogResult.OK)
            {
                return Path.GetDirectoryName(folderBrowser.FileName);
            }
            return "";
        }

        private void but_load_conf_cam1_Click(object sender, EventArgs e)
        {
            textB_cam1_conf.Text =  get_file_name(Directory.GetCurrentDirectory(),"txt");
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path);
        }

        private void but_load_conf_cam2_Click(object sender, EventArgs e)
        {
            textB_cam2_conf.Text = get_file_name(Directory.GetCurrentDirectory(), "txt");
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path);
        }

        private void but_stereo_cal_path_Click(object sender, EventArgs e)
        {
            textB_stereo_cal_path.Text = get_folder_name(Directory.GetCurrentDirectory());
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path);
        }

        private void but_scan_path_Click(object sender, EventArgs e)
        {
            textB_scan_path.Text = get_folder_name(Directory.GetCurrentDirectory());
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path);
        }

        private void MainScanningForm_Load(object sender, EventArgs e)
        {
            formSettings.load_settings(textB_cam1_conf,textB_cam2_conf,textB_stereo_cal_path,textB_scan_path);
        }

        private void but_gl_clear_Click(object sender, EventArgs e)
        {
            GL1.buffersGl.removeObj(scan_i);
            GL1.buffersGl.removeObj(traj_i);
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(10, 0, 0), new Point3d_GL(0, 10, 0), new Point3d_GL(0, 0, 10));
            //GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(10, 0, 0), new Point3d_GL(0, 10, 0), new Point3d_GL(0, 0, 10));
            
        }

        private void but_traj_clear_Click(object sender, EventArgs e)
        {
            GL1.buffersGl.removeObj(traj_i);
            //GL1.SortObj();
        }


        private void but_calibr_Bfs_Click(object sender, EventArgs e)
        {
            var cam1_conf_path = textB_cam1_conf.Text;
            var cam2_conf_path = textB_cam2_conf.Text;
            var cam1 = CameraCV.load_camera(cam1_conf_path);
            var cam2 = CameraCV.load_camera(cam2_conf_path);
            var stereo = new StereoCamera(new CameraCV[] { cam1, cam2 });
            stereocam_scan = stereo;
            var stereo_cal_1 = textB_stereo_cal_path.Text.Split('\\').Reverse().ToArray()[0];
            var cams_path = new string[] { @"cam1\" + stereo_cal_1, @"cam2\" + stereo_cal_1 }; var reverse = true;
            var frms_stereo1 = FrameLoader.loadImages_stereoCV(cams_path[0], cams_path[1], FrameType.Pattern, reverse);
            comboImages.Items.AddRange(frms_stereo1);

            chess_size = new Size(10, 11);
            var markSize = 6.2273f;

            chess_size = new Size(6, 7);
            markSize = 10f;
            stereo.calibrateBfs(frms_stereo1,chess_size, markSize);
        }

        private void but_im_to_3d_im1_Click(object sender, EventArgs e)
        {
            var im1 = (Mat)imageBox1.Image;
            //var im = (Mat)imBox_base_1.Image;
            var im2 = new Mat();
            CvInvoke.CvtColor(im1, im2, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(im2, im2, new Size(7, 7), -1);
            send_buffer_img(im2.ToImage<Gray,Byte>(), PrimitiveType.Triangles,GL1);
            
        }

        private void but_load_fr_cal_Click(object sender, EventArgs e)
        {
            var stereo_cal_1 = textB_stereo_cal_path.Text.Split('\\').Reverse().ToArray()[0];
            var cams_path = new string[] { @"cam1\" + stereo_cal_1, @"cam2\" + stereo_cal_1 }; var reverse = true;
            //cams_path = new string[] { openGl_folder+"/monitor_0/distort", openGl_folder + "/monitor_1/distort" };  reverse = false;
            var frms_stereo = FrameLoader.loadImages_stereoCV(cams_path[0], cams_path[1], FrameType.Pattern, reverse);

            var cam1_conf_path = textB_cam1_conf.Text;
            var cam2_conf_path = textB_cam2_conf.Text;
            var cam1 = CameraCV.load_camera(cam1_conf_path);
            var cam2 = CameraCV.load_camera(cam2_conf_path);
            var stereo = new StereoCamera(new CameraCV[] { cam1, cam2 });
            stereocam_scan = stereo;
            //stereocam_scan.calibrate_stereo(frms_stereo, PatternType.Mesh,chess_size);
            chess_size = new Size(6, 7);
            var markSize = 10f;
            stereocam_scan.calibrate_stereo_rob(frms_stereo, PatternType.Mesh, chess_size,  markSize );
            comboImages.Items.AddRange(frms_stereo);

        }

        private void but_save_stl_Click(object sender, EventArgs e)
        {

            var stl_name = save_file_name(Directory.GetCurrentDirectory(), tree_models.SelectedNode.Text, "stl");
            //var scan_stl = Polygon3d_GL.toMesh(mesh);
            if(stl_name == null) return;
            STLmodel.saveMesh(GL1.buffersGl.objs[tree_models.SelectedNode.Text].vertex_buffer_data, stl_name);
        }
        private void but_load_stl_Click(object sender, EventArgs e)
        {
            var stl_name = get_file_name(Directory.GetCurrentDirectory(), "stl");
            
            var scan_stl = new Model3d(stl_name, false);
            scan_i = GL1.add_buff_gl(scan_stl.mesh, scan_stl.color, scan_stl.normale, PrimitiveType.Triangles,Path.GetFileNameWithoutExtension(stl_name));
        }

        private void but_del_obj3d_Click(object sender, EventArgs e)
        {

            GL1.buffersGl.removeObj(tree_models.SelectedNode.Text);
            int i_n = GL1.node_obj_in_tree(tree_models.SelectedNode.Text);
            if (i_n >= 0) tree_models.Nodes.Remove(tree_models.Nodes[i_n]);

        }


        #endregion

        #region rob_sc_but

        private void but_rob_con_sc_Click(object sender, EventArgs e)
        {
            if (con1 == null)
            {
                con1 = new TCPclient();
            }
            var pulse = "localhost";
            var kuka = "172.31.1.147";
            port_tcp = Convert.ToInt32(tb_port_tcp.Text);
            con1.Connection(port_tcp, kuka);
        }

        private void but_rob_discon_sc_Click(object sender, EventArgs e)
        {
            try
            {
                con1.send_mes("q\n");
                con1.close_con();
            }
            catch
            {

            }
        }

        private void but_rob_send_sc_Click(object sender, EventArgs e)
        {
                con1?.send_mes(debugBox.Text + "\n");
        }

        private void but_rob_res_sc_Click(object sender, EventArgs e)
        {
            
        }

        private void but_rob_auto_sc_Click(object sender, EventArgs e)
        {
            try_send_rob("a\n");
            printing = true;
        }

        private void but_rob_manual_sc_Click(object sender, EventArgs e)
        {
            try_send_rob("m\n");
            printing = false;
        }

        private void but_rob_clear_sc_Click(object sender, EventArgs e)
        {
            try_send_rob("c\n");
        }

        void try_send_rob(string mes)
        {
            try
            {
                con1.send_mes(mes);
            }
            catch
            {

            }
        }

        private void but_rob_start_sc_Click(object sender, EventArgs e)
        {
            try_send_rob("s\n");
        }

        private void but_rob_traj_pulse_Click(object sender, EventArgs e)
        {

            debugBox.Text = gen_traj_rob(RobotFrame.RobotType.PULSE);
        }

        private void but_rob_traj_kuka_Click(object sender, EventArgs e)
        {
            debugBox.Text = gen_traj_rob(RobotFrame.RobotType.KUKA);
        }



        #endregion

        private void tree_models_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                prop_grid_model.SelectedObject = GL1.buffersGl.objs[e.Node.Text];
                prop_grid_model.Text = e.Node.Text;
                if(ModifierKeys == Keys.Control)
                {
                    e.Node.BackColor = Color.Green;
                    GL1.buffersGl.objs[e.Node.Text] = GL1.buffersGl.objs[e.Node.Text].setSelected(true);
                }
                else
                {
                    e.Node.BackColor = Color.White;
                    GL1.buffersGl.objs[e.Node.Text] = GL1.buffersGl.objs[e.Node.Text].setSelected(false);
                }
                   
            }
            catch
            {

            }
            
        }

        private void tree_models_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right) clear_selected_nodes();
        }
        private void tree_models_AfterCheck(object sender, TreeViewEventArgs e)
        {
            //GL1.buffersGl.objs[e.Node.Text] = GL1.buffersGl.objs[e.Node.Text].setVisible(e.Node.Checked);
        }

        string[] selected_nodes()
        {
            List<string> nodes = new List<string>();
            for(int i=0; i<tree_models.Nodes.Count; i++)
            {
                if (tree_models.Nodes[i].BackColor == Color.Green) nodes.Add(tree_models.Nodes[i].Text);
            }
            return nodes.ToArray();
        }
        string selected_object()
        {
            var selected_obj = selected_nodes();
            if (selected_obj == null) return null;
            if (selected_obj.Length < 1) return null;
            //Console.WriteLine(selected_obj[0]);
            return selected_obj[0];
        }

        void clear_selected_nodes()
        {
            try
            {
                for (int i = 0; i < tree_models.Nodes.Count; i++)
                {
                    tree_models.Nodes[i].BackColor = Color.White;
                    GL1.buffersGl.objs[tree_models.Nodes[i].Text].setSelected(false);

                }
            }
            catch
            {

            }
            
        }

        private void but_home_las_Click(object sender, EventArgs e)
        {
            laserLine?.set_home_laser();
        }

        private void but_div_disp_Click(object sender, EventArgs e)
        {

            laserLine?.set_div_disp(Convert.ToInt32(tb_div_disp.Text));
        }

        private void but_dir_disp_Click(object sender, EventArgs e)
        {
            laserLine?.set_dir_disp(Convert.ToInt32(tb_dir_disp.Text));
        }

        private void but_extr_st_Click(object sender, EventArgs e)
        {
            double vel_noz = Convert.ToDouble(tb_print_vel.Text);
            double d_noz = Convert.ToDouble(tb_print_nozzle_d.Text);
            double d_syr = Convert.ToDouble(tb_print_syr_d.Text);
            var div = LaserLine.vel_div(vel_noz, d_noz, d_syr);
            laserLine?.set_dir_disp(-1);
            laserLine?.set_div_disp(div);
        }

        private void but_printer_traj_fab_Click(object sender, EventArgs e)
        {
            var pattern = PathPlanner.gen_traj_3d_pores(param_patt, param_tr);
            var traj = PathPlanner.ps_to_matr(pattern);
            var prog = PathPlanner.generate_printer_prog(traj, param_tr);
            debugBox.Text = prog;
            if (GL1.buffersGl.objs.Keys.Contains("prog")) GL1.buffersGl.removeObj("prog");
            //GL1.addLineMeshTraj(pattern.ToArray(), new Color3d_GL(1, 0, 0), "prog");
            GL1.addTraj(pattern.ToArray(), "prog");
            //GL1.addTrajPoint(pattern.ToArray(), "prog");
        }

        private void but_scan_virt_Click(object sender, EventArgs e)
        {
            var n = Convert.ToInt32(boxN.Text);
            GL1.start_animation(100);
            var folder_scan = box_scanFolder.Text;
            UtilOpenCV.saveImage(imBox_mark1, imBox_mark2, "1.png", folder_scan + "\\orig");
            startWrite(1,n);
            startWrite(2,n);
        }

        private void but_start_anim_Click(object sender, EventArgs e)
        {
            GL1.start_animation(100);
        }

        private void but_photo_gl_Click(object sender, EventArgs e)
        {

            UtilOpenCV.saveImage(imBox_mark1, imBox_mark2, txBx_photoName.Text + "_" + photo_number.ToString() + ".png", box_photoFolder.Text);
            photo_number++;
        }

        private void but_con_scan_Click(object sender, EventArgs e)
        {
            videoStart(2);
            videoStart(1); Thread.Sleep(5000);
            find_ports(); Thread.Sleep(100);
            laserLine = new LaserLine(portArd); Thread.Sleep(1000);
            laserLine?.setShvpVel(200); Thread.Sleep(100);
            laserLine?.laserOn(); Thread.Sleep(100);
            laserLine?.set_home_laser(); Thread.Sleep(1000);
            laserLine?.setShvpPos(350); Thread.Sleep(100);

        }

        private void but_scan_pres_Click(object sender, EventArgs e)
        {
            var scan_path = textB_scan_path.Text;
            var filepath = scan_path.Split('\\').Reverse().ToArray()[0];
            var ve_paths1 = get_video_path(1, filepath);
            string video_path1 = ve_paths1[0];

            // string enc_path1 = ve_paths1[1];

            var ve_paths2 = get_video_path(2, filepath);
            string video_path2 = ve_paths2[0];
            // string enc_path2 = ve_paths2[1];

            string enc_path = ve_paths1[1];
            var pairs = frames_sync_from_file(enc_path, lab_scan_pres);
        }
    }
}

//Manipulator.calcRob_pulse();
//load_camers_v2();
// 
//var patt_ph = new Mat("old_patt.png");//"old_patt.png" || @"cam2\test_circle\1_2.png"
//patt[0] = patt_ph;


// var frms = FrameLoader.loadPathsDiff(new string[] { 
// @"cam1\cam1_cal_0508_1\1", 
//  @"cam2\cam2_cal_0508_1" }, FrameType.Pattern, PatternType.Mesh);
// comboImages.Items.AddRange(frms);
//load_camers_v2();
//var scan = Reconstruction.loadScan(@"cam1\pos_cal_Z_2609_2\test", @"cam1\las_cal_2609_3", @"cam1\table_scanl_2609_3", @"cam1\pos_basis_2609_2", 52.5, 30,40, SolveType.Complex, 0.1f, 0.1f, 0.8f,comboImages);
//var scan = Reconstruction.loadScan(@"cam2\pos_cal_1906_1\test", @"cam2\las_cal_2", @"cam2\mouse_scan_1906_3", @"cam1\pos_basis_2609_2", 52.5, 30, 40, SolveType.Complex, 0.1f, 0.1f, 0.8f, comboImages);   
//var path_d = @"D:\reposD\kuka\bin_0110\";
/*loadScannerLinLas(
     new string[] { path_d+ @"cam1\cdwz0307" },
     new string[] { path_d + @"cam2\las_cal_0307_2a", path_d + @"cam2\las_cal_0307_2b" },
     new string[] { path_d + @"cam2\las_cal_0307_2a\orig", path_d + @"cam2\las_cal_0307_2b\orig" },
     path_d + @"cam2\las_cal_0307_2a", path_d + @"cam2\las_cal_0307_2a\orig",
     new float[] { 0.5f, 0.5f, 0.1f }, true);*/

/*loadScannerLinLas(
     new string[] { path_d + @"cam1\cdwz0307" },
     new string[] { path_d + @"cam1\las_cal_0707_1" },
     new string[] { path_d + @"cam1\las_cal_0707_1\orig" },
     path_d + @"cam1\las_cal_0707_1", path_d + @"cam1\las_cal_0707_1\orig",
     new float[] { 0.5f, 0.5f, 0.1f }, true);*/


/*var scan_path = @"C:\Users\ASUS PC\source\repos\Heis97\opengl3\opengl3\bin\x86\Debug\cam1\las_cal_1004_1";
var cam1_conf_path = @"C:\Users\ASUS PC\source\repos\Heis97\opengl3\opengl3\bin\x86\Debug\cam1_conf_0508_1.txt";

int strip = 10;
double smooth = Convert.ToDouble(tp_smooth_scan.Text);

loadScanner_sing(cam1_conf_path);
load_calib_sing(scan_path, strip, smooth);*/


/* var mesh = STLmodel.parsingStl_GL4("half_sphere.STL");
            mesh = GL1.scaleMesh(mesh, 8f);
            mesh = GL1.translateMesh(mesh, -65.8f, -107.3f);*/

//GL1.addGLMesh(mesh,PrimitiveType.Triangles);
/*FrameLoader.substractionImage(@"cam1\las_cal_2706_1b", @"cam1\cam_orig_2706_1b");
FrameLoader.substractionImage(@"cam1\las_cal_2706_1a", @"cam1\cam_orig_2706_1a");
FrameLoader.substractionImage(@"cam1\scan_2706_1", @"cam1\cam_orig_2706_2\1");*/

//loadScanner();
//loadStereo(); 
// loadScannerLin(new string[] { @"cam1\camera_cal_1006_1" }, @"cam1\las_cal_2606_2\1", @"cam1\lin_cal_2606_1", @"cam1\scan_2606_1\dif",new float[] { 0.1f,0.5f,0.5f });
//loadScannerLin(new string[] { @"cam1\calib_1_2505" }, @"cam2\las_cal_1006_1\1", @"cam1\lin_cal_0106_1\def", @"cam2\scan_1006_3\dif", new float[] { 0.1f, 0.9f, 0.1f });

/* loadScannerLinLas(
     new string[] { @"cam1\cdwz0307" },
     new string[] { @"cam2\las_cal_0307_2a", @"cam2\las_cal_0307_2b" },
     new string[] { @"cam2\las_cal_0307_2a\orig", @"cam2\las_cal_0307_2b\orig" },
     @"cam2\las_cal_0307_2a", @"cam2\las_cal_0307_2a\orig",
     new float[] { 0.5f, 0.5f, 0.1f }, true);*/

/*  loadScannerLinLas(
      new string[] { @"cam1\camera_cal_1006_1" },
      new string[] { @"cam1\las_cal_0707_1"},
      new string[] { @"cam1\las_cal_0707_1\orig" },
      //@"cam1\las_cal_0707_1", @"cam1\las_cal_0707_1\orig",

      @"cam1\scan_0707_1", @"cam1\scan_0707_1\orig",
      new float[] { 0.5f, 0.5f, 0.1f }, true);*/


/*loadScannerStereoLas(
     @"camera_cal_1006_1",
     @"stereo_cal_0907_1",
     @"test_1107_7",
     new float[] { 0.1f, 0.5f, 0.1f }, true);*/


/*addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(30, 0, 0), new Point3d_GL(0, 30, 0), new Point3d_GL(0, 0, 30));

 GL1.printDebug(debugBox);
 addGLMesh(cube_buf, PrimitiveType.Triangles, 60.0f, 0, 0, 0.5f, 0.5f, 0.5f, 5.0f);
double side = 17.1428;
 double fov = 57.4;
 var frames_pos = loadImages(@"cam2\pos_cal_1906_1", fov,side);
 var fr = from f in frames_pos
          orderby f.pos_rob.z
          select f;
 frames_pos = fr.ToList();
imageBox2.Image =  lineErr(frames_pos, 51.4, 17.1428);
var frames_pos = loadImages(@"cam1\pos_cal_1906_2", 53, 30);
fov3dMap(frames_pos, 53, 30, 80, 0.5, 5,5);
double side = 30;
int n = 10;

for (int i=0; i<frames_pos.Count-n; i++)
{
    var frs = frames_pos.GetRange(i, n);

    var retFov = findOneVarDec(45, 60, frs , side, calcPixForCam);
    Console.WriteLine(retFov);
}
var retFov = findOneVarDec(45, 60, frames_pos, side, calcPixForCam);
Console.WriteLine(retFov);

 var frames_scan = loadImages_simple(@"cam1\mouse_scan_1906_2");


loadScan(@"cam1\pos_cal_Z_2609_2\test", @"cam1\las_cal_2609_3", @"cam1\table_scanl_2609_3", @"cam1\pos_basis_2609_2", 52.5, 30, SolveType.Complex, 0.1f, 0.1f, 0.8f);
 loadScan(@"cam2\pos_cal_Z_2609_2\test", @"cam2\las_cal_2609_3", @"cam2\table_scanl_2609_3", @"cam2\pos_basis_2609_2", 52.5, 30, SolveType.Complex, 0.1f, 0.8f, 0.1f);

loadScan(@"cam1\pos_cal_mid_Z_1\test", @"cam1\las_cal_mid_1", @"cam1\scanl_mid_1\test", @"cam1\pos_basis_mid", 55.1, 30, SolveType.Complex, 0.1f, 0.8f, 0.1f);

loadScan(@"cam1\pos_cal_big_Z\test", @"cam1\las_cal_big_1", @"cam1\scanl_big_2", @"cam1\pos_basis_big", 53.8, 30, SolveType.Complex, 0.1f, 0.8f, 0.1f);


loadImages_stereo(@"orient_cal_1108_1", 53, 30, 30, true);
loadScan(@"cam2\pos_cal_1906_1\test", @"cam2\las_cal_2", @"cam2\mouse_scan_1906_3", 54, 68);
loadImages_test(@"cam1\scan_table_2609_2\test");

var frs = loadImages_calib(@"fov_pic/test_3");
var fov_points = map_fov_3d(frs);
paint3dpointsRGB(fov_points);

generateImage_BOARD(7, 8);
generateImage(7, 0.5);

frames = loadImages_simple(@"tutor\cam2");
EmguCVUndistortFisheye(@"ref_model\test4\distort", new Size(7, 6));
 calibrateFishEyeCam(frames.ToArray(), new Size(7, 6));
 calibrateCam(frames.ToArray(),new Size(7,6));
cameraDistortionCoeffs[0, 0] = -0.1*Math.Pow(10,-6);
 cameraDistortionCoeffs[1, 0] = 0;
  cameraDistortionCoeffs[2, 0] = 0;
  cameraDistortionCoeffs[3, 0] = 0;
cameraDistortionCoeffs[4, 0] =0;
 distortFolder(@"ref_model\test4");

generateImage3D(7, 0.5f, 30.0f);
generateImage3D_BOARD(8, 7, 10);

 calcFov(frs);
 testCalib();

loadImages(@"cam1\pos_basis_big\test", 53, 30,40);



loadImages(@"ref_model\test5", 53,30);

loadImages_stereo(@"pos_cal_Z_2609_2", 53, 30, 40, true);
 loadImages_stereo(@"pos_cal_1108_Y_1", 53, 30, 40, true);
loadImages_stereo(@"pos_cal_1108_X_1", 53, 30, 30, true);

loadImages_basis(@"cam1\pos_cal_basis_1108",53, 30, 30);
 loadImages_stereo(@"pos_cal_2707_1", 53, 30);

 loadImages(@"cam1\pos_cal_mid_Z_1\test", 53, 30, 30,15, true);

calcRob();*/

// timer = new System.Timers.Timer(100);
// timer.Elapsed += drawTimeLabel;
// timer.Start();
//loadVideo_stereo(@"test_1107_7");
/*loadScannerStereoLas(
new string[] { @"camera_cal_1807_1", @"camera_cal_1807_2" },
 @"stereocal_2607_1",
 @"scan_2607_4",
 new float[] { 0.1f, 0.5f, 0.1f }, true, 20);*/




/*for(int i=0; i< frms_stereo1.Length;i++)
{
    scanner.initStereo(new Mat[] { frms_stereo1[i].im, frms_stereo1[i].im_sec }, PatternType.Mesh);
}*/

// oneCam(new string[] { @"cam1\camera_cal_1807_1" },10f);