using Accord.Math;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using Accord.Statistics.Distributions.Univariate;

namespace opengl3
{
    public struct Square
    {
        public float x;
        public float y;
        public float w;
        public float h;
        public Square(float _x, float _y, float _w, float _h)
        {
            x = _x;
            y = _y;
            w = _w;
            h = _h;
        }
    }
    static public class UtilOpenCV
    {

        public static (VectorOfMat, VectorOfMat) to_vec_mat(Matrix<double>[] matrixs)
        {
            var ms_r = new List<Mat>();
            var ms_t = new List<Mat>();
            for(int i=0; i<matrixs.Length;i++)
            {
                ms_r.Add(matrixs[i].GetRows(0, 3,1).Transpose().GetRows(0, 3, 1).Transpose().Mat);
                ms_t.Add(matrixs[i].GetCol(3).GetRows(0, 3, 1).Mat);

                /*prin.t(matrixs[i]);
                prin.t(ms_r[i]);
                prin.t(ms_t[i]);*/
            }

            return (new VectorOfMat(ms_r.ToArray()), new VectorOfMat(ms_t.ToArray()));
        }


        public static Mat draw_map_xy(RasterMap map, Polygon3d_GL[] surface,Point3d_GL[] traj)
        {
            var res = map.res;
            var k = 80;
            var im = new Image<Bgr, byte>((int)(k * map.map.GetLength(0)), (int)(k * map.map.GetLength(1))).Mat;
            var color_triang = new MCvScalar(255, 0, 255);
            var color_grid = new MCvScalar(128, 128, 128);
            var color_traj = new MCvScalar(0, 255, 255);
            for (int i = 1; i < traj.Length; i++)
            {
                var p1 = ((traj[i] - map.pt_min) / res) * k;
                var p0 = ((traj[i - 1] - map.pt_min) / res) * k;

                CvInvoke.Line(im, p0.get_syst_p(), p1.get_syst_p(), color_traj, 4);
                CvInvoke.Circle(im, p0.get_syst_p(), 10, color_traj);
                CvInvoke.PutText(im, i.ToString()+" "+ traj[i - 1], (p0 + new Point3d_GL(-60, -10)).get_syst_p(), FontFace.HersheyTriplex, 0.5, color_traj);
            }

            for (int i = 0; i < surface.Length; i++)
            {
                var pol =( (surface[i] - map.pt_min) / res)*k;

                draw_polyg_xy(im, pol, color_triang);
                CvInvoke.PutText(im, i.ToString(), (pol.centr+new Point3d_GL(-10,10)).get_syst_p(), FontFace.HersheyTriplex, 0.4, color_triang);
            }

            

            for (int x = 0; x < map.map.GetLength(0); x++)
            {
                CvInvoke.Line(im, new Point((int)(k*x),0), new Point((int)(k * x), im.Height), color_grid);
                CvInvoke.PutText(im, x.ToString(), new Point((int)(k * x ), 30), FontFace.HersheyComplexSmall, 1, color_grid);
            }
            for (int y = 0; y < map.map.GetLength(1); y++)
            {
                CvInvoke.Line(im, new Point(0,(int)(k * y)), new Point(im.Width,(int)(k * y)), color_grid);
                CvInvoke.PutText(im, y.ToString(), new Point(10,(int)(k * y )), FontFace.HersheyComplexSmall, 1, color_grid);
            }

            for (int x = 0; x < map.map.GetLength(0); x++)
            {
                for (int y = 0; y < map.map.GetLength(1); y++)
                {
                    if(map.map[x,y] != null)
                    {
                        var dy = 0;
                        var text = "";
                        for (int i = 0; i < map.map[x, y].Length; i++)
                        {
                           // if(map.map[x, y][i]==20)
                                text += map.map[x, y][i].ToString()+ " ";
                            if(text.Length>20)
                            {
                                
                               
                                CvInvoke.PutText(im, text, new Point((int)(k * x), (int)(k * y) + 10+dy), FontFace.HersheyTriplex, 0.4, color_grid);
                                text = "";
                                dy += 10;
                            }
                        }
                        CvInvoke.PutText(im, text, new Point((int)(k * x), (int)(k * y) + 10 + dy), FontFace.HersheyTriplex, 0.4, color_grid);

                    }
                    
                }
            }
           //CvInvoke.Imshow("map_xy", im);
            return im;
        }


       

        public static void draw_polyg_xy(Mat im, Polygon3d_GL polyg,MCvScalar color)
        {
            CvInvoke.Line(im, polyg.ps[0].get_syst_p(), polyg.ps[1].get_syst_p(), color);
            CvInvoke.Line(im, polyg.ps[1].get_syst_p(), polyg.ps[2].get_syst_p(), color);
            CvInvoke.Line(im, polyg.ps[2].get_syst_p(), polyg.ps[0].get_syst_p(), color);
        }

        public static Mat[] resizeMats(Mat[] mats)
        {
            for (int i=0; i<mats.Length;i++)
            {
                mats[i] = resizeMat(mats[i]);
            }
            return mats;
        }

        public static Mat resizeMat(Mat mat)
        {         
            //CvInvoke.Resize(mat, mat, new Size(640, 640));         
            return mat;
        }


        public static MCvPoint3D32f[] takeGabObp(MCvPoint3D32f[] obp, Size patt_size)
        {
            var ps3d = new MCvPoint3D32f[4];
            var w = patt_size.Width;
            var h = patt_size.Height;
            var inds_1 = new int[4] { 0, w - 1, w * (h - 1), w * h - 1 };
            for(int i=0; i<inds_1.Length;i++)
            {
                ps3d[i] = obp[inds_1[i]];
            }
            return ps3d;
        }
        public static System.Drawing.PointF[] takeGabObp(System.Drawing.PointF[] obp, Size patt_size)
        {
            var ps2d = new System.Drawing.PointF[4];
            var w = patt_size.Width;
            var h = patt_size.Height;
            Console.WriteLine("take ob: "+w + " "+h);
            var inds_1 = new int[4] {
                0             ,          w - 1,
                w * (h - 1)   ,        w * h - 1 };
            for (int i = 0; i < inds_1.Length; i++)
            {
                ps2d[i] = obp[inds_1[i]];
            }
            return ps2d;
        }
        public static Mat[] warpPerspNorm(Mat[] mats, Matrix<double> matrixPers,Size size)
        {
            var mat = mats[0];           
            var p_control = matToPointF(mats[1]);
            var p_control_2 = matToPointF(mats[2]);
            var bord = new System.Drawing.PointF[4]
                {
                    new System.Drawing.PointF(0,0),
                    new System.Drawing.PointF(mat.Width,0),
                    new System.Drawing.PointF(0,mat.Height),
                    new System.Drawing.PointF(mat.Width,mat.Height),
                };
            
            var bord_warp = CvInvoke.PerspectiveTransform(bord, matrixPers);
            var rect = CvInvoke.BoundingRectangle(new VectorOfPointF( bord_warp));
            for (int i=0; i<4;i++)
            {
                bord_warp[i].X -= rect.X;
                bord_warp[i].Y -= rect.Y;
            }
            var persp_Norm = CvInvoke.GetPerspectiveTransform(new VectorOfPointF(bord), new VectorOfPointF(bord_warp));
            var mat_warp = new Mat();

            CvInvoke.WarpPerspective(mat, mat_warp, persp_Norm, rect.Size, Inter.Linear, Warp.Default);
            var p_warp = CvInvoke.PerspectiveTransform(p_control, persp_Norm);
            var p_warp_2 = CvInvoke.PerspectiveTransform(p_control_2, persp_Norm);

            double kx = (double)size.Width / (double)mat_warp.Width;
            double ky = (double)size.Height / (double)mat_warp.Height;
            double k = 1;
            double offx = 0;
            double offy = 0;
            if (kx>ky)
            {
                k = ky;
                offx = (size.Width - mat_warp.Width * ky) / 2;
            }
            else
            {
                k = kx;
                offy = (size.Height - mat_warp.Height * kx) / 2;
            }
            
            
            var affineMatr = new Matrix<double>(new double[2, 3]
            {
                {k,0,offx },
                {0,k,offy },
            });
           
            CvInvoke.WarpAffine(mat_warp, mat_warp, affineMatr, 
                new Size(
                    (int)(k * mat_warp.Width+offx), 
                    (int)(k * mat_warp.Height+offy)
                    ));
            var affineMatr_3d = affineMatr.ConcateVertical(new Matrix<double>(new double[1, 3] { { 0, 0, 1 } }));
            var p_aff = CvInvoke.PerspectiveTransform(p_warp, affineMatr_3d);
            var p_aff_2 = CvInvoke.PerspectiveTransform(p_warp_2, affineMatr_3d);
            return new Mat[] { mat_warp , pointFTomat(p_aff), pointFTomat(p_aff_2) };
        }
        //static System.Drawing.PointF[] 

        public static System.Drawing.PointF[] transfAffine(System.Drawing.PointF[] pointFs, Matrix<double> matrix)
        {
            var affineMatr_3d = matrix.ConcateVertical(new Matrix<double>(new double[1, 3] { { 0, 0, 1 } }));
           // prin.t("affineMatr_3d");
           //prin.t(affineMatr_3d);
            var p_aff = CvInvoke.PerspectiveTransform(pointFs, affineMatr_3d);
            return p_aff;
        }
        public static Mat normalize(Mat im, int max = 255)
        {
            var data = new byte[1, 1, 1];
            if (im.NumberOfChannels == 1)
            {
                data = im.ToImage<Gray, byte>().Data;
            }
            else if (im.NumberOfChannels == 3)
            {
                data = im.ToImage<Bgr, byte>().Data;
            }
            int maxH = int.MinValue;
            var data_n = new byte[data.GetLength(0), data.GetLength(1), data.GetLength(2)];
            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    for (int c = 0; c < data.GetLength(2); c++)
                    {
                        var val = data[x, y, c];
                        if (val > maxH)
                        {
                            maxH = val;
                        }
                    }
                }
            }

            for (int x = 0; x < data.GetLength(0); x++)
            {
                for (int y = 0; y < data.GetLength(1); y++)
                {
                    for (int c = 0; c < data.GetLength(2); c++)
                    {
                        //Console.WriteLine("h " + h + "maxH " + maxH);
                        data_n[x, y, c] = (byte)((float)(max - 1) * data[x, y, c] / (float)maxH);
                    }
                }
            }

