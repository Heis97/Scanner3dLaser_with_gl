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
        public static PointF[] detectLine(Mat mat, int wind=40)
        {
            var ps = new PointF[mat.Width];
            var data = (byte[,,])mat.GetData();
            for(int i =0;i < data.GetLength(1);i++)
            {
                byte br_max = 0;
                int j_max = 0;
                for (int j = 0; j < data.GetLength(0); j++)
                {
                    var br_cur = data[j, i, 0];
                    if(br_cur>br_max)
                    {
                        br_max = br_cur;
                        j_max = j;
                    }
                }
                if(br_max>0)
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
                        col[k, 1] = data[j, i, 0];
                    }
                    var j_max_2 = centerOfMass(col);
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
            float mas_sum = 0;
            float masX_sum = 0;
            for(int i=0; i<col.GetLength(0);i++)
            {
                mas_sum += (float)col[i,1];
                masX_sum+= (float)(col[i,1]* col[i, 0]);
            }
            return masX_sum / mas_sum;
        }


    }
}
