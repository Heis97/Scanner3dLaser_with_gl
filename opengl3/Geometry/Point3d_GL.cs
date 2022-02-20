using Emgu.CV;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public struct Point3d_GL
    {
        public double x;
        public double y;
        public double z;
        public bool exist;
        public Point3d_GL(double _x = 0, double _y = 0, double _z = 0)
        {
            x = _x;
            y = _y;
            z = _z;
            exist = true;
        }

        public Point3d_GL(Point p, double _z)
        {
            x = p.X;
            y = p.Y;
            z = _z;
            exist = true;
        }
        public Point3d_GL(PointF p, double _z)
        {
            x = p.X;
            y = p.Y;
            z = _z;
            exist = true;
        }
        public Point3d_GL(Point3d_GL p1, Point3d_GL p2)
        {
            x = p1.x + (p2.x - p1.x) / 2;
            y = p1.y + (p2.y - p1.y) / 2;
            z = p1.z + (p2.z - p1.z) / 2;
            exist = true;
        }
        public Point3d_GL(double[,] cor)
        {
            x = cor[0, 0];
            y = cor[1, 0];
            z = cor[2, 0];
            exist = true;
        }
        public Point3d_GL normalize()
        {
            var norm = Math.Sqrt(x * x + y * y + z * z);
            if (norm != 0)
            {
                x /= norm;
                y /= norm;
                z /= norm;
            }
            else
            {
                x = 0;
                y = 0;
                z = 0;
            }

            return new Point3d_GL(x, y, z);
        }
        public double magnitude()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }
        public static Point3d_GL notExistP()
        {
            var p = new Point3d_GL();
            p.exist = false;
            return p;
        }
        public double[,] ToDouble()
        {
            return new double[,] { { x }, { y }, { z }, { 1 } };
        }
        public static Point3d_GL operator *(Point3d_GL p, double k)
        {
            return new Point3d_GL(p.x * k, p.y * k, p.z * k);
        }
        public static Point3d_GL operator *(double[,] matrixA, Point3d_GL p)
        {
            double[,] matrixB = new double[1, 1];
            if (matrixA.GetLength(0) == 4)
            {
                matrixB = new double[,] { { p.x }, { p.y }, { p.z }, { 1 } };
            }
            else if (matrixA.GetLength(0) == 3)
            {
                matrixB = new double[,] { { p.x }, { p.y }, { p.z } };
            }
            else
            {
                return Point3d_GL.notExistP();
            }

            if (matrixA.GetLength(1) != matrixB.GetLength(0))
            {
                return Point3d_GL.notExistP();
            }
            var matrixC = new double[matrixA.GetLength(0), matrixB.GetLength(1)];
            for (var i = 0; i < matrixA.GetLength(0); i++)
            {
                for (var j = 0; j < matrixB.GetLength(1); j++)
                {
                    matrixC[i, j] = 0;
                    for (var k = 0; k < matrixB.GetLength(0); k++)
                    {
                        matrixC[i, j] += matrixA[i, k] * matrixB[k, j];
                    }
                }
            }
            return new Point3d_GL(matrixC);
        }
        static double[,] Matrix4x4ToDouble(Matrix4x4f matrixA)
        {
            var ret = new double[4, 4];
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    ret[i, j] = (double)matrixA[(uint)i, (uint)j];

                }
            }
            return ret;
        }

        static double[,] Matrix4x4ToDouble(Matrix<double> matrixA)
        {

            var ret = new double[matrixA.Cols, matrixA.Rows];
            for (var i = 0; i < 4; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    ret[i, j] = (double)matrixA[i, j];

                }
            }
            return ret;
        }


        public static Point3d_GL operator *(Matrix4x4f matrixA, Point3d_GL p)
        {
            var matrix = Matrix4x4ToDouble(matrixA);
            return matrix * p;
        }

        public static Point3d_GL operator *(Matrix<double> matrixA, Point3d_GL p)
        {
            var matrix = Matrix4x4ToDouble(matrixA);
            return matrix * p;
        }

        public static Point3d_GL operator +(Point3d_GL p, Vector3d_GL v1)
        {
            return new Point3d_GL(p.x + v1.x, p.y + v1.y, p.z + v1.z);
        }
        public static Point3d_GL operator +(Point3d_GL p1, Point3d_GL p2)
        {
            return new Point3d_GL(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
        }

        public static Point3d_GL operator -(Point3d_GL p1, Point3d_GL p2)
        {
            return new Point3d_GL(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }
        public override string ToString()
        {

            return Math.Round(x, 4).ToString() + " " + Math.Round(y, 4).ToString() + " " + Math.Round(z, 4).ToString();
        }
    }

}
