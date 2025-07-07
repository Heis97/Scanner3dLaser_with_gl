using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace opengl3
{
    
    public class StereoCamera
    {
        //base = world, flange, scanner = cam1, model = local cord
        public enum mode {model,world,camera,world_virt };
        public CameraCV[] cameraCVs;
        public RobotFrame cur_pos;
        public Matrix<double> R;//1 * R -> 2
        public Matrix<double> Bfs;//Flange->scaner

        public Matrix<double> Bfs_r;//Flange->scaner

        public Matrix<double> Bbf;//Base->Flange

        public Matrix<double> BS;//Flange->scaner
        public mode scan_coord_sys;

        public StereoCamera(CameraCV[] _cameraCVs,string bfs_file = null)
        {
            cameraCVs = _cameraCVs;  
            if(bfs_file != null)
            {
                /*string file;
                using (StreamReader sr = new StreamReader(bfs_file))
                {
                    file = sr.ReadToEnd();
                }
                string[] lines = file.Split(new char[] { '\n' });
                Bfs = Settings_loader.matrix_load(lines[0]);*/
                Bfs = (Matrix<double>)Settings_loader.load_data(bfs_file)[0];
            }

           
        }

        


        public void calibrate(Mat[] mats,PatternType patternType,System.Drawing.Size pattern_size, float marksize)
        {
            if(mats.Length == cameraCVs.Length)
            {
               
                bool comp_pos = true;
                for(int i = 0; i < mats.Length; i++)
                {
                    comp_pos &= cameraCVs[i].compPos(mats[i], patternType, pattern_size,marksize);
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

                    inv_cs1 = new Matrix<double>(4, 4);
                    CvInvoke.Invert(cameraCVs[1].matrixSC, inv_cs1, DecompMethod.LU);
                    R = cameraCVs[0].matrixSC* inv_cs1  ;

                    Settings_loader.save_file("stereo_cal.txt", new object[] { R });
                    prin.t("stereo calib R_sc:");
                    prin.t(R);
                }
            }
        }
        public void calibrate_stereo(Frame[] frames, PatternType patternType, System.Drawing.Size pattern_size)
        {
            
            if (cameraCVs.Length==2)
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    var mark_size = 6.2273f;
                    var pos1 = cameraCVs[0].compPos(frames[i].im, patternType, pattern_size, mark_size);
                    var pos2 = cameraCVs[1].compPos(frames[i].im_sec, patternType, pattern_size, mark_size);
                    if(pos1&&pos2)
                    {
                        var inv_cs1 = new Matrix<double>(4, 4);
                        CvInvoke.Invert(cameraCVs[0].matrixCS, inv_cs1, DecompMethod.LU);

                        R = inv_cs1 * cameraCVs[1].matrixCS; 
                        var c1 = cameraCVs[0].matrixCS;
                        Console.WriteLine(i + " " + R[0, 3] + " " + R[1, 3] + " " + R[2, 3] + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " "//
                        + c1[0, 3] + " " + c1[1, 3] + " " + c1[2, 3] + " " + c1[2, 0] + " " + c1[2, 1] + " " + c1[2, 2]) ;// ; ;
                        GC.Collect();
                    }

                    //Console.WriteLine(comp_pos);
                }
            }
        }

        public void calibrate_stereo_rob(Frame[] frames, PatternType patternType, System.Drawing.Size pattern_size, float markSize)
        {

            if (cameraCVs.Length == 2)
            {
                var p_rob = new List<Point3d_GL>();
                var p_cam = new List<Point3d_GL>();
                Console.WriteLine("calibrate_stereo_rob");
               
                for (int i = 0; i < frames.Length; i++)
                {
                    var pos1 = cameraCVs[0].compPos(frames[i].im, patternType, pattern_size, markSize);
                    var pos2 = cameraCVs[1].compPos(frames[i].im_sec, patternType, pattern_size, markSize);
                    if (pos1 && pos2)
                    {
                        var inv_cs1 = new Matrix<double>(4, 4);
                        CvInvoke.Invert(cameraCVs[0].matrixCS, inv_cs1, DecompMethod.LU);

                        R = inv_cs1 * cameraCVs[1].matrixCS;
                        var c1 = cameraCVs[0].matrixCS;
                        var rob_pos = new RobotFrame(frames[i].name);
                        var r1 = rob_pos.getMatrix();
                        var p1 = new Point3d_GL(r1[0, 3], r1[1, 3], r1[2, 3]);
                        var p2 = new Point3d_GL(c1[0, 3], c1[1, 3], c1[2, 3]);
                        p_rob.Add(p1);
                        p_cam.Add(p2);
                        
                        //Console.WriteLine(i + " " + r1[0, 3] + " " + r1[1, 3] + " " + r1[2, 3]+" ");// + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " ");
                        Console.WriteLine(R[0, 3] + " " + R[1, 3] + " " + R[2, 3] + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " ");// + c1[0, 3] + " " + c1[1, 3] + " " + c1[2, 3] + " " + c1[2, 0] + " " + c1[2, 1] + " " + c1[2, 2]) ;// ; ;
                        GC.Collect();
                    }

                    //Console.WriteLine(comp_pos);
                }
                for (int i = 1; i <p_cam.Count; i++)
                {
                    Console.WriteLine((p_cam[i] - p_cam[0]).magnitude() + " " + (p_rob[i] - p_rob[0]).magnitude());
                }

            }
        }
        static public List< Matrix<double>> calibrate_stereo_rob_handeye(CameraCV cameraCV, Frame[] frames, PatternType patternType, System.Drawing.Size pattern_size, float markSize, string file_name = "bfs_cal.txt", RobotFrame.RobotType robot = RobotFrame.RobotType.PULSE,GraphicGL graphic = null,int k = 1)
        {

            var p_rob = new List<Point3d_GL>();
            var p_cam = new List<Point3d_GL>();

            var ms_rob = new List<Matrix<double>>();
            var ms_cam = new List<Matrix<double>>();
            Console.WriteLine("calibrate_stereo_rob");
            for (int i = 0; i < frames.Length; i++)
            {
                Console.WriteLine(frames[i].name);
                var pos1 = cameraCV.compPos(frames[i].im, patternType, pattern_size, markSize);
                if (pos1 )
                {
                    var inv_cs1 = new Matrix<double>(4, 4);//
                    CvInvoke.Invert(cameraCV.matrixCS, inv_cs1, DecompMethod.LU);

                    //R = inv_cs1 * cameraCV.matrixCS;
                    var c1 = cameraCV.matrixCS;
                    var rob_pos = new RobotFrame(frames[i].name,robot);
                    var r1 = rob_pos.getMatrix();
                       
                    var r1_inv = r1.Clone();
                    var c1_inv = c1.Clone();
                    CvInvoke.Invert(r1_inv, r1_inv, DecompMethod.Svd);
                    CvInvoke.Invert(c1_inv, c1_inv, DecompMethod.Svd);

                    ms_rob.Add(r1);
                    ms_cam.Add(c1_inv);
                    var p1 = new Point3d_GL(r1[0, 3], r1[1, 3], r1[2, 3]);
                    var p2 = new Point3d_GL(c1[0, 3], c1[1, 3], c1[2, 3]);
                    p_rob.Add(p1);
                    p_cam.Add(p2);

                    //Console.WriteLine(i + " " + r1[0, 3] + " " + r1[1, 3] + " " + r1[2, 3]+" ");// + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " ");
                    //Console.WriteLine(R[0, 3] + " " + R[1, 3] + " " + R[2, 3] + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " ");// + c1[0, 3] + " " + c1[1, 3] + " " + c1[2, 3] + " " + c1[2, 0] + " " + c1[2, 1] + " " + c1[2, 2]) ;// ; ;
                    GC.Collect();
                }

                //Console.WriteLine(comp_pos);
            }
            //int k = 12;
            ms_rob = get_matrices_filter(ms_rob,k);
            ms_cam = get_matrices_filter(ms_cam, k);
            VectorOfMat mr_r, mr_t, mc_r, mc_t;
            (mr_r, mr_t) = UtilOpenCV.to_vec_mat(ms_rob.ToArray());
            (mc_r, mc_t) = UtilOpenCV.to_vec_mat(ms_cam.ToArray());
            Mat mr = new Mat();
            Mat mt = new Mat();
            CvInvoke.CalibrateHandEye(mr_r, mr_t, mc_r, mc_t, mr, mt,HandEyeCalibrationMethod.Andreff);
            prin.t("result:");
            prin.t(mr);
            prin.t(mt);
            prin.t("______");
            var mt_m= new Matrix<double>( (double[,])(mt.GetData()));
            var mr_m = new Matrix<double>((double[,])(mr.GetData()));
            Console.WriteLine(new Point3d_GL(mt_m[0,0], mt_m[1,0], mt_m[2,0]).magnitude()+" ");

            var Bfs_med = new Matrix<double>(new double[4, 4]);

            for(int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Bfs_med[i, j] = mr_m[i, j];
                }
            }
            Bfs_med[0, 3] = mt_m[0, 0];
            Bfs_med[1, 3] = mt_m[1, 0];
            Bfs_med[2, 3] = mt_m[2, 0];
            Bfs_med[3, 3] = 1;

            var Bfs_med_real = new Matrix<double>(new double[,] { { -1, 0, 0, 8 }, { 0, 0, 1, 16 }, { 0, 1, 0, 38 }, { 0, 0, 0, 1 } });//
           // Bfs_med = Bfs_med_real;//
            var ms_check = new List<Matrix<double>>();

            var pattern = ms_rob[0] * Bfs_med * ms_cam[0];
            var bfs_del = Bfs_med - Bfs_med_real;
            prin.t("bfs_del");
            prin.t(bfs_del);
            ms_check.Add(pattern);
            prin.t("pattern");
            prin.t(pattern);
            //graphic.addFrame(pattern,50,"PATTERN");
            for (int i = 0; i < ms_cam.Count; i++)
            {
                //Console.WriteLine((p_cam[i] - p_cam[0]).magnitude() + " " + (p_rob[i] - p_rob[0]).magnitude());
                //var m_check = ms_rob[i] * Bfs_med * ms_cam[i];
                var m_check = ms_rob[i];// * Bfs_med;// * ms_cam[i];
                ms_check.Add(m_check);

                var m_check2 = ms_rob[i] * Bfs_med * ms_cam[i];
                //graphic.addFrame(m_check2);
                //prin.t(m_check2);
                 
                //Console.WriteLine(m_check2[0, 3] + " " + m_check2[1, 3] + " " + m_check2[2, 3] + " " + i);
            }

            for (int i = 0; i < ms_cam.Count; i++)
            {
                var m_check = ms_rob[i] * Bfs_med * ms_cam[i];
                Console.WriteLine(m_check[0, 3] + " " + m_check[1, 3] + " " + m_check[2, 3] + " " + i);
            }


            Settings_loader.save_file(file_name, new object[] { Bfs_med });// Bfs_med_real   // Bfs_med
            return ms_check;
        }

        static List<Matrix<double>> get_matrices_filter(List<Matrix<double>> matrices,int k)
        {
            var ms_filt = new List<Matrix<double>>();
            for (int i = 0; i < matrices.Count; i+=k)
            {
                ms_filt.Add(matrices[i]);
            }
            return ms_filt;
        }

        public void calibrate_basis_rob_xyz(Frame[] frames, PatternType patternType, System.Drawing.Size pattern_size, float markSize)
        {

            if (cameraCVs.Length == 2)
            {
                var p_rob = new List<Point3d_GL>();
                var p_cam = new List<Point3d_GL>();
                Console.WriteLine("calibrate_basis_rob");

                for (int i = 0; i < frames.Length; i++)
                {
                    var pos1 = cameraCVs[0].compPos(frames[i].im, patternType, pattern_size, markSize);
                    if (pos1)
                    {
                        var c1 = cameraCVs[0].matrixCS;
                        var rob_pos = new RobotFrame(frames[i].name);
                        var r1 = rob_pos.getMatrix();
                        var p1 = new Point3d_GL(r1[0, 3], r1[1, 3], r1[2, 3]);
                        var p2 = new Point3d_GL(c1[0, 3], c1[1, 3], c1[2, 3]);
                        p_rob.Add(p1);
                        p_cam.Add(p2);

                        //Console.WriteLine(i + " " + r1[0, 3] + " " + r1[1, 3] + " " + r1[2, 3]+" ");// + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " ");
                        //Console.WriteLine(i + " " + c1[0, 3] + " " + c1[1, 3] + " " + c1[2, 3] + " ");
                        //Console.WriteLine(R[0, 3] + " " + R[1, 3] + " " + R[2, 3] + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " ");// + c1[0, 3] + " " + c1[1, 3] + " " + c1[2, 3] + " " + c1[2, 0] + " " + c1[2, 1] + " " + c1[2, 2]) ;// ; ;
                        GC.Collect();
                    }
                    
                }
                var matr1 = UtilMatr.calcTransformMatr_cv(p_cam.ToArray(), p_rob.ToArray());

                Settings_loader.save_file("basis_matr_xyz.txt", new object[] { matr1 });
                prin.t(matr1);
                prin.t("____________");

                for (int i = 1; i < p_cam.Count; i++)
                {
                    Console.WriteLine("dist: "+(p_cam[i] - p_cam[0]).magnitude() + " " + (p_rob[i] - p_rob[0]).magnitude());

                    Console.WriteLine("p_cam[i] * matr1: "+ matr1*p_cam[i]  +"\n"+ "p_rob[i]:          " + p_rob[i]);

                    //Console.WriteLine("p_rob[i] * matr1: " + matr1 * p_rob[i]+ "\n" + "p_cam[i]:        " + p_cam[i]);
                    prin.t("____________");

                }

            }
        }

        public void calibrate_basis_rob_abc(Frame[] frames, PatternType patternType, System.Drawing.Size pattern_size, float markSize)
        {

            if (cameraCVs.Length == 2)
            {
                var p_rob = new List<Point3d_GL>();
                var p_cam = new List<Point3d_GL>();
                Console.WriteLine("calibrate_basis_rob");

                for (int i = 0; i < frames.Length; i++)
                {
                    var pos1 = cameraCVs[0].compPos(frames[i].im, patternType, pattern_size, markSize);
                    if (pos1)
                    {
                        var c1 = cameraCVs[0].matrixCS;
                        var rob_pos = new RobotFrame(frames[i].name);
                        var cam_pos = new RobotFrame(cameraCVs[0].matrixCS);
                        var r1 = rob_pos.getMatrix();
                        var p1 = rob_pos.get_rot();
                        var p2 = cam_pos.get_rot();
                        p_rob.Add(p1);
                        p_cam.Add(p2);

                        //Console.WriteLine(i + " " + r1[0, 3] + " " + r1[1, 3] + " " + r1[2, 3]+" ");// + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " ");
                        //Console.WriteLine(i + " " + c1[0, 3] + " " + c1[1, 3] + " " + c1[2, 3] + " ");
                        //Console.WriteLine(R[0, 3] + " " + R[1, 3] + " " + R[2, 3] + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " ");// + c1[0, 3] + " " + c1[1, 3] + " " + c1[2, 3] + " " + c1[2, 0] + " " + c1[2, 1] + " " + c1[2, 2]) ;// ; ;
                        GC.Collect();
                    }

                }
                var matr1 = UtilMatr.calcTransformMatr_cv(p_cam.ToArray(), p_rob.ToArray());

                Settings_loader.save_file("basis_matr_abc.txt", new object[] { matr1 });
                prin.t(matr1);
                prin.t("____________");

                for (int i = 1; i < p_cam.Count; i++)
                {
                    Console.WriteLine("dist: " + (p_cam[i] - p_cam[0]).magnitude() + " " + (p_rob[i] - p_rob[0]).magnitude());

                    Console.WriteLine("p_cam[i] * matr1: " + matr1 * p_cam[i] + "\n" + "p_rob[i]:          " + p_rob[i]);

                    //Console.WriteLine("p_rob[i] * matr1: " + matr1 * p_rob[i]+ "\n" + "p_cam[i]:        " + p_cam[i]);
                    prin.t("____________");

                }

            }
        }

        public void calibrate_basis_rob_abc_test(Frame[] frames, PatternType patternType, System.Drawing.Size pattern_size, float markSize)
        {

            if (cameraCVs.Length == 2)
            {
                var p_rob = new List<Point3d_GL>();
                var p_cam = new List<Point3d_GL>();
                Console.WriteLine("calibrate_basis_rob");

                for (int i = 0; i < frames.Length; i++)
                {
                    var pos1 = cameraCVs[0].compPos(frames[i].im, patternType, pattern_size, markSize);
                    if (pos1)
                    {
                        var c1 = cameraCVs[0].matrixCS;
                        var rob_pos = new RobotFrame(frames[i].name);
                        var cam_pos = new RobotFrame(cameraCVs[0].matrixCS);
                        var r1 = rob_pos.getMatrix();
                        var p1 = rob_pos.get_rot();
                        var p2 = cam_pos.get_rot();
                        p_rob.Add(p1);
                        p_cam.Add(p2);

                        //Console.WriteLine(i + " " + r1[0, 3] + " " + r1[1, 3] + " " + r1[2, 3]+" ");// + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " ");
                        //Console.WriteLine(i + " " + c1[0, 3] + " " + c1[1, 3] + " " + c1[2, 3] + " ");
                        //Console.WriteLine(R[0, 3] + " " + R[1, 3] + " " + R[2, 3] + " " + " " + R[0, 2] + " " + R[1, 2] + " " + R[2, 2] + " ");// + c1[0, 3] + " " + c1[1, 3] + " " + c1[2, 3] + " " + c1[2, 0] + " " + c1[2, 1] + " " + c1[2, 2]) ;// ; ;
                        GC.Collect();
                    }

                }
                //var matr1 = UtilMatr.calcTransformMatr_cv(p_cam.ToArray(), p_rob.ToArray());

                //Settings_loader.save_file("basis_matr_abc.txt", new object[] { matr1 });
                var matr1 = (Matrix<double>)Settings_loader.load_data("basis_matr_abc.txt")[0];
                Bfs_r = matr1;
                prin.t(matr1);
                prin.t("____________");

                for (int i = 1; i < p_cam.Count; i++)
                {
                    Console.WriteLine("dist: " + (p_cam[i] - p_cam[0]).magnitude() + " " + (p_rob[i] - p_rob[0]).magnitude());

                    Console.WriteLine("p_cam[i] * matr1: " + matr1 * p_cam[i] + "\n" + "p_rob[i]:          " + p_rob[i]);

                    //Console.WriteLine("p_rob[i] * matr1: " + matr1 * p_rob[i]+ "\n" + "p_cam[i]:        " + p_cam[i]);
                    prin.t("____________");

                }

            }
        }

        public Matrix<double> calibrateBfs(Frame[] pos,System.Drawing.Size pattern_size, float markSize,string file_name = "bfs_cal.txt")
        {
            var Bfs_l = new List<Matrix<double>>();
            RobotFrame.RobotType robotType = RobotFrame.RobotType.PULSE;
            
            //var markSize = 10f;
            for (int i=0; i<pos.Length; i++)
            {
                cameraCVs[0].compPos(pos[i].im, PatternType.Mesh, pattern_size, markSize);
                var Bsm = cameraCVs[0].matrixCS.Clone();
                var Bbf = new RobotFrame(pos[i].name,robotType).getMatrix();
                //var Bft = new RobotFrame("-0.1709395 0.0687465 0.0480957 1.55116 1.1946166 0.0").getMatrix(robotType);
                //Bbf *= Bft;
                //prin.t(pos[i].name);
                // prin.t(Bbf);
                //var Bbm = new RobotFrame("510.9 6.4 55.4 1.5 -0.002 -0.1").getMatrix(robotType);

                //var Bbm = new RobotFrame("-193.677 -334.085 -30.528 -0.01515 -0.00087 -1.54447").getMatrix(robotType);
                //var Bbm = new RobotFrame("-199.191 -350.3198 5.9134 -0.00519 -0.00268 -1.52663").getMatrix(robotType);
                //var Bbm = new RobotFrame("-218.37 -239.51 -33.85 -0.014 -0.0058 -1.5658").getMatrix(robotType);

                //var Bbm = new RobotFrame("-227.8394 -380.7143 -33.724 -0.00703 -0.00259 -1.569604").getMatrix(robotType);
                //var Bbm = new RobotFrame("-226.7156 -339.505 -33.46688 -0.00779 -0.00117 -1.552798").getMatrix(robotType);

                var Bbm = new RobotFrame("-419.22153 -18.75754 43.3817 0.0120435 -0.0045299 1.64342",robotType).getMatrix();

                /* prin.t("--------------------------------");
                 prin.t("Bsm");
                 prin.t(Bsm);
                 prin.t("Bbf");
                 prin.t(Bbf);
                 prin.t("Bbm");
                 prin.t(Bbm);*/
                var Bsm_1 = Bsm.Clone();
                var Bbf_1 = Bbf.Clone();
                var Bbm_1 = Bbm.Clone();
                CvInvoke.Invert(Bsm, Bsm_1, DecompMethod.LU);
                CvInvoke.Invert(Bbf, Bbf_1, DecompMethod.LU);
                CvInvoke.Invert(Bbm, Bbm_1, DecompMethod.LU);
                if(robotType == RobotFrame.RobotType.KUKA) Bbm_1 = Bbm;                
                var Bfs = Bbf_1 * Bbm_1 * Bsm;
                //var Bfs = Bbf_1 * Bbm_1 * Bsm;
                Bfs_l.Add(Bfs);
                Console.WriteLine(Bfs[0, 3] + " " + Bfs[1, 3] + " " + Bfs[2, 3] + " " + Bfs[0, 0] + " " + Bfs[0, 1] + " " + Bfs[0, 2]);
                /*
                //prin.t(Bfs);
                using (StreamWriter sw = new StreamWriter(file_name, false, Encoding.UTF8))
                {
                    sw.WriteLine(Settings_loader.matrix_save(Bfs));
                }
                Console.WriteLine(Bfs[0, 3] + " " + Bfs[1, 3] + " " + Bfs[2, 3] + " " + Bfs[0, 0] + " " + Bfs[0, 1] + " " + Bfs[0, 2]);
                
                */
               
            }
            var Bfs_med = new Matrix<double>(new double[4,4]);
            for(int i=0; i<Bfs_l.Count;i++)
            {
                Bfs_med+=Bfs_l[i];
            }
            Bfs_med /= Bfs_l.Count;
            Settings_loader.save_file(file_name, new object[] { Bfs_med });
            return Bfs_med;
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
        static public void calcSizesScanner_v3(double alpha, double beta, double L)//alpha - fov, beta - cam_z^surf_n
        {
            var gamma = (180 - alpha) / 2;
            var tetta = 180 - beta - gamma;
            var omega = 90 - beta;
            var m1 = (L * sin(tetta)) / (2 * sin(gamma));
            var m2 = m1 * tg(beta);
            var k1 = m1 / cos(beta);
            var k2 = m1 * (tg(gamma) - tg(beta));
            // var k = (tg(gamma) - tg(beta)) * cos(beta);
            var y = k2 * cos(omega) - L / 2 + k1;

            var z = k2 * sin(omega);
            Console.WriteLine("X: " + y + "; Y: " + z);
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
