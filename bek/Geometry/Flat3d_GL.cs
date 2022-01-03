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
        public Flat3d_GL(double _A, double _B, double _C, double _D)
        {
            A = _A;
            B = _B;
            C = _C;
            D = _D;
        }
        public Flat3d_GL(Point3d_GL P1, Point3d_GL P2, Point3d_GL P3)
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
        }
        public Flat3d_GL(Point3d_GL P1, Point3d_GL P2, Flat3d_GL F)
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

        }
        public double valP(Point3d_GL p)
        {
            return A * p.x + B * p.y + C * p.z + D;
        }
    }
}
