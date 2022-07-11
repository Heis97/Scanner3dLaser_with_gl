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
                for(int i = 0; i < mats.Length; i++)
                {
                    cameraCVs[i].compPos(mats[i], patternType, 10f);
                }
                if(mats.Length>1)
                {
                    var inv_cs1 = new Matrix<double>(4,4);
                    CvInvoke.Invert(cameraCVs[0].matrixCS, inv_cs1, DecompMethod.LU);
                    R = inv_cs1 * cameraCVs[1].matrixCS;
                    prin.t("stereo calib R_cs:");
                    prin.t(R);

                    inv_cs1 = new Matrix<double>(4, 4);
                    CvInvoke.Invert(cameraCVs[1].matrixSC, inv_cs1, DecompMethod.LU);
                    R = cameraCVs[0].matrixSC* inv_cs1  ;
                    prin.t("stereo calib R_sc:");
                    prin.t(R);
                }
            }
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
