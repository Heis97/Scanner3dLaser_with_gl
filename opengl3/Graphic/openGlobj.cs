using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{

    public class openGlobj
    {
        public float[] vertex_buffer_data { get; set; }
        public float[] color_buffer_data { get; set; }
        public float[] normal_buffer_data { get; set; }
        public float[] texture_buffer_data;
        public int vert_len { get; set; }
        public PrimitiveType tp { get; set; }
        public int Textureid;
        public bool visible { get; set; }
        public bool text_vis { get; set; }
        public bool light_vis { get; set; }
        public float transparency { get; set; }
        uint buff_array;

        public string name { get; set; }
        uint v_ubuf;
        uint n_ubuf;



        public trsc[] trsc { get; set; }
        public int count;
        Vertex4f cross_flat;
        public int comp_flat;

        public openGlobj(float[] v_buf, float[] c_buf, float[] n_buf, float[] t_buf=null, PrimitiveType type=PrimitiveType.Triangles,string name = "name", int _count = 1, int textureId = -1)
        {
            text_vis = false;
            light_vis = false;
            cross_flat = new Vertex4f();
            comp_flat = 0;
            buff_array = uint.MaxValue;
            v_ubuf = uint.MaxValue;
            n_ubuf = uint.MaxValue;
            vert_len = (int)v_buf.Length / 3;
            count = _count;
            transparency = 1f;
            trsc = new trsc[_count];
            this.name = name;
            for (int i=0;i<trsc.Length;i++)
            {
                trsc[i] = new trsc(Matrix4x4f.Identity);
            }
            if (v_buf.Length>6000000)
            {
                vertex_buffer_data = v_buf;
                normal_buffer_data = n_buf;
                color_buffer_data = c_buf;
                texture_buffer_data = t_buf;
            }
            else
            {
                vertex_buffer_data = new float[v_buf.Length];
                if (n_buf == null)
                {
                    normal_buffer_data = new float[v_buf.Length];
                }
                else
                {
                    normal_buffer_data = new float[n_buf.Length];

                }


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
                
                v_buf.CopyTo(vertex_buffer_data, 0);
                n_buf?.CopyTo(normal_buffer_data, 0);
            }
            //Console.WriteLine(vertex_buffer_data.Length + " " + normal_buffer_data.Length + " " + color_buffer_data.Length + " " + texture_buffer_data.Length);


            Textureid = textureId;
            tp = type;
            visible = true;

            setBuffers();

        }
        public openGlobj() { }
        public void set_vert_data(float[] v_buf, float[] n_buf)
        {
            if (n_buf == null) normal_buffer_data = new float[v_buf.Length];
            else normal_buffer_data = new float[n_buf.Length];
            vertex_buffer_data = new float[v_buf.Length];
            n_buf?.CopyTo(normal_buffer_data, 0);
            v_buf.CopyTo(vertex_buffer_data, 0);
            setBuffers();
        }
        public void setBuffers()
        {
            
            if (buff_array== uint.MaxValue) buff_array = Gl.GenVertexArray();
            Gl.BindVertexArray(buff_array);
            v_ubuf =  bindBuffer(vertex_buffer_data, 0, 3,v_ubuf);
            if (normal_buffer_data != null) n_ubuf = bindBuffer(normal_buffer_data, 1, 3, n_ubuf);
            if (color_buffer_data != null) bindBuffer(color_buffer_data, 2, 3);
            if (texture_buffer_data != null) bindBuffer(texture_buffer_data, 3, 2);
            //Console.WriteLine(buff_array + "set_buff " + v_ubuf);
        }

        public void useBuffers()
        {
            Gl.BindVertexArray(buff_array);
        }

        public void loadModels()
        {
            bindBufferInstanceMatr(modelData(), 4);
        }
        uint bindBuffer(float[] data, uint lvl, int strip, uint ubuf = uint.MaxValue)
        {
            uint buff = 0;
            if (ubuf == uint.MaxValue)
                buff = Gl.GenBuffer();
            else
                buff = ubuf;
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4 * data.Length), data, BufferUsage.StaticDraw);
            Gl.EnableVertexAttribArray(lvl);
            Gl.VertexAttribPointer(lvl, strip, VertexAttribType.Float, false, 0, (IntPtr)0);
            return buff;
        }
        Matrix4x4f[] modelData()
        {
            var matrs = new Matrix4x4f[trsc.Length];
            for (int i = 0; i < trsc.Length; i++)
            {
                matrs[i] = trsc[i].getModelMatrix();
            }
            return matrs;
        }
        uint bindBufferInstanceMatr(Matrix4x4f[] data, uint lvl)
        {


            var buff = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4 * 16 * data.Length), data, BufferUsage.DynamicDraw);

            Gl.EnableVertexAttribArray(lvl);
            Gl.VertexAttribPointer(lvl, 4, VertexAttribType.Float, false, 4 * 16, (IntPtr)0);

            Gl.EnableVertexAttribArray(lvl + 1);
            Gl.VertexAttribPointer(lvl + 1, 4, VertexAttribType.Float, false, 4 * 16, (IntPtr)(4 * 4));

            Gl.EnableVertexAttribArray(lvl + 2);
            Gl.VertexAttribPointer(lvl + 2, 4, VertexAttribType.Float, false, 4 * 16, (IntPtr)(4 * 8));

            Gl.EnableVertexAttribArray(lvl + 3);
            Gl.VertexAttribPointer(lvl + 3, 4, VertexAttribType.Float, false, 4 * 16, (IntPtr)(4 * 12));

            Gl.VertexAttribDivisor(lvl, 1);
            Gl.VertexAttribDivisor(lvl + 1, 1);
            Gl.VertexAttribDivisor(lvl + 2, 1);
            Gl.VertexAttribDivisor(lvl + 3, 1);

            return buff;
        }


        #region setters
        public openGlobj setType(PrimitiveType ptype)
        {
            tp = ptype;
            return this;
        }

        public openGlobj setName(string name)
        {
            this.name = name;
            return this;
        }
        public openGlobj setVisible(bool _visible)
        {
            visible = _visible;
            return this;
        }
        public openGlobj setScale(int i, float _scale)
        {
            trsc[i].scale = _scale;
            return this;
        }

        public openGlobj setTransf(int i, trsc _trsc)
        {
            trsc[i] = _trsc;
            return this;
        }

        public openGlobj setTransf(int i, Point3d_GL transl, Point3d_GL rotate)
        {
            trsc[i] = new trsc(transl, rotate, trsc[i].scale);
            return this;
        }
        public openGlobj setX(int i, double x)
        {
            trsc[i].transl.setx(x);
            return this;
        }
        public openGlobj setY(int i, double y)
        {
            trsc[i].transl.sety(y);
            return this;
        }
        public openGlobj setZ(int i, double z)
        {
            trsc[i].transl.setz(z);
            return this;
        }
        public openGlobj setRotX(int i, double x)
        {
            trsc[i].rotate.setx(x);
            return this;
        }
        public openGlobj setRotY(int i, double y)
        {
            trsc[i].rotate.sety(y);
            return this;
        }
        public openGlobj setRotZ(int i, double z)
        {
            trsc[i].rotate.setz(z);
            return this;
        }

        public openGlobj setlight_vis(int i,bool vis)
        {
            light_vis = vis;
            return this;
        }

        public openGlobj settext_vis(int i, bool vis)
        {
            text_vis = vis;
            return this;
        }

        public openGlobj setMatr(int i, Matrix4x4f matr)
        {
            trsc[i].matr = matr;
            return this;
        }
        public openGlobj addMatr(int i, Matrix4x4f matr)
        {
            trsc[i].matr = matr * trsc[i].matr;
            return this;
        }
        public openGlobj setTrasp(float trans)
        {
            transparency = trans;
            return this;
        }
        public openGlobj setComp_flat(int comp)
        {
            comp_flat = comp;
            return this;
        }

        public openGlobj crossFlat(Vertex4f flat)
        {
            cross_flat = flat;
            comp_flat = 1;
            return this;
        }

        #endregion


    }

}
