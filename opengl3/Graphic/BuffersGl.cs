using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace opengl3
{

    public class BuffersGl
    {
        public int countObj = 0;
        List<openGlobj> objs;
        public List<openGlobj> objs_dynamic;
        public List<openGlobj> objs_static;
        public BuffersGl()
        {
            objs = new List<openGlobj>();
            objs_dynamic = new List<openGlobj>();
            objs_static = new List<openGlobj>();
        }
        public int add_obj(openGlobj opgl_obj)
        {
            if (opgl_obj.animType == openGlobj.AnimType.Dynamic)
            {
                objs_dynamic.Add(opgl_obj);
                countObj++;
                return countObj - 1;
            }
            else
            {
                objs.Add(opgl_obj);

                return -1;
            }
        }
        public void sortObj()
        {
            objs_static = new List<openGlobj>();
            foreach (PrimitiveType val_tp in Enum.GetValues(typeof(PrimitiveType)))
            {
                var vertex_buffer_data = new List<float>();
                var color_buffer_data = new List<float>();
                var normal_buffer_data = new List<float>();
                var texture_buffer_data = new List<float>();

                for (int i = 0; i < objs.Count; i++)
                {
                    if (objs[i].tp == val_tp && objs[i].animType == openGlobj.AnimType.Static)
                    {
                        vertex_buffer_data.AddRange(objs[i].vertex_buffer_data);
                        color_buffer_data.AddRange(objs[i].color_buffer_data);
                        normal_buffer_data.AddRange(objs[i].normal_buffer_data);
                        texture_buffer_data.AddRange(objs[i].texture_buffer_data);

                    }
                }

                if (vertex_buffer_data.Count > 2)
                {

                    objs_static.Add(new openGlobj(vertex_buffer_data.ToArray(), color_buffer_data.ToArray(), normal_buffer_data.ToArray(), texture_buffer_data.ToArray(), val_tp));
                }

            }
        }

        public void removeObj(int id)
        {
            objs_dynamic[id] = new openGlobj();
        }

        public void clearObj()
        {
            objs = new List<openGlobj>();
            objs_dynamic = new List<openGlobj>();
            objs_static = new List<openGlobj>();
            countObj = 0;
        }

    }


}
