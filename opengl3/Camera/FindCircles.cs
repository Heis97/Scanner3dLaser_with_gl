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
        static public Mat sobel_mat(Mat mat,bool simple=false)
        {
            simple = true;
            if (simple)
            {
                var gray_x0 = new Mat();
                var gray_y0 = new Mat();
                CvInvoke.Sobel(mat, gray_x0, DepthType.Cv32F, 1, 0, 3);
                 CvInvoke.Sobel(mat, gray_y0, DepthType.Cv32F, 0, 1, 3);
                  CvInvoke.ConvertScaleAbs(gray_x0, gray_x0, 1, 0);
                CvInvoke.ConvertScaleAbs(gray_y0, gray_y0, 1, 0);
                return gray_x0 + gray_y0;
            }
            var gray_x = mat.Clone();
            var gray_y = mat.Clone(); 
            var gray_xn = mat.Clone();
            var gray_yn = mat.Clone();

            var gray_xy_r = mat.Clone();
            var gray_xy_rn = mat.Clone();
            var gray_xy_l = mat.Clone();
            var gray_xy_ln = mat.Clone();

            var med = 10;
            var min = 3;

            var med_xy = 14;
            var min_xy = 5;
            
            Point anchor = new Point(-1, -1);
            Matrix<float> kernel_x = new Matrix<float>(new float[3, 3] { 
                { -min, 0f, min }, 
                { - med, 0f, med }, 
                { -min, 0f, min } });
            Matrix<float> kernel_y = new Matrix<float>(new float[3, 3] {
                { min, med, min },
                { 0f, 0f, 0f },
                { -min, -med, -min} });

            Matrix<float> kernel_xn = new Matrix<float>(new float[3, 3] {
                { min, 0f, -min },
                { med, 0f, -med },
                { min, 0f, -min } });

            Matrix<float> kernel_yn = new Matrix<float>(new float[3, 3] {
                { -min, -med, -min },
                { 0f, 0f, 0f },
                { min, med, min} });

            Matrix<float> kernel_xy_r = new Matrix<float>(new float[3, 3] {
                { 0, min_xy, med_xy },
                { -min_xy, 0f, min_xy },
                { -med_xy, -min_xy, 0} });

            Matrix<float> kernel_xy_rn = new Matrix<float>(new float[3, 3] {
                { 0, -min_xy,-med_xy },
                { min_xy, 0f, -min_xy },
                { med_xy, min_xy, 0} });

            Matrix<float> kernel_xy_l = new Matrix<float>(new float[3, 3] {
                {  med_xy, min_xy, 0 },
                { min_xy, 0f, -min_xy },
                { 0, -min_xy,  -med_xy } });

            Matrix<float> kernel_xy_ln = new Matrix<float>(new float[3, 3] {
                {  -med_xy,-min_xy, 0 },
                { -min_xy, 0f, min_xy },
                { 0, min_xy,  med_xy } });
            CvInvoke.Filter2D(mat, gray_yn, kernel_yn, anchor);
            CvInvoke.Filter2D(mat, gray_xn, kernel_xn, anchor);
            CvInvoke.Filter2D(mat, gray_x, kernel_x, anchor);
            CvInvoke.Filter2D(mat, gray_y, kernel_y, anchor);

            CvInvoke.Filter2D(mat, gray_xy_r, kernel_xy_r, anchor);
            CvInvoke.Filter2D(mat, gray_xy_rn, kernel_xy_rn, anchor);
            CvInvoke.Filter2D(mat, gray_xy_l, kernel_xy_l, anchor);
            CvInvoke.Filter2D(mat, gray_xy_ln, kernel_xy_ln, anchor);

            //  CvInvoke.ConvertScaleAbs(gray_x, gray_x, 1, 0);
            //CvInvoke.ConvertScaleAbs(gray_y, gray_y, 1, 0);
            return 0.2 * gray_x + 0.2 * gray_y + 0.2 * gray_xn + 0.2 * gray_yn
                +0.2 * gray_xy_r + 0.2 * gray_xy_rn + 0.2 * gray_xy_l + 0.2 * gray_xy_ln;

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
            bool debug = false;
           // debug = true;
            /* mat.CopyTo(rec);
             mat.CopyTo(orig);
             var im = rec.ToImage<Gray, byte>();
             var im_blur = im.SmoothGaussian(7);
             var im_sob = sobel(im_blur);
             var im_tr = im_sob.ThresholdBinary(new Gray(85), new Gray(255));*/

            CvInvoke.CvtColor(mat, im_tr, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(im_tr, im_tr, new Size(7, 7), 3);
            if(debug)
            {
                CvInvoke.Imshow("gauss", im_tr);                
                CvInvoke.WaitKey();
            }
            im_tr = sobel_mat(im_tr);
          
            //CvInvoke.Laplacian(im_tr, im_tr,DepthType.Default);
            if (debug)
            {
               // CvInvoke.Normalize(im_tr, im_tr);
                CvInvoke.Imshow("sobel_d", im_tr);
                CvInvoke.WaitKey();
            }

            CvInvoke.Threshold(im_tr, im_tr,55 , 255, ThresholdType.Binary);
            if (debug)
            {
                CvInvoke.Imshow("im_tr", im_tr);
                CvInvoke.WaitKey();
            }


            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(im_tr, contours, hier, RetrType.Tree, ChainApproxMethod.ChainApproxSimple);
            //var conts = sameContours_cv(contours);
            contours = size_filter(contours,10);
            orig = mat.Clone();
            CvInvoke.DrawContours(orig, contours, -1, new MCvScalar(255, 0, 0), 1, LineType.EightConnected);
            //CvInvoke.DrawContours(orig, conts, -1, new MCvScalar(0, 255, 0), 2, LineType.EightConnected);
            /*Console.WriteLine("find_circ");
            CvInvoke.Imshow("orig_c", orig);
            CvInvoke.WaitKey();*/
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
            //CvInvoke.WaitKey();
            if (conts!=null)
            {
               // CvInvoke.DrawContours(im_tr, conts, -1, new MCvScalar(255, 0, 0), 2, LineType.EightConnected);
            }
           
            //CvInvoke.DrawContours(im_tr, contours, -1, new MCvScalar(255, 0, 255), 1, LineType.EightConnected);
            //CvInvoke.DrawContours(im_tr, conts_sc, -1, new MCvScalar(255, 0, 255), 1, LineType.EightConnected);

            //CvInvoke.Imshow("bin ", im_tr);
            
            counter++;

            if (cents == null)
            {
                Console.WriteLine("find_circ ret null");
                return null;
            }


            //prin.t(cents);
            //prin.t("____________");
            // orig = UtilOpenCV.drawPointsF(orig, cents, 255, 0, 0);
            if (debug)
            CvInvoke.Imshow("fnd", orig);
            CvInvoke.WaitKey();
            corn = new System.Drawing.PointF[pattern_size.Width * pattern_size.Height];
            //UtilOpenCV.drawLines(orig, cents, 0, 255, 0, 2);
            
            if (order)
            {
                orig = UtilOpenCV.drawTours(orig, PointF.toPoint(cents), 255, 0,2);
                //CvInvoke.Imshow("fnd", orig);
                //CvInvoke.WaitKey();
               // UtilOpenCV.drawTours(im_tr, PointF.toPoint(cents), 255, 0, 0, 2);
                var ps_ord = orderPoints(cents, pattern_size);
                /*var ps_ord2 = orderPoints_assym(cents, pattern_size);
                orig = UtilOpenCV.drawPoints_2d(orig,PointF.toSystemPoint_ss_2d( ps_ord2),0, 255,  0);
                CvInvoke.Imshow("fnd", orig);
                CvInvoke.WaitKey();*/
                //ps_ord = ps_ord.Reverse().ToArray();
                if (ps_ord != null && ps_ord.Length<=corn.Length)
                {
                    corn = new System.Drawing.PointF[ps_ord.Length];
                    ps_ord.CopyTo(corn, 0);
                    //Console.WriteLine("cents");
                    UtilOpenCV.drawTours(im_tr, PointF.toPoint(ps_ord), 255, 0, 0, 2);
                    UtilOpenCV.drawTours(im_tr, PointF.toPoint(new System.Drawing.PointF[] { ps_ord[0] }), 0, 255, 255, 10);
                    /*UtilOpenCV.drawTours(im_tr, PointF.toPoint(corn), 255, 0, 0, 2);
                    UtilOpenCV.drawLines(im_tr, corn, 0, 0, 255, 2);
                    CvInvoke.Imshow("circ", im_tr);
                    CvInvoke.WaitKey();*/
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
                return im_tr;
            }
            else
            {
                cents.CopyTo(corn, 0);
                
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
        public static VectorOfVectorOfPoint sameContours(VectorOfVectorOfPoint contours,double err_form= 0.115, double err_area= 0.85)
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
            //Console.WriteLine("mainDiag[0,1] " + " " + mainDiag[0] + " " + mainDiag[1] + " addDiag[0,1] " + " " + additDiag[0] + " " + additDiag[1]);

            var inds_ord = findAllLines(ps, starts, step);
            var ind_size = ordBySize(inds_ord, size_patt);
            if(ind_size == null)
            {
                Console.WriteLine("ind_size NULL");
                return null;
            }
            var arr_def = arrFromP_2(ps, ind_size,size_patt);
            arr_def = arr_zero_to_up(arr_def);
            if(arr_def == null) { return null; }
            return unif_points(arr_def);
        }
        static System.Drawing.PointF[,] orderPoints_assym(System.Drawing.PointF[] ps, Size size_patt)
        {

            var mainDiag = findMainDiag(ps);
            var step = mainDiag[2];
            var angle = calcAngleX(ps[mainDiag[0]], ps[mainDiag[1]]);
            var additDiag = findAdditDiag(ps, angle);
            additDiag[2] = mainDiag[2];
            var inds_ps = new int[size_patt.Width*size_patt.Height][];
            for(int i=0; i < inds_ps.Length; i++)
            {
                inds_ps[i] = new int[] { -1, -1 };
            }

            var starts1 = findStarts_asym(ps, mainDiag, additDiag);
            var inds_ord1 = findAllLines(ps, starts1, step);
            var ind_size1 = ordBySize(inds_ord1, size_patt);
            inds_ps = set_inds_from_lines(inds_ps, inds_ord1, 0);
            if (ind_size1 == null)
            {
                //Console.WriteLine("ind_size NULL");
                return null;
            }
            mainDiag = new int[] { mainDiag[1], mainDiag[0], mainDiag[2] };
            //additDiag = new int[] { additDiag[1], additDiag[0], mainDiag[2] };
            var starts2 = findStarts_asym(ps, additDiag, mainDiag);
            var inds_ord2 = findAllLines(ps, starts2, step);
            var ind_size2 = ordBySize(inds_ord2, size_patt);
            inds_ps = set_inds_from_lines(inds_ps, inds_ord2,1);
            if (ind_size2 == null)
            {
                //Console.WriteLine("ind_size NULL");
                return null;
            }

            return arrFromP_2d(ps, inds_ps, new Size( inds_ord1.Length, inds_ord2.Length));
        }

        static int[][] set_inds_from_lines(int[][] inds_patt, int[][] inds_lines,int coord)
        {
            for (int i = 0; i < inds_lines.Length; i++)
            {
                for (int j = 0; j < inds_lines[i].Length; j++)
                {
                    var ind_p = inds_lines[i][j];
                    inds_patt[ind_p][coord] = i;
                }
            }

            return inds_patt;
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
                Console.WriteLine("TRANSP FALSE");
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
                        //Console.WriteLine("min: "+min);
                    }
                }
            }
            return new int[] { ind1, ind2,(int)Math.Sqrt( min) };
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
        static int[][] findStarts_asym(System.Drawing.PointF[] ps, int[] main, int[] addit)
        {
            int st_line1 = main[0];
            int st_line2 = addit[1];


            var line1 = findLinePoints(ps, main[0], addit[0], main[2]);
            var line1_sort = sortLine(ps, line1, st_line1);

            var line2 = findLinePoints(ps, main[1], addit[1], main[2]);
            var line2_sort = sortLine(ps, line2, st_line2);


            return new int[][] { line1_sort, line2_sort };
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
        static double calcAngleX_o(System.Drawing.PointF p1, System.Drawing.PointF p2)
        {
            if( Math.Abs( p1.X - p2.X) <= 0.001)
            {
                return toDegree(Math.PI/2);
            }
            return toDegree(Math.Atan((p1.Y - p2.Y) / (p1.X - p2.X)));
        }
        static double calcAngleX(System.Drawing.PointF p1, System.Drawing.PointF p2)
        {
            var v1 = new Point3d_GL(p1.X - p2.X, p1.Y - p2.Y);
            var v2 = new Point3d_GL(1);
            var angle = Math.Sign(p1.Y - p2.Y)* Math.Acos( v1 ^ v2);
            return toDegree(angle);
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
            return new int[] { i_min, i_max,0 };
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
                if (ind[i]!=null)
                {
                    for (int j = 0; j < ind[i].Length; j++)
                    {

                        ps_arr.Add(ps[ind[i][j]]);
                    }
                }
                
            }


            return ps_arr.ToArray();
        }

        static System.Drawing.PointF[,] arrFromP_2(System.Drawing.PointF[] ps, int[][] ind, Size size)
        {
            if (ind[0] == null) return null ;
            var ps_arr = new System.Drawing.PointF[ind.Length, ind[0].Length];
            for (int i = 0; i < ind.Length; i++)
            {
                if (ind[i] != null)
                {
                    for (int j = 0; j < ind[i].Length; j++)
                    {
                        ps_arr[i,j] = ps[ind[i][j]];
                    }
                }
            }
            return ps_arr;
        }

        static System.Drawing.PointF[,] arr_zero_to_up(System.Drawing.PointF[,] ps)
        {
            float k = 0.3f;
            if(ps == null) return null ;
            var ps_zu = new System.Drawing.PointF[ps.GetLength(0), ps.GetLength(1)];
            var psc = new System.Drawing.PointF[] { ps[0, 0], ps[ps.GetLength(0) - 1, 0], ps[0, ps.GetLength(1) - 1], ps[ps.GetLength(0) - 1, ps.GetLength(1) - 1] };
            var y_min = float.PositiveInfinity;
            var i_min = 0;
            for(int i = 0; i < psc.Length; i++)
            {
                if(psc[i].Y + k*psc[i].X < y_min)
                {
                    y_min = psc[i].Y + k * psc[i].X;
                    i_min = i;
                }
            }
            if (i_min == 0)
            {
                for (int i = 0; i < ps_zu.GetLength(0); i++)
                {
                    for (int j = 0; j < ps_zu.GetLength(1); j++)
                    {
                        ps_zu[i, j] = ps[i, j];
                    }
                }
            }
            else if (i_min==1)
            {
                for(int i = 0; i < ps_zu.GetLength(0); i++)
                {
                    for (int j = 0; j < ps_zu.GetLength(1); j++)
                    {
                        ps_zu[i, j] = ps[ps.GetLength(0) - 1 - i, j];
                    }
                }
            }
            else if (i_min == 2)
            {
                for (int i = 0; i < ps_zu.GetLength(0); i++)
                {
                    for (int j = 0; j < ps_zu.GetLength(1); j++)
                    {
                        ps_zu[i, j] = ps[ i, ps.GetLength(1) - 1 - j];
                    }
                }
            }
            else if (i_min == 3)
            {
                for (int i = 0; i < ps_zu.GetLength(0); i++)
                {
                    for (int j = 0; j < ps_zu.GetLength(1); j++)
                    {
                        ps_zu[i, j] = ps[ps.GetLength(0)-1 - i, ps.GetLength(1) - 1 - j];
                    }
                }
            }
            return ps_zu;
        }
        static System.Drawing.PointF[] unif_points(System.Drawing.PointF[,] ps)
        {
            var ps_un = new List<System.Drawing.PointF>();
            for (int i = 0; i < ps.GetLength(0); i++)
            {
                for (int j = 0; j < ps.GetLength(1); j++)
                {
                    ps_un .Add(ps[ i, j]);
                }
            }
            return ps_un.ToArray();
        }

        static System.Drawing.PointF[,] arrFromP_2d(System.Drawing.PointF[] ps, int[][] ind, Size size)
        {
            var ps_arr = new System.Drawing.PointF[size.Width, size.Height];
            int k = 0;
            for (int i = 0; i < size.Width; i++)
            {
                for (int j = 0; j < size.Height; j++)
                {

                    if(k<ps.Length)
                    {
                        ps_arr[ind[k][0], ind[k][1]] = ps[k]; k++;
                    }
                    
                }
            }
            return ps_arr;
        }
        static System.Drawing.PointF[,] arr2FromP(System.Drawing.PointF[] ps, int[][] ind)
        {
            var ps_arr2 = new System.Drawing.PointF[ind.Length,ind[0].Length];
            var ps_list = new List<System.Drawing.PointF>();
            var dist = 0d;
            for (int i = 0; i < ind.Length; i++)
            {
                for (int j = 0; j < ind[i].Length; j++)
                {
                    
                    ps_list.Add(ps[ind[i][j]]);
                    //dist = distSq(ps_list[ps_list.Count - 1], ps_list[ps_list.Count - 2]);
                    ps_arr2 [i,j] = ps[ind[i][j]];
                }
            }
            return ps_arr2;
        }
    }
}

   
