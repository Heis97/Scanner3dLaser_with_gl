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
        public Flat3d_GL oneLasFlat = new Flat3d_GL(1,0,0,0);
        public Flat3d_GL start_LasFlat = new Flat3d_GL(1, 0, 0, 0);
        public double start_pos = 0;

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

        static int right_white_pixel(Mat mat)
        {
            var im = mat.ToImage<Gray, byte>();
            int min_x = mat.Width;
            for(int j = 0; j < im.Height; j++)
            {
                for (int i = 0; i < im.Width; i++)
                {
                    if( im.Data[j,i,0] > 127)
                    {
                        if(i<min_x)
                        {
                            min_x = i;
                        }
                       
                    }
                }
            }
            return min_x;
        }

        static System.Drawing.Point[] find_gab_pix(Mat mat)
        {
            var im = mat.ToImage<Gray, byte>();

            var ps = new System.Drawing.Point[]
               {
                    new System.Drawing.Point(0,0),
                    new System.Drawing.Point(0,0),
                    new System.Drawing.Point(0,0),
                    new System.Drawing.Point(0,0),
               };

            int min_x = mat.Width;
            int max_x = 0;

            int min_y = mat.Height;
            int max_y = 0;
            for (int j = 0; j < im.Height; j++)
            {
                for (int i = 0; i < im.Width; i++)
                {
                    if (im.Data[j, i, 0] > 50)
                    {
                        if (i < min_x)
                        {
                            min_x = i;
                            ps[0] = new System.Drawing.Point(i, j);
                        }
                       

                        if (j < min_y)
                        {
                            min_y = j;
                            ps[1] = new System.Drawing.Point(i, j);
                        }

                        if (i > max_x)
                        {
                            max_x = i;
                            ps[2] = new System.Drawing.Point(i, j);
                        }
                        if (j > max_y)
                        {
                            max_y = j;
                            ps[3] = new System.Drawing.Point(i, j);
                        }
                    }
                }
            }

           
            return ps;
        }
        public Mat get_corners_calibrate_model(Mat[] mats)
        {
            Mat up_surf = null;
            var i_max = 0;
            var i_min = mats.Length-1;
            var delts = new float[mats.Length];
            var delts_b = new bool[mats.Length];
            double dest_max = 0;
            for (int i = 0; i< mats.Length;i++)
            {         
                var ps = Detection.detectLineDiff(mats[i]);
                var ps_par = Detection.parall_Points(Detection.filtr_y0_Points(ps));
                var ps_o = LaserSurface.order_x(ps_par);
                if (ps_o == null) continue;
                if (ps_o.Length<3) continue;
                var del = ps_o[0].X - ps_o[ps_par.Length-1].X;
                delts[i] = Math.Abs(del);
                if(delts[i]>dest_max)
                {
                    dest_max = delts[i];
                }
                Console.WriteLine(i + " " + delts[i]);
                /*Mat test_1 = mats[i].Clone();
                test_1 = UtilOpenCV.drawPointsF(test_1, ps_o, 0, 255, 0);
                CvInvoke.Imshow("test", test_1);
                CvInvoke.WaitKey();*/
            }
            for (int i = 0; i < delts.Length; i++)
            {
                if (delts[i] > dest_max*0.7)
                {
                    delts_b[i] = true;
                }
            }
            for (int i = 1; i < delts_b.Length; i++)
            {
                if(delts_b[i-1] == false && delts_b[i] == true) i_min = i;
                if (delts_b[i - 1] == true && delts_b[i] == false) i_max = i;
            }
            Console.WriteLine(i_min + " " + i_max);
            for (int i = 0;  i < mats.Length; i++)
            {
                if ((i < i_min + 4 && i > i_min) || ((i < i_max && i > i_max - 4)))
                {


                    Console.WriteLine(i);
                    //if (Math.Abs(delts[i]) > 55)
                    {
                        var bin = new Mat();
                        var r = mats[i].Split()[2];
                        CvInvoke.Rotate(r, r, RotateFlags.Rotate180);
                        CvInvoke.GaussianBlur(r, r, new System.Drawing.Size(7, 7), -1);
                        CvInvoke.Threshold(r, bin, 40, 255, ThresholdType.Binary);
                        var x_min = right_white_pixel(bin);
                        var dx = 25;
                        var ps_rec = new System.Drawing.Point[]
                        {
                        new System.Drawing.Point(x_min+dx, 0),
                        new System.Drawing.Point(x_min+dx, bin.Height-1),
                         new System.Drawing.Point(bin.Width-1, bin.Height-1),
                        new System.Drawing.Point(bin.Width-1, 0),
                        };
                        CvInvoke.FillPoly(bin, new VectorOfPoint(ps_rec), new MCvScalar(0));

                        CvInvoke.Rotate(bin, bin, RotateFlags.Rotate180);
                        CvInvoke.Imshow("bin", bin);
                        //CvInvoke.WaitKey();
                        if (up_surf == null)
                        {
                            up_surf = bin.Clone();
                        }
                        else
                        {
                            up_surf += bin;
                        }

                    }
                }
            }
           // CvInvoke.Imshow("bin2", up_surf);
            //CvInvoke.WaitKey();
            return up_surf.ToImage<Bgr,byte>().Mat;
        }
        static public Mat bin_to_green(Mat bin)
        {
            var im = bin.ToImage<Bgr, byte>();
            for (int j = 0; j < im.Height; j++)
            {
                for (int i = 0; i < im.Width; i++)
                {
                    im.Data[j, i, 0] = 0;
                    im.Data[j, i, 2] = 0;
                }
            }
            return im.Mat;
        }


        public bool calibrateLas_step(Mat[] mats, Mat orig, double[] positions, CameraCV cameraCV, PatternType patternType, GraphicGL graphicGL=null)
        {
            var inds_part = Detection.max_claster_im(cameraCV.scan_points.ToArray(), 4);

            // CvInvoke.Imshow("im1", mats[inds_part[inds_part.Length / 4]]);
            // CvInvoke.Imshow("im2", mats[inds_part[inds_part.Length* 2/ 4]]);
            // CvInvoke.Imshow("im3", mats[inds_part[inds_part.Length*3 / 4]]);

            //CvInvoke.WaitKey();

           var up_surf = bin_to_green( get_corners_calibrate_model(mats));

            //CvInvoke.Imshow(" up_surf", up_surf);
            //CvInvoke.WaitKey();
            var aff_matr = CameraCV.affinematr(Math.PI / 4,1,500);

            var aff_matr_inv = aff_matr.Clone();
            var up_s_r = new Mat();
            CvInvoke.WarpAffine(up_surf, up_s_r, aff_matr,new System.Drawing.Size(2000,2000));
            

            var ps_g = PointF.toSystemPoint(PointF.toPointF(find_gab_pix(up_s_r)));

            CvInvoke.Imshow(" up_surf", UtilOpenCV.drawPointsF(up_s_r.Clone(), ps_g, 255, 255, 0, 3));
            //CvInvoke.WaitKey();

            var aff_matr_3d = aff_matr.ConcateVertical(new Matrix<double>(new double[1, 3] { { 0, 0, 1 } }));
           
            var aff_matr_inv_3d = aff_matr_inv.ConcateVertical(new Matrix<double>(new double[1, 3] { { 0, 0, 1 } }));
            CvInvoke.Invert(aff_matr_3d, aff_matr_inv_3d, DecompMethod.LU);
            aff_matr_inv = aff_matr_inv_3d.GetRows(0, 2, 1);

            ps_g = UtilOpenCV.transfAffine(ps_g, aff_matr_inv);
            CvInvoke.WarpAffine(up_s_r, up_s_r, aff_matr_inv, new System.Drawing.Size(2000, 2000));
            //up_s_r = UtilOpenCV.drawPointsF(up_s_r, ps_g, 255, 255, 0, 3);
             //orig = UtilOpenCV.drawPointsF(orig, ps_g, 255, 0, 0,3);
            //CvInvoke.Imshow(" up_s_r", up_s_r);
           // CvInvoke.Imshow(" orig_ps", orig );
            //CvInvoke.WaitKey();
            var mats_calib = new Mat[] { mats[inds_part[inds_part.Length/4]], mats[inds_part[2 * inds_part.Length / 4]], mats[inds_part[3*inds_part.Length / 4]] };
            positions = new double[] { positions[inds_part[inds_part.Length / 4]], positions[inds_part[2 * inds_part.Length / 4]], positions[inds_part[3 * inds_part.Length / 4]] };

            var x_dim = 70;
            var y_dim = 50;

            // var corners = corner_step(orig);
           /* ps_g = new System.Drawing.PointF[]
             {
                new System.Drawing.PointF(376,542),
                new System.Drawing.PointF(384,144),
                new System.Drawing.PointF(919,160),
                new System.Drawing.PointF(917,547)

             };*/
            var corners = ps_g;

            

            var orig_c = orig.Clone();
            //orig_c = cameraCV.undist(orig_c);
            UtilOpenCV.drawPointsF(orig_c, corners,255,0,0,2,true);
            CvInvoke.Imshow("orig_corn", orig_c);
            CvInvoke.WaitKey();
            cameraCV.compPos(new MCvPoint3D32f[] {
                new MCvPoint3D32f(0,0,0),
            new MCvPoint3D32f(0,y_dim,0),
            new MCvPoint3D32f(x_dim,y_dim,0),
            new MCvPoint3D32f(x_dim,0,0)},corners);

             

            cur_matrix_cam = cameraCV.matrixCS;
            Console.WriteLine("cur_matrix_cam");
            prin.t(cur_matrix_cam);
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
          //  CvInvoke.Threshold(orig1, orig1, 30, 255, ThresholdType.Binary);
            CvInvoke.Imshow("corn1s", orig1);
            var cont = FindCircles.find_max_contour(orig1);
            var c_f = PointF.from_contour(cont);
            var corn = FindCircles.findGab(PointF.toSystemPoint(c_f));
           // UtilOpenCV.drawPointsF(orig1, corn, 255, 0, 0, 2, true);
            //UtilOpenCV.drawPointsF(orig, corn, 255, 0, 0, 2, true);
           // CvInvoke.Imshow("corns", orig1);


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
            if (cameraCV.compPos(mat, patternType,cameraCV.pattern_size))
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
