using Emgu.CV;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public struct Matr3x3f
    {
        public float[] data;
        public Matr3x3f(float[] _data)
        {
            data = _data;
        }
        public Matr3x3f(Matrix3x3f matrix)
        {
            data = new float[9];
            for(int i=0; i<3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    data[i * 3 + j] = matrix[(uint)i, (uint)j];
                }
            }
        }
        public Matr3x3f(Matrix<double> matrix)
        {
            data = new float[9];
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    data[i * 3 + j] = (float)matrix[i, j];
                }
            }
        }
        public Matr3x3f Transpose()
        {
            float[] res = new float[9];
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    
                    res[i * 3 + j] += data[j * 3 + i];
                    
                }
            }
            return new Matr3x3f(res);
        }
        public static Matr3x3f operator *(Matr3x3f matr1, Matr3x3f matr2)
        {
            float[] res = new float[9];
            var m1 = matr1.data;
            var m2 = matr2.data;
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {
                    for (int k = 0; k < 3; ++k)
                    {
                        res[i * 3 + j] += m1[i * 3 + k] * m2[k * 3 + j];
                    }
                }
            }
            return new Matr3x3f(res);
        }
       
    }
    public struct Vert3f
    {
        public float[] data;
        public float x { get { return data[0]; } }
        public float y { get { return data[1]; } }
        public float z { get { return data[2]; } }
        public Vert3f Norm 
        {
            get
            {
                return new Vert3f(data[0] / data[2], data[1] / data[2], 1);
            }
        }

        public Vert3f(float[] _data)
        {
            data = _data;
        }
        public Vert3f(float x, float y, float z)
        {
            data = new float[] { x, y, z};
        }
        public PointF getPoint()
        {
            if(data[2]!=0)
            {
                var x = data[0] / data[2];
                var y = data[1] / data[2];
                return new PointF(x, y);
            }
            return null;
        }
        public void Normalyse()
        {
            if (data[2] != 0)
            {
                data[0] /= data[2];
                data[1] /= data[2];
                data[2] = 1;
            }
        }
        static public  Vert3f operator *(Matr3x3f matr1, Vert3f matr2)
        {
            float[] res = new float[3];
            var m1 = matr1.data;
            var m2 = matr2.data;
            for (int i = 0; i < 3; ++i)
            {
                for (int j = 0; j < 3; ++j)
                {                    
                    res[i] += m1[i * 3 + j] * m2[j];                    
                }
            }
            return new Vert3f(res);
        }
        static public Vert3f operator /(Vert3f ver1, float d)
        {
            for (int i = 0; i < 3; ++i)
            {
                ver1.data[i] /= d;
            }
            return ver1;
        }
        static public Vert3f operator *(Vert3f ver1, float d)
        {
            for (int i = 0; i < 3; ++i)
            {
                ver1.data[i] *= d;
            }
            return ver1;
        }
        public override  string ToString()
        {
            var str = "";
            for(int i=0; i<data.Length;i++)
            {
                str += Convert.ToString(data[i]) + "; ";
            }
            return str;
        }

    }
}
