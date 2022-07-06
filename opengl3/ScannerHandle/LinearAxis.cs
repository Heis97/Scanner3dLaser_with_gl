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
        public GraphicGL GraphicGL;
        Matrix<double> cur_matrix_cam;
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

        public bool calibrateLas(Mat[][] mats, Mat[] origs, double[] positions, CameraCV cameraCV, PatternType patternType, GraphicGL graphicGL)
        {

            if (addLaserFlats(mats, origs, positions, cameraCV, patternType))
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
                Console.WriteLine(cameraCV.matrixCS[0,3]+" "+ cameraCV.matrixCS[1, 3]+" "+ cameraCV.matrixCS[2, 3]+" "+ position);
                MatrixesCamera.Add(cameraCV.matrixCS);
                PositionsAxis.Add(position);
                return true;
            }
            else
            {
                return false;
            }
        }
        bool addLasFlat(Mat[] mats, Mat[] origs, double position, CameraCV cameraCV, PatternType patternType)
        {
            
            var las = new LaserSurface(mats, origs, cameraCV, patternType);
            cur_matrix_cam = cameraCV.matrixCS;
            PositionsAxis.Add(position);
            if (LasFlats.Count > 0)
            {
                //Console.WriteLine(position);

            }
            LasFlats.Add(las.flat3D);
            //GraphicGL?.addFlat3d_XZ(las.flat3D);
            Console.WriteLine(las.flat3D + " " + position);

            return true;
            
        }

        bool addLaserFlats(Mat[][] mats, Mat[] origs, double[] positions,CameraCV cameraCV, PatternType patternType)
        {
            LasFlats = new List<Flat3d_GL>();
            PositionsAxis = new List<double>();
           
           var sob_im = FindCircles.sobel(origs[0].ToImage<Gray, byte>()).Convert<Bgr, byte>().Mat;
            
            if (mats[0].Length == positions.Length)
            {
                int j = 0;
                for (int i = 0; i < mats[0].Length; i++)
                {
                    if(mats.Length==2)
                    {
                        if (addLasFlat(new Mat[] { mats[0][i], mats[1][i] }, origs, positions[i], cameraCV, patternType))
                        {
                            j++;
                        }
                    }
                    if(mats.Length == 1)
                    {
                        if (addLasFlat(new Mat[] { mats[0][i] - sob_im }, origs, positions[i], cameraCV, patternType))
                        {
                            j++;
                        }
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
            int next_val = (int)(LasFlats.Count / 2) - 1;
            next_val = 10;
            oneLasFlat = (LasFlats[next_val] - LasFlats[0]) / (PositionsAxis[next_val] - PositionsAxis[0]);
            Console.WriteLine(oneLasFlat);
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
                var lasFlat = LasFlats[0] + oneLasFlat * delPos;
                GraphicGL?.addFlat3d_XZ(lasFlat,cur_matrix_cam);
                return lasFlat;
            }
            return new Flat3d_GL();
        }

    }
}
