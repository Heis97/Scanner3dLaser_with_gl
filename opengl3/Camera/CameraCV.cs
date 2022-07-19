using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Accord.Math.Decompositions;
using Accord.Math;

namespace opengl3
{
    //public class Pa
   public enum PatternType { Chess,Mesh};
    public class CameraCV
    {
        public Matrix<double> cameramatrix;
        public Matrix<double> cameramatrix_inv;

        public Matrix<double> distortmatrix;
        public Frame[] frames;
        public System.Drawing.PointF[][] corners;
        public MCvPoint3D32f[][] objps;
        public Mat[] tvecs;
        public Mat[] rvecs;
        public Mat cur_t;
        public Mat cur_r;
        public float[] pos;
        public Mat mapx;
        public Mat mapy;
        public Matrix<double> prjmatrix;
        public Matrix<double> prjmatrix_inv;
        public Matrix<double> matrixCS;
        public Matrix<double> matrixSC;
        public Size image_size;
        public float mark_size = 10f;

        #region monitCalib
        public static MCvPoint3D32f[][] generateObjps(ImageBox pattern_box, Mat[] pattern, bool gen_board = false, bool affine = true)
        {
            int num = 1;
            if (gen_board)
            {
                num = 2;
            }
            var matrs = new Matrix<double>[1];
            if (affine)
            {
                matrs = GetMatricesCalibAffine(pattern[0].Size, pattern_box.Size);
            }
            else
            {
                matrs = GetMatricesCalib();
            }

            var objps = new MCvPoint3D32f[matrs.Length][];
            for (int i = 0; i < matrs.Length; i++)
            {
                var ps = new System.Drawing.PointF[1];
                if (affine)
                {
                    ps = UtilOpenCV.transfAffine(UtilOpenCV.matToPointF(pattern[num]), matrs[i]);
                }
                else
                {
                    ps = UtilOpenCV.matToPointF(UtilOpenCV.warpPerspNorm(pattern, matrs[i], pattern_box.Size)[num]);
                }

                var ps3d = new MCvPoint3D32f[ps.Length];
                for (int j = 0; j < ps3d.Length; j++)
                {
                    ps3d[j] = new MCvPoint3D32f(ps[j].X, ps[j].Y, 0);
                }
                objps[i] = ps3d;
            }
            return objps;
        }
        async public static void calibrMonit(ImageBox pattern_box, ImageBox[] input, Mat[] pattern, string path, GraphicGL graphicGL, bool affine = true)
        {
            var matrs = new Matrix<double>[1];
            if (affine)
            {
                matrs = GetMatricesCalibAffine(pattern[0].Size, pattern_box.Size);
            }
            else
            {
                matrs = GetMatricesCalib();
            }
            var boards = new float[1][];

            if (graphicGL != null)
            {
                var p3d = generateObjps(pattern_box, pattern, true, affine);
                boards = UtilOpenCV.generate_BOARDs(p3d);
            }
            // prin.t(boards);
            for (int i = 0; i < matrs.Length; i++)
            {
                if (graphicGL != null)
                {
                    if (i != 0)
                    {
                        graphicGL.remove_buff_gl_id(i + 9);
                    }
                    //prin.t(i);
                    graphicGL.add_buff_gl_mesh_id(boards[i], i + 10, true);
                    await Task.Delay(100);
                    SaveImage_Chess(input, path, i);
                    await Task.Delay(100);
                }
                else
                {
                    await Task.Delay(500);
                    if (affine)
                    {
                        showAndSaveImage_Chess_affine_tr(matrs[i], pattern_box, input, pattern, path, i);
                    }
                    else
                    {
                        showAndSaveImage_Chess_persp_tr(matrs[i], pattern_box, input, pattern, path, i);
                    }
                    await Task.Delay(500);
                }
            }
        }
        static void SaveImage_Chess(ImageBox[] input, string path, int i)
        {
            for (int j = 0; j < input.Length; j++)
            {
                if (input[j].Image != null)
                {
                    var inp = (Mat)input[j].Image;
                    UtilOpenCV.saveImage(input[j], "cam" + (j + 1).ToString() + "\\" + path, i.ToString());
                }
            }
        }
        static void showAndSaveImage_Chess_affine_tr(Matrix<double> matrix, ImageBox pattern_box, ImageBox[] input, Mat[] pattern, string path, int i)
        {
            var pat_size = new Size(6, 7);
            var mat = pattern[0];
            var mat_aff = new Mat();
            int font = 255;
            CvInvoke.WarpAffine(mat, mat_aff, matrix, pattern_box.Size, Inter.Linear, Warp.Default, BorderType.Constant, new MCvScalar(font, font, font));
            pattern_box.Image = mat_aff;

            for (int j = 0; j < input.Length; j++)
            {
                if (input[j].Image != null)
                {
                    var inp = (Mat)input[j].Image;
                    //if (CvInvoke.FindChessboardCorners(inp.ToImage<Gray, byte>(), pat_size, new Mat()))
                    // {
                    UtilOpenCV.saveImage(input[j], "cam" + (j + 1).ToString() + "\\" + path, i.ToString());
                    //}
                }
            }
        }
        static void showAndSaveImage_Chess_persp_tr(Matrix<double> matrix, ImageBox pattern_box, ImageBox[] input, Mat[] pattern, string path, int i)
        {
            var pat_size = new Size(6, 7);
            pattern_box.Image = UtilOpenCV.warpPerspNorm(pattern, matrix, pattern_box.Size)[0];
            for (int j = 0; j < input.Length; j++)
            {
                if (input[j].Image != null)
                {
                    var inp = (Mat)input[j].Image;
                    if (CvInvoke.FindChessboardCorners(inp.ToImage<Gray, byte>(), pat_size, new Mat()))
                    {
                        UtilOpenCV.saveImage(input[j], "cam" + (j + 1).ToString() + "\\" + path, i.ToString());
                    }
                }
            }
        }
        static Matrix<double>[] GetMatricesCalib()
        {
            var p1 = 4; var p2 = 3;
            var matrs = new Matrix<double>[2 * p1 * p2];

            double a00 = 1; double a01 = 0; double a02 = 0;

            double a10 = 0; double a11 = 1; double a12 = 0;

            double a20 = 0; double a21 = 0; double a22 = 1;
            int ind = 0;
            var diap1 = 0.3;
            var diap2 = 0.0001;
            for (int i = 0; i < p1; i++)
            {
                for (int j = 0; j < p2; j++)
                {
                    a01 = calcCurA(i, p1, diap1);
                    a21 = calcCurA(j, p2, diap2);
                    matrs[ind] = new Matrix<double>(new double[3, 3] {
                        { a00, a01, a02 },
                        { a10, a11, a12 },
                        { a20, a21, a22 } }); ind++;
                }
            }
            for (int i = 0; i < p1; i++)
            {
                for (int j = 0; j < p2; j++)
                {
                    a10 = calcCurA(i, p1, diap1);
                    a20 = calcCurA(j, p2, diap2);
                    matrs[ind] = new Matrix<double>(new double[3, 3] {
                        { a00, a01, a02 },
                        { a10, a11, a12 },
                        { a20, a21, a22 } }); ind++;
                }
            }
            return matrs;
        }

