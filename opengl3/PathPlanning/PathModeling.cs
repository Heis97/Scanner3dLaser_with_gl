using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using Emgu.CV.Util;
using opengl3;

namespace PathPlanning
{
    static class PathModeling
    {
        static public Polygon3d_GL[] modeling_print_path(Polygon3d_GL[] surf, TrajectoryPath path, TrajParams trajParams,GraphicGL graphicGL = null)
        {
             path = path.fill_gaps(trajParams.div_step);
            path = path.gauss(4);
            var path_c = comp_whv(path,trajParams);
            var path_g = modeling_gravity(path_c,surf, graphicGL);
            var model = path_to_model(path_g.to_ps());
           
            return model;
        }

        static public Polygon3d_GL[] modeling_print_path_3d(Polygon3d_GL[] surf, TrajectoryPath path, TrajParams trajParams, GraphicGL graphicGL = null )
        {
            path = path.fill_gaps(trajParams.div_step);
            path = path.gauss(2);
            //graphicGL.remove_buff_gl_id("prog");
            //graphicGL.addLineMeshTraj(path.to_ps().ToArray(), Color3d_GL.red(), "prog");
            var path_c = comp_whv(path, trajParams);
            TrajectoryPath path_g;
            List<List<Point3d_GL>> ns;
            (path_g,ns) = modeling_gravity_3d(path_c, surf, graphicGL);
            //graphicGL.remove_buff_gl_id("prog_grav");
            //graphicGL.addLineMeshTraj(path_g.to_ps().ToArray(), Color3d_GL.purple(), "prog_grav");
            var model = path_to_model_3d(path_g.to_ps(),ns);


            return model;
        }
        static public TrajectoryPath comp_whv(TrajectoryPath path,TrajParams trajParams)
        {
            for(int i = 0; i < path.layers.Count; i++)
            {
                for(int j = 0; j < path.layers[i].lines.Count; j++)
                {
                    for (int k = 0; k < path.layers[i].lines[j].ps.Count; k++)
                    {
                        var h = trajParams.dz;
                        var w = trajParams.line_width;
                        var color = comp_whv_from_rect(w, h);
                        path.layers[i].lines[j].ps[k] =  path.layers[i].lines[j].ps[k].set_color_this(color);

                        //Console.WriteLine(path.layers[i].lines[j].ps[k].color.ToString());
                    }
                }
            }

            return path;
        }

        static Color3d_GL comp_whv_from_rect(double w, double h)
        {
            var v = h * w;
            if (h < 0.00001) h = 0.00001;
            var w_sh = (w - (Math.PI * h) / 4);
            w = w_sh + h;
            v = (w - h + Math.PI * h / 4) * h;
            return new Color3d_GL(w, h, v);
        }
        static Color3d_GL comp_whv_from_norm(double v, double h_new)
        {

            if (h_new < 0.00001) h_new = 0.00001;
            var w = v/h_new + h_new - Math.PI*h_new / 4;
            v = (w - h_new + Math.PI * h_new / 4) * h_new;
            return new Color3d_GL(w, h_new, v);
        }
        static public TrajectoryPath modeling_gravity(TrajectoryPath path, Polygon3d_GL[] surf, GraphicGL graphicGL = null)
        {
            var path_g = new List<LayerPath>();
            var layers = path.layers;
            var surf_o = surf;
            for(int i = 0; i < layers.Count; i++)
            {
                if(i!=0) surf_o = surf_from_layer(layers[i-1]);
               // graphicGL?.addMesh(Polygon3d_GL.toMesh(surf_o)[0], OpenGL.PrimitiveType.Triangles);
                var path_o = modeling_gravity_one(layers[i], surf_o, graphicGL);
                path_g.Add(path_o);

            }
            return new TrajectoryPath { layers = path_g };
        }

