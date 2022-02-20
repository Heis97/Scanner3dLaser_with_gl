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
        public static PointF[] detectLine(Mat mat, int wind=20)
        {
            var ps = new PointF[mat.Width];
            var data = (byte[,,])mat.GetData();
            for(int i =0;i < data.GetLength(1);i++)
            {
                //Console.WriteLine("_________________________________________________");
                float br_max = -512;
                int j_max = 0;
                for (int j = 0; j < data.GetLength(0); j++)
                {
                    //float sum = (float)Math.Max(data[j, i, 2], Math.Max(data[j, i, 1], data[j, i, 0]));
                    float br_cur = -512;
                    
                    //br_cur = (float)data[j, i, 2]  - 0.5f*(float)data[j, i, 1] - 0.5f*(float)data[j, i, 0] ;
                    br_cur = (float)data[j, i, 2] + (float)data[j, i, 1] + (float)data[j, i, 0];
                    //if(i>150 && i<160)
                    //Console.WriteLine(br_max + "| " + data[j, i, 0] + " " + data[j, i, 1] + " " + data[j, i, 2]);
                    if (br_cur>br_max)
                    {
                        
                        br_max = br_cur;
                        j_max = j;
                        
                    }
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
            return ps;
        }

        static float centerOfMass(int[,] col)
        {
            int mas_sum = 0;
            int masX_sum = 0;
            for(int i=0; i<col.GetLength(0);i++)
            {
                mas_sum += col[i,1];
                masX_sum+= (col[i,1]* col[i, 0]);
                Console.WriteLine(col[i, 0] + " "+col[i, 1]);
            }
            
            var mc = (float)masX_sum / (float)mas_sum;
            Console.WriteLine(mc+"   _________________");
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

    }
}
