using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.UI;
using Emgu.CV.Structure;
using Emgu.Util;


namespace opengl3
{   
    public enum modeGL { Paint, View}
    public enum viewType { Perspective, Ortho }

    public class TextureGL
    {
        public uint id;
        public int binding;
        public int w, h, ch;
        public PixelFormat pixelFormat;
        InternalFormat internalFormat;
        public float[] data;
        public TextureGL()
        {
        }
        public TextureGL(int _binding, int _w, int _h = 1, PixelFormat _pixelFormat = PixelFormat.Red, float[] _data = null)
        {
            ch = ch_from_format(_pixelFormat);
            
            binding = _binding;
            w = _w;
            h = _h;
            pixelFormat = _pixelFormat;
            //Console.WriteLine("genTexture");
            //Console.WriteLine("st_genTexture bind " + _binding + "; w " + _w + " h " + _h + " ch " + ch + "; " + _pixelFormat);
            if (_data != null)
            {
                data = new float[w * h * ch];
                _data.CopyTo(data, 0);
                //data = (float[])_data.Clone();

            }
            else
            {
                data = null;
            }
            var buff = genTexture(_binding, _w, _h, _pixelFormat, data);
            id = buff;
            //Console.WriteLine("genTexture bind " + binding + "; w " + w + " h " + h + " ch " + ch + "; " + pixelFormat);
        }
        public float[] getData()
        {
            Gl.BindTexture(TextureTarget.Texture2d, id);
            float[] dataf = new float[w * h * ch];
            //Console.WriteLine("get text");
            Gl.GetTexImage(TextureTarget.Texture2d, 0, pixelFormat, PixelType.Float, dataf);
            //Console.WriteLine(w+" "+h+" "+ch+" "+ dataf.Length);
            return dataf;
        }
        public void setData(float[] data)
        {
            Gl.BindTexture(TextureTarget.Texture2d, id);
            Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, w, h, 0, pixelFormat, PixelType.Float, data);
        }
        static int  ch_from_format(PixelFormat pixelFormat)
        {
            if (pixelFormat == PixelFormat.Red)
            {
                return 1;
            }
            else if (pixelFormat == PixelFormat.Rg)
            {
                return 2;
            }
            else if (pixelFormat == PixelFormat.Rgb)
            {
                return 3;
            }
            else if (pixelFormat == PixelFormat.Rgba)
            {
                return 4;
            }
            else
            {
                return 1;
            }
        }
        uint genTexture(int binding, int w, int h = 1, PixelFormat pixelFormat = PixelFormat.Red, float[] data = null)
        {
            ch = ch_from_format(pixelFormat);
            var buff_texture = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0 + binding);
            Gl.BindTexture(TextureTarget.Texture2d, buff_texture);

            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMagFilter, Gl.NEAREST);
            Gl.TexParameteri(TextureTarget.Texture2d, TextureParameterName.TextureMinFilter, Gl.NEAREST);
            internalFormat = InternalFormat.R32f;
            if (pixelFormat == PixelFormat.Rgb || pixelFormat == PixelFormat.Rgba)
            {
                internalFormat = InternalFormat.Rgba32f;
            }

            var len = data?.Length ?? 0;
            //Console.WriteLine("genTexture bind " + binding + "; w " + w + " h " + h + " ch " + ch + "; " + pixelFormat+" "+ internalFormat+" data.len "+len+" from "+ w * h * 4);
           
            Gl.TexImage2D(TextureTarget.Texture2d, 0, internalFormat, w, h, 0, pixelFormat, PixelType.Float, data);

            Gl.BindImageTexture((uint)binding, buff_texture, 0, false, 0, BufferAccess.ReadWrite, internalFormat);
            return buff_texture;
        }
        

    }
    public class IDs
    {
        public uint buff_tex;
        public uint buff_tex1;
        public uint programID;
        public uint vert;
        public int[] LocationVPs = new int[4];
        public int[] LocationVs = new int[4];
        public int[] LocationPs = new int[4];
        public int LocationM;
        public int LightID;
        public int textureVisID;
        public int lightVisID;
        public int LightPowerID;
        public int MaterialDiffuseID;
        public int MaterialAmbientID;
        public int currentMonitor = 1;
        public int MaterialSpecularID;
        public int TextureID;
        public int MouseLocID;
        public int MouseLocGLID;
        public int translMeshID;
        public int comp_proj_ID;
        public int render_count_ID;
        public int show_faces_ID;
        public int transparency_ID;
        public int selected_ID;
        public int inv_norm_ID;
        public int[] surfs_cross_ID = new int[30];
        public int surfs_len_ID;
        public int surf_crossID;
    }
    public class GraphicGL
    {
        #region vars
        

        public int surfs_len = 0;
        public int inv_norm = 0;
        public int show_faces = 0;


        IDs idsPs = new IDs();
        IDs idsLs = new IDs();
        IDs idsTs = new IDs();
        IDs idsTsOne = new IDs();
        IDs idsTsOneSlice = new IDs();
        IDs idsPsOne = new IDs();
        IDs idsLsOne = new IDs();
        IDs idsCs = new IDs();
        IDs idsCsSlice = new IDs();
        Vertex4f surf_cross = new Vertex4f();
        public int point_type = 0;
        static float PI = 3.1415926535f;
        public CameraCV cameraCV;
        public int startGen = 0;
        public int saveImagesLen = 0;
        public int renderdelim = 15;
        public int rendercout = 0;
        public viewType typeProj = viewType.Perspective;
        Size sizeControl;
        Point lastPos;

        uint programID_comp;
        public int texture_vis = 0;

        int currentMonitor = 0;
        float LightPower = 1000000.0f;
        Label Label_cor;
        Label Label_cor_cur;
        Label Label_trz_cur;
        RichTextBox debug_box;
        TreeView tree_mod;
        Matrix4x4f Pm;
        Matrix4x4f Vm;
        public Buffers buffersGl= new Buffers();
        Matrix4x4f Mm;
        Matrix4x4f MVP;

        public Matrix4x4f[] VPs;
        public Matrix4x4f[] Vs;
        public Matrix4x4f[] Ps;
        Vertex3f lightPos = new Vertex3f(0.0f, 0.0f, 123.0f);
        Vertex3f MaterialDiffuse = new Vertex3f(0.1f, 0.1f, 0.1f);
        Vertex3f MaterialAmbient = new Vertex3f(0.1f, 0.1f, 0.1f);
        Vertex3f MaterialSpecular = new Vertex3f(0.1f, 0.1f, 0.1f);
        public modeGL modeGL = modeGL.View;
        List<Vertex4f> pointsPaint = new List<Vertex4f>();
        Vertex4f curPointPaint = new Vertex4f(0, 0, 0, 1);
        public List<TransRotZoom> transRotZooms = new List<TransRotZoom>();
        public List<TransRotZoom[]> trzForSave;
        public int[] monitorsForGenerate;
        public string pathForSave;
        public ImageBox[] imageBoxesForSave;
        public Size size = new Size(1,1);
        Point locationBox = new Point(0, 0);
        public Vertex2f MouseLoc;
        public Vertex2f MouseLocGL;
        public int lightVis = 0;
        public int textureVis = 0;
        public TextureGL isolines_data,mesh_data;


        #endregion

        #region main

        public void resize(object sender, EventArgs e)
        {
            var contr = (Control)sender;

            sizeControl = contr.Size;

            Gl.Viewport(0, 0, sizeControl.Width, sizeControl.Height);
        }

        public void update_tree()
        {
            if (tree_mod == null) return;          
            foreach (var name in buffersGl.objs)
            {
                if(!check_obj_in_tree(name.Value.name))
                {
                    tree_mod.Nodes.Add(name.Value.name);
                    
                }
            }
        }

        public bool check_obj_in_tree(string obj)
        {
            bool check = false;
            for (int i = 0; i < tree_mod.Nodes.Count; i++)
                if (tree_mod.Nodes[i].Text == obj)
                    check = true;
            return check;
        }
        public int node_obj_in_tree(string obj)
        {
            for (int i = 0; i < tree_mod.Nodes.Count; i++)
                if (tree_mod.Nodes[i].Text == obj)
                    return i;

            return -1;
        }
        void draw_cont()
        {

        }

        public void glControl_Render(object sender, GlControlEventArgs e)
        {
            
            VPs = new Matrix4x4f[4];
            Vs = new Matrix4x4f[4];
            Ps = new Matrix4x4f[4];
            var txt = "";
            for (int i = 0; i < transRotZooms.Count; i++)
            {
                var trz = transRotZooms[i].getInfo(transRotZooms.ToArray());
                Gl.ViewportIndexed((uint)i,
                    trz.rect.X,
                    trz.rect.Y,
                    trz.rect.Width,
                    trz.rect.Height);
                var retM = TransRotZoom.getVPmatrix(trz);
                VPs[i] = retM[2];
                Vs[i] = retM[1];
                Ps[i] = retM[0];

                txt += "TRZ " + i + ": "+ trz.ToString()+"\n";
            }
            //Console.WriteLine("_________");
            Label_trz_cur.Text = txt;
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            addCams();

            if (buffersGl.objs != null)
            {
                if (buffersGl.objs.Count != 0)
                {
                    foreach (var opglObj in buffersGl.objs)
                    {
                        renderGlobj(opglObj.Value);
                    }
                }
            }
            rendercout++;
            if(rendercout%renderdelim==0)
            {
                rendercout = 0;
            }
            update_tree();
        }
        
        void renderGlobj(openGlobj opgl_obj)
        {
            if(opgl_obj.visible)
            {
                try
                {
                    var ids = new IDs();
                    if (opgl_obj.tp == PrimitiveType.Points)
                    {
                        ids = idsPs;
                        if (opgl_obj.count == 1)
                        {
                            ids = idsPsOne;
                        }
                        else
                        {
                            ids = idsPs;
                        }
                    }
                    else if (opgl_obj.tp == PrimitiveType.Triangles)
                    {
                        ids = idsTs;
                        if (opgl_obj.count == 1)
                        {
                            ids = idsTsOne;
                        }
                        else
                        {
                            ids = idsTs;
                        }
                    }
                    else if (opgl_obj.tp == PrimitiveType.Lines)
                    {
                        ids = idsLs;
                        if (opgl_obj.count == 1)
                        {
                            ids = idsLsOne;
                        }
                        else
                        {
                            ids = idsLs;
                        }
                    }
                    load_vars_gl(ids, opgl_obj);
                    opgl_obj.useBuffers();
                   // Console.WriteLine(opgl_obj.count);
                   // Console.WriteLine(opgl_obj.tp);

                    if (opgl_obj.count > 1)
                    {
                        opgl_obj.loadModels();
                        Gl.DrawArraysInstanced(opgl_obj.tp, 0, opgl_obj.vert_len, opgl_obj.count);
                    }
                    else
                    {
                        Gl.DrawArrays(opgl_obj.tp, 0, opgl_obj.vert_len);
                    }

                }
                catch
                {
                }
            }
            
        }
        
        public void glControl_ContextCreated(object sender, GlControlEventArgs e)
        {
            var contr = (Control)sender;
            sizeControl = contr.Size;
            locationBox = contr.Location;
            Gl.Initialize();
            Gl.Enable(EnableCap.Multisample);
            Gl.ClearColor(0.9f, 0.9f, 0.95f, 0.0f);
            Gl.PointSize(2f);
            
            var VertexSourceGL = assembCode(new string[] { @"Graphic\Shaders_face\Vert\VertexSh_Models.glsl" });
            var VertexOneSourceGL = assembCode(new string[] { @"Graphic\Shaders_face\Vert\VertexSh_ModelsOne.glsl" });

            var FragmentSourceGL = assembCode(new string[] { @"Graphic\Shaders_face\Frag\FragmSh.glsl" });
            var FragmentSimpleSourceGL = assembCode(new string[] { @"Graphic\Shaders_face\Frag\FragmSh_Simple.glsl" });

            var GeometryShaderPointsGL = assembCode(new string[] { @"Graphic\Shaders_face\Geom\GeomSh_Points.glsl" });
            var GeometryShaderLinesGL = assembCode(new string[] { @"Graphic\Shaders_face\Geom\GeomSh_Lines.glsl" });
            var GeometryShaderTrianglesGL = assembCode(new string[] { @"Graphic\Shaders_face\Geom\GeomSh_Triangles.glsl" });
            var GeometryShaderTrianglesSliceGL = assembCode(new string[] { @"Graphic\Shaders\Geom\slice_shader_one.glsl" });

            var CompShaderGL = assembCode(new string[] { @"Graphic\Shaders\Comp\CompSh_cross_stereo_f.glsl" });
            programID_comp = createShaderCompute(CompShaderGL);

            var CompShaderSliceGL = assembCode(new string[] { @"Graphic\Shaders\Comp\slice_shader_one.glsl" });
            idsCsSlice.programID = createShaderCompute(CompShaderSliceGL);

            var CompShaderCsGL = assembCode(new string[] { @"Graphic\Shaders\Comp\slice_test.glsl" });
            idsCs.programID = createShaderCompute(CompShaderCsGL);

            


            idsLs.programID = createShader(VertexSourceGL, GeometryShaderLinesGL, FragmentSimpleSourceGL);
            idsLsOne.programID = createShader(VertexOneSourceGL, GeometryShaderLinesGL, FragmentSimpleSourceGL);

            idsPs.programID = createShader(VertexSourceGL, GeometryShaderPointsGL, FragmentSimpleSourceGL);
            idsPsOne.programID = createShader(VertexOneSourceGL, GeometryShaderPointsGL, FragmentSimpleSourceGL);

            idsTs.programID = createShader(VertexSourceGL, GeometryShaderTrianglesGL, FragmentSourceGL);
            idsTsOne.programID = createShader(VertexOneSourceGL, GeometryShaderTrianglesGL, FragmentSourceGL);


            init_vars_gl(idsLs);
            init_vars_gl(idsPs);
            init_vars_gl(idsTs);
            init_vars_gl(idsTsOne);
            init_vars_gl(idsPsOne);
            init_vars_gl(idsLsOne);
            init_vars_gl(idsCsSlice);
            init_vars_gl(idsCs);
            //Gl.Enable(EnableCap.CullFace);
            Gl.Enable(EnableCap.DepthTest);
            Gl.Enable(EnableCap.Blend);
            Gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            //Gl.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            cameraCV = new CameraCV(UtilOpenCV.matrixForCamera(new Size(400, 400), 53), new Matrix<double>(5, 1), new Size(400, 400));
            cameraCV.distortmatrix[0,0] = -0.1;

            


        }
        private void init_vars_gl(IDs ids)
        {
            Gl.UseProgram(ids.programID);

            for (int i = 0; i < 4; i++)
            {
                ids.LocationVPs[i] = Gl.GetUniformLocation(ids.programID, "VPs[" + i + "]");
                ids.LocationVs[i] = Gl.GetUniformLocation(ids.programID, "Vs[" + i + "]");
                ids.LocationPs[i] = Gl.GetUniformLocation(ids.programID, "Ps[" + i + "]");
            }
            ids.LocationM = Gl.GetUniformLocation(ids.programID, "ModelMatrix");
            ids.TextureID = Gl.GetUniformLocation(ids.programID, "textureSample");
            ids.MaterialDiffuseID = Gl.GetUniformLocation(ids.programID, "MaterialDiffuse");
            ids.MaterialAmbientID = Gl.GetUniformLocation(ids.programID, "MaterialAmbient");
            ids.MaterialSpecularID = Gl.GetUniformLocation(ids.programID, "MaterialSpecular");
            ids.LightID = Gl.GetUniformLocation(ids.programID, "LightPosition_world");
            ids.LightPowerID = Gl.GetUniformLocation(ids.programID, "lightPower");

            ids.textureVisID = Gl.GetUniformLocation(ids.programID, "textureVis");
            ids.lightVisID = Gl.GetUniformLocation(ids.programID, "lightVis");
            ids.selected_ID = Gl.GetUniformLocation(ids.programID, "selected");

            ids.MouseLocID = Gl.GetUniformLocation(ids.programID, "MouseLoc");
            ids.MouseLocGLID = Gl.GetUniformLocation(ids.programID, "MouseLocGL");

            ids.comp_proj_ID = Gl.GetUniformLocation(ids.programID, "comp_proj");
            ids.render_count_ID = Gl.GetUniformLocation(ids.programID, "render_count");
            ids.show_faces_ID = Gl.GetUniformLocation(ids.programID, "show_faces");
            ids.transparency_ID = Gl.GetUniformLocation(ids.programID, "transparency");
            ids.inv_norm_ID = Gl.GetUniformLocation(ids.programID, "inv_norm");
            ids.surfs_len_ID = Gl.GetUniformLocation(ids.programID, "surfs_len");

            ids.surf_crossID = Gl.GetUniformLocation(ids.programID, "surf_cross");

        }
        private void load_vars_gl(IDs ids, openGlobj openGlobj)
        {

            Gl.UseProgram(ids.programID);
            if (openGlobj.count == 1)
            {
                var ModelMatr = openGlobj.trsc[0].getModelMatrix();
                Gl.UniformMatrix4f(ids.LocationM, 1, false, ModelMatr);
            }

            //Console.WriteLine(ModelMatr);

            for (int i = 0; i < 4; i++)
            {
                Gl.UniformMatrix4f(ids.LocationVPs[i], 1, false, VPs[i]);
                Gl.UniformMatrix4f(ids.LocationVs[i], 1, false, Vs[i]);
                Gl.UniformMatrix4f(ids.LocationPs[i], 1, false, Ps[i]);
            }

            Gl.Uniform3f(ids.MaterialDiffuseID, 1, MaterialDiffuse);
            Gl.Uniform3f(ids.MaterialAmbientID, 1, MaterialAmbient);
            Gl.Uniform3f(ids.MaterialSpecularID, 1, MaterialSpecular);
            Gl.Uniform3f(ids.LightID, 1, lightPos);
            Gl.Uniform1f(ids.LightPowerID, 1, LightPower);
            Gl.Uniform2f(ids.MouseLocID, 1, MouseLoc);
            Gl.Uniform2f(ids.MouseLocGLID, 1, MouseLocGL);

            // Gl.Uniform1i(ids.textureVisID, 1, textureVis);
            //Gl.Uniform1i(ids.lightVisID, 1, lightVis);
            int text_vis = 0;
            int light_vis = 0;
            int selected = 0;
            if (openGlobj.light_vis) light_vis = 1;
            if (openGlobj.text_vis) text_vis = 1;
            if (openGlobj.selected) selected = 1;

            Gl.Uniform1i(ids.textureVisID, 1, text_vis);
            Gl.Uniform1i(ids.lightVisID, 1, light_vis);
            Gl.Uniform1i(ids.selected_ID, 1, selected);

            Gl.Uniform1i(ids.show_faces_ID, 1, show_faces);
            Gl.Uniform1f(ids.transparency_ID, 1, openGlobj.transparency);
            Gl.Uniform1i(ids.inv_norm_ID, 1, inv_norm);

            Gl.Uniform4f(ids.surf_crossID, 1, surf_cross);

        }

        #endregion
       
        #region comp_gpu
        public Point3d_GL[] cross_flat_gpu(float[] ps1, float[] ps2)
        {
            //var debug_t = new TextureGL(4, ps1.Length/4, 6, PixelFormat.Rgba);//lines
            var ps1_t = new TextureGL(5, ps1.Length/4, 1, PixelFormat.Rgba, ps1);//lines
            var ps2_t = new TextureGL(6, ps2.Length/4, 1, PixelFormat.Rgba, ps2);//triangles
            var ps_cross_t = new TextureGL(7, ps1.Length/4, 1, PixelFormat.Rgba);//ans

           //Console.WriteLine(toStringBuf(ps1_t.getData(), 4, 0, "ps1_t"));
           //Console.WriteLine(toStringBuf(ps2_t.getData(), 4, 0, "ps2_t"));
           //Console.WriteLine(toStringBuf(ps_cross_t.getData(), 4, 0, "ps_cross_t"));
            Gl.UseProgram(programID_comp);
            Gl.DispatchCompute(((uint)ps1.Length / 4), 1, 1);
            Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);

            //Console.WriteLine(toStringBuf(debug_t.getData(),ps1.Length, 4, "ps_cross_t"));
            //Console.WriteLine(toStringBuf(ps_cross_t.getData(), ps1.Length, 4, "ps_cross_t"));

            return Point3d_GL.dataToPoints_ex(ps_cross_t.getData());
        }
        public Point3d_GL[][] cross_flat_gpu_all(Point3d_GL[][] ps1, Point3d_GL[][] ps2)
        {
            if(ps1 == null || ps2 == null)
            {
                return null;
            }
            if (ps1[0] == null || ps2[0] == null)
            {
                return null;
            }
            var ps1_data = Point3d_GL.toData(ps1);
            var ps2_data = Point3d_GL.toData(ps2);

            int h = ps1.Length;
            int w = Math.Max( ps1[0].Length, ps2[0].Length) ;

            var debug_t = new TextureGL(4, w, 6, PixelFormat.Rgba);//lines
            var ps1_t = new TextureGL(5, w, h, PixelFormat.Rgba, ps1_data);//lines
            var ps2_t = new TextureGL(6, w, h, PixelFormat.Rgba, ps2_data);//triangles
            var ps_cross_t = new TextureGL(7, w, h, PixelFormat.Rgba);//ans
            Console.WriteLine("loaded on gpu.");
            //Console.WriteLine(toStringBuf(ps1_t.getData(), 4, 0, "ps1_t"));
            //Console.WriteLine(toStringBuf(ps2_t.getData(), 4, 0, "ps2_t"));
            //Console.WriteLine(toStringBuf(ps_cross_t.getData(), 4, 0, "ps_cross_t"));

            Gl.UseProgram(programID_comp);
            Gl.DispatchCompute((uint)w, (uint)h, 1);
            Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
            Console.WriteLine("computed.");
            var ps_data = ps_cross_t.getData();
            var ps_data_div = Point3d_GL.divide_data(ps_data, w);
            var ps_cr = Point3d_GL.dataToPoints2d(ps_data_div);
            Console.WriteLine("loaded from gpu.");
            Point3d_GL.colorPoints2d(ps1, ps_cr);
            Console.WriteLine("colored.");

            //Console.WriteLine(toStringBuf(debug_t.getData(),w * 4, 4, "debug_t"));
            //Console.WriteLine(w + " " + h);
            //Console.WriteLine(toStringBuf(ps_cross_t.getData(), w*4, 4, "ps_cross_t"));


            return Point3d_GL.filtrExistPoints2d(ps_cr) ;
        }
        public Point3d_GL[][] cross_flat_gpu_mesh(float[] mesh_m, Flat3d_GL[] flats)
        {
            mesh_m = Point3d_GL.mesh3to4(mesh_m);
            int verts_tr = 3;
            int max_w_tex = 8000;
            int max_w_tex_buf = 952000;
            max_w_tex = 1200;//1200
            max_w_tex_buf = 480000;//480000
            var mesh_m_l = mesh_m.ToList();
            int h_buf = 1;
            int w_buf = mesh_m.Length / 4;
            if (mesh_m.Length > max_w_tex_buf)
            {
                h_buf = (int)(mesh_m.Length / max_w_tex_buf) + 1;
            }

            var pss = new List<Point3d_GL>[flats.Length];
            var pss_r = new Point3d_GL[flats.Length][];

            for (int j = 1; j <= h_buf; j++)
            {
                var stop = j * max_w_tex_buf;
                var start = (j - 1) * max_w_tex_buf;
                if (start >= mesh_m.Length) break;
                if (stop >= mesh_m.Length) stop = mesh_m.Length - 1;
                var mesh = mesh_m_l.GetRange(start, stop - start).ToArray();

                int h = 1;
                int w = mesh.Length / 4;
                if (((double)mesh.Length / 4) % 1 > 0) w++;

                if (w > max_w_tex)
                {
                    h = (int)(w / max_w_tex) + 1;
                    w = max_w_tex;
                }
                Console.WriteLine(w + " " + h + " " + w * h * 4 + " " + mesh.Length);
                var mesh_data = new TextureGL(2, w, h, PixelFormat.Rgba, mesh);

                for (int i = 0; i < flats.Length; i++)
                {
                    surf_cross = new Vertex4f((float)flats[i].A, (float)flats[i].B, (float)flats[i].C, (float)flats[i].D);

                    isolines_data = new TextureGL(3, w, h, PixelFormat.Rgba);

                    load_vars_gl(idsCsSlice, new openGlobj());
                    Gl.DispatchCompute((uint)(w), (uint)(h), 1);
                    Gl.MemoryBarrier(MemoryBarrierMask.ShaderImageAccessBarrierBit);
                    var ps_data = isolines_data.getData();
                    var ps_data_div = Point3d_GL.divide_data(ps_data, w);
                    var ps_cr = Point3d_GL.dataToPoints2d(ps_data_div);
                    var ps = Point3d_GL.unifPoints2d(Point3d_GL.filtrExistPoints2d(ps_cr));
                    if (pss[i] == null)
                    {
                        pss[i] = new List<Point3d_GL>();
                    }
                    pss[i].AddRange(ps);
                    //Console.WriteLine(toStringBuf(ps_data, ps_data.Length/w, 4, "isolines_data"));
                }
            }
            for (int i = 0; i < pss.Length; i++)
            {
                if (pss[i] != null)
                    pss_r[i] = pss[i].ToArray();
            }

            return pss_r;
        }

        public Point3d_GL[] intersect_two_mesh(Polygon3d_GL[] pols1, Polygon3d_GL[] pols2)
        {


            return null;
        }

        #endregion

        #region texture
        private void useTexture(uint buff_texture)
        {
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2d, buff_texture);
        }
        Bitmap byteToBitmap(byte[] arr, Size size)
        {
            var bmp = new Bitmap(size.Width, size.Height);
            for (int i = 0; i < size.Width; i++)
            {
                for (int j = 0; j < size.Height; j++)
                {
                    // Console.WriteLine(3 * (j * size.Width + j));
                    var color = Color.FromArgb(
                        arr[3 * (j * size.Width + i)],
                        arr[3 * (j * size.Width + i) + 1],
                        arr[3 * (j * size.Width + i) + 2]
                        );
                    bmp.SetPixel(i, j, color);
                }
            }
            return bmp;
        }
        byte[] textureLoad(Mat mat)
        {

            var bytearr = (byte[,,])mat.GetData();
            Console.WriteLine(mat.Rows + " " + mat.Cols + " " + mat.NumberOfChannels + " ");
            var bytetext = new byte[bytearr.GetLength(0) * bytearr.GetLength(1) * bytearr.GetLength(2)];
            Console.WriteLine(bytearr.GetLength(0));
            Console.WriteLine(bytearr.GetLength(1));
            Console.WriteLine(bytearr.GetLength(0) * bytearr.GetLength(1) * 3);
            Console.WriteLine("___");
            int ind = 0;

            for (int i = 0; i < bytearr.GetLength(0); i++)
            {
                for (int j = 0; j < bytearr.GetLength(1); j++)
                {
                    bytetext[ind] = bytearr[bytearr.GetLength(0) - i - 1, j, 0]; ind++;
                    bytetext[ind] = bytearr[bytearr.GetLength(0) - i - 1, j, 1]; ind++;
                    bytetext[ind] = bytearr[bytearr.GetLength(0) - i - 1, j, 2]; ind++;
                }
            }
            Console.WriteLine(ind);
            var textureSize = new Size(mat.Width, mat.Height);
            return bytetext;
        }
        private uint bindTexture(byte[] arrB, Size textureSize)
        {
            var buff_texture = Gl.GenTexture();
            Gl.ActiveTexture(TextureUnit.Texture0);
            Gl.BindTexture(TextureTarget.Texture2d, buff_texture);
            // Gl.TexParameter(TextureTarget.Texture2d, TextureParameterName.TextureWrapS, Gl.REPEAT);

            Gl.TexImage2D(TextureTarget.Texture2d, 0, InternalFormat.Rgb, textureSize.Width, textureSize.Height, 0, PixelFormat.Bgr, PixelType.UnsignedByte, arrB);
            Gl.GenerateMipmap(TextureTarget.Texture2d);
            return buff_texture;
        }

        #endregion


        #region util
        public Matr4x4f rightMatrMon(int ind_mon)
        {
            var data_r = new float[16];
            var left_m = Vs[ind_mon];
            for(int i=0; i < 4; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    data_r[4*i + j] = left_m[(uint)i, (uint)j];                                     
                }
            }
            return new Matr4x4f(data_r);
        }
        public void SaveToFolder(string folder,int id)
        {
            var bitmap = matFromMonitor(id);
            var invVm = Vs[id].Inverse;
            var trz_in = transRotZooms[selectTRZ_id(id)];
            var trz = trz_in.getInfo(transRotZooms.ToArray());
            //Gl.depth
            var path_gl = Path.Combine(folder, "monitor_" + id.ToString());
            Directory.CreateDirectory(path_gl);
            bitmap.Save(path_gl + "/" + trz.ToString() + ".png");
        }

        public Bitmap bitmapFromMonitor(int id)
        {
            var recTRZ = (transRotZooms[selectTRZ_id(id)]).rect;
            var lockMode = System.Drawing.Imaging.ImageLockMode.WriteOnly;
            var format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
            var bitmap = new Bitmap(recTRZ.Width, recTRZ.Height, format);

            var bitmapRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(bitmapRectangle, lockMode, format);
            Gl.ReadPixels(recTRZ.X, recTRZ.Y, recTRZ.Width, recTRZ.Height, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            bitmap.UnlockBits(bmpData);
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);
           
            return bitmap;

        }

        public Mat matFromMonitor(int id)
        {
            //Console.WriteLine("selectTRZ_id(id)" + selectTRZ_id(id));
            var selecTrz = selectTRZ_id(id);
            if(selecTrz<0)
            {
                return null;
            }
            var trz = transRotZooms[selectTRZ_id(id)];
            var recTRZ = trz.rect;
            var data = new Mat(recTRZ.Width, recTRZ.Height, Emgu.CV.CvEnum.DepthType.Cv8U, 3);
            //Console.WriteLine("recTRZ.Width " + recTRZ.X + " " + recTRZ.Y + " " + recTRZ.Width + " " + recTRZ.Height);
           // Console.WriteLine(data.DataPointer);
           // Console.WriteLine(trz);
            Gl.ReadPixels(recTRZ.X, recTRZ.Y, recTRZ.Width, recTRZ.Height, PixelFormat.Bgr, PixelType.UnsignedByte, data.DataPointer);
            //CvInvoke.Rotate(data, data, Emgu.CV.CvEnum.RotateFlags.Rotate180);
            //CvInvoke.Flip(data, data, Emgu.CV.CvEnum.FlipType.Vertical);
            return data;
        }

        PointF toGLcord(PointF pf)
        {
            var sizeView = transRotZooms[0].rect;

            var x = (sizeView.Width / 2) * pf.X + sizeView.Width / 2;
            var y = -((sizeView.Width / 2) * pf.Y) + sizeView.Height / 2;
            return new PointF(x, y);
        }


        PointF toTRZcord(PointF pf)
        {
            var sizeView = transRotZooms[0].rect;

            var x = (sizeView.Width / 2) * pf.X + sizeView.Width / 2;
            var y = (sizeView.Width / 2) * pf.Y + sizeView.Height / 2;
            return new PointF(x, y);
        }
        PointF toTRZcord_photo(PointF pf,Size size_trz)
        {
            var sizeView = new Rectangle(new Point(0, 0), size_trz);

            var x = (sizeView.Width / 2) * pf.X + sizeView.Width / 2;
            var y = (sizeView.Width / 2) * pf.Y + sizeView.Height / 2;
            return new PointF(x, y);
        }
        public void swapTRZ(int ind1, int ind2)
        {
            var lamb1 = transRotZooms[ind1].rect;
            var lamb2 = transRotZooms[ind2].rect;
            var trz1 = transRotZooms[ind1];
            var trz2 = transRotZooms[ind2];
            trz1.rect = lamb2;
            trz2.rect = lamb1;
            transRotZooms[ind1] = trz1;
            transRotZooms[ind2] = trz2;
        }
        int selectTRZ_id(int id)
        {
            int ind = 0;
            foreach (var trz in transRotZooms)
            {
                if (trz.id == id)
                {
                    return ind;
                }
                ind++;
            }
            return -1;
        }
        public Matrix4x4f[] compMVP_photo(string data)
        {
            var trz = new TransRotZoom(data);
            var zoom = trz.zoom;
            var off_x = trz.off_x;
            var off_y = trz.off_y;
            var off_z = trz.off_z;
            var xRot = trz.xRot;
            var yRot = trz.yRot;
            var zRot = trz.zRot;
            
            var _Pm = ProjmatrF(53f);
            var _Vm = Transmatr((float)off_x, -(float)off_y, (float)zoom * (float)off_z) * RotXmatr(xRot) * RotYmatr(yRot) * RotZmatr(zRot);
            var Pm_ = new Matrix4x4f((_Pm).data);
            var Vm_ = new Matrix4x4f((_Vm).data);

            var Mm_ = Matrix4x4f.Identity;
            var MVP_ = new Matrix4x4f((_Pm * _Vm).data);

            return new Matrix4x4f[] { Pm_, Vm_, Mm_, MVP_ };
        }

        public Matrix4x4f[] compMVPmatrix(TransRotZoom trz_in)
        {
            var trz = trz_in.getInfo(transRotZooms.ToArray());
            
            var zoom = trz.zoom;
            var off_x = trz.off_x;
            var off_y = trz.off_y;
            var off_z = trz.off_z;
            var xRot = trz.xRot;
            var yRot = trz.yRot;
            var zRot = trz.zRot;
            if (trz.viewType_ == viewType.Perspective)
            {
                var _Pm = ProjmatrF((float)trz.fovx);              
                var _Vm = Transmatr((float)off_x, -(float)off_y, (float)zoom * (float)off_z) * RotXmatr(xRot) * RotYmatr(yRot) * RotZmatr(zRot);
                Pm = new Matrix4x4f((_Pm).data);
                Vm = new Matrix4x4f((_Vm).data);
                Mm = Matrix4x4f.Identity;
                MVP = new Matrix4x4f((_Pm * _Vm).data);
            }
            else if (trz.viewType_ == viewType.Ortho)
            {
                var _Pm = OrthoF((float)zoom);
                var _Vm = Transmatr((float)off_x, -(float)off_y,(float)off_z) * RotXmatr(xRot) * RotYmatr(yRot) * RotZmatr(zRot);
                Pm = new Matrix4x4f((_Pm).data);
                Vm = new Matrix4x4f((_Vm).data);
                Mm = Matrix4x4f.Identity;
                MVP = new Matrix4x4f((_Pm * _Vm).data);
            }
            

            return new Matrix4x4f[] { Pm, Vm, Mm, MVP };
        }
        static public float toRad(float degrees)
        {
            return degrees * PI / 180;
        }
        static public float cos(double alpha)
        {
            return (float)Math.Cos(toRad((float)alpha));
        }
        static public float sin(double alpha)
        {
            return (float)Math.Sin(toRad((float)alpha));
        }
        static public Matr4x4f Transmatr(float x = 0, float y = 0, float z = 0)
        {
            var data = new float[] {
                 1, 0, 0, x ,
                 0, 1, 0, y,
                 0, 0, 1, z ,
                 0, 0, 0, 1  };
            return new Matr4x4f(data);
        }
        static public Matr4x4f ProjmatrF(float fx = 1, float aspec = 1, float n =0.1f ,float f = 3000f)
        {
            var fy = fx / aspec;
            var a = (f + n) / (f - n);
            var b = (-2*f * n) / (f - n);
            var cx = 1 / (float)Math.Tan(toRad(fx) / 2);
            var cy = 1 / (float)Math.Tan(toRad(fy) / 2);
            var data = new float[] {
                 cx, 0, 0, 0 ,
                 0, cy, 0, 0 ,
                 0, 0, a,  b,
                 0, 0, 1, 0  };
            return new Matr4x4f(data);
        }

        static public Matr4x4f OrthoF(float dim = 20, float n = -2000f, float f = 2000f)
        {
            float left = -dim;
            float right = dim;
            float bottom = -dim;
            float top = dim;
            var fx = 2 / (right - left);
            var fy = 2 / (top - bottom);
            var a = (f + n) / (f - n);
            var b = 2  / (f - n);
            var cx = (right+left)/(right-left);
            var cy = (top + bottom) / (top - bottom);
            var data = new float[] {
                 fx, 0, 0, -cx ,
                 0, fy, 0, -cy ,
                 0, 0, -b, -a,
                 0, 0, 0, 1 };
            return new Matr4x4f(data);
        }

        static public Matr4x4f Projmatr(float f = 1)
        {

            var data = new float[] {
                 f, 0, 0, 0 ,
                 0, f, 0, 0 ,
                 0, 0, 1.001f,  0.04f,
                 0, 0, 100, 0  };
            return new Matr4x4f(data);
        }
        static public Matr4x4f RotZmatr(double alpha)
        {
            var data =  new float[] {
                 cos(alpha), -sin(alpha), 0,0 ,
                 sin(alpha), cos(alpha), 0, 0 ,
                 0, 0, 1, 0 ,
                 0, 0, 0, 1  };
            return new Matr4x4f(data);
        }
        static public Matr4x4f RotYmatr(double alpha)
        {
            var data = new float[] {
                 cos(alpha),0, sin(alpha), 0,
                 0,1,0 , 0,
                 -sin(alpha), 0, cos(alpha), 0 ,
                 0, 0, 0, 1  };
            return new Matr4x4f(data);
        }
        static public Matr4x4f RotXmatr(double alpha)
        {
            var data = new float[] {
                1,0,0,0,
                0, cos(alpha), -sin(alpha), 0,
                0, sin(alpha), cos(alpha), 0, 
                 0, 0, 0, 1  };
            return new Matr4x4f(data);
        }


        /// <summary>
        /// 3dGL->2dIm
        /// </summary>
        /// <param name="point"></param>
        /// <param name="mvp"></param>
        /// <returns></returns>
        public PointF calcPixel(Vertex4f point, int id)
        {          
            var p2 =new  Matr4x4f(VPs[id]) * new Vert4f(point);
            p2.Norm();
            var p3 = toTRZcord(new PointF(p2.data[0], p2.data[1]));      
            //Console.WriteLine("v: " + p3.X + " " + p3.Y + " " + p2.z + " " + p2.w + " mvp_len: " + MVPs[0].ToString());
            return p3;
        }

        public PointF calcPixel_photo(Vertex4f point, string data,Size size_trz)
        {
            var p2 = new Matr4x4f(compMVP_photo(data)[3]) * new Vert4f(point);
            p2.Norm();
            var p3 = toTRZcord_photo(new PointF(p2.data[0], p2.data[1]), size_trz);

            //Console.WriteLine("v: " + p3.X + " " + p3.Y + " " + p2.z + " " + p2.w + " mvp_len: " + MVPs[0].ToString());
            return p3;
        }

        public PointF calcPixelInv(Vertex4f point, Matrix4x4f mvp)
        {
            var p2 = mvp.Inverse * point;
            var p4 = p2 / p2.w;
            var p3 = toGLcord(new PointF(p4.x, p4.y));
            //Console.WriteLine("v: " + p3.X + " " + p3.Y + " " + p2.z + " " + p2.w + " mvp_len: " + MVPs[0].ToString());
            return p3;
        }

        string[] stringAdd(string[] st1, string[] st2)
        {
            var st_ret = new string[st1.Length + st2.Length];
            for (int i = 0; i < st1.Length; i++)
            {
                st_ret[i] = st1[i];
            }
            for (int i = 0; i < st2.Length; i++)
            {
                st_ret[st1.Length + i] = st2[i];
            }
            return st_ret;
        }
        public void printDebug(RichTextBox box)
        {
            string txt = "";
            foreach(var ob in buffersGl.objs.Values)
            {
                for (int i = 0; i < ob.vertex_buffer_data.Length / 3; i++)
                {
                    txt += "pos| " + i.ToString() + " " + ob.vertex_buffer_data[i * 3].ToString() + " "
                        + ob.vertex_buffer_data[i * 3 + 1].ToString() + " "
                        + ob.vertex_buffer_data[i * 3 + 2].ToString() + " |";
                }
                txt += "\n________________________";
            }
            box.Text = txt;
        }
        static public string toStringBuf(float[] buff, int strip, int substrip, string name)
        {
            if (buff == null)
                return name + " null ";
            StringBuilder txt = new StringBuilder();
            txt.Append(name + " " + buff.Length);
            for (int i = 0; i < buff.Length / strip+1; i++)
            {
                txt.Append("| \n");
                for (int j = 0; j < strip; j++)
                {
                    if (substrip != 0)
                    {
                        if (j % substrip == 0)
                        {
                            txt.Append("|");
                        }
                    }
                    int ind = i * strip + j;
                    if (ind >= buff.Length) break;
                    txt.Append(str_to_same_len(Math.Round( buff[ind],4)) + ",");
                }
            }
            txt.Append(" |\n--------------------------------\n");
            return txt.ToString();

        }
        static string str_to_same_len(object var, int len = 6)
        {
            string str = var.ToString();
            if (str.Length > len) return str;
            while(str.Length<len)
            {
                str += " ";
            }
            //str+=str.Length.ToString();
            return str;
        }
        #endregion

        #region mouse
        public void add_Label(Label label_list, Label label_cur, Label label_trz)
        {
            Label_trz_cur = label_trz;
            Label_cor = label_list;
            Label_cor_cur = label_cur;
            if (Label_cor == null || Label_cor_cur==null || Label_trz_cur==null) 
            {
                Console.WriteLine("null_start");
            }
            
        }
        public void add_TreeView(TreeView tree)
        {
            tree_mod = tree;
        }
        public void add_TextBox(RichTextBox richTextBox)
        {
            debug_box = richTextBox;
        }
        public void addMonitor(Rectangle rect,int id)
        {
            var trz = new TransRotZoom(rect, id);
            transRotZooms.Add(trz);
        }
        public void addMonitor(Rectangle rect, int id, Vertex3d rotVer, Vertex3d transVer, int _idMast)
        {
            transRotZooms.Add(new TransRotZoom(rect, id, rotVer, transVer, _idMast));
        }
        int selectTRZ(MouseEventArgs e)
        {
            int ind = 0;
            foreach(var trz in transRotZooms)
            {
                var recGL = new Rectangle(trz.rect.X, sizeControl.Height - trz.rect.Y - trz.rect.Height, trz.rect.Width, sizeControl.Height - trz.rect.Y);
                if(recGL.Contains(e.Location))
                {
                    return ind;
                }
                ind++;
            }
            return -1;
        }


        public void glControl_MouseDown(object sender, MouseEventArgs e)
        {           
            switch(modeGL)
            {
                case modeGL.View:
                    lastPos = e.Location;
                    break;
                case modeGL.Paint:
                    if (e.Button == MouseButtons.Left)
                    {
                        pointsPaint.Add(curPointPaint);
                        if(pointsPaint.Count>2)
                        {
                            var name_cont = "cont";
                            buffersGl.removeObj(name_cont);
                        addMeshWithoutNorm(
                            z_mesh_from_cont_xy(
                                Point3d_GL.toPoints(pointsPaint.ToArray()), 400),
                            PrimitiveType.Triangles,
                            new Color3d_GL(1, 0, 0), name_cont);
                            buffersGl.setTranspobj(name_cont, 0.3f);
                        }
                           

                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        pointsPaint.Clear();
                        var name_cont = "cont";
                        buffersGl.removeObj(name_cont);
                    }
                    break;
            }
        }
        public void glControl_MouseMove(object sender, MouseEventArgs e)
        {
            int sel_trz = selectTRZ(e);
            if(sel_trz < 0)
            {
                return;
            }
            var trz = transRotZooms[sel_trz];
            Label_cor_cur.Text = e.X + " " + e.Y;
            int w = trz.rect.Width;
            int h = trz.rect.Height;
            var proj_xy = new Vertex4f(0, 0, 0, 0);
            if (Vs!=null)
            {
                var vm = Vs[sel_trz];

                proj_xy = vm.Inverse * new Vertex4f(
                    (float)trz.zoom * ((float)e.X / (0.5f * (float)sizeControl.Width) - 1f),
                    -((float)trz.zoom * ((float)e.Y / (0.5f * (float)sizeControl.Height) - 1f)), 0, 1);

            }
            

            switch (modeGL)
            {
                case modeGL.View:
                    
                    var dx = e.X - lastPos.X;
                    var dy = e.Y - lastPos.Y;
                    double dyx = lastPos.Y - w / 2;
                    double dxy = lastPos.X - h / 2;
                    var delim = (Math.Sqrt(dy * dy + dx * dx) * Math.Sqrt(dxy * dxy + dyx * dyx));
                    double dz = 0;
                    if (delim != 0)
                    {
                        dz = (dy * dxy + dx * dyx) / delim;

                    }
                    else
                    {
                        dz = 0;
                    }
                    if (e.Button == MouseButtons.Left)
                    {
                        trz.xRot += dy;
                        //trz.yRot -= dx;
                        trz.zRot += dx;
                        
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        trz.off_x += Convert.ToDouble(dx);
                        trz.off_y += Convert.ToDouble(dy);
                    }
                    lastPos = e.Location;
                    break;
                    


                case modeGL.Paint:
                    /*var p_XY = new Vertex4f((float)e.Location.X/ (0.5f* (float)w), (float)e.Location.Y/(0.5f* (float)h), 0,1f);

                    var p_XY_1 = new Vertex4f(
                         (float)trz.zoom*((float)(e.Location.X - trz.rect.X)/ (0.5f * (float)w)-1),
                        (float)trz.zoom * ((float)(-e.Location.Y + sizeControl.Width - trz.rect.Y) / (0.5f * (float)h)-1),
                        0, 1f);

                    */

                    try
                    {
                        if (Label_cor != null)
                        {

                            curPointPaint = proj_xy;
                            Label_cor.Text = curPointPaint.ToString() + "\n" + "\n";//;// + (invM * p_YZ).ToString() + "\n" + (invM * p_ZX).ToString();
                            if(pointsPaint.Count!=0)
                            {
                                foreach(var p in pointsPaint)
                                {
                                    Label_cor.Text += p.ToString() + "\n";
                                }
                            }

                            if (pointsPaint.Count > 1)
                            {
                                var dis = (pointsPaint[pointsPaint.Count - 1] - pointsPaint[pointsPaint.Count - 2]).Module();
                                Label_cor.Text +="dist = "+ Math.Round(dis,4).ToString() + "\n";
                            }
                        }
                    }
                    catch
                    {

                    }
                    
                    break;
            }
            transRotZooms[sel_trz] = trz;
        }

        public Point3d_GL[] get_contour()
        {
            if(pointsPaint.Count!=0)
            {
                pointsPaint.Add(pointsPaint[0]);
                return Point3d_GL.toPoints(pointsPaint.ToArray());
            }
            return null;
        }
        public void Form1_mousewheel(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("P m = " + Pm);
            //Console.WriteLine("V m = " + Vm);
            // var invVm = Vm.Inverse;
            //Console.WriteLine("invV m = " + invVm);
            int sel_trz = selectTRZ(e);
            if (sel_trz < 0)
            {
                return;
            }
            var trz = transRotZooms[sel_trz];
            var angle = e.Delta;
            if (angle > 0)
            {
                if (trz.zoom < 0.002)
                {
                }
                else
                {
                    trz.zoom = 0.7 * trz.zoom;
                    trz.zoom = Math.Round(trz.zoom, 4);
                }
            }
            else
            {
                trz.zoom = 1.3 * trz.zoom;
                trz.zoom = Math.Round(trz.zoom, 4);
            }
            transRotZooms[sel_trz] = trz;
        }


        public void lightPowerScroll(int value)
        {
            var f = (float)value*100;
            LightPower = f;
        }
        public void diffuseScroll(int value)
        {
            var f = (float)value / 10;
            MaterialDiffuse.x = f;
            MaterialDiffuse.y = f;
            MaterialDiffuse.z = f;
        }
        public void ambientScroll(int value)
        {
            var f = (float)value / 10;
            MaterialAmbient.x = f;
            MaterialAmbient.y = f;
            MaterialAmbient.z = f;
        }
        public void specularScroll(int value)
        {

            var f = (float)value / 10;
            MaterialSpecular.x = f;
            MaterialSpecular.y = f;
            MaterialSpecular.z = f;
        }
        
        public void lightXscroll(int value)
        {
            lightPos.x = (float)value * 10;
        }
        public void lightYscroll(int value)
        {
            lightPos.y = (float)value * 10;

        }
        public void lightZscroll(int value)
        {
            lightPos.z = (float)value * 10;
        }
        public void orientXscroll(int value)
        {
            var trz = transRotZooms[currentMonitor];
            trz.setxRot(value);
            transRotZooms[currentMonitor] = trz;
        }
        public void orientYscroll(int value)
        {
            var trz = transRotZooms[currentMonitor];
            trz.setyRot(value);
            transRotZooms[currentMonitor] = trz;
        }
        public void orientZscroll(int value)
        {
            var trz = transRotZooms[currentMonitor];
            trz.setzRot(value);
            transRotZooms[currentMonitor] = trz;
        }

        public void planeXY()
        {
            var trz = transRotZooms[currentMonitor];
            trz.setRot(0, 0, 0);
            transRotZooms[currentMonitor] = trz;
        }
        public void planeYZ()
        {
            var trz = transRotZooms[currentMonitor];
            trz.setRot(0, 90, 0);
            transRotZooms[currentMonitor] = trz;
        }
        public void planeZX()
        {
            var trz = transRotZooms[currentMonitor];
            trz.setRot(90, 0, 0);
            transRotZooms[currentMonitor] = trz;
        }
        public void changeViewType(int ind)
        {
            if (ind >= 0 && ind<transRotZooms.Count)
            {
                var trz = transRotZooms[ind];
                if(trz.viewType_==viewType.Ortho)
                {
                    trz.viewType_ = viewType.Perspective;
                    transRotZooms[ind] = trz;
                }
                else if (trz.viewType_ == viewType.Perspective)
                {
                    trz.viewType_ = viewType.Ortho;
                    transRotZooms[ind] = trz;
                }

            }
        }

        public void changeVisible(int ind)
        {
            if (ind >= 0 && ind < transRotZooms.Count)
            {
                var trz = transRotZooms[ind];
                if (trz.visible == true)
                {
                    trz.visible = false;
                    transRotZooms[ind] = trz;
                }
                else if (trz.visible == false)
                {
                    trz.visible = true;
                    transRotZooms[ind] = trz;
                }

            }
        }
        public void setMode(modeGL mode)
        {
            modeGL = mode;
        }
        #endregion

        #region mesh
        void addCams()
        {
            for(int i=0; i<transRotZooms.Count;i++)
            {
                addCamView(i);
            }
            //Console.WriteLine("_______________");
        }
        void addCamView(int i)
        {
            var trz = transRotZooms[i].getInfo(transRotZooms.ToArray());

            var verts = new Vertex4f[0];
            if (trz.viewType_ == viewType.Perspective)
            {
                var p1 = new Vertex4f(0, 0, 0.01f, 1f);
                double _z = 1000;
                double _x = _z * Math.Tan(toRad((float)(trz.fovx / 2))), _y = _z * Math.Tan(toRad((float)(trz.fovx / 2)));
                float x = (float)_x; float y = (float)_y; float z = -(float)_z;
                var p2 = new Vertex4f(-x, -y, z, 1);
                var p3 = new Vertex4f(-x, y, z, 1);
                var p4 = new Vertex4f(x, -y, z, 1);
                var p5 = new Vertex4f(x, y, z, 1);
                p1 = Vs[i].Inverse * p1;
                p2 = Vs[i].Inverse * p2;
                p3 = Vs[i].Inverse * p3;
                p4 = Vs[i].Inverse * p4;
                p5 = Vs[i].Inverse * p5;
                verts = new Vertex4f[16]
                {
            p1,p2, p1,p3, p1,p4, p1,p5, p2,p3, p3,p5, p5,p4, p4,p2
                };
                   
            }
            else if (trz.viewType_ == viewType.Ortho)
            {
                    
                double _z = -2000;
                double _z0 = 0.1;
                double _x =  1*trz.zoom;
                double _y = 1 * trz.zoom;
                float x = (float)_x; float y = (float)_y; float z = (float)_z; float z0 = (float)_z0;

                var p = new Vertex4f[8]
                {
            new Vertex4f(-x, -y, z0, 1),
            new Vertex4f(-x, y, z0, 1),
            new Vertex4f(x, y, z0, 1),
            new Vertex4f(x, -y, z0, 1),

            new Vertex4f(-x, -y, z, 1),
            new Vertex4f(-x, y, z, 1),
            new Vertex4f(x, y, z, 1),
                new Vertex4f(x, -y, z, 1)
                };
                for (int j = 0; j < p.Length; j++)
                {
                    p[j] = Vs[i].Inverse * p[j];
                }

                verts = new Vertex4f[24]
                {
            p[0],p[1], p[1],p[2], p[2],p[3], p[3],p[0],
            p[4],p[5], p[5],p[6], p[6],p[6], p[7],p[4],
            p[0],p[4], p[1],p[5], p[2],p[6], p[3],p[7]
                };
               
            }
            


            if (Convert.ToDouble( trz.view_3d) < 0)
            {
                transRotZooms[i].view_3d = add_buff_gl(toFloat(verts), null, null, PrimitiveType.Lines, i.ToString());
            }
            else
            {

                buffersGl.setObjVdata(trz.view_3d, toFloat(verts), null);
                buffersGl.setVisibleobj(trz.view_3d, trz.visible);
            }
            //Console.WriteLine(verts[0]);

            
        }
        float[] toFloat(Point3d_GL[] points)
        {
            var fl = new float[points.Length * 3];
            for(int i=0; i< points.Length; i++)
            {
                fl[3 * i] = (float)points[i].x;
                fl[3 * i+1] = (float)points[i].y;
                fl[3 * i+2] = (float)points[i].z;
            }
            return fl;
        }
        float[] toFloat(Vertex4f[] points)
        {
            var fl = new float[points.Length * 3];
            for (int i = 0; i < points.Length; i++)
            {
                fl[3 * i] = points[i].x;
                fl[3 * i + 1] = points[i].y;
                fl[3 * i + 2] = points[i].z;
            }
            return fl;
        }

        public string add_buff_gl(float[] data_v, float[] data_c, float[] data_n, PrimitiveType tp,string name = "new obj")
        {
            if (data_v == null)
            {
                Console.WriteLine("date_v == NULL");
                return null;
            }
            return buffersGl.add_obj(new openGlobj(data_v, data_c, data_n,null, tp,name),name);
        }

        public void remove_buff_gl_id(string name)
        {
            buffersGl.removeObj(name);
        }
        public string addFrame(Point3d_GL pos, Point3d_GL x, Point3d_GL y, Point3d_GL z, string name = "new Frame")
        {
            //addLineMesh(new Point3d_GL[] { pos, x }, 1.0f, 1.0f, 0);
            //addLineMesh(new Point3d_GL[] { pos, x }, 1.0f, 0, 0);
            var mesh = new List<float>();
            var color = new List<float>();
            var points = new Point3d_GL[] { x, y, z };
            var colors = new Color3d_GL[] { Color3d_GL.red(), Color3d_GL.green(), Color3d_GL.blue() };
            for (int i=0; i<points.Length;i++)
            {
                mesh.Add((float)pos.x); 
                mesh.Add((float)pos.y); 
                mesh.Add((float)pos.z); 

                color.Add(colors[i].r);
                color.Add(colors[i].g);
                color.Add(colors[i].b);

                mesh.Add((float)points[i].x);
                mesh.Add((float)points[i].y);
                mesh.Add((float)points[i].z);

                color.Add(colors[i].r);
                color.Add(colors[i].g);
                color.Add(colors[i].b);

            }

            return addMeshColor(mesh.ToArray(), color.ToArray(), PrimitiveType.Lines,name);
        }

        public void addFrame_Cam(Camera cam, int frame_len = 15)
        {

            addFrame(cam.pos, cam.pos + cam.oX * frame_len, cam.pos + cam.oY * frame_len, cam.pos + cam.oZ * frame_len * 1.3);
        }

        public void addFlat3d_XZ(Flat3d_GL flat3D_GL, Matrix<double> matrix=null, Color3d_GL color = null, string name = "new Flat XZ")
        {
            var p0 = (new Line3d_GL(new Vector3d_GL(0, 10, 0), new Point3d_GL(-50, 0, -1000))).calcCrossFlat(flat3D_GL);
            var p1 = (new Line3d_GL(new Vector3d_GL(0, 10, 0), new Point3d_GL(50, 0, -1000))).calcCrossFlat(flat3D_GL);

            var p2 = (new Line3d_GL(new Vector3d_GL(0, 10, 0), new Point3d_GL(-50, 0, 1000))).calcCrossFlat(flat3D_GL);
            var p3 = (new Line3d_GL(new Vector3d_GL(0, 10, 0), new Point3d_GL(50, 0, 1000))).calcCrossFlat(flat3D_GL);

            if(matrix!=null)
            {
                p0 = matrix * p0;
                p1 = matrix * p1;
                p2 = matrix * p2;
                p3 = matrix * p3;
            }

            var ps = new Point3d_GL[]
            {
                p1,p3,p2,
                p2,p0,p1,


            };
            addMesh(Point3d_GL.toMesh(ps), PrimitiveType.Triangles, color, name);

        }

        public void addFlat3d_YZ(Flat3d_GL flat3D_GL, Matrix<double> matrix = null, Color3d_GL color = null, string name = "new Flat YZ")
        {
            var p0 = (new Line3d_GL(new Vector3d_GL(10, 0, 0), new Point3d_GL(0, -50,  -1000))).calcCrossFlat(flat3D_GL);
            var p1 = (new Line3d_GL(new Vector3d_GL(10, 0, 0), new Point3d_GL(0, 50, -1000))).calcCrossFlat(flat3D_GL);

            var p2 = (new Line3d_GL(new Vector3d_GL(10, 0,  0), new Point3d_GL(0, -50, 1000))).calcCrossFlat(flat3D_GL);
            var p3 = (new Line3d_GL(new Vector3d_GL(10, 0,  0), new Point3d_GL(0, 50,  1000))).calcCrossFlat(flat3D_GL);

            if (matrix != null)
            {
                p0 = matrix * p0;
                p1 = matrix * p1;
                p2 = matrix * p2;
                p3 = matrix * p3;
            }

            var ps = new Point3d_GL[]
            {
                p1,p3,p2,
                p2,p0,p1,


            };
            addMesh(Point3d_GL.toMesh(ps), PrimitiveType.Triangles, color, name);

        }
        public void addFlat3d_XY(Flat3d_GL flat3D_GL, Matrix<double> matrix = null, Color3d_GL color = null, string name = "new Flat XY")
        {
            var p0 = (new Line3d_GL(new Vector3d_GL( 0, 0,10), new Point3d_GL( -50, -1000,0))).calcCrossFlat(flat3D_GL);
            var p1 = (new Line3d_GL(new Vector3d_GL( 0, 0, 10), new Point3d_GL( 50, -1000,0))).calcCrossFlat(flat3D_GL);

            var p2 = (new Line3d_GL(new Vector3d_GL( 0, 0, 10), new Point3d_GL( -50, 1000,0))).calcCrossFlat(flat3D_GL);
            var p3 = (new Line3d_GL(new Vector3d_GL( 0, 0, 10), new Point3d_GL( 50, 1000,0))).calcCrossFlat(flat3D_GL);

            if (matrix != null)
            {
                p0 = matrix * p0;
                p1 = matrix * p1;
                p2 = matrix * p2;
                p3 = matrix * p3;
            }

            var ps = new Point3d_GL[]
            {
                p1,p3,p2,
                p2,p0,p1,


            };
            addMesh(Point3d_GL.toMesh(ps), PrimitiveType.Triangles,color,name);

        }
        public void addFlat3d_XY_zero(double z = 0,Color3d_GL color = null, string name = "new Flat XZ")
        {
            Flat3d_GL flat3D_GL = new Flat3d_GL(new Point3d_GL(10, 0, z), new Point3d_GL(10, 10, z), new Point3d_GL(0, 10, z));
            var p0 = (new Line3d_GL(new Vector3d_GL(0, 0, 10), new Point3d_GL(-50,  -10,0))).calcCrossFlat(flat3D_GL);
            var p1 = (new Line3d_GL(new Vector3d_GL(0, 0, 10), new Point3d_GL(50,  -10,0))).calcCrossFlat(flat3D_GL);

            var p2 = (new Line3d_GL(new Vector3d_GL(0, 0, 10), new Point3d_GL(-50,  100,0))).calcCrossFlat(flat3D_GL);
            var p3 = (new Line3d_GL(new Vector3d_GL(0, 0, 10), new Point3d_GL(50, 100,0))).calcCrossFlat(flat3D_GL);

            var ps = new Point3d_GL[]
            {
                p1,p3,p2,
                p2,p0,p1
            };
            addMesh(Point3d_GL.toMesh(ps), PrimitiveType.Triangles,color,name);

        }

        public void addFlat3d_XZ_zero(Color3d_GL color = null,string name = "new Flat XZ")
        {
            Flat3d_GL flat3D_GL = new Flat3d_GL(new Point3d_GL(10, 0, 0), new Point3d_GL(10, 0, 10), new Point3d_GL(0, 0, 10));
            var p0 = (new Line3d_GL(new Vector3d_GL(0, 10, 0), new Point3d_GL(-50, 0, -10))).calcCrossFlat(flat3D_GL);
            var p1 = (new Line3d_GL(new Vector3d_GL(0, 10, 0), new Point3d_GL(50, 0, -10))).calcCrossFlat(flat3D_GL);

            var p2 = (new Line3d_GL(new Vector3d_GL(0, 10, 0), new Point3d_GL(-50, 0, 100))).calcCrossFlat(flat3D_GL);
            var p3 = (new Line3d_GL(new Vector3d_GL(0, 10, 0), new Point3d_GL(50, 0, 100))).calcCrossFlat(flat3D_GL);

            var ps = new Point3d_GL[]
            {
                p1,p3,p2,
                p2,p0,p1
            };
            addMesh(Point3d_GL.toMesh(ps), PrimitiveType.Triangles,color,name);

        }
        public void addFrame_Cam(CameraCV cam, int frame_len = 15)
        {
            var posit = cam.matrixCS * new Point3d_GL(0, 0, 0);
            var ox = cam.matrixCS * new Point3d_GL( frame_len, 0, 0);
            var oy = cam.matrixCS * new Point3d_GL( 0, frame_len, 0);
            var oz = cam.matrixCS * new Point3d_GL( 0, 0, frame_len);
            addFrame(posit, ox, oy, oz);
        }

        public void addFrame(Matrix<double> m, int frame_len = 15)
        {
            var posit = m * new Point3d_GL(0, 0, 0);
            /*var ox = matrix * new Point3d_GL(frame_len, 0, 0);
             var oy = matrix * new Point3d_GL(0, frame_len, 0);
             var oz = matrix * new Point3d_GL(0, 0, frame_len);*/

            var ox = new Point3d_GL(m[0, 0] * frame_len, m[0, 1] * frame_len, m[0, 2] * frame_len);
            var oy = new Point3d_GL(m[1, 0] * frame_len, m[1, 1] * frame_len, m[1, 2] * frame_len);
            var oz = new Point3d_GL(m[2, 0] * frame_len, m[2, 1] * frame_len, m[2, 2] * frame_len);
            ox += posit;
            oy += posit;
            oz += posit;
            addFrame(posit, ox, oy, oz);
        }

        public Polygon3d_GL[] addNormals(Polygon3d_GL[] pols,double len)
        {
            var ps = new List<Point3d_GL>();
            for (int i = 0; i < pols.Length; i++)
            {
                //if(pols[i].v3.z<0.5)
                {
                    var p1 = pols[i].ps[0];
                    var p2 = p1 + pols[i].v3 * len;
                    ps.Add(p1); ps.Add(p2);
                    pols[i].ps[0].color = Color3d_GL.blue();
                    pols[i].ps[1].color = Color3d_GL.blue();
                    pols[i].ps[2].color = Color3d_GL.blue();
                }
            }
            //addLineMesh(ps.ToArray(), Color3d_GL.blue());
            return pols;
            //
        }

        public void addCamArea(CameraCV cam, double len, bool paint = true)
        {
            var p0 = cam.matrixCS * new Point3d_GL(0, 0, 0);
            var p1 = cam.matrixCS * (cam.point3DfromCam(new PointF(0, 0)) * len);
            var p2 = cam.matrixCS * (cam.point3DfromCam(new PointF(cam.image_size.Width, 0)) * len);
            var p3 = cam.matrixCS * (cam.point3DfromCam(new PointF(cam.image_size.Width, cam.image_size.Height)) * len);
            var p4 = cam.matrixCS * (cam.point3DfromCam(new PointF(0, cam.image_size.Height)) * len);
            

            var verts = new Point3d_GL[16]
                    {
                p0,p1, p0,p2, p0,p3, p0,p4, p1,p2, p2,p3, p3,p4, p4,p1
                    };
            addMeshWithoutNorm(Point3d_GL.toMesh(verts), PrimitiveType.Lines);

        }
        public void addGLMesh(float[] _mesh, PrimitiveType primitiveType, float x = 0, float y = 0, float z = 0,float scale = 1f, Color3d_GL color = null, string name = "new PointMesh")
        {
            // addMesh(cube_buf, PrimitiveType.Points);
            if (x == 0 && y == 0 && z == 0)
            {
                addMesh(_mesh, primitiveType, color,name);
            }
            else
            {
                addMesh(translateMesh(scaleMesh(_mesh, scale), x, y, z), primitiveType, color, name);
            }

        }
        public float[] translateMesh(float[] _mesh, float x=0, float y=0, float z=0)
        {
            var mesh = new float[_mesh.Length];
            for (int i = 0; i < mesh.Length; i += 3)
            {
                mesh[i] = _mesh[i] + x;
                mesh[i + 1] = _mesh[i + 1] + y;
                mesh[i + 2] = _mesh[i + 2] + z;
            }
            return mesh;
        }
        public float[] scaleMesh(float[] _mesh, float k, float kx = 1.0f, float ky = 1.0f, float kz = 1.0f)
        {
            var mesh = new float[_mesh.Length];
            for (int i = 0; i < mesh.Length; i += 3)
            {
                mesh[i] = _mesh[i] * k * kx;
                mesh[i + 1] = _mesh[i + 1] * k * ky;
                mesh[i + 2] = _mesh[i + 2] * k * kz;
            }
            return mesh;
        }
        public string addPointMesh(Point3d_GL[] points, Color3d_GL color = null, string name = "new PointMesh")
        {
            var mesh = new List<float>();
            foreach (var p in points)
            {
                mesh.Add((float)p.x);
                mesh.Add((float)p.y);
                mesh.Add((float)p.z);
            }
            return addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Points, color, name);
        }
        public string  addLineFanMesh(float[] startpoint, float[] points, Color3d_GL color = null, string name = "new LineFanMesh")
        {
            var mesh = new float[points.Length * 2];
            var j = 0;
            for(int i=0; i<points.Length-3;i+=3)
            {
                mesh[j] = startpoint[0]; j++;
                mesh[j] = startpoint[1]; j++;
                mesh[j] = startpoint[2]; j++;
                mesh[j] = points[i]; j++;
                mesh[j] = points[i+1]; j++;
                mesh[j] = points[i+2]; j++;
            }
            return addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Lines, color,name);
        }
        public string addLineFanMesh(Point3d_GL startpoint, Point3d_GL[] points, Color3d_GL color = null, string name = "new LineFanMesh")
        {
            var mesh = new float[points.Length * 6];
            var j = 0;
            for (int i = 0; i < points.Length; i++)
            {
                mesh[j] =(float)startpoint.x; j++;
                mesh[j] = (float)startpoint.y; j++;
                mesh[j] = (float)startpoint.z; j++;
                mesh[j] = (float)points[i].x; j++;
                mesh[j] = (float)points[i].y; j++;
                mesh[j] = (float)points[i].z; j++;
            }
            return addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Lines, color, name);
        }
        public void addLineMesh(Point3d_GL[] points, Color3d_GL color = null, string name = "new LineMesh")
        {
            var mesh = new List<float>();
            foreach (var p in points)
            {
                mesh.Add((float)p.x);
                mesh.Add((float)p.y);
                mesh.Add((float)p.z);
            }
            addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Lines, color,name);
        }
        public string addLinesMeshTraj(Point3d_GL[][] lines, Color3d_GL color = null, string name = "new LinesMeshTraj")
        {
            var mesh_l = new List<float>();
            foreach (var points in lines)
            {
                var mesh = new List<float>();
                for (int i = 1; i < points.Length; i++)
                {
                    mesh.Add((float)points[i - 1].x);
                    mesh.Add((float)points[i - 1].y);
                    mesh.Add((float)points[i - 1].z);

                    mesh.Add((float)points[i].x);
                    mesh.Add((float)points[i].y);
                    mesh.Add((float)points[i].z);
                }
                mesh_l.AddRange(mesh);
            }
           return  addMeshWithoutNorm(mesh_l.ToArray(), PrimitiveType.Lines, color, name);
        }
        public string addLineMeshTraj(Point3d_GL[] points, Color3d_GL color = null, string name = "new LineMesh")
        {
            var mesh = new List<float>();
            for(int i=1; i<points.Length;i++)
            {
                mesh.Add((float)points[i - 1].x);
                mesh.Add((float)points[i - 1].y);
                mesh.Add((float)points[i - 1].z);

                mesh.Add((float)points[i].x);
                mesh.Add((float)points[i].y);
                mesh.Add((float)points[i].z);
            }
            return addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Lines, color,name);
        }
        public string addLineMesh(Vertex4f[] points, Color3d_GL color = null, string name = "new LineMesh")
        {
            var mesh = new float[points.Length * 3];
            int ind = 0;
            foreach (var p in points)
            {
                mesh[ind] = p.x; ind++;
                mesh[ind] = p.y; ind++;
                mesh[ind] = p.z; ind++;
            }
            return addMeshWithoutNorm(mesh, PrimitiveType.Lines, color,name);
        }
        public string addMeshWithoutNorm(float[] gl_vertex_buffer_data, PrimitiveType primitiveType, Color3d_GL color = null,string name = "new mesh without norm")
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            var color_buffer_data = new float[gl_vertex_buffer_data.Length];
            if (color == null) color = new Color3d_GL();
            for (int i = 0; i < color_buffer_data.Length; i += 3)
            {
                color_buffer_data[i] = color.r;
                color_buffer_data[i + 1] = color.g;
                color_buffer_data[i + 2] = color.b;

                normal_buffer_data[i] = 0.1f;
                normal_buffer_data[i + 1] = 0.1f;
                normal_buffer_data[i + 2] = 0.1f;
            }

            return add_buff_gl(gl_vertex_buffer_data, color_buffer_data, normal_buffer_data, primitiveType,name);
        }
        public string addMeshColor(float[] gl_vertex_buffer_data, float[] gl_color_buffer_data, PrimitiveType primitiveType,string name= "new mesh color")
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            Point3d_GL p1, p2, p3, U, V, Norm1, Norm;
            for (int i = 0; i < normal_buffer_data.Length; i += 9)
            {
                p1 = new Point3d_GL(gl_vertex_buffer_data[i], gl_vertex_buffer_data[i + 1], gl_vertex_buffer_data[i + 2]);
                p2 = new Point3d_GL(gl_vertex_buffer_data[i + 3], gl_vertex_buffer_data[i + 4], gl_vertex_buffer_data[i + 5]);
                p3 = new Point3d_GL(gl_vertex_buffer_data[i + 6], gl_vertex_buffer_data[i + 7], gl_vertex_buffer_data[i + 8]);
                U = p1 - p2;
                V = p1 - p3;
                Norm = new Point3d_GL(
                    U.y * V.z - U.z * V.y,
                    U.z * V.x - U.x * V.z,
                    U.x * V.y - U.y * V.x);
                Norm1 = Norm.normalize();
                normal_buffer_data[i] = (float)Norm1.x;
                normal_buffer_data[i + 1] = (float)Norm1.y;
                normal_buffer_data[i + 2] = (float)Norm1.z;

                normal_buffer_data[i + 3] = (float)Norm1.x;
                normal_buffer_data[i + 4] = (float)Norm1.y;
                normal_buffer_data[i + 5] = (float)Norm1.z;

                normal_buffer_data[i + 6] = (float)Norm1.x;
                normal_buffer_data[i + 7] = (float)Norm1.y;
                normal_buffer_data[i + 8] = (float)Norm1.z;
            }
            // Console.WriteLine("vert len " + gl_vertex_buffer_data.Length);
            return add_buff_gl(gl_vertex_buffer_data, gl_color_buffer_data, normal_buffer_data, primitiveType, name);
        }
        public string addMesh(float[] gl_vertex_buffer_data, PrimitiveType primitiveType, Color3d_GL color = null,string name = "new mesh")
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            Point3d_GL p1,p2,p3,U,V,Norm1,Norm;
            for (int i = 0; i < normal_buffer_data.Length; i += 9)
            {
                try
                {
                    p1 = new Point3d_GL(gl_vertex_buffer_data[i], gl_vertex_buffer_data[i + 1], gl_vertex_buffer_data[i + 2]);
                    p2 = new Point3d_GL(gl_vertex_buffer_data[i + 3], gl_vertex_buffer_data[i + 4], gl_vertex_buffer_data[i + 5]);
                    p3 = new Point3d_GL(gl_vertex_buffer_data[i + 6], gl_vertex_buffer_data[i + 7], gl_vertex_buffer_data[i + 8]);
                    U = p1 - p2;
                    V = p1 - p3;
                    Norm = new Point3d_GL(
                        U.y * V.z - U.z * V.y,
                        U.z * V.x - U.x * V.z,
                        U.x * V.y - U.y * V.x);
                    Norm1 = Norm.normalize();
                    normal_buffer_data[i] = (float)Norm1.x;
                    normal_buffer_data[i + 1] = (float)Norm1.y;
                    normal_buffer_data[i + 2] = (float)Norm1.z;

                    normal_buffer_data[i + 3] = (float)Norm1.x;
                    normal_buffer_data[i + 4] = (float)Norm1.y;
                    normal_buffer_data[i + 5] = (float)Norm1.z;

                    normal_buffer_data[i + 6] = (float)Norm1.x;
                    normal_buffer_data[i + 7] = (float)Norm1.y;
                    normal_buffer_data[i + 8] = (float)Norm1.z;
                }
                catch(System.IndexOutOfRangeException)
                {
                    Console.WriteLine("Mesh IndexOutOfRangeException: "+i +" from "+ gl_vertex_buffer_data.Length);
                }
                
                
            }
            if (color == null) color = new Color3d_GL();
            var color_buffer_data = new float[gl_vertex_buffer_data.Length];
            for (int i = 0; i < color_buffer_data.Length; i += 3)
            {
                color_buffer_data[i] = color.r;
                color_buffer_data[i + 1] = color.g;
                color_buffer_data[i + 2] = color.b;
            }
            //Console.WriteLine( gl_vertex_buffer_data.Length);
            return add_buff_gl(gl_vertex_buffer_data, color_buffer_data, normal_buffer_data, primitiveType,name);
        }


        public float[] z_mesh_from_cont_xy(Point3d_GL[] ps, double z)
        {
            var ps_up = Point3d_GL.add_arr(ps, new Point3d_GL(0, 0, z));
            var ps_down = Point3d_GL.add_arr(ps, new Point3d_GL(0, 0, -z));
            var mesh = Polygon3d_GL.triangulate_two_same_conts(ps_up, ps_down);
            return Polygon3d_GL.toMesh(mesh)[0];
        
        }



        #endregion

        #region shader
   
        string[] assembCode(string[] paths)
        {
            var text = "";
            foreach (var path in paths)
                text += File.ReadAllText(path);
            return new string[] { text };
        }
        void debugShaderComp(uint ShaderName)
        {
            int compiled;

            Gl.GetShader(ShaderName, ShaderParameterName.CompileStatus, out compiled);
            if (compiled != 0)
            {
                Console.WriteLine("SHADER COMPILE");
                return;
            }


            // Throw exception on compilation errors
            const int logMaxLength = 1024;

            StringBuilder infolog = new StringBuilder(logMaxLength);
            int infologLength;

            Gl.GetShaderInfoLog(ShaderName, logMaxLength, out infologLength, infolog);

            throw new InvalidOperationException($"unable to compile shader: {infolog}");
        }
        private uint compileShader(string[] shSource, ShaderType shaderType)
        {
            uint ShaderID = Gl.CreateShader(shaderType);            
            Gl.ShaderSource(ShaderID, shSource);
            Gl.CompileShader(ShaderID);
            debugShaderComp(ShaderID);
            return ShaderID;
        }
        private uint createShader(string[] VertexSourceGL, string[] GeometryShaderGL, string[] FragmentSourceGL)
        {
            bool geom = false;
            uint GeometryShaderID = 0;
            if (GeometryShaderGL != null)
            {
                geom = true;
            }
            var VertexShaderID = compileShader(VertexSourceGL, ShaderType.VertexShader);
            var FragmentShaderID = compileShader(FragmentSourceGL, ShaderType.FragmentShader);
            if (geom)
            {
                GeometryShaderID = compileShader(GeometryShaderGL, ShaderType.GeometryShader);
            }

            uint ProgrammID = Gl.CreateProgram();
            Gl.AttachShader(ProgrammID, VertexShaderID);
            Gl.AttachShader(ProgrammID, FragmentShaderID);
            if (geom)
            {
                Gl.AttachShader(ProgrammID, GeometryShaderID);
            }
            Gl.LinkProgram(ProgrammID);

            int linked;

            Gl.GetProgram(ProgrammID, ProgramProperty.LinkStatus, out linked);

            if (linked == 0)
            {
                const int logMaxLength = 1024;

                StringBuilder infolog = new StringBuilder(logMaxLength);
                int infologLength;

                Gl.GetProgramInfoLog(ProgrammID, 1024, out infologLength, infolog);

                throw new InvalidOperationException($"unable to link program: {infolog}");
            }

            Gl.DeleteShader(VertexShaderID);
            Gl.DeleteShader(FragmentShaderID);
            if (geom)
            {
                Gl.DeleteShader(GeometryShaderID);
            }
            return ProgrammID;
        }
        private uint createShaderCompute(string[] ComputeSourceGL)
        {

            var ComputeShaderID = compileShader(ComputeSourceGL, ShaderType.ComputeShader);

            uint ProgrammID = Gl.CreateProgram();
            Gl.AttachShader(ProgrammID, ComputeShaderID);
            Gl.LinkProgram(ProgrammID);

            int linked;

            Gl.GetProgram(ProgrammID, ProgramProperty.LinkStatus, out linked);

            if (linked == 0)
            {
                const int logMaxLength = 1024;

                StringBuilder infolog = new StringBuilder(logMaxLength);
                int infologLength;

                Gl.GetProgramInfoLog(ProgrammID, 1024, out infologLength, infolog);

                throw new InvalidOperationException($"unable to link program: {infolog}");
            }

            Gl.DeleteShader(ComputeShaderID);
            return ProgrammID;
        }

        #endregion


    }

    /*
     * 
     * private  string[] _VertexSourceGL = {
            "#version 460 core\n",


            "in vec3 _vertexPosition_modelspace;\n",
            "in vec3 _vertexNormal_modelspace;\n",
            "in vec3 _vertexColor;\n",

            "out VS_GS_INTERFACE\n",
            "{\n",
            "vec3 vertexPosition_modelspace;\n",
            "vec3 vertexNormal_modelspace;\n",
            "vec3 vertexColor;\n",
            "} vs_out;\n",

            "void main() {\n",
            "   gl_Position = vec4(_vertexPosition_modelspace, 1.0);\n",
            "	vs_out.vertexPosition_modelspace = _vertexPosition_modelspace;\n",
            "	vs_out.vertexNormal_modelspace = _vertexNormal_modelspace;\n",
            "	vs_out.vertexColor = _vertexColor;\n",
            "}\n"
        };
        private  string[] _GeometryShaderBody = {
            "uniform mat4 MVP;\n",
            "uniform mat4 M;\n",
            "uniform mat4 V;\n",
            "uniform vec3 LightPosition_worldspace;\n",

            "in VS_GS_INTERFACE\n",
            "{\n",
            "vec3 vertexPosition_modelspace;\n",
            "vec3 vertexNormal_modelspace;\n",
            "vec3 vertexColor;\n",
              "}vs_out[];\n",

            "out GS_FS_INTERFACE\n",
            "{\n",
            "vec3 Position_worldspace;\n",
            "vec3 Color;\n",
            "vec3 Normal_cameraspace;\n",
            "vec3 EyeDirection_cameraspace;\n",
            "vec3 LightDirection_cameraspace;\n",
            "} fs_in;\n",

            "uniform mat4 MVPs[4];\n",
            "uniform mat4 Ms[4];\n",
            "uniform mat4 Vs[4];\n",



            "void main() {\n",

            "   for (int i = 0; i < gl_in.length(); i++){ \n",
            "	    gl_ViewportIndex = gl_InvocationID;\n",

            "       gl_Position = MVPs[gl_InvocationID] * vec4(vs_out[i].vertexPosition_modelspace, 1.0);\n",

            "	    fs_in.Position_worldspace = (M * vec4(vs_out[i].vertexPosition_modelspace,1)).xyz;\n",

            "	    vec3 vertexPosition_cameraspace = ( Vs[gl_InvocationID] * Ms[gl_InvocationID] * vec4(vs_out[i].vertexPosition_modelspace,1)).xyz;\n",

            "	    float[16] mx = float[](0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0);\n",

            "	    fs_in.EyeDirection_cameraspace = vec3(0,0,0) - vertexPosition_cameraspace;\n",
            "	    vec3 LightPosition_cameraspace = ( Vs[gl_InvocationID] * vec4(LightPosition_worldspace,1)).xyz;\n",
            "	    fs_in.LightDirection_cameraspace = LightPosition_cameraspace + fs_in.EyeDirection_cameraspace;\n",
            "	    fs_in.Normal_cameraspace = ( Vs[gl_InvocationID] * Ms[gl_InvocationID] * vec4(vs_out[i].vertexNormal_modelspace,0)).xyz;\n",
            "	    fs_in.Color = vs_out[i].vertexColor;\n",

            "	    EmitVertex();}\n",


            "}\n"
        };

        private string[] _GeometryShaderLinesGL = {
            "#version 460 core\n",

            "layout (lines, invocations = 4) in;\n",
            "layout (line_strip, max_vertices = 2) out;\n", 
        };

        private  string[] _GeometryShaderPointsGL = {
            "#version 460 core\n",

            "layout (points, invocations = 4) in;\n",
            "layout (points, max_vertices = 1) out;\n", 
        };

        private string[] _GeometryShaderTrianglesGL = {
            "#version 460 core\n",

            "layout (triangles, invocations = 4) in;\n",
            "layout (triangle_strip, max_vertices = 3) out;\n",

        };

        private string[] _FragmentSourceGL = {
            "#version 460 core\n",
            "uniform vec3 LightPosition_worldspace;\n",
            "uniform vec3 MaterialDiffuse;\n",
            "uniform vec3 MaterialAmbient;\n",
            "uniform vec3 MaterialSpecular;\n",
            "uniform float lightPower;\n",


            "in GS_FS_INTERFACE\n",
            "{\n",
            "vec3 Position_worldspace;\n",
            "vec3 Color;\n",
            "vec3 Normal_cameraspace;\n",
            "vec3 EyeDirection_cameraspace;\n",
            "vec3 LightDirection_cameraspace;\n",
              "}fs_in;\n",


            //"in int  gl_ViewportIndex;\n",

            "out vec4 color;\n",
            "void main() {\n",
            "	vec3 LightColor = vec3(1,1,1);\n",
            "	float LightPower = lightPower;\n",
            "	vec3 MaterialDiffuseColor = MaterialDiffuse;\n",
            "	vec3 MaterialAmbientColor = MaterialAmbient;\n",
            "	vec3 MaterialSpecularColor = MaterialSpecular;\n",
            "	float distance = length( LightPosition_worldspace - fs_in.Position_worldspace );\n",
            "	vec3 n = normalize( fs_in.Normal_cameraspace );\n",
            "	vec3 l = normalize( fs_in.LightDirection_cameraspace );\n",
            "	float cosTheta = clamp( dot( n,l ), 0,1 );\n",
            "	vec3 E = normalize(fs_in.EyeDirection_cameraspace);\n",
            "	vec3 R = reflect(-l,n);\n",
            "	float cosAlpha = clamp( dot( E,R ), 0,1 );\n",
             "	MaterialDiffuseColor = fs_in.Color;\n",
            "	color.xyz = MaterialAmbientColor + MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance*distance) +MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha,5) / (distance*distance);\n",
            "	color.w = 1.0;\n",
          //  "	if(gl_ViewportIndex==0)\n",
           // "	{color.xyz =  vec3(1,0,0);}\n",
          //  "	if(gl_ViewportIndex==1)\n",
          //  "	{color.xyz =  vec3(0,1,0);}\n",
            //"	if(gl_ViewportIndex);\n",
           // "	color.xyz = Color;\n",
          //  "	float color_grey = (color_true.x+color_true.y+color_true.z)/3;\n",
           // "	color = vec3(color_grey,color_grey,color_grey);\n",
            "}\n"
        };


        private string[] _FragmentSourceGL_ = {
            "#version 460 core\n",
            "uniform vec3 LightPosition_worldspace;\n",
            "uniform vec3 MaterialDiffuse;\n",
            "uniform vec3 MaterialAmbient;\n",
            "uniform vec3 MaterialSpecular;\n",
            "uniform float lightPower;\n",
            "in vec3 Color;\n",
            "in vec3 Position_worldspace;\n",
            "in vec3 Normal_cameraspace;\n",
            "in vec3 EyeDirection_cameraspace;\n",
            "in vec3 LightDirection_cameraspace;\n",
            "out vec4 color;\n",
            "void main() {\n",
            "	vec3 LightColor = vec3(1,1,1);\n",
            "	float LightPower = lightPower;\n",
            "	vec3 MaterialDiffuseColor = MaterialDiffuse;\n",
            "	vec3 MaterialAmbientColor = MaterialAmbient;\n",
            "	vec3 MaterialSpecularColor = MaterialSpecular;\n",
            "	float distance = length( LightPosition_worldspace - Position_worldspace );\n",
            "	vec3 n = normalize( Normal_cameraspace );\n",
            "	vec3 l = normalize( LightDirection_cameraspace );\n",
            "	float cosTheta = clamp( dot( n,l ), 0,1 );\n",
            "	vec3 E = normalize(EyeDirection_cameraspace);\n",
            "	vec3 R = reflect(-l,n);\n",
            "	float cosAlpha = clamp( dot( E,R ), 0,1 );\n",
             "	MaterialDiffuseColor = Color;\n",
            "	color.xyz = MaterialAmbientColor + MaterialDiffuseColor * LightColor * LightPower * cosTheta / (distance*distance) +MaterialSpecularColor * LightColor * LightPower * pow(cosAlpha,5) / (distance*distance);\n",
            "	color.w = 1.0;\n",
           // "	color.xyz = Color;\n",
          //  "	float color_grey = (color_true.x+color_true.y+color_true.z)/3;\n",
           // "	color = vec3(color_grey,color_grey,color_grey);\n",
            "}\n"
        };
        private string[] _GeometryShaderGL_ = {
            "#version 460 core\n",
            //"uniform mat4 MVP;\n",
            //"uniform mat4 M;\n",
            //"uniform mat4 V;\n",
            //"uniform vec3 LightPosition_worldspace;\n",

           // "in vec3 vertexPosition_modelspace;\n",
           // "in vec3 vertexNormal_modelspace;\n",
           // "in vec3 vertexColor;\n",


           // "out vec3 Color;\n",
           // "out vec3 Position_worldspace;\n",
           // "out vec3 Normal_cameraspace;\n",
           // "out vec3 EyeDirection_cameraspace;\n",
           // "out vec3 LightDirection_cameraspace;\n",
            "void main() {\n",
            "   for (int i = 0; i < gl_in.length(); i++)\n",
            "	gl_ViewportIndex = gl_InvocationID;\n",
            "	gs_color = colors[gl_InvocationID];\n",
            "	gs_normal = (model_matrix[gl_InvocationID] * vec4(vs_normal[i], 0.0)).xyz;\n",
            "	gl_Position = projection_matrix * (model_matrix[gl_InvocationID] * gl_in[i].gl_Position);\n",
            "	EmitVertex();\n",
           // "	Normal_cameraspace = ( V * M * vec4(vertexNormal_modelspace,0)).xyz;\n",
           // "	Color = vertexColor;\n",
            "}\n"
        };
     * 
     */
}
