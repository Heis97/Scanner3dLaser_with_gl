using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class RozumPoint
    {
        public point3d point;
        public rotation3d rotation;

        public DateTime timestamp;

        public RozumPoint(point3d p, rotation3d r)
        {
            this.point = p;
            this.rotation = r;
            timestamp = DateTime.Now;
        }
        public override string ToString()
        {
            return point.ToString() + " " + rotation.ToString() + timestamp.ToString();
        }

        public Point3d_GL to_point3dgl()
        {
            return new Point3d_GL(point.x, point.y, point.z);
        }
    }

    public class point3d
    {
        public double x;
        public double y;
        public double z;

        public point3d(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public override string ToString()
        {
            return x + " " + y + " " + z + ";";
        }
    }

    public class rotation3d
    {
        public double roll;
        public double pitch;
        public double yaw;

        public rotation3d(double roll, double pitch, double yaw)
        {
            this.roll = roll;
            this.pitch = pitch;
            this.yaw = yaw;
        }
        public override string ToString()
        {
            return roll + " " + pitch + " " + yaw+";";
        }
    }
}
