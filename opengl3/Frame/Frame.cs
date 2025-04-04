﻿using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public enum FrameType { Pos, LasRob, Test, MarkBoard, Pattern,LasHand, Undist ,LasLin,LasDif,CircleGrid ,ThreeDimens}

    public class Frame
    {
        public Mat im;
        public Mat im_sec;
        public Mat im_dif;
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
        public RobotFrame RobotFrame;
        public Frame()
        {

        }
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
            if(frameType == FrameType.LasLin || frameType == FrameType.LasDif)
            {
                linPos = posLinFromNameDouble(name);
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
            return  mats;
        }

        static public Mat[][] getMats(Frame[] frames,bool stereo)
        {
            var mats = new Mat[frames.Length][];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] =new Mat[] { frames[i].im, frames[i].im_sec };
            }
            return mats;
        }

        static public Mat[][] reshapeMats(Mat[][] mats1)
        {
            var mats = new Mat[mats1[0].Length][];
            for (int i = 0; i < mats.Length; i++)
            {
                var sub_mats = new List<Mat>();
                for(int j = 0; j < mats1.Length; j++)
                {
                    sub_mats.Add(mats1[j][i]);
                }
                mats[i] = sub_mats.ToArray();
            }
            return mats;
        }

        static public Mat[][] getMats(Frame[][] frames)
        {
            var mats = new Mat[frames.Length][];
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = getMats(frames[i]);
            }
            return mats;
        }

        static public double[] getLinPos(Frame[] frames)
        {
            
            var linpos = new double[frames.Length];
            for (int i = 0; i < linpos.Length; i++)
            {
                if (frames[i].frameType == FrameType.LasLin  || frames[i].frameType == FrameType.LasDif)
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

        static double posLinFromNameDouble(string name)
        {
            double val = 0;
            var names = name.Split('.');
            if(names.Length==2)
            {
                val = Convert.ToDouble(names[0]);
            }
            if (names.Length == 3)
            {
                if(names[1]=="png")
                {
                    val = Convert.ToDouble(names[0]);
                    
                }
                else
                {
                    val = Convert.ToDouble(names[0] + '.' + names[1]);
                }
                
            }
            Console.WriteLine(val);
            return val;
        }
    }


}
