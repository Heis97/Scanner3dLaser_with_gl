using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Emgu.CV;

namespace opengl3
{
    public struct VertexGl
    {
        public Point3d_GL p;
        public Point3d_GL n;
        public PointF t;
        public VertexGl(Point3d_GL _p, Point3d_GL _n, PointF _t)
        {
            p = _p;
            n = _n;
            t = _t;
        }
    }
    public struct TriangleGl
    {
        public VertexGl v1;
        public VertexGl v2;
        public VertexGl v3;
        public Vector3d_GL n;
        public TriangleGl(VertexGl _v1, VertexGl _v2, VertexGl _v3)
        {
            v1 = _v1;
            v2 = _v2;
            v3 = _v3;
            var vec1 = new Vector3d_GL(v1.p, v2.p);
            var vec2 = new Vector3d_GL(v1.p, v3.p);
            var vec3 = vec1 | vec2;//vector multiply
            vec3.normalize();
            n = vec3;
        }
        public bool affilationPoint(PointF _p)
        {
            float m, l;
            //var p = _p;
            var P = _p - v1.t;
            var B = v2.t - v1.t;
            var C = v3.t - v1.t;
            m = (P.X * B.Y - B.X * P.Y) / (C.X * B.Y - B.X * C.Y);
            if (m >= 0 && m <= 1)
            {
                l = (P.X - m * C.X) / B.X;
                if (l >= 0 && (m + l) <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        public bool affilationPoint_xy(Point3d_GL _p)
        {
            double m, l;
            var P = _p - v1.p;
            var B = v2.p - v1.p;
            var C = v3.p - v1.p;
            m = (P.x * B.y - B.x * P.y) / (C.x * B.y - B.x * C.y);
            if (m >= 0 && m <= 1)
            {
                l = (P.x - m * C.x) / B.x;
                if (l >= 0 && (m + l) <= 1)
                {
                    return true;
                }
            }
            return false;
        }

        public Point3d_GL project_point_xy(Point3d_GL p)
        {
            if (n.z == 0)
            {
                return new Point3d_GL(p.x, p.y, 0);
            }

            var z = (-(v1.p * n) - n.x * p.x - n.y * p.y) / n.z;
            return new Point3d_GL(p.x, p.y, 0);
        }
        public static void multiply_matr(TriangleGl[] triangles, Matrix<double> matrix)
        {
            for (int i = 0; i < triangles.Length; i++)
            {
                triangles[i].v1.p = matrix * triangles[i].v1.p;
                triangles[i].v2.p = matrix * triangles[i].v2.p;
                triangles[i].v3.p = matrix * triangles[i].v3.p;
            }
        }

        public static float[] get_mesh(TriangleGl[] triangles)
        {
            float[] mesh = new float[triangles.Length * 9];
            for (int i = 0; i < triangles.Length; i++)
            {
                mesh[9 * i] = (float)triangles[i].v1.p.x;
                mesh[9 * i + 1] = (float)triangles[i].v1.p.y;
                mesh[9 * i + 2] = (float)triangles[i].v1.p.z;

                mesh[9 * i + 3] = (float)triangles[i].v2.p.x;
                mesh[9 * i + 4] = (float)triangles[i].v2.p.y;
                mesh[9 * i + 5] = (float)triangles[i].v2.p.z;

                mesh[9 * i + 6] = (float)triangles[i].v3.p.x;
                mesh[9 * i + 7] = (float)triangles[i].v3.p.y;
                mesh[9 * i + 8] = (float)triangles[i].v3.p.z;
            }
            return mesh;
        }
    }
    public struct Model3d
    {
        public string path;
        public float[] mesh;
        public float[] texture;
        public float[] normale;
        public float[] color;
        public TriangleGl[] triangles;
        public Point3d_GL center;
        public float scale;
        public Matrix<double> matrix_norm;

        public Polygon3d_GL[] pols;

        public Model3d(string _path, bool centering = false, float _scale = 1)
        {
            path = _path;
            var name_list = path.Split('.');
            var format = name_list[name_list.Length - 1].ToLower();
            var center1 = new Point3d_GL(0, 0, 0);
            scale = _scale;
            matrix_norm = null;
            pols = null;
            if (format == "obj")
            {
                var arrays = parsingObj(path, out pols, out center1, scale, ref matrix_norm);
                mesh = arrays[0];
                texture = arrays[1];
                normale = arrays[2];
                color = new float[mesh.Length];
                triangles = null;
                center = center1;


            }
            else if (format == "stl")
            {
                var arrays = parsingStl_GL4(path, out center1,_scale);
                mesh = (float[])arrays[0];

                    
                texture = new float[mesh.Length];
                normale = (float[])arrays[1];
                pols = (Polygon3d_GL[])arrays[2];
                color = new float[mesh.Length];
                for(int i = 0; i < color.Length; i++)
                {
                    color[i] = 0.5f;
                }
                triangles = null;
                center = center1;
            }
            else
            {
                mesh = new float[0];
                texture = new float[0];
                normale = new float[0];
                color = new float[0];
                triangles = null;
                center = center1;
            }
            if (centering)
            {
                FrameToCenter();
            }

        }
        public void FrameToCenter()
        {
            for (int i = 0; i < mesh.Length; i += 3)
            {
                mesh[i] += (float)center.x;
                mesh[i + 1] += (float)center.y;
                mesh[i + 2] += (float)center.z;
            }
        }
        public float[] parsingTxt_Tab(string path)
        {

            var text = File.ReadAllText(path);
            var lines = text.Split('\n');
            var mesh = new float[(lines.Length - 2) * 3];
            var ind = 0;
            for (int i = 1; i < lines.Length - 1; i++)
            {
                var p = lines[i].Split('\t');
                mesh[ind] = (float)parseE(p[0]); ind++;
                mesh[ind] = (float)parseE(p[1]) - 30; ind++;
                mesh[ind] = (float)parseE(p[2]); ind++;

            }
            for (int i = 0; i < 41 - 1; i++)
            {
                for (int j = 0; j < 91 - 1; j++)
                {
                    //Console.WriteLine(mesh[3 * (i * 91 + j+1) ] - mesh[3 * (i * 91 + j) ]);
                    //mesh[3*(i * 91 + j) +2] += j;    

                }
            }
            var text1 = text.Replace('\t', ';');
            // Console.WriteLine("Len = "+((double)lines.Length-2)/91);
            return mesh;
        }

        static public double parseE(string num)
        {
            if (num.Contains("e"))
            {
                var splnum = num.Split(new char[] { 'e' });
                return Convert.ToDouble(splnum[0]) * Math.Pow(10, Convert.ToInt32(splnum[1]));
            }
            else if (num.Contains("E"))
            {
                var splnum = num.Split(new char[] { 'E' });
                return Convert.ToDouble(splnum[0]) * Math.Pow(10, Convert.ToInt32(splnum[1]));
            }
            else
            {
                return Convert.ToDouble(num);
            }

        }

        static public int[] parseFace(string num)
        {
            var splnum = num.Split('/');
            var splnum_int = new int[splnum.Length];
            for (int i = 0; i < splnum.Length; i++)
            {
                splnum_int[i] = Convert.ToInt32(splnum[i]);
            }
            return splnum_int;

        }

        static float[] minCompar(float[] val, float[] min)
        {
            if (val == null || min == null)
            {
                return min;
            }
            if (val.Length != min.Length)
            {
                return min;
            }
            for (int i = 0; i < val.Length; i++)
            {
                if (min[i] > val[i])
                {
                    min[i] = val[i];
                }
            }
            return min;
        }

        static float[] maxCompar(float[] val, float[] max)
        {
            if (val == null || max == null)
            {
                return max;
            }
            if (val.Length != max.Length)
            {
                return max;
            }
            for (int i = 0; i < val.Length; i++)
            {
                if (max[i] < val[i])
                {
                    max[i] = val[i];
                }
            }
            return max;
        }

        static public float[] parsingStl_GL4_nenorm(string path, out Point3d_GL _center)
        {
            float[] min_v = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
            float[] max_v = new float[] { float.MinValue, float.MinValue, float.MinValue };
            string file1;
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            int len = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 3)
                {
                    if (vert[0].Contains("ert"))
                    {
                        len += 3;
                    }

                }
            }
            float[] ret1 = new float[len];
            Console.WriteLine("Len Stl " + len);
            int i2 = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 3)
                {

                    if (vert[0].Contains("ert"))
                    {
                        ret1[i2] = (float)parseE(vert[1]); i2++;
                        ret1[i2] = (float)parseE(vert[2]); i2++;
                        ret1[i2] = (float)parseE(vert[3]); i2++;

                        min_v = minCompar(new float[] { ret1[i2 - 3], ret1[i2 - 2], ret1[i2 - 1] }, min_v);
                        max_v = maxCompar(new float[] { ret1[i2 - 3], ret1[i2 - 2], ret1[i2 - 1] }, max_v);
                    }

                }
            }
            float x_sr = (max_v[0] - min_v[0]) / 2 + min_v[0];
            float y_sr = (max_v[1] - min_v[1]) / 2 + min_v[1];
            float z_sr = (max_v[2] - min_v[2]) / 2 + min_v[2];
            _center = new Point3d_GL(x_sr, y_sr, z_sr);


