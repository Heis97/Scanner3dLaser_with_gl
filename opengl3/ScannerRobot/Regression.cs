using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using Accord.Statistics.Models.Regression.Linear;
using Emgu.CV;
using Emgu.CV.Structure;

namespace opengl3
{
    static public class Regression
    {
        static public Point3d_GL[][] extendPoints(Point3d_GL[][] points)
        {
            Console.WriteLine(points.Length);

            for (int i = 0; i < points.Length; i++)
            {
                Console.WriteLine(points[i].Length);
                points[i] = regress3DLine(points[i], 2);

            }
            return points;
        }
        static public Point3d_GL[][] translPoints(Point3d_GL[][] points)
        {
            Console.WriteLine("------------");
            for (int i = 0; i < points.Length; i++)
            {
                Console.WriteLine(points[i].Length);
            }
            Point3d_GL[][] points_tr = new Point3d_GL[points[0].Length][];

            for (int i = 0; i < points[0].Length; i++)
            {
                points_tr[i] = new Point3d_GL[points.Length];
                //Console.WriteLine(i);
                for (int j = 0; j < points.Length; j++)
                {
                    points_tr[i][j] = points[j][i];
                    //Console.WriteLine(j);
                }
            }
            return points_tr;
        }
        static public Point3d_GL[] regress3DLine(Point3d_GL[] points, int p_len, int grad = 2)
        {
            List<Point3d_GL> ret = new List<Point3d_GL>();
            var xval = new double[points.Length][];
            var yval = new double[points.Length][];
            double maxZ = double.MinValue;
            double minZ = double.MaxValue;
            for (int i = 0; i < points.Length; i++)
            {
                xval[i] = new double[] { points[i].z, points[i].x };
                yval[i] = new double[] { points[i].z, points[i].y };
                if (points[i].z > maxZ)
                {
                    maxZ = points[i].z;
                }
                if (points[i].z < minZ)
                {
                    minZ = points[i].z;
                }
            }
            var xkoef = regression(xval, grad);
            var ykoef = regression(yval, grad);
            var dz = (maxZ - minZ) / (double)points.Length;

            for (double z = -p_len * dz; z < (points.Length + 0.9 * p_len) * dz; z += dz)
            {
                ret.Add(new Point3d_GL(calcPolynSolv(xkoef, z), calcPolynSolv(ykoef, z), z));
            }
            return ret.ToArray();
        }
        static public PointF[] regressionPoints(Size size, double[][] values, int delim = 80, int grad = 2, int board = 10)
        {
            var points = new List<PointF>();
            var koef = regression(values, grad);
            var stepx = size.Width / delim;

            float x = 0;
            for (int i = -board; i <= delim + board; i++)
            {
                double y = calcPolynSolv(koef, x);
                //if (y < size.Height - 1)
                // {
                points.Add(new PointF((float)x, (float)y));
                //}
                if (i == delim - 1)
                {
                    stepx--;
                }
                x += stepx;
            }
            return points.ToArray();
        }
        static public Mat paintRegression(Mat mat, double[][] values)
        {
            var koef = regression(values, 4);
            var im = mat.ToImage<Bgr, Byte>();
            for (int x = 0; x < im.Width; x++)
            {
                int y = (int)calcPolynSolv(koef, x);
                if (y >= im.Height)
                {
                    y = im.Height - 1;
                }
                if (y < 0)
                {
                    y = 0;
                }
                //Console.WriteLine(y + " " + im.Height);
                im.Data[y, x, 1] = 255;
            }
            return im.Mat;
        }
        static public double calcPolynSolv(double[] k, double x)
        {
            double solv = 0;

            for (int i = 0; i < k.Length; i++)
            {
                solv += k[i] * Math.Pow(x, i);
            }
            return solv;
        }
        static public double[] regression(double[][] data, int degree)
        {
            double[] inputs = data.GetColumn(0);  // X
            double[] outputs = data.GetColumn(1); // Y
            var ls = new PolynomialLeastSquares()
            {
                Degree = degree
            };
            PolynomialRegression poly = ls.Learn(inputs, outputs);
            double[] weights = poly.Weights;
            double intercept = poly.Intercept;
            var koef = new List<double>();
            koef.AddRange(weights);
            koef.Add(intercept);
            koef.Reverse();
            return koef.ToArray();
        }

        static public double[] regression_div(double[] inputs, double[] outputs, int degree)
        {
            var ls = new PolynomialLeastSquares()
            {
                Degree = degree
            };
            PolynomialRegression poly = ls.Learn(inputs, outputs);
            double[] weights = poly.Weights;
            double intercept = poly.Intercept;
            var koef = new List<double>();
            koef.AddRange(weights);
            koef.Add(intercept);
            koef.Reverse();
            return koef.ToArray();
        }
    }
}