            var mat_ret = new Mat();
            if (im.NumberOfChannels == 1)
            {
                mat_ret = new Image<Gray, byte>(data_n).Mat;
            }
            else if (im.NumberOfChannels == 3)
            {
                mat_ret = new Image<Bgr, byte>(data_n).Mat;
            }

            return mat_ret;
        }
        public static Mat histogram(Mat im, int max = 300, int range = 256)
        {

            var data = new byte[1, 1, 1];
            if (im.NumberOfChannels == 1)
            {
                data = im.ToImage<Gray, byte>().Data;
            }
            else if (im.NumberOfChannels == 3)
            {
                data = im.ToImage<Bgr, byte>().Data;
            }
            var hist = new int[range, data.GetLength(2)];
            int maxH = int.MinValue;

            for (int c = 0; c < data.GetLength(2); c++)
            {
                for (int x = 0; x < data.GetLength(0); x++)
                {
                    for (int y = 0; y < data.GetLength(1); y++)
                    {
                        var val = data[x, y, c];
                        hist[val, c]++;
                    }
                }


                for (int i = 0; i < hist.GetLength(0); i++)
                {
                    var val = hist[i, c];
                    if (val > maxH)
                    {
                        maxH = val;
                    }
                }

            }

            var hist_im = new byte[max, range, hist.GetLength(1)];
            for (int c = 0; c < hist_im.GetLength(2); c++)
            {
                for (int x = 0; x < hist_im.GetLength(0); x++)
                {
                    for (int y = 0; y < hist_im.GetLength(1); y++)
                    {
                        hist_im[x, y, c] = 0;

                    }
                }
            }
            for (int c = 0; c < hist.GetLength(1); c++)
            {
                for (int i = 0; i < range; i++)
                {
                    var h = (int)((float)(max - 1) * hist[i, c] / (float)maxH);
                    //Console.WriteLine("h " + h + "maxH " + maxH);
                    //hist_im[h, i, c] = 255;
                    for (int j = 0; j < h; j++)
                    {
                        hist_im[j, i, c] = 255;
                    }
                }
            }
            var mat_ret = new Mat();
            if (im.NumberOfChannels == 1)
            {
                mat_ret = new Image<Gray, byte>(hist_im).Mat;
            }
            else if (im.NumberOfChannels == 3)
            {
                mat_ret = new Image<Bgr, byte>(hist_im).Mat;
            }
            CvInvoke.Flip(mat_ret, mat_ret, FlipType.Vertical);
            return mat_ret;
        }

        public static Mat noise(Mat mat, int val = 0, int range = 30)
        {
            Matrix<byte> matrix = new Matrix<byte>(mat.Width, mat.Height);
            var data = (byte[,,])mat.GetData();
            for(int i=0; i<data.GetLength(0);i++)
            {
                for (int j = 0; j < data.GetLength(1); j++)
                {
                    for (int k = 0; k < data.GetLength(2); k++)
                    {

                        data[i, j, k] += (byte)NormalDistribution.Random(val, range);
                    }
                }
            }
            //CvInvoke.cvConvertScale(mat, matrix);
            //matrix.SetRandNormal(new MCvScalar(val), new MCvScalar(range));

           // prin.t(matrix);
            return new Image<Bgr,byte>(data).Mat;
        }
        public static Mat GLnoise(Mat mat, int val = 0, int range = 30, int kernel_size = 7)
        {            
            CvInvoke.GaussianBlur(mat, mat, new Size(kernel_size, kernel_size), 0);
            var noise = mat.Clone();
            CvInvoke.Randn(noise, new MCvScalar(val,val,val), new MCvScalar(range,range,range));

           // mat = noise(mat, val, range);

            mat += noise;
            //var mat1 = new Mat();


            //CvInvoke.Flip(mat, mat1, FlipType.Vertical);
            return mat;
        }
        public static int[] takeLineFromMat(Image<Gray, byte> im, int y)
        {
            var line = new int[im.Width];
            for (int i = 0; i < line.Length; i++)
            {
                line[i] = im.Data[y, i, 0];
            }
            return line;
        }

        public static int[][] takeLineFromMat(Image<Bgr, byte> im, int y)
        {
            var line = new int[im.Width][];
            for (int i = 0; i < line.Length; i++)
            {
                line[i] = new int[3];
                line[i][0] = im.Data[y, i, 0];
                line[i][1] = im.Data[y, i, 1];
                line[i][2] = im.Data[y, i, 2];
                Console.WriteLine(i+" "+line[i][0]+" "+ line[i][2] + " " + line[i][1] + " " );
            }
            
            return line;
        }

        public static Image<Bgr, byte>[] PaintLines(Image<Gray, byte> im1, Image<Gray, byte> im2, int y, Features features)
        {
            var disp = Features.disparMap_3d(im1.Mat, im2.Mat, 40, 3);
            var line1 = takeLineFromMat(im1, y);
            var line2 = takeLineFromMat(im2, y);
            var dispLine = takeLineFromMat(disp[0].ToImage<Gray, byte>(), y);
            var diffLine = takeLineFromMat(disp[1].ToImage<Gray, byte>(), y);

            var data = new byte[im1.Height, im1.Width, 3];

            /* for(int i=0; i<line1.Length;i++)
             {
                 //Console.WriteLine("im1.Width: " + im1.Width + " im1.Height: " + im1.Height);
                 //Console.WriteLine("line2[i]: " + line2[i] + "; line1[i]: " + line1[i] + "; i: " + i);
                 data[line1[i], i, 0] = 255;

                 data[line2[i], i, 1] = 255;

                 data[dispLine[i], i, 0] = 255;
                 data[dispLine[i], i, 2] = 255;

                 data[diffLine[i], i, 0] = 255;
                 data[diffLine[i], i, 1] = 255;
             }*/

            return new Image<Bgr, byte>[] { new Image<Bgr, byte>(data), disp[0].ToImage<Bgr, byte>(), disp[1].ToImage<Bgr, byte>() };
        }
        static double toRad(double degree)
        {
            return (Math.PI * 2 * degree) / 360;
        }
        public static Matrix<double> matrixForCamera(Size size, double fov)
        {
            var cx = size.Width/2;
            var cy = size.Height/2;
            var radFoV = toRad(fov);
            var f = cx / Math.Tan(radFoV / 2);
            var dataCam = new double[,]
            {
                {f,0,cx },
                {0,f,cy },
                {0,0,1 }
            };
            return new Matrix<double>(dataCam);
        }
        public static void saveImage(ImageBox box, string folder, string name)
        {
            var mat1 = (Mat)box.Image;
            saveImage(mat1, folder, name);
        }
        public static void saveImage(Mat mat1, string folder, string name)
        {
            if (mat1 != null)
            {
                Directory.CreateDirectory(folder);
                var im1 = mat1.ToImage<Bgr, byte>();
                Console.WriteLine(folder + "\\" + name);
                im1.Save(folder + "\\" + name+".png");
            }
        }
        public static void saveImage(ImageBox box1, ImageBox box2, string name, string folder)
        {
            var mat1 = (Mat)box1.Image;
            if(mat1!=null)
            {
                Console.WriteLine("cam1\\" + folder + "\\" + name);
                Directory.CreateDirectory("cam1\\" + folder);
                mat1.Save("cam1\\" + folder + "\\" + name);
            }  
            
            mat1 = (Mat)box2.Image;
            if (mat1 != null)
            {
                Console.WriteLine("cam2\\" + folder + "\\" + name);
                Directory.CreateDirectory("cam2\\" + folder);
                mat1.Save("cam2\\" + folder + "\\" + name);
            }
        }


        /// <summary>
        /// ret (err, dist betw 2 points)
        /// </summary>
        /// <param name="size"></param>
        /// <param name="graphicGL"></param>
        /// <param name="markSize"></param>
        /// <param name="id_monit"></param>
        /// <param name="inpMat"></param>
        /// <param name="photo"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        static public System.Drawing.PointF[] calcSubpixelPrec(Size size, GraphicGL graphicGL, float markSize, int id_monit, PatternType patternType, Mat inpMat = null, string photo = null,int kernel=11)
        {
            Mat mat = new Mat();
            if(inpMat==null)
            {
                mat = graphicGL.matFromMonitor(id_monit);
            }
            else
            {
                mat = inpMat;
            }
            mat = GLnoise(mat, 0, 10);
            var len = size.Width * size.Height;
            var obp = new MCvPoint3D32f[len];
            var cornF = new System.Drawing.PointF[len];
            var cornF_GL = new System.Drawing.PointF[len];
            var cornF_delt = new System.Drawing.PointF[len];
            var sum = new System.Drawing.PointF(0, 0);
            var kvs = new System.Drawing.PointF(0, 0);
            var S = new System.Drawing.PointF(0, 0);

            var ret = compPatternCoords(mat, ref obp, ref cornF, size,kernel,patternType);
            if (!ret)
            {
                return new System.Drawing.PointF[] { new System.Drawing.PointF(float.MaxValue, float.MaxValue), new System.Drawing.PointF(float.MaxValue, float.MaxValue) };
            }
            for (int i = 0; i < obp.Length; i++)
            {
                var p_GL = new PointF(0,0);
                if (photo==null)
                {
                    p_GL = graphicGL.calcPixel(new Vertex4f(markSize * obp[i].X, markSize * obp[i].Y, obp[i].Z, 1), id_monit);
                }
                else
                {
                    p_GL = graphicGL.calcPixel_photo(new Vertex4f(markSize * obp[i].X, markSize * obp[i].Y, obp[i].Z, 1), photo, mat.Size);
                }
                cornF_GL[i] = new System.Drawing.PointF(p_GL.X, p_GL.Y);
                var p_chess = cornF[i];
                cornF_delt[i] = new System.Drawing.PointF(p_GL.X - p_chess.X, p_GL.Y - p_chess.Y);
                sum.X += cornF_delt[i].X;
                sum.Y += cornF_delt[i].Y;
                //prin.t(cornF[i].ToString());
                
                
            }
            var x = cornF_GL[0].X - cornF_GL[1].X;
            var y = cornF_GL[0].Y - cornF_GL[1].Y;

            var dist = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            //prin.t("_________________");
            var raz = new System.Drawing.PointF(0, 0);
            raz.X = sum.X / len;
            raz.Y = sum.Y / len;

            for (int i = 0; i < obp.Length; i++)
            {
                cornF_delt[i].X -= raz.X;
                cornF_delt[i].Y -= raz.Y;
                //prin.t(cornF_delt[i].ToString());
                S.X += Math.Abs(cornF_delt[i].X);
                S.Y += Math.Abs(cornF_delt[i].Y);

            }          
            //Console.WriteLine(S);
            var matM = new Mat(mat, new Rectangle(0, 0, mat.Width, mat.Height));
            drawMatches(matM, cornF, cornF_GL, 0, 255, 0);
            //drawPointsF(mat, cornF, 255, 0, 0, 3);
            //drawPointsF(mat, cornF_GL, 0, 0, 255, 3);
            CvInvoke.Imshow("2", matM);
            
            return new System.Drawing.PointF[] { S,  new System.Drawing.PointF((float)dist,0) };
        }

        static public System.Drawing.PointF[] calcSubpixelPrecCircle(Size size, Mat[] mat)
        {
            var len = size.Width * size.Height;
            var obp = new MCvPoint3D32f[len];
            var cornF = new System.Drawing.PointF[len];
          
            var cornF_delt = new System.Drawing.PointF[len];
            var sum = new System.Drawing.PointF(0, 0);
            var S = new System.Drawing.PointF(0, 0);

            var mat_c = FindCircles.findCircles(mat[0],ref cornF, size);
           // CvInvoke.Imshow("matC", mat_c);
           // prin.t(cornF);
            var corn_patt = matToPointF(mat[1]);
         
            for (int i = 0; i < obp.Length; i++)
            {
                cornF_delt[i] = new System.Drawing.PointF(corn_patt[i].X - cornF[i].X, corn_patt[i].Y - cornF[i].Y);
                sum.X += cornF_delt[i].X;
                sum.Y += cornF_delt[i].Y;
            }
            var x = cornF[0].X - cornF[1].X;
            var y = cornF[0].Y - cornF[1].Y;

            var dist = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));
            //prin.t("_________________");
            var raz = new System.Drawing.PointF(0, 0);
            raz.X = sum.X / len;
            raz.Y = sum.Y / len;

            for (int i = 0; i < obp.Length; i++)
            {
                cornF_delt[i].X -= raz.X;
                cornF_delt[i].Y -= raz.Y;
                //prin.t(cornF_delt[i].ToString());
                S.X += Math.Abs(cornF_delt[i].X);
                S.Y += Math.Abs(cornF_delt[i].Y);

            }



            Console.WriteLine(S);
            var matM = new Mat(mat[0], new Rectangle(0, 0, mat[0].Width, mat[0].Height));
            drawMatches(matM, cornF, corn_patt, 0, 255, 0);
            CvInvoke.Imshow("2", matM);
            //drawPointsF(mat, cornF, 255, 0, 0, 1);
            //drawPointsF(mat, cornF_GL, 0, 0, 255, 1);
            return new System.Drawing.PointF[] { S, new System.Drawing.PointF((float)dist, 0) };
        }

        static public MCvPoint3D32f[] removeFromNegative( MCvPoint3D32f[] points)
        {
            var psPosit = new MCvPoint3D32f[points.Length];
            float min_x = float.MaxValue;
            float min_y = float.MaxValue;
            int min_i_x = 0;
            int min_i_y = 0;
            for(int i=0; i<points.Length;i++)
            {
                if(points[i].X<min_x)
                {
                    min_x = points[i].X;
                    min_i_x = i;
                }

                if (points[i].Y < min_y)
                {
                    min_y = points[i].Y;
                    min_i_y = i;
                }
            }
            for (int i = 0; i < points.Length; i++)
            {
                var x = points[i].X - min_x;
                var y = points[i].Y - min_y;
                psPosit[i] = new MCvPoint3D32f(x, y, points[i].Z);
            }
            return psPosit;
        }

        #region draw_something
        static public Mat drawMatches(Mat im, MCvPoint3D32f[] points1, System.Drawing.PointF[] points2, int r, int g, int b, int size = 1)
        {
            return drawMatches(im, PointF.toPoint(points1), PointF.toPoint(points2), r, g, b, size);
        }
        static public Mat drawMatches(Mat im, System.Drawing.PointF[] points1, System.Drawing.PointF[] points2, int r, int g, int b, int size = 1)
        {
            return drawMatches(im, PointF.toPoint(points1), PointF.toPoint(points2), r, g, b, size);
        }
        static public MCvScalar randomColor()
        {
            var rand = Accord.Math.Random.Generator.Random;
            int r = rand.Next(0, 255);
            int g = rand.Next(0, 255);
            int b = rand.Next(0, 255);
            //Console.WriteLine(r + " " + g + " " + b);
            return new MCvScalar(b, g, r);
        }
        static public Mat drawMatches(Mat im, System.Drawing.Point[] points1, System.Drawing.Point[] points2, int r, int g, int b, int size = 1)
        {
            int ind = 0;
            var color = new MCvScalar(b, g, r);//bgr

            if (points1.Length != 0 && points2.Length != 0 && points1.Length == points2.Length)
            {
                double delt = 0;
                for(int i=0; i<points1.Length;i++)
                {
                    
                    //CvInvoke.Circle(im, points1[i], size - 1, color, -1);
                    //CvInvoke.Circle(im, points2[i], size - 1, color, -1);
                    //CvInvoke.Line(im, points1[i], points2[i], randomColor(), size);
                    CvInvoke.Line(im, points1[i], points2[i], color, size);
                    delt += Math.Sqrt(Math.Pow((points1[i].X - points2[i].X), 2) + Math.Pow((points1[i].Y - points2[i].Y), 2));
                    ind++;
                }
                Console.WriteLine("delt: " + delt);
            }
            else
            {
                Console.WriteLine("Cannot draw match " + points1.Length + " != " + points2.Length);
            }
            return im;
        }

        static public Mat drawLines(Mat im, System.Drawing.PointF[] points1, int r, int g, int b, int size = 1)
        {
            return drawLines(im, PointF.toPoint(points1), r, g, b, size);
        }
        static public Mat drawLines(Mat im, System.Drawing.Point[] points1, int r, int g, int b, int size = 1,int size_c = 1)
        {
            int ind = 0;
            var color = new MCvScalar(b, g, r);//bgr
            //color = randomColor();
            if(points1 == null)
            {
                return im;
            }
            if (points1.Length != 0 )
            {
                for (int i = 1; i < points1.Length; i++)
                {                  
                    CvInvoke.Circle(im, points1[i], size_c, color, 1);
                    CvInvoke.Line(im, points1[i-1], points1[i], color, size);
                    ind++;
                }
            }
            return im;
        }

        static public Mat  drawPointsF(Mat im, System.Drawing.PointF[] points, int r, int g, int b, int size = 1, bool text = false)
        {
            return drawPoints(im, PointF.toPoint(points), r, g, b, size, text);
        }
        static public Mat draw_point_data(Mat im, double[][] data, int r, int g, int b, int size = 1, bool text = false)
        {
            return drawPoints(im, PointF.toPoint(data_to_pf(data)), r, g, b, size, text);
        }
        static public Mat draw_line_data(Mat im, double[][] data, int r, int g, int b, int size = 1, bool text = false)
        {
            return drawLines(im, PointF.toPoint(data_to_pf(data)), r, g, b, size);
        }
        static public PointF[] data_to_pf(double[][] data)
        {
            var pf = new PointF[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                pf[i] = new PointF(data[i][0], data[i][1]);
            }
            return pf;
        }

        static public Mat drawPointsF(Mat im, PointF[] points, int r, int g, int b, int size = 0, bool text = false)
        {
            return drawPoints(im, PointF.toPoint(points), r, g, b, size,text);
        }
        static public Mat drawPoints(Mat im, Point[] points, int r, int g, int b, int size = 0,bool text = false)
        {
            var color = new MCvScalar(b, g, r);//bgr            
            if(points==null || im == null)
            {
                return im;
            }
            if (points.Length != 0)
            {
                for (int i = 0; i < points.Length; i++)
                {
                    
                    CvInvoke.Circle(im, points[i], size, color, -1);
                    if(text)
                    {
                        CvInvoke.PutText(im, "P" + i, points[i], FontFace.HersheyComplex, 0.5, new MCvScalar(r, g, b), 1);
                    }
                    //

                }
            }
            return im;
        }


        static public void drawPoints(Mat im, PointF[] points, int r, int g, int b, int size = 1)
        {
            drawPoints(im, PointF.toPoint(points), r, g, b, size);
        }

        static public void drawPoints(Mat im, System.Drawing.PointF[] points,MCvPoint3D32f[] points3d, int r, int g, int b, int size = 1)
        {
            var points2d = PointF.toPoint(points);
            drawPoints(im, points2d, r, g, b, size);
            if(points2d.Length == points3d.Length)
            {
                for(int i=0; i<points2d.Length; i++)
                {
                    CvInvoke.PutText(im, points3d[i].X + " " + points3d[i].Y + " " + points3d[i].Z, points2d[i], FontFace.HersheyComplex, 0.5, new MCvScalar(r, g, b), size);
                }
            }

        }

        static public void drawPoints(Mat im, System.Drawing.PointF[] points, int r, int g, int b, int size = 1)
        {
            drawPoints(im, PointF.toPoint(points), r, g, b, size);
        }
        static public void drawTours(Mat im, Point[] points, int r, int g, int b, int size = 4)
        {
            int ind = 0;
            if (points.Length != 0)
            {
                foreach (var p in points)
                {
                    draw_tour(p, size, ind, im, r, g, b);
                    ind++;
                }
            }
        }

        static public Mat drawTours(Mat im, PointF[] d_points, int r, int g, int b, int size = 4)
        {
            if (d_points != null)
            {

                int ind = 0;
                var points = PointF.toPoint(d_points);
                if (points.Length != 0)
                {
                    foreach (var p in points)
                    {
                        draw_tour(p, size, ind, im, r, g, b);
                        ind++;
                    }
                }
            }
            return im;
        }

        static public void drawTours(Mat im, PointF[][] d_points, int r, int g, int b, int size = 4)
        {
            int ind = 0;
            foreach (var d_ps in d_points)
            {
                var points = PointF.toPoint(d_ps);
                if (points.Length != 0)
                {
                    foreach (var p in points)
                    {
                        draw_tour(p, size, ind, im, r, g, b);
                        ind++;
                    }
                }
            }

        }
        static public void draw_tour(Point p1, int size, int ind, Mat im, int r, int g, int b)//size - размер креста
        {

            var pt1 = new Point(p1.X + size, p1.Y);
            var pt2 = new Point(p1.X - size, p1.Y);
            var pt3 = new Point(p1.X, p1.Y + size);
            var pt4 = new Point(p1.X, p1.Y - size);
            var pt5 = new Point(p1.X + size, p1.Y - size);
            var color = new MCvScalar(b, g, r);//bgr
            CvInvoke.Line(im, pt1, pt2, color, 2);//
            CvInvoke.Line(im, pt3, pt4, color, 2);//krest
            CvInvoke.Line(im, new Point(im.Width / 2, 0), new Point(im.Width / 2, im.Height), color, 1);//
            CvInvoke.Line(im, new Point(0, im.Height / 2), new Point(im.Width, im.Height / 2), color, 1);//central krest
            CvInvoke.PutText(im, "P" + Convert.ToString(ind), pt5, FontFace.HersheyPlain, 1, color);
        }

        static public void printMatch(MCvPoint3D32f[][] p3d, System.Drawing.PointF[][] p2d )
        {
            for(int i=0;i<p3d.Length;i++)
            {
                for (int j = 0; j < p3d[i].Length; j++)
                {
                    Console.WriteLine(p3d[i][j].X + " | " + p3d[i][j].Y + " | " + p3d[i][j].Z + " -> " + p2d[i][j].X + " | " + p2d[i][j].X + " | ");
                }
                Console.WriteLine("________________________________________");
            }
            
        }

        #endregion
        static public TransRotZoom[] readTrz(string path)
        {
            var trzs = new List<TransRotZoom>();
            var files = Directory.GetFiles(path);

            for (int i = 0; i < files.Length; i++)
            {
                var filename = Path.GetFileName(files[i]);
                var trz = new TransRotZoom(filename);
                trz.dateTime = File.GetCreationTime(filename);
                if (trz != null)
                {
                    trzs.Add(trz);
                }

            }
            if (trzs.Count != 0)
            {
                var trzs1 = from f in trzs
                            orderby f.dateTime.Ticks
                            select f;
                return trzs1.ToArray();
            }
            return null;
        }
        
        static public void generateImagesFromAnotherFolder(string[] paths, GraphicGL graphicGL, CameraCV cameraCV)
        {
            var trzs_L = new List<TransRotZoom[]>();
            foreach (var path in paths)
            {
                var trzs = readTrz(path);
                trzs_L.Add(trzs);

            }
            if (trzs_L.Count == 0)
            {
                return;
            }
            graphicGL.monitorsForGenerate = new int[] { 2, 3 };
            graphicGL.pathForSave = "virtual_stereo\\test2";
            //graphicGL.imageBoxesForSave = new ImageBox[] { imBox_mark1, imBox_mark2 };
            graphicGL.trzForSave = trzs_L;
            graphicGL.saveImagesLen = trzs_L[0].Length - 1;
            graphicGL.cameraCV = cameraCV;
            Console.WriteLine("GL1.saveImagesLen " + graphicGL.saveImagesLen);
        }

        static public void generateImagesFromAnotherFolderStereo(string[] paths, GraphicGL graphicGL, CameraCV cameraCV)
        {
            var trzs_L = new List<TransRotZoom[]>();
            int ind = 0;
            var offtrz = new TransRotZoom(0, 0, 0, 0, 0, 0, -0.02);
            var trzs_prev = readTrz(paths[0]);
            foreach (var path in paths)
            {
                if (ind == 0)
                {
                    var trzs = readTrz(path);
                    var trzs_clone = new TransRotZoom[trzs.Length];
                    for (int i = 0; i < trzs.Length; i++)
                    {
                        trzs_clone[i] = trzs[i].minusDelta(offtrz);
                    }
                    trzs_L.Add(trzs_clone);
                }
                else
                {
                    var trzs = readTrz(path);

                    if (trzs.Length != trzs_L[0].Length)
                    {
                        return;
                    }
                    var trzs_slave = new TransRotZoom[trzs.Length];
                    var trz_delta = trzs_prev[0] - trzs[0];
                    for (int i = 0; i < trzs_L[0].Length; i++)
                    {
                        trzs_slave[i] = trzs_L[0][i].minusDelta(trz_delta);
                    }
                    trzs_L.Add(trzs_slave);
                }
                ind++;

            }
            if (trzs_L.Count == 0)
            {
                return;
            }

            graphicGL.monitorsForGenerate = new int[] { 2, 3 };
            graphicGL.pathForSave = "virtual_stereo\\test6";
            //GL1.imageBoxesForSave = new ImageBox[] { imBox_mark1, imBox_mark2 };
            graphicGL.trzForSave = trzs_L;
            graphicGL.saveImagesLen = trzs_L[0].Length - 1;
            graphicGL.cameraCV = cameraCV;
            graphicGL.startGen = 1;
            Console.WriteLine("GL1.saveImagesLen " + graphicGL.saveImagesLen);
        }
        static public void SaveMonitor(object obj)
        {
            var graphicGL = (GraphicGL)obj;

            //Console.WriteLine("GL1.saveImagesLen " + GL.saveImagesLen);
            if (graphicGL.saveImagesLen >= 0 && graphicGL.startGen == 1)
            {

                var monitors = graphicGL.monitorsForGenerate;
                string newPath = graphicGL.pathForSave;
                int ind_im = graphicGL.saveImagesLen;
                var trzs_L = graphicGL.trzForSave;
                int ind = 0;
                foreach (var trzs in trzs_L)
                {
                    var indMonit = monitors[ind];
                    //Console.WriteLine("indMonit" + indMonit);
                    var trzMonitor = graphicGL.transRotZooms[indMonit];
                    trzMonitor.setTrz(trzs[ind_im]);
                    graphicGL.transRotZooms[indMonit] = trzMonitor;
                    graphicGL.SaveToFolder(newPath, indMonit);

                    var mat1 = remapDistImOpenCvCentr(graphicGL.matFromMonitor(indMonit), graphicGL.cameraCV.distortmatrix);
                    saveImage(mat1, newPath, Path.Combine("monitor_" + indMonit, trzMonitor.ToString() + ".png"));
                    ind++;
                }
                graphicGL.saveImagesLen--;
                if (graphicGL.saveImagesLen == -1)
                {
                    graphicGL.startGen = 0;
                }
            }

        }
        static public Mat generateImage(Size size)
        {
            var data = new byte[size.Height, size.Width, 1];
            int w = data.GetLength(1);
            int h = data.GetLength(0);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    data[j, i, 0] = 0;
                    if (i > 100 && i < 200)
                    {
                        if (j > 100 && j < 200)
                        {
                            data[j, i, 0] = 250;
                        }
                    }

                    if (i % 10 == 0)
                    {
                        data[j, i, 0] = 250;
                    }

                    if (j % 10 == 0)
                    {
                        data[j, i, 0] = 250;
                    }


                }
            }
            return (new Image<Gray, byte>(data)).Mat;
        }
        static public float[,] generateMap(Size size)
        {
            var data = new float[size.Height, size.Width];
            int w = data.GetLength(1);
            int h = data.GetLength(0);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    if (i > 190)
                    {
                        data[j, i] = i * 1.5f;
                    }
                    else
                    {
                        data[j, i] = i;
                    }

                    //data[j, i] = i+100;
                }
            }
            return data;
        }

        static public float[,] generateMapY(Size size)
        {
            var data = new float[size.Height, size.Width];
            int w = data.GetLength(1);
            int h = data.GetLength(0);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    data[j, i] = j;
                }
            }
            return data;
        }
        static public Image<Gray, byte> mapToIm(float[,] map, PlanType planType, Mat mapy = null)
        {
            var dataMap = map;
            var dataMapy = new float[1, 1];
            if (mapy != null)
            {
                dataMapy = (float[,])mapy.GetData();
            }
            int w = dataMap.GetLength(1);
            int h = dataMap.GetLength(0);
            float max = dataMap.Max();
            float min = dataMap.Min();
            var dataIm = new float[h, w, 1];
            switch (planType)
            {
                case PlanType.X:
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var val = dataMap[j, i] - i;
                            dataIm[j, i, 0] = val;
                        }
                    }
                    break;

                case PlanType.Y:
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var val = dataMap[j, i] - j;
                            dataIm[j, i, 0] = val;
                        }
                    }
                    break;
                case PlanType.XY:
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var val = Math.Sqrt(Math.Pow(dataMap[j, i] - i, 2) + Math.Pow(dataMapy[j, i] - j, 2));
                            dataIm[j, i, 0] = (float)val;
                        }
                    }
                    break;
            }
            return new Image<Gray, byte>(normalyse(dataIm));
        }

        static public Image<Gray, byte> mapToIm(Mat map, PlanType planType, Mat mapy = null)
        {
            return mapToIm((float[,])map.GetData(), planType, mapy);
        }


        static public Mat mapToMat(float[,] map)
        {
            int w = map.GetLength(1);
            int h = map.GetLength(0);
            var im = new Image<Gray, float>(w, h);
            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    im.Data[j, i, 0] = map[j, i];
                }
            }
            return im.Mat;
        }
        static public byte[,,] normalyse(float[,,] mat)
        {
            int w = mat.GetLength(0);
            int h = mat.GetLength(1);
            int d = mat.GetLength(2);
            var koef = normalyseKoef(mat, 0, 255);
            var K = koef[0];
            var offs = koef[1];

            var data = new byte[w, h, d];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    for (int k = 0; k < d; k++)
                    {
                        var val = mat[i, j, k];
                        data[i, j, k] = (byte)((val - offs) * K);
                    }
                }
            }
            return data;
        }

        static public float[] normalyseKoef(float[,,] mat, float minN, float maxN)//{ k, offset}
        {
            float max = float.MinValue;
            float min = float.MaxValue;
            int w = mat.GetLength(0);
            int h = mat.GetLength(1);
            int d = mat.GetLength(2);

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    for (int k = 0; k < d; k++)
                    {
                        var val = mat[i, j, k];
                        if (val > max)
                        {
                            max = val;
                        }
                        if (val < min)
                        {
                            min = val;
                        }
                    }
                }
            }
            var delt = max - min;
            var deltN = maxN - minN;
            float K = 1;
            Console.WriteLine("delt " + delt);
            if (delt > 0)
            {
                K = deltN / delt;
            }
            var offset = min * K;//!!!!!!


            return new float[] { K, min };
        }

        static public Mat epipolarTest(Mat matL, Mat matR)
        {
            // var _imL = new Mat();
            var imL = matL.ToImage<Gray, byte>();
            var imR = matR.ToImage<Gray, byte>();

            CvInvoke.Resize(imL, imL, new Size(600, 600));
            CvInvoke.Resize(imR, imR, new Size(600, 600));
            var minDisparity = 0;
            var numDisparities = 64;
            var blockSize = 8;
            var disp12MaxDiff = 1;
            var uniquenessRatio = 10;
            var speckleWindowSize = 100;
            var speckleRange = 8;
            var p1 = 8 * imL.NumberOfChannels * blockSize * blockSize;
            var p2 = 32 * imL.NumberOfChannels * blockSize * blockSize;
            var stereo = new StereoSGBM(minDisparity, numDisparities, blockSize, p1, p2, disp12MaxDiff, 8, uniquenessRatio, speckleWindowSize, speckleRange, StereoSGBM.Mode.SGBM);
            var disp = new Mat();
            stereo.Compute(imL, imR, disp);
            //CvInvoke.Imshow("imL", imL);
            //CvInvoke.Imshow("imR", imR);
            // CvInvoke.Imshow("epipolar0", disp);
            // Console.WriteLine(disp.max)
            // CvInvoke.Normalize(disp, disp, 0, 255, NormType.MinMax);
            // CvInvoke.Imshow("epipolar", disp);
            return disp;
        }
        static public Mat drawDisparityMap(ref Mat mat1, ref Mat mat2)
        {
            var kps1 = new VectorOfKeyPoint();
            var desk1 = new Mat();
            var kps2 = new VectorOfKeyPoint();
            var desk2 = new Mat();
            var detector_ORB = new Emgu.CV.Features2D.ORBDetector();

            //CvInvoke.StereoRectify();
            // CvInvoke.ComputeCorrespondEpilines();
            detector_ORB.DetectAndCompute(mat1, null, kps1, desk1, false);
            detector_ORB.DetectAndCompute(mat2, null, kps2, desk2, false);
            var matcher = new Emgu.CV.Features2D.BFMatcher(Emgu.CV.Features2D.DistanceType.Hamming, true);
            var matches = new VectorOfDMatch();
            matcher.Match(desk1, desk2, matches);
            var mat3 = new Mat();
            Emgu.CV.Features2D.Features2DToolbox.DrawMatches(mat1, kps1, mat2, kps2, matches, mat3, new MCvScalar(255, 0, 0), new MCvScalar(0, 0, 255));
            return mat3;
        }
        static public Mat drawChessboard(Mat im, Size size, bool subpix = false,bool blur = false, CalibCbType calibCbType = CalibCbType.AdaptiveThresh, System.Drawing.PointF[] points = null)
        {
            if(im==null)
            {
                return null;
            }
            var corn = new VectorOfPointF();
            var mat_ch = im.Clone();
            var gray = mat_ch.ToImage<Gray, byte>();
            
            if(blur)
            {
                CvInvoke.GaussianBlur(gray, gray, new Size(5, 5), 0);
            }
           
            var ret = CvInvoke.FindChessboardCorners(gray, size, corn, calibCbType);
            if(subpix)
            {
                CvInvoke.CornerSubPix(gray, corn, new Size(5, 5), new Size(-1, -1), new MCvTermCriteria(30, 0.001));
            }

            //perspective2Dmatr(size, corn);
            
            Console.WriteLine("chess: " + ret + " " + size.Width + " " + size.Height);
            
            CvInvoke.DrawChessboardCorners(mat_ch, size, corn, ret);

            return mat_ch;
           // return gray.Mat;
        }

        static public Mat drawInsideRectChessboard(Mat im, Size size, bool subpix = false, bool blur = false, CalibCbType calibCbType = CalibCbType.AdaptiveThresh)
        {
            if (im == null)
            {
                return null;
            }
            var corn = new VectorOfPointF();
            var mat_ch = im.Clone();
            var gray = mat_ch.ToImage<Gray, byte>();

            if (blur)
            {
                CvInvoke.GaussianBlur(gray, gray, new Size(5, 5), 0);
            }

            var ret = CvInvoke.FindChessboardCorners(gray, size, corn, calibCbType);
            if (subpix)
            {
                CvInvoke.CornerSubPix(gray, corn, new Size(5, 5), new Size(-1, -1), new MCvTermCriteria(30, 0.001));
            }

            var gab = takeGabObp(corn.ToArray(), size);
            var ps_ins = GeometryAnalyse.compPointsInsideRectWarp(gab, size);

            //perspective2Dmatr(size, corn);

            Console.WriteLine("chess: " + ret + " " + size.Width + " " + size.Height);

            //CvInvoke.DrawChessboardCorners(mat_ch, size, corn, ret);
            //drawPointsF(mat_ch, ps_ins, 255, 0, 0, 2);
            //drawPointsF(mat_ch, gab, 0, 255, 0, 4);
            drawMatches(mat_ch, ps_ins, corn.ToArray(), 255, 0, 0);
            return mat_ch;
            // return gray.Mat;
        }

        static public Mat drawInsideRectCirc(Mat im, Size size,bool blur = false)
        {
            if (im == null)
            {
                return null;
            }
            var mat_ch = im.Clone();
            var gray = mat_ch.ToImage<Gray, byte>();

            if (blur)
            {
                CvInvoke.GaussianBlur(gray, gray, new Size(5, 5), 0);
            }

            var len = size.Width * size.Height;
            var corn = new System.Drawing.PointF[len];
            //FindCircles.findCircles(im, corn, size);
            GeometryAnalyse.findCirclesIter(im,ref corn, size);
            var gab = takeGabObp(corn, size);
            var ps_ins = GeometryAnalyse.compPointsInsideRectWarp(gab, size,im);
            
            //perspective2Dmatr(size, corn);

            Console.WriteLine("chess: "  + size.Width + " " + size.Height);

            //CvInvoke.DrawChessboardCorners(mat_ch, size, corn, ret);
            //drawPointsF(mat_ch, ps_ins, 255, 0, 0, 2);
            //drawPointsF(mat_ch, gab, 0, 255, 0, 4);
            drawMatches(mat_ch, ps_ins, corn, 255, 0, 0);
            return mat_ch;
            // return gray.Mat;
        }


        static Mat perspective2Dmatr(Size size,VectorOfPointF corn)
        {
            var w = size.Width;
            var h = size.Height;
            var ps = generatePoints(size, 100f);
            var points_1 = new System.Drawing.PointF[4];
            var points_2 = new System.Drawing.PointF[4];
            var inds_1 = new int[4] { 0, w - 1, w * (h - 1), w * h - 1 };
            var inds_2 = new int[4] { 0, h - 1, h * (w - 1), h * w - 1 };
            for (int i=0; i<4; i++)
            {
                int ind_1 = inds_1[i];
                int ind_2 = inds_2[i];
                points_1[i] = corn[ind_1];
                points_2[i] = ps[ind_2];
            }
            var perspMatr = CvInvoke.GetPerspectiveTransform(points_1, points_2);
            prin.t("perspMatr");
            prin.t(perspMatr);
            return perspMatr;
        }
        public static System.Drawing.PointF[] generatePoints(Size size, float side = 1f)
        {
            var ps = new System.Drawing.PointF[size.Width * size.Height];
            int ind = 0;
            for(int i = 0; i<size.Width;i++)
            {
                for (int j = 0; j < size.Height; j++)
                {
                    ps[ind] = new System.Drawing.PointF(i * side, j * side); ind++;
                }
            }
            return ps;
        }

        static public bool compPatternCoords(Mat mat, ref MCvPoint3D32f[] obp, ref System.Drawing.PointF[] cornF, Size size, int kernel = 11,PatternType patternType = PatternType.Mesh)
        {
            var corn = new VectorOfPointF(cornF);
            obp = new MCvPoint3D32f[size.Width * size.Height];
            int ind = 0;
            for (int j = 0; j < size.Height; j++)
            {
                for (int i = 0; i < size.Width; i++)
                {
                    obp[ind] = new MCvPoint3D32f((float)i, (float)j, 0.0f);
                    ind++;
                }
            }
            bool ret = false;
            if(patternType==PatternType.Chess)
            {
                var gray = mat.ToImage<Gray, byte>();
                ret = CvInvoke.FindChessboardCorners(gray, size, corn, CalibCbType.FastCheck);
                if (ret)
                {
                    CvInvoke.CornerSubPix(gray, corn, new Size(kernel, kernel), new Size(-1, -1), new MCvTermCriteria(100, 0.0001));
                    //CvInvoke.DrawChessboardCorners(mat, size, corn, ret);
                    //CvInvoke.Imshow("2", mat);
                    cornF = corn.ToArray();
                }
            }
            else
            {
                mat = FindCircles.findCircles(mat,ref cornF, size);
                //CvInvoke.Imshow("2", mat);
                if (mat != null)
                {
                    ret = true;

                }
            }


            
            return ret;
        }


        static public Mat remapDistIm(Mat mat, Matrix<double> matrixCamera, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();

            var roi = computeDistortionMaps(ref mapx, ref mapy, matrixCamera, matrixDistCoef, mat.Size);
            var invmap = remap(mapx, mapy, mat);
            //CvInvoke.Rectangle(invmap, roi, new MCvScalar(255, 0, 0));

            //return  new Mat(invmap,roi);
            return invmap;
        }
        static public Mat remapUnDistIm(Mat mat, Matrix<double> matrixCamera, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();
            var size = mat.Size;
            matrixCamera[0, 2] = size.Width / 2;
            matrixCamera[1, 2] = size.Height / 2;
            var matr = new Mat();
            CvInvoke.InitUndistortRectifyMap(matrixCamera, matrixDistCoef, null, matr, mat.Size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(mat, und_pic, mapx, mapy, Inter.Linear);
            return und_pic;
        }
        static public Mat remapDistImOpenCvCentr(Mat mat, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();
            var size = mat.Size;
            var matr = new Mat();
            var reversDistor = new Matrix<double>(5, 1);
            for (int i = 0; i < 5; i++)
            {
                reversDistor[i, 0] = -matrixDistCoef[i, 0];
            }
            double fov = 53;
            //_x = _z * Math.Tan(toRad(53 / 2))
            var fxc = size.Width / 2;
            var fyc = size.Height / 2;
            var f = size.Width / (2 * Math.Tan(UtilMatr.toRad((float)(fov / 2))));
            var matrixData = new double[3, 3] { { f, 0, fxc }, { 0, f, fyc }, { 0, 0, 1 } };
            var matrixData_T = matrixData.Transpose();
            var matrixCamera = new Matrix<double>(matrixData);
            // print(matrixCamera);
            CvInvoke.InitUndistortRectifyMap(matrixCamera, reversDistor, null, matr, size, DepthType.Cv32F, mapx, mapy);

            var und_pic = new Mat();
            CvInvoke.Remap(mat, und_pic, mapx, mapy, Inter.Linear);
            double k = 0.8;
            var nw = (int)(size.Width * k);
            var nh = (int)(size.Height * k);
            var x = (size.Width - nw) / 2;
            var y = (size.Height - nh) / 2;
            und_pic = new Mat(und_pic,new Rectangle(x,y,nw,nh));
            CvInvoke.Resize(und_pic, und_pic,size);
            return und_pic;
        }
        static public Mat remapDistImOpenCv(Mat mat, Matrix<double> matrixCamera, Matrix<double> matrixDistCoef)
        {
            var mapx = new Mat();
            var mapy = new Mat();
            var matr = new Mat();
            var size = mat.Size;
            var reversDistor = new Matrix<double>(5, 1);
            for (int i = 0; i < 5; i++)
            {
                reversDistor[i, 0] = -matrixDistCoef[i, 0];
            }
            CvInvoke.InitUndistortRectifyMap(matrixCamera, reversDistor, null, matr, size, DepthType.Cv32F, mapx, mapy);
            var und_pic = new Mat();
            CvInvoke.Remap(mat, und_pic, mapx, mapy, Inter.Linear);
            return und_pic;
        }
        static public Mat Minus(Mat mat1, Mat mat2)
        {
            var data1 = (byte[,,])mat1.GetData();
            var w1 = data1.GetLength(0);
            var h1 = data1.GetLength(1);
            var data2 = (byte[,,])mat2.GetData();
            var w2 = data2.GetLength(0);
            var h2 = data2.GetLength(1);
            var w = Math.Min(w1, w2);
            var h = Math.Min(h1, h2);

            var data = new byte[w, h, 1];
            Console.WriteLine("w h " + w + " " + h);
            var cut_map1 = new Mat(mat1, new Rectangle(0, 0, h, w));
            var cut_map2 = new Mat(mat2, new Rectangle(0, 0, h, w));
            /* for (int i=0; i< w; i++)
             {
                 for (int j = 0; j < h; j++)
                 {
                     var val = data1[i, j,0] - data2[i, j,0] + 127;
                     if(val>255)
                     {
                         val = 255;
                     }
                     if (val<0)
                     {
                         val = 0;
                     }
                     data[i, j,0] =  (byte)val;
                 }
             }*/
            //return new Image<Gray,byte>(data).Mat;
            return cut_map1 - cut_map2;
        }

        static public Mat invRemap(float[,] mapx, float[,] mapy, Mat mat)
        {
            return remap(
                mapToMat(inverseMap(mapx, PlanType.X)),
                 mapToMat(inverseMap(mapy, PlanType.Y)),
                 mat);
        }
        static public Mat invRemap(Mat mapx, Mat mapy, Mat mat)
        {
            return invRemap(
                  mapToFloat(mapx),
                  mapToFloat(mapy),
                  mat);
        }
        static public float[,] mapToFloat(Mat map)
        {
            return (float[,])map.GetData();
        }

        static public Size findRemapSize(float[,] mapx, float[,] mapy)
        {
            var x = mapx.Max();// findMaxX(mapx);
            var y = mapy.Max();// findMaxY(mapy);
            Console.WriteLine("FLOAT SIZE: " + x + " " + y);
            var ix = (int)Math.Round(x, 0) + 4;
            var iy = (int)Math.Round(y, 0) + 4;
            return new Size(ix, iy);
        }


        static public float[,] inverseMap(float[,] map, PlanType planType)
        {
            int w = map.GetLength(1);
            int h = map.GetLength(0);
            var inv_map = new float[h, w];

            switch (planType)
            {
                case PlanType.X:
                    var deltE = w;
                    var deltI = (map.Max() - map.Min());
                    var k = deltI / deltE;
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var del = (map[j, i] - i) / k;
                            inv_map[j, i] = i - del;
                        }
                    }
                    break;
                case PlanType.Y:
                    deltE = h;
                    deltI = (map.Max() - map.Min());
                    k = deltI / deltE;
                    for (int i = 0; i < w; i++)
                    {
                        for (int j = 0; j < h; j++)
                        {
                            var del = (map[j, i] - j) / k;
                            inv_map[j, i] = j - del;
                        }
                    }
                    break;
            }
            return inv_map;
        }

        static public float[,,] compRemap(float[,] mapx, float[,] mapy, Mat mat)
        {
            var size = findRemapSize(mapx, mapy);
            var im = mat.ToImage<Bgr, byte>();
            Console.WriteLine("NEW SIZE: " + size.Width + " " + size.Height);
            var data = new float[size.Height, size.Width, 3];
            Console.WriteLine(data.GetLength(0) + " " + data.GetLength(1) +
                " " + mapx.GetLength(0) + " " + mapx.GetLength(1) +
                " " + im.Data.GetLength(0) + " " + im.Data.GetLength(1) + " ");
            var size_p = im.Size;
            for (int i = 1; i < size_p.Width - 2; i++)
            {
                for (int j = 1; j < size_p.Height - 2; j++)
                {
                    var x = mapx[j, i];
                    var y = mapy[j, i];
                    var w = 0f;
                    var h = 0f;
                    if (j == 0)
                    {
                        h = Math.Abs(mapy[j, i] - mapy[j + 1, i]);
                    }
                    else
                    {
                        h = Math.Abs(mapy[j, i] - mapy[j - 1, i]);
                    }
                    if (i == 0)
                    {
                        w = Math.Abs(mapx[j, i] - mapx[j, i + 1]);
                    }
                    else
                    {
                        w = Math.Abs(mapx[j, i] - mapx[j, i - 1]);
                    }
                    var sq1 = new Square(x, y, w, h);
                    //Console.WriteLine("wh " + w + " " + h);
                    var ix = (int)Math.Round(x, 0);
                    var iy = (int)Math.Round(y, 0);

                    for (int _i = ix - 1; _i <= ix + 1; _i++)
                    {
                        for (int _j = iy - 1; _j <= iy + 1; _j++)
                        {
                            var sq2 = new Square(_i, _j, 1, 1);
                            var intens = compCrossArea(sq1, sq2);
                            //Console.WriteLine(" _i " + _i+ " _j " + _j + " i " + i + " j " + j);
                            data[_j, _i, 0] += intens * im.Data[j, i, 0];
                            data[_j, _i, 1] += intens * im.Data[j, i, 1];
                            data[_j, _i, 2] += intens * im.Data[j, i, 2];

                        }
                    }

                }
            }
            return data;
        }

        static public float compCrossArea(Square s1, Square s2)
        {
            var dx = compDelt(s1.w, s2.w, Math.Abs(s1.x - s2.x));
            var dy = compDelt(s1.h, s2.h, Math.Abs(s1.y - s2.y));
            return Math.Abs(dx * dy);
        }
        static public float compDelt(float w1, float w2, float dx)
        {
            float dw = 0;
            if (w2 < w1)
            {
                var lam = w1;
                w1 = w2;
                w2 = lam;
            }
            if (dx < (w1 + w2) / 2)//условие пересечения
            {
                if (dx > w2 / 2)
                {
                    if ((w1 / 2 + dx) <= w2 / 2)
                    {
                        dw = w1;
                        return dw;
                    }
                    else
                    {
                        dw = (w1 + w2) / 2 - dx;
                        return dw;
                    }
                }
                else
                {
                    if (w1 <= w2 / 2)
                    {
                        dw = (w1 + w2) / 2 - dx;
                        return dw;
                    }
                    else
                    {
                        if ((w1 / 2 + dx) >= w2 / 2)
                        {
                            dw = w2 / 2 + (w1 / 2 - dx);
                            return dw;
                        }
                        else
                        {
                            dw = w1;
                            return dw;
                        }
                    }
                }
            }
            else
            {
                return dw;
            }
        }

        static public byte[,,] toByte(float[,,] arr)
        {
            var byte_arr = new byte[arr.GetLength(0), arr.GetLength(1), arr.GetLength(2)];
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {
                    for (int k = 0; k < arr.GetLength(2); k++)
                    {
                        if (arr[i, j, k] > 255)
                        {
                            byte_arr[i, j, k] = 255;
                        }

                        else
                        {
                            byte_arr[i, j, k] = (byte)Math.Round(arr[i, j, k], 0);
                        }

                    }
                }
            }
            return byte_arr;
        }

        static public byte[,,] toByteGray(float[,,] arr)
        {
            var byte_arr = new byte[arr.GetLength(0), arr.GetLength(1), 1];
            for (int i = 0; i < arr.GetLength(0); i++)
            {
                for (int j = 0; j < arr.GetLength(1); j++)
                {

                    if (arr[i, j, 0] > 255)
                    {
                        byte_arr[i, j, 0] = 255;
                    }

                    else
                    {
                        byte_arr[i, j, 0] = (byte)Math.Round(arr[i, j, 0], 0);
                    }

                }
            }
            return byte_arr;
        }
        static public Mat remap(Mat _mapx, Mat _mapy, Mat mat)
        {

            var mapx = compUnsignedMap((float[,])_mapx.GetData(), PlanType.X);
            var mapy = compUnsignedMap((float[,])_mapy.GetData(), PlanType.Y);

            mapx = (float[,])_mapx.GetData();
            mapy = (float[,])_mapy.GetData();
            //  print("________________________________");
            // print(mapy);
            // print("________________________________");
            var data = compRemap(mapx, mapy, mat);

            var color = 0;
            try
            {
                color = mat.GetData().GetLength(2);
                Console.WriteLine("color" + color);
            }
            catch
            {
                color = 1;
                prin.t("color " + color);
            }

            if (color == 1)
            {
                var im = new Image<Gray, byte>(toByteGray(data));

                return im.Mat;
            }
            else if (color == 3)
            {
                var im = new Image<Bgr, byte>(toByte(data));
                return im.Mat;
            }


            //print(mapx);
            // print("________________________________");
            //print(mapy);
            return null;
        }

        static public float[,] compUnsignedMap(float[,] map, PlanType planType)
        {
            float min = float.MaxValue;
            float[,] rmap = new float[map.GetLength(0), map.GetLength(1)];
            if (planType == PlanType.X)
            {
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    if (min > map[i, 0])
                    {
                        min = map[i, 0];
                    }
                }
                min -= 1;
            }
            else if (planType == PlanType.Y)
            {
                for (int i = 0; i < map.GetLength(1); i++)
                {
                    if (min > map[0, i])
                    {
                        min = map[0, i];
                    }
                }
                min -= 1;
            }
            if (min < 0)
            {
                for (int i = 0; i < map.GetLength(0); i++)
                {
                    for (int j = 0; j < map.GetLength(1); j++)
                    {
                        rmap[i, j] = map[i, j] - min;

                    }
                }
            }
            else
            {
                return map;
            }

            return rmap;
        }



        static public PointF calcDistorcPix(int dim, int xd, int yd, double xc, double yc, Matrix<double> distCoefs)
        {


            var K1 = distCoefs[0, 0];
            var K2 = distCoefs[1, 0];
            var P1 = distCoefs[2, 0];
            var P2 = distCoefs[3, 0];
            var K3 = distCoefs[4, 0];

            var delt = new PointF((float)((double)xd - xc), (float)((double)yd - yc));
            var r = (double)delt.norm;
            var r2 = Math.Pow(r, 2);
            var r4 = Math.Pow(r, 4);
            var r6 = Math.Pow(r, 6);

            var delx = xd - xc;
            var dely = yd - yc;

            var xu = xd + delx * (K1 * r2 + K2 * r4 + K3 * r6) + P1 * (r2 + 2 * Math.Pow(delx, 2)) + 2 * P2 * delx * dely;
            var yu = yd + dely * (K1 * r2 + K2 * r4 + K3 * r6) + 2 * P1 * delx * dely + P2 * (r2 + 2 * Math.Pow(delx, 2));


            return new PointF(xu, yu);
        }
        static public PointF calcDistorcPix_BC(int dim, int _xd, int _yd, double _xc, double _yc, Matrix<double> distCoefs)
        {

            var K1 = distCoefs[0, 0];
            var K2 = distCoefs[1, 0];
            var P1 = distCoefs[2, 0];
            var P2 = distCoefs[3, 0];
            var K3 = distCoefs[4, 0];
            float xd = (float)_xd / dim;
            float yd = (float)_yd / dim;

            float xc = (float)_xc / dim;
            float yc = (float)_yc / dim;
            var delt = new PointF((float)((double)xd - xc), (float)((double)yd - yc));
            var r = (double)delt.norm;
            var r2 = Math.Pow(r, 2);
            var r4 = Math.Pow(r, 4);
            var r6 = Math.Pow(r, 6);

            var delx = xd - xc;
            var dely = yd - yc;


            var _xu = xc + delx / (1 + K1 * r2 + K2 * r4 + K3 * r6);
            var _yu = yc + dely / (1 + K1 * r2 + K2 * r4 + K3 * r6);

            // var xu = xc + delx *(1 + K1 * r2 + K2 * r4 + K3 * r6);
            // var yu = yc + dely * (1 + K1 * r2 + K2 * r4 + K3 * r6);

            return new PointF(_xu * dim, _yu * dim);
        }

        static public float findMaxMin(float[,] map, int col, PlanType planType, int maxminF)//min - 0; max - 1;
        {
            float maxmin = 0;
            int b_i_ind = 0;
            int b_j_ind = 0;
            int e_i_ind = 0;
            int e_j_ind = 0;
            if (planType == PlanType.X)
            {
                b_i_ind = 0;
                e_i_ind = map.GetLength(0) - 1;

                b_j_ind = col;
                e_j_ind = col;
            }
            else if (planType == PlanType.Y)
            {
                b_i_ind = col;
                e_i_ind = col;

                b_j_ind = 0;
                e_j_ind = map.GetLength(1) - 1;
            }
            if (maxminF == 0)
            {
                maxmin = float.MaxValue;
            }
            else
            {
                maxmin = float.MinValue;
            }

            for (int i = b_i_ind; i <= e_i_ind; i++)
            {
                for (int j = b_j_ind; j <= e_j_ind; j++)
                {
                    if (maxminF == 0)
                    {
                        if (map[i, j] < maxmin)
                        {
                            maxmin = map[i, j];
                        }
                    }
                    else
                    {
                        if (map[i, j] > maxmin)
                        {
                            maxmin = map[i, j];
                        }
                    }


                }
            }
            return maxmin;
        }
        static public Rectangle compROI(Mat mapx, Mat mapy)
        {
            return compROI((float[,])mapx.GetData(), (float[,])mapy.GetData());
        }
        static public Rectangle compROI(float[,] mapx, float[,] mapy)
        {
            float L = findMaxMin(mapx, 0, PlanType.X, 1);
            float R = findMaxMin(mapx, mapx.GetLength(1) - 1, PlanType.X, 0);

            float U = findMaxMin(mapy, 0, PlanType.Y, 1);
            float D = findMaxMin(mapy, mapx.GetLength(0) - 1, PlanType.Y, 0);

            //Console.WriteLine("L R U D " + L + " " + R + " " + U + " " + D + " ");
            return new Rectangle((int)L, (int)U, (int)(R - L), (int)(D - U));
        }
        static public Rectangle newRoi(Size size, double xc, double yc, Matrix<double> distCoefs, Func<int, int, int, double, double, Matrix<double>, PointF> calcDistPix)
        {
            var p1 = calcDistPix(size.Width, 0, 0, xc, yc, distCoefs);
            var p2 = calcDistPix(size.Width, size.Width, 0, xc, yc, distCoefs);
            var p3 = calcDistPix(size.Width, size.Width, size.Height, xc, yc, distCoefs);
            var p4 = calcDistPix(size.Width, 0, size.Height, xc, yc, distCoefs);
            prin.t(p1 + " " + p2 + " " + p3 + " " + p4 + " ");
            return RoiFrom4Points(p1, p2, p3, p4);
        }
        static public Rectangle RoiFrom4Points(PointF p1, PointF p2, PointF p3, PointF p4)
        {
            int x = 0, y = 0, xW = 1, yH = 1;
            if (p1.X >= p4.X) { x = (int)p1.X; } else { x = (int)p4.X; }
            if (p1.Y >= p2.Y) { x = (int)p1.Y; } else { x = (int)p2.Y; }

            if (p3.X >= p2.X) { x = (int)p2.X; } else { x = (int)p3.X; }
            if (p3.Y >= p4.Y) { x = (int)p4.Y; } else { x = (int)p3.Y; }
            return new Rectangle(x, y, (xW - x), (yH - y));
        }
        static public Rectangle computeDistortionMaps(ref Mat _mapx, ref Mat _mapy, Matrix<double> cameraMatr, Matrix<double> distCoefs, Size size)
        {

            Matrix<float> mapx = new Matrix<float>(size.Height, size.Width);
            Matrix<float> mapy = new Matrix<float>(size.Height, size.Width);
            double xc = cameraMatr[0, 2];
            double yc = cameraMatr[1, 2];
            xc = size.Width / 2;
            yc = size.Height / 2;
            Console.WriteLine("---xcyc-- " + xc + " " + yc);
            // print(cameraMatr);
            //print(mapx);
            for (int i = 0; i < size.Height; i++)
            {
                for (int j = 0; j < size.Width; j++)
                {

                    var p = calcDistorcPix_BC(size.Width, j, i, xc, yc, distCoefs);
                    mapx[i, j] = p.X;
                    mapy[i, j] = p.Y;
                    //Console.WriteLine(i + " " + j);
                }
            }

            _mapx = mapx.Mat;
            _mapy = mapy.Mat;
            return compROI(_mapx, _mapy);

        }
        static public void distortFolder(string path, CameraCV cameraCV)
        {

            var frms = FrameLoader.loadImages_test(path);
            var distPath = Path.Combine(path, "distort");

            var fr1 = from f in frms
                      orderby f.dateTime.Ticks
                      select f; 
            var vfrs = fr1.ToList();
            int ind = 0;
            foreach (var fr in vfrs)
            {
               // var matD = remapDistIm(fr.im, cameraCV.cameramatrix, cameraCV.distortmatrix);
                var matD = remapDistImOpenCv(fr.im, cameraCV.cameramatrix, cameraCV.distortmatrix);
                saveImage(matD, distPath, ind + " " + fr.name);
                ind++;
            }
        }
        static public System.Drawing.PointF[] matToPointF(Mat mat)
        {
            var data = (float[,])mat.GetData();
            var ps = new System.Drawing.PointF[data.GetLength(0)];
            for (int i=0; i< data.GetLength(0);i++)
            {
                ps[i] = new System.Drawing.PointF(data[i, 0], data[i, 1]);
            }
            return ps;
        }
        static public  Mat pointFTomat(System.Drawing.PointF[] ps)
        {
            var data = new float[ps.Length, 2];
           // var ps = new System.Drawing.PointF[data.GetLength(0)];
            for (int i = 0; i < data.GetLength(0); i++)
            {
                data[i, 0] = ps[i].X;
                data[i, 1] = ps[i].Y;
            }
            return new Matrix<float>(data).Mat;
        }

        static public Mat Mcvscalar_to_mat(MCvScalar[] ps)
        {
            var data = new double[ps.Length, 3];
            // var ps = new System.Drawing.PointF[data.GetLength(0)];
            for (int i = 0; i < data.GetLength(0); i++)
            {
                data[i, 0] = ps[i].V0;
                data[i, 1] = ps[i].V1;
                data[i, 2] = ps[i].V2;
            }
            return new Matrix<double>(data).Mat;
        }
        static public Mat data1ch_to_mat(double[] data)//ps.len x 1
        {
            return new Matrix<double>(data).Mat;
        }
        static MCvScalar mean_std_dev(MCvScalar[] ps_pix)
        {
            var ps = UtilOpenCV.Mcvscalar_to_mat(ps_pix);

            MCvScalar mean = new MCvScalar(0, 0, 0);
            MCvScalar std_dev = new MCvScalar(0, 0, 0);
            CvInvoke.MeanStdDev(ps, ref mean, ref std_dev);
            return new MCvScalar(mean.V0, std_dev.V0);
        }
        public static MCvScalar mean_std_dev(double[] ps_pix)
        {
            var ps = data1ch_to_mat(ps_pix);

            MCvScalar mean = new MCvScalar(0);
            MCvScalar std_dev = new MCvScalar(0);
            CvInvoke.MeanStdDev(ps, ref mean, ref std_dev);
            return new MCvScalar(mean.V0, std_dev.V0);
        }
        public static (MCvScalar, MCvScalar) std_dev_3ch(double[,] ps_pix)
        {
            var data_r = ps_pix.GetColumn(0);
            var data_g = ps_pix.GetColumn(1);
            var data_b = ps_pix.GetColumn(2);
            var md_r = mean_std_dev(data_r);
            var md_g = mean_std_dev(data_g);
            var md_b = mean_std_dev(data_b);

            MCvScalar mean = new MCvScalar(md_r.V0, md_g.V0, md_b.V0);
            MCvScalar std_dev = new MCvScalar(md_r.V1, md_g.V1, md_b.V1);
            return (mean, std_dev);
        }
        static public Mat p3d_to_mat(Point3d_GL[] ps)
        {
            var data = new double[ps.Length, 3];
            // var ps = new System.Drawing.PointF[data.GetLength(0)];
            for (int i = 0; i < data.GetLength(0); i++)
            {
                data[i, 0] = ps[i].x;
                data[i, 1] = ps[i].y;
                data[i, 2] = ps[i].z;
            }
            return new Matrix<double>(data).Mat;
        }

        public static System.Drawing.PointF[][] dividePointF(System.Drawing.PointF[] ps, int len0)
        {
            prin.t(ps.Length);
            prin.t("ps.Length");
            if (ps.Length%len0!=0)
            {
                return null;
            }
            var ps2d = new System.Drawing.PointF[len0][];
            var len1 = ps.Length / len0;
            for (int i=0; i<len0;i++)
            {
                ps2d[i] = new System.Drawing.PointF[len1];
                for (int j = 0; j < len1; j++)
                {
                    ps2d[i][j] = ps[i * len1 + j];
                }
            }
            return ps2d;
        }

        #region gen_board
        public static Mat[] generateImage_chessboard(int n, int m,int side = 100)//!!!!!!!!!!remake
        {
  
            int q_side = side / 2;
            int im_side_w = q_side * (n + 2);
            int im_side_h = q_side * (m + 2);
            var im_ret = new Image<Bgr, Byte>(im_side_w, im_side_h);
            var pattern_s = new Size(side, side);

            var quad_s = new Size(q_side, q_side);
            /*Console.WriteLine(k + "k-");
            Console.WriteLine(side + "s-");
            Console.WriteLine(q_side + "q-");*/

            var p_start = new List<Point>();
            var offx = pattern_s.Width/2;
            var offy = pattern_s.Height/2;
            var w_cv = n - 1;
            var h_cv = m - 1;
            var points_cv = new float[w_cv * h_cv, 2];
            var points_all = new float[n*m*2, 2];
            int ind = 0;
            for (int x = 0; x < w_cv ; x++)
            {
                for (int y = h_cv-1; y >= 0 ; y--)
                {
                    points_cv[ind, 0] = (x + 2) * q_side;
                    points_cv[ind, 1] = (y + 2) * q_side;
                    ind++;
                }
            }


            ind = 0;
            for (int x = offx; x < im_ret.Width - q_side; x += pattern_s.Width)
            {
                for (int y = offy; y < im_ret.Height - q_side; y += pattern_s.Height)
                {
                    p_start.Add(new Point(x, y));
                    points_all[ind, 0] = x;
                    points_all[ind, 1] = y; ind++;

                    points_all[ind, 0] = x + q_side;
                    points_all[ind, 1] = y; ind++;

                    points_all[ind, 0] = x;
                    points_all[ind, 1] = y + q_side; ind++;

                    points_all[ind, 0] = x + q_side;
                    points_all[ind, 1] = y + q_side; ind++;
                }
            }
            for (int x = q_side + offx; x < im_ret.Width - q_side; x += pattern_s.Width)
            {
                for (int y = q_side + offy; y < im_ret.Height - q_side; y += pattern_s.Height)
                {
                    p_start.Add(new Point(x, y));
                    points_all[ind, 0] = x;
                    points_all[ind, 1] = y; ind++;

                    points_all[ind, 0] = x + q_side;
                    points_all[ind, 1] = y; ind++;

                    points_all[ind, 0] = x;
                    points_all[ind, 1] = y + q_side; ind++;

                    points_all[ind, 0] = x + q_side;
                    points_all[ind, 1] = y + q_side; ind++;
                }
            }
            Console.WriteLine("point all len0: " + points_all.GetLength(0)+" ind : "+ind);
            Console.WriteLine(p_start.Count);
            for (int x = 0; x < im_ret.Width; x++)
            {
                for (int y = 0; y < im_ret.Height; y++)
                {
                    im_ret.Data[y, x, 0] = 255;
                    im_ret.Data[y, x, 1] = 255;
                    im_ret.Data[y, x, 2] = 255;
                }
            }

            for (int i = 0; i < p_start.Count; i++)
            {
                for (int x = p_start[i].X ; x < p_start[i].X + quad_s.Width; x++)
                {
                    for (int y = p_start[i].Y ; y < p_start[i].Y + quad_s.Height; y++)
                    {
                        im_ret.Data[y, x, 0] = 0;
                        im_ret.Data[y, x, 1] = 0;
                        im_ret.Data[y, x, 2] = 0;
                    }
                }
            }
            im_ret.Save("black_sq_br_" + n + "_" + m + ".png");
            return new Mat[] { im_ret.Mat, new Matrix<float>(points_cv).Mat, new Matrix<float>(points_all).Mat } ;
        }

        public static Mat[] generateImage_chessboard_circle(int n, int m, int q_side = 100)//!!!!!!!!!!remake
        {
            int im_side_w = q_side * (n + 1);
            int im_side_h = q_side * (m + 1);
            var im_ret = new Image<Bgr, Byte>(im_side_w, im_side_h);
            var p_start = new List<Point>();     
            var points_all = new float[n * m , 2];

            int ind = 0;

            for (int i = 0; i < n; i ++)
            {
                for (int j = 0; j < m; j ++)
                {
                    var x =  (i+1) * q_side;
                    var y =  (j+1) * q_side;
                    p_start.Add(new Point(x, y));
                    points_all[ind, 0] = x;
                    points_all[ind, 1] = y; ind++;

                }
            }

            Console.WriteLine("point all len0: " + points_all.GetLength(0) + " ind : " + ind);
            Console.WriteLine(p_start.Count);
            for (int x = 0; x < im_ret.Width; x++)
            {
                for (int y = 0; y < im_ret.Height; y++)
                {
                    im_ret.Data[y, x, 0] = 255;
                    im_ret.Data[y, x, 1] = 255;
                    im_ret.Data[y, x, 2] = 255;
                }
            }

            for (int i = 0; i < p_start.Count; i++)
            {   
               CvInvoke.Circle(im_ret, new Point(p_start[i].X, p_start[i].Y), (int)(q_side / 2.3), new MCvScalar(0, 0, 0),-1);       
            }
            im_ret.Save("black_br_" + n + "_" + m + ".png");
            return new Mat[] { im_ret.Mat, new Matrix<float>(points_all).Mat, new Matrix<float>(points_all).Mat };
        }
        static public float[][] generate_BOARDs(MCvPoint3D32f[][] point3D32Fs)
        {
            var boards = new float[point3D32Fs.Length][];
            for (int i = 0; i < point3D32Fs.Length; i++)
            {
                var board = new List<float>();                
                for (int j = 0; j < point3D32Fs[i].Length; j += 4)
                {
                    float[] square_buf = {
                            point3D32Fs[i][j].X  , point3D32Fs[i][j].Y  , 0.0f, // triangle 1 : begin
                            point3D32Fs[i][j+1].X, point3D32Fs[i][j+1].Y, 0.0f,
                            point3D32Fs[i][j+3].X, point3D32Fs[i][j+3].Y, 0.0f, // triangle 1 : end
                            point3D32Fs[i][j+3].X, point3D32Fs[i][j+3].Y, 0.0f, // triangle 2 : begin
                            point3D32Fs[i][j+2].X, point3D32Fs[i][j+2].Y, 0.0f,
                            point3D32Fs[i][j].X  , point3D32Fs[i][j].Y  , 0.0f};
                    board.AddRange(square_buf);
                }
                boards[i] = board.ToArray();
            }
            return boards;
        }
        public static Image<Gray, Byte> generateImage_mesh(int n, double k)
        {
            int im_side = 700;
            int side = im_side / n;
            var im_ret = new Image<Gray, Byte>(im_side, im_side);
            var pattern_s = new Size(side, side);
            int q_side = (int)(k * side);
            var quad_s = new Size(q_side, q_side);
            /*Console.WriteLine(k + "k-");
            Console.WriteLine(side + "s-");
            Console.WriteLine(q_side + "q-");*/

            var p_start = new List<Point>();
            for (int x = 0; x <= im_ret.Width - pattern_s.Width; x += pattern_s.Width)
            {
                for (int y = 0; y <= im_ret.Height - pattern_s.Height; y += pattern_s.Height)
                {
                    p_start.Add(new Point(x, y));
                }
            }
            for (int x = 0; x < im_ret.Width; x++)
            {
                for (int y = 0; y < im_ret.Height; y++)
                {
                    im_ret.Data[y, x, 0] = 255;
                }
            }

            for (int i = 0; i < p_start.Count; i++)
            {
                for (int x = p_start[i].X; x < p_start[i].X + quad_s.Width; x++)
                {
                    for (int y = p_start[i].Y; y < p_start[i].Y + quad_s.Height; y++)
                    {
                        im_ret.Data[y, x, 0] = 0;
                    }
                }
            }
            im_ret.Save("black_sq_" + n + "_" + k + ".png");
            return im_ret;
        }

        public static void showMap(int[,] map)
        {
            var im = new Image<Gray, Byte>(map.GetLength(0), map.GetLength(1));
            for (int x = 0; x < im.Width; x++)
                for (int y = 0; y < im.Height; y++)
                {
                    if(map[x, y] > 0 )
                    {
                        im.Data[y, x, 0] = 255;
                    }
                    
                }
            var k = 10;
            //CvInvoke.Resize(im,im,new Size((int)(k*im.Height), (int)(k*im.Width)));
            CvInvoke.Imshow("map", im);
        }

        public static void showInds(int[][] map)
        {
            var im = new Image<Gray, Byte>(1000,1000);



            for (int i = 0; i < map.Length; i++)
            { 

                im.Data[map[i][1], map[i][0], 0] = 255;
            }
            var k = 10;
            //CvInvoke.Resize(im,im,new Size((int)(k*im.Height), (int)(k*im.Width)));
            CvInvoke.Imshow("map_inds", im);
        }
        #endregion
    }
}
