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
        public LaserSurface(Mat[] mats, Mat[] origs, CameraCV cameraCV, PatternType patternType, bool compPos = true)
        {
            calibrate(mats, origs, cameraCV,patternType, null,compPos);
        }
        public LaserSurface()
        {
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
        public bool calibrate(Mat[] mats, Mat[] origs, CameraCV cameraCV,PatternType patternType, GraphicGL graphicGL = null,bool compPos = true)
        {
            //Console.WriteLine("LAS CALIB#######################################");
            if(origs==null)
            {
                origs = new Mat[2];
            }
            var ps1 = points3dInCam(mats[0], origs[0], cameraCV, patternType, graphicGL, compPos);
            var ps2 = points3dInCam(mats[1], origs[1], cameraCV, patternType, graphicGL, compPos);
            if(ps1==null || ps2==null)
            {
                Console.WriteLine("ps1: " +ps1);
                Console.WriteLine("ps2: " + ps2);
                return false;
            }
            var ps = ps1.ToList();
            ps.AddRange(ps2);
            flat3D = computeSurface(ps.ToArray());
            //Console.WriteLine(flat3D);
            return true;
        }

        static Point3d_GL[] points3dInCam(Mat mat, Mat orig, CameraCV cameraCV,PatternType patternType,GraphicGL graphicGL = null,bool compPos = true)
        {
            //var points = Detection.detectLine(cameraCV.undist(mat));
            var points = Detection.detectLineDiff(mat,5);
            var ps = takePointsForFlat(points);
            if(compPos)
            {
                if(orig!=null)
                {
                    cameraCV.compPos(orig, patternType);
                   // prin.t(cameraCV.matrixSC);
                }
                else
                {
                    cameraCV.compPos(mat, patternType);
                }
            }
            var lines = PointCloud.computeTracesCam(ps, cameraCV);
            var ps3d = PointCloud.intersectWithFlat(lines, zeroFlatInCam(cameraCV.matrixSC));
            return ps3d;          
        }

        static PointF[] takePointsForFlat(PointF[] ps)
        {
            var psС = new PointF[2];
            var quart = (int)(ps.Length / 3);
            psС[0] = ps[quart];
            psС[1] = ps[ps.Length - quart];
            return psС;
        }

        static Flat3d_GL zeroFlatInCam(Matrix<double> matrix)
        {
            if(matrix ==null)
            {
                Console.WriteLine("matrxZeroFlat   NULL");
            }
            var p1 = matrix * new Point3d_GL(100, 0, 0);
            var p2 = matrix * new Point3d_GL(0, 0, 0);
            var p3 = matrix * new Point3d_GL(0, 100, 0);
            return new Flat3d_GL(p1, p2, p3);
        }
    }
}
