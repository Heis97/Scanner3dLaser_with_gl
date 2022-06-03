using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public struct STLmodel
    {
        public string path;
        public STLmodel(string _path)
        {
            path = _path;
        }
        public float[] parsingTxt_Tab(string path)
        {
            
            var text = File.ReadAllText(path);
            var lines = text.Split('\n');
            var mesh = new float[(lines.Length - 2) * 3];
            var ind = 0;
            for(int i=1; i<lines.Length-1;i++)
            {
                var p = lines[i].Split('\t');
                mesh[ind] = (float)parseE(p[0]); ind++;
                mesh[ind] = (float)parseE(p[1])-30; ind++;
                mesh[ind] = (float)parseE(p[2]); ind++;

            }
            for(int i=0; i<41-1;i++)
            {
                for (int j = 0; j < 91-1; j++)
                {
                    //Console.WriteLine(mesh[3 * (i * 91 + j+1) ] - mesh[3 * (i * 91 + j) ]);
                    //mesh[3*(i * 91 + j) +2] += j;    
                    
                }
            }
            var text1 = text.Replace('\t', ';');
           // Console.WriteLine("Len = "+((double)lines.Length-2)/91);
            return mesh;
        }

        public double parseE(string num)
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
        public float[] parsingStl_GL4(string path)
        {
            // var offx = 200;
            // var offy = 500;
            //  var offz = 600;
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
                    }

                }
            }
            return ret1;
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

       public static void saveMesh(float[] mesh, string name)
        {
            var sb = new StringBuilder();
            //string text = "solid\n";
            sb.Append("solid\n");
            for (int i=0; i<mesh.Length;i+=9)
            {
                var text1 = "";
                var p1 = new Point3d_GL(mesh[i], mesh[i + 1], mesh[i + 2]);
                var p2 = new Point3d_GL(mesh[i + 3], mesh[i + 4], mesh[i + 5]);
                var p3 = new Point3d_GL(mesh[i + 6], mesh[i + 7], mesh[i + 8]);
                var U = p1 - p2;
                var V = p1 - p3;
                var Norm = new Point3d_GL(
                    U.y * V.z - U.z * V.y,
                    U.z * V.x - U.x * V.z,
                    U.x * V.y - U.y * V.x);
                var Norm1 = Norm.normalize();
                text1 += "facet normal " + Norm1.x + " " + Norm1.y + " " + Norm1.z + "\n";
                text1 += "outer loop\n";
                text1 += "vertex " + mesh[i] + " " + mesh[i + 1] + " " + mesh[i + 2] + "\n";
                text1 += "vertex " + mesh[i+3] + " " + mesh[i + 4] + " " + mesh[i + 5] + "\n";
                text1 += "vertex " + mesh[i+6] + " " + mesh[i + 7] + " " + mesh[i + 8] + "\n";
                text1 += "endloop\n";
                text1 += "endfacet \n";
                //text += text1;
                sb.Append(text1);

               
            }

            sb.Append("endsolid\n");

            Console.WriteLine("startWRITE");
            var wr = new StreamWriter(name + ".stl");
            wr.Write(sb);
            wr.Close();
            
        }
    }

}
