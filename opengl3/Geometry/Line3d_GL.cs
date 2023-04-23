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
