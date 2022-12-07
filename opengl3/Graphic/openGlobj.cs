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
        public float[] texture_buffer_data;
        public int vert_len;
        public PrimitiveType tp;
        public AnimType animType;
        public int id;
        public int Textureid;
        public bool visible;
        public float transparency;
        uint buff_array;

        public int count;



        public openGlobj(float[] v_buf, float[] c_buf, float[] n_buf, float[] t_buf=null, PrimitiveType type=PrimitiveType.Triangles, int _id = -1, int _count = 1, int textureId = -1)
        {
            buff_array = 0;
            vert_len = (int)v_buf.Length / 3;
            count = _count;
            transparency = 1f;

            if (v_buf.Length>1000000)
            {
                vertex_buffer_data = v_buf;
                normal_buffer_data = c_buf;
                color_buffer_data = null;
                texture_buffer_data = null;
            }
            else
            {
                vertex_buffer_data = new float[v_buf.Length];
                normal_buffer_data = new float[n_buf.Length];


                if (t_buf == null)
                {
                    texture_buffer_data = new float[v_buf.Length];
                }
                else
                {
                    texture_buffer_data = new float[t_buf.Length];
                    t_buf.CopyTo(texture_buffer_data, 0);
                }

                if (c_buf == null)
                {
                    color_buffer_data = new float[v_buf.Length];

                }
                else
                {
                    color_buffer_data = new float[c_buf.Length];
                    c_buf.CopyTo(color_buffer_data, 0);
                }
                // Console.WriteLine(color_buffer_data[0] + " " + color_buffer_data[1] + " " + color_buffer_data[2]);

                v_buf.CopyTo(vertex_buffer_data, 0);
                n_buf.CopyTo(normal_buffer_data, 0);
            }

            


            Textureid = textureId;
            tp = type;
            visible = true;
            id = _id;
            if (_id == -1)
            {
                //animType = AnimType.Static;
                animType = AnimType.Dynamic;
                setBuffers();
            }
            else
            {
                animType = AnimType.Dynamic;
                setBuffers();
            }

        }

        public openGlobj setBuffers()
        {
            buff_array = Gl.GenVertexArray();
            Gl.BindVertexArray(buff_array);
            bindBuffer(vertex_buffer_data, 0, 3);
            bindBuffer(normal_buffer_data, 1, 3);
            if(color_buffer_data!=null) bindBuffer(color_buffer_data, 2, 3);
            if (texture_buffer_data != null) bindBuffer(texture_buffer_data, 3, 2);
            return this;
        }

        public void useBuffers()
        {
            Gl.BindVertexArray(buff_array);
        }

        uint bindBuffer(float[] data, uint lvl, int strip)
        {
            
            var buff = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4 * data.Length), data, BufferUsage.StaticDraw);
            Gl.EnableVertexAttribArray(lvl);
            Gl.VertexAttribPointer(lvl, strip, VertexAttribType.Float, false, 0, (IntPtr)0);
            return buff;
        }


    }

}
