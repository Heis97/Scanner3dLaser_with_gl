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
        public enum mode {model,world,camera };
        public CameraCV[] cameraCVs;
        public Matrix<double> R;//1 * R -> 2
        public Matrix<double> Bfs;//Flange->scaner
        public Matrix<double> Bbf;//Base->Flange
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

        


        public void calibrate(Mat[] mats,PatternType patternType,System.Drawing.Size pattern_size)
        {
            if(mats.Length == cameraCVs.Length)
            {
                bool comp_pos = true;
                for(int i = 0; i < mats.Length; i++)
                {
                    comp_pos &= cameraCVs[i].compPos(mats[i], patternType, pattern_size,10f);
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

                    /*inv_cs1 = new Matrix<double>(4, 4);
                    CvInvoke.Invert(cameraCVs[1].matrixSC, inv_cs1, DecompMethod.LU);
                    R = cameraCVs[0].matrixSC* inv_cs1  ;
                    prin.t("stereo calib R_sc:");
                    prin.t(R);*/
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

        public void calibrate_stereo_rob(Frame[] frames, PatternType patternType, System.Drawing.Size pattern_size)
        {

            if (cameraCVs.Length == 2)
            {
                var p_rob = new List<Point3d_GL>();
                var p_cam = new List<Point3d_GL>();
                Console.WriteLine("calibrate_stereo_rob");
                for (int i = 0; i < frames.Length; i++)
                {
                    var pos1 = cameraCVs[0].compPos(frames[i].im, patternType, pattern_size, 10f);
                    var pos2 = cameraCVs[1].compPos(frames[i].im_sec, patternType, pattern_size, 10f);
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

        public Matrix<double> calibrateBfs(Frame[] pos,System.Drawing.Size pattern_size, string file_name = "bfs_cal.txt")
        {
            var Bfs_l = new List<Matrix<double>>();
            RobotFrame.RobotType robotType = RobotFrame.RobotType.PULSE;
            for (int i=0; i<pos.Length; i++)
            {
                cameraCVs[0].compPos(pos[i].im, PatternType.Mesh, pattern_size, 10f);
                var Bsm = cameraCVs[0].matrixCS.Clone();
                var Bbf = new RobotFrame(pos[i].name).getMatrix(robotType);

                //prin.t(pos[i].name);
               // prin.t(Bbf);
                //var Bbm = new RobotFrame("510.9 6.4 55.4 1.5 -0.002 -0.1").getMatrix(robotType);

                var Bbm = new RobotFrame("-342.302 -151.944 216.247 3.130 0.003 -3.129").getMatrix(robotType);
                /*prin.t("Bsm");
                prin.t(Bsm);
                prin.t("Bbf");
                prin.t(Bbf);
                prin.t("Bbm");
                prin.t(Bbm);*/
                var Bsm_1 = Bsm.Clone();
                var Bbf_1 = Bbf.Clone();
                var Bbm_1 = Bbm.Clone();
                CvInvoke.Invert(Bsm,Bsm_1,DecompMethod.LU);
                CvInvoke.Invert(Bbf, Bbf_1, DecompMethod.LU);
                CvInvoke.Invert(Bbm, Bbm_1, DecompMethod.LU);
                var Bfs = Bbf_1 * Bbm_1 * Bsm;
                Bfs_l.Add(Bfs);
                Console.WriteLine(Bfs[0, 3] + " " + Bfs[1, 3] + " " + Bfs[2, 3] + " " + Bfs[0, 0] + " " + Bfs[0, 1] + " " + Bfs[0, 2]);
                /*
                //prin.t(Bfs);
                using (StreamWriter sw = new StreamWriter(file_name, false, Encoding.UTF8))
                {
                    sw.WriteLine(Settings_loader.matrix_save(Bfs));
                }
                Console.WriteLine(Bfs[0, 3] + " " + Bfs[1, 3] + " " + Bfs[2, 3] + " " + Bfs[0, 0] + " " + Bfs[0, 1] + " " + Bfs[0, 2]);
                //prin.t("--------------------------------");
                */
            }
            var Bfs_med = new Matrix<double>(new double[4,4]);
            for(int i=0; i<Bfs_l.Count;i++)
            {
                Bfs_med+=Bfs_l[i];
            }
            Bfs_med /= Bfs_l.Count;
            Settings_loader.save_file("bfs_cal.txt", new object[] { Bfs_med });
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
