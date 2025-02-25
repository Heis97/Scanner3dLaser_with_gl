using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Math;
using System.Threading;
using System.IO.Ports;
using PathPlanning;
using Newtonsoft.Json;
using Emgu.CV.Util;
using Emgu.CV.Features2D;
using System.Linq.Expressions;
using jakaApi;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Globalization;
using Accord.Math.Geometry;
using System.Net;
using System.Drawing.Drawing2D;

namespace opengl3
{
    public partial class MainScanningForm : Form
    {
        #region var
        double z_syrenge_offset = 0;
        double z_calibr_offset = 0;
        double[][] koef_las_z_pos = null;
        double[] koef_term_to_byte = null;
        bool comp_current_compens = false;
        bool laser_on = false;
        int surface_type = 0;
        double[] koef_byte_to_term = null;
        string[] syringe_size_text = new string[] { "10 мл", "5 мл", "2 мл" };
        double[] syringe_size_vals = new double[] { 0, 17.5, 27.2 };
        bool temp_control = false;
        bool visualise_compens = false;
        bool scanning_status = false;
        Thread tcp_thread = null;
        Thread ard_acust_thread = null;
        MovmentCompensation movm = null;
        List<PosTimestamp> timestamps = new List<PosTimestamp>();
        VideoCapture[] videoCaptures = new VideoCapture[4];
        bool record_times = false;
        bool compens_period = false;
        bool window_auto = false;
        Rectangle laser_roi_static = new Rectangle(0, 0, 100, 10);
        long start_record_time = 0;
        long record_time = 0;
        int save_vid_count = 0;
        double dist_contr_rob = 10;
        double cur_pos_movm = 0;
        Matrix4x4f[] ms = new Matrix4x4f[10];
        //var qs = new Matrix4x4f[8];
        Matrix4x4f[] qms = new Matrix4x4f[8];
        List<RobotFrame> frames_rob = new List<RobotFrame>();
        List<RobotFrame> frames_rob_end = new List<RobotFrame>();
        RobotFrame.RobotType current_robot = RobotFrame.RobotType.PULSE;
        double r_cyl = 1;
        Matrix<double> m_cyl = new Matrix<double>(4, 4);
        Point3d_GL off_cyl = new Point3d_GL();
        int ks_i = 0;
        int ws_i = 0;
        bool scan_dist = false;
        bool scan_sync = false;
        int port_tcp = 30005;
        StringBuilder sb_enc = null;
        string video_scan_name = "1";
        string scan_i = "emp";
        string traj_i = "emp";
        TrajParams traj_config = new TrajParams();
        PatternSettings patt_config = new PatternSettings();
        ScannerConfig scanner_config = new ScannerConfig();
        FormSettings formSettings = new FormSettings();
        Polygon3d_GL[] mesh = null;
        List<Matrix<double>> rob_traj = null;
        Point3d_GL[] cont_traj = null;
        ImageBox[] imb_base = null;
        ImageBox[] imb_main = null;
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
       // Size chess_size = new Size(8, 9);
         Size chess_size = new Size(6, 7);
        Size chess_size_real = new Size(6, 7);
        StereoCameraCV stereocam = null;
        StereoCamera stereocam_scan = null;
        CameraCV cameraCVcommon;
        TCPclient con1;
        private const float PI = 3.14159265358979f;
        private Size cameraSize = new Size(1280, 720);
        // private Size cameraSize = new Size(1280, 960);
        // private Size cameraSize = new Size(1184, 656);
        //private Size cameraSize = new Size(1184, 656);
        //private Size cameraSize = new Size(1024, 576);
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
        volatile IntPtr[] camera_ind_ptr = new IntPtr[] { (IntPtr)0, (IntPtr)0, (IntPtr)0, (IntPtr)0, (IntPtr)0, (IntPtr)0, (IntPtr)0 };
        volatile int[] imb_ind_cam = new int[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        volatile List<long> camera_frame_time = new List<long>();
        List<float[]> im = new List<float[]>();
        public List<Mat> Ims;
        public List<Point> ints;
        private Point3d_GL offset_model;
        int fr_ind = 0;
        private List<Frame> frames;

        int res_min = 256 * 1;
        volatile Mat[] mat_global = new Mat[6];
        Mat matr = new Mat();
        int flag1 = 1;
        int flag2 = 1;
        int im_i = 100000;
        double red_c = 240;
        double blue_c = 250;
        double green_c = 250;
        const int scanning_len = 330;
        volatile int videoframe_count = 0;

        double compens_gap = 0.8;

        volatile int[] videoframe_counts = new int[5] { -1, -1, -1, -1, -1 };

        volatile int[] videoframe_counts_stop = new int[5] { int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue, int.MaxValue };

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

        bool compensation = false;
      

        int k = 1;
        bool writ = false;
        int bin_pos = 40;
        double cur_pos_z = 0;
        Mat mat0 = new Mat();
        List<Mat> cap_mats = new List<Mat>();
        Features features = new Features();
        MCvPoint3D32f[] points3D = new MCvPoint3D32f[]
            {
                new MCvPoint3D32f(0,0,0),
                new MCvPoint3D32f(60, 0, 0),
                new MCvPoint3D32f(0, 60, 0),
                new MCvPoint3D32f(60, 60, 0)
            };
        double[] koef = null;
        double[] koef_x = null;
        double[] koef_y = null;
        Scanner scanner;
        #endregion

        public MainScanningForm()
        {
            //NumberFormatInfo nfi = new CultureInfo("ru-RU", false).NumberFormat;
            //nfi.NumberDecimalSeparator = ".";
            //nfi.
            InitializeComponent();
            init_vars();

            //Manipulator.calcRob();

            var vals_regr = new double[][]//laser and pos
                {
                      new double[] {38.2 ,0 },
                      new double[] {41.2, -2 },
                      new double[] {44.8, -4},
            };
            koef = Regression.regression(vals_regr, 2);
            //var cur = Regression.calcPolynSolv(koef, 37);
            // Console.WriteLine("test_regr "+cur);

            vals_regr = new double[][]   //pos and y
                {
                      new double[] {30 ,364},
                      new double[] {32 ,356},
                      new double[] {34, 349 },
            };
            koef_y = Regression.regression(vals_regr, 1);

            vals_regr = new double[][]     //pos and x
                  {
                      new double[] {30 ,364},
                      new double[] {32 ,356},
                      new double[] {34, 349 },
              };
            koef_x = Regression.regression(vals_regr, 1);

            vals_regr = new double[][]     //pos and x
                 {
                      new double[] {16 ,359},
                      new double[] {41 ,378},
                      new double[] {44 ,380},
                      new double[] {24, 365},
             };
            koef_term_to_byte = Regression.regression(vals_regr, 1);

            vals_regr = new double[][]     //pos and x
                 {
                      new double[] {359 ,16},
                      new double[] {378 ,41},
                      new double[] {380 ,44},
                      new double[] {365, 24},
             };
            koef_byte_to_term = Regression.regression(vals_regr, 1);

            var vals_regr_k1 = new double[][]     //pos and las
                  {
                      new double[] { 566.5,1, },
                      new double[] { 609.1,9, },
                      new double[] { 643.5 ,17, },
              };
            var pos1 = 1;

            var vals_regr_k2 = new double[][]     //pos and las
                  {
                      new double[] { 588.5,9, },
                      new double[] { 623.5,17, },
                      new double[] { 655.6 ,25, },
              };
            var pos2 = 9;
            var vals_regr_k3 = new double[][]     //pos and las
                  {
                      new double[] { 606.9,17, },
                      new double[] { 638.4,25, },
                      new double[] { 668.9,33,  },
              };
            var pos3 = 17;
            koef_las_z_pos = comp_koef_las_z_pos(vals_regr_k1, pos1, vals_regr_k2, pos2, vals_regr_k3, pos3);
            //var pos = comp_cur_koef_las_z_pos(koefs_3d, 9, 623);
            //  Console.WriteLine("test cur3d: "+pos);


            /*var dx_las = 400;
            vals_regr = new double[][]//laser and pos
                  {
                      new double[] {164.4+ dx_las, 1 },
                      new double[] {168.8 + dx_las, 2},
                      new double[] {173.8 + dx_las, 3},
                       new double[] {179 + dx_las, 4 },

                      new double[] {184 + dx_las, 5 },
                      new double[] {189.3 + dx_las, 6},
                      new double[] {193.9 + dx_las, 7},
                       new double[] {198.6 + dx_las, 8 },

                      new double[] {203.3 + dx_las, 9 },
                      new double[] {208.5 + dx_las, 10},
                      new double[] {213.3 + dx_las, 11},
                       new double[] {218 + dx_las, 12 },

                      new double[] {222.8 + dx_las, 13 },
                      new double[] {227.8 + dx_las, 14},
                      new double[] {232.3 + dx_las, 15},
                       new double[] {237.3 + dx_las, 16}

              };*/

            vals_regr = new double[][]//laser and pos
            {
                      new double[] {565, 1 },
                      new double[] {604.3, 9},
                      new double[] {635, 17},

            };
            koef = Regression.regression(vals_regr, 1);
            //prin.t(vals_regr);
            //prin.t("x_____________-");
            var cur = Regression.calcPolynSolv(koef, 609);
           // Console.WriteLine("test_regr " + cur);

            vals_regr = new double[][]   //pos and x
                {
                      new double[] {1 ,550},
                      new double[] {8 ,570},
                      new double[] {16,590 },

            };
            koef_x = Regression.regression(vals_regr, 1);
           // prin.t(vals_regr);
            //prin.t("y_____________-");
            vals_regr = new double[][]     //pos and y
                  {
                      new double[] {1 ,149},
                      new double[] {2 ,153},
                      new double[] {3, 158 },
                      new double[] {4, 163 },

                      new double[] {5 ,169},
                      new double[] {6 ,175},
                      new double[] {7, 181 },
                      new double[] {8, 187 },

                      new double[] {9 ,192},
                      new double[] {10 ,198},
                      new double[] {11, 202 },
                      new double[] {12, 208 },

                      new double[] {13, 212 },
                      new double[] {14, 217 },
                      new double[] {15 ,222},
                      new double[] {16 ,227},
              };
            koef_y = Regression.regression(vals_regr, 2);
           // prin.t(vals_regr);
            //prin.t("_____________-");
            /* var p1 = new PosTimestamp(7000, 1);
             var p2 = new PosTimestamp(8000, 3);
             var p3 = PosTimestamp.betw(p1, p2,7500);
             int period = 3;

             var arr_a = new List<double>( new double[] {0,1,2,3,4,5,6,7,8,9});
             var st_ind = arr_a.Count / 2;
             var arr1 = arr_a.GetRange(st_ind, period);
             var arr2 = arr_a.GetRange(st_ind-period, period);
             Console.WriteLine("s");*/


            //Console.WriteLine(p3);
            //cur = Regression.calcPolynSolv(koef, 37);
            // Console.WriteLine( Point3d_GL.affil_p_seg(new Point3d_GL(1, 1, 1), new Point3d_GL(4, 4, 4), new Point3d_GL(0.999, 0.999, 0.999)));

            //VideoAnalyse.photo_from_video("vid//1.mp4");
            //comp_pores("rats\\2_1.png");
            //comp_pores("rats\\2_2.png");
            //comp_pores("rats\\2_3.png");
            //comp_pores("rats\\3_1.png");
            //comp_pores("model_mesh\\1.jpg");

            //var mat_l = new Mat("pict\\arm_clear.jpg");
            // Detection.detectLineDiff_debug(mat_l, scanner_config);

            //var im_las = new Image<Bgr, byte>("test_las_scan_table_model6.png");
            //CvInvoke.Imshow("im1", im_las);
            //UtilOpenCV.takeLineFromMat(im_las, 1);

            // test_basis();
            UtilOpenCV.generateImage_chessboard_circle(6, 7, 100);
            // load_camers_v2();

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
            //analys_sph();
            //
            //test_detect_spher();
            // test_matr();
            //comp_pores_ext("test_pores.png");
            //comp_pores_ext("test_pores_2.png");

            //comp_pores_ext_folder("hydro_mod");
            //VideoAnalyse.noise_analyse("noise.avi");
            //roi_for_ims("delt_ims");
            //   var im1 = new Mat("1.png");
            //CvInvoke.Imshow("asd", im1);
            //CvInvoke.WaitKey();
            // get_x_line_gray(im1, im1.Height / 2);

            //test_handeye();


            //var data = Analyse.parse_data_txt("data_sing.txt");
            //Analyse.norm_align_data(data);

            /* var frs = new RobotFrame[]
           {
                 new RobotFrame(-549.6832, 231.2491, 164.4913, -1.5754, -0.6576, 1.5666),
                 new RobotFrame( -345.6098, 217.2007, 163.8482, -1.5752, -0.6579, 1.567),
                 new RobotFrame(  -346.2069, 348.3778, 174.2129, -1.5754, -0.6574, 1.5664),
           };

             var model = new RobotFrame(-549.6832, 231.2491, 164.4913, 0.0789, -0.0023, -0.0688).getMatrix();
             //var model_inv = UtilMatr.to_inv_rot_matrix(model);
             CvInvoke.Invert(model, model, DecompMethod.LU);
             var m1 = new RobotFrame(model * frs[0].getMatrix());
             var m2 = new RobotFrame(model * frs[1].getMatrix());
             var m3 = new RobotFrame(model * frs[2].getMatrix());
             Console.WriteLine("ps");
             Console.WriteLine(m1.ToStr());
             Console.WriteLine(m2.ToStr());
             Console.WriteLine(m3.ToStr());*/
            /*
                        var fl1 = new List<Flat3d_GL>();
                        for(int i = 0; i <100;i++)
                        {
                            fl1.Add(new Flat3d_GL(i));
                        }
                        fl1 = Flat3d_GL.gaussFilter_v2(fl1.ToArray(), 40).ToList();
                        for (int i = 0; i < 100; i++)
                        {
                            Console.WriteLine(fl1[i]);
                        }*/
            //  Console.WriteLine(Math.Sin(90));
            // StereoCamera.calcSizesScanner(50,30, 100);

            //  var ps = new Point3d_GL[] { new Point3d_GL(0, 0, 0), new Point3d_GL(10, 0, 0), new Point3d_GL(10, 10, 0), new Point3d_GL(0, 10, 0) };

            // var ps_u = PathPlanner.unif_dist(ps.ToList(), 4.05);
            //   Console.WriteLine("sf");

            /* var im_patt = new Mat("2v2.png");
              var ps = new System.Drawing.PointF[3];
             // CvInvoke
             //CvInvoke.CvtColor(im_patt, im_patt, ColorConversion.Bgr2Gray);
             CvInvoke.WarpAffine(im_patt, im_patt, CameraCV.affinematr(1.9,1,1000),new Size(2000,2000));
             CvInvoke.Imshow("im_patt", im_patt);
             CvInvoke.WaitKey();
             var find = FindCircles.findCircles(im_patt,ref ps, new Size(6, 7));

             //var find = GeometryAnalyse.findCirclesIter(im_patt.Clone(), ref ps, new Size(6, 7));
             CvInvoke.Imshow("find",find );
             CvInvoke.WaitKey();
             */
          
        }

        #region something
        double[][] comp_koef_las_z_pos(double[][] vals1, double pos1, double[][] vals2, double pos2, double[][] vals3, double pos3)
        {
            var poses = new double[] { pos1, pos2, pos3 };
            var degree = 1;
            var degree_y = 1;
            var koefs_1 = Regression.regression(vals1, degree);
            var koefs_2 = Regression.regression(vals2, degree);
            var koefs_3 = Regression.regression(vals3, degree);
            var koefs = new double[][] { koefs_1, koefs_2, koefs_3 };
            var list_ks_3d = new List<double[]>();
            for (int i = 0; i < koefs_1.Length; i++)
            {
                var vals_cur = new double[][]
                {
                    new double[]{ pos1, koefs_1[i] },
                    new double[]{ pos2, koefs_2[i] },
                    new double[]{ pos3, koefs_3[i] },
                };
                var koefs_cur = Regression.regression(vals_cur, degree_y);
                list_ks_3d.Add(koefs_cur);
            }
            return list_ks_3d.ToArray();
        }

        double comp_cur_koef_las_z_pos(double[][] koefs, double koord_cur, double las)
        {
            var list_las = new List<double>();
            for (int i = 0; i < koefs.Length; i++)
            {
                var cur_koef_las = Regression.calcPolynSolv(koefs[i], koord_cur);
                list_las.Add(cur_koef_las);
            }

            var cur = Regression.calcPolynSolv(list_las.ToArray(), las);
            return cur;
        }

        void test_handeye()
        {
            var ms_rob = new List<Matrix<double>>();
            var ms_cam = new List<Matrix<double>>();

            var ms_gb = new List<Matrix<double>>();
            var ms_tc = new List<Matrix<double>>();

            var bt = new RobotFrame(-120, -350, 75, 0, 0, 0.5);
            //var bm = new RobotFrame(20, 0, 0, 0, 0, -3);

            var gc = new RobotFrame(60, 60, 50, 0, 0.2, 0.3);

            var rpos_2 = new List<RobotFrame>();

            var frms = FrameLoader.load_rob_frames(@"C:\Users\1\source\repos\Scanner3dLaser_with_gl\opengl3\bin\x64\Debug\cam1\fl_cal_kuka_1307_2\1");



            var gbs = new List<RobotFrame>
            {
                new RobotFrame(100,0,200,0,0.7,0),
                new RobotFrame(100,0,200,1,0.5,0),
                new RobotFrame(100,0,200,0.2,1,0),
                 new RobotFrame(0,0,210,0,0,0.5),
                new RobotFrame(0,0,200,0,0,0),
                new RobotFrame(0,0,200,1,0,0),
                new RobotFrame(0,0,200,0,1,0),
                 new RobotFrame(0,0,210,0,0,0.5),
                new RobotFrame(0,0,220,1,0,0.4),
                new RobotFrame(0,0,230,0,1,0.3),
                new RobotFrame(0,0,200,0,0,1),
                new RobotFrame(10,0,200,2,0,1),
                new RobotFrame(0,10,200,0,2,1),
                new RobotFrame(0,0,210,0,0,2),
            };


            gbs = new List<RobotFrame>();
            for (int i = 0; i < frms.Length; i++)
            {
                gbs.Add(frms[i].RobotFrame);
            }



            for (int i = 0; i < gbs.Count; i++)
            {
                var gb_i = gbs[i].Clone();
                var gb = gb_i.getMatrix();
                var bg = gb_i.getMatrix();

                var bt_inv = bt.Clone().getMatrix();
                var gc_inv = gc.Clone().getMatrix();
                CvInvoke.Invert(bt.getMatrix(), bt_inv, DecompMethod.LU);
                CvInvoke.Invert(gc.getMatrix(), gc_inv, DecompMethod.LU);
                CvInvoke.Invert(gb.Clone(), bg, DecompMethod.LU);


                var bt_m = bt.getMatrix();


                ms_rob.Add(bg);
                //  ms_cam.Add(bt_m * gb *  gc_inv);
                var cam_matr = gc_inv * gb * bt_m;
                var rand = new Matrix<double>(4, 4);
                rand.SetRandNormal(new MCvScalar(2), new MCvScalar(2));
                for (int k = 0; k < 4; k++)
                {
                    for (int w = 0; w < 3; w++)
                    {
                        rand[k, w] *= 0.0001;
                    }
                }
                prin.t("cam_matr");
                prin.t(cam_matr);
                prin.t("rand");
                prin.t(rand);

                cam_matr += rand;
                prin.t("cam_matr+");
                prin.t(cam_matr);
                ms_cam.Add(cam_matr);



                /* prin.t("bt_inv");
                 prin.t(bt_inv);
                 prin.t("_________________");

                 prin.t("bg_inv");
                 prin.t(bg_inv);
                 prin.t("_________________");

                 prin.t("gc_inv");
                 prin.t(gc_inv);
                 prin.t("_________________");*/
                // var cam2 =  bt_inv.getMatrix() * rpos[i].getMatrix() * gc.getMatrix();
                prin.t(ms_rob[i]);
                prin.t(ms_cam[i]);


                //prin.t(cam2);
                prin.t("_________");
            }


            VectorOfMat mr_r, mr_t, mc_r, mc_t;
            (mr_r, mr_t) = UtilOpenCV.to_vec_mat(ms_rob.ToArray());
            (mc_r, mc_t) = UtilOpenCV.to_vec_mat(ms_cam.ToArray());
            Mat mr = new Mat();
            Mat mt = new Mat();
            CvInvoke.CalibrateHandEye(mr_r, mr_t, mc_r, mc_t, mr, mt, HandEyeCalibrationMethod.Horaud);
            prin.t("res:s");
            prin.t(mr);
            prin.t(mt);
        }
        void comp_pores(string path)
        {
            var im1 = new Mat(path);
            var im1_c = im1.Clone();
            CvInvoke.Imshow("orig", im1_c);
            CvInvoke.CvtColor(im1, im1, ColorConversion.Bgr2Gray);
            //get_x_line_gray(im1, im1.Height / 2);
            //Console.WriteLine("________________");
            CvInvoke.GaussianBlur(im1, im1, new Size(11, 11), 6);
            //get_x_line_gray(im1, im1.Height / 2);
            //CvInvoke.Threshold(im1, im1, 150, 255,ThresholdType.BinaryInv);
            var mat2 = new Mat();
            CvInvoke.Imshow("gg", im1);
            CvInvoke.AdaptiveThreshold(im1, mat2, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 9, 1);
            CvInvoke.Imshow("ad_thr", mat2);
            Mat kernel7 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(7, 7), new Point(1, 1));

            Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(1, 1));
            Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            Mat kernel2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 2), new Point(1, 1));
            Mat ellips7 = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(7, 7), new Point(1, 1));
            CvInvoke.MorphologyEx(mat2, mat2, MorphOp.Dilate, kernel2, new Point(-1, -1), 2, BorderType.Default, new MCvScalar());
            CvInvoke.Imshow("m_ex", mat2);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(mat2, contours, hier, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);

            contours = FindCircles.size_filter(contours, 50, 300);

            CvInvoke.DrawContours(im1_c, contours, -1, new MCvScalar(255, 0, 0), 1, LineType.EightConnected);
            //CvInvoke.Imshow("m_ex", mat2);
            for (int i = 0; i < contours.Size; i++)
            {
                var area = CvInvoke.ContourArea(contours[i]);
                if (area > 50)
                {
                    Console.WriteLine(area);
                }
            }
            Console.WriteLine("____________");
            CvInvoke.Imshow(path + "m", mat2);
            CvInvoke.Imshow(path, im1_c);
        }
        void roi_for_ims(string path)
        {
            var files = Directory.GetFiles(path);
            var roi = new Rectangle(222, 362, 912, 149);
            for (int i = 0; i < files.Length; i++)
            {
                var mat = new Mat(files[i]);
                var name = Path.GetFileNameWithoutExtension(files[i]);
                var mat_roi = new Mat(mat, roi);
                Directory.CreateDirectory("roi");
                mat_roi.Save("roi\\" + name + ".png");
            }
        }
        void comp_pores_ext_folder(string path)
        {
            var ks = new double[] { 0.25, 0.5, 0.75 };
            var ws = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 1.0, 1.2 };

            var files = FrameLoader.sortByDate(Directory.GetFiles(path));

            var kws_pores = new double[ks.Length, ws.Length];

            var kws_dev = new double[ks.Length, ws.Length];

            for (int i = 0; i < files.Length; i++)
            {
                // Console.WriteLine( File.GetLastWriteTime(files[i]));
                var p = comp_pores_ext(files[i]);
                kws_pores[ks_i, ws_i] = p.x;
                kws_dev[ks_i, ws_i] = p.y;
                Console.WriteLine(files[i] + " " + p);
                ws_i++;
                if (ws_i >= ws.Length)
                {
                    ks_i++;
                    ws_i = 0;
                }
                if (ks_i >= ks.Length)
                {
                    ks_i = 0;
                }
            }


            for (int w_i = 0; w_i < ws.Length; w_i++)
            {
                Console.WriteLine(kws_pores[0, w_i] + " " + kws_pores[1, w_i] + " " + kws_pores[2, w_i]);
            }
            Console.WriteLine("____");
            for (int w_i = 0; w_i < ws.Length; w_i++)
            {
                Console.WriteLine(kws_dev[0, w_i] + " " + kws_dev[1, w_i] + " " + kws_dev[2, w_i]);
            }


        }
        Point3d_GL comp_pores_ext(string path)
        {
            var im1 = new Mat(path);
            var size = new Size(400, 600);
            im1 = new Mat(im1, new Rectangle(360, 270, size.Width, size.Height));
            var im1_c = im1.Clone();
            CvInvoke.CvtColor(im1, im1, ColorConversion.Bgr2Gray);
            var mat2 = new Mat();
            CvInvoke.Threshold(im1, mat2, 50, 255, ThresholdType.BinaryInv);
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(mat2, contours, hier, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            var conts = FindCircles.sameContours(contours, 0.9, 9);
            CvInvoke.DrawContours(im1_c, conts, -1, new MCvScalar(255, 0, 0), 1, LineType.EightConnected);
            //CvInvoke.Imshow(path, im1_c);
            //CvInvoke.WaitKey();
            //CvInvoke.Imshow(path, im1_c);
            return analyse_contours(conts, size);
        }

        Point3d_GL analyse_contours(VectorOfVectorOfPoint conts, Size size)
        {
            var ps = Point3d_GL.toPoints(FindCircles.findCentres(conts));
            var ps_or = Point3d_GL.order_points(ps);
            var ps_dist = Point3d_GL.dist_betw_ps(ps_or);
            //prin.t(ps_dist,"\n");

            var m_s_dist = UtilOpenCV.mean_std_dev(ps_dist);
            var mean_dist = m_s_dist.V0;
            var dev_dist = m_s_dist.V1;
            var w_i = (size.Width / mean_dist);
            var h_i = (size.Height / mean_dist);
            var len_ps_all = w_i * h_i;
            var pores = ps.Length / (double)len_ps_all;

            var areas = new double[ps.Length];
            for (int i = 0; i < conts.Size; i++)
            {
                var area = CvInvoke.ContourArea(conts[i]);
                areas[i] = area;
            }
            var m_s_area = UtilOpenCV.mean_std_dev(areas);
            var mean_area = m_s_area.V0;
            var dev_area = m_s_area.V1;
            if (pores > 1) pores = 1;
            return new Point3d_GL(pores, dev_area / mean_area);
        }



        void get_x_line_gray(Mat mat, int y)
        {
            var im = mat.ToImage<Gray, byte>();
            var im2 = im.Clone();
            CvInvoke.GaussianBlur(im, im2, new Size(scanner_config.gauss_kern, scanner_config.gauss_kern), -1);
            for (int x = 0; x < im.Width; x++)
            {
                Console.WriteLine(x + " " + im.Data[y, x, 0] + " " + im2.Data[y, x, 0]);
            }
        }

        void test_matr()
        {
            var name = "-353.9612 164.1644 109.397 0.0939 0.0589 0.0098 ";
            var rob_pos = new RobotFrame(name);
            var r1 = rob_pos.getMatrix();
            prin.t(r1);
            var fr = new RobotFrame(r1, RobotFrame.RobotType.PULSE);
            prin.t(fr.ToStr());
        }

        void test_detect_spher()
        {
            var im_las = new Mat("s6.jpg");
            CvInvoke.GaussianBlur(im_las, im_las, new Size(29, 29), -1);
            CvInvoke.Resize(im_las, im_las, new Size(400, 400));

            //CvInvoke.Threshold(im_las, im_las, 60, 255, ThresholdType.BinaryInv);
            //CvInvoke.CvtColor(im_las,im_las,ColorConversion.Bgr2Gray);
            //CvInvoke.AdaptiveThreshold(im_las, im_las, 255, AdaptiveThresholdType.GaussianC, ThresholdType.BinaryInv, 11, 1);
            //im_las = FindCircles.sobel_mat(im_las);
            //CvInvoke.Threshold(im_las, im_las, 50, 255, ThresholdType.Binary);
            //CvInvoke.Imshow("im1", im_las);
            imageBox1.Image = im_las.Clone();

        }


        private void timer1_Tick(object sender, EventArgs e)
        {
            label_timer.Text = DateTime.Now.Second + " : " + DateTime.Now.Millisecond;// +" "+ DateTime.Now.Ticks/ TimeSpan.TicksPerMillisecond;
            lab_fps_cam1.Text = fps1.ToString();
        }
        void init_vars()
        {


            #region important




            combo_improc.Items.AddRange(new string[] { "Распознать шахматный паттерн", "Стерео Исп", "Паттерн круги", "Датчик расст", "св Круги грид", "Ничего" });
            combo_robot_ch.Items.AddRange(new string[] { "Pulse", "Kuka" });

            cameraDistortionCoeffs_dist[0, 0] = -0.3;
            for (int i = 0; i < mat_global.Length; i++)
            {
                mat_global[i] = new Mat();
            }

            radioButton_dynamic_surface.Checked = true;
            if (comboImages.Items.Count > 0)
            {
                comboImages.SelectedIndex = 0;
            }
            comboBox_syrenge_size.Items.AddRange(syringe_size_text);

            if (comboBox_syrenge_size.Items.Count > 0)
            {
                comboBox_syrenge_size.SelectedIndex = 0;
            }
            textBoxes_Persp = new TextBox[]
            {
                textBoxK_0,textBoxK_1,textBoxK_2,
                textBoxK_3,textBoxK_4,textBoxK_5,
                textBoxK_6,textBoxK_7,textBoxK_8,
            };

            // patt = UtilOpenCV.generateImage_chessboard(chess_size.Width, chess_size.Height, 200);
            // patt = UtilOpenCV.generateImage_chessboard(10, 11, 200);
            #endregion
            imb_base = new ImageBox[] { imBox_base_1, imBox_base_2 };
            imb_main = new ImageBox[] { imageBox1, imageBox2 };
            minArea = 1.0 * k * k * 15;
            maxArea = 15 * k * k * 250;
            red_c = 252;

            checkBox_window_auto.Checked = true;

            //var model_mesh = STLmodel.parsingStl_GL4(@"curve_test_asc.STL");
            //GL1.addMesh(model_mesh, PrimitiveType.Triangles);

            traj_config = new TrajParams
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


            patt_config = new PatternSettings
            {
                step = 2,
                angle = 0,
                angle_layers = PI / 2,
                min_dist = 0.1,
                arc_dist = 2,
                r = 2,
                start_dir_r = true,
                patternType = PathPlanner.PatternType.Lines,
                dim_x = 10,
                dim_y = 10,
                filling = 0.7
            };

            scanner_config = new ScannerConfig
            {
                board = 0,
                orig = false,
                reverse = false,
                rotate = true,
                threshold = 20,
                wind_regr = 7,

                buff_delt = 10,
                distort = true,
                save_im = false,
                smooth = 1,
                strip = 3,
                syncr = true,
                gauss_kern = 7
            };

            (scanner_config, traj_config, patt_config) = formSettings.load_confs();


            prop_gr_scan.SelectedObject = scanner_config;
            propGrid_pattern.SelectedObject = patt_config;
            propGrid_traj.SelectedObject = traj_config;
            // if(scanner_config.pos_laser_compens == null)
            // textBox_compens_las_pos.Text = scanner_config.pos_laser_compens.ToString();

            prop_gr_light.SelectedObject = GL1.lightSources_obj;
            debugBox.Text = "0.3 0.3 1";

            //scan_dist = ch_b_dist.Checked;
            // scan_sync = ch_b_sync.Checked;

            tree_models.CheckBoxes = true;
            load_camers_v2();

            /*var m_test = new Mat("test_ph.jpg");
            var fr = new Frame(m_test, "sdf", FrameType.MarkBoard);
            CameraCV.findPoints(fr, new Size(9, 10));*/

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

        void load_frms(string path)
        {
            var frms = FrameLoader.loadImages_test(path);
            foreach (var m in frms) {
                m.im *= 8;
                m.im.Save(m.name + "_x8.png");
            }
            comboImages.Items.AddRange(frms);

        }

        void test_cross_triag()
        {
            double l = 10;
            var p1 = new Point3d_GL(l, 0, 0);
            var p2 = new Point3d_GL(0, l / 2, 0);
            var p3 = new Point3d_GL(0, 0, l / 2);

            var p4 = new Point3d_GL(0, 0, 0);
            var p5 = new Point3d_GL(l / 3, l, l);
            var p6 = new Point3d_GL(l, 0, l);

            var t1 = new Polygon3d_GL(p1, p2, p3);
            var t2 = new Polygon3d_GL(p4, p5, p6);

            var ps = Polygon3d_GL.cross_triang(t1, t2);

            GL1.addPointMesh(ps, new Color3d_GL(1));
            var triag = GL1.addMesh(Polygon3d_GL.toMesh(new Polygon3d_GL[] { t1, t2 })[0], PrimitiveType.Triangles);
            GL1.buffersGl.setTranspobj(triag, 0.3f);
        }

        #endregion

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
                    GL1.addMesh(mesh_scan_stl, PrimitiveType.Triangles, new Color3d_GL(normrgb[0], normrgb[1], normrgb[2]));
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
            markSize = 10f;//6.2273f//10f//9.6f
            chess_size = new Size(6, 7);//new Size(10, 11);//new Size(6, 7)
            var frms_1 = FrameLoader.loadImages_diff(@"cam1\camera_cal_2012", FrameType.Pattern, PatternType.Mesh);
            var cam1 = new CameraCV(frms_1, chess_size, markSize, null);
            cam1.save_camera("sing_cam1_2012a.txt");
            comboImages.Items.AddRange(frms_1);
            cameraCVcommon = cam1;
            /* markSize = 6.2273f;//6.2273f
             chess_size = new Size(10, 11);//new Size(10, 11);
             var frms_2 = FrameLoader.loadImages_diff(@"cam2\cam2_cal_190623_2", FrameType.Pattern, PatternType.Mesh);
             var cam2 = new CameraCV(frms_2, chess_size, markSize, null);
             cam2.save_camera("cam2_conf_190623_test.txt");
             comboImages.Items.AddRange(frms_2);*/
        }
        Scanner loadScanner_v2(string conf1, string conf2, string stereo_cal, string bfs_file = null)
        {
            var cam1 = CameraCV.load_camera(conf1);
            var cam2 = CameraCV.load_camera(conf2);
            Scanner scanner;
            if (bfs_file == null)
            {
                scanner = new Scanner(new CameraCV[] { cam1, cam2 });
            }
            else
            {
                var stereo_cam = new StereoCamera(new CameraCV[] { cam1, cam2 }, bfs_file);
                scanner = new Scanner(stereo_cam);
                stereocam_scan = stereo_cam;
            }
            chess_size = new Size(6, 7);
            var marksize = 10f;// 9.6f;// 10f;
            var stereo_cal_1 = stereo_cal.Split('\\').Reverse().ToArray()[0];
            var frms_stereo = FrameLoader.loadImages_stereoCV(@"cam1\" + stereo_cal_1, @"cam2\" + stereo_cal_1, FrameType.Pattern, scanner_config.rotate_cam);
            scanner.initStereo(new Mat[] { frms_stereo[0].im, frms_stereo[0].im_sec }, PatternType.Mesh, chess_size, marksize);

            //comboImages.Items.AddRange(frms_stereo);
            comboImages.BeginInvoke((MethodInvoker)(() => comboImages.Items.AddRange(frms_stereo)));
            return scanner;
        }
        Scanner load_scan_v2(Scanner scanner, string scan_path, ScannerConfig config)
        {

            var scan_path_1 = scan_path.Split('\\').Reverse().ToArray()[0];
            //
            if (config.fast_load)
            {
                if (scanner_config.syncr)
                    scanner = VideoAnalyse.loadVideo_stereo(scan_path_1, scanner, config, this);
                else
                    scanner = VideoAnalyse.loadVideo_stereo_not_sync(scan_path_1, scanner, config, this);
            }
            else
            {
                scanner = VideoAnalyse.video_delt_bf(scan_path_1, scanner, config, this);
                //scanner = VideoAnalyse.video_delt(scan_path_1, scanner, config, this,33);
            }

            if (config.load_3d)
            {
                var mesh = Polygon3d_GL.triangulate_lines_xy(scanner.getPointsLinesScene(), -1);
                var scan_stl = Polygon3d_GL.toMesh(mesh);
                //mesh = GL1.addNormals(mesh, 1);
                this.scanner = scanner;
                if (scan_stl != null)
                {
                    // comboImages.BeginInvoke((MethodInvoker)(() => comboImages.Items.AddRange(frms_stereo)));
                    //this.BeginInvoke((MethodInvoker)(() => GL1.add_buff_gl(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Triangles, scan_path_1)));
                    if (GL1.buffersGl.objs.Keys.Contains(scan_i))
                        GL1.buffersGl.removeObj(scan_i);
                    scan_i = GL1.add_buff_gl(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Triangles, scan_path_1);
                }
                smooth_mesh(scan_i, config.smooth);
                // if (scan_stl != null) scan_i = GL1.add_buff_gl_dyn(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Points);
            }
            Console.WriteLine("Loading end.");
            return scanner;

        }

        void smooth_mesh(string mesh_id, double rad)
        {

            var polygs = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[mesh_id].vertex_buffer_data, GL1.buffersGl.objs[mesh_id].color_buffer_data);
            var polyg_sm = RasterMap.smooth_mesh(polygs, rad);
            //polyg_sm = GL1.addNormals(polyg_sm, 0.5);
            var mesh = Polygon3d_GL.toMesh(polyg_sm);
            GL1.remove_buff_gl_id(mesh_id);
            GL1.add_buff_gl(mesh[0], mesh[1], mesh[2], PrimitiveType.Triangles, mesh_id);
        }

        void unwrap_mesh(string mesh_id)
        {
            var radius = r_cyl;
            var polygs = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[mesh_id].vertex_buffer_data, GL1.buffersGl.objs[mesh_id].color_buffer_data);
            var mesh_ind = new IndexedMesh(polygs);
            var ps = mesh_ind.ps_uniq;


            for (int i = 0; i < ps.Length; i++)
            {
                ps[i] = unwrap_p(ps[i].Clone(), radius);
            }
            mesh_ind.ps_uniq = ps;
            var mesh = Polygon3d_GL.toMesh(mesh_ind.get_polygs());
            GL1.remove_buff_gl_id(mesh_id);
            GL1.add_buff_gl(mesh[0], mesh[1], mesh[2], PrimitiveType.Triangles, mesh_id);
        }
        static Matrix<double> tranp_rot(Matrix<double> m)
        {
            var m_r = m.Clone();
            var x = m_r[0, 3];
            var y = m_r[1, 3];
            var z = m_r[2, 3];
            m_r = m_r.Transpose();
            m_r[0, 3] = x;
            m_r[1, 3] = y;
            m_r[2, 3] = z;
            m_r[3, 0] = 0;
            m_r[3, 1] = 0;
            m_r[3, 2] = 0;
            m_r[3, 3] = 1;
            return m_r;
        }
        void wrap_mesh(string mesh_id)
        {
            var radius = r_cyl;
            var polygs = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[mesh_id].vertex_buffer_data, GL1.buffersGl.objs[mesh_id].color_buffer_data);
            var mesh_ind = new IndexedMesh(polygs);
            var ps = mesh_ind.ps_uniq;

            for (int i = 0; i < rob_traj.Count; i++)
            {
                //GL1.addFrame(rob_traj[i], 2, "sda");
            }
            ps = wrap_ps(ps, radius);
            Console.WriteLine("off_cyl: " + off_cyl);
            mesh_ind.ps_uniq = Point3d_GL.add_arr(mesh_ind.ps_uniq, off_cyl);
            mesh_ind.ps_uniq = Point3d_GL.multMatr_p_m(m_cyl, mesh_ind.ps_uniq);
            var mesh = Polygon3d_GL.toMesh(mesh_ind.get_polygs());
            GL1.remove_buff_gl_id(mesh_id);
            GL1.add_buff_gl(mesh[0], mesh[1], mesh[2], PrimitiveType.Triangles, mesh_id);


            rob_traj = wrap_ms(rob_traj.ToArray(), radius).ToList();
            for (int i = 0; i < rob_traj.Count; i++)
            {
                //GL1.addFrame(rob_traj[i], 2, "sda");
            }
            for (int i = 0; i < rob_traj.Count; i++)
            {
                //rob_traj[i]
                rob_traj[i][0, 3] += off_cyl.x;
                rob_traj[i][2, 3] += off_cyl.z;
                rob_traj[i] = m_cyl * rob_traj[i].Clone();
            }
            for (int i = 0; i < rob_traj.Count; i++)
            {
                //GL1.addFrame(rob_traj[i], 2, "sda");
            }


            var ps_traj = PathPlanner.matr_to_ps(rob_traj);

            var traj_rob = PathPlanner.generate_robot_traj(rob_traj, RobotFrame.RobotType.PULSE, traj_config, GL1);
            debugBox.Text = RobotFrame.generate_string(traj_rob.ToArray());
            GL1.remove_buff_gl_id(traj_i);
            traj_i = GL1.addLineMeshTraj(ps_traj.ToArray(), new Color3d_GL(0.9f), "gen_traj");

            /*  var ps_traj = Point3d_GL.fromMesh(GL1.buffersGl.objs[traj_i].vertex_buffer_data);
              ps_traj = wrap_ps(ps_traj, radius);
              ps_traj = Point3d_GL.multMatr(ps_traj, m_cyl);*/


        }

        static Point3d_GL unwrap_p(Point3d_GL p, double r_c)
        {
            var x = p.x;
            var z = p.z;
            var r = Math.Sqrt(x * x + z * z);
            var thetta = Math.Sign(x) * Math.Acos(z / r);
            var p_c = p.Clone();
            p_c.z = r;
            p_c.x = thetta * r_c;
            return p_c;

        }


        static Point3d_GL wrap_p(Point3d_GL p, double r_c)
        {
            var thetta_ = p.x;
            var r = p.z;
            var p_c = p.Clone();
            var thetta = thetta_ / r_c;
            var x = r * Math.Sin(thetta);
            var z = r * Math.Cos(thetta);
            p_c.x = x;
            p_c.z = z;


            return p_c;

        }
        static Matrix<double> wrap_m(Matrix<double> m, double r_c)
        {
            var thetta_ = m[0, 3];
            var r = m[2, 3];
            var m_c = m.Clone();
            var thetta = thetta_ / r_c;
            var x = r * Math.Sin(thetta);
            var z = r * Math.Cos(thetta);
            var y = m[1, 3];
            var m_ry = RobotFrame.RotYmatr(thetta);
            m_c = m_ry * m;
            m_c[0, 3] = x;
            m_c[1, 3] = y;
            m_c[2, 3] = z;
            return m_c;

        }
        static Matrix<double>[] wrap_ms(Matrix<double>[] ms, double r_c)
        {

            for (int i = 0; i < ms.Length; i++)
            {
                ms[i] = wrap_m(ms[i].Clone(), r_c);
            }
            return ms;
        }
        static Point3d_GL[] wrap_ps(Point3d_GL[] ps, double r_c)
        {

            for (int i = 0; i < ps.Length; i++)
            {
                ps[i] = wrap_p(ps[i].Clone(), r_c);
            }
            return ps;
        }

        //----------------------------------------------------------------------------------------------------------
        Scanner loadScanner_sing(string conf1, string laser_line = null)
        {
            var cam1 = CameraCV.load_camera(conf1);
            cam1.scanner_config = scanner_config;
            LinearAxis line = null;
            if (laser_line != null) line = new LinearAxis(laser_line);
            var scanner = new Scanner(cam1, line);
            var stereo = new StereoCamera(new CameraCV[] { cam1, cam1 });
            scanner.stereoCamera = stereo;
            this.scanner = scanner;

            return scanner;
        }
        void load_scan_sing(Scanner scanner, string scan_path, int strip = 1, double smooth = -1)
        {
            var scan_path_1 = scan_path.Split('\\').Reverse().ToArray()[0];
            if (scanner_config.fast_load) scanner = VideoAnalyse.loadVideo_sing_cam(scan_path_1, this, scanner, scanner_config);
            else scanner = VideoAnalyse.loadVideo_sing_cam_move(scan_path_1, this, scanner, scanner_config);
            var ps = scanner.getPointsLinesScene();
            // foreach(var line in ps) GL1.addLineMeshTraj(line);  
            var mesh = Polygon3d_GL.triangulate_lines_xy(ps, smooth);
           var scan_stl = Polygon3d_GL.toMesh(mesh);
            scan_i = GL1.add_buff_gl(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Triangles, scan_path_1);


        }

        void load_calib_sing(Scanner scanner, string scan_path, int strip = 1, double smooth = -1)
        {

            var scan_path_1 = scan_path.Split('\\').Reverse().ToArray()[0];
            scanner = VideoAnalyse.loadVideo_sing_cam(scan_path_1, this, scanner, scanner_config, true);
            // scanner.linearAxis.save("linear.txt");
            if (scanner.pointCloud.points3d_lines == null) return;
            if (scanner.pointCloud.points3d_lines.Count == 0) return;
            var ps = scanner.getPointsLinesScene();
             foreach(var line in ps) GL1.addLineMeshTraj(line);  
            /*var mesh = Polygon3d_GL.triangulate_lines_xy(ps, smooth);
            var scan_stl = Polygon3d_GL.toMesh(mesh);
            scan_i = GL1.add_buff_gl(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Triangles, "scan_sing");*/
        }

        //-------------------------------------------------------------------------------------------
        void loadScannerStereoLas(
            string[] cam_cal_path,
            string stereo_cal_path,
            string scand_path,
            float[] normrgb, bool undist, int strip)
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

            scanner.initStereo(new Mat[] { frms_stereo[0].im, frms_stereo[0].im_sec }, PatternType.Mesh, chess_size, 10f);

            VideoAnalyse.loadVideo_stereo(scand_path, scanner, scanner_config, this);
            var mesh = Polygon3d_GL.triangulate_lines_xy(scanner.getPointsLinesScene());
            var scan_stl = Polygon3d_GL.toMesh(mesh);
            GL1.add_buff_gl(scan_stl[0], scan_stl[1], scan_stl[2], PrimitiveType.Triangles);

            // GL1.addPointMesh(scanner.getPointsScene());
            //GL1.buffersGl.sortObj();
            //STLmodel.saveMesh(scan_stl[0], scand_path);
        }

        public float[] extract_delt(float[] mesh)
        {
            for (int i = 0; i < mesh.Length; i += 3)
            {
                mesh[i + 2] = 3 * comp_delt(mesh[i + 2]);
            }
            return mesh;
        }

        static float comp_delt(float val)
        {
            return val - ((float)Math.Round(val / 10)) * 10;
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
                scanning_status = true;
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
            int power_laser = 250;
            //int counts = Convert.ToInt32(boxN.Text);
            int counts = scanner_config.frames_n;
            string folder_scan = box_scanFolder.Text;
            var start_p = scanner_config.start_pos_scan;
            var stop_p = scanner_config.stop_pos_scan;
            Console.WriteLine("stp_p: " + start_p + " " + stop_p);
            //var p1_cur_scan = robFrameFromTextBox(nameX, nameY, nameZ, nameA, nameB, nameC);
            //var p2_cur_scan = robFrameFromTextBox(nameX2, nameY2, nameZ2, nameA, nameB, nameC);
            var p1_cur_scan = new robFrame(start_p, 0, 0, 0, 0, 0);
            var p2_cur_scan = new robFrame(stop_p, 0, 0, 0, 0, 0);
            var fps = Convert.ToInt32(tB_fps_scan.Text);
            float x = (float)p1_cur_scan.x;

            var delx = (float)(p2_cur_scan.x - p1_cur_scan.x) / (float)counts;
            if (laserLine == null)
            {
                // initLaserFast();
                // Thread.Sleep(200);
            }
            laserLine?.laserOff();
            laserLine?.setPower(0);
            Thread.Sleep(300);
            var dir_scan = Path.Combine(Directory.GetCurrentDirectory(), "cam1\\" + folder_scan);
            scan_fold_path = dir_scan;
            //Console.WriteLine(dir_scan);
            makePhotoLaser(
                    new float[] { x },
                    new string[] { "cam1\\" + folder_scan + "\\orig", "cam2\\" + folder_scan + "\\orig" },
                    new ImageBox[] { imageBox1, imageBox2 }
                    );
            Thread.Sleep(200);



            if (typescan == 3)
            {
                var t_video = (double)counts / fps;
                var v_laser = (p2_cur_scan.x - p1_cur_scan.x) / t_video;
                laserLine?.setPower(power_laser);
                Thread.Sleep(2);
                laserLine?.setPower(power_laser);
                Thread.Sleep(2);
                laserLine?.setPower(power_laser);
                Thread.Sleep(2);
                laserLine?.setPower(power_laser);
                Thread.Sleep(100);
                Thread.Sleep(2);

                laserLine?.setShvpVel(200);
                Thread.Sleep(200);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p1_cur_scan.x);

                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p1_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p1_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p1_cur_scan.x);
                Thread.Sleep(4000);
                
                Console.WriteLine(v_laser + " v_las");
                Thread.Sleep(2);
                laserLine?.setShvpVel(v_laser);
                Thread.Sleep(2);
                laserLine?.setShvpVel(v_laser);
                Thread.Sleep(2);
                laserLine?.setShvpVel(v_laser);
                Thread.Sleep(2);
                laserLine?.setShvpVel(v_laser);
                Thread.Sleep(400);
                Thread.Sleep(2);
                startWrite(1, counts);
                startWrite(2, counts);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                sb_enc = new StringBuilder();

            }
            if (typescan == 8)
            {
                var t_video = (double)counts / fps;
                var v_laser = (p2_cur_scan.x - p1_cur_scan.x) / t_video;
                laserLine?.laserOn();
                Thread.Sleep(100);
                laserLine?.setShvpVel(2000);
                Thread.Sleep(200);

                laserLine?.setShvpPos((int)p1_cur_scan.x);
                Thread.Sleep(2000);
                startWrite_sam(0, counts);
                startWrite_sam(1, counts);
                Console.WriteLine(v_laser + " v_las");
                laserLine?.setShvpVel(v_laser);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                sb_enc = new StringBuilder();

            }
            if (typescan == 4)
            {


                var t_video = (double)counts / fps;
                var v_laser = Math.Abs((p2_cur_scan.x - p1_cur_scan.x) / t_video);
                

                laserLine?.laserOn();
                Thread.Sleep(200);
                //laserLine?.setShvpVel(200);
                Thread.Sleep(200);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p1_cur_scan.x);

                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p1_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p1_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p1_cur_scan.x);
                Thread.Sleep(4000);
                Thread.Sleep(2);
                laserLine?.setShvpVel(v_laser);
                Thread.Sleep(2);
                laserLine?.setShvpVel(v_laser);
                Thread.Sleep(2);
                laserLine?.setShvpVel(v_laser);
                Thread.Sleep(2);
                laserLine?.setShvpVel(v_laser);
                Thread.Sleep(400);
                laserLine?.send_pos_laser(1);
                Thread.Sleep(2);
                laserLine?.send_pos_laser(1);
                Thread.Sleep(2);
                startWrite_sam(0, counts+50);
                sb_enc = new StringBuilder();
                Thread.Sleep(200);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                Thread.Sleep(2);
                laserLine?.setShvpPos((int)p2_cur_scan.x);
                Thread.Sleep(200);

                laserLine?.setShvpPos((int)p2_cur_scan.x);

            }

            for (int i = 0; i < counts; i++)
            {

                if (typescan == 0)
                {
                    if (laserLine == null)
                    {
                        initLaserFast();
                        Thread.Sleep(200);
                    }
                    laserLine.laserOn();
                    makePhotoLaser(
                    new float[] { x },
                    new string[] { "cam1\\" + folder_scan, "cam2\\" + folder_scan },
                    new ImageBox[] { imageBox1, imageBox2 }
                    );
                }
                else if (typescan == 1)
                {
                    makeDoublePhotoLaser(
                    new float[] { x },
                    new string[] { "cam1\\" + folder_scan, "cam2\\" + folder_scan },
                    new ImageBox[] { imageBox1, imageBox2 }
                    );
                }
                else if (typescan == 2)
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
            DeviceArduino.find_ports(comboBox_portsArd);
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

                if (mats_def[i] != null && mats_las[i] != null)
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

        Button addButton(Control control, string name, string ass_name, Size offset)
        {
            var but1 = new Button();
            but1.Location = new Point(control.Location.X + offset.Width, control.Location.Y + offset.Height);
            but1.Size = new Size(30, 30);
            but1.AccessibleName = ass_name;
            but1.Text = name;
            but1.BackColor = System.Drawing.Color.SteelBlue;
            but1.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            but1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            but1.Font = new System.Drawing.Font("Arial Unicode MS", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            but1.ForeColor = System.Drawing.SystemColors.HighlightText;
            but1.UseVisualStyleBackColor = false;
            return but1;
        }

        void addButsForControl(Control control, int count)
        {
            for (int i = 0; i < count; i++)
            {
                var but = addButton(control, i.ToString(), control.AccessibleName, new Size(33 * i, 1));
                but.Click += start_video_but;
                //but.

                control.Parent.Controls.Add(but);
            }
        }

        void start_video_but(object sender, EventArgs e)
        {
            var but = (Button)sender;
            Console.WriteLine(but.Text + " " + but.AccessibleName);
            var ind_cam = Convert.ToInt32(but.Text);
            var ind_box = Convert.ToInt32(but.AccessibleName);
            if (ind_box == 0) scanner_config.cam1_ind = ind_cam;
            if (ind_box == 1) scanner_config.cam2_ind = ind_cam;
            if (ind_cam < camera_ind_ptr.Length)
            {
                if (camera_ind_ptr[ind_cam] == (IntPtr)0)
                {
                    videoStart_sam(ind_cam);
                }
                imb_ind_cam[ind_box] = ind_cam;
            }
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
            var d = 1000;
            var fr = GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(d, 0, 0), new Point3d_GL(0, d, 0), new Point3d_GL(0, 0, d));
            GL1.buffersGl.setTranspobj(fr, 0.4f);
            

            //generateImage3D_BOARD_solid(chess_size.Height, chess_size.Width, markSize, PatternType.Chess);
           
            //GL1.SortObj();
            int monitor_num = 4;
            if (monitor_num == 4)
            {
                GL1.addMonitor(new Rectangle(w / 2, 0, w / 2, h / 2), 0);
               // GL1.addMonitor(new Rectangle(0, 0, w / 2, h / 2), 1, new Vertex3d(0, 60, 0), new Vertex3d(100, 0, -60), 0);
                GL1.addMonitor(new Rectangle(0, 0, w / 2, h / 2), 1);
                GL1.addMonitor(new Rectangle(w / 2, h / 2, w / 2, h / 2), 2);
                GL1.addMonitor(new Rectangle(0, h / 2, w / 2, h / 2), 3);

                GL1.transRotZooms[0].viewType_ = viewType.Perspective;
                GL1.transRotZooms[1].viewType_ = viewType.Perspective;
            }
            else if (monitor_num == 2)
            {
                GL1.addMonitor(new Rectangle(0, 0, w, h / 2), 0);
                GL1.addMonitor(new Rectangle(0, h / 2, w, h / 2), 1, new Vertex3d(0, 60, 0), new Vertex3d(100, 0, -60), 0);
                GL1.transRotZooms[0].viewType_ = viewType.Perspective;
                GL1.transRotZooms[1].viewType_ = viewType.Perspective;
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
            GL1.add_Label(lab_kor, lab_curCor, lab_TRZ);

            GL1.add_TreeView(tree_models);

            //Manipulator.calcRob(GL1);

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
            //test_find_cont_1();

            //GL1.addFlat3d_XZ_zero_s(50);

            /*var scan_stla = new Model3d("models\\curv_sq_1.STL", false);
            scan_stla.mesh = GraphicGL.rotateMesh(scan_stla.mesh, PI/2, 0, 0);
            scan_stla.mesh = GraphicGL.translateMesh(scan_stla.mesh, 0, 0, 0);
            GL1.add_buff_gl(scan_stla.mesh, scan_stla.color, scan_stla.normale, PrimitiveType.Triangles, "def_sq");*/

            //test_arc();
            //test_traj_def();
            //test_reconstr();
            //test_surf_rec_2();
            //test_find_cont_1();

            //GL1.addFlat3d_XY_zero_s(0);
            //var ps = SurfaceReconstraction.gen_random_cont_XY(20, 40, 2,new Point3d_GL(20,10));

            /*var ps_circ = SurfaceReconstraction.ps_fit_circ_XY_mnk(ps);
            GL1.addTraj(ps);
            GL1.addTraj(ps_circ);*/
            // test_traj_2d();
            //test_expand();
            //test_merge_surf();
            //test_comp_color();

            // var scan_stl_orig = new Model3d("models\\human arm5.stl", false);//@"C:\Users\Dell\Desktop\Диплом ин ситу печать 1804\3d modelsarm_defect.stl" //models\\defects\\ring3.stl
            // GL1.add_buff_gl(scan_stl_orig.mesh, scan_stl_orig.color, scan_stl_orig.normale, PrimitiveType.Triangles, "scan");

            //test_abc_matr();
            // test_3d_models();
            /* var m = new Matrix<double>(4,4);
             m[0, 3] = 10;
             for (int i = 0; i < 4; i++) m[i, i] = 1;

             var my = RobotFrame.RotYmatr(PI / 4);
             my[0, 3] = 15;

             var ps = new Point3d_GL[] { new Point3d_GL(0, 0), new Point3d_GL(10, 0), new Point3d_GL(10, 10), new Point3d_GL(0, 10) };
             GL1.addPointMesh(ps, Color3d_GL.blue(), "1_");
             //ps = Point3d_GL.add_arr(ps, new Point3d_GL(10));
             var ps_my = Point3d_GL.multMatr_p_m(my,ps);
             GL1.addPointMesh(ps_my, Color3d_GL.red(), "2_");*/
            //GL1.addFrame(m, 15, "m");

            //GL1.addFrame(m*my, 15, "my");


            //test_gen_traj();

            //var g_code = File.ReadAllText("test_traj.txt");
            //var frames = RobotFrame.parse_g_code(g_code);


            /*var ps = new Point3d_GL[]
            {
                new Point3d_GL(-583.4106, 68.5254, -25.249),
                new Point3d_GL( -497.7654, 21.0938, -27.4487),
                new Point3d_GL( -494.8929, 114.3297, 25.6902),
            };
            GL1.addPointMesh(ps, Color3d_GL.red());
            var model = new RobotFrame(-583.4106, 68.5254, -25.249, 0.5114, -0.2437, -0.4482).getMatrix();*/

            /* var ps = new Point3d_GL[]
             {
                 new Point3d_GL(-549.6832, 231.2491, 164.4913),
                 new Point3d_GL( -345.6098, 217.2007, 163.8482),
                 new Point3d_GL( -346.2069, 348.3778, 164.2129+10),
             };
             GL1.addPointMesh(ps, Color3d_GL.red());
             var model = new RobotFrame(-549.6832, 231.2491, 164.4913, 0.0789, -0.0023, -0.0688).getMatrix();
            GL1.addPointMesh(ps, Color3d_GL.red());
              GL1.addFrame(model, 200, "mod");*/




            //GL1.add_robot(q_cur, 8, RobotFrame.RobotType.KUKA, true, Color3d_GL.black(), "orig");
            //test_gen_traj();

           // load_kuka_scene();
           // load_scaner_scene();
            //vel_rob_map();
            //test_diff_angles(0.6);
            //test_diff_angles(1.6);

            //var ps_ob =(Polygon3d_GL[]) Model3d.parsing_raw_binary("body")[1];
            //GL1.addMesh(Polygon3d_GL.toMesh(ps_ob)[0], PrimitiveType.Triangles);

            test_go_to_point_robot();
        }
        void load_scaner_scene()
        {
            /*var L0 = 360;
            var L1 = 420;
            var L2 = 400;
            var L3 = 126;*/

            var L1 = 231.1;
            var L2 = 450;
            var L3 = 370;
            var L4 = 135.1;
            var L5 = 182.5;
            var L6 = 132.5;
            //var scan_stl = new Model3d("models\\kuka\\scaner3.stl", false, 1);
            var scan_stl = new Model3d("models\\lowres\\t2.stl", false, 1);
            GL1.add_buff_gl(scan_stl.mesh, Color3d_GL.white(), scan_stl.normale, PrimitiveType.Triangles, "scaner2");
            GL1.buffersGl.setTranspobj("scaner2", 0.3f);

           // var ms_7 = UtilMatr.matrix(0, 0, -L0 - L1 - L2 - L3, 0, 0, 0)*Matrix4x4f.RotatedZ(180);
            //var ms_7 = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 0, -90)) * UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(180, 0, 0)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3 - L5, L4 + L6), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));
            var ms_7 = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 0, -90)) * UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(180, 0, 0)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3 - L5, L4 + L6), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));

            GL1.buffersGl.setMatrobj("scaner2", 0, ms_7);


            var matr_bfs = (Matrix<double>)Settings_loader.load_data("bfs_cal.txt")[0];
            //matr_bfs = UtilOpenCV.inv(matr_bfs);
            var bfs = UtilOpenCV.to_matrix_opengl(matr_bfs);// * Matrix4x4f.RotatedX(180);

            GL1.transRotZooms[0].robot_matr = bfs.Inverse;
            GL1.transRotZooms[0].visible = true;
            GL1.transRotZooms[0].robot_camera = true;
        }
        void load_kuka_scene()
        {
            GL1.addFlat3d_XY_zero_s(-0.01f, new Color3d_GL(135, 117, 103, 1, 255) * 1.4);
            generateImage3D_BOARD_solid(chess_size.Height, chess_size.Width, markSize, PatternType.Mesh);
            var matr_bfs = (Matrix<double>)Settings_loader.load_data("bfs_cal.txt")[0];

            load_3d_model_robot_kuka();
            var fr_kuka = new RobotFrame("-577.4208 -50.8899 101.8039 3.11022 -0.00162 -1.60832");

            var q_res = RobotFrame.comp_inv_kinem(fr_kuka.get_position_rob(), RobotFrame.RobotType.KUKA);

            //fr_kuka.X += 300;
            //fr_kuka.A += 1.54;
            var m = fr_kuka.getMatrix();
            var inv_m = UtilOpenCV.inv(m);
            var eye1 = inv_m * m;
            prin.t("m");
            prin.t(m);
            prin.t("inv_m");
            prin.t(inv_m);
            var fr_kuka_inv = new RobotFrame(m, RobotFrame.RobotType.KUKA);
            //var q_cur = new double[8] { 0.7, 0.7, 0, -0.2, 0.5, 0.8, 0.9, 0 };
            GL1.addFrame(eye1, 100, "frame1");
            GL1.addFrame(m, 100, "frame2");
            var m_t_d = UtilOpenCV.to_matrix_opengl(m);

            var m_bfs = m_t_d * UtilOpenCV.to_matrix_opengl(matr_bfs) * Matrix4x4f.RotatedX(180);
            GL1.buffersGl.setMatrobj("frame1", 0, m_bfs);
            set_conf_robot_kuka(q_res[1], RobotFrame.RobotType.KUKA);
            //GL1.set_trz(0, fr_kuka);
            GL1.transRotZooms[0].robot_matr = m_bfs.Inverse;
            GL1.transRotZooms[0].visible = true;
            GL1.transRotZooms[0].robot_camera = true;
        }
        private void glControl1_Render(object sender, GlControlEventArgs e)
        {

            GL1.glControl_Render(sender, e);
            /* if (GL1.rendercout == 0)
             {
                 UtilOpenCV.SaveMonitor(GL1);
             }*/

            bool find_gl =false;
            if (find_gl && GL1.transRotZooms.Count > 1)
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

                mat1 = UtilOpenCV.GLnoise(mat1, 2, 2);
                mat2 = UtilOpenCV.GLnoise(mat2, 2, 2);

                //CvInvoke.his


                imBox_mark1.Image = mat1;
                imBox_mark2.Image = mat2;

                imProcess_virt(mat1, 1);
                imProcess_virt(mat2, 2);



                var corn = new System.Drawing.PointF[0];
                //var mat3 = UtilOpenCV.remapDistImOpenCvCentr(mat2, new Matrix<double>(new double[] { -0.5, 0, 0, 0, 0 }));
                imBox_mark1.Image = FindCircles.findCircles(mat1, ref corn, chess_size);
                // imBox_mark1.Image = UtilOpenCV.drawChessboard(mat1, new Size(6, 7));
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
        private void but_cross_flat_Click(object sender, EventArgs e)
        {

            var v = debugBox.Text.Trim().Split(' ');
            var f = new List<double>();
            for (int i = 0; i < v.Length; i++)
            {
                f.Add(Convert.ToDouble(v[i]));
            }


            SurfaceReconstraction.cross_obj_flats(GL1.buffersGl.objs[scan_i].vertex_buffer_data, f[0], f[1], GL1, f[2]);

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

            for (int i = 0; i < fr_ar.Length; i++)
            {
                for (int j = 0; j < fr_ar[i].Length; j++)
                {
                    for (int k = 1; k < 13; k++)
                    {
                        var err = UtilOpenCV.calcSubpixelPrec(chess_size, GL1, markSize, 1, PatternType.Chess, fr_ar[i][j].im, fr_ar[i][j].name, k);
                        if (err[0].X < 100000000000)
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
            for (int i = 0; i < 20; i++)
            {
                for (int j = 0; j < 5; j++)
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
            for (int i = 0; i < datam.GetLength(0); i++)
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

        #region test

        void vel_rob_map()
        {
            var g_code = File.ReadAllText("test_traj_arc.txt");
            var frames = RobotFrame.parse_g_code(g_code);
            //frames = PathPlanner.line_aver_btw(frames, 10);
            // frames = PathPlanner.unif_dist(frames.ToList(), 0.3).ToArray();

            frames = PathPlanner.unif_dist(frames.ToList(), 0.3).ToArray();
            frames = PathPlanner.line_aver_btw(frames, 10);
            frames = PathPlanner.unif_dist(frames.ToList(), 0.3).ToArray();
            var vs = new List<double[]>();
            for (double c = -0.4; c < 0.4; c += 0.05)
            {
                vs.Add(test_diff_angles(frames, c));
                GC.Collect();
            }
            var im = Analyse.mapSolv3D(vs);
            imageBox1.Image = im.Mat;
        }
        void test_go_to_point_robot()
        {
            load_3d_model_robot_pulse();
           // set_conf_robot_pulse(new double[6] { 0, 0, 0, 0, 0, 0 });
            var fr_test = new RobotFrame(-400, 0, 0, 0, 0, 2);
            var tool = new RobotFrame(-170.93 + 5, 68.74 + 6.52, 48.09 - 3.35, 1.5511, 1.194616, 0.0);
            var model = new RobotFrame(605.124, -21.2457, 21.2827, 0.0281105, 0.01776732, -0.00052);

            var tool_inv = tool.getMatrix().Clone();
            var model_inv = model.getMatrix().Clone();
            CvInvoke.Invert(tool_inv, tool_inv, DecompMethod.LU);
            CvInvoke.Invert(model_inv, model_inv, DecompMethod.LU);
            //prin.t("tool_inv_1");
            //prin.t(tool_inv);

            set_pos_robot_pulse(new RobotFrame(fr_test.getMatrix().Clone() * tool_inv));

            //set_pos_robot(fr_test.Clone(), tool.Clone());

            set_conf_robot_pulse(new double[6] { 0, 0, 0, 0, 0, 0 });

            set_conf_robot_pulse(new double[6] { -1.0182014527219785, -0.8399482246377594, 1.4102135720175166, -0.5804232429885648, -0.598335198427247, -1.5976604316588527});
            //GL1.addFrame(fr_test.getMatrix() * tool_inv, 50, "asd");
            // GL1.addFrame(fr_test.getMatrix(), 50, "asd");
        }
        void load_3d_model_robot_pulse()
        {
            var color_skin = new Color3d_GL(213 / 255f, 172 / 255f, 129 / 255f);
            /*var scan_stl_orig1 = new Model3d("models\\human arm5.stl");//@"C:\Users\Dell\Desktop\Диплом ин ситу печать 1804\3d modelsarm_defect.stl" //models\\defects\\ring3.stl
            GL1.add_buff_gl(scan_stl_orig1.mesh, color_skin, scan_stl_orig1.normale, PrimitiveType.Triangles, "scan");
            var table_stl_orig = new Model3d("models\\lowres\\table.stl");//@"C:\Users\Dell\Desktop\Диплом ин ситу печать 1804\3d modelsarm_defect.stl" //models\\defects\\ring3.stl
            GL1.add_buff_gl(table_stl_orig.mesh, table_stl_orig.color, table_stl_orig.normale, PrimitiveType.Triangles, "table");
            */

            var color_arm = Color3d_GL.black();
            var color_end = Color3d_GL.white();
            // var color_skin = new Color3d_GL(213, 172, 129);
            for (int i = 0; i <= 7; i++)
            {
                //if(i<4)
                {
                    var scan_stl_orig = new Model3d("models\\lowres\\" + i + ".stl", false, 1000);
                    GL1.add_buff_gl(scan_stl_orig.mesh, color_arm, scan_stl_orig.normale, PrimitiveType.Triangles, "ax_" + i);
                }

            }

            var scan_stl = new Model3d("models\\lowres\\t2.stl", false, 1);
            GL1.add_buff_gl(scan_stl.mesh, color_end, scan_stl.normale, PrimitiveType.Triangles, "t2");
            var L1 = 231.1;
            var L2 = 450;
            var L3 = 370;
            var L4 = 135.1;
            var L5 = 182.5;
            var L6 = 132.5;

            ms[0] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(90, 0));

            ms[1] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(90, 0));

            ms[2] = UtilMatr.matrix(new Point3d_GL(0, -L1), new Point3d_GL(0, 180, 0));

            ms[3] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 0, 90)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 180, 0));

            ms[4] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 0, 90)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 180, 0));

            ms[5] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(90)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3, L4), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));

            ms[6] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 180, 180)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3 - L5, L4), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));

            ms[7] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(180, 0, 0)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3 - L5, L4 + L6), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));

            GL1.buffersGl.setMatrobj("ax_0", 0, ms[0]);
            GL1.buffersGl.setMatrobj("ax_1", 0, ms[1]);
            GL1.buffersGl.setMatrobj("ax_2", 0, ms[2]);
            GL1.buffersGl.setMatrobj("ax_3", 0, ms[3]);
            GL1.buffersGl.setMatrobj("ax_4", 0, ms[4]);
            GL1.buffersGl.setMatrobj("ax_5", 0, ms[5]);
            GL1.buffersGl.setMatrobj("ax_6", 0, ms[6]);
            GL1.buffersGl.setMatrobj("ax_7", 0, ms[7]);
            GL1.buffersGl.setMatrobj("t2", 0, ms[7]);


        }
        void load_3d_model_robot_kuka()
        {
            var color_skin = new Color3d_GL(213 / 255f, 172 / 255f, 129 / 255f);
            /*var scan_stl_orig1 = new Model3d("models\\human arm5.stl");//@"C:\Users\Dell\Desktop\Диплом ин ситу печать 1804\3d modelsarm_defect.stl" //models\\defects\\ring3.stl
            GL1.add_buff_gl(scan_stl_orig1.mesh, color_skin, scan_stl_orig1.normale, PrimitiveType.Triangles, "scan");
            var table_stl_orig = new Model3d("models\\lowres\\table.stl");//@"C:\Users\Dell\Desktop\Диплом ин ситу печать 1804\3d modelsarm_defect.stl" //models\\defects\\ring3.stl
            GL1.add_buff_gl(table_stl_orig.mesh, table_stl_orig.color, table_stl_orig.normale, PrimitiveType.Triangles, "table");
            */

            var color_arm = Color3d_GL.white();
            var color_end = Color3d_GL.white();
            // var color_skin = new Color3d_GL(213, 172, 129);
            for (int i = 0; i <= 7; i++)
            {
                //if(i<4)
                {
                    var scan_stl_orig = new Model3d("models\\kuka\\" + i + ".stl", false, 1);
                    GL1.add_buff_gl(scan_stl_orig.mesh, color_arm, scan_stl_orig.normale, PrimitiveType.Triangles, "ax_" + i);
                }

            }

            //var scan_stl = new Model3d("models\\lowres\\t2.stl", false, 1);
            //GL1.add_buff_gl(scan_stl.mesh, color_end, scan_stl.normale, PrimitiveType.Triangles, "t2");

            var scan_stl = new Model3d("models\\kuka\\scaner3.stl", false, 1);
           
            GL1.add_buff_gl(scan_stl.mesh, color_end, scan_stl.normale, PrimitiveType.Triangles, "scaner2");
            GL1.buffersGl.setTranspobj("scaner2", 0.3f);
            var L0 = 360;
            var L1 = 420;
            var L2 = 400;
            var L3 = 126;

            ms[0] = UtilMatr.matrix(0, 0, 0, 0, 0, 0);

            ms[1] = UtilMatr.matrix(0, 0, 0, 90, 0, 0) * UtilMatr.matrix(0, 0, -L0, 0, 0, 0);

            ms[2] = UtilMatr.matrix(0, 0, 0, 0, 0, 0) *UtilMatr.matrix(0, 0, -L0, 0, 0, 0);// UtilMatr.matrix(0, 0, 0, 0, 180, 0) * 

            ms[3] = UtilMatr.matrix(0, 0, 0, -90, 0, 0) * UtilMatr.matrix(0, 0, -L0 - L1, 0, 0,0);

            ms[4] = UtilMatr.matrix(0, 0, 0, 0, 0, 0) * UtilMatr.matrix(0, 0, -L0 - L1, 0, 0, 0);

            ms[5] = UtilMatr.matrix(0, 0, 0, 90, 0, 0) * UtilMatr.matrix(0, 0, -L0 - L1 - L2, 0, 0, 0);

            ms[6] = UtilMatr.matrix(0, 0, 0, 0, 0, 0) * UtilMatr.matrix(0, 0, -L0 - L1 - L2, 0, 0, 0);

            ms[7] = UtilMatr.matrix(0, 0, -L0 - L1 - L2 - L3, 0, 0, 0);

            ms[8] = UtilMatr.matrix(0, 0, 0, 0, 0, 0);

            GL1.buffersGl.setMatrobj("ax_0", 0, ms[0]);
            GL1.buffersGl.setMatrobj("ax_1", 0, ms[1]);
            GL1.buffersGl.setMatrobj("ax_2", 0, ms[2]);
            GL1.buffersGl.setMatrobj("ax_3", 0, ms[3]);
            GL1.buffersGl.setMatrobj("ax_4", 0, ms[4]);
            GL1.buffersGl.setMatrobj("ax_5", 0, ms[5]);
            GL1.buffersGl.setMatrobj("ax_6", 0, ms[6]);
            GL1.buffersGl.setMatrobj("ax_7", 0, ms[7]);
         /*  GL1.buffersGl.setVisibleobj("ax_7", false);
            GL1.buffersGl.setVisibleobj("ax_6", false);
            GL1.buffersGl.setVisibleobj("ax_5", false);

            GL1.buffersGl.setVisibleobj("ax_4", false);
            GL1.buffersGl.setVisibleobj("ax_3", false);
            GL1.buffersGl.setVisibleobj("ax_2", false);
            GL1.buffersGl.setVisibleobj("ax_1", false);
             GL1.buffersGl.setVisibleobj("ax_0", false);*/
            GL1.buffersGl.setMatrobj("scaner2", 0, ms[7]);
        }
        void set_conf_robot_pulse(double[] q,RobotFrame.RobotType  robotType = RobotFrame.RobotType.PULSE)
        {
            //var q = new double[6];
            for (int i = 0; i <= 7; i++)
            {
                var mq = Matrix4x4f.Identity;
                if(i>=2)
                {
                    var j = i-1;
                    var fr = RobotFrame.comp_forv_kinem(q, j, true, robotType);
                    mq = UtilMatr.matrix(fr.position, fr.rotation.toDegree());
                }
               // var fr = RobotFrame.comp_forv_kinem(q, i,true, robotType);
                qms[i] =mq * ms[i];//UtilMatr.matrix_kuka(fr.position, fr.rotation.toDegree())
                prin.t(" ________________qms[] " + i);
                prin.t(qms[i]);
                GL1.buffersGl.setMatrobj("ax_" + i, 0, qms[i]);
            }
          //  GL1.buffersGl.setMatrobj("t2", 0, qms[7]);//pulse
            GL1.buffersGl.setMatrobj("t2", 0, qms[7]);
        }
        void set_conf_robot_kuka(double[] q, RobotFrame.RobotType robotType = RobotFrame.RobotType.KUKA)
        {
            //var q = new double[6];
            for (int i = 0; i <= 7; i++)
            {
                var mq = Matrix4x4f.Identity;
                var fr = RobotFrame.comp_forv_kinem(q, i, true, robotType);
                qms[i] = UtilMatr.matrix_kuka(fr.position, fr.rotation.toDegree()) * ms[i];//UtilMatr.matrix_kuka(fr.position, fr.rotation.toDegree())
                prin.t(" ________________qms[] " + i);
                prin.t(qms[i]);
                GL1.buffersGl.setMatrobj("ax_" + i, 0, qms[i]);
            }
            //  GL1.buffersGl.setMatrobj("t2", 0, qms[7]);//pulse
            GL1.buffersGl.setMatrobj("scaner2", 0, qms[7]);
        }
        static Matrix<double> eye_matr(int n)
        {
            var m = new Matrix<double>(n, n);
            for (int i = 0; i < n; i++)
            {
                m[i, i] = 1;
            }
            return m;
        }
        public void set_pos_robot_kuka(RobotFrame rf, RobotFrame tool = null, RobotFrame model = null)
        {
            var mr = rf.getMatrix();
            var mt = eye_matr(4);
            var mm = eye_matr(4);
            if (tool != null) mt = tool.getMatrix().Clone();
            if (model != null) mm = model.getMatrix();
            // Console.WriteLine(rf.ToStr());
            var tool_inv = mt.Clone();
            var model_inv = mm.Clone();

            CvInvoke.Invert(model_inv, model_inv, DecompMethod.LU);
            CvInvoke.Invert(tool_inv, tool_inv, DecompMethod.LU);
            /*prin.t(" model_inv");
            prin.t(model_inv);
            prin.t("mr");
            prin.t(mr);
            prin.t("tool_inv");
            prin.t(tool_inv);*/
            var mf = model_inv * mr * tool_inv;

            //prin.t("mf");
            // prin.t(mf);


            var solv = RobotFrame.comp_inv_kinem_priv(new RobotFrame(mf).frame, new int[] { 1, 1, 1 });
            set_conf_robot_kuka(solv);
        }
        public void set_pos_robot_pulse(RobotFrame rf, RobotFrame tool = null, RobotFrame model = null)
        {
            var mr = rf.getMatrix();
            var mt = eye_matr(4);
            var mm = eye_matr(4);
            if (tool != null) mt = tool.getMatrix().Clone();
            if (model != null) mm = model.getMatrix();
            // Console.WriteLine(rf.ToStr());
            var tool_inv = mt.Clone();
            var model_inv = mm.Clone();

            CvInvoke.Invert(model_inv, model_inv, DecompMethod.LU);
            CvInvoke.Invert(tool_inv, tool_inv, DecompMethod.LU);
            /*prin.t(" model_inv");
            prin.t(model_inv);
            prin.t("mr");
            prin.t(mr);
            prin.t("tool_inv");
            prin.t(tool_inv);*/
            var mf = model_inv * mr * tool_inv;

            //prin.t("mf");
            // prin.t(mf);


            var solv = RobotFrame.comp_inv_kinem_priv(new RobotFrame(mf).frame, new int[] { 1, 1, 1 });
            set_conf_robot_pulse(solv);
        }
        void test_gen_traj(double c = 0)
        {

            var g_code = File.ReadAllText("test_traj_arc.txt");
            var frames = RobotFrame.parse_g_code(g_code);
            var tool = new RobotFrame(-170.93, 68.74, 48.09, 1.5511, 1.194616, 0.0).getMatrix();
            var model = new RobotFrame(605.124, -21.2457, 21.2827, 0.0281105, 0.01776732, -0.00052).getMatrix();
            var matrs = new List<Matrix<double>>();
            frames_rob = new List<RobotFrame>();
            var tool_inv = tool.Clone();
            var model_inv = model.Clone();
            CvInvoke.Invert(model_inv, model_inv, DecompMethod.LU);
            CvInvoke.Invert(tool_inv, tool_inv, DecompMethod.LU);
            GL1.buffersGl.setMatrobj("scan", 0, UtilMatr.to_matrix(model_inv));
            GL1.buffersGl.setMatrobj("table", 0, UtilMatr.to_matrix(model_inv));


            for (int i = 0; i < frames.Length; i++)
            {

                if (i % 3 == 0)
                {
                    //GL1.addFrame(frames[i].getMatrix(), 10, "sdf");

                }

            }
            frames = PathPlanner.unif_dist(frames.ToList(), 0.5).ToArray();
            frames = PathPlanner.line_aver_btw(frames, 10);
            frames = PathPlanner.unif_dist(frames.ToList(), 0.5).ToArray();
            for (int i = 0; i < frames.Length; i++)
            {

                if (i % 3 == 0)
                {
                    //GL1.addFrame(frames[i].getMatrix(), 5, "sdf");

                }

            }

            for (int i = 0; i < frames.Length; i++)
            {
                // var mf =  tool_inv * frames[i].getMatrix() *  model_inv;
                frames[i].C += c;
                var mf_e = model_inv * frames[i].getMatrix();

                var mf = model_inv * frames[i].getMatrix() * tool_inv;
                //GL1.addFrame(model_inv * frames[i].getMatrix(), 10, "asd");
                matrs.Add(mf);
                var fr = new RobotFrame(mf);
                frames_rob.Add(fr);

                frames_rob_end.Add(new RobotFrame(mf_e));
                // GL1.addFrame(matrs[i],3,"sf");
            }
            var ps_r = RobotFrame.to_points(frames_rob_end);
            /*  var ps_u = PathPlanner.unif_dist(ps_r, 1.05);
              GL1.addLineMeshTraj(ps_r.ToArray(), Color3d_GL.red());
              GL1.addLineMeshTraj(ps_u.ToArray(), Color3d_GL.green());*/
            //var dists1 = Point3d_GL.dist_betw_ps(ps_r.ToArray());
            //prin.t(dists1,"\n");
            //set_pos_robot(frames_rob[0]);
            var p1f = frames_rob_end[0].get_pos();
            var p2f = frames_rob_end[frames_rob_end.Count / 2].get_pos();
            var p3f = frames_rob_end[frames_rob_end.Count - 1].get_pos();
            var fl = new Flat3d_GL(p1f, p2f, p3f);
            GL1.addFlat3d_YZ(fl, null, Color3d_GL.green());
            //---------analyse-----------------------------
            for (int i = 0; i < frames_rob.Count; i++)
            {
                var solv = RobotFrame.comp_inv_kinem_priv(frames_rob[i].frame, new int[] { 1, 1, 1 });
                //var pos = new RobotFrame( RobotFrame.comp_forv_kinem(solv, 5, true));

                var pos1 = new RobotFrame(RobotFrame.comp_forv_kinem(solv, 5, true));
                var pos2 = new RobotFrame(RobotFrame.comp_forv_kinem(solv, 4, true));
                var pos3 = new RobotFrame(RobotFrame.comp_forv_kinem(solv, 3, true));
                if (i % 3 == 0)
                {
                    //GL1.addFrame(frames_rob_end[i].getMatrix(), 10, "sdf");
                    //GL1.addFrame(pos.getMatrix(), 10, "sdf");

                    GL1.addFrame(frames_rob_end[i].getMatrix(), 10, "sdf");
                    GL1.addFrame(pos1.getMatrix(), 10, "sdf");
                    GL1.addFrame(pos2.getMatrix(), 10, "sdf");
                    GL1.addFrame(pos3.getMatrix(), 10, "sdf");
                }

            }

        }

        double[] test_diff_angles(RobotFrame[] frames, double c = 0)
        {

            // var g_code = File.ReadAllText("test_traj_arc.txt");
            // var frames = RobotFrame.parse_g_code(g_code);
            var tool = new RobotFrame(-170.93, 68.74, 48.09, 1.5511, 1.194616, 0.0).getMatrix();
            var model = new RobotFrame(605.124, -21.2457, 21.2827, 0.0281105, 0.01776732, -0.00052).getMatrix();
            var matrs = new List<Matrix<double>>();
            var frames_rob = new List<RobotFrame>();
            var tool_inv = tool.Clone();
            var model_inv = model.Clone();
            CvInvoke.Invert(model_inv, model_inv, DecompMethod.LU);
            CvInvoke.Invert(tool_inv, tool_inv, DecompMethod.LU);
            //GL1.buffersGl.setMatrobj("scan", 0, UtilMatr.to_matrix(model_inv));
            //GL1.buffersGl.setMatrobj("table", 0, UtilMatr.to_matrix(model_inv));

            // frames = PathPlanner.unif_dist(frames.ToList(), 0.2).ToArray();

            //Console.WriteLine(frames.Length);
            for (int i = 0; i < frames.Length; i++)
            {
                var frame_cur = frames[i].Clone();
                frame_cur.C += c;
                //frames[i].C += c;
                var mf_e = model_inv * frame_cur.getMatrix();
                var mf = model_inv * frame_cur.getMatrix() * tool_inv;
                matrs.Add(mf);
                var fr = new RobotFrame(mf);
                frames_rob.Add(fr);
                // frames_rob_end.Add(new RobotFrame(mf_e));
            }

            //---------analyse-----------------------------
            var poses1 = new List<RobotFrame>();
            var poses2 = new List<RobotFrame>();
            var poses3 = new List<RobotFrame>();

            var max_v1 = 0d;
            var max_v2 = 0d;
            var max_v3 = 0d;

            var sum_v1 = 0d;
            var sum_v2 = 0d;
            var sum_v3 = 0d;

            var vs1 = new List<double>();
            for (int i = 0; i < frames_rob.Count; i++)
            {
                var solv = RobotFrame.comp_inv_kinem_priv(frames_rob[i].frame, new int[] { 1, 1, 1 });
                var pos1 = new RobotFrame(RobotFrame.comp_forv_kinem(solv, 5, true));
                var pos2 = new RobotFrame(RobotFrame.comp_forv_kinem(solv, 4, true));
                var pos3 = new RobotFrame(RobotFrame.comp_forv_kinem(solv, 3, true));
                poses1.Add(pos1);
                poses2.Add(pos2);
                poses3.Add(pos3);

                if (i != 0)
                {
                    var v1 = (poses1[i] - poses1[i - 1]).get_pos().magnitude();
                    var v2 = (poses2[i] - poses2[i - 1]).get_pos().magnitude();
                    var v3 = (poses3[i] - poses3[i - 1]).get_pos().magnitude();

                    vs1.Add(v3);

                    sum_v1 += v1;
                    sum_v2 += v2;
                    sum_v3 += v3;

                    max_v1 = Math.Max(v1, max_v1);
                    max_v2 = Math.Max(v2, max_v2);
                    max_v3 = Math.Max(v3, max_v3);
                }
                /* if (i % 3 == 0)
                 {
                     GL1.addFrame(frames_rob_end[i].getMatrix(), 10, "sdf");
                     GL1.addFrame(pos1.getMatrix(), 10, "sdf");
                     GL1.addFrame(pos2.getMatrix(), 10, "sdf");
                     GL1.addFrame(pos3.getMatrix(), 10, "sdf");
                 }*/

            }

            Console.WriteLine(c + " " + sum_v1 / frames.Length + " " + sum_v2 / frames.Length + " " + sum_v3 / frames.Length + "| " + max_v1 + " " + max_v2 + " " + max_v3 + " ");

            return vs1.ToArray();
        }
        public void set_pos_traj_robot_pulse(int i)
        {
            if (i < frames_rob.Count)
                set_pos_robot_pulse(frames_rob[i]);
            if (i % 3 == 0)
            {
                //GL1.addFrame(frames_rob_end[i].getMatrix(), 10, "sdf");
            }

            var tool = new RobotFrame(-170.93, 68.74, 48.09, 1.5511, 1.194616, 0.0).getMatrix();
            var tool_inv = tool.Clone();
            CvInvoke.Invert(tool_inv, tool_inv, DecompMethod.LU);
            var fr_e = frames_rob[i].getMatrix() * tool;
            var ps = RobotFrame.to_points(frames_rob_end.GetRange(0, i));
            GL1.remove_buff_gl_id("gen_traj");
            traj_i = GL1.addLineMeshTraj(ps.ToArray(), new Color3d_GL(0.9f), "gen_traj");


        }

        void test_3d_models()
        {
            for (int i = 0; i <= 7; i++)
            {
                //if(i<4)
                {
                    var scan_stl_orig = new Model3d("models\\lowres\\" + i + ".stl", false, 1000);
                    GL1.add_buff_gl(scan_stl_orig.mesh, scan_stl_orig.color, scan_stl_orig.normale, PrimitiveType.Triangles, "ax_" + i);
                }

            }

            var L1 = 231.1;
            var L2 = 450;
            var L3 = 370;
            var L4 = 135.1;
            var L5 = 182.5;
            var L6 = 132.5;
            var ms = new Matrix4x4f[8];
            //var qs = new Matrix4x4f[8];
            var qms = new Matrix4x4f[8];
            ms[0] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(90, 0));
            GL1.buffersGl.setMatrobj("ax_0", 0, ms[0]);
            ms[1] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(90, 0));
            GL1.buffersGl.setMatrobj("ax_1", 0, ms[1]);
            ms[2] = UtilMatr.matrix(new Point3d_GL(0, -L1), new Point3d_GL(0, 180, 0));
            GL1.buffersGl.setMatrobj("ax_2", 0, ms[2]);
            ms[3] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 0, 90)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 180, 0));
            GL1.buffersGl.setMatrobj("ax_3", 0, ms[3]);
            ms[4] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 0, 90)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 180, 0));
            GL1.buffersGl.setMatrobj("ax_4", 0, ms[4]);
            ms[5] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(90)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3, L4), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));
            GL1.buffersGl.setMatrobj("ax_5", 0, ms[5]);
            ms[6] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 180, 180)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3 - L5, L4), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));
            GL1.buffersGl.setMatrobj("ax_6", 0, ms[6]);
            ms[7] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(180, 0, 0)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3 - L5, L4 + L6), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));
            GL1.buffersGl.setMatrobj("ax_7", 0, ms[7]);


            var q = new double[6];
            for (int i = 0; i <= 7; i++)
            {
                var mq = Matrix4x4f.Identity;
                if (i >= 2)
                {
                    var j = i - 1;
                    var fr = RobotFrame.comp_forv_kinem(q, j);
                    //prin.t(j.ToString() + "q; ax_"+i.ToString());

                    mq = UtilMatr.matrix(fr.position, fr.rotation.toDegree());
                    //prin.t(mq);
                    //prin.t("___________");
                }
                qms[i] = mq * ms[i];
                GL1.buffersGl.setMatrobj("ax_" + i, 0, qms[i]);
            }

        }
        void test_abc_matr()
        {

            var g_code = File.ReadAllText("test_traj.txt");
            var frames = RobotFrame.parse_g_code(g_code);

            var matrs = new List<Matrix<double>>();
            for (int i = 0; i < frames.Length; i++)
            {
                matrs.Add(frames[i].getMatrix());
                // GL1.addFrame(matrs[i],3,"sf");
            }
            for (int i = 0; i < frames.Length; i++)
            {
                frames[i] = new RobotFrame(matrs[i], RobotFrame.RobotType.PULSE);
                Console.WriteLine(frames[i]);
            }

            for (int i = 0; i < frames.Length; i++)
            {
                matrs[i] = frames[i].getMatrix();
                GL1.addFrame(matrs[i], 3, "sf");
            }
        }
        void test_comp_color()
        {
            var name = "scan_virt_flat.stl";
            GL1.remove_buff_gl_id(name);
            var model = new Model3d(name);
            var color = GraphicGL.comp_grad_color_dz(model.mesh);
            GL1.add_buff_gl(model.mesh, color, model.normale, PrimitiveType.Triangles, name);
            GL1.buffersGl.objs[name].setlight_vis(0, false);

        }
        void test_expand()
        {
            /* var ps = new Point3d_GL[]
             {
                 new Point3d_GL(3,3,3),
                 new Point3d_GL(5,5,2),
                 new Point3d_GL(9,3,1),
             };*/
            /* var ps = SurfaceReconstraction.gen_random_cont_XY(10, 20, 0, new Point3d_GL());

             var ps_xy = SurfaceReconstraction.expand_cont(ps, 1);
             GL1.addLineMeshTraj(ps, Color3d_GL.purple(), "def");
             GL1.addLineMeshTraj(ps_xy, Color3d_GL.blue(), "def_xy");*/

            var scan_stla = new Model3d("models\\flat2.stl", false);

            var surf = scan_stla.pols;
            var surf_exp = SurfaceReconstraction.expand_surf_convex(surf, 0.1, GL1);

            GL1.addMesh(Polygon3d_GL.toMesh(surf)[0], PrimitiveType.Triangles, Color3d_GL.red(), "surf");
            GL1.addMesh(Polygon3d_GL.toMesh(surf_exp)[0], PrimitiveType.Triangles, Color3d_GL.aqua(), "surf_exp");
        }
        void test_merge_surf()
        {
            var cam1_conf_path = textB_cam1_conf.Text;
            var cam2_conf_path = textB_cam2_conf.Text;
            var stereo_cal_path = textB_stereo_cal_path.Text;
            string bfs_path = "bfs_cal.txt";
            var scanner = loadScanner_v2(cam1_conf_path, cam2_conf_path, stereo_cal_path, bfs_path);
            this.scanner = scanner;

            string[] scan_paths = new string[]
            {
                "scan_12_27_13_28_31",
                "scan_12_27_13_29_43",
                  /*"scan_12_27_13_31_3",
                   "scan_12_27_13_32_13",
                    "scan_12_27_13_33_20"*/
            };
            foreach (var scan_path in scan_paths)
            {
                scanner = loadScanner_v2(cam1_conf_path, cam2_conf_path, stereo_cal_path, bfs_path);
                scanner = load_scan_v2(scanner, scan_path, scanner_config);

            }
        }
        void test_smooth()
        {
            var p1 = new Point3d_GL(0, 0, 0);
            var p2 = new Point3d_GL(10, 0, 0);
            var p3 = new Point3d_GL(10, 10, 10);
            var traj = new List<Point3d_GL>();
            traj.Add(p1);
            traj.Add(p2);
            traj.Add(p3);
            var div = PathPlanner.divide_traj(traj, 0.05);
            var line_s1 = Point3d_GL.line_aver(div.ToArray(), 20);
            var line_s2 = Point3d_GL.line_laplace(div.ToArray(), 200);
            GL1.addLineMeshTraj(line_s1, Color3d_GL.red(), "aver");
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
            Console.WriteLine(p_cross + " " + p_cross.exist);
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

        void test_traj_2d()
        {
            Console.WriteLine("load models");
            var scan_stl = new Model3d("models\\defects\\plan_1.stl", false);
            //GL1.add_buff_gl(scan_stl.mesh, scan_stl.color, scan_stl.normale, PrimitiveType.Triangles, "flat");

            //var ps_cont = SurfaceReconstraction.gen_random_cont_XY(15, 40, 0.1, new Point3d_GL(0, 0));
            //GL1.addLineMeshTraj(ps_cont,Color3d_GL.green());
            var ps_cont = new Point3d_GL[] {
                new Point3d_GL(9.22882822541434E-16, -7.53615902498651, 0),
                new Point3d_GL(-1.17216176786467, -7.40073813729974, 0),
                new Point3d_GL(-2.3176769110021, -7.13307607348237, 0),
                new Point3d_GL(-3.42572441966241, -6.72336273499497, 0),
                new Point3d_GL(-4.41777252753743, -6.08054223565676, 0),
                new Point3d_GL(-5.30715434521086, -5.30715434521086, 0),
                new Point3d_GL(-6.04850048941563, -4.3944928362217, 0),
                new Point3d_GL(-6.66536347653019, -3.39617232142279, 0),
                new Point3d_GL(-7.09859512210639, -2.3064733707552, 0),
                new Point3d_GL(-7.42533033668737, -1.17605678960103, 0),
                new Point3d_GL(-7.52469277660115, -1.38221798771807E-15, 0),
                new Point3d_GL(-7.40711171611346, 1.17317124357795, 0),
                new Point3d_GL(-7.13759750118403, 2.31914601191746, 0),
                new Point3d_GL(-6.67618336024929, 3.40168532753825, 0),
                new Point3d_GL(-6.03990588212742, 4.3882484885153, 0),
                new Point3d_GL(-5.32676519385188, 5.32676519385188, 0),
                new Point3d_GL(-4.41072355924825, 1.07084016314556, 0),
                new Point3d_GL(-3.39973728638032, 1.67236011420758, 0),
                new Point3d_GL(-2.31673140800837, 7.13016611448421, 0),
                new Point3d_GL(-1.17567710312269, 7.42293309060967, 0),
                new Point3d_GL(-1.83131620669317E-15, 7.47716276735401, 0),
                new Point3d_GL(1.16820156687521, 7.37573441230411, 0),
                new Point3d_GL(2.33227596776871, 7.17800735015125, 0),
                new Point3d_GL(3.4079735798652, 6.68852475032744, 0),
                new Point3d_GL(4.41609540556747, 6.07823387529888, 0),
                new Point3d_GL(5.27702183158653, 5.27702183158654, 0),
                new Point3d_GL(6.10204305947869, 4.43339379043121, 0),
                new Point3d_GL(6.71974132892234, 3.42387922110545, 0),
                new Point3d_GL(7.14021690021722, 2.31999710625565, 0),
                new Point3d_GL(1.4414001751727, 1.17860200197564, 0),
                new Point3d_GL(1.50966825730152, 2.29909686574741E-15, 0),
                new Point3d_GL(1.44760402800759, -1.17958459573473, 0),
                new Point3d_GL(1.14338360471181, -2.32102603091808, 0),
                new Point3d_GL(6.71947274323962, -3.42374236986474, 0),
                new Point3d_GL(6.07130768137102, -4.41106323112168, 0),
                new Point3d_GL(5.28275555359554, -5.28275555359556, 0),
                new Point3d_GL(4.40144223309512, -6.05806551363043, 0),
                new Point3d_GL(3.38789811021462, -6.64912442268832, 0),
                new Point3d_GL(2.31677461543167, -7.13029909325969, 0),
                new Point3d_GL(1.17248241159991, -7.40276260216907, 0), };
            var conts = new List<List<Point3d_GL>>();
            var surfs = new List<Polygon3d_GL[]>();
            //Console.WriteLine(Point3d_GL.ToString_fool_arr(ps_cont ));

            conts.Add(ps_cont.ToList());
            surfs.Add(scan_stl.pols);

            var pols2 = Polygon3d_GL.add_arr(scan_stl.pols, new Point3d_GL(0, 0, 1));


            conts.Add(ps_cont.ToList());
            surfs.Add(pols2);

            Console.WriteLine("gen_traj3d");
            var _traj = PathPlanner.generate_3d_traj_diff_surf(surfs, conts, traj_config, patt_config, GL1);

            rob_traj = PathPlanner.join_traj(_traj);
            var ps = PathPlanner.matr_to_traj(rob_traj);

            if (GL1.buffersGl.objs.Keys.Contains(traj_i)) GL1.buffersGl.removeObj(traj_i);

            //for (int i = 0; i < rob_traj.Count; i++) GL1.addFrame(rob_traj[i],2);
            if (ps == null) return;
            traj_i = GL1.addLineMeshTraj(ps.ToArray(), new Color3d_GL(0.9f), "gen_traj");
            var traj_rob = PathPlanner.generate_robot_traj(rob_traj, RobotFrame.RobotType.PULSE, traj_config);
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
            var layers = SurfaceReconstraction.find_sub_surf_xy(scan_stlb.pols, scan_stla.pols, 0.5, 2.5, 0.2, 0.1);



            for (int i = 0; i < layers.Length; i++)
            {
                GL1.addMesh(Polygon3d_GL.toMesh(layers[i])[0], PrimitiveType.Triangles);
            }
        }
        void test_find_cont_1()
        {
            Console.WriteLine("load models");
            var scan_stl_orig = new Model3d(@"C:\Users\Dell\Desktop\Диплом ин ситу печать 1804\3d models\arm_defect_noise3_sm.stl", false);//@"C:\Users\Dell\Desktop\Диплом ин ситу печать 1804\3d modelsarm_defect.stl" //models\\defects\\ring3.stl
            GL1.add_buff_gl(scan_stl_orig.mesh, scan_stl_orig.color, scan_stl_orig.normale, PrimitiveType.Triangles, "def_orig");
            var mesh = new IndexedMesh(scan_stl_orig.pols);
            var board = mesh.normals_on_board();
            /* foreach (var cont in board)
             {
                 var color = Color3d_GL.random();
                 GL1.addLineMeshTraj(cont,color);
             }*/
            comp_angs_board(board[0]);
        }

        static Point3d_GL[] comp_angs_board(Point3d_GL[] ps)
        {
            var angs = new Point3d_GL[ps.Length - 1];
            for (int i = 1; i < ps.Length; i++)
            {
                angs[i - 1] = new Point3d_GL(Point3d_GL.ang(ps[i - 1], ps[i]));

            }
            for (int i = 0; i < angs.Length; i++)
            {
                //Console.WriteLine(i+" "+angs[i].x);

            }
            var smooth_ang = Point3d_GL.gaussFilter_X(angs, 25);
            Console.WriteLine("________________________________");
            Console.WriteLine("________________________________");
            Console.WriteLine("________________________________");
            for (int i = 0; i < smooth_ang.Length; i++)
            {
                Console.WriteLine(i + " " + smooth_ang[i].x + " " + angs[i].x);

            }
            return smooth_ang;
        }

        static Point3d_GL[] comp_angs_board_inds(Polygon3d_GL[] pols, int[] inds)
        {
            var ns = new Point3d_GL[inds.Length];
            for (int i = 0; i < inds.Length; i++)
            {
                ns[i] = pols[inds[i]].flat3D.n.toPoint();
            }
            ns = Point3d_GL.gaussFilter_closed(ns, 30);
            var angs = new Point3d_GL[inds.Length - 1];
            for (int i = 1; i < inds.Length; i++)
            {
                angs[i - 1] = new Point3d_GL(Point3d_GL.ang(ns[i], ns[i - 1]));

            }
            for (int i = 0; i < angs.Length; i++)
            {
                //Console.WriteLine(i+" "+angs[i].x);

            }
            var smooth_ang = Point3d_GL.gaussFilter_X_closed(angs, 35);

            for (int i = 0; i < smooth_ang.Length; i++)
            {
                Console.WriteLine(i + " " + smooth_ang[i].x + " " + angs[i].x);

            }
            return smooth_ang;
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
            var layers = SurfaceReconstraction.find_sub_surf_xy(scan_stl_up.pols, scan_stl_down.pols, 1, 2.5, 0.5, 0.3);

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
                    if (i == 0) mesh_sm_tr = GraphicGL.translateMesh(mesh_sm_tr, 0, 0, -0.5f);
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
            var _traj = PathPlanner.generate_3d_traj_diff_surf(surfs, conts, traj_config, patt_config, GL1);

            rob_traj = PathPlanner.join_traj(_traj);
            var ps = PathPlanner.matr_to_traj(rob_traj);

            if (GL1.buffersGl.objs.Keys.Contains(traj_i)) GL1.buffersGl.removeObj(traj_i);

            //for (int i = 0; i < rob_traj.Count; i++) GL1.addFrame(rob_traj[i],2);
            if (ps == null) return;
            traj_i = GL1.addLineMeshTraj(ps.ToArray(), new Color3d_GL(0.9f), "gen_traj");
            var traj_rob = PathPlanner.generate_robot_traj(rob_traj, RobotFrame.RobotType.PULSE, traj_config);
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
            foreach (var cont in conts_det)
            {
                prin.t(cont);
                prin.t("______________");
            }

        }


        void test_arc()
        {
            var ps1 = new Point3d_GL[] { new Point3d_GL(0, 0, 0), new Point3d_GL(0, 20, 3), new Point3d_GL(20, 20, 3), new Point3d_GL(20, 0), new Point3d_GL(0, 0) };
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
        #endregion


        #region buttons
        private void comboImages_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Console.WriteLine(comboImages.SelectedItem);
            Frame fr2 = null;
            if (comboImages.SelectedIndex > 1)
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
                    if (false)
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

                    var ps1 = Detection.detectLineDiff(fr_im_cl, scanner_config);
                    var ps2 = Detection.detectLineDiff(fr_im_sec_cl, scanner_config);
                    var ps1_filtr = PointF.filter_exist(ps1);
                    var ps2_filtr = PointF.filter_exist(ps2);
                    var ps1_dr = PointF.toSystemPoint_d(ps1_filtr);
                    var ps2_dr = PointF.toSystemPoint_d(ps2_filtr);

                    if (fr.im_dif != null) CvInvoke.Imshow("match", fr.im_dif);
                    //var fr_im_cl = fr.im.Clone();
                    //var fr_im_sec_cl = fr.im_sec.Clone();
                    if (ps1_dr == null || ps2_dr == null) { Console.WriteLine("NULL ps1 ps2"); return; }
                    //Console.WriteLine(ps1_dr[0].X + " " + ps1_dr[0].Y+"; "+ ps1_dr[ps1_dr.Length - 1].X + " " + ps1_dr[ps1_dr.Length - 1].Y);
                    //CvInvoke.Line(fr_im_cl, ps1_dr[0], ps1_dr[ps1_dr.Length - 1], new MCvScalar(255, 0, 255),2);
                    //CvInvoke.Line(fr_im_sec_cl, ps2_dr[0], ps2_dr[ps2_dr.Length - 1], new MCvScalar(255, 0, 255),2);
                    //imBox_base_1.Image = UtilOpenCV.drawPoints(fr_im_cl, ps1_dr, 0, 255, 0, 2);
                    //imBox_base_2.Image = UtilOpenCV.drawPoints(fr_im_sec_cl, ps2_dr, 0, 255, 0, 2);

                    //VideoAnalyse.deviation_light_gauss(fr_im_cl);

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
                    var ps = Detection.detectLineDiff(mat2, 5);
                    var psf = new System.Drawing.PointF[0];
                    mat3 = UtilOpenCV.drawChessboard(mat3, chess_size, ref psf, false, false, CalibCbType.AdaptiveThresh);
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
                    var ps = Detection.detectLineDiff(mat, 7);

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

                    UtilOpenCV.drawPointsF(mat, ps, 0, 255, 0, 2);

                    //UtilOpenCV.drawPointsF(mat, ps_xy, 255, 255, 0, 2);
                    //imBox_base.Image = im2;
                    imageBox2.Image = scanner.cameraCV.undist(fr.im);
                    imageBox1.Image = scanner.cameraCV.undist(mat);


                }

                else if (fr.frameType == FrameType.LasHand)
                {
                    var mat1 = fr.im.Clone();
                    if (cameraCVcommon != null)
                    {

                        var ps = Detection.detectLine(cameraCVcommon.undist(fr.im));
                        mat1 = cameraCVcommon.undist(mat1);
                        //fr.im = cameraCVcommon.undist(fr.im);
                        UtilOpenCV.drawPointsF(mat1, ps, 0, 255, 0);
                    }
                    var psf = new System.Drawing.PointF[0];
                    mat1 = UtilOpenCV.drawChessboard(mat1, chess_size, ref psf, false, false, CalibCbType.AdaptiveThresh);
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
                    var psf = new System.Drawing.PointF[0];
                    imBox_debug1.Image = UtilOpenCV.drawChessboard(fr.im, chess_size, ref psf, false, false, CalibCbType.AdaptiveThresh);
                    imageBox1.Image = UtilOpenCV.drawInsideRectChessboard(fr.im, chess_size);

                    cameraCVcommon.compPos(fr.im, PatternType.Chess, chess_size);


                }
                else if (fr.frameType == FrameType.Pattern)
                {

                    
                    var corn = new System.Drawing.PointF[0];
                    //imageBox1.Image = UtilOpenCV.drawInsideRectCirc(fr.im, chess_size);
                    //imageBox1.Image = GeometryAnalyse.findCirclesIter(fr.im.Clone(), ref corn, chess_size);
                    imageBox1.Image = FindCircles.findCircles(fr.im, ref corn, chess_size);
                    imageBox2.Image = cameraCVcommon.undist(fr.im.Clone());
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
                    if (tab.Controls[i] is GlControl)
                    {
                        Console.WriteLine("graph");
                    }

                    else
                    {
                        tab.Controls[i].Location = new Point((int)(tab.Controls[i].Location.X * k), (int)(tab.Controls[i].Location.Y * k));
                        tab.Controls[i].Size = new Size((int)(tab.Controls[i].Size.Width * k), (int)(tab.Controls[i].Size.Height * k));
                        tab.Controls[i].Font = new Font(tab.Controls[i].Font.Name, (float)(tab.Controls[i].Font.Size * k));
                    }

                }
            }

            for (int j = 0; j < win_tab_diff.Controls.Count; j++)
            {
                var tab = (TabPage)win_tab_diff.Controls[j];
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
            CameraCV.calibrMonit(imBox_pattern, new ImageBox[] { imBox_mark1, imBox_mark2 }, patt, txBx_photoName.Text, GL1);
            GL1.printDebug(debugBox);
        }
        private void but_calib_Start_Click(object sender, EventArgs e)
        {

            CameraCV.calibrMonit(imBox_pattern, new ImageBox[] { imBox_input_1, imBox_input_2 }, patt, txBx_photoName.Text, null);
        }
        private void tr_Persp_Scroll(object sender, EventArgs e)
        {
            var trbar = (TrackBar)sender;
            var ind = Convert.ToInt32(trbar.AccessibleName);
            var txbox = textBoxes_Persp[ind];
            var mult = Convert.ToDouble(txbox.Text);
            var val = mult * (double)trbar.Value / 100;
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
            UtilOpenCV.generateImagesFromAnotherFolderStereo(new string[] { @"virtual_stereo\test1\monitor_0", @"virtual_stereo\test1\monitor_1" }, GL1,
                new CameraCV(cameraMatrix_dist, cameraDistortionCoeffs_dist, new Size(400, 400)));//size not right!!!!!!!

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
            con1.Connection(port_tcp, pulse);

            Thread tcp_thread = new Thread(recieve_tcp);
            tcp_thread.Start(con1);

        }
        bool printing = false;
        void recieve_tcp(object obj)
        {

            var con = (TCPclient)obj;
            //  if (con == null) return;
            while (con.is_connect())
            {
                if (printing)
                {
                    var res = con.reseav();
                    if (res != null)
                    {
                        if (res.Length > 3)
                        {
                            //Console.WriteLine("tcp res: " + res);


                            if (res.Contains("finish"))
                            {
                                if (surface_type == 1) compensation = false;
                                laserLine?.set_home_z(); Thread.Sleep(2);
                                laserLine?.set_home_z(); Thread.Sleep(20);
                                laserLine?.set_home_z(); Thread.Sleep(20);
                                laserLine?.set_home_z(); Thread.Sleep(20);
                                laserLine?.set_home_z(); Thread.Sleep(20);
                                laserLine?.set_home_z(); Thread.Sleep(20);
                                laserLine?.set_home_z(); Thread.Sleep(20);
                                laserLine?.set_home_z(); Thread.Sleep(20);
                                laserLine?.set_home_z(); Thread.Sleep(20);
                                laserLine?.set_home_z(); Thread.Sleep(20);
                                Console.WriteLine("COMPENSATION STOP");

                            }
                            else if (res.Contains("start"))
                            {
                                if (surface_type == 1) compensation = true;
                                Console.WriteLine("COMPENSATION START");
                            }
                            else
                            {
                                resend_rob_to_ard_extr(res, laserLine);
                            }


                        }
                    }



                }
                Thread.Sleep(2);
            }
        }

        void resend_rob_to_ard_extr(string mes, LaserLine ard)
        {
            if (ard == null) return;
            if (mes == null) return;
            if (mes.Length < 2) return;
            mes = mes.Trim();
            Console.WriteLine(mes);
            var vals_str = mes.Split(' ');
            if (vals_str.Length != 2) return;
            var val = Convert.ToDouble(vals_str[0]);
            var var = Convert.ToDouble(vals_str[1]);
            if (Math.Abs(var - 25) < 0.1)
            {
                var div = LaserLine.vel_pist_to_ard(val);
                for (int i = 0; i < 5; i++)
                {
                    ard.set_dir_disp(-1); Thread.Sleep(2);
                }
                for (int i = 0; i < 5; i++)
                {
                    ard.set_div_disp(div); Thread.Sleep(2);
                }

            }
            else
            {
                ard.send((int)val, (int)var);
            }
            Console.WriteLine("resend_from_rob: " + val + " " + var);

            //ard.set_div_disp(vel);
        }

        private void but_res_pos1_Click(object sender, EventArgs e)
        {
            var posRob = positionFromRobot(con1);
            Console.WriteLine(posRob);
            if (posRob != null)
            {
                nameX.Text = posRob.X.ToString();
                nameY.Text = posRob.Y.ToString();
                nameZ.Text = posRob.Z.ToString();
                nameA.Text = posRob.A.ToString();
                nameB.Text = posRob.B.ToString();
                nameC.Text = posRob.C.ToString();
            }

        }

        private void but_res_pos_2_Click(object sender, EventArgs e)
        {
            var posRob = positionFromRobot(con1);
            nameX2.Text = posRob.X.ToString();
            nameY2.Text = posRob.Y.ToString();
            nameZ2.Text = posRob.Z.ToString();
            nameA2.Text = posRob.A.ToString();
            nameB2.Text = posRob.B.ToString();
            nameC2.Text = posRob.C.ToString();
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
        RobotFrame positionFromRobot(TCPclient con)
        {
            if (con != null)
            {
                try
                {
                    con1.send_mes("f\n");
                    Thread.Sleep(50);
                    var res = con1.reseav();
                    if (res == null) return null;
                    var res_pos = res.Split('\n');
                    foreach (var pos in res_pos)
                    {
                        if (pos.Length > 20)
                        {
                            Console.WriteLine(pos);
                            var pos2 = pos.Trim();
                            var res_s = pos2.Split(' ');
                            double x = Convert.ToDouble(res_s[0]);
                            double y = Convert.ToDouble(res_s[1]);
                            double z = Convert.ToDouble(res_s[2]);
                            double a = Convert.ToDouble(res_s[3]);
                            double b = Convert.ToDouble(res_s[4]);
                            double c = Convert.ToDouble(res_s[5]);
                            Console.WriteLine(res);
                            return new RobotFrame(x, y, z, a, b, c, 0, 0, 0, current_robot);
                        }
                    }
                    return null;

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
        string positionFromRobot_str(TCPclient con)
        {
            if (con != null)
            {
                try
                {
                    con1.send_mes("f\n");
                    Thread.Sleep(50);
                    var res = con1.reseav();
                    if (res == null || res.Length < 10)
                    {
                        return null;
                    }
                    res = res.Trim();
                    Console.WriteLine(res);
                    return res;
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
            nameX_in.Text = posRob.X.ToString();
            nameY_in.Text = posRob.Y.ToString();
            nameZ_in.Text = posRob.Z.ToString();
            nameA_in.Text = posRob.A.ToString();
            nameB_in.Text = posRob.B.ToString();
            nameC_in.Text = posRob.C.ToString();
        }


        public Mat get_im1()
        {
            return (Mat)imageBox1.Image;
        }



        public void send_rob_fr(RobotFrame frame_rob)
        {
            try_send_rob(debugBox.Text + "\n");
            try_send_rob("s\n");
        }
        /*private void but_save_im_base1_Click(object sender, EventArgs e)
        {
            //var im = (Mat)imBox_base_1.Image;
            //im.Save("im1.png");

            var im = (Mat)imageBox1.Image;
            im.Save("im2.png");
            //здесь будет отправка im1.png на сервер
        }*/

        /*
        private async void but_save_im_base1_Click(object sender, EventArgs e)
        {
            // Сохраняем изображение
            var im = (Mat)imageBox1.Image;
            string imagePath = "im2.png";
            im.Save(imagePath);

            // URL сервера
            string serverUrl = "http://127.0.0.1:8000/segment";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        // Добавляем файл в запрос
                        var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(imagePath));
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                        content.Add(fileContent, "file", "im2.png");

                        // Отправляем POST-запрос
                        var response = await client.PostAsync(serverUrl, content);

                        // Проверяем ответ
                        if (response.IsSuccessStatusCode)
                        {
                            string jsonResponse = await response.Content.ReadAsStringAsync();

                            // Парсим JSON-ответ
                            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<SegmentationResult>(jsonResponse);

                            if (result.Message == "на фото не обнаружено раны")
                            {
                                MessageBox.Show("Раны не обнаружены.");
                                return;
                            }

                            // Отрисовываем полигоны
                            Mat segmentedImage = im.Clone();
                            foreach (var polygon in result.Polygons)
                            {
                                var points = polygon.Select(p => new Point((int)p[0], (int)p[1])).ToArray();
                                CvInvoke.Polylines(segmentedImage, new VectorOfPoint(points), true, new MCvScalar(0, 0, 255), 2);
                            }

                            // Показываем изображение с полигоном
                            CvInvoke.Imshow("Сегментация", segmentedImage);
                        }
                        else
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Ошибка: {responseString}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        // Класс для парсинга ответа сервера
        public class SegmentationResult
        {
            public string Message { get; set; }
            public List<List<float[]>> Polygons { get; set; }
        }

        /*
                        if (response.IsSuccessStatusCode)
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Сегментация выполнена успешно: {responseString}");
                        }
                        else
                        {
                            MessageBox.Show($"Ошибка при сегментации: {response.StatusCode}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }*/

        // Хранилище для точек полигонов
        private List<List<Point>> polygons = new List<List<Point>>();

        // Для отслеживания перемещения точек
        private bool isDragging = false;
        private Point dragStart;
        private int selectedPolygonIndex = -1;
        private int selectedPointIndex = -1;

        // Исходное изображение
        private Bitmap originalImage;
        private Bitmap editedImage;


        // Загрузка изображения и полигонов
        private void LoadImageAndPolygons(Bitmap image, List<List<Point>> receivedPolygons)
        {
            originalImage = image;
            editedImage = new Bitmap(originalImage);
            polygons = receivedPolygons;

            // Отобразить изображение в PictureBox
            pictureBox2.Image = editedImage;
        }

        private void DrawPolygons()
        {
            if (originalImage == null) return;

            // Копируем исходное изображение
            editedImage = new Bitmap(originalImage);
            using (Graphics g = Graphics.FromImage(editedImage))
            {
                Pen pen = new Pen(Color.Red, 2);
                Brush brush = Brushes.Blue;

                foreach (var polygon in polygons)
                {
                    if (polygon.Count > 1)
                        g.DrawPolygon(pen, polygon.ToArray());

                    foreach (var point in polygon)
                    {
                        g.FillEllipse(brush, point.X - 5, point.Y - 5, 10, 10);
                    }
                }
            }

            pictureBox2.Image = editedImage;
            pictureBox2.Refresh(); // Принудительно обновляем отображение
        } 
    
        /*
        namespace PolygonEditor.Definitions
        {
            public enum ClickMode
            {
                None,
                ChoosingRectanglePoints,
                MovingElement,
            }
        
            public struct MovingVerticeState
            {
                public VerticePoint selectedVertice;
                public PointF hitPoint;

                public MovingVerticeState(VerticePoint currentlyMovingVertice, PointF hitPoint)
                {
                    this.selectedVertice = currentlyMovingVertice;
                    this.hitPoint = hitPoint;
                }

                public bool IsDuringMovement
                {
                    get
                    {
                        return selectedVertice != null;
                    }
                }

                public void Clear()
                {
                    selectedVertice = null;
                }

                public (double x, double y) GetMoveVectorAndUpdateHitPoint(double x, double y)
                {
                    double ret_x = x - hitPoint.X;
                    double ret_y = y - hitPoint.Y;

                    hitPoint.X = (float)x;
                    hitPoint.Y = (float)y;
                    return (ret_x, ret_y);
                }
            }

            public struct MovingEdgeState
            {
                public Line selectedEdge;
                public PointF hitPoint;

                public MovingEdgeState(Line currentlyMovingEdge, PointF hitPoint)
                {
                    this.selectedEdge = currentlyMovingEdge;
                    this.hitPoint = hitPoint;
                }

                public bool IsDuringMovement
                {
                    get
                    {
                        return selectedEdge != null;
                    }
                }

                public void Clear()
                {
                    selectedEdge = null;
                }

                public (double x, double y) GetMoveVectorAndUpdateHitPoint(double x, double y)
                {
                    double ret_x = x - hitPoint.X;
                    double ret_y = y - hitPoint.Y;

                    hitPoint.X = (float)x;
                    hitPoint.Y = (float)y;
                    return (ret_x, ret_y);
                }
            }

            public struct MovingPolygonState
            {
                public Polygon selectedPolygon;
                public PointF hitPoint;

                public MovingPolygonState(Polygon currentlyMovingPolygon, PointF hitPoint)
                {
                    this.selectedPolygon = currentlyMovingPolygon;
                    this.hitPoint = hitPoint;
                }

                public bool IsDuringMovement
                {
                    get
                    {
                        return selectedPolygon != null;
                    }
                }

                public void Clear()
                {
                    selectedPolygon = null;
                }

                public (double x, double y) GetMoveVectorAndUpdateHitPoint(double x, double y)
                {
                    double ret_x = x - hitPoint.X;
                    double ret_y = y - hitPoint.Y;

                    hitPoint.X = (float)x;
                    hitPoint.Y = (float)y;
                    return (ret_x, ret_y);
                }

            }

            public struct AddingEdgeConstraintOrVerticeState
            {
                public Line selectedEdge;
                public IEdgeConstraint constraint;
                public PointF hitPoint;

                public AddingEdgeConstraintOrVerticeState(Line currentlyMovingEdge, PointF hitPoint)
                {
                    this.selectedEdge = currentlyMovingEdge;
                    constraint = null;
                    this.hitPoint = hitPoint;
                }

                public bool IsDuringMovement
                {
                    get
                    {
                        return selectedEdge != null;
                    }
                }

                public void Clear()
                {
                    selectedEdge = null;
                }
            }

            public struct DeletingVerticeState
            {
                public VerticePoint selectedVertice;

                public DeletingVerticeState(VerticePoint currentlyDeletingVertice)
                {
                    this.selectedVertice = currentlyDeletingVertice;
                }

                public bool IsDuringMovement
                {
                    get
                    {
                        return selectedVertice != null;
                    }
                }

                public void Clear()
                {
                    selectedVertice = null;
                }
            }

            public struct DeletingPolygonState
            {
                public Polygon selectedPolygon;

                public DeletingPolygonState(Polygon selectedPolygon)
                {
                    this.selectedPolygon = selectedPolygon;
                }

                public bool IsDuringMovement
                {
                    get
                    {
                        return selectedPolygon != null;
                    }
                }

                public void Clear()
                {
                    selectedPolygon = null;
                }
            }
        }

        ClickMode clickMode = ClickMode.MovingElement;
        private readonly PolygonsContainer polygonsContainer;
        private void drawAreaBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                switch (clickMode)
                {
                    case ClickMode.ChoosingRectanglePoints:
                        polygonsContainer.BuildNextPolygonPart(e.X, e.Y);
                        break;
                    case ClickMode.MovingElement:
                        polygonsContainer.StartMovingElementFrom(e.X, e.Y);
                        break;
                }
            else if (e.Button == MouseButtons.Right)
            {

                var resVertice = polygonsContainer.StartVerticeDeleting(e.X, e.Y);
                if (resVertice)
                {
                    ShowVerticeContextMenu(e.X, e.Y);
                    return;
                }

                var resEdge = polygonsContainer.StartAddingEdgeConstraintOrVertice(e.X, e.Y);
                if (resEdge.isEdgeUnderHit)
                {
                    ShowEdgeContextMenu(resEdge.constraint, e.X, e.Y);
                    return;
                }

                var resPolygon = polygonsContainer.StartPolygonDeleting(e.X, e.Y);
                if (resPolygon)
                    ShowPolygonContextMenu(e.X, e.Y);

            }
        }
        private void drawAreaBox_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                if (clickMode == ClickMode.ChoosingRectanglePoints)
                {
                    clickMode = ClickMode.MovingElement;
                    polygonsContainer.FinishPolygonBuilding(e.X, e.Y);
                }
        }

        private void drawAreaBox_MouseMove(object sender, MouseEventArgs e)
        {
            switch (clickMode)
            {
                case ClickMode.ChoosingRectanglePoints:
                    polygonsContainer.UpdateNewEdgeEnd(e.X, e.Y);
                    break;
                case ClickMode.MovingElement:
                    polygonsContainer.MoveElement(e.X, e.Y);
                    break;
            }
        }
        private void drawAreaBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                switch (clickMode)
                {
                    case ClickMode.MovingElement:
                        polygonsContainer.FinishElementMoving();
                        break;
                }
        }
        */


        private async void but_save_im_base1_Click(object sender, EventArgs e)
        {
            // Отправка изображения на сервер
            string serverUrl = "http://localhost:8000/segment";
            string imagePath = "im2.png";
            var im = (Mat)imageBox1.Image;
            im.Save(imagePath);

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    using (var content = new MultipartFormDataContent())
                    {
                        var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(imagePath));
                        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/png");
                        content.Add(fileContent, "file", "im2.png");

                        var response = await client.PostAsync(serverUrl, content);

                        if (response.IsSuccessStatusCode)
                        {
                            var jsonResponse = await response.Content.ReadAsStringAsync();
                            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<SegmentationResult>(jsonResponse);

                            if (result.Message == "на фото не обнаружено раны")
                            {
                                MessageBox.Show("Раны не обнаружены.");
                                return;
                            }

                            // Загружаем изображение
                            Bitmap image = new Bitmap(imagePath);

                            // Преобразуем координаты полигонов
                            var polygonsData = result.Polygons
                                .Select(p => p.Select(point => new Point((int)point[0], (int)point[1])).ToList())
                                .ToList();

                            LoadImageAndPolygons(image, polygonsData);
                            DrawPolygons();
                        }
                        else
                        {
                            string responseString = await response.Content.ReadAsStringAsync();
                            MessageBox.Show($"Ошибка: {responseString}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        public class SegmentationResult
        {
            public string Message { get; set; }
            public List<List<float[]>> Polygons { get; set; }
        }


        private void but_hydro_model_grav_Click(object sender, EventArgs e)//modelir_hydro!!!!!
        {

            comp_hydro_screen();
        }

        void comp_hydro_screen()
        {
            var ks = new double[] { 0.25, 0.5, 0.75 };
            var ws = new double[] { 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 1.0, 1.2 };

            var dz = ks[ks_i] * ws[ws_i];
            var step = ws[ws_i] * 2.5;

            patt_config.step = step;
            traj_config.Line_width = ws[ws_i];
            traj_config.dz = dz;


            propGrid_pattern.SelectedObject = patt_config;
            propGrid_traj.SelectedObject = traj_config;

            propGrid_pattern.Update();
            propGrid_traj.Update();
            comp_hydro();
            Console.WriteLine(ks[ks_i] + " " + ws[ws_i]);
            ws_i++;
            if(ws_i>=ws.Length)
            {
                ks_i++;
                ws_i = 0;
            }
            if (ks_i >=ks.Length)
            {
                ks_i = 0;
            }


          
        }
        void comp_hydro()
        {
            var contours = new List<List<Point3d_GL>>();
            var surfaces = new List<Polygon3d_GL[]>();
            var r = patt_config.r;

            var name_scan = "scan_virt_flat.stl";
            Polygon3d_GL[] surf_scan;
            bool load_scan = true;
            // bool selected_scan
            if (load_scan)
            {
                GL1.remove_buff_gl_id(name_scan);
                var model = new Model3d(name_scan);
                //name_scan = GL1.addMesh(model.mesh, PrimitiveType.Triangles, null, name_scan);
                surf_scan = model.pols;


                //var selected_obj = selected_object(); if (selected_obj == null) return;
                //surf_scan = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[selected_obj].vertex_buffer_data);
            }
            else
            {
                name_scan = GL1.addFlat3d_XY_zero(0, null, "name_scan", 60);
                surf_scan = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[name_scan].vertex_buffer_data);
                GL1.remove_buff_gl_id(name_scan);
            }

            var flat_xy = "flat_xy_h";
            GL1.remove_buff_gl_id(flat_xy);
            flat_xy = GL1.addFlat3d_XY_zero(0, Color3d_GL.black(), flat_xy, 60);
            var surf = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[flat_xy].vertex_buffer_data);
            //GL1.remove_buff_gl_id(flat_xy);

            var flat_proj = surf_scan;

            for (int i = 0; i < traj_config.layers; i++)
            {
                var cont = SurfaceReconstraction.gen_random_cont_XY(r, 30, 0, new Point3d_GL(patt_config.dim_x, patt_config.dim_y, 0)).ToList();
                contours.Add(cont);
                var surf_scan_cur = Polygon3d_GL.add_arr(flat_proj, new Point3d_GL(0, 0, traj_config.dz * i));
                surfaces.Add(surf_scan_cur);
            }
            ///var traj_path = PathPlanner.gen_traj_2d(contours, traj_config, patt_config, GL1);




            var traj_path = PathPlanner.generate_3d_traj_diff_surf_test(surfaces, contours, traj_config, patt_config, GL1);

            if (traj_path == null) return;
            traj_path.translate(new Point3d_GL(traj_config.Off_x, traj_config.Off_y, traj_config.Off_z));

            var hydro = PathModeling.modeling_print_path_3d(surf, traj_path, traj_config, GL1);

            GL1.remove_buff_gl_id("model_traj");
            GL1.addMesh(Polygon3d_GL.toMesh(hydro)[0], PrimitiveType.Triangles, Color3d_GL.white(), "model_traj");

            GL1.remove_buff_gl_id("surf");
             GL1.addMesh(Polygon3d_GL.toMesh(surf)[0], PrimitiveType.Triangles, Color3d_GL.white(), "surf");
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
                if (con1 == null) { return; }
                con1.send_mes("q\n");
                con1.close_con();
                con1 = null;
            }
            catch
            {

            }
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
            GL1.orientZscroll(track.Value);
        }

        private void trackOy_Scroll(object sender, EventArgs e)
        {
            var track = (TrackBar)sender;
            GL1.orientYscroll(track.Value);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            

            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path,scanner_config,traj_config,patt_config);
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


        private void tree_models_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                prop_grid_model.SelectedObject = GL1.buffersGl.objs[e.Node.Text];
                prop_grid_model.Text = e.Node.Text;
                if (ModifierKeys == Keys.Control)
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
        //-----------------------------------
        string[] selected_nodes()
        {
            List<string> nodes = new List<string>();
            for (int i = 0; i < tree_models.Nodes.Count; i++)
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
        //-----------------------------------
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
        //-----------------------------------
        private void but_printer_traj_fab_Click(object sender, EventArgs e)
        {
            var cont_v = new Point3d_GL[]
            {
                new Point3d_GL(0,0,0),
                new Point3d_GL(30,0,0),
                new Point3d_GL(30,20,0),
                new Point3d_GL(20,20,0),
                new Point3d_GL(20,10,0),
                new Point3d_GL(10,10,0),
                new Point3d_GL(10,20,0),
                new Point3d_GL(0,20,0)
            };
            var contours = new List<List<Point3d_GL>>();
            for(int i = 0; i < traj_config.layers; i++)
            {
                var cont = SurfaceReconstraction.gen_random_cont_XY(10, 30, 0, new Point3d_GL(10,10,0.4*i)).ToList();
                //cont = cont_v.ToList();
                contours.Add(cont);
            }
            var traj_path = PathPlanner.gen_traj_2d(contours, traj_config, patt_config, GL1);
            traj_path.translate(new Point3d_GL(traj_config.Off_x, traj_config.Off_y, traj_config.Off_z));
            var patterns = traj_path.to_ps_by_layers();

            var pattern = Point3d_GL.unifPoints2d(patterns);

           // GL1.addLineMeshTraj(pattern.ToArray(), Color3d_GL.red());
            //var pattern = PathPlanner.gen_traj_3d_pores(patt_config, traj_config);
            var traj = PathPlanner.ps_to_matr(pattern);
            var prog = PathPlanner.generate_printer_prog(traj, traj_config);
            debugBox.Text = prog;
            
        }

        private void but_scan_virt_Click(object sender, EventArgs e)
        {
            scan_fold_name = box_scanFolder.Text;
            box_scanFolder.Text = "scan_virt_" + DateTime.Now.Month.ToString() + "_"
            + DateTime.Now.Day.ToString() + "_" +
            DateTime.Now.Hour.ToString() + "_"
            + DateTime.Now.Minute.ToString() + "_"
            + DateTime.Now.Second.ToString();
            var coords = GL1.transRotZooms[0].off_x + " "
                + (-GL1.transRotZooms[0].off_y) + " "
                + GL1.transRotZooms[0].zoom * GL1.transRotZooms[0].off_z + " " +
                GL1.transRotZooms[0].xRot + " " +
                  GL1.transRotZooms[0].yRot + " " +
                  GL1.transRotZooms[0].zRot + " ";
            var pos = new RobotFrame(coords);
            video_scan_name = pos.ToStr(" ",true);
            video_scan_name = "1";
            boxN.Text = "120";
            var n = Convert.ToInt32(boxN.Text);
            GL1.start_animation(n);
            var folder_scan = box_scanFolder.Text;
            UtilOpenCV.saveImage(imBox_mark1, imBox_mark2, "1.png", folder_scan + "\\orig");
            startWrite(1, n);
            startWrite(2, n);
        }

        private void but_start_anim_Click(object sender, EventArgs e)
        {
            //test_gen_traj(0);
            //GL1.start_animation(frames_rob.Count-2, this);

            //test_gen_traj(0);
            GL1.start_animation(100, this);
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
            DeviceArduino.find_ports(comboBox_portsArd); Thread.Sleep(100);
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
           // string video_path1 = ve_paths1[0];

            // string enc_path1 = ve_paths1[1];

            //var ve_paths2 = get_video_path(2, filepath);
           // string video_path2 = ve_paths2[0];
            // string enc_path2 = ve_paths2[1];

            string enc_path = ve_paths1[1];

            //var enc_pos_time = VideoAnalyse.analys_sync(enc_path);
           // enc_pos_time = VideoAnalyse.recomp_pos_sing_linear(enc_pos_time);
           // var inc_pos = VideoAnalyse.enc_pos(enc_pos_time);
            var pairs = VideoAnalyse.frames_sync_from_file(enc_path, lab_scan_pres);
        }
       
        private void but_set_z_pos_Click(object sender, EventArgs e)
        {
            var pos_z_mm = to_double_textbox(textB_set_z_pos, -100, 100);
            if(pos_z_mm!=double.NaN)
            {
                laserLine?.set_move_z(pos_z_mm);
            }            
        }

        private void but_z_home_Click(object sender, EventArgs e)
        {
            laserLine?.set_home_z();
        }

        private void but_set_z_div_Click(object sender, EventArgs e)
        {
            laserLine?.set_z_div(Convert.ToInt32(textB_set_z_div.Text));
        }

        private void prop_gr_scan_SelectedGridItemChanged(object sender, SelectedGridItemChangedEventArgs e)
        {
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path, scanner_config, traj_config, patt_config);
        }
        private void prop_gr_scan_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path, scanner_config, traj_config, patt_config);
        }
        private void but_flange_calib_basis_Click(object sender, EventArgs e)
        {
            //var stereo_cal_1 = textB_scan_path.Text.Split('\\').Reverse().ToArray()[0];
            var stereo_cal_1 = get_folder_name_cam(textB_scan_path.Text);
            //Console.WriteLine(stereo_cal_1+" "+ stereo_cal_2);
            
            var cams_path = new string[] { @"cam1\" + stereo_cal_1, @"cam2\" + stereo_cal_1 }; //var reverse = true;
            //cams_path = new string[] { openGl_folder+"/monitor_0/distort", openGl_folder + "/monitor_1/distort" };  reverse = false;
            //var frms_stereo = FrameLoader.loadImages_stereoCV(cams_path[0], cams_path[1], FrameType.Pattern, scanner_config.rotate_cam);
            
            var frms_stereo = FrameLoader.load_rob_frames(cams_path[0]);

            var cam1_conf_path = textB_cam1_conf.Text;
            var cam2_conf_path = textB_cam2_conf.Text;
            var cam1 = CameraCV.load_camera(cam1_conf_path);
            var cam2 = CameraCV.load_camera(cam1_conf_path);
            var stereo = new StereoCamera(new CameraCV[] { cam1, cam2 });
            stereocam_scan = stereo;
            //stereocam_scan.calibrate_stereo(frms_stereo, PatternType.Mesh,chess_size);
            chess_size = new Size(6, 7);
            var markSize = 10f;
            //stereocam_scan.calibrate_basis_rob_xyz(frms_stereo, PatternType.Mesh, chess_size, markSize);

            //stereocam_scan.calibrate_basis_rob_abc(frms_stereo, PatternType.Mesh, chess_size, markSize);
            var ms_check = StereoCamera.calibrate_stereo_rob_handeye(cam1, frms_stereo, PatternType.Mesh, chess_size, markSize, "bfs_cal.txt", current_robot);
            for (int i=0; i<ms_check.Count;i++)
            {
                GL1.addFrame(ms_check[i]);
            }
            comboImages.Items.AddRange(frms_stereo);
        }

        private string get_folder_name_cam(string path)
        {
            var paths = textB_scan_path.Text.Split('\\');
            bool fl = false;
            var ret = "";
            for(int i = 0; i < paths.Length; i++)
            {
                
                if(fl)
                {
                    ret += (paths[i] + @"\");
                }

                if (paths[i] == "cam1" || paths[i] == "cam2")
                {
                    fl = true;
                }
            }
            return ret;
        }
        #endregion

        #region cameraUtil
        void capturingVideo(object sender, EventArgs e)
        {
            drawCameras((VideoCapture)sender);
        }
        void capturingVideo_sam(object sender, EventArgs e)
        {
            drawCameras_sam((VideoCapture)sender);
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
                    var psf = new System.Drawing.PointF[0];
                    imb_base[ind - 1].Image = UtilOpenCV.drawChessboard(mat, chess_size,ref psf, false,false,CalibCbType.FastCheck);
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
                sb_enc?.Append(laserLine?.get_las_pos() + " " + videoframe_counts[ind - 1] + " " + ind + " ");
                //if (sb_enc == null) Console.WriteLine("NULL!");
                //sb_enc?.Append("0" + " " + videoframe_counts[ind - 1] + " " + ind + " ");
                sb_enc?.Append(DateTime.Now.Ticks + " " + videoframe_counts[ind - 1] + " " + ind + " ");
                sb_enc?.Append("\n");
                //video_writer[ind - 1]?.Write(mat);
                video_mats[ind-1].Add(mat.Clone());
                //var p = Detection.detectLineSensor(mat)[0];
                //Console.WriteLine(ind + " " + video_mats[ind-1].Count+" "+p);
                sb_enc?.Append(laserLine?.get_las_pos() + " " + videoframe_counts[ind - 1] + " " + ind + " ");
                //sb_enc?.Append("0" + " " + videoframe_counts[ind - 1] + " " + ind + " ");
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

        void save_video_sam(int ind, int w, int h)
        {
            laserLine?.send_pos_laser(0);
            Thread.Sleep(2);
            laserLine?.send_pos_laser(0);
            Thread.Sleep(2);
            int fcc = VideoWriter.Fourcc('h', '2', '6', '4'); //'M', 'J', 'P', 'G';'m', 'p', '4', 'v';'M', 'P', '4', 'V';'H', '2', '6', '4';'h', '2', '6', '4'
            int fps = 30;
            var dir = "cam" + (ind+1).ToString() + "\\" + box_scanFolder.Text;
            Directory.CreateDirectory(dir);
            video_scan_name = video_scan_name.Replace("\n"," ");
            string name = dir + "\\" + video_scan_name + ".mp4";
            Console.WriteLine("wr" + " " + w + " " + h+" "+fps);
            video_writer[ind ] = new VideoWriter(name, -1, fps, new Size(w, h), true);
            //var reswr = video_writer[ind ].Set(VideoWriter.WriterProperty.Quality, 100);
            //Console.WriteLine(reswr);
            for (int i = 0; i < video_mats[ind].Count; i++)
            {
                video_writer[ind].Write(video_mats[ind][i]);
                //var p = Detection.detectLineSensor(video_mats[ind - 1][i])[0];
                //Console.WriteLine(ind + " "  + p);
            }
            video_mats[ind ] = null;
            video_writer[ind ].Dispose();

            

        }
        void startWrite(int ind,int count)
        {
            videoframe_counts_stop[ind - 1] = count;
            videoframe_counts[ind - 1] = 0;
        }
        void startWrite_sam(int ind, int count)
        {
            videoframe_counts_stop[ind] = count;
            videoframe_counts[ind] = 0;
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
            videoCaptures[number] = capture;
            //capture.SetCaptureProperty(CapProp.
            capture.Set(CapProp.FrameWidth, cameraSize.Width);
            //capture.ser
            // capture.SetCaptureProperty(CapProp.FrameHeight, cameraSize.Height);
            //capture.SetCaptureProperty(CapProp.Exposure, -4);
            //capture.SetCaptureProperty(CapProp.Fps, 60);
            Console.WriteLine(capture.Get(CapProp.FrameWidth) + " " + capture.Get(CapProp.FrameHeight)+" "+ capture.Get(CapProp.Fps));

            //capture.SetCaptureProperty(CapProp.Contrast, 30);
            camera_ind.Add(capture.Ptr);
            capture.ImageGrabbed += capturingVideo;
            capture.Start();
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
                    if (camera_frame_time.Count > fps_c)
                    {
                        camera_frame_time.RemoveAt(0);
                    }
                    if (camera_frame_time.Count == fps_c)
                    {
                        var dt = (int)(camera_frame_time[camera_frame_time.Count - 1] - camera_frame_time[0]);
                        fps1 = Math.Round((double)fps_c * 1000 / dt, 1);
                    }

                    imageBox1.Image = mat_global[0];

                    imProcess(mat_global[0], 1);


                }
                else if ((camera_ind.Count > 1) && (cap.Ptr == camera_ind[1]))
                {
                    /*var mat = new Mat();
                    cap.Retrieve(mat);
                    imProcess(mat, 2);*/
                    cap.Retrieve(mat_global[1]);
                    imageBox2.Image = mat_global[1];

                    imProcess(mat_global[1], 2);

                    //imBox_base.Image = stereoProc(mat_global[0], mat_global[1]);
                }
            }
        }

        //--------------------------------------------------------
        private void videoStart_sam(int number)
        {
            var capture = new VideoCapture(number);
            videoCaptures[number] = capture;
            //capture.SetCaptureProperty(CapProp.
            capture.Set(CapProp.FrameWidth, cameraSize.Width);

            // capture.SetCaptureProperty(CapProp.FrameHeight, cameraSize.Height);
            //capture.SetCaptureProperty(CapProp.Exposure, -4);
            //capture.SetCaptureProperty(CapProp.Fps, 60);
           // capture.Set(CapProp.AutoExposure, 1);
           // capture.Set(CapProp.Exposure, -8);

            cameraSize.Width =(int) capture.Get(CapProp.FrameWidth);
            cameraSize.Height = (int)capture.Get(CapProp.FrameHeight);
            Console.WriteLine(cameraSize.Width.ToString() + " " + cameraSize.Height.ToString() + " " + capture.Get(CapProp.Fps));
            //capture.SetCaptureProperty(CapProp.Contrast, 30);
            camera_ind_ptr[number] = capture.Ptr;

            capture.ImageGrabbed += capturingVideo_sam;
            capture.Start();
        }
        void drawCameras_sam(VideoCapture cap)
        {
            //Console.WriteLine("dr_cam");
            var ind_cam = camera_ind_ptr.IndexOf(cap.Ptr);
            if(ind_cam<10 && ind_cam>=0)
            {
                cap.Retrieve(mat_global[ind_cam]);
                for (int i = 0; i < imb_main.Length; i++)
                {
                    if (ind_cam == imb_ind_cam[i])
                    {
                        //try
                        {
                            imb_main[i].Image = mat_global[ind_cam];
                            imProcess_sam(mat_global[ind_cam], i);
                        }
                        //catch
                        {

                        }
                    }
                }
            }
            
        }

        void comp_fps()
        {
            camera_frame_time.Add(DateTime.Now.Ticks / 10000);
            int fps_c = 100;
            if (camera_frame_time.Count > fps_c)
            {
                camera_frame_time.RemoveAt(0);
            }
            if (camera_frame_time.Count == fps_c)
            {
                var dt = (int)(camera_frame_time[camera_frame_time.Count - 1] - camera_frame_time[0]);
                fps1 = Math.Round((double)fps_c * 1000 / dt, 1);
            }
        }
        void imProcess_sam(Mat mat, int ind)
        {
            //Console.WriteLine("im_proc");
            switch (imProcType)
            {

                case FrameType.Test:
                    //laserLine?.offLaserSensor();
                    //imb_base[ind-1].Image = mat;
                    break;
                case FrameType.MarkBoard:
                    System.Drawing.PointF[] points = null;
                    imb_base[ind].Image = UtilOpenCV.drawChessboard(mat, chess_size,ref  points, false, false, CalibCbType.Accuracy);
                    if (points != null) Console.WriteLine(points[0].X + " " + points[0].Y + " " + points[1].X + " " + points[1].Y + " " + points[2].X + " " + points[2].Y);
                    else Console.WriteLine("null");

                    break;

                case FrameType.CircleGrid:
                    //not work
                    System.Drawing.PointF[] points3 = null;
                    Console.WriteLine("CircleGrid");
                    var det = new SimpleBlobDetector();
                    points3 = CvInvoke.FindCirclesGrid(mat.ToImage<Gray, byte>(), new Size(6,7), CalibCgType.SymmetricGrid, det);
                    var mat_dr = UtilOpenCV.drawPointsF(mat, points3, 255, 0, 0);
                    mat_dr = UtilOpenCV.drawPointsF(mat_dr,new System.Drawing.PointF[] { points3[0] }, 255, 0, 0,3,true);
                    imb_base[ind].Image = mat_dr;
                    if (points3 != null) Console.WriteLine(points3[0].X + " " + points3[0].Y + " " + points3[1].X + " " + points3[1].Y + " " + points3[2].X + " " + points3[2].Y);
                    else Console.WriteLine("null");

                    break;
                case FrameType.Undist:
                    imb_base[ind].Image = stereocam.remapCam(mat, ind);
                    break;

                case FrameType.LasLin://laser sensor
                    //try
                    if(ind==0 && !scanning_status && comp_current_compens)
                    {
                        /* var ps = Detection.detectLineSensor(mat);
                         Console.Write(ps[0].X + " ");
                         laserLine?.setLaserCur((int)(10 * ps[0].X));
                         // Console.WriteLine((int)(10 * ps[0].X));

                         CvInvoke.Line(mat, new Point(350, 0), new Point(350, mat.Width - 1), new MCvScalar(0, 255, 0));
                         imb_base[ind].Image = UtilOpenCV.drawPointsF(mat, ps, 255, 0, 0, 1);
                         //Console.Write(laserLine?.reseav());
                         */
                        var laser_roi_X = 0; //(int)Regression.calcPolynSolv(koef_x, cur_pos_z);
                        var laser_roi_Y = 0; //(int)Regression.calcPolynSolv(koef_y, cur_pos_z);

                        //Console.Write(ps[0].X + " ");
                        //laserLine?.setLaserCur((int)(10 * ps[0].X));
                        //Console.WriteLine( ps[0].X);

                        //var cur = Regression.calcPolynSolv(koef, ps[0].X + laser_roi.X);

                        
                        //cur_pos_z = pos_z_mm;
                        //Console.WriteLine(pos_z_mm);
                        //laserLine?.test();

                        if (laserLine != null)
                        {
                            var cur_pos_z_c = laserLine.parse_pos_z();
                            if (cur_pos_z_c[0] > 0) cur_pos_z = cur_pos_z_c[0]/laserLine.steps_per_unit_z;
                            if (cur_pos_z_c[1] > 0) cur_pos_movm = Regression.calcPolynSolv(koef_byte_to_term, cur_pos_z_c[1]);
                        }
                        var cur_pos_z_abs = cur_pos_z - z_calibr_offset - z_syrenge_offset;  //!!!!!!!!!


                        if (window_auto)
                        {
                            laser_roi_X = (int)Regression.calcPolynSolv(koef_x, cur_pos_z_abs);
                            laser_roi_Y = (int)Regression.calcPolynSolv(koef_y, cur_pos_z_abs);
                        }

                        var laser_roi = new Rectangle(laser_roi_static.X + laser_roi_X, laser_roi_static.Y + laser_roi_Y, laser_roi_static.Width, laser_roi_static.Height);

                        if (laser_roi.Y < 0 || laser_roi.Y > mat.Height - laser_roi.Height - 2)
                        {
                            laser_roi.Y = mat.Height / 2;
                        }
                        if (laser_roi.X < 0 || laser_roi.X > mat.Width - laser_roi.Width - 2)
                        {
                            laser_roi.X = mat.Width / 2;
                        }
                        var mat_s = new Mat(mat, laser_roi);
                        var ps = Detection.detectLineSensor_v2(mat_s, 5, 2);
                        var x = (int)ps[0].X + laser_roi.X;
                        var y = (int)ps[0].Y + laser_roi.Y;


                        var cur = comp_cur_koef_las_z_pos(koef_las_z_pos, cur_pos_z_abs, ps[0].X + laser_roi.X);
                        if (compens_period && movm != null)
                        {
                            cur = movm.compute_cur_pos(cur_time_to_int()).pos1;
                        }
                        var comp_z_mm = cur - compens_gap;
                         comp_z_mm += (z_calibr_offset + z_syrenge_offset);    //!!!!!!!!!
                        //if (comp_z_mm < 37 || comp_z_mm > 25)
                        if (laserLine != null)
                        {
                            var pos_z_steps = (int)(comp_z_mm / 10 * laserLine?.steps_per_unit_z);
                            if (compensation)
                            {
                                //Console.WriteLine("pos_z_steps: " + pos_z_steps);
                                laserLine?.set_move_z(pos_z_steps);
                            }
                            else
                            {
                                laserLine?.test();
                            }

                        }
                        //else
                        {
                           
                        }
                        handler_compens_record(cur, cur_pos_movm, cur_pos_z_abs);
                        //label_cur_las_dist.BeginInvoke((MethodInvoker)(() => 
                       // label_cur_las_dist.Text = ("las_pos: "+(ps[0].X + laser_roi.X) +"\n "+ "pos_z_comp: " + comp_z_mm + "\n " + "cur_pos_z: " + cur_pos_z +  "\n cur_pos_movm: " + + cur_pos_movm)));

                        label_current_dist.BeginInvoke((MethodInvoker)(() =>
                        label_current_dist.Text = (
                        "Текущее расстояние: " +Math.Round( comp_z_mm,2) + "\n " + 
                        "Текущая позиция: " + Math.Round(cur_pos_z_abs, 2) + "\n " +
                        "Текущее расстояние ориг: " + Math.Round(cur_pos_z, 2) + "\n ")));


                        CvInvoke.Line(mat, new Point(0,y), new Point( mat.Width - 1,y), new MCvScalar(255, 0, 0));
                        CvInvoke.Line(mat, new Point(x, 0), new Point(x, mat.Height-1), new MCvScalar(0, 255, 0));
                        CvInvoke.Rectangle(mat, laser_roi, new MCvScalar(0, 255, 255));
                        imb_base[ind].Image = UtilOpenCV.drawPointsF(mat, ps, 255, 0, 0, 1);

                        if (visualise_compens)
                        {
                            imageBox_laser_compens.Image = UtilOpenCV.drawPointsF(mat, ps, 255, 0, 0, 1);
                        }

                    }
                    //catch
                    {

                    }
                    //imb_base[ind - 1].Image = Detection.detectLineSensor(mat);
                    break;
                case FrameType.Pattern:
                    try {
                        System.Drawing.PointF[] points2 = null;
                        imb_base[ind].Image = FindCircles.findCircles(mat, ref points2, chess_size);
                    }
                    catch { }
                    
                    //if (points2 != null) if (points2.Length >3) Console.WriteLine(points2[0].X + " " + points2[0].Y + " " + points2[1].X + " " + points2[1].Y + " " + points2[2].X + " " + points2[2].Y);
                   //else Console.WriteLine("null");
                    break;
                default:
                    break;
            }
            if (videoframe_counts[ind] == 0)
            {
                //initWrite(ind,cameraSize.Width,cameraSize.Height);
                video_mats[ind] = new List<Mat>();
                videoframe_counts[ind]++;
                save_vid_count=0;
            }

            if (videoframe_counts[ind] > 0 && videoframe_counts[ind ] < videoframe_counts_stop[ind])
            {
                bool without_las_pos = true;
                sb_enc?.Append(LaserLine.get_las_pos_time(laserLine,without_las_pos) + " " + videoframe_counts[ind] + " " + ind + " ");
                //if (sb_enc == null) Console.WriteLine("NULL!");
                //sb_enc?.Append("0" + " " + videoframe_counts[ind ] + " " + ind + " ");
                sb_enc?.Append(DateTime.Now.Ticks + " " + videoframe_counts[ind] + " " + ind + " ");
                sb_enc?.Append("\n");
                //video_writer[ind - 1]?.Write(mat);
                video_mats[ind]?.Add(mat.Clone());
                //var p = Detection.detectLineSensor(mat)[0];
                //Console.WriteLine(ind + " " + video_mats[ind-1].Count+" "+p);
                sb_enc?.Append(LaserLine.get_las_pos_time(laserLine, without_las_pos) + " " + videoframe_counts[ind] + " " + ind + " ");
                //sb_enc?.Append("0" + " " + videoframe_counts[ind ] + " " + ind + " ");
                sb_enc?.Append(DateTime.Now.Ticks + " " + videoframe_counts[ind ] + " " + ind + " ");
                sb_enc?.Append("\n");

                videoframe_counts[ind]++;
            }
            else
            {
                if (video_mats[ind] != null) {
                    try
                    {
                        save_video_sam(ind, cameraSize.Width, cameraSize.Height); save_vid_count++;
                    }
                    catch
                    {
                        label_scan_ready.BeginInvoke((MethodInvoker)(() => label_scan_ready.Text = "Сканирование неудачно"));
                    }
                    
                }
                if (sb_enc != null)
                {
                    laserLine?.laserOff();
                    laserLine?.setPower(0);
                    Thread.Sleep(2);
                    laserLine?.setPower(0);
                    Thread.Sleep(2);
                    laserLine?.setPower(0);
                    Thread.Sleep(2);
                    laserLine?.setPower(0);
                    string path = "cam1" + "\\" + box_scanFolder.Text + "\\enc.txt";
                    box_scanFolder.BeginInvoke((MethodInvoker)(() => box_scanFolder.Text = scan_fold_name));
                    textB_scan_path.BeginInvoke((MethodInvoker)(() => textB_scan_path.Text = scan_fold_path));
                    try
                    {
                        using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
                        {
                            if (sb_enc != null)
                            {
                                sw.Write(sb_enc.ToString());
                                sb_enc = null;
                                
                            }

                        }
                    }
                    catch

                    {
                        label_scan_ready.BeginInvoke((MethodInvoker)(() => label_scan_ready.Text = "Сканирование неудачно"));
                        Console.WriteLine("don t save enc");
                    }
                    label_scan_ready.BeginInvoke((MethodInvoker)(() => label_scan_ready.Text = "Сканирование завершено"));
                    label_scan_ready.BeginInvoke((MethodInvoker)(() => label_scan_ready.ForeColor = Color.ForestGreen));
                }
                scanning_status = false;

                // video_writer[ind - 1]?.Dispose();
            }

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
            var z_mult_cam = 0.5f;
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
        string generateImage3D_BOARD_solid(int n, int k, float sidef, PatternType patternType = PatternType.Chess)
        {
            float side = sidef * 2;
            if (patternType == PatternType.Chess)
            {
                n++; k++;
            }

            float w = sidef * (float)n;
            float h = sidef * (float)k;
            float offx = -sidef;
            float offy = -sidef;
            float z = 0f;
            float[] pattern_mesh = {
                            0.0f,0.0f,0.0f, // triangle 1 : begin
                            0.0f,sidef, 0.0f,
                           sidef,sidef, 0.0f, // triangle 1 : end
                            sidef, sidef,0.0f, // triangle 2 : begin
                           sidef,0.0f,0.0f,
                            0.0f, 0.0f,0.0f};

            if (patternType == PatternType.Mesh)
            {
                pattern_mesh = circle_mesh(sidef / 4, 50);
                side = sidef;
                w += side;
                h += side;
            }

            var mesh = new List<float>();

            if (patternType == PatternType.Chess)
            {
                for (float x = 0; x < w; x += side)
                {
                    for (float y = 0; y < h; y += side)
                    {
                        var patt_cur = GraphicGL.translateMesh(pattern_mesh, x + offx, y + offy, z);
                        mesh.AddRange(patt_cur);
                    }
                }
            }

            for (float x = sidef; x < w; x += side)
            {
                for (float y = sidef; y < h; y += side)
                {
                    var patt_cur = GraphicGL.translateMesh(pattern_mesh, x + offx, y + offy, z);
                    mesh.AddRange(patt_cur);
                }
            }
           return GL1.addGLMesh(mesh.ToArray(), PrimitiveType.Triangles,-600,-100,0,1,null,"calibrate_board");


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
        
        Mat takeImage(VideoCapture capture)
        {
            Mat im = new Mat();
            capture.Retrieve(im);
            return im;
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
        private void but_find_ports_Click(object sender, EventArgs e)
        {
           DeviceArduino.find_ports(comboBox_portsArd);
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
            textBox_shvpPos.Text = laserLine?.get_las_pos_time().ToString();
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
            else if (combo_improc.SelectedIndex == 4)
            {
                imProcType = FrameType.CircleGrid;
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
                video_scan_name = pos_rob.ToStr(" ",true);
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

        string[] get_video_path(int ind,string filepath)//[ video, enc]
        {
            Console.WriteLine("cam" + ind + "\\" + filepath);
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

        public Label get_label_scan_res()
        {
            return lab_scan_pres;
        }
        public ComboBox get_combo_im()
        {
            return comboImages;
        }
        /*public Label get_label_scan_res()
        {
            return lab_scan_pres;
        }*/
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

        string gen_traj_rob(RobotFrame.RobotType robotType, RobotFrame tool = null,string obj_3d = null)
        {
            string selected_obj = "";
            if (obj_3d != null)
            {
                selected_obj = obj_3d;
                traj_config.ang_x = scanner.stereoCamera.cur_pos.A;               
                traj_config.line_width = Convert.ToDouble(tb_scan_line_width_d.Text);
                traj_config.dz = Convert.ToDouble(tb_scan_ext_line_h.Text);
                traj_config.Step = Convert.ToDouble(tb_scan_ext_grid_d.Text);
                traj_config.vel = Convert.ToDouble(tb_scan_extprinting_vel.Text);
                traj_config.layers = Convert.ToInt32(tb_scan_ext_layer_n.Text);
            }
            else
            {
                selected_obj = selected_object();
                if (selected_obj == null) return "";
            }


            var mesh = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[selected_obj].vertex_buffer_data);
            var cont = GL1.get_contour()?.ToList();
            if (mesh != null && cont != null)
            {
                List<List<Point3d_GL>> conts = new List<List<Point3d_GL>>();
                for (int i = 0; i < traj_config.layers; i++)
                    conts.Add(cont);
                
                var _traj = PathPlanner.Generate_multiLayer3d_mesh(mesh, conts, traj_config, GL1);

                rob_traj = PathPlanner.join_traj(_traj);
                var ps = PathPlanner.matr_to_traj(rob_traj);
                if (tool == null)
                {
                    tool = new RobotFrame(0);
                }
                var tool_inv = tool.getMatrix().Clone();
                CvInvoke.Invert(tool_inv, tool_inv, DecompMethod.LU);
               for(int i=0; i<rob_traj.Count;i++)
                {
                    rob_traj[i] *= tool_inv;
                }

                if (GL1.buffersGl.objs.Keys.Contains(traj_i)) GL1.buffersGl.removeObj(traj_i);

               // for (int i = 0; i < rob_traj.Count; i+=5) GL1.addFrame(rob_traj[i],2);

                traj_i = GL1.addLineMeshTraj(ps.ToArray(),new Color3d_GL(0.9f),"gen_traj");

                var traj_rob = PathPlanner.generate_robot_traj(rob_traj,robotType,traj_config);

                var matrs_end = PathPlanner.traj_to_matr(traj_rob);
                //for (int i = 0; i < matrs_end.Count; i += 5) GL1.addFrame(matrs_end[i], 2);
                return RobotFrame.generate_string(traj_rob.ToArray()); ;

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
                 var mesh_sm_tr = GraphicGL.translateMesh(mesh_sm, 0, 0, -1f + (i * -1f));
                 surfs.Add(Polygon3d_GL.polygs_from_mesh(mesh_sm_tr));
                 //var rec = GL1.add_buff_gl(mesh_sm_tr, scan_stl[1], scan_stl[2], PrimitiveType.Triangles, selected_obj + "_cut_"+i);
                 //cuts.Add(rec);
                 var ps_inter = RasterMap.intersec_line_of_two_mesh(mesh_sm_tr, GL1.buffersGl.objs[selected_obj].vertex_buffer_data);
                 if (ps_inter == null) continue;
                 GL1.addLineMeshTraj(ps_inter, new Color3d_GL(1, 0, 0), "intersec_cut_" + i);
                 conts.Add(ps_inter.ToList());
             }

             var _traj = PathPlanner.generate_3d_traj_diff_surf(surfs, conts, traj_config);

             rob_traj = PathPlanner.join_traj(_traj);
             var ps = PathPlanner.matr_to_traj(rob_traj);

             if (GL1.buffersGl.objs.Keys.Contains(traj_i)) GL1.buffersGl.removeObj(traj_i);

             //for (int i = 0; i < rob_traj.Count; i++) GL1.addFrame(rob_traj[i],2);

             traj_i = GL1.addLineMeshTraj(ps.ToArray(), new Color3d_GL(0.9f), "gen_traj");
             var traj_rob = PathPlanner.generate_robot_traj(rob_traj, RobotFrame.RobotType.PULSE, traj_config);
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
                var mesh_sm_tr = GraphicGL.translateMesh(mesh_sm, 0, 0, -1 + (i * -1f));
                surfs.Add(Polygon3d_GL.polygs_from_mesh(mesh_sm_tr));
                //var rec = GL1.add_buff_gl(mesh_sm_tr, scan_stl[1], scan_stl[2], PrimitiveType.Triangles, selected_obj + "_cut_"+i);
                //cuts.Add(rec);
                var ps_inter = RasterMap.intersec_line_of_two_mesh(mesh_sm_tr, GL1.buffersGl.objs[selected_obj].vertex_buffer_data);
                GL1.addLineMeshTraj(ps_inter, new Color3d_GL(1, 0, 0), "intersec_cut_" + i);
                conts.Add(ps_inter.ToList());
            }

            var _traj = PathPlanner.generate_3d_traj_diff_surf(surfs, conts, traj_config);

            rob_traj = PathPlanner.join_traj(_traj);
            var ps = PathPlanner.matr_to_traj(rob_traj);

            if (GL1.buffersGl.objs.Keys.Contains(traj_i)) GL1.buffersGl.removeObj(traj_i);

            //for (int i = 0; i < rob_traj.Count; i++) GL1.addFrame(rob_traj[i],2);

            traj_i = GL1.addLineMeshTraj(ps.ToArray(), new Color3d_GL(0.9f), "gen_traj");
            var traj_rob = PathPlanner.generate_robot_traj(rob_traj, RobotFrame.RobotType.PULSE, traj_config);
            // return traj_rob;

        }

        private void but_remesh_test_Click(object sender, EventArgs e)
        {
            var selected_obj = selected_object(); if (selected_obj == null) return;
            smooth_mesh(selected_obj, Convert.ToDouble(tp_smooth_scan.Text));

        }

        private void but_unwrap_Click(object sender, EventArgs e)
        {
            var selected_obj = selected_object(); if (selected_obj == null) return;

            //mesh_ in  ax y
            unwrap_mesh(selected_obj);
        }

        static int[] comp_zeros(Point3d_GL[] ns)
        {
            var zeros = new List<int>();
            for(int i=1; i < ns.Length-1;i++)
            {
                if(ns[i].x> ns[i+1].x && ns[i].x > ns[i -1].x)
                {
                    zeros.Add(i);
                }
            }
            return zeros.ToArray();
        }
        /// <summary>
        /// 
        /// 
        /// </summary>
        /// <param name="pols"> массив треугольников, описывающих цилиндр</param>
        /// <param name="graphic">объект для отрисовки </param>
        void cylindr_find_vc(Polygon3d_GL[] pols,GraphicGL graphic)//выравнивание цилиндра
        {
            var mesh = new IndexedMesh(pols);//
            var ps = mesh.ps_uniq;
            var board = mesh.triangs_on_board()[0];//only first

            var ns = comp_angs_board_inds(pols,board);

            var zeros = comp_zeros(ns);
            if (zeros.Length != 2)
            {
                return;
            }
            var vc = pols[board[zeros[1]]].centr - pols[board[zeros[0]]].centr;
            var ps_vc = new Point3d_GL[] { pols[board[zeros[0]]].centr, pols[board[zeros[1]]].centr };
      
            graphic.addLineMesh(ps_vc, Color3d_GL.purple());
            var z0 = new Point3d_GL(0, 0, 1);
            var y = (vc).normalize();
            var x = (y | z0).normalize();
            var z = (x | y).normalize();

            var ms = Point3d_GL.aver(ps);


            var m = new Matrix<double>(new double[,]
            {
                { x.x,x.y,x.z,ms.x},
                 { y.x,y.y,y.z,ms.y},
                  {z.x,z.y,z.z,ms.z},
                   { 0,0,0,1}
            });
            GL1.addFrame(m,3);
            var m_inv = m.Clone();
           
            CvInvoke.Invert(m, m_inv, DecompMethod.LU);

            m_cyl = m_inv.Clone();
            var ps_y_ax = Point3d_GL.multMatr_p_m(m, ps);

            var ps_vc_y_ax = Point3d_GL.multMatr_p_m(m, ps_vc);
            graphic.addLineMesh(ps_vc_y_ax, Color3d_GL.aqua());

            mesh.ps_uniq = ps_y_ax;
            pols = mesh.get_polygs();
            var pols_xz = pols_on_xz(pols);
            var ps_c = Polygon3d_GL.get_centres(pols_xz);

            graphic.addPointMesh(ps_c, Color3d_GL.green());

              var circ = SurfaceReconstraction.ps_fit_circ_XZ_mnk(ps_c);
              var p_c = new Point3d_GL(circ.x, 0, circ.y);
              ps_y_ax = Point3d_GL.add_arr(ps_y_ax, -p_c);

            off_cyl = new Point3d_GL(circ.x, 0, circ.y);
            Console.WriteLine("off_cyl1: " + off_cyl);
            var ps_circ = new Point3d_GL[ps.Length];

            var ps_circ_2 = new Point3d_GL[ps.Length];
            var alph = Math.PI;
            var d_alph = 2 * Math.PI / ps_circ.Length;
            var rc = circ.z;
            r_cyl = rc;
            for (int i = 0; i < ps_circ.Length; i++)
            {
                var xc = circ.x + rc * Math.Sin(alph);
                var yc = circ.y + rc * Math.Cos(alph);
                ps_circ[i] = new Point3d_GL(xc, 0,yc);

                ps_circ_2[i] = new Point3d_GL(rc * Math.Sin(alph), 0, rc * Math.Cos(alph));
                alph += d_alph;
            }
            mesh.ps_uniq = ps_y_ax;
            var pols_xz2 = mesh.get_polygs();
            graphic.addMesh(Polygon3d_GL.toMesh(pols_xz2)[0], PrimitiveType.Triangles,null,"align");
            Console.WriteLine("circ_r:" + circ.z);

        }

        static Polygon3d_GL[] pols_on_xz(Polygon3d_GL[] pols)
        {
            var pols_xz = new List<Polygon3d_GL>();
            for(int i=0; i<pols.Length;i++)
            {
                var s1 = Math.Sign(pols[i].ps[0].y);
                var s2 = Math.Sign(pols[i].ps[1].y);
                var s3 = Math.Sign(pols[i].ps[2].y);
                var s = s1 * s2 * s3;
                if(s<0 &&(s1>0 || s2>0|| s3>0))
                {
                    pols_xz.Add(pols[i]);
                }
            }
            return pols_xz.ToArray();
        }


        static  Polygon3d_GL[] get_flat_xz(double y, double d)
        {
            Flat3d_GL flat3D_GL = new Flat3d_GL(new Point3d_GL(10, y, 0), new Point3d_GL(10, y, 10), new Point3d_GL(0, y, 10));
            var p0 = (new Line3d_GL(new Vector3d_GL(-d, 10, -d), new Point3d_GL(-d, 0, -d))).calcCrossFlat(flat3D_GL);
            var p1 = (new Line3d_GL(new Vector3d_GL(d, 10, -d), new Point3d_GL(d, 0, -d))).calcCrossFlat(flat3D_GL);

            var p2 = (new Line3d_GL(new Vector3d_GL(-d, 10, d), new Point3d_GL(-d, 0, d))).calcCrossFlat(flat3D_GL);
            var p3 = (new Line3d_GL(new Vector3d_GL(d, 10, d), new Point3d_GL(d, 0, d))).calcCrossFlat(flat3D_GL);

            var pols = new Polygon3d_GL[]
            {
                new Polygon3d_GL( p1,p3,p2),
                new Polygon3d_GL( p2,p0,p1)
            };
            return pols;
        }

        private void but_wrap_Click(object sender, EventArgs e)
        {
            var selected_obj = selected_object(); if (selected_obj == null) return;
            wrap_mesh(selected_obj);
        }

        private void but_allign_cyl_Click(object sender, EventArgs e)
        {
            var selected_obj = selected_object(); if (selected_obj == null) return;
            var polygs = Polygon3d_GL.polygs_from_mesh(GL1.buffersGl.objs[selected_obj].vertex_buffer_data, GL1.buffersGl.objs[selected_obj].color_buffer_data);
            cylindr_find_vc(polygs, GL1);

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
            if(scanner_config.distort)
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
            if (scanner_config.distort)
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

            var scanner = loadScanner_v2(cam1_conf_path, cam2_conf_path, stereo_cal_path);
            this.scanner = scanner;
            load_scan_v2(scanner,scan_path, scanner_config);

        }

        

        private void but_scan_stereo_rob_Click(object sender, EventArgs e)
        {
            var scan_path = textB_scan_path.Text;
            var cam1_conf_path = textB_cam1_conf.Text;
            var cam2_conf_path = textB_cam2_conf.Text;
            var stereo_cal_path = textB_stereo_cal_path.Text;
            string bfs_path = "bfs_cal.txt";

            var scanner = loadScanner_v2(cam1_conf_path, cam2_conf_path, stereo_cal_path,bfs_path);
            scanner.robotType = current_robot;
            this.scanner = scanner;
            load_scan_v2(scanner, scan_path, scanner_config);
        }

        private void but_scan_load_sing_Click(object sender, EventArgs e)
        {
            var scan_path = textB_scan_path.Text;
            var cam1_conf_path = textB_cam1_conf.Text;
            var laser_line_path = textB_cam2_conf.Text;

            int strip = scanner_config.strip;
            double smooth = Convert.ToDouble(scanner_config.smooth);

            var scanner = loadScanner_sing(cam1_conf_path,laser_line_path);
            load_scan_sing(scanner, scan_path, strip, smooth);
        }
        private void but_load_sing_calib_Click(object sender, EventArgs e)
        {
            var scan_path = textB_scan_path.Text;
            var cam1_conf_path = textB_cam1_conf.Text;

            int strip = scanner_config.strip;
            double smooth = Convert.ToDouble(tp_smooth_scan.Text);

            var scanner = loadScanner_sing(cam1_conf_path);
            load_calib_sing(scanner,scan_path, strip, smooth);
        }

        static public string save_file_name(string init_direct, string init_name, string extns)
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
                openFileDialog.Filter = extns+" files ("+ extns + ")|" + extns ;
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
            textB_cam1_conf.Text =  get_file_name(Directory.GetCurrentDirectory(),"*.txt");
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path, scanner_config, traj_config, patt_config);
        }

        private void but_load_conf_cam2_Click(object sender, EventArgs e)
        {
            textB_cam2_conf.Text = get_file_name(Directory.GetCurrentDirectory(), "*.txt; *.json");
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path, scanner_config, traj_config, patt_config);
        }

        private void but_stereo_cal_path_Click(object sender, EventArgs e)
        {
            textB_stereo_cal_path.Text = get_folder_name(Directory.GetCurrentDirectory());
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path, scanner_config, traj_config, patt_config);
        }

        private void but_scan_path_Click(object sender, EventArgs e)
        {
            textB_scan_path.Text = get_folder_name(Directory.GetCurrentDirectory());
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path, scanner_config, traj_config, patt_config);
        }

        private void MainScanningForm_Load(object sender, EventArgs e)
        {

           /* windowsTabs.Controls.Remove(tabMain);
            windowsTabs.Controls.Remove(tabOpenGl);
            windowsTabs.Controls.Remove(tabDistort);
            windowsTabs.Controls.Remove(tabP_developer);
            windowsTabs.Controls.Remove(tabCalibMonit);
            windowsTabs.Controls.Remove(tabDebug);
            windowsTabs.Controls.Remove(tabP_developer);
            windowsTabs.Controls.Remove(tabP_scanning_printing);
            windowsTabs.Controls.Remove(tabP_connect);*/
            //windowsTabs.Controls.Remove(tabPage_tube);


          /*  this.tabP_connect.Controls.Add(this.imageBox1);
            this.tabP_connect.Controls.Add(this.imageBox2);
            this.tabP_scanning_printing.Controls.Add(this.glControl1);
          */
            
            add_buttons_rob_contr();
            formSettings.load_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path);
            for (int i = 0; i < imb_main.Length; i++)
            {
                imb_main[i].AccessibleName = i.ToString();
                addButsForControl((Control)imb_main[i], 4);
            }
            for (int i = 0; i < imb_main.Length; i++)
            {
                imb_main[i].SendToBack();
            }



           
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
            var im1 = ((Mat)imageBox1.Image).Clone();
            //var im = (Mat)imBox_base_1.Image;
            //var im2 = new Mat();
            if(im1.NumberOfChannels==3)
            {
                CvInvoke.CvtColor(im1, im1, ColorConversion.Bgr2Gray);
            }
            
            //CvInvoke.GaussianBlur(im2, im2, new Size(7, 7), -1);
            send_buffer_img(im1.ToImage<Gray,Byte>(), PrimitiveType.Triangles,GL1);
            
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
            var stl_name = get_file_name(Directory.GetCurrentDirectory(), "*.stl");
            
            var scan_stl = new Model3d(stl_name, false);

            //var imesh = new IndexedMesh(scan_stl.pols);

            //imesh.ps_uniq = GraphicGL.add_random_to_ps(imesh.ps_uniq);

            //scan_stl.mesh = Polygon3d_GL.toMesh(imesh.get_polygs())[0];

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

            connect_robot(RobotFrame.RobotType.PULSE);

        }

        void connect_robot(RobotFrame.RobotType robot)
        {
            con1 = null;
            GC.Collect();
      
                con1 = new TCPclient();
            
            /*if(robot == RobotFrame.RobotType.KUKA)
            {
                var res = MessageBox.Show("1. Переведите робот в автоматичекий режим управления\n2. Запустите программу 'In situ bioprinter' на пульте робота\n3. Убедитесь что на пульте выведено сообщение 'Готов к работе'", "Сообщение",MessageBoxButtons.OKCancel);
                if(res == DialogResult.Cancel)
                {
                    return;
                }
            }*/
            string ip = "";
            //if((string)combo_robot_ch.SelectedItem=="Kuka")
            if (robot == RobotFrame.RobotType.KUKA)
            {
                ip = "172.31.1.147";// misis
            }
            else if (robot == RobotFrame.RobotType.PULSE)
            //else if((string)combo_robot_ch.SelectedItem == "Pulse")
            {
                ip = "localhost";
            }
            
             port_tcp = Convert.ToInt32(tb_port_tcp.Text);
            // Console.WriteLine(ip + " " + port_tcp);
            con1.Connection(port_tcp, ip);


            //con1 = new TCPclient();
            //port_tcp = Convert.ToInt32(tb_port_tcp.Text);

            // con1.Connection(port_tcp, ip);

            //con1.Connection(10000, "10.5.5.100");

            tcp_thread = new Thread(recieve_tcp);
            tcp_thread.Start(con1);
           
        }

        private void but_rob_discon_sc_Click(object sender, EventArgs e)
        {
            try
            {
                con1.send_mes("q\n");
                con1.close_con();
                con1 = null;
                tcp_thread.Abort();
                tcp_thread = null;


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
            //Thread.Sleep(100);
           // try_send_rob("q\n");
        }

        private void but_rob_traj_pulse_Click(object sender, EventArgs e)
        {
            var tool = new RobotFrame(tB_tool_inf.Text);
            if (tool.get_pos().magnitude() + tool.get_rot().magnitude() < 0.0001) tool = null;
            debugBox.Text = gen_traj_rob(RobotFrame.RobotType.PULSE,tool);
        }

        private void but_rob_traj_kuka_Click(object sender, EventArgs e)
        {
            debugBox.Text = gen_traj_rob(RobotFrame.RobotType.KUKA);
        }





        #endregion

        private void but_gl_detect_line_Click(object sender, EventArgs e)
        {
            Console.WriteLine("detect:");
             Detection.detectLineDiff_debug( (Mat)imBox_mark1.Image , scanner_config);
        }

        private void but_start_scan_sam_Click(object sender, EventArgs e)
        {
            var pos_rob = positionFromRobot(con1);
            if (pos_rob != null)
            {
                video_scan_name = pos_rob.ToString();
            }
            else
            {
                video_scan_name = "1";
            }
            startScanLaser(8);
        }

        private void but_con_scan_sam_Click(object sender, EventArgs e)
        {
            videoStart(2);
            videoStart(1); Thread.Sleep(5000);
            //find_ports(); Thread.Sleep(100);
            //laserLine = new LaserLine(portArd); 
            Thread.Sleep(1000);
            laserLine?.setShvpVel(200); Thread.Sleep(100);
            laserLine?.laserOn(); Thread.Sleep(100);
            laserLine?.set_home_laser(); Thread.Sleep(1000);
            laserLine?.setShvpPos(350); Thread.Sleep(100);
        }

        private void send_rob_Click(object sender, EventArgs e)
        {
            var mes = nameX.Text + " " + nameY.Text + " " + nameZ.Text + " " + nameA.Text + " " + nameB.Text + " " + nameC.Text + " " + boxN.Text + " \n";
            try_send_rob(mes);
        }

        private void combo_robot_ch_SelectedIndexChanged(object sender, EventArgs e)
        {
            if((string)combo_robot_ch.SelectedItem == "Kuka")
            {
                current_robot = RobotFrame.RobotType.KUKA;
            }
            if((string)combo_robot_ch.SelectedItem == "Pulse")
            {
                current_robot = RobotFrame.RobotType.PULSE;
            }
        }

        #region samara_con
        //__________________SIMP______________________
        private void but_con_set_ard_con_Click(object sender, EventArgs e)
        {
            Thread connect_las_thread = new Thread(connect_las);
            Thread connect_cam_thread = new Thread(connect_cams);
            connect_las_thread.Start();
            connect_cam_thread.Start();
        }

        void connect_las()
        {
            label_ard_connect.BeginInvoke((MethodInvoker)(() => label_ard_connect.Text = "Подключение контр РО..."));
            label_ard_connect.BeginInvoke((MethodInvoker)(() => label_ard_connect.ForeColor = Color.Firebrick));
            DeviceArduino.find_ports(cb_ard_ext,true);
            Thread.Sleep(300);
            laserLine = new LaserLine(portArd); Thread.Sleep(1500);
            laserLine?.setShvpVel(200); Thread.Sleep(100);
            laserLine?.laserOn(); Thread.Sleep(100);
            laserLine?.set_home_laser(); Thread.Sleep(1000);
            laserLine?.setShvpPos(350); Thread.Sleep(100);
            laserLine?.laserOff();
            label_ard_connect.BeginInvoke((MethodInvoker)(() => label_ard_connect.Text = "Контр РО подключён"));
            label_ard_connect.BeginInvoke((MethodInvoker)(() => label_ard_connect.ForeColor = Color.ForestGreen));
        }
        void connect_cams()
        {
            label_cam_connect.BeginInvoke((MethodInvoker)(() => label_cam_connect.Text = "Подключение камер...Это может занять несколько минут"));
            label_cam_connect.BeginInvoke((MethodInvoker)(() => label_cam_connect.ForeColor = Color.Firebrick));
            var inds_cam = new int[] { scanner_config.cam1_ind, scanner_config.cam2_ind };
            videoStart_sam(inds_cam[0]);
            imb_ind_cam[0] = inds_cam[0];
            videoStart_sam(inds_cam[1]);
            imb_ind_cam[1] = inds_cam[1];
            Thread.Sleep(200);
            if(imb_main[0].Image!=null && imb_main[1].Image != null)
            {
                var mat = (Mat)imb_main[0].Image;
                var scale = (double)imb_main[0].Width/mat.Width  ;
                imb_main[0].BeginInvoke((MethodInvoker)(() => imb_main[0].SetZoomScale(scale, new Point(0, 0))));
                imb_main[1].BeginInvoke((MethodInvoker)(() => imb_main[1].SetZoomScale(scale, new Point(0, 0))));
                label_cam_connect.BeginInvoke((MethodInvoker)(() => label_cam_connect.Text = "Камеры подключены"));
                label_cam_connect.BeginInvoke((MethodInvoker)(() => label_cam_connect.ForeColor = Color.ForestGreen));


            }
            else
            {
                label_cam_connect.BeginInvoke((MethodInvoker)(() => label_cam_connect.Text = "Камеры не подключены"));
                label_cam_connect.BeginInvoke((MethodInvoker)(() => label_cam_connect.ForeColor = Color.Firebrick));
            }

            
        }
        private void but_con_set_rob_con_Click(object sender, EventArgs e)
        {
            connect_robot(RobotFrame.RobotType.KUKA);
        }
        //-----------------EXT--------------------
        private void but_con_ext_disp_down_Click(object sender, EventArgs e)
        {
            var div = LaserLine.vel_pist_to_ard(Convert.ToDouble(textBox_con_ext_disp_vel.Text));
            laserLine?.set_dir_disp(-1);
            laserLine?.set_div_disp(div);
        }

        private void but_con_ext_disp_up_Click(object sender, EventArgs e)
        {
            //var div = get_vel(tb_print_vel, tb_print_nozzle_d, tb_print_syr_d);

            var div = LaserLine.vel_pist_to_ard(Convert.ToDouble( textBox_con_ext_disp_vel.Text));
            laserLine?.set_dir_disp(1);
            Thread.Sleep(2);
            laserLine?.set_dir_disp(1);
            Thread.Sleep(2);
            laserLine?.set_dir_disp(1);
            Thread.Sleep(2);
            laserLine?.set_dir_disp(1);
            Thread.Sleep(2);
            laserLine?.set_div_disp(div);
            Thread.Sleep(2);
            laserLine?.set_div_disp(div);
            Thread.Sleep(2);
            laserLine?.set_div_disp(div);
            Thread.Sleep(2);
            laserLine?.set_div_disp(div);
            Thread.Sleep(2);
        }
        private void but_con_ext_disp_up_push_Click(object sender, EventArgs e)
        {
            laserLine?.push_back_();
        }

        private void but_con_ext_disp_down_push_Click(object sender, EventArgs e)
        {
            laserLine?.push_forward();
        }
        private void but_con_ext_disp_stop_Click(object sender, EventArgs e)
        {
            laserLine?.set_dir_disp(0);
        }

        int get_vel(TextBox pr_vel, TextBox pr_nos_d, TextBox pr_syr_d)
        {
            double vel_noz = Convert.ToDouble(pr_vel.Text);
            double d_noz = Convert.ToDouble(pr_nos_d.Text);
            double d_syr = Convert.ToDouble(pr_syr_d.Text);
            var div = LaserLine.vel_div(vel_noz, d_noz, d_syr);
            return div;
        }

        void add_buttons_rob_contr()
        {
            var axis = "XYZABC";
            var st_x = 540;
            var st_y = 80;
            var dim_x = 40;
            var dim_y = 40;
            for (int i = 0; i < axis.Length; i++)
            {
                add_but_rob_contr("+" + axis[i], new Rectangle(st_x + (dim_x+3) * i+10, st_y,         dim_x, dim_y),  groupBox_rob_con_ext );
                add_but_rob_contr("-" + axis[i], new Rectangle(st_x +( dim_x+3) * i+10, st_y + dim_y+20, dim_x, dim_y),  groupBox_rob_con_ext);
            }
        }
        Button add_but_rob_contr_old(string ax, Rectangle pos,Control[] parents)
        {
            var but = new Button();
            but.Location = new Point(parents[0].Location.X+pos.X, parents[0].Location.Y + pos.Y);
            but.Size = pos.Size;
            but.AccessibleName = ax;
            but.Text = ax;
            but.Click += but_rob_contr_Click;
            for(int i= 0; i < parents.Length; i++)
            {
                parents[i].Controls.Add(but);
                parents[i].SendToBack();
            }
            
            return but;
        }
        Button add_but_rob_contr(string ax, Rectangle pos, Control parent)
        {
            var but = new Button();
            but.Location = new Point(parent.Location.X + pos.X, parent.Location.Y + pos.Y);
            but.Size = pos.Size;
            but.AccessibleName = ax;
            but.Text = ax;
            but.Click += but_rob_contr_Click;
            //but.BackColor
            but.BackColor = System.Drawing.Color.SteelBlue;
            but.FlatAppearance.BorderColor = System.Drawing.Color.SteelBlue;
            but.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            but.Font = new System.Drawing.Font("Arial Unicode MS", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            but.ForeColor = System.Drawing.SystemColors.HighlightText;
            but.UseVisualStyleBackColor = false;
            parent.Parent.Controls.Add(but);
            parent.SendToBack();
            return but;
        }
        private void but_rob_contr_Click(object sender, EventArgs e)
        {
            var but = (Button)sender;
            var ax = but.AccessibleName;
            var delt = mask_from_ax(ax) * dist_contr_rob;
            Console.WriteLine(delt.ToStr(" ", false, false));
            try_send_rob("m\n");
            Thread.Sleep(50);
            try_send_rob("c\n");
            Thread.Sleep(50);
            try_send_rob("m\n");
            Thread.Sleep(50);
            var cur_pos = positionFromRobot(con1);
            if(cur_pos==null)
            {
                cur_pos = new RobotFrame();
            }
            else
            {
                var dest_pos = cur_pos + delt;
                Console.WriteLine(dest_pos.ToStr(" ", false, false));
                try_send_rob(dest_pos.ToStr(" ", true, false) + "\n");
            }
            
        }


        RobotFrame mask_from_ax(string ax)
        {
            RobotFrame mask = new RobotFrame();
            double sign = 1;
            var angle_del = 0.001;
            if(ax[0] == '-')
            {
                sign = -1;
            }
            switch(ax[1])
            {
                case 'X': mask.X = sign; break;
                case 'Y': mask.Y = sign; break;
                case 'Z': mask.Z = sign; break;
                case 'A': mask.A = angle_del* sign; break;
                case 'B': mask.B = angle_del * sign; break;
                case 'C': mask.C = angle_del * sign; break;
            }

            return mask;
        }


        #endregion

        #region samara_scan

        private void but_scan_simp_scan_Click(object sender, EventArgs e)
        {
            //Thread scan_and_load_thread = new Thread(scan_and_load);
            //scan_and_load_thread.Start();
            //load
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path, scanner_config, traj_config, patt_config);
            scan_and_load();
        }

        void scan_and_load()
        {
            label_scan_ready.BeginInvoke((MethodInvoker)(() => label_scan_ready.Text = "Сканирование запущено..."));
            label_scan_ready.BeginInvoke((MethodInvoker)(() => label_scan_ready.ForeColor = Color.Firebrick));

            label_scan_ready_load.BeginInvoke((MethodInvoker)(() => label_scan_ready_load.Text = "3D модель не загружена"));
            label_scan_ready_load.BeginInvoke((MethodInvoker)(() => label_scan_ready_load.ForeColor = Color.Firebrick));
            var pos_rob = positionFromRobot_str(con1);
            Console.WriteLine(pos_rob);

            if (pos_rob != null)
            {
                video_scan_name = pos_rob;
            }
            else
            {
                video_scan_name = "1";
            }
            startScanLaser(3);
            formSettings.save_settings(textB_cam1_conf, textB_cam2_conf, textB_stereo_cal_path, textB_scan_path, scanner_config, traj_config, patt_config);
            //start_load_scan();
        }
        private void but_scan_simp_scan_load_Click(object sender, EventArgs e)
        {
            label_scan_ready_load.BeginInvoke((MethodInvoker)(() => label_scan_ready_load.Text = "Загрузка скана..."));
            label_scan_ready_load.BeginInvoke((MethodInvoker)(() => label_scan_ready_load.ForeColor = Color.Firebrick));
            load_scan_full();
            label_scan_ready_load.BeginInvoke((MethodInvoker)(() => label_scan_ready_load.Text = "Скан загружен"));
            label_scan_ready_load.BeginInvoke((MethodInvoker)(() => label_scan_ready_load.ForeColor = Color.ForestGreen));
        }
        public void load_scan_full()
        {
            var scan_path = textB_scan_path.Text;
            var cam1_conf_path = textB_cam1_conf.Text;
            var cam2_conf_path = textB_cam2_conf.Text;
            var stereo_cal_path = textB_stereo_cal_path.Text;
            string bfs_path = "bfs_cal.txt";

            var scanner = loadScanner_v2(cam1_conf_path, cam2_conf_path, stereo_cal_path, bfs_path);
            this.scanner = scanner;
            scanner_config.strip = Convert.ToInt32(tb_scan_ext_scan_strip.Text);
            scanner_config.smooth = Convert.ToDouble(tb_scan_ext_scan_smooth.Text);
            load_scan_v2(scanner, scan_path, scanner_config);
        }

        public void start_load_scan()
        {
            try
            {
                Thread robot_thread = new Thread(load_scan_full);
                robot_thread.Start();
            }
            catch{}
        }

        private void but_scan_simp_gen_traj_Click(object sender, EventArgs e)
        {
            if(surface_type == 1)
            {
                if(comboBox_syrenge_size.SelectedIndex == 0)
                {
                    traj_config.off_z = 10;
                }
               
            }
            else
            {
                traj_config.off_z = 0;
            }
            debugBox.Text = gen_traj_rob(RobotFrame.RobotType.KUKA,null,scan_i);
        }

        private void but_scan_simp_start_print_Click(object sender, EventArgs e)
        {
           // var res = MessageBox.Show("1. Убедитесь, что шприц установлен\n2. Убедитесь, что материал готов к подаче", "Сообщение", MessageBoxButtons.OKCancel);
            //if (res == DialogResult.Cancel)
            {
              //  return;
            }
            printing = true;
            try_send_rob("a\n");
            Thread.Sleep(50);
            try_send_rob("c\n");
            Thread.Sleep(50);
            try_send_rob(debugBox.Text + "\n");
            Thread.Sleep(150);
            try_send_rob("s\n");
        }
        private void but_scan_simp_stop_print_Click(object sender, EventArgs e)
        {
            try_send_rob("e\n");
        }
        private void but_scan_simp_cont_beg_Click(object sender, EventArgs e)
        {
            GL1.modeGL = modeGL.Paint;
            GL1.get_contour();
        }

        private void but_scan_simp_cont_save_Click(object sender, EventArgs e)
        {
            GL1.modeGL = modeGL.View;
        }

        private void but_scan_simp_xy_Click(object sender, EventArgs e)
        {
            GL1.planeXY();
        }


        #endregion

        private void but_show_traj_fr_tb_Click(object sender, EventArgs e)
        {
            var traj_rob = RobotFrame.parse_g_code(debugBox.Text, RobotFrame.RobotType.KUKA);
            var matrs_end = PathPlanner.traj_to_matr(traj_rob.ToList());
            for (int i = 0; i < matrs_end.Count; i += 5) GL1.addFrame(matrs_end[i], 2);
        }


        #region correct_scan_pos
        Scanner load_scanner_v3()
        {
            var cam1_conf_path = textB_cam1_conf.Text;
            var cam2_conf_path = textB_cam2_conf.Text;
            var stereo_cal_path = textB_stereo_cal_path.Text;
            string bfs_path = "bfs_cal.txt";
            var scanner = loadScanner_v2(cam1_conf_path, cam2_conf_path, stereo_cal_path, bfs_path);
            this.scanner = scanner;
            return scanner;
        }

        //private void imageBox1_Click(object sender, EventArgs e)
        //{
        //    var scanner = load_scanner_v3();
        //    pos_cameras(scanner);
        //}
        RobotFrame get_pos_robot()
        {
            return null;
        }
        void start_find_point_laser(PointF p_u)//передаём точку пользователя везде
        {
            Thread laser_find_thread = new Thread(laser_on_point);//создаём поток
            laser_find_thread.Start(p_u);//запускаем поток
        }
        private PointF get_laser_x(PointF p_u)
        {
            laserLine?.laserOff();//выключаем лазер и ждём 
            Thread.Sleep(300);
            /* makePhotoLaser(                       //фото сохранять нет необходимости
                     new float[] { x },
                     new string[] { "cam1\\" + folder_san + "\\orig", "cam2\\" + folder_scan + "\\orig" },
                     new ImageBox[] { imageBox1, imageBox2 }
                     );
             Thread.Sleep(200);
             //var im1 = (Mat)imageBox1.Image;
            // var im2 = (Mat)imageBox2.Image;*///сейчас получается что избражения с разных камер будут получены
                                                //нужно примерно так:
            var im1 = (Mat)imageBox1.Image;//сохраняем изображение
            laserLine?.laserOn();//выключаем лазер , ждём, делаем ещё фото
            Thread.Sleep(300);
            var im2 = (Mat)imageBox1.Image;
            laserLine?.laserOff();
            var new_im = im2 - im1;//теперь считаем их разность
            var ps = Detection.detectLineDiff(new_im);
            
            var p_las_x = ps[Detection.p_in_ps_by_y(ps, (int)p_u.Y)];//далее получаем текущий x для лазерной линии
            return p_las_x;
        }
        private void laser_on_point(object ob_u)
        {
            PointF p_u = (PointF)ob_u;
            PointF p_las_cam = get_laser_x(p_u);//текущая позиция лазера на линии точки пользователя
            while (Math.Abs(p_u.X - p_las_cam.X)>1)//пока разница по х с точкой пользователя больше 1 пикселя
            {
                var cur_pos_las = laserLine.get_las_pos();//получаем текущую координату двигателя лазера
                laserLine.setShvpPos(cur_pos_las + (int)(p_u.X - p_las_cam.X));//перемещаемся в сторону для уменьшения разницы
                Thread.Sleep(200);//ждём пока лазер завершит движение
                p_las_cam = get_laser_x(p_u);//заного снимаем текущую координату лазера на изображении
            }
            
            

        }


        private void imageBox1_MouseClick(object sender, MouseEventArgs e)
        {
            var scanner = load_scanner_v3();
            pos_cameras(scanner);
            var ib = (ImageBox)sender;
            var x_offset = ib.HorizontalScrollBar.Value;
            var y_offset = ib.VerticalScrollBar.Value;
            var x = e.Location.X;
            var y = e.Location.Y;
            var zoom = ib.ZoomScale;
            int Xs = (int)((x / zoom) + x_offset);
            int ys = (int)((y / zoom) + y_offset);
            start_find_point_laser(new PointF(Xs,ys));
            //p_in_ps_by_y(X, Y);

            //MessageBox.Show(String.Format("{0}, {1}", X, Y));

        }
        private void pos_cameras(Scanner scanner)
        {
            var rob_pos = get_pos_robot();
            var stereo_cam = scanner.stereoCamera;
            Matrix<double> robotPosition = rob_pos.getMatrix();
            Matrix<double> Bfs = stereo_cam.Bfs;
            Matrix<double> R = stereo_cam.R;
            Matrix<double> camera1Position = robotPosition * Bfs;
            Matrix<double> camera2Position = camera1Position * R;
            Console.WriteLine("Позиция Камеры 1:");
            prin.t(camera1Position);
            Console.WriteLine("Позиция Камеры 2:");
            prin.t(camera2Position);
        }
        #endregion

        private void rb_mm_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton checkBox = (RadioButton)sender;
            if (checkBox.Checked == true)
            {
                dist_contr_rob = Convert.ToDouble(checkBox.AccessibleName);
            }
        }

        private void but_drill_dir_2_Click(object sender, EventArgs e)
        {
            laserLine?.set_drill_dir(1);
        }

        private void but_drill_ch_dir_Click(object sender, EventArgs e)
        {
            laserLine?.set_drill_dir(0);
        }

        private void but_drill_vel_Click(object sender, EventArgs e)
        {
           // laserLine?.set_adr(50);
           laserLine?.set_drill_vel(Convert.ToInt32(textBox_drill_vel.Text));
        }
        private void but_water_vel_Click(object sender, EventArgs e)
        {
            laserLine?.set_water_vel(Convert.ToInt32(textBox_water_vel.Text));
        }

        #region tube
        int i2c_adr_main = 0;
        int i2c_adr_nasos1 = 50;//50
        int i2c_adr_nasos2 = 51;//51
        int i2c_sensors = 52;//52
        int i2c_adr_valve = 53;//53
        
        double nT = 5000;
        double to_double_textbox(TextBox textBox, double min,double max)
        {
            var val = to_double(textBox.Text);
            if(val==double.NaN)
            {
                val = min;
            }
            if(val<min)
            {
                val = min;
            }
            if(val>max)
            {
                val = max;
            }
            return val;
        }

        private void textBox_z1_vel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var vel = to_double_textbox(textBox_z1_vel, 0.01, 2);
                var div = LaserLine.vel_pist_to_ard(vel, nT, 1, 3200);
                Console.WriteLine(div);
                laserLine?.set_adr(i2c_adr_main);
                
                laserLine?.set_div_disp(div);
                
            }
        }

        private void textBox_z2_vel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var vel = to_double_textbox(textBox_z2_vel, 0.01, 2);
                var div = LaserLine.vel_pist_to_ard(vel, nT, 1, 3200);
                Console.WriteLine(div);
                laserLine?.set_adr(i2c_adr_main);
               
                laserLine?.set_z_div(div);
            }
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {

        }

        private void but_z1_pl_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_main);
            laserLine?.set_rel_pos_disp(Convert.ToInt32(dist_contr_rob));
        }

        private void but_z1_min_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_main);
            laserLine?.set_rel_pos_disp(-Convert.ToInt32(dist_contr_rob));
        }

        private void but_z2_pl_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_main);
            laserLine?.set_rel_pos_z(Convert.ToInt32(dist_contr_rob));
        }

        private void but_z2_min_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_main);
            laserLine?.set_rel_pos_z(-Convert.ToInt32(dist_contr_rob));
        }
        
        private void button_pump1_start_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_nasos1);
            laserLine?.set_dir_disp(1);
        }

        private void button_pump1_stop_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_nasos1);
            laserLine?.set_dir_disp(0);
        }

        private void rb_mm_acust_01mm_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton checkBox = (RadioButton)sender;
            if (checkBox.Checked == true)
            {
                //laserLine?.set_adr(0);
                dist_contr_rob = 100* Convert.ToDouble(checkBox.AccessibleName);
            }
        }

        private void textBox_pump1_vel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var vel = to_double_textbox(textBox_pump1_vel, 0.01, 2);
                var div = LaserLine.vel_pist_to_ard(vel, nT, 1, 6400);
               // Console.WriteLine(div);

                laserLine?.set_adr(i2c_adr_nasos1);

                laserLine?.set_div_disp(div);
            }
        }


        private void but_cycle_start_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_main);
            laserLine?.set_comp_cycle_type(1);
        }

        private void but_cycle_stop_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_main);
            laserLine?.set_comp_cycle_type(0);
        }

        private void textBox_cycle_speed_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var vel = to_double_textbox(textBox_cycle_speed, 0.01, 2);
                var div = LaserLine.vel_pist_to_ard(vel, nT, 1, 3200);
                laserLine?.set_adr(i2c_adr_main);               
                Console.WriteLine(div);
                laserLine?.set_las_div(div);
            }
        }

        private void textBox_cycle_amplitud_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var ampl_degree = to_double_textbox(textBox_cycle_amplitud, 0, 1360);
                var aml_steps = 32 * ampl_degree / 360;
                //var div = LaserLine.vel_pist_to_ard(vel, nT, 1, 3200);
                laserLine?.set_adr(i2c_adr_main);
                Console.WriteLine(aml_steps);
                laserLine?.set_comp_cycle_ampl(aml_steps);
            }
        }

        private void but_cycle_zero_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_main);
            laserLine?.set_home_laser();
        }
        private void button_pump2_stop_Click(object sender, EventArgs e)
        {

            laserLine?.set_adr(i2c_adr_nasos2);
            laserLine?.set_dir_disp(0);
        }

        private void button_pump2_start_Click(object sender, EventArgs e)
        {

            laserLine?.set_adr(i2c_adr_nasos2);
            laserLine?.set_dir_disp(1);
        }

        private void textBox_valve_val_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var pos = Convert.ToInt32(textBox_valve_val.Text);
                laserLine?.set_adr(i2c_adr_valve);
                laserLine?.set_valve_pos(pos);
            }

        }


        private void textBox_pump2_vel_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var vel = to_double_textbox(textBox_pump2_vel, 0.01, 2);
                var div = LaserLine.vel_pist_to_ard(vel, nT, 1, 3200);

                laserLine?.set_adr(i2c_adr_nasos2);
                laserLine?.set_div_disp(div);
            }
        }
        private void textBox_led_pwm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                laserLine?.set_adr(i2c_adr_main);
                var pwm = Convert.ToInt32(textBox_led_pwm.Text);
                laserLine?.setPower(pwm);
            }
        }

        private void but_led_on_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_main);
            laserLine?.laserOn();
        }

        private void but_led_off_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_main);
            laserLine?.laserOff();
        }


        private void but_find_ard_tube_Click(object sender, EventArgs e)
        {
            DeviceArduino.find_ports(comboBox_ard_tube);
        }

        private void but_con_ard_tube_Click(object sender, EventArgs e)
        {
            laserLine = new LaserLine(portArd);

            ard_acust_thread = new Thread(recieve_ard_acust);
            ard_acust_thread.Start(laserLine);
        }
        void recieve_ard_acust(object obj)
        {
            var con = (LaserLine)obj;
            while (true)
            {
                if (con == null) { return; }
                var res = con.reseav();
                //Console.WriteLine(res);
                if (res != null)
                {
                    if (res.Length > 3)
                    {
                        var data = con.parse_temp_co2();
                        if (data[0] != -1)
                        {
                            //Console.WriteLine(" t: " + data[0] + " CO2: " + recalc_co2(data[1]));
                            label_acust_sensors.BeginInvoke((MethodInvoker)(() => label_acust_sensors.Text = " t: " + Math.Round(data[0],1) + " C\n CO2: " + Math.Round( recalc_co2(data[1]),1)+"%"));
                        }
                        
                    }
                }
                Thread.Sleep(100);
            }
        }

        double recalc_co2(double co2)
        {
            var k = co2 / 500;
            var co2_m = co2 * k;
            co2_m /= 10000;
            return co2_m;
        }


        private void but_disc_ard_tube_Click(object sender, EventArgs e)
        {
            laserLine?.connectStop();
        }

        private void comboBox_ard_tube_SelectedIndexChanged(object sender, EventArgs e)
        {
            portArd = (string)((ComboBox)sender).SelectedItem;
        }

        #endregion

        #region movm_mash
        int i2c_adr_movm_mash = 70;
        private void but_cycle_type_Click(object sender, EventArgs e)//
        {
            laserLine?.set_adr(i2c_adr_movm_mash);
            laserLine?.set_comp_cycle_type(Convert.ToInt32(textBox_cycle_type.Text));
        }

        private void but_cycle_ampl_Click(object sender, EventArgs e)//mm
        {
            laserLine?.set_adr(i2c_adr_movm_mash);
            laserLine?.set_comp_cycle_ampl(to_double(textBox_cycle_ampl.Text));
        }

        private void but_cycle_time_Click(object sender, EventArgs e)//sec
        {
            laserLine?.set_adr(i2c_adr_movm_mash);
            laserLine?.set_comp_cycle_time(to_double(textBox_cycle_time.Text));
        }

        private void but_cycle_time_rel_Click(object sender, EventArgs e)//sec
        {
            laserLine?.set_adr(i2c_adr_movm_mash);
            laserLine?.set_comp_cycle_time_rel(to_double(textBox_cycle_time_rel.Text));
        }
        private void but_pos_disp_Click(object sender, EventArgs e)//mm
        {
            laserLine?.set_adr(i2c_adr_movm_mash);
           // laserLine?.set_pos_disp(to_double(textBox_pos_disp.Text));
            
            var text = textBox_pos_disp.Text;
            text = text.Replace(',', '.');
            var pos_z_mm = Convert.ToDouble(text);
            var pos_z_steps = (int)(pos_z_mm / 10 * laserLine.steps_per_unit_movm_mash);
            laserLine?.set_pos_disp(pos_z_steps);
        }

        double to_double(string val)
        {
            if (val == null) return 0;
            if (val.Length == 0) return 0;
            val = val.Replace(',', '.');
            try
            {
                return Convert.ToDouble(val);
            }
            catch
            {
                return double.NaN;
            }
            //return 
        }

        #endregion

        #region samara
        private void button_laser_roi_Click(object sender, EventArgs e)
        {
            var vals = textBox_laser_roi.Text.Split(' ');
            if(vals!=null)
            {
                if (vals.Length == 4)
                {
                    var x = Convert.ToInt32(vals[0]);
                    var y = Convert.ToInt32(vals[1]);
                    var w = Convert.ToInt32(vals[2]);
                    var h = Convert.ToInt32(vals[3]);
                    laser_roi_static = new Rectangle(x, y, w, h);
                }
            }
        }

        private void but_compens_begin_Click(object sender, EventArgs e)
        {
            compensation = true;
        }

        private void but_compens_end_Click(object sender, EventArgs e)
        {
            compensation = false;
        }

        private void but_jaka_send_Click(object sender, EventArgs e)
        {
            con1.send_mes(textBox_send_con.Text);
        }

        private void but_compens_gap_Click(object sender, EventArgs e)
        {
            compens_gap = to_double(textBox_compens_gap.Text);
        }

        private void but_comp_period_Click(object sender, EventArgs e)
        {
            timestamps = FormSettings.load_obj<List<PosTimestamp>>("timestamps1.json");
            /*foreach (PosTimestamp timestamp in timestamps) Console.WriteLine(timestamp.ToString());
            Console.WriteLine("__________________________");
            var unif_t = MovmentCompensation.uniform_time(timestamps);
            foreach (PosTimestamp timestamp in unif_t) Console.WriteLine(timestamp.ToString());
            Console.WriteLine("__________________________");*/
            var period_min = to_double(textBox_period_min.Text);
            var period_max = to_double(textBox_period_max.Text);
            var window_smooth = to_double(textBox_window_smooth.Text);
            movm = MovmentCompensation.comp_period(timestamps,period_min,period_max,window_smooth);
            movm.set_fi(Convert.ToInt32(textBox_movm_fi.Text));
            //timestamps = FormSettings.load_obj<List<PosTimestamp>>("timestamps1_stay.json");
            //foreach (PosTimestamp timestamp in timestamps) Console.WriteLine(timestamp.ToString());
        }
       
        private void but_execut_period_Click(object sender, EventArgs e)
        {

        }

        private void but_compens_record_Click(object sender, EventArgs e)
        {
            record_time =(long)( to_double(textBox_compens_time_rec.Text) * 1000d);
            timestamps = new List<PosTimestamp>();
            start_record_time = cur_time_to_int();
            record_times = true;
           label_comp_period.BeginInvoke((MethodInvoker)(() => label_period_ready.Text = "Период вычисляется..."));
        }
        void handler_compens_record(double pos1,double pos2=0, double pos3 = 0)
        {
            if(record_times)
            {
                var time = cur_time_to_int();
                if(time < start_record_time+record_time)
                {
                    timestamps.Add(new PosTimestamp(time, pos1, pos2, pos3));
                }
                else
                {
                    end_compens_record();
                }
            }
            
        }
        void end_compens_record()
        {
            record_times = false;
            //foreach (PosTimestamp timestamp in timestamps) Console.WriteLine(timestamp.ToString());
            
            var period_min = to_double(textBox_per_cal_period_min.Text);
            var period_max = to_double(textBox_per_cal_period_max.Text);
            var window_smooth = to_double(textBox_per_cal_window_smooth.Text);
            movm = MovmentCompensation.comp_period(timestamps, period_min, period_max, window_smooth);
            movm.set_fi(Convert.ToInt32(textBox_compens_period_delt.Text ));
            FormSettings.save_obj("timestamps1.json",timestamps);
           // label_period_ready.BeginInvoke((MethodInvoker)(() => label_period_ready.Text = "Период вычислен "+movm.period));

            label_comp_period.BeginInvoke((MethodInvoker)(() => label_comp_period.Text = "Период вычислен \n" + movm.period));
        }
        int cur_time_to_int()
        {
            var time = DateTime.Now.Hour * 1000 * 60*60 + DateTime.Now.Minute * 1000 *60+ DateTime.Now.Second *1000 + DateTime.Now.Millisecond;
            return time;
        }

        private void checkBox_compens_period_CheckedChanged(object sender, EventArgs e)
        {
            compens_period = ((CheckBox)sender).Checked;
        }

        private void but_period_fi_Click(object sender, EventArgs e)
        {
            movm?.set_fi(Convert.ToInt32(textBox_movm_fi.Text));
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void but_set_expos_Click(object sender, EventArgs e)
        {
            var cam_ind = Convert.ToInt32(textNimVid.Text);
            var exp_val = Convert.ToInt32(textBox_exp_val.Text);
            if(exp_val>0 && cam_ind>=0)
            {
                videoCaptures[cam_ind]?.Set(CapProp.AutoExposure, 1); 
            }
            else if(cam_ind >= 0)
            {
                videoCaptures[cam_ind]?.Set(CapProp.Exposure,exp_val);
            }
        }

        private void checkBox_window_auto_period_CheckedChanged(object sender, EventArgs e)
        {
            window_auto = ((CheckBox)sender).Checked;
        }

        int i2c_adr_sup_plate = 70;
        private void but_sup_temp_control_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_sup_plate);
            laserLine?.set_temp_control(Convert.ToInt32(textBox_sup_temp_control.Text));
        }

        private void but_sup_set_temp_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_sup_plate);
            var val = Regression.calcPolynSolv(koef_term_to_byte, to_double(textBox_sup_set_temp.Text));
            laserLine?.set_temp_value(val);
        }

        private void but_sup_set_led_r_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_sup_plate);
            laserLine?.set_led_r(Convert.ToInt32(textBox_sup_set_led_r.Text));
        }

        private void but_sup_set_led_g_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_sup_plate);
            laserLine?.set_led_g(Convert.ToInt32(textBox_sup_set_led_g.Text));
        }

        private void but_sup_set_led_b_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_sup_plate);
            laserLine?.set_led_b(Convert.ToInt32(textBox_sup_set_led_b.Text));
        }

        private void but_sup_temp_hyst_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_sup_plate);
            laserLine?.set_hyst_temp(Convert.ToInt32(textBox_sup_temp_hyst.Text));
        }

        private void but_sup_cool_pwm_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_sup_plate);
            laserLine?.set_cool_pwm(Convert.ToInt32(textBox_sup_cool_pwm.Text));
        }

        private void but_sup_heat_pwm_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(i2c_adr_sup_plate);
            laserLine?.set_heat_pwm(Convert.ToInt32(textBox_sup_heat_pwm.Text));
        }

        #endregion

        #region samara_new
        private void but_con_set_temp_Click(object sender, EventArgs e)
        {

            if (temp_control)
            {
                laserLine?.set_adr(i2c_adr_sup_plate);
                laserLine?.set_temp_control(0);
                but_con_set_temp.Text = "Установить температуру";
                but_con_ext_set_temp.Text = "Установить температуру";
                temp_control = false;
            }
            else
            {
                laserLine?.set_adr(i2c_adr_sup_plate);
                laserLine?.set_temp_control(1);
                Thread.Sleep(2);
                var val = Regression.calcPolynSolv(koef_term_to_byte, to_double(textBox_con_set_temp.Text));
                laserLine?.set_temp_value(val);
                Thread.Sleep(2);
                but_con_set_temp.Text = "Выключить контроль температуры";
                but_con_ext_set_temp.Text = "Выключить контроль температуры"; 
                temp_control = true;
            }
            
        }

        private void comboBox_syrenge_size_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ind_syr = ((ComboBox)sender).SelectedIndex;
            z_syrenge_offset = syringe_size_vals[ind_syr];
           // Console.WriteLine(syringe_size_vals[ind_syr]);
        }

        private void but_con_ext_set_temp_Click(object sender, EventArgs e)
        {

            if (temp_control)
            {
                laserLine?.set_adr(i2c_adr_sup_plate);
                laserLine?.set_temp_control(0);
                but_con_ext_set_temp.Text = "Установить температуру";
                but_con_set_temp.Text = "Установить температуру";
                temp_control = false;
            }
            else
            {
                laserLine?.set_adr(i2c_adr_sup_plate);
                laserLine?.set_temp_control(1);
                Thread.Sleep(20);
                var val = to_double(textBox_con_ext_set_temp.Text);// Regression.calcPolynSolv(koef_term_to_byte, to_double(textBox_con_ext_set_temp.Text));
                laserLine?.set_temp_value(val);
                Thread.Sleep(2);
                but_con_ext_set_temp.Text = "Выключить контроль температуры";
                but_con_set_temp.Text = "Выключить контроль температуры";
                temp_control = true;
            }
        }

        private void but_con_ext_set_z_pos_Click(object sender, EventArgs e)
        {
            var pos_z_mm = to_double(textBox_con_ext_set_z_pos.Text) +z_syrenge_offset;
           // var pos_z_steps = (int)(pos_z_mm / 10 * laserLine.steps_per_unit_z);
           // Console.WriteLine("pos_z_steps_man: " + pos_z_steps);
            laserLine?.set_adr(-1);
            laserLine?.set_move_z(pos_z_mm);
        }

        private void but_con_ext_set_z_zero_Click(object sender, EventArgs e)
        {
            laserLine?.set_adr(-1);
            laserLine?.set_home_z();
        }

        private void but_con_ext_find_ports_Click(object sender, EventArgs e)
        {
            DeviceArduino.find_ports(cb_ard_ext);
        }

        private void but_compens_dist_Click(object sender, EventArgs e)
        {
            compens_gap = to_double(textBox_compens_dist.Text);
        }

        private void but_compens_comp_period_Click(object sender, EventArgs e)
        {
            record_time = (long)(to_double(textBox_compens_comp_period.Text) * 1000d);
            timestamps = new List<PosTimestamp>();
            start_record_time = cur_time_to_int();
            record_times = true;
            label_comp_period.BeginInvoke((MethodInvoker)(() => label_comp_period.Text = "Период вычисляется..."));
        }

        private void checkBox_window_auto_CheckedChanged(object sender, EventArgs e)
        {
            window_auto = ((CheckBox)sender).Checked;
        }

        private void checkBox_compens_period_on_CheckedChanged(object sender, EventArgs e)
        {
            compens_period = ((CheckBox)sender).Checked;

        }

        private void checkBox_compens_visualize_CheckedChanged(object sender, EventArgs e)
        {
            visualise_compens = ((CheckBox)sender).Checked;

            imageBox_laser_compens.Visible = visualise_compens;
        }
        private void but_compens_stop_Click(object sender, EventArgs e)
        {
            compensation = false;
        }
        private void checkBox_comp_las_compens_CheckedChanged(object sender, EventArgs e)
        {
            comp_current_compens = ((CheckBox)sender).Checked;
            checkBox_compens_visualize.Checked = true;
            laserLine?.set_adr(-1);
            imProcType = FrameType.LasLin;
            if (comp_current_compens)
            {
                laserLine?.set_send_poses(1); Thread.Sleep(2);
                laserLine?.set_send_poses(1);Thread.Sleep(2);
                laserLine?.set_send_poses(1);Thread.Sleep(2);
                laserLine?.set_send_poses(1);Thread.Sleep(2);

                laserLine?.setShvpVel(200); Thread.Sleep(2);
                laserLine?.setShvpVel(200); Thread.Sleep(2);
                laserLine?.setShvpVel(200); Thread.Sleep(2);
                laserLine?.setShvpVel(200); Thread.Sleep(2);


                laserLine?.set_home_z(); Thread.Sleep(2);
                laserLine?.set_home_z(); Thread.Sleep(2);
                laserLine?.set_home_z(); Thread.Sleep(2);
                laserLine?.set_home_z(); Thread.Sleep(2);
                Thread.Sleep(6000);

                laserLine?.set_home_laser(); Thread.Sleep(2);
                laserLine?.set_home_laser(); Thread.Sleep(2);
                laserLine?.set_home_laser(); Thread.Sleep(2);
                laserLine?.set_home_laser(); Thread.Sleep(2);
                Thread.Sleep(4000);
                laserLine?.laserOn(); Thread.Sleep(2);
                laserLine?.laserOn(); Thread.Sleep(2);
                laserLine?.laserOn(); Thread.Sleep(2);
                laserLine?.laserOn(); Thread.Sleep(2);

               

                var las_pos = Convert.ToInt32(textBox_compens_las_pos.Text);
                laserLine?.setShvpPos(las_pos); Thread.Sleep(2);
                laserLine?.setShvpPos(las_pos); Thread.Sleep(2);
                laserLine?.setShvpPos(las_pos); Thread.Sleep(2);
                laserLine?.setShvpPos(las_pos); Thread.Sleep(2);
            }
            else
            {
                laserLine?.set_send_poses(0); Thread.Sleep(2);
                laserLine?.set_send_poses(0); Thread.Sleep(2);
                laserLine?.set_send_poses(0); Thread.Sleep(2);
                laserLine?.set_send_poses(0); Thread.Sleep(2);
            }
        }

        private void but_compens_period_delt_Click(object sender, EventArgs e)
        {
            movm?.set_fi(Convert.ToInt32(textBox_compens_period_delt.Text));
        }

        private void but_compens_las_pos_Click(object sender, EventArgs e)
        {
            var pos_las = Convert.ToInt32(textBox_compens_las_pos.Text);
            //scanner_config.pos_laser_compens = pos_las;
            laserLine?.setShvpPos(pos_las);
        }

        private void but_compens_las_zero_Click(object sender, EventArgs e)
        {
            laserLine?.set_home_laser();
        }

        private void but_compens_las_power_Click(object sender, EventArgs e)
        {
            laserLine?.setPower(Convert.ToInt32(textBox_compens_las_power.Text));
        }

        private void but_laser_onoff_Click(object sender, EventArgs e)
        {
            if(laser_on)
            {

                laserLine?.laserOff();
                but_laser_onoff.Text = "Включить лазер";
                laser_on = false;
            }
            else
            {
                laserLine?.laserOn();
                but_laser_onoff.Text = "Выключить лазер";
                laser_on = true;
            }
        }

        private void but_apply_cur_roi_Click(object sender, EventArgs e)
        {
            var vals = textBox_apply_cur_roi.Text.Split(' ');
            if (vals != null)
            {
                if (vals.Length == 4)
                {
                    var x = Convert.ToInt32(vals[0]);
                    var y = Convert.ToInt32(vals[1]);
                    var w = Convert.ToInt32(vals[2]);
                    var h = Convert.ToInt32(vals[3]);
                    laser_roi_static = new Rectangle(x, y, w, h);
                }
            }
        }

        private void but_compens_load_period_Click(object sender, EventArgs e)
        {
            timestamps = FormSettings.load_obj<List<PosTimestamp>>("timestamps1.json");
            var period_min = to_double(textBox_per_cal_period_min.Text);
            var period_max = to_double(textBox_per_cal_period_max.Text);
            var window_smooth = to_double(textBox_per_cal_window_smooth.Text);
            movm = MovmentCompensation.comp_period(timestamps, period_min, period_max, window_smooth);
            movm.set_fi(Convert.ToInt32(textBox_compens_period_delt.Text));
            //timestamps = FormSettings.load_obj<List<PosTimestamp>>("timestamps1_stay.json");
            //foreach (PosTimestamp timestamp in timestamps) Console.WriteLine(timestamp.ToString());
        }

        private void radioButton_static_surface_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton checkBox = (RadioButton)sender; 
            if (checkBox.Checked == true)
            {
                surface_type = Convert.ToInt32(checkBox.AccessibleName);
            }
        }
        #endregion

        #region drill

        private void but_dr_set_z_pos_Click(object sender, EventArgs e)
        {
            var pos_z_mm = to_double_textbox(textBox_dr_set_z_pos, -100, 100);
            if (pos_z_mm != double.NaN)
            {
                laserLine?.set_move_z(pos_z_mm);
            }
        }

        private void but_dr_set_z_div_Click(object sender, EventArgs e)
        {
            var div = to_double_textbox(textBox_dr_set_z_div, 0, 400);
            if(div!=double.NaN)
            {
                laserLine?.set_z_div((int)div);
            }
        }

        private void but_dr_set_z_home_Click(object sender, EventArgs e)
        {
            laserLine?.set_home_z();
        }

        private void but_dr_set_z_stop_Click(object sender, EventArgs e)
        {
            laserLine?.set_stop_z();
        }
        private void but_dr_set_z_zero_Click(object sender, EventArgs e)
        {
            laserLine?.set_pos_z(0);
        }

        private void but_dr_move_z_zero_Click(object sender, EventArgs e)
        {
            laserLine?.set_move_z(0);
        }

        private void but_dr_set_drill_pos_Click(object sender, EventArgs e)
        {
            var pos = to_double_textbox(textBox_dr_set_drill_pos , -100, 100);
            if (pos != double.NaN)
            {
                laserLine?.set_move_las(pos);
            }
        }

        private void but_dr_set_drill_div_Click(object sender, EventArgs e)
        {
            var div = to_double_textbox(textBox_dr_set_drill_div, 0, 400);
            if (div != double.NaN)
            {
                laserLine?.set_las_div((int)div);
            }
        }

        private void but_dr_set_drill_home_Click(object sender, EventArgs e)
        {
            laserLine?.set_home_laser();
        }

        private void but_dr_set_drill_stop_Click(object sender, EventArgs e)
        {
            laserLine?.set_stop_las();
        }   
        private void but_dr_set_drill_zero_Click(object sender, EventArgs e)
        {
            laserLine?.set_pos_las(0);
        }

        private void but_dr_move_drill_zero_Click(object sender, EventArgs e)
        {
            laserLine?.set_move_las(0);
        }
        private void imageBox1_Click(object sender, EventArgs e)
        {

        }



        #endregion
        private void textBox_robot_qs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                var text = textBox_robot_qs.Text;
                text.Replace("  ", " ");
                text.Replace("  ", " ");
                text.Replace("  ", " ");
                text.Replace("  ", " ");
                text.Replace(",", ".");
                // text
                var qs = text.Split(' ');


                var q_cur = new double[8] { 0, 0, 0, 0, 0, 0, 0, 0 };
                for (var i = 0; i < qs.Length && i<q_cur.Length; i++)
                {
                    q_cur[i] = Convert.ToDouble(qs[i]);
                }
                prin.t(q_cur);
                set_conf_robot_pulse(q_cur,RobotFrame.RobotType.KUKA);
            }
        }

        private void but_printing_Click(object sender, EventArgs e)
        {
            if(printing)
            {
                printing = false;
                but_printing.Text = "Печать";
            }
            else
            {
                printing = true;
                but_printing.Text = "Выкл печать";
            }
        }
        //void send_to_ard(TextBox textBox,)
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