        static public (TrajectoryPath, List<List<Point3d_GL>>)  modeling_gravity_3d(TrajectoryPath path, Polygon3d_GL[] surf, GraphicGL graphicGL = null)
        {
            var nss = new List<List<Point3d_GL>>();
            var path_g = new List<LayerPath>();
            var layers = path.layers;
            var surf_o = surf;
            for (int i = 0; i < layers.Count; i++)
            {
                if (i != 0) surf_o = surf_from_layer(layers[i - 1]);
                surf_o = SurfaceReconstraction.expand_surf_convex(surf_o, 20);
                //graphicGL?.addMesh(Polygon3d_GL.toMesh(surf_o)[0], OpenGL.PrimitiveType.Triangles);
                List<Point3d_GL> ns;
                LayerPath path_o;
                (path_o,ns) = modeling_gravity_one_3d(layers[i], surf_o, graphicGL);
                path_g.Add(path_o);
                nss.Add(ns);
            }
            return (new TrajectoryPath { layers = path_g }, nss);
        }
        static public LayerPath modeling_gravity_one(LayerPath layer, Polygon3d_GL[] surf, GraphicGL graphicGL = null)
        {
            var map_xy = new RasterMap(surf,1);
            for (int i=0; i< layer.lines.Count;i++)
            {
                for(int j=0; j< layer.lines[i].ps.Count;j++)
                {
                    var p = layer.lines[i].ps[j];
                    var p_proj = map_xy.proj_point_xy(p, surf);
                    if(p_proj.exist)
                    {
                        layer.lines[i].ps[j] = comp_whv_2p(p, p_proj);

                       // Console.WriteLine(layer.lines[i].ps[j].color.ToString());
                    }
                    else
                    {

                    }
                }
            }
            return layer;
        }

        static public (LayerPath,List<Point3d_GL>) modeling_gravity_one_3d(LayerPath layer, Polygon3d_GL[] surf, GraphicGL graphicGL = null)
        {
            var ns = new List<Point3d_GL>();
            var map_xy = new RasterMap(surf, 1);
            for (int i = 0; i < layer.lines.Count; i++)
            {
                for (int j = 0; j < layer.lines[i].ps.Count; j++)
                {
                    var p = layer.lines[i].ps[j];
                    Point3d_GL p_proj, n;
                    (p_proj,n) = map_xy.proj_point_xy_ex(p, surf);
                    if (n.z < 0) n = -n;
                    ns.Add(n);
                    if (p_proj.exist)
                    {
                        layer.lines[i].ps[j] = comp_whv_2p(p, p_proj);
                    }
                    else{}
                }
            }
            return (layer,ns);
        }


        static Point3d_GL comp_whv_2p(Point3d_GL p, Point3d_GL p_proj)//!!!!MOST IMPORTANT GRAVITY
        {
            var h = p.z - p_proj.z;
            var h_max = comp_h_max(p.color);
           // h_max = 
            var h_min = 0.2*(double)p.color.g;
            if(h>h_max)
            {
                h = h_max;
            }
            else if(h<h_min)
            {
                h = h_min;
            }
            p.z = p_proj.z + h;
            
            p.color = comp_whv_from_norm(p.color.b, h);
            return p;
        }
        static double comp_h_max(Color3d_GL whv)
        {
            var v = whv.b;
            var k_mech = 0.95;

            return whv.g;
           // Console.WriteLine()
            //return 2 * Math.Sqrt((k_mech * v) / Math.PI);
        }
        static public Polygon3d_GL[] surf_from_layer(LayerPath layer)
        {
            var ps = Point3d_GL.to_arr( layer.to_ps_by_lines_main_only());
            var pols = Polygon3d_GL.triangulate_lines_xy(ps);
            return pols;
        }

        static public Polygon3d_GL[] path_to_model(List<Point3d_GL> path)
        {
            var matrs = comp_matr_from_path(path);
            var sections = gen_sections(path, matrs);
            var model = triangulate_sections(sections);
            return model;
        }