        static Matrix<double>[] GetMatricesCalibAffine(Size pat_size, Size box_size)
        {

            var len = 9;
            double n = 3;
            double k_delim = 1.7;
            var matrs = new Matrix<double>[len];


            //Console.WriteLine("k " + k);
            double k1 = (double)box_size.Width / (k_delim * pat_size.Width);
            double k2 = (double)box_size.Height / (k_delim * pat_size.Height);
            double k = Math.Min(k1, k2);

            // Console.WriteLine("k " +k);
            double offx = (box_size.Width * k - box_size.Width) / n;
            double offy = (box_size.Height * k - box_size.Height) / n;
            int ind = 0;

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrs[ind] = affinematr(0.01, k, -i * offx, -j * offy);
                    ind++;
                }
            }
            // matrs[ind] = affinematr(0.01, 3 * k, 0, 0);

            return matrs;
        }
        static Matrix<double> affinematr(double alpha = 0, double k = 0, double offx = 0, double offy = 0)
        {

            return new Matrix<double>(new double[2, 3] {
                    { k*Math.Cos(alpha),-Math.Sin(alpha),  offx},
                    { Math.Sin(alpha),  k*Math.Cos(alpha),offy}
                    });
        }
        static double calcCurA(int i, int p, double diap)
        {
            var step = (2 * diap) / p;
            return -diap + step * i;
        }

        #endregion
        void init_vars()
        {
            cur_t = new Mat();
            cur_r = new Mat();
            matrixCS = new Matrix<double>(4, 4);
            matrixSC = new Matrix<double>(4, 4);
            pos = new float[3] { 0, 0, 0 };
        }
        public CameraCV(Matrix<double> _cameramatrix, Matrix<double> _distortmatrix)
        {
            cameramatrix = _cameramatrix;
            distortmatrix = _distortmatrix;
            init_vars();
        }
        public CameraCV(Frame[] _frames, Size _size, float markSize, MCvPoint3D32f[][] obp_inp)
        {
            calibrateCam(_frames, _size, markSize, obp_inp);
            //calibrateCamFish(_frames, _size, markSize, obp_inp);
            init_vars();
        }
        void setPos()
        {
            pos[0] = (float)matrixCS[0, 3];
            pos[1] = (float)matrixCS[1, 3];
            pos[2] = (float)matrixCS[2, 3];
        }
        public void setMatrixScene(Matr4x4f matrixSc)
        {
            matrixSC = matrixSc.ToOpenCVMatr();
            CvInvoke.Invert(matrixSC, matrixCS, DecompMethod.LU);
            cameramatrix_inv = new Matrix<double>(3, 3);
            CvInvoke.Invert(cameramatrix, cameramatrix_inv, DecompMethod.LU);

            prjmatrix = cameramatrix * matrixSC.GetRows(0, 3, 1);
            setPos();

            // prin.t(prjmatrix);
        }
        /*static public Matrix<double> assembMatrix(Mat rvec, Mat tvec)
        {
            var rotMatr = new Matrix<double>(3, 3);
            CvInvoke.Rodrigues(rvec, rotMatr);
            var t = (double[,])tvec.GetData();
            var trMatr = new Matrix<double>(t);
            var asMatr = rotMatr.ConcateHorizontal(trMatr);
            var lastRow = new double[1, 4] { { 0, 0, 0, 1 } };
            var lastMatr = new Matrix<double>(lastRow);
            return asMatr.ConcateVertical(lastMatr);
        }*/
        static public Matrix<double> invMatrix(Matrix<double> matr)
        {
            var inv_matr = new Matrix<double>(matr.Size);
            CvInvoke.Invert(matr, inv_matr, DecompMethod.LU);
            return inv_matr;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cur_r"></param>
        /// <param name="cur_t"></param>
        /// <returns>matr Cam->Scene,matr Scene->Cam</returns>
        static public Matrix<double>[] assemblMatrix(Mat cur_r, Mat cur_t)
        {

            var r = new Matrix<double>(3, 3);
            CvInvoke.Rodrigues(cur_r, r);
            var data_t = (double[,])cur_t.GetData();
            var data_mx = new double[4, 4]
            {
                {r[0,0] ,r[0,1],r[0,2],data_t[0,0]},
                {r[1,0] ,r[1,1],r[1,2],data_t[1,0]},
                {r[2,0] ,r[2,1],r[2,2],data_t[2,0]},
                {0,0,0,1 }
            };

            var matrSC = new Matrix<double>(data_mx);
            var matrCS = new Matrix<double>(4, 4);
            CvInvoke.Invert(matrSC, matrCS, DecompMethod.LU);
            return new Matrix<double>[] { matrCS, matrSC };

        }
        public float[] compPos(MCvPoint3D32f[] points3D, System.Drawing.PointF[] points2D)
        {
            //distortmatrix = new Matrix<double>(new double[,]{ { 0,0,0,0,0} });
            CvInvoke.SolvePnP(points3D, points2D, cameramatrix, distortmatrix, cur_r, cur_t);
            var matrs = assemblMatrix(cur_r, cur_t);
            matrixCS = matrs[0];
            matrixSC = matrs[1];

            var points_cam = PointCloud.fromLines(PointF.toPointF( points2D), this, LaserSurface.zeroFlatInCam(this.matrixSC));
            var points3d_cur = PointCloud.camToScene(points_cam, this.matrixCS);
            /*prin.t(this.matrixSC);
            prin.t(this.matrixCS);
            Console.WriteLine("points3d_cur");
            foreach (var p in points3d_cur) Console.WriteLine(p);*/
            setPos();
            return pos;
        }
        public bool compPos(Mat _mat, PatternType patternType,float mark = -1)
        {
            var mat = _mat.Clone();

            if (patternType == PatternType.Chess)
            {
                Size size_patt = new Size(6, 7);
                var mat1 = mat.Clone();
                var gray = mat.ToImage<Gray, byte>();
                var corn = new VectorOfPointF();

                float markSize = this.mark_size;
                if (mark >0)
                {
                    markSize = mark;
                }
                var obp = new MCvPoint3D32f[size_patt.Width * size_patt.Height];
                int ind = 0;
                for (int j = 0; j < size_patt.Height; j++)
                {
                    for (int i = size_patt.Width - 1; i >= 0; i--)
                    {
                        obp[ind] = new MCvPoint3D32f(markSize * (float)i, markSize * (float)j, 0.0f);
                        ind++;
                    }
                }
                var ret = CvInvoke.FindChessboardCorners(gray, size_patt, corn, CalibCbType.AdaptiveThresh);
                if (ret == true)
                {
                    //Console.WriteLine("CHESS TRUE");
                    CvInvoke.CornerSubPix(gray, corn, new Size(5, 5), new Size(-1, -1), new MCvTermCriteria(30, 0.001));
                    var corn2 = corn.ToArray();

                    var points2d = UtilOpenCV.takeGabObp(corn2, size_patt);
                    var points3d = UtilOpenCV.takeGabObp(obp, size_patt);
                    compPos(points3d, points2d);
                    //UtilOpenCV.drawPoints(mat1, points2d, points3d, 255, 0, 255, 2);
                    //CvInvoke.Imshow("pos", mat1);
                    return true;

                }
                else
                {
                    //Console.WriteLine("CHESS FALSE");
                }
            }
            else if (patternType == PatternType.Mesh)
            {
                Size size_patt = new Size(6, 7);
                float markSize = this.mark_size;
                if (mark > 0)
                {
                    markSize = mark;
                }

                var points3d = new MCvPoint3D32f[]
                {
                    new MCvPoint3D32f(0,0,0),
                    new MCvPoint3D32f(markSize*size_patt.Width,0,0),
                    new MCvPoint3D32f(0,markSize*size_patt.Height,0),
                    new MCvPoint3D32f(markSize*size_patt.Width,markSize*size_patt.Height,0)
                };

                var len = size_patt.Width * size_patt.Height;
                var cornF = new System.Drawing.PointF[len];
                var matDraw = FindCircles.findCircles(mat, cornF, size_patt);
                var points2d = UtilOpenCV.takeGabObp(cornF, size_patt);

                compPos(points3d, points2d);
                return true;
            }
            return false;
        }


        public Point3d_GL point3DfromCam(PointF _p)
        {
            var p =  (cameramatrix_inv * new Point3d_GL(_p.X, _p.Y, 1));
            return p;
        }


        public Mat undist(Mat mat)
        {
            Console.WriteLine("undist");
            //var mat_ret = new Mat();
            CvInvoke.Remap(mat, mat, mapx, mapy, Inter.Linear);
            return mat;
        }

        void calibrateCam(Frame[] frames, Size size, float markSize, MCvPoint3D32f[][] obp_inp)
        {
            this.frames = frames;
            this.mark_size = markSize;
            Matrix<double> _cameramatrix = new Matrix<double>(3, 3);
            Matrix<double> _distortmatrix = new Matrix<double>(5, 1);

            var objps = new List<MCvPoint3D32f[]>();
            var corners = new List<System.Drawing.PointF[]>();

            var obp = new MCvPoint3D32f[size.Width * size.Height];

            int ind = 0;
            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    obp[ind] = new MCvPoint3D32f(-markSize * (float)i, markSize * (float)j, 0.0f);
                    ind++;
                }
            }


            Console.WriteLine("fr len: " + frames.Length);
            int ind_fr = 0;
            foreach (var frame in frames)
            {
                var corn2 = findPoints(frame, size);
                if (corn2 == null)
                {
                    Console.WriteLine("NOT:");
                    Console.WriteLine(frame.name);
                }
                else
                {
                    objps.Add(obp);
                    corners.Add(corn2);
                }
                ind_fr++;
            }



            var rvecs = new Mat[corners.Count];
            var tvecs = new Mat[corners.Count];


            this.objps = objps.ToArray();
            this.corners = corners.ToArray();
            this.image_size = frames[0].im.Size;
            if (obp_inp != null)
            {
                this.objps = obp_inp;
            }
            Console.WriteLine("objp: " + this.objps.Length);
            Console.WriteLine("corn: " + this.corners.Length);
            //prin.t("_______________");
            //UtilOpenCV.printMatch(this.objps, this.corners);
            /* prin.t(this.objps);
             prin.t("_______________");
             prin.t(this.corners);*/


            var err = CvInvoke.CalibrateCamera(this.objps, this.corners, this.image_size, _cameramatrix, _distortmatrix, CalibType.Default, new MCvTermCriteria(100, 0.01), out rvecs, out tvecs);

            Console.WriteLine("err: " + err);
            Console.WriteLine("t,r_len: " + tvecs.Length + " " + rvecs.Length);
            var newRoI = new Rectangle();

            this.tvecs = tvecs;
            this.rvecs = rvecs;
            foreach (var v in tvecs)
            {
                //prin.t(v);
            }

            var matr = CvInvoke.GetOptimalNewCameraMatrix(_cameramatrix, _distortmatrix, frames[0].im.Size, 1, frames[0].im.Size, ref newRoI);

            //computeDistortionMaps(ref mapx, ref mapy, _cameramatrix, _distortmatrix, frames[0].im.Size);
            mapx = new Mat();
            mapy = new Mat();
            CvInvoke.InitUndistortRectifyMap(_cameramatrix, _distortmatrix, null, matr, frames[0].im.Size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(frames[0].im, und_pic, mapx, mapy, Inter.Linear);
            //_cameramatrix[0, 0] *= 2.25;
            //_cameramatrix[1, 1] *= 2.25;
            cameramatrix = _cameramatrix;
            cameramatrix_inv = invMatrix(cameramatrix);
            distortmatrix = _distortmatrix;
            
            var objpsn = new MCvPoint3D32f[0][];
            var cornersn = new System.Drawing.PointF[0][];
            //normalyseData(this.objps, this.corners, 640, 10, out objpsn, out cornersn);
            //calibrateCameraTest(this.objps, this.corners);
            prin.t("cameramatrix:");
            prin.t(cameramatrix);
            prin.t("distortmatrix:");
            prin.t(distortmatrix);

        }


        public  static System.Drawing.PointF[] findPoints(Mat mat, Size size_patt)
        {
            var gray = mat.ToImage<Gray, byte>();
            var corn = new VectorOfPointF();
            var ret = CvInvoke.FindChessboardCorners(gray, size_patt, corn);
            if (ret == true)
            {
                CvInvoke.CornerSubPix(gray, corn, new Size(5, 5), new Size(-1, -1), new MCvTermCriteria(30, 0.001));
                var corn2 = corn.ToArray();
                return corn2;
                /*if(obp_inp!=null)
                    {               
                        UtilOpenCV.drawMatches(mat1, corn2, UtilMatr.toPointF(obp_inp[ind_fr]), 255, 0, 0, 3);                       
                    }
                    else
                    {
                        CvInvoke.DrawChessboardCorners(mat1, size, corn, ret);
                        //UtilOpenCV.drawMatches(mat1, corn2, UtilMatr.toPointF(obp), 255, 0, 0, 3);
                    }
                    UtilOpenCV.drawMatches(mat1, corn2, UtilMatr.toPointF(obp_inp[ind_fr]), 255, 0, 0, 3);
                    CvInvoke.Imshow("asda", mat1);
                     CvInvoke.WaitKey();*/


                //Console.WriteLine(frame.name);
            }
            else
            {
                return null;
            }
        }
        public static System.Drawing.PointF[] findPoints(Frame frame, Size size_patt)
        {
            
            if (frame.frameType == FrameType.MarkBoard)
            {
                return findPoints(frame.im, size_patt);
            }
            else if (frame.frameType == FrameType.Pattern)
            {
                var len = size_patt.Width * size_patt.Height;
                var cornF = new System.Drawing.PointF[len];
                var mat = FindCircles.findCircles(frame.im, cornF, size_patt);
                //CvInvoke.Imshow("calib",mat);
                //CvInvoke.WaitKey();
                if (cornF == null)
                {
                    return null;
                }
                if (cornF.Length != len)
                {
                    return null;
                }
                else
                {
                    return cornF;
                }
            }
            else
            {
                return null;
            }

        }



        void normalyseData(MCvPoint3D32f[][] points3d, System.Drawing.PointF[][] points2d, float k2d, float k3d, out MCvPoint3D32f[][] npoints3d, out System.Drawing.PointF[][] npoints2d)
        {
            npoints3d = (MCvPoint3D32f[][])points3d.Clone();
            for (int i = 0; i < npoints3d.Length; i++)
            {
                npoints3d[i] = (MCvPoint3D32f[])points3d[i].Clone();
            }
            npoints2d = (System.Drawing.PointF[][])points2d.Clone();
            for (int i = 0; i < npoints3d.Length; i++)
            {
                for (int j = 0; j < npoints3d[i].Length; j++)
                {
                    Console.WriteLine("_____________________");
                    Console.WriteLine(points3d[i][j].X + " " + points3d[i][j].Y);
                    npoints3d[i][j] = new MCvPoint3D32f(points3d[i][j].X / k3d, points3d[i][j].Y / k3d, points3d[i][j].Z / k3d);
                    Console.WriteLine(npoints3d[i][j].X + " " + npoints3d[i][j].Y);
                }
            }
            Console.WriteLine("|||||||||||||||||||||||||||||||||||||");
            for (int i = 0; i < npoints2d.Length; i++)
            {
                for (int j = 0; j < npoints2d[i].Length; j++)
                {
                    //Console.WriteLine("_____________________");
                    //Console.WriteLine(points2d[i][j].X + " " + points2d[i][j].Y);
                    npoints2d[i][j] = new System.Drawing.PointF(npoints2d[i][j].X / k2d, npoints2d[i][j].Y / k2d);
                    //Console.WriteLine(npoints2d[i][j].X + " " + npoints2d[i][j].Y);
                }
            }

        }
        void calibrateCamFish(Frame[] frames, Size size, float markSize, MCvPoint3D32f[][] obp_inp)
        {
            this.frames = frames;

            Matrix<double> _cameramatrix = new Matrix<double>(3, 3);
            Matrix<double> _distortmatrix = new Matrix<double>(4, 1);

            var objps = new List<MCvPoint3D32f[]>();
            var corners = new List<System.Drawing.PointF[]>();
            size = new Size(size.Height, size.Width);
            var obp = new MCvPoint3D32f[size.Width * size.Height];

            int ind = 0;
            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    obp[ind] = new MCvPoint3D32f(markSize * ((float)i - size.Width / 2), markSize * ((float)j - size.Height / 2), 0.0f);
                    ind++;
                }
            }

            Console.WriteLine("FISH______________________________: " + frames.Length);
            Console.WriteLine("fr len: " + frames.Length);
            int ind_fr = 0;
            foreach (var frame in frames)
            {
                var corn2 = findPoints(frame, size);
                var im1 = frame.im.Clone();
                //UtilOpenCV.drawMatches(im1, UtilOpenCV.removeFromNegative( obp), corn2, 1, 0, 0);
                //CvInvoke.Imshow("yk"+ind_fr.ToString(), im1);
                if (corn2 == null)
                {
                    Console.WriteLine("NOT:");
                    Console.WriteLine(frame.name);
                }
                else
                {
                    objps.Add(obp);
                    corners.Add(corn2);
                }
                ind_fr++;
            }

            //CvInvoke.WaitKey();

            var rvecs = new Mat();
            var tvecs = new Mat();


            this.objps = objps.ToArray();
            this.corners = corners.ToArray();

            var p3d = new VectorOfVectorOfPoint3D32F(objps.GetRange(0, 10).ToArray());
            var p2d = new VectorOfVectorOfPointF(corners.GetRange(0, 10).ToArray());

            this.image_size = frames[0].im.Size;
            if (obp_inp != null)
            {
                this.objps = obp_inp;
            }
            Console.WriteLine("objp: " + this.objps.Length);
            Console.WriteLine("corn: " + this.corners.Length);
            //prin.t("_______________");
            //UtilOpenCV.printMatch(this.objps, this.corners);
            /* prin.t(this.objps);
             prin.t("_______________");
             prin.t(this.corners);*/
            //var err = CvInvoke.CalibrateCamera(this.objps, this.corners, this.image_size, _cameramatrix, _distortmatrix, CalibType.Default, new MCvTermCriteria(100, 0.01), out rvecs, out tvecs);


            Fisheye.Calibrate(p3d, p2d, this.image_size, _cameramatrix, _distortmatrix, rvecs, tvecs, Fisheye.CalibrationFlag.Default, new MCvTermCriteria(30, 0.01));

            //Console.WriteLine("err: " + err);
            //Console.WriteLine("t,r_len: " + tvecs.Length + " " + rvecs.Length);
            var newRoI = new Rectangle();

            //this.tvecs = tvecs;
            //this.rvecs = rvecs;
            prin.t(tvecs);
            var matrP = new Matrix<double>(3, 3);
            matrP[0, 0] = 1;
            matrP[1, 1] = 1;
            matrP[2, 2] = 1;
            var matrR = new Matrix<double>(3, 3);
            Fisheye.EstimateNewCameraMatrixForUndistorRectify(_cameramatrix, _distortmatrix, this.image_size, matrR, matrP);
            var mapx = new Mat();
            var mapy = new Mat();
            /* var matr = CvInvoke.GetOptimalNewCameraMatrix(_cameramatrix, _distortmatrix, frames[0].im.Size, 1, frames[0].im.Size, ref newRoI);

             //computeDistortionMaps(ref mapx, ref mapy, _cameramatrix, _distortmatrix, frames[0].im.Size);
             mapx = new Mat();
             mapy = new Mat();
             CvInvoke.InitUndistortRectifyMap(_cameramatrix, _distortmatrix, null, matr, frames[0].im.Size, DepthType.Cv32F, mapx, mapy);

             var und_pic = new Mat();
             CvInvoke.Remap(frames[0].im, und_pic, mapx, mapy, Inter.Linear);
             cameramatrix = _cameramatrix;
             cameramatrix_inv = invMatrix(cameramatrix);
             distortmatrix = _distortmatrix;*/

            Fisheye.InitUndistorRectifyMap(_cameramatrix, _distortmatrix, matrR, matrP, frames[0].im.Size, DepthType.Cv32F, mapx, mapy);
            //prin.t(mapx);
            prin.t(" matrR:");
            prin.t(matrR);
            prin.t(" matrP:");
            prin.t(matrP);
            var im = UtilOpenCV.mapToMat(UtilOpenCV.mapToFloat(mapx));
            CvInvoke.Imshow("mapx", im);
            var und_pic = new Mat();
            CvInvoke.Remap(frames[0].im, und_pic, mapx, mapy, Inter.Linear);
            CvInvoke.Imshow("orig", frames[0].im);
            CvInvoke.Imshow("undist", und_pic);
            // CvInvoke.WaitKey();
            prin.t("Fish cameramatrix:");
            prin.t(_cameramatrix);
            prin.t("Fish distortmatrix:");
            prin.t(_distortmatrix);

        }


        #region test calib
        void EmguCVUndistortFisheye(string path, Size patternSize)
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
            prin.t("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _");
            prin.t(objPoints);
            prin.t("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _");
            prin.t(imagePoints);
            prin.t("_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _");
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
            prin.t("K:");
            prin.t(K);
            prin.t("D:");
            prin.t(D);

            prin.t("calib done");
            foreach (string file in fileNames)
            {
                Mat img = CvInvoke.Imread(file, ImreadModes.Grayscale);
                Mat output = img.Clone();
                Fisheye.UndistorImage(img, output, K, D);
                CvInvoke.Imshow("output", output);
            }
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
                var gray = frame.im.ToImage<Gray, byte>();
                var ret = CvInvoke.FindChessboardCorners(gray, size, corn);
                if (ret == true)
                {

                    CvInvoke.CornerSubPix(gray, corn, new Size(11, 11), new Size(-1, -1), new MCvTermCriteria(100, 0.0001));
                    UtilOpenCV.draw_tour(new Point((int)corn[0].X, (int)corn[0].Y), 3, 0, frame.im, 255, 0, 0);
                    //CvInvoke.Imshow(frame.name, frame.im);
                    //CvInvoke.WaitKey(500);
                    var corn2 = corn.ToArray();
                    int ind1 = 0;
                    for (int i = 0; i < corn2.Length; i++)
                    {
                        // matcorners.Data[indf, i, 0] = corn2[i].X;
                        //matcorners.Data[indf, i, 1] = corn2[i].Y;

                        imcorndata[ind1, indf, 0] = corn2[i].X; ind1++;
                        imcorndata[ind1, indf, 0] = corn2[i].Y; ind1++;
                        prin.t(i);

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
            var m1 = matcorners1.Mat.Reshape(2, 10);

            var matobjp = new Image<Bgr, double>(42, 10);

            int ind = 0;
            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    for (int k = 0; k < corners.Size; k++)
                    {
                        matobjp.Data[k, ind, 0] = i;
                        matobjp.Data[k, ind, 1] = j;
                    }

                    //Console.WriteLine(i + " " + j);
                    ind++;
                }
            }
            var m2 = matobjp.Mat;
            prin.t(corners.Size);

            distortmatrix = new Matrix<double>(4, 1);
            var m3 = m2.Reshape(3, 42);
            var m4 = m1.Reshape(2, 42);

            var K = new Mat();
            var D = new Mat();
            var tvec = new Mat();
            var rvec = new Mat();
            Fisheye.Calibrate(objps, corners, frames[0].im.Size, cameramatrix, distortmatrix, rvec, tvec, Fisheye.CalibrationFlag.Default, new MCvTermCriteria(30, 0.1));
            //Fisheye.Calibrate(m2, m1, frames[0].im.Size, cameraMatrix, cameraDistortionCoeffs, rvecs, tvecs, Fisheye.CalibrationFlag.Default, new MCvTermCriteria(30, 0.001));
            prin.t(tvec);
            prin.t("_____________TVECS ^^^^^^");
            var matrP = new Matrix<double>(3, 3);
            var matrR = new Matrix<double>(3, 3);
            Fisheye.EstimateNewCameraMatrixForUndistorRectify(cameramatrix, distortmatrix, frames[0].im.Size, matrR, matrP);
            var mapx = new Mat();
            var mapy = new Mat();
            Console.WriteLine("cameraDistortionCoeffs:");
            prin.t(distortmatrix);
            Console.WriteLine("cameraMatrix:");
            prin.t(cameramatrix);
            /*cameraDistortionCoeffs[0, 0] = -0.5 * Math.Pow(10, 0);
            cameraDistortionCoeffs[1, 0] = 0;
            cameraDistortionCoeffs[2, 0] = 0;
            cameraDistortionCoeffs[3, 0] = 0;
            cameraDistortionCoeffs[4, 0] = 0;*/
            //  computeDistortionMaps(ref mapx, ref mapy, cameraMatrix, cameraDistortionCoeffs, frames[0].im.Size);
            Console.WriteLine("||||||||||||");
            Console.WriteLine(mapx.Depth);
            prin.t("matr:");

            prin.t(matrP);
            // Console.WriteLine("MAPX_________________");
            //print(mapx);
            // cameraDistortionCoeffs[0, 0] = 0.5;
            Fisheye.InitUndistorRectifyMap(cameramatrix, distortmatrix, matrR, matrP, frames[0].im.Size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(frames[0].im, und_pic, mapx, mapy, Inter.Linear);
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

        void calibrateCameraTest(MCvPoint3D32f[][] points3d, System.Drawing.PointF[][] points2d)
        {
            var matrsH = new List<Matrix<double>>();
            var matrsSubV = new List<Matrix<double>>();
            var matrsSubV9 = new List<Matrix<double>>();
            var p2ds = new List<System.Drawing.PointF[]>();
            var p3ds = new List<MCvPoint3D32f[]>();
            var Ainv = new Matrix<double>(3, 3);
            
            CvInvoke.Invert(cameramatrix, Ainv, DecompMethod.LU);
            var B = Ainv.Transpose() * Ainv;
            for (int i = 0; i < points3d.Length; i++)
            {
                var p2d = UtilOpenCV.takeGabObp(points2d[i], new Size(6, 7));
                var p3d = UtilOpenCV.takeGabObp(points3d[i], new Size(6, 7));
                var hMatr = homographyMatr(p3d, p2d);
                var hMat = CvInvoke.FindHomography(PointF.toSystemPoint(points3d[i]), points2d[i]);
                prin.t("ps_____________");
                for (int p =0; p< p2d.Length;p++)
                {
                    prin.t(p2d[p].X + " " + p2d[p].Y + " ");
                    prin.t(p3d[p].X + " " + p3d[p].Y + " ");
                }
                prin.t("homo_____________");
                var hMatrcv = new Matrix<double>((double[,])hMat.GetData());
                var ch1 = hMatrcv * new Matrix<double>(new double[,] { { p3d[3].X }, { p3d[3].Y }, { 1 } });
                prin.t(ch1/ch1[2,0]) ;
                prin.t(hMatr* new Matrix<double>(new double[,] { { p3d[3].X }, { p3d[3].Y }, { 1 } }));
                prin.t(p2d[3].X+" "+ p2d[3].Y + " ");
                
                var h1 = hMatrcv.GetCol(0);
                var h2 = hMatrcv.GetCol(1);
                var h1T = h1.Transpose();
                var h2T = h2.Transpose();
                prin.t("hMatr_____________");
                prin.t(hMatr);
                prin.t("h1_____________");
                prin.t(h1);
                prin.t("h2_____________");
                prin.t(h2);
                prin.t("h1T_____________");
                prin.t(h1T);
                prin.t("h2T_____________");
                prin.t(h2T);
                prin.t("A_____________");
                prin.t(cameramatrix);
                prin.t("B_____________");
                prin.t(B);
                prin.t("h1T*B*h2_____________");
                prin.t(h1T*B*h2);
                prin.t("h1T*B*h1_____________");
                prin.t(h1T * B * h1);
                prin.t("h2T*B*h2_____________");
                prin.t(h2T * B * h2);
                //prin.t("_____________");
                //prin.t(hMatr);
                //prin.t(new Matrix<double>( (double[,])hMat.GetData()));
                p2ds.Add(p2d);
                p3ds.Add(p3d);
                var subVmatr = matrix_subV(hMatrcv);
                var subVmatr9 = matrix_subV9(hMatr);
                matrsH.Add(hMatrcv);
                matrsSubV.Add(subVmatr);
                matrsSubV9.Add(subVmatr9);
                //
            }
            var matrV9 = matrix_V9(matrsSubV9.GetRange(0, matrsSubV.Count / 2).ToArray());
            
            var matrV1 = matrix_V(matrsSubV.GetRange(1, matrsSubV.Count / 2).ToArray());
            var matrV2 = matrix_V(matrsSubV.GetRange(matrsSubV.Count / 2, matrsSubV.Count / 2).ToArray());
            var w1 = new Mat();
            var u1 = new Mat();
            var v1 = new Mat();
            Console.WriteLine("matrsSubV.Count " + matrsSubV.Count);

            for (int i=0; i< matrsSubV.Count-3;i++)
            {;
                var matrV = matrix_V(matrsSubV.GetRange(i,  3).ToArray());
                var vec = vecWithSmallestEigevVal(matrV);
                var intr = calcIntrisicParam(vec.Transpose());
                var Iinv = new Matrix<double>(3, 3);

                CvInvoke.Invert(intr, Iinv, DecompMethod.LU);
                var iB = Iinv.Transpose() * Iinv;

                Console.WriteLine("Intr ___________");
                prin.t(intr);

                for(int hm =0; hm< matrsH.Count;hm++)
                {
                    var h1 = matrsH[hm].GetCol(0);
                    var h2 = matrsH[hm].GetCol(1);
                    var h1T = h1.Transpose();
                    var h2T = h2.Transpose();
                    prin.t("h1T*B*h2_____________");
                    prin.t(h1T * iB * h2);
                    prin.t("h1T*B*h1 - h2T*B*h2_____________");
                    prin.t(h1T * iB * h1 - h2T * iB * h2);

                }
                
            }
            CvInvoke.SVDecomp(matrV1, w1, u1, v1, SvdFlag.FullUV);
            //prin.t("matrV1_____________");
            //prin.t(matrV1);
            //CvInvoke.cal
            prin.t("w1_____________");
            prin.t(w1);
            //prin.t("u1_____________");
            //prin.t(u1);
            prin.t("v1T_____________");
            prin.t(v1.T());
            var vec1 = vecWithSmallestEigevVal(matrV1);
            var vec2 = vecWithSmallestEigevVal(matrV2);
            prin.t("matrV1_____________");
            prin.t(matrV1);
            prin.t("matrV2_____________");
            prin.t(matrV2);
            prin.t("vec1_____________");
            prin.t(vec1);
            prin.t("vec2_____________");
            prin.t(vec2);

            //var b = calcBmatrParam(cameramatrix);
            var intr1 = calcIntrisicParam(vec1.Transpose());
            var intr2 = calcIntrisicParam(vec2.Transpose());
            Console.WriteLine("Intr ___________");
            prin.t(intr1);


            var b = calcBmatrParam(cameramatrix);

        }

       
        Matrix<double> vecWithSmallestEigevVal(Matrix<double> matr1)
        {

            var matr = matr1.Transpose() * matr1;
           // prin.t("matr_____________");
            //prin.t(matr.Data.GetLength(0) + " " + matr.Data.GetLength(1) + " ");
            var eig = new EigenvalueDecomposition(matr.Data);
            var eiVec = new Matrix<double>(eig.Eigenvectors);
            var eiVal = new Matrix<double>(eig.RealEigenvalues);
            var pmax = new Point();
            var pmin = new Point();
            var vmax = 0d;
            var vmin = 0d;

            eiVal.MinMax(out vmin, out vmax, out pmin, out pmax);
            var minVec = new Matrix<double>(eiVec.Rows, 1);
            var maxVec = new Matrix<double>(eiVec.Rows, 1);
            for (int i = 0; i < minVec.Rows; i++)
            {
                minVec[i, 0] = eiVec[i, pmin.Y];
                maxVec[i, 0] = eiVec[i, pmax.Y];
            }
            


            minVec = eiVec.GetCol(pmin.Y);
            //calcIntrisicParam(minVec.Transpose());
            var matrD = matr.Data;
            

           /*prin.t("eiVec_____________");
            prin.t(eiVec);
            prin.t("eiVal_____________");
            prin.t(eiVal);
            prin.t("minVec_____________");
            prin.t(minVec);
            prin.t("matr1 * minVec_____________");
            prin.t(matr1 * minVec);*/



            return minVec;
        }


        Matrix<double> matrix_V(Matrix<double>[] matr)
        {
            var matr_res = new Matrix<double>(3, 3);
            if (matr.Length > 2)
            {
                matr_res = matr[0];
                for (int i = 1; i < 3; i++)
                {
                    matr_res = matr_res.ConcateVertical(matr[i]);
                }
            }
            return matr_res;
        }

        Matrix<double> matrix_vij(Matrix<double> matr, int i, int j)
        {
            return new Matrix<double>(new double[,] {
                { matr[i,0]* matr[j, 0] },
                { matr[i,0]* matr[j, 1] + matr[i,1]* matr[j, 0] },
                { matr[i,1]* matr[j, 1]},
                { matr[i,2]* matr[j, 0] + matr[i,0]* matr[j, 2] },
                { matr[i,2]* matr[j, 1] + matr[i,1]* matr[j, 2]},
                { matr[i,2]* matr[j, 2]}
            });
        }


        Matrix<double> matrix_subV(Matrix<double> matr)
        {
            //prin.t("hMatr____________________");
            //prin.t(matr);
            var v12 = matrix_vij(matr, 0, 1);
            var v11 = matrix_vij(matr, 0, 0);
            var v22 = matrix_vij(matr, 1, 1);
            /*prin.t("v12____________________");
            prin.t(v12);
            prin.t("v11____________________");
            prin.t(v11);
            prin.t("v22____________________");
            prin.t(v22);*/
            var matr1 = v12.Transpose();
            var matr2 = (v11 - v22).Transpose();
            /*prin.t("m1____________________");
            prin.t(matr1);
            prin.t("m2____________________");
            prin.t(matr2);*/
            var matr_res = matr1.ConcateVertical(matr2);

            return matr_res;
        }
        Matrix<double> matrix_vij9(Matrix<double> matr, int i, int j)
        {
            return new Matrix<double>(new double[,] {
                { matr[i,0]* matr[j, 0] },
                { matr[i,0]* matr[j, 1] },
                { matr[i,0]* matr[j, 2] },
                { matr[i,1]* matr[j, 0] },
                { matr[i,1]* matr[j, 1] },
                { matr[i,1]* matr[j, 2] },
                { matr[i,2]* matr[j, 0] },
                { matr[i,2]* matr[j, 1] },
                { matr[i,2]* matr[j, 2] },
            });
        }

        Matrix<double> matrix_subV9(Matrix<double> matr)
        {
            var v12 = matrix_vij9(matr, 0, 1);
            var v11 = matrix_vij9(matr, 0, 0);
            var v22 = matrix_vij9(matr, 1, 1);
            var matr1 = v12.Transpose();
            var matr2 = (v11 - v22).Transpose();
            var matr_res = matr1.ConcateVertical(matr2);

            return matr_res;
        }

        Matrix<double> matrix_V9(Matrix<double>[] matr)
        {
            var matr_res = new Matrix<double>(3, 3);
            if (matr.Length > 2)
            {
                matr_res = matr[0];
                for (int i = 1; i < 5; i++)
                {
                    matr_res = matr_res.ConcateVertical(matr[i]);
                }
            }
            if (matr_res.Rows > 9)
            {

                var data = matr_res.Data;
                var data9 = new double[9, 9];
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        data9[i, j] = data[i, j];
                    }
                }
                matr_res = new Matrix<double>(data9);
            }
            return matr_res;
        }

        Matrix<double> homographyMatr(MCvPoint3D32f[] points3d, System.Drawing.PointF[] points2d)
        {
            Matrix<double> matrix3d = new Matrix<double>(3, 3);
            Matrix<double> matrix2d = new Matrix<double>(3, 3);
            Matrix<double> matrix = new Matrix<double>(3, 3);
            if (points3d.Length > 3 && points2d.Length > 3)
            {
                for (int i = 0; i < 3; i++)
                {
                    matrix3d[i, 0] = points3d[i].X;
                    matrix3d[i, 1] = points3d[i].Y;
                    matrix3d[i, 2] = 1;

                    matrix2d[i, 0] = points2d[i].X;
                    matrix2d[i, 1] = points2d[i].Y;
                    matrix2d[i, 2] = 1;
                }
                var K = new Matrix<double>(3, 3);
                CvInvoke.Invert(matrix3d, K, DecompMethod.LU);
                var matrix1 = K * matrix2d;
                //prin.t(matrix);

                for (int i = 0; i < 3; i++)
                {
                    matrix3d[i, 0] = points3d[i + 1].X;
                    matrix3d[i, 1] = points3d[i + 1].Y;
                    matrix3d[i, 2] = 1;

                    matrix2d[i, 0] = points2d[i + 1].X;
                    matrix2d[i, 1] = points2d[i + 1].Y;
                    matrix2d[i, 2] = 1;
                }
                K = new Matrix<double>(3, 3);
                CvInvoke.Invert(matrix3d, K, DecompMethod.LU);
                var matrix2 = K * matrix2d;

                matrix = (matrix1 + matrix2) / 2;
            }
            return matrix.Transpose();
        }

        Matrix<double> solveMatr(Matrix<double> matrix)
        {
            var rcol = remove(matrix, matrix.Rows , matrix.Cols, 0, matrix.Cols - 1)*(-1);
            var matrixsmall = remove(matrix, matrix.Rows-1, matrix.Cols-1);
            var revmatrixsmall = new Matrix<double>(matrixsmall.Size);
            CvInvoke.Invert(matrixsmall, revmatrixsmall, DecompMethod.LU);
            var rcols = remove(matrix, matrix.Rows-1, matrix.Cols,  0, matrix.Cols - 1) * (-1);
            var solv = (revmatrixsmall * rcols).ConcateVertical(new Matrix<double>(new double[,] { { 1 } }));
            return solv;
        }

        Matrix<double> remove(Matrix<double> matrix, int rows, int cols,int strow =0,int stcol=0)
        {
            var data = matrix.Data;
            var dataR = new double[rows- strow, cols-stcol];
            int isl = 0;
            for (int i = strow; i < rows; i++)
            {
                int jsl = 0;
                for (int j = stcol; j < cols; j++)
                {
                    dataR[isl, jsl] = data[i, j];
                    jsl++;
                }
                isl++;
            }
            return new Matrix<double>(dataR);
        }
        Matrix<double> calcIntrisicParam(Matrix<double> eigenvector)
        {
            var B11 = 0d;
            var B12 = 0d;
            var B22 = 0d;
            var B13 = 0d;
            var B23 = 0d;
            var B33 = 0d;
            if (eigenvector.Cols == 6)
            {
                B11 = eigenvector[0, 0];
                B12 = eigenvector[0, 1];
                B22 = eigenvector[0, 2];
                B13 = eigenvector[0, 3];
                B23 = eigenvector[0, 4];
                B33 = eigenvector[0, 5];
            }
            else if(eigenvector.Cols == 9)
            {
                B11 = eigenvector[0, 0];
                B12 = eigenvector[0, 1];
                B13 = eigenvector[0, 2];

                B22 = eigenvector[0, 4];               
                B23 = eigenvector[0, 5];


                B33 = eigenvector[0, 8];
            }
            else
            {

            }

                var v0 = (B12 * B13 - B11 * B23) / (B11 * B22 - B12 * B12);
                var lam = B33 - (B13 * B13 + v0 * (B12 * B13 - B11 * B23)) / B11;
                var alph = Math.Sqrt(lam / B11);
                var beta = Math.Sqrt((lam * B11) / (B11 * B22 - B12 * B12));
                var gamm = -B12 * alph * alph * beta / lam;
                var u0 = gamm * v0 / beta - B13 * alph * alph / lam;
                /*Console.WriteLine("Intr ___________");
                Console.WriteLine("v0 = " + v0);
                Console.WriteLine("lam = " + lam);
                Console.WriteLine("alph = " + alph);
                Console.WriteLine("beta = " + beta);
                Console.WriteLine("gamm = " + gamm);
                Console.WriteLine("u0 = " + u0);*/

            return new Matrix<double>(new double[,]
            {
                {alph,gamm,u0 },
                {0,beta,v0 },
                {0,0,lam },
            })/lam;
        }

        Matrix<double> calcBmatrParam(Matrix<double> A)
        {
            var alph = A[0, 0];
            var gamma = A[0, 1];
            var u0 = A[0, 2];
            var beta = A[1, 1];
            var v0 = A[1, 2];

            var B11 = 1 / (alph * alph);
            var B12 =- gamma / (alph * alph*beta);
            var B13 = (v0*gamma-u0*beta )/ (alph * alph * beta);

            var B22 = (gamma* gamma) / (alph * alph * beta * beta)+1/ (beta * beta);

            var B23 = -(gamma*( v0* gamma-u0*beta)) / (alph * alph * beta * beta) - v0 / (beta * beta);

            var B33 = ((v0 * gamma - u0 * beta) * (v0 * gamma - u0 * beta)) / (alph * alph * beta * beta) + (v0*v0) / (beta * beta)+1;



            Console.WriteLine("B11 = " + B11);
            Console.WriteLine("B12 = " + B12);
            Console.WriteLine("B22 = " + B22);
            Console.WriteLine("B13 = " + B13);
            Console.WriteLine("B23 = " + B23);
            Console.WriteLine("B33 = " + B33);
            var b = new Matrix<double>(new double[,] { { B11, B12, B22, B13, B23, B33 } });
            var intr = calcIntrisicParam(b);
            prin.t("intrTes:____________");
            prin.t(intr);

            return b;
        }

        public static class LinearEquationSolver
        {
            /// <summary>Computes the solution of a linear equation system.</summary>
            /// <param name="M">
            /// The system of linear equations as an augmented matrix[row, col] where (rows + 1 == cols).
            /// It will contain the solution in "row canonical form" if the function returns "true".
            /// </param>
            /// <returns>Returns whether the matrix has a unique solution or not.</returns>
            public static bool Solve(double[,] M)
            {
                prin.t("gauss_____________");
                prin.t(M);
                // input checks
                int rowCount = M.GetUpperBound(0) + 1;
                if (M == null || M.Length != rowCount * (rowCount + 1))
                    throw new ArgumentException("The algorithm must be provided with a (n x n+1) matrix.");
                if (rowCount < 1)
                    throw new ArgumentException("The matrix must at least have one row.");

                // pivoting
                for (int col = 0; col + 1 < rowCount; col++) if (M[col, col] == 0)
                    // check for zero coefficients
                    {
                        // find non-zero coefficient
                        int swapRow = col + 1;
                        for (; swapRow < rowCount; swapRow++) if (M[swapRow, col] != 0) break;

                        if (M[swapRow, col] != 0) // found a non-zero coefficient?
                        {
                            // yes, then swap it with the above
                            double[] tmp = new double[rowCount + 1];
                            for (int i = 0; i < rowCount + 1; i++)
                            { tmp[i] = M[swapRow, i]; M[swapRow, i] = M[col, i]; M[col, i] = tmp[i]; }
                        }
                        else return false; // no, then the matrix has no unique solution
                    }

                // elimination
                for (int sourceRow = 0; sourceRow + 1 < rowCount; sourceRow++)
                {
                    for (int destRow = sourceRow + 1; destRow < rowCount; destRow++)
                    {
                        double df = M[sourceRow, sourceRow];
                        double sf = M[destRow, sourceRow];
                        for (int i = 0; i < rowCount + 1; i++)
                            M[destRow, i] = M[destRow, i] * df - M[sourceRow, i] * sf;
                    }
                }
                prin.t("gauss_____________");
                prin.t(M);
                // back-insertion
                for (int row = rowCount - 1; row >= 0; row--)
                {
                    double f = M[row, row];
                    if (f == 0) return false;

                    for (int i = 0; i < rowCount + 1; i++) M[row, i] /= f;
                    for (int destRow = 0; destRow < row; destRow++)
                    { M[destRow, rowCount] -= M[destRow, row] * M[row, rowCount]; M[destRow, row] = 0; }
                }
                prin.t("gauss_____________");
                prin.t(M);
                return true;
            }

            public static bool SolveSq(double[,] M)
            {
                prin.t("gauss_____________");
                prin.t(M);
                // input checks
                int rowCount = M.GetLength(0);
                Console.WriteLine(rowCount +" "+ M.Length);
                if (M == null || M.Length != rowCount * rowCount )
                    throw new ArgumentException("The algorithm must be provided with a n x n matrix.");
                if (rowCount < 1)
                    throw new ArgumentException("The matrix must at least have one row.");

                // pivoting
                for (int col = 0; col + 1 < rowCount; col++) if (M[col, col] == 0)
                    // check for zero coefficients
                    {
                        // find non-zero coefficient
                        int swapRow = col + 1;
                        for (; swapRow < rowCount; swapRow++) if (M[swapRow, col] != 0) break;

                        if (M[swapRow, col] != 0) // found a non-zero coefficient?
                        {
                            // yes, then swap it with the above
                            double[] tmp = new double[rowCount + 1];
                            for (int i = 0; i < rowCount + 1; i++)
                            { 
                                tmp[i] = M[swapRow, i]; 
                                M[swapRow, i] = M[col, i]; 
                                M[col, i] = tmp[i]; 
                            }
                        }
                        else return false; // no, then the matrix has no unique solution
                    }

                // elimination
                for (int sourceRow = 0; sourceRow + 1 < rowCount; sourceRow++)
                {
                    for (int destRow = sourceRow + 1; destRow < rowCount; destRow++)
                    {
                        double df = M[sourceRow, sourceRow];
                        double sf = M[destRow, sourceRow];
                        for (int i = 0; i < rowCount; i++)
                        {
                            //Console.WriteLine(destRow + " " + i + " " + sourceRow);
                            M[destRow, i] = M[destRow, i] * df - M[sourceRow, i] * sf;
                           
                        }            

                    }
                    prin.t("gauss_____________");
                    prin.t(M);
                }
                prin.t("gauss_____________");
                prin.t(M);
                // back-insertion
                /*for (int row = rowCount - 1; row >= 0; row--)
                {
                    double f = M[row, row];
                    if (f == 0) return false;

                    for (int i = 0; i < rowCount + 1; i++) M[row, i] /= f;
                    for (int destRow = 0; destRow < row; destRow++)
                    { M[destRow, rowCount] -= M[destRow, row] * M[row, rowCount]; M[destRow, row] = 0; }
                }*/
                //prin.t("gauss_____________");
                //prin.t(M);
                return true;
            }
        }
        #endregion
    }
}
