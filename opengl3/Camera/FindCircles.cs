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
    public static class FindCircles
    {

        public static Mat findCircles(Mat mat, System.Drawing.PointF[]  corn,Size pattern_size,bool order = true)
        {
            var rec = new Mat();
            var orig = new Mat();
            mat.CopyTo(rec);
            mat.CopyTo(orig);
            var im = rec.ToImage<Gray, byte>();
            var im_blur = im.SmoothGaussian(3);
            var im_sob = sobel(im_blur);
           
            var im_tr = im_sob.ThresholdBinary(new Gray(120), new Gray(255));
            
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(im_tr, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
            var conts = sameContours(contours);
            var cents = findCentres(conts);
            CvInvoke.DrawContours(orig, contours, -1, new MCvScalar(255, 0, 0), 1, LineType.EightConnected);
            CvInvoke.DrawContours(orig, conts, -1, new MCvScalar(0, 255, 0), 2, LineType.EightConnected);
            

            
            //prin.t(cents);
            //prin.t("____________");
            //CvInvoke.Imshow("fnd", mat);
            if(corn==null)
            {
                corn = new System.Drawing.PointF[pattern_size.Width * pattern_size.Height];
            }
            if(order)
            {
                var ps_ord = orderPoints(cents, pattern_size);
                //ps_ord = ps_ord.Reverse().ToArray();

                if (corn != null && ps_ord != null)
                {
                    ps_ord.CopyTo(corn, 0);
                    UtilOpenCV.drawTours(orig, PointF.toPoint(corn), 255, 0, 0, 2);
                }
                else
                {
                    if (ps_ord == null)
                    {
                        Console.WriteLine("ps_ord NULL");
                    }
                }
                UtilOpenCV.drawLines(orig, ps_ord, 0, 0, 255, 2);
                return orig;
            }
            else
            {
                cents.CopyTo(corn, 0);
                UtilOpenCV.drawLines(orig, cents, 0, 255, 0, 2);
                return orig;
            }
            

            
        }
        static System.Drawing.PointF[] findCentres(VectorOfVectorOfPoint contours)
        {
            var ps = new System.Drawing.PointF[contours.Size];
            for (int i = 0; i < contours.Size; i++)
            {
                var M = CvInvoke.Moments(contours[i]);
                var cX = M.M10 / M.M00;
                var cY = M.M01 / M.M00;
                ps[i] = new System.Drawing.PointF((float)cX, (float)cY);
            }
            return ps;
        }
        static VectorOfVectorOfPoint sameContours(VectorOfVectorOfPoint contours)
        {
            var clasters = new List<VectorOfVectorOfPoint>();
            var err = 0.65;
            for(int i=0; i< contours.Size;i++)
            {
                var area_cur = CvInvoke.ContourArea(contours[i]);
                var perim_cur = CvInvoke.ArcLength(contours[i], true);
                var fig_cur = area_cur / perim_cur;
                if(i==0)
                {
                    clasters.Add(new VectorOfVectorOfPoint(new VectorOfPoint[] { contours[i] }));
                }
                else
                {
                    bool added = false;
                    for(int j=0; j<clasters.Count;j++)
                    {
                        var area_clast = areaAver(clasters[j]);
                        var perim_clast = perimAver(clasters[j]);
                        var fig_clast = figAver(clasters[j]);
                        // Console.WriteLine("area_clast: " + area_clast + " perim_clast: " + perim_clast + "area_cur: " + area_cur + " perim_cur: " + perim_cur);
                        if ( ( (area_cur>(1-err)*area_clast)  &&  (area_cur < (1 + err) * area_clast) ) 
                            && ((perim_cur > (1 - err) * perim_clast) && (perim_cur < (1 + err) * perim_clast))
                            && ((fig_cur > (1 - err) * fig_clast) && (fig_cur < (1 + err) * fig_clast)))
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
            //Console.WriteLine("clasters[i].Size______________");
            for (int i=0; i<clasters.Count;i++)
            {
                if(clasters[i].Size>max)
                {
                    max = clasters[i].Size;
                    i_max = i;
                }
                if(clasters[i].Size>2)
                {
                    //Console.WriteLine(clasters[i].Size);
                }
               
            }
           // Console.WriteLine("clasters[i].Size");
            return clasters[i_max];
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
            var im_sob = new Image<Gray, byte>(im.Size);
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
                Console.WriteLine("ind_size NULL");
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
                Console.WriteLine("inds NULL");
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
                Console.WriteLine("starts NULL");
                return null;
            }
            if (starts[0] == null || starts[1] == null)
            {
                Console.WriteLine("starts[0] == null || starts[1] == null");
                return null;
            }
            if(starts[0].Length!=starts[1].Length)
            {
                Console.WriteLine("starts[0].Length!=starts[1].Length");
                Console.WriteLine(starts[0].Length+" "+starts[1].Length);
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

   
