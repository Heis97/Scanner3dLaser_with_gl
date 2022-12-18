using Accord.Math;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    static public class prin
    {
        public static void t(Image<Gray, float> matr)
        {
            var flarr = matr.Data;
            var ch = matr.NumberOfChannels;
            for (int i = 0; i < matr.Rows; i++)
            {
                for (int j = 0; j < matr.Cols; j++)
                {
                    for (int k = 0; k < ch; k++)
                    {
                        Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                    }
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(Image<Bgr, float> matr)
        {
            var flarr = matr.Data;
            var ch = 3;
            for (int i = 0; i < matr.Rows; i++)
            {
                for (int j = 0; j < matr.Cols; j++)
                {
                    for (int k = 0; k < ch; k++)
                    {
                        Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                    }
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(VectorOfVectorOfPoint3D32F matr)
        {
            for (int i = 0; i < matr.Size; i++)
            {
                for (int y = 0; y < matr[i].Size; y++)
                {
                    Console.Write(matr[i][y].X + " " + matr[i][y].Y + " " + matr[i][y].Z + "; ");
                }
                Console.WriteLine("; ");
            }
            Console.WriteLine(" ");
        }
        public static void t(VectorOfPoint3D32F matr)
        {
            for (int i = 0; i < matr.Size; i++)
            {               
                Console.WriteLine(matr[i].X + " " + matr[i].Y + " " + matr[i].Z + "; ");
            }
            Console.WriteLine(" ");
        }

        public static void t(VectorOfKeyPoint matr)
        {
            for (int i = 0; i < matr.Size; i++)
            {
                Console.WriteLine(matr[i].Point.X + " " + matr[i].Point.Y + "; ");
            }
            Console.WriteLine(" ");
        }
        public static void t(VectorOfVectorOfPointF matr)
        {
            for (int i = 0; i < matr.Size; i++)
            {
                for (int y = 0; y < matr[i].Size; y++)
                {
                    Console.Write(matr[i][y].X + " " + matr[i][y].Y + "; ");
                }
                Console.WriteLine("; ");
            }
            Console.WriteLine(" ");
        }
        public static void t(double[] matr)
        {
            for (int i = 0; i < matr.Length; i++)
            {
                Console.Write(matr[i] + " ");
            }
            Console.WriteLine(" ");
        }

        public static void t(float[] matr)
        {
            for (int i = 0; i < matr.Length; i++)
            {
                Console.Write(matr[i] + " ");
                if((i+1)%3==0)
                {
                    Console.WriteLine("; ");
                }
            }
            Console.WriteLine(" ");
        }
        public static void t(Frame frame, int cols = 3)
        {
            var name = frame.name;
            var name_t = name.Trim();
            var name_s = name_t.Split(' ');
            for (int i = 0; i < name_s.Length / cols; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Console.Write(name_s[i * cols + j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(double[,] matr)
        {
            Console.WriteLine(matr.GetLength(0) + " "+ matr.GetLength(1));
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                for (int j = 0; j < matr.GetLength(1); j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(float[,,] matr)
        {
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                for (int j = 0; j < matr.GetLength(1); j++)
                {
                    for (int k = 0; k < matr.GetLength(2); k++)
                    {
                        Console.Write(matr[i, j, k] + " ");
                    }
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(float[,] matr)
        {
            for (int i = 0; i < matr.GetLength(0); i++)
            {
                for (int j = 0; j < matr.GetLength(1); j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(float[][] matr)
        {
            for (int i = 0; i < matr.Length; i++)
            {
                for (int j = 0; j < matr[i].Length; j++)
                {
                    Console.Write(matr[i][ j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(Point3d_GL[][] matr)
        {
            for (int i = 0; i < matr.GetColumn(0).Length; i++)
            {
                for (int j = 0; j < matr.GetRow(0).Length; j++)
                {
                    Console.Write(matr[i][j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(MCvPoint3D32f[][] matr)
        {
            for (int i = 0; i < matr.GetColumn(0).Length; i++)
            {
                for (int j = 0; j < matr.GetRow(0).Length; j++)
                {
                    Console.Write(matr[i][j].X + " " + matr[i][j].Y + " " + matr[i][j].Z + "; ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(System.Drawing.PointF[][] matr)
        {
            for (int i = 0; i < matr.GetColumn(0).Length; i++)
            {
                for (int j = 0; j < matr.GetRow(0).Length; j++)
                {
                    Console.Write(matr[i][j].X + " " + matr[i][j].Y + "; ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(System.Drawing.PointF[] matr)
        {
            if(matr == null)
            {
                return;
            }
            for (int i = 0; i < matr.Length; i++)
            {
               Console.Write(matr[i].X + " " + matr[i].Y + "; ");               
               Console.WriteLine(" ");
            }
        }
        public static void t(PointF[] matr)
        {
            if (matr == null)
            {
                return;
            }
            for (int i = 0; i < matr.Length; i++)
            {
                Console.Write(matr[i].X + " " + matr[i].Y + "; ");
                Console.WriteLine(" ");
            }
        }
        public static void t(Point3d_GL[] matr)
        {
            if (matr == null)
            {
                return;
            }
            for (int i = 0; i < matr.Length; i++)
            {
                Console.WriteLine(matr[i].x + " " + matr[i].y + " " + matr[i].z+ ";");

            }
        }
        public static void t(Vertex4f[] matr)
        {
            if (matr == null)
            {
                return;
            }
            for (int i = 0; i < matr.Length; i++)
            {
                Console.WriteLine(matr[i].x + " " + matr[i].y + " " + matr[i].z + ";");

            }
        }

        public static void t(Flat3d_GL[] matr)
        {
            if (matr == null)
            {
                return;
            }
            for (int i = 0; i < matr.Length; i++)
            {
                Console.WriteLine(matr[i].ToString() + ";");
            }
        }
        public static void t(Line3d_GL[] matr)
        {
            if (matr == null)
            {
                return;
            }
            for (int i = 0; i < matr.Length; i++)
            {
                Console.WriteLine(matr[i].ToString() + ";");
            }
        }
        public static void t(Matrix<double> matr)
        {
            Console.WriteLine("cols x rows: " + matr.Rows + " x " + matr.Cols);
            if (matr.Cols != 1)
            {
                for (int i = 0; i < matr.Rows; i++)
                {
                    for (int j = 0; j < matr.Cols; j++)
                    {
                        Console.Write(Math.Round(matr[i, j],10) + " ");
                    }
                    Console.WriteLine(" ");
                }
            }
            else
            {
                for (int i = 0; i < matr.Rows; i++)
                {
                    Console.Write(Math.Round(matr[i, 0],10) + " ");
                }
                Console.WriteLine(" ");
            }

        }
        public static void t(Matrix<byte> matr)
        {
            Console.WriteLine("cols x rows: " + matr.Rows + " x " + matr.Cols);
            if (matr.Cols != 1)
            {
                for (int i = 0; i < matr.Rows; i++)
                {
                    for (int j = 0; j < matr.Cols; j++)
                    {
                        if(matr.NumberOfChannels==1)
                        {
                            Console.Write(matr[i, j] + " ");
                        }
                        else
                        {
                            for(int k=0; k< matr.NumberOfChannels; k++)
                            {
                                //Console.Write(matr. + " ");
                            }
                        }
                    }
                    Console.WriteLine(" ");
                }
            }
            else
            {
                for (int i = 0; i < matr.Rows; i++)
                {
                    Console.Write(matr[i, 0] + " ");
                }
                Console.WriteLine(" ");
            }

        }
        public static void t(Matrix4x4f matr)
        {
            for (uint i = 0; i < 4; i++)
            {
                for (uint j = 0; j < 4; j++)
                {
                    Console.Write(Math.Round(matr[i, j],3) + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public static void t(Matrix3x3f matr)
        {
            for (uint i = 0; i < 3; i++)
            {
                for (uint j = 0; j < 3; j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        
        public static void t(Mat mat)
        {
            Console.WriteLine("cols x rows: " + mat.Cols + " x " + mat.Rows+" "+mat.Depth + " " + mat.NumberOfChannels);
            var arr = mat.GetData();
            var ch = mat.NumberOfChannels;

            if (ch > 1)
            {
                if (mat.Depth == DepthType.Cv64F)
                {
                    var flarr = (double[,,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            for (int k = 0; k < ch; k++)
                            {
                                Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                            }
                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv32F)
                {
                    var flarr = (float[,,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            for (int k = 0; k < ch; k++)
                            {
                                Console.Write(Math.Round(flarr[i, j, k], 3) + " ");
                            }
                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv16S)
                {
                    var flarr = (Int16[,,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            for (int k = 0; k < ch; k++)
                            {
                                Console.Write(flarr[i, j, k] + " ");
                            }
                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv8U)
                {
                    var flarr = (byte[,,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            for (int k = 0; k < ch; k++)
                            {
                                Console.Write(flarr[i, j, k] + " ");
                            }
                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
            }
            else
            {
                if (mat.Depth == DepthType.Cv64F)
                {
                    var flarr = (double[,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {

                            Console.Write(flarr[i, j] + " ");

                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv32F)
                {
                    var flarr = (float[,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            Console.Write(Math.Round(flarr[i, j], 3) + " ");

                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv16S)
                {
                    var flarr = (Int16[,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            Console.Write(flarr[i, j] + " ");

                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
                else if (mat.Depth == DepthType.Cv8U)
                {
                    var flarr = (byte[,])arr;
                    for (int i = 0; i < mat.Rows; i++)
                    {
                        for (int j = 0; j < mat.Cols; j++)
                        {
                            Console.Write(flarr[i, j] + " ");

                            Console.Write(" | ");
                        }
                        Console.WriteLine(" ");
                    }
                }
            }




        }
        public static void t(Vertex3f v)
        {
            Console.WriteLine(v.x + " " + v.y + " " + v.z);
        }
        public static void t(string s)
        {
            Console.WriteLine(s);
        }
        public static void t(int s)
        {
            Console.WriteLine(s);
        }
        public static void t(double s)
        {
            Console.WriteLine(s);
        }


    }
}
