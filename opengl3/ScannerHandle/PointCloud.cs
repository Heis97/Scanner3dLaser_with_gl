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
        public Point3d_GL[] points3d_cur;
        public PointCloud()
        {
            points3d = new Point3d_GL[0];
        }
        public bool addPoints(Mat mat, CameraCV cameraCV,LaserSurface laserSurface)
        {
            var points_im = Detection.detectLine(mat);
            var points_cam = fromLines(points_im, cameraCV, laserSurface);
            cameraCV.compPos(mat, PatternType.Chess);
            points3d_cur = camToScene(points_cam, cameraCV);
            var ps_list = points3d.ToList();
            ps_list.AddRange(points3d_cur);
            points3d = ps_list.ToArray();
            return true;
        }

        static Point3d_GL[] camToScene(Point3d_GL[] points_cam, CameraCV cameraCV)
        {

            var matr = cameraCV.matrixCS;
            var points3d = new Point3d_GL[points_cam.Length];
            for (int i = 0; i < points3d.Length; i++)
            {
                points3d[i] = matr * points_cam[i];
            }
            return points3d;
        }
        static Point3d_GL[] fromLines(PointF[] points_im, CameraCV cameraCV, LaserSurface laserSurface)
        {
            var lines3d = computeTraces(points_im, cameraCV);
            var points_cam = intersectWithLaser(lines3d, laserSurface);
            return points_cam;
        }

        static Point3d_GL[] intersectWithLaser(Line3d_GL[] lines3d, LaserSurface laserSurface)
        {
            return intersectWithFlat(lines3d, laserSurface.flat3D);
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
        public static Line3d_GL[] computeTraces(PointF[] points_im, CameraCV cameraCV)
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
