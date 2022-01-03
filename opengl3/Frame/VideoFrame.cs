﻿using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class VideoFrame
    {
        public Mat[] im;
        public Point3d_GL pos_rob;
        public string name;
        public VideoFrame(Mat[] _im, Point3d_GL _pos_rob, string _name)
        {
            im = _im;
            pos_rob = _pos_rob;
            name = _name;
        }
        override public string ToString()
        {
            return name;
        }

        public Frame toFrame()
        {
            return new Frame(findMostWhite(im), new Point3d_GL(0, 0, 0), pos_rob, name, new PointF[1]);
        }
        Mat findMostWhite(Mat[] mats)
        {
            var sort_mats = from mat in mats
                            orderby calcWhiteIm(mat) descending
                            select mat;
            return sort_mats.ToArray()[0];
        }
        int calcWhiteIm(Mat mat)
        {
            int ret = 0;
            var im = mat.ToImage<Gray, Byte>();
            for (int x = 0; x < mat.Width; x++)
            {
                for (int y = 0; y < mat.Height; y++)
                {
                    if (im.Data[y, x, 0] > 252)
                    {
                        ret++;
                    }
                }
            }
            return ret;
        }
    }
}
