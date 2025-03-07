using Emgu.CV.Structure;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

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
        public PointF Clone()
        {
            return new PointF(X, Y);
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
        public static void check_exist(PointF[] ps)
        {
            for (int i = 0; i < ps.Length; i++)
            {
                if(!ps[i].exist) Console.WriteLine(ps[i].exist + " " + i);

            }
        }

        public static PointF[] filter_exist(PointF[] ps)
        {
            if (ps == null) return null;
            var ps_ex = new List<PointF>();
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].exist) ps_ex.Add(ps[i]);
            }
            return ps_ex.ToArray();
        }

        public System.Drawing.Point toPoint()
        {
            return new System.Drawing.Point((int)X, (int)Y);
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
        public static Point[] toPoint(MCvPoint3D32f[] ps)
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
        public static PointF[] toPointF(MCvPoint3D32f[] ps)
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
        public static System.Drawing.Point[] toSystemPoint_d(PointF[] ps)
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
        public static System.Drawing.Point[,] toSystemPoint_d_2d(PointF[,] ps)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new System.Drawing.Point[ps.GetLength(0), ps.GetLength(1)];
            for (int i = 0; i < ps.GetLength(0); i++)
                for (int j = 0; j < ps.GetLength(1); j++)
                {
                    ret[i,j] = new System.Drawing.Point((int)ps[i,j].X, (int)ps[i,j].Y);
                }
            return ret;
        }
        public static System.Drawing.PointF[,] toSystemPoint_2d(PointF[,] ps)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new System.Drawing.PointF[ps.GetLength(0), ps.GetLength(1)];
            for (int i = 0; i < ps.GetLength(0); i++)
                for (int j = 0; j < ps.GetLength(1); j++)
                {
                    ret[i, j] = new System.Drawing.PointF(ps[i, j].X, ps[i, j].Y);
                }
            return ret;
        }
        public static PointF[,] toSystemPoint_s_2d(System.Drawing.PointF[,] ps)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new PointF[ps.GetLength(0), ps.GetLength(1)];
            for (int i = 0; i < ps.GetLength(0); i++)
                for (int j = 0; j < ps.GetLength(1); j++)
                {
                    ret[i, j] = new PointF(ps[i, j].X, ps[i, j].Y);
                }
            return ret;
        }

        public static System.Drawing.Point[,] toSystemPoint_ss_2d(System.Drawing.PointF[,] ps)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new System.Drawing.Point[ps.GetLength(0), ps.GetLength(1)];
            for (int i = 0; i < ps.GetLength(0); i++)
                for (int j = 0; j < ps.GetLength(1); j++)
                {
                    if(ps[i, j] != null)
                        ret[i, j] = new System.Drawing.Point((int)ps[i, j].X,(int) ps[i, j].Y);
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

        public static System.Drawing.PointF[] toSystemPoint(MCvPoint3D32f[] ps)
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
        public static System.Drawing.PointF[] addNoise(System.Drawing.PointF[] ps,float ampl)
        {
            if (ps == null)
            {
                return null;
            }
            var ret = new System.Drawing.PointF[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                var rand = Accord.Math.Random.Generator.Random;
                ret[i] = new System.Drawing.PointF(ps[i].X+ampl* (float)rand.NextDouble(), ps[i].Y + ampl * (float)rand.NextDouble());
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
            return X.ToString() + " " + Y.ToString() + " ";
        }

        public static PointF[] operator +(PointF[] ps, PointF p1)
        {
            if (ps == null) return null;
            var ps_p = new PointF[ps.Length];
            for (int i = 0; i < ps.Length; i++) ps_p[i] = ps[i] + p1;
            return ps_p;
        }

        public static PointF[] from_contour(VectorOfPoint ps)
        {
            if (ps == null) return null;
            var ps_p = new PointF[ps.Size];
            for (int i = 0; i < ps.Size; i++) ps_p[i] = new PointF(ps[i].X, ps[i].Y);
            return ps_p;
        }

        public static PointF[] averX(PointF[] ps1, PointF[] ps2, double k = 0.5)
        {
            if (ps1 == null || ps2 == null) return null;
            if (ps1.Length != ps2.Length) return null;
            var ps_aver = new PointF[ps1.Length];
            for(int i=0; i<ps1.Length;i++)
            {
                
                ps_aver[i] = new PointF(ps1[i].X+(ps2[i].X - ps1[i].X) *k, ps1[i].Y);
                if (!ps1[i].exist || !ps2[i].exist) ps_aver[i].exist = false;
            }

            return ps_aver;

        }

        public static PointF[] averY(PointF[] ps1, PointF[] ps2, double k = 0.5)
        {
            if (ps1 == null || ps2 == null) return null;
            if (ps1.Length != ps2.Length) return null;
            var ps_aver = new PointF[ps1.Length];
            for (int i = 0; i < ps1.Length; i++)
            {
                ps_aver[i] = new PointF(ps1[i].X,ps1[i].Y + (ps2[i].Y - ps1[i].Y) * k);
            }

            return ps_aver;

        }

        public static float aver_y_value(PointF[] ps)
        {
            if (ps == null) return 0;
            if (ps.Length < 2) return 0;
            float val = 0;
            for (int i = 0; i < ps.Length; i++)
            {
                val += ps[i].Y;
            }
            return val;
        }

        public static float aver_x_value(PointF[] ps)
        {
            if (ps == null) return 0;
            if (ps.Length < 2) return 0;
            float val = 0;
            for (int i = 0; i < ps.Length; i++)
            {
                val += ps[i].X;
            }
            return val/ ps.Length;
        }
        public static PointF[] filter_global_y(PointF[] ps, float delta)
        {
            if (ps == null) return null;
            if (ps.Length <2) return null;
            var ps_filter = new PointF[ps.Length];
            var aver_y = aver_y_value(ps);
            for (int i = 0; i < ps.Length; i++)
            {
                if(ps[i].Y < aver_y+delta && ps[i].Y < aver_y - delta)
                {
                    ps_filter[i] = new PointF(ps[i].X, ps[i].Y);
                }                
            }
            return ps_filter;
        }
        public static PointF[] filter_global_x(PointF[] ps, float delta)
        {
            if (ps == null) return null;
            if (ps.Length < 2) return null;
            var ps_filter = new List< PointF>();
            var aver_x = aver_x_value(ps);
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].X < aver_x + delta && ps[i].X > aver_x - delta)
                {
                    ps_filter.Add( new PointF(ps[i].X, ps[i].Y));
                }
            }
            Console.WriteLine(ps.Length + " " + ps_filter.Count + " " + aver_x);
            return ps_filter.ToArray();
        }
    }
}
