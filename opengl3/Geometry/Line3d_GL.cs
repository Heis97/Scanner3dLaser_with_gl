using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public struct Line3d_GL
    {
        public Point3d_GL k;//delt
        public Point3d_GL p;//start_point
        public Point3d_GL p_end;//start_point
        public Color3d_GL color;
        public Line3d_GL(Vector3d_GL _k, Point3d_GL _p, Color3d_GL _color = null)
        {
            k = new Point3d_GL(_k.x, _k.y, _k.z);
            p = _p;
            p_end = p + k;
            color = _color;
        }

        public Line3d_GL(Point3d_GL _k, Point3d_GL _p)
        {
            k = new Point3d_GL(_k.x - _p.x, _k.y - _p.y, _k.z - _p.z);
            p = _p;
            p_end = _k;
            color = _k.color;
        }



        public Point3d_GL calcCrossFlat(Flat3d_GL F)
        {

            var del = F.A * k.x + F.B * k.y + F.C * k.z;
            var num = F.A * p.x + F.B * p.y + F.C * p.z + F.D;
            // Console.WriteLine(num + " " + del);
            // Console.WriteLine(p+" "+k);
            if (del != 0)
            {
                var t = -num / del;
                return new Point3d_GL(
                    k.x * t + p.x,
                        k.y * t + p.y,
                        k.z * t + p.z, color);
            }
            else
            {
                return Point3d_GL.notExistP();
            }       
        }


        public Point3d_GL[] calcCrossSphere(Point3d_GL c, double R)
        {
            var kx = k.x;
            var ky = k.y;
            var kz = k.z;
            var u = p.x - c.x;
            var v = p.y - c.y;
            var w = p.z - c.z;
            var D = R * R * kx * kx + R * R * ky * ky +
                R * R * kz * kz - kx * kx * v * v - kx * kx * w * w + 
                2 * kx * ky * u * v + 2 * kx * kz * u * w - ky * ky * u * u - 
                ky * ky * w * w + 2 * ky * kz * v * w - kz * kz * u * u - kz * kz * v * v;

            
            var del = kx*kx + ky*ky+ kz*kz;

            if (D < 0 || del==0) return null;
            var t1 = (Math.Sqrt(D)+kx*u+ky*v+kz*w)/del;
            var t2 = (-Math.Sqrt(D) + kx * u + ky * v + kz * w) / del;


                return new Point3d_GL[]{ 
                    new Point3d_GL(k.x * t1 + p.x,k.y * t1 + p.y,k.z * t1 + p.z, color),
                    new Point3d_GL(k.x * t2 + p.x, k.y * t2 + p.y, k.z * t2 + p.z, color) };

        }

        public Point3d_GL calcCrossFlat_deb(Flat3d_GL F)
        {

            var del = F.A * k.x + F.B * k.y + F.C * k.z;
            var num = F.A * p.x + F.B * p.y + F.C * p.z + F.D;
            // Console.WriteLine(num + " " + del);
            // Console.WriteLine(p+" "+k);
            if (del != 0)
            {
                var t = -num / del;
                return new Point3d_GL(
                    del,
                       num,
                        t, color);
            }
            else
            {
                return Point3d_GL.notExistP();
            }
        }
        public override string ToString()
        {
            return "k = " +k.ToString() + "; p = " + p.ToString()+";";
        }
    }

}
