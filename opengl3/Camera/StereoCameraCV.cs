using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
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
        public Mat t,r,e,f,p1,p2,mx1,mx2,my1,my2;
        public Matrix<double> prM1, prM2;
        StereoSGBM stereosolver;
        StereoBM stereosolverBM;
        public SGBM_param solver_param;
        public StereoCameraCV(CameraCV[] _cameraCVs, Size size, float markSize, Frame[][] stereoFrames = null)
        {
            cameraCVs = _cameraCVs;
            calibrateCamStereo(cameraCVs, size, markSize, stereoFrames);
            rectify();
            init();
        }

        public StereoCameraCV()
        {
            init();
        }

        void calibrateCamStereo(CameraCV[] _cameraCVs, Size size,float markSize, Frame[][] stereoFrames = null)
        {
            if (_cameraCVs.Length < 2)
            {
                return;
            }

            var cam1 = _cameraCVs[0];
            var cam2 = _cameraCVs[1];

            var objs = cam1.objps;
            var cor1 = cam1.corners;
            var cor2 = cam2.corners;
            if (stereoFrames!=null)
            {
                var objps = new List<MCvPoint3D32f[]>();
                var corners1 = new List<System.Drawing.PointF[]>();
                var corners2 = new List<System.Drawing.PointF[]>();

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


                Console.WriteLine("fr len: " + stereoFrames[0].Length);
                for(int i =0; i< stereoFrames[0].Length;i++)
                {
                    var corn1 = CameraCV.findPoints(stereoFrames[0][i], size);
                    var corn2 = CameraCV.findPoints(stereoFrames[1][i], size);
                    if (corn1 == null || corn2 == null)
                    {
                        Console.WriteLine("NOT:");
                        Console.WriteLine(i);
                    }
                    else
                    {
                        objps.Add(obp);
                        corners1.Add(corn1);
                        corners2.Add(corn2);
                    }
                }
                objs = objps.ToArray();
                cor1 = corners1.ToArray();
                cor2 = corners2.ToArray();
                Console.WriteLine("stereoFrames ");
            }
            else
            {
                Console.WriteLine("stereoFrames NULL");
            }
            /*for (int i = 0; i < cam1.tvecs.Length; i++)
            {
                
                prin.t(cam1.frames[i].name);
                prin.t(cam2.frames[i].name);
                //prin.t(cam1.tvecs[i]- cam2.tvecs[i]);
                var A_1 = CameraCV.assemblMatrix(cam1.rvecs[i], cam1.tvecs[i])[0];
                var B = CameraCV.assemblMatrix(cam2.rvecs[i], cam2.tvecs[i])[1];
                var C = A_1 * B;
                prin.t(C);
                prin.t("_________________");
            }*/



            var r = new Mat();
            var t = new Mat();
            var e = new Mat();
            var f = new Mat();
            Console.WriteLine(cor1.Length + " " + cor2.Length);
            var err = CvInvoke.StereoCalibrate
                (objs,
                cor1,
                cor2,
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
            this.e = e;
            this.f = f;
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
        void rectify()
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

            mx1 = new Mat(); my1 = new Mat(); mx2 = new Mat(); my2 = new Mat();
            CvInvoke.InitUndistortRectifyMap(cam1.cameramatrix, cam1.distortmatrix, r1, p1, cam1.frames[0].im.Size, DepthType.Cv32F, mx1, my1);
            CvInvoke.InitUndistortRectifyMap(cam2.cameramatrix, cam2.distortmatrix, r2, p2, cam2.frames[0].im.Size, DepthType.Cv32F, mx2, my2);
            prin.t("p1: ");
            prin.t(p1);
            prin.t("_________");

            prin.t("p2: ");
            prin.t(p2);
            prin.t("_________");
            this.p1 = p1;
            this.p2 = p2;
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
        public Mat[] drawEpipolarLines(Mat mat1, Mat mat2, VectorOfPointF points)
        {  
            var lines = new VectorOfPoint3D32F() ;
            var size = mat1.Size;
            CvInvoke.ComputeCorrespondEpilines(points, 1, f, lines);
            
            for(int i=0; i<points.Size;i++)
            {
                int x0 = 0;
                int y0 = (int)(-lines[i].Z/ lines[i].Y);

                int x1 = size.Width;
                int y1 = (int)(-(lines[i].Z+ lines[i].X* size.Width) / lines[i].Y);
                CvInvoke.Line(mat1, new Point(x0, y0), new Point(x1, y1), new MCvScalar(255, 0, 0));
            }

            //prin.t(lines);
           // prin.t("_________");
            return new Mat[] { mat1, mat2 };
        }

        public Mat remapCam(Mat mat,int ind)
        {
            var remat = new Mat();
            if(ind==1)
            {
                CvInvoke.Remap(mat, remat, mx1, my1, Inter.Linear);
            }
            if(ind == 2)
            {
                CvInvoke.Remap(mat, remat, mx2, my2, Inter.Linear);
            }
            return remat;
        }
        public Mat epipolarTest(Mat matL, Mat matR)
        {
            var mat1 = matL.Clone();
            var mat2 = matR.Clone();
            if (stereosolver == null)
            {
                return null;
            }
            if(cameraCVs!=null)
            {
                if(cameraCVs.Length>1)
                {
                    CvInvoke.Remap(mat1, mat1, mx1, my1, Inter.Linear);
                    CvInvoke.Remap(mat2, mat2, mx2, my2, Inter.Linear);
                }
                
            }

            var imL = mat1.ToImage<Gray, byte>();
            var imR = mat2.ToImage<Gray, byte>();

            mat1.Dispose();
            mat2.Dispose();
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
                disp.Dispose();
                // CvInvoke.Normalize(depth, depth_norm, 255, 0);
                depth.ConvertTo(depth_norm, DepthType.Cv8U);
                depth.Dispose();
                GC.Collect();
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
}
