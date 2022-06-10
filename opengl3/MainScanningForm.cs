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

namespace opengl3
{

    public partial class MainScanningForm : Form
    {
        #region var
        ImageBox[] imb_base = null;
        FrameType imProcType = FrameType.Test;
        LaserLine laserLine;
        Point cam_calib_p1 = new Point(0, 0);
        Point cam_calib_p2 = new Point(0, 0);
        bool settingWindow = false;
        Mat[] patt;
        Matrix<double> persp_matr = new Matrix<double>(new double[3,3] { {1,0,0},{0,1,0 },{0,0,1 } });
        TextBox[] textBoxes_Persp;
        int photo_number = 0;
        float markSize = 10f;
        StereoCameraCV stereocam = new StereoCameraCV();
        CameraCV cameraCVcommon;
        TCPclient con1;
        private const float PI = 3.14159265358979f;
        // private Size cameraSize = new Size(1280, 960);
        private Size cameraSize = new Size(640, 480);
        private GraphicGL GL1 = new GraphicGL();
        private VideoCapture myCapture1 = null;
        VideoWriter writer = null;
        private float z_mult_cam = 0.2f;
        private float[] vertex_buffer_data = { 0.0f };
        private float[] normal_buffer_data = { 0.0f };
        private float[] color_buffer_data = { 0.0f };
        volatile List<int> camera_ind = new List<int>();
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
        double minArea = 1;
        double maxArea = 10;
        string name_scan = "test_1008_1";
        string openGl_folder = @"virtual_stereo/test1";
        Point3d_GL p1_scan = new Point3d_GL(548.0, -60.0, 225.0);//(655.35, -73.21, 80.40);
        Point3d_GL p2_scan = new Point3d_GL(548.0, 60.0, 225.0);
        RobotModel RobotModel_1;

        Matrix<double> cameraDistortionCoeffs = new Matrix<double>(5, 1);
        Matrix<double> cameraMatrix = new Matrix<double>(3, 3);

        Matrix<double> cameraDistortionCoeffs_dist = new Matrix<double>(5, 1);
        Matrix<double> cameraMatrix_dist = new Matrix<double>(3, 3);

      

        float[] reconst = new float[3];
        float[] reconst_lines1 = new float[3];
        float[] reconst_lines2 = new float[3];
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
        #endregion
        public MainScanningForm()
        {
            InitializeComponent();
            init_vars();
            var model = new STLmodel();
            var mesh = model.parsingStl_GL4("cube_walls.STL");
            mesh = GL1.scaleMesh(mesh, 4f);
            mesh = GL1.translateMesh(mesh, -25, -70);
            GL1.addGLMesh(mesh,PrimitiveType.Triangles);
            //loadScanner();
            //loadStereo();
            loadScannerLin(new string[] { @"cam1\camera_cal_1006_1" }, @"cam1\las_cal_1006_1\1", @"cam1\lin_cal_3\1", @"cam1\scan_1006_pla\dif",new float[] { 0.1f,0.5f,0.5f });
            //loadScannerLin(new string[] { @"cam2\camera_cal_1006_1" }, @"cam2\las_cal_1006_1\1", @"cam2\lin_cal_3\1", @"cam2\scan_1006_3\dif", new float[] { 0.1f, 0.9f, 0.1f });
            GL1.buffersGl.sortObj();
        }
        void init_vars()
        {
            #region important
            combo_improc.Items.AddRange(new string[] { "Распознать шахматный паттерн","Стерео Исп", "Ничего" });
            GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(10, 0, 0), new Point3d_GL(0, 10, 0), new Point3d_GL(0, 0, 10));
            cameraDistortionCoeffs_dist[0, 0] = -0.1;

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

            patt = UtilOpenCV.generateImage_chessboard_circle(7, 6, 220);
            #endregion
            imb_base = new ImageBox[] { imBox_base_1, imBox_base_2 };
            minArea = 1.0 * k * k * 15;
            maxArea = 15 * k * k * 250;
            red_c = 252;
            // 
            //var patt_ph = new Mat("old_patt.png");//"old_patt.png" || @"cam2\test_circle\1_2.png"
            //patt[0] = patt_ph;

             generateImage3D_BOARD(7, 8, markSize);
            //oneCam();
            //var scan = Reconstruction.loadScan(@"cam1\pos_cal_Z_2609_2\test", @"cam1\las_cal_2609_3", @"cam1\table_scanl_2609_3", @"cam1\pos_basis_2609_2", 52.5, 30,40, SolveType.Complex, 0.1f, 0.1f, 0.8f,comboImages);

