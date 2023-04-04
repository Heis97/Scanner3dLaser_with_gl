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
        Flat3d_GL start_LasFlat = new Flat3d_GL(1, 0, 0, 0);
        double start_pos = 0;

        bool calibrated = false;
        public GraphicGL GraphicGL;
        Matrix<double> cur_matrix_cam;
        int count_flats = 0;
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
               //prin.t(cameraCV.matrixCS);
               // Console.WriteLine(cameraCV.matrixCS[0,2]+" "+ cameraCV.matrixCS[1, 2]+" "+ cameraCV.matrixCS[2, 2]+" "+ position);
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
            
            if (count_flats % 1 ==0)
            {
                var las = new LaserSurface(mats, origs, cameraCV, patternType, true, GraphicGL);
                cur_matrix_cam = cameraCV.matrixCS;
                PositionsAxis.Add(position);
                LasFlats.Add(las.flat3D);
                Console.WriteLine(position);

            }
            count_flats++;            
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
                    //Console.WriteLine("mats.Length"+ mats.Length);
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
           /* int start_ind = (int)(LasFlats.Count / 2) + 1;
            int end_ind = (int)(LasFlats.Count / 2) - 1;*/

            int start_ind = (int)(LasFlats.Count ) - 1;
            int end_ind = 1;

            oneLasFlat = (LasFlats[end_ind] - LasFlats[start_ind]) / (PositionsAxis[end_ind] - PositionsAxis[start_ind]);

            start_LasFlat = LasFlats[start_ind];
            start_pos = PositionsAxis[start_ind];

            Console.WriteLine("comp_flat");
            /*Console.WriteLine(PositionsAxis[next_val + 1]+" "+ PositionsAxis[next_val - 1]);
            GraphicGL?.addFlat3d_XZ(LasFlats[next_val + 1],null,0.1f,0.9f);
            GraphicGL?.addFlat3d_XZ(LasFlats[next_val - 1], null, 0.1f, 0.9f);*/
            Console.WriteLine(oneLasFlat*400);//for 1 mm
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
                var delPos = PositionLinear - start_pos;
                var lasFlat = start_LasFlat + oneLasFlat * delPos;
                //GraphicGL?.addFlat3d_XZ(lasFlat,cur_matrix_cam);
                return lasFlat;
            }
            return new Flat3d_GL();
        }

    }
}
