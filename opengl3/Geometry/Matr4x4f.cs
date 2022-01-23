using Emgu.CV;
using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public struct Matr4x4f
    {
        public float[] data;
        public Matr4x4f(float[] _data)
        {
            data = _data;
        }
        public Matr4x4f(Matrix4x4f matrix)
        {
            data = new float[16];
            for(int i=0; i<4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    data[i * 4 + j] = matrix[(uint)i, (uint)j];
                }
            }
        }
        public Matr4x4f Transpose()
        {
            float[] res = new float[16];
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    
                    res[i * 4 + j] += data[j * 4 + i];
                    
                }
            }
            return new Matr4x4f(res);
        }
        public static Matr4x4f operator *(Matr4x4f matr1, Matr4x4f matr2)
        {
            float[] res = new float[16];
            var m1 = matr1.data;
            var m2 = matr2.data;
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {
                    for (int k = 0; k < 4; ++k)
                    {
                        res[i * 4 + j] += m1[i * 4 + k] * m2[k * 4 + j];
                    }
                }
            }
            return new Matr4x4f(res);
        }

        public Matrix<double> ToOpenCVMatr()
        {
            var matr = new Matrix<double>(4, 4);
            for (int i = 0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    matr[i,j] = data[4*i+j];
                }
            }
            return matr;
        }
    }
    public struct Vert4f
    {
        public float[] data;
        public Vert4f(float[] _data)
        {
            data = _data;
        }
        public Vert4f(float x, float y, float z)
        {
            data = new float[] { x, y, z, 1 };
        }
        public PointF getPoint()
        {
            if(data[3]!=0)
            {
                var x = data[0] / data[3];
                var y = data[1] / data[3];
                return new PointF(x, y);
            }
            return null;
        }
        public void Norm()
        {
            if (data[3] != 0)
            {
                data[0] /= data[3];
                data[1] /= data[3];
                data[2] /= data[3];
                data[3] = 1;
            }
        }
        static public  Vert4f operator *(Matr4x4f matr1, Vert4f matr2)
        {
            float[] res = new float[4];
            var m1 = matr1.data;
            var m2 = matr2.data;
            for (int i = 0; i < 4; ++i)
            {
                for (int j = 0; j < 4; ++j)
                {                    
                    res[i] += m1[i * 4 + j] * m2[j];                    
                }
            }
            return new Vert4f(res);
        }
        static public Vert4f operator /(Vert4f ver1, float d)
        {
            for (int i = 0; i < 4; ++i)
            {
                ver1.data[i] /= d;
            }
            return ver1;
        }
        static public Vert4f operator *(Vert4f ver1, float d)
        {
            for (int i = 0; i < 4; ++i)
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
