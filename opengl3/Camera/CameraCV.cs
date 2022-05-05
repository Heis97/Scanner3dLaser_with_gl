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

namespace opengl3
{

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

        #region monitCalib
        public static MCvPoint3D32f[][] generateObjps(ImageBox pattern_box, Mat[] pattern,bool gen_board = false,bool affine = true)
        {
            int num = 1;
            if(gen_board)
            {
                num = 2;
            }
            var matrs = new Matrix<double>[1];
            if(affine)
            {
                matrs = GetMatricesCalibAffine(pattern[0].Size, pattern_box.Size);
            }
            else
            {
                matrs = GetMatricesCalib();
            }
           
            var objps = new MCvPoint3D32f[matrs.Length][];
            for (int i=0; i<matrs.Length;i++)
            {
                var ps = new System.Drawing.PointF[1];
                if(affine)
                {
                    ps = UtilOpenCV.transfAffine(UtilOpenCV.matToPointF(pattern[num]), matrs[i]);
                }
                else
                {
                    ps = UtilOpenCV.matToPointF(UtilOpenCV.warpPerspNorm(pattern, matrs[i], pattern_box.Size)[num]);
                }

                var ps3d = new MCvPoint3D32f[ps.Length];
                for(int j=0; j<ps3d.Length;j++)
                {
                    ps3d[j] = new MCvPoint3D32f(ps[j].X, ps[j].Y, 0);
                }
                objps[i] = ps3d;
            }
            return objps;
        }
        async public static void calibrMonit(ImageBox pattern_box, ImageBox[] input,Mat[] pattern, string path,  GraphicGL graphicGL, bool affine = true)
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
                var p3d = generateObjps(pattern_box, pattern, true,affine);
                boards = UtilOpenCV.generate_BOARDs(p3d);
            }
           // prin.t(boards);
            for (int i=0; i<matrs.Length;i++)
            {
                if(graphicGL!=null)
                {                   
                    if(i!=0)
                    {
                        graphicGL.remove_buff_gl_id ( i + 9);
                    }
                    //prin.t(i);
                    graphicGL.add_buff_gl_mesh_id(boards[i], i + 10, true);
                    await Task.Delay(100);
                    SaveImage_Chess(input, path,i);
                    await Task.Delay(100);
                }
                else
                {
                    await Task.Delay(500);
                    if(affine)
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
        static void SaveImage_Chess(ImageBox[] input, string path,int i)
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
            CvInvoke.WarpAffine(mat, mat_aff, matrix, pattern_box.Size,Inter.Linear,Warp.Default,BorderType.Constant,new MCvScalar(font, font, font));
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
        static void showAndSaveImage_Chess_persp_tr(Matrix<double> matrix, ImageBox pattern_box, ImageBox[] input, Mat[] pattern, string path, int i )
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
            var matrs =new  Matrix<double>[2*p1*p2];

            double a00 = 1; double a01 = 0; double a02 = 0;

            double a10 = 0; double a11 = 1; double a12 = 0;

            double a20 = 0; double a21 = 0; double a22 = 1;
            int ind = 0;
            var diap1 = 0.3;
            var diap2 = 0.0001;
            for (int i=0; i<p1;i++)
            {
                for (int j = 0; j < p2; j++)
                {
                    a01 = calcCurA(i, p1, diap1);
                    a21 = calcCurA(j, p2, diap2);
                    matrs[ind] = new Matrix<double>(new double[3, 3] {
                        { a00, a01, a02 }, 
                        { a10, a11, a12 }, 
                        { a20, a21, a22 } });ind++;
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
            double k1 = (double)box_size.Width/(k_delim * pat_size.Width);
            double k2 = (double)box_size.Height / (k_delim * pat_size.Height);
            double k = Math.Min(k1, k2);
           
            // Console.WriteLine("k " +k);
            double offx = (box_size.Width * k - box_size.Width )/n;
            double offy = (box_size.Height * k - box_size.Height )/n;
            int ind = 0;
            
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    matrs[ind] = affinematr(0.01, k,  -i * offx,   -j * offy);
                    ind++;
                }
            }
           // matrs[ind] = affinematr(0.01, 3 * k, 0, 0);

            return matrs;
        }
        static Matrix<double> affinematr(double alpha = 0, double k = 0, double offx = 0, double offy=0)
         {
           
            return new Matrix<double>(new double[2, 3] {
                    { k*Math.Cos(alpha),-Math.Sin(alpha),  offx},
                    { Math.Sin(alpha),  k*Math.Cos(alpha),offy}
                    });
        }
        static double calcCurA(int i,int p,double diap)
        {
            var step = (2 * diap) / p;
            return -diap + step * i;
        }

        #endregion
        void init_vars()
        {
            cur_t = new Mat();
            cur_r = new Mat();
            matrixCS = new Matrix<double>(4,4);
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

            prjmatrix = cameramatrix*matrixSC.GetRows(0, 3, 1);
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

            CvInvoke.SolvePnP(points3D,points2D,cameramatrix,distortmatrix, cur_r, cur_t);
            var matrs = assemblMatrix(cur_r, cur_t);
            matrixCS = matrs[0];
            matrixSC = matrs[1];
            setPos();
            return pos;
        }
        public bool compPos(Mat mat, PatternType patternType)
        {
            if(patternType == PatternType.Chess)
            {
                Size size_patt = new Size(6, 7);
                var mat1 = mat.Clone();
                var gray = mat.ToImage<Gray, byte>();
                var corn = new VectorOfPointF();
                float markSize = 10;
                var obp = new MCvPoint3D32f[size_patt.Width * size_patt.Height];
                int ind = 0;
                for (int j = 0; j < size_patt.Height; j++)
                {
                    for (int i = size_patt.Width-1; i >=0; i--)
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
            else if(patternType == PatternType.Mesh)
            {
                Size size_patt = new Size(7, 7);
                float markSize = 30;
                var points3d = new MCvPoint3D32f[]
                {
                    new MCvPoint3D32f(0,0,0),new MCvPoint3D32f(markSize,0,0),
                    new MCvPoint3D32f(markSize,markSize,0),new MCvPoint3D32f(0,markSize,0)
                };
               
                var len = size_patt.Width * size_patt.Height;
                var cornF = new System.Drawing.PointF[len];
                var matDraw = FindCircles.findCircles(mat, cornF, size_patt);
                var points2d = FindCircles.findGab(cornF);
                compPos(points3d, points2d);
                return true;
            }
            return false;
        }


        public Point3d_GL point3DfromCam(PointF _p)
        {
            var p = cameramatrix_inv * new Point3d_GL(_p.X, _p.Y, 1);
            return p;
        }

        
        public Mat undist(Mat mat)
        {
            var mat_ret = new Mat();
            CvInvoke.Remap(mat, mat_ret, mapx, mapy, Inter.Linear);
            return mat_ret;
        }
        
        void calibrateCam(Frame[] frames, Size size, float markSize, MCvPoint3D32f[][] obp_inp)
        {
            this.frames = frames;

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


            Console.WriteLine("fr len: "+frames.Length);
            int ind_fr = 0;
            foreach (var frame in frames)
            {
                var corn2 = findPoints(frame, size);
                if(corn2==null)
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
            Console.WriteLine("objp: "+this.objps.Length);
            Console.WriteLine("corn: " + this.corners.Length);
            //prin.t("_______________");
            //UtilOpenCV.printMatch(this.objps, this.corners);
            /* prin.t(this.objps);
             prin.t("_______________");
             prin.t(this.corners);*/

            calibrateCameraTest(this.objps, this.corners);
            var err = CvInvoke.CalibrateCamera(this.objps, this.corners, this.image_size, _cameramatrix, _distortmatrix, CalibType.Default, new MCvTermCriteria(100, 0.01), out rvecs, out tvecs);

            Console.WriteLine("err: " + err);
            Console.WriteLine("t,r_len: " + tvecs.Length +" "+ rvecs.Length);
            var newRoI = new Rectangle();

            this.tvecs = tvecs;
            this.rvecs = rvecs;
            foreach(var v in tvecs)
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
            cameramatrix = _cameramatrix;
            cameramatrix_inv = invMatrix(cameramatrix);
            distortmatrix = _distortmatrix;
            prin.t("cameramatrix:");
            prin.t(cameramatrix);
            prin.t("distortmatrix:");
            prin.t(distortmatrix);

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
                    obp[ind] = new MCvPoint3D32f(markSize * ((float)i - size.Width/2), markSize * ((float)j - size.Height / 2), 0.0f);
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

            var p3d = new VectorOfVectorOfPoint3D32F(objps.GetRange(0,10).ToArray());
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


            Fisheye.Calibrate(p3d, p2d, this.image_size, _cameramatrix, _distortmatrix, rvecs, tvecs, Fisheye.CalibrationFlag.Default , new MCvTermCriteria(30, 0.01));

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

            Fisheye.InitUndistorRectifyMap(_cameramatrix, _distortmatrix, matrR, matrP, frames[0].im.Size, DepthType.Cv32F , mapx, mapy);
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


        static System.Drawing.PointF[] findPoints(Frame frame,Size size_patt)
        {
            if(frame.frameType == FrameType.MarkBoard)
            {
                var gray = frame.im.ToImage<Gray, byte>();
                var corn = new VectorOfPointF();
                var ret = CvInvoke.FindChessboardCorners(gray, size_patt, corn);
                if (ret == true)
                {
                    CvInvoke.CornerSubPix(gray, corn, new Size(5, 5), new Size(-1, -1), new MCvTermCriteria(30, 0.001));
                    var corn2 = corn.ToArray();
                    return corn2;
                     var mat1 = new Mat(frame.im, new Rectangle(new Point(0, 0), frame.im.Size));
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
            else if(frame.frameType == FrameType.Pattern)
            {
                var len = size_patt.Width * size_patt.Height;
                var cornF = new System.Drawing.PointF[len];
                var mat = FindCircles.findCircles(frame.im, cornF, size_patt);
                //CvInvoke.Imshow("calib",mat);
                //CvInvoke.WaitKey();
                if(cornF == null)
                {
                    return null;
                }
                if(cornF.Length != len)
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
            for (int i = 0; i < points3d.Length; i++)
            {
                var p2d = UtilOpenCV.takeGabObp(points2d[i], new Size(6, 7));
                var p3d = UtilOpenCV.takeGabObp(points3d[i], new Size(6, 7));
                var hMatr = homographyMatr(p3d, p2d);

                var hMat = CvInvoke.FindHomography(PointF.toSystemPoint(p3d), p2d);
                //prin.t("_____________");
                //prin.t(hMatr);
                //prin.t(new Matrix<double>( (double[,])hMat.GetData()));
                var subVmatr = matrix_subV(hMatr);
                matrsH.Add(hMatr);
                matrsSubV.Add(subVmatr);
                //
            }
            var matrV1 = matrix_V(matrsSubV.GetRange(0, matrsSubV.Count / 2).ToArray());
            var matrV2 = matrix_V(matrsSubV.GetRange(matrsSubV.Count / 2, matrsSubV.Count / 2).ToArray());
            var w1 = new Mat();
            var u1 = new Mat();
            var v1 = new Mat();
            var w2 = new Mat();
            var u2 = new Mat();
            var v2 = new Mat();

            CvInvoke.SVDecomp(matrV1, w1, u1, v1, SvdFlag.FullUV);
            CvInvoke.SVDecomp(matrV2, w2, u2, v2, SvdFlag.FullUV);

            /*prin.t("matrV1_____________");
            prin.t(matrV1);
            prin.t("matrV2_____________");
            prin.t(matrV2);*/
            prin.t("w1_____________");
            prin.t(w1);
            prin.t("w2_____________");
            prin.t(w2);
            /*prin.t("u1_____________");
            prin.t(u1);
            prin.t("u2_____________");
            prin.t(u2);*/
            prin.t("v1_____________");
            prin.t(v1);
            prin.t("v2_____________");
            prin.t(v2);

            //var testMatr = new Matrix<double>(new double[,] { {0.96,1.72 }, { 2.28,0.96} });
            var testMatr = new Matrix<double>(new double[,] { { -1, -6 }, {2, 6 } });
           // var testMatr = new Matrix<double>(new double[,] { { 3, 2 }, { 2, 0 } });
            var w = new Mat();
            var u = new Mat();
            var v = new Mat();
            CvInvoke.SVDecomp(testMatr, w, u, v, SvdFlag.Default);
            prin.t("testMatr_____________");
            prin.t(testMatr);
            prin.t("w_____________");
            prin.t(w);
            prin.t("u_____________");
            prin.t(u);
            prin.t("v_____________");
            prin.t(v);

            calcIntrisicParam(new Matrix<double>(new double[,]{ { 0.695, -0.104, -0.711, -0.003, -0.003, 0 } }));
            calcIntrisicParam(new Matrix<double>(new double[,] { { 0.739, - 0.063, - 0.67, - 0.004, - 0.003, 0 } }));
            calcIntrisicParam(new Matrix<double>(new double[,] { { -0.632, - 0.559, - 0.536, 0.003, 0.002, 0 } }));
            calcIntrisicParam(new Matrix<double>(new double[,] { { -0.573, - 0.581, - 0.578, 0.003, 0.002, 0 } }));
        }

        Matrix<double> matrix_vij(Matrix<double> matr,int i, int j)
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
            var v12 = matrix_vij(matr, 1, 2);
            var v11 = matrix_vij(matr, 1, 1);
            var v22 = matrix_vij(matr, 2, 2);
            var matr1 = v12.Transpose();
            var matr2 = (v11-v22).Transpose();
            var matr_res = matr1.ConcateVertical(matr2);

            return matr_res;
        }
        Matrix<double> matrix_V(Matrix<double>[] matr)
        {
            var matr_res = new Matrix<double>(3,3);
            if (matr.Length>2)
            {
                matr_res = matr[0];
                for (int i=1; i< matr.Length; i++)
                {
                    matr_res = matr_res.ConcateVertical(matr[i]);
                }
            }
            return matr_res;
        }

        Matrix<double> homographyMatr(MCvPoint3D32f[] points3d, System.Drawing.PointF[] points2d)
        {
            Matrix<double> matrix3d = new Matrix<double>(3,3);
            Matrix<double> matrix2d = new Matrix<double>(3,3);
            Matrix<double> matrix = new Matrix<double>(3, 3);
            if (points3d.Length>3 && points2d.Length > 3)
            {
                for(int i = 0; i < 3; i++)
                {
                    matrix3d[i, 0] = points3d[i].X; matrix3d[i, 1] = points3d[i].Y; matrix3d[i, 2] = 1;
                    matrix2d[i, 0] = points2d[i].X; matrix2d[i, 1] = points2d[i].Y; matrix2d[i, 2] = 1;
                }
                var K = new Matrix<double>(3, 3);
                CvInvoke.Invert(matrix3d, K, DecompMethod.LU);
                var matrix1 = K * matrix2d;
                //prin.t(matrix);

                for (int i = 0; i < 3; i++)
                {
                    matrix3d[i, 0] = points3d[i + 1].X; matrix3d[i, 1] = points3d[i + 1].Y; matrix3d[i, 2] = 1;
                    matrix2d[i, 0] = points2d[i + 1].X; matrix2d[i, 1] = points2d[i + 1].Y; matrix2d[i, 2] = 1;
                }
                K = new Matrix<double>(3, 3);
                CvInvoke.Invert(matrix3d, K, DecompMethod.LU);
                var matrix2 = K * matrix2d;

                matrix = (matrix1 + matrix2) / 2;
            }

            return matrix.Transpose();
        }


        void calcIntrisicParam(Matrix<double> eigenvector)
        {

            if(eigenvector.Cols>5)
            {
                var B11 = eigenvector[0, 0];
                var B12 = eigenvector[0, 1];
                var B22 = eigenvector[0, 2];
                var B13 = eigenvector[0, 3];
                var B23 = eigenvector[0, 4];
                var B33 = eigenvector[0, 5];

                var v0 = (B12 * B13 - B11 * B23) / (B11 * B22 - B12 * B12);
                var lam = B33 - (B13 * B13 + v0 * (B12 * B13 - B11 * B23)) / B11;
                var alph = Math.Sqrt(lam / B11);
                var beta = Math.Sqrt((lam * B11) / (B11 * B22 - B12 * B12));
                var gamm = -B12 * alph * alph * beta / lam;
                var u0 = gamm * v0 / beta - B13 * alph * alph / lam;

                Console.WriteLine("v0 = " + v0);
                Console.WriteLine("lam = " + lam);
                Console.WriteLine("alph = " + alph);
                Console.WriteLine("beta = " + beta);
                Console.WriteLine("gamm = " + gamm);
                Console.WriteLine("u0 = " + u0);
            }
            
        }

        public double Determinant3x3(Matrix<double> matr)
        {
            if(matr.Cols==3 && matr.Rows==3)
            {
                double a = matr[0, 0], b = matr[1, 0], c = matr[2, 0];
                double d = matr[0, 1], e = matr[1, 1], f = matr[2, 1];
                double g = matr[0, 2], h = matr[1, 2], k = matr[2, 2];

                return (e * k - f * h) * a + -(d * k - f * g) * b + (d * h - e * g) * c;
            }
            else
            {
                return 0;
            }
            
            
        }

        /// <summary>
        /// Compute the inverse of this Matrix3x3f.
        /// </summary>
        public Matrix<double> Inverse(Matrix<double> matr)
        {
            if (matr.Cols == 3 && matr.Rows == 3)
            {
                double det = Determinant3x3(matr);
                if (Math.Abs(det) < 1e-6f)
                {
                    throw new InvalidOperationException("not invertible");
                }
                    
                det = 1.0f / det;

                double a = matr[0, 0], b = matr[1, 0], c = matr[2, 0];
                double d = matr[0, 1], e = matr[1, 1], f = matr[2, 1];
                double g = matr[0, 2], h = matr[1, 2], k = matr[2, 2];

                return new Matrix<double>(new double[,] {
                    { (e * k - f * h) * det, -(d * k - f * g) * det, (d * h - e * g) * det, },
                    { -(b * k - c * h) * det, (a * k - c * g) * det, -(a * h - b * g) * det, },
                    {  (b * f - c * e) * det, -(a * f - c * d) * det, (a * e - b * d) * det }
                        }
                    );
            }
            else
            {
                return new Matrix<double>(3,3);
            }
        }
    }
}
