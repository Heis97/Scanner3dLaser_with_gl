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
using Accord.MachineLearning.VectorMachines.Learning;
using PathPlanning;

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

        static public Point3d_GL[] regress3DLine_ax(Point3d_GL[] points,  int grad = 2)
        {
            List<Point3d_GL> ret = new List<Point3d_GL>();
            points = (from p in points
                      orderby p.x
                      select p).ToArray();
            var xval = new double[points.Length][];
            var yval = new double[points.Length][];
            var zval = new double[points.Length][];
            //var tval = new double[points.Length];
            double t = 0;
            var vec_main = new Vector3d_GL(points[0], points[points.Length-1]);
            //Console.WriteLine("_________________");
            for (int i = 0; i < points.Length; i++)
            {
                if (i > 0)
                {
                    var alph = Vector3d_GL.cos( new Vector3d_GL(points[i-1], points[i]) , vec_main);
                    var dist = (points[i] - points[i - 1]).magnitude();
                    var dt1 = dist;//*alph
                    t += dt1;
                    
                    //Console.WriteLine("t " + t +"; dt1 "+dt1+"; dist "+dist+"; alph " + alph);
                }                
                xval[i] = new double[] { t, points[i].x };
                yval[i] = new double[] { t, points[i].y };
                zval[i] = new double[] { t, points[i].z };                
            }

            var xkoef = regression(xval, grad);
            var ykoef = regression(yval, grad);
            var zkoef = regression(zval, grad);
            var dt = t/points.Length;

            for (double ti = 0; ti < t; ti += dt)
            {
                ret.Add(new Point3d_GL(calcPolynSolv(xkoef, ti), calcPolynSolv(ykoef, ti), calcPolynSolv(zkoef, ti)));
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
        static public Mat paintRegression(Mat mat, double[][] values, int degree)
        {
            var koef = regression(values, degree);
            var im = mat.ToImage<Bgr, Byte>();
            var a = koef[2];
            var b = koef[1];
            var x_cent =(int)(-b / (2 * a));
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
                if(x==x_cent)
                {
                    for (int y1 = 0; y1 < im.Height; y1++)
                    {
                        im.Data[y1, x, 2] = 255;
                    }
                }
            }

            return im.Mat;
        }
        static public Mat paintRegression(double[][] values_x, double[][] values_y, double[][] values_z)
        {
            var im = new Image<Bgr, Byte>(1000,1000);

            for (int i = 1; i < values_x.Length; i++)
            {
                
                var x = (uint)values_x[i][0]; if (x > 999) x = 999;
                var y = (uint)values_x[i][1]; if (y > 999) y = 999;
                im.Data[y, x, 0] = 255;

                x = (uint)values_y[i][0]; if (x > 999) x = 999;
                y = (uint)values_y[i][1]; if (y > 999) y = 999;
                im.Data[y, x, 1] = 255;

                x = (uint)values_z[i][0]; if (x > 999) x = 999;
                y = (uint)values_z[i][1]; if (y > 999) y = 999;
                im.Data[y, x, 2] = 255;

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
        static public CubicSpline spline(double[][] data)
        {
            double[] inputs = data.GetColumn(0);
            double[] outputs = data.GetColumn(1);
            var spline = new CubicSpline();
            spline.BuildSpline(inputs, outputs, inputs.Length);
            return spline;
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

        static public Point3d_GL[] spline3DLine(Point3d_GL[] points,double d)
        {
            List<Point3d_GL> ret = new List<Point3d_GL>();
            //points = Point3d_GL.order_points(points);
            points = PathPlanner.filter_traj(points.ToList(), d).ToArray();
           /* points = (from p in points
                      orderby p.x
                      select p).ToArray();*/
            var xval = new double[points.Length][];
            var yval = new double[points.Length][];
            var zval = new double[points.Length][];
            //var tval = new double[points.Length];
            double t = 0;
            var vec_main = new Vector3d_GL(points[0], points[points.Length - 1]);
            //Console.WriteLine("_________________");
            for (int i = 0; i < points.Length; i++)
            {
                if (i > 0)
                {
                    var alph = Vector3d_GL.cos(new Vector3d_GL(points[i - 1], points[i]), vec_main);
                    var dist = (points[i] - points[i - 1]).magnitude();
                    var dt1 = dist;//*alph
                    t += dt1;

                    //Console.WriteLine("t " + t +"; dt1 "+dt1+"; dist "+dist+"; alph " + alph);
                }
                xval[i] = new double[] { t, points[i].x };
                yval[i] = new double[] { t, points[i].y };
                zval[i] = new double[] { t, points[i].z };
            }

            var xkoef = spline(xval);
            var ykoef = spline(yval);
            var zkoef = spline(zval);
            var dt = t / points.Length;

            for (double ti = 0; ti < t; ti += dt/10)
            {
                ret.Add(new Point3d_GL(xkoef.Interpolate( ti), ykoef.Interpolate(ti),zkoef.Interpolate(ti)));
            }
            return ret.ToArray();
        }


    }
}