            return ret1;
        }


        private static object[] parsingStl_GL4_binary(string fileName, out Point3d_GL _center, float scale = 1)
        {
            int i2 = 0;
            int i3 = 0;
            float[] min_v = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
            float[] max_v = new float[] { float.MinValue, float.MinValue, float.MinValue };
            float[] ret1 = null;
            float[] ret2 = null;
            var polygs = new List<Polygon3d_GL>();
            var ps = new List<Point3d_GL>();

            using (BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                // Read header info
                byte[] header = br.ReadBytes(80);
                byte[] length = br.ReadBytes(4);
                int len_f = BitConverter.ToInt32(length, 0);
                string headerInfo = Encoding.UTF8.GetString(header, 0, header.Length).Trim();
                //Console.WriteLine(String.Format("\nImporting: {0}\n\tHeader: {1}\n\tNumber of faces:{2}\n", fileName, headerInfo, len_f));
                ret1 = new float[len_f * 9];
                ret2 = new float[len_f * 9];
                // Read Data
                    

                for(int b = 0; b < len_f*3; b++)  
                {
                    byte[] block = br.ReadBytes(50);
                    if (block.Length > 47)
                    {
                        byte[] xComp = new byte[4];
                        byte[] yComp = new byte[4];
                        byte[] zComp = new byte[4];

                        for (int i = 1; i < 4; i++)
                        {

                            float x = BitConverter.ToSingle(block, i * 12);
                            float y = BitConverter.ToSingle(block, i * 12 + 4);
                            float z = BitConverter.ToSingle(block, i * 12 + 8);

                            ret1[i2] = scale * x; i2++;
                            ret1[i2] = scale * y; i2++;
                            ret1[i2] = scale * z; i2++;
                            ps.Add(new Point3d_GL(scale * x, scale * y, scale * z));
                            min_v = minCompar(new float[] { ret1[i2 - 3], ret1[i2 - 2], ret1[i2 - 1] }, min_v);
                            max_v = maxCompar(new float[] { ret1[i2 - 3], ret1[i2 - 2], ret1[i2 - 1] }, max_v);

                        }

                        var p = new Polygon3d_GL(ps[0], ps[1], ps[2]);
                        polygs.Add(p);
                        var n = p.v3;
                        for (int j = 0; j < 3; j++)
                        {
                            ret2[i3] = (float)n.x; i3++;
                            ret2[i3] = (float)n.y; i3++;
                            ret2[i3] = (float)n.z; i3++;
                        }
                        ps = new List<Point3d_GL>();
                    }
                }
            } 

