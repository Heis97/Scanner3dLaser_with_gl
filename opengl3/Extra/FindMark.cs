using Accord.Math;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public struct PointC
    {
        public Point point;
        public int count;
        public PointC(int Count, Point Point)
        {
            point = Point;
            count = Count;
        }
    }
    public static class FindMark
    {
        static private List<Point> takePoints(Mat im, int res_min)
        {
            Image<Gray, Byte> im_gray = im.ToImage<Gray, Byte>();
            List<Point> points = new List<Point>();

            points.Add(downPoint(im_gray, res_min));
            points.Add(leftPoint(im_gray, res_min));
            points.Add(upPoint(im_gray, res_min));
            return points;
        }
        static public Point[] finPointFs1(Mat im, int bin, ImageBox box, int res_min)
        {
            return takePoints(revealContour(im, bin, box), res_min).ToArray();
        }
        static public Point[] finPointFs(List<Mat> ims, ImageBox box, int res_min)
        {
            var points = new Point[3];
            var countp = new int[3];
            int count1 = 0;
            var maxVal = PointF.calc_p_len(new Point(0, 0), new Point(ims[0].Width, ims[0].Height));
            var prev_points = new Point[3];
            var cur_points = new Point[3];
            prev_points[0] = new Point(0, 0);
            prev_points[1] = new Point(0, 0);
            prev_points[2] = new Point(0, 0);
            List<PointC>[] points_A = new List<PointC>[3];
            points_A[0] = new List<PointC>();
            points_A[1] = new List<PointC>();
            points_A[2] = new List<PointC>();
            double k = box.Width / 255;
            var im_res = new Image<Bgr, Byte>(ims[0].Width, ims[0].Width);
            var im_res1 = new Image<Bgr, Byte>(box.Width, box.Height);
            foreach (var im in ims)
            {
                for (int i = 0; i < 255; i += 100)
                {
                    cur_points = takePoints(revealContour(im, 137, box), res_min).ToArray();
                    for (int w = 0; w < 3; w++)
                    {
                        var p = cur_points[w];
                        int x = p.X;
                        int y = p.Y;

                        if (p.Y != 0 && p.Y != im.Height - 1 && p.X != 0 && p.X != im.Width - 1)
                        {
                            im_res.Data[p.Y, p.X, w] += 1;

                        }
                    }
                }

                /* for (int w = 0; w < 3; w++)
                 {
                     int d = 20 * (int)(calc_p_len(cur_points[w], prev_points[w]));
                     d = d + 10;
                     if (d > imageBox3.Height-1)
                     {
                         d = imageBox3.Height-1;
                     }

                     if (prev_points[w] == cur_points[w]  &&
                         prev_points[w].X != 0 && prev_points[w].Y != im.Height - 1 &&
                         prev_points[w].X != 0 && prev_points[w].X != im.Width - 1)
                     {
                         count1++;
                         //im_res1.Data[d + 30 * w, (int)(i * k), w] = 255;
                     }
                     if (prev_points[w] != cur_points[w])
                     {
                         points_A[w].Add(new PointC(count1, new Point(prev_points[w].X, prev_points[w].Y)));
                         count1 = 0;
                     }
                 }
                 for (int w = 0; w < 3; w++)
                 {
                     prev_points[w] = cur_points[w];
                 }  */
            }
            /* countp = new int[3]{ 0, 0, 0 };
             for (int w = 0; w < 3; w++)
             {
                 int ind = 0;
                 int ind2 = 0;
                 foreach(var item in points_A[w])
                 {
                     if(points_A[w][ind2].count>countp[w])
                     {
                         ind = ind2;
                     }
                     ind2++;
                 }
                 points[w] = points_A[w][ind].point;
             }*/


            for (int x = 0; x < im_res.Width; x++)
            {
                for (int y = 0; y < im_res.Height; y++)
                {
                    for (int w = 0; w < 3; w++)
                    {
                        if (im_res.Data[y, x, w] > countp[w])
                        {
                            points[w] = new Point(x, y);
                            countp[w] = im_res.Data[y, x, w];
                        }

                    }

                }
            }
            Console.WriteLine(points.Length);
            box.Image = im_res;
            return points;
        }
        static Point[] findCrossing(PointF[] lines, Size size)
        {
            var linesX = new List<PointF>();
            var linesY = new List<PointF>();
            var crossP = new List<Point>();
            foreach (var p in lines)
            {
                if (p.X >= 0)
                {
                    linesX.Add(p);
                }
                else
                {
                    linesY.Add(p);
                }
            }
            foreach (var p1 in linesX)
            {
                foreach (var p2 in linesY)
                {
                    var x = (p2.Y - p1.Y) / (p1.X - p2.X);
                    var y = p1.X * x + p1.Y;
                    if (x >= 0 && x < size.Width)
                    {
                        if (y >= 0 && y < size.Height)
                        {
                            crossP.Add(new Point((int)x, (int)y));
                        }
                    }

                }
            }
            if (crossP.Count != 0)
            {
                return crossP.ToArray();
            }
            return null;
        }
        static PointF[] findCrossingD(PointF[] lines, Size size, Mat im)
        {
            var linesX = new List<PointF>();
            var linesY = new List<PointF>();
            var crossP = new List<PointF>();
            var f = from x in lines
                    orderby Math.Abs(x.X)
                    select x;
            var lines_s = f.ToArray();
            for (int i = 0; i < lines.Length / 2; i++)
            {
                linesX.Add(lines_s[i]);
                Console.WriteLine("Line X a =" + lines_s[i].X + " b =  " + lines_s[i].Y);
            }

            for (int i = lines.Length / 2; i < lines.Length; i++)
            {
                linesY.Add(lines_s[i]);
                Console.WriteLine("Line Y a =" + lines_s[i].X + " b =  " + lines_s[i].Y);
            }
            foreach (var p in linesX)
            {
                var pts = find2Points(p, im.Size);
                if (pts != null)
                {
                    CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(255, 0, 0), 2);
                }
            }

            foreach (var p in linesY)
            {
                var pts = find2Points(p, im.Size);
                if (pts != null)
                {
                    CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(0, 255, 0), 2);
                }
            }
            Console.WriteLine("FIND CROSSING------------------");
            foreach (var p1 in linesX)
            {
                foreach (var p2 in linesY)
                {

                    var x = (p2.Y - p1.Y) / (p1.X - p2.X);
                    var y = p1.X * x + p1.Y;
                    var y2 = p2.X * x + p2.Y;
                    var dy = y - y2;

                    if (x >= 0 && x < size.Width)
                    {
                        if (y >= 0 && y < size.Height)
                        {
                            crossP.Add(new PointF(x, y));
                        }
                    }
                    Console.WriteLine("X " + x + " Y " + y + " Y2 " + y2);

                }
            }
            if (crossP.Count != 0)
            {
                return crossP.ToArray();
            }
            return null;
        }

        static private Point leftPoint(Image<Gray, Byte> im, int res_min)
        {
            for (int x = 6; x < im.Width - 6; x++)
            {
                for (int y = 6; y < im.Height - 6; y++)
                {
                    if (im.Data[y, x, 0] > 100 && calcRes(im.Data, x, y) > res_min)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return new Point(0, 0);
        }
        static private Point rightPoint(Image<Gray, Byte> im)
        {
            for (int x = im.Width; x > 0; x--)
            {
                for (int y = 0; y < im.Height - 1; y++)
                {
                    if (im.Data[y, x, 0] > 100)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return new Point(0, 0);
        }
        static private Point downPoint(Image<Gray, Byte> im, int res_min)
        {
            for (int y = 6; y < im.Height - 6; y++)
            {
                for (int x = 6; x < im.Width - 6; x++)
                {

                    if (im.Data[y, x, 0] > 100 && calcRes(im.Data, x, y) > res_min)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return new Point(0, 0);
        }
        static private Point upPoint(Image<Gray, Byte> im, int res_min)
        {
            for (int y = im.Height - 6; y > 6; y--)
            {
                for (int x = 6; x < im.Width - 6; x++)
                {
                    if (im.Data[y, x, 0] > 100 && calcRes(im.Data, x, y) > res_min)
                    {
                        return new Point(x, y);
                    }
                }
            }
            return new Point(0, 0);
        }

        static public PointF[] finPointFsFromImPattern(Mat im, int bin, ImageBox box, ImageBox box_debug, double maxArea = 1000, double minArea = 0.1)
        {
            var receivedImage = new Mat();
            var orig = new Mat();
            im.CopyTo(receivedImage);
            im.CopyTo(orig);

            var size_pat = new Size(7, 7);
            var pointsD_s = new System.Drawing.PointF[size_pat.Width * size_pat.Height];
            FindCircles.findCircles(orig, pointsD_s, size_pat,false);
            var pointsD = PointF.toPointF(pointsD_s);

            //prin.t(pointsD_s);
            if (pointsD != null)
            {
                if (pointsD.Length != 0)
                {
                    var gbs = checkPoints(pointsD);

                    //Console.WriteLine(pointsD[gbs[1]].X + " " + pointsD[gbs[1]].Y);
                    if (gbs != null)
                    {
                        var pos = new List<PointF>();
                        pos.Add(pointsD[gbs[3]]);
                        pos.Add(pointsD[gbs[1]]);
                        pos.Add(pointsD[gbs[2]]);
                        pos.Add(pointsD[gbs[0]]);
                        UtilOpenCV.drawTours(orig, pos.ToArray(), 255, 255, 0, 6);
                        // box.Image = orig;
                        return pos.ToArray();
                    }
                    else
                    {
                        Console.WriteLine("gbs == null");
                        return null;
                    }
                }
                else
                {
                    Console.WriteLine("pointsD.length == null");
                }
            }
            else
            {
                Console.WriteLine("pointsD == null");
            }

            return null;
        }

        static public PointF[] finPointFsFromIm(Mat im, int bin, ImageBox box, ImageBox box_debug, double maxArea = 1000, double minArea = 0.1)
        {
            var receivedImage = new Mat();
            var orig = new Mat();
            im.CopyTo(receivedImage);
            im.CopyTo(orig);
            
            var im_cont = revealContourQ(receivedImage, bin, box_debug);
            Image<Gray, byte> image = im_cont.ToImage<Gray, Byte>();//.ThresholdBinary(new Gray(bin), new Gray(255));

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint contours1 = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            //  imageBox5.Image = im_cont;
            
            CvInvoke.FindContours(image, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            //imageBox6.Image = image;
            //box_debug.Image = image;
            //Console.WriteLine("cont all= " + contours.Size);
            if (contours.Size != 0)
            {
                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    var perim = CvInvoke.ArcLength(contours[i], true);
                    var d = area / (perim * perim);
                    /*Console.WriteLine("area " + area);
                    Console.WriteLine("perim " + perim);
                    Console.WriteLine("d " + d);*/
                    if (area < maxArea && area > minArea && d < 0.2 && d > 0.01)
                    {
                        contours1.Push(contours[i]);
                    }
                }
            }

            CvInvoke.DrawContours(orig, contours1, -1, new MCvScalar(0, 255, 0), 2, LineType.EightConnected);
            //Console.WriteLine("cont add= " + contours1.Size);
            // box_debug.Image = orig;
            
            // box.Image = im_cont;
            var lines = findSquares_rotate(contours1, 7, orig);

            if (lines != null)
            {
                if (lines.Count != 0)
                {
                    //Console.WriteLine("LINES: " + lines.Count);
                    foreach (var p in lines)
                    {
                        var pts = find2Points(p, image.Size);
                        if (pts != null)
                        {
                            //CvInvoke.Line(orig, pts[0], pts[1], new MCvScalar(255, 255, 0), 2);
                        }
                    }

                    // imageBox3.Image = orig;
                    //var pointsD = findCrossingD_rotate(lines.ToArray(), image.Size, orig);
                    var size_pat = new Size(7, 7);
                    var pointsD_s = new System.Drawing.PointF[size_pat.Width* size_pat.Height];
                    FindCircles.findCircles(orig, pointsD_s, size_pat);
                    var pointsD = PointF.toPointF(pointsD_s);
                    //drawTours(im, pointsD, 255, 255, 0, 3);
                    //box.Image = orig;
                    if (pointsD != null)
                    {
                        if (pointsD.Length != 0)
                        {
                            //Console.WriteLine("CROSSINGS: " + pointsD.Length);
                            /*var gbs = findGabarit_rotate(pointsD);
                            var pos = new List<PointF>();
                            pos.Add(pointsD[gbs[1]]);
                            pos.Add(pointsD[gbs[0]]);
                            pos.Add(pointsD[gbs[3]]);
                            pos.Add(pointsD[gbs[2]]);*/
                            var gbs = checkPoints(pointsD);

                            //Console.WriteLine(pointsD[gbs[1]].X + " " + pointsD[gbs[1]].Y);
                            if (gbs != null)
                            {
                                var pos = new List<PointF>();
                                pos.Add(pointsD[gbs[3]]);
                                pos.Add(pointsD[gbs[1]]);
                                pos.Add(pointsD[gbs[2]]);
                                pos.Add(pointsD[gbs[0]]);
                                UtilOpenCV.drawTours(orig, pos.ToArray(), 255, 255, 0, 6);
                             
                               // box.Image = orig;
                                return pos.ToArray();
                            }
                            else
                            {
                                return null;
                            }

                        }
                    }

                }

            }
            return null;
        }
        static public List<PointF[]> finPointFsFromIm_kalib(Mat im, int bin, ImageBox box_debug, double maxArea = 1000, double minArea = 0.1)
        {
            var receivedImage = new Mat();
            var orig = new Mat();
            im.CopyTo(receivedImage);
            im.CopyTo(orig);
            // imageBox3.Image = im;
            var im_cont = revealContourQ(receivedImage, bin, box_debug);
            Image<Gray, byte> image = im_cont.ToImage<Gray, Byte>();//.ThresholdBinary(new Gray(bin), new Gray(255));

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint contours1 = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            //  imageBox5.Image = im_cont;
            CvInvoke.FindContours(image, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            if (contours.Size != 0)
            {
                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    var perim = CvInvoke.ArcLength(contours[i], true);
                    var d = area / (perim * perim);
                    if (area < maxArea && area > minArea && d < 0.2 && d > 0.02)
                    {
                        contours1.Push(contours[i]);
                    }
                }
            }
            CvInvoke.DrawContours(orig, contours1, -1, new MCvScalar(0, 255, 0), 2, LineType.EightConnected);
            Console.WriteLine("cont = " + contours1.Size);
            //   imageBox3.Image = orig;
            var lines = findSquares(contours1, 7, orig);
            if (lines != null)
            {
                if (lines.Count != 0)
                {
                    foreach (var p in lines)
                    {
                        var pts = find2Points(p, image.Size);
                        if (pts != null)
                        {
                            CvInvoke.Line(orig, pts[0], pts[1], new MCvScalar(255, 255, 0), 2);
                        }
                    }
                    var pointsXY = findCrossingD_C(lines.ToArray(), image.Size);
                    if (pointsXY != null)
                    {
                        if (pointsXY[0].Length != 0 && pointsXY[1].Length != 0)
                        {
                            UtilOpenCV.drawTours(orig, pointsXY[0], 255, 255, 0, 6);
                            UtilOpenCV.drawTours(orig, pointsXY[1], 150, 150, 150, 6);
                            var p_X = from f in pointsXY[1]
                                      orderby f.X
                                      select f;
                            var P_X = p_X.ToList();
                            var x_cen = P_X[0].X + (P_X[P_X.Count - 1].X - P_X[0].X) / 2;

                            var p_Y = from f in pointsXY[0]
                                      orderby f.Y
                                      select f;
                            var P_Y = p_Y.ToList();
                            var y_cen = P_Y[0].Y + (P_Y[P_Y.Count - 1].Y - P_Y[0].Y) / 2;

                            var d_x_cen = orig.Width / 2 - x_cen;
                            var d_y_cen = orig.Height / 2 - y_cen;

                            var d_len = (P_X[P_X.Count - 1].X - P_X[0].X) - (P_Y[P_Y.Count - 1].Y - P_Y[0].Y);

                            CvInvoke.PutText(orig, "Xc = " + d_x_cen.ToString(), new Point((int)(0.7 * orig.Width), (int)(0.8 * orig.Height)), FontFace.HersheyPlain, 1, new MCvScalar(255, 0, 0));
                            CvInvoke.PutText(orig, "Yc = " + d_y_cen.ToString(), new Point((int)(0.7 * orig.Width), (int)(0.85 * orig.Height)), FontFace.HersheyPlain, 1, new MCvScalar(255, 0, 0));
                            CvInvoke.PutText(orig, "dL = " + d_len.ToString(), new Point((int)(0.7 * orig.Width), (int)(0.9 * orig.Height)), FontFace.HersheyPlain, 1, new MCvScalar(255, 0, 0));

                            box_debug.Image = orig;
                            Console.WriteLine(P_X[0].X + " " + P_X[1].X + " ");
                            Console.WriteLine(P_Y[0].X + " " + P_Y[1].X + " ");
                            var list_p = new List<PointF[]>();
                            list_p.Add(P_X.ToArray());
                            list_p.Add(P_Y.ToArray());
                            return list_p;
                        }

                    }
                }
            }
            return null;
        }

        static Mat calcMark(Mat im, Mat orig,ImageBox box)
        {

            var c = findMark(im, orig,box);
            
            return paintBlackAboveCont(orig, c,box);
        }

        static VectorOfVectorOfPoint findBiggestContour(Mat im, Mat orig, ImageBox box)
        {
            Image<Gray, byte> image = im.ToImage<Gray, Byte>();
            VectorOfVectorOfPoint contours1 = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            double maxA = 0;
            CvInvoke.FindContours(image, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            VectorOfPoint maxC = new VectorOfPoint();
            if (contours.Size != 0)
            {
                //Console.WriteLine(contours.Size);
                for (int i = 0; i < contours.Size; i++)
                {
                    var area = CvInvoke.ContourArea(contours[i]);
                    var perim = CvInvoke.ArcLength(contours[i], true);
                    //Console.WriteLine("area " + area);
                    //Console.WriteLine("perim " + perim);

                    var d = area / (perim * perim);
                    //Console.WriteLine("d " + d);

                    if (area > maxA && d < 0.2 && d > 0.006)
                    {
                        maxC = contours[i];
                        maxA = area;
                        // Console.WriteLine("add big");
                    }
                }
            }
            if (maxC.Size != 0)
            {
                contours1.Push(maxC);
                //CvInvoke.DrawContours(orig, contours1, -1, new MCvScalar(0, 255, 0), 1);
                return contours1;
            }
            else
            {
                return null;
            }
        }
        static VectorOfVectorOfPoint findMark(Mat im, Mat orig,ImageBox box)
        {
           // box.Image = im;
            var im_m = revealArea(im,box);
            
            return findBiggestContour(im_m, orig,box);
        }
        static private Mat revealArea(Mat im,ImageBox box)
        {
            double scale = (double)im.Width / 600;
            var w = im.Width;
            var h = im.Height;
            var im_ = new Mat();
            CvInvoke.Resize(im, im_, new Size((int)(w / scale), (int)(h / scale)));
            var im_inp = im_.ToImage<Gray, Byte>();
            var im_med = new Image<Gray, Byte>(im_inp.Width, im_inp.Height);
            var im_lap = im_inp.Laplace(9).Convert<Gray, Byte>();
            //box.Image = im_lap.Mat;
            // CvInvoke.AdaptiveThreshold(im_lap, im_med, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 21, red_c);
            CvInvoke.Threshold(im_lap, im_med, 140, 255, ThresholdType.Binary);

            CvInvoke.MedianBlur(im_med, im_med, 3);

            Mat kernel7 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(7, 7), new Point(1, 1));

            Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(1, 1));
            Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));

            Mat ellips7 = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(7, 7), new Point(1, 1));
            Image<Gray, Byte> im_med1 = im_med.MorphologyEx(MorphOp.Gradient, kernel5, new Point(-1, -1), 6, BorderType.Default, new MCvScalar());
            //im_med = im_med1.MorphologyEx(MorphOp.Close, kernel3, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.Resize(im_med1, im_med1, new Size(w, h));
            CvInvoke.Rectangle(im_med1, new Rectangle(0, 0, im_med1.Width + 40, im_med1.Height), new MCvScalar(0), 1);
            /*for (int i = 0; i < im_med1.Width; i++)
            {
                im_med1.
                im_med1.Data[im_med1.Height-100, i, 0] = 120;
            }

            for (int i = 0; i < im_med1.Height; i++)
            {
                im_med1.Data[i, 0, 0] = 0;
                im_med1.Data[i, im_med1.Width-1, 0] = 0;
            }*/

            //Console.WriteLine(im_med.Width + " " + im_med.Height);
            // var im_inp1 = deleteOnePixel(im_med.Mat);
            
            var im_inp1 = im_med1.Mat;

            return im_inp1;
        }
        static private Mat revealContourQ(Mat im, int bin, ImageBox box)
        {
            double scale = (double)im.Width / 1200;
            var w = im.Width;
            var h = im.Height;
            var im_ = new Mat();
            // Console.WriteLine("scale = " + scale);
            CvInvoke.Resize(im, im_, new Size((int)(w / scale), (int)(h / scale)));
            var im_c = new Mat();
            im.CopyTo(im_c);

            var im_inp = im.ToImage<Gray, Byte>();
            var im_med = new Image<Gray, Byte>(im.Width, im.Height);
            var im_lap = im_inp.Laplace(5).Convert<Gray, Byte>();
            CvInvoke.MedianBlur(im_lap, im_lap, 5);

            //CvInvoke.AdaptiveThreshold(im_lap, im_med, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 21, bin);
            CvInvoke.Threshold(im_inp, im_med, bin, 255, ThresholdType.BinaryInv);

            //CvInvoke.AdaptiveThreshold(im_lap, im_med, 255, AdaptiveThresholdType.GaussianC, ThresholdType.Binary, 7, -8);

            //box.Image = im_med;
            /*Mat kernel31 = CvInvoke.GetStructuringElement(ElementShape.Ellipse, new Size(31, 31), new Point(1, 1));
            Mat kernel11 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(11, 11), new Point(1, 1));
            Mat kernel9 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(9, 9), new Point(1, 1));
            Mat kernel7 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(7, 7), new Point(1, 1));
            Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(1, 1));
            Mat kernel3 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(3, 3), new Point(1, 1));
            Image<Gray, Byte> im_med1 = im_med.MorphologyEx(MorphOp.Gradient, kernel3, new Point(-1, -1), 2, BorderType.Default, new MCvScalar());
            im_med = im_med1.MorphologyEx(MorphOp.Close, kernel31, new Point(-1, -1), 1, BorderType.Default, new MCvScalar());
            CvInvoke.Resize(im_med, im_med, new Size(w, h));
            box.Image = im_med.Mat;*/
            

            var mat_med = im_med.Mat;
            //box.Image = mat_med;
            var mat_out = calcMark(im_c, mat_med,box);
            Console.WriteLine("FIN");
            
            return mat_out;
        }
        static int[,] calcBorder(Point[] cont, Size size)
        {
            Image<Gray, Byte> im = new Image<Gray, byte>(size);
            Image<Bgr, Byte> im_deb = new Image<Bgr, byte>(size);
            var inds = checkPoints(PointF.toPointF(cont), 10, (float)Math.PI / 16, true);
            var map = new int[size.Width, 2];

            for (int i = 0; i < map.Length / 2; i++)
            {
                map[i, 0] = 0;
                map[i, 1] = 0;
            }

            for (int i = 0; i < cont.Length; i++)
            {

                var x = cont[i].X;
                var y = cont[i].Y;
                im.Data[y, x, 0] = 255;
                if (map[x, 0] == 0)
                {
                    map[x, 0] = y;

                }
                else if (map[x, 1] == 0)
                {
                    map[x, 1] = y;

                    if (map[x, 0] > map[x, 1])
                    {
                        var tmp = map[x, 0];
                        map[x, 0] = map[x, 1];
                        map[x, 1] = tmp;
                    }
                }
                else
                {

                    if (y > map[x, 1])
                    {
                        map[x, 1] = y;
                    }
                    else if (y < map[x, 0])
                    {
                        map[x, 0] = y;
                    }
                }

            }
            for (int i = 0; i < map.Length / 2; i++)
            {

                im_deb.Data[map[i, 0], i, 0] = 255;
                im_deb.Data[map[i, 1], i, 1] = 255;
            }
            for (int x = 0; x < size.Width; x++)
            {

            }
            map[0, 0] = cont[inds[1]].X;
            map[0, 1] = cont[inds[0]].X;
            var test = PointF.toPointF(cont);
            var pos = new List<Point>();
            pos.Add(cont[inds[0]]);
            pos.Add(cont[inds[1]]);
            pos.Add(cont[inds[2]]);
            pos.Add(cont[inds[3]]);
            //drawTours(im_deb.Mat, pos.ToArray(), 255, 0, 0, 6);
            //drawPoints(im_deb.Mat, rotateTransPointF(toPointF(cont), PI / 16,700.0f),255,255,0,2);
            foreach (var p in inds)
            {
                // Console.WriteLine("gab "+test[p]);
            }
            // Console.WriteLine("BOARDER LR " + cont[inds[0]].X + " " + cont[inds[2]].X);
            //imageBox6.Image = im_deb.Mat;
            return map;
        }
        static Point[] fullCont(VectorOfVectorOfPoint contours, Size size)
        {
            if (contours != null)
            {
                if (contours.Size == 0)
                {
                    return null;
                }


                else
                {
                    var vecP = new List<Point>();
                    Image<Gray, Byte> im = new Image<Gray, byte>(size);
                    CvInvoke.DrawContours(im, contours, -1, new MCvScalar(255), 1);
                    for (int y = 0; y < im.Height; y++)
                    {
                        for (int x = 0; x < im.Width; x++)
                        {
                            if (im.Data[y, x, 0] > 100)
                            {
                                vecP.Add(new Point(x, y));
                            }
                        }
                    }

                    return vecP.ToArray();
                }
            }
            else
            {
                return null;
            }
        }
        static Mat paintBlackAboveCont(Mat _im, VectorOfVectorOfPoint cont, ImageBox box)
        {
            Image<Gray, Byte> im = _im.ToImage<Gray, Byte>();

            var fullcont = fullCont(cont, im.Size);
            var border = calcBorder(fullcont, im.Size);

            for (int x = 0; x < im.Width; x++)
            {
                if (x > border[0, 0] || x < border[0, 1])
                {
                    for (int y = 0; y < border[x, 0]; y++)
                    {
                        im.Data[y, x, 0] = 0;
                    }

                    for (int y = border[x, 1]; y < im.Height; y++)
                    {
                        im.Data[y, x, 0] = 0;
                    }
                }
                else
                {
                    for (int y = 0; y < im.Height; y++)
                    {
                        im.Data[y, x, 0] = 0;
                    }
                }
            }
            return im.Mat;
        }
        static PointF[] rotateTransPointF(PointF[] mass_p, float angle = 0, float x = 0, float y = 0)
        {
            if (mass_p == null)
            {
                return null;
            }
            if (mass_p.Length == 0)
            {
                return null;
            }
            var vec = new PointF(x, y);
            if (x != 0 || y != 0)
            {
                var trans = translPointF(mass_p, vec);
                var polar = decToPolPointF(trans);
                var rotate = PolToDecPointF(polar, angle);
                return rotate;
            }
            else
            {
                var polar = decToPolPointF(mass_p);
                var rotate = PolToDecPointF(polar, angle);
                return rotate;
            }



        }
        static PointF[] decToPolPointF(PointF[] mass_p)
        {
            var mass_p_ret = new PointF[mass_p.Length];
            int i = 0;
            foreach (var p in mass_p)
            {
                var r = p.norm;
                if (p.X > 0)
                {
                    var fi = Math.Asin(p.Y / p.norm);
                    mass_p_ret[i] = new PointF(r, (float)fi);
                    i++;
                }
                else if (p.X < 0)
                {
                    var fi = Math.Asin(p.Y / p.norm);
                    mass_p_ret[i] = new PointF(r, (float)Math.PI - (float)fi);
                    i++;
                }
                else
                {
                    if (p.Y > 0)
                    {
                        var fi = (float)Math.PI / 2;
                        mass_p_ret[i] = new PointF(r, (float)fi);
                        i++;
                    }
                    else
                    {
                        var fi = -(float)Math.PI / 2;
                        mass_p_ret[i] = new PointF(r, (float)fi);
                        i++;
                    }
                }

            }

            return mass_p_ret;
        }
        static PointF[] PolToDecPointF(PointF[] mass_p, float angle = 0.0f)
        {
            var mass_p_ret = new PointF[mass_p.Length];
            int i = 0;
            foreach (var p in mass_p)
            {
                var r = p.X;
                var fi = p.Y - angle;
                var x = r * Math.Cos(fi);
                var y = r * Math.Sin(fi);
                mass_p_ret[i] = new PointF((float)x, (float)y);
                i++;
            }
            return mass_p_ret;
        }
        static PointF[] translPointF(PointF[] mass_p, PointF vec)
        {
            var mass_p_ret = new PointF[mass_p.Length];
            int i = 0;
            foreach (var p in mass_p)
            {
                mass_p_ret[i] = p - vec;
                i++;
            }
            return mass_p_ret;
        }
        static public int calcRes(byte[,,] im_res, int x, int y)
        {

            int res = im_res[y - 5, x - 5, 0] + im_res[y - 5, x - 4, 0] + im_res[y - 5, x - 3, 0] + im_res[y - 5, x - 2, 0] + im_res[y - 5, x - 1, 0] + im_res[y - 5, x, 0] + im_res[y - 5, x + 1, 0] + im_res[y - 5, x + 2, 0] + im_res[y - 5, x + 3, 0] + im_res[y - 5, x + 4, 0] + im_res[y - 5, x + 5, 0] +
                        im_res[y - 4, x - 5, 0] + im_res[y - 4, x + 5, 0] +
                        im_res[y - 3, x - 5, 0] + im_res[y - 3, x + 5, 0] +
                        im_res[y - 2, x - 5, 0] + im_res[y - 2, x + 5, 0] +
                        im_res[y - 1, x - 5, 0] + im_res[y - 1, x + 5, 0] +
                        im_res[y, x - 5, 0] + im_res[y, x + 5, 0] +
                        im_res[y + 1, x - 5, 0] + im_res[y + 1, x + 5, 0] +
                        im_res[y + 2, x - 5, 0] + im_res[y + 2, x + 5, 0] +
                        im_res[y + 3, x - 5, 0] + im_res[y + 3, x + 5, 0] +
                        im_res[y + 4, x - 5, 0] + im_res[y + 4, x + 5, 0] +
                    im_res[y + 5, x - 5, 0] + im_res[y + 5, x - 4, 0] + im_res[y + 5, x - 3, 0] + im_res[y + 5, x - 2, 0] + im_res[y + 5, x - 1, 0] + im_res[y + 5, x, 0] + im_res[y + 5, x + 1, 0] + im_res[y + 5, x + 2, 0] + im_res[y + 5, x + 3, 0] + im_res[y + 5, x + 4, 0] + im_res[y + 5, x + 5, 0];
            return res;
        }
        static Mat deleteOnePixel(Mat src)
        {
            var im_res = src.ToImage<Gray, Byte>();
            var im_ret = new Image<Gray, Byte>(src.Width, src.Width);
            for (int x = 5; x < im_res.Width - 5; x++)
            {
                for (int y = 5; y < im_res.Height - 5; y++)
                {

                    int res = (int)im_res.Data[y - 4, x - 4, 0] + (int)im_res.Data[y - 4, x - 3, 0] + (int)im_res.Data[y - 4, x - 2, 0] + (int)im_res.Data[y - 4, x - 1, 0] + (int)im_res.Data[y - 4, x, 0] + (int)im_res.Data[y - 4, x + 1, 0] + (int)im_res.Data[y - 4, x + 2, 0] + (int)im_res.Data[y - 4, x + 3, 0] + (int)im_res.Data[y - 4, x + 4, 0] +
                        (int)im_res.Data[y - 3, x - 4, 0] + (int)im_res.Data[y - 2, x + 4, 0] +
                        (int)im_res.Data[y - 2, x - 4, 0] + (int)im_res.Data[y - 2, x + 4, 0] +
                        (int)im_res.Data[y - 1, x - 4, 0] + (int)im_res.Data[y - 1, x + 4, 0] +
                        (int)im_res.Data[y, x - 4, 0] + (int)im_res.Data[y, x + 4, 0] +
                        (int)im_res.Data[y + 1, x - 4, 0] + (int)im_res.Data[y + 1, x + 4, 0] +
                        (int)im_res.Data[y + 2, x - 4, 0] + (int)im_res.Data[y + 2, x + 4, 0] +
                        (int)im_res.Data[y + 3, x - 4, 0] + (int)im_res.Data[y + 3, x + 4, 0] +
                     (int)im_res.Data[y + 4, x - 4, 0] + (int)im_res.Data[y + 4, x - 3, 0] + (int)im_res.Data[y + 4, x - 2, 0] + (int)im_res.Data[y + 4, x - 1, 0] + (int)im_res.Data[y + 4, x, 0] + (int)im_res.Data[y + 4, x + 1, 0] + (int)im_res.Data[y + 4, x + 2, 0] + (int)im_res.Data[y + 4, x + 3, 0] + (int)im_res.Data[y + 4, x + 4, 0];
                    if (res < 250)
                    {
                        im_res.Data[y, x, 0] = 0;
                    }
                    else
                    {
                        im_ret.Data[y, x, 0] = im_res.Data[y, x, 0];

                    }
                }
            }

            return im_ret.Mat;
        }


        static private Mat revealContour(Mat im, int bin, ImageBox box)
        {
            var im_inp = im.ToImage<Gray, Byte>();
            var im_med = new Image<Gray, Byte>(im.Width, im.Height);
            var im_lap = im_inp.Laplace(5).Convert<Gray, Byte>();
            CvInvoke.Threshold(im_lap, im_med, bin, 255, ThresholdType.Binary);
            var im_inp1 = deleteOnePixel(im_med.Mat);
            box.Image = im_inp1.ToImage<Gray, Byte>();

            return im_inp1;
        }
        static Point calc_sr_p(Point p1, Point p2)
        {
            return new Point(p2.X + (p1.X - p2.X) / 2, p2.Y + (p1.Y - p2.Y) / 2);//ОКРУГЛИЛ
        }
        static PointF calc_sr_dp(Point p1, Point p2)
        {
            return new PointF(p1.X + (p2.X - p1.X) / 2, p1.Y + (p2.Y - p1.Y) / 2);
        }
        static PointF calc_sr_dp(PointF p1, PointF p2)
        {
            return new PointF(p1.X + (p2.X - p1.X) / 2, p1.Y + (p2.Y - p1.Y) / 2);
        }

        static PointF calcLine(PointF p1, PointF p2)
        {
            var dx = p1.X - p2.X;
            if (dx != 0)
            {
                var a = (p1.Y - p2.Y) / dx;
                return new PointF(a, p1.Y - (a * p1.X));
            }
            else
            {
                Console.WriteLine("dx = 0");
                return new PointF(0, p1.Y);
            }
        }


        static int[] findLine(List<PointF[]> points, double a, double b, PlanType planType)
        {
            var Xindecis = new List<int>();

            for (int i = 0; i < points.Count; i++)
            {
                var p = points[i][0];
                if (planType == PlanType.X)
                {
                    if (p.Y > a * p.X + b)
                    {
                        Xindecis.Add(i);
                    }
                }
                else
                {
                    /* if (p.Y > a * p.X + b)
                     {
                         Xindecis.Add(i);
                     }*/
                    double x = 0;
                    if (a != 0)
                    {
                        x = (p.Y - b) / a;
                    }
                    if (p.X < x)
                    {
                        Xindecis.Add(i);
                    }
                }

            }
            return Xindecis.ToArray();
        }

        static int[] findLine_rotate(PointF[] points)
        {
            var Xindecis = new List<int>();
            for (int i = 0; i < points.Length; i++)
            {
                var p = points[i];

                if (p.Y > 0)
                {
                    //Console.WriteLine
                    Xindecis.Add(i);
                }
            }
            return Xindecis.ToArray();
        }
        static PointF linearApprox(List<PointF> points)
        {
            int n = points.Count;
            double x = 0;
            double y = 0;
            double xy = 0;
            double xx = 0;
            foreach (var p in points)
            {
                x += p.X;
                y += p.Y;
                xy += p.X * p.Y;
                xx += p.X * p.X;
            }
            var del = (n * xx - x * x);
            if (Math.Abs(del) < 0.0001)
            {
                del = 0.00001;
                //return new PointF(4000, (float)x/(float)n);
            }
            float a = (float)((n * xy - x * y) / del);
            Console.WriteLine("a  = " + a);
            Console.WriteLine("del  = " + del);
            Console.WriteLine("num  = " + (n * xy - x * y));
            float b = (float)((y - a * x) / n);
            return new PointF(a, b);
        }
        static List<PointF[]> findOneLine(List<PointF[]> points, List<PointF> points_ret, int side, PlanType planType, Mat im)
        {
            var Xindecis = new List<int>();
            var points_cut = new List<PointF[]>();
            var indices = checkPoints(ListPointToPoint(points));
            //var indices = findGabarit_rotate(points);
            int ind = 0;
            /* foreach (var i in indices)
             {
                 Console.WriteLine("i"+ind+" = "+ i);
                 ind++;
             }*/

            var upPoint = points[indices[3]][0];
            var downPoint = points[indices[1]][0];
            var leftPoint = points[indices[0]][0];
            var rightPoint = points[indices[2]][0];
            var pss = new PointF[] { upPoint, downPoint, leftPoint, rightPoint };
            if (planType == PlanType.Y)
            {
                UtilOpenCV.drawTours(im, pss, 255, 0, 0, 2);
            }


            var Line_up = new PointF(1, 1);
            var Line_down = new PointF(1, 1);

            if (planType == PlanType.X)
            {
                Line_down = calcLine(downPoint, leftPoint);
                Line_up = calcLine(rightPoint, upPoint);
                if (downPoint.X < leftPoint.X || upPoint.X > rightPoint.X)
                {
                    Line_down = calcLine(downPoint, leftPoint);
                    Line_up = calcLine(rightPoint, upPoint);
                }
            }
            else
            {
                Line_down = calcLine(downPoint, rightPoint);
                Line_up = calcLine(leftPoint, upPoint);
                if (downPoint.X < leftPoint.X || upPoint.X > rightPoint.X)
                {

                    Line_down = calcLine(upPoint, leftPoint);
                    Line_up = calcLine(rightPoint, downPoint);
                }
            }

            var col = (float)points.Count / (float)side - 1;
            Console.WriteLine(col);

            double b_k = 0.5;
            double b_1 = b_k * (Line_up.Y - Line_down.Y) / col;
            if (col == 0)
            {
                b_1 = 4000;//for last 100%
            }

            double a = Line_up.X;
            double b = Line_up.Y - b_1;

            Console.WriteLine("UP   a = " + a + " b = " + b);
            Console.WriteLine("DOWN a = " + Line_down.X + " b = " + Line_down.Y);
            var xind = findLine(points, a, b, planType);

            if (xind.Length > side)
            {
                b_k -= 0.2;
                b_1 = b_k * (Line_up.Y - Line_down.Y) / col;
                a = Line_up.X;
                b = Line_up.Y - b_1;
                Console.WriteLine("x = " + a + " y = " + b);
                xind = findLine(points, a, b, planType);
            }
            else if (xind.Length < side)
            {
                b_k += 0.2;
                b_1 = b_k * (Line_up.Y - Line_down.Y) / col;
                a = Line_up.X;
                b = Line_up.Y - b_1;
                Console.WriteLine("x = " + a + " y = " + b);
                xind = findLine(points, a, b, planType);
            }
            else
            {

            }
            var pts = find2Points(new PointF((float)a, (float)b), im.Size);
            if (pts != null)
            {
                CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(0, 255, 255), (int)col + 1);
            }

            var points_cen = new List<PointF>();
            foreach (var i in xind)
            {
                points_cen.Add(points[i][0]);
            }
            Xindecis.AddRange(xind);
            for (int i = 0; i < points.Count; i++)
            {
                if (Xindecis.Contains(i))
                {
                }
                else
                {
                    points_cut.Add(points[i]);
                }
            }
            points = new List<PointF[]>();
            for (int i = 0; i < points_cut.Count; i++)
            {
                points.Add(points_cut[i]);
            }
            //var points_cen_rev_rot = rotatePoints(points_cen, -rotate_angle);
            var appLine = linearApprox(points_cen);
            points_ret.Add(appLine);
            return points;
        }
        static List<PointF[]> findOneLine_rotate(List<PointF[]> points, List<LineF> points_ret, int side, PlanType planType, Mat im)
        {
            //Console.WriteLine("LINE_________________________________________");
            var Xindecis = new List<int>();
            var points_cut = new List<PointF[]>();
            var points_rot = new PointF[points.Count];
            for (int i = 0; i < points_rot.Length; i++)
            {
                points_rot[i] = points[i][0];
            }
            var indices = findGabarit_rotate(points_rot, (float)Math.PI / 6);
            indices = checkPoints(points_rot, 10, (float)Math.PI / 20);

            if (points.Count <= side)
            {

                var appLine = LineF.calcLine(points_rot, planType);
                points_ret.Add(appLine);

                return new List<PointF[]>();
            }

            /* foreach (var i in indices)
             {
                 Console.WriteLine("i"+ind+" = "+ i);
                 ind++;
             }*/

            /*var upPoint = points_rot[indices1[0]];
            var downPoint = points_rot[indices1[3]];
            var leftPoint = points_rot[indices1[2]];
            var rightPoint = points_rot[indices1[1]];
            Console.WriteLine(upPoint + " " + downPoint + " " + leftPoint + " " + rightPoint + " ");*/
            if (indices == null)
            {
                return null;
            }
            var upPoint = points_rot[indices[3]];
            var downPoint = points_rot[indices[1]];
            var leftPoint = points_rot[indices[0]];
            var rightPoint = points_rot[indices[2]];

            bool sq_1 = (Math.Abs(leftPoint.X - downPoint.X)) < 5 && ((upPoint.X - rightPoint.X) < 5);
            bool sq_2 = (Math.Abs(upPoint.X - downPoint.X)) < 5 && ((leftPoint.X - rightPoint.X) < 5);
            bool sq = sq_1 || sq_2;

            //Console.WriteLine("DELTS X " + (Math.Abs(leftPoint.X - downPoint.X)) + " " + ((upPoint.X - rightPoint.X)));
            //Console.WriteLine("DELTS X " + (Math.Abs(upPoint.X - downPoint.X)) + " " + ((leftPoint.X - rightPoint.X)));
            if (sq)//check square
            {
                //Console.WriteLine("THIS IS SQUARE");
                //indices = findGabarit_rotate(points_rot, -PI / 2);
                indices = checkPoints(points_rot, 10, -(float)Math.PI / 3, true);
                //Console.WriteLine("LEN " +points_rot.Length);
                upPoint = points_rot[indices[2]];
                downPoint = points_rot[indices[3]];
                leftPoint = points_rot[indices[1]];
                rightPoint = points_rot[indices[0]];
            }

            upPoint = points_rot[indices[2]];
            downPoint = points_rot[indices[3]];
            leftPoint = points_rot[indices[1]];
            rightPoint = points_rot[indices[0]];
            //check:0 3 2 1 
            //rot:3 1 0 2 

            //Console.WriteLine(upPoint + " | " + downPoint + " | " + leftPoint + " | " + rightPoint + " | ");
            var pss = new PointF[] { upPoint, downPoint, leftPoint, rightPoint };


            var Line_down = new PointF[2];



            if (planType == PlanType.X)
            {
                Line_down = new PointF[] { rightPoint, downPoint };
            }
            else
            {
                Line_down = new PointF[] { leftPoint, downPoint };
            }
            var vec = Line_down[1] - Line_down[0];
            float angle = 0.0f;

            if (vec.X > 0)
            {
                angle = (float)Math.Asin(vec.Y / vec.norm);
            }
            else if (vec.X < 0)
            {
                angle = (float)((float)Math.PI - Math.Asin(vec.Y / vec.norm));

            }
            else
            {
                if (vec.Y > 0)
                {
                    angle = (float)Math.PI / 2;
                }
                else
                {
                    angle = -(float)Math.PI / 2;

                }
            }
            if (planType == PlanType.Y)
            {
                angle += (float)Math.PI;
            }

            if (sq)
            {
                if (planType == PlanType.X)
                {
                    //Console.WriteLine("PLAN X");
                    angle = 0.0f;
                }
                else
                {
                    //Console.WriteLine("PLAN Y");
                    angle = -(float)Math.PI / 2;
                }

            }
            // Console.WriteLine("ANGLE " + angle);
            var points_end = rotateTransPointF(points_rot, angle, downPoint.X, downPoint.Y);//translate points
            if (sq)
            {
                points_end = rotateTransPointF(points_rot, angle, upPoint.X, upPoint.Y);
            }

            var col = (float)points.Count / (float)side - 1;
            //Console.WriteLine(col);


            var test = findGabarit_rotate(points_end);
            // var test1 = checkPoints(points_end);

            foreach (var p in test)
            {
                //Console.WriteLine("ps_end1 " + points_end[p]);
            }


            var size = findSize(points_end);
            if (col == 0)
            {
                col = 0.5f;
            }
            float H = -(float)size.Height / (2 * col);

            if (col < 0.6)
            {
                H = -1000.0f;
            }

            var points_trans = translPointF(points_end, new PointF(00.0f, H));

            if (planType == PlanType.X && col == 4)
            {
                //drawTours(im, points_rot, 0, 255, 0, 2);
                //drawTours(im, points_trans, 0, 0, 255, 2);
                // drawTours(im, pss, 255, 0, 0, 2);
            }
            // drawTours(im, points_trans, 255, 0, 0, 2);

            test = findGabarit_rotate(points_trans);
            foreach (var p in test)
            {
                //Console.WriteLine("ps_trans_r " + points_trans[p]);
            }
            //find inds

            var xind = findLine_rotate(points_trans);
            // Console.WriteLine("LINE_COUNT " + xind.Length);
            /*if (xind.Length > side)
            {
                b_k -= 0.2;
                b_1 = b_k * (Line_up.Y - Line_down.Y) / col;
                a = Line_up.X;
                b = Line_up.Y - b_1;
                Console.WriteLine("x = " + a + " y = " + b);
                xind = findLine(points, a, b, planType);
            }
            else if (xind.Length < side)
            {
                b_k += 0.2;
                b_1 = b_k * (Line_up.Y - Line_down.Y) / col;
                a = Line_up.X;
                b = Line_up.Y - b_1;
                Console.WriteLine("x = " + a + " y = " + b);
                xind = findLine(points, a, b, planType);
            }
            else
            {

            }*/


            /*var pts = find2Points(new PointF((float)a, (float)b), im.Size);
            if (pts != null)
            {
                CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(0, 255, 255), (int)col + 1);
            }
            */
            var points_cen = new List<PointF>();
            foreach (var i in xind)
            {
                points_cen.Add(points[i][0]);
            }
            Xindecis.AddRange(xind);
            for (int i = 0; i < points.Count; i++)
            {
                if (Xindecis.Contains(i))
                {
                }
                else
                {
                    points_cut.Add(points[i]);
                }
            }
            points = new List<PointF[]>();
            for (int i = 0; i < points_cut.Count; i++)
            {
                points.Add(points_cut[i]);
            }
            // var points_cen_rev_rot = rotatePoints(points_cen, -rotate_angle);
            var appLine1 = LineF.calcLine(points_cen.ToArray(), planType);
            points_ret.Add(appLine1);

            return points;
        }
        static List<PointF> findSquares(VectorOfVectorOfPoint contours, int side, Mat im)
        {
            if (contours.Size == 0)
            {
                return null;
            }
            else
            {
                var points_ret = new List<PointF>();
                var pointsX = new List<PointF[]>();
                var pointsY = new List<PointF[]>();

                var points_t = new List<PointF>();
                for (int i = 0; i < contours.Size; i++)
                {
                    //points.Add(findCentrD(contours[i]));
                    var ps = findCentrD_rotate(contours[i], im);
                    pointsX.Add(ps);
                    pointsY.Add(ps);
                    points_t.Add(ps[0]);
                }
                //drawTours(im, points_t.ToArray(), 255, 0, 0, 2);
                //var points_rot = rotatePoints(points, rotate_angle);
                //points = points_rot;
                Console.WriteLine("points = " + points_t.Count);
                if (points_t.Count == 0)
                {
                    return null;
                }
                else
                {
                    int ind_max = 2 * side;
                    int ind = 0;
                    while (pointsX.Count > 0 && ind < ind_max)
                    {

                        pointsX = findOneLine(pointsX, points_ret, side, PlanType.X, im);
                        if (pointsX == null)
                        {
                            Console.WriteLine("X NULL");
                            return null;
                        }
                        //drawTours(im, PointToP(pointsY), ind*20, ind * 20, ind * 20, 4);
                        Console.WriteLine("X_rem = " + pointsX.Count);
                        Console.WriteLine("X_ind = " + ind);
                        ind++;

                    }
                    Console.WriteLine("indX = " + ind);
                    ind = 0;
                    Console.WriteLine("-----------------------------------------------------------------------");
                    while (pointsY.Count > 0 && ind < ind_max)
                    {
                        pointsY = findOneLine(pointsY, points_ret, side, PlanType.Y, im);
                        if (pointsY == null)
                        {
                            return null;
                        }
                        Console.WriteLine("Y_rem = " + pointsY.Count);
                        ind++;
                    }
                    Console.WriteLine("indY = " + ind);
                    return points_ret;
                }
            }
        }
        static List<LineF> findSquares_rotate(VectorOfVectorOfPoint contours, int side, Mat im)
        {
            if (contours.Size == 0)
            {
                return null;
            }
            else
            {
                var points_ret = new List<LineF>();
                var pointsX = new List<PointF[]>();
                var pointsY = new List<PointF[]>();

                var points_t = new List<PointF>();
                for (int i = 0; i < contours.Size; i++)
                {
                    //points.Add(findCentrD(contours[i]));
                    var ps = findCentrD_rotate(contours[i], im);
                    if (ps != null)
                    {
                        pointsX.Add(ps);
                        pointsY.Add(ps);
                        points_t.Add(ps[0]);
                    }

                }
                UtilOpenCV.drawTours(im, points_t.ToArray(), 255, 0, 0, 2);

                //var points_rot = rotatePoints(points, rotate_angle);
                //points = points_rot;
                //Console.WriteLine("points = " + points_t.Count);
                if (points_t.Count == 0)
                {
                    return null;
                }
                else
                {

                    int ind_max = 2 * side;
                    int ind = 0;
                    while (pointsX.Count > 0 && ind < ind_max)
                    {

                        pointsX = findOneLine_rotate(pointsX, points_ret, side, PlanType.X, im);
                        if (pointsX == null)
                        {
                            return null;
                        }
                        //drawTours(im, PointToP(pointsY), ind*20, ind * 20, ind * 20, 4);
                        // Console.WriteLine("X_rem = " + pointsX.Count);
                        ind++;

                    }
                    // Console.WriteLine("indX = " + ind);
                    ind = 0;
                    //Console.WriteLine("-----------------------------------------------------------------------");
                    while (pointsY.Count > 0 && ind < ind_max)
                    {
                        pointsY = findOneLine_rotate(pointsY, points_ret, side, PlanType.Y, im);
                        if (pointsY == null)
                        {
                            return null;
                        }
                        //Console.WriteLine("Y_rem = " + pointsY.Count);
                        ind++;
                    }
                    // Console.WriteLine("indY = " + ind);
                    return points_ret;
                }
            }
        }



        static PointF[] ListPointToPoint(List<PointF[]> ps)
        {
            List<PointF> ret = new List<PointF>();
            foreach (var p in ps)
            {
                ret.Add(p[0]);
            }
            return ret.ToArray();
        }
        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        static int[] findGabarit_rotate(PointF[] cont, float maxAngle = 2 * (float)Math.PI, int count = 100)
        {
            var alpha_max = maxAngle;
            var del = alpha_max / count;
            List<int[]> ps = new List<int[]>();
            for (float alpha = 0; alpha < alpha_max; alpha += del)
            {
                var cont_rot = rotateTransPointF(cont, alpha);
                ps.Add(findGabarit(cont_rot));
            }
            int len = cont.Length;
            int[] inds = new int[len];
            foreach (var p in ps)
            {
                foreach (var s in p)
                {
                    inds[s]++;
                }
            }
            int n = 4;
            var ps_i = indexOfMaxValue(inds, n);
            PointF[] cont_ret = new PointF[n];
            for (int i = 0; i < cont_ret.Length; i++)
            {
                cont_ret[i] = cont[ps_i[i]];
            }
            var inds_f = findGabarit_cut(cont_ret);
            int[] inds_r = new int[n];
            for (int i = 0; i < n; i++)
            {
                inds_r[i] = ps_i[inds_f[i]];
            }
            return inds_r;
        }
        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        static int[] findGabarit_rotate(VectorOfPoint cont)
        {
            var alpha_max = 2 * (float)Math.PI;
            var del = alpha_max / 60;
            List<int[]> ps = new List<int[]>();
            for (float alpha = 0; alpha < alpha_max; alpha += del)
            {
                var cont_rot = rotatePoints(cont, alpha);
                ps.Add(findGabarit(cont_rot));
            }
            int len = cont.Size;
            int[] inds = new int[len];

            foreach (var p in ps)
            {
                foreach (var s in p)
                {
                    inds[s]++;
                }
            }
            int n = 4;
            var ps_i = indexOfMaxValue(inds, n);
            VectorOfPoint cont_ret = new VectorOfPoint();
            for (int i = 0; i < ps_i.Length; i++)
            {
                cont_ret.Push(new Point[] { cont[ps_i[i]] });
            }
            var inds_f = findGabarit_cut(cont_ret);
            int[] inds_r = new int[n];
            for (int i = 0; i < n; i++)
            {
                inds_r[i] = ps_i[inds_f[i]];
            }
            return inds_r;
        }
        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        static int[] findGabarit_rotate(List<PointF[]> cont)
        {
            if (cont == null)
            {
                return null;
                if (cont.Count == 0)
                {
                    return null;
                }
            }
            var alpha_max = 2 * (float)Math.PI;
            var del = alpha_max / 60;
            List<int[]> ps = new List<int[]>();

            for (float alpha = 0; alpha < alpha_max; alpha += del)
            {
                var cont_rot = rotatePoints(cont, alpha);
                ps.Add(findGabarit(cont_rot));
            }

            int len = cont.Count;
            int[] inds = new int[len];
            foreach (var p in ps)
            {
                foreach (var s in p)
                {
                    inds[s]++;
                }
            }
            int n = 4;
            var ps_i = indexOfMaxValue(inds, n);
            List<PointF[]> cont_ret = new List<PointF[]>();
            for (int i = 0; i < ps_i.Length; i++)
            {
                cont_ret.Add(cont[ps_i[i]]);
            }
            var inds_f = findGabarit_cut(cont_ret);
            int[] inds_r = new int[n];
            for (int i = 0; i < inds_r.Length; i++)
            {
                inds_r[i] = ps_i[inds_f[i]];
            }
            return inds_r;
        }

        static int[] indexOfMaxValue(int[] mass, int n)
        {
            List<int> ps = new List<int>();
            int[] mass_copy = new int[mass.Length];
            mass.CopyTo(mass_copy);
            for (int i = 0; i < n; i++)
            {
                int ind = indexOfMaxValueOne(mass_copy);
                ps.Add(ind);
                mass_copy[ind] = int.MinValue;
            }
            return ps.ToArray();
        }
        static int indexOfMaxValueOne(int[] mass)
        {
            int ind_max = 0;
            var max_val = int.MinValue;
            for (int i = 0; i < mass.Length; i++)
            {
                if (mass[i] > max_val)
                {
                    max_val = mass[i];
                    ind_max = i;
                }
            }
            return ind_max;
        }


        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        static int[] findGabarit(VectorOfPoint cont)

        {
            int X_min = int.MaxValue;
            int Y_min = int.MaxValue;
            int X_max = int.MinValue;
            int Y_max = int.MinValue;
            var indices = new int[4];
            for (int i = 0; i < cont.Size; i++)
            {
                var p = cont[i];
                if (p.X > X_max)
                {
                    X_max = p.X;
                    indices[2] = i;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                    indices[0] = i;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }
            }
            return indices;
        }
        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        static int[] findGabarit(Point[] cont)
        {
            int X_min = int.MaxValue;
            int Y_min = int.MaxValue;
            int X_max = int.MinValue;
            int Y_max = int.MinValue;
            var indices = new int[4];
            for (int i = 0; i < cont.Length; i++)
            {
                var p = cont[i];
                if (p.X > X_max)
                {
                    X_max = p.X;
                    indices[2] = i;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                    indices[0] = i;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }
            }
            return indices;
        }
        ///<summary>
        ///LEFT, DOWN, RIGHT, UP
        ///</summary>
        ///
        static int[] findGabarit(PointF[] cont)

        {

            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var indices = new int[4];
            for (int i = 0; i < cont.Length; i++)
            {
                var p = cont[i];
                if (p.X > X_max)
                {
                    X_max = p.X;
                    indices[2] = i;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                    indices[0] = i;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }
            }
            return indices;
        }
        static int[] findGabarit(List<PointF[]> cont)

        {
            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var indices = new int[4];
            for (int i = 0; i < cont.Count; i++)
            {
                var p = cont[i][0];
                if (p.X > X_max)
                {
                    X_max = p.X;
                    indices[2] = i;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                    indices[0] = i;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }
            }
            return indices;
        }
        static int[] findGabarit_cut(VectorOfPoint cont)

        {
            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var indices = new int[4];


            for (int i = 0; i < cont.Size; i++)
            {

                var p = cont[i];
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }

            }
            for (int i = 0; i < cont.Size; i++)
            {
                if ((i != indices[1]) && (i != indices[3]))
                {
                    var p = cont[i];
                    if (p.X > X_max)
                    {
                        X_max = p.X;
                        indices[2] = i;
                    }
                    if (p.X < X_min)
                    {
                        X_min = p.X;
                        indices[0] = i;
                    }
                }
            }
            return indices;
        }
        static int[] findGabarit_cut(PointF[] cont)

        {
            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var indices = new int[4];


            for (int i = 0; i < cont.Length; i++)
            {

                var p = cont[i];
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }

            }
            for (int i = 0; i < cont.Length; i++)
            {
                if ((i != indices[1]) && (i != indices[3]))
                {
                    var p = cont[i];
                    if (p.X > X_max)
                    {
                        X_max = p.X;
                        indices[2] = i;
                    }
                    if (p.X < X_min)
                    {
                        X_min = p.X;
                        indices[0] = i;
                    }
                }
            }
            return indices;
        }
        static int[] findGabarit_cut(List<PointF[]> cont)
        {
            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var indices = new int[4];
            for (int i = 0; i < cont.Count; i++)
            {

                var p = cont[i][0];
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    indices[3] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    indices[1] = i;
                }

            }
            for (int i = 0; i < cont.Count; i++)
            {
                if ((i != indices[1]) && (i != indices[3]))
                {
                    var p = cont[i][0];
                    if (p.X > X_max)
                    {
                        X_max = p.X;
                        indices[2] = i;
                    }
                    if (p.X < X_min)
                    {
                        X_min = p.X;
                        indices[0] = i;
                    }
                }
            }


            return indices;
        }
        /// <summary>
        /// MaxX MinX MaxY MinY
        /// </summary>
        static int[] findMaxMin(PointF[] cont)
        {
            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            var ret = new int[4];
            for (int i = 0; i < cont.Length; i++)
            {
                var p = cont[i];
                if (p.X > X_max)
                {
                    X_max = p.X;
                    ret[0] = i;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                    ret[1] = i;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                    ret[2] = i;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                    ret[3] = i;
                }
            }
            return ret;
        }
        static Size findSize(PointF[] cont)

        {

            float X_min = float.MaxValue;
            float Y_min = float.MaxValue;
            float X_max = float.MinValue;
            float Y_max = float.MinValue;
            for (int i = 0; i < cont.Length; i++)
            {
                var p = cont[i];
                if (p.X > X_max)
                {
                    X_max = p.X;
                }
                if (p.X < X_min)
                {
                    X_min = p.X;
                }
                if (p.Y > Y_max)
                {
                    Y_max = p.Y;
                }
                if (p.Y < Y_min)
                {
                    Y_min = p.Y;
                }
            }
            return new Size((int)(X_max - X_min), (int)(Y_max - Y_min));
        }
        static List<PointF[]> rotatePoints(List<PointF[]> inp, double alpha)
        {
            var outp = new List<PointF[]>();
            foreach (var p in inp)
            {
                var outP = new List<PointF>();
                foreach (var P in p)
                {
                    var r = PointF.calc_p_len(P, new PointF(0, 0));
                    double gamma = 0;
                    if (r != 0)
                    {
                        gamma = Math.Asin(P.Y / r);
                    }
                    outP.Add(new PointF((float)(r * Math.Cos(gamma - alpha)),
                        (float)(r * Math.Sin(gamma - alpha))));
                }
                outp.Add(outP.ToArray());
            }
            return outp;
        }

        static List<PointF> rotatePoints(List<PointF> inp, double alpha)
        {
            var outP = new List<PointF>();
            foreach (var P in inp)
            {
                var r = PointF.calc_p_len(P, new PointF(0, 0));
                double gamma = 0;
                if (r != 0)
                {
                    gamma = Math.Asin(P.Y / r);
                }
                outP.Add(new PointF((float)(r * Math.Cos(gamma - alpha)),
                    (float)(r * Math.Sin(gamma - alpha))));
            }
            return outP;
        }

        static VectorOfPoint rotatePoints(VectorOfPoint inpV, double alpha)
        {
            var inp = new List<Point>();
            var outP = new List<Point>();
            for (int i = 0; i < inpV.Size; i++)
            {
                inp.Add(inpV[i]);
            }
            foreach (var P in inp)
            {
                var r = PointF.calc_p_len(P, new Point(0, 0));
                double gamma = 0;
                if (r != 0)
                {
                    gamma = Math.Asin(P.Y / r);
                }
                outP.Add(new Point((int)(r * Math.Cos(gamma - alpha)),
                    (int)(r * Math.Sin(gamma - alpha))));
            }
            return new VectorOfPoint(outP.ToArray());
        }

        static PointF[] findCentrD(VectorOfPoint cont)
        {
            var indices = findGabarit(cont);
            var p1 = calc_sr_dp(cont[indices[0]], cont[indices[2]]);
            var p2 = calc_sr_dp(cont[indices[1]], cont[indices[3]]);
            var ret = new List<PointF>();
            ret.Add(calc_sr_dp(p1, p2));
            for (int i = 0; i < 4; i++)
            {
                var p = cont[indices[i]];
                ret.Add(new PointF(p.X, p.Y));
            }
            return ret.ToArray();
        }

        static PointF[] findCentrD_rotate(VectorOfPoint cont, Mat im)
        {
            var pf = PointF.toPointF(cont);
            //drawPoints(im, pf, 0, 255, 0, 2);
            var indices = checkPoints(pf, 100, (float)Math.PI / 18, true);
            if (indices == null)
            {
                return null;
            }
            var p1 = calc_sr_dp(pf[indices[0]], pf[indices[2]]);
            var p2 = calc_sr_dp(pf[indices[1]], pf[indices[3]]);
            var ret = new List<PointF>();
            ret.Add(calc_sr_dp(p1, p2));
            for (int i = 0; i < 4; i++)
            {
                var p = pf[indices[i]];
                ret.Add(new PointF(p.X, p.Y));
            }
            return ret.ToArray();
        }
        static Point findCentr(VectorOfPoint cont)
        {
            var indices = findGabarit(cont);
            var p1 = calc_sr_p(cont[indices[0]], cont[indices[2]]);
            var p2 = calc_sr_p(cont[indices[1]], cont[indices[3]]);
            return calc_sr_p(p1, p2);
        }
        static Point[] find2Points(PointF ab, Size size)
        {
            int ind = 0;
            var ret = new Point[4];
            var a = ab.X;
            var b = ab.Y;

            var x0 = -b / a;
            var xH = (size.Height - b) / a;

            var y0 = b;
            var yW = a * size.Width + b;
            if (x0 >= 0 && x0 <= size.Width)
            {

                ret[ind] = new Point((int)x0, 0);
                ind++;
            }
            if (xH >= 0 && xH <= size.Width)
            {
                ret[ind] = new Point((int)xH, size.Height - 1);
                ind++;
            }
            if (y0 >= 0 && y0 <= size.Height)
            {
                ret[ind] = new Point(0, (int)y0);
                ind++;
            }
            if (yW >= 0 && yW <= size.Height)
            {
                ret[ind] = new Point(size.Width - 1, (int)yW);
                ind++;
            }
            if (ind == 2)
            {
                return ret;
            }
            return ret;
        }
        static Point[] find2Points(LineF ab, Size size)
        {
            if (ab == null)
            {
                return null;
            }
            int ind = 0;

            var ret = new Point[4];
            var a = ab.X;
            var b = ab.Y;

            var x0 = -b / a;
            var xH = (size.Height - b) / a;

            var y0 = b;
            var yW = a * size.Width + b;

            if (ab.plan == PlanType.Y)
            {
                y0 = -b / a;
                yW = (size.Width - b) / a;

                x0 = b;
                xH = a * size.Height + b;
            }
            if (x0 >= 0 && x0 <= size.Width)
            {

                ret[ind] = new Point((int)x0, 0);
                ind++;
            }
            if (xH >= 0 && xH <= size.Width)
            {
                ret[ind] = new Point((int)xH, size.Height - 1);
                ind++;
            }
            if (y0 >= 0 && y0 <= size.Height)
            {
                ret[ind] = new Point(0, (int)y0);
                ind++;
            }
            if (yW >= 0 && yW <= size.Height)
            {
                ret[ind] = new Point(size.Width - 1, (int)yW);
                ind++;
            }
            if (ind == 2)
            {
                return ret;
            }
            return ret;
        }


        static PointF[] findCrossingD_rotate(LineF[] lines, Size size, Mat im)
        {
            var linesX = new List<LineF>();
            var linesY = new List<LineF>();
            var crossP = new List<PointF>();
            int lx = 0;
            foreach (var line in lines)
            {
                if (line != null)
                {
                    if (line.plan == PlanType.X)
                    {
                        lx++;
                    }
                }
            }
            if (lx / lines.Length == 0.5)
            {
                foreach (var line in lines)
                {
                    if (line != null)
                    {
                        if (line.plan == PlanType.X)
                        {
                            linesX.Add(line);
                        }
                        else if (line.plan == PlanType.Y)
                        {
                            linesY.Add(line);
                        }
                    }
                }
            }
            else
            {
                var f = from x in lines
                        orderby x.X
                        select x;
                var lines_s = f.ToArray();
                for (int i = 0; i < lines.Length / 2; i++)
                {
                    linesX.Add(lines_s[i]);
                    //Console.WriteLine("Line X a =" + lines_s[i].X + " b =  " + lines_s[i].Y);
                }

                for (int i = lines.Length / 2; i < lines.Length; i++)
                {
                    linesY.Add(lines_s[i]);
                    //Console.WriteLine("Line Y a =" + lines_s[i].X + " b =  " + lines_s[i].Y);
                }
            }

            foreach (var p in linesX)
            {
                var pts = find2Points(p, im.Size);
                if (pts != null)
                {
                    CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(255, 0, 0), 2);
                }
            }

            foreach (var p in linesY)
            {
                var pts = find2Points(p, im.Size);
                if (pts != null)
                {
                    CvInvoke.Line(im, pts[0], pts[1], new MCvScalar(0, 255, 0), 2);
                }
            }
            var L = new LineF(1, 1);
            //Console.WriteLine("FIND CROSSING------------------");
            foreach (var p1 in linesX)
            {
                foreach (var p2 in linesY)
                {
                    var p = L.findCrossing(p1, p2);
                    var x = p.X;
                    var y = p.Y;

                    if (x >= 0 && x < size.Width)
                    {
                        if (y >= 0 && y < size.Height)
                        {
                            crossP.Add(new PointF(x, y));
                        }
                    }
                    //Console.WriteLine("X " + x + " Y " + y );

                }
            }
            if (crossP.Count != 0)
            {
                return crossP.ToArray();
            }
            return null;
        }

        static List<PointF[]> findCrossingD_C(PointF[] lines, Size size)
        {
            var linesX = new List<PointF>();
            var linesY = new List<PointF>();
            var crossX = new List<PointF>();
            var crossY = new List<PointF>();
            foreach (var p in lines)
            {
                if (p.X >= 0)
                {
                    linesX.Add(p);
                }
                else
                {
                    linesY.Add(p);
                }
            }
            int i_x = 0;
            foreach (var p1 in linesX)
            {
                int i_y = 0;
                foreach (var p2 in linesY)
                {
                    var x = (p2.Y - p1.Y) / (p1.X - p2.X);
                    var y = p1.X * x + p1.Y;
                    if (x >= 0 && x < size.Width)
                    {
                        if (y >= 0 && y < size.Height)
                        {
                            if (i_x == i_y && i_x != (int)((linesX.Count - 1) / 2))
                            {
                                crossX.Add(new PointF(x, y));
                            }
                            if (i_x == linesX.Count - i_y - 1 && i_x != (int)((linesX.Count - 1) / 2))
                            {
                                crossY.Add(new PointF(x, y));
                            }

                        }
                    }
                    i_y++;
                }
                i_x++;
            }
            var ret = new List<PointF[]>();
            if (crossX.Count != 0 && crossY.Count != 0)
            {
                ret.Add(crossX.ToArray());
                ret.Add(crossY.ToArray());
                return ret;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points_in"></param>
        /// <param name="iter"></param>
        /// <param name="thickness">
        /// max area in percent? normal value 30...80
        /// </param>
        /// <param name="solid">
        /// contour or solid
        /// </param>
        /// <returns>
        /// limitPoints of Square
        /// </returns>
        static int[] checkPoints(PointF[] points_in, int iter = 10, float angle = (float)Math.PI / 10, bool skip_first = false)
        {
            if (skip_first)
            {
                if (iter > 0)
                {
                    iter--;
                    var points_rot = rotateTransPointF(points_in, angle);
                    return checkPoints(points_rot, iter);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                var ints = findMaxMin(points_in);
                var ps = new PointF[] { new PointF(points_in[ints[0]]),
                new PointF(points_in[ints[1]]),
                new PointF(points_in[ints[2]]),
                new PointF(points_in[ints[3]])};
                //
                bool maxX = (ps[0].X > ps[1].X) && (ps[0].X > ps[2].X) && (ps[0].X > ps[3].X);

                bool minX = (ps[1].X < ps[0].X) && (ps[1].X < ps[2].X) && (ps[1].X < ps[3].X);

                bool maxY = (ps[2].Y > ps[0].Y) && (ps[2].Y > ps[1].Y) && (ps[2].Y > ps[3].Y);

                bool minY = (ps[3].Y < ps[0].Y) && (ps[3].Y < ps[1].Y) && (ps[3].Y < ps[2].Y);

                bool checking = maxX && minX && maxY && minY;
               // Console.WriteLine("ITER N " + iter + " " + maxX + " " + minX + " " + maxY + " " + minY);
               // Console.WriteLine("Max x " + ps[0] + " Min x " + ps[1] + " Max Y " + ps[2] + " Min Y " + ps[3]);
                if (checking == true)
                {
                    return ints;
                }
                else
                {
                    if (iter > 0)
                    {
                        iter--;
                        var points_rot = rotateTransPointF(points_in, angle);
                        return checkPoints(points_rot, iter);
                    }
                    else
                    {
                        return null;
                    }
                }
            }


        }
    }
}
