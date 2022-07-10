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





    }   
}
