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
        List<Flat3d_GL> LasFlats;
        List<double> PositionsAxis;
        Matrix<double> oneMatrix = new Matrix<double>(4, 4);
        Flat3d_GL oneLasFlat = new Flat3d_GL(1,0,0,0);
        bool calibrated = false;
        public LinearAxis()
        {
            MatrixesCamera= new List<Matrix<double>>();
            PositionsAxis = new List<double>();
            LasFlats = new List<Flat3d_GL>();
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

        public bool calibrateLas(Mat[][] mats, double[] positions, CameraCV cameraCV, PatternType patternType, GraphicGL graphicGL)
        {
            if (addLaserFlats(mats, positions, cameraCV, patternType))
            {
                compOneFlat();
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
        bool addLasFlat(Mat[] mats, double positions, CameraCV cameraCV, PatternType patternType)
        {
            var las = new LaserSurface(mats, cameraCV, patternType);
            MatrixesCamera.Add(cameraCV.matrixCS);
            LasFlats.Add(las.flat3D);
            return true;
            
        }
        bool addLaserFlats(Mat[][] mats, double[] positions,CameraCV cameraCV, PatternType patternType)
        {
            LasFlats = new List<Flat3d_GL>();
            PositionsAxis = new List<double>();
            if (mats.Length == positions.Length)
            {
                int j = 0;
                for (int i = 0; i < mats.Length; i++)
                {
                    if (addLasFlat(mats[i], positions[i], cameraCV, patternType))
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

        void compOneMatrix()
        {
            oneMatrix = (MatrixesCamera[MatrixesCamera.Count - 1] - MatrixesCamera[0]) / (PositionsAxis[PositionsAxis.Count - 1] - PositionsAxis[0]);
            prin.t(oneMatrix);
        }

        void compOneFlat()
        {
            oneLasFlat = (LasFlats[LasFlats.Count - 1] - LasFlats[0]) / (PositionsAxis[PositionsAxis.Count - 1] - PositionsAxis[0]);
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
                var compMatr = LasFlats[0] + oneLasFlat * delPos;
            }
            return new Flat3d_GL();
        }

    }
}
