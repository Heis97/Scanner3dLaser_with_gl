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
            cameraCV.compPos(mat, PatternType.Chess,cameraCV.pattern_size);
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
            cameraCV.compPos(orig, patternType, cameraCV.pattern_size);  
            var points_im = Detection.detectLineDiff(mat,5,0.05f,false,false);
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
        public bool addPointsLinLas_step(Mat mat, Image<Bgr,byte> orig, double LinPos, CameraCV cameraCV, LinearAxis linearAxis, PatternType patternType)
        {
            var points_im = Detection.detectLineDiff(mat, 5, 0.05f, false, true);
            //var points_im = Detection.detectLineDiff(mat);
            if(points_im == null) return false;
            Console.WriteLine(points_im.Length);
          /*var orig = mat.Clone();
            UtilOpenCV.drawPoints(orig, points_im, 0, 255, 0);
            CvInvoke.Imshow("ps", orig);
            CvInvoke.WaitKey();*/
            
            var points_cam = fromLines(points_im, cameraCV, linearAxis.getLaserSurf(LinPos));
            points_cam = color_points3d(points_im, points_cam, orig);
            //graphicGL.addFlat3d_YZ(linearAxis.getLaserSurf(LinPos),null,0.1f,0.1f,0.4f);
            points3d_cur = points_cam;
            //points3d_cur = camToScene(points_cam, cameraCV.matrixCS);
            //prin.t(cameraCV.matrixCS);
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
            var points_cam = fromStereoLaser(points_im1, points_im2, stereocamera, graphicGL, color_im);
            //var points_cam = fromStereoLaser_gpu(points_im1, points_im2, stereocamera, graphicGL, color_im);
            points3d_cur = points_cam;
            //points3d_cur = camToScene(points_cam, stereocamera.cameraCVs[1].matrixCS);
            var ps_list = points3d.ToList();
            ps_list.AddRange(points3d_cur);
            points3d_lines.Add(points3d_cur);
            points3d = ps_list.ToArray();
            return true;
        }

        public bool addPoints2dStereoLas(Mat[] mat, StereoCamera stereocamera, ScannerConfig config)
        {
            if (config.distort)
            {
                mat[0] = stereocamera.cameraCVs[0].undist(mat[0]);
                mat[1] = stereocamera.cameraCVs[1].undist(mat[1]);
            }
            var points_im1 = Detection.detectLineDiff(mat[0], config);
            var points_im2 = Detection.detectLineDiff(mat[1], config);

            //var srx1 = Detection.detectLineSensor(mat[0], 7)[0];
            //var srx2 = Detection.detectLineSensor(mat[1], 7)[0];
            //Console.WriteLine(srx1.X + " " + srx2.X);
            if (points_im1 != null && points_im2 != null)
                if(points_im1.Length == points_im2.Length)
                {
                    stereocamera.cameraCVs[0].scan_points.Add(points_im1);
                    stereocamera.cameraCVs[1].scan_points.Add(points_im2);
                    //Console.WriteLine(points_im1.Length + " " + points_im2.Length);
                }
            
            return true;
        }

        public bool addPoints2dStereoLas_sync(Mat[] mat, StereoCamera stereocamera, double k, int cam_min, int cam_max,ScannerConfig config)
        {

            if (config.distort)
            {
                mat[0] = stereocamera.cameraCVs[cam_min - 1].undist(mat[0]);
                mat[1] = stereocamera.cameraCVs[cam_max - 1].undist(mat[1]);
                mat[2] = stereocamera.cameraCVs[cam_max - 1].undist(mat[2]);
            }

            
            var points_im1 = Detection.detectLineDiff(mat[0], config);
            var points_im_max = Detection.detectLineDiff(mat[1], config);
            var points_im_max_prev = Detection.detectLineDiff(mat[2], config);


            var points_im2 = PointF.averX(points_im_max_prev, points_im_max, k);

            //var srx1 = Detection.detectLineSensor(mat[0], 7)[0];
            //var srx2 = Detection.detectLineSensor(mat[1], 7)[0];
           // Console.WriteLine(srx1.X + " " + srx2.X+ " " + points_im2[(int)points_im2.Length/2].X);
            /*var m1 = mat[0].Clone();
            var m2 = mat[1].Clone();
            m1 = UtilOpenCV.drawPointsF(m1, points_im1, 0, 255, 0, 1);
            m2 = UtilOpenCV.drawPointsF(m2, points_im2, 0, 255, 0, 1);
            CvInvoke.Imshow("m1", m1);
            CvInvoke.Imshow("m2", m2);
            CvInvoke.WaitKey();*/
            //points_im2 = points_im_max;
            if (points_im1 != null && points_im2 != null)
                if (points_im1.Length == points_im2.Length)
                {
                    //PointF.check_exist(points_im1);
                   // Console.WriteLine("add_sync");
                   //Console.WriteLine()
                    stereocamera.cameraCVs[cam_min - 1].scan_points.Add(points_im1);
                    stereocamera.cameraCVs[cam_max - 1].scan_points.Add(points_im2);
                    //Console.WriteLine(points_im1.Length + " " + points_im2.Length);
                }

            return true;
        }

        public bool addPoints2dSingLas(Mat mat, CameraCV cameraCV, bool undist,bool orig = false)
        {
            if (undist)
            {
                mat = cameraCV.undist(mat);
            }
            PointF[] points_im;
            if (orig) points_im = Detection.detectLineDiff(mat, 7,0.05f,false,true,true);
            else points_im = Detection.detectLineDiff(mat, 7);
          
            if (points_im != null)
            {
                cameraCV.scan_points.Add(points_im);
                Console.WriteLine(points_im.Length);
            }
            return true;
        }

        public void comp_cross(StereoCamera stereocamera)
        {
            var points_cam = fromStereoLaser_gpu_all(
               stereocamera.cameraCVs[0].scan_points.ToArray(),
               stereocamera.cameraCVs[1].scan_points.ToArray(),
               stereocamera, graphicGL, color_im);
            Console.WriteLine("gpu computed.");
            if (points_cam == null) return;
            for (int i= 0; i < points_cam.Length; i++)
            {
                points3d_cur = points_cam[i];
                if(points3d_cur != null)
                {
                    if(points3d_cur.Length>0)
                    {
                        //points3d_cur = camToScene(points_cam, stereocamera.cameraCVs[1].matrixCS);
                        var ps_list = points3d.ToList();
                        ps_list.AddRange(points3d_cur);
                        points3d_lines.Add(points3d_cur);
                        points3d = ps_list.ToArray();
                    }
                }
                
            }
            
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
             //Console.WriteLine(data_to_str(polygons3d_2, lines3d_1));

             var points_cam2b = Polygon3d_GL.createLightFlat(polygons3d_2, lines3d_1);
            // graphicGL?.addMesh(Polygon3d_GL.toMesh(polygons3d_1), OpenGL.PrimitiveType.Triangles);
            //  graphicGL?.addMesh(Polygon3d_GL.toMesh(polygons3d_2), OpenGL.PrimitiveType.Lines);

            //Console.WriteLine(graphicGL.toStringBuf(Point3d_GL.toMesh(points_cam2b), points_cam2b.Length, 0, "orig ps"));
            return points_cam2b;
        }
         
        static string data_to_str(Polygon3d_GL[] pols, Line3d_GL[] lns)
        {
            var str_data = new StringBuilder();
            /*foreach (var ln in lns) str_data.Append(ln.p_end + " | "); str_data.Append(" \n");
            foreach (var ln in lns) str_data.Append(ln.p + " | "); str_data.Append(" \n");
            foreach (var pol in pols) str_data.Append(pol.ps[0]+ " | "); str_data.Append(" \n");
            foreach (var pol in pols) str_data.Append(pol.ps[1] + " | "); str_data.Append(" \n");
            foreach (var pol in pols) str_data.Append(pol.ps[2] + " | "); str_data.Append(" \n");
            foreach (var pol in pols) str_data.Append(pol.flat3D + " | "); str_data.Append(" \n");*/
            for (int i = 0; i < pols.Length; i++) str_data.Append(pols[i].crossLine_deb(lns[i]) + " | "); str_data.Append(" \n");
            foreach (var ln in lns) str_data.Append(ln.k + " | "); str_data.Append(" \n");
            //foreach (var ln in lns) str_data.Append(ln.p + " | "); str_data.Append(" \n");
          
            foreach (var pol in pols) str_data.Append(pol.v1 + " | "); str_data.Append(" \n");
            foreach (var pol in pols) str_data.Append(pol.v2 + " | "); str_data.Append(" \n");
            foreach (var pol in pols) str_data.Append(pol.v3 + " | "); str_data.Append(" \n");
            foreach (var pol in pols) str_data.Append(pol.flat3D + " | "); str_data.Append(" \n");

            return str_data.ToString();
        }

        public static Point3d_GL[] fromStereoLaser_gpu(PointF[] points_im1, PointF[] points_im2, StereoCamera stereocamera, GraphicGL graphicGL = null, Image<Bgr, byte>[] color_im = null)
        {
            var points3d_1 = computePointsCam(points_im1, stereocamera.cameraCVs[0], color_im[0]);         
            var points3d_2 = computePointsCam(points_im2, stereocamera.cameraCVs[1], color_im[1]);

            var ps1 = comp_points_for_gpu(points3d_1, stereocamera.cameraCVs[0].matrixCS);
            var ps2 = comp_points_for_gpu(points3d_2, stereocamera.cameraCVs[1].matrixCS);
            var points_cam2b = graphicGL.cross_flat_gpu(Point3d_GL.toData(ps1), Point3d_GL.toData(ps2));

            //Console.WriteLine(graphicGL.toStringBuf(Point3d_GL.toMesh(points_cam2b), 3, 0, "orig ps"));

            return points_cam2b;
        }

        public static Point3d_GL[] comp_stereo_ps(PointF[] points_im1, PointF[] points_im2, StereoCamera stereocamera, GraphicGL graphicGL = null, Image<Bgr, byte>[] color_im = null)
        {
            var points3d_1 = computePointsCam(points_im1, stereocamera.cameraCVs[0], null);
            var points3d_2 = computePointsCam(points_im2, stereocamera.cameraCVs[1], null);
            var m1 = stereocamera.cameraCVs[0].matrixCS;
            if (stereocamera.scan_coord_sys == StereoCamera.mode.camera)
            {
                m1 = UtilMatr.eye_matr(4);
            }
            if (stereocamera.scan_coord_sys == StereoCamera.mode.world)
            {
                if (stereocamera.Bbf != null && stereocamera.Bfs != null)
                    m1 = stereocamera.Bbf * stereocamera.Bfs;//or inverse                
            }
            points3d_1=  Point3d_GL.mult(points3d_1, 300);
            points3d_2 = Point3d_GL.mult(points3d_2, 300);
            var m2 = m1 * stereocamera.R;
            var ps1 = comp_points_for_gpu(points3d_1, m1);
            var ps2 = comp_points_for_gpu(points3d_2, m2);
            //graphicGL?.addLineFanMesh(ps1[0], ps1, Color3d_GL.red());
            //graphicGL?.addLineFanMesh(ps2[0], ps2, Color3d_GL.green());
            var ps3d = comp_stereo_ps(ps1, ps2);
            return ps3d;
        }

        public static (Point3d_GL[], Point3d_GL[]) comp_stereo_ps_from_cam(Point3d_GL[] points3d_1, Point3d_GL[] points3d_2, StereoCamera stereocamera, GraphicGL graphicGL = null, Image<Bgr, byte>[] color_im = null)
        {
            var m1 = stereocamera.cameraCVs[0].matrixCS;
            if (stereocamera.scan_coord_sys == StereoCamera.mode.camera)
            {
                m1 = UtilMatr.eye_matr(4);
            }
            if (stereocamera.scan_coord_sys == StereoCamera.mode.world)
            {
                if (stereocamera.Bbf != null && stereocamera.Bfs != null)
                    m1 = stereocamera.Bbf * stereocamera.Bfs;//or inverse                
            }

            var m2 = m1 * stereocamera.R;
            var ps1 = Point3d_GL.multMatr(points3d_1, m1);
            var ps2 = Point3d_GL.multMatr(points3d_2, m2);
            //graphicGL?.addLineFanMesh(ps1[0], ps1, Color3d_GL.red());
            //graphicGL?.addLineFanMesh(ps2[0], ps2, Color3d_GL.green());
            return (ps1,ps2); 
        }

        public static Point3d_GL[] comp_stereo_ps(Point3d_GL[] ps1, Point3d_GL[] ps2)//first point - camera pos
        {
            var ps3d = new List<Point3d_GL>();
            var v_cc = ps2[0] - ps1[0];

            for(int i = 1; i < ps1.Length; i++)
            {
                var v_i = ps1[i] - ps1[0];
                var v_n = v_i|v_cc;
                var p_n = ps1[0] + v_n;
                var f1 = new Flat3d_GL(ps1[0],ps1[i],v_n);
                v_i = ps2[i] - ps2[0];
                v_n = v_i | (-v_cc);
                p_n = ps2[0] + v_n;
                var f2 = new Flat3d_GL(ps2[0], ps2[i], v_n);
                var f3 = new Flat3d_GL(ps1[0], ps2[0], ps1[i]);
                var p3d = Flat3d_GL.cross(f1, f2, f3);
                ps3d.Add(p3d);
            }
            return ps3d.ToArray();
        }

        public static Point3d_GL[][] fromStereoLaser_gpu_all(PointF[][] points_im1, PointF[][] points_im2, StereoCamera stereocamera, GraphicGL graphicGL = null, Image<Bgr, byte>[] color_im = null)
        {
            /*for(int i = 0; i < points_im1.Length; i++)
            {
                PointF.check_exist(points_im1[i]);
                PointF.check_exist(points_im2[i]);
                Console.WriteLine("comp_sync");
            }*/
           
            var points3d_1 = computePointsCam_2d(points_im1, stereocamera.cameraCVs[0], color_im[0]);
            var points3d_2 = computePointsCam_2d(points_im2, stereocamera.cameraCVs[1], color_im[1]);
            

            var m1 = stereocamera.cameraCVs[0].matrixCS;

            if(stereocamera.scan_coord_sys == StereoCamera.mode.camera)
            {
                m1 = UtilMatr.eye_matr(4);
            }
            if (stereocamera.scan_coord_sys == StereoCamera.mode.world)
            {
                
                if (stereocamera.Bbf!= null && stereocamera.Bfs!= null)
                    m1 = stereocamera.Bbf * stereocamera.Bfs;//or inverse                
            }

            if (stereocamera.scan_coord_sys == StereoCamera.mode.world_virt)
            {

                if (stereocamera.Bbf != null)
                    m1 = stereocamera.Bbf;//or inverse                
            }
            Console.WriteLine(stereocamera.scan_coord_sys);
            var m2 = m1 * stereocamera.R;
            var ps1 = comp_points_for_gpu_2d(points3d_1, m1);
            var ps2 = comp_points_for_gpu_2d(points3d_2, m2);
            Console.WriteLine("points prepared.");

            var points_cam2b = graphicGL.cross_flat_gpu_all(ps1, ps2);

            //Console.WriteLine(graphicGL.toStringBuf(Point3d_GL.toMesh(points_cam2b), 3, 0, "orig ps"));

            return points_cam2b;
        }

        static Point3d_GL[][] comp_points_for_gpu_2d(Point3d_GL[][] ps_cam, Matrix<double> matrix)
        {
            var ps_3d = new List<Point3d_GL[]>();
            for(int i = 0; i < ps_cam.Length; i++)
            {
                ps_3d.Add(comp_points_for_gpu(ps_cam[i], matrix));
            }
            return ps_3d.ToArray();
        }
        public static Point3d_GL[] comp_points_for_gpu(Point3d_GL[] points_im, Matrix<double> matrix)
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

        public static Point3d_GL[] computePointsCam(PointF[] points_im, CameraCV cameraCV, Image<Bgr, byte> image_in = null)
        {
            var points3d = new Point3d_GL[points_im.Length];
            Image<Bgr, byte> image = cameraCV.undist(image_in.Mat).ToImage<Bgr, byte>();
            for (int i = 0; i < points3d.Length; i++)
            {
                points3d[i] = cameraCV.point3DfromCam(points_im[i]);
                points3d[i].exist = points_im[i].exist;
                if (image != null)
                {
                    var y= (int)points_im[i].Y;
                    var x = (int)points_im[i].X;
                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        var color = image[y, x];
                        points3d[i].color = new Color3d_GL((float)color.Red / 255, (float)color.Green / 255, (float)color.Blue / 255);
                        
                        //points3d[i].color = new Color3d_GL(x, y, (float)color.Blue / 255);
                    }                   
                }               
            } 
            return points3d;
        }
        public static PointF[] computePointsCam3d_to2d(Point3d_GL[] points3d, CameraCV cameraCV, Image<Bgr, byte> image = null)
        {
            var points2d = new PointF[points3d.Length];
            for (int i = 0; i < points3d.Length; i++)
            {
                points2d[i] = cameraCV.point2DfromCam(points3d[i]);
                points2d[i].exist = points3d[i].exist;
            }
            return points2d;
        }
        static Point3d_GL[] color_points3d(PointF[] points_im, Point3d_GL[] points3d, Image<Bgr,byte> image)
        {
            for (int i = 0; i < points3d.Length; i++)
            {
                if (image != null)
                {
                    var y = (int)points_im[i].Y;
                    var x = (int)points_im[i].X;
                    if (x >= 0 && x < image.Width && y >= 0 && y < image.Height)
                    {
                        var color = image[y, x];
                        points3d[i].color = new Color3d_GL((float)color.Red / 255, (float)color.Green / 255, (float)color.Blue / 255);
                    }

                }
            }
            return points3d;
        }

        public static Point3d_GL[][] computePointsCam_2d(PointF[][] points_im, CameraCV cameraCV, Image<Bgr, byte> image = null)
        {
            var points3d = new Point3d_GL[points_im.Length][];
            for (int i = 0; i < points_im.Length; i++)
            {
                points3d[i] = computePointsCam(points_im[i], cameraCV, image);
            }
            return points3d;
        }


    }
}