            float x_sr = (max_v[0] - min_v[0]) / 2 + min_v[0];
            float y_sr = (max_v[1] - min_v[1]) / 2 + min_v[1];
            float z_sr = (max_v[2] - min_v[2]) / 2 + min_v[2];
            _center = new Point3d_GL(x_sr, y_sr, z_sr);
            return new object[] { ret1, ret2, polygs.ToArray() };
        }
        public static object[] parsing_raw_binary(string fileName)
        {
            int i2 = 0;
            int i3 = 0;
            float[] min_v = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
            float[] max_v = new float[] { float.MinValue, float.MinValue, float.MinValue };
            float[] ret1 = null;
            float[] ret2 = null;
            var polygs = new List<Polygon3d_GL>();
            var ps = new List<Point3d_GL>();
            bool cont = true;
            using (BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                // Read header info
                
                byte[] header = br.ReadBytes(4);
                byte[] length = br.ReadBytes(4);
                int len_f = BitConverter.ToInt32(length, 0);
                string headerInfo = Encoding.UTF8.GetString(header, 0, header.Length).Trim();


                for (int b = 0; cont; b++)
                {
                    byte[] block = br.ReadBytes(12);
                    if (block.Length > 11)
                    {
                        //if (block[])
                        float x = BitConverter.ToSingle(block, 0);
                        float y = BitConverter.ToSingle(block, 4);
                        float z = BitConverter.ToSingle(block, 8);
                        ps.Add(new Point3d_GL(x, y, z));
                       
                    }
                    else
                    {
                        cont = false;
                    }
                }
            }


