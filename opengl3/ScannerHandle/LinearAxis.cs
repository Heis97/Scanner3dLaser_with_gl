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
        public Flat3d_GL betw_LasFlat = new Flat3d_GL(1, 0, 0, 0);
        public Flat3d_GL stop_LasFlat = new Flat3d_GL(1, 0, 0, 0);
        public double start_pos = 0, betw_pos = 0, stop_pos = 0;

        public Flat3d_GL akoef, bkoef, ckoef, dkoef;

        bool calibrated = false;
        public GraphicGL GraphicGL;
        Matrix<double> cur_matrix_cam;
        int count_flats = 0;
        int start_flat = 0;
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

        public LinearAxis(string path)
        {
            MatrixesCamera = new List<Matrix<double>>();
            PositionsAxis = new List<double>();
            LasFlats = FileManage.loadFromJson_flats(path).ToList();
            akoef = LasFlats[0];
            bkoef = LasFlats[1];
            ckoef = LasFlats[2];
            dkoef = LasFlats[3];
            var st_ind = LasFlats[0].numb;
            //LasFlats.ad
            //var ind = System.IO.Path.GetFileNameWithoutExtension(path).Split('_')[1];
            start_flat = LasFlats[0].numb;


            calibrated = true;
        }
        public LinearAxis(Flat3d_GL f1, Flat3d_GL f2, Flat3d_GL f3, double pos1, double pos2, double pos3)
        {
            MatrixesCamera = new List<Matrix<double>>();
            PositionsAxis = new List<double>();
            LasFlats = new List<Flat3d_GL>();
            start_LasFlat = f1;
            betw_LasFlat = f2;
            stop_LasFlat = f3;
            start_pos = pos1;
            betw_pos = pos2;
            stop_pos = pos3;
            comp_flat_koef();
            calibrated = true;
        }

        public LinearAxis(Flat3d_GL f1, Flat3d_GL f2, Flat3d_GL f3, Flat3d_GL f4)
        {
            MatrixesCamera = new List<Matrix<double>>();
            PositionsAxis = new List<double>();
            LasFlats = new List<Flat3d_GL>();
            akoef = f1;
            bkoef = f2;
            ckoef = f3;
            dkoef = f4;
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
        public Mat get_corners_calibrate_model(Mat[] mats,CameraCV cameraCV)
        {
            Mat up_surf = null;
            var i_max = 0;
            var i_min = mats.Length-1;
            var delts = new float[mats.Length];
            var delts_b = new bool[mats.Length];
            double dest_max = 0;
            for (int i = 0; i< mats.Length;i++)
            {         
                var ps = Detection.detectLineDiff(cameraCV.undist( mats[i].Clone()),cameraCV.scanner_config);
                var ps_par = Detection.parall_Points(Detection.filtr_y0_Points(ps));
                var ps_o = LaserSurface.order_x(ps_par);
                if (ps_o == null) continue;
                if (ps_o.Length<3) continue;
                var del = ps_o[0].X - ps_o[ps_par.Length-1].X;
                delts[i] = Math.Abs(del);
                if (i > 1 && delts[i] < 10) delts[i] = delts[i - 1];
                if (delts[i]>dest_max && delts[i]<200)
                {
                    dest_max = delts[i];
                }
                Console.WriteLine(i + " " + delts[i]);
                 Mat test_1 = mats[i].Clone();
                 test_1 = UtilOpenCV.drawPointsF(test_1, ps_o, 0, 255, 0);
                // CvInvoke.Imshow("test"+i, test_1);
                 //CvInvoke.WaitKey();
            }
            Console.WriteLine("dest_max = "+  dest_max);
            for (int i = 0; i < delts.Length; i++)
            {
                if (delts[i] > dest_max*0.86)
                {
                    delts_b[i] = true;
                }
                if (delts[i]>200 && i>0)
                {
                    delts_b[i] = delts_b[i-1];
                }
                Console.WriteLine(i+" "+delts_b[i]+" "+ delts[i]);
            }
            for (int i = 1; i < delts_b.Length; i++)
            {
                if(delts_b[i-1] == false && delts_b[i] == true) i_min = i;
                if (delts_b[i - 1] == true && delts_b[i] == false) i_max = i;
            }
            Console.WriteLine(i_min + " " + i_max);
            int len_for_corn = 14;
            var rotate = false;
            for (int i = 0;  i < mats.Length; i++)
            {
                if ((i < i_min + len_for_corn && i > i_min) || ((i < i_max && i > i_max - len_for_corn)))
                {


                    Console.WriteLine(i);
                    //if (Math.Abs(delts[i]) > 50)
                    {
                        var bin = new Mat();
                        var r = mats[i].Split()[2];
                        if(rotate) CvInvoke.Rotate(r, r, RotateFlags.Rotate180);                       
                        //CvInvoke.WaitKey();
                        CvInvoke.GaussianBlur(r, r, new System.Drawing.Size(7, 7), -1);
                        
                        CvInvoke.Threshold(r, bin, 40, 255, ThresholdType.Binary);
                        //CvInvoke.Imshow("r", r);
                        //CvInvoke.Imshow("bin", bin);
                        //CvInvoke.WaitKey();
                        var x_min = right_white_pixel(bin);
                        var dx = 35;
                        var ps_rec = new System.Drawing.Point[]
                        {
                            new System.Drawing.Point(x_min+dx, 0),
                            new System.Drawing.Point(x_min+dx, bin.Height-1),
                            new System.Drawing.Point(bin.Width-1, bin.Height-1),
                            new System.Drawing.Point(bin.Width-1, 0),
                        };
                        
                        CvInvoke.FillPoly(bin, new VectorOfPoint(ps_rec), new MCvScalar(0));

                        if (rotate) CvInvoke.Rotate(bin, bin, RotateFlags.Rotate180);
                        //CvInvoke.Imshow("bin", bin);
                        //CvInvoke.WaitKey();
                        if (up_surf == null)
                        {
                            up_surf = bin.Clone();
                        }
                        else
                        {
                            up_surf += bin;
                        }
                       // CvInvoke.Imshow("up_surf", up_surf);
                       // CvInvoke.WaitKey();

                    }
                }
            }
            //CvInvoke.Imshow("bin2", up_surf);
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


        public bool calibrateLas_step(Mat[] mats, Mat orig, double[] positions, CameraCV cameraCV, PatternType patternType, GraphicGL graphicGL = null, PointCloud pointCloud = null, ScannerConfig config = null)
        {

            var pos_all = (double[])positions.Clone();
            //var sob = FindCircles.sobel_mat(orig);
            //CvInvoke.Imshow("sobel", sob);

            var inds_part = Detection.max_claster_im(cameraCV.scan_points.ToArray(), 4);

           /* CvInvoke.Imshow("im1", mats[inds_part[inds_part.Length / 4+10]]);
            CvInvoke.Imshow("im2", mats[inds_part[inds_part.Length* 2/ 4]]);
            CvInvoke.Imshow("im3", mats[inds_part[inds_part.Length*3 / 4-10]]);
            CvInvoke.WaitKey();
            */

           // var up_surf = bin_to_green( get_corners_calibrate_model(mats, cameraCV));
            var up_surf = orig.Clone();
           CvInvoke.Imshow(" up_surf", up_surf+orig);
            CvInvoke.WaitKey();
            var aff_matr = CameraCV.affinematr(Math.PI / 4,1,500);
            var aff_matr_inv = aff_matr.Clone();
            var up_s_r = new Mat();
            CvInvoke.WarpAffine(up_surf, up_s_r, aff_matr,new System.Drawing.Size(2000,2000));
            

            var ps_g = PointF.toSystemPoint(PointF.toPointF(find_gab_pix(up_s_r)));

            //CvInvoke.Imshow(" up_surf", UtilOpenCV.drawPointsF(up_s_r.Clone(), ps_g, 255, 255, 0, 3));
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
            //CvInvoke.Imshow(" orig_ps", orig );
            //CvInvoke.WaitKey();
            //var mats_calib = new Mat[] { mats[inds_part[inds_part.Length/4]], mats[inds_part[2 * inds_part.Length / 4]], mats[inds_part[3*inds_part.Length / 4]] };
            //positions = new double[] { positions[inds_part[inds_part.Length / 4]], positions[inds_part[2 * inds_part.Length / 4]], positions[inds_part[3 * inds_part.Length / 4]] };

            var mats_calib = new Mat[] { mats[inds_part[inds_part.Length / 4]], mats[inds_part[2 * inds_part.Length / 4]], mats[inds_part[3 * inds_part.Length / 4]] };
            //positions = new double[] { positions[inds_part[inds_part.Length / 4]], positions[inds_part[2 * inds_part.Length / 4]], positions[inds_part[3 * inds_part.Length / 4]] };
            var mats_calib_l = new List<Mat>();
            var positions_l = new List<double>();
            for(int i = inds_part.Length/4+5; i < 3* inds_part.Length/4-13;i++)//(int i = inds_part.Length/6; i < 5* inds_part.Length/6;i++ )
            {
                mats_calib_l.Add(mats[inds_part[i]]);
                //CvInvoke.Imshow(" mats[inds_part[i]]", mats[inds_part[i]]);
                //CvInvoke.WaitKey();
                positions_l.Add(positions[inds_part[i]]);
            }
            mats_calib = mats_calib_l.ToArray();
            positions = positions_l.ToArray();
            var x_dim = 70;//70
            var y_dim = 50;//50

            // var corners = corner_step(orig);
            /*  ps_g = new System.Drawing.PointF[]
               {
                  new System.Drawing.PointF(79,370),
                  new System.Drawing.PointF(79,90),
                  new System.Drawing.PointF(468,90),
                  new System.Drawing.PointF(475,367)

               };
              */
            /* ps_g = new System.Drawing.PointF[]
              {
                 new System.Drawing.PointF(199,401),
                 new System.Drawing.PointF(191,49),
                 new System.Drawing.PointF(678,32),
                 new System.Drawing.PointF(697,381)

              };*/
            ps_g = new System.Drawing.PointF[]
              {
                new System.Drawing.PointF(216,370),
                new System.Drawing.PointF(218,48),
                new System.Drawing.PointF(665,31),
                new System.Drawing.PointF(681,358)

              };
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
            var LasSurfs = new List<LaserSurface>();
            for (int i=0;i< mats_calib.Length;i++)
            {
                var las = new LaserSurface(mats_calib[i], cameraCV, patternType,graphicGL);    
                LasSurfs.Add(las);
                PositionsAxis.Add(positions[i]);
                Console.WriteLine(" "+positions[i]+" "+las.flat3D);
                var flat = graphicGL.addFlat3d_YZ(las.flat3D,null, Color3d_GL.gray());
                graphicGL.buffersGl.setTranspobj(flat, 0.3f);
                LasFlats.Add(las.flat3D);
            }


            //---------------------------------------
            var flat_b = LasFlats[1];
            var flat_e = LasFlats[LasFlats.Count - 2];
            var f_0 = LaserSurface.zeroFlatInCam_XZ(null, 0);

           // graphicGL.addFlat3d_YZ(flat_b);
            //graphicGL.addFlat3d_YZ(flat_e);
           // graphicGL.addFlat3d_XZ(f_0);

            //return false;
            var p_las = Flat3d_GL.cross(flat_e, flat_b, f_0);

            //PositionsAxis = new List<double>();
            var LasFlats_all = new List<Flat3d_GL>();
            for (int i = 0; i < mats_calib.Length; i++)
            {
                var fl = new Flat3d_GL(p_las, LasSurfs[i].ps[0], LasSurfs[i].ps[1]);

                //graphicGL.addPointMesh(new Point3d_GL[] { p_las, LasSurfs[i].ps[0], LasSurfs[i].ps[1] });
                //PositionsAxis.Add(positions[i]);
                Console.WriteLine(" " + positions[i] + " " + fl);
                //graphicGL.addFlat3d_YZ(las.flat3D,null,0.3f);
                LasFlats_all.Add(fl);
            }
            LasFlats = LasFlats_all;
            //return false;
            //---------------------------------------
           /* var f_i = 2;
            var l_i = LasSurfs.Count - 3;
            var ps1 = PointCloud.intersectWithFlat(new Line3d_GL[] { LasSurfs[f_i].lines[5], LasSurfs[f_i].lines[6] }, LasSurfs[f_i].flat3D);
            var ps2 = PointCloud.intersectWithFlat(new Line3d_GL[] { LasSurfs[l_i].lines[5], LasSurfs[l_i].lines[6] }, LasSurfs[l_i].flat3D);

            //graphicGL.addPointMesh(ps1, Color3d_GL.red());
            //graphicGL.addPointMesh(ps2, Color3d_GL.red());

            var flat1 = new Flat3d_GL(ps2[0], ps1[0], ps1[1]);
            var flat2 = new Flat3d_GL(ps2[0], ps1[1], ps2[1]);
            flat1 = (flat1 + flat2) / 2;
            //graphicGL.addFlat3d_XY(flat1);
            var LasFlats_all2 = new List<Flat3d_GL>();
            PositionsAxis = new List<double>();
            var mats_work = new List<Mat>();
            for (int i = 1; i < mats.Length; i+=1)
            {

                var points_im = Detection.detectLineDiff(cameraCV.undist ( mats[i].Clone()), cameraCV.scanner_config);
                var mat_p1 = UtilOpenCV.drawPointsF(mats[i].Clone(), points_im,0,255,0);
                //CvInvoke.Imshow("ma", mat_p1);
                //CvInvoke.WaitKey();
                if (points_im == null) continue;
                var ps_l = new List<PointF>();
                var len_from_board = 10;

                ps_l.Add(new PointF(points_im[len_from_board]));
                ps_l.Add(new PointF(points_im[points_im.Length - 1 - len_from_board]));
                var lines = PointCloud.computeTracesCam( ps_l.ToArray(),cameraCV,graphicGL);

                 var ps3 = PointCloud.intersectWithFlat(lines, flat1);

                 var fl = new Flat3d_GL(p_las, ps3[0], ps3[1]);
                // CvInvoke.Imshow("mats", mats[i]);
                
                //CvInvoke.WaitKey();
                mats_work.Add(mats[i]);
                // Console.WriteLine(" " + positions[i] + " " + fl);
               // graphicGL.addFlat3d_YZ(fl);
                LasFlats_all2.Add(fl);
                PositionsAxis.Add(pos_all[i]);
            }
            LasFlats = LasFlats_all2;
            PositionsAxis = pos_all.ToList();
            mats_calib = mats_work.ToArray();
            positions = PositionsAxis.ToArray();*/
            //---------------------------------------
            comp_flat_koef_full(LasFlats.ToArray(), PositionsAxis.ToArray(),"calibr");


            var flats_cam_sm = Flat3d_GL.gaussFilter_v2(LasFlats.ToArray(), 3).ToList();
            for(int i=0; i< LasFlats.Count;i++)
            {
                var fl = flats_cam_sm[i];
                fl.numb = (int)positions[i];
                flats_cam_sm[i] = fl;
            }

           // FileManage.saveToJson_flats(flats_cam_sm.ToArray(), "linearcal_" + positions[0] + "_.json");
            LasFlats = flats_cam_sm;
            start_flat =(int)LasFlats[0].numb;
            var flats_del = new List<Flat3d_GL>();
            calibrated = true;
            Console.WriteLine("_____________");
           pointCloud = null;

            if (pointCloud!=null)
            {
                var flats_cam = new List<Flat3d_GL>();
                for (int i = 0; i < mats_calib.Length; i++)
                {
                    flats_cam.Add(LasFlats[i]);
                }
               // var flats_cam_sm = Flat3d_GL.gaussFilter_v2(flats_cam.ToArray(), 3).ToList();
                for (int i = 0; i < mats_calib.Length; i++)
                {
                    var m_c = cameraCV.undist(mats_calib[i].Clone());
                    var points_im = Detection.detectLineDiff(m_c, config);
                    var mat_p = UtilOpenCV.drawPointsF(m_c, points_im, 0, 255, 0, 1);
                    //CvInvoke.Imshow("sf", mat_p);
                   // CvInvoke.WaitKey();
                    var y1 = 325;
                    var y2 = 270;
                    var y3 = 240;
                    var p_i_1 = Detection.p_in_ps_by_y(points_im, y1);
                    var p_i_2 = Detection.p_in_ps_by_y(points_im, y2);
                    var p_i_3 = Detection.p_in_ps_by_y(points_im, y3);
                    Console.Write(" " + positions[i] + " " + points_im[p_i_1].X);

                    var get_flat = getLaserSurf(positions[i]);
                    //LasFlats[i] = new Flat3d_GL(LasFlats[i].A, get_flat.B, LasFlats[i].C, LasFlats[i].D);

                    //var las_fl_c = flats_cam_sm[i];//flats_cam_sm[i] // LasFlats[i]
                    var points_cam = PointCloud.fromLines(points_im, cameraCV, get_flat);//flats_cam_sm[i]);
                    //flats_del.Add(flats_cam_sm[i] - getLaserSurf(positions[i]));

                    Console.WriteLine(" "+ get_flat);

                  //  Console.WriteLine(LasFlats_all[i] - LasFlats[i]);
                    //Console.WriteLine((flats_cam_sm[i] - getLaserSurf(positions[i])).D +" "+ (LasFlats[i] - getLaserSurf(positions[i])).D);
                    // points_cam = PointCloud.color_points3d(points_im, points_cam, orig);

                    //var las_fl = -flat_las_from_ps(points_cam[p_i], linearAxis);

                    //graphicGL?.addFlat3d_YZ(las_fl);


                    //Console.Write(" " + points_cam[p_i_1].z + " " + points_cam[p_i_2].z + " " + points_cam[p_i_3].z);// + pos_from_las_flat(las_fl, linearAxis));

                    //graphicGL.addFlat3d_YZ(linearAxis.getLaserSurf(LinPos),null,0.1f,0.1f,0.4f);
                    pointCloud.points3d_cur = points_cam;
                    //points3d_cur = camToScene(points_cam, cameraCV.matrixCS);
                    //prin.t(cameraCV.matrixCS);
                    ///Console.WriteLine();
                    //var ps_list = points3d.ToList();
                    //ps_list.AddRange(pointCloud.points3d_cur);
                    pointCloud.points3d_lines.Add(pointCloud.points3d_cur);
                }




                for (int i = 5; i < 40; i += 10)
                {
                    Console.WriteLine(i + ":_______________________");
                    //var flats_del_sm = Flat3d_GL.gaussFilter_v2(flats_del.ToArray(), i);
                 //   foreach (var fl in flats_del_sm) Console.WriteLine(fl);
                }



            }

            

            calibrated = true;
            return true;
        }



        System.Drawing.PointF[] corner_step(Mat orig)
        {
            var orig1= orig.Clone();
            orig1 = FindCircles.sobel_mat(orig1);

            CvInvoke.CvtColor(orig1, orig1, ColorConversion.Rgb2Gray);
            CvInvoke.MedianBlur(orig1, orig1, 5);
            //CvInvoke.Threshold(orig1, orig1, 30, 255, ThresholdType.Binary);
            CvInvoke.Imshow("corn1s", orig1);
            var cont = FindCircles.find_max_contour(orig1);
            var c_f = PointF.from_contour(cont);
            var corn = FindCircles.findGab(PointF.toSystemPoint(c_f));
            //UtilOpenCV.drawPointsF(orig1, corn, 255, 0, 0, 2, true);
            //UtilOpenCV.drawPointsF(orig, corn, 255, 0, 0, 2, true);
            //CvInvoke.Imshow("corns", orig1);


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

        void comp_flat_koef()
        {

            /* int start_ind = (int)(LasFlats.Count / 2) + 1;
                        int end_ind = (int)(LasFlats.Count / 2) - 1;*/
            int grad = 1;
            var flats = new Flat3d_GL[] { start_LasFlat, betw_LasFlat, stop_LasFlat };
            var poses = new double[] { start_pos, betw_pos, stop_pos };
            var aval = new double[flats.Length][];
            var bval = new double[flats.Length][];
            var cval = new double[flats.Length][];
            var dval = new double[flats.Length][];
            for (int i = 0; i < flats.Length; i++)
            {
                aval[i] = new double[] { poses[i], flats[i].A  };
                bval[i] = new double[] { poses[i], flats[i].B };
                cval[i] = new double[] { poses[i], flats[i].C };
                dval[i] = new double[] { poses[i], flats[i].D };
            }
            akoef =new Flat3d_GL( Regression.regression(aval, grad));
            bkoef = new Flat3d_GL(Regression.regression(bval, grad));
            ckoef = new Flat3d_GL(Regression.regression(cval, grad));
            dkoef = new Flat3d_GL(Regression.regression(dval, grad));
            calibrated = true;
            var fl1 = getLaserSurf(poses[0]);

            var fl2 = getLaserSurf(poses[1]);
            var fl3 = getLaserSurf(poses[2]);
        }

        void comp_flat_koef_full(Flat3d_GL[] flats, double[] poses,string name = "default")
        {

            /* int start_ind = (int)(LasFlats.Count / 2) + 1;
                        int end_ind = (int)(LasFlats.Count / 2) - 1;*/
            int grad = 1;
            var aval = new double[flats.Length][];
            var bval = new double[flats.Length][];
            var cval = new double[flats.Length][];
            var dval = new double[flats.Length][];
            for (int i = 0; i < flats.Length; i++)
            {
                aval[i] = new double[] { poses[i], flats[i].A };
                bval[i] = new double[] { poses[i], flats[i].B };
                cval[i] = new double[] { poses[i], flats[i].C };
                dval[i] = new double[] { poses[i], flats[i].D };
            }
            akoef = new Flat3d_GL(Regression.regression(aval, grad));
            bkoef = new Flat3d_GL(Regression.regression(bval, grad));
            ckoef = new Flat3d_GL(Regression.regression(cval, grad));
            dkoef = new Flat3d_GL(Regression.regression(dval, grad));

            FileManage.saveToJson_flats(new Flat3d_GL[] {akoef, bkoef, ckoef, dkoef}, "linearcal_" + poses[0] + "_.json");
            calibrated = true;
           /* var fl1 = getLaserSurf(poses[0]);

            var fl2 = getLaserSurf(poses[1]);
            var fl3 = getLaserSurf(poses[2]);*/
        }

        void compOneFlat()
        {
         
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

        public Flat3d_GL getLaserSurf_old(double PositionLinear)
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

        public Flat3d_GL getLaserSurf(double PositionLinear)//_regr2
        {
            if (calibrated)
            {
                var a = Regression.calcPolynSolv(akoef.ToDouble(), PositionLinear);
                var b = Regression.calcPolynSolv(bkoef.ToDouble(), PositionLinear);
                var c = Regression.calcPolynSolv(ckoef.ToDouble(), PositionLinear);
                var d = Regression.calcPolynSolv(dkoef.ToDouble(), PositionLinear);
           
                return new Flat3d_GL(a,b,c,d);
            }
            return new Flat3d_GL();
        }

        public Flat3d_GL getLaserSurf_match(double PositionLinear)//_match
        {
            if (calibrated)
            {
                var ind = (int)PositionLinear - start_flat;
                if(ind <0 || ind > LasFlats.Count-1) return new Flat3d_GL();
                return LasFlats[ind];
            }
            return new Flat3d_GL();
        }
        static public LinearAxis load_old(string path)
        {
            var settings = Settings_loader.load_data(path);

            var oneF = (Flat3d_GL)settings[0];
            var stF = (Flat3d_GL)settings[1];
            var stP = (double)settings[2];
            return new LinearAxis(oneF, stF, stP);
        }
        static public LinearAxis load_old2(string path)
        {
            var settings = Settings_loader.load_data(path);

            var f1 = (Flat3d_GL)settings[0];
            var f2 = (Flat3d_GL)settings[1];
            var f3 = (Flat3d_GL)settings[2];
            var pos1 = (double)settings[3];
            var pos2 = (double)settings[4];
            var pos3 = (double)settings[5];
            return new LinearAxis(f1, f2, f3, pos1, pos2, pos3);
        }
        static public LinearAxis load_old_v2(string path)
        {
            var settings = Settings_loader.load_data(path);

            var f1 = (Flat3d_GL)settings[0];
            var f2 = (Flat3d_GL)settings[1];
            var f3 = (Flat3d_GL)settings[2];
            var f4 = (Flat3d_GL)settings[3];
            return new LinearAxis(f1, f2, f3,f4);
        }

        public void save_old_2(string path)
        {
            Settings_loader.save_file(path, new object[] { start_LasFlat, betw_LasFlat, stop_LasFlat , start_pos, betw_pos, stop_pos });
        }
        public void save(string path)
        {
            Settings_loader.save_file(path, new object[] { akoef, bkoef, ckoef, dkoef });
        }

    }
}
