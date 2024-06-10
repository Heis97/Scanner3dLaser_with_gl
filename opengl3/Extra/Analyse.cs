using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    static public class Analyse
    {

        public static double[] norm_align_data(double[] data)
        {
            var f = data.First();
            var l = data.Last();
            var data_c =(double[]) data.Clone();
            var df = (l - f) / f;
            for(int i=0; i<data.Length;i++)
            {
                //var a = data[i]/f - df*((double)i/data.Length);
                var a =( data[i] - f) / (l-f) - (double)i/data.Length;
                data_c[i] = a;
            }
            return data_c;
        }

        public static void norm_align_2_data(double[] data1, double[] data2)
        {
            var f = data1.First();
            var l = data1.Last();
            var data_c = (double[])data1.Clone();
            var df = (l - f) / f;
            for (int i = 0; i < data1.Length; i++)
            {
                //var a = data[i]/f - df*((double)i/data.Length);
                var a1 = (data1[i] - f) / (l - f) - (double)i / data1.Length;
                var a2 = (data2[i] - f) / (l - f) - (double)i / data1.Length;
                Console.WriteLine(a1 + " " + a2);
            }
        }

        public static double[][] norm_align_data(double[][] data)
        {

            var data_split = new double[data[0].Length][];
            for(int k=0;k< data_split.Length;k++)
            {
                data_split[k] = new double[data.Length];
                for (int i = 0; i < data_split[k].Length; i++)
                {
                    data_split[k][i] = data[i][k];
                }
                
            }
            norm_align_2_data(data_split[1], data_split[5]);    

            for(int i = 0; i < data_split.Length;i++)
            {
                data_split[i] = norm_align_data(data_split[i]);
            }

            /*for (int i = 0; i < data_split[0].Length; i++)
            {
                for  (int k = 0; k < data_split.Length; k++)
                {
                    Console.Write(data_split[k][i]+" ");
                }
                Console.WriteLine();
            }*/


            return data_split;
        }
        static public double[][] parse_data_txt(string file)
        {
            var data = new List<double[]>();
            var lines = File.ReadAllText(file).Split('\n');
            foreach(var line in lines)
            {

                var s = line.Trim().Replace("  "," ").Replace("  ", " ").Split(' ');
                var data_s = new double[s.Length];
                for(int i=0; i<s.Length;i++)
                {
                    data_s[i] = Convert.ToDouble(s[i]);
                }
                data.Add(data_s);
            }

            return data.ToArray();
        }

       /* static public double[,] remap_data(double[][]  data)
        {
            var data_r = new double[data.l]
        }*/

        static void analys_sph()
        {

            var ps1 = new PointF[]
            {
                new PointF(16,8),
                new PointF(6,34),
                new PointF(20,30),
                new PointF(56,1),
                new PointF(43,12),

                new PointF(45,17),
                new PointF(42,23),
                new PointF(30,36),
                new PointF(15,45),
                new PointF(17,51),

                new PointF(16,76),
                new PointF(28,85),
                new PointF(6,106),
                new PointF(63,37),
                new PointF(62,45),

                new PointF(84,10),
                new PointF(94,5),
                new PointF(109,9),
                new PointF(123,13),
                new PointF(116,21),

                new PointF(140,33),
                new PointF(161,24),
                new PointF(157,21),
                new PointF(187,24),
                new PointF(215,31),

                new PointF(171,59),
                new PointF(152,71),
                new PointF(95,59),
                new PointF(114,60),
                new PointF(123,74),

                new PointF(73,81),
                new PointF(87,89),
                new PointF(155,79),
                new PointF(40,112),
                new PointF(74,108),

                new PointF(158,93),
                new PointF(177,64),
                new PointF(212,53),
                new PointF(225,60),
                new PointF(221,79),

                new PointF(210,78),
                new PointF(192,85),
                new PointF(193,88),
                new PointF(212,102),
                new PointF(218,102),

                new PointF(202,115),
                new PointF(212,92)
            };

            var ps2 = new PointF[]
            {
                new PointF(69,35),
                new PointF(113,36),
                new PointF(154,34),
                new PointF(200,35),
                new PointF(244,34),

                new PointF(68,79),
                new PointF(114,79),
                new PointF(156,79),
                new PointF(202,77),
                new PointF(246,77),

                new PointF(66,124),
                new PointF(111,124),
                new PointF(157,123),
                new PointF(199,122),
                new PointF(247,120),

                new PointF(68,167),
                new PointF(114,168),
                new PointF(154,168),
                new PointF(200,166),
                new PointF(245,163),

                new PointF(70,214),
                new PointF(112,212),
                new PointF(159,210),
                new PointF(203,214),
                new PointF(246,209)
            };



            var vals1 = analyse_points_2(ps1, 3);
            var vals2 = analyse_points_2(ps2, 3);

            var max_v = Math.Max(vals1.Max(), vals2.Max());

            var gist1 = create_gist(vals1, 20, max_v);
            var gist2 = create_gist(vals2, 20, max_v);



            print_gists(gist1, gist2);
        }

        static int comp_centr_mass(int[] arr)
        {
            var m = 0;
            var mr = 0;
            for (var i = 0; i < arr.Length; i++)
            {
                m += arr[i];
                mr += arr[i] * i;
            }
            return mr / m;
        }

        static int[] create_gist(double[] vals, int N, double max_v)//n - number of sections
        {
            var gist = new int[N + 1];
            var k = max_v / N;
            Console.WriteLine("k: " + k);
            for (int i = 0; i < vals.Length; i++)
            {
                gist[(int)((vals[i]) / k)]++;
            }
            return gist;
        }

        static void print_gists(int[] gist1, int[] gist2)
        {
            var len1 = gist1.Length;
            var len2 = gist2.Length;
            if (len1 != len2)
            {
                if (len1 > len2)
                {
                    var d = len1 - len2;
                    var d_arr = new int[d];
                    var len2_l = new List<int>(len2);
                    len2_l.AddRange(d_arr);
                    gist2 = len2_l.ToArray();
                }
                else
                {
                    var d = len2 - len1;
                    var d_arr = new int[d];
                    var len1_l = new List<int>(len1);
                    len1_l.AddRange(d_arr);
                    gist1 = len1_l.ToArray();
                }
            }

            for (int i = 0; i < gist1.Length; i++)
            {
                Console.WriteLine(i + " " + gist1[i] + " " + gist2[i]);
            }

        }


        static double[] analyse_points(PointF[] ps)//aver dist of every two points
        {

            var vals = new List<double>();
            for (int i = 0; i < ps.Length; i++)
            {
                for (int j = i; j < ps.Length; j++)
                {
                    if (i != j)
                    {
                        var dist = (ps[i] - ps[j]).norm;
                        vals.Add((double)dist);
                    }
                }
            }
            return vals.ToArray();
        }

        static double[] analyse_points_2(PointF[] ps, int n)//aver dist for n nearest points
        {
            var vals = new List<double>();

            for (int i = 0; i < ps.Length; i++)
            {
                var vals_1 = new List<double>();
                for (int j = 0; j < ps.Length; j++)
                {
                    if (i != j)
                    {
                        var dist = (ps[i] - ps[j]).norm;
                        vals_1.Add((double)dist);
                    }
                }
                vals_1.Sort();
                vals.AddRange(vals_1.GetRange(0, n));
            }
            return vals.ToArray();
        }


        public static void comp_defor(Mat nodef, Mat def)
        {
            var def_c = def - nodef;            
            // def_c = FindCircles.sobel(def_c.ToImage<Gray, byte>()).Mat;
            def_c = new Mat(def_c.Clone(),new Rectangle(200,0,500,def_c.Height));
            var orig = def_c.Clone();
            Mat hier = new Mat();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            CvInvoke.Threshold(def_c.ToImage<Gray, byte>().Mat, def_c, 50, 255, ThresholdType.Binary);
            CvInvoke.FindContours(def_c, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);

            var area = comp_defor(ref contours);
            CvInvoke.DrawContours(orig, contours, -1, new MCvScalar(0, 255, 0), 2, LineType.EightConnected);
            CvInvoke.PutText(orig, "S: "+area.ToString(),new Point(10,70),FontFace.HersheyScriptSimplex,1,new MCvScalar(0, 255, 0));
            CvInvoke.Imshow("Area comp", orig);
        }
        public static double comp_defor(ref VectorOfVectorOfPoint contours)
        {
            int i_max = 0;
            double max_cont = 0;
            for (int i = 0; i < contours.Size; i++)
            {
                var area_cur = CvInvoke.ContourArea(contours[i]);
                if(area_cur > max_cont)
                {
                    i_max = i;
                    max_cont = area_cur;
                }               
            }
            var ret = new VectorOfVectorOfPoint();
            ret.Push(contours[i_max]);
            contours = ret;
            return max_cont;
        }
        private static List<string> GetFilesList(string path)
        {
            List<string> filesList = new List<string>();
            string[] dirs = Directory.GetDirectories(path);
            filesList.AddRange(Directory.GetFiles(path));
            foreach (string subdirectory in dirs)
            {
                try
                {
                    filesList.AddRange(GetFilesList(subdirectory));
                }
                catch { }
            }
            return filesList;
        }
        static void chang_ph()
        {
            var files = GetFilesList(@"C:\Users\1\Desktop\Polina");
            int ind = 0;
            foreach (var f in files)
            {
               change_to_green(f);

                ind++;
                Console.WriteLine(ind + " " + files.Count);

            }
            Console.WriteLine(files.Count);

        }

        static void change_to_green(string f)
        {

            try
            {
                Console.WriteLine(f);
                var mat1 = CvInvoke.Imread(f);
                Console.WriteLine(mat1.NumberOfChannels);
                Console.WriteLine(mat1.Depth);
                var im = mat1.ToImage<Gray, Byte>();
                var im_gr = new Image<Bgr, Byte>(im.Width, im.Height);
                for (int i = 0; i < im.Width; i++)
                {
                    for (int j = 0; j < im.Height; j++)
                    {
                        im_gr.Data[j, i, 1] = im.Data[j, i, 0];
                        im_gr.Data[j, i, 0] = 0;
                        im_gr.Data[j, i, 2] = 0;
                    }
                }
                //CvInvoke.Imshow("fa",im_gr);
                // f = Path.GetFileName(f);
                GC.Collect();
                im_gr.Save(f);

            }
            catch
            {
                Console.WriteLine("not save: " + f);
            }

        }
        static public void paint3dpointsRGB(List<Point3d_GL[]> inp_points)
        {
            //addPointMesh(inp_points[0], 1.0f,0.0f, 0.0f);
            //addPointMesh(inp_points[1], 0.0f,1.0f, 0.0f);
            //addPointMesh(inp_points[2], 0.0f, 0.0f, 1.0f);
            //GL1.addPointMesh(inp_points[3], 0.9f);
        }
        static public double[] calc_preFov(List<Frame> fr)
        {
            var points1 = FindMark.finPointFsFromIm_kalib(fr[0].im, 60, null);
            var points2 = FindMark.finPointFsFromIm_kalib(fr[1].im, 60, null);
            Console.WriteLine(fr.Count);
            Console.WriteLine(points1[0].Length);
            Console.WriteLine(points2[0].Length);
            double wmm1 = Math.Sqrt(2) * (6 * fr[0].size_mark / 7);
            double wpix1 = points1[0][5].X - points1[0][0].X;


            double wmm2 = Math.Sqrt(2) * (4 * fr[0].size_mark / 7);
            double wpix2 = points1[0][4].X - points1[0][1].X;


            double wmm3 = Math.Sqrt(2) * (6 * fr[1].size_mark / 7);
            double wpix3 = points2[0][5].X - points2[0][0].X;
            double delta = fr[1].pos_rob.z - fr[0].pos_rob.z;




            Console.WriteLine("1 = " + wmm1 + " " + wpix1);
            Console.WriteLine("2 = " + wmm2 + " " + wpix2);
            Console.WriteLine("3 = " + wmm3 + " " + wpix3);
            Console.WriteLine("d = " + delta);
            return new double[] { wmm1, wpix1, wmm2, wpix2, wmm3, wpix3, delta };
            //ka,f,h
            //findOneVarDec_new(new double[] { 0.6, wpix1/3, wmm1/3 }, new double[] { 3, 2000, 200 }, consts, calc_F_simple);
        }
        static public double[] calc_preFov_all(List<Frame> fr)
        {
            var points1 = FindMark.finPointFsFromIm_kalib(fr[0].im, 60, null);

            int len = fr.Count + 1;
            var consts = new double[(3 * len) - 2];
            double[] w_mm = new double[len];
            double[] w_pix = new double[len];
            double[] delta = new double[len];

            w_mm[0] = Math.Sqrt(2) * (6 * fr[0].size_mark / 7);
            w_pix[0] = points1[0][5].X - points1[0][0].X;


            w_mm[1] = Math.Sqrt(2) * (4 * fr[0].size_mark / 7);
            w_pix[1] = points1[0][4].X - points1[0][1].X;

            int j = 0;
            consts[j] = w_mm[0]; j++;
            consts[j] = w_pix[0]; j++;
            consts[j] = w_mm[1]; j++;
            consts[j] = w_pix[1]; j++;
            for (int i = 2; i < fr.Count + 1; i++)
            {
                var points2 = FindMark.finPointFsFromIm_kalib(fr[i - 1].im, 60, null);
                w_mm[i] = Math.Sqrt(2) * (6 * fr[i - 1].size_mark / 7);
                consts[j] = w_mm[i]; j++;
                w_pix[i] = points2[0][5].X - points2[0][0].X;
                consts[j] = w_pix[i]; j++;
                delta[i] = fr[i - 1].pos_rob.z - fr[0].pos_rob.z;
                consts[j] = delta[i]; j++;
            }
            Console.WriteLine("1 = " + w_mm[0] + " " + w_pix[0]);
            Console.WriteLine("2 = " + w_mm[1] + " " + w_pix[1]);
            Console.WriteLine("3 = " + w_mm[2] + " " + w_pix[2]);
            Console.WriteLine("d = " + delta);
            return consts;

            //ka,f,h
            //findOneVarDec_new(new double[] { 0.6, wpix1/3, wmm1/3 }, new double[] { 3, 2000, 200 }, consts, calc_F_simple);
        }
        static public List<Point3d_GL[]> map_fov_3d(List<Frame> frs)
        {
            List<Point3d_GL> ps1 = new List<Point3d_GL>();
            List<Point3d_GL> ps2 = new List<Point3d_GL>();
            List<Point3d_GL> ps3 = new List<Point3d_GL>();
            List<Point3d_GL> ps4 = new List<Point3d_GL>();
            var consts = calc_preFov_all(frs);
            var mins = new double[] { 0.5, 400, 20 };
            var maxs = new double[] { 2, 800, 90 };
            double dim = 100;
            double delt_k = (maxs[0] - mins[0]) / dim;
            double delt_f = (maxs[1] - mins[1]) / dim;
            double delt_h = (maxs[2] - mins[2]) / dim;
            var max_k = -10000;
            var min_k = 10000;
            var max_f = -10000;
            var min_f = 10000;
            var max_h = -10000;
            var min_h = 10000;
            double eps = 0.08;

            double cur_k = mins[0];

            for (int i1 = 0; i1 < dim; i1++)
            {
                double cur_f = mins[1];
                for (int i2 = 0; i2 < dim; i2++)
                {
                    double cur_h = mins[2];
                    for (int i3 = 0; i3 < dim; i3++)
                    {

                        var ret = calc_F_all_ret(new double[] { cur_k, cur_f, cur_h }, consts);
                        if (ret[0] < eps)
                        {
                            ps1.Add(new Point3d_GL(i1, i2, i3));
                        }
                        if (ret[1] < eps)
                        {
                            ps2.Add(new Point3d_GL(i1, i2, i3));

                        }
                        if (ret[2] < eps)
                        {
                            ps3.Add(new Point3d_GL(i1, i2, i3));
                        }
                        double sum = 0;
                        foreach (var r in ret)
                        {
                            sum += r;
                        }
                        ps4.Add(new Point3d_GL(100 * sum, i2, i3));
                        if (sum < eps)
                        {
                            //ps4.Add(new Point3d_GL(i1, i2, i3));
                            // Console.WriteLine(" k= " + cur_k + " f = " + cur_f + " h = " + cur_h + " ");

                        }
                        cur_h += delt_h;
                    }
                    cur_f += delt_f;
                }
                cur_k += delt_k;
                cur_k = 1.0;
                Console.WriteLine("calc = " + i1 + " from " + dim);
            }
            List<Point3d_GL[]> all_p = new List<Point3d_GL[]>();
            all_p.Add(ps1.ToArray());
            all_p.Add(ps2.ToArray());
            all_p.Add(ps3.ToArray());
            all_p.Add(ps4.ToArray());
            return all_p;
        }
        static public double errPos(PointF[] points, Size size, double fov, double side)
        {
            var Camera1 = new Camera(fov, size);
            var points1 = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points1[i] = new PointF(points[i].X - size.Width / 2,
                                        points[i].Y - size.Height / 2);
                points1[i].Y *= -1;
            }
            var cl_P1 = new Point3d_GL(0, 0, 0);
            var cl_P2 = new Point3d_GL(side, 0, 0);
            var cl_P3 = new Point3d_GL(side, side, 0);
            var cl_P4 = new Point3d_GL(0, side, 0);
            Camera1.calc_pos_all(points1[0], points1[1], points1[2], points1[3], cl_P1, cl_P2, cl_P3, cl_P4);
            Console.WriteLine("poS_ " + Camera1.pos.x + " " + Camera1.pos.y + " " + Camera1.pos.z + " " + Camera1.pos.magnitude());
            //addGLMesh(cube_buf, PrimitiveType.Triangles, (float)Camera1.pos.x, (float)Camera1.pos.y, (float)Camera1.pos.z);
            //addFrame(Camera1.pos, Camera1.pos + Camera1.oX * 15, Camera1.pos + Camera1.oY * 15, Camera1.pos + Camera1.oZ * 20);
            return Camera1.err_pos;
        }
        static public double calcPixForCam(List<Frame> frames, double fov, double side)
        {
            var pos_cam = new List<Point3d_GL>();
            double delta = 0;
            //Console.WriteLine(fov + " " + side);
            for (int i = 0; i < frames.Count; i++)
            {
                pos_cam.Add(UtilMatr.calcPos(frames[i].points, frames[i].im.Size, fov, side).pos);
            }
            int alld = 0;
            for (int i = 0; i < pos_cam.Count; i++)
            {
                for (int ie = i + 1; ie < pos_cam.Count; ie++)
                {
                    var d1 = pos_cam[i] - pos_cam[ie];
                    var err1 = d1.magnitude();

                    var d2 = frames[i].pos_rob - frames[ie].pos_rob;
                    var err2 = d2.magnitude();

                    delta += Math.Abs(err1 - err2);
                    alld++;
                }
            }
            return delta;
        }
        static public double findOneVarDih(double minVal, double maxVal, List<Frame> fr, double side,
                                Func<List<Frame>, double, double, double> func)
        {
            //ret of func - error
            double epsilon = 0.01;
            var a = minVal;
            var b = maxVal;
            var c = 0.0;
            int i = 0;

            while (b - a > epsilon && i < 1000)
            {
                i++;
                c = (a + b) / 2;
                if (Math.Abs(func(fr, a, side)) > Math.Abs(func(fr, b, side)))
                    a = c;
                else
                    b = c;
                //Console.WriteLine(" i = " + i + "fov = " +c+" er = "+ Math.Abs(func(fr, c, side)));
            };
            return c;
        }
        static public double findOneVarDec(double minVal, double maxVal, List<Frame> fr, double side,
                                Func<List<Frame>, double, double, double> func)
        {
            //ret of func - error
            const int dec = 6;
            double epsilon = 0.01;
            var a = minVal;
            var b = maxVal;
            double[] c = new double[dec];

            double[] ret = new double[dec];
            int ind = 0;

            while (b - a > epsilon && ind < 100)
            {
                //Console.WriteLine((a + b) / 2);
                ind++;
                double min_ret = 100000000;
                int min_ind = 0;
                for (int i = 0; i < dec; i++)
                {

                    c[i] = a + i * (b - a) / dec;
                    ret[i] = Math.Abs(func(fr, c[i], side));
                    //Console.WriteLine(" i = " + ind + "fov = " + c[i] + " er = " + ret[i]);
                    if (ret[i] < min_ret)
                    {
                        min_ind = i;
                        min_ret = ret[i];
                    }
                }
                if (min_ind == 0)
                {
                    a = c[0];
                    b = c[1];
                }
                else if (min_ind == dec - 1)
                {
                    a = c[dec - 2];
                    b = c[dec - 1];
                }
                else
                {
                    a = c[min_ind - 1];
                    b = c[min_ind + 1];
                }
                var c1 = (a + b) / 2;
                //  Console.WriteLine(" i = " + ind + "fov = " +c1+" er = "+ Math.Abs(func(fr, c1, side)));
            };
            return (a + b) / 2;
        }

        /* double findOneVarDec_fov(double[] minVal, double[] maxVal, double[] consts,
                                Func<double[], double[], double> func)
         {
             //ret of func - error
             const int dec = 6;
             double epsilon = 0.01;
             int len = minVal.Length;
             var a = minVal;
             var b = maxVal;
             double[] c = new double[dec];

             double[] ret = new double[dec];
             int ind = 0;
             double min_ret = 100000000;

             while (min_ret > epsilon && ind < 100)
             {
                 min_ret = 100000000;
                 ind++;

                 int min_ind = 0;
                 for (int i = 0; i < dec; i++)
                 {

                     c[i] = a + i * (b - a) / dec;
                     ret[i] = func(c, consts);
                     if (ret[i] < min_ret)
                     {
                         min_ind = i;
                         min_ret = ret[i];
                     }
                 }
                 if (min_ind == 0)
                 {
                     a = c[0];
                     b = c[1];
                 }
                 else if (min_ind == dec - 1)
                 {
                     a = c[dec - 2];
                     b = c[dec - 1];
                 }
                 else
                 {
                     a = c[min_ind - 1];
                     b = c[min_ind + 1];
                 }
                 //Console.WriteLine(" i = " + ind + "fov = " +c+" er = "+ Math.Abs(func(fr, c, side)));
             };
             return (a + b) / 2;
         }*/
        static public double[] findOneVarDec_new(double[] minVal, double[] maxVal, double[] consts, Func<double[], double[], double> func)
        {
            //ret of func - error
            const int dec = 6;
            double epsilon = 0.01;
            int len = minVal.Length;
            var a = new double[len];
            var b = new double[len];
            double[][] c = new double[len][];
            double ret = 1000000;

            for (int i = 0; i < len; i++)
            {
                c[i] = new double[dec];
                a[i] = minVal[i];
                b[i] = maxVal[i];
            }
            double err = 0;
            for (int i = 0; i < len; i++)
            {
                err += Math.Abs(a[i] - b[i]);
            }
            int ind = 0;
            double min_ret = 1000000;
            int[] min_ind = { 0, 0, 0 };
            while (err > epsilon && ind < 100)
            {
                for (int dim1 = 0; dim1 < dec; dim1++)
                {
                    for (int dim2 = 0; dim2 < dec; dim2++)
                    {
                        for (int dim3 = 0; dim3 < dec; dim3++)
                        {
                            for (int i = 0; i < dec; i++)
                            {

                                c[0][dim1] = a[0] + dim1 * (b[0] - a[0]) / dec;
                                c[1][dim2] = a[1] + dim2 * (b[1] - a[1]) / dec;
                                c[2][dim3] = a[2] + dim3 * (b[2] - a[2]) / dec;
                                var vars = new double[] { c[0][dim1], c[1][dim2], c[2][dim3] };
                                ret = Math.Abs(func(vars, consts));
                                if (ret < min_ret)
                                {
                                    min_ind[0] = dim1;
                                    min_ind[1] = dim2;
                                    min_ind[2] = dim3;
                                    min_ret = ret;
                                }
                            }


                        }
                    }
                }
                for (int i = 0; i < 3; i++)
                {
                    if (min_ind[i] == 0)
                    {
                        a[i] = c[i][0];
                        b[i] = c[i][1];
                    }
                    else if (min_ind[i] == dec - 1)
                    {
                        a[i] = c[i][dec - 2];
                        b[i] = c[i][dec - 1];
                    }
                    else
                    {
                        a[i] = c[i][min_ind[i] - 1];
                        b[i] = c[i][min_ind[i] + 1];
                    }
                }
                for (int i = 0; i < len; i++)
                {
                    err += Math.Abs(a[i] - b[i]);
                }
                ind++;
            }

            return null;
        }
        static private double[] calc_F(double[] vars, double[] consts)
        {
            double k_alpha = vars[0];
            double f = vars[1];
            double h = vars[2];
            double w_mm1 = consts[0];
            double w_pix1 = consts[1];
            double w_mm2 = consts[2];
            double w_pix2 = consts[3];
            double w_mm3 = consts[4];
            double w_pix3 = consts[5];
            double delta = consts[6];
            double[] ret = new double[3];
            ret[0] = Math.Atan(w_mm1 / (2 * h)) - k_alpha * Math.Atan(w_pix1 / (2 * f));
            ret[1] = Math.Atan(w_mm2 / (2 * h)) - k_alpha * Math.Atan(w_pix2 / (2 * f));
            ret[2] = Math.Atan(w_mm3 / (2 * (h + delta))) - k_alpha * Math.Atan(w_pix3 / (2 * f));
            return ret;
        }
        static private double calc_F_simple(double[] vars, double[] consts)
        {
            double k_alpha = vars[0];
            double f = vars[1];
            double h = vars[2];
            double w_mm1 = consts[0];
            double w_pix1 = consts[1];
            double w_mm2 = consts[2];
            double w_pix2 = consts[3];
            double w_mm3 = consts[4];
            double w_pix3 = consts[5];
            double delta = consts[6];
            double[] ret = new double[3];
            ret[0] = Math.Atan(w_mm1 / (2 * h)) - k_alpha * Math.Atan(w_pix1 / (2 * f));
            ret[1] = Math.Atan(w_mm2 / (2 * h)) - k_alpha * Math.Atan(w_pix2 / (2 * f));
            ret[2] = Math.Atan(w_mm3 / (2 * (h + delta))) - k_alpha * Math.Atan(w_pix3 / (2 * f));
            var ret1 = ret[0] * ret[0] + ret[1] * ret[1] + ret[2] * ret[2];

            Console.WriteLine("ret = " + ret1 + " k_alpha = " + k_alpha + " f = " + f + " h = " + h + " ");
            return ret1;
        }
        //k_alpha = 1.66666666666667 f = 796.979717565466 h = 24.2177821260455 
        static private double[] calc_F_ret(double[] vars, double[] consts)
        {
            double k_alpha = vars[0];
            double f = vars[1];
            double h = vars[2];
            double w_mm1 = consts[0];
            double w_pix1 = consts[1];
            double w_mm2 = consts[2];
            double w_pix2 = consts[3];
            double w_mm3 = consts[4];
            double w_pix3 = consts[5];
            double delta = consts[6];
            double[] ret = new double[3];
            ret[0] = Math.Atan(w_mm1 / (2 * h)) - k_alpha * Math.Atan(w_pix1 / (2 * f));
            ret[1] = Math.Atan(w_mm2 / (2 * h)) - k_alpha * Math.Atan(w_pix2 / (2 * f));
            ret[2] = Math.Atan(w_mm3 / (2 * (h + delta))) - k_alpha * Math.Atan(w_pix3 / (2 * f));
            for (int i = 0; i < 3; i++)
            {
                ret[i] = Math.Abs(ret[i]);
            }
            //var ret1 = ret[0] * ret[0] + ret[1] * ret[1] + ret[2] * ret[2];
            //Console.WriteLine("ret = " + ret1 + " k_alpha = " + k_alpha + " f = " + f + " h = " + h + " ");
            return ret;
        }
        static double[] calc_F_all_ret(double[] vars, double[] consts)
        {
            double k_alpha = vars[0];
            double f = vars[1];
            double h = vars[2];
            int len = (consts.Length - 4) / 3 + 2;
            double[] w_mm = new double[len];
            double[] w_pix = new double[len];
            double[] delta = new double[len];
            w_mm[0] = consts[0];
            w_pix[0] = consts[1];
            w_mm[1] = consts[2];
            w_pix[1] = consts[3];
            int j = 4;
            for (int i = 2; i < len; i++)
            {
                w_mm[i] = consts[j]; j++;
                w_pix[i] = consts[j]; j++;
                delta[i - 2] = consts[j]; j++;
            }

            double[] ret = new double[len];
            ret[0] = Math.Atan(w_mm[0] / (2 * h)) - k_alpha * Math.Atan(w_pix[0] / (2 * f));
            ret[1] = Math.Atan(w_mm[1] / (2 * h)) - k_alpha * Math.Atan(w_pix[2] / (2 * f));

            for (int i = 2; i < len; i++)
            {
                ret[i] = Math.Atan(w_mm[i] / (2 * (h + delta[i - 2]))) - k_alpha * Math.Atan(w_pix[i] / (2 * f));
            }
            for (int i = 0; i < len; i++)
            {
                ret[i] = Math.Abs(ret[i]);
            }
            //var ret1 = ret[0] * ret[0] + ret[1] * ret[1] + ret[2] * ret[2];
            //Console.WriteLine("ret = " + ret1 + " k_alpha = " + k_alpha + " f = " + f + " h = " + h + " ");
            return ret;
        }
        static public Image<Gray, Byte> mapSolv(List<Frame> frames, double start_fov, double start_side, int dim, double delta)
        {
            var ret = new Image<Gray, Byte>(dim, dim);
            var fov = (1 - delta) * start_fov;
            var side = (1 - delta) * start_side;
            var fov_delta = 2 * delta * start_fov / ret.Width;
            var side_delta = 2 * delta * start_side / ret.Width;

            for (int x = 0; x < ret.Width; x++)
            {
                fov = (1 - delta) * start_fov;
                Console.WriteLine(x);
                for (int y = 0; y < ret.Height; y++)
                {
                    int color = (int)calcPixForCam(frames, fov, side);
                    if (color > 255)
                    {
                        color = 255;
                    }
                    ret.Data[y, x, 0] = (byte)color;
                    Console.WriteLine(fov + " " + side + " " + ret.Data[y, x, 0] + " ");
                    if ((int)ret.Data[y, x, 0] < 10)
                    {
                        Console.WriteLine(fov + " " + side + " " + ret.Data[y, x, 0] + " ");
                    }

                    fov += fov_delta;

                }
                side += side_delta;
            }

            return ret;
        }
        static public void fov3dMap(List<Frame> frames, double start_fov, double side, int dim, double delta, int n, int step = 1)
        {
            var dbs = new List<double[]>();
            for (int i = 0; i < frames.Count - n; i += step)
            {
                var frs = frames.GetRange(i, n);
                dbs.Add(lineSolv_doub(frs, start_fov, side, dim, delta));
            }
            var im = mapSolv3D(dbs);
            //GL1.addGLMesh(meshFromImage(im), PrimitiveType.Triangles);
        }
        static public Image<Gray, Byte> mapSolv3D(List<double[]> frames)
        {
            var im_res = new Image<Gray, Byte>(frames.Count, frames[0].Length);
            var min_f = new double[frames.Count];
            var max_f = new double[frames.Count];
            for (int i = 0; i < frames.Count; i++)
            {
                max_f[i] = frames[i].Max();
                min_f[i] = frames[i].Min();
            }
            double min_val = min_f.Min();
            double max_val = max_f.Max();

            double range = max_val - min_val;
            double off = min_val;

            double scalez = 255 / range;
            //var frs = frames.ToArray();
            Console.WriteLine(off);
            Console.WriteLine(range);
            Console.WriteLine(max_val);
            Console.WriteLine(min_val);
            for (int x = 0; x < im_res.Width - 1; x++)
            {
                for (int y = 0; y < im_res.Height - 1; y++)
                {
                    //Console.WriteLine(frames[x][y]);
                    var z = (-off + frames[x][y]) * scalez;

                    if (z > 255)
                    {
                        z = 255;
                    }
                    im_res.Data[y, x, 0] = (byte)z;
                }
            }
            return im_res;
        }
        static public double[] lineSolv_doub(List<Frame> frames, double start_fov, double side, int dim, double delta)
        {
            var ret = new double[dim];
            var fov = (1 - delta) * start_fov;
            var fov_delta = 2 * delta * start_fov / ret.Length;
            for (int x = 0; x < ret.Length; x++)
            {
                ret[x] = calcPixForCam(frames, fov, side);
                fov += fov_delta;
            }
            return ret;
        }
        static public Image<Gray, Byte> lineSolv(List<Frame> frames, double start_fov, double side, int dim, double delta)
        {
            var ret = new Image<Gray, Byte>(dim, 300);
            var fov = (1 - delta) * start_fov;
            var fov_delta = 2 * delta * start_fov / ret.Width;

            int y = 0;
            for (int x = 0; x < ret.Width; x++)
            {

                y = (int)calcPixForCam(frames, fov, side);
                if (y > ret.Height)
                {
                    y = ret.Height - 1;
                }
                ret.Data[y, x, 0] = 255;
                Console.WriteLine(fov + " " + y + " ");
                fov += fov_delta;
            }
            return ret;
        }
        static public Image<Gray, Byte> lineErr(List<Frame> frames, double fov, double side)
        {
            var ret = new Image<Gray, Byte>(frames.Count, 300);


            double y = 0;
            for (int x = 0; x < ret.Width; x++)
            {

                y = 50 * errPos(frames[x].points, frames[x].im.Size, fov, side);
                if (y > ret.Height)
                {
                    y = ret.Height - 1;
                }
                ret.Data[(int)y, x, 0] = 255;
                Console.WriteLine(y);
            }
            return ret;
        }
        static public double findFov(List<Frame> frames, double start_fov, double side, int dim, double delta)
        {
            double n = 200;
            var fov = (1 - delta) * start_fov;
            var fov_delta = 2 * delta * start_fov / n;
            int y = 0;
            for (int x = 0; x < n; x++)
            {

                y = (int)calcPixForCam(frames, fov, side);
                fov += fov_delta;
            }

            return n;
        }



    }

}
