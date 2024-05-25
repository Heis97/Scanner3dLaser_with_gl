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
            CvInvoke.GaussianBlur(mat_data, mat_data, new Size(5, 5), -1);
            var data = (byte[,])mat_data.GetData();

            int j = (int)(data.GetLength(0) / 2);
            float br = 0;
            int i_m = 0;
            var ps_b = new List<PointF>();
            for (int i = 0; i < data.GetLength(1); i++)
            {

                int br_sum = 0;
                for (int k = -wind; k < wind; k++)
                    br_sum += data[j + k, i];
                float br_cur = br_sum / (wind * 2);
                ps_b.Add(new PointF(i, 480 - br_cur));//480
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

            var ps = new PointF[] { new PointF(x_cent, j) };

            /*return Regression.paintRegression(mat, vals_regr.ToArray(),2);

            var color = UtilOpenCV.drawPointsF(mat, ps_imp, 0, 0, 255);
            color = UtilOpenCV.drawPointsF(color, ps, 255, 0, 0);
            Console.WriteLine(ps[0].X);
            return color;*/
            return ps;
        }

        public static Mat findLaserPoint(Mat mat, Mat mat_f)
        {
            var bin = new Mat();
            var gr = new Mat();
            mat -= mat_f;
            CvInvoke.CvtColor(mat, gr, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(gr, gr, new Size(13, 13), 0);

            CvInvoke.Threshold(gr, bin, 20, 255, ThresholdType.Binary);
            var cont = FindCircles.find_max_contour(bin);
            if (cont != null)
            {
                var pf = FindCircles.findCentrCont(cont);
                var p = new System.Drawing.Point((int)pf.X, (int)pf.Y);
                Console.WriteLine(pf.X);
                CvInvoke.DrawMarker(mat, p, new MCvScalar(255, 0, 0), MarkerTypes.TiltedCross, 10);
                CvInvoke.DrawContours(mat, new VectorOfVectorOfPoint(new VectorOfPoint[] { cont }), -1, new MCvScalar(255, 0, 255), 1, LineType.EightConnected);
              
            }
            return mat;
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

        public static PointF[] detectLineDiff_2(Mat _mat, int wind = 3,float board = 0,bool reverse = false)
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

        public static PointF[] detectLineDiff(Mat _mat,
            int wind = 3, float board = 0f,
            bool reverse = false, bool rotate = true,
            bool orig = false)
            {
            if (_mat.GetData() == null) return null;
            var mat = _mat.Clone();
            //var mat = _mat;
            if (rotate)
            {
                if (reverse)
                {
                    CvInvoke.Rotate(_mat, mat, RotateFlags.Rotate90Clockwise);
                }
                else
                {
                    CvInvoke.Rotate(_mat, mat, RotateFlags.Rotate90CounterClockwise);
                }
            }
            

            
            var ps_list = new List<PointF>();
            var ps = ps_list.ToArray();
            CvInvoke.CvtColor(mat, mat, ColorConversion.Bgr2Gray);
            //var mats = mat.Split();
            //mat = mats[0];
            CvInvoke.GaussianBlur(mat, mat, new Size(17, 17), -1);
            //CvInvoke.Imshow("detect_dif",mat);
           // CvInvoke.WaitKey();
            var data = (byte[,])mat.GetData();
            var ps_arr_j = new PointF[data.GetLength(0)];
            for (int i = 0; i < ps_arr_j.Length; i++) ps_arr_j[i] = PointF.notExistP();
            int add_count = 0;

           



            for (int i = (int)(board * data.GetLength(1)); i < data.GetLength(1) - (int)(board * data.GetLength(1)); i++)
            //for (int i = start; i < stop; i+=di)
            {
                bool p_add = false;
                var br_max = int.MinValue;
                int j_max = 0;

                bool reverse_direct = false;

                if (reverse_direct)
                {
                    for (int j = data.GetLength(0)-1; j >= 0; j--)
                    {
                        int br_cur = (int)data[j, i];
                        //for(int i_w =0; i_w<wind-1; i_w++)
                            //br_cur += (int)data[j + i_w, i ];
                        
                        if (br_cur > br_max)
                        {
                            br_max = br_cur;
                            j_max = j;
                        }
                        ps_arr_j[j] = new PointF(j, br_cur);
                    }

                }
                else
                {
                    for (int j = 0; j < data.GetLength(0); j++)
                    {
                        int br_cur = (int)data[j, i];
                       // for(int i_w =0; i_w<wind-1; i_w++)
                            //br_cur += (int)data[j + i_w, i ];
                        
                        if (br_cur > br_max)
                        {
                            br_max = br_cur;
                            j_max = j;
                        }
                        ps_arr_j[j] = new PointF(j, br_cur);
                    }
                }
               

                var ps_list_j = ps_arr_j.ToList();
                
                if (j_max < wind) j_max = wind;
                if (j_max > data.GetLength(0) - wind - 1) j_max = data.GetLength(0) - wind - 1;

                var ps_imp = ps_list_j.GetRange(j_max - wind, 2 * wind + 1).ToArray();
                var vals_regr = new List<double[]>();
                for (int k1 = 0; k1 < ps_imp.Length; k1++)
                {
                    vals_regr.Add(new double[] { ps_imp[k1].X, ps_imp[k1].Y });
                   // Console.WriteLine(ps_imp[k1].Y);
                }
                //Console.WriteLine("___________");
                //for (int k1 = j_max - wind; k1 < j_max + wind; k1++)
                // vals_regr.Add(new double[] { data[k1, i],k1 });

                var threshold = 15;
                var koef = Regression.regression(vals_regr.ToArray(), 2);
                var a = koef[2];
                var b = koef[1];
                var j_max_2 = (-b / (2 * a));

                
                //j_max_2 = j_max;
                if (j_max_2 > 0 && j_max_2 < data.GetLength(0))
                {
                    if (data[(int)j_max_2, i] > threshold)
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
            //ps = PointF.filter_exist(ps);
            ps = filtr_y0_Points(ps);
            if(!orig)
            {                
                //ps = medianFilter_real(ps, 20);
                //ps = connectPoints(ps);
            }
            
            for(int i = 0; i < ps.Length; i++)
            {
               //if(!ps[i].exist)
                    //Console.WriteLine(i);
            }

            GC.Collect();
            //CvInvoke.Imshow("ds", UtilOpenCV.drawPointsF(mat, ps, 255, 255,255,2));
          //  CvInvoke.WaitKey();
            if (rotate)
            {
                if (reverse)
                {
                    ps = rotatePointsCounterClockwise(ps, _mat.Size);
                }
                else
                {
                    ps = rotatePointsClockwise(ps, _mat.Size);
                }
            }
            else
            {
                //ps = (PointF[])ps.Reverse();
            }
            
           // CvInvoke.Imshow("ds_rot", UtilOpenCV.drawPointsF(_mat, ps, 0, 255, 0));
            return ps; 
        }
        public static PointF[] detectLineDiff(Mat _mat,
            ScannerConfig config)
        {

            if (_mat.GetData() == null) return null;
            var mat = _mat.Clone();
            //var mat = _mat;
            if (config.rotate)
            {
                if (config.reverse)
                {
                    CvInvoke.Rotate(_mat, mat, RotateFlags.Rotate90Clockwise);
                }
                else
                {
                    CvInvoke.Rotate(_mat, mat, RotateFlags.Rotate90CounterClockwise);
                }
            }



            var ps_list = new List<PointF>();
            var ps = ps_list.ToArray();
            CvInvoke.CvtColor(mat, mat, ColorConversion.Bgr2Gray);
            //var mats = mat.Split();
            //mat = mats[0];
            CvInvoke.GaussianBlur(mat, mat, new Size(config.gauss_kern, config.gauss_kern), -1);
            //CvInvoke.Imshow("detect_dif",mat);
            //CvInvoke.WaitKey();
            var data = (byte[,])mat.GetData();
            var ps_arr_j = new PointF[data.GetLength(0)];
            for (int i = 0; i < ps_arr_j.Length; i++) ps_arr_j[i] = PointF.notExistP();
            int add_count = 0;





            for (int i = (int)(config.board * data.GetLength(1)); i < data.GetLength(1) - (int)(config.board * data.GetLength(1)); i++)
            //for (int i = start; i < stop; i+=di)
            {
                bool p_add = false;
                var br_max = int.MinValue;
                int j_max = 0;

                bool reverse_direct = false;

                if (reverse_direct)
                {
                    for (int j = data.GetLength(0) - 1; j >= 0; j--)
                    {
                        int br_cur = (int)data[j, i];
                        //for(int i_w =0; i_w<wind-1; i_w++)
                        //br_cur += (int)data[j + i_w, i ];

                        if (br_cur > br_max)
                        {
                            br_max = br_cur;
                            j_max = j;
                        }
                        ps_arr_j[j] = new PointF(j, br_cur);
                    }

                }
                else
                {
                    for (int j = 0; j < data.GetLength(0); j++)
                    {
                        int br_cur = (int)data[j, i];
                        // for(int i_w =0; i_w<wind-1; i_w++)
                        //br_cur += (int)data[j + i_w, i ];

                        if (br_cur > br_max)
                        {
                            br_max = br_cur;
                            j_max = j;
                        }
                        ps_arr_j[j] = new PointF(j, br_cur);
                    }
                }


                var ps_list_j = ps_arr_j.ToList();
                var start_i = j_max - config.wind_regr; var stop_i = j_max + config.wind_regr + 1;
                if (start_i < 0) start_i = 0;
                if (stop_i > data.GetLength(0) - 1) stop_i = data.GetLength(0) - 1;

                var ps_imp = ps_list_j.GetRange(start_i, stop_i - start_i).ToArray();
                var vals_regr = new List<double[]>();
                for (int k1 = 0; k1 < ps_imp.Length; k1++)
                {
                    vals_regr.Add(new double[] { ps_imp[k1].X, ps_imp[k1].Y });
                    // Console.WriteLine(k1 + " " + ps_imp[k1].X + " " + ps_imp[k1].Y);

                }

                //for (int k1 = j_max - wind; k1 < j_max + wind; k1++)
                // vals_regr.Add(new double[] { data[k1, i],k1 });

                var threshold = config.threshold;
                var koef = Regression.regression(vals_regr.ToArray(), 2);
                var a = koef[2];
                var b = koef[1];
                var j_max_2 = (-b / (2 * a));

                //var j_max_2 = j_max;
                if (j_max_2 > 0 && j_max_2 < data.GetLength(0))
                {
                    if (data[(int)j_max_2, i] > threshold)
                    {
                        ps_list.Add(new PointF(i, j_max_2));
                        p_add = true;
                        add_count++;
                    }
                }

                if (!p_add)
                {

                    ps_list.Add(PointF.notExistP());
                }



            }
            if (add_count < 5) return null;
            ps = ps_list.ToArray();
            //ps = PointF.filter_exist(ps);
            if (!config.orig)
            {
                ps = medianFilter_real(ps, 20);
                //ps = connectPoints(ps);
            }

            for (int i = 0; i < ps.Length; i++)
            {
                //if(!ps[i].exist)
                //Console.WriteLine(i);
            }

            GC.Collect();
            // CvInvoke.Imshow("ds", UtilOpenCV.drawPointsF(mat, ps, 255, 255,255));
            //  CvInvoke.WaitKey();
            if (config.rotate)
            {
                if (config.reverse)
                {
                    ps = rotatePointsCounterClockwise(ps, _mat.Size);
                }
                else
                {
                    ps = rotatePointsClockwise(ps, _mat.Size);
                }
            }
            else
            {
                //ps = (PointF[])ps.Reverse();
            }

            // CvInvoke.Imshow("ds_rot", UtilOpenCV.drawPointsF(_mat, ps, 0, 255, 0));
            return ps;
        }
        public static PointF[] detectLineDiff_debug(Mat _mat,
            ScannerConfig config)
        {
            
            if (_mat.GetData() == null) return null;
            var mat = _mat.Clone();
            //var mat = _mat;
            if (config.rotate)
            {
                if (config.reverse)
                {
                    CvInvoke.Rotate(_mat, mat, RotateFlags.Rotate90Clockwise);
                }
                else
                {
                    CvInvoke.Rotate(_mat, mat, RotateFlags.Rotate90CounterClockwise);
                }
            }



            var ps_list = new List<PointF>();
            var ps = ps_list.ToArray();
            var mat_c = mat.Clone();
            var data_rgb = (byte[,,])mat_c.GetData();
            CvInvoke.CvtColor(mat, mat, ColorConversion.Bgr2Gray);
            //var mats = mat.Split();
            //mat = mats[0];
            CvInvoke.GaussianBlur(mat, mat, new Size(config.gauss_kern, config.gauss_kern), -1);
            //CvInvoke.Imshow("detect_dif",mat);
            //CvInvoke.WaitKey();
            var data = (byte[,])mat.GetData();
            var ps_arr_j = new PointF[data.GetLength(0)];
            for (int i = 0; i < ps_arr_j.Length; i++) ps_arr_j[i] = PointF.notExistP();
            int add_count = 0;



                

            for (int i = (int)(config.board * data.GetLength(1)); i < data.GetLength(1) - (int)(config.board * data.GetLength(1)); i++)
            //for (int i = start; i < stop; i+=di)
            {
                bool p_add = false;
                var br_max = int.MinValue;
                int j_max = 0;

                bool reverse_direct = false;

                if (reverse_direct)
                {
                    for (int j = data.GetLength(0) - 1; j >= 0; j--)
                    {
                        int br_cur = (int)data[j, i];
                        //for(int i_w =0; i_w<wind-1; i_w++)
                        //br_cur += (int)data[j + i_w, i ];

                        if (br_cur > br_max)
                        {
                            br_max = br_cur;
                            j_max = j;
                        }
                        ps_arr_j[j] = new PointF(j, br_cur);
                    }

                }
                else
                {
                    for (int j = 0; j < data.GetLength(0); j++)
                    {
                        int br_cur = (int)data[j, i];
                        // for(int i_w =0; i_w<wind-1; i_w++)
                        //br_cur += (int)data[j + i_w, i ];

                        if (br_cur > br_max)
                        {
                            br_max = br_cur;
                            j_max = j;
                        }
                        ps_arr_j[j] = new PointF(j, br_cur);
                    }
                }


                var ps_list_j = ps_arr_j.ToList();
                var wind_full = 40;

                var start_i = j_max - wind_full; var stop_i = j_max + wind_full+1;

                if (start_i < 0) start_i = 0;
                if (stop_i> data.GetLength(0) - 1) stop_i = data.GetLength(0) - 1;
               // Console.WriteLine(start_i + " " + stop_i + " " + (stop_i - start_i) + " ");
                var ps_imp = ps_list_j.GetRange(start_i, stop_i - start_i).ToArray();
                var vals_regr = new List<double[]>();
                for (int k1 = 0; k1 < ps_imp.Length; k1++)
                {
                    vals_regr.Add(new double[] { ps_imp[k1].X, ps_imp[k1].Y });
                   // Console.WriteLine(k1 + " " + ps_imp[k1].X + " " + ps_imp[k1].Y);

                }

                //for (int k1 = j_max - wind; k1 < j_max + wind; k1++)
                // vals_regr.Add(new double[] { data[k1, i],k1 });
                var wind_half = config.wind_regr;
                start_i = j_max - wind_half; stop_i = j_max+wind_half+1;

                if (start_i < 0) start_i =0;
                if (stop_i > data.GetLength(0) - 1) stop_i = data.GetLength(0) - 1;

                ps_imp = ps_list_j.GetRange(start_i, stop_i - start_i).ToArray();
                var for_regr = new List<double[]>();
                for (int k1 = 0; k1 < ps_imp.Length; k1++)
                {
                    for_regr.Add(new double[] { ps_imp[k1].X, ps_imp[k1].Y });
                    // Console.WriteLine(k1 + " " + ps_imp[k1].X + " " + ps_imp[k1].Y);
                }
                var threshold = config.threshold;
                var koef = Regression.regression(for_regr.ToArray(), 2);
                var a = koef[2];
                var b = koef[1];
                var j_max_2 = (-b / (2 * a));

                var vals_regr_re = new List<double[]>();
                
                    

                if (i == data.GetLength(1) / 2)
                {
                    for (int k2 = 0; k2 < vals_regr.Count; k2++)
                    {
                        vals_regr_re.Add(new double[] { vals_regr[k2][0], Regression.calcPolynSolv(koef, vals_regr[k2][0]) });
                        //Console.WriteLine(k2 + " " + vals_regr[k2][0] + " " + Regression.calcPolynSolv(koef, vals_regr[k2][0]));
                    }

                    for (int k2 = 0; k2 < vals_regr.Count; k2++)
                    {
                        var x_c = i;
                        var y_c = (int)vals_regr_re[k2][0];
                        Console.WriteLine(vals_regr[k2][0]
                            + " " + vals_regr_re[k2][1]
                            + " " + vals_regr[k2][1]
                            + " " + data_rgb[y_c, x_c, 0]
                            + " " + data_rgb[y_c, x_c, 1]
                            + " " + data_rgb[y_c, x_c, 2]);
                    }

                    Console.WriteLine("j_max_2: "+j_max_2);
                }


                //var j_max_2 = j_max;
                if (j_max_2 > 0 && j_max_2 < data.GetLength(0))
                {
                    if (data[(int)j_max_2, i] > threshold)
                    {
                        ps_list.Add(new PointF(i, j_max_2));
                        p_add = true;
                        add_count++;
                    }
                }

                if (!p_add)
                {

                    ps_list.Add(PointF.notExistP());
                }



            }
            if (add_count < 5) return null;
            ps = ps_list.ToArray();
            //ps = PointF.filter_exist(ps);
            if (!config.orig)
            {
                ps = medianFilter_real(ps, 20);
                //ps = connectPoints(ps);
            }

            for (int i = 0; i < ps.Length; i++)
            {
                //if(!ps[i].exist)
                //Console.WriteLine(i);
            }

            GC.Collect();
            // CvInvoke.Imshow("ds", UtilOpenCV.drawPointsF(mat, ps, 255, 255,255));
            //  CvInvoke.WaitKey();
            if (config.rotate)
            {
                if (config.reverse)
                {
                    ps = rotatePointsCounterClockwise(ps, _mat.Size);
                }
                else
                {
                    ps = rotatePointsClockwise(ps, _mat.Size);
                }
            }
            else
            {
                //ps = (PointF[])ps.Reverse();
            }

            // CvInvoke.Imshow("ds_rot", UtilOpenCV.drawPointsF(_mat, ps, 0, 255, 0));
            return ps;
        }
        static PointF[] detectLineDiff_up_line(Mat mat)
        {
            var ps = detectLineDiff(mat,5,0.05f,false,true,true);
            var dx =(int) Math.Max(ps[0].X, ps[ps.Length-1].X) + 5;
            var mat_cut = new Mat(mat, new Rectangle(dx, 0, mat.Width - dx, mat.Height));
            var ps_cut = detectLineDiff(mat_cut, 5, 0.05f, false, true, true);
            var clast_cut = x_max_claster(ps_cut, 2);
            var ps_up = clast_cut + new PointF(dx,0);
            //UtilOpenCV.drawPointsF(mat, ps_up, 0, 255, 0);
            //UtilOpenCV.drawPointsF(mat_cut, clast_cut, 0, 255, 0);
            //CvInvoke.Imshow("ps_cut", mat);
           // CvInvoke.Imshow("mat_cut", mat_cut);
           // CvInvoke.WaitKey();
            
            return ps_up;
        }
        public static System.Drawing.PointF[] detectLineDiff_corn_calibr(Mat[] mats)
        {
            var ps = new List<PointF>();
            foreach(var mat in mats)
            {
                var psi = detectLineDiff_up_line(mat);
                if(psi!=null) ps.AddRange(psi);
            }
            var corn = FindCircles.findGab(PointF.toSystemPoint( ps.ToArray()));
            return corn;
        }

        public static PointF[] detectLineDiff_X(Mat _mat,
           int wind = 5, float board = 0.05f,
           bool reverse = false, bool rotate = true,
           bool orig = false)
        {
            var mat = _mat.Clone();
            var ps_list = new List<PointF>();

            CvInvoke.CvtColor(mat, mat, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(mat, mat, new Size(5, 5), -1);

            var data = (byte[,])mat.GetData();
            var ps_arr_j = new PointF[data.GetLength(0)];
            for (int i = 0; i < ps_arr_j.Length; i++) ps_arr_j[i] = PointF.notExistP();
            int add_count = 0;
  
            for (int j = data.GetLength(0)-1; j >= 0; j--)
            {
                bool p_add = false;
                var br_max = int.MinValue;
                int i_max = 0;
                var board_i = (int)(board * data.GetLength(1));
                for (int i = wind; i < data.GetLength(1) - wind; i++)
                {
                    int br_cur = (int)data[j, i];

                    if (br_cur > br_max)
                    {
                        br_max = br_cur;
                        i_max = i;
                    }
                   
                }
                var vals_regr = new List<double[]>();

                for (int k1 = i_max-wind; k1 < i_max+wind; k1++)
                {
                    vals_regr.Add(new double[] { k1, data[j, k1] });
                }

                 var koef = Regression.regression(vals_regr.ToArray(), 2);
                 var a = koef[2];
                 var b = koef[1];
                var i_max_2 = (-b / (2 * a));
                if (i_max_2 > 0 && i_max_2 < data.GetLength(1))
                {
                    if (data[j,(int)i_max_2] > 10)
                    {
                        ps_list.Add(new PointF(i_max_2, j));
                        p_add = true;
                        add_count++;
                    }
                }

                if (!p_add)
                {
                    ps_list.Add(PointF.notExistP());
                }
            }
            if (add_count < 5) return null;

            var ps = ps_list.ToArray();
           /* if (!orig)
            {
                ps = connectPoints(ps);
                ps = medianFilter(ps, 5, 5);
            }*/
            GC.Collect();
           
            return ps;
        }

        static PointF[] rotatePointsClockwise(PointF[] ps,Size size)
        {
            if(ps==null) return null;
            var ps_rot = new PointF[ps.Length];
            for(int i=0; i<ps_rot.Length;i++)
            {
                //ps_rot[i] = new PointF(size.Width - ps[i].Y,size.Height- ps[i].X);
                ps_rot[i] = new PointF(size.Width - ps[i].Y,  ps[i].X);
                //ps_rot[i] = new PointF( ps[i].Y, ps[i].X);
                ps_rot[i].exist = ps[i].exist;
            }
            return ps_rot;
        }

        static PointF[] rotatePointsCounterClockwise(PointF[] ps, Size size)
        {
            var ps_rot = new PointF[ps.Length];
            for (int i = 0; i < ps_rot.Length; i++)
            {
               // ps_rot[i] = new PointF( ps[i].Y,  ps[i].X);
                ps_rot[i] = new PointF(ps[i].Y, size.Height - ps[i].X);
                ps_rot[i].exist = ps[i].exist;
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
            if (indixes.Count < 5) return null;
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
                ret[i] = new PointF(inp[0].X+i, y);
            }
            return ret;
        }
      
        static public PointF[] parall_Points(PointF[] inp)
        {
            /* var ps = (from p in inp
                            orderby p.Y
                            select p).ToArray();*/
            if (inp == null) return null; 
            var ps = (PointF[])inp.Clone();

            var dx = ps[0].X - ps[ps.Length - 1].X;
            var dy = ps[0].Y - ps[ps.Length - 1].Y;
            var k = dx / dy;

            for(int i=0; i<ps.Length;i++)
            {
                var delt = ps[i].Y*k;
                ps[i].X -= delt;
                //ps[i].X *= -1;
            }

            var ps2 = (from p in ps
                  orderby p.X
                  select p).ToArray();
            var x_min = ps2[0].X;
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i].X -= x_min;
            }
            return ps;
        }
        static public int p_in_ps_by_y(PointF[] inp,int y)
        {
            if (inp == null) return 0;
            for(int i = 0; i < inp.Length;i++)
            {
                if((int)inp[i].Y == y)return i;
            }
            return 0;
        }
        static public PointF[] claster_Points(PointF[] inp,int clast,bool max = true)
        {
            var ps = (from p in inp
                      orderby p.X
                  select p).ToArray();
            var x_min = ps[0].X;
            var x_max = ps[ps.Length-1].X;

            var clasters = new List<List<PointF>>();

            var err = 0.3*(x_max-x_min)/(clast-1);


            for (int i = 0; i < inp.Length; i++)
            {

                if (i == 0)
                {
                    var cl = new List<PointF>();
                    cl.Add(inp[i]);
                    clasters.Add(cl);
                }
                else
                {
                    bool added = false;
                    for (int j = 0; j < clasters.Count; j++)
                    {
                        var area_cur = inp[i].X;

                        var area_clast = averageXps(clasters[j].ToArray());
                        //Console.WriteLine(" i: " + i + " j: " + j + " area_clast: " + area_clast + " perim_clast: " + perim_clast + " area_cur: " + area_cur + " perim_cur: " + perim_cur);
                        if ( Math.Abs(area_cur - area_clast) <  err)
                        {
                            clasters[j].Add(inp[i]);
                            added = true;
                            break;
                        }
                    }
                    if (!added)
                    {
                        var cl = new List<PointF>();
                        cl.Add(inp[i]);
                        clasters.Add(cl);
                    }
                }
            }
            var clasters_a = new List<PointF[]>();
            for (int i=0; i<clasters.Count; i++)
            {
                clasters_a.Add(clasters[i].ToArray());
            }

            var clasters_count = (from c in clasters_a
                                  orderby c.Length descending
                                  select c).ToList();
            if(clasters_count.Count<clast)
            {
                clast = clasters_count.Count;
            }
            var big_clasters = clasters_count.GetRange(0, clast);

            var clasters_x_max = (from c in clasters_a
                                  orderby averageXps(c.ToArray()) descending
                                  select c).ToArray();
            if (max) return clasters_x_max[0];
            else return clasters_x_max[clasters_x_max.Length-1];
        }
        static public PointF[] same_y_Points(PointF[] orig, PointF[] ps2)
        {
            var ps3 = new List<PointF>();
            
            for (int j = 0; j < ps2.Length; j++)
            {
                var add = false;
                for (int i = 0; i < orig.Length && !add; i++)
                {
                    if (Math.Abs(orig[i].Y - ps2[j].Y) < 0.01)
                    {
                        ps3.Add(orig[i]);
                        add = true;
                    }
                }
            }
            

            return ps3.ToArray();
        }

        static public PointF[]  x_max_claster(PointF[] ps,int clast_count)
        {
            if (ps == null) return null;
            var paral = parall_Points(filtr_y0_Points(ps));
            var ps_max = claster_Points(paral, clast_count);
            return same_y_Points(ps, ps_max);
        }
        static public PointF[] x_min_claster(PointF[] ps, int clast_count)
        {
            if (ps == null) return null;
            var paral = parall_Points(filtr_y0_Points(ps));
            var ps_max = claster_Points(paral, clast_count,false);
            return same_y_Points(ps, ps_max);
        }

        static public PointF[] filtr_y0_Points(PointF[] ps)
        {
            var ps_f = new List<PointF>();
            if (ps == null) return null;
            for (int i = 0; i < ps.Length; i++)
            {
                if(ps[i].Y!=0)
                {
                    ps_f.Add(ps[i]);
                }
            }
            return ps_f.ToArray();
        }

        static public int[] max_claster_im(PointF[][] ps, int clast_count)
        {
            var dxs = new double[ps.Length];
            for (int i=0; i<ps.Length;i++)
            {
                if (ps[i]!=null)
                {
                    var ps1 = filtr_y0_Points(ps[i]);
                    var paral = parall_Points(ps1);
                    var ps_or = (from p in paral
                                 orderby p.X
                                 select p).ToArray();
                    dxs[i] = Math.Abs(ps_or[0].X - ps_or[ps_or.Length - 1].X);

                }

            }

            var dx_or = dxs[(int)(ps.Length / 2)];
            var ps_cl = new List<int>();
            for (int i = 0; i < ps.Length; i++)
            {
                //Console.WriteLine(dxs[i] + " " + dx_or);
                if(dxs[i] > dx_or*0.8)
                {                   
                     ps_cl.Add(i);                                       
                }               
            }
            return ps_cl.ToArray();
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

        static PointF[] medianFilter_real(PointF[] ps, int wind = 10)
        {
            var ps_m = new PointF[ps.Length];
            var ps_l = ps.ToList(); 
            for (int i = 0; i < ps.Length; i++)
            {
                var beg = i - wind ; if (beg < 0) beg = 0;
                var end = i + wind ; if (end > ps.Length-1) end = ps.Length - 1;
                var len = end - beg;
                var arr = ps_l.GetRange(beg,len);
                arr = (from p in arr
                      orderby p.Y
                      select p).ToList();

                ps_m[i] = arr[arr.Count/2];
            }
            return ps_m;
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

        public static float averageXps(PointF[] ps1)
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
