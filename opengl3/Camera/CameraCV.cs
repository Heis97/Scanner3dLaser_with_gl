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
            CvInvoke.WarpAffine(mat, mat_aff, matrix, pattern_box.Size,Inter.Linear,Warp.Default,BorderType.Constant,new MCvScalar(127,127,127));
            pattern_box.Image = mat_aff;
            
            for (int j = 0; j < input.Length; j++)
            {
                if (input[j].Image != null)
                {
                    var inp = (Mat)input[j].Image;
                    if (CvInvoke.FindChessboardCorners(inp.ToImage<Gray, byte>(), pat_size, new Mat()))
                    {
                        //UtilOpenCV.saveImage(input[j], "cam" + (j + 1).ToString() + "\\" + path, i.ToString());
                    }
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

            var len = 10;
            var matrs = new Matrix<double>[len];

            double n = 3;
            //Console.WriteLine("k " + k);
            double k1 = (double)box_size.Height/ ((double)3*pat_size.Height);
            double k2 = (double)box_size.Width / ((double)3 * pat_size.Width);
            double k = Math.Min(k1, k2);
           
            // Console.WriteLine("k " +k);
            double offx = pat_size.Width * k;
            double offy = pat_size.Height * k;
            int ind = 0;
            
            for (int i = 1; i < 4; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    matrs[ind] = affinematr(0.01, 1.5*k, i * offx/2, j * offy/2);ind++;
                }
            }
            matrs[ind] = affinematr(0.01, 3 * k, 0, 0);

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
        static public Matrix<double> assembMatrix(Mat rvec, Mat tvec)
        {
            var rotMatr = new Matrix<double>(3, 3);
            CvInvoke.Rodrigues(rvec, rotMatr);
            var t = (double[,])tvec.GetData();
            var trMatr = new Matrix<double>(t);
            var asMatr = rotMatr.ConcateHorizontal(trMatr);
            var lastRow = new double[1, 4] { { 0, 0, 0, 1 } };
            var lastMatr = new Matrix<double>(lastRow);
            return asMatr.ConcateVertical(lastMatr);
        }
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

            var matrCS = new Matrix<double>(data_mx);
            var matrSC = new Matrix<double>(4, 4);
            CvInvoke.Invert(matrCS, matrSC, DecompMethod.LU);
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
                var gray = frame.im.ToImage<Gray, byte>();
                var corn = new VectorOfPointF();
                var ret = CvInvoke.FindChessboardCorners(gray, size, corn);
                if (ret == true)
                {
                    CvInvoke.CornerSubPix(gray, corn, new Size(5, 5), new Size(-1, -1), new MCvTermCriteria(30, 0.001));
                    var corn2 = corn.ToArray();
                    objps.Add(obp);
                    corners.Add(corn2);

                   /* var mat1 = new Mat(frame.im, new Rectangle(new Point(0, 0), frame.im.Size));                
                    if(obp_inp!=null)
                    {               
                        UtilOpenCV.drawMatches(mat1, corn2, UtilMatr.toPointF(obp_inp[ind_fr]), 255, 0, 0, 3);                       
                    }
                    else
                    {
                        CvInvoke.DrawChessboardCorners(mat1, size, corn, ret);
                        //UtilOpenCV.drawMatches(mat1, corn2, UtilMatr.toPointF(obp), 255, 0, 0, 3);
                    }
                    
                    CvInvoke.Imshow("asda", mat1);
                    CvInvoke.WaitKey();*/

                    
                    //Console.WriteLine(frame.name);
                }
                else
                {
                    Console.WriteLine("NOT:");
                    Console.WriteLine(frame.name);
                }
                ind_fr++;
            }

            

            var rvecs = new Mat[corners.Count];
            var tvecs = new Mat[corners.Count];

            
            this.objps = objps.ToArray();
            this.corners = corners.ToArray();

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
            var err = CvInvoke.CalibrateCamera(this.objps, this.corners, frames[0].im.Size, _cameramatrix, _distortmatrix, CalibType.Default, new MCvTermCriteria(100, 0.01), out rvecs, out tvecs);
            Console.WriteLine("err: " + err);
            Console.WriteLine("t,r_len: " + tvecs.Length +" "+ rvecs.Length);
            var newRoI = new Rectangle();

            this.tvecs = tvecs;
            this.rvecs = rvecs;

            var matr = CvInvoke.GetOptimalNewCameraMatrix(_cameramatrix, _distortmatrix, frames[0].im.Size, 1, frames[0].im.Size, ref newRoI);

            //computeDistortionMaps(ref mapx, ref mapy, _cameramatrix, _distortmatrix, frames[0].im.Size);
            mapx = new Mat();
            mapy = new Mat();
            CvInvoke.InitUndistortRectifyMap(_cameramatrix, _distortmatrix, null, matr, frames[0].im.Size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(frames[0].im, und_pic, mapx, mapy, Inter.Linear);
            cameramatrix = _cameramatrix;
            distortmatrix = _distortmatrix;
            prin.t("cameramatrix");
            prin.t(cameramatrix);

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
    }
}