            //var scan = Reconstruction.loadScan(@"cam2\pos_cal_1906_1\test", @"cam2\las_cal_2", @"cam2\mouse_scan_1906_3", @"cam1\pos_basis_2609_2", 52.5, 30, 40, SolveType.Complex, 0.1f, 0.1f, 0.8f, comboImages);   
        }
        void loadStereo()
        {
            var cam_cal_1 = new string[] { @"cam1\camera_cal_1006_1" };
            var cam_cal_2 = new string[] { @"cam2\camera_cal_1006_1" };
            
            var frms1 = FrameLoader.loadPathsDiff(cam_cal_1, FrameType.MarkBoard);
            var frms2 = FrameLoader.loadPathsDiff(cam_cal_2, FrameType.MarkBoard);
            var cam1 = new CameraCV(frms1, new Size(6, 7), markSize, null);
            var cam2 = new CameraCV(frms2, new Size(6, 7), markSize, null);

            var cam_cal_stereo = new string[] { @"camera_cal_1006_1" };
            var frms = FrameLoader.loadPathsDiffDouble(cam_cal_stereo, FrameType.MarkBoard);

            stereocam = new StereoCameraCV(new CameraCV[] { cam1, cam2 }, new Size(6, 7), markSize, frms);

            var frms3 = FrameLoader.loadImages_stereoCV(@"cam1\" + cam_cal_stereo[0], @"cam2\" + cam_cal_stereo[0], FrameType.Pattern);
            comboImages.Items.AddRange(frms3);
            
        }
        void oneCam()
        {
            var cam_cal_paths = new string[] { @"ref_model\test3"};
            var frms = FrameLoader.loadPathsDiff(cam_cal_paths, FrameType.MarkBoard);
            var cam1 = new CameraCV(frms, new Size(6, 7), markSize, null);

            comboImages.Items.AddRange(frms);
          
        }
        void loadScanner()
        {
            var cam_cal_paths = new string[] { @"cam1\photo_9" , @"cam1\photo_11", @"cam1\photo_13" };//, @"cam1\photo_10"
            var scan_path = @"cam1\scan_2002_1"; 
            //var scan_path = @"cam1\las_cal_2002_1\test";
            var las_cal_path = @"cam1\las_cal_2002_1\test";

            

            var frms_las_cal = FrameLoader.loadImages_diff(las_cal_path, FrameType.LasHand,PatternType.Chess);
            var frms_scan = FrameLoader.loadImages_diff(scan_path, FrameType.LasHand, PatternType.Chess);

            var frms = FrameLoader.loadPathsDiff(cam_cal_paths,FrameType.Pattern);
            var cam1 = new CameraCV(frms, new Size(6, 7), markSize, null);
            cameraCVcommon = cam1;
            //comboImages.Items.AddRange(frms_las_cal);
            comboImages.Items.AddRange(frms_scan);
            //comboImages.Items.AddRange(frms);

            var scanner1 = new Scanner(cam1);
            if (scanner1.calibrateLaser(Frame.getMats(frms_las_cal), PatternType.Chess,GL1))
            {
                scanner1.addPoints(Frame.getMats(frms_scan));
                var p3d_scan_sc = scanner1.getPointsScene();
                var mesh_scan_sc = Point3d_GL.toMesh(p3d_scan_sc);
                GL1.addMeshWithoutNorm(mesh_scan_sc, PrimitiveType.Points,0.9f);
            }
            else
            {
                Console.WriteLine("CalibLas FALSE");
            }
        }

        void loadScannerLin(string[] cam_cal_paths, string las_cal_path, string lin_cal_path, string scand_path,float[] normrgb)
        {

            var frms_lin_cal = FrameLoader.loadImages_diff(lin_cal_path, FrameType.LasLin, PatternType.Chess);
            var frms_las_cal = FrameLoader.loadImages_diff(las_cal_path, FrameType.LasLin, PatternType.Chess);
            //var frms_scan = FrameLoader.loadImages_diff(scan_path, FrameType.LasLin, PatternType.Chess);

            var frms_scan_diff = FrameLoader.loadImages_diff(scand_path, FrameType.LasDif, PatternType.Chess);

            var frms = FrameLoader.loadPathsDiff(cam_cal_paths, FrameType.MarkBoard);
            var cam1 = new CameraCV(frms, new Size(6, 7), markSize, null);

            cameraCVcommon = cam1;
            comboImages.Items.AddRange(frms_lin_cal);
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

                        //GL1.addMeshWithoutNorm(mesh_scan_sc, PrimitiveType.Points, normrgb[0], normrgb[1], normrgb[2]);

                        var mesh_scan_stl = meshFromPoints(scanner1.getPointsLinesScene());
                        STLmodel.saveMesh(mesh_scan_stl, "test_" + normrgb[0] + "_" + normrgb[1] + "_" + normrgb[2]);
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
            }
        }

