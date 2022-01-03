using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public struct openGlobj
    {
        public enum AnimType { Static, Dynamic }
        public float[] vertex_buffer_data;
        public float[] color_buffer_data;
        public float[] normal_buffer_data;
        public PrimitiveType tp;
        public AnimType animType;
        public int id;
        public bool visible;
        public openGlobj(float[] v_buf, float[] c_buf, float[] n_buf, PrimitiveType type)
        {
            vertex_buffer_data = new float[v_buf.Length];
            color_buffer_data = new float[c_buf.Length];
            normal_buffer_data = new float[n_buf.Length];
            v_buf.CopyTo(vertex_buffer_data, 0);
            c_buf.CopyTo(color_buffer_data, 0);
            n_buf.CopyTo(normal_buffer_data, 0);
            tp = type;
            animType = AnimType.Static;
            id = -1;
            visible = true;
        }
        public openGlobj(float[] v_buf, float[] c_buf, float[] n_buf, PrimitiveType type, int _id)
        {
            vertex_buffer_data = new float[v_buf.Length];
            color_buffer_data = new float[c_buf.Length];
            normal_buffer_data = new float[n_buf.Length];
            v_buf.CopyTo(vertex_buffer_data, 0);
            c_buf.CopyTo(color_buffer_data, 0);
            n_buf.CopyTo(normal_buffer_data, 0);
            tp = type;
            animType = AnimType.Dynamic;
            id = _id;
            visible = true;
        }
    }

}
