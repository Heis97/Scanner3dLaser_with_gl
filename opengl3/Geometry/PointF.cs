using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public struct PointF
    {
        public float X;
        public float Y;
        public bool exist;
        public float norm 
        { 
            get 
            { 
                return (float)Math.Sqrt(X * X + Y * Y); 
            } 
        }
        public PointF(float _x, float _y)
        {
            X = _x;
            Y = _y;
            exist = true;
        }
        public PointF(double _x, double _y)
        {
            X = (float)_x;
            Y = (float)_y;
            exist = true;
        }
        public PointF(PointF p)
        {
            X = p.X;
            Y = p.Y;
            exist = p.exist;
        }
        public PointF(Point p)
        {
            X = p.X;
            Y = p.Y;
            exist = true;
        }
        static public PointF notExistP()
        {
            var p = new PointF();
            p.exist = false;
            return p;
        }

        public void normalize()
        {
            var n = (float)Math.Sqrt(X * X + Y * Y);
            if (n != 0)
            {
                X /= n;
                Y /= n;
            }
        }
        public static Point[] toPoint(PointF[] ps)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new Point[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ret[i] = new Point((int)ps[i].X, (int)ps[i].Y);
            }
            return ret;
        }
        public static Point[] toPoint(System.Drawing.PointF[] ps)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new System.Drawing.Point[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ret[i] = new System.Drawing.Point((int)ps[i].X, (int)ps[i].Y);
            }
            return ret;
        }
        public static PointF[] toPointF(System.Drawing.PointF[] ps)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new PointF[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ret[i] = new PointF(ps[i].X, ps[i].Y);
            }
            return ret;
        }
        public static System.Drawing.PointF[] toSystemPoint(PointF[] ps)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new System.Drawing.PointF[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ret[i] = new System.Drawing.PointF(ps[i].X, ps[i].Y);
            }
            return ret;
        }
        public static double calc_p_len(Point p1, Point p2)
        {
            var p = new Point(p1.X - p2.X, p1.Y - p2.Y);
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }

        public static double calc_p_len(PointF p1, PointF p2)
        {
            var p = new PointF(p1.X - p2.X, p1.Y - p2.Y);
            return Math.Sqrt(p.X * p.X + p.Y * p.Y);
        }
        public static PointF[] toPointF(Point[] ps)
        {
            var ret = new PointF[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ret[i] = new PointF(ps[i].X, ps[i].Y);
            }
            return ret;
        }

        public static PointF[] toPointF(VectorOfPoint ps)
        {
            var ret = new PointF[ps.Size];
            for (int i = 0; i < ps.Size; i++)
            {
                ret[i] = new PointF(ps[i].X, ps[i].Y);
            }
            return ret;
        }
        public static double operator *(PointF p, PointF p1)
        {
            return p.X * p1.X + p.Y * p1.Y;
        }

        public static double operator ^(PointF p, PointF p1)
        {
            return Math.Acos((p * p1) / (p.norm * p1.norm));
        }

        public static PointF operator *(PointF p, float k)
        {
            return new PointF(p.X * k, p.Y * k);
        }
        public static PointF operator +(PointF p, PointF p1)
        {
            return new PointF(p.X + p1.X, p.Y + p1.Y);
        }
        public static PointF operator -(PointF p, PointF p1)
        {
            return new PointF(p.X - p1.X, p.Y - p1.Y);
        }
        public override string ToString()
        {
            return X.ToString() + " " + Y.ToString() + ";";
        }

    }
}
