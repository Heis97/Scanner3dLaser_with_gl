using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class PointCloud
    {
        public Point3d_GL[] points3d;
        public List<Point3d_GL[]> points3d_lines;
        public Point3d_GL[] points3d_cur;

        public PointCloud()
        {
            points3d = new Point3d_GL[0];
            points3d_lines = new  List<Point3d_GL[]>();
        }

        public void clearPoints()
        {
            points3d = new Point3d_GL[0];
            points3d_lines = new List<Point3d_GL[]>();
        }
        public bool addPoints(Mat mat, CameraCV cameraCV,LaserSurface laserSurface)
        {
            //var points_im = Detection.detectLine(cameraCV.undist(mat));
            var points_im = Detection.detectLine(mat);
            var points_cam = fromLines(points_im, cameraCV, laserSurface.flat3D);
            cameraCV.compPos(mat, PatternType.Chess);
            points3d_cur = camToScene(points_cam, cameraCV.matrixSC);
            var ps_list = points3d.ToList();
            ps_list.AddRange(points3d_cur);
            points3d_lines.Add(points3d_cur);
            points3d = ps_list.ToArray();
            return true;
        }

        public bool addPointsLin(Mat mat,double LinPos, CameraCV cameraCV, LaserSurface laserSurface,LinearAxis linearAxis )
        {
            //var points_im = Detection.detectLine(cameraCV.undist(mat));
            var points_im = Detection.detectLineDiff(mat);
            var points_cam = fromLines(points_im, cameraCV, laserSurface.flat3D);
            var matrixSC = linearAxis.getMatrixCamera(LinPos);
            points3d_cur = camToScene(points_cam, matrixSC);
            var ps_list = points3d.ToList();
            ps_list.AddRange(points3d_cur);
            points3d_lines.Add(points3d_cur);
            points3d = ps_list.ToArray();
            return true;
        }

        public bool addPointsLinLas(Mat mat, double LinPos, CameraCV cameraCV, LinearAxis linearAxis,Mat orig,PatternType patternType)
        {

            cameraCV.compPos(orig, patternType);  
            var points_im = Detection.detectLineDiff(mat);
            //var points_im = Detection.detectLineDiff(mat);
            var points_cam = fromLines(points_im, cameraCV, linearAxis.getLaserSurf(LinPos));
            points3d_cur = camToScene(points_cam, cameraCV.matrixCS);
            var ps_list = points3d.ToList();
            ps_list.AddRange(points3d_cur);
            points3d_lines.Add(points3d_cur);
            points3d = ps_list.ToArray();
            return true;
        }



        static Point3d_GL[] camToScene(Point3d_GL[] points_cam, Matrix<double> MatrixSC)
        {
            var points3d = new Point3d_GL[points_cam.Length];
            for (int i = 0; i < points3d.Length; i++)
            {
                points3d[i] = MatrixSC * points_cam[i];
            }
            return points3d;
        }
        static Point3d_GL[] fromLines(PointF[] points_im, CameraCV cameraCV, Flat3d_GL laserSurface)
        {
            var lines3d = computeTracesCam(points_im, cameraCV);
            var points_cam = intersectWithLaser(lines3d, laserSurface);
            return points_cam;
        }

        static Point3d_GL[] intersectWithLaser(Line3d_GL[] lines3d, Flat3d_GL laserSurface)
        {
            return intersectWithFlat(lines3d, laserSurface);
        }

        public static Point3d_GL[] intersectWithFlat(Line3d_GL[] lines3d, Flat3d_GL flat)
        {
            var points3d = new Point3d_GL[lines3d.Length];
            for (int i = 0; i < points3d.Length; i++)
            {
                points3d[i] = lines3d[i].calcCrossFlat(flat);
            }
            return points3d;
        }
        public static Line3d_GL[] computeTracesCam(PointF[] points_im, CameraCV cameraCV)
        {
            var lines3d = new Line3d_GL[points_im.Length];
            for(int i=0; i<lines3d.Length;i++)
            {

                lines3d[i] = new Line3d_GL(
                    cameraCV.point3DfromCam(points_im[i]),
                    new Point3d_GL(0, 0, 0));
            }

            return lines3d;
        }

        static Point3d_GL[] fromHomography(PointF[] points_im, CameraCV cameraCV, LaserSurface laserSurface)
        {
            return null;
        }

    }
}
