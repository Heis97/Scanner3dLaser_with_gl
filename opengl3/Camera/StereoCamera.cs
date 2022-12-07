using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;

namespace opengl3
{
    public class StereoCamera
    {
        public CameraCV[] cameraCVs;
        public Matrix<double> R;//1 * R -> 2

        public StereoCamera(CameraCV[] _cameraCVs)
        {
            cameraCVs = _cameraCVs;
        }

        public void calibrate(Mat[] mats,PatternType patternType)
        {
            if(mats.Length == cameraCVs.Length)
            {
                bool comp_pos = true;
                for(int i = 0; i < mats.Length; i++)
                {
                    comp_pos &= cameraCVs[i].compPos(mats[i], patternType, 10f);
                    //Console.WriteLine(comp_pos);
                }
                if(mats.Length>1 && comp_pos)
                {
                    var inv_cs1 = new Matrix<double>(4,4);
                    CvInvoke.Invert(cameraCVs[0].matrixCS, inv_cs1, DecompMethod.LU);
                    R = inv_cs1 * cameraCVs[1].matrixCS;
                    /*prin.t("stereo calib R_cs:");
                    prin.t(R);
                    R[0, 0] = 0.572; R[2, 2] = 0.572;
                    R[0, 3] = 114.2;
                    R[1, 3] = 4.8;
                    R[2, 3] = 61.8;*/

                    // Console.WriteLine(R[0, 3] + " " + R[1, 3] + " " + R[2, 3] + " " + R[0, 0] + " " + R[0, 1] + " " + R[0, 2]);

                    /*inv_cs1 = new Matrix<double>(4, 4);
                    CvInvoke.Invert(cameraCVs[1].matrixSC, inv_cs1, DecompMethod.LU);
                    R = cameraCVs[0].matrixSC* inv_cs1  ;
                    prin.t("stereo calib R_sc:");
                    prin.t(R);*/
                }
            }
        }

        public Matrix<double> calibrateBfs(Frame[] pos)
        {
            for(int i=0; i<pos.Length; i++)
            {
                cameraCVs[0].compPos(pos[i].im, PatternType.Mesh, 10f,true);
                var Bsm = cameraCVs[0].matrixCS.Clone();
                var Bbf = new RobotFrame(pos[i].name).getMatrix();
                var Bbm = new RobotFrame("510.9 6.4 55.4 1.5 -0.002 -0.1").getMatrix();
                /*prin.t("Bsm");
                prin.t(Bsm);
                prin.t("Bbf");
                prin.t(Bbf);
                prin.t("Bbm");
                prin.t(Bbm);*/
                var Bsm_1 = Bsm.Clone();
                var Bbf_1 = Bbf.Clone();
                var Bbm_1 = Bbm.Clone();
                CvInvoke.Invert(Bsm,Bsm_1,DecompMethod.LU);
                CvInvoke.Invert(Bbf, Bbf_1, DecompMethod.LU);
                CvInvoke.Invert(Bbm, Bbm_1, DecompMethod.LU);
                var Bfs = Bbf_1 * Bbm * Bsm;
                //prin.t(Bfs);
                Console.WriteLine(Bfs[0, 3] + " " + Bfs[1, 3] + " " + Bfs[2, 3] + " " + Bfs[0, 0] + " " + Bfs[0, 1] + " " + Bfs[0, 2]);
                //prin.t("--------------------------------");

            }
            return null;
        }
        /// <summary>
        /// For make housing
        /// </summary>
        /// <param name="alpha">fov x, degree</param>
        /// <param name="beta">xy Norm ^ camera Norm ,degree</param>
        /// <param name="L">size of scan, mm</param>
        static public void calcSizesScanner(double alpha, double beta, double L)
        {
            var gamma = (180 - alpha) / 2;
            var tetta = 180 - beta - gamma;
            var m1 = (L * sin(tetta) )/ (2 * sin(gamma));
            var m2 = m1 * tg(beta);
            var k1 = m1 / cos(beta);
            var k2 = m1 * (tg(gamma) - tg(beta));
            var k = (tg(gamma) - tg(beta)) * cos(beta);
            var y = k * m1;
            var x = k * m2 + k1 - L / 2;
            Console.WriteLine("X: "+x + "; Y: " + y);
        }
        public static double calcFov(double size, double f)
        {
            return 2 * arctg(size / (2 * f));
        }
        static double toRad(double degree)
        {
            return degree * Math.PI / 180;
        }
        static double toDegree(double rad)
        {
            return rad * 180 / Math.PI  ;
        }
        static double sin(double degree)
        {
            return Math.Sin(toRad(degree));
        }
        static double arctg(double val)
        {
            return  toDegree( Math.Atan(val));
        }
        static double cos(double degree)
        {
            return Math.Cos(toRad(degree));
        }

        static double tg(double degree)
        {
            return Math.Tan(toRad(degree));
        }

    }   
}
