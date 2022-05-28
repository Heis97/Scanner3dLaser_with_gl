using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public enum FrameType { Pos, LasRob, Test, MarkBoard, Pattern,LasHand, Undist ,LasLin }

    public class Frame
    {
        public Mat im;
        public Mat im_sec;
        public Point3d_GL pos_cam;
        public Point3d_GL pos_rob;
        public Point3d_GL pos_rob_or;
        public string name;
        public Size size;
        public PointF[] points;
        public FrameType frameType;
        public PatternType patternType;
        public Camera camera;
        public double size_mark;
        public double linPos;
        public DateTime dateTime;
        public bool stereo = false;
        public Frame(Mat _im, Point3d_GL _pos_cam, Point3d_GL _pos_rob, string _name, PointF[] _points)
        {
            im = _im;
            pos_cam = _pos_cam;
            pos_rob = _pos_rob;
            name = _name;
            points = _points;
            size = _im.Size;
            frameType = FrameType.LasRob;
        }
        public Frame(Mat _im, Point3d_GL _pos_cam, Point3d_GL _pos_rob, string _name, PointF[] _points, Point3d_GL _pos_rob_or)
        {
            im = _im;
            pos_cam = _pos_cam;
            pos_rob = _pos_rob;
            name = _name;
            points = _points;
            size = _im.Size;
            pos_rob_or = _pos_rob_or;
            frameType = FrameType.Pos;
        }
        public Frame(Mat _im, Point3d_GL _pos_cam, Point3d_GL _pos_rob, string _name)
        {
            im = _im;
            pos_cam = _pos_cam;
            pos_rob = _pos_rob;
            name = _name;
            size = _im.Size;
            frameType = FrameType.LasRob;

        }
        public Frame(Mat _im, string _name)
        {
            im = _im;
            name = _name;
            size = _im.Size;
            frameType = FrameType.Test;
        }

        public Frame(Mat _im, string _name, FrameType _frameType)
        {
            im = _im;
            name = _name;
            size = _im.Size;
            frameType = _frameType;
        }
        public Frame(Mat _im, string _name, FrameType _frameType,PatternType _patternType)
        {
            im = _im;
            name = _name;
            size = _im.Size;
            frameType = _frameType;
            if(frameType == FrameType.LasLin)
            {
                linPos = posLinFromName(name);
            }
            patternType = _patternType;
        }
        public Frame(Mat _im, Mat _im2, string _name, FrameType _frameType)
        {
            im = _im;
            im_sec = _im2;
            name = _name;
            size = _im.Size;
            frameType = _frameType;
            stereo = true;

        }

        static public Mat[] getMats(Frame[] frames)
        {
            var mats = new Mat[frames.Length];
            for(int i=0; i<mats.Length;i++)
            {
                mats[i] = frames[i].im;
            }
            return mats;
        }

        static public double[] getLinPos(Frame[] frames)
        {
            
            var linpos = new double[frames.Length];
            for (int i = 0; i < linpos.Length; i++)
            {
                if (frames[i].frameType == FrameType.LasLin)
                {
                    linpos[i] = frames[i].linPos;
                }                
            }
            return linpos;
        }
        override public string ToString()
        {
            return name;
        }

        static double posLinFromName(string name)
        {
            var names = name.Split('.');
            if (names[0].Contains('_'))
            {
                var nms = names[0].Split('_');
                names[0] = nms[nms.Length - 1];
            }
            double val = 0;
            try
            {
                val = Convert.ToDouble(names[0]);
            }
            catch
            {

            }
            Console.WriteLine(val);
            return val;
        }
    }


}
