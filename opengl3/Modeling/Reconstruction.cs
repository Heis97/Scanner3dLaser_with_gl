using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace opengl3
{
    public enum DirectionType { Up, Down }
    public enum SolveType { Simple, Complex }

    static public class Reconstruction
    {
        static public void loadScan_leg(string path_pos_calib, string path_laser_calib, string path_scan, double FoV, double Side, int bin_pos = 40, SolveType type = SolveType.Simple, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var frames_pos = FrameLoader.loadImages(path_pos_calib, FoV, Side, bin_pos);
            var frames_las = FrameLoader.loadImages_simple(path_laser_calib);
            var frames_scan = FrameLoader.loadImages_simple(path_scan);
            var zero_frame = findZeroFrame(frames_las);

            var fr = from f in frames_scan
                     orderby f.pos_rob.y
                     select f;
            frames_scan = fr.ToArray();

            var laserFlat = calibrLaser(frames_las, frames_pos[0], zero_frame, 252, type);

            var model = paintScanningModel(laserFlat, frames_scan, frames_pos[0], zero_frame, type);
            Console.WriteLine("loading done");
            //if (offset_model != null)
            // {
            //Console.WriteLine("x = " + offset_model.x + "y = " + offset_model.y + "z = " + offset_model.z);
            // offset_model.x = 0;
            //offset_model.y = 0;
            //offset_model.z = 0;
            //GL1.addGLMesh(model, PrimitiveType.Triangles, (float)-offset_model.x, (float)-offset_model.y, (float)-offset_model.z, r, g, b);
            // }
        }
        static public void loadScan(string path_pos_calib, string path_laser_calib, string path_scan, string path_basis, double FoV, double Side, int bin_pos = 40, SolveType type = SolveType.Simple, float r = 0.1f, float g = 0.1f, float b = 0.1f, ComboBox comboBox = null)
        {
            var frames_pos = FrameLoader.loadImages(path_pos_calib, FoV, Side, bin_pos, 15, true);
            var frames_las = FrameLoader.loadImages_simple(path_laser_calib);
            var frames_scan = FrameLoader.loadImages_simple(path_scan);
            var zero_frame = findZeroFrame(frames_las);
            var robToCam = FrameLoader.loadImages_basis(path_basis, FoV, Side, bin_pos);

            if(comboBox!=null)
            {
                comboBox.Items.AddRange(frames_pos);
                comboBox.Items.AddRange(frames_las);
                comboBox.Items.AddRange(frames_scan);
                //comboBox.Items.AddRange(robToCam);
            }

            var fr = from f in frames_scan
                     orderby f.pos_rob.y
                     select f;
            frames_scan = fr.ToArray();

            //laserFlat = calibrLaser(frames_las, frames_pos[0], zero_frame, (int)red_c, type,robToCam,DirectionType.Down);


            // var model = paintScanningModel_pts(laserFlat, frames_scan, frames_pos[0], zero_frame, type, robToCam, DirectionType.Down);
            //addPointMesh(model, r, g, b);
            //Console.WriteLine("loading done");
            //Console.WriteLine("x = " + offset_model.x + "y = " + offset_model.y + "z = " + offset_model.z);
            //addGLMesh(model, PrimitiveType.Points, 0, 0, 0, r, g, b);

            //var laserFlat = calibrLaser(frames_las, frames_pos[0], zero_frame, 252, type, robToCam, DirectionType.Down);
           // var model = paintScanningModel(laserFlat, frames_scan, frames_pos[0], zero_frame, type, robToCam, DirectionType.Down);
           // Console.WriteLine("loading done");


            //Console.WriteLine("x = " + offset_model.x + "y = " + offset_model.y + "z = " + offset_model.z);
            //GL1.addGLMesh(model, PrimitiveType.Triangles, 0, 0, 0, 1, 0, 0);
        }
        static public Point3d_GL[] paintScanningModel_pts(List<Flat_4P> laserFlat, List<Frame> videoframes, Frame calib_frame, Frame zero_frame, SolveType type, double[,] matr = null, DirectionType directionType = DirectionType.Down)
        {
            List<Frame> frames = new List<Frame>();

            var points = new List<Point3d_GL>();
            var fr = from f in videoframes
                     orderby f.pos_rob.x
                     select f;
            var vfrs = fr.ToList();
            foreach (var f in vfrs)
            {
                var mat_im = new Mat();
                f.im.CopyTo(mat_im);
                var stroka = ContourAnalyse.findContourZ_real(mat_im, null, 252, directionType);
                f.points = UtilMatr.moveToCentr(UtilMatr.doubleToPointF_real(stroka), f.size);

                frames.Add(f);
                Console.WriteLine(f);
            }
            float r = 0.1f;
            float g = 0.1f;
            float b = 0.1f;
            var mesh = new List<float>();
            var flatInds = new int[frames.Count, frames[0].points.Length];
            var flatLasInds = new int[laserFlat.Count];
            List<Point3d_GL> points3d = new List<Point3d_GL>();
            var colr = 1;
            for (int i = 0; i < frames.Count; i++)
            {
                for (int j = 0; j < frames[i].points.Length; j++)
                {
                    var p = calcPoint(frames[i].points[j], calib_frame, frames[i], laserFlat, zero_frame, type, matr);
                    if (p.exist)
                    {
                        points3d.Add(new Point3d_GL(p.x, p.y, p.z));
                    }
                }
            }
            Console.WriteLine(frames.Count + " LEN_FR");
            return points3d.ToArray();
        }
        static public float[] paintScanningModel(List<Flat_4P> laserFlat, Frame[] videoframes, Frame calib_frame, Frame zero_frame, SolveType type, double[,] matr = null, DirectionType directionType = DirectionType.Down)
        {
            List<Frame> frames = new List<Frame>();

            var points = new List<Point3d_GL>();
            var fr = from f in videoframes
                     orderby f.pos_rob.x
                     select f;
            var vfrs = fr.ToList();
            foreach (var f in vfrs)
            {
                var mat_im = new Mat();
                f.im.CopyTo(mat_im);
                var stroka = ContourAnalyse.findContourZ(mat_im, null, 252, directionType);
                f.points = UtilMatr.moveToCentr(UtilMatr.doubleToPointF(stroka), f.size);

                frames.Add(f);
                Console.WriteLine(f);
            }
            float r = 0.1f;
            float g = 0.1f;
            float b = 0.1f;
            double xmin = double.MaxValue;
            double xmax = double.MinValue;
            double ymin = double.MaxValue;
            double ymax = double.MinValue;
            double zmin = double.MaxValue;
            double zmax = double.MinValue;
            var mesh = new List<float>();
            var flatInds = new int[frames.Count, frames[0].points.Length];
            var flatLasInds = new int[laserFlat.Count];
            Point3d_GL[,] points3d = new Point3d_GL[frames.Count, frames[0].points.Length];
            var colr = 1;
            for (int i = 0; i < frames.Count; i++)
            {
                for (int j = 0; j < frames[i].points.Length; j++)
                {
                    var p = calcPoint(frames[i].points[j], calib_frame, frames[i], laserFlat, zero_frame, type, matr);
                    if (p.exist)
                    {
                        points3d[i, j] = new Point3d_GL(p.x, p.y, p.z);
                        flatInds[i, j] = colr;
                        //addPointMesh(new Point3d_GL[] { points3d[i, j] }, 1, 0, 0);
                        if (p.x < xmin)
                        {
                            xmin = p.x;
                        }
                        if (p.x > xmax)
                        {
                            xmax = p.x;
                        }
                        if (p.y < ymin)
                        {
                            ymin = p.y;
                        }
                        if (p.y > ymax)
                        {
                            ymax = p.y;
                        }
                        if (p.z < zmin)
                        {
                            zmin = p.z;
                        }
                        if (p.z > zmax)
                        {
                            zmax = p.z;
                        }
                    }
                }
            }
            #region colors
            /*
            for (int i = 0; i < frames.Count; i++)
            {
                for (int j = 0; j < frames[i].points.Length; j++)
                {
                    //Console.WriteLine(flatInds[i, j]);
                    if(flatInds[i,j]%6 == 0)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 1, 0, 0);
                    }
                    else if(flatInds[i, j] % 6 == 1)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 0, 1, 0);
                    }
                    else if (flatInds[i, j] % 6 == 2)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 0, 0, 1);
                    }
                    else if (flatInds[i, j] % 6 == 3)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 1, 1, 0);
                    }
                    else if (flatInds[i, j] % 6 == 4)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 0, 1, 1);
                    }
                    else if (flatInds[i, j] % 6 ==5)
                    {
                        addPointMesh(new Point3d_GL[] { points3d[i, j] }, 1, 0,1);
                    }
                }
            }

            for (int i = 0; i < laserFlat.Count; i++)
            {
                var ps = laserFlat[i].P;
                if (i % 6 == 0)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 1, 0, 0);
                }
                else if (i % 6 == 1)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 0, 1, 0);
                }
                else if (i % 6 == 2)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 0, 0, 1);
                }
                else if (i % 6 == 3)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 1, 1, 0);
                }
                else if (i % 6 == 4)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 0, 1, 1);
                }
                else if (i % 6 == 5)
                {
                    addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 1, 0, 1);
                }
                //addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, r, g, 0.1f);
            }
            */
            #endregion
            double offx = xmin + (xmax - xmin) / 2;
            double offy = ymin + (ymax - ymin) / 2;
            double offz = zmin + (zmax - zmin) / 2;
            Console.WriteLine(
                "xmin " + xmin + " " +
                "xmax " + xmax + " " +
                "ymin " + ymin + " " +
                "ymax " + ymax + " " +
                "zmin " + zmin + " " +
                "zmax " + zmax + " ");
            var offset_model = new Point3d_GL(offx, offy, offz);
            Console.WriteLine(frames.Count + " LEN_FR");
            for (int i = 0; i < frames.Count - 1; i++)
            {
                //Console.WriteLine(frames[0].points.Length - 2);
                for (int j = 0; j < frames[0].points.Length - 1; j++)
                {
                    //Console.WriteLine(i + " " + j);
                    var p1 = points3d[i, j];
                    var p2 = points3d[i + 1, j];
                    var p3 = points3d[i, j + 1];
                    if (p1.exist & p2.exist & p3.exist)
                    {
                        mesh.Add((float)p1.x); mesh.Add((float)p1.y); mesh.Add((float)p1.z);
                        mesh.Add((float)p2.x); mesh.Add((float)p2.y); mesh.Add((float)p2.z);
                        mesh.Add((float)p3.x); mesh.Add((float)p3.y); mesh.Add((float)p3.z);
                    }

                    p1 = points3d[i + 1, j];
                    p2 = points3d[i + 1, j + 1];
                    p3 = points3d[i, j + 1];
                    if (p1.exist & p2.exist  & p3.exist)
                    {
                        mesh.Add((float)p1.x); mesh.Add((float)p1.y); mesh.Add((float)p1.z);
                        mesh.Add((float)p2.x); mesh.Add((float)p2.y); mesh.Add((float)p2.z);
                        mesh.Add((float)p3.x); mesh.Add((float)p3.y); mesh.Add((float)p3.z);
                    }

                }
            }
            return mesh.ToArray();
        }
        static public Frame findZeroFrame(Frame[] frames)
        {
            var fr = from f in frames
                     orderby f.pos_rob.z
                     select f;
            var vfrs = fr.ToList();
            return vfrs[vfrs.Count - 1];
        }
        static public List<Flat_4P> calibrLaser(Frame[] videoframes, Frame calib_frame, Frame zero_frame, int bin, SolveType type, double[,] matr = null, DirectionType directionType = DirectionType.Down)
        {
            List<Frame> frames = new List<Frame>();
            List<Flat_4P> mesh = new List<Flat_4P>();
            Console.WriteLine("a----");
            var points = new List<Point3d_GL>();
            var fr = from f in videoframes
                     orderby f.pos_rob.z
                     select f;
            Console.WriteLine("b----");
            var vfrs = fr.ToList();
            foreach (var v in vfrs)
            {
                var f = v;
                var mat_im = new Mat();
                f.im.CopyTo(mat_im);
                var stroka = ContourAnalyse.findContourZ(mat_im, null, bin, directionType);
                f.points = UtilMatr.moveToCentr(Regression.regressionPoints(mat_im.Size, stroka, 80, 2), v.size);

                Console.WriteLine(f.points.Length);
                frames.Add(f);
                Console.WriteLine(f);
            }

            float r = 0.1f;
            float g = 0.8f;
            float b = 0.1f;

            var mesh_p = computeMeshFromFrames(frames.ToArray(), calib_frame, zero_frame, matr);
            var ps_tr = Regression.translPoints(mesh_p);
            var ps_ext = Regression.extendPoints(ps_tr);
            var ps_all = Regression.translPoints(ps_ext);

            if (type == SolveType.Complex)
            {
                for (int i = 1; i < ps_all.Length; i++)
                {
                    for (int j = 0; j < ps_all[0].Length - 1; j++)
                    {
                        var ps = new Point3d_GL[] { ps_all[i][j], ps_all[i][j + 1], ps_all[i - 1][j], ps_all[i - 1][j + 1] };
                        mesh.Add(new Flat_4P(ps));
                        //Console.WriteLine("e2----");
                        //addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, r, g, 0.1f);
                        //addPointMesh(new Point3d_GL[] { frames[i].pos_rob + del, ps[0], frames[i - 1].pos_rob + del, ps[2] }, r,g,0.5f);
                        //g += 0.1f;
                        //Console.WriteLine("e3----");
                    }
                    //r += 0.2f;
                }
            }
            else
            {
                var ps = new Point3d_GL[4];
                var del = calib_frame.pos_cam - calib_frame.pos_rob;
                ps[0] = calcPoint(frames[0].points[0], calib_frame, frames[0], zero_frame, matr);
                ps[1] = calcPoint(frames[0].points[frames[0].points.Length - 1], calib_frame, frames[0], zero_frame, matr);
                ps[2] = calcPoint(frames[frames.Count - 1].points[0], calib_frame, frames[frames.Count - 1], zero_frame, matr);
                ps[3] = calcPoint(frames[frames.Count - 1].points[frames[frames.Count - 1].points.Length - 1], calib_frame, frames[frames.Count - 1], zero_frame, matr);
                //addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, r, g, 0.5f);
                // addFrame(new Point3d_GL[] { frames[i].pos_rob + del, ps[0], frames[i - 1].pos_rob + del, ps[2] }, r, g, 0.5f);
                mesh.Add(new Flat_4P(ps));
            }
            return mesh;
        }

        static public Point3d_GL[][] computeMeshFromFrames(Frame[] frames, Frame calib_frame, Frame zero_frame, double[,] matr)
        {
            var ret = new Point3d_GL[frames.Length][];

            for (int i = 0; i < frames.Length; i++)
            {
                ret[i] = new Point3d_GL[frames[i].points.Length];
                Console.WriteLine(frames[i].points.Length);
                for (int j = 0; j < frames[i].points.Length; j++)
                {
                    ret[i][j] = calcPoint(frames[i].points[j], calib_frame, frames[i], zero_frame, matr);
                }
            }
            Console.WriteLine("frames[i].points.Length");
            return ret;
        }

        static public double[,] translToRot(double[,] transMatr)
        {
            var rotMatr = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    rotMatr[i, j] = transMatr[i, j];
                }
            }
            return rotMatr;
        }
        static public Point3d_GL calcPoint(PointF point, Frame cframe, Frame frame, List<Flat_4P> laserFlat, Frame zero_frame, SolveType type, double[,] matr = null)
        {
            var ox = cframe.camera.oX;
            var oy = cframe.camera.oY;
            var oz = cframe.camera.oZ;
            var f = cframe.camera.f;
            float k = (float)cframe.im.Width / (float)frame.im.Width;
            var x = point.X * k;
            var y = point.Y * k;
            var P1_cam = new Vector3d_GL(x, y, f);
            var P1_x = ox * P1_cam.x;
            var P1_y = oy * P1_cam.y;
            var P1_z = oz * P1_cam.z;
            var vec = P1_x + P1_y + P1_z;
            //addFrame_Cam(cframe.camera);
            vec.normalize();
            Point3d_GL zf = new Point3d_GL(0, 0, 0);
            if (matr != null)
            {
                var matr_3 = new double[3, 3];
                zf = translToRot(matr) * (zero_frame.pos_rob - frame.pos_rob);
            }
            else
            {
                zf = zero_frame.pos_rob - frame.pos_rob;
            }

            var moveToReal = zf;
            var zeroPos = matr * zero_frame.pos_rob;
            var line = new Line3d_GL(vec, zeroPos);
            int ids = 0;
            if (type == SolveType.Complex)
            {
                Point3d_GL ret = Point3d_GL.notExistP();
                foreach (var flat in laserFlat)
                {

                    ret = flat.crossLine(line);
                    if (ret.exist)
                    {

                        var colr = ids;
                        //Console.WriteLine("colr: : " +colr);
                        var ps = flat.P;
                        return ret - moveToReal;

                    }
                    ids++;
                }
                Console.WriteLine("null");
                var flat_max = laserFlat[0].F[0];
                ret = line.calcCrossFlat(flat_max);
                if (ret.exist)
                {
                    return ret - moveToReal;
                }
            }
            else
            {
                var flat = laserFlat[0].F[0];
                var ret = line.calcCrossFlat(flat);
                if (ret.exist)
                {
                    return ret - moveToReal;

                }
            }

            return Point3d_GL.notExistP();
        }
        static public Point3d_GL calcPoint(PointF point, Frame cframe, Frame frame, Frame frame_zero, double[,] matr)
        {
            var ox = cframe.camera.oX;
            var oy = cframe.camera.oY;
            var oz = cframe.camera.oZ;
            var f = cframe.camera.f;
            float k = (float)cframe.im.Width / (float)frame.im.Width;
            var x = point.X * k;
            var y = point.Y * k;
            var P1_cam = new Vector3d_GL(x, y, f);
            var P1_x = ox * P1_cam.x;
            var P1_y = oy * P1_cam.y;// * (-1);//*-1
            var P1_z = oz * P1_cam.z;
            var vec = P1_x + P1_y + P1_z;
            vec.normalize();

            var z = frame_zero.pos_rob.z - frame.pos_rob.z;
            //z = 0;
            if (matr == null)
                Console.WriteLine("matr NULL");
            var line = new Line3d_GL(vec, matr * frame_zero.pos_rob);
            //addPointMesh(new Point3d_GL[] { matr * frame_zero.pos_rob + del }, 0.8f, 0.8f, 0.1f);
            // z = 0;
            var flat = new Flat3d_GL(new Point3d_GL(10, 10, z),
                new Point3d_GL(0, 10, z),
                new Point3d_GL(10, 0, z));
            //Console.WriteLine("las z = "+z);
            var cr = line.calcCrossFlat(flat);
            if (cr.exist)
            {
                return cr;
            }
            //Console.WriteLine(vec.ToString());
            return Point3d_GL.notExistP();
        }
        static public Point3d_GL calcPoint_leg(PointF point, Frame cframe, Frame frame, List<Flat_4P> laserFlat, Frame zero_frame, SolveType type, double[,] matr = null)
        {
            var ox = cframe.camera.oX;
            var oy = cframe.camera.oY;
            var oz = cframe.camera.oZ;
            var f = cframe.camera.f;
            float k = (float)cframe.im.Width / (float)frame.im.Width;
            var x = point.X * k;
            var y = point.Y * k;
            var P1_cam = new Vector3d_GL(x, y, f);
            var P1_x = ox * P1_cam.x;
            var P1_y = oy * P1_cam.y;//*-1
            var P1_z = oz * P1_cam.z;
            var vec = P1_x + P1_y + P1_z;
            vec.normalize();
            var del = cframe.pos_cam - cframe.pos_rob;//dist flange->cam
            Point3d_GL zf = new Point3d_GL(0, 0, 0);
            if (matr != null)
            {
                zf = matr * zero_frame.pos_rob - matr * frame.pos_rob;
            }
            else
            {
                zf = zero_frame.pos_rob - frame.pos_rob;
            }

            double alpha = 0.3;

            var x1 = zf.x;// * Math.Cos(alpha) + zf.x * Math.Sin(alpha);
            var y1 = zf.y;// * Math.Cos(alpha) + zf.y * Math.Sin(alpha);
            var z1 = zf.z;
            var moveToReal = new Point3d_GL(x1, y1, z1);
            //var moveToReal = new Point3d_GL(zf.x, zf.y, zf.z);
            //var moveToReal = new Point3d_GL(0,0, 0);//!!!!!!!!!!
            //var lam_zero = zero_frame.pos_rob;
            // lam_zero.z = frame.pos_rob.z;
            //var zeroPos = lam_zero + del;

            var zeroPos = zero_frame.pos_rob + del;


            //addFrame(real_pos, real_pos + ox * 5, real_pos + oy * 5, real_pos + oz * 5);

            var line = new Line3d_GL(vec, zeroPos);
            int ids = 0;
            List<Point3d_GL> ret_m = new List<Point3d_GL>();
            // Console.WriteLine("ids");
            if (type == SolveType.Complex)
            {
                Point3d_GL ret = Point3d_GL.notExistP();
                foreach (var flat in laserFlat)
                {

                    ret = flat.crossLine(line);
                    if (ret.exist)
                    {
                        var ps = flat.P;
                        ret_m.Add(ret);
                        //addLineMesh(new Point3d_GL[] { ret, zeroPos }, 0.1f, 0.8f, 0.8f);
                        //GL1.addLineMesh(new Point3d_GL[] { ret - moveToReal, zeroPos }, 0.8f, 0.1f, 0.8f);
                        //addLineMesh(new Point3d_GL[] { ps[0], ps[1], ps[1], ps[3], ps[3], ps[2], ps[2], ps[0] }, 0.8f, 0.1f, 0.1f);
                        //Console.WriteLine(ids);                        
                    }
                    ids++;
                }
                //addPointMesh(ret_m.ToArray(), 0.1f, 0.8f, 0.1f);
                // GL1.addPointMesh(new Point3d_GL[] { new Point3d_GL(0, 0, 0), moveToReal }, 0.8f, 0.8f, 0.1f);
                if (ret.exist)
                {
                    return ret - moveToReal;
                }
                else
                {
                    //Console.WriteLine("NULL");
                }
                //return ret - moveToReal;
                var flat_max = laserFlat[0].F[0];
                ret = line.calcCrossFlat(flat_max);
                if (ret.exist)
                {
                    //addLineMesh(new Point3d_GL[] { ret - moveToReal, zeroPos }, 0.8f, 0.8f, 0.1f);
                    return ret - moveToReal;
                    //return ret - moveToReal;
                }
            }
            else
            {
                var flat = laserFlat[0].F[0];
                var ret = line.calcCrossFlat(flat);
                if (ret.exist)
                {
                    //Console.WriteLine("NULL");
                }

                if (ret.exist)
                {
                    return ret - moveToReal;

                }
            }

            return Point3d_GL.notExistP();
        }
        static public Point3d_GL calcPoint_leg(PointF point, Frame cframe, Frame frame, Frame frame_zero)
        {
            var ox = cframe.camera.oX;
            var oy = cframe.camera.oY;
            var oz = cframe.camera.oZ;
            var f = cframe.camera.f;
            float k = (float)cframe.im.Width / (float)frame.im.Width;
            var x = point.X * k;
            var y = point.Y * k;
            var P1_cam = new Vector3d_GL(x, y, f);
            var P1_x = ox * P1_cam.x;
            var P1_y = oy * P1_cam.y;// * (-1);//*-1
            var P1_z = oz * P1_cam.z;
            var vec = P1_x + P1_y + P1_z;
            vec.normalize();
            var del = cframe.pos_cam - cframe.pos_rob;
            var flat_ = frame.pos_rob + del;
            //addFrame(flat_, flat_ + ox * 5, flat_ + oy * 5, flat_ + oz * 5);

            //var line = new Line3d_GL(vec, flat_);

            var z = frame_zero.pos_rob.z - frame.pos_rob.z;
            //z = 0;
            var line = new Line3d_GL(vec, frame_zero.pos_rob + del);
            // z = 0;
            var flat = new Flat3d_GL(new Point3d_GL(10, 10, z),
                new Point3d_GL(0, 10, z),
                new Point3d_GL(10, 0, z));
            //Console.WriteLine("las z = "+z);
            var cr = line.calcCrossFlat(flat);
            if (cr.exist)
            {
                return cr;
            }
            //Console.WriteLine(vec.ToString());
            return Point3d_GL.notExistP();
        }
    }
}
