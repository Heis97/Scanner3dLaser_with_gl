using Accord.Math;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
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
    static public class UtilOpenCV
    {

        public static void saveImage(ImageBox box, string folder, string name)
        {
            var mat1 = (Mat)box.Image;
            saveImage(mat1, folder, name);
        }
        public static void saveImage(Mat mat1, string folder, string name)
        {
            if (mat1 != null)
            {
                Directory.CreateDirectory(folder);
                var im1 = mat1.ToImage<Bgr, byte>();
                Console.WriteLine(folder + "\\" + name);
                im1.Save(folder + "\\" + name);
            }
        }
        public static void saveImage(ImageBox box1, ImageBox box2, string name, string folder)
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
        static public Mat calcSubpixelPrec(Mat mat, Size size, GraphicGL graphicGL, float markSize)
        {
            var len = size.Width * size.Height;
            var obp = new MCvPoint3D32f[len];
            var cornF = new System.Drawing.PointF[len];
            var cornF_GL = new System.Drawing.PointF[len];
            var cornF_delt = new System.Drawing.PointF[len];
            var sum = new System.Drawing.PointF(0, 0);
            var kvs = new System.Drawing.PointF(0, 0);
            var S = new System.Drawing.PointF(0, 0);
            var ret = compChessCoords(mat, ref obp, ref cornF, size);
            if (!ret)
            {
                return null;
            }
            var mvpMtx = graphicGL.compMVPmatrix(graphicGL.transRotZooms[0]);
            for (int i = 0; i < obp.Length; i++)
            {
                var p_GL = graphicGL.calcPixel(new Vertex4f(markSize * obp[i].Y, -markSize * obp[i].X, obp[i].Z, 1), mvpMtx[3]);
                cornF_GL[i] = new System.Drawing.PointF(p_GL.X, p_GL.Y);
                var p_chess = cornF[i];
                cornF_delt[i] = new System.Drawing.PointF(p_GL.X - p_chess.X, p_GL.Y - p_chess.Y);
                sum.X += cornF_delt[i].X;
                sum.Y += cornF_delt[i].Y;
                //prin.t(cornF[i].ToString());
                //prin.t(cornF_GL[i].ToString());
                //prin.t("_________________");
            }

            for (int i = 0; i < obp.Length; i++)
            {
                kvs.X += (float)Math.Pow((cornF_delt[i].X - sum.X), 2);
                kvs.Y += (float)Math.Pow((cornF_delt[i].Y - sum.Y), 2);
            }
            S.X = kvs.X / len;
            S.Y = kvs.Y / len;
            Console.WriteLine(S);
            drawPointsF(mat, cornF, 255, 0, 0, 1);
            drawPointsF(mat, cornF_GL, 0, 0, 255, 1);
            return mat;
        }
        static public void drawPointsF(Mat im, System.Drawing.PointF[] points, int r, int g, int b, int size = 1)
        {
            drawPoints(im, PointF.toPoint(points), r, g, b, size);
        }
        static public void drawPoints(Mat im, Point[] points, int r, int g, int b, int size = 1)
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
        static public void drawPoints(Mat im, PointF[] points, int r, int g, int b, int size = 1)
        {
            drawPoints(im, PointF.toPoint(points), r, g, b, size);
        }

        static public void drawPoints(Mat im, System.Drawing.PointF[] points, int r, int g, int b, int size = 1)
        {
            drawPoints(im, PointF.toPoint(points), r, g, b, size);
        }
        static public void drawTours(Mat im, Point[] points, int r, int g, int b, int size = 4)
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
        static public void drawTours(Mat im, PointF[] d_points, int r, int g, int b, int size = 4)
        {
            if (d_points != null)
            {

                int ind = 0;
                var points = PointF.toPoint(d_points);
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
        static public void drawTours(Mat im, PointF[][] d_points, int r, int g, int b, int size = 4)
        {
            int ind = 0;
            foreach (var d_ps in d_points)
            {
                var points = PointF.toPoint(d_ps);
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
        static public void draw_tour(Point p1, int size, int ind, Mat im, int r, int g, int b)//size - размер креста
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
        static public TransRotZoom[] readTrz(string path)
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

        static public void generateImagesFromAnotherFolder(string[] paths, GraphicGL graphicGL, CameraCV cameraCV)
        {
            var trzs_L = new List<TransRotZoom[]>();
            foreach (var path in paths)
            {
                var trzs = readTrz(path);
                trzs_L.Add(trzs);

            }
            if (trzs_L.Count == 0)
            {
                return;
            }
            graphicGL.monitorsForGenerate = new int[] { 2, 3 };
            graphicGL.pathForSave = "virtual_stereo\\test2";
            //graphicGL.imageBoxesForSave = new ImageBox[] { imBox_mark1, imBox_mark2 };
            graphicGL.trzForSave = trzs_L;
            graphicGL.saveImagesLen = trzs_L[0].Length - 1;
            graphicGL.cameraCV = cameraCV;
            Console.WriteLine("GL1.saveImagesLen " + graphicGL.saveImagesLen);
        }

        static public void generateImagesFromAnotherFolderStereo(string[] paths, GraphicGL graphicGL, CameraCV cameraCV)
        {
            var trzs_L = new List<TransRotZoom[]>();
            int ind = 0;
            var offtrz = new TransRotZoom(0, 0, 0, 0, 0, 0, -0.02);
            var trzs_prev = readTrz(paths[0]);
            foreach (var path in paths)
            {
                if (ind == 0)
                {
                    var trzs = readTrz(path);
                    var trzs_clone = new TransRotZoom[trzs.Length];
                    for (int i = 0; i < trzs.Length; i++)
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
                    for (int i = 0; i < trzs_L[0].Length; i++)
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

            graphicGL.monitorsForGenerate = new int[] { 2, 3 };
            graphicGL.pathForSave = "virtual_stereo\\test5";
            //GL1.imageBoxesForSave = new ImageBox[] { imBox_mark1, imBox_mark2 };
            graphicGL.trzForSave = trzs_L;
            graphicGL.saveImagesLen = trzs_L[0].Length - 1;
            graphicGL.cameraCV = cameraCV;
            graphicGL.startGen = 1;
            Console.WriteLine("GL1.saveImagesLen " + graphicGL.saveImagesLen);
        }
        static public void SaveMonitor(object obj)
        {
            var graphicGL = (GraphicGL)obj;

            //Console.WriteLine("GL1.saveImagesLen " + GL.saveImagesLen);
            if (graphicGL.saveImagesLen >= 0 && graphicGL.startGen == 1)
            {

                var monitors = graphicGL.monitorsForGenerate;
                string newPath = graphicGL.pathForSave;
                int ind_im = graphicGL.saveImagesLen;
                var trzs_L = graphicGL.trzForSave;
                int ind = 0;
                foreach (var trzs in trzs_L)
                {
                    var indMonit = monitors[ind];
                    //Console.WriteLine("indMonit" + indMonit);
                    var trzMonitor = graphicGL.transRotZooms[indMonit];
                    trzMonitor.setTrz(trzs[ind_im]);
                    graphicGL.transRotZooms[indMonit] = trzMonitor;
                    graphicGL.SaveToFolder(newPath, indMonit);

                    var mat1 = remapDistImOpenCvCentr(graphicGL.matFromMonitor(indMonit), graphicGL.cameraCV.distortmatrix);
                    saveImage(mat1, newPath, Path.Combine("monitor_" + indMonit, trzMonitor.ToString() + ".png"));
                    ind++;
                }
                graphicGL.saveImagesLen--;
                if (graphicGL.saveImagesLen == -1)
                {
                    graphicGL.startGen = 0;
                }
            }

        }
        static public Mat generateImage(Size size)
        {
            var data = new byte[size.Height, size.Width, 1];
            int w = data.GetLength(1);
            int h = data.GetLength(0);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    data[j, i, 0] = 0;
                    if (i > 100 && i < 200)
                    {
                        if (j > 100 && j < 200)
                        {
                            data[j, i, 0] = 250;
                        }
                    }

                    if (i % 10 == 0)
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
        static public float[,] generateMap(Size size)
        {
            var data = new float[size.Height, size.Width];
            int w = data.GetLength(1);
            int h = data.GetLength(0);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (i > 190)
                    {
                        data[j, i] = i * 1.5f;
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

        static public float[,] generateMapY(Size size)
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
        static public Image<Gray, byte> mapToIm(float[,] map, PlanType planType, Mat mapy = null)
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

        static public Image<Gray, byte> mapToIm(Mat map, PlanType planType, Mat mapy = null)
        {
            return mapToIm((float[,])map.GetData(), planType, mapy);
        }


        static public Mat mapToMat(float[,] map)
        {
            int w = map.GetLength(1);
            int h = map.GetLength(0);
            var im = new Image<Gray, float>(w, h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    im.Data[j, i, 0] = map[j, i];
                }
            }
            return im.Mat;
        }
        static public byte[,,] normalyse(float[,,] mat)
        {
            int w = mat.GetLength(0);
            int h = mat.GetLength(1);
            int d = mat.GetLength(2);
            var koef = normalyseKoef(mat, 0, 255);
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

        static public float[] normalyseKoef(float[,,] mat, float minN, float maxN)//{ k, offset}
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
                        if (val > max)
                        {
                            max = val;
                        }
                        if (val < min)
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
            if (delt > 0)
            {
                K = deltN / delt;
            }
            var offset = min * K;//!!!!!!


            return new float[] { K, min };
        }

        static public MKeyPoint[] drawDescriptors(ref Mat mat)
        {
            var detector_ORB = new Emgu.CV.Features2D.ORBDetector(50);
            var detector_SURF = new Emgu.CV.Features2D.FastFeatureDetector();
            var kp = detector_ORB.Detect(mat);



            // matcher.Match()
            var desc_brief = new Emgu.CV.XFeatures2D.BriefDescriptorExtractor();
            //new VectorOfKeyPoint();
            var descrs = new Mat();


            // desc_brief.DetectAndCompute(mat, null, kp, descrs, false);
            //var mat_desc = new Mat();
            for (int i = 0; i < kp.Length; i++)
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
        static public Mat drawDescriptorsMatch(ref Mat mat1, ref Mat mat2)
        {
            var kps1 = new VectorOfKeyPoint();
            var desk1 = new Mat();
            var kps2 = new VectorOfKeyPoint();
            var desk2 = new Mat();
            var detector_ORB = new Emgu.CV.Features2D.ORBDetector();
            detector_ORB.DetectAndCompute(mat1, null, kps1, desk1, false);
            detector_ORB.DetectAndCompute(mat2, null, kps2, desk2, false);
            var matcher = new Emgu.CV.Features2D.BFMatcher(Emgu.CV.Features2D.DistanceType.L1, false);
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

        static public Mat epipolarTest(Mat matL, Mat matR)
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
            var p1 = 8 * imL.NumberOfChannels * blockSize * blockSize;
            var p2 = 32 * imL.NumberOfChannels * blockSize * blockSize;
            var stereo = new StereoSGBM(minDisparity, numDisparities, blockSize, p1, p2, disp12MaxDiff, 8, uniquenessRatio, speckleWindowSize, speckleRange, StereoSGBM.Mode.SGBM);
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
        static public Mat drawDisparityMap(ref Mat mat1, ref Mat mat2)
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
        static public Mat drawChessboard(Mat im, Size size)
        {
            var corn = new VectorOfPointF();
            var gray = im.ToImage<Gray, byte>();
            var ret = CvInvoke.FindChessboardCorners(gray, size, corn);
            Console.WriteLine("chess: " + ret + " " + size.Width + " " + size.Height);
            CvInvoke.DrawChessboardCorners(im, size, corn, ret);
            return im;
        }



        static public bool compChessCoords(Mat mat, ref MCvPoint3D32f[] obp, ref System.Drawing.PointF[] cornF, Size size)
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


        static public Mat remapDistIm(Mat mat, Matrix<double> matrixCamera, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();

            var roi = computeDistortionMaps(ref mapx, ref mapy, matrixCamera, matrixDistCoef, mat.Size);
            var invmap = remap(mapx, mapy, mat);
            //CvInvoke.Rectangle(invmap, roi, new MCvScalar(255, 0, 0));

            //return  new Mat(invmap,roi);
            return invmap;
        }
        static public Mat remapUnDistIm(Mat mat, Matrix<double> matrixCamera, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();
            var size = mat.Size;
            matrixCamera[0, 2] = size.Width / 2;
            matrixCamera[1, 2] = size.Height / 2;
            var matr = new Mat();
            CvInvoke.InitUndistortRectifyMap(matrixCamera, matrixDistCoef, null, matr, mat.Size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(mat, und_pic, mapx, mapy, Inter.Linear);
            return und_pic;
        }
        static public Mat remapDistImOpenCvCentr(Mat mat, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();
            var size = mat.Size;
            var matr = new Mat();
            var reversDistor = new Matrix<double>(5, 1);
            for (int i = 0; i < 5; i++)
            {
                reversDistor[i, 0] = -matrixDistCoef[i, 0];
            }
            double fov = 53;
            //_x = _z * Math.Tan(toRad(53 / 2))
            var fxc = size.Width / 2;
            var fyc = size.Height / 2;
            var f = size.Width / (2 * Math.Tan(UtilMatr.toRad((float)(fov / 2))));
            var matrixData = new double[3, 3] { { f, 0, fxc }, { 0, f, fyc }, { 0, 0, 1 } };
            var matrixData_T = matrixData.Transpose();
            var matrixCamera = new Matrix<double>(matrixData);
            // print(matrixCamera);
            CvInvoke.InitUndistortRectifyMap(matrixCamera, reversDistor, null, matr, size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(mat, und_pic, mapx, mapy, Inter.Linear);
            return und_pic;
        }
        static public Mat remapDistImOpenCv(Mat mat, Matrix<double> matrixCamera, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();
            var matr = new Mat();
            var size = mat.Size;
            var reversDistor = new Matrix<double>(5, 1);
            for (int i = 0; i < 5; i++)
            {
                reversDistor[i, 0] = -matrixDistCoef[i, 0];
            }
            CvInvoke.InitUndistortRectifyMap(matrixCamera, reversDistor, null, matr, size, DepthType.Cv32F, mapx, mapy);
            var und_pic = new Mat();
            CvInvoke.Remap(mat, und_pic, mapx, mapy, Inter.Linear);
            return und_pic;
        }
        static public Mat Minus(Mat mat1, Mat mat2)
        {
            var data1 = (byte[,,])mat1.GetData();
            var w1 = data1.GetLength(0);
            var h1 = data1.GetLength(1);
            var data2 = (byte[,,])mat2.GetData();
            var w2 = data2.GetLength(0);
            var h2 = data2.GetLength(1);
            var w = Math.Min(w1, w2);
            var h = Math.Min(h1, h2);

            var data = new byte[w, h, 1];
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

        static public Mat invRemap(float[,] mapx, float[,] mapy, Mat mat)
        {
            return remap(
                mapToMat(inverseMap(mapx, PlanType.X)),
                 mapToMat(inverseMap(mapy, PlanType.Y)),
                 mat);
        }
        static public Mat invRemap(Mat mapx, Mat mapy, Mat mat)
        {
            return invRemap(
                  mapToFloat(mapx),
                  mapToFloat(mapy),
                  mat);
        }
        static public float[,] mapToFloat(Mat map)
        {
            return (float[,])map.GetData();
        }

        static public Size findRemapSize(float[,] mapx, float[,] mapy)
        {
            var x = mapx.Max();// findMaxX(mapx);
            var y = mapy.Max();// findMaxY(mapy);
            Console.WriteLine("FLOAT SIZE: " + x + " " + y);
            var ix = (int)Math.Round(x, 0) + 4;
            var iy = (int)Math.Round(y, 0) + 4;
            return new Size(ix, iy);
        }


        static public float[,] inverseMap(float[,] map, PlanType planType)
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
                            var del = (map[j, i] - i) / k;
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
                            var del = (map[j, i] - j) / k;
                            inv_map[j, i] = j - del;
                        }
                    }
                    break;
            }
            return inv_map;
        }

        static public float[,,] compRemap(float[,] mapx, float[,] mapy, Mat mat)
        {
            var size = findRemapSize(mapx, mapy);
            var im = mat.ToImage<Bgr, byte>();
            Console.WriteLine("NEW SIZE: " + size.Width + " " + size.Height);
            var data = new float[size.Height, size.Width, 3];
            Console.WriteLine(data.GetLength(0) + " " + data.GetLength(1) +
                " " + mapx.GetLength(0) + " " + mapx.GetLength(1) +
                " " + im.Data.GetLength(0) + " " + im.Data.GetLength(1) + " ");
            var size_p = im.Size;
            for (int i = 1; i < size_p.Width - 2; i++)
            {
                for (int j = 1; j < size_p.Height - 2; j++)
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
                    var sq1 = new Square(x, y, w, h);
                    //Console.WriteLine("wh " + w + " " + h);
                    var ix = (int)Math.Round(x, 0);
                    var iy = (int)Math.Round(y, 0);

                    for (int _i = ix - 1; _i <= ix + 1; _i++)
                    {
                        for (int _j = iy - 1; _j <= iy + 1; _j++)
                        {
                            var sq2 = new Square(_i, _j, 1, 1);
                            var intens = compCrossArea(sq1, sq2);
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

        static public float compCrossArea(Square s1, Square s2)
        {
            var dx = compDelt(s1.w, s2.w, Math.Abs(s1.x - s2.x));
            var dy = compDelt(s1.h, s2.h, Math.Abs(s1.y - s2.y));
            return Math.Abs(dx * dy);
        }
        static public float compDelt(float w1, float w2, float dx)
        {
            float dw = 0;
            if (w2 < w1)
            {
                var lam = w1;
                w1 = w2;
                w2 = lam;
            }
            if (dx < (w1 + w2) / 2)//условие пересечения
            {
                if (dx > w2 / 2)
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
                    if (w1 <= w2 / 2)
                    {
                        dw = (w1 + w2) / 2 - dx;
                        return dw;
                    }
                    else
                    {
                        if ((w1 / 2 + dx) >= w2 / 2)
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

        static public byte[,,] toByte(float[,,] arr)
        {
            var byte_arr = new byte[arr.GetLength(0), arr.GetLength(1), arr.GetLength(2)];
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    for (int k = 0; k < arr.GetLength(2); k++)
                    {
                        if (arr[i, j, k] > 255)
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

        static public byte[,,] toByteGray(float[,,] arr)
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
        static public Mat remap(Mat _mapx, Mat _mapy, Mat mat)
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
                prin.t("color " + color);
            }

            if (color == 1)
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

        static public float[,] compUnsignedMap(float[,] map, PlanType planType)
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
            if (min < 0)
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



        static public PointF calcDistorcPix(int dim, int xd, int yd, double xc, double yc, Matrix<double> distCoefs)
        {


            var K1 = distCoefs[0, 0];
            var K2 = distCoefs[1, 0];
            var P1 = distCoefs[2, 0];
            var P2 = distCoefs[3, 0];
            var K3 = distCoefs[4, 0];

            var delt = new PointF((float)((double)xd - xc), (float)((double)yd - yc));
            var r = (double)delt.norm;
            var r2 = Math.Pow(r, 2);
            var r4 = Math.Pow(r, 4);
            var r6 = Math.Pow(r, 6);

            var delx = xd - xc;
            var dely = yd - yc;

            var xu = xd + delx * (K1 * r2 + K2 * r4 + K3 * r6) + P1 * (r2 + 2 * Math.Pow(delx, 2)) + 2 * P2 * delx * dely;
            var yu = yd + dely * (K1 * r2 + K2 * r4 + K3 * r6) + 2 * P1 * delx * dely + P2 * (r2 + 2 * Math.Pow(delx, 2));


            return new PointF(xu, yu);
        }
        static public PointF calcDistorcPix_BC(int dim, int _xd, int _yd, double _xc, double _yc, Matrix<double> distCoefs)
        {

            var K1 = distCoefs[0, 0];
            var K2 = distCoefs[1, 0];
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


            var _xu = xc + delx / (1 + K1 * r2 + K2 * r4 + K3 * r6);
            var _yu = yc + dely / (1 + K1 * r2 + K2 * r4 + K3 * r6);

            // var xu = xc + delx *(1 + K1 * r2 + K2 * r4 + K3 * r6);
            // var yu = yc + dely * (1 + K1 * r2 + K2 * r4 + K3 * r6);

            return new PointF(_xu * dim, _yu * dim);
        }

        static public float findMaxMin(float[,] map, int col, PlanType planType, int maxminF)//min - 0; max - 1;
        {
            float maxmin = 0;
            int b_i_ind = 0;
            int b_j_ind = 0;
            int e_i_ind = 0;
            int e_j_ind = 0;
            if (planType == PlanType.X)
            {
                b_i_ind = 0;
                e_i_ind = map.GetLength(0) - 1;

                b_j_ind = col;
                e_j_ind = col;
            }
            else if (planType == PlanType.Y)
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

            for (int i = b_i_ind; i <= e_i_ind; i++)
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
        static public Rectangle compROI(Mat mapx, Mat mapy)
        {
            return compROI((float[,])mapx.GetData(), (float[,])mapy.GetData());
        }
        static public Rectangle compROI(float[,] mapx, float[,] mapy)
        {
            float L = findMaxMin(mapx, 0, PlanType.X, 1);
            float R = findMaxMin(mapx, mapx.GetLength(1) - 1, PlanType.X, 0);

            float U = findMaxMin(mapy, 0, PlanType.Y, 1);
            float D = findMaxMin(mapy, mapx.GetLength(0) - 1, PlanType.Y, 0);

            //Console.WriteLine("L R U D " + L + " " + R + " " + U + " " + D + " ");
            return new Rectangle((int)L, (int)U, (int)(R - L), (int)(D - U));
        }
        static public Rectangle newRoi(Size size, double xc, double yc, Matrix<double> distCoefs, Func<int, int, int, double, double, Matrix<double>, PointF> calcDistPix)
        {
            var p1 = calcDistPix(size.Width, 0, 0, xc, yc, distCoefs);
            var p2 = calcDistPix(size.Width, size.Width, 0, xc, yc, distCoefs);
            var p3 = calcDistPix(size.Width, size.Width, size.Height, xc, yc, distCoefs);
            var p4 = calcDistPix(size.Width, 0, size.Height, xc, yc, distCoefs);
            prin.t(p1 + " " + p2 + " " + p3 + " " + p4 + " ");
            return RoiFrom4Points(p1, p2, p3, p4);
        }
        static public Rectangle RoiFrom4Points(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            int x = 0, y = 0, xW = 1, yH = 1;
            if (p1.X >= p4.X) { x = (int)p1.X; } else { x = (int)p4.X; }
            if (p1.Y >= p2.Y) { x = (int)p1.Y; } else { x = (int)p2.Y; }

            if (p3.X >= p2.X) { x = (int)p2.X; } else { x = (int)p3.X; }
            if (p3.Y >= p4.Y) { x = (int)p4.Y; } else { x = (int)p3.Y; }
            return new Rectangle(x, y, (xW - x), (yH - y));
        }
        static public Rectangle computeDistortionMaps(ref Mat _mapx, ref Mat _mapy, Matrix<double> cameraMatr, Matrix<double> distCoefs, Size size)
        {

            Matrix<float> mapx = new Matrix<float>(size.Height, size.Width);
            Matrix<float> mapy = new Matrix<float>(size.Height, size.Width);
            double xc = cameraMatr[0, 2];
            double yc = cameraMatr[1, 2];
            xc = size.Width / 2;
            yc = size.Height / 2;
            Console.WriteLine("---xcyc-- " + xc + " " + yc);
            // print(cameraMatr);
            //print(mapx);
            for (int i = 0; i < size.Height; i++)
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
            return compROI(_mapx, _mapy);

        }
        static public void distortFolder(string path, CameraCV cameraCV)
        {

            var frms = FrameLoader.loadImages_test(path);
            var distPath = Path.Combine(path, "distort");

            var fr1 = from f in frms
                      orderby f.dateTime.Ticks
                      select f;
            var vfrs = fr1.ToList();
            int ind = 0;
            foreach (var fr in vfrs)
            {
                var matD = remapDistIm(fr.im, cameraCV.cameramatrix, cameraCV.distortmatrix);
                saveImage(matD, distPath, ind + " " + fr.name);
                ind++;
            }
        }
    }
}
