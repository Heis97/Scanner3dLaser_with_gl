using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class LaserSurface
    {
        public Flat3d_GL flat3D;
        public LaserSurface(Point3d_GL[] points)
        {
            flat3D = computeSurface(points);
        }
        public Flat3d_GL computeSurface(Point3d_GL[] points)
        {
            if (points.Length < 3)
            {
                return Flat3d_GL.notExistFlat();
            }
            var flat = new Flat3d_GL(points[0], points[1], points[2]);
            return flat;
        }
        public bool calibrate(Mat[] mats,Matrix<double>[] matrCS)
        {
            var points1 = Detection.detectLine(mats[0]);
            var points2 = Detection.detectLine(mats[1]);
            var ps1 = takePointsForFlat(points1);
            var ps2 = takePointsForFlat(points2);

            return true;
        }

        public static PointF[] takePointsForFlat(PointF[] ps1)
        {
            var ps = new PointF[2];
            var quart1 = (int)ps1.Length / 4;
            ps[0] = ps1[quart1];
            ps[1] = ps1[ps1.Length - quart1];
            return ps;
        }


    }
}
