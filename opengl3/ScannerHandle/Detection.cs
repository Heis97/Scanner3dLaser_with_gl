﻿using System;
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
        public static PointF[] detectLine(Mat mat)
        {
            var ps = new PointF[mat.Width];
            var data = (byte[,,])mat.GetData();
            for(int i =0;i < data.GetLength(1);i++)
            {
                byte br_max = 0;

                for (int j = 0; j < data.GetLength(0); j++)
                {
                    var br_cur = data[j, i, 0];
                    if(br_cur>br_max)
                    {
                        br_max = br_cur;
                    }
                }
                if(br_max>0)
                {
                    ps[i] = new PointF(i, br_max);
                }
                else
                {
                    ps[i] = PointF.notExistP();
                }
            }
            return ps;
        }

    }
}
