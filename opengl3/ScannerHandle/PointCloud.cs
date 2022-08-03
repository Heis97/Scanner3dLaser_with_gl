using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        public Image<Bgr,byte>[] color_im;
        public GraphicGL graphicGL;

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
            points3d_cur = camToScene(points_cam, cameraCV.matrixCS);
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
            var matrixCS = linearAxis.getMatrixCamera(LinPos);
            points3d_cur = camToScene(points_cam, matrixCS);
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
            //points3d_cur = points_cam;
            points3d_cur = camToScene(points_cam, cameraCV.matrixCS);
            var ps_list = points3d.ToList();
            ps_list.AddRange(points3d_cur);
            points3d_lines.Add(points3d_cur);
            points3d = ps_list.ToArray();
            return true;
        }

        public bool addPointsStereoLas(Mat[] mat, StereoCamera stereocamera, bool undist)
        {
            if(undist)
            {
                mat[0] = stereocamera.cameraCVs[0].undist(mat[0]);
                mat[1] = stereocamera.cameraCVs[1].undist(mat[1]);
            }
            var points_im1 = Detection.detectLineDiff(mat[0], 7);
            var points_im2 = Detection.detectLineDiff(mat[1], 7);
            var points_cam = fromStereoLaser_gpu(points_im1, points_im2, stereocamera, graphicGL, color_im);
            points3d_cur = points_cam;
            //points3d_cur = camToScene(points_cam, stereocamera.cameraCVs[1].matrixCS);
            var ps_list = points3d.ToList();
            ps_list.AddRange(points3d_cur);
            points3d_lines.Add(points3d_cur);
            points3d = ps_list.ToArray();
            return true;
        }

        public static Point3d_GL[] camToScene(Point3d_GL[] points_cam, Matrix<double> MatrixSC)
        {
            var points3d = new Point3d_GL[points_cam.Length];
            for (int i = 0; i < points3d.Length; i++)
            {
                points3d[i] = MatrixSC * points_cam[i];
            }
            return points3d;
        }



        public static Point3d_GL[] fromLines(PointF[] points_im, CameraCV cameraCV, Flat3d_GL laserSurface)
        {
            var lines3d = computeTracesCam(points_im, cameraCV);
            var points_cam = intersectWithLaser(lines3d, laserSurface);
            return points_cam;
        }

        public static Point3d_GL[] fromStereoLaser(PointF[] points_im1, PointF[] points_im2, StereoCamera stereocamera,GraphicGL graphicGL=null, Image<Bgr, byte>[] color_im = null)
        {

            var points3d_1 = computePointsCam(points_im1, stereocamera.cameraCVs[0],color_im[0]) ;
            var lines3d_1 = computeTracesCam(points3d_1, stereocamera.cameraCVs[0].matrixCS);//stereocamera.R
            //var polygons3d_1 = computePolygonsCam(points3d_1, stereocamera.cameraCVs[0].matrixCS);

         //   CvInvoke.Rotate(color_im[1], color_im[1], Emgu.CV.CvEnum.RotateFlags.Rotate180);
            var points3d_2 = computePointsCam(points_im2, stereocamera.cameraCVs[1], color_im[1]);

           // var lines3d_2 = computeTracesCam(points3d_2, stereocamera.cameraCVs[1].matrixCS);
            var polygons3d_2 = computePolygonsCam(points3d_2, stereocamera.cameraCVs[1].matrixCS);

            
            //var points_cam2a = Polygon3d_GL.createLightFlat(polygons3d_1, lines3d_2);


             var points_cam2b = Polygon3d_GL.createLightFlat(polygons3d_2, lines3d_1);
            // graphicGL?.addMesh(Polygon3d_GL.toMesh(polygons3d_1), OpenGL.PrimitiveType.Triangles);
            //  graphicGL?.addMesh(Polygon3d_GL.toMesh(polygons3d_2), OpenGL.PrimitiveType.Lines);


            return points_cam2b;
        }

        public static Point3d_GL[] fromStereoLaser_gpu(PointF[] points_im1, PointF[] points_im2, StereoCamera stereocamera, GraphicGL graphicGL = null, Image<Bgr, byte>[] color_im = null)
        {
            var points3d_1 = computePointsCam(points_im1, stereocamera.cameraCVs[0], color_im[0]);         
            var points3d_2 = computePointsCam(points_im2, stereocamera.cameraCVs[1], color_im[1]);

            var ps1 = comp_points_for_gpu(points3d_1, stereocamera.cameraCVs[0].matrixCS);
            var ps2 = comp_points_for_gpu(points3d_2, stereocamera.cameraCVs[1].matrixCS);
            var points_cam2b = graphicGL.cross_flat_gpu(Point3d_GL.toData(ps1), Point3d_GL.toData(ps2));
           


            return points_cam2b;
        }

        public static Point3d_GL[] comp_points_for_gpu(Point3d_GL[] points_im, Matrix<double> matrix )
        {
            var ps3d = new Point3d_GL[points_im.Length+1];
            ps3d[0] = matrix * new Point3d_GL(0, 0, 0);
            for(int i = 0; i < points_im.Length; i++)
            {
                ps3d[i+1] = matrix * points_im[i];
            }
            return ps3d;
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
        public static Line3d_GL[] computeTracesCam(PointF[] points_im, CameraCV cameraCV,GraphicGL graphicGL=null)
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

        public static Line3d_GL[] computeTracesCam(Point3d_GL[] points_im, Matrix<double> matrix = null)
        {
            var lines3d = new Line3d_GL[points_im.Length];
            for (int i = 0; i < lines3d.Length; i++)
            {
                if(matrix!=null)
                {
                    lines3d[i] = new Line3d_GL(
                   matrix * points_im[i],
                   matrix * new Point3d_GL(0, 0, 0));
                }
                else
                {
                    lines3d[i] = new Line3d_GL(
                    points_im[i],
                    new Point3d_GL(0, 0, 0));
                }
                
            }
            return lines3d;
        }

        public static Polygon3d_GL[] computePolygonsCam(Point3d_GL[] points_im, Matrix<double> matrix = null)
        {
            var polygons3d = new Polygon3d_GL[points_im.Length-1];
            for (int i = 0; i < polygons3d.Length; i++)
            {
                if (matrix != null)
                {
                    polygons3d[i] = new Polygon3d_GL(
                    matrix * new Point3d_GL(0, 0, 0),
                    matrix * points_im[i],
                    matrix * points_im[i + 1]
                    );
                }
                else
                {
                    polygons3d[i] = new Polygon3d_GL(
                    new Point3d_GL(0, 0, 0),
                    points_im[i],
                    points_im[i + 1]
                    );
                }
                
            }
            return polygons3d;
        }

        public static Point3d_GL[] computePointsCam(PointF[] points_im, CameraCV cameraCV, Image<Bgr, byte> image = null)
        {
            var points3d = new Point3d_GL[points_im.Length];
            for (int i = 0; i < points3d.Length; i++)
            {
                points3d[i] = cameraCV.point3DfromCam(points_im[i]);
                if(image != null)
                {
                    var color = image[(int)points_im[i].Y, (int)points_im[i].X];
                    points3d[i].color = new Colo3d_GL(color.Red/255, color.Green / 255, color.Blue / 255);
                }
                

            }
            return points3d;
        }




    }
}
