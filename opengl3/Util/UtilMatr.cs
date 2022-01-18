using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    static public class UtilMatr
    {
        static public Matrix4x4f matrixGLFromCam(CameraCV cam)
        {
            var rotateMatrix = new Matrix<double>(3, 3);
            CvInvoke.Rodrigues(cam.cur_r, rotateMatrix);
            var tvec = toVertex3f(cam.cur_t);
            var mx = assemblMatrix_Near(rotateMatrix, tvec);
            return mx;
            
        }


        static float PI = (float)Math.PI;
        static public float toRad(float degrees)
        {
            return degrees * PI / 180;
        }

        static public float toDegrees(float rad)
        {
            return rad * 180 / PI;
        }
        static public Matrix4x4f assemblMatrix(Matrix<double> rot, Vertex3f trans)
        {
            var ret = new Matrix4x4f();
            for (int i = 0; i < rot.Cols; i++)
            {
                for (int j = 0; j < rot.Rows; j++)
                {
                    ret[(uint)i, (uint)j] = (float)rot[i, j];
                }
            }
            //ret[3, 0] = trans.x; ret[3, 1] = trans.y; ret[3, 2] = trans.z;
           // ret[0, 3] = 0; ret[1, 3] = 0; ret[2, 3] = 0; ret[3, 3] = (float)1;

            ret[3, 0] = 0; ret[3, 1] = 0; ret[3, 2] = 0;
            ret[0, 3] = trans.x; ret[1, 3] = trans.y; ret[2, 3] = trans.z; ret[3, 3] = (float)1;
            return ret;
        }

        static public Matrix4x4f assemblMatrix_Near(Matrix<double> rot, Vertex3f trans)
        {
            var ret = new Matrix4x4f();
            for (int i = 0; i < rot.Cols; i++)
            {
                for (int j = 0; j < rot.Rows; j++)
                {
                    ret[(uint)i, (uint)j] = (float)rot[i, j];
                }
            }
            ret[3, 0] = trans.x; ret[3, 1] = trans.y; ret[3, 2] = trans.z;
             ret[0, 3] = 0; ret[1, 3] = 0; ret[2, 3] = 0; ret[3, 3] = (float)1;

            //ret[3, 0] = 0; ret[3, 1] = 0; ret[3, 2] = 0;
           // ret[0, 3] = trans.x; ret[1, 3] = trans.y; ret[2, 3] = trans.z; ret[3, 3] = (float)1;
            return ret;
        }

        static public Vertex3f toVertex3f(Mat mat)
        {
            var arr = mat.GetData();
            var v = (double[,])arr;
            return new Vertex3f((float)v[0, 0], (float)v[1, 0], (float)v[2, 0]);
        }
        static public System.Drawing.PointF[] toPointF(Mat corn)
        {

            var arr = corn.GetData();
            var flarr = (float[,,])arr;
            var points = new System.Drawing.PointF[flarr.Length / 2];
            //  Console.WriteLine(flarr.Length);
            for (int i = 0; i < flarr.Length / 2; i++)
            {
                points[i] = new System.Drawing.PointF(flarr[i, 0, 0], flarr[i, 0, 1]);
                // Console.WriteLine(flarr[i, 0, 0] + " " + flarr[i, 0, 1]);
            }
            return points;
        }
        static public System.Drawing.PointF[] toPointF(VectorOfPointF corn)
        {
            var points = new System.Drawing.PointF[corn.Size];
            //  Console.WriteLine(flarr.Length);
            for (int i = 0; i < corn.Size; i++)
            {
                points[i] = new System.Drawing.PointF(corn[i].X, corn[i].Y);
                // Console.WriteLine(flarr[i, 0, 0] + " " + flarr[i, 0, 1]);
            }
            return points;
        }
        static public double[,] convToDouble(float[,] f_mass)
        {
            var d_mass = new double[f_mass.GetLength(0), f_mass.GetLength(1)];
            for (int i = 0; i < f_mass.GetLength(0); i++)
            {
                for (int j = 0; j < f_mass.GetLength(1); j++)
                {
                    d_mass[i, j] = (double)f_mass[i, j];
                }
            }
            return d_mass;
        }
        static public Point3d_GL[] transfPoints(Point3d_GL[] points, double[,] matr)
        {
            Point3d_GL[] points_trans = new Point3d_GL[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points_trans[i] = new Point3d_GL(MatrixMultiplication(matr, points[i].ToDouble()));
            }
            return points_trans;
        }
        static public double cos(double alpha)
        {
            return Math.Cos(alpha);
        }
        static public double sin(double alpha)
        {
            return Math.Sin(alpha);
        }
        static public double[,] RotZmatr(double alpha)
        {
            return new double[,] {
                { cos(alpha), -sin(alpha), 0,0 },
                { sin(alpha), cos(alpha), 0, 0 },
                { 0, 0, 1, 0 },
                { 0, 0, 0, 1 } };
        }
        static public double[,] MatrixMultiplication(double[,] matrixA, Point3d_GL p)
        {
            double[,] matB = new double[,] { { p.x }, { p.y }, { p.z }, { 1 } };
            return MatrixMultiplication(matrixA, matB);
        }
        static public double[,] MatrixMultiplication(double[,] matrixA, double[,] matrixB)
        {
            if (matrixA.GetLength(1) != matrixB.GetLength(0))
            {
                return null;
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
            return matrixC;
        }
        /// <summary>
        /// Matrix basis1->basis2
        /// </summary>
        /// <param name="basis1"></param>
        /// <param name="basis2"></param>
        /// <returns></returns>
        static public double[,] calcTransformMatr(double[,] basis1, double[,] basis2)
        {
            if (basis1 == null || basis2 == null)
            {
                return null;
            }
            if (basis1.GetLength(0) < 2 || basis2.GetLength(0) < 2)
            {
                return null;
            }

            int n = basis1.GetLength(1);
            prin.t(n);
            var trans_Matr = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                var matrG = new double[n, n];
                var colG = new double[n];
                for (int x = 0; x < n; x++)
                {
                    for (int y = 0; y < n; y++)
                    {
                        matrG[x, y] = basis1[x, y];
                    }
                    colG[x] = basis2[x, i];
                }
                var row_GS = new CalcGauss(matrG, colG);
                var row = row_GS.getAnswer();
                for (int x = 0; x < n; x++)
                {
                    trans_Matr[i, x] = row[x];
                }
            }
            return trans_Matr;
        }
        static public double[,] calcTransformMatr(Point3d_GL[] basis1, Point3d_GL[] basis2)
        {
            if (basis1.Length != basis2.Length)
            {
                return null;
            }
            var basis1d = new double[basis1.Length, 4];
            var basis2d = new double[basis2.Length, 4];
            for (int i = 0; i < basis1.Length; i++)
            {
                basis1d[i, 0] = basis1[i].x; basis1d[i, 1] = basis1[i].y; basis1d[i, 2] = basis1[i].z; basis1d[i, 3] = 1;
                basis2d[i, 0] = basis2[i].x; basis2d[i, 1] = basis2[i].y; basis2d[i, 2] = basis2[i].z; basis2d[i, 3] = 1;
            }
            return calcTransformMatr(basis1d, basis2d);
        }
        static public Mat doubleToMat(double[][] inp, Size size)
        {
            Image<Gray, Byte> im_gray = new Image<Gray, byte>(size);
            for (int x = 0; x < inp.Length; x++)
            {
                //Console.WriteLine("n = "+inp[x][0] + "od = " + inp[x][1]);
                int y = (int)inp[x][1];
                if (y < 0)
                {
                    y = 0;
                }
                if (y >= size.Height)
                {
                    y = size.Height - 1;
                }
                im_gray.Data[y, x, 0] = 255;
            }
            return im_gray.Mat;
        }
        static public PointF[] doubleToPointF(double[][] inp)
        {
            var points = new List<PointF>();
            for (int i = 0; i < inp.Length; i++)
            {
                points.Add(new PointF((float)inp[i][0], (float)inp[i][1]));
            }
            return points.ToArray();
        }

        static public PointF[] doubleToPointF_real(double[][] inp)
        {
            var points = new List<PointF>();
            for (int i = 0; i < inp.Length; i++)
            {
                if (inp[i][2] > 0)
                {
                    points.Add(new PointF((float)inp[i][0], (float)inp[i][1]));
                }

            }
            return points.ToArray();
        }
        static public Matrix4x4f matrFromCam(Camera cam)
        {
            var Orient = new Vector3d_GL[] { cam.oX, cam.oY, cam.oZ };
            var retMatr = new Matrix4x4f();
            for (uint i = 0; i < 3; i++)
            {
                retMatr[i, 0] = (float)Orient[i].x;
                retMatr[i, 1] = (float)Orient[i].y;
                retMatr[i, 2] = (float)Orient[i].z;
            }
            retMatr[3, 0] = (float)cam.pos.x;
            retMatr[3, 1] = (float)cam.pos.y;
            retMatr[3, 2] = (float)cam.pos.z;
            return retMatr;
        }

        static public Camera calcPos(PointF[] points, Size size, double fov, double side)
        {
            var Camera1 = new Camera(fov, size);
            var points1 = moveToCentr(points, size);
            var cl_P1 = new Point3d_GL(0, 0, 0);
            var cl_P2 = new Point3d_GL(side, 0, 0);
            var cl_P3 = new Point3d_GL(side, side, 0);
            var cl_P4 = new Point3d_GL(0, side, 0);
            var basis1 = new Point3d_GL[] { cl_P1, cl_P2, cl_P3, cl_P4 };
            var basis2 = new Point3d_GL[] { new Point3d_GL(points1[0], -100), new Point3d_GL(points1[1], -200), new Point3d_GL(points1[2], -100), new Point3d_GL(points1[3], -200) };
            var transf = calcTransformMatr(basis1, basis2);
            prin.t(transf);
            Console.WriteLine("_________________^^^^^^");
            Camera1.calc_pos_all(points1[0], points1[1], points1[2], points1[3], cl_P1, cl_P2, cl_P3, cl_P4);
            /*Console.WriteLine("Ox " + Camera1.oX);
            Console.WriteLine("Oy " + Camera1.oY);
            Console.WriteLine("Oz " + Camera1.oZ);
            Console.WriteLine("Tr " + Camera1.pos);*/
            return Camera1;
        }
        static public PointF[] moveToCentr(PointF[] points, Size size)
        {
            var points1 = new PointF[points.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points1[i] = new PointF(points[i].X - size.Width / 2,
                                        points[i].Y - size.Height / 2);
                points1[i].Y *= -1;
            }
            return points1;
        }

        static public Matrix4x4f AbcToMatrix(float a, float b, float c)
        {
            //Console.WriteLine("Z");
            //print(Matrix4x4f.RotatedZ(a));
            var rotZ = Matrix4x4f.RotatedZ(a);
            rotZ[0, 1] = -rotZ[0, 1];
            rotZ[1, 0] = -rotZ[1, 0];
            // print(rotZ);
            //Console.WriteLine("Y");
            var rotY = Matrix4x4f.RotatedY(b);
            rotY[2, 0] = -rotY[2, 0];
            rotY[0, 2] = -rotY[0, 2];
            //print(rotY);
            //Console.WriteLine("X");

            var rotX = Matrix4x4f.RotatedX(c);
            rotX[2, 1] = -rotX[2, 1];
            rotX[1, 2] = -rotX[1, 2];
            //print(rotX);
            return rotX * rotY * rotZ;
        }
    }
}
