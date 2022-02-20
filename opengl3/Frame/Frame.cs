using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public enum FrameType { Pos, LasRob, Test, MarkBoard, Pattern }

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
        public FrameType type;
        public Camera camera;
        public double size_mark;
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
            type = FrameType.LasRob;
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
            type = FrameType.Pos;
        }
        public Frame(Mat _im, Point3d_GL _pos_cam, Point3d_GL _pos_rob, string _name)
        {
            im = _im;
            pos_cam = _pos_cam;
            pos_rob = _pos_rob;
            name = _name;
            size = _im.Size;
            type = FrameType.LasRob;

        }
        public Frame(Mat _im, string _name)
        {
            im = _im;
            name = _name;
            size = _im.Size;
            type = FrameType.Test;

        }

        public Frame(Mat _im, string _name, FrameType frameType)
        {
            im = _im;
            name = _name;
            size = _im.Size;
            type = frameType;

        }
        public Frame(Mat _im, Mat _im2, string _name, FrameType frameType)
        {
            im = _im;
            im_sec = _im2;
            name = _name;
            size = _im.Size;
            type = frameType;
            stereo = true;

        }
        override public string ToString()
        {
            return name;
        }
    }


}
