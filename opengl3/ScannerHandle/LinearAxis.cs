using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV.UI;

namespace opengl3
{
    public class LinearAxis
    {
        List<Matrix<double>> MatrixesCamera;
        List<double> PositionsAxis;
        Matrix<double> oneMatrix = new Matrix<double>(4, 4);
        bool calibrated = false;
        public LinearAxis()
        {
            MatrixesCamera= new List<Matrix<double>>();
            PositionsAxis = new List<double>();
        }
        public bool calibrate(Mat[] mats, double[] positions, CameraCV cameraCV,PatternType patternType, GraphicGL graphicGL)
        {
            if(addPositions(mats, positions, cameraCV, patternType))
            {
                compOneMatrix();
                calibrated = true;
                return true;
            }
            return false;
        }
        bool addPositions(Mat[] mats, double[] positions, CameraCV cameraCV, PatternType patternType)
        {
            MatrixesCamera = new List<Matrix<double>>();
            PositionsAxis = new List<double>();
            if (mats.Length == positions.Length)
            {
                int j = 0;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (addPosition(mats[i], positions[i], cameraCV, patternType))
                    {
                        j++;
                    }
                }
                if (j > 1)
                {
                    return true;
                }
            }
            return false;            
        }

        bool addPosition(Mat mat, double position, CameraCV cameraCV, PatternType patternType)
        {
            if (cameraCV.compPos(mat, patternType))
            {
                MatrixesCamera.Add(cameraCV.matrixCS);
                PositionsAxis.Add(position);
                return true;
            }
            else
            {
                return false;
            }
        }

        void compOneMatrix()
        {
            
            oneMatrix = (MatrixesCamera[MatrixesCamera.Count - 1] - MatrixesCamera[0]) / (PositionsAxis[PositionsAxis.Count - 1] - PositionsAxis[0]);
            prin.t(oneMatrix);
        }

        public Matrix<double> getMatrixCamera(double PositionLinear)
        {
            if(calibrated)
            {
                var delPos = PositionLinear - PositionsAxis[0];
                var compMatr = MatrixesCamera[0] + oneMatrix * delPos;
                return compMatr;
            }
            return null;
        }

        public Flat3d_GL getLaserSurf(double PositionLinear)
        {
            if (calibrated)
            {
                var delPos = PositionLinear - PositionsAxis[0];
                var compMatr = MatrixesCamera[0] + oneMatrix * delPos;
            }
            return new Flat3d_GL();
        }

    }
}
