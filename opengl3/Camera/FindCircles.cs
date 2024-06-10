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
using System.Drawing;

namespace opengl3
{
    public class ContourCV
    {
        public VectorOfPoint cont_orig;
        public double perim;
        public double area;
        public double fig;//  fig = area/perim
        public System.Drawing.PointF pc;
        public ContourCV(VectorOfPoint cont)
        {
            area= CvInvoke.ContourArea(cont);
            perim = CvInvoke.ArcLength(cont, true);
            fig = area / perim;
            pc = FindCircles.findCentrCont(cont);
            cont_orig = cont;
        }

        static public ContourCV[] contourCVs(VectorOfVectorOfPoint conts)
        {
            var conts_cv = new List<ContourCV>();
            for(int i=0; i<conts.Size; i++)
            {
                conts_cv.Add(new ContourCV(conts[i]));
            }
            return conts_cv.ToArray();
        }

    }
    public static class FindCircles
    {
        static int counter = 0;
        static public Mat sobel_mat(Mat mat)
        {
            

            var gray_x = new Mat();
            var gray_y = new Mat();
            CvInvoke.Sobel(mat, gray_x, DepthType.Cv32F, 1, 0, 3);
            CvInvoke.Sobel(mat, gray_y, DepthType.Cv32F, 0, 1, 3);
            CvInvoke.ConvertScaleAbs(gray_x, gray_x, 1, 0);
            CvInvoke.ConvertScaleAbs(gray_y, gray_y, 1, 0);
            return gray_x + gray_y;

        }

