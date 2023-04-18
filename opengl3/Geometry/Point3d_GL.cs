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
        public Colo3d_GL color;
        public Point3d_GL(double _x = 0, double _y = 0, double _z = 0, Colo3d_GL _color = null)
        {
            x = _x;
            y = _y;
            z = _z;
            exist = true;
            color = _color;
        }
        public Point3d_GL(Vertex4f vertex, Colo3d_GL _color = null)
        {
            x = vertex.x;
            y = vertex.y;
            z = vertex.z;
            exist = true;
            color = _color;
        }
        public Point3d_GL(Point p, double _z)
        {
            x = p.X;
            y = p.Y;
            z = _z;
            exist = true;
            color = null;
        }
        public Point3d_GL(PointF p, double _z)
        {
            x = p.X;
            y = p.Y;
            z = _z;
            exist = true;
            color = null;
        }
        public Point3d_GL(Point3d_GL p1, Point3d_GL p2)
        {
            x = p1.x + (p2.x - p1.x) / 2;
            y = p1.y + (p2.y - p1.y) / 2;
            z = p1.z + (p2.z - p1.z) / 2;
            exist = true;
            color = p1.color;
        }


        public Point3d_GL Copy()
        {
            return new Point3d_GL(x, y, z, color);
        }
        public Point3d_GL(double[,] cor, Colo3d_GL _color = null)
        {
            x = cor[0, 0];
            y = cor[1, 0];
            z = cor[2, 0];
            exist = true;
            color = _color;
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

            return new Point3d_GL(x, y, z,color);
        }
        public static float[] toData(Point3d_GL[] ps)
        {
            var data = new float[ps.Length * 4];
            for (int i = 0; i < ps.Length; i++)
            {
                data[4 * i] = (float)ps[i].x;
                data[4 * i + 1] = (float)ps[i].y;
                data[4 * i + 2] = (float)ps[i].z;
            }
            return data;
        }

        public static float[] toData(Point3d_GL[][] ps)
        {
            var data = new List<float>();
            for (int i = 0; i < ps.Length; i++)
            {
                data.AddRange(toData(ps[i]));
            }
            return data.ToArray();
        }

        public static Point3d_GL[] dataToPoints(float[] data)
        {
            var ps = new Point3d_GL[data.Length / 4];
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i].x = data[4 * i];
                ps[i].y = data[4 * i + 1];
                ps[i].z = data[4 * i + 2];
            }
            return ps;
        }
        public static Point3d_GL[][] dataToPoints2d(float[][] data)
        {

            var ps = new List< Point3d_GL[]>();
            for (int i = 0; i < data.Length; i++)
            {
                ps.Add(dataToPoints_ex(data[i]));
            }
            return ps.ToArray();
        }

        public static float[][] divide_data(float[] data, int w)
        {
            w *= 4;
            int h = data.Length / w;
            float[][] result = new float[h][];
            for (int i = 0; i < h; i++)
            {
                result[i] = new float[w];
                for (int j = 0; j < w; j++)
                {
                    result[i][j] = data[w*i + j];
                }
            }
           
            return result;
        }


        public static Point3d_GL[] dataToPoints_ex(float[] data)
        {
            var ps = new List<Point3d_GL>();
            for (int i = 0; i < data.Length/4; i++)
            {
                if(data[4 * i + 3]>0)
                {
                    ps.Add(new Point3d_GL(data[4 * i], data[4 * i + 1], data[4 * i + 2]));
                }
                else
                {
                    ps.Add(notExistP());
                }
            }
            return ps.ToArray();
        }

        public static Point3d_GL[][] colorPoints2d(Point3d_GL[][] ps_color, Point3d_GL[][] ps)
        {           
            for (int i = 0; i < ps_color.Length; i++)
            {
                for (int j = 0; j < ps_color[i].Length; j++)
                {
                    ps[i][j].color = ps_color[i][j].color;
                }
            }
            return ps;
        }

        public static Point3d_GL[][] filtrExistPoints2d(Point3d_GL[][] ps)
        {
            var ps_fil = new List<Point3d_GL[]>();
            for (int i = 0; i < ps.Length; i++)
            {
                var list = new List<Point3d_GL>();
                for (int j = 0; j < ps[0].Length; j++)
                {
                    if(ps[i][j].exist) list.Add(ps[i][j]);
                }

                ps_fil.Add(list.ToArray());
            }
            return ps_fil.ToArray();
        }
        public static Point3d_GL[] unifPoints2d(Point3d_GL[][] ps)
        {
            var ps_fil = new List<Point3d_GL>();
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i]!=null)
                {
                    for (int j = 0; j < ps[i].Length; j++)
                    {
                        ps_fil.Add(ps[i][j]);
                    }
                }
                           
            }
            return ps_fil.ToArray();
        }
        public double magnitude()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public double magnitude_xy()
        {
            return Math.Sqrt(x * x + y * y );
        }

        public double magnitude_ax(Ax ax)
        {
            if(ax==Ax.X) return Math.Abs(x);
            if (ax == Ax.Y) return Math.Abs(y);
            else return Math.Abs(z);
        }
        public static float[] toMesh(Point3d_GL[] point3Ds)
        {
            var mesh = new float[point3Ds.Length * 3];
            for(int i=0; i<point3Ds.Length;i++)
            {
                mesh[3 * i] =  (float)point3Ds[i].x;
                mesh[3 * i +1] = (float)point3Ds[i].y;
                mesh[3 * i +2] = (float)point3Ds[i].z;
            }
            return mesh;
        }

        public static float[] mesh3to4(float[] mesh3)
        {
            var mesh = new float[mesh3.Length * 4/3];
            for (int i = 0; i < mesh.Length/4; i++)
            {
                mesh[4 * i] = mesh3[3 * i];
                mesh[4 * i + 1] = mesh3[3 * i + 1];
                mesh[4 * i + 2] = mesh3[3 * i + 2];
            }
            return mesh;
        }

        public static float[] toMesh(Point3d_GL[][] point3Ds)
        {
            var mesh = new List<float>();
            for (int i = 0; i < point3Ds.Length; i++)
            {
                mesh.AddRange(toMesh(point3Ds[i]));
            }
            return mesh.ToArray();
        }
        public static Point3d_GL[] toPoints(Vertex4f[] vertexs)
        {
            var ps = new Point3d_GL[vertexs.Length];
            for(int i=0; i<vertexs.Length;i++)
            {
                ps[i] = new Point3d_GL(vertexs[i]);
            }
            return ps;
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

        public static Point3d_GL[] multMatr(Point3d_GL[] ps,Matrix<double> matrix)
        {
            var ps_ret = new Point3d_GL[ps.Length];
            for(int i=0; i<ps_ret.Length; i++)
            {
                ps_ret[i] = matrix * ps[i];
            }
            return ps_ret;
        }

        public static Point3d_GL[][] multMatr(Point3d_GL[][] ps, Matrix<double> matrix)
        {
            var ps_ret = new Point3d_GL[ps.Length][];

            for (int i = 0; i < ps_ret.Length; i++)
            {
                ps_ret[i] = multMatr(ps[i],matrix);
            }
            return ps_ret;
        }
        public static double operator *(Point3d_GL p, Vector3d_GL v1)
        {
            return p.x * v1.x + p.y * v1.y + p.z * v1.z;
        }
        public static Point3d_GL operator *(Point3d_GL p, double k)
        {
            return new Point3d_GL(p.x * k, p.y * k, p.z * k,p.color);
        }

        public static Point3d_GL operator /(Point3d_GL p, double k)
        {
            return new Point3d_GL(p.x / k, p.y / k, p.z / k,p.color);
        }

        public static Point3d_GL operator *(Point3d_GL p1, Point3d_GL p2)
        {
            return new Point3d_GL(p1.x * p2.x, p1.y * p2.y, p1.z * p2.z,p1.color);
        }
        public static Point3d_GL operator *(Point3d_GL p, Flat3d_GL f)
        {
            return new Point3d_GL(p.x * f.A, p.y * f.B, p.z * f.C,p.color);
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
            return new Point3d_GL(matrixC,p.color);
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
           // prin.t(matrixA);
            var ret = new double[matrixA.Cols, matrixA.Rows];
            for (var i = 0; i < ret.GetLength(0); i++)
            {
                for (var j = 0; j < ret.GetLength(1); j++)
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
            return new Point3d_GL(p.x + v1.x, p.y + v1.y, p.z + v1.z,p.color);
        }
        public static Point3d_GL operator +(Point3d_GL p1, Point3d_GL p2)
        {
            return new Point3d_GL(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z, p1.color);
        }

        public static Point3d_GL operator -(Point3d_GL p1, Point3d_GL p2)
        {
            return new Point3d_GL(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z, p1.color);
        }

        public static Point3d_GL operator -(Point3d_GL p)
        {
            return new Point3d_GL(-p.x, -p.y, -p.z,p.color);
        }

        public Point3d_GL Clone()
        {
            return new Point3d_GL(x, y, z, color);
        }
        public override string ToString()
        {

            return Math.Round(x, 4).ToString() + " " + Math.Round(y, 4).ToString() + " " + Math.Round(z, 4).ToString();
        }

        public static Point3d_GL[] order_points(Point3d_GL[] ps)
        {
            /*ps = (from p in ps
                      orderby p.z
                      select p).ToArray();*/
            if (ps == null) return null;
            var inds_rem = remote_element(ps);

            
            var ps_or = new List<Point3d_GL>();
            ps_or.Add(ps[inds_rem[0]]);
            ps = remove_element(ps, inds_rem[0]);

            for (int j = 0; j < ps_or.Count && ps.Length > 0; j++)
            {
                int i_min = 0;
                double min = double.MaxValue;
                for (int i = 0; i < ps.Length; i++)
                {

                    var d = (ps_or[j] - ps[i]).magnitude();
                    if (d < min)
                    {
                        min = d;
                        i_min = i;
                    }
                }                   
                ps_or.Add(ps[i_min]);
                ps = remove_element(ps, i_min);
                //Console.WriteLine(ps.Length);
            }

            return ps_or.ToArray();
        }
        public static int[] remote_element(Point3d_GL[] ps)
        {
            var max = double.MinValue;
            var min = double.MaxValue;
            int ind1 = 0;
            int ind2 = 0;
            for (int i = 0; i < ps.Length; i++)
            {
                for (int j = i + 1; j < ps.Length; j++)
                {
                    var dist = (ps[i]- ps[j]).magnitude();
                    if (dist > max)
                    {
                        max = dist;
                        ind1 = i;
                        ind2 = j;
                    }
                    if (dist < min)
                    {
                        min = dist;
                        //  Console.WriteLine("min: "+min);
                    }
                }
            }
            return new int[] { ind1, ind2, (int)min };
        }
        public static Point3d_GL[] remove_element(Point3d_GL[] ps,int ind)
        {
            var ps_l = new List<Point3d_GL>();
            for (int i = 0; i < ps.Length; i++)
            {
                if(i!=ind) ps_l.Add(ps[i]);
            }

            return ps_l.ToArray();
        }

        public static Point3d_GL centr_mass(Point3d_GL[] ps)
        {
            var cm = new Point3d_GL(0,0,0);
            foreach(var p in ps)
            {
                cm += p;
            }
            return cm/ps.Length;
        }

        public static double calc_curve_cm_2(Point3d_GL[] ps)
        {
            var cm = centr_mass(ps);
            var v1 = new Vector3d_GL(cm , ps[0]);
            var v2 = new Vector3d_GL(cm, ps[ps.Length-1]);
            var cos = Vector3d_GL.cos(v1,v2);
            return Math.Abs( cos);
        }
        public static double calc_curve_cm(Point3d_GL[] ps)
        {
            var cm = centr_mass(ps);
            var r = (ps[ps.Length / 2] - cm).magnitude();
            return r;
        }


    }

}
