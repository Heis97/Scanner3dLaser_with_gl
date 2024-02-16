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
        public double x { get; set; }
        public double y { get; set; }
        public double z { get; set; }
        public bool exist { get; set; }
        public Color3d_GL color { get; set; }
        //public PointF uv { get; set; }
        public Point3d_GL(double _x = 0, double _y = 0, double _z = 0, Color3d_GL _color = null)
        {
            x = _x;
            y = _y;
            z = _z;
            exist = true;
            if (_color != null) color = _color;
            else color = new Color3d_GL(0.5f, 0.5f, 0.5f);
        }

        public Point3d_GL(Vertex4f vertex, Color3d_GL _color = null)
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
        public Point3d_GL(double[,] cor, Color3d_GL _color = null)
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

            return new Point3d_GL(x, y, z, color);
        }
        public static float[] toData(Point3d_GL[] ps)
        {
            var data = new float[ps.Length * 4];
            for (int i = 0; i < ps.Length; i++)
            {
                data[4 * i] = (float)ps[i].x;
                data[4 * i + 1] = (float)ps[i].y;
                data[4 * i + 2] = (float)ps[i].z;
                if (ps[i].exist) data[4 * i + 3] = 1;
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

            var ps = new List<Point3d_GL[]>();
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
                    result[i][j] = data[w * i + j];
                }
            }

            return result;
        }


        public static Point3d_GL[] dataToPoints_ex(float[] data)
        {
            var ps = new List<Point3d_GL>();
            for (int i = 0; i < data.Length / 4; i++)
            {
                if (data[4 * i + 3] > 0)
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
                    if (ps[i][j].exist) list.Add(ps[i][j]);
                }

                ps_fil.Add(list.ToArray());
            }
            return ps_fil.ToArray();
        }

        public static Point3d_GL[][] reshape(Point3d_GL[][] ps)
        {
            var ps_re = new List<List<Point3d_GL>>();
            for (int i = 0; i < ps[0].Length; i++)
            {
                ps_re.Add(new List<Point3d_GL>());
                for (int j = 0; j < ps.Length; j++)
                {
                    ps_re[ps_re.Count - 1].Add(ps[j][i]);
                }
            }
            var ps_re_ar = new List<Point3d_GL[]>();
            for (int i = 0; i < ps_re.Count; i++)
            {
                ps_re_ar.Add(ps_re[i].ToArray());
            }
            return ps_re_ar.ToArray();
        }
        public static Point3d_GL[] unifPoints2d(Point3d_GL[][] ps)
        {
            var ps_fil = new List<Point3d_GL>();
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i] != null)
                {
                    for (int j = 0; j < ps[i].Length; j++)
                    {
                        ps_fil.Add(ps[i][j]);
                    }
                }

            }
            return ps_fil.ToArray();
        }

        public static Point3d_GL[][] to_arr(List<List<Point3d_GL>> ps)
        {
            var ps_arr = new List<Point3d_GL[]>();
            for (int i = 0; i < ps.Count; i++)
            {
                ps_arr.Add(ps[i].ToArray());
            }
            return ps_arr.ToArray();
        }

        public static List<Point3d_GL> unifPoints2d(List<List<Point3d_GL>> ps)
        {
            var ps_fil = new List<Point3d_GL>();
            for (int i = 0; i < ps.Count; i++)
            {
                if (ps[i] != null)
                {
                    for (int j = 0; j < ps[i].Count; j++)
                    {
                        ps_fil.Add(ps[i][j]);
                    }
                }

            }
            return ps_fil;
        }
        public double magnitude()
        {
            return Math.Sqrt(x * x + y * y + z * z);
        }

        public double magnitude_xy()
        {
            return Math.Sqrt(x * x + y * y);
        }

        public double magnitude_ax(Ax ax)
        {
            if (ax == Ax.X) return Math.Abs(x);
            if (ax == Ax.Y) return Math.Abs(y);
            else return Math.Abs(z);
        }
        public static float[] toMesh(Point3d_GL[] point3Ds)
        {
            var mesh = new float[point3Ds.Length * 3];
            for (int i = 0; i < point3Ds.Length; i++)
            {
                mesh[3 * i] = (float)point3Ds[i].x;
                mesh[3 * i + 1] = (float)point3Ds[i].y;
                mesh[3 * i + 2] = (float)point3Ds[i].z;
            }
            return mesh;
        }
        public static Point3d_GL[] fromMesh(float[] mesh)
        {
            if (mesh == null) return null;
            var ps = new List<Point3d_GL>();
            for (int i = 0; i < mesh.Length; i += 3)
            {
                ps.Add(new Point3d_GL(mesh[i], mesh[i + 1], mesh[i + 2]));
            }
            return ps.ToArray();
        }
        public static float[] mesh3to4(float[] mesh3)
        {
            var mesh = new float[mesh3.Length * 4 / 3];
            for (int i = 0; i < mesh.Length / 4; i++)
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
            for (int i = 0; i < vertexs.Length; i++)
            {
                ps[i] = new Point3d_GL(vertexs[i]);
            }
            return ps;
        }
        public System.Drawing.Point get_syst_p()
        {
            return new System.Drawing.Point((int)x, (int)y);
        }
        public static Point3d_GL[] toPoints(System.Drawing.PointF[] ps)
        {
            var ps3d = new Point3d_GL[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ps3d[i] = new Point3d_GL(ps[i].X, ps[i].Y, 0);
            }
            return ps3d;
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

        public static Point3d_GL[] multMatr(Point3d_GL[] ps, Matrix<double> matrix)
        {
            var ps_ret = new Point3d_GL[ps.Length];
            for (int i = 0; i < ps_ret.Length; i++)
            {
                ps_ret[i] = matrix * ps[i];
            }
            return ps_ret;
        }

        public static Point3d_GL[] mult(Point3d_GL[] ps, double k)
        {
            var ps_ret = new Point3d_GL[ps.Length];
            for (int i = 0; i < ps_ret.Length; i++)
            {
                ps_ret[i] = ps[i] * k;
            }
            return ps_ret;
        }

        public static Point3d_GL[][] multMatr(Point3d_GL[][] ps, Matrix<double> matrix)
        {
            var ps_ret = new Point3d_GL[ps.Length][];

            for (int i = 0; i < ps_ret.Length; i++)
            {
                ps_ret[i] = multMatr(ps[i], matrix);
            }
            return ps_ret;
        }
        public static double operator *(Point3d_GL p, Vector3d_GL v1)
        {
            return p.x * v1.x + p.y * v1.y + p.z * v1.z;
        }
        public static Point3d_GL operator *(Point3d_GL p, double k)
        {
            return new Point3d_GL(p.x * k, p.y * k, p.z * k, p.color);
        }

        public static Point3d_GL operator /(Point3d_GL p, double k)
        {
            return new Point3d_GL(p.x / k, p.y / k, p.z / k, p.color);
        }

        public static double operator *(Point3d_GL p1, Point3d_GL p2)
        {
            return p1.x * p2.x + p1.y * p2.y + p1.z * p2.z;
        }
        public static Point3d_GL operator *(Point3d_GL p, Flat3d_GL f)
        {
            return new Point3d_GL(p.x * f.A, p.y * f.B, p.z * f.C, p.color);
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
                return notExistP();
            }

            if (matrixA.GetLength(1) != matrixB.GetLength(0))
            {
                return notExistP();
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
            var pm = new Point3d_GL(matrixC, p.color);
            pm.exist = p.exist;
            pm.color = p.color;
            return pm;
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

        public static Point3d_GL operator |(Point3d_GL p1, Point3d_GL p2)//vector multiply
        {
            return new Point3d_GL(
                  p1.y * p2.z - p1.z * p2.y,
                  p1.z * p2.x - p1.x * p2.z,
                  p1.x * p2.y - p1.y * p2.x, p1.color);
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
        public static Point3d_GL operator *(Point3d_GL p, Matrix<double> m)//хрень
        {
            return matrix_to_p(p.p_to_matrix() * m);
        }
        public Matrix<double> p_to_matrix()
        {
            return new Matrix<double>(new double[,]
            {
                { 1,0,0,x},
                { 0,1,0,y},
                { 0,0,1,z},
                { 0,0,0,1}
            });
        }
        public static Point3d_GL matrix_to_p(Matrix<double> m)
        {
            return new Point3d_GL(m[0, 3], m[1, 3], m[2, 3]);
        }
        public static Point3d_GL operator +(Point3d_GL p, Vector3d_GL v1)
        {
            return new Point3d_GL(p.x + v1.x, p.y + v1.y, p.z + v1.z, p.color);
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
            return new Point3d_GL(-p.x, -p.y, -p.z, p.color);
        }
        public static double operator ^(Point3d_GL p1, Point3d_GL p2)
        {
            return (p1*p2) / (p1.magnitude() * p2.magnitude());
        }//scalar mult
        static double arccos(double cos)
        {
            double _cos = cos;
            if (_cos >= 1) _cos = 1;
            if (_cos <= -1) _cos = -1;
            return Math.Acos(_cos);

        }
        public static double ang(Point3d_GL p1, Point3d_GL p2)
        {
            return arccos(p1 ^ p2);
        }
        public static bool one_dir(Point3d_GL p1, Point3d_GL p2)
        {
            if (ang(p1, p2) < Math.PI / 2) return true;
            else return false;
        }
        public static int sign_r_v(Point3d_GL p1, Point3d_GL p2, Point3d_GL p3)
        {
            var sign = 1;
            var t_v3 = p1 | p2;
            if (!one_dir(p3, t_v3))
                sign = -1;
            return sign;
        }

        public Point3d_GL Clone()
        {
            return new Point3d_GL(x, y, z, color);
        }
        public override string ToString()
        {
            return Math.Round(x, 4).ToString() + " " + Math.Round(y, 4).ToString() + " " + Math.Round(z, 4).ToString();
        }
        public string ToString_fool()
        {
            return "new Point3d_GL ("+x+","+y+","+z+")";
        }
        static public string ToString_fool_arr(Point3d_GL[] ps)
        {
            var sb = new StringBuilder();
            sb.Append("new Point3d_GL[]{");
            foreach (var p in ps)
            {
                sb.Append(p.ToString_fool() + ",\n");
            }
            sb.Append("};");
            return sb.ToString();
        }
        public static Point3d_GL[] order_points(Point3d_GL[] ps)
        {
            /*ps = (from p in ps
                      orderby p.z
                      select p).ToArray();*/
            if (ps == null) return null;
            if (ps.Length == 0) return null;
            var inds_rem = remote_element(ps);


            var ps_or = new List<Point3d_GL>();
            ps_or.Add(ps[inds_rem[0]]);
            ps = remove_element(ps, inds_rem[0]);

            for (int j = 0; j < ps_or.Count && ps.Length > 0; j++)
            {
                var i_min = nearest_point(ps, ps_or[ps_or.Count - 1]);
                ps_or.Add(ps[i_min]);
                ps = remove_element(ps, i_min);
            }

            return ps_or.ToArray();
        }

        public static Point3d_GL[] order_points_by_dist(Point3d_GL[] ps, double max_dist)
        {
            if (ps == null) return null;
            if (ps.Length == 0) return null;
            var inds_rem = remote_element(ps);


            var ps_or = new List<Point3d_GL>();
            ps_or.Add(ps[inds_rem[0]]);
            ps = remove_element(ps, inds_rem[0]);

            for (; ps.Length > 0;)
            {
                var i_min = nearest_point(ps, ps_or[ps_or.Count - 1]);
                if ((ps[i_min] - ps_or[ps_or.Count - 1]).magnitude() > max_dist)
                {
                    ps = order_points(ps);
                    ps_or.Add(ps[0]);
                    ps = remove_element(ps, 0);
                }
                else
                {
                    ps_or.Add(ps[i_min]);
                    ps = remove_element(ps, i_min);
                }

            }

            return ps_or.ToArray();
        }

        public static int point_count_in_neighborhood(Point3d_GL[] ps, Point3d_GL p, double max_dist)
        {
            var count = 0;
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].exist)
                {
                    var d = (ps[i] - p).magnitude();
                    if (d < max_dist)
                    {
                        count++;
                    }
                }
            }
            return count;
        }


        public static Point3d_GL[][] get_contours(Point3d_GL[] ps)
        {
            if (ps == null) return null;
            var contours = new List<int[]>();
            var ps_cl = (Point3d_GL[])ps.Clone();

            var cont = new List<int>();
            cont.Add(0);
            for (int i = 0; i < ps.Length; i++)
            {
                if (cont.Count < 3) ps_cl[cont[0]] = Point3d_GL.notExistP();
                else ps_cl[cont[0]] = ps[cont[0]];

                var i_n = nearest_point(ps_cl, ps[cont[cont.Count - 1]]);
                cont.Add(i_n);

                ps_cl[i_n] = Point3d_GL.notExistP();
                if (i_n == cont[0])
                {
                    contours.Add(cont.ToArray());
                    cont = new List<int>();
                    cont.Add(first_exist(ps_cl));
                }
            }

            var conts_ret = new List<Point3d_GL[]>();
            for (int i = 0; i < contours.Count; i++)
            {
                var cont_ret = new Point3d_GL[contours[i].Length];
                for (int j = 0; j < contours[i].Length; j++)
                {
                    cont_ret[j] = ps[contours[i][j]];
                }
                conts_ret.Add(cont_ret);
            }

            return conts_ret.ToArray();
        }

        public static int nearest_point(Point3d_GL[] ps, Point3d_GL p)
        {
            if (ps == null || !p.exist) return -1;
            var d_min = double.MaxValue;
            int i_min = 0;

            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].exist)
                {
                    var d = (ps[i] - p).magnitude();
                    if (d < d_min)
                    {
                        d_min = d;
                        i_min = i;
                    }
                }
            }
            return i_min;
        }


        public static int first_exist(Point3d_GL[] ps)
        {
            if (ps == null) return -1;
            for (int i = 0; i < ps.Length; i++)
            {
                if (ps[i].exist)
                {
                    return i;
                }
            }
            return -1;
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
                    var dist = (ps[i] - ps[j]).magnitude();
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
        public static Point3d_GL[] remove_element(Point3d_GL[] ps, int ind)
        {
            var ps_l = new List<Point3d_GL>();
            for (int i = 0; i < ps.Length; i++)
            {
                if (i != ind) ps_l.Add(ps[i]);
            }

            return ps_l.ToArray();
        }

        public static Point3d_GL centr_mass(Point3d_GL[] ps)
        {
            var cm = new Point3d_GL(0, 0, 0);
            foreach (var p in ps)
            {
                cm += p;
            }
            return cm / ps.Length;
        }

        public static double calc_curve_cm_2(Point3d_GL[] ps)
        {
            var cm = centr_mass(ps);
            var v1 = new Vector3d_GL(cm, ps[0]);
            var v2 = new Vector3d_GL(cm, ps[ps.Length - 1]);
            var cos = Vector3d_GL.cos(v1, v2);
            return Math.Abs(cos);
        }
        public static double calc_curve_cm(Point3d_GL[] ps)
        {
            var cm = centr_mass(ps);
            var r = (ps[ps.Length / 2] - cm).magnitude();
            return r;
        }

        public static Point3d_GL[] divide_sect(Point3d_GL p1, Point3d_GL p2, int num)
        {
            var traj_div = new List<Point3d_GL>();
            var delt = p2 - p1;

            for (int i = 0; i < num; i++)
            {
                traj_div.Add(p1 + delt * ((double)i / num));
            }

            return traj_div.ToArray();
        }

        public static Point3d_GL[] divide_sect_dz(Point3d_GL p1, Point3d_GL p2, int num, double dz, double ddz)
        {
            var traj_div = new List<Point3d_GL>();
            var delt = p2 - p1;
            var d_delt = (delt / (double)num);
            if (d_delt.magnitude() > dz + ddz)
            {
                num = (int)(delt.magnitude() / (dz + ddz));
            }
            if (d_delt.magnitude() < dz - ddz)
            {
                num = (int)(delt.magnitude() / (dz + ddz));
            }
            for (int i = 0; i <= num; i++)
            {
                traj_div.Add(p1 + delt * ((double)i / num));
            }

            return traj_div.ToArray();
        }
        public static Point3d_GL Max(Point3d_GL p1, Point3d_GL p2)
        {
            var x = Math.Max(p1.x, p2.x);
            var y = Math.Max(p1.y, p2.y);
            var z = Math.Max(p1.z, p2.z);

            return new Point3d_GL(x, y, z);
        }
        public static Point3d_GL Min(Point3d_GL p1, Point3d_GL p2)
        {
            var x = Math.Min(p1.x, p2.x);
            var y = Math.Min(p1.y, p2.y);
            var z = Math.Min(p1.z, p2.z);

            return new Point3d_GL(x, y, z);
        }
        public static Point3d_GL Max(Point3d_GL[] ps)
        {
            var ps_max = new Point3d_GL(double.MinValue, double.MinValue, double.MinValue);
            for (int i = 0; i < ps.Length; i++)
            {
                ps_max = Max(ps_max, ps[i]);
            }
            return ps_max;
        }
        public static Point3d_GL Max(Point3d_GL[][] ps)
        {
            var ps_max = new Point3d_GL(double.MinValue, double.MinValue, double.MinValue);
            for (int i = 0; i < ps.Length; i++)
            {
                ps_max = Max(ps_max, Max(ps[i]));
            }
            return ps_max;
        }
        public static Point3d_GL Min(Point3d_GL[][] ps)
        {
            var ps_max = new Point3d_GL(double.MinValue, double.MinValue, double.MinValue);
            for (int i = 0; i < ps.Length; i++)
            {
                ps_max = Min(ps_max, Min(ps[i]));
            }
            return ps_max;
        }
        public static Point3d_GL Min(List<List<Point3d_GL>> ps)
        {
            var ps_max = new Point3d_GL(double.MaxValue, double.MaxValue, double.MaxValue);
            for (int i = 0; i < ps.Count; i++)
            {
                ps_max = Min(ps_max, Min(ps[i].ToArray()));
            }
            return ps_max;
        }
        public static Point3d_GL Max(List<List<Point3d_GL>> ps)
        {
            var ps_max = new Point3d_GL(double.MinValue, double.MinValue, double.MinValue);
            for (int i = 0; i < ps.Count; i++)
            {
                ps_max = Max(ps_max, Max(ps[i].ToArray()));
            }
            return ps_max;
        }
        public static Point3d_GL Min(Point3d_GL[] ps)
        {
            var ps_max = new Point3d_GL(double.MaxValue, double.MaxValue, double.MaxValue);
            for (int i = 0; i < ps.Length; i++)
            {
                ps_max = Min(ps_max, ps[i]);
            }
            return ps_max;
        }

        public Point3d_GL setx(double x) { this.x = x; return this; }
        public Point3d_GL sety(double y) { this.y = y; return this; }
        public Point3d_GL setz(double z) { this.z = z; return this; }

        public static Point3d_GL[] add_arr(Point3d_GL[] ps, Point3d_GL p)
        {
            var ret = new Point3d_GL[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ret[i] = ps[i] + p;
            }
            return ret;
        }
        public static List<Point3d_GL> add_arr(List<Point3d_GL> ps, Point3d_GL p)
        {
            var ret = new Point3d_GL[ps.Count];
            for (int i = 0; i < ps.Count; i++)
            {
                ret[i] = ps[i] + p;
            }
            return ret.ToList();
        }
        public static Point3d_GL rotate_point(Point3d_GL p, double angle)
        {
            var x_r = p.x * Math.Cos(angle) - p.y * Math.Sin(angle);
            var y_r = p.x * Math.Sin(angle) + p.y * Math.Cos(angle);
            return new Point3d_GL(x_r, y_r, p.z, p.color);
        }
        public static List<Point3d_GL> rotate_points(List<Point3d_GL> traj, double angle)
        {
            var traj_rot = new List<Point3d_GL>();
            for (int i = 0; i < traj.Count; i++)
            {
                traj_rot.Add(rotate_point(traj[i], angle));
            }
            return traj_rot;
        }
        public static double[] dist_ps(Point3d_GL[] ps1, Point3d_GL[] ps2)
        {
            if (ps1 == null || ps2 == null) return null;
            if (ps1.Length != ps2.Length) return null;
            var ret = new double[ps1.Length];
            for (int i = 0; i < ps1.Length; i++)
            {
                ret[i] = (ps1[i] - ps2[i]).magnitude();
            }
            return ret;
        }
        public static double[] dist_betw_ps(Point3d_GL[] ps1)
        {
            if (ps1 == null) return null;
            if (ps1.Length < 2) return null;
            var ret = new double[ps1.Length - 1];
            for (int i = 1; i < ps1.Length; i++)
            {
                ret[i - 1] = (ps1[i] - ps1[i - 1]).magnitude();
            }
            return ret;
        }
        public static Point3d_GL[] line_aver(Point3d_GL[] ps, int wind)
        {
            if (ps == null) return null;
            if (ps.Length < 3) return null;
            var ps_s = new Point3d_GL[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                int beg = i - wind; if (beg < 0) beg = 0;
                int end = i + wind; if (end > ps.Length - 1) end = ps.Length - 1;
                var ps_av = new Point3d_GL(0, 0, 0);
                for (int j = beg; j < end; j++)
                {
                    ps_av += ps[j];
                }
                ps_s[i] = ps_av / (end - beg);
            }

            return ps_s;
        }

        public static Point3d_GL[] line_laplace(Point3d_GL[] ps, int iter)
        {
            if (ps == null) return null;
            if (ps.Length < 3) return null;
            var ps_s = new Point3d_GL[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                int beg = i - 1; if (beg < 0) beg = 0;
                int end = i + 1; if (end > ps.Length - 1) end = ps.Length - 1;
                var L_p = (ps[end] - ps[i]) * 0.5 + (ps[beg] - ps[i]) * 0.5;
                ps_s[i] = ps[i] + L_p * 0.5;
            }
            iter--;

            if (iter <= 0)
                return ps_s;
            else
                return line_laplace(ps_s, iter);
        }

        public static Point3d_GL aver(Point3d_GL[] ps, Color3d_GL color = null)
        {
            var p = new Point3d_GL(0, 0, 0);
            for (int i = 0; i < ps.Length; i++)
            {
                p += ps[i];
            }
            p.color = color;
            return p / ps.Length;
        }

        public static Point3d_GL vec_perpend_2_vecs(Point3d_GL v1, Point3d_GL v2)
        {
            double d = Math.Pow(v1.x, 2) * Math.Pow(v2.y, 2) + Math.Pow(v1.x, 2) * Math.Pow(v2.z, 2)
                - 2 * v1.x * v1.y * v2.x * v2.y - 2 * v1.x * v2.x * v1.z * v2.z + Math.Pow(v1.y, 2) * Math.Pow(v2.x, 2)
                + Math.Pow(v1.y, 2) * Math.Pow(v2.z, 2) - 2 * v1.y * v1.z * v2.y * v2.z + Math.Pow(v2.x, 2) * Math.Pow(v1.z, 2)
                + Math.Pow(v1.z, 2) * Math.Pow(v2.y, 2);
            double vx = (v1.y * v2.z - v1.z * v2.y) * Math.Sqrt(1 / d);
            double vy = -(v1.x * v2.z - v2.x * v1.z) * Math.Sqrt(1 / d);
            double vz = (v1.x * v2.y - v1.y * v2.x) * Math.Sqrt(1 / d);

            return new Point3d_GL(vx, vy, vz);
        }
        /* public static Point3d_GL vec_perpend_vec_p(Point3d_GL v1, Point3d_GL v2, Point3d_GL p)
         {
             double d = Math.Pow(v1.x, 2) * Math.Pow(v2.y, 2) + Math.Pow(v1.x, 2) * Math.Pow(v2.z, 2)
                 - 2 * v1.x * v1.y * v2.x * v2.y - 2 * v1.x * v2.x * v1.z * v2.z + Math.Pow(v1.y, 2) * Math.Pow(v2.x, 2)
                 + Math.Pow(v1.y, 2) * Math.Pow(v2.z, 2) - 2 * v1.y * v1.z * v2.y * v2.z + Math.Pow(v2.x, 2) * Math.Pow(v1.z, 2)
                 + Math.Pow(v1.z, 2) * Math.Pow(v2.y, 2);
             double vx = (v1.y * v2.z - v1.z * v2.y) * Math.Sqrt(1 / d);
             double vy = -(v1.x * v2.z - v2.x * v1.z) * Math.Sqrt(1 / d);
             double vz = (v1.x * v2.y - v1.y * v2.x) * Math.Sqrt(1 / d);

             return new Point3d_GL(vx, vy, vz);
         }*/
        public static Point3d_GL vec_perpend_xy(Point3d_GL v1)
        {
            var x1 = -v1.y;
            var y1 = v1.x;

            return new Point3d_GL(x1, y1);
        }

        public Point3d_GL toPolar()//x -> r, y -> fi
        {
            var r = Math.Sqrt(x * x + y * y);

            double fi;
            if (x > 0 && y >= 0) fi = Math.Atan(y / x);
            else if (x > 0 && y < 0) fi = Math.Atan(y / x) + 2 * Math.PI;
            else if (x < 0) fi = Math.Atan(y / x) + Math.PI;
            else if (x == 0 && y > 0) fi = Math.PI / 2;
            else if (x == 0 && y < 0) fi = 3 * Math.PI / 2;
            else fi = 0;

            return new Point3d_GL(r, fi);
        }

        public Point3d_GL toCart()//r -> x, fi -> y
        {
            return new Point3d_GL(x * Math.Cos(y), x * Math.Sin(y));
        }

        public static Point3d_GL[] toPolar(Point3d_GL[] ps_cart)
        {
            var ps = new Point3d_GL[ps_cart.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i] = ps_cart[i].toPolar();
            }
            return ps;
        }

        public static Point3d_GL[] toCart(Point3d_GL[] ps_pol)
        {
            var ps = new Point3d_GL[ps_pol.Length];
            for (int i = 0; i < ps.Length; i++)
            {
                ps[i] = ps_pol[i].toCart();
            }
            return ps;
        }

        public static Point3d_GL[] gaussFilter_X(Point3d_GL[] ps1, int wind = 3)
        {
            var ps1L = ps1.ToList();
            var ps1ret = (Point3d_GL[])ps1.Clone();
            for (int i = wind; i < ps1.Length - wind; i++)
            {
                if (i > 0)
                {

                    Point3d_GL sum = new Point3d_GL();
                    Array.ForEach(ps1L.GetRange(i - wind, 2 * wind).ToArray(), p => sum += p);
                    var aver = sum.x / (2 * wind);
                    ps1ret[i].x = aver;
                }
            }
            return ps1ret;
        }
        public static Point3d_GL[] sortByX(Point3d_GL[] ps)
        {
            var ps_sort = from p in ps
                          orderby p.x descending
                          select p;
            return ps_sort.ToArray();
        }
        public static Point3d_GL[] sortByY(Point3d_GL[] ps)
        {
            var ps_sort = from p in ps
                          orderby p.y descending
                          select p;
            return ps_sort.ToArray();
        }
        static public Point3d_GL calcCirc(Point3d_GL p1, Point3d_GL p2, Point3d_GL p3)
        {
            double mx1 = p1.x;
            double my1 = p1.y;
            double mx2 = p2.x;
            double my2 = p2.y;
            double mx3 = p3.x;
            double my3 = p3.y;
            if (((2 * my2 - 2 * my1) * (2 * mx2 - 2 * mx3) - (2 * my3 - 2 * my2) * (2 * mx1 - 2 * mx2)) == 0 || (2 * my2 - 2 * my1) == 0) return notExistP();
            var x_okr = ((2 * my3 - 2 * my2) * (my2 * my2 - my1 * my1 + mx2 * mx2 - mx1 * mx1) - (2 * my2 - 2 * my1) * (my3 * my3 - my2 * my2 + mx3 * mx3 - mx2 * mx2)) / ((2 * my2 - 2 * my1) * (2 * mx2 - 2 * mx3) - (2 * my3 - 2 * my2) * (2 * mx1 - 2 * mx2));
            var y_okr = (my2 * my2 - my1 * my1 + mx2 * mx2 - mx1 * mx1 + 2 * mx1 * x_okr - 2 * mx2 * x_okr) / (2 * my2 - 2 * my1);
            var r_okr = Math.Sqrt(Math.Pow((my1 - y_okr), 2) + Math.Pow((mx1 - x_okr), 2));
            return new Point3d_GL(x_okr, y_okr, r_okr);
        }

        
        public Point3d_GL set_color_this(Color3d_GL _color)
        {
            color = _color;
            return this;
        }


    }

    


}
