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
    public struct STLmodel
    {
        public string path;
        public STLmodel(string _path)
        {
            path = _path;
        }
        public double parseE(string num)
        {
            if(num.Contains("e"))
            {
                var splnum = num.Split(new char[] { 'e' });
                return Convert.ToDouble(splnum[0]) * Math.Pow(10, Convert.ToInt32(splnum[1]));
            }
            else if(num.Contains("E"))
            {
                var splnum = num.Split(new char[] { 'E' });
                return Convert.ToDouble(splnum[0]) * Math.Pow(10, Convert.ToInt32(splnum[1]));
            }
            else
            {
                return Convert.ToDouble(num);
            }
            
        }
        public float[] parsingStl_GL4(string path)
        {
           // var offx = 200;
           // var offy = 500;
          //  var offz = 600;
            string file1;            
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            int len = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 3)
                {
                    if (vert[0].Contains("ert"))
                    {
                        len += 3;
                    }

                }
            }
            float[] ret1 = new float[len];

            int i2 = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });

                if (vert.Length > 3)
                {
                   if (vert[0].Contains("ert"))
                    {
                        ret1[i2] = (float)parseE(vert[1]); i2++;
                        ret1[i2] = (float)parseE(vert[2]); i2++;
                        ret1[i2] = (float)parseE(vert[3]); i2++;
                    }

                }
            }
            return ret1;
        }
        public List<double[,]> parsingStl_GL2(string path)
        {
            int i2 = 0;
            string file1;
            List<double[,]> ret1 = new List<double[,]>();
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            double[,] norm = new double[(int)(lines.Length / 7), 3];
            double[,] p1   = new double[(int)(lines.Length / 7), 3];
            double[,] p2   = new double[(int)(lines.Length / 7), 3];
            double[,] p3   = new double[(int)(lines.Length / 7), 3];
            Console.WriteLine((int)(lines.Length / 7));
            Console.WriteLine("-------------------");
            int i3 = 0;
            foreach (string str in lines)
            {
                string ver = str.Trim();
                string[] vert = ver.Split(new char[] { ' ' });
                if (vert.Length > 3)
                {
                    if (vert[1].Contains("orma"))
                    {
                        norm[i2, 0] = parseE(vert[2]);
                        norm[i2, 1] = parseE(vert[3]);
                        norm[i2, 2] = parseE(vert[4]);
                        
                        i3 = 0;
                    }
                    else if (vert[0].Contains("ert") && i3 == 0)
                    {
                        p1[i2, 0] = parseE(vert[1]);
                        p1[i2, 1] = parseE(vert[2]);
                        p1[i2, 2] = parseE(vert[3]);
                        i3++;
                    }
                    else if (vert[0].Contains("ert") && i3 == 1)
                    {
                        p2[i2, 0] = parseE(vert[1]);
                        p2[i2, 1] = parseE(vert[2]);
                        p2[i2, 2] = parseE(vert[3]);
                        i3++;
                    }
                    else if (vert[0].Contains("ert") && i3 == 2)
                    {
                        p3[i2, 0] = parseE(vert[1]);
                        p3[i2, 1] = parseE(vert[2]);
                        p3[i2, 2] = parseE(vert[3]);
                        i2++;
                    }
                }
            }
        
            Console.WriteLine("-------------------");
            Console.WriteLine(i2);
            ret1.Add(norm);
            ret1.Add(p1);
            ret1.Add(p2);
            ret1.Add(p3);

            return ret1;
        }
    }
    public class TransRotZoom
    {
        public enum TRZtype { Master, Slave,Const }
        public double zoom;
        public double xRot;
        public double yRot;
        public double zRot;
        public double off_x;
        public double off_y;
        public double off_z;
        public TRZtype type;
        public viewType viewType_;
        public int id;
        public int id_m;
        public bool visible;
        public Rectangle rect;
        public DateTime dateTime;
        public TransRotZoom consttransf;


        public TransRotZoom(Rectangle _rect,int _id)
        {
            zoom = 1;
            xRot = 0;
            yRot = 0;
            zRot = 0;
            off_x = 0;
            off_y = 0;
            off_z = -400;
            rect = _rect;
            id = _id;
            type = TRZtype.Master;
            viewType_ = viewType.Perspective;
            visible = false;
        }

        public TransRotZoom(Rectangle _rect, int _id,Vertex3d rotVer, Vertex3d transVer,int _idMast)
        {
            zoom = 1;
            xRot = 0;
            yRot = 0;
            zRot = 0;
            off_x = 0;
            off_y = 0;
            off_z = -400;
            rect = _rect;
            id = _id;
            id_m = _idMast;
            type = TRZtype.Slave;
            consttransf = new TransRotZoom(rotVer, transVer);
            viewType_ = viewType.Perspective;
            visible = false;
        }
        public TransRotZoom(Vertex3d rotVer, Vertex3d transVer)
        {
            xRot = rotVer.x;
            yRot = rotVer.y;
            zRot = rotVer.z;
            off_x = transVer.x;
            off_y = transVer.y;
            off_z = transVer.z;            
            type = TRZtype.Const;
            viewType_ = viewType.Perspective;
            visible = false;
        }

        public TransRotZoom getInfo(TransRotZoom[] transRotZooms)
        {
            switch (type)
            {
                case TRZtype.Master:
                    return this;
                    
                case TRZtype.Slave:
                    var trz_m = transRotZooms[id_m];
                    var trz_info = new TransRotZoom();
                    trz_info.zoom = trz_m.zoom;
                    trz_info.off_x = trz_m.off_x + consttransf.off_x;
                    trz_info.off_y = trz_m.off_y +consttransf.off_y;
                    trz_info.off_z = trz_m.off_z + consttransf.off_z;
                    trz_info.xRot = trz_m.xRot + consttransf.xRot;
                    trz_info.yRot = trz_m.yRot + consttransf.yRot;
                    trz_info.zRot = trz_m.zRot + consttransf.zRot;
                    trz_info.viewType_ = trz_m.viewType_;
                    trz_info.rect = rect;
                    trz_info.visible = trz_m.visible;
                    
                    return trz_info;
                default:
                    return null;
            }
        }
        public TransRotZoom(TransRotZoom _trz)
        {
            zoom = _trz.zoom;
            xRot = _trz.xRot;
            yRot = _trz.yRot;
            zRot = _trz.zRot;
            off_x = _trz.off_x;
            off_y = _trz.off_y;
            off_z = _trz.off_z;
            rect = _trz.rect;
            id = _trz.id;
            type = _trz.type;
        }
        public TransRotZoom()
        {

        }

        public TransRotZoom(string data)
        {
            var dt = data.Split();
            if(dt.Length<7)
            {
                
            }
            xRot = Convert.ToDouble(dt[0]);
            yRot = Convert.ToDouble(dt[1]);
            zRot = Convert.ToDouble(dt[2]);
            off_x = Convert.ToDouble(dt[3]);
            off_y = Convert.ToDouble(dt[4]);
            off_z = Convert.ToDouble(dt[5]);
            zoom = Convert.ToDouble(dt[6]);
        }
        public TransRotZoom(double _xRot, double _yRot, double _zRot,
            double _off_x, double _off_y, double _off_z, double _zoom)
        {
            xRot = _xRot;
            yRot = _yRot;
            zRot = _zRot;
            off_x = _off_x;
            off_y = _off_y;
            off_z = _off_z;
            zoom = _zoom;
        }
        public void setTrz(double _xRot, double _yRot, double _zRot,
            double _off_x, double _off_y, double _off_z, double _zoom)
        {
            xRot = _xRot;
            yRot = _yRot;
            zRot = _zRot;
            off_x = _off_x;
            off_y = _off_y;
            off_z = _off_z;
            zoom = _zoom;
        }
        public TransRotZoom minusDelta(TransRotZoom trz)
        {
            var _xRot = xRot - trz.xRot;
            var _yRot = yRot - trz.yRot;
            var _zRot = zRot - trz.zRot;
            var _off_x = off_x - trz.off_x;
            var _off_y = off_y - trz.off_y;
            var _off_z = off_z - trz.off_z;
            var _zoom = zoom - trz.zoom;
            return new TransRotZoom(_xRot,_yRot,_zRot,_off_x,_off_y,_off_z,_zoom);
        }
        public void setTrz(TransRotZoom trz)
        {
            xRot = trz.xRot;
            yRot = trz.yRot;
            zRot = trz.zRot;
            off_x = trz.off_x;
            off_y = trz.off_y;
            off_z = trz.off_z;
            zoom = trz.zoom;
        }
        public override string ToString()
        {
            return xRot + " " + yRot + " " + zRot + " "
                + off_x + " " + off_y + " " + off_z + " "
                + zoom + " " + viewType_ + " ";
        }

        public static TransRotZoom operator -(TransRotZoom trz1,TransRotZoom trz2)
        {
            return new TransRotZoom(
                trz1.xRot - trz2.xRot,
                 trz1.yRot - trz2.yRot,
                  trz1.zRot - trz2.zRot,
                   trz1.off_x - trz2.off_x,
                   trz1.off_y - trz2.off_y,
                   trz1.off_z - trz2.off_z,
                   trz1.zoom - trz2.zoom
                  );
        }
        public void setxRot(double value)
        {
            xRot = value;
        }
        public void setyRot(double value)
        {
            yRot = value;
        }
        public void setzRot(double value)
        {
            zRot = value;
        }
        public void setRot(double valuex, double valuey, double valuez)
        {
            xRot = valuex;
            yRot = valuey;
            zRot = valuez;
        }
    }
    class GraphicGL
    {
        #region vars
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
        RichTextBox debug_box;
        Matrix4x4f Pm;
        Matrix4x4f Vm;
        public BuffersGl buffersGl = new BuffersGl();
        Matrix4x4f Mm;
        Matrix4x4f MVP;
        Matrix4x4f[] MVPs;
        Matrix4x4f[] Ms;
        Matrix4x4f[] Vs;
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
            }
            
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
        public void glControl_ContextCreated(object sender, GlControlEventArgs e)
        {
            sizeControl = ((Control)sender).Size;
            Gl.Initialize();
            Gl.Enable(EnableCap.Multisample);
            Gl.ClearColor(0.9f, 0.9f, 0.95f, 0.0f);
            Gl.PointSize(2f);
            programID_ps = createShader(_VertexSourceGL, _GeometryShaderPointsGL, _FragmentSourceGL);
            programID_lns = createShader(_VertexSourceGL, _GeometryShaderLinesGL, _FragmentSourceGL);
            programID_trs = createShader(_VertexSourceGL, _GeometryShaderTrianglesGL, _FragmentSourceGL);
            //Gl.Enable(EnableCap.CullFace);
            Gl.Enable(EnableCap.DepthTest);
            Gl.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
            
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
            }
            
            LocationMVP = Gl.GetUniformLocation(prog, "MVP");
            LocationV = Gl.GetUniformLocation(prog, "V");
            LocationM = Gl.GetUniformLocation(prog, "M");
            MaterialDiffuseID = Gl.GetUniformLocation(prog, "MaterialDiffuse");
            MaterialAmbientID = Gl.GetUniformLocation(prog, "MaterialAmbient");
            MaterialSpecularID = Gl.GetUniformLocation(prog, "MaterialSpecular");
            LightID = Gl.GetUniformLocation(prog, "LightPosition_worldspace");
            LightPowerID = Gl.GetUniformLocation(prog, "lightPower");

            
            for (int i = 0; i < 4; i++)
            {
                Gl.UniformMatrix4f(LocationMVPs[i], 1, false, MVPs[i]);
                Gl.UniformMatrix4f(LocationMs[i], 1, false, Ms[i]);
                Gl.UniformMatrix4f(LocationVs[i], 1, false, Vs[i]);
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
        public void SaveToFolder(string folder,int id)
        {
            var bitmap = matFromMonitor(id);
            var invVm = Vs[id].Inverse;
            var trz_in = transRotZooms[selectTRZ_id(id)];
            var trz = trz_in.getInfo(transRotZooms.ToArray());
            
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
            return data;
        }
        
        PointF toGLcord(PointF pf, Size sizeView)
        {
            var x = (sizeView.Width / 2) * pf.X + sizeView.Width / 2;
            var y = -((sizeView.Width / 2) * pf.Y) + sizeView.Height / 2;
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
        Matrix4x4f[] compMVPmatrix(TransRotZoom trz_in)
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
                Pm = Matrix4x4f.Perspective(53.0f, (float)trz.rect.Width / (float)trz.rect.Height, 0.1f, 3000.0f);
                Vm = Matrix4x4f.Translated((float)(off_x), -(float)(off_y), (float)zoom * (float)(off_z)) *
               Matrix4x4f.RotatedX((float)xRot) *
               Matrix4x4f.RotatedY((float)yRot) *
               Matrix4x4f.RotatedZ((float)zRot);
            }
            else if (trz.viewType_ == viewType.Ortho)
            {
                Pm = Matrix4x4f.Ortho(-100.0f * (float)zoom, +100.0f * (float)zoom, -100.0f * (float)zoom, +100.0f * (float)zoom, 0.1f, 6000.0f);
                Vm = Matrix4x4f.Translated((float)(off_x), -(float)(off_y), (float)(off_z)) *
               Matrix4x4f.RotatedX((float)xRot) *
               Matrix4x4f.RotatedY((float)yRot) *
               Matrix4x4f.RotatedZ((float)zRot);
            }
            Mm = Matrix4x4f.Identity;
            MVP = Pm * Vm * Mm;
            return new Matrix4x4f[] { Pm, Vm, Mm, MVP };
        }
        PointF calcPixel(Vertex4f point, Matrix4x4f mvp)
        {
            var p2 = mvp * point;
            var p4 = p2 / p2.w;
            var p3 = toGLcord(new PointF(p4.x, p4.y), sizeControl);
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
                txt += "_____________\n";
            }
            box.Text = txt;
        }
        #endregion

        #region shader
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

        private readonly string[] _VertexSourceGL = {
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

        private readonly string[] _GeometryShaderLinesGL = {
            "#version 460 core\n",

            "layout (lines, invocations = 4) in;\n",
            "layout (line_strip, max_vertices = 2) out;\n",

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
            "	    fs_in.EyeDirection_cameraspace = vec3(0,0,0) - vertexPosition_cameraspace;\n",
            "	    vec3 LightPosition_cameraspace = ( Vs[gl_InvocationID] * vec4(LightPosition_worldspace,1)).xyz;\n",
            "	    fs_in.LightDirection_cameraspace = LightPosition_cameraspace + fs_in.EyeDirection_cameraspace;\n",
            "	    fs_in.Normal_cameraspace = ( Vs[gl_InvocationID] * Ms[gl_InvocationID] * vec4(vs_out[i].vertexNormal_modelspace,0)).xyz;\n",
            "	    fs_in.Color = vs_out[i].vertexColor;\n",

            "	    EmitVertex();}\n",
          

            "}\n"
        };

        private readonly string[] _GeometryShaderPointsGL = {
            "#version 460 core\n",

            "layout (points, invocations = 4) in;\n",
            "layout (points, max_vertices = 1) out;\n",

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
            "	    fs_in.EyeDirection_cameraspace = vec3(0,0,0) - vertexPosition_cameraspace;\n",
            "	    vec3 LightPosition_cameraspace = ( Vs[gl_InvocationID] * vec4(LightPosition_worldspace,1)).xyz;\n",
            "	    fs_in.LightDirection_cameraspace = LightPosition_cameraspace + fs_in.EyeDirection_cameraspace;\n",
            "	    fs_in.Normal_cameraspace = ( Vs[gl_InvocationID] * Ms[gl_InvocationID] * vec4(vs_out[i].vertexNormal_modelspace,0)).xyz;\n",
            "	    fs_in.Color = vs_out[i].vertexColor;\n",

            "	    EmitVertex();}\n",


            "}\n"
        };

        private readonly string[] _GeometryShaderTrianglesGL = {
            "#version 460 core\n",

            "layout (triangles, invocations = 4) in;\n",
            "layout (triangle_strip, max_vertices = 3) out;\n",

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
            "	    fs_in.EyeDirection_cameraspace = vec3(0,0,0) - vertexPosition_cameraspace;\n",
            "	    vec3 LightPosition_cameraspace = ( Vs[gl_InvocationID] * vec4(LightPosition_worldspace,1)).xyz;\n",
            "	    fs_in.LightDirection_cameraspace = LightPosition_cameraspace + fs_in.EyeDirection_cameraspace;\n",
            "	    fs_in.Normal_cameraspace = ( Vs[gl_InvocationID] * Ms[gl_InvocationID] * vec4(vs_out[i].vertexNormal_modelspace,0)).xyz;\n",
            "	    fs_in.Color = vs_out[i].vertexColor;\n",

            "	    EmitVertex();}\n",


            "}\n"
        };

        private readonly string[] _FragmentSourceGL = {
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


        private readonly string[] _FragmentSourceGL_ = {
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
        private readonly string[] _GeometryShaderGL_ = {
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
        #endregion
        #region mouse
        public void add_Label(Label label_list, Label label_cur)
        {
            Label_cor = label_list;
            Label_cor_cur = label_cur;
            if (Label_cor == null || Label_cor_cur==null) 
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
        double toRad(double degree)
        {
            return (Math.PI * 2 * degree) / 360;
        }
        void addCamView(int id)
        {
            var trz = transRotZooms[selectTRZ_id(id)].getInfo(transRotZooms.ToArray());
            if (trz.visible == true)
            {
                if (trz.viewType_ == viewType.Perspective)
                {
                    var p1 = new Vertex4f(0, 0, 0.1f, 1f);
                    double _z = -1000;
                    double _x = _z * Math.Tan(toRad(53 / 2)), _y = _z * Math.Tan(toRad(53 / 2));
                    float x = (float)_x; float y = (float)_y; float z = (float)_z;
                    var p2 = new Vertex4f(-x, -y, z, 1);
                    var p3 = new Vertex4f(-x, y, z, 1);
                    var p4 = new Vertex4f(x, -y, z, 1);
                    var p5 = new Vertex4f(x, y, z, 1);
                    p1 = Vs[id].Inverse * p1;
                    p2 = Vs[id].Inverse * p2;
                    p3 = Vs[id].Inverse * p3;
                    p4 = Vs[id].Inverse * p4;
                    p5 = Vs[id].Inverse * p5;

                    var verts = new Vertex4f[16]
                    {
                p1,p2, p1,p3, p1,p4, p1,p5, p2,p3, p3,p5, p5,p4, p4,p2
                    };
                    add_buff_gl_lines_id(toFloat(verts), id,true);
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
            buffersGl.add_obj_id(data_v,id, visible);
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
        public float[] translateMesh(float[] _mesh, float x, float y, float z)
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
            // Console.WriteLine("vert len " + gl_vertex_buffer_data.Length);
            add_buff_gl(gl_vertex_buffer_data, color_buffer_data, normal_buffer_data, primitiveType);
        }
        #endregion
    }
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
            if(opgl_obj.animType == openGlobj.AnimType.Dynamic)
            {
                objs_out.Add(opgl_obj);
            }
        }

        public void add_obj_id(float[] data_v, int id, bool visible)
        {
            int ind = 0;
            if (objs.Count!=0)
            {
                
                foreach (var ob in objs_out)
                {
                    if (ob.id == id)
                    {
                        var lam_obj = ob;
                        if(visible)
                        {
                            lam_obj.visible = true;
                            var data_n_ = new float[data_v.Length];
                            var data_c_ = new float[data_v.Length];
                            for (int i = 0; i < data_v.Length; i++)
                            {
                                data_c_[i] = 0.5f;
                                data_n_[i] = 0.5f;
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
                data_c[i] = 0.5f;
                data_n[i] = 0.5f;
            }
            //Console.WriteLine("new ver " + id+" all "+ind);
            add_obj(new openGlobj(data_v,data_c,data_n,PrimitiveType.Lines,id));
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
                    if(objs[i].animType == openGlobj.AnimType.Dynamic)
                    {
                        objs_out.Add(objs[i]);
                    }
                }
                
                if (vertex_buffer_data.Count>2)
                {
                    
                    objs_out.Add(new openGlobj(vertex_buffer_data.ToArray(), color_buffer_data.ToArray(), normal_buffer_data.ToArray(), val_tp));
                }
                
            }            
        }
        public void removeObj(int id)//!!!!!!!!!!!!!!
        {
            foreach(var ob in objs)
            {
                if(ob.id == id)
                {

                    objs.Remove(ob); 
                }
            }
            foreach (var ob in objs_out)
            {
                if (ob.id == id)
                {
                   objs_out.Remove(ob);
                }
            }
        }

    }
    public struct openGlobj
    {
        public enum AnimType { Static, Dynamic}
        public float[] vertex_buffer_data;
        public float[] color_buffer_data;
        public float[] normal_buffer_data;
        public PrimitiveType tp;
        public AnimType animType;
        public int id;
        public bool visible;
        public openGlobj(float[] v_buf, float[] c_buf, float[] n_buf,PrimitiveType type)
        {
            vertex_buffer_data = new float[v_buf.Length];
            color_buffer_data = new float[c_buf.Length];
            normal_buffer_data = new float[n_buf.Length];
            v_buf.CopyTo(vertex_buffer_data,0);
            c_buf.CopyTo(color_buffer_data,0);
            n_buf.CopyTo(normal_buffer_data,0);          
            tp = type;
            animType = AnimType.Static;
            id = -1;
            visible = true;
        }
        public openGlobj(float[] v_buf, float[] c_buf, float[] n_buf, PrimitiveType type,int _id)
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
