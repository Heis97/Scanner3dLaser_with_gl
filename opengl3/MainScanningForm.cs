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
        int startGen = 0;
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

           // var stl_loader = new STLmodel();
           // var mesh = stl_loader.parsingStl_GL4(@"cube_scene.STL");
          //  GL1.addGLMesh(mesh, PrimitiveType.Triangles);
            //loadScan(@"cam1\pos_cal_big_Z\test", @"cam1\las_cal_big_1", @"cam1\scanl_big_2", @"cam1\pos_basis_big", 53.8, 30, SolveType.Complex, 0.1f, 0.8f, 0.1f);

           /* var frLdr = new FrameLoader();
            var frms1 = frLdr.loadImages_chess(@"virtual_stereo\test5\monitor_2");
            comboImages.Items.AddRange(frms1);
            var cam1 = new CameraCV(frms1, new Size(6, 7));
            var frms2 = frLdr.loadImages_chess(@"virtual_stereo\test5\monitor_3");
            comboImages.Items.AddRange(frms2);
            var cam2 = new CameraCV(frms2, new Size(6, 7));

            stereocam = new StereoCameraCV(new CameraCV[] { cam1, cam2 });*/

            if (comboImages.Items.Count>0)
            {
                comboImages.SelectedIndex = 0;
            }
            

            cameraDistortionCoeffs_dist[0, 0] = -0.1;
            generateImage3D_BOARD(8, 7, markSize);
            GL1.buffersGl.sortObj();
        }
        Mat calcSubpixelPrec(Mat mat, Size size)
        {
            var len = size.Width * size.Height;
            var obp = new MCvPoint3D32f[len];
            var cornF = new System.Drawing.PointF[len];
            var cornF_GL = new System.Drawing.PointF[len];
            var cornF_delt = new System.Drawing.PointF[len];
            var sum = new System.Drawing.PointF(0, 0);
            var kvs = new System.Drawing.PointF(0, 0);
            var S = new System.Drawing.PointF(0, 0);
            var ret =   compChessCoords(mat, ref obp, ref cornF,size);
            if(!ret)
            {
                return null;
            }
            var mvpMtx = GL1.compMVPmatrix(GL1.transRotZooms[0]);
            for(int i=0; i<obp.Length;i++)
            {
               var p_GL =  GL1.calcPixel(new Vertex4f(markSize* obp[i].Y, -markSize * obp[i].X, obp[i].Z, 1),mvpMtx[3]);
                cornF_GL[i] = new System.Drawing.PointF(p_GL.X, p_GL.Y);
                var p_chess = cornF[i];
                cornF_delt[i] = new System.Drawing.PointF(p_GL.X-p_chess.X, p_GL.Y-p_chess.Y);
                sum.X += cornF_delt[i].X;
                sum.Y += cornF_delt[i].Y;
                //prin.t(cornF[i].ToString());
                //prin.t(cornF_GL[i].ToString());
                //prin.t("_________________");
            }
            
            for (int i = 0; i < obp.Length; i++)
            {
                kvs.X += (float) Math.Pow((cornF_delt[i].X - sum.X),2);
                kvs.Y += (float)Math.Pow((cornF_delt[i].Y - sum.Y), 2);
            }
            S.X = kvs.X / len;
            S.Y = kvs.Y / len;
            Console.WriteLine(S);
            drawPointsF(mat, cornF, 255, 0, 0,1);
            drawPointsF(mat, cornF_GL, 0, 0, 255,1);
            return mat;
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
                saveImage(imageBox1, imageBox2, name, name_scan);

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

        void saveImage(ImageBox box, string folder, string name)
        {
            var mat1 = (Mat)box.Image;
            saveImage(mat1, folder, name);
        }
        void saveImage(Mat mat1, string folder, string name)
        {
            if (mat1 != null)
            {
                Directory.CreateDirectory(folder);
                var im1 = mat1.ToImage<Bgr, byte>();
                Console.WriteLine(folder + "\\" + name);
                im1.Save(folder + "\\" + name);
            }
        }
        void saveImage(ImageBox box1, ImageBox box2, string name, string folder)
        {
            var mat1 = (Mat)box1.Image;

            var im1 = mat1.ToImage<Bgr, byte>();
            Console.WriteLine("cam2\\" + folder + "\\" + name);
            Directory.CreateDirectory("cam1\\" + folder);
            im1.Save("cam1\\" + folder + "\\" + name);

            mat1 = (Mat)box2.Image;
            Directory.CreateDirectory("cam2\\" + folder);
            im1 = mat1.ToImage<Bgr, byte>();
            im1.Save("cam2\\" + folder + "\\" + name);

        }
        #endregion
        #region findLineLaser

        private Mat findLaserArea(Mat im, ImageBox box, int bin)
        {

            List<double[]> strk = new List<double[]>();
            double[,] stroka = new double[im.Width, 2];
            Image<Gray, Byte> im_gray = im.ToImage<Gray, Byte>();
            Image<Bgr, Byte> im_rgb = im.ToImage<Bgr, Byte>();
            // im_rgb.ThresholdTrunc(new Bgr(10, 10, 250));
            var im_tr = im_rgb.ThresholdBinary(new Bgr(253, 253, 6), new Bgr(255, 255, 255));
            var im_tr_gr = (im_tr.Mat).ToImage<Gray, Byte>();


            CvInvoke.Threshold(im_gray, im_gray, bin, 255, ThresholdType.Binary);
            CvInvoke.Threshold(im_tr_gr, im_tr_gr, 253, 255, ThresholdType.Binary);

            // CvInvoke.Threshold(im, im, bin, 255, ThresholdType.);
            //var im_res = new Image<Gray, Byte>(im.Width, im.Height);
            Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(1, 1));
            Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            Mat kernel2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 2), new Point(1, 1));
            Mat ellips7 = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(7, 7), new Point(1, 1));
            var im_res = im_gray.MorphologyEx(MorphOp.Gradient, kernel5, new Point(-1, -1), 6, BorderType.Default, new MCvScalar());

            imageBox3.Image = im_gray.MorphologyEx(MorphOp.Close, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox4.Image = im_gray.MorphologyEx(MorphOp.Dilate, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox5.Image = im_gray.MorphologyEx(MorphOp.Erode, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox6.Image = im_gray.MorphologyEx(MorphOp.HitMiss, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox7.Image = im_rgb.MorphologyEx(MorphOp.Open, kernel2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar()).MorphologyEx(MorphOp.Dilate, kernel5, new Point(-1, -1), 3, BorderType.Default, new MCvScalar());
            // imageBox8.Image = im_gray.MorphologyEx(MorphOp.Tophat, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            imageBox8.Image = im_tr_gr;
            box.Image = im_res;
            return im_res.Mat;
        }
        private double[][] findContour(Mat im, ImageBox box, int bin)
        {

            List<double[]> strk = new List<double[]>();
            double[,] stroka = new double[im.Width, 2];
            Image<Gray, Byte> im_gray = im.ToImage<Gray, Byte>();
            CvInvoke.Threshold(im_gray, im_gray, bin, 255, ThresholdType.Binary);
            var im_res = new Image<Gray, Byte>(im.Width, im.Height);

            for (int x = 0; x < im_gray.Width; x++)
            {
                int y = 0;
                int y_pos = 0;
                for (y = 0; y < im_gray.Height; y++)
                {
                    if (im_gray.Data[y, x, 0] > 245)
                    {
                        y_pos = y;
                    }
                }
                im_res.Data[y_pos, x, 0] = 255;
                stroka[x, 0] = x;
                stroka[x, 1] = y_pos;
                if (y_pos != 0)
                {
                    strk.Add(new double[] { x, y_pos });
                }

            }
            box.Image = im_res;
            return strk.ToArray();
        }
        double calcYbetween2Point(double x, PointF p1, PointF p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            var a = dy / dx;
            var b = p1.Y - a * p1.X;
            return a * x + b;
        }
        private double[][] connectPoints(double[][] inp)
        {
            double[][] ret = new double[inp.Length][];
            List<int> indixes = new List<int>();
            for (int i = 0; i < inp.Length; i++)
            {
                if (inp[i][2] == 1)
                {
                    indixes.Add(i);
                }
            }
            var indes = indixes.ToArray();
            int next_p = 1;
            for (int i = 0; i < inp.Length; i++)
            {
                double y = 0;

                if (i < inp.Length - 1 && indes[next_p] <= i && next_p < indes.Length - 1)
                {
                    next_p++;
                }
                if (inp[i][2] == 0)
                {
                    var y1 = (float)inp[indes[next_p - 1]][1];
                    var y2 = (float)inp[indes[next_p]][1];
                    var y_prev = calcYbetween2Point(i, new PointF(indes[next_p - 1], y1), new PointF(indes[next_p], y2));
                    var mx = Math.Max(y1, y2);
                    var mn = Math.Min(y1, y2);
                    if (y_prev > mx)
                    {
                        y = mx;
                    }
                    else if (y_prev < mn)
                    {
                        y = mn;
                    }
                    else
                    {
                        y = y_prev;
                    }
                }
                else
                {
                    y = inp[i][1];
                }
                ret[i] = new double[] { i, y, (int)inp[i][2] };
            }
            return ret;
        }
        private double[][] gauss2D(double[][] inp, double len)
        {
            double[][] ret = new double[inp.Length][];
            //List<double[]> strk = new List<double[]>();
            for (int i = 0; i < inp.Length; i++)
            {
                int real = 0;
                double sum = 0;
                for (int i_m = -(int)(len / 2); i_m < len / 2; i_m++)
                {
                    int ind = i + i_m;
                    if (ind >= inp.Length)
                    {
                        ind = inp.Length - 1;
                    }
                    if (ind < 0)
                    {
                        ind = 0;
                    }
                    sum += inp[ind][1];
                }
                if (inp[i].Length > 2)
                {
                    real = (int)inp[i][2];
                }
                ret[i] = new double[] { i, sum / len, real };
            }
            return ret;
        }
        private double[][] findContourZ_real(Mat im, ImageBox box, int bin, DirectionType directionType = DirectionType.Down)
        {
            List<double[]> strk = new List<double[]>();
            Image<Bgr, Byte> im_gray = im.ToImage<Bgr, Byte>();
            CvInvoke.Threshold(im_gray, im_gray, bin, 255, ThresholdType.Binary);
            if (directionType == DirectionType.Down)
            {
                for (int x = 0; x < im_gray.Width; x++)
                {
                    int y = 0;
                    int y_pos = 0;
                    int real = 1;
                    for (y = 0; y < im_gray.Height; y++)
                    {
                        if (im_gray.Data[y, x, 0] > 245)
                        {
                            y_pos = y;
                        }
                    }
                    if (y_pos == 0)
                    {
                        y_pos = im_gray.Height - 1;
                        real = 0;
                    }
                    strk.Add(new double[] { x, y_pos, real });
                }
                imageBox3.Image = im_gray;
                imageBox6.Image = doubleToMat(strk.ToArray(), im_gray.Size);
                var strk_1 = connectPoints(strk.ToArray());
                imageBox4.Image = doubleToMat(strk_1, im.Size);
                var gauss = gauss2D(strk_1, 8);
                imageBox5.Image = doubleToMat(gauss, im.Size);
                box.Image = doubleToMat(gauss, im.Size);
                //Console.WriteLine("STRK");
                return strk.ToArray();
            }
            else
            {
                for (int x = 0; x < im_gray.Width; x++)
                {
                    int y = 0;
                    int y_pos = 0;
                    int real = 1;
                    for (y = im_gray.Height - 1; y >= 0; y--)
                    {
                        if (im_gray.Data[y, x, 0] > 245)
                        {
                            y_pos = y;
                        }
                    }
                    if (y_pos == 0)
                    {
                        y_pos = im_gray.Height - 1;
                        real = 0;
                    }
                    strk.Add(new double[] { x, y_pos, real });
                }
                imageBox3.Image = im_gray;
                var strk_1 = connectPoints(strk.ToArray());
                imageBox4.Image = doubleToMat(strk_1, im.Size);
                var gauss = gauss2D(strk_1, 10);
                imageBox5.Image = doubleToMat(gauss, im.Size);
                //box.Image = (Mat)box.Image + doubleToMat(gauss, im.Size);
                return gauss;
            }



        }
        private double[][] findContourZ(Mat im, ImageBox box, int bin, DirectionType directionType = DirectionType.Down)
        {
            List<double[]> strk = new List<double[]>();
            Image<Bgr, Byte> im_gray = im.ToImage<Bgr, Byte>();
            CvInvoke.Threshold(im_gray, im_gray, bin, 255, ThresholdType.Binary);
            if (directionType == DirectionType.Down)
            {
                for (int x = 0; x < im_gray.Width; x++)
                {
                    int y = 0;
                    int y_pos = 0;
                    int real = 1;
                    for (y = 0; y < im_gray.Height; y++)
                    {
                        if (im_gray.Data[y, x, 0] > 245)
                        {
                            y_pos = y;
                        }
                    }
                    if (y_pos == 0)
                    {
                        y_pos = im_gray.Height - 1;
                        real = 0;
                    }
                    strk.Add(new double[] { x, y_pos, real });
                }
                imageBox3.Image = im_gray;
                imageBox6.Image = doubleToMat(strk.ToArray(), im_gray.Size);
                var strk_1 = connectPoints(strk.ToArray());
                imageBox4.Image = doubleToMat(strk_1, im.Size);
                var gauss = gauss2D(strk_1, 8);
                imageBox5.Image = doubleToMat(gauss, im.Size);
                box.Image = doubleToMat(gauss, im.Size);
                //Console.WriteLine("STRK");
                return gauss;
            }
            else
            {
                for (int x = 0; x < im_gray.Width; x++)
                {
                    int y = 0;
                    int y_pos = 0;
                    int real = 1;
                    for (y = im_gray.Height - 1; y >= 0; y--)
                    {
                        if (im_gray.Data[y, x, 0] > 245)
                        {
                            y_pos = y;
                        }
                    }
                    if (y_pos == 0)
                    {
                        y_pos = im_gray.Height - 1;
                        real = 0;
                    }
                    strk.Add(new double[] { x, y_pos, real });
                }
                imageBox3.Image = im_gray;
                var strk_1 = connectPoints(strk.ToArray());
                imageBox4.Image = doubleToMat(strk_1, im.Size);
                var gauss = gauss2D(strk_1, 10);
                imageBox5.Image = doubleToMat(gauss, im.Size);
                //box.Image = (Mat)box.Image + doubleToMat(gauss, im.Size);
                return gauss;
            }



        }
        private double[][] findContourZ_leg(Mat im, ImageBox box, int bin)
        {
            List<double[]> strk = new List<double[]>();
            Image<Bgr, Byte> im_gray = im.ToImage<Bgr, Byte>();
            CvInvoke.Threshold(im_gray, im_gray, bin, 255, ThresholdType.Binary);
            for (int x = 0; x < im_gray.Width; x++)
            {
                int y = 0;
                int y_pos = 0;
                int real = 1;
                for (y = im_gray.Height; y > 0; y--)
                {
                    if (im_gray.Data[y, x, 0] > 245)
                    {
                        y_pos = y;
                    }
                }
                if (y_pos == 0)
                {
                    y_pos = im_gray.Height - 1;
                    real = 0;
                }
                strk.Add(new double[] { x, y_pos, real });
            }
            imageBox3.Image = im_gray;
            var strk_1 = connectPoints(strk.ToArray());
            imageBox4.Image = doubleToMat(strk_1, im.Size);
            var gauss = gauss2D(strk_1, 10);
            imageBox6.Image = doubleToMat(gauss, im.Size);
            box.Image = doubleToMat(gauss, im.Size);
            return gauss;
        }
        #endregion
        #region scan
        void loadScan_leg(string path_pos_calib, string path_laser_calib, string path_scan, double FoV, double Side, SolveType type = SolveType.Simple, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var frames_pos = loadImages(path_pos_calib, FoV, Side, bin_pos);
            var frames_las = loadImages_simple(path_laser_calib);
            var frames_scan = loadImages_simple(path_scan);
            var zero_frame = findZeroFrame(frames_las);

            var fr = from f in frames_scan
                     orderby f.pos_rob.y
                     select f;
            frames_scan = fr.ToList();

            laserFlat = calibrLaser(frames_las, frames_pos[0], zero_frame, (int)red_c, type);

            var model = paintScanningModel(laserFlat, frames_scan, frames_pos[0], zero_frame, type);
            Console.WriteLine("loading done");
            if (offset_model != null)
            {
                Console.WriteLine("x = " + offset_model.x + "y = " + offset_model.y + "z = " + offset_model.z);
                offset_model.x = 0;
                offset_model.y = 0;
                offset_model.z = 0;
                GL1.addGLMesh(model, PrimitiveType.Triangles, (float)-offset_model.x, (float)-offset_model.y, (float)-offset_model.z, r, g, b);
            }
        }
        void loadScan(string path_pos_calib, string path_laser_calib, string path_scan, string path_basis, double FoV, double Side, SolveType type = SolveType.Simple, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var frames_pos = loadImages(path_pos_calib, FoV, Side, bin_pos, 15, true);
            var frames_las = loadImages_simple(path_laser_calib);
            var frames_scan = loadImages_simple(path_scan);
            var zero_frame = findZeroFrame(frames_las);
            var robToCam = loadImages_basis(path_basis, FoV, Side, bin_pos);

            var fr = from f in frames_scan
                     orderby f.pos_rob.y
                     select f;
            frames_scan = fr.ToList();

            //laserFlat = calibrLaser(frames_las, frames_pos[0], zero_frame, (int)red_c, type,robToCam,DirectionType.Down);


            // var model = paintScanningModel_pts(laserFlat, frames_scan, frames_pos[0], zero_frame, type, robToCam, DirectionType.Down);
            //addPointMesh(model, r, g, b);
            //Console.WriteLine("loading done");
            //Console.WriteLine("x = " + offset_model.x + "y = " + offset_model.y + "z = " + offset_model.z);
            //addGLMesh(model, PrimitiveType.Points, 0, 0, 0, r, g, b);

            laserFlat = calibrLaser(frames_las, frames_pos[0], zero_frame, (int)red_c, type, robToCam, DirectionType.Down);

            var model = paintScanningModel(laserFlat, frames_scan, frames_pos[0], zero_frame, type, robToCam, DirectionType.Down);
            Console.WriteLine("loading done");
            Console.WriteLine("x = " + offset_model.x + "y = " + offset_model.y + "z = " + offset_model.z);
            GL1.addGLMesh(model, PrimitiveType.Triangles, 0, 0, 0, 1, 0, 0);
        }
        Point3d_GL[] paintScanningModel_pts(List<Flat_4P> laserFlat, List<Frame> videoframes, Frame calib_frame, Frame zero_frame, SolveType type, double[,] matr = null, DirectionType directionType = DirectionType.Down)
        {
            List<Frame> frames = new List<Frame>();

            var points = new List<Point3d_GL>();
            var fr = from f in videoframes
                     orderby f.pos_rob.x
                     select f;
            var vfrs = fr.ToList();
            foreach (var f in vfrs)
            {
                var mat_im = new Mat();
                f.im.CopyTo(mat_im);
                var stroka = findContourZ_real(mat_im, imageBox3, (int)red_c, directionType);
                f.points = moveToCentr(doubleToPointF_real(stroka), f.size);

                frames.Add(f);
                Console.WriteLine(f);
            }
            float r = 0.1f;
            float g = 0.1f;
            float b = 0.1f;
            var mesh = new List<float>();
            flatInds = new int[frames.Count, frames[0].points.Length];
            flatLasInds = new int[laserFlat.Count];
            List<Point3d_GL> points3d = new List<Point3d_GL>();
            colr = 1;
            for (int i = 0; i < frames.Count; i++)
            {
                for (int j = 0; j < frames[i].points.Length; j++)
                {
                    var p = calcPoint(frames[i].points[j], calib_frame, frames[i], laserFlat, zero_frame, type, matr);
                    if (p != null)
                    {
                        points3d.Add(new Point3d_GL(p.x, p.y, p.z));
                    }
                }
            }
            Console.WriteLine(frames.Count + " LEN_FR");
            return points3d.ToArray();
        }
        float[] paintScanningModel(List<Flat_4P> laserFlat, List<Frame> videoframes, Frame calib_frame, Frame zero_frame, SolveType type, double[,] matr = null, DirectionType directionType = DirectionType.Down)
        {
            List<Frame> frames = new List<Frame>();

            var points = new List<Point3d_GL>();
            var fr = from f in videoframes
                     orderby f.pos_rob.x
                     select f;
            var vfrs = fr.ToList();
            foreach (var f in vfrs)
            {
                var mat_im = new Mat();
                f.im.CopyTo(mat_im);
                var stroka = findContourZ(mat_im, imageBox3, (int)red_c, directionType);
                f.points = moveToCentr(doubleToPointF(stroka), f.size);

                frames.Add(f);
                Console.WriteLine(f);
            }
            float r = 0.1f;
            float g = 0.1f;
            float b = 0.1f;
            double xmin = double.MaxValue;
            double xmax = double.MinValue;
            double ymin = double.MaxValue;
            double ymax = double.MinValue;
            double zmin = double.MaxValue;
            double zmax = double.MinValue;
            var mesh = new List<float>();
            flatInds = new int[frames.Count, frames[0].points.Length];
            flatLasInds = new int[laserFlat.Count];
            Point3d_GL[,] points3d = new Point3d_GL[frames.Count, frames[0].points.Length];
            colr = 1;
            for (int i = 0; i < frames.Count; i++)
            {
                for (int j = 0; j < frames[i].points.Length; j++)
                {
                    var p = calcPoint(frames[i].points[j], calib_frame, frames[i], laserFlat, zero_frame, type, matr);
                    if (p != null)
                    {
                        points3d[i, j] = new Point3d_GL(p.x, p.y, p.z);
                        flatInds[i, j] = colr;
                        //addPointMesh(new Point3d_GL[] { points3d[i, j] }, 1, 0, 0);
                        if (p.x < xmin)
                        {
                            xmin = p.x;
                        }
                        if (p.x > xmax)
                        {
                            xmax = p.x;
                        }
                        if (p.y < ymin)
                        {
                            ymin = p.y;
                        }
                        if (p.y > ymax)
                        {
                            ymax = p.y;
                        }
                        if (p.z < zmin)
                        {
                            zmin = p.z;
                        }
                        if (p.z > zmax)
                        {
                            zmax = p.z;
                        }
                    }
                }
            }
            #region colors
            /*
            for (int i = 0; i < frames.Count; i++)
            {
                for (int j = 0; j < frames[i].points.Length; j++)
                {
                    //Console.WriteLine(flatInds[i, j]);
                    if(flatInds[i,j]%6 == 0)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 1, 0, 0);
                    }
                    else if(flatInds[i, j] % 6 == 1)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 0, 1, 0);
                    }
                    else if (flatInds[i, j] % 6 == 2)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 0, 0, 1);
                    }
                    else if (flatInds[i, j] % 6 == 3)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 1, 1, 0);
                    }
                    else if (flatInds[i, j] % 6 == 4)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 0, 1, 1);
                    }
                    else if (flatInds[i, j] % 6 ==5)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 1, 0,1);
                    }
                }
            }

            for (int i = 0; i < laserFlat.Count; i++)
            {
                var ps = laserFlat[i].P;
                if (i % 6 == 0)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 1, 0, 0);
                }
                else if (i % 6 == 1)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 0, 1, 0);
                }
                else if (i % 6 == 2)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 0, 0, 1);
                }
                else if (i % 6 == 3)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 1, 1, 0);
                }
                else if (i % 6 == 4)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 0, 1, 1);
                }
                else if (i % 6 == 5)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 1, 0, 1);
                }
                //addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, r, g, 0.1f);
            }
            */
            #endregion
            double offx = xmin + (xmax - xmin) / 2;
            double offy = ymin + (ymax - ymin) / 2;
            double offz = zmin + (zmax - zmin) / 2;
            Console.WriteLine(
                "xmin " + xmin + " " +
                "xmax " + xmax + " " +
                "ymin " + ymin + " " +
                "ymax " + ymax + " " +
                "zmin " + zmin + " " +
                "zmax " + zmax + " ");
            offset_model = new Point3d_GL(offx, offy, offz);
            Console.WriteLine(frames.Count + " LEN_FR");
            for (int i = 0; i < frames.Count - 1; i++)
            {
                //Console.WriteLine(frames[0].points.Length - 2);
                for (int j = 0; j < frames[0].points.Length - 1; j++)
                {
                    //Console.WriteLine(i + " " + j);
                    var p1 = points3d[i, j];
                    var p2 = points3d[i + 1, j];
                    var p3 = points3d[i, j + 1];
                    if (p1 != null & p2 != null & p3 != null)
                    {
                        mesh.Add((float)p1.x); mesh.Add((float)p1.y); mesh.Add((float)p1.z);
                        mesh.Add((float)p2.x); mesh.Add((float)p2.y); mesh.Add((float)p2.z);
                        mesh.Add((float)p3.x); mesh.Add((float)p3.y); mesh.Add((float)p3.z);
                    }

                    p1 = points3d[i + 1, j];
                    p2 = points3d[i + 1, j + 1];
                    p3 = points3d[i, j + 1];
                    if (p1 != null & p2 != null & p3 != null)
                    {
                        mesh.Add((float)p1.x); mesh.Add((float)p1.y); mesh.Add((float)p1.z);
                        mesh.Add((float)p2.x); mesh.Add((float)p2.y); mesh.Add((float)p2.z);
                        mesh.Add((float)p3.x); mesh.Add((float)p3.y); mesh.Add((float)p3.z);
                    }

                }
            }
            return mesh.ToArray();
        }
        Frame findZeroFrame(List<Frame> frames)
        {
            var fr = from f in frames
                     orderby f.pos_rob.z
                     select f;
            var vfrs = fr.ToList();
            return vfrs[vfrs.Count - 1];
        }
        List<Flat_4P> calibrLaser(List<Frame> videoframes, Frame calib_frame, Frame zero_frame, int bin, SolveType type, double[,] matr = null, DirectionType directionType = DirectionType.Down)
        {
            List<Frame> frames = new List<Frame>();
            List<Flat_4P> mesh = new List<Flat_4P>();
            Console.WriteLine("a----");
            var points = new List<Point3d_GL>();
            var fr = from f in videoframes
                     orderby f.pos_rob.z
                     select f;
            Console.WriteLine("b----");
            var vfrs = fr.ToList();
            foreach (var v in vfrs)
            {
                var f = v;
                var mat_im = new Mat();
                f.im.CopyTo(mat_im);
                var stroka = findContourZ(mat_im, imageBox5, bin, directionType);
                f.points = moveToCentr(regressionPoints(mat_im.Size, stroka, 80, 2), v.size);

                Console.WriteLine(f.points.Length);
                frames.Add(f);
                Console.WriteLine(f);
            }

            float r = 0.1f;
            float g = 0.8f;
            float b = 0.1f;

            var mesh_p = computeMeshFromFrames(frames.ToArray(), calib_frame, zero_frame, matr);
            var ps_tr = translPoints(mesh_p);
            var ps_ext = extendPoints(ps_tr);
            var ps_all = translPoints(ps_ext);

            if (type == SolveType.Complex)
            {
                for (int i = 1; i < ps_all.Length; i++)
                {
                    for (int j = 0; j < ps_all[0].Length - 1; j++)
                    {
                        var ps = new Point3d_GL[] { ps_all[i][j], ps_all[i][j + 1], ps_all[i - 1][j], ps_all[i - 1][j + 1] };
                        mesh.Add(new Flat_4P(ps));
                        //Console.WriteLine("e2----");
                        //addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, r, g, 0.1f);
                        //addPointMesh(new Point3d_GL[] { frames[i].pos_rob + del, ps[0], frames[i - 1].pos_rob + del, ps[2] }, r,g,0.5f);
                        //g += 0.1f;
                        //Console.WriteLine("e3----");
                    }
                    //r += 0.2f;
                }
            }
            else
            {
                var ps = new Point3d_GL[4];
                var del = calib_frame.pos_cam - calib_frame.pos_rob;
                ps[0] = calcPoint(frames[0].points[0], calib_frame, frames[0], zero_frame, matr);
                ps[1] = calcPoint(frames[0].points[frames[0].points.Length - 1], calib_frame, frames[0], zero_frame, matr);
                ps[2] = calcPoint(frames[frames.Count - 1].points[0], calib_frame, frames[frames.Count - 1], zero_frame, matr);
                ps[3] = calcPoint(frames[frames.Count - 1].points[frames[frames.Count - 1].points.Length - 1], calib_frame, frames[frames.Count - 1], zero_frame, matr);
                //addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, r, g, 0.5f);
                // addFrame(new Point3d_GL[] { frames[i].pos_rob + del, ps[0], frames[i - 1].pos_rob + del, ps[2] }, r, g, 0.5f);
                mesh.Add(new Flat_4P(ps));
            }
            return mesh;
        }

        Point3d_GL[][] computeMeshFromFrames(Frame[] frames, Frame calib_frame, Frame zero_frame, double[,] matr)
        {
            var ret = new Point3d_GL[frames.Length][];

            for (int i = 0; i < frames.Length; i++)
            {
                ret[i] = new Point3d_GL[frames[i].points.Length];
                Console.WriteLine(frames[i].points.Length);
                for (int j = 0; j < frames[i].points.Length; j++)
                {
                    ret[i][j] = calcPoint(frames[i].points[j], calib_frame, frames[i], zero_frame, matr);
                }
            }
            Console.WriteLine("frames[i].points.Length");
            return ret;
        }

        double[,] translToRot(double[,] transMatr)
        {
            var rotMatr = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    rotMatr[i, j] = transMatr[i, j];
                }
            }
            return rotMatr;
        }
        Point3d_GL calcPoint(PointF point, Frame cframe, Frame frame, List<Flat_4P> laserFlat, Frame zero_frame, SolveType type, double[,] matr = null)
        {
            var ox = cframe.camera.oX;
            var oy = cframe.camera.oY;
            var oz = cframe.camera.oZ;
            var f = cframe.camera.f;
            float k = (float)cframe.im.Width / (float)frame.im.Width;
            var x = point.X * k;
            var y = point.Y * k;
            var P1_cam = new Vector3d_GL(x, y, f);
            var P1_x = ox * P1_cam.x;
            var P1_y = oy * P1_cam.y;
            var P1_z = oz * P1_cam.z;
            var vec = P1_x + P1_y + P1_z;
            //addFrame_Cam(cframe.camera);
            vec.normalize();
            Point3d_GL zf = new Point3d_GL(0, 0, 0);
            if (matr != null)
            {
                var matr_3 = new double[3, 3];
                zf = translToRot(matr) * (zero_frame.pos_rob - frame.pos_rob);
            }
            else
            {
                zf = zero_frame.pos_rob - frame.pos_rob;
            }

            var moveToReal = zf;
            var zeroPos = matr * zero_frame.pos_rob;
            var line = new Line3d_GL(vec, zeroPos);
            int ids = 0;
            if (type == SolveType.Complex)
            {
                Point3d_GL ret = null;
                foreach (var flat in laserFlat)
                {

                    ret = flat.crossLine(line);
                    if (ret != null)
                    {

                        colr = ids;
                        //Console.WriteLine("colr: : " +colr);
                        var ps = flat.P;
                        return ret - moveToReal;

                    }
                    ids++;
                }
                Console.WriteLine("null");
                var flat_max = laserFlat[0].F[0];
                ret = line.calcCrossFlat(flat_max);
                if (ret != null)
                {
                    return ret - moveToReal;
                }
            }
            else
            {
                var flat = laserFlat[0].F[0];
                var ret = line.calcCrossFlat(flat);
                if (ret != null)
                {
                    return ret - moveToReal;

                }
            }

            return null;
        }
        Point3d_GL calcPoint(PointF point, Frame cframe, Frame frame, Frame frame_zero, double[,] matr)
        {
            var ox = cframe.camera.oX;
            var oy = cframe.camera.oY;
            var oz = cframe.camera.oZ;
            var f = cframe.camera.f;
            float k = (float)cframe.im.Width / (float)frame.im.Width;
            var x = point.X * k;
            var y = point.Y * k;
            var P1_cam = new Vector3d_GL(x, y, f);
            var P1_x = ox * P1_cam.x;
            var P1_y = oy * P1_cam.y;// * (-1);//*-1
            var P1_z = oz * P1_cam.z;
            var vec = P1_x + P1_y + P1_z;
            vec.normalize();

            var z = frame_zero.pos_rob.z - frame.pos_rob.z;
            //z = 0;
            if (matr == null)
                Console.WriteLine("matr NULL");
            var line = new Line3d_GL(vec, matr * frame_zero.pos_rob);
            //addPointMesh(new Point3d_GL[] { matr * frame_zero.pos_rob + del }, 0.8f, 0.8f, 0.1f);
            // z = 0;
            var flat = new Flat3d_GL(new Point3d_GL(10, 10, z),
                new Point3d_GL(0, 10, z),
                new Point3d_GL(10, 0, z));
            //Console.WriteLine("las z = "+z);
            var cr = line.calcCrossFlat(flat);
            if (cr != null)
            {
                return cr;
            }
            //Console.WriteLine(vec.ToString());
            return null;
        }
        Point3d_GL calcPoint_leg(PointF point, Frame cframe, Frame frame, List<Flat_4P> laserFlat, Frame zero_frame, SolveType type, double[,] matr = null)
        {
            var ox = cframe.camera.oX;
            var oy = cframe.camera.oY;
            var oz = cframe.camera.oZ;
            var f = cframe.camera.f;
            float k = (float)cframe.im.Width / (float)frame.im.Width;
            var x = point.X * k;
            var y = point.Y * k;
            var P1_cam = new Vector3d_GL(x, y, f);
            var P1_x = ox * P1_cam.x;
            var P1_y = oy * P1_cam.y;//*-1
            var P1_z = oz * P1_cam.z;
            var vec = P1_x + P1_y + P1_z;
            vec.normalize();
            var del = cframe.pos_cam - cframe.pos_rob;//dist flange->cam
            Point3d_GL zf = new Point3d_GL(0, 0, 0);
            if (matr != null)
            {
                zf = matr * zero_frame.pos_rob - matr * frame.pos_rob;
            }
            else
            {
                zf = zero_frame.pos_rob - frame.pos_rob;
            }

            double alpha = 0.3;

            var x1 = zf.x;// * Math.Cos(alpha) + zf.x * Math.Sin(alpha);
            var y1 = zf.y;// * Math.Cos(alpha) + zf.y * Math.Sin(alpha);
            var z1 = zf.z;
            var moveToReal = new Point3d_GL(x1, y1, z1);
            //var moveToReal = new Point3d_GL(zf.x, zf.y, zf.z);
            //var moveToReal = new Point3d_GL(0,0, 0);//!!!!!!!!!!
            //var lam_zero = zero_frame.pos_rob;
            // lam_zero.z = frame.pos_rob.z;
            //var zeroPos = lam_zero + del;

            var zeroPos = zero_frame.pos_rob + del;


            //addFrame(real_pos, real_pos + ox * 5, real_pos + oy * 5, real_pos + oz * 5);

            var line = new Line3d_GL(vec, zeroPos);
            int ids = 0;
            List<Point3d_GL> ret_m = new List<Point3d_GL>();
            // Console.WriteLine("ids");
            if (type == SolveType.Complex)
            {
                Point3d_GL ret = null;
                foreach (var flat in laserFlat)
                {

                    ret = flat.crossLine(line);
                    if (ret != null)
                    {
                        var ps = flat.P;
                        ret_m.Add(ret);
                        //addLineMesh(new Point3d_GL[] { ret, zeroPos }, 0.1f, 0.8f, 0.8f);
                        GL1.addLineMesh(new Point3d_GL[] { ret - moveToReal, zeroPos }, 0.8f, 0.1f, 0.8f);
                        //addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 0.8f, 0.1f, 0.1f);
                        //Console.WriteLine(ids);                        
                    }
                    ids++;
                }
                //addPointMesh(ret_m.ToArray(), 0.1f, 0.8f, 0.1f);
                GL1.addPointMesh(new Point3d_GL[] { new Point3d_GL(0, 0, 0), moveToReal }, 0.8f, 0.8f, 0.1f);
                if (ret != null)
                {
                    return ret - moveToReal;
                }
                else
                {
                    //Console.WriteLine("NULL");
                }
                //return ret - moveToReal;
                var flat_max = laserFlat[0].F[0];
                ret = line.calcCrossFlat(flat_max);
                if (ret != null)
                {
                    //addLineMesh(new Point3d_GL[] { ret - moveToReal, zeroPos }, 0.8f, 0.8f, 0.1f);
                    return ret - moveToReal;
                    //return ret - moveToReal;
                }
            }
            else
            {
                var flat = laserFlat[0].F[0];
                var ret = line.calcCrossFlat(flat);
                if (ret == null)
                {
                    //Console.WriteLine("NULL");
                }

                if (ret != null)
                {
                    return ret - moveToReal;

                }
            }

            return null;
        }
        Point3d_GL calcPoint_leg(PointF point, Frame cframe, Frame frame, Frame frame_zero)
        {
            var ox = cframe.camera.oX;
            var oy = cframe.camera.oY;
            var oz = cframe.camera.oZ;
            var f = cframe.camera.f;
            float k = (float)cframe.im.Width / (float)frame.im.Width;
            var x = point.X * k;
            var y = point.Y * k;
            var P1_cam = new Vector3d_GL(x, y, f);
            var P1_x = ox * P1_cam.x;
            var P1_y = oy * P1_cam.y;// * (-1);//*-1
            var P1_z = oz * P1_cam.z;
            var vec = P1_x + P1_y + P1_z;
            vec.normalize();
            var del = cframe.pos_cam - cframe.pos_rob;
            var flat_ = frame.pos_rob + del;
            //addFrame(flat_, flat_ + ox * 5, flat_ + oy * 5, flat_ + oz * 5);

            //var line = new Line3d_GL(vec, flat_);

            var z = frame_zero.pos_rob.z - frame.pos_rob.z;
            //z = 0;
            var line = new Line3d_GL(vec, frame_zero.pos_rob + del);
            // z = 0;
            var flat = new Flat3d_GL(new Point3d_GL(10, 10, z),
                new Point3d_GL(0, 10, z),
                new Point3d_GL(10, 0, z));
            //Console.WriteLine("las z = "+z);
            var cr = line.calcCrossFlat(flat);
            if (cr != null)
            {
                return cr;
            }
            //Console.WriteLine(vec.ToString());
            return null;
        }
        private double[,] findContourRGB(Mat im, ImageBox box)
        {
            int color = 2;//012 - bgr
            double[,] stroka = new double[im.Width, 2];
            Image<Bgr, Byte> im_bgr = im.ToImage<Bgr, Byte>();
            var im_res = new Image<Bgr, Byte>(im.Width, im.Height);
            for (int x = 0; x < im_bgr.Width; x++)
            {
                int y = 0;
                int y_pos = 0;
                for (y = 0; y < im_bgr.Height; y++)
                {
                    if (im_bgr.Data[y, x, color] > 250)
                    {
                        y_pos = y;
                    }
                }
                im_res.Data[y_pos, x, color] = 255;
                stroka[x, 0] = x;
                stroka[x, 1] = y_pos;
            }
            box.Image = im_res;
            return stroka;
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
        #region regress
        Point3d_GL[][] extendPoints(Point3d_GL[][] points)
        {
            Console.WriteLine(points.Length);

            for (int i = 0; i < points.Length; i++)
            {
                Console.WriteLine(points[i].Length);
                points[i] = regress3DLine(points[i], 2);

            }
            return points;
        }
        Point3d_GL[][] translPoints(Point3d_GL[][] points)
        {
            Console.WriteLine("------------");
            for (int i = 0; i < points.Length; i++)
            {
                Console.WriteLine(points[i].Length);
            }
            Point3d_GL[][] points_tr = new Point3d_GL[points[0].Length][];

            for (int i = 0; i < points[0].Length; i++)
            {
                points_tr[i] = new Point3d_GL[points.Length];
                //Console.WriteLine(i);
                for (int j = 0; j < points.Length; j++)
                {
                    points_tr[i][j] = points[j][i];
                    //Console.WriteLine(j);
                }
            }
            return points_tr;
        }
        Point3d_GL[] regress3DLine(Point3d_GL[] points, int p_len, int grad = 2)
        {
            List<Point3d_GL> ret = new List<Point3d_GL>();
            var xval = new double[points.Length][];
            var yval = new double[points.Length][];
            double maxZ = double.MinValue;
            double minZ = double.MaxValue;
            for (int i = 0; i < points.Length; i++)
            {
                xval[i] = new double[] { points[i].z, points[i].x };
                yval[i] = new double[] { points[i].z, points[i].y };
                if (points[i].z > maxZ)
                {
                    maxZ = points[i].z;
                }
                if (points[i].z < minZ)
                {
                    minZ = points[i].z;
                }
            }
            var xkoef = regression(xval, grad);
            var ykoef = regression(yval, grad);
            var dz = (maxZ - minZ) / (double)points.Length;

            for (double z = -p_len * dz; z < (points.Length + 0.9 * p_len) * dz; z += dz)
            {
                ret.Add(new Point3d_GL(calcPolynSolv(xkoef, z), calcPolynSolv(ykoef, z), z));
            }
            return ret.ToArray();
        }
        PointF[] regressionPoints(Size size, double[][] values, int delim = 80, int grad = 2, int board = 10)
        {
            var points = new List<PointF>();
            var koef = regression(values, grad);
            var stepx = size.Width / delim;

            float x = 0;
            for (int i = -board; i <= delim + board; i++)
            {
                double y = calcPolynSolv(koef, x);
                //if (y < size.Height - 1)
                // {
                points.Add(new PointF((float)x, (float)y));
                //}
                if (i == delim - 1)
                {
                    stepx--;
                }
                x += stepx;
            }
            return points.ToArray();
        }
        Mat paintRegression(Mat mat, double[][] values)
        {
            var koef = regression(values, 4);
            var im = mat.ToImage<Bgr, Byte>();
            for (int x = 0; x < im.Width; x++)
            {
                int y = (int)calcPolynSolv(koef, x);
                if (y >= im.Height)
                {
                    y = im.Height - 1;
                }
                if (y < 0)
                {
                    y = 0;
                }
                //Console.WriteLine(y + " " + im.Height);
                im.Data[y, x, 1] = 255;
            }
            return im.Mat;
        }
        double calcPolynSolv(double[] k, double x)
        {
            double solv = 0;

            for (int i = 0; i < k.Length; i++)
            {
                solv += k[i] * Math.Pow(x, i);
            }
            return solv;
        }
        double[] regression(double[][] data, int degree)
        {
            double[] inputs = data.GetColumn(0);  // X
            double[] outputs = data.GetColumn(1); // Y
            var ls = new PolynomialLeastSquares()
            {
                Degree = degree
            };
            PolynomialRegression poly = ls.Learn(inputs, outputs);
            double[] weights = poly.Weights;
            double intercept = poly.Intercept;
            var koef = new List<double>();
            koef.AddRange(weights);
            koef.Add(intercept);
            koef.Reverse();
            return koef.ToArray();
        }
        #endregion

        #region loadFrames
        public double[,] loadImages_basis(string path, double FoV, double Side, int bin = 60, int frame_len = 15, bool visible = false)
        {
            Console.WriteLine(path);
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage(file, FoV, Side, bin, frame_len, visible);
                if (frame != null)
                {
                    frames.Add(frame);
                    comboImages.Items.Add(frame);
                }
            }
            Console.WriteLine("frame_co n " + frames.Count);
            if (frames.Count > 3)
            {
                var basis1 = new Point3d_GL[4];
                var basis2 = new Point3d_GL[4];

                for (int i = 0; i < 4; i++)
                {
                    basis1[i] = frames[i].pos_rob;
                    basis2[i] = frames[i].pos_cam;
                    //addLineMesh(new Point3d_GL[] { basis1[i], basis2[i] });
                }
                var transf = calcTransformMatr(basis1, basis2);
                print(transf);
                return transf;
            }
            return null;
        }

        public Frame loadImage(string filepath, double FoV, double Side, int bin, int frame_len = 15, bool visible = false)//11.02,30.94  //41.9874, 112.7 FOV  
        {
            string name = Path.GetFileName(filepath);
            name = name.Trim();
            var coords = name.Split(new char[] { ' ' });

            //Console.WriteLine("NAME = " +name);
            if (coords[0].Length == 0)
            {
                var lb = coords.ToList();
                coords = lb.GetRange(1, coords.Length - 1).ToArray();
            }
            for (int i = 0; i < coords.Length; i++)
            {
                if (!coords[i].Contains("."))
                {
                    coords[i] += ".0";
                }
            }
            Point3d_GL name_pos = null;
            Point3d_GL name_pos_or = null;
            if (coords.Length > 2)
            {
                name_pos = new Point3d_GL(Convert.ToDouble(coords[0]),
                                            Convert.ToDouble(coords[1]),
                                            Convert.ToDouble(coords[2]));
            }
            if (coords.Length > 5)
            {
                name_pos_or = new Point3d_GL(Convert.ToDouble(coords[3]),
                                            Convert.ToDouble(coords[4]),
                                            Convert.ToDouble(coords[5]));
            }

            var im = new Mat(filepath);

            int koef = k;
            CvInvoke.Resize(im, im, new Size(im.Width * koef, im.Height * koef));


            var ps = finPointFsFromIm(im, bin, imageBox6);

            if (ps != null)
            {
                var cam = calcPos(ps, im.Size, FoV, Side);     //41.727, 68.89649           //8mm: 11.02; 7mm: 41.8; 5.5mm : 30.02
                var pos = cam.pos;
                var vec = new Vector3d_GL[] { cam.oX, cam.oY, cam.oZ };
                var err = cam.err_pos;
                // Console.WriteLine(err);
                Console.WriteLine("----------------");
                Console.WriteLine(name);
                Console.WriteLine("pos_rob " + pos.x + " " + pos.y + " " + pos.z);
                Console.WriteLine("err " + (name_pos - pos).magnitude());
                if (name_pos_or != null)
                {
                    var matr_rob = AbcToMatrix(toDegrees((float)name_pos_or.x), toDegrees((float)name_pos_or.y), toDegrees((float)name_pos_or.z));
                    matr_rob[3, 0] = (float)name_pos.x;
                    matr_rob[3, 1] = (float)name_pos.y;
                    matr_rob[3, 2] = (float)name_pos.z;

                    var inv_matr_rob = matr_rob.Inverse;

                    var matr_cam = matrFromCam(cam);
                    var matr_RobToCam = inv_matr_rob * matr_cam;
                    print(matr_RobToCam);

                }
                var f = new Frame(im, pos, name_pos, name, ps, name_pos_or);

                if (visible)
                {
                    GL1.addFrame_Cam(cam, frame_len);
                }
                f.camera = cam;
                return f;
            }
            return null;
        }

        public Frame loadImage_simple(string filepath)
        {
            string name = Path.GetFileName(filepath);
            //Console.WriteLine(name);
            var name_t = name.Trim();
            var file_ext = name_t.Split(new char[] { '.' });
            var coords = name_t.Split(new char[] { ' ' });
            // Console.WriteLine(file_ext[file_ext.Length - 1]);

            if ((file_ext[file_ext.Length - 1] == "jpg" || file_ext[file_ext.Length - 1] == "png") && file_ext[0] != "calibresult")
            {
                //Console.WriteLine(coords.Length);
                if (coords.Length > 2)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (coords[i].Contains(".") != true)
                        {
                            coords[i] += ".0";
                        }
                    }
                    var name_pos = new Point3d_GL(Convert.ToDouble(coords[0]),
                                                    Convert.ToDouble(coords[1]),
                                                    Convert.ToDouble(coords[2]));
                    //Console.WriteLine(name_pos.x + " " + name_pos.y + " " + name_pos.z + " ");
                    var im = new Mat(filepath);
                    return new Frame(im, name_pos, name_pos, name);
                }
                else if (coords.Length > 0)
                {
                    var name_pos = new Point3d_GL(0,
                                                    0,
                                                    0);
                    var im = new Mat(filepath);
                    return new Frame(im, name_pos, name_pos, name);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
        public Frame loadImage_calib(string filepath)
        {
            string name = Path.GetFileName(filepath);
            var coords = name.Split(new char[] { ' ' });
            Console.WriteLine(coords[0].Length + " " + coords.Length);
            if (coords[0].Length == 0)
            {
                var lamd_ar = new string[coords.Length];
                coords.CopyTo(lamd_ar, 0);
                lamd_ar.CopyTo(coords, 0);
            }
            for (int i = 0; i < 4; i++)
            {
                if (coords[i].Contains(".") != true)
                {
                    coords[i] += ".0";
                }
            }
            var name_pos = new Point3d_GL(Convert.ToDouble(coords[0]),
                                            Convert.ToDouble(coords[1]),
                                            Convert.ToDouble(coords[2])
                                            );
            var mk = 10 * Convert.ToDouble(coords[3]);
            var im = new Mat(filepath);
            var ret = new Frame(im, name_pos, name_pos, name);
            ret.size_mark = mk;
            return ret;
        }

        public Frame loadImage_test(string filepath)
        {
            string name = Path.GetFileName(filepath);
            var im = new Mat(filepath);
            var fr = new Frame(im, name);
            fr.dateTime = File.GetCreationTime(filepath);
            return fr;
        }

        public Frame loadImage_stereoCV(string filepath1, string filepath2)
        {
            string name1 = Path.GetFileName(filepath1);
            var im1 = new Mat(filepath1);
            string name2 = Path.GetFileName(filepath2);
            var im2 = new Mat(filepath2);
            Console.WriteLine(name1);
            Console.WriteLine(name2);
            Console.WriteLine("------------");
            var fr = new Frame(im1, im2, name1);
            fr.dateTime = File.GetCreationTime(filepath1);
            return fr;
        }
        string[] sortByDate(string[] files)
        {
            var sortFiles =  from f in files
                             orderby File.GetCreationTime(f)
                             select f;
            return sortFiles.ToArray();
        }
        public List<Frame> loadImages_stereoCV(string path1, string path2)
        {
            Console.WriteLine(path1);
            var files1 = sortByDate(Directory.GetFiles(path1));
            var files2 = sortByDate(Directory.GetFiles(path2));
            List<Frame> frames = new List<Frame>();
            for (int i = 0; i < files1.Length; i++)
            {
                var frame = loadImage_stereoCV(files1[i], files2[i]);
                if (frame != null)
                {
                    frames.Add(frame);
                    comboImages.Items.Add(frame);
                    //comboImages.Items.Add(frame.im_sec);
                }
            }
            if (frames.Count != 0)
            {
                return frames;
            }
            return null;
        }


        public List<Frame> loadImages(string path, double FoV, double Side, int bin = 60, int frame_len = 15, bool visible = false)
        {
            Console.WriteLine(path);
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage(file, FoV, Side, bin, frame_len, visible);
                if (frame != null)
                {
                    frames.Add(frame);
                    comboImages.Items.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames;
            }
            return null;
        }



         public List<Frame> loadImages_stereo(string path, double FoV, double Side, int bin = 40, bool visible = false)
        {
            Console.WriteLine(path);
            var files1 = Directory.GetFiles(@"cam1\" + path);
            var files2 = Directory.GetFiles(@"cam2\" + path);
            List<Frame> frames = new List<Frame>();

            if (files1.Length != files2.Length)
            {
                return null;
            }
            int n = files1.Length;
            for (int i = 0; i < n; i++)
            {
                if (Path.GetFileName(files1[i]) == Path.GetFileName(files2[i]))
                {
                    Console.WriteLine(Path.GetFileName(files2[i]));
                    var frame1 = loadImage(files1[i], FoV, Side, bin, 10, visible);
                    if (frame1 != null)
                    {
                        frames.Add(frame1);

                    }
                    var frame2 = loadImage(files2[i], FoV, Side, bin, 20, visible);
                    if (frame2 != null)
                    {
                        frames.Add(frame2);
                        if (frame1 != null)
                        {
                            var b = frame1.camera.pos - frame2.camera.pos;
                            Console.WriteLine("B = " + b.magnitude());
                            Console.WriteLine("___________________________________");
                        }
                    }


                }
            }
            if (frames.Count != 0)
            {
                foreach (var fr in frames)
                {
                    comboImages.Items.Add(fr);
                }
                return frames;
            }
            return null;
        }
        public List<Frame> loadImages_simple(string path)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_simple(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                foreach (var fr in frames)
                {
                    comboImages.Items.Add(fr);
                }
                return frames;
            }
            return null;
        }
       public List<Frame> loadImages_test(string path)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_test(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                foreach (var fr in frames)
                {
                    comboImages.Items.Add(fr);
                }
                return frames;
            }
            return null;
        }
        public List<Frame> loadImages_calib(string path)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_calib(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                foreach (var fr in frames)
                {
                    comboImages.Items.Add(fr);
                }
                return frames;
            }
            return null;
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
            GL1.addMonitor(new Rectangle(w / 2, h / 2, w / 2,h / 2), 2);
            GL1.addMonitor(new Rectangle(0, h/2, w / 2, h / 2), 3);
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
        private void glControl1_Render(object sender, GlControlEventArgs e)
        {

            GL1.glControl_Render(sender, e);
            //GL1.printDebug(debugBox);
            if (GL1.rendercout == 0)
            {
                SaveMonitor(GL1);
            }
            // imBox_mark1.
            //imBox_mark1.Image = remapDistImOpenCvCentr(GL1.matFromMonitor(0), cameraDistortionCoeffs_dist);
            //imBox_mark2.Image = remapDistImOpenCvCentr(GL1.matFromMonitor(1), cameraDistortionCoeffs_dist);
            // var mat1 = GL1.matFromMonitor(0);
            // var mat2 = GL1.matFromMonitor(1);
            //drawDescriptors(ref mat1);
            //imBox_mark1.Image = drawDescriptorsMatch(ref mat1, ref mat2);
            imBox_mark2.Image = calcSubpixelPrec( GL1.matFromMonitor(0), new Size(6, 7));
            //imBox_disparity.Image =  stereocam.epipolarTest(GL1.matFromMonitor(1), GL1.matFromMonitor(0));
            // imBox_mark1.Image =  drawChessboard((Mat)imBox_mark1.Image, new Size(6, 7));
            //  imBox_mark2.Image =  drawChessboard((Mat)imBox_mark2.Image, new Size(6, 7));


        }
        private void Form1_mousewheel(object sender, MouseEventArgs e)
        {
            GL1.Form1_mousewheel(sender, e);
        }


        #endregion
        #region buttons
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
                    stereocam.solver_param.numDisparities = 16*val;
                    break;
                case 3://blockSize
                    stereocam.solver_param.blockSize= val;
                    break;
                case 4://p1
                    stereocam.solver_param.p1 = bs*bs* val;
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
                    stereocam.solver_param.speckleWindowSize = 20*val;
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
            trackBar1.Value = stereocam.solver_param.minDisparity ;
            trackBar2.Value = stereocam.solver_param.numDisparities/ 16 ;
            trackBar3.Value = stereocam.solver_param.blockSize;
            trackBar4.Value = stereocam.solver_param.p1/( bs * bs) ;
            trackBar5.Value = stereocam.solver_param.p2/(bs * bs );
            trackBar6.Value = stereocam.solver_param.disp12MaxDiff;
            trackBar7.Value = stereocam.solver_param.preFilterCap;
            trackBar8.Value = stereocam.solver_param.uniquenessRatio;
            trackBar9.Value = stereocam.solver_param.speckleWindowSize/20;
            trackBar10.Value = stereocam.solver_param.speckleRange;        
        }

            private void but_imGen_Click(object sender, EventArgs e)
        {
            generateImagesFromAnotherFolderStereo(new string[] { @"virtual_stereo\test1\monitor_0", @"virtual_stereo\test1\monitor_1" });
            startGen = 1;
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
                saveImage(imageBox1, imageBox2, res + ".png", box_photoFolder.Text);
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

                                        saveImage(imageBoxes[i], folders[i], name_ph + ".png");
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
                finPointFsFromIm(fr.im, bin_pos, imageBox1);
            }
            else if (fr.type == FrameType.Las)
            {
                findContourZ(fr.im, imageBox1, (int)red_c, DirectionType.Down);
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
                imBox_debug1.Image = drawChessboard(fr.im, new Size(6, 7));
                imBox_debug2.Image = drawChessboard(fr.im_sec, new Size(6, 7));
                imageBox1.Image = fr.im_sec;
            }
            else if (fr.type == FrameType.MarkBoard)
            {
                imBox_debug1.Image = drawChessboard(fr.im, new Size(6, 7));
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
        public float toRad(float degrees)
        {
            return degrees * PI / 180;
        }

        public float toDegrees(float rad)
        {
            return rad * 180 / PI;
        }

        private void butSave_Click(object sender, EventArgs e)
        {
            string name = " " + nameX.Text + " " + nameY.Text + " " + nameZ.Text + " " + boxN.Text + " .png";
            saveImage(imageBox1, imageBox2, name, "single");
            Console.WriteLine(name);

        }

        private void butSaveSing_Click(object sender, EventArgs e)
        {
            string name = " " + nameX.Text + " " + nameY.Text + " " + nameZ.Text + " " + boxN.Text + " .png";
            saveImage(imageBox1, "saved", name);
            Console.WriteLine(name);
        }
        private double calc_p_len(Point p1, Point p2)
        {
            var p = new Point(p1.X - p2.X, p1.Y - p2.Y);
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        private double calc_p_len(PointF p1, PointF p2)
        {
            var p = new PointF(p1.X - p2.X, p1.Y - p2.Y);
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
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
            imageBox_cameraDist.Image = remapDistImOpenCvCentr(frame.im, cameraDistortionCoeffs_dist);


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
            imageBox_cameraDist.Image = remapUnDistIm(frame.im, cameraMatrix, cameraDistortionCoeffs);
        }
        #endregion
        
        #region CalcMark
        PointF[] finPointFsFromIm(Mat im, int bin, ImageBox box)
        {
            var receivedImage = new Mat();
            var orig = new Mat();
            im.CopyTo(receivedImage);
            im.CopyTo(orig);

            var im_cont = revealContourQ(receivedImage, bin, imageBox4);
            Image<Gray, byte> image = im_cont.ToImage<Gray, Byte>();//.ThresholdBinary(new Gray(bin), new Gray(255));

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint contours1 = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            //  imageBox5.Image = im_cont;
            CvInvoke.FindContours(image, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            //imageBox6.Image = image;
            //Console.WriteLine("cont all= " + contours.Size);
            if (contours.Size != 0)
            {
                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    var perim = CvInvoke.ArcLength(contours[i], true);
                    var d = area / (perim * perim);
                    /*Console.WriteLine("area " + area);
                    Console.WriteLine("perim " + perim);
                    Console.WriteLine("d " + d);*/
                    if (area < maxArea && area > minArea && d < 0.2 && d > 0.01)
                    {
                        contours1.Push(contours[i]);
                    }
                }
            }

            CvInvoke.DrawContours(orig, contours1, -1, new MCvScalar(0, 255, 0), 2, LineType.EightConnected);
            //Console.WriteLine("cont add= " + contours1.Size);
            imageBox4.Image = orig;

            // box.Image = im_cont;
            var lines = findSquares_rotate(contours1, 7, orig);

            if (lines != null)
            {
                if (lines.Count != 0)
                {
                    //Console.WriteLine("LINES: " + lines.Count);
                    foreach (var p in lines)
                    {
                        var pts = find2Points(p, image.Size);
                        if (pts != null)
                        {
                            //CvInvoke.Line(orig, pts[0], pts[1], new MCvScalar(255, 255, 0), 2);
                        }
                    }

                    // imageBox3.Image = orig;
                    var pointsD = findCrossingD_rotate(lines.ToArray(), image.Size, orig);
                    //drawTours(im, pointsD, 255, 255, 0, 3);
                    //box.Image = orig;
                    if (pointsD != null)
                    {
                        if (pointsD.Length != 0)
                        {
                            //Console.WriteLine("CROSSINGS: " + pointsD.Length);
                            /*var gbs = findGabarit_rotate(pointsD);
                            var pos = new List<PointF>();
                            pos.Add(pointsD[gbs[1]]);
                            pos.Add(pointsD[gbs[0]]);
                            pos.Add(pointsD[gbs[3]]);
                            pos.Add(pointsD[gbs[2]]);*/
                            var gbs = checkPoints(pointsD);

                            //Console.WriteLine(pointsD[gbs[1]].X + " " + pointsD[gbs[1]].Y);
                            if (gbs != null)
                            {
                                var pos = new List<PointF>();
                                pos.Add(pointsD[gbs[3]]);
                                pos.Add(pointsD[gbs[1]]);
                                pos.Add(pointsD[gbs[2]]);
                                pos.Add(pointsD[gbs[0]]);
                                drawTours(orig, pos.ToArray(), 255, 255, 0, 6);
                                box.Image = orig;
                                imageBox6.Image = orig;
                                return pos.ToArray();
                            }
                            else
                            {
                                return null;
                            }

                        }
                    }

                }

            }

            return null;
        }
        List<PointF[]> finPointFsFromIm_kalib(Mat im, int bin)
        {
            var receivedImage = new Mat();
            var orig = new Mat();
            im.CopyTo(receivedImage);
            im.CopyTo(orig);
            // imageBox3.Image = im;
            var im_cont = revealContourQ(receivedImage, bin, imageBox4);
            Image<Gray, byte> image = im_cont.ToImage<Gray, Byte>();//.ThresholdBinary(new Gray(bin), new Gray(255));

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint contours1 = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            //  imageBox5.Image = im_cont;
            CvInvoke.FindContours(image, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            if (contours.Size != 0)
            {
                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    var perim = CvInvoke.ArcLength(contours[i], true);
                    var d = area / (perim * perim);
                    if (area < maxArea && area > minArea && d < 0.2 && d > 0.02)
                    {
                        contours1.Push(contours[i]);
                    }
                }
            }
            CvInvoke.DrawContours(orig, contours1, -1, new MCvScalar(0, 255, 0), 2, LineType.EightConnected);
            Console.WriteLine("cont = " + contours1.Size);
            //   imageBox3.Image = orig;
            var lines = findSquares(contours1, 7, orig);
            if (lines != null)
            {
                if (lines.Count != 0)
                {
                    foreach (var p in lines)
                    {
                        var pts = find2Points(p, image.Size);
                        if (pts != null)
                        {
                            CvInvoke.Line(orig, pts[0], pts[1], new MCvScalar(255, 255, 0), 2);
                        }
                    }
                    var pointsXY = findCrossingD_C(lines.ToArray(), image.Size);
                    if (pointsXY != null)
                    {
                        if (pointsXY[0].Length != 0 && pointsXY[1].Length != 0)
                        {
                            drawTours(orig, pointsXY[0], 255, 255, 0, 6);
                            drawTours(orig, pointsXY[1], 150, 150, 150, 6);
                            var p_X = from f in pointsXY[1]
                                      orderby f.X
                                      select f;
                            var P_X = p_X.ToList();
                            var x_cen = P_X[0].X + (P_X[P_X.Count - 1].X - P_X[0].X) / 2;

                            var p_Y = from f in pointsXY[0]
                                      orderby f.Y
                                      select f;
                            var P_Y = p_Y.ToList();
                            var y_cen = P_Y[0].Y + (P_Y[P_Y.Count - 1].Y - P_Y[0].Y) / 2;

                            var d_x_cen = orig.Width / 2 - x_cen;
                            var d_y_cen = orig.Height / 2 - y_cen;

                            var d_len = (P_X[P_X.Count - 1].X - P_X[0].X) - (P_Y[P_Y.Count - 1].Y - P_Y[0].Y);

                            CvInvoke.PutText(orig, "Xc = " + d_x_cen.ToString(), new Point((int)(0.7 * orig.Width), (int)(0.8 * orig.Height)), FontFace.HersheyPlain, 1, new MCvScalar(255, 0, 0));
                            CvInvoke.PutText(orig, "Yc = " + d_y_cen.ToString(), new Point((int)(0.7 * orig.Width), (int)(0.85 * orig.Height)), FontFace.HersheyPlain, 1, new MCvScalar(255, 0, 0));
                            CvInvoke.PutText(orig, "dL = " + d_len.ToString(), new Point((int)(0.7 * orig.Width), (int)(0.9 * orig.Height)), FontFace.HersheyPlain, 1, new MCvScalar(255, 0, 0));

                            imageBox3.Image = orig;
                            Console.WriteLine(P_X[0].X + " " + P_X[1].X + " ");
                            Console.WriteLine(P_Y[0].X + " " + P_Y[1].X + " ");
                            var list_p = new List<PointF[]>();
                            list_p.Add(P_X.ToArray());
                            list_p.Add(P_Y.ToArray());
                            return list_p;
                        }

                    }
                }
            }
            return null;
        }

        Mat calcMark(Mat im, Mat orig)
        {

            var c = findMark(im, orig);

            return paintBlackAboveCont(orig, c);
        }

        VectorOfVectorOfPoint findBiggestContour(Mat im, Mat orig)
        {
            Image<Gray, byte> image = im.ToImage<Gray, Byte>();
            VectorOfVectorOfPoint contours1 = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            double maxA = 0;
            CvInvoke.FindContours(image, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            VectorOfPoint maxC = new VectorOfPoint();
            if (contours.Size != 0)
            {
                //Console.WriteLine(contours.Size);
                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    var perim = CvInvoke.ArcLength(contours[i], true);
                    //Console.WriteLine("area " + area);
                    //Console.WriteLine("perim " + perim);

                    var d = area / (perim * perim);
                    //Console.WriteLine("d " + d);

                    if (area > maxA && d < 0.2 && d > 0.006)
                    {
                        maxC = contours[i];
                        maxA = area;
                        // Console.WriteLine("add big");
                    }
                }
            }
            if (maxC.Size != 0)
            {
                contours1.Push(maxC);
                //CvInvoke.DrawContours(orig, contours1, -1, new MCvScalar(0, 255, 0), 1);
                return contours1;
            }
            else
            {
                return null;
            }
        }
        VectorOfVectorOfPoint findMark(Mat im, Mat orig)
        {
            var im_m = revealArea(im, imageBox3);
            return findBiggestContour(im_m, orig);
        }
        private Mat revealArea(Mat im, ImageBox box)
        {
            double scale = (double)im.Width / 600;
            var w = im.Width;
            var h = im.Height;
            var im_ = new Mat();
            CvInvoke.Resize(im, im_, new Size((int)(w / scale), (int)(h / scale)));
            var im_inp = im_.ToImage<Gray, Byte>();
            var im_med = new Image<Gray, Byte>(im_inp.Width, im_inp.Height);
            var im_lap = im_inp.Laplace(9).Convert<Gray, Byte>();

            // CvInvoke.AdaptiveThreshold(im_lap, im_med, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 21, red_c);
            CvInvoke.Threshold(im_lap, im_med, 140, 255, ThresholdType.Binary);

            CvInvoke.MedianBlur(im_med, im_med, 3);

            Mat kernel7 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(7, 7), new Point(1, 1));

            Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(1, 1));
            Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));

            Mat ellips7 = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(7, 7), new Point(1, 1));
            Image<Gray, Byte> im_med1 = im_med.MorphologyEx(MorphOp.Gradient, kernel5, new Point(-1, -1), 6, BorderType.Default, new MCvScalar());
            //im_med = im_med1.MorphologyEx(MorphOp.Close, kernel3, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.Resize(im_med1, im_med1, new Size(w, h));
            CvInvoke.Rectangle(im_med1, new Rectangle(0, 0, im_med1.Width + 40, im_med1.Height), new MCvScalar(0), 1);
            /*for (int i = 0; i < im_med1.Width; i++)
            {
                im_med1.
                im_med1.Data[im_med1.Height-100, i, 0] = 120;
            }

            for (int i = 0; i < im_med1.Height; i++)
            {
                im_med1.Data[i, 0, 0] = 0;
                im_med1.Data[i, im_med1.Width-1, 0] = 0;
            }*/

            imageBox8.Image = im_med1.Mat;
            //Console.WriteLine(im_med.Width + " " + im_med.Height);
            // var im_inp1 = deleteOnePixel(im_med.Mat);
            var im_inp1 = im_med1.Mat;

            return im_inp1;
        }
        private Mat revealContourQ(Mat im, int bin, ImageBox box)
        {
            double scale = (double)im.Width / 1200;
            var w = im.Width;
            var h = im.Height;
            var im_ = new Mat();
            // Console.WriteLine("scale = " + scale);
            CvInvoke.Resize(im, im_, new Size((int)(w / scale), (int)(h / scale)));
            var im_c = new Mat();
            im.CopyTo(im_c);

            var im_inp = im.ToImage<Gray, Byte>();
            var im_med = new Image<Gray, Byte>(im.Width, im.Height);
            var im_lap = im_inp.Laplace(5).Convert<Gray, Byte>();
            CvInvoke.MedianBlur(im_lap, im_lap, 5);

            //CvInvoke.AdaptiveThreshold(im_lap, im_med, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 21, bin);
            CvInvoke.Threshold(im_inp, im_med, bin, 255, ThresholdType.BinaryInv);

            //CvInvoke.AdaptiveThreshold(im_lap, im_med, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 7, -8);
            imageBox7.Image = im_med.Mat;
            //box.Image = im_med;
            /*Mat kernel31 = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(31, 31), new Point(1, 1));
            Mat kernel11 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(11, 11), new Point(1, 1));
            Mat kernel9 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(9, 9), new Point(1, 1));
            Mat kernel7 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(7, 7), new Point(1, 1));
            Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(1, 1));
            Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            Image<Gray, Byte> im_med1 = im_med.MorphologyEx(MorphOp.Gradient, kernel3, new Point(-1, -1), 2, BorderType.Default, new MCvScalar());
            im_med = im_med1.MorphologyEx(MorphOp.Close, kernel31, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.Resize(im_med, im_med, new Size(w, h));
            box.Image = im_med.Mat;*/


            var mat_med = im_med.Mat;
            var mat_out = calcMark(im_c, mat_med);
            return mat_out;
        }
        int[,] calcBorder(Point[] cont, Size size)
        {
            Image<Gray, Byte> im = new Image<Gray, byte>(size);
            Image<Bgr, Byte> im_deb = new Image<Bgr, byte>(size);
            var inds = checkPoints(toPointF(cont), 10, PI / 16, true);
            var map = new int[size.Width, 2];

            for (int i = 0; i < map.Length / 2; i++)
            {
                map[i, 0] = 0;
                map[i, 1] = 0;
            }

            for (int i = 0; i < cont.Length; i++)
            {

                var x = cont[i].X;
                var y = cont[i].Y;
                im.Data[y, x, 0] = 255;
                if (map[x, 0] == 0)
                {
                    map[x, 0] = y;

                }
                else if (map[x, 1] == 0)
                {
                    map[x, 1] = y;

                    if (map[x, 0] > map[x, 1])
                    {
                        var tmp = map[x, 0];
                        map[x, 0] = map[x, 1];
                        map[x, 1] = tmp;
                    }
                }
                else
                {

                    if (y > map[x, 1])
                    {
                        map[x, 1] = y;
                    }
                    else if (y < map[x, 0])
                    {
                        map[x, 0] = y;
                    }
                }

            }
            for (int i = 0; i < map.Length / 2; i++)
            {

                im_deb.Data[map[i, 0], i, 0] = 255;
                im_deb.Data[map[i, 1], i, 1] = 255;
            }
            for (int x = 0; x < size.Width; x++)
            {

            }
            map[0, 0] = cont[inds[1]].X;
            map[0, 1] = cont[inds[0]].X;
            var test = toPointF(cont);
            var pos = new List<Point>();
            pos.Add(cont[inds[0]]);
            pos.Add(cont[inds[1]]);
            pos.Add(cont[inds[2]]);
            pos.Add(cont[inds[3]]);
            imageBox3.Image = im_deb.Mat;
            //drawTours(im_deb.Mat, pos.ToArray(), 255, 0, 0, 6);
            //drawPoints(im_deb.Mat, rotateTransPointF(toPointF(cont), PI / 16,700.0f),255,255,0,2);
            foreach (var p in inds)
            {
                // Console.WriteLine("gab "+test[p]);
            }
            // Console.WriteLine("BOARDER LR " + cont[inds[0]].X + " " + cont[inds[2]].X);
            //imageBox6.Image = im_deb.Mat;
            return map;
        }
        Point[] fullCont(VectorOfVectorOfPoint contours, Size size)
        {
            if (contours != null)
            {
                if (contours.Size == 0)
                {
                    return null;
                }


                else
                {
                    var vecP = new List<Point>();
                    Image<Gray, Byte> im = new Image<Gray, byte>(size);
                    CvInvoke.DrawContours(im, contours, -1, new MCvScalar(255), 1);
                    for (int y = 0; y < im.Height; y++)
                    {
                        for (int x = 0; x < im.Width; x++)
                        {
                            if (im.Data[y, x, 0] > 100)
                            {
                                vecP.Add(new Point(x, y));
                            }
                        }
                    }

                    return vecP.ToArray();
                }
            }
            else
            {
                return null;
            }
        }
        Mat paintBlackAboveCont(Mat _im, VectorOfVectorOfPoint cont)
        {
            Image<Gray, Byte> im = _im.ToImage<Gray, Byte>();

            var fullcont = fullCont(cont, im.Size);
            var border = calcBorder(fullcont, im.Size);

            for (int x = 0; x < im.Width; x++)
            {
                if (x > border[0, 0] || x < border[0, 1])
                {
                    for (int y = 0; y < border[x, 0]; y++)
                    {
                        im.Data[y, x, 0] = 0;
                    }

                    for (int y = border[x, 1]; y < im.Height; y++)
                    {
                        im.Data[y, x, 0] = 0;
                    }
                }
                else
                {
                    for (int y = 0; y < im.Height; y++)
                    {
                        im.Data[y, x, 0] = 0;
                    }
                }
            }
            imageBox6.Image = im.Mat;
            return im.Mat;
        }
        PointF[] rotateTransPointF(PointF[] mass_p, float angle = 0, float x = 0, float y = 0)
        {
            if (mass_p == null)
            {
                return null;
            }
            if (mass_p.Length == 0)
            {
                return null;
            }
            var vec = new PointF(x, y);
            if (x != 0 || y != 0)
            {
                var trans = translPointF(mass_p, vec);
                var polar = decToPolPointF(trans);
                var rotate = PolToDecPointF(polar, angle);
                return rotate;
            }
            else
            {
                var polar = decToPolPointF(mass_p);
                var rotate = PolToDecPointF(polar, angle);
                return rotate;
            }



        }
        PointF[] decToPolPointF(PointF[] mass_p)
        {
            var mass_p_ret = new PointF[mass_p.Length];
            int i = 0;
            foreach (var p in mass_p)
            {
                var r = p.norm;
                if (p.X > 0)
                {
                    var fi = Math.Asin(p.Y / p.norm);
                    mass_p_ret[i] = new PointF(r, (float)fi);
                    i++;
                }
                else if (p.X < 0)
                {
                    var fi = Math.Asin(p.Y / p.norm);
                    mass_p_ret[i] = new PointF(r, PI - (float)fi);
                    i++;
                }
                else
                {
                    if (p.Y > 0)
                    {
                        var fi = PI / 2;
                        mass_p_ret[i] = new PointF(r, (float)fi);
                        i++;
                    }
                    else
                    {
                        var fi = -PI / 2;
                        mass_p_ret[i] = new PointF(r, (float)fi);
                        i++;
                    }
                }

            }

            return mass_p_ret;
        }
        PointF[] PolToDecPointF(PointF[] mass_p, float angle = 0.0f)
        {
            var mass_p_ret = new PointF[mass_p.Length];
            int i = 0;
            foreach (var p in mass_p)
            {
                var r = p.X;
                var fi = p.Y - angle;
                var x = r * Math.Cos(fi);
                var y = r * Math.Sin(fi);
                mass_p_ret[i] = new PointF((float)x, (float)y);
                i++;
            }
            return mass_p_ret;
        }
        PointF[] translPointF(PointF[] mass_p, PointF vec)
        {
            var mass_p_ret = new PointF[mass_p.Length];
            int i = 0;
            foreach (var p in mass_p)
            {
                mass_p_ret[i] = p - vec;
                i++;
            }
            return mass_p_ret;
        }
        public int calcRes(byte[,,] im_res, int x, int y)
        {

            int res = im_res[y - 5, x - 5, 0] + im_res[y - 5, x - 4, 0] + im_res[y - 5, x - 3, 0] + im_res[y - 5, x - 2, 0] + im_res[y - 5, x - 1, 0] + im_res[y - 5, x, 0] + im_res[y - 5, x + 1, 0] + im_res[y - 5, x + 2, 0] + im_res[y - 5, x + 3, 0] + im_res[y - 5, x + 4, 0] + im_res[y - 5, x + 5, 0] +
                        im_res[y - 4, x - 5, 0] + im_res[y - 4, x + 5, 0] +
                        im_res[y - 3, x - 5, 0] + im_res[y - 3, x + 5, 0] +
                        im_res[y - 2, x - 5, 0] + im_res[y - 2, x + 5, 0] +
                        im_res[y - 1, x - 5, 0] + im_res[y - 1, x + 5, 0] +
                        im_res[y, x - 5, 0] + im_res[y, x + 5, 0] +
                        im_res[y + 1, x - 5, 0] + im_res[y + 1, x + 5, 0] +
                        im_res[y + 2, x - 5, 0] + im_res[y + 2, x + 5, 0] +
                        im_res[y + 3, x - 5, 0] + im_res[y + 3, x + 5, 0] +
                        im_res[y + 4, x - 5, 0] + im_res[y + 4, x + 5, 0] +
                    im_res[y + 5, x - 5, 0] + im_res[y + 5, x - 4, 0] + im_res[y + 5, x - 3, 0] + im_res[y + 5, x - 2, 0] + im_res[y + 5, x - 1, 0] + im_res[y + 5, x, 0] + im_res[y + 5, x + 1, 0] + im_res[y + 5, x + 2, 0] + im_res[y + 5, x + 3, 0] + im_res[y + 5, x + 4, 0] + im_res[y + 5, x + 5, 0];
            return res;
        }



        private Mat revealContour(Mat im, int bin, ImageBox box)
        {
            var im_inp = im.ToImage<Gray, Byte>();
            var im_med = new Image<Gray, Byte>(im.Width, im.Height);
            var im_lap = im_inp.Laplace(5).Convert<Gray, Byte>();
            CvInvoke.Threshold(im_lap, im_med, bin, 255, ThresholdType.Binary);
            var im_inp1 = deleteOnePixel(im_med.Mat);
            box.Image = im_inp1.ToImage<Gray, Byte>();

            return im_inp1;
        }
        Point calc_sr_p(Point p1, Point p2)
        {
            return new Point(p2.X + (p1.X - p2.X) / 2, p2.Y + (p1.Y - p2.Y) / 2);//ОКРУГЛИЛ
        }
        PointF calc_sr_dp(Point p1, Point p2)
        {
            return new PointF(p1.X + (p2.X - p1.X) / 2, p1.Y + (p2.Y - p1.Y) / 2);
        }
        PointF calc_sr_dp(PointF p1, PointF p2)
        {
            return new PointF(p1.X + (p2.X - p1.X) / 2, p1.Y + (p2.Y - p1.Y) / 2);
        }

        PointF calcLine(PointF p1, PointF p2)
        {
            var dx = p1.X - p2.X;
            if (dx != 0)
            {
                var a = (p1.Y - p2.Y) / dx;
                return new PointF(a, p1.Y - (a * p1.X));
            }
            else
            {
                Console.WriteLine("dx = 0");
                return new PointF(0, p1.Y);
            }
        }
        Point[] toPoint(PointF[] ps)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new Point[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ret[i] = new Point((int)ps[i].X, (int)ps[i].Y);
            }
            return ret;
        }
        Point[] toPoint(System.Drawing.PointF[] ps)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new System.Drawing.Point[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ret[i] = new System.Drawing.Point((int)ps[i].X, (int)ps[i].Y);
            }
            return ret;
        }
        public PointF[] toPointF(Point[] ps)
        {
            var ret = new PointF[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ret[i] = new PointF(ps[i].X, ps[i].Y);
            }
            return ret;
        }

        PointF[] toPointF(VectorOfPoint ps)
        {
            var ret = new PointF[ps.Size];
            for (int i = 0; i < ps.Size; i++)
            {
                ret[i] = new PointF(ps[i].X, ps[i].Y);
            }
            return ret;
        }

        int[] findLine(List<PointF[]> points, double a, double b, PlanType planType)
        {
            var Xindecis = new List<int>();

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i][0];
                if (planType == PlanType.X)
                {
                    if (p.Y > a * p.X + b)
                    {
                        Xindecis.Add(i);
                    }
                }
                else
                {
                    /* if (p.Y > a * p.X + b)
                     {
                         Xindecis.Add(i);
                     }*/
                    double x = 0;
                    if (a != 0)
                    {
                        x = (p.Y - b) / a;
                    }
                    if (p.X < x)
                    {
                        Xindecis.Add(i);
                    }
                }

            }
            return Xindecis.ToArray();
        }

        int[] findLine_rotate(PointF[] points)
        {
            var Xindecis = new List<int>();
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];

                if (p.Y > 0)
                {
                    //Console.WriteLine
                    Xindecis.Add(i);
                }
            }
            return Xindecis.ToArray();
        }
        PointF linearApprox(List<PointF> points)
        {
            int n = points.Count;
            double x = 0;
            double y = 0;
            double xy = 0;
            double xx = 0;
            foreach (var p in points)
            {
                x += p.X;
                y += p.Y;
                xy += p.X * p.Y;
                xx += p.X * p.X;
            }
            var del = (n * xx - x * x);
            if (Math.Abs(del) < 0.0001)
            {
                del = 0.00001;
                //return new PointF(4000, (float)x/(float)n);
            }
            float a = (float)((n * xy - x * y) / del);
            Console.WriteLine("a  = " + a);
            Console.WriteLine("del  = " + del);
            Console.WriteLine("num  = " + (n * xy - x * y));
            float b = (float)((y - a * x) / n);
            return new PointF(a, b);
        }
        List<PointF[]> findOneLine(List<PointF[]> points, List<PointF> points_ret, int side, PlanType planType, Mat im)
        {
            var Xindecis = new List<int>();
            var points_cut = new List<PointF[]>();
            var indices = checkPoints(ListPointToPoint(points));
            //var indices = findGabarit_rotate(points);
            int ind = 0;
            /* foreach (var i in indices)
             {
                 Console.WriteLine("i"+ind+" = "+ i);
                 ind++;
             }*/

            var upPoint = points[indices[3]][0];
            var downPoint = points[indices[1]][0];
            var leftPoint = points[indices[0]][0];
            var rightPoint = points[indices[2]][0];
            var pss = new PointF[] { upPoint, downPoint, leftPoint, rightPoint };
            if (planType == PlanType.Y)
            {
                drawTours(im, pss, 255, 0, 0, 2);
            }


            var Line_up = new PointF(1, 1);
            var Line_down = new PointF(1, 1);

            if (planType == PlanType.X)
            {
                Line_down = calcLine(downPoint, leftPoint);
                Line_up = calcLine(rightPoint, upPoint);
                if (downPoint.X < leftPoint.X || upPoint.X > rightPoint.X)
                {
                    Line_down = calcLine(downPoint, leftPoint);
                    Line_up = calcLine(rightPoint, upPoint);
                }
            }
            else
            {
                Line_down = calcLine(downPoint, rightPoint);
                Line_up = calcLine(leftPoint, upPoint);
                if (downPoint.X < leftPoint.X || upPoint.X > rightPoint.X)
                {

                    Line_down = calcLine(upPoint, leftPoint);
                    Line_up = calcLine(rightPoint, downPoint);
                }
            }

            var col = (float)points.Count / (float)side - 1;
            Console.WriteLine(col);

            double b_k = 0.5;
            double b_1 = b_k * (Line_up.Y - Line_down.Y) / col;
            if (col == 0)
            {
                b_1 = 4000;//for last 100%
            }

            double a = Line_up.X;
            double b = Line_up.Y - b_1;

            Console.WriteLine("UP   a = " + a + " b = " + b);
            Console.WriteLine("DOWN a = " + Line_down.X + " b = " + Line_down.Y);
            var xind = findLine(points, a, b, planType);

            if (xind.Length > side)
            {
                b_k -= 0.2;
                b_1 = b_k * (Line_up.Y - Line_down.Y) / col;
                a = Line_up.X;
                b = Line_up.Y - b_1;
                Console.WriteLine("x = " + a + " y = " + b);
                xind = findLine(points, a, b, planType);
            }
            else if (xind.Length < side)
            {
                b_k += 0.2;
                b_1 = b_k * (Line_up.Y - Line_down.Y) / col;
                a = Line_up.X;
                b = Line_up.Y - b_1;
                Console.WriteLine("x = " + a + " y = " + b);
                xind = findLine(points, a, b, planType);
            }
            else
            {

            }
            var pts = find2Points(new PointF((float)a, (float)b), im.Size);
            if (pts != null)
            {
                CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(0, 255, 255), (int)col + 1);
            }

            var points_cen = new List<PointF>();
            foreach (var i in xind)
            {
                points_cen.Add(points[i][0]);
            }
            Xindecis.AddRange(xind);
            for (int i = 0; i < points.Count; i++)
            {
                if (Xindecis.Contains(i))
                {
                }
                else
                {
                    points_cut.Add(points[i]);
                }
            }
            points = new List<PointF[]>();
            for (int i = 0; i < points_cut.Count; i++)
            {
                points.Add(points_cut[i]);
            }
            //var points_cen_rev_rot = rotatePoints(points_cen, -rotate_angle);
            var appLine = linearApprox(points_cen);
            points_ret.Add(appLine);
            return points;
        }
        List<PointF[]> findOneLine_rotate(List<PointF[]> points, List<LineF> points_ret, int side, PlanType planType, Mat im)
        {
            //Console.WriteLine("LINE_________________________________________");
            var l = new LineF(1, 1);
            var Xindecis = new List<int>();
            var points_cut = new List<PointF[]>();
            var points_rot = new PointF[points.Count];
            for (int i = 0; i < points_rot.Length; i++)
            {
                points_rot[i] = points[i][0];
            }
            var indices = findGabarit_rotate(points_rot, PI / 6);
            indices = checkPoints(points_rot, 10, PI / 20);

            if (points.Count <= side)
            {

                var appLine = l.calcLine(points_rot, planType);
                points_ret.Add(appLine);

                return new List<PointF[]>();
            }

            /* foreach (var i in indices)
             {
                 Console.WriteLine("i"+ind+" = "+ i);
                 ind++;
             }*/

            /*var upPoint = points_rot[indices1[0]];
            var downPoint = points_rot[indices1[3]];
            var leftPoint = points_rot[indices1[2]];
            var rightPoint = points_rot[indices1[1]];
            Console.WriteLine(upPoint + " " + downPoint + " " + leftPoint + " " + rightPoint + " ");*/
            if (indices == null)
            {
                return null;
            }
            var upPoint = points_rot[indices[3]];
            var downPoint = points_rot[indices[1]];
            var leftPoint = points_rot[indices[0]];
            var rightPoint = points_rot[indices[2]];

            bool sq_1 = (Math.Abs(leftPoint.X - downPoint.X)) < 5 && ((upPoint.X - rightPoint.X) < 5);
            bool sq_2 = (Math.Abs(upPoint.X - downPoint.X)) < 5 && ((leftPoint.X - rightPoint.X) < 5);
            bool sq = sq_1 || sq_2;

            //Console.WriteLine("DELTS X " + (Math.Abs(leftPoint.X - downPoint.X)) + " " + ((upPoint.X - rightPoint.X)));
            //Console.WriteLine("DELTS X " + (Math.Abs(upPoint.X - downPoint.X)) + " " + ((leftPoint.X - rightPoint.X)));
            if (sq)//check square
            {
                //Console.WriteLine("THIS IS SQUARE");
                //indices = findGabarit_rotate(points_rot, -PI / 2);
                indices = checkPoints(points_rot, 10, -PI / 3, true);
                //Console.WriteLine("LEN " +points_rot.Length);
                upPoint = points_rot[indices[2]];
                downPoint = points_rot[indices[3]];
                leftPoint = points_rot[indices[1]];
                rightPoint = points_rot[indices[0]];
            }

            upPoint = points_rot[indices[2]];
            downPoint = points_rot[indices[3]];
            leftPoint = points_rot[indices[1]];
            rightPoint = points_rot[indices[0]];
            //check:0 3 2 1 
            //rot:3 1 0 2 

            //Console.WriteLine(upPoint + " | " + downPoint + " | " + leftPoint + " | " + rightPoint + " | ");
            var pss = new PointF[] { upPoint, downPoint, leftPoint, rightPoint };


            var Line_down = new PointF[2];



            if (planType == PlanType.X)
            {
                Line_down = new PointF[] { rightPoint, downPoint };
            }
            else
            {
                Line_down = new PointF[] { leftPoint, downPoint };
            }
            var vec = Line_down[1] - Line_down[0];
            float angle = 0.0f;

            if (vec.X > 0)
            {
                angle = (float)Math.Asin(vec.Y / vec.norm);
            }
            else if (vec.X < 0)
            {
                angle = (float)(PI - Math.Asin(vec.Y / vec.norm));

            }
            else
            {
                if (vec.Y > 0)
                {
                    angle = PI / 2;
                }
                else
                {
                    angle = -PI / 2;

                }
            }
            if (planType == PlanType.Y)
            {
                angle += PI;
            }

            if (sq)
            {
                if (planType == PlanType.X)
                {
                    //Console.WriteLine("PLAN X");
                    angle = 0.0f;
                }
                else
                {
                    //Console.WriteLine("PLAN Y");
                    angle = -PI / 2;
                }

            }
            // Console.WriteLine("ANGLE " + angle);
            var points_end = rotateTransPointF(points_rot, angle, downPoint.X, downPoint.Y);//translate points
            if (sq)
            {
                points_end = rotateTransPointF(points_rot, angle, upPoint.X, upPoint.Y);
            }

            var col = (float)points.Count / (float)side - 1;
            //Console.WriteLine(col);


            var test = findGabarit_rotate(points_end);
            // var test1 = checkPoints(points_end);

            foreach (var p in test)
            {
                //Console.WriteLine("ps_end1 " + points_end[p]);
            }


            var size = findSize(points_end);
            if (col == 0)
            {
                col = 0.5f;
            }
            float H = -(float)size.Height / (2 * col);

            if (col < 0.6)
            {
                H = -1000.0f;
            }

            var points_trans = translPointF(points_end, new PointF(00.0f, H));

            if (planType == PlanType.X && col == 4)
            {
                //drawTours(im, points_rot, 0, 255, 0, 2);
                //drawTours(im, points_trans, 0, 0, 255, 2);
                // drawTours(im, pss, 255, 0, 0, 2);
            }
            // drawTours(im, points_trans, 255, 0, 0, 2);

            test = findGabarit_rotate(points_trans);
            foreach (var p in test)
            {
                //Console.WriteLine("ps_trans_r " + points_trans[p]);
            }
            //find inds

            var xind = findLine_rotate(points_trans);
            // Console.WriteLine("LINE_COUNT " + xind.Length);
            /*if (xind.Length > side)
            {
                b_k -= 0.2;
                b_1 = b_k * (Line_up.Y - Line_down.Y) / col;
                a = Line_up.X;
                b = Line_up.Y - b_1;
                Console.WriteLine("x = " + a + " y = " + b);
                xind = findLine(points, a, b, planType);
            }
            else if (xind.Length < side)
            {
                b_k += 0.2;
                b_1 = b_k * (Line_up.Y - Line_down.Y) / col;
                a = Line_up.X;
                b = Line_up.Y - b_1;
                Console.WriteLine("x = " + a + " y = " + b);
                xind = findLine(points, a, b, planType);
            }
            else
            {

            }*/


            /*var pts = find2Points(new PointF((float)a, (float)b), im.Size);
            if (pts != null)
            {
                CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(0, 255, 255), (int)col + 1);
            }
            */
            var points_cen = new List<PointF>();
            foreach (var i in xind)
            {
                points_cen.Add(points[i][0]);
            }
            Xindecis.AddRange(xind);
            for (int i = 0; i < points.Count; i++)
            {
                if (Xindecis.Contains(i))
                {
                }
                else
                {
                    points_cut.Add(points[i]);
                }
            }
            points = new List<PointF[]>();
            for (int i = 0; i < points_cut.Count; i++)
            {
                points.Add(points_cut[i]);
            }
            // var points_cen_rev_rot = rotatePoints(points_cen, -rotate_angle);
            var appLine1 = l.calcLine(points_cen.ToArray(), planType);
            points_ret.Add(appLine1);

            return points;
        }
        List<PointF> findSquares(VectorOfVectorOfPoint contours, int side, Mat im)
        {
            if (contours.Size == 0)
            {
                return null;
            }
            else
            {
                var points_ret = new List<PointF>();
                var pointsX = new List<PointF[]>();
                var pointsY = new List<PointF[]>();

                var points_t = new List<PointF>();
                for (int i = 0; i < contours.Size; i++)
                {
                    //points.Add(findCentrD(contours[i]));
                    var ps = findCentrD_rotate(contours[i], im);
                    pointsX.Add(ps);
                    pointsY.Add(ps);
                    points_t.Add(ps[0]);
                }
                //drawTours(im, points_t.ToArray(), 255, 0, 0, 2);
                //var points_rot = rotatePoints(points, rotate_angle);
                //points = points_rot;
                Console.WriteLine("points = " + points_t.Count);
                if (points_t.Count == 0)
                {
                    return null;
                }
                else
                {
                    int ind_max = 2 * side;
                    int ind = 0;
                    while (pointsX.Count > 0 && ind < ind_max)
                    {

                        pointsX = findOneLine(pointsX, points_ret, side, PlanType.X, im);
                        if (pointsX == null)
                        {
                            Console.WriteLine("X NULL");
                            return null;
                        }
                        //drawTours(im, PointToP(pointsY), ind*20, ind * 20, ind * 20, 4);
                        Console.WriteLine("X_rem = " + pointsX.Count);
                        Console.WriteLine("X_ind = " + ind);
                        ind++;

                    }
                    Console.WriteLine("indX = " + ind);
                    ind = 0;
                    Console.WriteLine("-----------------------------------------------------------------------");
                    while (pointsY.Count > 0 && ind < ind_max)
                    {
                        pointsY = findOneLine(pointsY, points_ret, side, PlanType.Y, im);
                        if (pointsY == null)
                        {
                            return null;
                        }
                        Console.WriteLine("Y_rem = " + pointsY.Count);
                        ind++;
                    }
                    Console.WriteLine("indY = " + ind);
                    return points_ret;
                }
            }
        }
        List<LineF> findSquares_rotate(VectorOfVectorOfPoint contours, int side, Mat im)
        {
            if (contours.Size == 0)
            {
                return null;
            }
            else
            {
                var points_ret = new List<LineF>();
                var pointsX = new List<PointF[]>();
                var pointsY = new List<PointF[]>();

                var points_t = new List<PointF>();
                for (int i = 0; i < contours.Size; i++)
                {
                    //points.Add(findCentrD(contours[i]));
                    var ps = findCentrD_rotate(contours[i], im);
                    if (ps != null)
                    {
                        pointsX.Add(ps);
                        pointsY.Add(ps);
                        points_t.Add(ps[0]);
                    }

                }
                drawTours(im, points_t.ToArray(), 255, 0, 0, 2);
                imageBox5.Image = im;
                //var points_rot = rotatePoints(points, rotate_angle);
                //points = points_rot;
                //Console.WriteLine("points = " + points_t.Count);
                if (points_t.Count == 0)
                {
                    return null;
                }
                else
                {

                    int ind_max = 2 * side;
                    int ind = 0;
                    while (pointsX.Count > 0 && ind < ind_max)
                    {

                        pointsX = findOneLine_rotate(pointsX, points_ret, side, PlanType.X, im);
                        if (pointsX == null)
                        {
                            return null;
                        }
                        //drawTours(im, PointToP(pointsY), ind*20, ind * 20, ind * 20, 4);
                        // Console.WriteLine("X_rem = " + pointsX.Count);
                        ind++;

                    }
                    // Console.WriteLine("indX = " + ind);
                    ind = 0;
                    //Console.WriteLine("-----------------------------------------------------------------------");
                    while (pointsY.Count > 0 && ind < ind_max)
                    {
                        pointsY = findOneLine_rotate(pointsY, points_ret, side, PlanType.Y, im);
                        if (pointsY == null)
                        {
                            return null;
                        }
                        //Console.WriteLine("Y_rem = " + pointsY.Count);
                        ind++;
                    }
                    // Console.WriteLine("indY = " + ind);
                    return points_ret;
                }
            }
        }



        PointF[] ListPointToPoint(List<PointF[]> ps)
        {
            List<PointF> ret = new List<PointF>();
            foreach (var p in ps)
            {
                ret.Add(p[0]);
            }
            return ret.ToArray();
        }
        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        int[] findGabarit_rotate(PointF[] cont, float maxAngle = 2 * PI, int count = 100)
        {
            var alpha_max = maxAngle;
            var del = alpha_max / count;
            List<int[]> ps = new List<int[]>();
            for (float alpha = 0; alpha < alpha_max; alpha += del)
            {
                var cont_rot = rotateTransPointF(cont, alpha);
                ps.Add(findGabarit(cont_rot));
            }
            int len = cont.Length;
            int[] inds = new int[len];
            foreach (var p in ps)
            {
                foreach (var s in p)
                {
                    inds[s]++;
                }
            }
            int n = 4;
            var ps_i = indexOfMaxValue(inds, n);
            PointF[] cont_ret = new PointF[n];
            for (int i = 0; i < cont_ret.Length; i++)
            {
                cont_ret[i] = cont[ps_i[i]];
            }
            var inds_f = findGabarit_cut(cont_ret);
            int[] inds_r = new int[n];
            for (int i = 0; i < n; i++)
            {
                inds_r[i] = ps_i[inds_f[i]];
            }
            return inds_r;
        }
        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        int[] findGabarit_rotate(VectorOfPoint cont)
        {
            var alpha_max = 2 * PI;
            var del = alpha_max / 60;
            List<int[]> ps = new List<int[]>();
            for (float alpha = 0; alpha < alpha_max; alpha += del)
            {
                var cont_rot = rotatePoints(cont, alpha);
                ps.Add(findGabarit(cont_rot));
            }
            int len = cont.Size;
            int[] inds = new int[len];

            foreach (var p in ps)
            {
                foreach (var s in p)
                {
                    inds[s]++;
                }
            }
            int n = 4;
            var ps_i = indexOfMaxValue(inds, n);
            VectorOfPoint cont_ret = new VectorOfPoint();
            for (int i = 0; i < ps_i.Length; i++)
            {
                cont_ret.Push(new Point[] { cont[ps_i[i]] });
            }
            var inds_f = findGabarit_cut(cont_ret);
            int[] inds_r = new int[n];
            for (int i = 0; i < n; i++)
            {
                inds_r[i] = ps_i[inds_f[i]];
            }
            return inds_r;
        }
        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        int[] findGabarit_rotate(List<PointF[]> cont)
        {
            if (cont == null)
            {
                return null;
                if (cont.Count == 0)
                {
                    return null;
                }
            }
            var alpha_max = 2 * PI;
            var del = alpha_max / 60;
            List<int[]> ps = new List<int[]>();

            for (float alpha = 0; alpha < alpha_max; alpha += del)
            {
                var cont_rot = rotatePoints(cont, alpha);
                ps.Add(findGabarit(cont_rot));
            }

            int len = cont.Count;
            int[] inds = new int[len];
            foreach (var p in ps)
            {
                foreach (var s in p)
                {
                    inds[s]++;
                }
            }
            int n = 4;
            var ps_i = indexOfMaxValue(inds, n);
            List<PointF[]> cont_ret = new List<PointF[]>();
            for (int i = 0; i < ps_i.Length; i++)
            {
                cont_ret.Add(cont[ps_i[i]]);
            }
            var inds_f = findGabarit_cut(cont_ret);
            int[] inds_r = new int[n];
            for (int i = 0; i < inds_r.Length; i++)
            {
                inds_r[i] = ps_i[inds_f[i]];
            }
            return inds_r;
        }

        int[] indexOfMaxValue(int[] mass, int n)
        {
            List<int> ps = new List<int>();
            int[] mass_copy = new int[mass.Length];
            mass.CopyTo(mass_copy);
            for (int i = 0; i < n; i++)
            {
                int ind = indexOfMaxValueOne(mass_copy);
                ps.Add(ind);
                mass_copy[ind] = int.MinValue;
            }
            return ps.ToArray();
        }
        int indexOfMaxValueOne(int[] mass)
        {
            int ind_max = 0;
            var max_val = int.MinValue;
            for (int i = 0; i < mass.Length; i++)
            {
                if (mass[i] > max_val)
                {
                    max_val = mass[i];
                    ind_max = i;
                }
            }
            return ind_max;
        }


        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        int[] findGabarit(VectorOfPoint cont)

        {
            int X_min = int.MaxValue;
            int Y_min = int.MaxValue;
            int X_max = int.MinValue;
            int Y_max = int.MinValue;
            var indices = new int[4];
            for (int i = 0; i < cont.Size; i++)
            {
                var p = cont[i];
                if (p.X > X_max)
                {
                    X_max = p.X;
                    indices[2] = i;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                    indices[0] = i;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }
            }
            return indices;
        }
        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        int[] findGabarit(Point[] cont)
        {
            int X_min = int.MaxValue;
            int Y_min = int.MaxValue;
            int X_max = int.MinValue;
            int Y_max = int.MinValue;
            var indices = new int[4];
            for (int i = 0; i < cont.Length; i++)
            {
                var p = cont[i];
                if (p.X > X_max)
                {
                    X_max = p.X;
                    indices[2] = i;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                    indices[0] = i;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }
            }
            return indices;
        }
        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        ///
        int[] findGabarit(PointF[] cont)

        {

            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var indices = new int[4];
            for (int i = 0; i < cont.Length; i++)
            {
                var p = cont[i];
                if (p.X > X_max)
                {
                    X_max = p.X;
                    indices[2] = i;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                    indices[0] = i;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }
            }
            return indices;
        }
        int[] findGabarit(List<PointF[]> cont)

        {
            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var indices = new int[4];
            for (int i = 0; i < cont.Count; i++)
            {
                var p = cont[i][0];
                if (p.X > X_max)
                {
                    X_max = p.X;
                    indices[2] = i;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                    indices[0] = i;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }
            }
            return indices;
        }
        int[] findGabarit_cut(VectorOfPoint cont)

        {
            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var indices = new int[4];


            for (int i = 0; i < cont.Size; i++)
            {

                var p = cont[i];
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }

            }
            for (int i = 0; i < cont.Size; i++)
            {
                if ((i != indices[1]) && (i != indices[3]))
                {
                    var p = cont[i];
                    if (p.X > X_max)
                    {
                        X_max = p.X;
                        indices[2] = i;
                    }
                    if (p.X < X_min)
                    {
                        X_min = p.X;
                        indices[0] = i;
                    }
                }
            }
            return indices;
        }
        int[] findGabarit_cut(PointF[] cont)

        {
            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var indices = new int[4];


            for (int i = 0; i < cont.Length; i++)
            {

                var p = cont[i];
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }

            }
            for (int i = 0; i < cont.Length; i++)
            {
                if ((i != indices[1]) && (i != indices[3]))
                {
                    var p = cont[i];
                    if (p.X > X_max)
                    {
                        X_max = p.X;
                        indices[2] = i;
                    }
                    if (p.X < X_min)
                    {
                        X_min = p.X;
                        indices[0] = i;
                    }
                }
            }
            return indices;
        }
        int[] findGabarit_cut(List<PointF[]> cont)
        {
            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var indices = new int[4];
            for (int i = 0; i < cont.Count; i++)
            {

                var p = cont[i][0];
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }

            }
            for (int i = 0; i < cont.Count; i++)
            {
                if ((i != indices[1]) && (i != indices[3]))
                {
                    var p = cont[i][0];
                    if (p.X > X_max)
                    {
                        X_max = p.X;
                        indices[2] = i;
                    }
                    if (p.X < X_min)
                    {
                        X_min = p.X;
                        indices[0] = i;
                    }
                }
            }


            return indices;
        }
        /// <summary>
        /// MaxX MinX MaxY MinY
        /// </summary>
        int[] findMaxMin(PointF[] cont)
        {
            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var ret = new int[4];
            for (int i = 0; i < cont.Length; i++)
            {
                var p = cont[i];
                if (p.X > X_max)
                {
                    X_max = p.X;
                    ret[0] = i;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                    ret[1] = i;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    ret[2] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    ret[3] = i;
                }
            }
            return ret;
        }
        Size findSize(PointF[] cont)

        {

            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            for (int i = 0; i < cont.Length; i++)
            {
                var p = cont[i];
                if (p.X > X_max)
                {
                    X_max = p.X;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                }
            }
            return new Size((int)(X_max - X_min), (int)(Y_max - Y_min));
        }
        List<PointF[]> rotatePoints(List<PointF[]> inp, double alpha)
        {
            var outp = new List<PointF[]>();
            foreach (var p in inp)
            {
                var outP = new List<PointF>();
                foreach (var P in p)
                {
                    var r = calc_p_len(P, new PointF(0, 0));
                    double gamma = 0;
                    if (r != 0)
                    {
                        gamma = Math.Asin(P.Y / r);
                    }
                    outP.Add(new PointF((float)(r * Math.Cos(gamma - alpha)),
                        (float)(r * Math.Sin(gamma - alpha))));
                }
                outp.Add(outP.ToArray());
            }
            return outp;
        }

        List<PointF> rotatePoints(List<PointF> inp, double alpha)
        {
            var outP = new List<PointF>();
            foreach (var P in inp)
            {
                var r = calc_p_len(P, new PointF(0, 0));
                double gamma = 0;
                if (r != 0)
                {
                    gamma = Math.Asin(P.Y / r);
                }
                outP.Add(new PointF((float)(r * Math.Cos(gamma - alpha)),
                    (float)(r * Math.Sin(gamma - alpha))));
            }
            return outP;
        }

        VectorOfPoint rotatePoints(VectorOfPoint inpV, double alpha)
        {
            var inp = new List<Point>();
            var outP = new List<Point>();
            for (int i = 0; i < inpV.Size; i++)
            {
                inp.Add(inpV[i]);
            }
            foreach (var P in inp)
            {
                var r = calc_p_len(P, new Point(0, 0));
                double gamma = 0;
                if (r != 0)
                {
                    gamma = Math.Asin(P.Y / r);
                }
                outP.Add(new Point((int)(r * Math.Cos(gamma - alpha)),
                    (int)(r * Math.Sin(gamma - alpha))));
            }
            return new VectorOfPoint(outP.ToArray());
        }

        PointF[] findCentrD(VectorOfPoint cont)
        {
            var indices = findGabarit(cont);
            var p1 = calc_sr_dp(cont[indices[0]], cont[indices[2]]);
            var p2 = calc_sr_dp(cont[indices[1]], cont[indices[3]]);
            var ret = new List<PointF>();
            ret.Add(calc_sr_dp(p1, p2));
            for (int i = 0; i < 4; i++)
            {
                var p = cont[indices[i]];
                ret.Add(new PointF(p.X, p.Y));
            }
            return ret.ToArray();
        }

        PointF[] findCentrD_rotate(VectorOfPoint cont, Mat im)
        {
            var pf = toPointF(cont);
            //drawPoints(im, pf, 0, 255, 0, 2);
            var indices = checkPoints(pf, 100, PI / 18, true);
            if (indices == null)
            {
                return null;
            }
            var p1 = calc_sr_dp(pf[indices[0]], pf[indices[2]]);
            var p2 = calc_sr_dp(pf[indices[1]], pf[indices[3]]);
            var ret = new List<PointF>();
            ret.Add(calc_sr_dp(p1, p2));
            for (int i = 0; i < 4; i++)
            {
                var p = pf[indices[i]];
                ret.Add(new PointF(p.X, p.Y));
            }
            return ret.ToArray();
        }
        Point findCentr(VectorOfPoint cont)
        {
            var indices = findGabarit(cont);
            var p1 = calc_sr_p(cont[indices[0]], cont[indices[2]]);
            var p2 = calc_sr_p(cont[indices[1]], cont[indices[3]]);
            return calc_sr_p(p1, p2);
        }
        Point[] find2Points(PointF ab, Size size)
        {
            int ind = 0;
            var ret = new Point[4];
            var a = ab.X;
            var b = ab.Y;

            var x0 = -b / a;
            var xH = (size.Height - b) / a;

            var y0 = b;
            var yW = a * size.Width + b;
            if (x0 >= 0 && x0 <= size.Width)
            {

                ret[ind] = new Point((int)x0, 0);
                ind++;
            }
            if (xH >= 0 && xH <= size.Width)
            {
                ret[ind] = new Point((int)xH, size.Height - 1);
                ind++;
            }
            if (y0 >= 0 && y0 <= size.Height)
            {
                ret[ind] = new Point(0, (int)y0);
                ind++;
            }
            if (yW >= 0 && yW <= size.Height)
            {
                ret[ind] = new Point(size.Width - 1, (int)yW);
                ind++;
            }
            if (ind == 2)
            {
                return ret;
            }
            return ret;
        }
        Point[] find2Points(LineF ab, Size size)
        {
            if (ab == null)
            {
                return null;
            }
            int ind = 0;

            var ret = new Point[4];
            var a = ab.X;
            var b = ab.Y;

            var x0 = -b / a;
            var xH = (size.Height - b) / a;

            var y0 = b;
            var yW = a * size.Width + b;

            if (ab.plan == PlanType.Y)
            {
                y0 = -b / a;
                yW = (size.Width - b) / a;

                x0 = b;
                xH = a * size.Height + b;
            }
            if (x0 >= 0 && x0 <= size.Width)
            {

                ret[ind] = new Point((int)x0, 0);
                ind++;
            }
            if (xH >= 0 && xH <= size.Width)
            {
                ret[ind] = new Point((int)xH, size.Height - 1);
                ind++;
            }
            if (y0 >= 0 && y0 <= size.Height)
            {
                ret[ind] = new Point(0, (int)y0);
                ind++;
            }
            if (yW >= 0 && yW <= size.Height)
            {
                ret[ind] = new Point(size.Width - 1, (int)yW);
                ind++;
            }
            if (ind == 2)
            {
                return ret;
            }
            return ret;
        }


        PointF[] findCrossingD_rotate(LineF[] lines, Size size, Mat im)
        {
            var linesX = new List<LineF>();
            var linesY = new List<LineF>();
            var crossP = new List<PointF>();
            int lx = 0;
            foreach (var line in lines)
            {
                if (line != null)
                {
                    if (line.plan == PlanType.X)
                    {
                        lx++;
                    }
                }
            }
            if (lx / lines.Length == 0.5)
            {
                foreach (var line in lines)
                {
                    if (line != null)
                    {
                        if (line.plan == PlanType.X)
                        {
                            linesX.Add(line);
                        }
                        else if (line.plan == PlanType.Y)
                        {
                            linesY.Add(line);
                        }
                    }
                }
            }
            else
            {
                var f = from x in lines
                        orderby x.X
                        select x;
                var lines_s = f.ToArray();
                for (int i = 0; i < lines.Length / 2; i++)
                {
                    linesX.Add(lines_s[i]);
                    //Console.WriteLine("Line X a =" + lines_s[i].X + " b =  " + lines_s[i].Y);
                }

                for (int i = lines.Length / 2; i < lines.Length; i++)
                {
                    linesY.Add(lines_s[i]);
                    //Console.WriteLine("Line Y a =" + lines_s[i].X + " b =  " + lines_s[i].Y);
                }
            }

            foreach (var p in linesX)
            {
                var pts = find2Points(p, im.Size);
                if (pts != null)
                {
                    CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(255, 0, 0), 2);
                }
            }

            foreach (var p in linesY)
            {
                var pts = find2Points(p, im.Size);
                if (pts != null)
                {
                    CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(0, 255, 0), 2);
                }
            }
            var L = new LineF(1, 1);
            //Console.WriteLine("FIND CROSSING------------------");
            foreach (var p1 in linesX)
            {
                foreach (var p2 in linesY)
                {
                    var p = L.findCrossing(p1, p2);
                    var x = p.X;
                    var y = p.Y;

                    if (x >= 0 && x < size.Width)
                    {
                        if (y >= 0 && y < size.Height)
                        {
                            crossP.Add(new PointF(x, y));
                        }
                    }
                    //Console.WriteLine("X " + x + " Y " + y );

                }
            }
            if (crossP.Count != 0)
            {
                return crossP.ToArray();
            }
            return null;
        }

        List<PointF[]> findCrossingD_C(PointF[] lines, Size size)
        {
            var linesX = new List<PointF>();
            var linesY = new List<PointF>();
            var crossX = new List<PointF>();
            var crossY = new List<PointF>();
            foreach (var p in lines)
            {
                if (p.X >= 0)
                {
                    linesX.Add(p);
                }
                else
                {
                    linesY.Add(p);
                }
            }
            int i_x = 0;
            foreach (var p1 in linesX)
            {
                int i_y = 0;
                foreach (var p2 in linesY)
                {
                    var x = (p2.Y - p1.Y) / (p1.X - p2.X);
                    var y = p1.X * x + p1.Y;
                    if (x >= 0 && x < size.Width)
                    {
                        if (y >= 0 && y < size.Height)
                        {
                            if (i_x == i_y && i_x != (int)((linesX.Count - 1) / 2))
                            {
                                crossX.Add(new PointF(x, y));
                            }
                            if (i_x == linesX.Count - i_y - 1 && i_x != (int)((linesX.Count - 1) / 2))
                            {
                                crossY.Add(new PointF(x, y));
                            }

                        }
                    }
                    i_y++;
                }
                i_x++;
            }
            var ret = new List<PointF[]>();
            if (crossX.Count != 0 && crossY.Count != 0)
            {
                ret.Add(crossX.ToArray());
                ret.Add(crossY.ToArray());
                return ret;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points_in"></param>
        /// <param name="iter"></param>
        /// <param name="thickness">
        /// max area in percent? normal value 30...80
        /// </param>
        /// <param name="solid">
        /// contour or solid
        /// </param>
        /// <returns>
        /// limitPoints of Square
        /// </returns>
        int[] checkPoints(PointF[] points_in, int iter = 10, float angle = PI / 10, bool skip_first = false)
        {
            if (skip_first)
            {
                if (iter > 0)
                {
                    iter--;
                    var points_rot = rotateTransPointF(points_in, angle);
                    return checkPoints(points_rot, iter);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var ints = findMaxMin(points_in);
                var ps = new PointF[] { new PointF(points_in[ints[0]]),
                new PointF(points_in[ints[1]]),
                new PointF(points_in[ints[2]]),
                new PointF(points_in[ints[3]])};
                //
                bool maxX = (ps[0].X > ps[1].X) && (ps[0].X > ps[2].X) && (ps[0].X > ps[3].X);

                bool minX = (ps[1].X < ps[0].X) && (ps[1].X < ps[2].X) && (ps[1].X < ps[3].X);

                bool maxY = (ps[2].Y > ps[0].Y) && (ps[2].Y > ps[1].Y) && (ps[2].Y > ps[3].Y);

                bool minY = (ps[3].Y < ps[0].Y) && (ps[3].Y < ps[1].Y) && (ps[3].Y < ps[2].Y);

                bool checking = maxX && minX && maxY && minY;
                //Console.WriteLine("ITER N " + iter + " " + maxX + " " + minX + " " + maxY + " " + minY);
                //Console.WriteLine("Max x " + ps[0] + " Min x " + ps[1] + " Max Y " + ps[2] + " Min Y " + ps[3]);
                if (checking == true)
                {
                    return ints;
                }
                else
                {
                    if (iter > 0)
                    {
                        iter--;
                        var points_rot = rotateTransPointF(points_in, angle);
                        return checkPoints(points_rot, iter);
                    }
                    else
                    {
                        return null;
                    }
                }
            }


        }
        #endregion

        #region util
        Matrix4x4f assemblMatrix(Matrix<double> rot, Vertex3f trans)
        {
            var ret = new Matrix4x4f();
            for (int i = 0; i < rot.Cols; i++)
            {
                for (int j = 0; j < rot.Rows; j++)
                {
                    ret[(uint)i, (uint)j] = (float)rot[i, j];
                }
            }
            ret[3, 0] = 10.0f * trans.x; ret[3, 1] = 10.0f * trans.y; ret[3, 2] = 10.0f * trans.z;
            ret[0, 3] = 0; ret[1, 3] = 0; ret[2, 3] = 0; ret[3, 3] = (float)1;
            return ret;
        }
        Vertex3f toVertex3f(Mat mat)
        {
            var arr = mat.GetData();
            var v = (double[,])arr;
            return new Vertex3f((float)v[0, 0], (float)v[1, 0], (float)v[2, 0]);
        }
        System.Drawing.PointF[] toPointF(Mat corn)
        {

            var arr = corn.GetData();
            var flarr = (float[,,])arr;
            var points = new System.Drawing.PointF[flarr.Length / 2];
            //  Console.WriteLine(flarr.Length);
            for (int i = 0; i < flarr.Length / 2; i++)
            {
                points[i] = new System.Drawing.PointF(flarr[i, 0, 0], flarr[i, 0, 1]);
                // Console.WriteLine(flarr[i, 0, 0] + " " + flarr[i, 0, 1]);
            }
            return points;
        }
        System.Drawing.PointF[] toPointF(VectorOfPointF corn)
        {
            var points = new System.Drawing.PointF[corn.Size];
            //  Console.WriteLine(flarr.Length);
            for (int i = 0; i < corn.Size; i++)
            {
                points[i] = new System.Drawing.PointF(corn[i].X, corn[i].Y);
                // Console.WriteLine(flarr[i, 0, 0] + " " + flarr[i, 0, 1]);
            }
            return points;
        }
        double[,] convToDouble(float[,] f_mass)
        {
            var d_mass = new double[f_mass.Rows(), f_mass.Columns()];
            for (int i = 0; i < f_mass.Rows(); i++)
            {
                for (int j = 0; j < f_mass.Columns(); j++)
                {
                    d_mass[i, j] = (double)f_mass[i, j];
                }
            }
            return d_mass;
        }
        Point3d_GL[] transfPoints(Point3d_GL[] points, double[,] matr)
        {
            Point3d_GL[] points_trans = new Point3d_GL[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points_trans[i] = new Point3d_GL(MatrixMultiplication(matr, points[i].ToDouble()));
            }
            return points_trans;
        }
        double cos(double alpha)
        {
            return Math.Cos(alpha);
        }
        double sin(double alpha)
        {
            return Math.Sin(alpha);
        }
        double[,] RotZmatr(double alpha)
        {
            return new double[,] {
                { cos(alpha), -sin(alpha), 0,0 },
                { sin(alpha), cos(alpha), 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 } };
        }
        double[,] MatrixMultiplication(double[,] matrixA, Point3d_GL p)
        {
            double[,] matB = new double[,] { { p.x }, { p.y }, { p.z }, { 1 } };
            return MatrixMultiplication(matrixA, matB);
        }
        double[,] MatrixMultiplication(double[,] matrixA, double[,] matrixB)
        {
            if (matrixA.GetLength(1) != matrixB.GetLength(0))
            {
                return null;
            }

            var matrixC = new double[matrixA.GetLength(0), matrixB.GetLength(1)];


            for (var i = 0; i < matrixA.GetLength(0); i++)
            {
                for (var j = 0; j < matrixB.GetLength(1); j++)
                {

                    matrixC[i, j] = 0;
                    for (var k = 0; k < matrixB.GetLength(0); k++)
                    {

                        matrixC[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                }
            }
            return matrixC;
        }
        /// <summary>
        /// Matrix basis1->basis2
        /// </summary>
        /// <param name="basis1"></param>
        /// <param name="basis2"></param>
        /// <returns></returns>
        double[,] calcTransformMatr(double[,] basis1, double[,] basis2)
        {
            if (basis1 == null || basis2 == null)
            {
                return null;
            }
            if (basis1.GetLength(0) < 2 || basis2.GetLength(0) < 2)
            {
                return null;
            }

            int n = basis1.GetLength(1);
            print(n);
            var trans_Matr = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                var matrG = new double[n, n];
                var colG = new double[n];
                for (int x = 0; x < n; x++)
                {
                    for (int y = 0; y < n; y++)
                    {
                        matrG[x, y] = basis1[x, y];
                    }
                    colG[x] = basis2[x, i];
                }
                var row_GS = new CalcGauss(matrG, colG);
                var row = row_GS.getAnswer();
                for (int x = 0; x < n; x++)
                {
                    trans_Matr[i, x] = row[x];
                }
            }
            return trans_Matr;
        }
        double[,] calcTransformMatr(Point3d_GL[] basis1, Point3d_GL[] basis2)
        {
            if (basis1.Length != basis2.Length)
            {
                return null;
            }
            var basis1d = new double[basis1.Length, 4];
            var basis2d = new double[basis2.Length, 4];
            for (int i = 0; i < basis1.Length; i++)
            {
                basis1d[i, 0] = basis1[i].x; basis1d[i, 1] = basis1[i].y; basis1d[i, 2] = basis1[i].z; basis1d[i, 3] = 1;
                basis2d[i, 0] = basis2[i].x; basis2d[i, 1] = basis2[i].y; basis2d[i, 2] = basis2[i].z; basis2d[i, 3] = 1;
            }
            return calcTransformMatr(basis1d, basis2d);
        }
        Mat doubleToMat(double[][] inp, Size size)
        {
            Image<Gray, Byte> im_gray = new Image<Gray, byte>(size);
            for (int x = 0; x < inp.Length; x++)
            {
                //Console.WriteLine("n = "+inp[x][0] + "od = " + inp[x][1]);
                int y = (int)inp[x][1];
                if (y < 0)
                {
                    y = 0;
                }
                if (y >= size.Height)
                {
                    y = size.Height - 1;
                }
                im_gray.Data[y, x, 0] = 255;
            }
            return im_gray.Mat;
        }
        PointF[] doubleToPointF(double[][] inp)
        {
            var points = new List<PointF>();
            for (int i = 0; i < inp.Length; i++)
            {
                points.Add(new PointF((float)inp[i][0], (float)inp[i][1]));
            }
            return points.ToArray();
        }

        PointF[] doubleToPointF_real(double[][] inp)
        {
            var points = new List<PointF>();
            for (int i = 0; i < inp.Length; i++)
            {
                if (inp[i][2] > 0)
                {
                    points.Add(new PointF((float)inp[i][0], (float)inp[i][1]));
                }

            }
            return points.ToArray();
        }
        Matrix4x4f matrFromCam(Camera cam)
        {
            var Orient = new Vector3d_GL[] { cam.oX, cam.oY, cam.oZ };
            var retMatr = new Matrix4x4f();
            for (uint i = 0; i < 3; i++)
            {
                retMatr[i, 0] = (float)Orient[i].x;
                retMatr[i, 1] = (float)Orient[i].y;
                retMatr[i, 2] = (float)Orient[i].z;
            }
            retMatr[3, 0] = (float)cam.pos.x;
            retMatr[3, 1] = (float)cam.pos.y;
            retMatr[3, 2] = (float)cam.pos.z;
            return retMatr;
        }
        public void drawPointsF(Mat im, System.Drawing.PointF[] points, int r, int g, int b, int size = 1)
        {
            drawPoints(im, toPoint(points), r, g, b, size);
        }
        public void drawPoints(Mat im, Point[] points, int r, int g, int b, int size = 1)
        {
            int ind = 0;
            var color = new MCvScalar(b, g, r);//bgr
            if (points.Length != 0)
            {
                Console.WriteLine("LEN_ DRAW_P " + points.Length);

                foreach (var p in points)
                {
                    CvInvoke.Circle(im, p, size, color, -1);
                    ind++;
                }
            }
        }
        public void drawPoints(Mat im, PointF[] points, int r, int g, int b, int size = 1)
        {
            drawPoints(im, toPoint(points), r, g, b, size);
        }

        public void drawPoints(Mat im, System.Drawing.PointF[] points, int r, int g, int b, int size = 1)
        {
            drawPoints(im, toPoint(points), r, g, b, size);
        }
        public void drawTours(Mat im, Point[] points, int r, int g, int b, int size = 4)
        {
            int ind = 0;
            if (points.Length != 0)
            {
                foreach (var p in points)
                {
                    draw_tour(p, size, ind, im, r, g, b);
                    ind++;
                }
            }
        }
        public void drawTours(Mat im, PointF[] d_points, int r, int g, int b, int size = 4)
        {
            if (d_points != null)
            {

                int ind = 0;
                var points = toPoint(d_points);
                if (points.Length != 0)
                {
                    foreach (var p in points)
                    {
                        draw_tour(p, size, ind, im, r, g, b);
                        ind++;
                    }
                }
            }
        }
        public void drawTours(Mat im, PointF[][] d_points, int r, int g, int b, int size = 4)
        {
            int ind = 0;
            foreach (var d_ps in d_points)
            {
                var points = toPoint(d_ps);
                if (points.Length != 0)
                {
                    foreach (var p in points)
                    {
                        draw_tour(p, size, ind, im, r, g, b);
                        ind++;
                    }
                }
            }

        }
        public void draw_tour(Point p1, int size, int ind, Mat im, int r, int g, int b)//size - размер креста
        {

            var pt1 = new Point(p1.X + size, p1.Y);
            var pt2 = new Point(p1.X - size, p1.Y);
            var pt3 = new Point(p1.X, p1.Y + size);
            var pt4 = new Point(p1.X, p1.Y - size);
            var pt5 = new Point(p1.X + size, p1.Y - size);
            var color = new MCvScalar(b, g, r);//bgr
            CvInvoke.Line(im, pt1, pt2, color, 2);//
            CvInvoke.Line(im, pt3, pt4, color, 2);//krest
            CvInvoke.Line(im, new Point(im.Width / 2, 0), new Point(im.Width / 2, im.Height), color, 1);//
            CvInvoke.Line(im, new Point(0, im.Height / 2), new Point(im.Width, im.Height / 2), color, 1);//central krest
            CvInvoke.PutText(im, "P" + Convert.ToString(ind), pt5, FontFace.HersheyPlain, 1, color);
        }
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

        public Camera calcPos(PointF[] points, Size size, double fov, double side)
        {
            var Camera1 = new Camera(fov, size);
            var points1 = moveToCentr(points, size);
            var cl_P1 = new Point3d_GL(0, 0, 0);
            var cl_P2 = new Point3d_GL(side, 0, 0);
            var cl_P3 = new Point3d_GL(side, side, 0);
            var cl_P4 = new Point3d_GL(0, side, 0);
            var basis1 = new Point3d_GL[] { cl_P1, cl_P2, cl_P3, cl_P4 };
            var basis2 = new Point3d_GL[] { new Point3d_GL(points1[0], -100), new Point3d_GL(points1[1], -200), new Point3d_GL(points1[2], -100), new Point3d_GL(points1[3], -200) };
            var transf = calcTransformMatr(basis1, basis2);
            print(transf);
            Console.WriteLine("_________________^^^^^^");
            Camera1.calc_pos_all(points1[0], points1[1], points1[2], points1[3], cl_P1, cl_P2, cl_P3, cl_P4);
            /*Console.WriteLine("Ox " + Camera1.oX);
            Console.WriteLine("Oy " + Camera1.oY);
            Console.WriteLine("Oz " + Camera1.oZ);
            Console.WriteLine("Tr " + Camera1.pos);*/
            return Camera1;
        }
        PointF[] moveToCentr(PointF[] points, Size size)
        {
            var points1 = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points1[i] = new PointF(points[i].X - size.Width / 2,
                                        points[i].Y - size.Height / 2);
                points1[i].Y *= -1;
            }
            return points1;
        }
        Matrix4x4f AbcToMatrix(float a, float b, float c)
        {
            //Console.WriteLine("Z");
            //print(Matrix4x4f.RotatedZ(a));
            var rotZ = Matrix4x4f.RotatedZ(a);
            rotZ[0, 1] = -rotZ[0, 1];
            rotZ[1, 0] = -rotZ[1, 0];
            // print(rotZ);
            //Console.WriteLine("Y");
            var rotY = Matrix4x4f.RotatedY(b);
            rotY[2, 0] = -rotY[2, 0];
            rotY[0, 2] = -rotY[0, 2];
            //print(rotY);
            //Console.WriteLine("X");

            var rotX = Matrix4x4f.RotatedX(c);
            rotX[2, 1] = -rotX[2, 1];
            rotX[1, 2] = -rotX[1, 2];
            //print(rotX);
            return rotX * rotY * rotZ;
        }
        void calcRob()
        {
            float[] q = new float[8]{ toRad(-7.85f),
                    toRad(44.46f),
                    toRad(6.73f),
                   toRad(-109.17f),
                    toRad(13.97f),
                   toRad(-23.34f),
                    toRad(59.64f),
                    toRad(0)};
            //pos: 566.31 -30.62 220.70
            //or : 94.43  12.03  132.04
            float dbs = 360.0f;
            float dse = 420.0f;
            float dew = 400.0f;
            float dwf = 126.0f;
            float[] pos = { 0, 0, 0 };
            Manipulator Kuka = new Manipulator();

            float[] par = {  q[0], -PI / 2, 0, dbs,
                             q[1],  PI / 2, 0, 0,
                             q[2],  PI / 2, 0, dse,
                             q[3], -PI / 2, 0, 0,
                             q[4], -PI / 2, 0, dew,
                             q[5], PI / 2, 0, 0,
                             q[6], 0, 0, dwf,
                             0   ,       0, 0, 0 };

            float[] par_1 = {q[0], -PI/2, 0, dbs,
                             q[1], PI / 2, 0, 0,
                             q[2], PI / 2, 0, dse,
                             q[3], -PI / 2, 0, 0,
                             q[4], -PI / 2, 0, dew,
                             q[5], PI / 2, 0, 0,
                             q[6], 0, 0, dwf,
                             0   , 0, 0, 0 };
            Vector3d_GL pos1 = Kuka.calcPoz(par);
            Kuka.printMatrix(Kuka.flange_matr);
            Console.WriteLine("--------------");
            print(AbcToMatrix(94.43f, 12.03f, 132.04f));
            //print(AbcToMatrix(90f, 90f, 90f));
            Console.WriteLine(pos1);
        }
        #endregion

        #region openCVcalib



        TransRotZoom[] readTrz(string path)
        {
            var trzs = new List<TransRotZoom>();
            var files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                var filename = Path.GetFileName(files[i]);
                var trz = new TransRotZoom(filename);
                trz.dateTime = File.GetCreationTime(filename);
                if (trz != null)
                {
                    trzs.Add(trz);
                }
                
            }
            if (trzs.Count != 0)
            {
                var trzs1 = from f in trzs
                          orderby f.dateTime.Ticks
                          select f;
                return trzs1.ToArray();
            }
            return null;
        }

         void generateImagesFromAnotherFolder(string[] paths)
        {           
            var trzs_L = new List<TransRotZoom[]>();
            foreach(var path in paths)
            {
                var trzs = readTrz(path);
                trzs_L.Add(trzs);
                
            }
            if ( trzs_L.Count == 0)
            {
                return;
            }
            GL1.monitorsForGenerate = new int[] { 2, 3 };
            GL1.pathForSave  = "virtual_stereo\\test2";
            GL1.imageBoxesForSave = new ImageBox[] { imBox_mark1, imBox_mark2 };
            GL1.trzForSave = trzs_L;
            GL1.saveImagesLen = trzs_L[0].Length-1;
            Console.WriteLine("GL1.saveImagesLen " + GL1.saveImagesLen);        
        }

        void generateImagesFromAnotherFolderStereo(string[] paths)
        {
            var trzs_L = new List<TransRotZoom[]>();
            int ind = 0;
            var offtrz = new TransRotZoom(0, 0, 0, 0, 0, 0, -0.02);
            var trzs_prev = readTrz(paths[0]);
            foreach (var path in paths)
            {
                if(ind==0)
                {
                    var trzs = readTrz(path);
                    var trzs_clone = new TransRotZoom[trzs.Length];
                    for(int i=0; i< trzs.Length;i++)
                    {
                        trzs_clone[i] = trzs[i].minusDelta(offtrz);
                    }
                    trzs_L.Add(trzs_clone);
                }
                else
                {
                    var trzs = readTrz(path);
                    
                    if (trzs.Length != trzs_L[0].Length)
                    {
                        return;
                    }
                    var trzs_slave = new TransRotZoom[trzs.Length];
                    var trz_delta = trzs_prev[0] - trzs[0];
                    for(int i =0; i< trzs_L[0].Length;i++)
                    {
                        trzs_slave[i] = trzs_L[0][i].minusDelta(trz_delta);
                    }
                    trzs_L.Add(trzs_slave);
                }
                ind++;

            }
            if (trzs_L.Count == 0)
            {
                return;
            }

            GL1.monitorsForGenerate = new int[] { 2, 3 };
            GL1.pathForSave = "virtual_stereo\\test5";
            GL1.imageBoxesForSave = new ImageBox[] { imBox_mark1, imBox_mark2 };
            GL1.trzForSave = trzs_L;
            GL1.saveImagesLen = trzs_L[0].Length - 1;
            Console.WriteLine("GL1.saveImagesLen " + GL1.saveImagesLen);
        }
        void SaveMonitor(object obj)
        {
            var GL = (GraphicGL)obj;

            //Console.WriteLine("GL1.saveImagesLen " + GL.saveImagesLen);
            if (GL.saveImagesLen >= 0 && startGen ==1)
            {

                var monitors = GL.monitorsForGenerate;
                string newPath = GL.pathForSave;
                int ind_im = GL.saveImagesLen;
                var trzs_L = GL.trzForSave;
                int ind = 0;
                foreach(var trzs in trzs_L)
                {
                    var indMonit = monitors[ind];
                    //Console.WriteLine("indMonit" + indMonit);
                    var trzMonitor = GL.transRotZooms[indMonit];
                    trzMonitor.setTrz(trzs[ind_im]);
                    GL.transRotZooms[indMonit] = trzMonitor;
                    GL.SaveToFolder(newPath, indMonit);

                    var mat1 = remapDistImOpenCvCentr(GL1.matFromMonitor(indMonit), cameraDistortionCoeffs_dist);
                    saveImage(mat1, newPath, Path.Combine( "monitor_"+ indMonit, trzMonitor.ToString()+".png"));
                    ind++;
                }
                GL.saveImagesLen--;
                if(GL1.saveImagesLen==-1)
                {
                    startGen = 0;
                }
            }
            
        }
        Mat generateImage(Size size)
        {
            var data = new byte[size.Height, size.Width,1];
            int w = data.GetLength(1);
            int h = data.GetLength(0);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    data[j, i, 0] = 0;
                    if (i>100 && i<200)
                    {
                        if (j > 100 && j < 200)
                        {
                            data[j, i, 0] = 250;
                        }
                    }
                    
                    if(i%10==0)
                    {
                        data[j, i, 0] = 250;
                    }

                    if (j % 10 == 0)
                    {
                        data[j, i, 0] = 250;
                    }


                }
            }
            return (new Image<Gray, byte>(data)).Mat;
        }
        float[,] generateMap(Size size)
        {
            var data = new float[size.Height, size.Width];
            int w = data.GetLength(1);
            int h = data.GetLength(0);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if(i>190)
                    {
                        data[j, i] = i*1.5f;
                    }
                    else
                    {
                        data[j, i] = i;
                    }
                    
                    //data[j, i] = i+100;
                }
            }
            return data;
        }

        float[,] generateMapY(Size size)
        {
            var data = new float[size.Height, size.Width];
            int w = data.GetLength(1);
            int h = data.GetLength(0);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {                   
                    data[j, i] = j;
                }
            }
            return data;
        }
        Image<Gray, byte> mapToIm(float[,] map, PlanType planType, Mat mapy = null)
        {
            var dataMap = map;
            var dataMapy = new float[1, 1];
            if (mapy != null)
            {
                dataMapy = (float[,])mapy.GetData();
            }
            int w = dataMap.GetLength(1);
            int h = dataMap.GetLength(0);
            float max = dataMap.Max();
            float min = dataMap.Min();
            var dataIm = new float[h, w, 1];
            switch (planType)
            {
                case PlanType.X:
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var val = dataMap[j, i] - i;
                            dataIm[j, i, 0] = val;
                        }
                    }
                    break;

                case PlanType.Y:
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var val = dataMap[j, i] - j;
                            dataIm[j, i, 0] = val;
                        }
                    }
                    break;
                case PlanType.XY:
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var val = Math.Sqrt(Math.Pow(dataMap[j, i] - i, 2) + Math.Pow(dataMapy[j, i] - j, 2));
                            dataIm[j, i, 0] = (float)val;
                        }
                    }
                    break;
            }
            return new Image<Gray, byte>(normalyse(dataIm));
        }

        Image<Gray,byte> mapToIm(Mat map, PlanType planType,Mat mapy = null)
        {
            return mapToIm((float[,])map.GetData(), planType, mapy);
        }


        Mat mapToMat(float[,] map)
        {
            int w = map.GetLength(1);
            int h = map.GetLength(0);
            var im = new Image<Gray, float>(w, h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    im.Data[j, i,0] = map[j,i];
                }
            }
            return im.Mat;
        }
        byte[,,] normalyse(float[,,] mat)
        {
            int w = mat.GetLength(0);
            int h = mat.GetLength(1);
            int d = mat.GetLength(2);
            var koef = normalyseKoef(mat,0,255);
            var K = koef[0];
            var offs = koef[1];
           
            var data = new byte[w, h, d];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    for (int k = 0; k < d; k++)
                    {
                        var val = mat[i, j, k];
                        data[i, j, k] = (byte)((val - offs) * K);
                    }
                }
            }
            return data;
        }

        float[] normalyseKoef(float[,,] mat, float minN, float maxN)//{ k, offset}
        {
            float max = float.MinValue;
            float min = float.MaxValue;
            int w = mat.GetLength(0);
            int h = mat.GetLength(1);
            int d = mat.GetLength(2);

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    for (int k = 0; k < d; k++)
                    {
                        var val = mat[i, j, k];
                        if(val>max)
                        {
                            max = val;
                        }
                        if(val<min)
                        {
                            min = val;
                        }
                    }
                }
            }
            var delt = max - min;
            var deltN = maxN - minN;
            float K = 1;
            Console.WriteLine("delt " + delt);
            if (delt>0)
            {
                K = deltN / delt;
            }
            var offset = min * k;


            return new float[] { K, min };
        }

        MKeyPoint[] drawDescriptors(ref Mat mat)
        {
            var detector_ORB= new Emgu.CV.Features2D.ORBDetector(50);
            var detector_SURF = new Emgu.CV.Features2D.FastFeatureDetector();
            var kp = detector_ORB.Detect(mat);


            
           // matcher.Match()
            var desc_brief = new Emgu.CV.XFeatures2D.BriefDescriptorExtractor();
           //new VectorOfKeyPoint();
            var descrs = new Mat();
           

           // desc_brief.DetectAndCompute(mat, null, kp, descrs, false);
            //var mat_desc = new Mat();
            for(int i = 0; i< kp.Length; i++)
            {
                CvInvoke.DrawMarker(
                    mat,
                    new Point((int)kp[i].Point.X, (int)kp[i].Point.Y),
                    new MCvScalar(0, 0, 255),
                    MarkerTypes.Cross,
                    4,
                    1);

            }

            return kp;
        }
        Mat drawDescriptorsMatch(ref Mat mat1, ref Mat mat2)
        {
            var kps1 = new VectorOfKeyPoint();
            var desk1 = new Mat();
            var kps2 = new VectorOfKeyPoint();
            var desk2 = new Mat();
            var detector_ORB = new Emgu.CV.Features2D.ORBDetector();
            detector_ORB.DetectAndCompute(mat1, null, kps1, desk1, false);
            detector_ORB.DetectAndCompute(mat2, null, kps2, desk2, false);
            var matcher = new Emgu.CV.Features2D.BFMatcher(Emgu.CV.Features2D.DistanceType.L1,false);
            //var matcherFlann = new Emgu.CV.Features2D.
            var matches = new VectorOfDMatch();
            matcher.Match(desk1, desk2, matches);
            var mat3 = new Mat();
            try
            {
                Emgu.CV.Features2D.Features2DToolbox.DrawMatches(mat1, kps1, mat2, kps2, matches, mat3, new MCvScalar(255, 0, 0), new MCvScalar(0, 0, 255));
            }
            catch
            {

            }
            return mat3;
        }
        
        Mat epipolarTest(Mat matL, Mat matR)
        {
            // var _imL = new Mat();
            var imL = matL.ToImage<Gray, byte>();
            var imR = matR.ToImage<Gray, byte>();

            CvInvoke.Resize(imL, imL, new Size(600, 600));
            CvInvoke.Resize(imR, imR, new Size(600, 600));
            var minDisparity = 0;
            var numDisparities = 64;
            var blockSize = 8;
            var disp12MaxDiff = 1;
            var uniquenessRatio = 10;
            var speckleWindowSize = 100;
            var speckleRange = 8;
            var p1 = 8* imL.NumberOfChannels * blockSize * blockSize;
            var p2 = 32 * imL.NumberOfChannels * blockSize * blockSize;
            var stereo = new StereoSGBM(minDisparity, numDisparities, blockSize, p1, p2, disp12MaxDiff, 8, uniquenessRatio, speckleWindowSize, speckleRange,StereoSGBM.Mode.SGBM);
            var disp = new Mat();
            stereo.Compute(imL, imR, disp);
            //CvInvoke.Imshow("imL", imL);
           //CvInvoke.Imshow("imR", imR);
           // CvInvoke.Imshow("epipolar0", disp);
           // Console.WriteLine(disp.max)
           // CvInvoke.Normalize(disp, disp, 0, 255, NormType.MinMax);
           // CvInvoke.Imshow("epipolar", disp);
            return disp;
        }
        Mat drawDisparityMap(ref Mat mat1, ref Mat mat2)
        {
            var kps1 = new VectorOfKeyPoint();
            var desk1 = new Mat();
            var kps2 = new VectorOfKeyPoint();
            var desk2 = new Mat();
            var detector_ORB = new Emgu.CV.Features2D.ORBDetector();
           
            //CvInvoke.StereoRectify();
           // CvInvoke.ComputeCorrespondEpilines();
            detector_ORB.DetectAndCompute(mat1, null, kps1, desk1, false);
            detector_ORB.DetectAndCompute(mat2, null, kps2, desk2, false);
            var matcher = new Emgu.CV.Features2D.BFMatcher(Emgu.CV.Features2D.DistanceType.Hamming, true);
            var matches = new VectorOfDMatch();
            matcher.Match(desk1, desk2, matches);
            var mat3 = new Mat();
            Emgu.CV.Features2D.Features2DToolbox.DrawMatches(mat1, kps1, mat2, kps2, matches, mat3, new MCvScalar(255, 0, 0), new MCvScalar(0, 0, 255));
            return mat3;
        }
        Mat drawChessboard(Mat im, Size size)
        {
            var corn = new VectorOfPointF();
            var gray = im.ToImage<Gray, byte>();
            var ret = CvInvoke.FindChessboardCorners(gray, size, corn);
            Console.WriteLine("chess: " + ret+ " "+size.Width+" "+size.Height);
            CvInvoke.DrawChessboardCorners(im, size, corn, ret);
            return im;
        }

        void calibrateCamStereo(Frame[] frames, Size size)
        { 
            var objps = new List<MCvPoint3D32f[]>();
            // var corners = new List<System.Drawing.PointF[]>();

            var corners1 = new List<System.Drawing.PointF[]>();
            var corners2 = new List<System.Drawing.PointF[]>();

            var obp = new MCvPoint3D32f[size.Width * size.Height];
            int ind = 0;
            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    obp[ind] = new MCvPoint3D32f((float)i, (float)j, 0.0f);
                    //Console.WriteLine(i + " " + j);
                    ind++;
                }
            }
            //draw_tour(new Point((int)corn[0].X, (int)corn[0].Y), 3, 0, frame.im, 255, 0, 0);
            //CvInvoke.Imshow(frame.name, frame.im);
            //CvInvoke.WaitKey(500);
            var corn1 = new VectorOfPointF();
            var corn2 = new VectorOfPointF();
            foreach (var frame in frames)
            {
                imageBox2.Image = frame.im;
                imageBox1.Image = frame.im_sec;
                var gray1 = frame.im.ToImage<Gray, byte>();
                var ret1 = CvInvoke.FindChessboardCorners(gray1, size, corn1);
                var gray2 = frame.im_sec.ToImage<Gray, byte>();
                var ret2 = CvInvoke.FindChessboardCorners(gray2, size, corn2);
                if (ret1 && ret2)
                {
                    CvInvoke.CornerSubPix(gray1, corn1, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(100, 0.0001));
                    CvInvoke.CornerSubPix(gray2, corn2, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(100, 0.0001));
                    var _corn1 = toPointF(corn1);
                    var _corn2 = toPointF(corn2);
                    objps.Add(obp);
                    corners1.Add(_corn1);
                    corners2.Add(_corn2);
                }
                else
                {
                    Console.WriteLine("NOT:");
                    Console.WriteLine(frame.name);
                }
            }
            Console.WriteLine(objps.Count);
            Console.WriteLine(corners1.Count);
            Console.WriteLine(corners2.Count);
            var r = new Mat();
            var t = new Mat();
            var e = new Mat();
            var f = new Mat();

            var err = CvInvoke.StereoCalibrate
                (objps.ToArray(), 
                corners1.ToArray(),
                corners2.ToArray(),
                cameraMatrix1,
                cameraDistortionCoeffs1,
                cameraMatrix2,
                cameraDistortionCoeffs2,
                frames[0].im.Size,
                r, 
                t,
                e,
                f,
                CalibType.Default,
                new MCvTermCriteria(100, 0.0001)
                );

            Console.WriteLine("cameraMatrix1:  ");
            print(cameraMatrix1);
            Console.WriteLine("cameraDistortionCoeffs1:  ");
            print(cameraDistortionCoeffs1);

            Console.WriteLine("cameraMatrix2:  ");
            print(cameraMatrix2);
            Console.WriteLine("cameraDistortionCoeffs2:  ");
            print(cameraDistortionCoeffs2);

            Console.WriteLine("r:  ");
            print(r);
            Console.WriteLine("t:  ");
            print(t);
            Console.WriteLine("e:  ");
            print(e);
            Console.WriteLine("f:  ");
            print(f);
            Console.WriteLine("err:  ");
            print(err);

        }
        
        bool compChessCoords(Mat mat, ref MCvPoint3D32f[] obp, ref System.Drawing.PointF[] cornF , Size size)
        {
            var corn = new VectorOfPointF(cornF);
             obp = new MCvPoint3D32f[size.Width * size.Height];
            int ind = 0;
            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    obp[ind] = new MCvPoint3D32f((float)i, (float)j, 0.0f);
                    ind++;
                }
            }


            var gray = mat.ToImage<Gray, byte>();
            var ret = CvInvoke.FindChessboardCorners(gray, size, corn);
            if (ret == true)
            {
                CvInvoke.CornerSubPix(gray, corn, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(100, 0.0001));
            }
            else
            {
                Console.WriteLine("NOT:");
                //Console.WriteLine(frame.name);
            }
            cornF = corn.ToArray();
            return ret;
        }
        CameraCV calibrateCam(Frame[] frames, Size size)
        {
       
          
            var corn = new VectorOfPointF();
            Mat mtx = new Mat();
            Mat dist = new Mat();
            Mat[] tvecs = new Mat[0];
            Mat[] rvecs = new Mat[0];

            var objps = new List<MCvPoint3D32f[]>();
            // var corners = new List<System.Drawing.PointF[]>();

            var corners = new List<System.Drawing.PointF[]>();

            var obp = new MCvPoint3D32f[size.Width * size.Height];
            int ind = 0;
            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    obp[ind] = new MCvPoint3D32f((float)i, (float)j, 0.0f);
                    //Console.WriteLine(i + " " + j);
                    ind++;
                }
            }
            
            foreach (var frame in frames)
            {
                imageBox2.Image = frame.im;
                var gray = frame.im.ToImage<Gray, byte>();
                var ret = CvInvoke.FindChessboardCorners(gray, size, corn);
                if (ret == true)
                {
                    CvInvoke.CornerSubPix(gray, corn, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(100, 0.0001));
                    draw_tour(new Point((int)corn[0].X, (int)corn[0].Y), 3, 0, frame.im, 255, 0, 0);
                    //CvInvoke.Imshow(frame.name, frame.im);
                    //CvInvoke.WaitKey(500);
                    var corn2 = toPointF(corn);
                    objps.Add(obp);
                    corners.Add(corn2);
                }
                else
                {
                    Console.WriteLine("NOT:");
                    Console.WriteLine(frame.name);
                }
            }
            
            
            Matrix<double> rotateMatrix = new Matrix<double>(3, 3);
            Console.WriteLine(objps);
            Console.WriteLine(corners.Count);
            
            var err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, cameraMatrix, cameraDistortionCoeffs, CalibType.Default, new MCvTermCriteria(100, 0.0001), out rvecs, out tvecs);
           foreach(var tv in tvecs)
            {
                //print(tv);
            }
            Console.WriteLine("cameraMatrix1:  ");
            print(cameraMatrix);
            Console.WriteLine("cameraDistortionCoeffs1:  ");
            print(cameraDistortionCoeffs);
            print("_____________TVECS ^^^^^^");
            var newRoI = new Rectangle();

            matr = CvInvoke.GetOptimalNewCameraMatrix(cameraMatrix, cameraDistortionCoeffs, frames[0].im.Size, 1, frames[0].im.Size, ref newRoI);
            var mapx = new Mat();
            var mapy = new Mat();
            Console.WriteLine("cameraDistortionCoeffs:");
             print(cameraDistortionCoeffs);
            Console.WriteLine("cameraMatrix:");
            print(cameraMatrix);
            /*cameraDistortionCoeffs[0, 0] = -0.5 * Math.Pow(10, 0);
            cameraDistortionCoeffs[1, 0] = 0;
            cameraDistortionCoeffs[2, 0] = 0;
            cameraDistortionCoeffs[3, 0] = 0;
            cameraDistortionCoeffs[4, 0] = 0;*/
            computeDistortionMaps(ref mapx,ref mapy, cameraMatrix, cameraDistortionCoeffs, frames[0].im.Size);
            Console.WriteLine("||||||||||||");
            Console.WriteLine(mapx.Depth);
            print("matr:");
            print(matr);
           // Console.WriteLine("MAPX_________________");
           //print(mapx);
           // cameraDistortionCoeffs[0, 0] = 0.5;

            CvInvoke.InitUndistortRectifyMap(cameraMatrix, cameraDistortionCoeffs, null, matr, frames[0].im.Size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(frames[0].im, und_pic, mapx, mapy, Inter.Linear);
            imageBox1.Image = und_pic;
            //imageBox1.Image = mapx;
            //Console.WriteLine("MAPX_________________");
            //print(mapx);
            Console.WriteLine("err = " + err);

            //print(cameraMatrix);
           // Console.WriteLine("distor:----------------");
           // print(cameraDistortionCoeffs);
            



            for (int i = 0; i < corners.Count; i++)
            {
                CvInvoke.Rodrigues(rvecs[i], rotateMatrix);
                var tvec = toVertex3f(tvecs[i]);
                var mx = assemblMatrix(rotateMatrix, tvec);
                var invMx = mx.Inverse;

                //Console.WriteLine("INV-----------");
               // print(invMx);
               // Console.WriteLine("FRAME-------------");
                //print(frames[i]);
            }
            return new CameraCV(cameraMatrix, cameraDistortionCoeffs);
        }
        void calibrateFishEyeCam(Frame[] frames, Size size)
        {
            var objps = new VectorOfVectorOfPoint3D32F();
            // var corners = new List<System.Drawing.PointF[]>();

            var corners = new VectorOfVectorOfPointF();

            var obp = new VectorOfPoint3D32F();
            List<MCvPoint3D32f> listObp = new List<MCvPoint3D32f>();

            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    listObp.Add(new MCvPoint3D32f((float)i, (float)j, 0.0f));
                   // obp.Push(new MCvPoint3D32f[] { new MCvPoint3D32f((float)i, (float)j, 0.0f) });
                    //Console.WriteLine(i + " " + j);
                   
                }
            }
            obp.Push(listObp.ToArray());
            double[,,] imcorndata = new double[84, 10, 1];
            var indf = 0;
            
            foreach (var frame in frames)
            {
                
                var corn = new VectorOfPointF();
                imageBox2.Image = frame.im;
                var gray = frame.im.ToImage<Gray, byte>();
                var ret = CvInvoke.FindChessboardCorners(gray, size, corn);
                if (ret == true)
                {
                    
                    CvInvoke.CornerSubPix(gray, corn, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(100, 0.0001));
                    draw_tour(new Point((int)corn[0].X, (int)corn[0].Y), 3, 0, frame.im, 255, 0, 0);
                    //CvInvoke.Imshow(frame.name, frame.im);
                    //CvInvoke.WaitKey(500);
                    var corn2 = toPointF(corn);
                    int ind1 = 0;
                    for (int i = 0; i< corn2.Length;i++)
                    {
                        // matcorners.Data[indf, i, 0] = corn2[i].X;
                        //matcorners.Data[indf, i, 1] = corn2[i].Y;

                        imcorndata[ind1, indf, 0] = corn2[i].X; ind1++;
                        imcorndata[ind1, indf, 0] = corn2[i].Y; ind1++;
                        print(i);

                    }
                   
                    objps.Push(obp);
                    corners.Push(corn);
                    indf++;
                }
                else
                {
                    Console.WriteLine("NOT:");
                    Console.WriteLine(frame.name);
                }
            }
            
            var matcorners1 = new Image<Gray, double>(imcorndata);
            var m1 = matcorners1.Mat.Reshape(2,10);
            
            var matobjp = new Image<Bgr, double>(42, 10);

            int ind = 0;
            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    for (int k = 0; k < corners.Size; k++)
                    {
                        matobjp.Data[k,ind, 0] = i;
                        matobjp.Data[k,ind, 1] = j;
                    }

                    //Console.WriteLine(i + " " + j);
                    ind++;
                }
            }
            var m2 = matobjp.Mat;
            print(corners.Size);
           
            cameraDistortionCoeffs = new Matrix<double>(4,1);
            var m3 = m2.Reshape(3, 42);
            var m4 = m1.Reshape(2, 42);

            var K = new Mat();
            var D = new Mat();
            var tvec = new Mat();
            var rvec = new Mat();
            Fisheye.Calibrate(objps, corners, frames[0].im.Size, cameraMatrix, cameraDistortionCoeffs, rvec, tvec, Fisheye.CalibrationFlag.Default , new MCvTermCriteria(30, 0.1));
            //Fisheye.Calibrate(m2, m1, frames[0].im.Size, cameraMatrix, cameraDistortionCoeffs, rvecs, tvecs, Fisheye.CalibrationFlag.Default, new MCvTermCriteria(30, 0.001));
            print(tvec);
            print("_____________TVECS ^^^^^^");
            var matrP = new Matrix<double>(3, 3);
            var matrR = new Matrix<double>(3, 3);
            Fisheye.EstimateNewCameraMatrixForUndistorRectify (cameraMatrix, cameraDistortionCoeffs, frames[0].im.Size, matrR ,matrP);
            var mapx = new Mat();
            var mapy = new Mat();
            Console.WriteLine("cameraDistortionCoeffs:");
            print(cameraDistortionCoeffs);
            Console.WriteLine("cameraMatrix:");
            print(cameraMatrix);
            /*cameraDistortionCoeffs[0, 0] = -0.5 * Math.Pow(10, 0);
            cameraDistortionCoeffs[1, 0] = 0;
            cameraDistortionCoeffs[2, 0] = 0;
            cameraDistortionCoeffs[3, 0] = 0;
            cameraDistortionCoeffs[4, 0] = 0;*/
          //  computeDistortionMaps(ref mapx, ref mapy, cameraMatrix, cameraDistortionCoeffs, frames[0].im.Size);
            Console.WriteLine("||||||||||||");
            Console.WriteLine(mapx.Depth);
            print("matr:");

            print(matrP);
            // Console.WriteLine("MAPX_________________");
            //print(mapx);
            // cameraDistortionCoeffs[0, 0] = 0.5;
            Fisheye.InitUndistorRectifyMap(cameraMatrix, cameraDistortionCoeffs, matrR, matrP, frames[0].im.Size, DepthType.Cv32F, mapx, mapy);
            
            var und_pic = new Mat();
            CvInvoke.Remap(frames[0].im, und_pic, mapx, mapy, Inter.Linear);
            imageBox3.Image = und_pic;
            //imageBox1.Image = mapx;
            //Console.WriteLine("MAPX_________________");
            //print(mapx);
            //Console.WriteLine("err = " + err);

            //print(cameraMatrix);
            // Console.WriteLine("distor:----------------");
            // print(cameraDistortionCoeffs);




         /*   for (int i = 0; i < corners.Count; i++)
            {
                CvInvoke.Rodrigues(rvecs[i], rotateMatrix);
                var tvec = toVertex3f(tvecs[i]);
                var mx = assemblMatrix(rotateMatrix, tvec);
                var invMx = mx.Inverse;*/

                //Console.WriteLine("INV-----------");
                // print(invMx);
                // Console.WriteLine("FRAME-------------");
                //print(frames[i]);

          //  }

        }
        private void EmguCVUndistortFisheye(string path, Size patternSize)
        {
            string[] fileNames = Directory.GetFiles(path, "*.png");
          
            VectorOfVectorOfPoint3D32F objPoints = new VectorOfVectorOfPoint3D32F();
            VectorOfVectorOfPointF imagePoints = new VectorOfVectorOfPointF();
            foreach (string file in fileNames)
            {
                Mat img = CvInvoke.Imread(file, ImreadModes.Grayscale);
                CvInvoke.Imshow("input", img);
                VectorOfPointF corners = new VectorOfPointF(patternSize.Width * patternSize.Height);
                bool find = CvInvoke.FindChessboardCorners(img, patternSize, corners);
                if (find)
                {
                    MCvPoint3D32f[] points = new MCvPoint3D32f[patternSize.Width * patternSize.Height];
                    int loopIndex = 0;
                    for (int i = 0; i < patternSize.Height; i++)
                    {
                        for (int j = 0; j < patternSize.Width; j++)
                            points[loopIndex++] = new MCvPoint3D32f(j, i, 0);
                    }
                    objPoints.Push(new VectorOfPoint3D32F(points));
                    imagePoints.Push(corners);
                }
            }
            Size imageSize = new Size(1280, 1024);
            Mat K = new Mat();
            Mat D = new Mat();
            Mat rotation = new Mat();
            Mat translation = new Mat();
            print("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _");
            print(objPoints);
            print("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _");
            print(imagePoints);
            print("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _");
            Fisheye.Calibrate(
                objPoints,
                imagePoints,
                imageSize,
                K,
                D,
                rotation,
                translation,
                Fisheye.CalibrationFlag.CheckCond,
                new MCvTermCriteria(30, 0.1)
            );
            print("K:");
            print(K);
            print("D:");
            print(D);
            
            print("calib done");
            foreach (string file in fileNames)
            {
                Mat img = CvInvoke.Imread(file, ImreadModes.Grayscale);
                Mat output = img.Clone();
                Fisheye.UndistorImage(img, output, K, D);
                CvInvoke.Imshow("output", output);
            }
        }

        Mat remapDistIm(Mat mat, Matrix<double> matrixCamera, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();
            
            var roi = computeDistortionMaps(ref mapx, ref mapy, matrixCamera , matrixDistCoef , mat.Size);
            var invmap = remap(mapx, mapy, mat);
            //CvInvoke.Rectangle(invmap, roi, new MCvScalar(255, 0, 0));

            

            imBox_debug2.Image = invmap;
            //return  new Mat(invmap,roi);
            return invmap;
        }
        Mat remapUnDistIm(Mat mat, Matrix<double> matrixCamera, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();
            var size = mat.Size;
            matrixCamera[0, 2] = size.Width / 2;
            matrixCamera[1, 2] = size.Height / 2;
            CvInvoke.InitUndistortRectifyMap(matrixCamera, matrixDistCoef, null, matr, mat.Size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(mat, und_pic, mapx, mapy, Inter.Linear);
            imBox_debug2.Image = mat;
            return und_pic;
        }
        Mat remapDistImOpenCvCentr(Mat mat, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();
            var size = mat.Size;

            var reversDistor = new Matrix<double>(5, 1);
            for (int i = 0; i < 5; i++)
            {
                reversDistor[i, 0] = -matrixDistCoef[i, 0];
            }
            double fov = 53;
            //_x = _z * Math.Tan(toRad(53 / 2))
            var fxc = size.Width / 2;
            var fyc = size.Height / 2;
            var f = size.Width/(2* Math.Tan(toRad((float)(fov / 2))));
            var matrixData = new double[3,3] { {f,0,fxc}, {0,f,fyc }, {0,0,1 } };
            var matrixData_T= matrixData.Transpose();
            var matrixCamera = new Matrix<double>(matrixData);
           // print(matrixCamera);
            CvInvoke.InitUndistortRectifyMap(matrixCamera, reversDistor, null, matr, size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(mat, und_pic, mapx, mapy, Inter.Linear);
            imBox_debug2.Image = mat;
            return und_pic;
        }
        Mat remapDistImOpenCv(Mat mat, Matrix<double> matrixCamera, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();
            var size = mat.Size;
            var reversDistor = new Matrix<double>(5, 1);
            for(int i=0; i<5; i++)
            {
                reversDistor[i, 0] = -matrixDistCoef[i, 0];
            }
            CvInvoke.InitUndistortRectifyMap(matrixCamera, reversDistor, null, matr, size, DepthType.Cv32F, mapx, mapy);
            var und_pic = new Mat();
            CvInvoke.Remap(mat, und_pic, mapx, mapy, Inter.Linear);
            imBox_debug2.Image = mat;
            return und_pic;
        }
        Mat Minus(Mat mat1, Mat mat2)
        {
            var data1 = (byte[,,])mat1.GetData();
            var w1 = data1.GetLength(0);
            var h1 = data1.GetLength(1);
            var data2 = (byte[,,])mat2.GetData();
            var w2 = data2.GetLength(0);
            var h2 = data2.GetLength(1);
            var w = Math.Min(w1, w2);
            var h = Math.Min(h1, h2);

            var data = new byte[w, h,1];
            Console.WriteLine("w h " + w + " " + h);
            var cut_map1 = new Mat(mat1, new Rectangle(0, 0, h, w));
            var cut_map2 = new Mat(mat2, new Rectangle(0, 0, h, w));
            /* for (int i=0; i< w; i++)
             {
                 for (int j = 0; j < h; j++)
                 {
                     var val = data1[i, j,0] - data2[i, j,0] + 127;
                     if(val>255)
                     {
                         val = 255;
                     }
                     if (val<0)
                     {
                         val = 0;
                     }
                     data[i, j,0] =  (byte)val;
                 }
             }*/
            //return new Image<Gray,byte>(data).Mat;
            return cut_map1 - cut_map2;
        }

        Mat invRemap(float[,] mapx, float[,] mapy, Mat mat)
        {
            return remap(
                mapToMat(inverseMap(mapx, PlanType.X)),
                 mapToMat(inverseMap(mapy, PlanType.Y)),
                 mat);
        }
        Mat invRemap(Mat mapx, Mat mapy, Mat mat)
        {
            return invRemap(
                  mapToFloat(mapx),
                  mapToFloat(mapy),
                  mat);
        }
        float[,] mapToFloat(Mat map)
        {
            return (float[,])map.GetData();
        }
        
        Size findRemapSize(float[,] mapx, float[,] mapy)
        {
            var x = mapx.Max();// findMaxX(mapx);
            var y = mapy.Max();// findMaxY(mapy);
            Console.WriteLine("FLOAT SIZE: " + x + " " + y);
            var ix = (int)Math.Round(x, 0) + 4;
            var iy = (int)Math.Round(y, 0) + 4;
            return new Size(ix, iy);
        }


        float[,] inverseMap(float[,] map,PlanType planType)
        {
            int w = map.GetLength(1);
            int h = map.GetLength(0);
            var inv_map = new float[h, w];

            switch (planType)
            {
                case PlanType.X:
                    var deltE = w;
                    var deltI = (map.Max() - map.Min());
                    var k = deltI / deltE;
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var del = (map[j, i]- i)/k ;
                            inv_map[j, i] = i - del;
                        }
                    }
                    break;
                case PlanType.Y:
                    deltE = h;
                    deltI = (map.Max() - map.Min());
                    k = deltI / deltE;
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var del = (map[j, i]  - j)/k;
                            inv_map[j, i] = j - del;
                        }
                    }
                    break;
            }
            return inv_map;
        }

        float[,,] compRemap(float[,] mapx, float[,] mapy, Mat mat)
        {
            var size = findRemapSize(mapx, mapy);
            var im = mat.ToImage<Bgr, byte>();
            Console.WriteLine("NEW SIZE: " + size.Width + " " + size.Height);
            var data = new float[size.Height, size.Width, 3];
            Console.WriteLine(data.GetLength(0) + " " + data.GetLength(1) + 
                " " + mapx.GetLength(0) + " " + mapx.GetLength(1) + 
                " " + im.Data.GetLength(0) + " " + im.Data.GetLength(1) + " ");
            var size_p = im.Size;
            for (int i = 1; i < size_p.Width-2; i++)
            {
                for (int j = 1; j < size_p.Height-2; j++)
                {
                    var x = mapx[j, i];
                    var y = mapy[j, i];
                    var w = 0f;
                    var h = 0f;
                    if (j == 0)
                    {
                        h = Math.Abs(mapy[j, i] - mapy[j + 1, i]);
                    }
                    else
                    {
                        h = Math.Abs(mapy[j, i] - mapy[j - 1, i]);
                    }
                    if (i == 0)
                    {
                        w = Math.Abs(mapx[j, i] - mapx[j, i + 1]);
                    }
                    else
                    {
                        w = Math.Abs(mapx[j, i] - mapx[j, i - 1]);
                    }
                    var sq1 = new Square(x, y, w,  h);
                    //Console.WriteLine("wh " + w + " " + h);
                    var ix = (int)Math.Round(x, 0);
                    var iy = (int)Math.Round(y, 0);
                    
                    for (int _i = ix - 1; _i <= ix + 1; _i++)
                    {
                        for (int _j = iy - 1; _j <= iy + 1; _j++)
                        {
                            var sq2 = new Square(_i, _j, 1, 1);
                            var intens = compCrossArea(sq1, sq2) ;
                            //Console.WriteLine(" _i " + _i+ " _j " + _j + " i " + i + " j " + j);
                            data[_j, _i, 0] += intens * im.Data[j, i, 0];
                            data[_j, _i, 1] += intens * im.Data[j, i, 1];
                            data[_j, _i, 2] += intens * im.Data[j, i, 2];
                            
                        }
                    }

                }
            }
            return data;
        }

        float compCrossArea(Square s1, Square s2)
        {
            var dx = compDelt(s1.w, s2.w, Math.Abs(s1.x - s2.x));
            var dy = compDelt(s1.h, s2.h, Math.Abs(s1.y - s2.y));
            return Math.Abs(dx * dy);
        }
        float compDelt(float w1, float w2, float dx)
        {
            float dw = 0;
            if(w2<w1)
            {
                var lam = w1;
                w1 = w2;
                w2 = lam;
            }
            if(dx <(w1+w2)/2)//условие пересечения
            {
                if(dx>w2/2)
                {
                    if ((w1 / 2 + dx) <= w2 / 2)
                    {
                        dw = w1;
                        return dw;
                    }
                    else
                    {
                        dw = (w1 + w2) / 2 - dx;
                        return dw;
                    }
                }
                else
                {
                    if(w1<=w2/2)
                    {
                        dw = (w1 + w2) / 2 - dx;
                        return dw;
                    }
                    else 
                    {
                        if((w1/2+dx)>=w2/2)
                        {
                            dw = w2 / 2 + (w1 / 2 - dx);
                            return dw;
                        }
                        else
                        {
                            dw = w1;
                            return dw;
                        }
                    }
                }
            }
            else
            {
                return dw;
            }
        }

        byte[,,] toByte(float[,,] arr)
        {
            var byte_arr = new byte[arr.GetLength(0), arr.GetLength(1), arr.GetLength(2)];
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    for (int k = 0; k < arr.GetLength(2); k++)
                    {
                        if(arr[i, j, k]>255)
                        {
                            byte_arr[i, j, k] = 255;
                        }

                        else
                        {
                            byte_arr[i, j, k] = (byte)Math.Round(arr[i, j, k], 0);
                        }
                       
                    }
                }
            }
            return byte_arr;
        }

        byte[,,] toByteGray(float[,,] arr)
        {
            var byte_arr = new byte[arr.GetLength(0), arr.GetLength(1), 1];
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {

                        if (arr[i, j, 0] > 255)
                        {
                            byte_arr[i, j, 0] = 255;
                        }

                        else
                        {
                            byte_arr[i, j, 0] = (byte)Math.Round(arr[i, j, 0], 0);
                        }

                }
            }
            return byte_arr;
        }
        Mat remap (Mat _mapx, Mat _mapy, Mat mat)
        {
          
            var mapx = compUnsignedMap((float[,])_mapx.GetData(), PlanType.X);
            var mapy = compUnsignedMap((float[,])_mapy.GetData(), PlanType.Y);

            mapx = (float[,])_mapx.GetData();
            mapy = (float[,])_mapy.GetData();
            //  print("________________________________");
            // print(mapy);
            // print("________________________________");
            var data = compRemap(mapx, mapy, mat);
            
            var color = 0;
            try
            {
                color = mat.GetData().GetLength(2);
                Console.WriteLine("color" + color);
            }
            catch
            {
                color = 1;
               print ("color " + color);
            }
               
            if ( color == 1)
            {
                var im = new Image<Gray, byte>(toByteGray(data));

                return im.Mat;
            }
            else if (color == 3)
            {
                var im = new Image<Bgr, byte>(toByte(data));
                return im.Mat;
            }


            //print(mapx);
            // print("________________________________");
            //print(mapy);
            return null;
        }

        float[,] compUnsignedMap(float[,] map, PlanType planType)
        {
            float min = float.MaxValue;
            float[,] rmap = new float[map.GetLength(0), map.GetLength(1)];
            if (planType == PlanType.X)
            {
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    if (min > map[i, 0])
                    {
                        min = map[i, 0];
                    }
                }
                min -= 1;
            }
            else if (planType == PlanType.Y)
            {
                for (int i = 0; i < map.GetLength(1); i++)
                {
                    if (min > map[0, i])
                    {
                        min = map[0, i];
                    }
                }
                min -= 1;
            }
            if(min<0)
            {
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        rmap[i, j] = map[i, j] - min;

                    }
                }
            }
            else
            {
                return map;
            }
            
            return rmap;
        }

        

        PointF calcDistorcPix(int dim, int xd, int yd, double xc, double yc, Matrix<double> distCoefs)
        {


            var K1 =  distCoefs[0, 0];
            var K2 =  distCoefs[1, 0];
            var P1 = distCoefs[2, 0];
            var P2 = distCoefs[3, 0];
            var K3 =  distCoefs[4, 0];

            var delt = new PointF((float)((double)xd-xc), (float)((double)yd - yc));
            var r = (double)delt.norm;
            var r2 = Math.Pow(r, 2);
            var r4 = Math.Pow(r, 4);
            var r6 = Math.Pow(r, 6);
            
            var delx = xd - xc;
            var dely = yd - yc;
            
            var xu = xd + delx * (K1 * r2 + K2 * r4 + K3 * r6)+ P1 * (r2 + 2 * Math.Pow(delx, 2)) + 2 * P2 * delx * dely;
            var yu = yd + dely * (K1 * r2 + K2 * r4 + K3 * r6) + 2 * P1 * delx * dely +   P2 * (r2 + 2 * Math.Pow(delx, 2));

            
            return new PointF(xu, yu);
        }
        PointF calcDistorcPix_BC(int dim, int _xd, int _yd, double _xc, double _yc, Matrix<double> distCoefs)
        {

            var K1 = distCoefs[0, 0];
            var K2 =  distCoefs[1, 0];
            var P1 = distCoefs[2, 0];
            var P2 = distCoefs[3, 0];
            var K3 = distCoefs[4, 0];
            float xd = (float)_xd / dim;
            float yd = (float)_yd / dim;

            float xc = (float)_xc / dim;
            float yc = (float)_yc / dim;
            var delt = new PointF((float)((double)xd - xc), (float)((double)yd - yc));
            var r = (double)delt.norm;
            var r2 = Math.Pow(r, 2);
            var r4 = Math.Pow(r, 4);
            var r6 = Math.Pow(r, 6);

            var delx = xd - xc;
            var dely = yd - yc;


            var _xu = xc + delx/(1+K1 * r2 + K2 * r4 + K3 * r6);
            var _yu = yc + dely/(1+K1 * r2 + K2 * r4 + K3 * r6);

           // var xu = xc + delx *(1 + K1 * r2 + K2 * r4 + K3 * r6);
           // var yu = yc + dely * (1 + K1 * r2 + K2 * r4 + K3 * r6);

            return new PointF(_xu* dim, _yu * dim);
        }

        float findMaxMin(float[,] map, int col,PlanType planType,int maxminF)//min - 0; max - 1;
        {
            float maxmin = 0; 
            int b_i_ind = 0;
            int b_j_ind = 0;
            int e_i_ind = 0;
            int e_j_ind = 0;
            if(planType == PlanType.X)
            {
                b_i_ind = 0;
                e_i_ind = map.GetLength(0) - 1;

                b_j_ind = col;     
                e_j_ind = col;
            }
            else if(planType == PlanType.Y)
            {
                b_i_ind = col;
                e_i_ind = col;

                b_j_ind = 0;
                e_j_ind = map.GetLength(1) - 1;
            }
            if (maxminF == 0)
            {
                maxmin = float.MaxValue;
            }
            else
            {
                maxmin = float.MinValue;
            }
            
            for (int i= b_i_ind; i<= e_i_ind; i++)
            {
                for (int j = b_j_ind; j <= e_j_ind; j++)
                {
                    if (maxminF == 0)
                    {
                        if (map[i, j] < maxmin)
                        {
                            maxmin = map[i, j];
                        }
                    }
                    else
                    {
                        if (map[i, j] > maxmin)
                        {
                            maxmin = map[i, j];
                        }
                    }

                    
                }
            }
            return maxmin;
        }
        Rectangle compROI(Mat mapx, Mat mapy)
        {
            return compROI((float[,])mapx.GetData(), (float[,])mapy.GetData());
        }
        Rectangle compROI(float[,] mapx, float[,] mapy)
        {
            float L = findMaxMin(mapx, 0, PlanType.X, 1);
            float R = findMaxMin(mapx, mapx.GetLength(1)-1, PlanType.X, 0);

            float U = findMaxMin(mapy, 0, PlanType.Y, 1);
            float D = findMaxMin(mapy, mapx.GetLength(0)-1, PlanType.Y, 0);

            //Console.WriteLine("L R U D " + L + " " + R + " " + U + " " + D + " ");
            return new Rectangle((int)L, (int)U, (int)(R-L), (int)(D - U)); 
        }
        Rectangle newRoi(Size size, double xc, double yc, Matrix<double> distCoefs, Func<int,int,int,double,double,Matrix<double>,PointF> calcDistPix)
        {
            var p1 = calcDistPix(size.Width,0, 0, xc, yc, distCoefs);
            var p2 = calcDistPix(size.Width, size.Width, 0, xc, yc, distCoefs);
            var p3 = calcDistPix(size.Width, size.Width, size.Height, xc, yc, distCoefs);
            var p4 = calcDistPix(size.Width, 0, size.Height, xc, yc, distCoefs);
            print(p1 + " " + p2 + " " + p3 + " " + p4 + " ");
            return RoiFrom4Points(p1, p2, p3, p4);
        }
        Rectangle RoiFrom4Points(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            int x=0, y=0, xW=1, yH=1;
            if (p1.X >= p4.X) { x = (int)p1.X; } else { x = (int)p4.X; }
            if (p1.Y >= p2.Y) { x = (int)p1.Y; } else { x = (int)p2.Y; }

            if (p3.X >= p2.X) { x = (int)p2.X; } else { x = (int)p3.X; }
            if (p3.Y >= p4.Y) { x = (int)p4.Y; } else { x = (int)p3.Y; }
            return new Rectangle(x, y, (xW - x), (yH - y));
        }
        Rectangle computeDistortionMaps(ref Mat _mapx, ref Mat _mapy, Matrix<double> cameraMatr, Matrix<double> distCoefs, Size size)
        {
           
            Matrix<float> mapx = new Matrix<float>(size.Height, size.Width);
            Matrix<float> mapy = new Matrix<float>(size.Height, size.Width);
            double xc = cameraMatr[0, 2];
            double yc = cameraMatr[1, 2];
            xc = size.Width/2;
            yc = size.Height/2;
            Console.WriteLine("---xcyc-- " + xc + " " + yc);
           // print(cameraMatr);
            //print(mapx);
            for (int i=0; i<size.Height;i++)
            {
                for (int j = 0; j < size.Width; j++)
                {

                    var p = calcDistorcPix_BC(size.Width, j, i, xc, yc, distCoefs);
                    mapx[i, j] = p.X;
                    mapy[i, j] = p.Y;
                    //Console.WriteLine(i + " " + j);
                }
            }

            _mapx = mapx.Mat;
            _mapy = mapy.Mat;
            return compROI(_mapx,_mapy);

        }
        void distortFolder(string path)
        {
            
            var frms = loadImages_test(path);
            var distPath = Path.Combine(path, "distort");

            var fr1 = from f in frms
                      orderby f.dateTime.Ticks
                      select f;
            var vfrs = fr1.ToList();
            int ind = 0;
            foreach (var fr in vfrs)
            {
                var matD = remapDistIm(fr.im, cameraMatrix, cameraDistortionCoeffs);            
                saveImage(matD, distPath, ind+" "+fr.name);
                ind++;
            }
        }

        #endregion

        #region print
        void print(Image<Gray, float> matr)
        {
            var flarr = matr.Data;
            var ch = matr.NumberOfChannels;
            for (int i = 0; i < matr.Rows; i++)
            {
                for (int j = 0; j < matr.Cols; j++)
                {
                    for (int k = 0; k < ch; k++)
                    {
                        Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                    }
                }
                Console.WriteLine(" ");
            }
        }
        void print(Image<Bgr,float> matr)
        {
            var flarr = matr.Data;
            var ch = 3;
            for (int i = 0; i < matr.Rows; i++)
            {
                for (int j = 0; j < matr.Cols; j++)
                {
                    for (int k = 0; k < ch; k++)
                    {
                        Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                    }
                }
                Console.WriteLine(" ");
            }
        }
        void print(VectorOfVectorOfPoint3D32F matr)
        {
            for (int i = 0; i < matr.Size; i++)
            {
                for (int y = 0; y < matr[i].Size; y++)
                {
                    Console.Write(matr[i][y].X + " "+ matr[i][y].Y+" "+ matr[i][y].Z+"; ");
                }
                Console.WriteLine("; ");
            }
            Console.WriteLine(" ");
        }
        void print(VectorOfVectorOfPointF matr)
        {
            for (int i = 0; i < matr.Size; i++)
            {
                for (int y = 0; y < matr[i].Size; y++)
                {
                    Console.Write(matr[i][y].X + " " + matr[i][y].Y + "; ");
                }
                Console.WriteLine("; ");
            }
            Console.WriteLine(" ");
        }
        void print(double[] matr)
        {
            for (int i = 0; i < matr.Length; i++)
            {
                Console.Write(matr[i] + " ");
            }
            Console.WriteLine(" ");
        }
        void print(Frame frame, int cols = 3)
        {
            var name = frame.name;
            var name_t = name.Trim();
            var name_s = name_t.Split(' ');
            for (int i = 0; i < name_s.Length / cols; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(name_s[i * cols + j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        void print(double[,] matr)
        {
            for (int i = 0; i < matr.GetColumn(0).Length; i++)
            {
                for (int j = 0; j < matr.GetRow(0).Length; j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        void print(float[,,] matr)
        {
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                for (int j = 0; j < matr.GetLength(1); j++)
                {
                    for (int k = 0; k < matr.GetLength(2); k++)
                    {
                        Console.Write(matr[i, j, k] + " ");
                    }
                }
                Console.WriteLine(" ");
            }
        }
        void print(float[,] matr)
        {
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                for (int j = 0; j < matr.GetLength(1); j++)
                {
                    Console.Write(matr[i, j] + " "); 
                }
                Console.WriteLine(" ");
            }
        }
        void print(Point3d_GL[][] matr)
        {
            for (int i = 0; i < matr.GetColumn(0).Length; i++)
            {
                for (int j = 0; j < matr.GetRow(0).Length; j++)
                {
                    Console.Write(matr[i][j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        void print(Matrix<double> matr)
        {
            Console.WriteLine("cols x rows: " + matr.Cols+ " x " + matr.Rows);
            if (matr.Cols!=1)
            {
                for (int i = 0; i < matr.Cols; i++)
                {
                    for (int j = 0; j < matr.Rows; j++)
                    {
                        Console.Write(matr[i, j] + " ");
                    }
                    Console.WriteLine(" ");
                }
            }
            else
            {
                for (int i = 0; i < matr.Rows; i++)
                {
                    Console.Write(matr[i,0] + " ");
                }
                Console.WriteLine(" ");
            }

        }
        void print(Matrix4x4f matr)
        {
            for (uint i = 0; i < 4; i++)
            {
                for (uint j = 0; j < 4; j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        void print(Matrix3x3f matr)
        {
            for (uint i = 0; i < 3; i++)
            {
                for (uint j = 0; j < 3; j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        /*
        void print(Mat mat)
        {
            Console.WriteLine("cols x rows: " + mat.Cols + " x " + mat.Rows);
            var arr = mat.GetData();
            double[,] flarr = new double[1, 1];
           // Console.WriteLine("st");
            try
            {
                
                flarr = (double[,])arr;
                //Console.WriteLine("fl");
            }
            catch (System.InvalidCastException)
            {
                try
                {
                    flarr = convToDouble((float[,])arr);
                }
                catch (System.InvalidCastException)
                {

                }
                    //Console.WriteLine("db");
            }
            
            for (int i = 0; i < mat.Rows; i++)
            {
                for (int j = 0; j < mat.Cols; j++)
                {
                    Console.Write(Math.Round(flarr[i, j],3) + " ");

                }
                Console.WriteLine(" ");
            }
        }
        */
        void print(Mat mat)
        {
            Console.WriteLine("cols x rows: " + mat.Cols + " x " + mat.Rows);
            var arr = mat.GetData();
            var ch = mat.NumberOfChannels;
            if(ch>1)
            {
                if (mat.Depth == DepthType.Cv64F)
                {
                    var flarr = (double[,,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            for (int k = 0; k < ch; k++)
                            {
                                Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                            }
                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv32F)
                {
                    var flarr = (float[,,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            for (int k = 0; k < ch; k++)
                            {
                                Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                            }
                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
            }
            else
            {
                if (mat.Depth == DepthType.Cv64F)
                {
                    var flarr = (double[,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {

                                Console.Write(Math.Round(flarr[i, j], 3) + " ");
                            
                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv32F)
                {
                    var flarr = (float[,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                                Console.Write(Math.Round(flarr[i, j], 3) + " ");
                          
                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
            }
            


            
        }
        void print(Vertex3f v)
        {
            Console.WriteLine(v.x + " " + v.y + " " + v.z);
        }
        void print(string s)
        {
            Console.WriteLine(s);
        }
        void print(int s)
        {
            Console.WriteLine(s);
        }
        void print(double s)
        {
            Console.WriteLine(s);
        }
        #endregion

        #region fov
        void paint3dpointsRGB(List<Point3d_GL[]> inp_points)
        {
            //addPointMesh(inp_points[0], 1.0f,0.0f, 0.0f);
            //addPointMesh(inp_points[1], 0.0f,1.0f, 0.0f);
            //addPointMesh(inp_points[2], 0.0f, 0.0f, 1.0f);
            GL1.addPointMesh(inp_points[3], 0.9f);
        }
        double[] calc_preFov(List<Frame> fr)
        {
            var points1 = finPointFsFromIm_kalib(fr[0].im, 60);
            var points2 = finPointFsFromIm_kalib(fr[1].im, 60);
            Console.WriteLine(fr.Count);
            Console.WriteLine(points1[0].Length);
            Console.WriteLine(points2[0].Length);
            double wmm1 = Math.Sqrt(2) * (6 * fr[0].size_mark / 7);
            double wpix1 = points1[0][5].X - points1[0][0].X;


            double wmm2 = Math.Sqrt(2) * (4 * fr[0].size_mark / 7);
            double wpix2 = points1[0][4].X - points1[0][1].X;


            double wmm3 = Math.Sqrt(2) * (6 * fr[1].size_mark / 7);
            double wpix3 = points2[0][5].X - points2[0][0].X;
            double delta = fr[1].pos_rob.z - fr[0].pos_rob.z;




            Console.WriteLine("1 = " + wmm1 + " " + wpix1);
            Console.WriteLine("2 = " + wmm2 + " " + wpix2);
            Console.WriteLine("3 = " + wmm3 + " " + wpix3);
            Console.WriteLine("d = " + delta);
            return new double[] { wmm1, wpix1, wmm2, wpix2, wmm3, wpix3, delta };
            //ka,f,h
            //findOneVarDec_new(new double[] { 0.6, wpix1/3, wmm1/3 }, new double[] { 3, 2000, 200 }, consts, calc_F_simple);
        }
        double[] calc_preFov_all(List<Frame> fr)
        {
            var points1 = finPointFsFromIm_kalib(fr[0].im, 60);

            int len = fr.Count + 1;
            var consts = new double[(3 * len) - 2];
            double[] w_mm = new double[len];
            double[] w_pix = new double[len];
            double[] delta = new double[len];

            w_mm[0] = Math.Sqrt(2) * (6 * fr[0].size_mark / 7);
            w_pix[0] = points1[0][5].X - points1[0][0].X;


            w_mm[1] = Math.Sqrt(2) * (4 * fr[0].size_mark / 7);
            w_pix[1] = points1[0][4].X - points1[0][1].X;

            int j = 0;
            consts[j] = w_mm[0]; j++;
            consts[j] = w_pix[0]; j++;
            consts[j] = w_mm[1]; j++;
            consts[j] = w_pix[1]; j++;
            for (int i = 2; i < fr.Count + 1; i++)
            {
                var points2 = finPointFsFromIm_kalib(fr[i - 1].im, 60);
                w_mm[i] = Math.Sqrt(2) * (6 * fr[i - 1].size_mark / 7);
                consts[j] = w_mm[i]; j++;
                w_pix[i] = points2[0][5].X - points2[0][0].X;
                consts[j] = w_pix[i]; j++;
                delta[i] = fr[i - 1].pos_rob.z - fr[0].pos_rob.z;
                consts[j] = delta[i]; j++;
            }
            Console.WriteLine("1 = " + w_mm[0] + " " + w_pix[0]);
            Console.WriteLine("2 = " + w_mm[1] + " " + w_pix[1]);
            Console.WriteLine("3 = " + w_mm[2] + " " + w_pix[2]);
            Console.WriteLine("d = " + delta);
            return consts;

            //ka,f,h
            //findOneVarDec_new(new double[] { 0.6, wpix1/3, wmm1/3 }, new double[] { 3, 2000, 200 }, consts, calc_F_simple);
        }
        List<Point3d_GL[]> map_fov_3d(List<Frame> frs)
        {
            List<Point3d_GL> ps1 = new List<Point3d_GL>();
            List<Point3d_GL> ps2 = new List<Point3d_GL>();
            List<Point3d_GL> ps3 = new List<Point3d_GL>();
            List<Point3d_GL> ps4 = new List<Point3d_GL>();
            var consts = calc_preFov_all(frs);
            var mins = new double[] { 0.5, 400, 20 };
            var maxs = new double[] { 2, 800, 90 };
            double dim = 100;
            double delt_k = (maxs[0] - mins[0]) / dim;
            double delt_f = (maxs[1] - mins[1]) / dim;
            double delt_h = (maxs[2] - mins[2]) / dim;
            var max_k = -10000;
            var min_k = 10000;
            var max_f = -10000;
            var min_f = 10000;
            var max_h = -10000;
            var min_h = 10000;
            double eps = 0.08;

            double cur_k = mins[0];

            for (int i1 = 0; i1 < dim; i1++)
            {
                double cur_f = mins[1];
                for (int i2 = 0; i2 < dim; i2++)
                {
                    double cur_h = mins[2];
                    for (int i3 = 0; i3 < dim; i3++)
                    {

                        var ret = calc_F_all_ret(new double[] { cur_k, cur_f, cur_h }, consts);
                        if (ret[0] < eps)
                        {
                            ps1.Add(new Point3d_GL(i1, i2, i3));
                        }
                        if (ret[1] < eps)
                        {
                            ps2.Add(new Point3d_GL(i1, i2, i3));

                        }
                        if (ret[2] < eps)
                        {
                            ps3.Add(new Point3d_GL(i1, i2, i3));
                        }
                        double sum = 0;
                        foreach (var r in ret)
                        {
                            sum += r;
                        }
                        ps4.Add(new Point3d_GL(100 * sum, i2, i3));
                        if (sum < eps)
                        {
                            //ps4.Add(new Point3d_GL(i1, i2, i3));
                            // Console.WriteLine(" k= " + cur_k + " f = " + cur_f + " h = " + cur_h + " ");

                        }
                        cur_h += delt_h;
                    }
                    cur_f += delt_f;
                }
                cur_k += delt_k;
                cur_k = 1.0;
                Console.WriteLine("calc = " + i1 + " from " + dim);
            }
            List<Point3d_GL[]> all_p = new List<Point3d_GL[]>();
            all_p.Add(ps1.ToArray());
            all_p.Add(ps2.ToArray());
            all_p.Add(ps3.ToArray());
            all_p.Add(ps4.ToArray());
            return all_p;
        }
        public double errPos(PointF[] points, Size size, double fov, double side)
        {
            var Camera1 = new Camera(fov, size);
            var points1 = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points1[i] = new PointF(points[i].X - size.Width / 2,
                                        points[i].Y - size.Height / 2);
                points1[i].Y *= -1;
            }
            var cl_P1 = new Point3d_GL(0, 0, 0);
            var cl_P2 = new Point3d_GL(side, 0, 0);
            var cl_P3 = new Point3d_GL(side, side, 0);
            var cl_P4 = new Point3d_GL(0, side, 0);
            Camera1.calc_pos_all(points1[0], points1[1], points1[2], points1[3], cl_P1, cl_P2, cl_P3, cl_P4);
            Console.WriteLine("poS_ " + Camera1.pos.x + " " + Camera1.pos.y + " " + Camera1.pos.z + " " + Camera1.pos.magnitude());
            //addGLMesh(cube_buf, PrimitiveType.Triangles, (float)Camera1.pos.x, (float)Camera1.pos.y, (float)Camera1.pos.z);
            //addFrame(Camera1.pos, Camera1.pos + Camera1.oX * 15, Camera1.pos + Camera1.oY * 15, Camera1.pos + Camera1.oZ * 20);
            return Camera1.err_pos;
        }
        private double calcPixForCam(List<Frame> frames, double fov, double side)
        {
            var pos_cam = new List<Point3d_GL>();
            double delta = 0;
            //Console.WriteLine(fov + " " + side);
            for (int i = 0; i < frames.Count; i++)
            {
                pos_cam.Add(calcPos(frames[i].points, frames[i].im.Size, fov, side).pos);
            }
            int alld = 0;
            for (int i = 0; i < pos_cam.Count; i++)
            {
                for (int ie = i + 1; ie < pos_cam.Count; ie++)
                {
                    var d1 = pos_cam[i] - pos_cam[ie];
                    var err1 = d1.magnitude();

                    var d2 = frames[i].pos_rob - frames[ie].pos_rob;
                    var err2 = d2.magnitude();

                    delta += Math.Abs(err1 - err2);
                    alld++;
                }
            }
            return delta;
        }
        double findOneVarDih(double minVal, double maxVal, List<Frame> fr, double side,
                                Func<List<Frame>, double, double, double> func)
        {
            //ret of func - error
            double epsilon = 0.01;
            var a = minVal;
            var b = maxVal;
            var c = 0.0;
            int i = 0;

            while (b - a > epsilon && i < 1000)
            {
                i++;
                c = (a + b) / 2;
                if (Math.Abs(func(fr, a, side)) > Math.Abs(func(fr, b, side)))
                    a = c;
                else
                    b = c;
                //Console.WriteLine(" i = " + i + "fov = " +c+" er = "+ Math.Abs(func(fr, c, side)));
            };
            return c;
        }
        double findOneVarDec(double minVal, double maxVal, List<Frame> fr, double side,
                                Func<List<Frame>, double, double, double> func)
        {
            //ret of func - error
            const int dec = 6;
            double epsilon = 0.01;
            var a = minVal;
            var b = maxVal;
            double[] c = new double[dec];

            double[] ret = new double[dec];
            int ind = 0;

            while (b - a > epsilon && ind < 100)
            {
                //Console.WriteLine((a + b) / 2);
                ind++;
                double min_ret = 100000000;
                int min_ind = 0;
                for (int i = 0; i < dec; i++)
                {

                    c[i] = a + i * (b - a) / dec;
                    ret[i] = Math.Abs(func(fr, c[i], side));
                    //Console.WriteLine(" i = " + ind + "fov = " + c[i] + " er = " + ret[i]);
                    if (ret[i] < min_ret)
                    {
                        min_ind = i;
                        min_ret = ret[i];
                    }
                }
                if (min_ind == 0)
                {
                    a = c[0];
                    b = c[1];
                }
                else if (min_ind == dec - 1)
                {
                    a = c[dec - 2];
                    b = c[dec - 1];
                }
                else
                {
                    a = c[min_ind - 1];
                    b = c[min_ind + 1];
                }
                var c1 = (a + b) / 2;
                //  Console.WriteLine(" i = " + ind + "fov = " +c1+" er = "+ Math.Abs(func(fr, c1, side)));
            };
            return (a + b) / 2;
        }

        /* double findOneVarDec_fov(double[] minVal, double[] maxVal, double[] consts,
                                Func<double[], double[], double> func)
         {
             //ret of func - error
             const int dec = 6;
             double epsilon = 0.01;
             int len = minVal.Length;
             var a = minVal;
             var b = maxVal;
             double[] c = new double[dec];

             double[] ret = new double[dec];
             int ind = 0;
             double min_ret = 100000000;

             while (min_ret > epsilon && ind < 100)
             {
                 min_ret = 100000000;
                 ind++;

                 int min_ind = 0;
                 for (int i = 0; i < dec; i++)
                 {

                     c[i] = a + i * (b - a) / dec;
                     ret[i] = func(c, consts);
                     if (ret[i] < min_ret)
                     {
                         min_ind = i;
                         min_ret = ret[i];
                     }
                 }
                 if (min_ind == 0)
                 {
                     a = c[0];
                     b = c[1];
                 }
                 else if (min_ind == dec - 1)
                 {
                     a = c[dec - 2];
                     b = c[dec - 1];
                 }
                 else
                 {
                     a = c[min_ind - 1];
                     b = c[min_ind + 1];
                 }
                 //Console.WriteLine(" i = " + ind + "fov = " +c+" er = "+ Math.Abs(func(fr, c, side)));
             };
             return (a + b) / 2;
         }*/
        double[] findOneVarDec_new(double[] minVal, double[] maxVal, double[] consts, Func<double[], double[], double> func)
        {
            //ret of func - error
            const int dec = 6;
            double epsilon = 0.01;
            int len = minVal.Length;
            var a = new double[len];
            var b = new double[len];
            double[][] c = new double[len][];
            double ret = 1000000;

            for (int i = 0; i < len; i++)
            {
                c[i] = new double[dec];
                a[i] = minVal[i];
                b[i] = maxVal[i];
            }
            double err = 0;
            for (int i = 0; i < len; i++)
            {
                err += Math.Abs(a[i] - b[i]);
            }
            int ind = 0;
            double min_ret = 1000000;
            int[] min_ind = { 0, 0, 0 };
            while (err > epsilon && ind < 100)
            {
                for (int dim1 = 0; dim1 < dec; dim1++)
                {
                    for (int dim2 = 0; dim2 < dec; dim2++)
                    {
                        for (int dim3 = 0; dim3 < dec; dim3++)
                        {
                            for (int i = 0; i < dec; i++)
                            {

                                c[0][dim1] = a[0] + dim1 * (b[0] - a[0]) / dec;
                                c[1][dim2] = a[1] + dim2 * (b[1] - a[1]) / dec;
                                c[2][dim3] = a[2] + dim3 * (b[2] - a[2]) / dec;
                                var vars = new double[] { c[0][dim1], c[1][dim2], c[2][dim3] };
                                ret = Math.Abs(func(vars, consts));
                                if (ret < min_ret)
                                {
                                    min_ind[0] = dim1;
                                    min_ind[1] = dim2;
                                    min_ind[2] = dim3;
                                    min_ret = ret;
                                }
                            }


                        }
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    if (min_ind[i] == 0)
                    {
                        a[i] = c[i][0];
                        b[i] = c[i][1];
                    }
                    else if (min_ind[i] == dec - 1)
                    {
                        a[i] = c[i][dec - 2];
                        b[i] = c[i][dec - 1];
                    }
                    else
                    {
                        a[i] = c[i][min_ind[i] - 1];
                        b[i] = c[i][min_ind[i] + 1];
                    }
                }
                for (int i = 0; i < len; i++)
                {
                    err += Math.Abs(a[i] - b[i]);
                }
                ind++;
            }

            return null;
        }
        private double[] calc_F(double[] vars, double[] consts)
        {
            double k_alpha = vars[0];
            double f = vars[1];
            double h = vars[2];
            double w_mm1 = consts[0];
            double w_pix1 = consts[1];
            double w_mm2 = consts[2];
            double w_pix2 = consts[3];
            double w_mm3 = consts[4];
            double w_pix3 = consts[5];
            double delta = consts[6];
            double[] ret = new double[3];
            ret[0] = Math.Atan(w_mm1 / (2 * h)) - k_alpha * Math.Atan(w_pix1 / (2 * f));
            ret[1] = Math.Atan(w_mm2 / (2 * h)) - k_alpha * Math.Atan(w_pix2 / (2 * f));
            ret[2] = Math.Atan(w_mm3 / (2 * (h + delta))) - k_alpha * Math.Atan(w_pix3 / (2 * f));
            return ret;
        }
        private double calc_F_simple(double[] vars, double[] consts)
        {
            double k_alpha = vars[0];
            double f = vars[1];
            double h = vars[2];
            double w_mm1 = consts[0];
            double w_pix1 = consts[1];
            double w_mm2 = consts[2];
            double w_pix2 = consts[3];
            double w_mm3 = consts[4];
            double w_pix3 = consts[5];
            double delta = consts[6];
            double[] ret = new double[3];
            ret[0] = Math.Atan(w_mm1 / (2 * h)) - k_alpha * Math.Atan(w_pix1 / (2 * f));
            ret[1] = Math.Atan(w_mm2 / (2 * h)) - k_alpha * Math.Atan(w_pix2 / (2 * f));
            ret[2] = Math.Atan(w_mm3 / (2 * (h + delta))) - k_alpha * Math.Atan(w_pix3 / (2 * f));
            var ret1 = ret[0] * ret[0] + ret[1] * ret[1] + ret[2] * ret[2];

            Console.WriteLine("ret = " + ret1 + " k_alpha = " + k_alpha + " f = " + f + " h = " + h + " ");
            return ret1;
        }
        //k_alpha = 1.66666666666667 f = 796.979717565466 h = 24.2177821260455 
        private double[] calc_F_ret(double[] vars, double[] consts)
        {
            double k_alpha = vars[0];
            double f = vars[1];
            double h = vars[2];
            double w_mm1 = consts[0];
            double w_pix1 = consts[1];
            double w_mm2 = consts[2];
            double w_pix2 = consts[3];
            double w_mm3 = consts[4];
            double w_pix3 = consts[5];
            double delta = consts[6];
            double[] ret = new double[3];
            ret[0] = Math.Atan(w_mm1 / (2 * h)) - k_alpha * Math.Atan(w_pix1 / (2 * f));
            ret[1] = Math.Atan(w_mm2 / (2 * h)) - k_alpha * Math.Atan(w_pix2 / (2 * f));
            ret[2] = Math.Atan(w_mm3 / (2 * (h + delta))) - k_alpha * Math.Atan(w_pix3 / (2 * f));
            for (int i = 0; i < 3; i++)
            {
                ret[i] = Math.Abs(ret[i]);
            }
            //var ret1 = ret[0] * ret[0] + ret[1] * ret[1] + ret[2] * ret[2];
            //Console.WriteLine("ret = " + ret1 + " k_alpha = " + k_alpha + " f = " + f + " h = " + h + " ");
            return ret;
        }
        private double[] calc_F_all_ret(double[] vars, double[] consts)
        {
            double k_alpha = vars[0];
            double f = vars[1];
            double h = vars[2];
            int len = (consts.Length - 4) / 3 + 2;
            double[] w_mm = new double[len];
            double[] w_pix = new double[len];
            double[] delta = new double[len];
            w_mm[0] = consts[0];
            w_pix[0] = consts[1];
            w_mm[1] = consts[2];
            w_pix[1] = consts[3];
            int j = 4;
            for (int i = 2; i < len; i++)
            {
                w_mm[i] = consts[j]; j++;
                w_pix[i] = consts[j]; j++;
                delta[i - 2] = consts[j]; j++;
            }

            double[] ret = new double[len];
            ret[0] = Math.Atan(w_mm[0] / (2 * h)) - k_alpha * Math.Atan(w_pix[0] / (2 * f));
            ret[1] = Math.Atan(w_mm[1] / (2 * h)) - k_alpha * Math.Atan(w_pix[2] / (2 * f));

            for (int i = 2; i < len; i++)
            {
                ret[i] = Math.Atan(w_mm[i] / (2 * (h + delta[i - 2]))) - k_alpha * Math.Atan(w_pix[i] / (2 * f));
            }
            for (int i = 0; i < len; i++)
            {
                ret[i] = Math.Abs(ret[i]);
            }
            //var ret1 = ret[0] * ret[0] + ret[1] * ret[1] + ret[2] * ret[2];
            //Console.WriteLine("ret = " + ret1 + " k_alpha = " + k_alpha + " f = " + f + " h = " + h + " ");
            return ret;
        }
        private Image<Gray, Byte> mapSolv(List<Frame> frames, double start_fov, double start_side, int dim, double delta)
        {
            var ret = new Image<Gray, Byte>(dim, dim);
            var fov = (1 - delta) * start_fov;
            var side = (1 - delta) * start_side;
            var fov_delta = 2 * delta * start_fov / ret.Width;
            var side_delta = 2 * delta * start_side / ret.Width;

            for (int x = 0; x < ret.Width; x++)
            {
                fov = (1 - delta) * start_fov;
                Console.WriteLine(x);
                for (int y = 0; y < ret.Height; y++)
                {
                    int color = (int)calcPixForCam(frames, fov, side);
                    if (color > 255)
                    {
                        color = 255;
                    }
                    ret.Data[y, x, 0] = (byte)color;
                    Console.WriteLine(fov + " " + side + " " + ret.Data[y, x, 0] + " ");
                    if ((int)ret.Data[y, x, 0] < 10)
                    {
                        Console.WriteLine(fov + " " + side + " " + ret.Data[y, x, 0] + " ");
                    }

                    fov += fov_delta;

                }
                side += side_delta;
            }

            return ret;
        }
        private void fov3dMap(List<Frame> frames, double start_fov, double side, int dim, double delta, int n, int step = 1)
        {
            var dbs = new List<double[]>();
            for (int i = 0; i < frames.Count - n; i += step)
            {
                var frs = frames.GetRange(i, n);
                dbs.Add(lineSolv_doub(frs, start_fov, side, dim, delta));
            }
            var im = mapSolv3D(dbs);
            GL1.addGLMesh(meshFromImage(im), PrimitiveType.Triangles);
        }
        private Image<Gray, Byte> mapSolv3D(List<double[]> frames)
        {
            var im_res = new Image<Gray, Byte>(frames.Count, frames[0].Length);
            var min_f = new double[frames.Count];
            var max_f = new double[frames.Count];
            for (int i = 0; i < frames.Count; i++)
            {
                max_f[i] = frames[i].Max();
                min_f[i] = frames[i].Min();
            }
            double min_val = min_f.Min();
            double max_val = max_f.Max();

            double range = max_val - min_val;
            double off = min_val;
            if (off > 0)
            {
                off *= -1;
            }
            double scalez = 255 / range;
            //var frs = frames.ToArray();
            Console.WriteLine(off);
            Console.WriteLine(range);
            Console.WriteLine(max_val);
            Console.WriteLine(min_val);
            for (int x = 0; x < im_res.Width - 1; x++)
            {
                for (int y = 0; y < im_res.Height - 1; y++)
                {
                    //Console.WriteLine(frames[x][y]);
                    var z = (off + frames[x][y]) * scalez;

                    if (z > 255)
                    {
                        z = 255;
                    }
                    im_res.Data[y, x, 0] = (byte)z;
                }
            }
            return im_res;
        }
        private double[] lineSolv_doub(List<Frame> frames, double start_fov, double side, int dim, double delta)
        {
            var ret = new double[dim];
            var fov = (1 - delta) * start_fov;
            var fov_delta = 2 * delta * start_fov / ret.Length;
            for (int x = 0; x < ret.Length; x++)
            {
                ret[x] = calcPixForCam(frames, fov, side);
                fov += fov_delta;
            }
            return ret;
        }
        private Image<Gray, Byte> lineSolv(List<Frame> frames, double start_fov, double side, int dim, double delta)
        {
            var ret = new Image<Gray, Byte>(dim, 300);
            var fov = (1 - delta) * start_fov;
            var fov_delta = 2 * delta * start_fov / ret.Width;

            int y = 0;
            for (int x = 0; x < ret.Width; x++)
            {

                y = (int)calcPixForCam(frames, fov, side);
                if (y > ret.Height)
                {
                    y = ret.Height - 1;
                }
                ret.Data[y, x, 0] = 255;
                Console.WriteLine(fov + " " + y + " ");
                fov += fov_delta;
            }
            return ret;
        }
        private Image<Gray, Byte> lineErr(List<Frame> frames, double fov, double side)
        {
            var ret = new Image<Gray, Byte>(frames.Count, 300);


            double y = 0;
            for (int x = 0; x < ret.Width; x++)
            {

                y = 50 * errPos(frames[x].points, frames[x].im.Size, fov, side);
                if (y > ret.Height)
                {
                    y = ret.Height - 1;
                }
                ret.Data[(int)y, x, 0] = 255;
                Console.WriteLine(y);
            }
            return ret;
        }
        private double findFov(List<Frame> frames, double start_fov, double side, int dim, double delta)
        {
            double n = 200;
            var fov = (1 - delta) * start_fov;
            var fov_delta = 2 * delta * start_fov / n;
            int y = 0;
            for (int x = 0; x < n; x++)
            {

                y = (int)calcPixForCam(frames, fov, side);
                fov += fov_delta;
            }

            return n;
        }
        #endregion

        #region Mesh
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

        void generateImage3D(int n, float k,float sidef)
        {
            float im_side = sidef*n/(n-1);
            float side = im_side / n;
            float q_side = (k * side);
            Console.WriteLine("q_side " + q_side);

            float[] square_buf = {
                            0.0f,0.0f,0.0f, // triangle 1 : begin
                            0.0f,q_side, 0.0f,
                           q_side,q_side, 0.0f, // triangle 1 : end
                            q_side, q_side,0.0f, // triangle 2 : begin
                           q_side,0.0f,0.0f,
                            0.0f, 0.0f,0.0f };

            //Console.WriteLine("side = "+side);
           // Console.WriteLine("side = " + q_side);
            for (float x = -side/4; x >= -im_side; x -= side)
            {
                for (float y = -side / 4; y >= -im_side; y -= side)
                {
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
                    GL1.addGLMesh(square_buf, PrimitiveType.Triangles, x+offx, y+offy);
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
        float[] pointsToMesh(double[][] inp)
        {
            float[] vert = new float[inp.Length];
            return null;
        }
        
        #endregion

        #region video
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
        Mat deleteOnePixel(Mat src)
        {
            var im_res = src.ToImage<Gray, Byte>();
            var im_ret = new Image<Gray, Byte>(src.Width, src.Width);
            for (int x = 5; x < im_res.Width - 5; x++)
            {
                for (int y = 5; y < im_res.Height - 5; y++)
                {

                    int res = (int)im_res.Data[y - 4, x - 4, 0] + (int)im_res.Data[y - 4, x - 3, 0] + (int)im_res.Data[y - 4, x - 2, 0] + (int)im_res.Data[y - 4, x - 1, 0] + (int)im_res.Data[y - 4, x, 0] + (int)im_res.Data[y - 4, x + 1, 0] + (int)im_res.Data[y - 4, x + 2, 0] + (int)im_res.Data[y - 4, x + 3, 0] + (int)im_res.Data[y - 4, x + 4, 0] +
                        (int)im_res.Data[y - 3, x - 4, 0] + (int)im_res.Data[y - 2, x + 4, 0] +
                        (int)im_res.Data[y - 2, x - 4, 0] + (int)im_res.Data[y - 2, x + 4, 0] +
                        (int)im_res.Data[y - 1, x - 4, 0] + (int)im_res.Data[y - 1, x + 4, 0] +
                        (int)im_res.Data[y, x - 4, 0] + (int)im_res.Data[y, x + 4, 0] +
                        (int)im_res.Data[y + 1, x - 4, 0] + (int)im_res.Data[y + 1, x + 4, 0] +
                        (int)im_res.Data[y + 2, x - 4, 0] + (int)im_res.Data[y + 2, x + 4, 0] +
                        (int)im_res.Data[y + 3, x - 4, 0] + (int)im_res.Data[y + 3, x + 4, 0] +
                     (int)im_res.Data[y + 4, x - 4, 0] + (int)im_res.Data[y + 4, x - 3, 0] + (int)im_res.Data[y + 4, x - 2, 0] + (int)im_res.Data[y + 4, x - 1, 0] + (int)im_res.Data[y + 4, x, 0] + (int)im_res.Data[y + 4, x + 1, 0] + (int)im_res.Data[y + 4, x + 2, 0] + (int)im_res.Data[y + 4, x + 3, 0] + (int)im_res.Data[y + 4, x + 4, 0];
                    if (res < 250)
                    {
                        im_res.Data[y, x, 0] = 0;
                    }
                    else
                    {
                        im_ret.Data[y, x, 0] = im_res.Data[y, x, 0];

                    }
                }
            }

            return im_ret.Mat;
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
        #region legacy

        private Point[] finPointFs1(Mat im, int bin, ImageBox box)
        {
            return takePoints(revealContour(im, bin, box)).ToArray();
        }
        private Point[] finPointFs(List<Mat> ims, ImageBox box)
        {
            var points = new Point[3];
            var countp = new int[3];
            int count1 = 0;
            var maxVal = calc_p_len(new Point(0, 0), new Point(ims[0].Width, ims[0].Height));
            var prev_points = new Point[3];
            var cur_points = new Point[3];
            prev_points[0] = new Point(0, 0);
            prev_points[1] = new Point(0, 0);
            prev_points[2] = new Point(0, 0);
            List<PointC>[] points_A = new List<PointC>[3];
            points_A[0] = new List<PointC>();
            points_A[1] = new List<PointC>();
            points_A[2] = new List<PointC>();
            double k = imageBox3.Width / 255;
            var im_res = new Image<Bgr, Byte>(ims[0].Width, ims[0].Width);
            var im_res1 = new Image<Bgr, Byte>(imageBox3.Width, imageBox3.Height);
            foreach (var im in ims)
            {
                for (int i = 0; i < 255; i += 100)
                {
                    cur_points = takePoints(revealContour(im, 137, box)).ToArray();
                    for (int w = 0; w < 3; w++)
                    {
                        var p = cur_points[w];
                        int x = p.X;
                        int y = p.Y;

                        if (p.Y != 0 && p.Y != im.Height - 1 && p.X != 0 && p.X != im.Width - 1)
                        {
                            im_res.Data[p.Y, p.X, w] += 1;

                        }
                    }
                }

                /* for (int w = 0; w < 3; w++)
                 {
                     int d = 20 * (int)(calc_p_len(cur_points[w], prev_points[w]));
                     d = d + 10;
                     if (d > imageBox3.Height-1)
                     {
                         d = imageBox3.Height-1;
                     }

                     if (prev_points[w] == cur_points[w]  &&
                         prev_points[w].X != 0 && prev_points[w].Y != im.Height - 1 &&
                         prev_points[w].X != 0 && prev_points[w].X != im.Width - 1)
                     {
                         count1++;
                         //im_res1.Data[d + 30 * w, (int)(i * k), w] = 255;
                     }
                     if (prev_points[w] != cur_points[w])
                     {
                         points_A[w].Add(new PointC(count1, new Point(prev_points[w].X, prev_points[w].Y)));
                         count1 = 0;
                     }
                 }
                 for (int w = 0; w < 3; w++)
                 {
                     prev_points[w] = cur_points[w];
                 }  */
            }
            /* countp = new int[3]{ 0, 0, 0 };
             for (int w = 0; w < 3; w++)
             {
                 int ind = 0;
                 int ind2 = 0;
                 foreach(var item in points_A[w])
                 {
                     if(points_A[w][ind2].count>countp[w])
                     {
                         ind = ind2;
                     }
                     ind2++;
                 }
                 points[w] = points_A[w][ind].point;
             }*/


            for (int x = 0; x < im_res.Width; x++)
            {
                for (int y = 0; y < im_res.Height; y++)
                {
                    for (int w = 0; w < 3; w++)
                    {
                        if (im_res.Data[y, x, w] > countp[w])
                        {
                            points[w] = new Point(x, y);
                            countp[w] = im_res.Data[y, x, w];
                        }

                    }

                }
            }
            Console.WriteLine(points.Length);
            box.Image = im_res;
            return points;
        }
        Point[] findCrossing(PointF[] lines, Size size)
        {
            var linesX = new List<PointF>();
            var linesY = new List<PointF>();
            var crossP = new List<Point>();
            foreach (var p in lines)
            {
                if (p.X >= 0)
                {
                    linesX.Add(p);
                }
                else
                {
                    linesY.Add(p);
                }
            }
            foreach (var p1 in linesX)
            {
                foreach (var p2 in linesY)
                {
                    var x = (p2.Y - p1.Y) / (p1.X - p2.X);
                    var y = p1.X * x + p1.Y;
                    if (x >= 0 && x < size.Width)
                    {
                        if (y >= 0 && y < size.Height)
                        {
                            crossP.Add(new Point((int)x, (int)y));
                        }
                    }

                }
            }
            if (crossP.Count != 0)
            {
                return crossP.ToArray();
            }
            return null;
        }
        PointF[] findCrossingD(PointF[] lines, Size size, Mat im)
        {
            var linesX = new List<PointF>();
            var linesY = new List<PointF>();
            var crossP = new List<PointF>();
            var f = from x in lines
                    orderby Math.Abs(x.X)
                    select x;
            var lines_s = f.ToArray();
            for (int i = 0; i < lines.Length / 2; i++)
            {
                linesX.Add(lines_s[i]);
                Console.WriteLine("Line X a =" + lines_s[i].X + " b =  " + lines_s[i].Y);
            }

            for (int i = lines.Length / 2; i < lines.Length; i++)
            {
                linesY.Add(lines_s[i]);
                Console.WriteLine("Line Y a =" + lines_s[i].X + " b =  " + lines_s[i].Y);
            }
            foreach (var p in linesX)
            {
                var pts = find2Points(p, im.Size);
                if (pts != null)
                {
                    CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(255, 0, 0), 2);
                }
            }

            foreach (var p in linesY)
            {
                var pts = find2Points(p, im.Size);
                if (pts != null)
                {
                    CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(0, 255, 0), 2);
                }
            }
            Console.WriteLine("FIND CROSSING------------------");
            foreach (var p1 in linesX)
            {
                foreach (var p2 in linesY)
                {

                    var x = (p2.Y - p1.Y) / (p1.X - p2.X);
                    var y = p1.X * x + p1.Y;
                    var y2 = p2.X * x + p2.Y;
                    var dy = y - y2;

                    if (x >= 0 && x < size.Width)
                    {
                        if (y >= 0 && y < size.Height)
                        {
                            crossP.Add(new PointF(x, y));
                        }
                    }
                    Console.WriteLine("X " + x + " Y " + y + " Y2 " + y2);

                }
            }
            if (crossP.Count != 0)
            {
                return crossP.ToArray();
            }
            return null;
        }

        private Point leftPoint(Image<Gray, Byte> im)
        {
            for (int x = 6; x < im.Width - 6; x++)
            {
                for (int y = 6; y < im.Height - 6; y++)
                {
                    if (im.Data[y, x, 0] > 100 && calcRes(im.Data, x, y) > res_min)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return new Point(0, 0);
        }
        private Point rightPoint(Image<Gray, Byte> im)
        {
            for (int x = im.Width; x > 0; x--)
            {
                for (int y = 0; y < im.Height - 1; y++)
                {
                    if (im.Data[y, x, 0] > 100)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return new Point(0, 0);
        }
        private Point downPoint(Image<Gray, Byte> im)
        {
            for (int y = 6; y < im.Height - 6; y++)
            {
                for (int x = 6; x < im.Width - 6; x++)
                {

                    if (im.Data[y, x, 0] > 100 && calcRes(im.Data, x, y) > res_min)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return new Point(0, 0);
        }
        private Point upPoint(Image<Gray, Byte> im)
        {
            for (int y = im.Height - 6; y > 6; y--)
            {
                for (int x = 6; x < im.Width - 6; x++)
                {
                    if (im.Data[y, x, 0] > 100 && calcRes(im.Data, x, y) > res_min)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return new Point(0, 0);
        }

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
                    var ps2 = finPointFs(Ims, imageBox3);
                    flag2 = 1;
                }
            }
            im.Dispose();
        }
        public Mat binVideo(Mat im, ImageBox box, int bin)
        {
            var mat_im = new Mat();
            im.CopyTo(mat_im);

            var stroka = findContour(mat_im, box, bin);
            var mat_out = paintRegression(mat_im, stroka);
            var points = regressionPoints(mat_im.Size, stroka);
            //drawTours(mat_out, points, 0, 255, 255);
            imageBox2.Image = im;
            return mat_out;
        }
        public void binVideo_Real(Mat im, ImageBox box, int bin)
        {
            var mat_im = new Mat();
            im.CopyTo(mat_im);
            var stroka = findContourZ(mat_im, box, bin);
        }
        private List<Point> takePoints(Mat im)
        {
            Image<Gray, Byte> im_gray = im.ToImage<Gray, Byte>();
            List<Point> points = new List<Point>();
            points.Add(downPoint(im_gray));
            points.Add(leftPoint(im_gray));
            points.Add(upPoint(im_gray));
            return points;
        }








        #endregion

        private void but_SubpixPrec_Click(object sender, EventArgs e)
        {
            calcSubpixelPrec(GL1.matFromMonitor(0), new Size(6, 7));
        }
    }

    static public class prin
    {
        public static void t(Image<Gray, float> matr)
        {
            var flarr = matr.Data;
            var ch = matr.NumberOfChannels;
            for (int i = 0; i < matr.Rows; i++)
            {
                for (int j = 0; j < matr.Cols; j++)
                {
                    for (int k = 0; k < ch; k++)
                    {
                        Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                    }
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(Image<Bgr, float> matr)
        {
            var flarr = matr.Data;
            var ch = 3;
            for (int i = 0; i < matr.Rows; i++)
            {
                for (int j = 0; j < matr.Cols; j++)
                {
                    for (int k = 0; k < ch; k++)
                    {
                        Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                    }
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(VectorOfVectorOfPoint3D32F matr)
        {
            for (int i = 0; i < matr.Size; i++)
            {
                for (int y = 0; y < matr[i].Size; y++)
                {
                    Console.Write(matr[i][y].X + " " + matr[i][y].Y + " " + matr[i][y].Z + "; ");
                }
                Console.WriteLine("; ");
            }
            Console.WriteLine(" ");
        }
        public static void t(VectorOfVectorOfPointF matr)
        {
            for (int i = 0; i < matr.Size; i++)
            {
                for (int y = 0; y < matr[i].Size; y++)
                {
                    Console.Write(matr[i][y].X + " " + matr[i][y].Y + "; ");
                }
                Console.WriteLine("; ");
            }
            Console.WriteLine(" ");
        }
        public static void t(double[] matr)
        {
            for (int i = 0; i < matr.Length; i++)
            {
                Console.Write(matr[i] + " ");
            }
            Console.WriteLine(" ");
        }
        public static void t(Frame frame, int cols = 3)
        {
            var name = frame.name;
            var name_t = name.Trim();
            var name_s = name_t.Split(' ');
            for (int i = 0; i < name_s.Length / cols; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(name_s[i * cols + j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(double[,] matr)
        {
            for (int i = 0; i < matr.GetColumn(0).Length; i++)
            {
                for (int j = 0; j < matr.GetRow(0).Length; j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(float[,,] matr)
        {
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                for (int j = 0; j < matr.GetLength(1); j++)
                {
                    for (int k = 0; k < matr.GetLength(2); k++)
                    {
                        Console.Write(matr[i, j, k] + " ");
                    }
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(float[,] matr)
        {
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                for (int j = 0; j < matr.GetLength(1); j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(Point3d_GL[][] matr)
        {
            for (int i = 0; i < matr.GetColumn(0).Length; i++)
            {
                for (int j = 0; j < matr.GetRow(0).Length; j++)
                {
                    Console.Write(matr[i][j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(MCvPoint3D32f[][] matr)
        {
            for (int i = 0; i < matr.GetColumn(0).Length; i++)
            {
                for (int j = 0; j < matr.GetRow(0).Length; j++)
                {
                    Console.Write(matr[i][j].X + " "+ matr[i][j].Y + " "+ matr[i][j].Z + "; ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(System.Drawing.PointF[][] matr)
        {
            for (int i = 0; i < matr.GetColumn(0).Length; i++)
            {
                for (int j = 0; j < matr.GetRow(0).Length; j++)
                {
                    Console.Write(matr[i][j].X + " " + matr[i][j].Y +"; ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(Matrix<double> matr)
        {
            Console.WriteLine("cols x rows: " + matr.Cols + " x " + matr.Rows);
            if (matr.Cols != 1)
            {
                for (int i = 0; i < matr.Cols; i++)
                {
                    for (int j = 0; j < matr.Rows; j++)
                    {
                        Console.Write(matr[i, j] + " ");
                    }
                    Console.WriteLine(" ");
                }
            }
            else
            {
                for (int i = 0; i < matr.Rows; i++)
                {
                    Console.Write(matr[i, 0] + " ");
                }
                Console.WriteLine(" ");
            }

        }
        public static void t(Matrix4x4f matr)
        {
            for (uint i = 0; i < 4; i++)
            {
                for (uint j = 0; j < 4; j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(Matrix3x3f matr)
        {
            for (uint i = 0; i < 3; i++)
            {
                for (uint j = 0; j < 3; j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        /*
        void print(Mat mat)
        {
            Console.WriteLine("cols x rows: " + mat.Cols + " x " + mat.Rows);
            var arr = mat.GetData();
            double[,] flarr = new double[1, 1];
           // Console.WriteLine("st");
            try
            {
                
                flarr = (double[,])arr;
                //Console.WriteLine("fl");
            }
            catch (System.InvalidCastException)
            {
                try
                {
                    flarr = convToDouble((float[,])arr);
                }
                catch (System.InvalidCastException)
                {

                }
                    //Console.WriteLine("db");
            }
            
            for (int i = 0; i < mat.Rows; i++)
            {
                for (int j = 0; j < mat.Cols; j++)
                {
                    Console.Write(Math.Round(flarr[i, j],3) + " ");

                }
                Console.WriteLine(" ");
            }
        }
        */
        public static void t(Mat mat)
        {
            Console.WriteLine("cols x rows: " + mat.Cols + " x " + mat.Rows);
            var arr = mat.GetData();
            var ch = mat.NumberOfChannels;
            if (ch > 1)
            {
                if (mat.Depth == DepthType.Cv64F)
                {
                    var flarr = (double[,,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            for (int k = 0; k < ch; k++)
                            {
                                Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                            }
                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv32F)
                {
                    var flarr = (float[,,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            for (int k = 0; k < ch; k++)
                            {
                                Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                            }
                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv16S)
                {
                    var flarr = (Int16[,,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            for (int k = 0; k < ch; k++)
                            {
                                Console.Write(flarr[i, j, k] + " ");
                            }
                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
            }
            else
            {
                if (mat.Depth == DepthType.Cv64F)
                {
                    var flarr = (double[,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {

                            Console.Write(Math.Round(flarr[i, j], 3) + " ");

                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv32F)
                {
                    var flarr = (float[,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            Console.Write(Math.Round(flarr[i, j], 3) + " ");

                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv16S)
                {
                    var flarr = (Int16[,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            Console.Write(flarr[i, j] + " ");

                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
            }




        }
        public static void t(Vertex3f v)
        {
            Console.WriteLine(v.x + " " + v.y + " " + v.z);
        }
        public static void t(string s)
        {
            Console.WriteLine(s);
        }
        public static void t(int s)
        {
            Console.WriteLine(s);
        }
        public static void t(double s)
        {
            Console.WriteLine(s);
        }
    }
    public  class FrameLoader
    {
        public FrameLoader()
        {
        }
        static public Frame loadImage_simple(string filepath)
        {
            string name = Path.GetFileName(filepath);
            //Console.WriteLine(name);
            var name_t = name.Trim();
            var file_ext = name_t.Split(new char[] { '.' });
            var coords = name_t.Split(new char[] { ' ' });
            // Console.WriteLine(file_ext[file_ext.Length - 1]);

            if ((file_ext[file_ext.Length - 1] == "jpg" || file_ext[file_ext.Length - 1] == "png") && file_ext[0] != "calibresult")
            {
                //Console.WriteLine(coords.Length);
                if (coords.Length > 2)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (coords[i].Contains(".") != true)
                        {
                            coords[i] += ".0";
                        }
                    }
                    var name_pos = new Point3d_GL(Convert.ToDouble(coords[0]),
                                                    Convert.ToDouble(coords[1]),
                                                    Convert.ToDouble(coords[2]));
                    //Console.WriteLine(name_pos.x + " " + name_pos.y + " " + name_pos.z + " ");
                    var im = new Mat(filepath);
                    return new Frame(im, name_pos, name_pos, name);
                }
                else if (coords.Length > 0)
                {
                    var name_pos = new Point3d_GL(0,
                                                    0,
                                                    0);
                    var im = new Mat(filepath);
                    return new Frame(im, name_pos, name_pos, name);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
        static public Frame loadImage_calib(string filepath)
        {
            string name = Path.GetFileName(filepath);
            var coords = name.Split(new char[] { ' ' });
            Console.WriteLine(coords[0].Length + " " + coords.Length);
            if (coords[0].Length == 0)
            {
                var lamd_ar = new string[coords.Length];
                coords.CopyTo(lamd_ar, 0);
                lamd_ar.CopyTo(coords, 0);
            }
            for (int i = 0; i < 4; i++)
            {
                if (coords[i].Contains(".") != true)
                {
                    coords[i] += ".0";
                }
            }
            var name_pos = new Point3d_GL(Convert.ToDouble(coords[0]),
                                            Convert.ToDouble(coords[1]),
                                            Convert.ToDouble(coords[2])
                                            );
            var mk = 10 * Convert.ToDouble(coords[3]);
            var im = new Mat(filepath);
            var ret = new Frame(im, name_pos, name_pos, name);
            ret.size_mark = mk;
            return ret;
        }

        static public Frame loadImage_test(string filepath)
        {
            string name = Path.GetFileName(filepath);
            var im = new Mat(filepath);
            var fr = new Frame(im, name);
            fr.dateTime = File.GetCreationTime(filepath);
            return fr;
        }
        static public Frame loadImage_chess(string filepath)
        {
            string name = Path.GetFileName(filepath);
            var im = new Mat(filepath);
            var fr = new Frame(im, name,FrameType.MarkBoard);
            fr.dateTime = File.GetCreationTime(filepath);
            return fr;
        }
        static public Frame loadImage_stereoCV(string filepath1, string filepath2)
        {
            string name1 = Path.GetFileName(filepath1);
            var im1 = new Mat(filepath1);
            string name2 = Path.GetFileName(filepath2);
            var im2 = new Mat(filepath2);
            Console.WriteLine(name1);
            Console.WriteLine(name2);
            Console.WriteLine("------------");
            var fr = new Frame(im1, im2, name1);
            fr.dateTime = File.GetCreationTime(filepath1);
            return fr;
        }
        string[] sortByDate(string[] files)
        {
            var sortFiles = from f in files
                            orderby File.GetCreationTime(f)
                            select f;
            return sortFiles.ToArray();
        }

        public List<Frame> loadImages_stereoCV(string path1, string path2)
        {
            Console.WriteLine(path1);
            var files1 = sortByDate(Directory.GetFiles(path1));
            var files2 = sortByDate(Directory.GetFiles(path2));
            List<Frame> frames = new List<Frame>();
            for (int i = 0; i < files1.Length; i++)
            {
                var frame = loadImage_stereoCV(files1[i], files2[i]);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames;
            }
            return null;
        }
   
        public List<Frame> loadImages_simple(string path)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_simple(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames;
            }
            return null;
        }
         public Frame[] loadImages_test(string path)
         {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_test(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames.ToArray();
            }
            return null;
        }
        public Frame[] loadImages_chess(string path)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_chess(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames.ToArray();
            }
            return null;
        }
        public List<Frame> loadImages_calib(string path)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_calib(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames;
            }
            return null;
        }

    }
    public class CameraCV
    {
        public Matrix<double> cameramatrix;
        public Matrix<double> distortmatrix;
        public Frame[] frames;
        public System.Drawing.PointF[][] corners;
        public MCvPoint3D32f[][] objps;
        public Mat[] tvecs;
        public Mat[] rvecs;
        public CameraCV(Matrix<double> _cameramatrix, Matrix<double> _distortmatrix)
        {
            cameramatrix = _cameramatrix;
            distortmatrix = _distortmatrix;
        }
        public CameraCV(Frame[] _frames, Size _size)
        {
            calibrateCam(_frames, _size);
        }
        void calibrateCam(Frame[] frames, Size size)
        {
            this.frames = frames;
            
            Matrix<double> _cameramatrix = new Matrix<double>(3,3);
            Matrix<double> _distortmatrix = new Matrix<double>(5, 1);

            var objps = new List<MCvPoint3D32f[]>();
            var corners = new List<System.Drawing.PointF[]>();

            var obp = new MCvPoint3D32f[size.Width * size.Height];
            int ind = 0;
            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    obp[ind] = new MCvPoint3D32f((float)i, (float)j, 0.0f);
                    ind++;
                }
            }
            
            foreach (var frame in frames)
            {
                var gray = frame.im.ToImage<Gray, byte>();
                var corn = new VectorOfPointF();
                var ret = CvInvoke.FindChessboardCorners(gray, size, corn);
                if (ret == true)
                {
                    CvInvoke.CornerSubPix(gray, corn, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(30, 0.001));
                    var corn2 = corn.ToArray();
                    objps.Add(obp);
                    corners.Add(corn2);
                }
                else
                { 
                    Console.WriteLine("NOT:");
                    Console.WriteLine(frame.name);
                }
            }

            Console.WriteLine(objps);
            Console.WriteLine(corners.Count);

            var rvecs = new Mat[corners.Count];
            var tvecs = new Mat[corners.Count];

            this.objps = objps.ToArray();
            this.corners = corners.ToArray();
            var err = CvInvoke.CalibrateCamera(objps.ToArray(), corners.ToArray(), frames[0].im.Size, _cameramatrix, _distortmatrix, CalibType.Default, new MCvTermCriteria(100, 0.0001), out rvecs, out tvecs);
            Console.WriteLine("err: " + err);
            var newRoI = new Rectangle();

            this.tvecs = tvecs;
            this.rvecs = rvecs;

            var matr = CvInvoke.GetOptimalNewCameraMatrix(_cameramatrix, _distortmatrix, frames[0].im.Size, 1, frames[0].im.Size, ref newRoI);
            var mapx = new Mat();
            var mapy = new Mat();

            //computeDistortionMaps(ref mapx, ref mapy, _cameramatrix, _distortmatrix, frames[0].im.Size);

            CvInvoke.InitUndistortRectifyMap(_cameramatrix, _distortmatrix, null, matr, frames[0].im.Size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(frames[0].im, und_pic, mapx, mapy, Inter.Linear);
            cameramatrix = _cameramatrix;
            distortmatrix = _distortmatrix;

            /*for (int i = 0; i < corners.Count; i++)
            {
                CvInvoke.Rodrigues(rvecs[i], rotateMatrix);
                var tvec = toVertex3f(tvecs[i]);
                var mx = assemblMatrix(rotateMatrix, tvec);
                var invMx = mx.Inverse;

                //Console.WriteLine("INV-----------");
                // print(invMx);
                // Console.WriteLine("FRAME-------------");
                //print(frames[i]);
            }*/

        }
    }
    public struct SGBM_param
    {
        public int minDisparity;
        public int numDisparities;
        public int blockSize;
        public int disp12MaxDiff;
        public int preFilterCap;
        public int uniquenessRatio;
        public int speckleWindowSize;
        public int speckleRange;
        public int p1;
        public int p2;
        public SGBM_param(
            int _minDisparity,
            int _numDisparities,
            int _blockSize, int _p1 = 0, int _p2 = 0,
            int _disp12MaxDiff = 0, int _preFilterCap = 0,
            int _uniquenessRatio = 0,
            int _speckleWindowSize = 0, int _speckleRange = 0)
        {
            minDisparity = _minDisparity;
            numDisparities = _numDisparities;
            blockSize = _blockSize;
            disp12MaxDiff = _disp12MaxDiff;
            preFilterCap = _preFilterCap;
            uniquenessRatio = _uniquenessRatio;
            speckleWindowSize = _speckleWindowSize;
            speckleRange = _speckleRange;
            p1 = _p1;
            p2 = _p2;
        }

    }
    public class StereoCameraCV
    {
        public CameraCV[] cameraCVs;
        public Mat t,r;
        StereoSGBM stereosolver;
        StereoBM stereosolverBM;
        public SGBM_param solver_param;
        public StereoCameraCV(CameraCV[] _cameraCVs)
        {
            cameraCVs = _cameraCVs;
            calibrateCamStereo(cameraCVs);
            init();
        }
        
        void calibrateCamStereo(CameraCV[] _cameraCVs)
        {
            if(_cameraCVs.Length<2)
            {
                return;
            }
            var cam1 = _cameraCVs[0];
            var cam2 = _cameraCVs[1];
            if (cam1.objps != cam2.objps)
            {
                Console.WriteLine("!=!=!=");
                //return;
            }
            for(int i=0; i< cam1.tvecs.Length; i++)
            {
                //printer.print("_________________");
                //printer.print(cam1.frames[i].name);
               // printer.print(cam1.tvecs[i]- cam2.tvecs[i]);

                //printer.print(cam2.tvecs[i]);
                
            }
            //printer.print(cam1.corners);
            //printer.print("______________");
            //printer.print(cam2.corners);

            var r = new Mat();
            var t = new Mat();
            var e = new Mat();
            var f = new Mat();

            var err = CvInvoke.StereoCalibrate
                (cam1.objps,
                cam1.corners,
                cam2.corners,
                cam1.cameramatrix,
                cam1.distortmatrix,
                cam2.cameramatrix,
                cam2.distortmatrix,
                cam1.frames[0].im.Size,
                r,
                t,
                e,
                f,
                CalibType.FixIntrinsic,
                new MCvTermCriteria(30, 0.001)
                );
            this.t = t;
            this.r = r;
            Console.WriteLine("r:  ");
            prin.t(r);
            Console.WriteLine("t:  ");
            prin.t(t);
            Console.WriteLine("e:  ");
            prin.t(e);
            Console.WriteLine("f:  ");
            prin.t(f);
            Console.WriteLine("err:  ");
            prin.t(err);

        }
        void epipolarStereo(Mat imL, Mat imR)
        {
            if (cameraCVs.Length < 2)
            {
                return;
            }
            var cam1 = cameraCVs[0];
            var cam2 = cameraCVs[1];
            var r1 = new Mat();
            var r2 = new Mat();
            var p1 = new Mat();
            var p2 = new Mat();
            var q = new Mat();
            var roi1 = new Rectangle();
            var roi2 = new Rectangle();
            CvInvoke.StereoRectify(
                cam1.cameramatrix, cam1.distortmatrix,
                cam2.cameramatrix, cam2.distortmatrix,
                cam1.frames[0].im.Size, r, t, r1, r2, p1, p2, q,
                StereoRectifyType.Default, -1, Size.Empty, ref roi1, ref roi2);

        }
        public void setSGBM_parameters()
        {
            stereosolver = new StereoSGBM(
                solver_param.minDisparity,
                solver_param.numDisparities,
                solver_param.blockSize,
                solver_param.p1, solver_param.p2,
                solver_param.disp12MaxDiff,
                solver_param.preFilterCap,
                solver_param.uniquenessRatio,
                solver_param.speckleWindowSize,
                solver_param.speckleRange, StereoSGBM.Mode.SGBM);
            stereosolverBM = new StereoBM(                
                solver_param.numDisparities,
                solver_param.blockSize
                );
        }
        void init()
        {
            var minDisparity = 0;
            var numDisparities = 64;
            var blockSize = 8;
            var disp12MaxDiff = 1;
            var uniquenessRatio = 10;
            var speckleWindowSize = 100;
            var speckleRange = 8;
            var p1 = 8 * blockSize * blockSize;
            var p2 = 32 * blockSize * blockSize;
            var preFilterCap = 8;
            solver_param = new SGBM_param(minDisparity, numDisparities, blockSize, p1, p2, disp12MaxDiff, preFilterCap, uniquenessRatio, speckleWindowSize, speckleRange);
            setSGBM_parameters();
        }
        public Mat epipolarTest(Mat matL, Mat matR)
        {
            if(stereosolver==null)
            {
                return null;
            }
            
            var imL = matL.ToImage<Gray, byte>();
            var imR = matR.ToImage<Gray, byte>();
            
            var disp = new Mat();
            var depth = new Mat();
            var depth_norm = new Mat();
            try
            {
                stereosolverBM.Compute(imL, imR, disp);
                disp.ConvertTo(disp, DepthType.Cv32F);
                disp += 1;
                disp /= 16;
                depth = 1000 / disp;

                // CvInvoke.Normalize(depth, depth_norm, 255, 0);
                depth.ConvertTo(depth_norm, DepthType.Cv8U);
                // prin.t(depth);
                //

                return depth_norm;
            }
            catch
            {
                return null;
            }
            
        }
    }

    public class robFrame
    {
        public double x;
        public double y;
        public double z;
        public double a;
        public double b;
        public double c;
        public robFrame()
        {

        }
        public robFrame(double _x, double _y, double _z, double _a, double _b, double _c)
        {
            x = _x;
            y = _y;
            z = _z;
            a = _a;
            b = _b;
            c = _c;
        }
        public robFrame(robFrame robFrame)
        {
            x = robFrame.x;
            y = robFrame.y;
            z = robFrame.z;
            a = robFrame.a;
            b = robFrame.b;
            c = robFrame.c;
        }
        public double rasstTo(robFrame robFrame)
        {
            var x1 = x;
            var y1 = y;
            var z1 = z;

            var x2 = robFrame.x;
            var y2 = robFrame.y;
            var z2 = robFrame.z;

            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) + (z2 - z1) * (z2 - z1));
        }
        public Point3d_GL pos()
        {
            return new Point3d_GL(x, y, z);
        }
        public Point3d_GL ori()
        {
            return new Point3d_GL(a,b,c);
        }
        override public string ToString()
        {
            return (x + " " +  y + " " + z + " " + a + " " + b + " " + c);
        }

    }
    public class LineF
    {
        public float X;
        public float Y;
        public PlanType plan;
        public LineF(float _x, float _y,PlanType _plan = PlanType.X)
        {
            X = _x;
            Y = _y;
            plan = _plan;
        }
        public LineF calcLine(PointF[] points, PlanType _plan = PlanType.X,int maxPoints = 1000)
        {

            int n = points.Length;
            //Console.WriteLine(n);
            if(n>maxPoints || n<2)
            {
                return null;
            }
            double x = 0;
            double y = 0;
            double xy = 0;
            double xx = 0;
            float X = 0;
            float Y = 0;
            foreach (var p in points)
            {
                if(_plan==PlanType.X)
                {
                    X = p.X;
                    Y = p.Y;
                }
                else
                {
                    X = p.Y;
                    Y = p.X;
                }               
                x += X;
                y += Y;
                xy += X * Y;
                xx += X * X;
            }
            var del = (n * xx - x * x);
            if (Math.Abs(del) < 0.1)
            {
                if( _plan == PlanType.X)
                {
                    return calcLine(points, PlanType.Y);
                }
                if (_plan == PlanType.Y)
                {
                    return calcLine(points, PlanType.X);
                }
                //return new PointF(4000, (float)x/(float)n);
            }
            float a = (float)((n * xy - x * y) / del);
            
            float b = (float)((y - a * x) / n);
            if (Math.Abs(a) >1.1)
            {
                if (_plan == PlanType.X)
                {
                    return calcLine(points, PlanType.Y);
                }
                if (_plan == PlanType.Y)
                {
                    return calcLine(points, PlanType.X);
                }
                //return new PointF(4000, (float)x/(float)n);
            }
            if (_plan == PlanType.X)
            {
                //Console.WriteLine("Line plan X " + a +" "+b+ " " + del);
            }
            else
            {
                //Console.WriteLine("Line plan Y " + a + " " + b);
            }
            return new LineF(a, b, _plan);
        }
        public PointF findCrossing(LineF L1, LineF L2)
        {

            if(L1.plan == PlanType.X && L2.plan == PlanType.X)
            {
                var x = (L2.Y - L1.Y) / (L1.X - L2.X);
                var y = L1.X * x + L1.Y;
                //Console.WriteLine(" X - X");
                return new PointF(x, y);
            }
            else if (L1.plan == PlanType.X && L2.plan == PlanType.Y)
            {
                var x = (L1.Y *L2.X+ L2.Y) / (1 - L1.X*L2.X);
                var y = L1.X * x + L1.Y;
                //Console.WriteLine(" X - Y");
                return new PointF(x, y);
            }
            else if (L1.plan == PlanType.Y && L2.plan == PlanType.X)
            {
                var x = (L2.Y * L1.X + L1.Y) / (1 - L2.X * L1.X);
                var y = L2.X * x + L2.Y;
               // Console.WriteLine(" Y - X");
                return new PointF(x, y);
            }
            else if (L1.plan == PlanType.Y && L2.plan == PlanType.Y)
            {

                var x = (L2.Y - L1.Y) / (L1.X - L2.X);
                var y = L1.X * x + L1.Y;
                //Console.WriteLine(" Y - Y");
                return new PointF(y, x);
            }
            else
            {
                return null;
            }
        }
       
    }
    public class PointF
    {
        public float X;
        public float Y;
        public float norm { get { return (float)Math.Sqrt(X * X + Y * Y); } }
        public PointF(float _x, float _y)
        {
            X = _x;
            Y = _y;
        }
        public PointF(double _x, double _y)
        {
            X = (float)_x;
            Y = (float)_y;
        }
        public PointF(PointF p)
        {
            X = p.X;
            Y = p.Y;
        }
        public PointF(Point p)
        {
            X = p.X;
            Y = p.Y;
        }
        public void normalize()
        {
            var n = (float)Math.Sqrt(X * X + Y * Y);
            if (n != 0)
            {
                X /= n;
                Y /= n;
            }
        }
        public static double operator *(PointF p, PointF p1)
        {
            return p.X * p1.X + p.Y * p1.Y;
        }

        public static double operator ^(PointF p, PointF p1)
        {
            return Math.Acos((p * p1) / (p.norm * p1.norm));
        }

        public static PointF operator *(PointF p, float k)
        {
            return new PointF(p.X * k, p.Y * k);
        }
        public static PointF operator +(PointF p, PointF p1)
        {
            return new PointF(p.X + p1.X, p.Y + p1.Y);
        }
        public static PointF operator -(PointF p, PointF p1)
        {
            return new PointF(p.X - p1.X, p.Y - p1.Y);
        }
        public override string ToString()
        {
            return X.ToString() + " " + Y.ToString()+";";
        }

    }
    public struct PointC
    {
        public Point point;
        public int count;
        public PointC(int Count, Point Point)
        {
            point = Point;
            count = Count;
        }
    }

    public struct Square
    {
        public float x;
        public float y;
        public float w;
        public float h;
        public Square(float _x, float _y, float _w, float _h)
        {
            x = _x;
            y = _y;
            w = _w;
            h = _h;
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