        static public Polygon3d_GL[] path_to_model_3d(List<Point3d_GL>  path,List<List<Point3d_GL>> ns)
        {
            var path_n = PathPlanner.join_traj(ns);
            //Console.WriteLine("Count: "+path_n.Count + " " + path.Count);
            var matrs = comp_matr_from_path_3d(path, path_n);
            var sections = gen_sections(path, matrs);
            var model = triangulate_sections(sections);
            return model;
        }
        static public List<Matrix<double>> comp_matr_from_path(List<Point3d_GL> path)
        {
            var ms = new List<Matrix<double>>();
            for (int i = 0; i < path.Count;i++)
            {
                var i_prev = i - 1; if (i_prev < 0) i_prev = 0;
                var i_next = i + 1; if (i_next > path.Count - 1) i_next = path.Count - 1;

                var vec_x = path[i_next] - path[i_prev];
                vec_x.normalize();
                var vec_z = new Point3d_GL(0, 0, 1);
                var vec_y = (vec_z | vec_x).normalize();


                var m = new Matrix<double>(new double[,]
                {
                  { vec_x.x,vec_y.x,vec_x.x,path[i].x},
                  { vec_x.y,vec_y.y,vec_z.y,path[i].y},
                  { vec_x.z,vec_y.z,vec_z.z,path[i].z},
                  { 0,0,0,1}
                });

                
                ms.Add(m);
            }
            return ms;
        }
        static public List<Matrix<double>> comp_matr_from_path_3d(List<Point3d_GL> path, List<Point3d_GL> ns)
        {
            var ms = new List<Matrix<double>>();
            for (int i = 0; i < path.Count; i++)
            {
                var i_prev = i - 1; if (i_prev < 0) i_prev = 0;
                var i_next = i + 1; if (i_next > path.Count - 1) i_next = path.Count - 1;

                var vec_x = path[i_next] - path[i_prev];
                vec_x.normalize();
                var vec_z = ns[i];
                var vec_y = (vec_z | vec_x).normalize();


                var m = new Matrix<double>(new double[,]
                {
                  { vec_x.x,vec_y.x,vec_z.x,path[i].x},
                  { vec_x.y,vec_y.y,vec_z.y,path[i].y},
                  { vec_x.z,vec_y.z,vec_z.z,path[i].z},
                  { 0,0,0,1}
                });


                ms.Add(m);
            }
            return ms;
        }

        static public List<List<Point3d_GL>> gen_sections(List<Point3d_GL> path, List<Matrix<double>> matrs)
        {
            var sects = new List<List<Point3d_GL>>();
            for (int i = 0; i < path.Count;i++)
            {
                var w = path[i].color.r;
                var h = path[i].color.g;
                var sect = gen_section(w,h);
                sect = Point3d_GL.multMatr(sect.ToArray(), matrs[i]).ToList();
                sects.Add(sect);
            }
            return sects;
        }
        static public List<Point3d_GL> gen_section(double w, double h)
        {
            var pres = 5;

            var sect = new List<Point3d_GL>();
            var sect_mirr = new List<Point3d_GL>();
            var fi = - Math.PI / 2;
            var step = Math.PI / pres;
            for (int i = 0; i < pres; i++)
            {
                fi += step;
                var y = (h/2)* Math.Cos(fi) + w / 2;
                var z = (h / 2) * Math.Sin(fi) - h / 2;
                sect.Add(new Point3d_GL(0,y,z));
                sect_mirr.Add(new Point3d_GL(0, -y, z));
            }
            sect_mirr.Reverse();
            sect.AddRange(sect_mirr);
            return sect; ;
        }
        static public Polygon3d_GL[] triangulate_sections(List<List<Point3d_GL>> sections)
        {
            var pols = new List<Polygon3d_GL>();
            for(var i = 1; i < sections.Count;i++)
            {
                if (sections[i] == null || sections[i - 1] == null) continue;
                if (sections[i].Count!= sections[i - 1].Count) continue;

                var sect =  Polygon3d_GL.triangulate_two_same_conts(sections[i - 1].ToArray(), sections[i].ToArray());
                pols.AddRange(sect);
                /*for (var j = 0; j < sections[i].Count; j++)
                {
                    var j_prev = j - 1;
                    if (j_prev < 0) j_prev = sections[i].Count - 1;
                    var pol1 = new Polygon3d_GL(sections[i][j], sections[i][j_prev], sections[i-1][j_prev]);
                    var pol2 = new Polygon3d_GL(sections[i][j], sections[i-1][j_prev], sections[i - 1][j]);
                    pols.Add(pol1);
                    pols.Add(pol2);
                }*/
            }
            return pols.ToArray();
        }


    }

}
