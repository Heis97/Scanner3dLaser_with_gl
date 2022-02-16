using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    
    static public class ContourAnalyse
    {
        static public Mat findLaserArea(Mat im, ImageBox box, int bin)
        {

            List<double[]> strk = new List<double[]>();
            double[,] stroka = new double[im.Width, 2];
            Image<Gray, Byte> im_gray = im.ToImage<Gray, Byte>();
            Image<Bgr, Byte> im_rgb = im.ToImage<Bgr, Byte>();
            // im_rgb.ThresholdTrunc(new Bgr(10, 10, 250));
            var im_tr = im_rgb.ThresholdBinary(new Bgr(253, 253, 6), new Bgr(255, 255, 255));
            var im_tr_gr = (im_tr.Mat).ToImage<Gray, Byte>();


            CvInvoke.Threshold(im_gray, im_gray, bin, 255, ThresholdType.Binary);
            CvInvoke.Threshold(im_tr_gr, im_tr_gr, 253, 255, ThresholdType.Binary);

            // CvInvoke.Threshold(im, im, bin, 255, ThresholdType.);
            //var im_res = new Image<Gray, Byte>(im.Width, im.Height);
            Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(1, 1));
            Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            Mat kernel2 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(2, 2), new Point(1, 1));
            Mat ellips7 = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(7, 7), new Point(1, 1));
            var im_res = im_gray.MorphologyEx(MorphOp.Gradient, kernel5, new Point(-1, -1), 6, BorderType.Default, new MCvScalar());

            //imageBox3.Image = im_gray.MorphologyEx(MorphOp.Close, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //imageBox4.Image = im_gray.MorphologyEx(MorphOp.Dilate, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //imageBox5.Image = im_gray.MorphologyEx(MorphOp.Erode, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //imageBox6.Image = im_gray.MorphologyEx(MorphOp.HitMiss, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //imageBox7.Image = im_rgb.MorphologyEx(MorphOp.Open, kernel2, new Point(-1, -1), 1, BorderType.Default, new MCvScalar()).MorphologyEx(MorphOp.Dilate, kernel5, new Point(-1, -1), 3, BorderType.Default, new MCvScalar());
            // imageBox8.Image = im_gray.MorphologyEx(MorphOp.Tophat, kernel5, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            //imageBox8.Image = im_tr_gr;
            box.Image = im_res;
            return im_res.Mat;
        }
        static public double[][] findContour(Mat im, ImageBox box, int bin)
        {

            List<double[]> strk = new List<double[]>();
            double[,] stroka = new double[im.Width, 2];
            Image<Gray, Byte> im_gray = im.ToImage<Gray, Byte>();
            CvInvoke.Threshold(im_gray, im_gray, bin, 255, ThresholdType.Binary);
            var im_res = new Image<Gray, Byte>(im.Width, im.Height);

            for (int x = 0; x < im_gray.Width; x++)
            {
                int y = 0;
                int y_pos = 0;
                for (y = 0; y < im_gray.Height; y++)
                {
                    if (im_gray.Data[y, x, 0] > 245)
                    {
                        y_pos = y;
                    }
                }
                im_res.Data[y_pos, x, 0] = 255;
                stroka[x, 0] = x;
                stroka[x, 1] = y_pos;
                if (y_pos != 0)
                {
                    strk.Add(new double[] { x, y_pos });
                }

            }
            box.Image = im_res;
            return strk.ToArray();
        }
        static public double calcYbetween2Point(double x, PointF p1, PointF p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            var a = dy / dx;
            var b = p1.Y - a * p1.X;
            return a * x + b;
        }
        static public double[][] connectPoints(double[][] inp)
        {
            double[][] ret = new double[inp.Length][];
            List<int> indixes = new List<int>();
            for (int i = 0; i < inp.Length; i++)
            {
                if (inp[i][2] == 1)
                {
                    indixes.Add(i);
                }
            }
            var indes = indixes.ToArray();
            int next_p = 1;
            for (int i = 0; i < inp.Length; i++)
            {
                double y = 0;

                if (i < inp.Length - 1 && indes[next_p] <= i && next_p < indes.Length - 1)
                {
                    next_p++;
                }
                if (inp[i][2] == 0)
                {
                    var y1 = (float)inp[indes[next_p - 1]][1];
                    var y2 = (float)inp[indes[next_p]][1];
                    var y_prev = calcYbetween2Point(i, new PointF(indes[next_p - 1], y1), new PointF(indes[next_p], y2));
                    var mx = Math.Max(y1, y2);
                    var mn = Math.Min(y1, y2);
                    if (y_prev > mx)
                    {
                        y = mx;
                    }
                    else if (y_prev < mn)
                    {
                        y = mn;
                    }
                    else
                    {
                        y = y_prev;
                    }
                }
                else
                {
                    y = inp[i][1];
                }
                ret[i] = new double[] { i, y, (int)inp[i][2] };
            }
            return ret;
        }
        static public double[][] gauss2D(double[][] inp, double len)
        {
            double[][] ret = new double[inp.Length][];
            //List<double[]> strk = new List<double[]>();
            for (int i = 0; i < inp.Length; i++)
            {
                int real = 0;
                double sum = 0;
                for (int i_m = -(int)(len / 2); i_m < len / 2; i_m++)
                {
                    int ind = i + i_m;
                    if (ind >= inp.Length)
                    {
                        ind = inp.Length - 1;
                    }
                    if (ind < 0)
                    {
                        ind = 0;
                    }
                    sum += inp[ind][1];
                }
                if (inp[i].Length > 2)
                {
                    real = (int)inp[i][2];
                }
                ret[i] = new double[] { i, sum / len, real };
            }
            return ret;
        }
        static public double[][] findContourZ_real(Mat im, ImageBox box, int bin, DirectionType directionType = DirectionType.Down)
        {
            List<double[]> strk = new List<double[]>();
            Image<Bgr, Byte> im_gray = im.ToImage<Bgr, Byte>();
            CvInvoke.Threshold(im_gray, im_gray, bin, 255, ThresholdType.Binary);
            if (directionType == DirectionType.Down)
            {
                for (int x = 0; x < im_gray.Width; x++)
                {
                    int y = 0;
                    int y_pos = 0;
                    int real = 1;
                    for (y = 0; y < im_gray.Height; y++)
                    {
                        if (im_gray.Data[y, x, 0] > 245)
                        {
                            y_pos = y;
                        }
                    }
                    if (y_pos == 0)
                    {
                        y_pos = im_gray.Height - 1;
                        real = 0;
                    }
                    strk.Add(new double[] { x, y_pos, real });
                }
                //imageBox3.Image = im_gray;
                // imageBox6.Image = UtilMatr.doubleToMat(strk.ToArray(), im_gray.Size);
                var strk_1 = connectPoints(strk.ToArray());
                //imageBox4.Image = UtilMatr.doubleToMat(strk_1, im.Size);
                var gauss = gauss2D(strk_1, 8);
                //imageBox5.Image = UtilMatr.doubleToMat(gauss, im.Size);
                box.Image = UtilMatr.doubleToMat(gauss, im.Size);
                //Console.WriteLine("STRK");
                return strk.ToArray();
            }
            else
            {
                for (int x = 0; x < im_gray.Width; x++)
                {
                    int y = 0;
                    int y_pos = 0;
                    int real = 1;
                    for (y = im_gray.Height - 1; y >= 0; y--)
                    {
                        if (im_gray.Data[y, x, 0] > 245)
                        {
                            y_pos = y;
                        }
                    }
                    if (y_pos == 0)
                    {
                        y_pos = im_gray.Height - 1;
                        real = 0;
                    }
                    strk.Add(new double[] { x, y_pos, real });
                }
                //imageBox3.Image = im_gray;
                var strk_1 = connectPoints(strk.ToArray());
                // imageBox4.Image = UtilMatr.doubleToMat(strk_1, im.Size);
                var gauss = gauss2D(strk_1, 10);
                // imageBox5.Image = UtilMatr.doubleToMat(gauss, im.Size);
                //box.Image = (Mat)box.Image + doubleToMat(gauss, im.Size);
                return gauss;
            }



        }
        static public double[][] findContourZ(Mat im, ImageBox box, int bin, DirectionType directionType = DirectionType.Down)
        {
            List<double[]> strk = new List<double[]>();
            Image<Bgr, Byte> im_gray = im.ToImage<Bgr, Byte>();
            CvInvoke.Threshold(im_gray, im_gray, bin, 255, ThresholdType.Binary);
            if (directionType == DirectionType.Down)
            {
                for (int x = 0; x < im_gray.Width; x++)
                {
                    int y = 0;
                    int y_pos = 0;
                    int real = 1;
                    for (y = 0; y < im_gray.Height; y++)
                    {
                        if (im_gray.Data[y, x, 0] > 245)
                        {
                            y_pos = y;
                        }
                    }
                    if (y_pos == 0)
                    {
                        y_pos = im_gray.Height - 1;
                        real = 0;
                    }
                    strk.Add(new double[] { x, y_pos, real });
                }
                //imageBox3.Image = im_gray;
                // imageBox6.Image = UtilMatr.doubleToMat(strk.ToArray(), im_gray.Size);
                var strk_1 = connectPoints(strk.ToArray());
                // imageBox4.Image = UtilMatr.doubleToMat(strk_1, im.Size);
                var gauss = gauss2D(strk_1, 8);
                //imageBox5.Image = UtilMatr.doubleToMat(gauss, im.Size);
                //box.Image = UtilMatr.doubleToMat(gauss, im.Size);
                //Console.WriteLine("STRK");
                return gauss;
            }
            else
            {
                for (int x = 0; x < im_gray.Width; x++)
                {
                    int y = 0;
                    int y_pos = 0;
                    int real = 1;
                    for (y = im_gray.Height - 1; y >= 0; y--)
                    {
                        if (im_gray.Data[y, x, 0] > 245)
                        {
                            y_pos = y;
                        }
                    }
                    if (y_pos == 0)
                    {
                        y_pos = im_gray.Height - 1;
                        real = 0;
                    }
                    strk.Add(new double[] { x, y_pos, real });
                }
                // imageBox3.Image = im_gray;
                var strk_1 = connectPoints(strk.ToArray());
                // imageBox4.Image = UtilMatr.doubleToMat(strk_1, im.Size);
                var gauss = gauss2D(strk_1, 10);
                //imageBox5.Image = UtilMatr.doubleToMat(gauss, im.Size);
                //box.Image = (Mat)box.Image + doubleToMat(gauss, im.Size);
                return gauss;
            }



        }
        static public double[][] findContourZ_leg(Mat im, ImageBox box, int bin)
        {
            List<double[]> strk = new List<double[]>();
            Image<Bgr, Byte> im_gray = im.ToImage<Bgr, Byte>();
            CvInvoke.Threshold(im_gray, im_gray, bin, 255, ThresholdType.Binary);
            for (int x = 0; x < im_gray.Width; x++)
            {
                int y = 0;
                int y_pos = 0;
                int real = 1;
                for (y = im_gray.Height; y > 0; y--)
                {
                    if (im_gray.Data[y, x, 0] > 245)
                    {
                        y_pos = y;
                    }
                }
                if (y_pos == 0)
                {
                    y_pos = im_gray.Height - 1;
                    real = 0;
                }
                strk.Add(new double[] { x, y_pos, real });
            }
            //imageBox3.Image = im_gray;
            var strk_1 = connectPoints(strk.ToArray());
            // imageBox4.Image = UtilMatr.doubleToMat(strk_1, im.Size);
            var gauss = gauss2D(strk_1, 10);
            // imageBox6.Image = UtilMatr.doubleToMat(gauss, im.Size);
            box.Image = UtilMatr.doubleToMat(gauss, im.Size);
            return gauss;
        }
        static public double[,] findContourRGB(Mat im, ImageBox box)
        {
            int color = 2;//012 - bgr
            double[,] stroka = new double[im.Width, 2];
            Image<Bgr, Byte> im_bgr = im.ToImage<Bgr, Byte>();
            var im_res = new Image<Bgr, Byte>(im.Width, im.Height);
            for (int x = 0; x < im_bgr.Width; x++)
            {
                int y = 0;
                int y_pos = 0;
                for (y = 0; y < im_bgr.Height; y++)
                {
                    if (im_bgr.Data[y, x, color] > 250)
                    {
                        y_pos = y;
                    }
                }
                im_res.Data[y_pos, x, color] = 255;
                stroka[x, 0] = x;
                stroka[x, 1] = y_pos;
            }
            box.Image = im_res;
            return stroka;
        }


    }
    }

