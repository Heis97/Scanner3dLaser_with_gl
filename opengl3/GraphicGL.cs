using OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace opengl3
{
    public enum modeGL { Paint, View}
    public struct STLmodel
    {
        public string path;
        public STLmodel(string _path)
        {
            path = _path;
        }
        public double parseE(string num)
        {
            var splnum = num.Split(new char[] { 'e' });
            return Convert.ToDouble(splnum[0]) * Math.Pow(10, Convert.ToInt32(splnum[1]));
        }
        public float[] parsingStl_GL4(string path)
        {
            var offx = 200;
            var offy = 500;
            var offz = 600;
            string file1;            
            using (StreamReader sr = new StreamReader(path, ASCIIEncoding.ASCII))
            {
                file1 = sr.ReadToEnd();
            }
            string[] lines = file1.Split(new char[] { '\n' });
            float[] ret1 = new float[(int)(9*lines.Length/7)];

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
    class GraphicGL
    {
        public int typeProj = 0;

        private double zoom = 1.0;
        private double xRot = 0;//2200;
        private double yRot = 0;
        private double zRot = 0;//1200;
        private double off_x = 0;
        private double off_y = 0;
        private double off_z = -450;
        private float[] vertex_buffer_data = { 0,0,0};
        private float[] color_buffer_data = { 0, 0, 0 };
        private float[] normal_buffer_data = { 0, 0, 0 };
         Point lastPos;
        uint buff_pos;
        uint buff_color;
        uint buff_normal;
        uint programID;
        int LocationMVP;
        int LocationV;
        int LocationM;
        int LightID;
        int LightPowerID;
        int MaterialDiffuseID;
        int MaterialAmbientID;
        
        int MaterialSpecularID;
        List<int> start = new List<int>();
        List<int> stop = new List<int>();
        int names_count = 0;
        float LightPower = 500000.0f;
        Label Label_cor;
        Matrix4x4f Pm;
        Matrix4x4f Vm;
        public BuffersGl buffersGl = new BuffersGl();
        Matrix4x4f Mm;
        Matrix4x4f MVP;
        Vertex3f lightPos = new Vertex3f(0.0f, 0.0f, 123.0f);
        Vertex3f MaterialDiffuse = new Vertex3f(0.1f, 0.1f, 0.1f);
        Vertex3f MaterialAmbient = new Vertex3f(0.1f, 0.1f, 0.1f);
        Vertex3f MaterialSpecular = new Vertex3f(0.1f, 0.1f, 0.1f);
        PrimitiveType[] types = new PrimitiveType[0];
        public modeGL modeGL = modeGL.View;
        List<Point3d_GL> pointsPaint = new List<Point3d_GL>();
        Point3d_GL curPointPaint = new Point3d_GL(0, 0, 0);
        void print(Matrix4x4f matr)
        {
            for (uint i = 0; i < 4; i++)
            {
                for (uint j = 0; j < 4; j++)
                {
                    Console.Write(matr[i, j] + " ");
                }
                Console.WriteLine(" ");
            }
        }
        public void glControl1_Render(object sender, GlControlEventArgs e)
        {
            
            Control senderControl = (Control)sender;
            int w = senderControl.ClientSize.Width;
            int h = senderControl.ClientSize.Height;
            Gl.Viewport(0, 0, w, h);

            load_vars_gl();
            if (typeProj==1)
            {
                Pm = Matrix4x4f.Perspective(53.0f, (float)w / (float)h, 0.1f, 1000.0f);
                Vm = Matrix4x4f.Translated((float)(off_x), -(float)(off_y), (float)zoom * (float)(off_z)) *
               Matrix4x4f.RotatedX((float)xRot) *
               Matrix4x4f.RotatedY((float)yRot) *
               Matrix4x4f.RotatedZ((float)zRot);
            }
            else if(typeProj==0)
            {
                Pm = Matrix4x4f.Ortho(-100.0f * (float)zoom, +100.0f * (float)zoom, -100.0f * (float)zoom, +100.0f * (float)zoom, 0.1f, 600.0f);
                Vm = Matrix4x4f.Translated((float)(off_x), -(float)(off_y), (float)(off_z)) *
               Matrix4x4f.RotatedX((float)xRot) *
               Matrix4x4f.RotatedY((float)yRot) *
               Matrix4x4f.RotatedZ((float)zRot);
            }
               
            //
           
               
            Mm = Matrix4x4f.Identity;
            MVP = Pm * Vm * Mm;

            var p_1 = new Point3d_GL(50, 0, 0);

            //Console.WriteLine(MVP*p_1);
            Gl.UseProgram(programID);
            Gl.UniformMatrix4f(LocationMVP, 1, false, MVP);
            Gl.UniformMatrix4f(LocationM, 1, false, Mm);
            Gl.UniformMatrix4f(LocationV, 1, false, Vm);

            Gl.Uniform3f(MaterialDiffuseID, 1, MaterialDiffuse);
            Gl.Uniform3f(MaterialAmbientID, 1, MaterialAmbient);
            Gl.Uniform3f(MaterialSpecularID, 1, MaterialSpecular);
            Gl.Uniform3f(LightID, 1, lightPos);
            Gl.Uniform1f(LightPowerID, 1, LightPower);
                


            Gl.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            if(buffersGl.objs_out!=null)
            {
                if (buffersGl.objs_out.Count!=0)
                {
                //Console.WriteLine("buffersGl.objs_out.Count " + buffersGl.objs_out.Count);
                    foreach (var opgl_obj in buffersGl.objs_out)
                    {
                        try
                        {
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
            }
            MaterialDiffuse.x = 0.5f;
            MaterialDiffuse.y = 0.5f;
            MaterialDiffuse.z = 0.5f;           
            Gl.DeleteProgram(programID);
        }

        public void SaveToBitmap(int x, int y, int width, int height, string folder)
        {
            var lockMode = System.Drawing.Imaging.ImageLockMode.WriteOnly;
            var format = System.Drawing.Imaging.PixelFormat.Format32bppRgb;
            var bitmap = new Bitmap(width, height, format);
            var bitmapRectangle = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData bmpData = bitmap.LockBits(bitmapRectangle, lockMode, format);
            Gl.ReadPixels(x, y, width, height, PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            bitmap.UnlockBits(bmpData);
            bitmap.RotateFlip(RotateFlipType.Rotate180FlipX);

            // Vm.Row3.ToString()
            //Vertex3f ver1 = new Vertex3f(Vm.Column3.x, Vm.Column3.y, Vm.Column3.z);
             var invVm = Vm.Inverse;
             Vertex3f ver0 = new Vertex3f(invVm.Column0.x, invVm.Column0.y, invVm.Column0.z);
             Vertex3f ver1 = new Vertex3f(invVm.Column1.x, invVm.Column1.y, invVm.Column1.z);
             Vertex3f ver2 = new Vertex3f(invVm.Column2.x, invVm.Column2.y, invVm.Column2.z);
             Vertex3f ver3 = new Vertex3f(invVm.Column3.x, invVm.Column3.y, invVm.Column3.z);
             bitmap.Save(folder+"/"+ver0.x+" "+ver0.y + " " + ver0.z  + " "
                 + ver1.x + " " + ver1.y + " " + ver1.z + " "
                 + ver2.x + " " + ver2.y + " " + ver2.z + " "
                 + ver3.x + " " + ver3.y + " " + ver3.z + " " + ".png");
            bitmap.Save("tesr.png");
        }
        public void glControl1_ContextCreated(object sender, GlControlEventArgs e)
        {
            Gl.Initialize();
            Gl.ClearColor(0.9f, 0.9f, 0.95f, 0.0f);
            Gl.PointSize(2f);
            programID = createShader();
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

            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff_color);
            Gl.VertexAttribPointer(2, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            Gl.EnableVertexAttribArray(2);

            Gl.BindBuffer(BufferTarget.ArrayBuffer, buff_normal);
            Gl.VertexAttribPointer(1, 3, VertexAttribType.Float, false, 0, IntPtr.Zero);
            Gl.EnableVertexAttribArray(1);
            
        }
        private void load_vars_gl()
        {
            LocationMVP = Gl.GetUniformLocation(programID, "MVP");
            LocationV = Gl.GetUniformLocation(programID, "V");
            LocationM = Gl.GetUniformLocation(programID, "M");
            MaterialDiffuseID = Gl.GetUniformLocation(programID, "MaterialDiffuse");
            MaterialAmbientID = Gl.GetUniformLocation(programID, "MaterialAmbient");
            MaterialSpecularID = Gl.GetUniformLocation(programID, "MaterialSpecular");
            LightID = Gl.GetUniformLocation(programID, "LightPosition_worldspace");
            LightPowerID = Gl.GetUniformLocation(programID, "lightPower");
        }
        public void add_buff_gl(float[] data_v, float[] data_c, float[] data_n, PrimitiveType tp)
        {
            buffersGl.add_obj(new openGlobj(data_v, data_c, data_n, tp));
            //buffersGl.sortObj();

            if(buffersGl.objs_out!=null)
            {
                if (buffersGl.objs_out.Count != 0)
                {
                    //Console.WriteLine("add buffer ");
                    int i = 0;
                    foreach(var ob in buffersGl.objs_out)
                    {
                       // Console.WriteLine(i + ": " + ob.vertex_buffer_data.Length);
                        i++;
                    }
                   // Console.WriteLine("-----------");
                }
            }
        }
        public void add_buff_gl_1(float[] data_v, float[] data_c, float[] data_n, PrimitiveType tp)
        {
            if(data_v.Length!=0)
            {
                float[] vr3 = new float[18];
                for (int i = 0; i<vr3.Length;i++)
                {
                    vr3[i] = 0;
                }
                var bv = vertex_buffer_data.ToList();
                var bn = normal_buffer_data.ToList();
                var bc = color_buffer_data.ToList();

                 start.Add(bv.Count / 3);
                stop.Add(start[names_count] + (int)(data_v.Length / 3)-1);

                if (types != null)
                {
                    var typesA = types.ToList();
                    typesA.Add(tp);
                    types = typesA.ToArray();
                }

                bv.AddRange(data_v);
                bc.AddRange(data_c);
                bn.AddRange(data_n);

                bv.AddRange(vr3);
                bc.AddRange(vr3);
                bn.AddRange(vr3);

                vertex_buffer_data = bv.ToArray();
                color_buffer_data = bc.ToArray();
                normal_buffer_data = bn.ToArray();
                Console.WriteLine("-------");
                Console.WriteLine(vertex_buffer_data.Length);
                Console.WriteLine(color_buffer_data.Length);
                Console.WriteLine(normal_buffer_data.Length);
                names_count++;
            }
            
        }
        public void printDebug(RichTextBox box)
        {
            string txt = "";
            for (int i=0; i<vertex_buffer_data.Length/3; i++)
            {
                txt += "pos| "+i.ToString() + " " + vertex_buffer_data[i*3].ToString() + " "
                    + vertex_buffer_data[i * 3+1].ToString() + " "
                    + vertex_buffer_data[i * 3+2].ToString() + " |";

                txt += "col| " + " " + color_buffer_data[i * 3].ToString() + " "
                    + color_buffer_data[i * 3 + 1].ToString() + " "
                    + color_buffer_data[i * 3 + 2].ToString() + " |";
                txt += "norm| "  + " " + normal_buffer_data[i * 3].ToString() + " "
                    + normal_buffer_data[i * 3 + 1].ToString() + " "
                    + normal_buffer_data[i * 3 + 2].ToString() + " |";

                if (start.Contains(i))
                {
                    //Console.WriteLine("LENN "+start.Count + " " + types.Length + " " + stop.Count);
                    txt += " start";
                    int ind = start.IndexOf(i);
                    //Console.WriteLine("INDR " + ind);
                    txt += ":" + ind.ToString()+" "+types[ind].ToString();
                }
                if (stop.Contains(i))
                {
                    txt += " stop";
                    int ind = stop.IndexOf(i);
                    //Console.WriteLine("INDP " + ind);
                    txt += ":" + ind.ToString() + " " + types[ind].ToString();
                }
                txt += "\n ";
            }
            box.Text = txt;
        }
        private uint createShader()
        {
            uint VertexShaderID = Gl.CreateShader(ShaderType.VertexShader);
            Gl.ShaderSource(VertexShaderID, _VertexSourceGL);
            Gl.CompileShader(VertexShaderID);

            uint FragmentShaderID = Gl.CreateShader(ShaderType.FragmentShader);
            Gl.ShaderSource(FragmentShaderID, _FragmentSourceGL);
            Gl.CompileShader(FragmentShaderID);


            uint ProgrammID = Gl.CreateProgram();
            Gl.AttachShader(ProgrammID, VertexShaderID);
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
            return ProgrammID;
        }
        private readonly string[] _VertexSourceGL = {
            "#version 460 core\n",
            "uniform mat4 MVP;\n",
            "uniform mat4 M;\n",
            "uniform mat4 V;\n",
            "uniform vec3 LightPosition_worldspace;\n",
            
            "in vec3 vertexPosition_modelspace;\n",
            "in vec3 vertexNormal_modelspace;\n",
            "in vec3 vertexColor;\n",
            

            "out vec3 Color;\n",
            "out vec3 Position_worldspace;\n",
            "out vec3 Normal_cameraspace;\n",
            "out vec3 EyeDirection_cameraspace;\n",
            "out vec3 LightDirection_cameraspace;\n",
            "void main() {\n",
            "   gl_Position = MVP * vec4(vertexPosition_modelspace, 1.0);\n",
            "	Position_worldspace = (M * vec4(vertexPosition_modelspace,1)).xyz;\n",
            "	vec3 vertexPosition_cameraspace = ( V * M * vec4(vertexPosition_modelspace,1)).xyz;\n",
            "	EyeDirection_cameraspace = vec3(0,0,0) - vertexPosition_cameraspace;\n",
            "	vec3 LightPosition_cameraspace = ( V * vec4(LightPosition_worldspace,1)).xyz;\n",
            "	LightDirection_cameraspace = LightPosition_cameraspace + EyeDirection_cameraspace;\n",
            "	Normal_cameraspace = ( V * M * vec4(vertexNormal_modelspace,0)).xyz;\n",
            "	Color = vertexColor;\n",
            "}\n"
        };
        private readonly string[] _FragmentSourceGL = {
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

        #region mouse
        public void add_Label(Label label)
        {
            Label_cor = label;
            if(Label_cor == null)
            {
                Console.WriteLine("null_start");
            }
            
        }
        public void save_buff_gl(float[] data1, float[] data2,float[] data3)
        {
            vertex_buffer_data = data1;            
            color_buffer_data = data2;
            normal_buffer_data = data3;
        }
        

        public void glControl1_MouseDown(object sender, MouseEventArgs e)
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
        public void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            Control senderControl = (Control)sender;
            int w = senderControl.ClientSize.Width;
            int h = senderControl.ClientSize.Height;
            switch (modeGL)
            {
                case modeGL.View:
                    
                    var dx = e.X - lastPos.X;
                    var dy = e.Y - lastPos.Y;
                    double dyx = lastPos.Y - w / 2;
                    double dxy = lastPos.X - h / 2;
                    double dz = (dy * dxy + dx * dyx) / (Math.Sqrt(dy * dy + dx * dx) * Math.Sqrt(dxy * dxy + dyx * dyx));
                    if (e.Button == MouseButtons.Left)
                    {
                        xRot += dy;
                        yRot -= dx;
                        zRot += dz;
                    }
                    else if (e.Button == MouseButtons.Right)
                    {
                        off_x += Convert.ToDouble(dx);
                        off_y += Convert.ToDouble(dy);
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
            
        }
        public void Form1_mousewheel(object sender, MouseEventArgs e)
        {
            //Console.WriteLine("P m = " + Pm);
            //Console.WriteLine("V m = " + Vm);
            var invVm = Vm.Inverse;
            Console.WriteLine("invV m = " + invVm);
            var angle = e.Delta;
            if (angle > 0)
            {
                if (zoom < 0.002)
                {
                }
                else
                {
                    zoom = 0.7 * zoom;
                    zoom = Math.Round(zoom, 4);
                }
            }
            else
            {
                zoom = 1.3 * zoom;
                zoom = Math.Round(zoom, 4);
            }
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
            xRot = (float)value;
        }
        public void orientYscroll(int value)
        {
            yRot = (float)value;
        }
        public void orientZscroll(int value)
        {
            zRot = (float)value;
        }

        public void planeXY()
        {
            xRot = 0;
            yRot = 0;
            zRot = 0;
        }
        public void planeYZ()
        {
            xRot = 0;
            yRot = 90;
            zRot = 0;
        }
        public void planeZX()
        {
            xRot = 90;
            yRot = 0;
            zRot = 0;
        }

        public void setMode(modeGL mode)
        {
            modeGL = mode;
        }
        #endregion

        Point calc_sr_p(Point p1, Point p2)
        {
            return new Point(p2.X + (p1.X - p2.X) / 2, p2.Y + (p1.Y - p2.Y) / 2);
        }

        double dist(Point p1, Point p2)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) - (p1.Y - p2.Y) * (p1.Y - p2.Y));
        }
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
                    if (objs[i].tp == val_tp)
                    {
                        vertex_buffer_data.AddRange(objs[i].vertex_buffer_data);
                        color_buffer_data.AddRange(objs[i].color_buffer_data);
                        normal_buffer_data.AddRange(objs[i].normal_buffer_data);
                    }                   
                }
                if(vertex_buffer_data.Count>2)
                {
                    objs_out.Add(new openGlobj(vertex_buffer_data.ToArray(), color_buffer_data.ToArray(), normal_buffer_data.ToArray(), val_tp));
                }
                
            }            
        }

    }
    public struct openGlobj
    {
        public float[] vertex_buffer_data;
        public float[] color_buffer_data;
        public float[] normal_buffer_data;
        public PrimitiveType tp;
        public openGlobj(float[] v_buf, float[] c_buf, float[] n_buf,PrimitiveType type)
        {
            vertex_buffer_data = new float[v_buf.Length];
            color_buffer_data = new float[c_buf.Length];
            normal_buffer_data = new float[n_buf.Length];
            v_buf.CopyTo(vertex_buffer_data,0);
            c_buf.CopyTo(color_buffer_data,0);
            n_buf.CopyTo(normal_buffer_data,0);
            tp = type;
        }

    }

}
