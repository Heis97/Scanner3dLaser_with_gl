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
        public Point3d_GL[] ps;
        public Line3d_GL[] lines;
        public LaserSurface(Point3d_GL[] points)
        {
            flat3D = computeSurface(points);
        }
        public LaserSurface(Mat[] mats, Mat[] origs, CameraCV cameraCV, PatternType patternType, bool compPos = true,GraphicGL graphicGL = null)
        {
            calibrate(mats, origs, cameraCV,patternType, graphicGL, compPos);
        }
        public LaserSurface(Mat mats,  CameraCV cameraCV, PatternType patternType, GraphicGL graphicGL = null)
        {
            calibrate_step(mats,  cameraCV, patternType, graphicGL);
        }
        public LaserSurface()
        {
        }
        public Flat3d_GL computeSurface(Point3d_GL[] points, GraphicGL graphicGL = null)
        {
            if (points.Length < 3)
            {
                return Flat3d_GL.notExistFlat();
            }
            var flat = new Flat3d_GL(points[1], points[2], points[4]);
            var flat1 = new Flat3d_GL(points[0], points[2], points[3]);
            this.ps = points.ToArray();
            // if (flat.A<)
            //Console.WriteLine(flat);
            // Console.WriteLine(flat1);
            //Console.WriteLine("_____________");
            return (flat+flat1)/2;
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

            //graphicGL?.addMesh(Point3d_GL.toMesh(ps.ToArray()),OpenGL.PrimitiveType.Points, 0.9f);
            
            flat3D = computeSurface(ps.ToArray(), graphicGL);


            //var flat3D_in_scene = computeSurface(ps_in_scene.ToArray());

            //graphicGL?.addFlat3d_XZ(flat3D);

            
           // Console.WriteLine(flat3D/flat3D.A);
            return true;
        }

        public bool calibrate_step(Mat mat, CameraCV cameraCV, PatternType patternType, GraphicGL graphicGL = null)
        {

            var ps = points3dInCam_step(mat.Clone(), cameraCV, patternType,5, graphicGL);

            //graphicGL?.addMesh(Point3d_GL.toMesh(ps.ToArray()),OpenGL.PrimitiveType.Points, Color3d_GL.red());

            flat3D = computeSurface(ps.ToArray());
            


            //var flat3D_in_scene = computeSurface(ps_in_scene.ToArray());

            //var flat = graphicGL?.addFlat3d_XZ(flat3D);
            //graphicGL?.buffersGl.setTranspobj(flat, 0.5f);


            // Console.WriteLine(flat3D/flat3D.A);
            return true;
        }

        static Point3d_GL[] points3dInCam(Mat mat, Mat orig, CameraCV cameraCV,PatternType patternType,GraphicGL graphicGL = null,bool compPos = true,bool oneMat = false,int div=-1)
        {
            var points = Detection.detectLineDiff(mat,5,0.05f,false,false);
            var mat_p = UtilOpenCV.drawPointsF(mat,points,255,0,0);
           /* CvInvoke.Imshow("asf", mat_p);
            CvInvoke.WaitKey();*/
            var ps = new PointF[0];
            double z = 0;
            if(div>0)
            {
                ps = takePointsForFlat(points, oneMat,div);
            }
            else
            {
                if (oneMat)
                {
                    ps = takePointsForFlat(points, oneMat);
                    Console.Write(ps[0] + " " + ps[1] + " " + ps[2] + " " + ps[3] + " ");
                }
                else
                {
                    ps = takePointsForFlat(points);
                }
            }
            
            
            
            if(compPos)
            {
                if(orig!=null)
                {
                    cameraCV.compPos(orig, patternType,cameraCV.pattern_size);
                }
                else
                {
                    cameraCV.compPos(mat, patternType, cameraCV.pattern_size);
                }
            }
            var lines = PointCloud.computeTracesCam(ps, cameraCV,graphicGL);
            var ps3d = new List<Point3d_GL>();
            ps3d.AddRange( PointCloud.intersectWithFlat(new Line3d_GL[] { lines[0], lines[1] }, zeroFlatInCam(cameraCV.matrixSC,z)));
           // graphicGL?.addFlat3d_XZ(zeroFlatInCam(cameraCV.matrixSC, z));
            if (oneMat)
            {
                z = 40;
                ps3d.AddRange(PointCloud.intersectWithFlat(new Line3d_GL[] { lines[2], lines[3] }, zeroFlatInCam(cameraCV.matrixSC, z)));
            }


           // graphicGL?.addFlat3d_XZ(zeroFlatInCam(cameraCV.matrixSC, z));
            //graphicGL?.addFlat3d_XZ(zeroFlatInCam(cameraCV.matrixSC, 0)+(zeroFlatInCam(cameraCV.matrixSC, 4)- zeroFlatInCam(cameraCV.matrixSC, 0))/2);

            return ps3d.ToArray();          
        }

        public static PointF[] order_y(PointF[] ps)
        {
            return (from p in ps
                    orderby p.Y
                    select p).ToArray();
        }
        public static PointF[] order_x(PointF[] ps)
        {
            if (ps == null) return null;
            return (from p in ps
                    orderby p.X
                    select p).ToArray();
        }
        public static int ind_y(PointF[] ps,float y)
        {
            for(int i = 0; i < ps.Length; i++)
            {
                if (Math.Abs(ps[i].Y-y)<0.01)
                {
                    return i;
                }
            }
            return 0;
        }

        Point3d_GL[] points3dInCam_step(Mat mat, CameraCV cameraCV, PatternType patternType, int div = -1, GraphicGL graphicGL = null)
        {

            mat = cameraCV.undist(mat);
            var points = order_y(Detection.detectLineDiff(mat, 10,0,false,true,true));
            var ps_m = order_y(Detection.x_max_claster(points,3));//x_min
                                                                  // CvInvoke.Imshow("orig_las_step", UtilOpenCV.drawPointsF(mat, ps_m, 255, 0, 0));
                                                                  //CvInvoke.WaitKey();
           /* var y1 = 325;
            var y2 = 270;
            var y3 = 240;
            var p_i_1 = Detection.p_in_ps_by_y(points, y1);
            var p_i_2 = Detection.p_in_ps_by_y(points, y2);
            var p_i_3 = Detection.p_in_ps_by_y(points, y3);
            Console.Write(" " + points[p_i_1].X + " " + points[p_i_2].X + " " + points[p_i_3].X);*/


            var p_mm = new PointF(ps_m[0].Y, ps_m[ps_m.Length-1].Y);
            var start = ind_y(points, p_mm.X);
            var len  = ind_y(points, p_mm.Y) - start;
            var ps_m_2 = points.ToList().GetRange(start, len).ToArray();

            var mat_p = UtilOpenCV.drawPointsF(mat, points, 255, 0, 0);
            
            //CvInvoke.Imshow("asf", mat_p);
            //CvInvoke.WaitKey();

            var ps = takePointsForFlat(ps_m_2, false, div);

            var ps_l = ps.ToList();
            var len_from_board = 30;
            ps_l.Add(new PointF(points[len_from_board]));
            ps_l.Add(new PointF(points[points.Length-1- len_from_board]));
            ps = ps_l.ToArray();
            
          /*  var orig_c = mat.Clone();
            UtilOpenCV.drawPointsF(orig_c, ps,255,0,255,2,true);
            UtilOpenCV.drawPointsF(orig_c, ps_m, 0,255,  0, 2);
            CvInvoke.Imshow("corn", orig_c);
            CvInvoke.WaitKey();*/

            var lines = PointCloud.computeTracesCam(ps, cameraCV);
            this.lines = lines;

            var ps3d = new List<Point3d_GL>();
           

            double z = 0;
            ps3d.AddRange(PointCloud.intersectWithFlat(new Line3d_GL[] { lines[0], lines[4] }, zeroFlatInCam(cameraCV.matrixSC, z)));
           
            //var flat1 = graphicGL?.addFlat3d_XY(zeroFlatInCam(cameraCV.matrixSC, z));
            //graphicGL?.buffersGl.setTranspobj(flat1, 0.5f);
            //Console.WriteLine("cameraCV.matrixSC");
            // prin.t(cameraCV.matrixSC);
            //graphicGL?.addFrame(cameraCV.matrixSC, 30,"cs");
            z = -10;
            ps3d.AddRange(PointCloud.intersectWithFlat(new Line3d_GL[] { lines[1], lines[3] }, zeroFlatInCam(cameraCV.matrixSC, z)));
            //var flat2 = graphicGL?.addFlat3d_XY(zeroFlatInCam(cameraCV.matrixSC, z));
           // graphicGL?.buffersGl.setTranspobj(flat2, 0.5f);

            z = -20;
            ps3d.AddRange(PointCloud.intersectWithFlat(new Line3d_GL[] { lines[2] }, zeroFlatInCam(cameraCV.matrixSC, z)));
           
           // var flat3 = graphicGL?.addFlat3d_XY(zeroFlatInCam(cameraCV.matrixSC, z),null);
           // graphicGL?.buffersGl.setTranspobj(flat3, 0.5f);

            return ps3d.ToArray();
        }

        //half--div--half
        public static PointF[] takePointsForFlat(PointF[] ps,bool oneMat=false,int div = -1)
        {
            
            if(div>0)
            {
                var quart = (int)(ps.Length / ((double)div));
                var psС = new PointF[div];
                for(int i=0; i<div;i++)
                {
                    psС[i] = ps[quart * i + quart / 2];
                }
                return psС;
            }
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

        public static Flat3d_GL zeroFlatInCam(Matrix<double> matrix,double z = 0)
        {
            var p1 = new Point3d_GL(100, 0, z);
            var p2 = new Point3d_GL(0, 0, z);
            var p3 = new Point3d_GL(0, 100, z);
            if (matrix ==null)
            {
                //Console.WriteLine("matrxZeroFlat   NULL");
                
                return new Flat3d_GL(p1, p2, p3);
            }
            p1 = matrix * new Point3d_GL(100, 0, z);
            p2 = matrix * new Point3d_GL(0, 0, z);
            p3 = matrix * new Point3d_GL(0, 100, z);
            return new Flat3d_GL(p1, p2, p3);
        }

        public static Flat3d_GL zeroFlatInCam_XZ(Matrix<double> matrix, double y = 0)
        {
            var p1 = new Point3d_GL(100, y, 0);
            var p2 = new Point3d_GL(0, y, 0);
            var p3 = new Point3d_GL(0, y, 100);
            if (matrix == null)
            {
                //Console.WriteLine("matrxZeroFlat   NULL");

                return new Flat3d_GL(p1, p2, p3);
            }
            p1 = matrix * new Point3d_GL(100, y, 0);
            p2 = matrix * new Point3d_GL(0, y, 0);
            p3 = matrix * new Point3d_GL(0, y, 100);
            return new Flat3d_GL(p1, p2, p3);
        }
    }
}
