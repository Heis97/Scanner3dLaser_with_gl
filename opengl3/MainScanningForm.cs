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

namespace opengl3
{

    public partial class MainScanningForm : Form
    {
        #region var
        float markSize = 10f;
        StereoCameraCV stereocam;
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
        Frame calib_frame;
        List<Flat_4P> laserFlat;
        RobotModel RobotModel_1;

        Matrix<double> cameraDistortionCoeffs = new Matrix<double>(5, 1);
        Matrix<double> cameraMatrix = new Matrix<double>(3, 3);

        Matrix<double> cameraDistortionCoeffs_dist = new Matrix<double>(5, 1);
        Matrix<double> cameraMatrix_dist = new Matrix<double>(3, 3);

        Matrix<double> cameraDistortionCoeffs1 = new Matrix<double>(5, 1);
        Matrix<double> cameraMatrix1 = new Matrix<double>(3, 3);
        Matrix<double> cameraDistortionCoeffs2 = new Matrix<double>(5, 1);
        Matrix<double> cameraMatrix2 = new Matrix<double>(3, 3);
        float[] reconst = new float[3];
        int k = 1;
        bool writ = false;
        int bin_pos = 40;
        float[] cube_buf = {
                            -1.0f,-1.0f,-1.0f, // triangle 1 : begin
                            -1.0f,-1.0f, 1.0f,
                            -1.0f, 1.0f, 1.0f, // triangle 1 : end
                            1.0f, 1.0f,-1.0f, // triangle 2 : begin
                            -1.0f,-1.0f,-1.0f,
                            -1.0f, 1.0f,-1.0f, // triangle 2 : end
                            1.0f,-1.0f, 1.0f,
                            -1.0f,-1.0f,-1.0f,
                            1.0f,-1.0f,-1.0f,
                            1.0f, 1.0f,-1.0f,
                            1.0f,-1.0f,-1.0f,
                            -1.0f,-1.0f,-1.0f,
                            -1.0f,-1.0f,-1.0f,
                            -1.0f, 1.0f, 1.0f,
                            -1.0f, 1.0f,-1.0f,
                            1.0f,-1.0f, 1.0f,
                            -1.0f,-1.0f, 1.0f,
                            -1.0f,-1.0f,-1.0f,
                            -1.0f, 1.0f, 1.0f,
                            -1.0f,-1.0f, 1.0f,
                            1.0f,-1.0f, 1.0f,
                            1.0f, 1.0f, 1.0f,
                            1.0f,-1.0f,-1.0f,
                            1.0f, 1.0f,-1.0f,
                            1.0f,-1.0f,-1.0f,
                            1.0f, 1.0f, 1.0f,
                            1.0f,-1.0f, 1.0f,
                            1.0f, 1.0f, 1.0f,
                            1.0f, 1.0f,-1.0f,
                            -1.0f, 1.0f,-1.0f,
                            1.0f, 1.0f, 1.0f,
                            -1.0f, 1.0f,-1.0f,
                            -1.0f, 1.0f, 1.0f,
                            1.0f, 1.0f, 1.0f,
                            -1.0f, 1.0f, 1.0f,
                            1.0f,-1.0f, 1.0f
                                    };
        List<Mat> cap_mats = new List<Mat>();
        int[,] flatInds = new int[1, 1];
        int[] flatLasInds = new int[1];
        int colr = 1;
        Features features = new Features();
        #endregion
        public MainScanningForm()
        {
            mat_global[0] = new Mat();
            mat_global[1] = new Mat();
            mat_global[2] = new Mat();
            InitializeComponent();
            minArea = 1.0 * k * k * 15;
            maxArea = 15 * k * k * 250;
            red_c = 252;

             var stl_loader = new STLmodel();
             var mesh = stl_loader.parsingStl_GL4(@"cube_scene.STL");
             GL1.addGLMesh(mesh, PrimitiveType.Triangles);
            //loadScan(@"cam1\pos_cal_big_Z\test", @"cam1\las_cal_big_1", @"cam1\scanl_big_2", @"cam1\pos_basis_big", 53.8, 30, SolveType.Complex, 0.1f, 0.8f, 0.1f);

           
             var frms1 = FrameLoader.loadImages_chess(@"virtual_stereo\test5\monitor_2");
             comboImages.Items.AddRange(frms1);
             var cam1 = new CameraCV(frms1, new Size(6, 7));
             var frms2 = FrameLoader.loadImages_chess(@"virtual_stereo\test5\monitor_3");
             comboImages.Items.AddRange(frms2);
             var cam2 = new CameraCV(frms2, new Size(6, 7));

             stereocam = new StereoCameraCV(new CameraCV[] { cam1, cam2 });


            if (comboImages.Items.Count > 0)
            {
                comboImages.SelectedIndex = 0;
            }


            cameraDistortionCoeffs_dist[0, 0] = -0.1;
            //generateImage3D_BOARD(7, 8, 10f);
            //generateImage3D(7, 0.5f,  markSize);
            GL1.addFrame(new Point3d_GL(0, 0, 0),
                new Point3d_GL(10, 0, 0),
                new Point3d_GL(0, 10, 0),
                new Point3d_GL(0, 0, 10));
            GL1.buffersGl.sortObj();
        }

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
            GL1.addMonitor(new Rectangle(0, 0, w / 2, h / 2), 0, new Vertex3d(0, 0, 0), new Vertex3d(10, 0, 0), 1);
            GL1.addMonitor(new Rectangle(w / 2, 0, w / 2, h / 2), 1);
            GL1.addMonitor(new Rectangle(w / 2, h / 2, w / 2, h / 2), 2);
            GL1.addMonitor(new Rectangle(0, h / 2, w / 2, h / 2), 3);
            Console.WriteLine();
            addButForMonitor(GL1, send.Size, send.Location);

