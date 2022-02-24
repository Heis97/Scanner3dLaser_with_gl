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
    public class GraphicGL
    {
        #region vars
        static float PI = 3.1415926535f;
        public CameraCV cameraCV;
        public int startGen = 0;
        public int saveImagesLen = 0;
        public int renderdelim = 15;
        public int rendercout = 0;
        public viewType typeProj = viewType.Perspective;
        Size sizeControl;
        Point lastPos;
        uint buff_pos;
        uint buff_color;
        uint buff_normal;
        uint programID_ps;
        uint programID_trs;
        uint programID_lns;
        int[] LocationMVPs = new int[4];
        int[] LocationMs = new int[4];
        int[] LocationVs = new int[4];
        int[] LocationPs = new int[4];
        int LocationMVP;
        int LocationV;
        int LocationM;
        int LightID;
        int LightPowerID;
        int MaterialDiffuseID;
        int MaterialAmbientID;
        int currentMonitor = 1;
        int MaterialSpecularID;
        float LightPower = 500000.0f;
        Label Label_cor;
        Label Label_cor_cur;
        Label Label_trz_cur;
        RichTextBox debug_box;
        Matrix4x4f Pm;
        Matrix4x4f Vm;
        public BuffersGl buffersGl = new BuffersGl();
        Matrix4x4f Mm;
        Matrix4x4f MVP;
        public Matrix4x4f[] MVPs;
        public Matrix4x4f[] Ms;
        public Matrix4x4f[] Vs;
        public Matrix4x4f[] Ps;
        Vertex3f lightPos = new Vertex3f(0.0f, 0.0f, 123.0f);
        Vertex3f MaterialDiffuse = new Vertex3f(0.1f, 0.1f, 0.1f);
        Vertex3f MaterialAmbient = new Vertex3f(0.1f, 0.1f, 0.1f);
        Vertex3f MaterialSpecular = new Vertex3f(0.1f, 0.1f, 0.1f);
        public modeGL modeGL = modeGL.View;
        List<Point3d_GL> pointsPaint = new List<Point3d_GL>();
        Point3d_GL curPointPaint = new Point3d_GL(0, 0, 0);
        public List<TransRotZoom> transRotZooms = new List<TransRotZoom>();
        public List<TransRotZoom[]> trzForSave;
        public int[] monitorsForGenerate;
        public string pathForSave;
        public ImageBox[] imageBoxesForSave;
        public Size size = new Size(1,1);
        #endregion
       
        public void glControl_Render(object sender, GlControlEventArgs e)
        {
            MVPs = new Matrix4x4f[4];
            Ms = new Matrix4x4f[4];
            Vs = new Matrix4x4f[4];
            Ps = new Matrix4x4f[4];
            var txt = "";
            for (int i = 0; i < transRotZooms.Count; i++)
            {
                Gl.ViewportIndexed((uint)i,
                    transRotZooms[i].rect.X,
                    transRotZooms[i].rect.Y,
                    transRotZooms[i].rect.Width,
                    transRotZooms[i].rect.Height);
                var retM = compMVPmatrix(transRotZooms[i]);               
                MVPs[i] = retM[3];
                Ms[i] = retM[2];
                Vs[i] = retM[1];
                Ps[i] = retM[0];

                txt += "TRZ " + i + ": "+transRotZooms[i].getInfo(transRotZooms.ToArray()).ToString()+"\n";
            }
            Label_trz_cur.Text = txt;
            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            addCams();
            if (buffersGl.objs_out!=null)
            {
                if (buffersGl.objs_out.Count!=0)
                {
                    foreach (var opgl_obj in buffersGl.objs_out)
                    {
                        renderGlobj(opgl_obj);  
                    }
                }
            }

            rendercout++;
            if(rendercout%renderdelim==0)
            {
                rendercout = 0;
            }
        }
        void addCams()
        {
           foreach(var trz in transRotZooms)
            {
                addCamView(trz.id);
            }

        }
        void renderGlobj(openGlobj opgl_obj)
        {
            if(opgl_obj.visible)
            {
                try
                {
                    uint prog = 0;
                    if (opgl_obj.tp == PrimitiveType.Points)
                    {
                        prog = programID_ps;
                    }
                    else if (opgl_obj.tp == PrimitiveType.Triangles)
                    {
                        prog = programID_trs;
                    }
                    else if (opgl_obj.tp == PrimitiveType.Lines)
                    {
                        prog = programID_lns;
                    }
                    load_vars_gl(prog);
                    load_buff_gl(opgl_obj.vertex_buffer_data, opgl_obj.color_buffer_data, opgl_obj.normal_buffer_data);
                    Gl.DrawArrays(opgl_obj.tp, 0, (int)(opgl_obj.vertex_buffer_data.Length / 3));
                    Gl.DeleteBuffers(buff_color);
                    Gl.DeleteBuffers(buff_pos);
                    Gl.DeleteBuffers(buff_normal);
                }
                catch
                {
                }
            }
            
        }
        string[] stringAdd(string[] st1, string[] st2)
        {
            var st_ret = new string[st1.Length+st2.Length];
            for(int i=0; i<st1.Length;i++)
            {
                st_ret[i] = st1[i];
            }
            for (int i = 0; i < st2.Length; i++)
            {
                st_ret[st1.Length+i] = st2[i];
            }
            return st_ret;
        }
        public void glControl_ContextCreated(object sender, GlControlEventArgs e)
        {
            sizeControl = ((Control)sender).Size;
            Gl.Initialize();
            Gl.Enable(EnableCap.Multisample);
            Gl.ClearColor(0.9f, 0.9f, 0.95f, 0.0f);
            Gl.PointSize(2f);

            var VertexSourceGL = assembCode(new string[] { @"Graphic\Shaders\Vert\VertexSh.txt" });
            var FragmentSourceGL = assembCode(new string[] { @"Graphic\Shaders\Frag\FragmSh.txt" });
            var GeometryShaderPointsGL = assembCode(new string[] { @"Graphic\Shaders\Geom_R\GeomShP_head.txt", @"Graphic\Shaders\Geom_R\GeomSh_body.txt" });
            var GeometryShaderLinesGL = assembCode(new string[] { @"Graphic\Shaders\Geom_R\GeomShL_head.txt", @"Graphic\Shaders\Geom_R\GeomSh_body.txt" });
            var GeometryShaderTrianglesGL = assembCode(new string[] { @"Graphic\Shaders\Geom_R\GeomShT_head.txt", @"Graphic\Shaders\Geom_R\GeomSh_body.txt" });

            programID_lns = createShader(VertexSourceGL, GeometryShaderLinesGL, FragmentSourceGL);
            programID_ps = createShader(VertexSourceGL, GeometryShaderPointsGL, FragmentSourceGL);
            programID_trs = createShader(VertexSourceGL, GeometryShaderTrianglesGL , FragmentSourceGL);
            //Gl.Enable(EnableCap.CullFace);
            Gl.Enable(EnableCap.DepthTest);
            Gl.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            cameraCV = new CameraCV(UtilOpenCV.matrixForCamera(new Size(400, 400), 53), new Matrix<double>(5, 1));
            cameraCV.distortmatrix[0,0] = -0.1;


        }
        string[] assembCode(string[] paths)
        {
            var text = "";
            foreach (var path in paths)
                text += File.ReadAllText(path);
            return new string[] { text };
        }
        
        private void load_buff_gl(float[] vertex_buffer_dat, float[] color_buffer_dat, float[] normal_buffer_dat)
        {
            buff_pos = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff_pos);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4 * vertex_buffer_dat.Length), vertex_buffer_dat, BufferUsage.StaticDraw);            

            buff_color = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff_color);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4 * color_buffer_dat.Length), color_buffer_dat, BufferUsage.StaticDraw);

            buff_normal = Gl.GenBuffer();
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff_normal);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(4 * normal_buffer_dat.Length), normal_buffer_dat, BufferUsage.StaticDraw);
            
            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff_pos);
            Gl.VertexAttribPointer(0, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            Gl.EnableVertexAttribArray(0);

            Gl.BindBuffer(BufferTarget.ArrayBuffer , buff_color);
            Gl.VertexAttribPointer(2, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            Gl.EnableVertexAttribArray(2);

            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff_normal);
            Gl.VertexAttribPointer(1, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            Gl.EnableVertexAttribArray(1);
            
        }
        private void load_vars_gl(uint prog)
        {

            Gl.UseProgram(prog);

            for (int i=0; i<4; i++)
            {
                LocationMVPs[i] = Gl.GetUniformLocation(prog, "MVPs["+i+"]");
                LocationMs[i] = Gl.GetUniformLocation(prog, "Ms[" + i + "]");
                LocationVs[i] = Gl.GetUniformLocation(prog, "Vs[" + i + "]");
                LocationPs[i] = Gl.GetUniformLocation(prog, "Ps[" + i + "]");
            }
            
            LocationMVP = Gl.GetUniformLocation(prog, "MVP");
            LocationV = Gl.GetUniformLocation(prog, "V");
            LocationM = Gl.GetUniformLocation(prog, "M");
            MaterialDiffuseID = Gl.GetUniformLocation(prog, "MaterialDiffuse");
            MaterialAmbientID = Gl.GetUniformLocation(prog, "MaterialAmbient");
            MaterialSpecularID = Gl.GetUniformLocation(prog, "MaterialSpecular");
            LightID = Gl.GetUniformLocation(prog, "LightPosition_world");
            LightPowerID = Gl.GetUniformLocation(prog, "lightPower");

            
            for (int i = 0; i < 4; i++)
            {
                Gl.UniformMatrix4f(LocationMVPs[i], 1, false, MVPs[i]);
                Gl.UniformMatrix4f(LocationMs[i], 1, false, Ms[i]);
                Gl.UniformMatrix4f(LocationVs[i], 1, false, Vs[i]);
                Gl.UniformMatrix4f(LocationPs[i], 1, false, Ps[i]);
            }

            
            Gl.UniformMatrix4f(LocationMVP, 1, false, MVP);
            Gl.UniformMatrix4f(LocationM, 1, false, Mm);
            Gl.UniformMatrix4f(LocationV, 1, false, Vm);

            Gl.Uniform3f(MaterialDiffuseID, 1, MaterialDiffuse);
            Gl.Uniform3f(MaterialAmbientID, 1, MaterialAmbient);
            Gl.Uniform3f(MaterialSpecularID, 1, MaterialSpecular);
            Gl.Uniform3f(LightID, 1, lightPos);
            Gl.Uniform1f(LightPowerID, 1, LightPower);
            
        }
        

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
                var _Pm = ProjmatrF(53f);              
                var _Vm = Transmatr((float)off_x, -(float)off_y, (float)zoom * (float)off_z) * RotXmatr(xRot) * RotYmatr(yRot) * RotZmatr(zRot);
                Pm = new Matrix4x4f((_Pm).data);
                Vm = new Matrix4x4f((_Vm).data);
                Mm = Matrix4x4f.Identity;
                MVP = new Matrix4x4f((_Pm * _Vm).data);
            }
            else if (trz.viewType_ == viewType.Ortho)
            {
                var _Pm = OrthoF();
                var _Vm = Transmatr((float)off_x, -(float)off_y, (float)zoom * (float)off_z) * RotXmatr(xRot) * RotYmatr(yRot) * RotZmatr(zRot);
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

        static public Matr4x4f OrthoF(float fx = 1, float aspec = 1, float n = 0.1f, float f = 3000f)
        {
            var fy = fx / aspec;
            var a = (f + n) / (f - n);
            var b = (-2 * f * n) / (f - n);
            var cx = 1 / (float)Math.Tan(toRad(fx) / 2);
            var cy = 1 / (float)Math.Tan(toRad(fy) / 2);
            var data = new float[] {
                 1, 0, 0, 0 ,
                 0, 1, 0, 0 ,
                 0, 0, 1,  0,
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
            var p2 =new  Matr4x4f( MVPs[id]) * new Vert4f(point);
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

        public void printDebug(RichTextBox box)
        {
            string txt = "";
            foreach(var ob in buffersGl.objs_out)
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
        public void add_TextBox(RichTextBox richTextBox)
        {
            debug_box = richTextBox;
        }
        public void addMonitor(Rectangle rect,int id)
        {
            transRotZooms.Add(new TransRotZoom(rect,id));
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
                        
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        pointsPaint.Clear();
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
                        trz.yRot -= dx;
                        trz.zRot += dz;
                        
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        trz.off_x += Convert.ToDouble(dx);
                        trz.off_y += Convert.ToDouble(dy);
                    }
                    lastPos = e.Location;
                    break;
                case modeGL.Paint:
                    var p_XY = new Point3d_GL((double)e.Location.X/ (0.5*(double)w), (double)e.Location.Y/(0.5* (double)h), 0);
                   // var p_YZ = new Point3d_GL(0,(double)e.Location.X / (0.5 * (double)w), (double)e.Location.Y / (0.5 * (double)h));
                   // var p_ZX = new Point3d_GL((double)e.Location.X / (0.5 * (double)w),0, (double)e.Location.Y / (0.5 * (double)h));
                    try
                    {
                        var invM = Pm.Inverse;
                        //var invM = MVPs[0].Inverse;
                        if (Label_cor != null)
                        {
                            curPointPaint = invM * p_XY;
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
                                var dis = (pointsPaint[pointsPaint.Count - 1] - pointsPaint[pointsPaint.Count - 2]).magnitude();
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
 
        void addCamView(int id)
        {
            var trz = transRotZooms[selectTRZ_id(id)].getInfo(transRotZooms.ToArray());
            if (trz.visible == true)
            {
                if (trz.viewType_ == viewType.Perspective)
                {
                   /* var p1 = new Vertex4f(0, 0, 0.01f, 1f);
                    double _z = 1000;
                    double _x = _z * Math.Tan(toRad(53 / 2)), _y = _z * Math.Tan(toRad(53 / 2));
                    float x = (float)_x; float y = (float)_y; float z = (float)_z;
                    var p2 = new Vertex4f(-x, -y, z, 1);
                    var p3 = new Vertex4f(-x, y, z, 1);
                    var p4 = new Vertex4f(x, -y, z, 1);
                    var p5 = new Vertex4f(x, y, z, 1);
                    p1 = Vs[id].Inverse.Transposed * p1;
                    p2 = Vs[id].Inverse.Transposed * p2;
                    p3 = Vs[id].Inverse.Transposed * p3;
                    p4 = Vs[id].Inverse.Transposed * p4;
                    p5 = Vs[id].Inverse.Transposed * p5;
                    var verts = new Vertex4f[16]
                    {
                p1,p2, p1,p3, p1,p4, p1,p5, p2,p3, p3,p5, p5,p4, p4,p2
                    };
                    add_buff_gl_lines_id(toFloat(verts), id,true);*/
                   
                }
                else if (trz.viewType_ == viewType.Ortho)
                {
                    
                    double _z = -2000;
                    double _z0 = 0.1;
                    double _x = 100 * trz.zoom;
                    double _y = 100 * trz.zoom;
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
                    for (int i = 0; i < p.Length; i++)
                    {
                        p[i] = Vs[id].Inverse * p[i];
                    }

                    var verts = new Vertex4f[24]
                    {
                p[0],p[1], p[1],p[2], p[2],p[3], p[3],p[0],
                p[4],p[5], p[5],p[6], p[6],p[6], p[7],p[4],
                p[0],p[4], p[1],p[5], p[2],p[6], p[3],p[7]
                    };
                    add_buff_gl_lines_id(toFloat(verts), id, true);
                }
            }
            else
            {
                var verts = new float[1] { 0 };
                add_buff_gl_lines_id(verts, id, false);
            }
            
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

        public void add_buff_gl(float[] data_v, float[] data_c, float[] data_n, PrimitiveType tp)
        {            
            buffersGl.add_obj(new openGlobj(data_v, data_c, data_n, tp));
        }
        public void add_buff_gl_id(float[] data_v, float[] data_c, float[] data_n, PrimitiveType tp,int id)
        {
            buffersGl.add_obj(new openGlobj(data_v, data_c, data_n, tp,id));
        }
        public void add_buff_gl_lines_id(float[] data_v, int id, bool visible)
        {
            buffersGl.add_obj_id(data_v,id, visible, PrimitiveType.Lines);
        }
        public void add_buff_gl_mesh_id(float[] data_v, int id, bool visible)
        {
            buffersGl.add_obj_id(data_v, id, visible, PrimitiveType.Triangles);
        }
        public void remove_buff_gl_id(int id)
        {
            buffersGl.removeObj(id);
        }
        public void addFrame(Point3d_GL pos, Point3d_GL x, Point3d_GL y, Point3d_GL z)
        {
            addLineMesh(new Point3d_GL[] { pos, x }, 1.0f, 0, 0);
            addLineMesh(new Point3d_GL[] { pos, y }, 0, 1.0f, 0);
            addLineMesh(new Point3d_GL[] { pos, z }, 0, 0, 1.0f);
        }

        public void addFrame_Cam(Camera cam, int frame_len = 15)
        {

            addFrame(cam.pos, cam.pos + cam.oX * frame_len, cam.pos + cam.oY * frame_len, cam.pos + cam.oZ * frame_len * 1.3);
        }

        public void addFrame_Cam(CameraCV cam, int frame_len = 15)
        {
            var posit = cam.matrixCS * new Point3d_GL(0, 0, 0);
            var ox = cam.matrixCS * new Point3d_GL( frame_len, 0, 0);
            var oy = cam.matrixCS * new Point3d_GL( 0, frame_len, 0);
            var oz = cam.matrixCS * new Point3d_GL( 0, 0, frame_len);
            addFrame(posit, ox, oy, oz);
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
        public void addGLMesh(float[] _mesh, PrimitiveType primitiveType, float x = 0, float y = 0, float z = 0, float r = 0.1f, float g = 0.1f, float b = 0.1f, float scale = 1f)
        {
            // addMesh(cube_buf, PrimitiveType.Points);
            if (x == 0 && y == 0 && z == 0)
            {
                addMesh(_mesh, primitiveType, r, g, b);
            }
            else
            {
                addMesh(translateMesh(scaleMesh(_mesh, scale), x, y, z), primitiveType, r, g, b);
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
        public void addPointMesh(Point3d_GL[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new List<float>();
            foreach (var p in points)
            {
                mesh.Add((float)p.x);
                mesh.Add((float)p.y);
                mesh.Add((float)p.z);
            }
            addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Points, r, g, b);
        }
        public void addLineFanMesh(float[] startpoint, float[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
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
            addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Lines, r, g, b);
        }
        public void addLineMesh(Point3d_GL[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new List<float>();
            foreach (var p in points)
            {
                mesh.Add((float)p.x);
                mesh.Add((float)p.y);
                mesh.Add((float)p.z);
            }
            addMeshWithoutNorm(mesh.ToArray(), PrimitiveType.Lines, r, g, b);
        }
        public void addLineMesh(Vertex4f[] points, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var mesh = new float[points.Length * 3];
            int ind = 0;
            foreach (var p in points)
            {
                mesh[ind] = p.x; ind++;
                mesh[ind] = p.y; ind++;
                mesh[ind] = p.z; ind++;
            }
            addMeshWithoutNorm(mesh, PrimitiveType.Lines, r, g, b);
        }
        public void addMeshWithoutNorm(float[] gl_vertex_buffer_data, PrimitiveType primitiveType, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            var color_buffer_data = new float[gl_vertex_buffer_data.Length];
            for (int i = 0; i < color_buffer_data.Length; i += 3)
            {
                color_buffer_data[i] = r;
                color_buffer_data[i + 1] = g;
                color_buffer_data[i + 2] = b;

                normal_buffer_data[i] = 0.1f;
                normal_buffer_data[i + 1] = 0.1f;
                normal_buffer_data[i + 2] = 0.1f;
            }
            add_buff_gl(gl_vertex_buffer_data, color_buffer_data, normal_buffer_data, primitiveType);
        }
        public void addMeshColor(float[] gl_vertex_buffer_data, float[] gl_color_buffer_data, PrimitiveType primitiveType, float r = 0.1f, float g = 0.1f, float b = 0.1f)
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
            add_buff_gl(gl_vertex_buffer_data, gl_color_buffer_data, normal_buffer_data, primitiveType);
        }
        public void addMesh(float[] gl_vertex_buffer_data, PrimitiveType primitiveType, float r = 0.1f, float g = 0.1f, float b = 0.1f)
        {
            var normal_buffer_data = new float[gl_vertex_buffer_data.Length];
            Point3d_GL p1,p2,p3,U,V,Norm1,Norm;
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
            var color_buffer_data = new float[gl_vertex_buffer_data.Length];
            for (int i = 0; i < color_buffer_data.Length; i += 3)
            {
                color_buffer_data[i] = r;
                color_buffer_data[i + 1] = g;
                color_buffer_data[i + 2] = b;
            }
            add_buff_gl(gl_vertex_buffer_data, color_buffer_data, normal_buffer_data, primitiveType);
        }
        #endregion


        #region shader
        void debugShaderComp(uint ShaderName)
        {
            int compiled;

            Gl.GetShader(ShaderName, ShaderParameterName.CompileStatus, out compiled);
            if (compiled != 0)
            {
                //Console.WriteLine("SHADER COMPILE");
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

            var VertexShaderID = compileShader(VertexSourceGL, ShaderType.VertexShader);
            var GeometryShaderID = compileShader(GeometryShaderGL, ShaderType.GeometryShader);
            var FragmentShaderID = compileShader(FragmentSourceGL, ShaderType.FragmentShader);

            uint ProgrammID = Gl.CreateProgram();
            Gl.AttachShader(ProgrammID, VertexShaderID);
            Gl.AttachShader(ProgrammID, GeometryShaderID);
            Gl.AttachShader(ProgrammID, FragmentShaderID);
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
            Gl.DeleteShader(GeometryShaderID);
            Gl.DeleteShader(FragmentShaderID);
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