            return new object[] { ps.ToArray()};
        }



        static public object[] parsingStl_GL4_ascii(string path, out Point3d_GL _center, float scale = 1)
        {
            float[] min_v = new float[] { float.MaxValue, float.MaxValue, float.MaxValue };
            float[] max_v = new float[] { float.MinValue, float.MinValue, float.MinValue };
            string file1;
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            int len = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 3)
                {
                    if (vert[0].Contains("ert"))
                    {
                        len += 3;
                    }

                }
            }
            var polygs = new List<Polygon3d_GL>();
            float[] ret1 = new float[len];
            float[] ret2 = new float[len];
            Console.WriteLine("Len Stl " + len);
            int i2 = 0;
            int i3 = 0;
            var ps = new List<Point3d_GL>();
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 3)
                {

                    if (vert[0].Contains("ert"))
                    {
                        var x = (float)parseE(vert[1]);
                        var y = (float)parseE(vert[2]);
                        var z = (float)parseE(vert[3]);

                        ret1[i2] = scale * x; i2++;
                        ret1[i2] = scale * y; i2++;
                        ret1[i2] = scale * z; i2++;

                        ps.Add(new Point3d_GL(x, y, z));

                        if (ps.Count > 2)
                        {
                            var p = new Polygon3d_GL(ps[0], ps[1], ps[2]);
                            polygs.Add(p);
                            var n = p.v3;
                            for (int j = 0; j < 3; j++)
                            {
                                ret2[i3] = (float)n.x; i3++;
                                ret2[i3] = (float)n.y; i3++;
                                ret2[i3] = (float)n.z; i3++;
                            }
                            ps = new List<Point3d_GL>();

                        }

                        min_v = minCompar(new float[] { ret1[i2 - 3], ret1[i2 - 2], ret1[i2 - 1] }, min_v);
                        max_v = maxCompar(new float[] { ret1[i2 - 3], ret1[i2 - 2], ret1[i2 - 1] }, max_v);
                    }

                }
            }
            float x_sr = (max_v[0] - min_v[0]) / 2 + min_v[0];
            float y_sr = (max_v[1] - min_v[1]) / 2 + min_v[1];
            float z_sr = (max_v[2] - min_v[2]) / 2 + min_v[2];
            _center = new Point3d_GL(x_sr, y_sr, z_sr);
            

            //prin.t(ret2);
            return new object[] { ret1, ret2, polygs.ToArray() };
        }

        static public object[] parsingStl_GL4(string path, out Point3d_GL _center, float scale = 1)
        {
            string file1;

            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            if (file1.Contains("solid")) return parsingStl_GL4_ascii(path, out _center, scale);
            else return parsingStl_GL4_binary(path, out _center, scale);            
        }


        static public float[][] parsingObj(string path, out Polygon3d_GL[] triangleGl, out Point3d_GL _center, float scale, ref Matrix<double> matrix)
        {
            var ret = new List<float[]>();
            string file1;
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            int v_len = 0;
            int vt_len = 0;
            int vn_len = 0;
            int f_len = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 2)
                {
                    if (vert[0] == "v")
                    {
                        v_len++;
                    }
                    if (vert[0] == "vt")
                    {
                        vt_len++;
                    }
                    if (vert[0] == "vn")
                    {
                        vn_len++;
                    }
                    if (vert[0] == "f")
                    {
                        f_len++;
                    }
                }
            }
            float[][] vertex = new float[v_len][];
            float[][] texture = new float[vt_len][];
            float[][] normale = new float[vn_len][];
            int[][] face_v = new int[f_len][];
            int[][] face_vt = new int[f_len][];
            int[][] face_vn = new int[f_len][];
            triangleGl = null;
            Console.WriteLine("Len v " + v_len);
            Console.WriteLine("Len vt " + vt_len);
            Console.WriteLine("Len vn " + vn_len);
            int i_v = 0;
            int i_vt = 0;
            int i_vn = 0;
            int i_f = 0;
            float[] min_v = new float[3];
            min_v[0] = float.MaxValue;
            min_v[1] = float.MaxValue;
            min_v[2] = float.MaxValue;
            float[] max_v = new float[3];
            max_v[0] = float.MinValue;
            max_v[1] = float.MinValue;
            max_v[2] = float.MinValue;
            double sum_x = 0;
            double sum_y = 0;
            double sum_z = 0;

            foreach (string str in lines)
            {
                string line = str.Trim();
                string[] subline = line.Split(new char[] { ' ' });

                if (subline.Length > 2)
                {
                    if (subline[0] == "v")
                    {
                        //Console.WriteLine
                        vertex[i_v] = new float[3];
                        vertex[i_v][0] = scale * (float)parseE(subline[1]);
                        vertex[i_v][1] = scale * (float)parseE(subline[2]);
                        vertex[i_v][2] = scale * (float)parseE(subline[3]);
                        max_v = maxCompar(vertex[i_v], max_v);
                        min_v = minCompar(vertex[i_v], min_v);
                        sum_x += vertex[i_v][0];
                        sum_y += vertex[i_v][1];
                        sum_z += vertex[i_v][2];

                        i_v++;
                    }
                    if (subline[0] == "vn")
                    {
                        //Console.WriteLine
                        normale[i_vn] = new float[3];
                        normale[i_vn][0] = (float)parseE(subline[1]);
                        normale[i_vn][1] = (float)parseE(subline[2]);
                        normale[i_vn][2] = (float)parseE(subline[3]);
                        i_vn++;
                    }
                    if (subline[0] == "vt")
                    {
                        texture[i_vt] = new float[2];
                        texture[i_vt][0] = (float)parseE(subline[1]);
                        texture[i_vt][1] = (float)parseE(subline[2]);
                        i_vt++;
                    }
                    if (subline[0] == "f")
                    {
                        face_v[i_f] = new int[3];
                        face_v[i_f][0] = parseFace(subline[1])[0];
                        face_v[i_f][1] = parseFace(subline[2])[0];
                        face_v[i_f][2] = parseFace(subline[3])[0];

                        face_vt[i_f] = new int[3];
                        face_vt[i_f][0] = parseFace(subline[1])[1];
                        face_vt[i_f][1] = parseFace(subline[2])[1];
                        face_vt[i_f][2] = parseFace(subline[3])[1];


                        face_vn[i_f] = new int[3];
                        face_vn[i_f][0] = parseFace(subline[1])[2];
                        face_vn[i_f][1] = parseFace(subline[2])[2];
                        face_vn[i_f][2] = parseFace(subline[3])[2];

                        i_f++;
                    }
                }

            }

            var vertexdata = new float[f_len * 9];
            var normaldata = new float[f_len * 9];
            var textureldata = new float[f_len * 6];
            i_v = 0;
            i_vn = 0;
            i_vt = 0;
            for (int i = 0; i < f_len; i++)
            {
                vertexdata[i_v] = vertex[face_v[i][0] - 1][0]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][0] - 1][1]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][0] - 1][2]; i_v++;

                normaldata[i_vn] = normale[face_vn[i][0] - 1][0]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][0] - 1][1]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][0] - 1][2]; i_vn++;

                textureldata[i_vt] = texture[face_vt[i][0] - 1][0]; i_vt++;
                textureldata[i_vt] = texture[face_vt[i][0] - 1][1]; i_vt++;

                /* var v1 = new VertexGl(
                        new Point3d_GL(vertexdata[i_v - 3], vertexdata[i_v - 2], vertexdata[i_v - 1]),
                        new Point3d_GL(normaldata[i_vn - 3], normaldata[i_vn - 2], normaldata[i_vn - 1]),
                        new PointF(textureldata[i_vt - 2], textureldata[i_vt - 1]));*/
                //--------------------------------------------------------------

                vertexdata[i_v] = vertex[face_v[i][1] - 1][0]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][1] - 1][1]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][1] - 1][2]; i_v++;

                normaldata[i_vn] = normale[face_vn[i][1] - 1][0]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][1] - 1][1]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][1] - 1][2]; i_vn++;

                textureldata[i_vt] = texture[face_vt[i][1] - 1][0]; i_vt++;
                textureldata[i_vt] = texture[face_vt[i][1] - 1][1]; i_vt++;

                /* var v2 = new VertexGl(
                        new Point3d_GL(vertexdata[i_v - 3], vertexdata[i_v - 2], vertexdata[i_v - 1]),
                        new Point3d_GL(normaldata[i_vn - 3], normaldata[i_vn - 2], normaldata[i_vn - 1]),
                        new PointF(textureldata[i_vt - 2], textureldata[i_vt - 1]));*/
                //--------------------------------------------------------------

                vertexdata[i_v] = vertex[face_v[i][2] - 1][0]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][2] - 1][1]; i_v++;
                vertexdata[i_v] = vertex[face_v[i][2] - 1][2]; i_v++;

                normaldata[i_vn] = normale[face_vn[i][2] - 1][0]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][2] - 1][1]; i_vn++;
                normaldata[i_vn] = normale[face_vn[i][2] - 1][2]; i_vn++;

                textureldata[i_vt] = texture[face_vt[i][2] - 1][0]; i_vt++;
                textureldata[i_vt] = texture[face_vt[i][2] - 1][1]; i_vt++;

                /*  var v3 = new VertexGl(new Point3d_GL(vertexdata[i_v - 3], vertexdata[i_v - 2], vertexdata[i_v - 1]),
                        new Point3d_GL(normaldata[i_vn - 3], normaldata[i_vn - 2], normaldata[i_vn - 1]),
                        new PointF(textureldata[i_vt - 2], textureldata[i_vt - 1]));*/

                //triangleGl[i] = new TriangleGl(v1, v2, v3);
            }


            float x_sr = (max_v[0] + min_v[0]) / 2;
            float y_sr = (max_v[1] + min_v[1]) / 2;
            float z_sr = (max_v[2] + min_v[2]) / 2;
            var size_x = (max_v[0] - min_v[0]);
            var size_y = (max_v[1] - min_v[1]);
            var size_z = (max_v[2] - min_v[2]);
            var size = Math.Max(size_z, Math.Max(size_x, size_y));
            scale = 2f / size;
            //Console.WriteLine(sum_x / i_v + " " + sum_y / i_v + " " + sum_z / i_v + " ");
            _center = new Point3d_GL(-sum_x / v_len, -sum_y / v_len, -sum_z / v_len);
            matrix = scale_matrix(scale) * transl_matrix(_center);
            //vertexdata = GraphicGL.translateMesh(vertexdata, matrix);
            //TriangleGl.multiply_matr(triangleGl, matrix);
            ret.Add(vertexdata);
            ret.Add(textureldata);
            ret.Add(normaldata);
            return ret.ToArray();
        }

        /*public void norm_mesh()
        {

        }*/


        static Matrix<double> scale_matrix(double scale)
        {
            return new Matrix<double>(new double[,] {
            { scale, 0, 0, 0 },
            { 0, scale, 0, 0 },
            { 0, 0, scale, 0 },
            { 0, 0, 0, 1} });
        }

        static Matrix<double> transl_matrix(Point3d_GL trans)
        {
            return new Matrix<double>(new double[,] {
            { 1, 0, 0, trans.x },
            { 0, 1, 0, trans.y },
            { 0, 0, 1, trans.z },
            { 0, 0, 0, 1 }});
        }

        public List<double[,]> parsingStl_GL2(string path)
        {
            int i2 = 0;
            string file1;
            List<double[,]> ret1 = new List<double[,]>();
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            double[,] norm = new double[(int)(lines.Length / 7), 3];
            double[,] p1 = new double[(int)(lines.Length / 7), 3];
            double[,] p2 = new double[(int)(lines.Length / 7), 3];
            double[,] p3 = new double[(int)(lines.Length / 7), 3];
            Console.WriteLine((int)(lines.Length / 7));
            Console.WriteLine("-------------------");
            int i3 = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });
                if (vert.Length > 3)
                {
                    if (vert[1].Contains("orma"))
                    {
                        norm[i2, 0] = parseE(vert[2]);
                        norm[i2, 1] = parseE(vert[3]);
                        norm[i2, 2] = parseE(vert[4]);

                        i3 = 0;
                    }
                    else if (vert[0].Contains("ert") && i3 == 0)
                    {
                        p1[i2, 0] = parseE(vert[1]);
                        p1[i2, 1] = parseE(vert[2]);
                        p1[i2, 2] = parseE(vert[3]);
                        i3++;
                    }
                    else if (vert[0].Contains("ert") && i3 == 1)
                    {
                        p2[i2, 0] = parseE(vert[1]);
                        p2[i2, 1] = parseE(vert[2]);
                        p2[i2, 2] = parseE(vert[3]);
                        i3++;
                    }
                    else if (vert[0].Contains("ert") && i3 == 2)
                    {
                        p3[i2, 0] = parseE(vert[1]);
                        p3[i2, 1] = parseE(vert[2]);
                        p3[i2, 2] = parseE(vert[3]);
                        i2++;
                    }
                }
            }

            Console.WriteLine("-------------------");
            Console.WriteLine(i2);
            ret1.Add(norm);
            ret1.Add(p1);
            ret1.Add(p2);
            ret1.Add(p3);

            return ret1;
        }

        public Point3d_GL take3dfrom2d(PointF point)
        {
            if (triangles != null)
            {
                for (int i = 0; i < triangles.Length; i++)
                {
                    if (triangles[i].affilationPoint(point))
                    {
                        return triangles[i].v1.p;
                    }
                }
            }
            return new Point3d_GL(0, 0, 0);
        }

        public Point3d_GL take3dfrom2d_gl(Point3d_GL point, double zoom)
        {
            var p = norm_to_gl_xy(point, zoom);

            if (triangles != null)
            {
                for (int i = 0; i < triangles.Length; i++)
                {
                    if (triangles[i].affilationPoint_xy(p))
                    {
                        //triangles[i].v1.p.z = 0;
                        p.z = triangles[i].v1.p.z;
                        return p;
                        //return triangles[i].project_point_xy(p);
                    }
                }
            }
            return new Point3d_GL(0, 0, 0);
        }
        static Point3d_GL norm_to_gl_xy(Point3d_GL p, double zoom)
        {
            p.x = (p.x - 0.5) * 2 * zoom;
            p.y = (0.5 - p.y) * 2 * zoom;
            return p;
        }
    }

}


