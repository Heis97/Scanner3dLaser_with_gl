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

        public static PointF[] detectLineDiff(Mat _mat, int wind = 3,float board = 0.05f,bool reverse = false)
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
            for (int i = (int)(board* data.GetLength(1)); i < data.GetLength(1)- (int)(board * data.GetLength(1)); i++)
            {
                br_max = int.MinValue;
                int j_max = 0;
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

                    ps_list.Add(new PointF(i, j_max_2));

                }
                //ps[i] = new PointF(i, j_max);
                

            }
            ps = ps_list.ToArray();
            medianFilter(ps);
            gaussFilter(ps);

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
            var ps1L = ps1.ToList();
            for(int i= wind; i< ps1.Length - wind; i++)
            {
                if( 
                    Math.Abs
                    (
                        ps1[i].Y - averageYps(ps1L.GetRange(i-wind,2* wind).ToArray())
                        )>delt
                    )
                    
                {
                    if(i>0)
                    {

                        ps1[i] = ps1[i - 1].Clone();
                    }
                    
                }
            }
            return ps1;
        }

        static PointF[] gaussFilter(PointF[] ps1,  int wind = 3)
        {
            var ps1L = ps1.ToList();
            for (int i = wind; i < ps1.Length - wind; i++)
            {
                if (i > 0)
                {
                    ps1[i] = new PointF(i, averageYps(ps1L.GetRange(i - wind, 2 * wind).ToArray()));
                }               
            }
            return ps1;
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



    }
}
