using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public struct Flat3d_GL
    {
        public double A;
        public double B;
        public double C;
        public double D;
        public bool exist;
        public Vector3d_GL n;
        public int numb;
        public Flat3d_GL(double _A=0, double _B=0, double _C=0, double _D=0)
        {
            A = _A;
            B = _B;
            C = _C;
            D = _D;
            n = new Vector3d_GL(A, B, C);
            exist = true;
            numb = 0;
        }
        public Flat3d_GL(double[] vals)
        {
            A = vals[0];
            B = vals[1];
            C = 0;
            if (vals.Length > 2) C = vals[2]; 
          
            D = 0;
            n = new Vector3d_GL(A, B, C);
            exist = true;
            numb = 0;
        }
        public Flat3d_GL(Point3d_GL P1, Point3d_GL P2, Point3d_GL P3, Point3d_GL P4)
        {
            var x1 = P1.x;
            var y1 = P1.y;
            var z1 = P1.z;
            var x2 = P2.x;
            var y2 = P2.y;
            var z2 = P2.z;
            var x3 = P3.x;
            var y3 = P3.y;
            var z3 = P3.z;
            A = -(y1 * z2 - y2 * z1 - y1 * z3 + y3 * z1 + y2 * z3 - y3 * z2);
            B = x1 * z2 - x2 * z1 - x1 * z3 + x3 * z1 + x2 * z3 - x3 * z2;
            C = -(x1 * y2 - x2 * y1 - x1 * y3 + x3 * y1 + x2 * y3 - x3 * y2);
            D = x1 * y2 * z3 - x1 * y3 * z2 - x2 * y1 * z3 + x2 * y3 * z1 + x3 * y1 * z2 - x3 * y2 * z1;
            n = new Vector3d_GL(A, B, C);
            exist = true;
            numb = 0;
        }

        public Flat3d_GL(Point3d_GL P1, Point3d_GL P2, Point3d_GL P3)
        {
            var v1 = new Vector3d_GL(P1,P2);
            var v2 = new Vector3d_GL(P1, P3);
            var v3 = v1 | v2;//vector multiply
            v3.normalize();
            D = -v3 * P1;
            A = v3.x;
            B = v3.y;
            C = v3.z;
            n = new Vector3d_GL(A, B, C);
            exist = true;
            numb = 0;
        }

        public Flat3d_GL(Point3d_GL P1, Point3d_GL P2, Flat3d_GL F)//perpendic
        {
            var x1 = P1.x;
            var y1 = P1.y;
            var z1 = P1.z;
            var x2 = P2.x;
            var y2 = P2.y;
            var z2 = P2.z;
            var A2 = F.A;
            var B2 = F.B;
            var C2 = F.C;
            //var A2 = P1.x;
            //var B2 = P1.y;
            //var C2 = P1.z;
            A = -(B2 * z1 - C2 * y1 - B2 * z2 + C2 * y2);
            B = A2 * z1 - C2 * x1 - A2 * z2 + C2 * x2;
            C = -(A2 * y1 - B2 * x1 - A2 * y2 + B2 * x2);
            D = A2 * y1 * z2 - A2 * y2 * z1 - B2 * x1 * z2 + B2 * x2 * z1 + C2 * x1 * y2 - C2 * x2 * y1;
            n = new Vector3d_GL(A, B, C);
            exist = true;
            numb = 0;

        }

        //public 

        static public Point3d_GL cross(Flat3d_GL F1, Flat3d_GL F2, Flat3d_GL F3)
        {
            var A = F1.A;
            var B = F1.B;
            var C = F1.C;
            var D = F1.D;
            var A1 = F2.A;
            var B1 = F2.B;
            var C1 = F2.C;
            var D1 = F2.D;
            var A2 = F3.A;
            var B2 = F3.B;
            var C2 = F3.C;
            var D2 = F3.D;
            var d = A * B1 * C2 - A * B2 * C1 - A1 * B * C2 + A1 * B2 * C + A2 * B * C1 - A2 * B1 * C;
            var x = -(B * C1 * D2 - B1 * C * D2 - B * C2 * D1 + B2 * C * D1 + B1 * C2 * D - B2 * C1 * D) / d;
            var y = (A * C1 * D2 - A1 * C * D2 - A * C2 * D1 + A2 * C * D1 + A1 * C2 * D - A2 * C1 * D) / d;
            var z = -(A * B1 * D2 - A1 * B * D2 - A * B2 * D1 + A2 * B * D1 + A1 * B2 * D - A2 * B1 * D) / d;
            return new Point3d_GL(x, y, z);
        }

        static public Flat3d_GL notExistFlat()
        {
            var flat = new Flat3d_GL();
            flat.exist = false;
            return flat;
        }

        public double dist_to_flat(Point3d_GL p)
        {
            var flat = new Flat3d_GL();
            flat.exist = false;
            return p.x * A + p.y * B + p.z * C + D;
        }
        public double valP(Point3d_GL p)
        {
            return A * p.x + B * p.y + C * p.z + D;
        }

       /* public Flat3d_GL normalise()
        {
            D = -v3 * P1;
            
            n = n.normalize();
            A = n.x;
            B = n.y;
            C = n.z;
            return this;
        }*/

        public static Flat3d_GL operator *(Flat3d_GL f, double k)
        {
            return new Flat3d_GL(f.A * k, f.B * k, f.C * k,f.D * k);
        }

        public static Flat3d_GL operator /(Flat3d_GL f, double k)
        {
            return new Flat3d_GL(f.A / k, f.B / k, f.C / k, f.D / k);
        }

        public static Flat3d_GL operator +(Flat3d_GL f1, Flat3d_GL f2)
        {
            return new Flat3d_GL(f1.A +f2.A, f1.B + f2.B, f1.C + f2.C, f1.D + f2.D);
        }

        public static Flat3d_GL operator -(Flat3d_GL f1, Flat3d_GL f2)
        {
            return new Flat3d_GL(f1.A - f2.A, f1.B - f2.B, f1.C - f2.C, f1.D - f2.D);
        }

        public static Flat3d_GL operator -(Flat3d_GL f1)
        {
            return new Flat3d_GL(-f1.A ,- f1.B, -f1.C , -f1.D );
        }
        public override string ToString()
        {
            int pres = 6;
            return Math.Round(A,pres) + " " + Math.Round(B, pres) + " " + Math.Round(C, pres) + " " + Math.Round(D, pres);
        }
        public double[] ToDouble()
        {

            return new double[] { A, B, C };
        }

        public static Flat3d_GL[] gaussFilter(Flat3d_GL[] ps1, int wind = 3)
        {
            var ps1L = ps1.ToList();
            var ps1ret = (Flat3d_GL[])ps1.Clone();
            for (int i = 0; i < ps1.Length; i++)
            {
                if (i > 0)
                {
                    var beg = i - wind; if (beg < 0) beg = 0;
                    var end = i + wind; if (end >= ps1.Length) end = ps1.Length - 1;
                    var range = end - beg;
                    Flat3d_GL sum = new Flat3d_GL(0,0,0,0);
                    Array.ForEach(ps1L.GetRange(beg, range).ToArray(), p => sum += p);
                    var aver = sum / range;
                    ps1ret[i] = aver;
                }
            }
            return ps1ret;
        }

        public static Flat3d_GL[] gaussFilter_v2(Flat3d_GL[] ps1, int wind = 3)
        {
            var ps1L = ps1.ToList();
            var ps1ret = (Flat3d_GL[])ps1.Clone();
            for (int i = 0; i < ps1.Length; i++)
            {
                if (i > wind && i< ps1.Length-wind)
                {
                    var beg = i - wind; if (beg < 0) beg = 0;
                    var end = i + wind; if (end >= ps1.Length) end = ps1.Length - 1;
                    var range = end - beg;
                    Flat3d_GL sum = new Flat3d_GL(0, 0, 0, 0);
                    Array.ForEach(ps1L.GetRange(beg, range).ToArray(), p => sum += p);
                    var aver = sum / range;
                    ps1ret[i] = aver;
                }
                else
                {
                    ps1ret[i] = ps1[i];
                }
            }
            return ps1ret;
        }
    }
}
