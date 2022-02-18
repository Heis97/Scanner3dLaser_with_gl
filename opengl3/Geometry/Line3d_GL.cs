using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public struct Line3d_GL
    {
        public Point3d_GL k;
        public Point3d_GL p;
        public Line3d_GL(Vector3d_GL _k, Point3d_GL _p)
        {
            k = new Point3d_GL(_k.x, _k.y, _k.z);
            p = _p;
        }
        public Line3d_GL(Point3d_GL _k, Point3d_GL _p)
        {
            k = _k;
            p = _p;
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
                     k.z * t + p.z);
            }
            else
            {
                return Point3d_GL.notExistP();
            }
        }
    }

}
