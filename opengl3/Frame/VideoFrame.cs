using Emgu.CV;
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
        public Mat orig;
        public Point3d_GL pos_rob;
        public string name;
        public VideoFrame(Mat[] _im, Point3d_GL _pos_rob, string _name)
        {
            im = _im;
            pos_rob = _pos_rob;
            name = _name;
        }

        public VideoFrame(Mat[] _im, Mat _orig, string _name)
        {
            im = _im;
            orig = _orig;
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

        public Mat[] getMatsLasDif(int count,int start_ind=0,int strip = 1)
        {
            if (start_ind + count  > im.Length)
            {
                Console.WriteLine("dont create las dif: " + count + ">" + im.Length);
                return null;
            }
            var mats_ld = new List<Mat>();
            for(int i= start_ind; i< start_ind+ count; i+= strip)
            {
                mats_ld.Add( im[i] - orig);
            }
            return mats_ld.ToArray();
        }
    }
}
