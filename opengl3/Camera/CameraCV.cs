﻿using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class CameraCV
    {
        public Matrix<double> cameramatrix;
        public Matrix<double> distortmatrix;
        public Frame[] frames;
        public System.Drawing.PointF[][] corners;
        public MCvPoint3D32f[][] objps;
        public Mat[] tvecs;
        public Mat[] rvecs;
        public Mat cur_t;
        public Mat cur_r;
        public CameraCV(Matrix<double> _cameramatrix, Matrix<double> _distortmatrix)
        {
            cameramatrix = _cameramatrix;
            distortmatrix = _distortmatrix;
            cur_t = new Mat();
            cur_r = new Mat();
        }
        public CameraCV(Frame[] _frames, Size _size)
        {
            calibrateCam(_frames, _size);
        }
        public void compPos(MCvPoint3D32f[] points3D, System.Drawing.PointF[] points2D)
        {            

            CvInvoke.SolvePnP(points3D,points2D,cameramatrix,distortmatrix, cur_r, cur_t);           
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
        void calibrateCam(Frame[] frames, Size size)
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
