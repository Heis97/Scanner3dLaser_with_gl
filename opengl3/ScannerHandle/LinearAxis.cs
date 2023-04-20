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

        public LinearAxis(Flat3d_GL oneF, Flat3d_GL stF,double stP)
        {
            MatrixesCamera = new List<Matrix<double>>();
            PositionsAxis = new List<double>();
            LasFlats = new List<Flat3d_GL>();
            oneLasFlat = oneF;
            start_LasFlat = stF;
            start_pos = stP;
            calibrated = true;
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

        public bool calibrateLas_step(Mat[] mats, Mat orig, double[] positions, CameraCV cameraCV, PatternType patternType, GraphicGL graphicGL=null)
        {
            var inds_part = Detection.max_claster_im(cameraCV.scan_points.ToArray(), 4);
/*
            CvInvoke.Imshow("im1", mats[inds_part[inds_part.Length / 4]]);
            CvInvoke.Imshow("im2", mats[inds_part[inds_part.Length* 2/ 4]]);
            CvInvoke.Imshow("im3", mats[inds_part[inds_part.Length*3 / 4]]);

            CvInvoke.WaitKey();*/

            var mats_calib = new Mat[] { mats[inds_part[inds_part.Length/4]], mats[inds_part[2 * inds_part.Length / 4]], mats[inds_part[3*inds_part.Length / 4]] };
            positions = new double[] { positions[inds_part[inds_part.Length / 4]], positions[inds_part[2 * inds_part.Length / 4]], positions[inds_part[3 * inds_part.Length / 4]] };

            var x_dim = 50;
            var y_dim = 70;

            var corners = corner_step(orig);

            var orig_c = orig.Clone();
           /* orig_c = cameraCV.undist(orig_c);
            UtilOpenCV.drawPointsF(orig_c, corners,255,0,0,2,true);
            CvInvoke.Imshow("corn", orig_c);
            CvInvoke.WaitKey();*/
            cameraCV.compPos(new MCvPoint3D32f[] {
                new MCvPoint3D32f(0,0,0),
            new MCvPoint3D32f(0,y_dim,0),
            new MCvPoint3D32f(x_dim,y_dim,0),
            new MCvPoint3D32f(x_dim,0,0)},corners);



            cur_matrix_cam = cameraCV.matrixCS;
            //Console.WriteLine("cur_matrix_cam");
            //prin.t(cur_matrix_cam);
            for (int i=0;i< mats_calib.Length;i++)
            {
                var las = new LaserSurface(mats_calib[i], cameraCV, patternType,graphicGL);                
                PositionsAxis.Add(positions[i]);
                //graphicGL.addFlat3d_YZ(las.flat3D,null,0.3f);
                LasFlats.Add(las.flat3D);
            }
            compOneFlat();


            calibrated = true;
            return true;
        }



        System.Drawing.PointF[] corner_step(Mat orig)
        {
            var orig1= orig.Clone();
            orig1 = FindCircles.sobel_mat(orig1);
            CvInvoke.CvtColor(orig1, orig1, ColorConversion.Rgb2Gray);
            CvInvoke.MedianBlur(orig1, orig1, 5);
            CvInvoke.Threshold(orig1, orig1, 70, 255, ThresholdType.Binary);
            var cont = FindCircles.find_max_contour(orig1);
            var c_f = PointF.from_contour(cont);
            var corn = FindCircles.findGab(PointF.toSystemPoint(c_f));
          //  UtilOpenCV.drawPointsF(orig, corn, 255, 0, 0, 2, true);
           // CvInvoke.Imshow("corns", orig);


            return corn;
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

            int start_ind = LasFlats.Count  - 1;
            int end_ind = 0;

            oneLasFlat = (LasFlats[end_ind] - LasFlats[start_ind]) / (PositionsAxis[end_ind] - PositionsAxis[start_ind]);

            start_LasFlat = LasFlats[start_ind];
            start_pos = PositionsAxis[start_ind];

            Console.WriteLine("comp_flat");
            /*Console.WriteLine(PositionsAxis[next_val + 1]+" "+ PositionsAxis[next_val - 1]);
            GraphicGL?.addFlat3d_XZ(LasFlats[next_val + 1],null,0.1f,0.9f);
            GraphicGL?.addFlat3d_XZ(LasFlats[next_val - 1], null, 0.1f, 0.9f);*/
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
                var delPos = PositionLinear - start_pos;
                var lasFlat = start_LasFlat + oneLasFlat * delPos;
                //GraphicGL?.addFlat3d_XZ(lasFlat,cur_matrix_cam, 0.1f, 0.1f, 0.4f);
                return lasFlat;
            }
            return new Flat3d_GL();
        }


        static public LinearAxis load(string path)
        {
            var settings = Settings_loader.load_data(path);

            var oneF = (Flat3d_GL)settings[0];
            var stF= (Flat3d_GL)settings[1];
            var  stP= (double)settings[2];
            return new LinearAxis(oneF, stF, stP);
        }
        public void save(string path)
        {
            Settings_loader.save_file(path, new object[] { oneLasFlat, start_LasFlat, start_pos});
        }


    }
}
