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
            var ps1 = new Point3d_GL[0];
            var ps2 = new Point3d_GL[0];
            if (origs==null)
            {
                origs = new Mat[2];
            }
            if(mats.Length>1)
            {
                ps1 = points3dInCam(mats[0], origs[0], cameraCV, patternType, graphicGL, compPos);
                ps2 = points3dInCam(mats[1], origs[1], cameraCV, patternType, graphicGL, compPos);
            }
            if (mats.Length == 1)
            {
                ps1 = points3dInCam(mats[0], origs[0], cameraCV, patternType, graphicGL, compPos,true);
                ps2 = new Point3d_GL[2] { ps1[2], ps1[3] };
            }
            
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

        static Point3d_GL[] points3dInCam(Mat mat, Mat orig, CameraCV cameraCV,PatternType patternType,GraphicGL graphicGL = null,bool compPos = true,bool oneMat = false)
        {
            var points = Detection.detectLineDiff(mat,5);
            var ps = new PointF[0];
            double z = 0;
            if(oneMat)
            {
                ps = takePointsForFlat(points, oneMat);
            }
            else
            {                
                ps = takePointsForFlat(points);
            }
            
            //Console.Write(ps[0] + " " + ps[1]);
            if(compPos)
            {
                if(orig!=null)
                {
                    cameraCV.compPos(orig, patternType);
                }
                else
                {
                    cameraCV.compPos(mat, patternType);
                }
            }
            var lines = PointCloud.computeTracesCam(ps, cameraCV);
            var ps3d = new List<Point3d_GL>();
            ps3d.AddRange( PointCloud.intersectWithFlat(new Line3d_GL[] { lines[0], lines[1] }, zeroFlatInCam(cameraCV.matrixSC,z)));
            if (oneMat)
            {
                z = 16;
            }
            ps3d.AddRange(PointCloud.intersectWithFlat(new Line3d_GL[] { lines[2], lines[3] }, zeroFlatInCam(cameraCV.matrixSC, z)));
            return ps3d.ToArray();          
        }

        public static PointF[] takePointsForFlat(PointF[] ps,bool oneMat=false)
        {

            
            
            if(oneMat)
            {
                var quart = (int)(ps.Length / 30);
                var psС = new PointF[4];
                psС[0] = ps[quart];
                psС[1] = ps[ps.Length - quart];

                psС[2] = ps[(int)(ps.Length/2) - quart];
                psС[3] = ps[(int)(ps.Length / 2) + quart];
                return psС;
            }
            else
            {
                var quart = (int)(ps.Length / 3);
                var psС = new PointF[2];
                psС[0] = ps[quart];
                psС[1] = ps[ps.Length - quart];
                return psС;
            }
            
           
        }

        static Flat3d_GL zeroFlatInCam(Matrix<double> matrix,double z = 0)
        {
            if(matrix ==null)
            {
                Console.WriteLine("matrxZeroFlat   NULL");
            }
            var p1 = matrix * new Point3d_GL(100, 0, z);
            var p2 = matrix * new Point3d_GL(0, 0, z);
            var p3 = matrix * new Point3d_GL(0, 100, z);
            return new Flat3d_GL(p1, p2, p3);
        }
    }
}
