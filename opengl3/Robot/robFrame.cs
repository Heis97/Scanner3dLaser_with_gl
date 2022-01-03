using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class robFrame
    {
        public double x;
        public double y;
        public double z;
        public double a;
        public double b;
        public double c;
        public robFrame()
        {

        }
        public robFrame(double _x, double _y, double _z, double _a, double _b, double _c)
        {
            x = _x;
            y = _y;
            z = _z;
            a = _a;
            b = _b;
            c = _c;
        }
        public robFrame(robFrame robFrame)
        {
            x = robFrame.x;
            y = robFrame.y;
            z = robFrame.z;
            a = robFrame.a;
            b = robFrame.b;
            c = robFrame.c;
        }
        public double rasstTo(robFrame robFrame)
        {
            var x1 = x;
            var y1 = y;
            var z1 = z;

            var x2 = robFrame.x;
            var y2 = robFrame.y;
            var z2 = robFrame.z;

            return Math.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1) + (z2 - z1) * (z2 - z1));
        }
        public Point3d_GL pos()
        {
            return new Point3d_GL(x, y, z);
        }
        public Point3d_GL ori()
        {
            return new Point3d_GL(a, b, c);
        }
        override public string ToString()
        {
            return (x + " " + y + " " + z + " " + a + " " + b + " " + c);
        }

    }
}
