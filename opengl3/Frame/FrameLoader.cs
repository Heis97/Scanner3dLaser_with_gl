using Emgu.CV;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    static public class FrameLoader
    {

        static public Frame[][] loadPathsDiffDouble(string[] paths)
        {
            var frm1 = new List<Frame>();
            var frm2 = new List<Frame>();
            for (int i = 0; i < paths.Length; i++)
            {
                frm1.AddRange(FrameLoader.loadImages_diff(@"cam1\" + paths[i], FrameType.Pattern));
                frm2.AddRange(FrameLoader.loadImages_diff(@"cam2\" + paths[i], FrameType.Pattern));
            }

            return new Frame[][] { frm1.ToArray(), frm2.ToArray() };
        }
        static public Frame[] loadPathsDiff(string[] paths)
        {
            var frm1 = new List<Frame>();
            for (int i = 0; i < paths.Length; i++)
            {
                frm1.AddRange(loadImages_diff(paths[i], FrameType.Pattern));

            }

            return frm1.ToArray();
        }
        static public Frame[] loadImages(string path, double FoV, double Side, int bin = 60, int frame_len = 15, bool visible = false)
        {
            Console.WriteLine(path);
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage(file, FoV, Side, bin, frame_len, visible);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames.ToArray();
            }
            return null;
        }
        static public double[,] loadImages_basis(string path, double FoV, double Side, int bin = 60, int frame_len = 15, bool visible = false)
        {
            Console.WriteLine(path);
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage(file, FoV, Side, bin, frame_len, visible);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            Console.WriteLine("frame_co n " + frames.Count);
            if (frames.Count > 3)
            {
                var basis1 = new Point3d_GL[4];
                var basis2 = new Point3d_GL[4];

                for (int i = 0; i < 4; i++)
                {
                    basis1[i] = frames[i].pos_rob;
                    basis2[i] = frames[i].pos_cam;
                    //addLineMesh(new Point3d_GL[] { basis1[i], basis2[i] });
                }
                var transf = UtilMatr.calcTransformMatr(basis1, basis2);
                prin.t(transf);
                return transf;
            }
            return null;
        }
        static public Frame loadImage(string filepath, double FoV, double Side, int bin, int frame_len = 15, bool visible = false)//11.02,30.94  //41.9874, 112.7 FOV  
        {
            //options:
            double maxArea = 0.1;
            double minArea = 1000;
            string name = Path.GetFileName(filepath);
            name = name.Trim();
            var coords = name.Split(new char[] { ' ' });

            //Console.WriteLine("NAME = " +name);
            if (coords[0].Length == 0)
            {
                var lb = coords.ToList();
                coords = lb.GetRange(1, coords.Length - 1).ToArray();
            }
            for (int i = 0; i < coords.Length; i++)
            {
                if (!coords[i].Contains("."))
                {
                    coords[i] += ".0";
                }
            }
            Point3d_GL name_pos = Point3d_GL.notExistP();
            Point3d_GL name_pos_or = Point3d_GL.notExistP();
            if (coords.Length > 2)
            {
                name_pos = new Point3d_GL(Convert.ToDouble(coords[0]),
                                            Convert.ToDouble(coords[1]),
                                            Convert.ToDouble(coords[2]));
            }
            if (coords.Length > 5)
            {
                name_pos_or = new Point3d_GL(Convert.ToDouble(coords[3]),
                                            Convert.ToDouble(coords[4]),
                                            Convert.ToDouble(coords[5]));
            }

            var im = new Mat(filepath);

            //int koef = k;
           //CvInvoke.Resize(im, im, new Size(im.Width * koef, im.Height * koef));


           //var ps = FindMark.finPointFsFromIm(im, bin, null, null, maxArea, minArea);

            var ps = FindMark.finPointFsFromImPattern(im, bin, null, null, maxArea, minArea);

            if(ps==null)
            {
                Console.WriteLine("PS NULL");
            }

            if (ps != null)
            {
                var cam = UtilMatr.calcPos(ps, im.Size, FoV, Side);     //41.727, 68.89649           //8mm: 11.02; 7mm: 41.8; 5.5mm : 30.02
                var pos = cam.pos;
                var vec = new Vector3d_GL[] { cam.oX, cam.oY, cam.oZ };
                var err = cam.err_pos;
                // Console.WriteLine(err);
                Console.WriteLine("----------------");
                Console.WriteLine(name);
                Console.WriteLine("pos_rob " + pos.x + " " + pos.y + " " + pos.z);
                Console.WriteLine("err " + (name_pos - pos).magnitude());
                if (name_pos_or.exist)
                {
                    var matr_rob = UtilMatr.AbcToMatrix(UtilMatr.toDegrees((float)name_pos_or.x), UtilMatr.toDegrees((float)name_pos_or.y), UtilMatr.toDegrees((float)name_pos_or.z));
                    matr_rob[3, 0] = (float)name_pos.x;
                    matr_rob[3, 1] = (float)name_pos.y;
                    matr_rob[3, 2] = (float)name_pos.z;

                    var inv_matr_rob = matr_rob.Inverse;

                    var matr_cam = UtilMatr.matrFromCam(cam);
                    var matr_RobToCam = inv_matr_rob * matr_cam;
                    prin.t(matr_RobToCam);

                }
                var f = new Frame(im, pos, name_pos, name, ps, name_pos_or);

                if (visible)
                {
                    //GL1.addFrame_Cam(cam, frame_len);
                }
                f.camera = cam;
                return f;
            }
            return null;
        }
        static public Frame loadImage_laserRob(string filepath)
        {
            string name = Path.GetFileName(filepath);
            //Console.WriteLine(name);
            var name_t = name.Trim();
            var file_ext = name_t.Split(new char[] { '.' });
            var coords = name_t.Split(new char[] { ' ' });
            // Console.WriteLine(file_ext[file_ext.Length - 1]);

            if ((file_ext[file_ext.Length - 1] == "jpg" || file_ext[file_ext.Length - 1] == "png") && file_ext[0] != "calibresult")
            {
                //Console.WriteLine(coords.Length);
                if (coords.Length > 2)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        if (coords[i].Contains(".") != true)
                        {
                            coords[i] += ".0";
                        }
                    }
                    var name_pos = new Point3d_GL(Convert.ToDouble(coords[0]),
                                                    Convert.ToDouble(coords[1]),
                                                    Convert.ToDouble(coords[2]));
                    //Console.WriteLine(name_pos.x + " " + name_pos.y + " " + name_pos.z + " ");
                    var im = new Mat(filepath);
                    return new Frame(im, name_pos, name_pos, name);
                }
                else if (coords.Length > 0)
                {
                    var name_pos = new Point3d_GL(0,
                                                    0,
                                                    0);
                    var im = new Mat(filepath);
                    return new Frame(im, name_pos, name_pos, name);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
        static public Frame loadImage_calib(string filepath)
        {
            string name = Path.GetFileName(filepath);
            var coords = name.Split(new char[] { ' ' });
            Console.WriteLine(coords[0].Length + " " + coords.Length);
            if (coords[0].Length == 0)
            {
                var lamd_ar = new string[coords.Length];
                coords.CopyTo(lamd_ar, 0);
                lamd_ar.CopyTo(coords, 0);
            }
            for (int i = 0; i < 4; i++)
            {
                if (coords[i].Contains(".") != true)
                {
                    coords[i] += ".0";
                }
            }
            var name_pos = new Point3d_GL(Convert.ToDouble(coords[0]),
                                            Convert.ToDouble(coords[1]),
                                            Convert.ToDouble(coords[2])
                                            );
            var mk = 10 * Convert.ToDouble(coords[3]);
            var im = new Mat(filepath);
            var ret = new Frame(im, name_pos, name_pos, name);
            ret.size_mark = mk;
            return ret;
        }

        static public Frame loadImage_test(string filepath)
        {
            string name = Path.GetFileName(filepath);
            var im = new Mat(filepath);
            var fr = new Frame(im, name);
            fr.dateTime = File.GetCreationTime(filepath);
            return fr;
        }
        static public Frame loadImage_diff(string filepath, FrameType frameType)
        {
            string name = Path.GetFileName(filepath);
            var im = new Mat(filepath);
            var fr = new Frame(im, name,frameType);
            fr.dateTime = File.GetCreationTime(filepath);
            return fr;
        }
        static public Frame loadImage_diff(string filepath, FrameType frameType,PatternType patternType)
        {
            string name = Path.GetFileName(filepath);
            var im = new Mat(filepath);
            var fr = new Frame(im, name, frameType,patternType);
            fr.dateTime = File.GetCreationTime(filepath);
            return fr;
        }
        static public Frame loadImage_chess(string filepath)
        {
            string name = Path.GetFileName(filepath);
            var im = new Mat(filepath);
            var fr = new Frame(im, name, FrameType.MarkBoard);
            fr.dateTime = File.GetCreationTime(filepath);
            return fr;
        }
        static public Frame loadImage_stereoCV(string filepath1, string filepath2, FrameType frameType)
        {
            string name1 = Path.GetFileName(filepath1);
            var im1 = new Mat(filepath1);
            string name2 = Path.GetFileName(filepath2);
            var im2 = new Mat(filepath2);
            //Console.WriteLine(name1);
            //Console.WriteLine(name2);
            //Console.WriteLine("------------");
            var fr = new Frame(im1, im2, name1,frameType);
            fr.dateTime = File.GetCreationTime(filepath1);
            return fr;
        }
        static string[] sortByDate(string[] files)
        {
            var sortFiles = from f in files
                            orderby File.GetCreationTime(f)
                            select f;
            return sortFiles.ToArray();
        }

        static public Frame[] loadImages_stereoCV(string path1, string path2,FrameType frameType)
        {
            Console.WriteLine(path1);
            var files1 = sortByDate(Directory.GetFiles(path1));
            var files2 = sortByDate(Directory.GetFiles(path2));
            List<Frame> frames = new List<Frame>();
            for (int i = 0; i < files1.Length; i++)
            {
                var frame = loadImage_stereoCV(files1[i], files2[i], frameType);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames.ToArray();
            }
            return null;
        }

        static public Frame[] loadImages_laserRob(string path)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_laserRob(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames.ToArray();
            }
            return null;
        }
        static public Frame[] loadImages_test(string path)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_test(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames.ToArray();
            }
            return null;
        }
        static public Frame[] loadImages_chess(string path)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_chess(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames.ToArray();
            }
            return null;
        }
        static public Frame[] loadImages_diff(string path,FrameType frameType)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                //Console.WriteLine(file);
                var frame = loadImage_diff(file,frameType);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames.ToArray();
            }
            return null;
        }
        static public Frame[] loadImages_diff(string path, FrameType frameType,PatternType patternType)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                //Console.WriteLine(file);
                var frame = loadImage_diff(file, frameType, patternType);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames.ToArray();
            }
            return null;
        }
        static public Frame[] loadImages_calib(string path)
        {
            var files = Directory.GetFiles(path);
            List<Frame> frames = new List<Frame>();
            foreach (string file in files)
            {
                var frame = loadImage_calib(file);
                if (frame != null)
                {
                    frames.Add(frame);
                }
            }
            if (frames.Count != 0)
            {
                return frames.ToArray();
            }
            return null;
        }


    }
}
