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
        public Mat t,r,e,f,p1,p2;
        public Matrix<double> prM1, prM2;
        StereoSGBM stereosolver;
        StereoBM stereosolverBM;
        public SGBM_param solver_param;
        public StereoCameraCV(CameraCV[] _cameraCVs)
        {
            cameraCVs = _cameraCVs;
            calibrateCamStereo(cameraCVs);
            rectify();
            init();
        }
       
        void debugCalib()
        {

        }
        void calibrateCamStereo(CameraCV[] _cameraCVs)
        {
            if (_cameraCVs.Length < 2)
            {
                return;
            }
            var cam1 = _cameraCVs[0];
            var cam2 = _cameraCVs[1];

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
            Console.WriteLine(cam2.objps.Length + " " + cam2.corners.Length);
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

        public Mat epipolarTest(Mat matL, Mat matR)
        {
            if (stereosolver == null)
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
}
