using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public struct Vector3d_GL
    {

        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }

       

        public double norm { get { return Math.Sqrt(x * x + y * y + z * z); } }
        public Vector3d_GL(double _x, double _y, double _z)
        {
            x = _x;
            y = _y;
            z = _z;
        }
        /// <summary>
        /// start, end
        /// </summary>
        /// <param name="b">start</param>
        /// <param name="a">end</param>
        public Vector3d_GL(Point3d_GL b, Point3d_GL a)//start,end
        {
            x = a.x - b.x;
            y = a.y - b.y;
            z = a.z - b.z;
        }

        public Vector3d_GL(Point p, double _z)
        {
            x = p.X;
            y = p.Y;
            z = _z;
        }

        public Point3d_GL toPoint()
        {
            return new Point3d_GL(x, y, z);
        }
        public Vector3d_GL normalize()
        {
            var n = Math.Sqrt(x * x + y * y + z * z);
            if (n != 0)
            {
                x /= n;
                y /= n;
                z /= n;
            }
            return this;
        }

        public static Vector3d_GL operator |(Vector3d_GL p1, Vector3d_GL p2)//vector multiply
        {
            return new Vector3d_GL(
                  p1.y * p2.z - p1.z * p2.y,
                  p1.z * p2.x - p1.x * p2.z,
                  p1.x * p2.y - p1.y * p2.x);
        }

        public static double operator *(Vector3d_GL p, Vector3d_GL p1)
        {
            return p.x * p1.x + p.y * p1.y + p.z * p1.z;
        }

        public static double operator *(Vector3d_GL p, Point3d_GL p1)
        {
            return p.x * p1.x + p.y * p1.y + p.z * p1.z;
        }

        public static double operator ^(Vector3d_GL p, Vector3d_GL p1)
        {
            return Math.Acos((p * p1) / (p.norm * p1.norm));
        }
        public static double cos(Vector3d_GL p, Vector3d_GL p1)
        {
            return (p * p1) / (p.norm * p1.norm);
        }

        public static Vector3d_GL operator *(Vector3d_GL p, double k)
        {
            return new Vector3d_GL(p.x * k, p.y * k, p.z * k);
        }
        public static Vector3d_GL operator /(Vector3d_GL p, double k)
        {
            return new Vector3d_GL(p.x / k, p.y / k, p.z / k);
        }
        public static Vector3d_GL operator *(Vector3d_GL p, float k)
        {
            return new Vector3d_GL(p.x * k, p.y * k, p.z * k);
        }
        public static Vector3d_GL operator +(Vector3d_GL p, Vector3d_GL p1)
        {
            return new Vector3d_GL(p.x + p1.x, p.y + p1.y, p.z + p1.z);
        }
        public static Vector3d_GL operator -(Vector3d_GL p, Vector3d_GL p1)
        {
            return new Vector3d_GL(p.x - p1.x, p.y - p1.y, p.z - p1.z);
        }
        public static Vector3d_GL operator -(Vector3d_GL p)
        {
            return new Vector3d_GL(-p.x, -p.y, -p.z);
        }
        public override string ToString()
        {
            return Math.Round(x,4) + " " + Math.Round(y, 4) + " " + Math.Round(z, 4);
        }

    }

}