            GL1.add_Label(lab_kor, lab_curCor);
            GL1.add_TextBox(debugBox);
            // startGenerate();

            //  trB_SGBM_Enter();

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

            var points3D = new MCvPoint3D32f[]
            {
                new MCvPoint3D32f(0,0,0),
                new MCvPoint3D32f(60, 0, 0),
                new MCvPoint3D32f(0, 60, 0),
                new MCvPoint3D32f(60, 60, 0)
            };
            var points2D = new System.Drawing.PointF[points3D.Length];
            for (int i = 0; i < points3D.Length; i++)
            {
                var p = GL1.calcPixel(new Vertex4f(points3D[i].X, points3D[i].Y, points3D[i].Z, 1), id_mon);
                points2D[i] = new System.Drawing.PointF(p.X, p.Y);
            }
            UtilOpenCV.drawTours(mat1, PointF.toPointF(points2D), 255, 0, 0);
            GL1.cameraCV.compPos(points3D, points2D);
            var mxCam = matrixFromCam(GL1.cameraCV);
            prin.t(mxCam);
            prin.t("-------------------");
            prin.t(GL1.Vs[id_mon]);
            prin.t("-------------------");
            imBox_mark2.Image = mat1;
        }

        private void glControl1_Render(object sender, GlControlEventArgs e)
        {

            GL1.glControl_Render(sender, e);
            //GL1.printDebug(debugBox);
            if (GL1.rendercout == 0)
            {
                UtilOpenCV.SaveMonitor(GL1);
            }

           // imBox_mark1.Image = UtilOpenCV.remapDistImOpenCvCentr(GL1.matFromMonitor(0), cameraDistortionCoeffs_dist);
           // imBox_mark2.Image = UtilOpenCV.remapDistImOpenCvCentr(GL1.matFromMonitor(1), cameraDistortionCoeffs_dist);
            // imBox_mark1.
            var mat1 = stereocam.cameraCVs[0].undist(UtilOpenCV.remapDistImOpenCvCentr(GL1.matFromMonitor(0), cameraDistortionCoeffs_dist));
            var mat2 = stereocam.cameraCVs[1].undist(UtilOpenCV.remapDistImOpenCvCentr(GL1.matFromMonitor(1), cameraDistortionCoeffs_dist));
            imBox_mark1.Image = mat1;
            imBox_mark2.Image = mat2;
            
            
            imBox_disparity.Image = features.drawDescriptorsMatch(ref mat1, ref mat2);
            reconst = features.reconstuctScene(stereocam, features.desks1, features.desks2, features.mchs);
          //  prin.t(reconst);
           // prin.t("_____________-");
           
            //imBox_3dDebug.Image  = stereocam.drawEpipolarLines(mat1, mat2, features.ps1)[0];

            // imBox_mark2.Image = UtilOpenCV.calcSubpixelPrec(GL1.matFromMonitor(0), new Size(6, 7),GL1,markSize);
            //imBox_disparity.Image =  stereocam.epipolarTest(GL1.matFromMonitor(1), GL1.matFromMonitor(0));
            // imBox_mark1.Image =  drawChessboard((Mat)imBox_mark1.Image, new Size(6, 7));
            //  imBox_mark2.Image =  drawChessboard((Mat)imBox_mark2.Image, new Size(6, 7));


        }
        Matrix4x4f matrixFromCam(CameraCV cam)
        {
            var rotateMatrix = new Matrix<double>(3, 3);
            CvInvoke.Rodrigues(cam.cur_r, rotateMatrix);
            var tvec = UtilMatr.toVertex3f(cam.cur_t);
            var mx = UtilMatr.assemblMatrix(rotateMatrix, tvec);
            // var invMx = mx.Inverse;
            return mx;
        }
        private void Form1_mousewheel(object sender, MouseEventArgs e)
        {
            GL1.Form1_mousewheel(sender, e);
        }

