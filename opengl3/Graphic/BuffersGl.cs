using OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace opengl3
{
    public class Buffers
    {
        public int countObj = 0;
        public Dictionary<string, openGlobj> objs;

        public Buffers()
        {
            objs = new Dictionary<string, openGlobj>();
        }
        public string add_obj(openGlobj opgl_obj, string name)
        {
            string name_obj =gen_name(name);
            opgl_obj = opgl_obj.setName(name_obj);
            objs.Add(opgl_obj.name, opgl_obj);
            sort_by_transp();
            return name_obj;
        }

        string gen_name(string name)
        {
            string name_obj = name;
            int i = 0;
            while (objs.ContainsKey(name_obj))
            {
                name_obj = name.Trim() + "_" + i;
                i++;
            }

            return name_obj;
        }

        public void removeObj(string name)
        {
            if(objs.ContainsKey(name) )
            {
                objs.Remove(name);
            }    
               
        }

        public void setObj(string name, openGlobj openGlobj)
        {
            objs[name] = openGlobj;
        }

        public void setObjVdata(string name, float[] v_data, float[] n_data)
        {
            objs[name].set_vert_data(v_data, n_data);
        }



        public void clearObj()
        {
            objs = new Dictionary<string, openGlobj>();
        }


        public void sort_by_transp()
        {
            var obj_sort = objs.OrderByDescending(pair => pair.Value.transparency).ToDictionary(pair => pair.Key, pair => pair.Value);
            objs = obj_sort;
        }

        #region setters

        public void setPrType(string name, PrimitiveType ptype)
        {
            objs[name] = objs[name].setType(ptype);
        }
        public void setScale(string name, int i, float _scale)
        {
            objs[name] = objs[name].setScale(i, _scale);
        }
        public void setTransfObj(string name, int i, Point3d_GL _transl, Point3d_GL _rotate)
        {
            objs[name] = objs[name].setTransf(i, _transl, _rotate);
        }
        public void setXobj(string name, int i, double x)
        {
            objs[name] = objs[name].setX(i, x);
        }
        public void setYobj(string name, int i, double y)
        {
            objs[name] = objs[name].setY(i, y);
        }
        public void setZobj(string name, int i, double z)
        {
            objs[name] = objs[name].setZ(i, z);
        }

        public void setRotXobj(string name, int i, double x)
        {
            objs[name] = objs[name].setRotX(i, x);
        }
        public void setRotYobj(string name, int i, double y)
        {
            objs[name] = objs[name].setRotY(i, y);
        }
        public void setRotZobj(string name, int i, double z)
        {
            objs[name] = objs[name].setRotZ(i, z);
        }
        public void setMatrobj(string name, int i, Matrix4x4f matr)
        {
            if (objs.ContainsKey(name))
                objs[name] = objs[name].setMatr(i, matr);
        }
        public void addMatrobj(string name, int i, Matrix4x4f matr)
        {
            if (objs.ContainsKey(name))
                objs[name] = objs[name].addMatr(i, matr);
        }
        public void setTranspobj(string name, float transp)
        {
            if (objs.ContainsKey(name))
                objs[name] = objs[name].setTrasp(transp);
            sort_by_transp();
        }
        public void setVisibleobj(string name, bool visible)
        {
            objs[name] = objs[name].setVisible(visible);
        }
        public void set_cross_flat_obj(string name, Vertex4f flat)
        {
            objs[name] = objs[name].crossFlat(flat);
        }
        public void set_comp_flat(string name, int comp)
        {
            objs[name] = objs[name].setComp_flat(comp);
        }
        #endregion

    }
   


}