        public static VectorOfPoint find_max_contour(Mat mat)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(mat, contours, hier, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            if (contours.Size == 0) return null;
            var size_m = 0d;
            var int_m = 0;
            for(int i=0;i<contours.Size;i++)
            {
                var size = CvInvoke.ContourArea(contours[i]);
                if (size>size_m)
                {
                    size_m = size;
                    int_m = i;
                }
            }

            return contours[int_m];
        }
        public static Mat findCircles(Mat mat,ref System.Drawing.PointF[]  corn,Size pattern_size,bool order = true)
        {
            var im_tr = new Mat();
            var orig = new Mat();


            /*mat.CopyTo(rec);
            mat.CopyTo(orig);
            var im = rec.ToImage<Gray, byte>();
            var im_blur = im.SmoothGaussian(7);
            var im_sob = sobel(im_blur);
            var im_tr = im_sob.ThresholdBinary(new Gray(85), new Gray(255));*/

            CvInvoke.CvtColor(mat, im_tr, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(im_tr, im_tr, new Size(7, 7), 3);
            CvInvoke.Imshow("gauss", im_tr);
            CvInvoke.WaitKey();
            im_tr = sobel_mat(im_tr);
            CvInvoke.Imshow("sobel_d", im_tr);
            CvInvoke.WaitKey();
            CvInvoke.Threshold(im_tr, im_tr, 95, 255, ThresholdType.Binary);
            CvInvoke.Imshow("im_tr", im_tr);
            CvInvoke.WaitKey();

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(im_tr, contours, hier, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            //var conts = sameContours_cv(contours);
            contours = size_filter(contours,20);
            orig = mat.Clone();
            CvInvoke.DrawContours(orig, contours, -1, new MCvScalar(255, 0, 0), 1, LineType.EightConnected);
          // CvInvoke.DrawContours(orig, conts, -1, new MCvScalar(0, 255, 0), 2, LineType.EightConnected);
            Console.WriteLine("find_circ");
            CvInvoke.Imshow("orig_c", orig);
            CvInvoke.WaitKey();
            contours = only_same_centres(contours);
            var conts = sameContours(contours);
            if (conts == null)
            {
                Console.WriteLine("find_circ ret null"); 
                return null;
            }
            if (conts.Size == 0)
            {
                Console.WriteLine("find_circ ret null"); 
                return null;
            }
            
            conts = filter_same_centres(conts);
            var cents = findCentres(conts);
            CvInvoke.CvtColor(im_tr, im_tr, ColorConversion.Gray2Bgr);
           // CvInvoke.WaitKey();
            if (conts!=null)
            {
                //CvInvoke.DrawContours(im_tr, conts, -1, new MCvScalar(255, 0, 0), 2, LineType.EightConnected);
            }
           
            CvInvoke.DrawContours(im_tr, contours, -1, new MCvScalar(255, 0, 255), 1, LineType.EightConnected);
            //CvInvoke.DrawContours(im_tr, conts_sc, -1, new MCvScalar(255, 0, 255), 1, LineType.EightConnected);

            CvInvoke.Imshow("bin ", im_tr);
            
            counter++;

            if (cents == null)
            {
                Console.WriteLine("find_circ ret null");
                return null;
            }
          
            
            //prin.t(cents);
            //prin.t("____________");
            // CvInvoke.Imshow("fnd", orig);
            // CvInvoke.WaitKey();
            corn = new System.Drawing.PointF[pattern_size.Width * pattern_size.Height];
            
            if(order)
            {
                var ps_ord = orderPoints(cents, pattern_size);
                //ps_ord = ps_ord.Reverse().ToArray();

                if (ps_ord != null && ps_ord.Length<=corn.Length)
                {
                    ps_ord.CopyTo(corn, 0);
                    
                    
                }
                else
                {
                    if (ps_ord == null)
                    {
                        Console.WriteLine("ps_ord NULL");
                        return null;                     
                    }
                }

                //Console.WriteLine(" corn______________________");
                //Console.WriteLine(ps_ord.Length+" "+ corn.Length);
                //prin.t(corn);
                UtilOpenCV.drawTours(orig, PointF.toPoint(corn), 255, 0, 0, 2);
                //UtilOpenCV.drawTours(im_tr, PointF.toPoint(corn), 255, 0, 0, 2);
                UtilOpenCV.drawLines(orig, ps_ord, 0, 0, 255, 2);

                
                return orig;//im_tr
            }
            else
            {
                cents.CopyTo(corn, 0);
                UtilOpenCV.drawLines(orig, cents, 0, 255, 0, 2);
                Console.WriteLine("cents");
                return orig;
            }          
        }
        static VectorOfVectorOfPoint filter_same_centres(VectorOfVectorOfPoint contours)
        {
            var filtr_conts =new VectorOfVectorOfPoint();
            filtr_conts.Push(contours[0]);
            for(int i=1; i < contours.Size; i++)
            {
                bool same_place = false;
                for (int j = 0; j < filtr_conts.Size; j++)
                {
                    if (distContours(contours[i], filtr_conts[j])<10)
                    {
                        same_place = true;
                    }
                }
                if(!same_place)
                {
                    filtr_conts.Push(contours[i]);
                }
            }
            return filtr_conts;
        }

        static VectorOfVectorOfPoint only_same_centres(VectorOfVectorOfPoint contours)
        {
            var filtr_conts = new VectorOfVectorOfPoint();
            for (int i = 0; i < contours.Size; i++)
            {

                bool same_place = false;
                for (int j = 0; j < contours.Size; j++)
                {
                    if(i!=j)
                        if (distContours(contours[i], contours[j]) < 3)
                            same_place = true;
                    
                }
                if (same_place)
                {
                    filtr_conts.Push(contours[i]);
                }
            }
            return filtr_conts;
        }
        static public  VectorOfVectorOfPoint size_filter(VectorOfVectorOfPoint contours,double min_size = double.MinValue, double max_size = double.MaxValue)
        {
            var filtr_conts = new VectorOfVectorOfPoint();
            for (int i = 0; i < contours.Size; i++)
            {
                if (CvInvoke.ContourArea(contours[i])> min_size && CvInvoke.ContourArea(contours[i]) < max_size)
                {
                    filtr_conts.Push(contours[i]);
                }
            }
            return filtr_conts;
        }

        static double dist(System.Drawing.PointF p1, System.Drawing.PointF p2)
        {
            return Math.Sqrt((p1.X-p2.X)* (p1.X - p2.X)+ (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
        static double distContours(VectorOfPoint c1, VectorOfPoint c2)
        {
            return dist(findCentrCont(c1), findCentrCont(c2));
        }
        static VectorOfVectorOfPoint sameContours_cv(VectorOfVectorOfPoint contours)
        {
            var conts_fil = new VectorOfVectorOfPoint();
            var conts_cv = ContourCV.contourCVs(contours);
            Console.WriteLine(contours.Size);
            int mult = 4;
            var im = new Image<Gray, byte>(mult*contours.Size, mult * contours.Size);
            for (int i = 0; i < contours.Size; i++)
            {
                bool add = false;
                for (int j = 0; j < contours.Size; j++)
                {
                    if(i!=j)
                    {
                        
                        double match = CvInvoke.MatchShapes(conts_cv[i].cont_orig, conts_cv[j].cont_orig, ContoursMatchType.I1);
                        //Console.WriteLine(i + " " + j + " " + match);
                        match *= 100;
                        if (match<5 && match!=0)
                        {
                            add = true;
                            
                        }
                        if(match > 255)
                        {
                            match = 255;
                        }
                        for(int k1 = 0; k1 < mult; k1++)
                        {
                            for (int k2 = 0; k2 < mult; k2++)
                            {
                                im.Data[mult * j+k1, mult * i + k2, 0] = (byte)(255 - match);
                            }
                        }
                           
                        
                    }
                    
                }
                if (add)
                {
                    conts_fil.Push(contours[i]);
                }
            }

            CvInvoke.Imshow("match", im);
            return conts_fil;
        }
        static double sumHuMom(VectorOfPoint cont)
        {
            var M = CvInvoke.Moments(cont);
            var ms = CvInvoke.HuMoments(M);
            
            var mass_2 = 0d;
            for (int j = 0; j < ms.Length; j++)
            {
                mass_2 += ms[j];
            }
            return mass_2;
        }
        public static VectorOfVectorOfPoint sameContours(VectorOfVectorOfPoint contours,double err_form= 0.095, double err_area= 0.85)
        {
            var clasters = new List<VectorOfVectorOfPoint>();
                                // Console.WriteLine("------------------------");
            for (int i=0; i< contours.Size;i++)
            {

                if (i==0)
                {
                    clasters.Add(new VectorOfVectorOfPoint(new VectorOfPoint[] { contours[i] }));
                }
                else
                {
                    bool added = false;
                    for(int j=0; j<clasters.Count;j++)
                    {
                        var area_cur  = CvInvoke.ContourArea(contours[i]);

                        var area_clast = areaAver(clasters[j]);

                        //Console.WriteLine(" i: " + i + " j: " + j + " area_clast: " + area_clast + " perim_clast: " + perim_clast + " area_cur: " + area_cur + " perim_cur: " + perim_cur);
                        if (Math.Abs(sumHuMom(contours[i]) - HuMomAver(clasters[j])) < HuMomAver(clasters[j])* err_form &&
                            Math.Abs(area_cur - area_clast) < area_clast * err_area)
                        {
                            clasters[j].Push(contours[i]);
                            added = true;
                            break;
                        }                       
                    }
                    if(!added)
                    {
                        clasters.Add(new VectorOfVectorOfPoint(new VectorOfPoint[] { contours[i] }));
                    }
                }
            }

            int max = int.MinValue;
            int i_max = 0;
            for (int i=0; i<clasters.Count;i++)
            {
                if(clasters[i].Size>max)
                {
                    i_max = i; 
                    max = clasters[i].Size;
                }
               // Console.WriteLine("clasters[i].Size: "+ clasters[i].Size);
            }           
            if(clasters.Count==0)
            {
                return null;
            }
            return clasters[i_max];
        }

        static List<ContourCV> filterContours(VectorOfVectorOfPoint contours)
        {
            var clasters = new List<List<ContourCV>>();
            var err = 0.65;
            Console.WriteLine("------------------------");
            var histogr_cont = new int[30];
            var histogr_cont_per = new int[30];
            var histogr_cont_area = new int[30];

            for (int i = 0; i < contours.Size; i++)
            {
                var cont = new ContourCV(contours[i]);
                if (i == 0)
                {
                    clasters.Add(new List<ContourCV>(new ContourCV[] { cont }));
                }
                else
                {
                    bool added = false;
                    for (int j = 0; j < clasters.Count; j++)
                    {
                        /*var area_clast = areaAver(clasters[j]);
                        var perim_clast = perimAver(clasters[j]);
                        var fig_clast = area_clast / perim_clast;*/
                        //Console.WriteLine(" i: " + i + " j: " + j + " area_clast: " + area_clast + " perim_clast: " + perim_clast + " area_cur: " + area_cur + " perim_cur: " + perim_cur);

                        if (true)
                        {
                            clasters[j].Add(cont);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        clasters.Add(new List<ContourCV>(new ContourCV[] { cont }));
                    }
                }
            }

            for (int i = 0; i < histogr_cont.Length; i++)
            {
                // Console.WriteLine(i + " " + histogr_cont[i]);
            }
            int max = int.MinValue;
            int i_max = 0;
            //Console.WriteLine("clasters[i].Size______________");
            for (int i = 0; i < clasters.Count; i++)
            {
                /*if(clasters[i].Size>max)
                {
                    max = clasters[i].Size;
                    i_max = i;
                }*/
                if (clasters[i].Count == 84)
                {
                    i_max = i;

                }
                //Console.WriteLine(clasters[i].Size);
            }
            // Console.WriteLine("clasters[i].Size");
            if (clasters.Count == 0)
            {
                return null;
            }
            return clasters[i_max];
        }
        public static System.Drawing.PointF findCentrCont(VectorOfPoint contour)
        {

            var M = CvInvoke.Moments(contour);
            var cX = M.M10 / M.M00;
            var cY = M.M01 / M.M00;
            var p = new System.Drawing.PointF((float)cX, (float)cY);
            return p;
        }
        public static System.Drawing.PointF[] findCentres(VectorOfVectorOfPoint contours)
        {
            if (contours == null)
            {
                return null;
            }
            var ps = new System.Drawing.PointF[contours.Size];
            for (int i = 0; i < contours.Size; i++)
            {
                ps[i] = findCentrCont(contours[i]);
            }
            return ps;
        }
        static double areaAver(VectorOfVectorOfPoint contours)
        {
            double area_sum = 0;
            for(int i=0; i<contours.Size;i++)
            {
                area_sum+= CvInvoke.ContourArea(contours[i]);
            }
            return area_sum / contours.Size;
        }
        static double perimAver(VectorOfVectorOfPoint contours)
        {
            double perim_sum = 0;
            for (int i = 0; i < contours.Size; i++)
            {
                perim_sum += CvInvoke.ArcLength(contours[i], true);
            }
            return perim_sum / contours.Size;
        }
        static double figAver(VectorOfVectorOfPoint contours)
        {
            var areaAv = areaAver(contours);
            var perimAv = perimAver(contours);
            return areaAv / perimAv;
        }
        static double HuMomAver(VectorOfVectorOfPoint contours)
        {
            double humom = 0;
            for (int i = 0; i < contours.Size; i++)
            {
                humom += sumHuMom(contours[i]);
            }
            return humom / contours.Size;
        }
        static Size calcSize(Size size)
        {
            var w = 600;
            var k1 = (double)size.Width / (double)w;
            var w_1 = (int)((double)size.Width / (double)k1);
            var h_1 = (int)((double)size.Height / (double)k1);
            return new Size(w_1, h_1);
        }
        static public Image<Gray, byte> sobel(Image<Gray, byte> im)
        {
            var size_1 = calcSize(im.Size);
            var im_sob =  new Image<Gray, byte>(im.Size);
            im.CopyTo(im_sob);
            //CvInvoke.Resize(im, im_sob, size_1);
            var gray_x = im_sob.Sobel(1, 0, 3);
            var gray_y = im_sob.Sobel(0, 1, 3);

           
            for (int x = 0; x < im_sob.Width; x++)
            {
                for (int y = 0; y < im_sob.Height; y++)
                {
                    var sob = (int)(Math.Abs(gray_x.Data[y, x, 0]) + Math.Abs(gray_y.Data[y, x, 0]));
                    if (sob > 255)
                    {
                        im_sob.Data[y, x, 0] = 255;
                    }
                    else
                    {
                        im_sob.Data[y, x, 0] = (byte)sob;
                    }

                }
            }
            gray_x = null;
            gray_y = null;
            GC.Collect();
            return im_sob;
        }
        
        static System.Drawing.PointF[] orderPoints(System.Drawing.PointF[] ps, Size size_patt)
        {

            var mainDiag = findMainDiag(ps);
            var step = mainDiag[2];
            var angle = calcAngleX(ps[mainDiag[0]], ps[mainDiag[1]]);//!!!add if dx small rot Y
            var additDiag = findAdditDiag(ps, angle);

            var starts = findStarts(ps, mainDiag, additDiag);


            var inds_ord = findAllLines(ps, starts, step);
            var ind_size = ordBySize(inds_ord, size_patt);
            if(ind_size == null)
            {
               // Console.WriteLine("ind_size NULL");
                return null;
            }

            return arrFromP(ps, ind_size);
        }
        static public System.Drawing.PointF[] findGab(System.Drawing.PointF[] ps)
        {
            var mainDiag = findMainDiag(ps);
            var step = mainDiag[2];
            var angle = calcAngleX(ps[mainDiag[0]], ps[mainDiag[1]]);//!!!add if dx small rot Y
            var additDiag = findAdditDiag(ps, angle);

            return new System.Drawing.PointF[]
            {
                ps[mainDiag[0]], ps[additDiag[0]],ps[mainDiag[1]], ps[additDiag[1]]
            };
        }
        static int[][] ordBySize(int[][] inds, Size size)
        {
            if(inds==null)
            {
                //Console.WriteLine("inds NULL");
                return null;
            }    
            if(inds.Length!=size.Height)
            {
                return transpose(inds);
            }
            else
            {
                return inds;
            }
        }

        static int[][] transpose(int[][] inds)
        {
            if(!checkTransp(inds))
            {
               // Console.WriteLine("TRANSP FALSE");
                return inds;
            }
            var inds_tr = new int[inds[0].Length][];
            for(int i=0; i<inds_tr.Length; i++)
            {
                inds_tr[i] = new int[inds.Length];
            }
            for(int i=0; i<inds.Length;i++)
            {
                for (int j = 0; j < inds[i].Length; j++)
                {
                    inds_tr[j][i] = inds[i][j];
                }
            }
            return inds_tr;
        }
        static bool checkTransp(int[][] matr)
        {
            if (matr == null){
                return false;
            }
            if(matr.Length==0)
            { 
                return false;
            }
            if(matr[0]==null)
            {
                return false;
            }
            var len1 = matr[0].Length;
            if(len1==0)
            {
                return false;
            }
            for(int i=0; i<matr.Length;i++)
            {
                if(matr[i]==null)
                {
                    return false;
                }
                if(matr[i].Length!=len1)
                {
                    return false;
                }
            }
            return true;
        }
        static int[] findMainDiag(System.Drawing.PointF[] ps)
        {
           // Console.WriteLine("P_LEN : " + ps.Length);
            var max = float.MinValue;
            var min = float.MaxValue;
            int ind1 = 0;
            int ind2 = 0;
            for (int i=0; i<ps.Length;i++)
            {
                for (int j = i+1; j < ps.Length; j++)
                {
                    var dist = distSq(ps[i], ps[j]);
                    if(dist>max)
                    {
                        max = dist;
                        ind1 = i;
                        ind2 = j;
                    }
                    if (dist < min)
                    {
                        min = dist;
                      //  Console.WriteLine("min: "+min);
                    }
                }
            }
            return new int[] { ind1, ind2,(int)min };
        }

        static int[] findAdditDiag(System.Drawing.PointF[]  ps,double angle)
         {
            var matAffMatr = new Matrix<double>(2,3);
            CvInvoke.GetRotationMatrix2D(new System.Drawing.PointF(0, 0), angle, 1, matAffMatr);
            var ps_rot = UtilOpenCV.transfAffine(ps, matAffMatr);            
            return findAdditFromRot(ps_rot);
         }
         
        static int[] findLinePoints(System.Drawing.PointF[] ps, int p1, int p2, float min)
         {
            var angle = calcAngleX(ps[p1], ps[p2]);
            var matAffMatr = new Matrix<double>(2, 3);
            CvInvoke.GetRotationMatrix2D(new System.Drawing.PointF(0, 0), angle, 1, matAffMatr);
            var ps_rot = UtilOpenCV.transfAffine(ps, matAffMatr);
            //Console.WriteLine("y1: " + ps_rot[p1].Y+ " y2: " + ps_rot[p2].Y);
            return findLineFromRot(ps_rot, ps_rot[p1].Y, min);
        }
         
        static int[][] findStarts(System.Drawing.PointF[] ps, int[] main, int[] addit)
        {
            int st_line1 = main[0];
            int st_line2 = addit[1];
            if (ps[main[0]].Y<ps[main[1]].Y)
            {
                st_line1 = main[0];
                st_line2 = addit[1];
            }
            else
            {
                st_line1 = addit[0];
                st_line2 = main[1];
            }
            var line1 = findLinePoints(ps, main[0], addit[0],main[2]);
            var line1_sort = sortLine(ps, line1, st_line1);

            var line2 = findLinePoints(ps,  main[1], addit[1], main[2]);
            var line2_sort = sortLine(ps, line2, st_line2);

           
            return new int[][] { line1_sort ,  line2_sort };
        }

        static int[][] findAllLines(System.Drawing.PointF[] ps, int[][] starts, float min)
        {
            if (starts == null)
            {
                //Console.WriteLine("starts NULL");
                return null;
            }
            if (starts[0] == null || starts[1] == null)
            {
                //Console.WriteLine("starts[0] == null || starts[1] == null");
                return null;
            }
            if(starts[0].Length!=starts[1].Length)
            {
                //Console.WriteLine("starts[0].Length!=starts[1].Length");
                //Console.WriteLine(starts[0].Length+" "+starts[1].Length);
                return null;
            }
            var inds_sort = new List<int[]>();
            var side1 = 0;
            var side2 = 0;
            if(ps[starts[0][0]].X < ps[starts[1][0]].X)
            {
                side1 = 0;
                side2 = 1;
            }
            else
            {
                side1 = 1;
                side2 = 0;
            }
            for (int i=0; i< starts[0].Length;i++)
            {
                var line = findLinePoints(ps, starts[side1][i], starts[side2][i], min);
                var line_sort = sortLine(ps, line, starts[side1][i]);
                inds_sort.Add(line_sort);
            }
            return inds_sort.ToArray();
        }
        static float distSq(System.Drawing.PointF p1, System.Drawing.PointF p2)
        {
            return (p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y);
        }
        static double calcAngleX(System.Drawing.PointF p1, System.Drawing.PointF p2)
        {
            if(p1.X - p2.X == 0)
            {
                return 1000000000;
            }
            return toDegree(Math.Atan((p1.Y - p2.Y) / (p1.X - p2.X)));
        }
        static double toDegree(double rad)
        {
            return 180 * rad / 3.1415926535;
        }
        static int[] findAdditFromRot(System.Drawing.PointF[] ps)
        {
            var maxY = float.MinValue;
            var minY = float.MaxValue;
            int i_min = 0;
            int i_max = 0;
            for(int i=0; i<ps.Length;i++)
            {
                var y = ps[i].Y;
                if (y>maxY)
                {
                    maxY = y;
                    i_max = i;
                }
                if (y < minY)
                {
                    minY = y;
                    i_min = i;
                }
            }
            return new int[] { i_min, i_max };
        }
        static int[] findLineFromRot(System.Drawing.PointF[] ps, float offset, float _min)
        {
            var min = (float)Math.Sqrt(_min);
            var inds = new List<int>();
            float wind = 0.5f;
            for (int i = 0; i < ps.Length; i++)
            {
                var y = ps[i].Y - offset;
                //Console.WriteLine("y: " + y);
                if(y<wind*min && y>-wind*min)
                {
                    inds.Add(i);
                }
            }
          //  Console.WriteLine("len: " + inds.Count + " min "+min);
            return inds.ToArray();
        }
       
        static int[] sortLine(System.Drawing.PointF[] ps, int[] _inds,int start_ind)
        {
            var inds_sort = new List<int>();
            var inds = new int[_inds.Length];
            _inds.CopyTo(inds, 0);
            var ind_st = getInd(inds, start_ind);
            if(ind_st<0)
            {
                return null;
            }
            var st = _inds[ind_st];
            inds_sort.Add(st);
            inds = removeInd(inds, st);
            for(int i = 0; i< _inds.Length-1;i++ )
            {
               
                var next = findNext(ps, inds, st);
                st = next;
                inds_sort.Add(st);
                inds = removeInd(inds, st);
            }
            return inds_sort.ToArray();
        }
        static int findNext(System.Drawing.PointF[] ps, int[] inds, int ind)
        {
            if(inds.Length==1)
            {
                return inds[0];
            }
            var min = float.MaxValue;
            int ind_min = 0;
          
            for (int i = 0; i < inds.Length; i++)
            {
               
                var dist = distSq(ps[ind], ps[inds[i]]);
      
                if (dist < min)
                {
                    min = dist;
                    ind_min = inds[i];
                    //Console.WriteLine("min: " + min+ " ind_min: " + ind_min);
                }
                
            }
            return ind_min;
        }

        static int getInd(int[] arr, int val)
        {
            for(int i=0; i<arr.Length;i++)
            {
                if(arr[i] == val)
                {
                    return i;
                }
            }
            return int.MinValue;
        }
       
        static int[] removeInd(int[] arr, int val)//special
        {

            var inds = new List<int>();
            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != val)
                {
                    inds.Add(arr[i]);
                }
            }
            return inds.ToArray();
        }

       static System.Drawing.PointF[] arrFromP(System.Drawing.PointF[] ps, int[] ind)
        {
            var ps_arr = new List<System.Drawing.PointF>();
            for(int i=0; i<ind.Length; i++)
            {
                ps_arr.Add(ps[ind[i]]);
            }
            return ps_arr.ToArray();
        }

        static System.Drawing.PointF[] arrFromP(System.Drawing.PointF[] ps, int[][] ind)
        {
            var ps_arr = new List<System.Drawing.PointF>();
            for (int i = 0; i < ind.Length; i++)
            {
                for (int j= 0; j < ind[i].Length; j++)
                {

                    ps_arr.Add(ps[ind[i][j]]);
                }
            }
            return ps_arr.ToArray();
        }
    }
}

   