        #region laserScanner
        public void startScanLaser(int typeScan)//0 - defolt, 1 - dif
        {
            try
            {
                Thread robot_thread = new Thread(scan_resLaser);
                robot_thread.Start(typeScan);
            }
            catch
            {
            }
        }
        private void scan_resLaser(object obj)
        {
            int typescan = (int)obj;
            int counts = Convert.ToInt32(boxN.Text);

            string folder_scan = box_scanFolder.Text;
            var p1_cur_scan = robFrameFromTextBox(nameX, nameY, nameZ, nameA, nameB, nameC);
            var p2_cur_scan = robFrameFromTextBox(nameX2, nameY2, nameZ2, nameA, nameB, nameC);
            float x = (float)p1_cur_scan.x;

            var delx = (float)(p2_cur_scan.x - p1_cur_scan.x) / (float)counts;

            for (int i = 0; i < counts; i++)
            {

                if (typescan==0)
                {
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
            if (laserLine == null)
            {
                initLaserFast();
                Thread.Sleep(200);
                laserLine.laserOn();
            }
            laserLine?.setShvpPos((int)pos[0]);
            Console.WriteLine("cur_pos: " + (int)pos[0]);
            Thread.Sleep(300);
            if (folders.Length == imageBoxes.Length)
            {
                for (int i = 0; i < folders.Length; i++)
                {

                    UtilOpenCV.saveImage(imageBoxes[i], folders[i], pos[0] + ".png");
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
           // GL1.addMonitor(new Rectangle(0, 0, w , h ), 0);
            GL1.addMonitor(new Rectangle(0, 0, w/2, h/2), 0, new Vertex3d(0, 0, 0), new Vertex3d(50, 0, 0), 1);
            GL1.addMonitor(new Rectangle(w / 2, 0, w / 2, h / 2), 1);
            GL1.addMonitor(new Rectangle(w / 2, h / 2, w / 2, h / 2), 2);
            GL1.addMonitor(new Rectangle(0, h / 2, w / 2, h / 2), 3);
          // GL1.transRotZooms[1].xRot = 33;
            GL1.transRotZooms[1].off_x = -25;
            GL1.transRotZooms[1].off_y = 31;
           // GL1.transRotZooms[1].zoom = 2.6699;
            
            addButForMonitor(GL1, send.Size, send.Location);

            GL1.add_Label(lab_kor, lab_curCor,lab_TRZ);

            //UtilOpenCV.distortFolder(@"cam1\photo_5", GL1.cameraCV);
            //UtilOpenCV.distortFolder(@"cam2\photo_5", GL1.cameraCV);

            // startGenerate();
            //trB_SGBM_Enter();

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
        private void glControl1_Render(object sender, GlControlEventArgs e)
        {

            GL1.glControl_Render(sender, e);
            if (GL1.rendercout == 0)
            {
                UtilOpenCV.SaveMonitor(GL1);
            }
        
            var mat1_or =  GL1.matFromMonitor(0);
            var mat2_or = GL1.matFromMonitor(1);

             var mat1 = new Mat();
             var mat2 = new Mat();
             CvInvoke.Flip(mat1_or, mat1, FlipType.Vertical);
             CvInvoke.Flip(mat2_or, mat2, FlipType.Vertical);

            imBox_mark1.Image = mat1;
            imBox_mark2.Image = mat2;

            //GL1.printDebug(debugBox);

            /* var mat1 = UtilOpenCV.remapDistImOpenCvCentr(UtilOpenCV.GLnoise(mat1_or, 0, 10), cameraDistortionCoeffs_dist);
             var mat2 = UtilOpenCV.GLnoise(mat2_or, 0, 10);
             imBox_mark1.Image = mat2;=
             imBox_mark2.Image = UtilOpenCV.drawChessboard(mat2, new Size(6, 7));*/

            //imBox_disparity.Image = features.drawDescriptorsMatch(ref mat1_or, ref mat2_or);

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
                        var err = UtilOpenCV.calcSubpixelPrec(new Size(6, 7), GL1, markSize, 1, fr_ar[i][j].im, fr_ar[i][j].name,k);
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
                var mat1 = fr.im_sec;
                var mat2 = fr.im;
                if (fr.frameType == FrameType.MarkBoard)
                {

                    imBox_debug1.Image = UtilOpenCV.drawChessboard(mat1, new Size(6, 7));
                    imBox_debug2.Image = UtilOpenCV.drawChessboard(mat2, new Size(6, 7));
                }
                else if (fr.frameType == FrameType.Pattern)
                {
                    imBox_debug1.Image = stereocam.cameraCVs[0].undist(mat1);
                    imBox_debug2.Image = stereocam.cameraCVs[1].undist(mat2);
                    //imBox_debug1.Image = FindCircles.findCircles(mat1, null, new Size(6, 7));
                    //imBox_debug2.Image = FindCircles.findCircles(mat2, null, new Size(6, 7));
                }
                imageBox1.Image = mat1;
                imageBox2.Image = mat2;
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
                    mat3= UtilOpenCV.drawChessboard(mat3, new Size(6, 7));
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
                    var mat1 = fr.im.Clone();
                    var mat2 = fr.im.Clone();
                    var rgb = mat1.Split();
                    var im1 = (rgb[0] + rgb[1] + rgb[2]).ToImage<Gray, Byte>();
                    var im2 = im1.Clone();
                
                    var ps = Detection.detectLineDiff(mat2,3,imBox_base);
                    UtilOpenCV.drawPointsF(mat2, ps, 0, 255, 0,2);
                    //imBox_base.Image = im2;
                    imageBox2.Image = im1;
                    imageBox1.Image = mat2;
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
                    mat1 =  UtilOpenCV.drawChessboard(mat1, new Size(6, 7));
                    imageBox1.Image = mat1;

                }
                else if (fr.frameType == FrameType.Test)
                {
                    //findLaserArea(fr.im, imageBox1, (int)red_c);
                    //imageBox_debug_cam_2.Image =  drawDescriptors(fr.im);
                    //findContourZ(fr.im, imageBox1, (int)red_c, DirectionType.Up);
                    //var gauss = ContourAnalyse.findContourZ(fr.im, null, 245, DirectionType.Down);
                    //UtilOpenCV.drawPointsF(mat1, UtilMatr.doubleToPointF(gauss), 0, 0, 255);
                }

                else if (fr.frameType == FrameType.MarkBoard)
                {
                    imBox_debug1.Image = UtilOpenCV.drawChessboard(fr.im, new Size(6, 7));
                    imageBox1.Image = UtilOpenCV.drawChessboard(fr.im, new Size(6, 7));
                }
                else if (fr.frameType == FrameType.Pattern)
                {

                    imBox_debug1.Image = cameraCVcommon.undist(fr.im.Clone());
                    imageBox1.Image = FindCircles.findCircles(fr.im, null, new Size(7, 7), false);
                }
            }


            //imageBox2.Image = fr.im;
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
            UtilOpenCV.calcSubpixelPrecCircle(new Size(6, 7), ptt);
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

            calibr_make_photo();
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
            UtilOpenCV.generateImagesFromAnotherFolderStereo(new string[] { @"virtual_stereo\test1\monitor_0", @"virtual_stereo\test1\monitor_1" },GL1,new CameraCV(cameraMatrix_dist,cameraDistortionCoeffs_dist));
     
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
            if (con1 == null)
            {
                con1 = new TCPclient();
            }
            con1.Connection(30006, "172.31.1.147");
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
            GL1.SaveToFolder(openGl_folder, Convert.ToInt32(textBox_monitor_id.Text));
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
            if (con1 != null)
            {
                con1.send_mes("q\n");
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
            RobotModel_1 = new RobotModel(new robFrame(600, 100, 150, 0.3, 0.1, 1.4), 8888);
            Thread.Sleep(1430);
            RobotModel_1.move(new robFrame(620, 120, 150, 0.3, 0.1, 1.4), 30, 30);
            RobotModel_1.move(new robFrame(590, 110, 150, 0.3, 0.1, 1.4), 30, 30);
            Thread.Sleep(500);

            RobotModel_1.move(new robFrame(620, 120, 150, 0.3, 0.1, 1.4), 30, 30);
            //con1 = new TCPclient();
            // con1.Connection(8888, "127.0.0.1");
            // Thread.Sleep(400);
            // var mes = con1.reseav();
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

                if ((camera_ind.Count > 0) && ((int)cap.Ptr == camera_ind[0]))
                {
                    
                    cap.Retrieve(mat_global[0]);
                    imageBox1.Image = mat_global[0];
                    imBox_input_1.Image = mat_global[0];
                    imProcess(mat_global[0],1);

                }
                else if ((camera_ind.Count > 1) && ((int)cap.Ptr == camera_ind[1]))
                {
                    cap.Retrieve(mat_global[1]);                  
                    imBox_base.Image = stereoProc(mat_global[0], mat_global[1]);
                    imageBox2.Image = mat_global[1];
                    imBox_input_2.Image = mat_global[1];
                    imProcess(mat_global[1],2);
                }
            }
        }

        void imProcess(Mat mat,int ind)
        {

            switch (imProcType)
            {
                case FrameType.Test:
                    imb_base[ind-1].Image = mat;
                    break;
                case FrameType.MarkBoard:
                    imb_base[ind - 1].Image = UtilOpenCV.drawChessboard(mat, new Size(6, 7),false,false,CalibCbType.FastCheck);
                    break;
                case FrameType.Undist:
                    imb_base[ind - 1].Image = stereocam.remapCam(mat, ind);
                    break;
                default:
                    break;
            }
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
            capture.SetCaptureProperty(CapProp.FrameWidth, cameraSize.Width);
            capture.SetCaptureProperty(CapProp.FrameHeight, cameraSize.Height);
            //capture.SetCaptureProperty(CapProp.Contrast, 30);
            camera_ind.Add((int)capture.Ptr);
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
        public void send_buffer_img(Image<Gray, Byte> im2, PrimitiveType type)
        {
            int lenght = im2.Width * im2.Height;
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
            GL1.addMesh(vertex_buffer_data, type);
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
        private float[] meshFromImage(Image<Gray, Byte> im2)
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
        float[] meshFromPoints(Point3d_GL[][] points3d_in)
        {
            var points3d = orderPoints(points3d_in);
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

        void generateImage3D_BOARD(int n, int k, float sidef)
        {
            float side = sidef * 2;

            float w = sidef * (float)n;
            float h = sidef * (float)k;
            float offx = -sidef;
            float offy =  -sidef;
            float z = 0f;
            float[] square_buf = {
                            0.0f,0.0f,0.0f, // triangle 1 : begin
                            0.0f,sidef, 0.0f,
                           sidef,sidef, 0.0f, // triangle 1 : end
                            sidef, sidef,0.0f, // triangle 2 : begin
                           sidef,0.0f,0.0f,
                            0.0f, 0.0f,0.0f};

            for (float x = 0; x < w; x += side)
            {
                for (float y = 0; y < h; y += side)
                {
                    GL1.addGLMesh(square_buf, PrimitiveType.Triangles, x + offx, y + offy, z);
                }
            }

            for (float x = sidef; x < w; x += side)
            {
                for (float y = sidef; y < h; y += side)
                {
                    GL1.addGLMesh(square_buf, PrimitiveType.Triangles, x + offx, y + offy,z);
                }
            }

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
            var mat_out = Regression.paintRegression(mat_im, stroka);
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
            videoframe_count = 0;
            cap_mats = new List<Mat>();
            var capture = new VideoCapture(filepath);
            capture.ImageGrabbed += loadingVideo;
            capture.Start();
            while (videoframe_count < 25)
            {
            }
            capture.ImageGrabbed -= loadingVideo;
            capture.Stop();
            string name = Path.GetFileName(filepath);
            var coords = name.Split(new char[] { ' ' });
            var name_pos = new Point3d_GL(Convert.ToDouble(coords[0]),
                                            Convert.ToDouble(coords[1]),
                                            Convert.ToDouble(coords[2]));
            Console.WriteLine(name);

            return new VideoFrame(cap_mats.ToArray(), name_pos, name);

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
        void loadingVideo(object sender, EventArgs e)
        {
            Mat im = new Mat();
            var cap = (VideoCapture)sender;
            cap.Retrieve(im);
            var mat_c = new Mat();
            im.CopyTo(mat_c);
            cap_mats.Add(mat_c);
            // Console.WriteLine(frame_count);
            videoframe_count++;
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

        private void comboBox_portsArd_SelectedIndexChanged(object sender, EventArgs e)
        {
            portArd = (string)((ComboBox)sender).SelectedItem;
        }
        #endregion

        private void but_setShvpPos_Click(object sender, EventArgs e)
        {
            laserLine?.setShvpPos(Convert.ToInt32(textBox_shvpPos.Text));
        }

        private void combo_improc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(combo_improc.SelectedIndex==0)
            {
                imProcType = FrameType.MarkBoard;
            }
            else if(combo_improc.SelectedIndex == 1)
            {
                imProcType = FrameType.Undist;
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
    }

}





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

