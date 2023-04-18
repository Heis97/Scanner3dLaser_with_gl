using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Emgu.CV;
using System.Drawing;
using System.IO;

namespace opengl3
{
    public static class Settings_loader
    {
        

        public static string matrix_save(Matrix<double> matrix)
        {
            var txt = matrix.Size.Width + " " + matrix.Size.Height + " ";
            for (int j = 0; j < matrix.Size.Height; j++)
            {
                for (int i = 0; i < matrix.Size.Width; i++)
                {
                    txt += matrix[j, i] + " ";
                }
            }
            txt += "Matrix";
            return txt;
        }

        public static Matrix<double> matrix_load(string matrix_txt)
        {
            var subline = matrix_txt.Trim().Split(' ');
            var w = Convert.ToInt32(subline[0]);
            var h = Convert.ToInt32(subline[1]);
            int k = 2;
            var matrix = new Matrix<double>(w, h);
            for (int j = 0; j < matrix.Size.Height; j++)
            {
                for (int i = 0; i < matrix.Size.Width; i++)
                {
                    matrix[j, i] = Convert.ToDouble(subline[k]); k++;
                }
            }
            return matrix;
        }

        public static string size_save(Size size)
        {
            var txt = size.Width + " " + size.Height + " "+"Size";
            return txt;
        }

        public static Size size_load(string size_txt)
        {
            var subline = size_txt.Trim().Split(' ');
            var w = Convert.ToInt32(subline[0]);
            var h = Convert.ToInt32(subline[1]);
            return new Size(w, h);
        }
        public static string double_save(double val)
        {
            var txt = val + " " + "double";
            return txt;
        }

        public static double double_load (string size_txt)
        {
            var subline = size_txt.Trim().Split(' ');
            var v = Convert.ToDouble(subline[0]);
            return v;
        }

        public static string flat3d_gl_save(Flat3d_GL flat)
        {
            var txt = flat.A + " " + flat.B + " " + flat.C + " " + flat.D + " " + "Flat3d_GL";
            return txt;
        }

        public static Flat3d_GL flat3d_gl_load(string size_txt)
        {
            var subline = size_txt.Trim().Split(' ');
            var A = Convert.ToDouble(subline[0]);
            var B = Convert.ToDouble(subline[1]);
            var C = Convert.ToDouble(subline[2]);
            var D = Convert.ToDouble(subline[3]);
            return new Flat3d_GL(A,B,C,D);
        }

        public static string save_data(object obj)
        {
            if (obj is Size size) return size_save(size);            
            else if(obj is Matrix<double> matrix) return matrix_save(matrix);            
            else if (obj is Flat3d_GL flat) return flat3d_gl_save(flat);
            else if (obj is double v) return double_save(v);
            else return null;            
        }

        public static object[] load_data(string path)
        {
            string file;
            using (StreamReader sr = new StreamReader(path))
            {
                file = sr.ReadToEnd();
            }
            string[] lines = file.Split(new char[] { '\n' });
            var objs = new List<object>();

            foreach(var l in lines)
            {
                var subline = l.Trim().Split(' ');
                switch (subline[subline.Length - 1])
                {
                    case "Size": objs.Add(size_load(l)); break;
                    case "Matrix": objs.Add(matrix_load(l)); break;
                    case "Flat3d_GL": objs.Add(flat3d_gl_load(l)); break;
                    case "double": objs.Add(double_load(l)); break;
                    default: break;

                }
            }
            return objs.ToArray();
        }

        public static void save_file(string path, object[] data)
        {
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
            {
                foreach(var d in data)
                {
                    sw.WriteLine(save_data(d));
                }
                
            }
        }
       

    }
}
