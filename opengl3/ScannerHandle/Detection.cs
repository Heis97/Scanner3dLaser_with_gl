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
    public static class Detection
    {
        public static PointF[] detectLineSensor(Mat mat, int wind = 5)
        {
            var mat_data = mat.Clone();
            CvInvoke.CvtColor(mat_data, mat_data, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(mat_data, mat_data, new Size(5,5), -1);
            var data = (byte[,])mat_data.GetData();
            
            int j = (int)(data.GetLength(0)/2);
            float br = 0;
            int i_m = 0;
            var ps_b = new List<PointF>();
            for (int i = 0; i < data.GetLength(1); i++)
            {
                
                int br_sum = 0;
                for (int k = -wind; k < wind ; k++)
                    br_sum += data[j + k, i];
                float br_cur = br_sum/(wind*2);
                ps_b.Add(new PointF(i, 480 - br_cur));
                if (br_cur > br)
                {
                    br = br_cur;
                    i_m = i;
                }
            }
            var ps_imp = ps_b.GetRange(i_m - 3, 7).ToArray();
            var vals_regr = new List<double[]>();
            for (int i = 0; i < ps_imp.Length; i++)
            {
                vals_regr.Add(new double[] { ps_imp[i].X, ps_imp[i].Y });
            }

            var koef = Regression.regression(vals_regr.ToArray(), 2);
            var a = koef[2];
            var b = koef[1];
            var x_cent = (-b / (2 * a));

            var ps = new PointF[] { new PointF(x_cent, j)};

            /*return Regression.paintRegression(mat, vals_regr.ToArray(),2);

            var color = UtilOpenCV.drawPointsF(mat, ps_imp, 0, 0, 255);
            color = UtilOpenCV.drawPointsF(color, ps, 255, 0, 0);
            Console.WriteLine(ps[0].X);
            return color;*/
            return ps;
        }

        public static PointF[] detectLineSensor_old(Mat mat, int wind = 20)
        {
            var mat_data = mat.Clone();
            var data = (byte[,,])mat_data.GetData();

            int j = (int)(data.GetLength(0) / 2);
            int br = 0;
            int i_m = 0;
            for (int i = wind; i < data.GetLength(1) - wind; i++)
            {
                /* int br_cur = 0;

                 for (int k = -wind; k<wind;k++)
                 {
                     br_cur += data[j, i+k, 0] + data[j, i + k, 1] + data[j, i + k, 2];
                 }*/
                int br_cur = data[j, i, 0] + data[j, i , 1] + data[j, i , 2];
                if (br_cur > br)
                {
                    br = br_cur;
                    i_m = i;
                }
            }
             return new PointF[] { new PointF(i_m, j) };
        }
        public static PointF[] detectLine(Mat mat, int wind=12)
        {
            var ps = new PointF[mat.Width];
            var data = (byte[,,])mat.GetData();
            //Console.WriteLine("_________________________________________________");
            for (int i =0;i < data.GetLength(1);i++)
            {
                //Console.WriteLine("_________________________________________________");
                float br_max = -512;
                float bl_max = -512;
                int j_max = 10;
                int jr_max = 0;
                int jl_max = 0;
                for (int j = 1; j < data.GetLength(0); j++)
                {
                    //float sum = (float)Math.Max(data[j, i, 2], Math.Max(data[j, i, 1], data[j, i, 0]));
                    float br_cur = -512;
                    float bl_cur = -512;
                    //br_cur = (float)data[j, i, 2]  - 0.5f*(float)data[j, i, 1] - 0.5f*(float)data[j, i, 0];
                    var br_cur_y = (float)data[j, i, 2]/ (float)data[j-1, i, 2];
                    var br_cur_x = (float)data[j, i, 2] / (float)data[j, i, 1];
                    br_cur = br_cur_x* br_cur_x + br_cur_y* br_cur_y;

                    var bl_cur_y = (float)data[j-1, i, 2] / (float)data[j, i, 2];
                    var bl_cur_x = (float)data[j, i, 2] / (float)data[j, i, 1];
                    bl_cur = bl_cur_x * bl_cur_x + bl_cur_y * bl_cur_y;
                    //br_cur = (float)data[j, i, 2];
                    //var br_r = (float)data[j, i, 2];
                    //if (i== 480)
                        //Console.WriteLine(br_max + "| " + data[j, i, 0] + " " + data[j, i, 1] + " " + data[j, i, 2]);
                    if (br_cur>br_max && data[j, i, 2]>220)
                    {                        
                        br_max = br_cur;
                        jr_max = j; 
                    }

                    if (bl_cur > bl_max && data[j, i, 2] > 220)
                    {
                        bl_max = bl_cur;
                        jl_max = j;
                    }
                }

                if( Math.Abs(jl_max - jr_max)<wind)
                {
                    j_max = jl_max;
                }
                if(j_max!=0)
                {
                    var col = new int[wind,2];
                    for (int k = 0; k < wind; k++)
                    {
                        var j = k + j_max - wind / 2;
                        if(j<0)
                        {
                            j = 0;
                        }
                        if(j>= data.GetLength(0))
                        {
                            j = data.GetLength(0) - 1;
                        }
                        col[k, 0] = j;
                        col[k, 1] = (int)data[j, i, 0] + (int)data[j, i, 1] + (int)data[j, i, 2];
                    }
                    //var j_max_2 = centerOfMass(col);
                    var j_max_2 = localMax(col);
                    ps[i] = new PointF(i, j_max_2);

                }
                else
                {
                   ps[i] = PointF.notExistP();
                }
            }

            var ps_med = medianFilter(ps);
            return ps;
        }
        
        public static PointF[] detectLineSobel(Mat mat, int wind = 12,int thr = 10)
        {
            var ps = new PointF[mat.Width];
            var im1 = mat.ToImage<Gray, Byte>();
            var bin = new Mat();
            var sob = new Mat();
            var gray = im1.Mat;
            CvInvoke.Sobel(gray, sob, DepthType.Cv8U, 0, 1);
            //CvInvoke.Threshold(gray, bin, 80, 255, ThresholdType.Binary);
            var data_sob = (byte[,])sob.GetData();
            var data = (byte[,])gray.GetData();
            for (int i = 0; i < data.GetLength(1); i++)
            {
                int j_max = 0;
                for (int j = wind; j < data.GetLength(0)- wind; j++)
                {                 
                    if (data_sob[j,i]>50)
                    {
                        if(Math.Abs((int)data[j+wind, i]-(int)data[j-wind, i])< thr)
                        {
                            j_max = j;
                        }
                        
                    }
                }

                if (j_max != 0)
                {

                    ps[i] = new PointF(i, j_max);
                }
                else
                {
                    ps[i] = PointF.notExistP();
                }
            }

            var ps_med = medianFilter(ps);

            return ps;
        }

        public static PointF[] detectLineDiff_2(Mat _mat, int wind = 3,float board = 0.05f,bool reverse = false)
        {
            var mat = _mat.Clone();
            
            if(reverse)
            {
                CvInvoke.Rotate(_mat, mat, RotateFlags.Rotate90Clockwise);
            }
            else
            {
                CvInvoke.Rotate(_mat, mat, RotateFlags.Rotate90CounterClockwise);
            }

            var ps = new PointF[mat.Width];
            var ps_list = new List<PointF>();
            var rgb = mat.Split();
            var fr = new Mat();
            var fg = new Mat();
            var fb = new Mat();
            rgb[0].ConvertTo(fr, DepthType.Cv32F);
            rgb[1].ConvertTo(fg, DepthType.Cv32F);
            rgb[2].ConvertTo(fb, DepthType.Cv32F);


            var matAll = (fr + fg + fb);
            //var matAll = (rgb[0]/4 + rgb[1] / 4 + rgb[2] / 4);
            //CvInvoke.Normalize(im1, im1, 255, 0);
            //var bin = new Mat();
            // var sob = new Mat();
            // var gray = new Mat();
            //var erros = new Mat();

            var kern3 = new Matrix<float>(new float[,] { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } });

            var kern5 = new Matrix<float>(new float[,] { { 1, 1, 1, 1,1 }, { 1, 1, 1, 1,1 }, { 1, 1, 1,1,1 }, { 1, 1, 1, 1, 1 }, { 1, 1, 1, 1, 1 } });
            CvInvoke.Filter2D(matAll, matAll, kern5, new Point(-1, -1));
            //CvInvoke.Sobel(gray, sob, DepthType.Cv8U, 0, 1);
            //CvInvoke.Threshold(matAll, bin, 80, 255, ThresholdType.Binary);
            //Mat kernel5 = CvInvoke.GetStructuringElement(ElementShape.Rectangle, new Size(5, 5), new Point(1, 1));
            //CvInvoke.MorphologyEx(bin, erros, MorphOp.Erode, kernel5, new Point(-1, -1), 1, BorderType.Constant, new MCvScalar(0));
            
            //var data_sob = (byte[,])sob.GetData();
            var data = (float[,])matAll.GetData();
            var br_max = int.MinValue;
            int start_miss = 0;
            for (int i = (int)(board* data.GetLength(1)); i < data.GetLength(1)- (int)(board * data.GetLength(1)); i++)
            {
                br_max = int.MinValue;
                int j_max = 0;
                bool p_add = false;
                for (int j = wind; j < data.GetLength(0) - wind; j++)
                {
                    int br_cur = (int)data[j, i];
                    /*for(int i_w =0; i_w<wind-1; i_w++)
                    {
                        br_cur += (int)data[j + i_w, i ];
                    }*/
                    if (br_cur > br_max)
                    {
                        br_max = br_cur;
                        j_max = j;
                    }
                }
                if (j_max != 0)
                {
                    var _wind = 3 * wind;
                    var col = new int[_wind, 2];
                    for (int k = 0; k < _wind; k++)
                    {
                        var j = k + j_max - _wind / 2;
                        if (j < 0)
                        {
                            j = 0;
                        }
                        if (j >= data.GetLength(0))
                        {
                            j = data.GetLength(0) - 1;
                        }
                        col[k, 0] = j;
                        col[k, 1] = (int)data[j, i];
                    }
                    var j_max_2 = centerOfMass(col);
                    //var j_max_2 = localMax(col);
                    //ps[i] = new PointF(i, j_max_2);
                    if((int)data[(int)j_max_2, i]>1300 )
                    {

                        ps_list.Add(new PointF(i, j_max_2));
                        p_add = true;
                    }               
                }
                if(!p_add)
                {
                    if (ps_list.Count > 0)
                    {
                        ps_list.Add(ps_list[ps_list.Count - 1]);
                    }
                    else
                    {
                        start_miss++;
                    }
                }

            }
            if (start_miss > 0)
            {
                ps_list.Reverse();
                for (int i = 0; i < start_miss; i++)
                {
                    ps_list.Add(ps_list[ps_list.Count - 1]);
                }
                ps_list.Reverse();
            }
            ps = ps_list.ToArray();
            ps = medianFilter(ps,5,4);
            //medianFilter(ps, 5, 5);
            // gaussFilter(ps);
            //ps = onesFilter(ps, 2);
            //Console.WriteLine(ps.Length);
            //Console.WriteLine(ps.Length);
            GC.Collect();
            if(reverse)
            {
                ps = rotatePointsCounterClockwise(ps, _mat.Size);
            }
            else
            {
                ps = rotatePointsClockwise(ps, _mat.Size);
            }
           

            return ps;
        }

        public static PointF[] detectLineDiff(Mat _mat, int wind = 5, float board = 0.05f, bool reverse = false)
        {
            var mat = _mat.Clone();

            if (reverse)
            {
                CvInvoke.Rotate(_mat, mat, RotateFlags.Rotate90Clockwise);
            }
            else
            {
                CvInvoke.Rotate(_mat, mat, RotateFlags.Rotate90CounterClockwise);
            }

            var ps = new PointF[mat.Width];
            var ps_list = new List<PointF>();

            CvInvoke.CvtColor(mat, mat, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(mat, mat, new Size(5, 5), -1);

            var data = (byte[,])mat.GetData();
            var ps_arr_j = new PointF[data.GetLength(0)];
            for (int i = 0; i < ps_arr_j.Length; i++) ps_arr_j[i] = PointF.notExistP();
            int add_count = 0;
            for (int i = (int)(board * data.GetLength(1)); i < data.GetLength(1) - (int)(board * data.GetLength(1)); i++)
            {
                bool p_add = false;
                var br_max = int.MinValue;
                int j_max = 0;
                
                for (int j = 0; j < data.GetLength(0); j++)
                {
                    int br_cur = (int)data[j, i];
                    /*for(int i_w =0; i_w<wind-1; i_w++)
                    {
                        br_cur += (int)data[j + i_w, i ];
                    }*/
                    if (br_cur > br_max)
                    {
                        br_max = br_cur;
                        j_max = j;
                    }
                    ps_arr_j[j] = new PointF(j, br_cur);
                }

                var ps_list_j = ps_arr_j.ToList();
                
                if (j_max < wind) j_max = wind;
                if (j_max > data.GetLength(0) - wind - 1) j_max = data.GetLength(0) - wind - 1;

                var ps_imp = ps_list_j.GetRange(j_max - wind, 2 * wind + 1).ToArray();
                var vals_regr = new List<double[]>();
                for (int k1 = 0; k1 < ps_imp.Length; k1++)
                {
                    vals_regr.Add(new double[] { ps_imp[k1].X, ps_imp[k1].Y });
                    
                }

                var koef = Regression.regression(vals_regr.ToArray(), 2);
                var a = koef[2];
                var b = koef[1];
                var j_max_2 = (-b / (2 * a));
                if (j_max_2 > 0 && j_max_2 < data.GetLength(0))
                {
                    if (data[(int)j_max_2, i] > 10)
                    {
                        ps_list.Add(new PointF(i, j_max_2));
                        p_add = true;
                        add_count++;
                    }
                }

                if(!p_add)
                {
                    ps_list.Add(PointF.notExistP());                     
                }

                

            }
            if (add_count < 5) return null;
            
            ps = ps_list.ToArray();
            ps = connectPoints(ps);
            ps = medianFilter(ps,5,5);

            GC.Collect();
            if (reverse)
            {
                ps = rotatePointsCounterClockwise(ps, _mat.Size);
            }
            else
            {
                ps = rotatePointsClockwise(ps, _mat.Size);
            }
            
            //CvInvoke.Imshow("ds", UtilOpenCV.drawPointsF(_mat, ps, 255, 0, 0));
            return ps;
        }

        static PointF[] rotatePointsClockwise(PointF[] ps,Size size)
        {
            var ps_rot = new PointF[ps.Length];
            for(int i=0; i<ps_rot.Length;i++)
            {
                //ps_rot[i] = new PointF(size.Width - ps[i].Y,size.Height- ps[i].X);
                ps_rot[i] = new PointF(size.Width - ps[i].Y,  ps[i].X);
            }
            return ps_rot;
        }

        static PointF[] rotatePointsCounterClockwise(PointF[] ps, Size size)
        {
            var ps_rot = new PointF[ps.Length];
            for (int i = 0; i < ps_rot.Length; i++)
            {
                ps_rot[i] = new PointF( ps[i].Y, size.Height - ps[i].X);
            }
            return ps_rot;
        }
        static public double calcYbetween2Point(double x, PointF p1, PointF p2)
        {
            var dx = p2.X - p1.X;
            var dy = p2.Y - p1.Y;
            var a = dy / dx;
            var b = p1.Y - a * p1.X;
            return a * x + b;
        }
        static public PointF[] connectPoints(PointF[] inp)
        {
            PointF[] ret = new PointF[inp.Length];
            List<int> indixes = new List<int>();
            for (int i = 0; i < inp.Length; i++)
            {
                if (inp[i].exist)
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
                if (!inp[i].exist)
                {
                    var y1 = (float)inp[indes[next_p - 1]].Y;
                    var y2 = (float)inp[indes[next_p]].Y;
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
                    y = inp[i].Y;
                }
                ret[i] = new PointF(i, y);
            }
            return ret;
        }
        static float centerOfMass(int[,] col)
        {
            int mas_sum = 0;
            int masX_sum = 0;
            for(int i=0; i<col.GetLength(0);i++)
            {
                mas_sum += col[i,1];
                masX_sum+= (col[i,1]* col[i, 0]);
                //Console.WriteLine(col[i, 0] + " "+col[i, 1]);
            }
            
            var mc = (float)masX_sum / (float)mas_sum;
            //Console.WriteLine(mc+"   _________________");
            return mc;
        }

        static PointF centerOfMass(PointF[] col)
        {
            float mas_sum = 0;
            float masX_sum = 0;
            for (int i = 0; i < col.Length; i++)
            {
                mas_sum += col[i].Y;
                masX_sum += (col[i].X * col[i].Y);
                //Console.WriteLine(col[i, 0] + " "+col[i, 1]);
            }

            var mc = masX_sum / mas_sum;
            //Console.WriteLine(mc+"   _________________");
            return new PointF(mc, mas_sum/col.Length);
        }

        static float localMax(int[,] col)
        {
            int masX = 0;
            int i_max = 0;
            for (int i = 0; i < col.GetLength(0); i++)
            {
                if(col[i, 1]>masX)
                {
                    masX = col[i, 1];
                    i_max = i;
                }
            }
            return (float)col[i_max, 0];
        }

        static PointF[] medianFilter(PointF[] ps1, float delt = 5, int wind = 10)
        {
            //var ps_med = (PointF[])ps1.Clone();
            var ps1L = ps1.ToList();
            var ps1L_cop = ps1.ToList();
            for (int i= wind; i< ps1.Length - wind; i++)
            {
                if(Math.Abs( ps1[i].Y - averageYps(ps1L.GetRange(i-wind,2* wind).ToArray()))>delt)                   
                {
                    if(i>0)
                    {
                        ps1L_cop[i] = ps1L_cop[i - 1].Clone();
                    }
                    
                }
            }
            return ps1L_cop.ToArray();
        }

        static PointF[] gaussFilter(PointF[] ps1,  int wind = 3)
        {
            var ps1L = ps1.ToList();
            var ps1ret = (PointF[])ps1.Clone();
            for (int i = wind; i < ps1.Length - wind; i++)
            {
                if (i > 0)
                {
                    ps1ret[i]  = new PointF(i, averageYps(ps1L.GetRange(i - wind, 2 * wind).ToArray()));
                }               
            }
            return ps1ret;
        }

        static float averageYps(PointF[] ps1)
        {
            float avY = 0;
            for (int i = 0; i < ps1.Length; i++)
            {
                avY += ps1[i].Y;
            }
            return avY / ps1.Length;
        }

        static float averageXps(PointF[] ps1)
        {
            float avX = 0;
            for (int i = 0; i < ps1.Length; i++)
            {
                avX += ps1[i].X;
            }
            return avX / ps1.Length;
        }

        static PointF[] onesFilter(PointF[] ps1,float dist)
        {
            var ps_fil = new List<PointF>();
            ps_fil.Add(ps1[0]);
            for (int i=1; i< ps1.Length-1; i++)
            {
                if ((ps1[i]- ps1[i-1]).norm < dist || (ps1[i] - ps1[i + 1]).norm < dist)
                {
                    //Console.WriteLine("(ps1[i] - ps1[i - 1]).norm");
                    //Console.WriteLine(ps1[i]);
                    //Console.WriteLine(ps1[i-1]);
                    ps_fil.Add(ps1[i].Clone());
                }
                
            }
            ps_fil.Add(ps1[ps1.Length-1]);
            return ps_fil.ToArray();
        }

    }
}