        #endregion

        #region buttons
        private void but_SubpixPrec_Click(object sender, EventArgs e)
        {       
           //UtilOpenCV.calcSubpixelPrec(new Size(6, 7),GL1,markSize,0);
            reconst = GL1.translateMesh(reconst, 0, 0, 100);
            GL1.addMeshWithoutNorm(reconst, PrimitiveType.Points, 1f, 0, 0);
            GL1.buffersGl.sortObj();
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

        private void comboImages_SelectionChangeCommitted(object sender, EventArgs e)
        {
            Console.WriteLine(comboImages.SelectedItem);
            var fr = (Frame)comboImages.SelectedItem;
            Console.WriteLine(fr.type);
            if (fr.type == FrameType.Pos)
            {
                FindMark.finPointFsFromIm(fr.im, bin_pos, imageBox1,imageBox4,maxArea,minArea);
            }
            else if (fr.type == FrameType.Las)
            {
                ContourAnalyse.findContourZ(fr.im, imageBox1, (int)red_c, DirectionType.Down);
                //findContourZ(fr.im, imageBox1, (int)red_c, DirectionType.Up);
            }
            else if (fr.type == FrameType.Test)
            {
                //findLaserArea(fr.im, imageBox1, (int)red_c);
                //imageBox_debug_cam_2.Image =  drawDescriptors(fr.im);
                //findContourZ(fr.im, imageBox1, (int)red_c, DirectionType.Up);
            }
            else if (fr.type == FrameType.Stereo)
            {
                imBox_debug1.Image = UtilOpenCV.drawChessboard(fr.im, new Size(6, 7));
                imBox_debug2.Image = UtilOpenCV.drawChessboard(fr.im_sec, new Size(6, 7));
                imageBox1.Image = fr.im_sec;
            }
            else if (fr.type == FrameType.MarkBoard)
            {
                imBox_debug1.Image = UtilOpenCV.drawChessboard(fr.im, new Size(6, 7));
            }
            imageBox2.Image = fr.im;
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
                    //finPointFsFromIm(mat_global[0], 40, imageBox1);
                    imageBox1.Image = mat_global[0];

                }
                else if ((camera_ind.Count > 1) && ((int)cap.Ptr == camera_ind[1]))
                {
                    cap.Retrieve(mat_global[1]);
                    //finPointFsFromIm(mat_global[1], 40, imageBox2);
                    imageBox2.Image = mat_global[1];

                }
            }
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
            capture.SetCaptureProperty(CapProp.Contrast, 30);
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
        private float[] meshFromImage(Image<Gray, float> im2)
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
        void generateImage(int n, double k)
        {
            int im_side = 700;
            int side = im_side / n;
            var im_ret = new Image<Gray, Byte>(im_side, im_side);
            var pattern_s = new Size(side, side);
            int q_side = (int)(k * side);
            var quad_s = new Size(q_side, q_side);
            /*Console.WriteLine(k + "k-");
            Console.WriteLine(side + "s-");
            Console.WriteLine(q_side + "q-");*/

            var p_start = new List<Point>();
            for (int x = 0; x <= im_ret.Width - pattern_s.Width; x += pattern_s.Width)
            {
                for (int y = 0; y <= im_ret.Height - pattern_s.Height; y += pattern_s.Height)
                {
                    p_start.Add(new Point(x, y));
                }
            }
            for (int x = 0; x < im_ret.Width; x++)
            {
                for (int y = 0; y < im_ret.Height; y++)
                {
                    im_ret.Data[y, x, 0] = 255;
                }
            }

            for (int i = 0; i < p_start.Count; i++)
            {
                for (int x = p_start[i].X; x < p_start[i].X + quad_s.Width; x++)
                {
                    for (int y = p_start[i].Y; y < p_start[i].Y + quad_s.Height; y++)
                    {
                        im_ret.Data[y, x, 0] = 0;
                    }
                }
            }

            im_ret.Save("black_sq_" + n + "_" + k + ".png");
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
            float offy = -h + sidef;

            float[] square_buf = {
                            0.0f,0.0f,0.0f, // triangle 1 : begin
                            0.0f,sidef, 0.0f,
                           sidef,sidef, 0.0f, // triangle 1 : end
                            sidef, sidef,0.0f, // triangle 2 : begin
                           sidef,0.0f,0.0f,
                            0.0f, 0.0f,0.0f };

            for (float x = 0; x < w; x += side)
            {
                for (float y = 0; y < h; y += side)
                {
                    GL1.addGLMesh(square_buf, PrimitiveType.Triangles, x + offx, y + offy);
                }
            }

            for (float x = sidef; x < w; x += side)
            {
                for (float y = sidef; y < h; y += side)
                {
                    GL1.addGLMesh(square_buf, PrimitiveType.Triangles, x + offx, y + offy);
                }
            }

        }
        void generateImage_BOARD(int n, int m)
        {

            int side = 500;
            int q_side = side / 2;
            int im_side_w = q_side * n;
            int im_side_h = q_side * m;
            var im_ret = new Image<Gray, Byte>(im_side_w, im_side_h);
            var pattern_s = new Size(side, side);

            var quad_s = new Size(q_side, q_side);
            /*Console.WriteLine(k + "k-");
            Console.WriteLine(side + "s-");
            Console.WriteLine(q_side + "q-");*/

            var p_start = new List<Point>();
            for (int x = 0; x < im_ret.Width; x += pattern_s.Width)
            {
                for (int y = 0; y < im_ret.Height; y += pattern_s.Height)
                {
                    p_start.Add(new Point(x, y));
                }
            }
            for (int x = q_side; x < im_ret.Width; x += pattern_s.Width)
            {
                for (int y = q_side; y < im_ret.Height; y += pattern_s.Height)
                {
                    p_start.Add(new Point(x, y));
                }
            }
            Console.WriteLine(p_start.Count);
            for (int x = 0; x < im_ret.Width; x++)
            {
                for (int y = 0; y < im_ret.Height; y++)
                {
                    im_ret.Data[y, x, 0] = 255;
                }
            }

            for (int i = 0; i < p_start.Count; i++)
            {
                for (int x = p_start[i].X; x < p_start[i].X + quad_s.Width; x++)
                {
                    for (int y = p_start[i].Y; y < p_start[i].Y + quad_s.Height; y++)
                    {
                        im_ret.Data[y, x, 0] = 0;
                    }
                }
            }

            im_ret.Save("black_br_" + n + "_" + m + ".png");
        }


        #endregion

        #region video
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

/*err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixAspectRatio, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixAspectRatio = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixFocalLength, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixFocalLength = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixIntrinsic, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixIntrinsic = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixK2, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixK2 = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixK3, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixK3 = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixK4, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixK4 = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixK5, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixK5 = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixK6, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixK6 = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixPrincipalPoint, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixPrincipalPoint = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixTauxTauy, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixTauxTauy = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.FixS1S2S3S4, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("FixS1S2S3S4 = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.RationalModel, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("RationalModel = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.SameFocalLength, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("SameFocalLength = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.ThinPrismModel, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("ThinPrismModel = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.TiltedModel, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("TiltedModel = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.UseIntrinsicGuess, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("UseIntrinsicGuess = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.UseLU, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("UseLU = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.UseQR, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("UseQR = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.ZeroDisparity, new MCvTermCriteria(30), out rvecs, out tvecs);
            Console.WriteLine("ZeroDisparity = " + err);
            err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, mtx, dist, CalibType.ZeroTangentDist, new MCvTermCriteria(30), out rvecs, out tvecs);*/