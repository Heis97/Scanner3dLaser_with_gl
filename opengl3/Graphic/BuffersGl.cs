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

        List<openGlobj> objs;
        public List<openGlobj> objs_out;
        public BuffersGl()
        {
            objs = new List<openGlobj>();
            objs_out = new List<openGlobj>();
        }
        public void add_obj(openGlobj opgl_obj)
        {
            objs.Add(opgl_obj);
            if (opgl_obj.animType == openGlobj.AnimType.Dynamic)
            {
                objs_out.Add(opgl_obj);
            }
        }

        public void add_obj_id(float[] data_v, int id, bool visible, PrimitiveType primitiveType)
        {
            int ind = 0;
            if(data_v ==null)
            {
                return;
            }
            if (objs.Count != 0)
            {
                foreach (var ob in objs_out)
                {
                    if (ob.id == id)
                    {
                        var lam_obj = ob;
                        if (visible)
                        {
                            lam_obj.visible = true;
                            var data_n_ = new float[data_v.Length];
                            var data_c_ = new float[data_v.Length];
                            for (int i = 0; i < data_v.Length; i++)
                            {
                                data_c_[i] = 1f;
                                data_n_[i] = 1f;
                            }
                            lam_obj.vertex_buffer_data = data_v;
                            lam_obj.color_buffer_data = data_c_;
                            lam_obj.normal_buffer_data = data_n_;
                            objs_out[ind] = lam_obj;
                            return;
                        }
                        else
                        {
                            lam_obj.visible = false;
                            objs_out[ind] = lam_obj;
                            return;
                        }

                    }
                    ind++;
                }

            }
            var data_n = new float[data_v.Length];
            var data_c = new float[data_v.Length];
            for (int i = 0; i < data_v.Length; i++)
            {
                data_c[i] = 1f;
                data_n[i] = 1f;
            }
            //Console.WriteLine("new ver " + id+" all "+ind);
            add_obj(new openGlobj(data_v, data_c, data_n, primitiveType, id));
        }
        public void sortObj()
        {
            objs_out = new List<openGlobj>();
            foreach (PrimitiveType val_tp in Enum.GetValues(typeof(PrimitiveType)))
            {
                var vertex_buffer_data = new List<float>();
                var color_buffer_data = new List<float>();
                var normal_buffer_data = new List<float>();

                for (int i = 0; i < objs.Count; i++)
                {
                    if (objs[i].tp == val_tp && objs[i].animType == openGlobj.AnimType.Static)
                    {
                        vertex_buffer_data.AddRange(objs[i].vertex_buffer_data);
                        color_buffer_data.AddRange(objs[i].color_buffer_data);
                        normal_buffer_data.AddRange(objs[i].normal_buffer_data);
                    }
                    if (objs[i].animType == openGlobj.AnimType.Dynamic)
                    {
                        objs_out.Add(objs[i]);
                    }
                }

                if (vertex_buffer_data.Count > 2)
                {

                    objs_out.Add(new openGlobj(vertex_buffer_data.ToArray(), color_buffer_data.ToArray(), normal_buffer_data.ToArray(), val_tp));
                }

            }
        }
        public void removeObj(int id)
        {
            var objs_lam = new openGlobj[objs.Count];
            objs.CopyTo(objs_lam);
            objs = new List<openGlobj>();
            objs_out = new List<openGlobj>();

            foreach (var ob in objs_lam)
            {
                if (ob.id != id)
                {
                    objs.Add(ob);
                }
            }
            sortObj();
        }
       

    }


}
