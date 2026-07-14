using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using Emgu.CV.Aruco;
using System.Drawing;
using System.ComponentModel;
using System.Drawing.Design;
using OpenGL;
using System.Text.Json;
using System.Windows.Forms;
using Dicom.Printing;
using CommunityToolkit.HighPerformance;
using Emgu.CV.Dnn;

namespace opengl3
{

    public class NavigModel
    {
        public CtSliceFull ctSliceFull;
        public Point3d_GL[] registr_ps;
        public int ind_ct_image = 0;
        public int ind_ct_image_sagit = 0;
        public int ind_ct_image_coronal = 0;
        double main_frame_len = 100;
        Point3d_GL transf_work_mri;

        public Mat[] ct_projections = new Mat[3];
        public int ct_bin_lvl = 255;
        public int ct_gauss_size = 1;
        public VoxelToStlGpu voxel_model;
        public string generate_model_name = "generate_model";
        public byte[,,] orig_model;
        public System.Drawing.Point[] limits_ps = new System.Drawing.Point[3];

        public bool vision_navig_axis = true;
        public bool vision_navig_work_area = true;
        public bool vision_navig_bin_area = false;
        public bool vision_navig_ct_start_frame = false;
        public bool vision_navig_target_traj = true;
        public bool vision_navig_instr = true;
        public NavigModel(string path)
        {
            ctSliceFull = DicomSorter.LoadAndSortSlices(path);
        }
        bool update_model = false;
        public Matrix<double> ct_model_matrix = UtilMatr.eye_matr(4);
        public void generate_mesh(bool limits)
        {
            //gen_voxel_model=============================================================
            if (limits) orig_model = DicomProcess.compVoxelModel(ctSliceFull.SlicesAxial_mat, limits_ps);
            else orig_model = DicomProcess.compVoxelModel(ctSliceFull.SlicesAxial_mat);
            if (orig_model == null) return;
            
            
            //========================================================================
            ct_model_matrix = UtilMatr.matrix_cv(new Point3d_GL(), new Point3d_GL());
            if (limits)
            {
                var p_trans = new Point3d_GL(ctSliceFull.pix_xy * (limits_ps[0].Y - limits_ps[0].X) / 2, ctSliceFull.pix_xy * (limits_ps[1].Y - limits_ps[1].X) / 2, ctSliceFull.pix_xy * (limits_ps[2].Y - limits_ps[2].X) / 2);
                var matr_cent = UtilMatr.to_matrix(UtilMatr.matrix_cv(-p_trans, new Point3d_GL(0, 0, 0)));
                transf_work_mri = p_trans + new Point3d_GL(limits_ps[0].X * ctSliceFull.pix_xy, limits_ps[1].X *ctSliceFull.pix_xy, limits_ps[2].X * ctSliceFull.pix_xy);
                ct_model_matrix = UtilMatr.matrix_cv(-p_trans, new Point3d_GL());
                M_self = ct_model_matrix;
            }
            else
            {
                transf_work_mri = new Point3d_GL(orig_model.GetLength(0) * ctSliceFull.pix_xy / 2, orig_model.GetLength(1) * ctSliceFull.pix_xy / 2, orig_model.GetLength(2) *ctSliceFull.pix_xy / 2);

                ct_model_matrix = UtilMatr.matrix_cv(-transf_work_mri, new Point3d_GL());
                M_self = ct_model_matrix;
            }

            //if registr in abs_model

            registr_ps = Point3d_GL.multMatr(registr_ps,M_self);

            update_model = true;
            //var reg_ps = GL1.addPointMesh(navig_system.model.ctSliceFull.registr_ps, Color3d_GL.red(), "reg_ps");
            //add_navig_scene_obj_ct(reg_ps, NavigVision);
            // set_matr_store_navig_scene_objs_ct(NavigMatrixType.CtToModel, UtilMatr.to_matrix(ct_model_matrix),NavigVision);  
        }
        public Mat draw_navig_targets(Mat mat, List<NavigTarget> targets, int ax = 0)//ax = 0, 1 ,2 -> x,y,z
        {

            foreach (var target in targets)
            {
                draw_navig_target(mat, target, ax);
            }

            return mat;
        }
        public Mat draw_navig_target(Mat mat, NavigTarget target, int ax = 0)//ax = 0, 1 ,2 -> x,y,z
        {

            var tp1 = target.p1 + transf_work_mri;
            var tp2 = target.p2 + transf_work_mri;
            var p1_x = (int)(tp1.x / ctSliceFull.pix_xy);
            var p1_y = (int)(tp1.y / ctSliceFull.pix_xy);
            var p1_z = (int)(tp1.z / ctSliceFull.pix_xy);

            var p2_x = (int)(tp2.x / ctSliceFull.pix_xy);
            var p2_y = (int)(tp2.y / ctSliceFull.pix_xy);
            var p2_z = (int)(tp2.z / ctSliceFull.pix_xy);

            if (ax == 0)
            {
                var p1 = new Point(p1_z, p1_y);
                var p2 = new Point(p2_z, p2_y);
                CvInvoke.Line(mat, p1, p2, new MCvScalar(255 * target.color[2], 255 * target.color[1], 255 * target.color[0]), 2);
            }

            if (ax == 1)
            {
                var p1 = new Point(p1_z, p1_x);
                var p2 = new Point(p2_z, p2_x);
                CvInvoke.Line(mat, p1, p2, new MCvScalar(255 * target.color[2], 255 * target.color[1], 255 * target.color[0]), 2);
            }

            if (ax == 2)
            {
                var p1 = new Point(p1_x, p1_y);
                var p2 = new Point(p2_x, p2_y);
                CvInvoke.Line(mat, p1, p2, new MCvScalar(255 * target.color[2], 255 * target.color[1], 255 * target.color[0]), 2);
            }

            return mat;
        }
        public Mat draw_navig_frame(Mat mat, double frame_len, int ax = 0)//ax = 0, 1 ,2 -> x,y,z
        {
            var color_x = new MCvScalar(0, 0, 255);
            var color_y = new MCvScalar(0, 255, 0);
            var color_z = new MCvScalar(255, 0, 0);

            var tp0 = (new Point3d_GL(frame_len, 0, 0) + transf_work_mri) / ctSliceFull.pix_xy;
            var tp1 = (new Point3d_GL(0, frame_len, 0) + transf_work_mri) / ctSliceFull.pix_xy;
            var tp2 = (new Point3d_GL(0, 0, frame_len) + transf_work_mri) / ctSliceFull.pix_xy;
            var tpc = transf_work_mri / ctSliceFull.pix_xy;

            if (ax == 0)
            {
                var p1 = new Point((int)tpc.z, (int)tpc.y);
                var p2 = new Point((int)tp2.z, (int)tp2.y);
                var p3 = new Point((int)tp1.z, (int)tp1.y);
                CvInvoke.Line(mat, p1, p2, color_z, 1);
                CvInvoke.Line(mat, p1, p3, color_y, 1);
            }

            if (ax == 1)
            {
                var p1 = new Point((int)tpc.z, (int)tpc.x);
                var p2 = new Point((int)tp2.z, (int)tp2.x);
                var p3 = new Point((int)tp0.z, (int)tp0.x);
                CvInvoke.Line(mat, p1, p2, color_z, 1);
                CvInvoke.Line(mat, p1, p3, color_x, 1);
            }

            if (ax == 2)
            {
                var p1 = new Point((int)tpc.x, (int)tpc.y);
                var p2 = new Point((int)tp0.x, (int)tp0.y);
                var p3 = new Point((int)tp1.x, (int)tp1.y);
                CvInvoke.Line(mat, p1, p2, color_x, 1);
                CvInvoke.Line(mat, p1, p3, color_y, 1);
            }

            return mat;
        }
        public void redraw_navig_slices(List<NavigTarget> targets,ref Emgu.CV.UI.ImageBox imageBox_navig_axial, ref Emgu.CV.UI.ImageBox imageBox_navig_sagital, ref Emgu.CV.UI.ImageBox imageBox_navig_coronal)
        {

            var mats = new Mat[3];
            if (ctSliceFull == null) return;
            if (ind_ct_image < ctSliceFull.SlicesAxial_mat.Count - 1 && ind_ct_image >= 0 && ct_projections[0] != null)//axial
            {
                mats[0] = ct_projections[0].Clone();
                if (vision_navig_bin_area) mats[0] = DicomProcess.filter_bone_ct(mats[0], ct_bin_lvl, ct_gauss_size);
                else { CvInvoke.CvtColor(mats[0], mats[0], ColorConversion.Gray2Bgr); }


                if (vision_navig_work_area) mats[0] = draw_limits_lines(mats[0], limits_ps[0], limits_ps[1]);

                if (vision_navig_axis)
                {
                    CvInvoke.Line(mats[0], new Point(0, ind_ct_image_coronal), new Point(mats[0].Width, ind_ct_image_coronal), new MCvScalar(0, 0, 255), 2);//X
                    CvInvoke.Line(mats[0], new Point(ind_ct_image_sagit, 0), new Point(ind_ct_image_sagit, mats[0].Height), new MCvScalar(0, 255, 0), 2);//Y
                }
                if (vision_navig_ct_start_frame)
                {
                    mats[0] = draw_navig_frame(mats[0], main_frame_len, 2);
                }

                if (targets.Count > 0 && vision_navig_target_traj)
                {

                    mats[0] = draw_navig_targets(mats[0], targets, 2);
                }

                imageBox_navig_axial.Image = mats[0];
            }

            if (ind_ct_image_sagit < ctSliceFull.SlicesSagital.Count - 1 && ind_ct_image_sagit >= 0 && ct_projections[1] != null)
            {
                mats[1] = ct_projections[1].Clone();
                if (vision_navig_bin_area) mats[1] = DicomProcess.filter_bone_ct(mats[1], ct_bin_lvl, ct_gauss_size);
                else { CvInvoke.CvtColor(ct_projections[1], mats[1], ColorConversion.Gray2Bgr); }

                if (vision_navig_work_area) mats[1] = draw_limits_lines(mats[1], limits_ps[2], limits_ps[1]);

                if (vision_navig_axis)
                {
                    CvInvoke.Line(mats[1], new Point((int)(ind_ct_image * ctSliceFull.axial_koef), 0), new Point((int)(ind_ct_image * ctSliceFull.axial_koef), mats[1].Height), new MCvScalar(255, 0, 0), 2);//Z
                    CvInvoke.Line(mats[1], new Point(0, ind_ct_image_coronal), new Point(mats[1].Width, ind_ct_image_coronal), new MCvScalar(0, 0, 255), 2);//Y
                }
                if (vision_navig_ct_start_frame)
                {
                    mats[1] = draw_navig_frame(mats[1], main_frame_len, 0);
                }

                if (targets.Count > 0 && vision_navig_target_traj)
                {
                    mats[1] = draw_navig_targets(mats[1], targets, 0);
                }

                imageBox_navig_sagital.Image = mats[1];
            }

            if (ind_ct_image_coronal < ctSliceFull.SlicesCoronal.Count - 1 && ind_ct_image_coronal >= 0 && ct_projections[2] != null)
            {
                mats[2] = ct_projections[2].Clone();
                if (vision_navig_bin_area) mats[2] = DicomProcess.filter_bone_ct(mats[2], ct_bin_lvl, ct_gauss_size);
                else { CvInvoke.CvtColor(ct_projections[2], mats[2], ColorConversion.Gray2Bgr); }

                if (vision_navig_work_area) mats[2] = draw_limits_lines(mats[2], limits_ps[2], limits_ps[0]);

                if (vision_navig_axis)
                {
                    CvInvoke.Line(mats[2], new Point((int)(ind_ct_image * ctSliceFull.axial_koef), 0), new Point((int)(ind_ct_image * ctSliceFull.axial_koef), mats[2].Height), new MCvScalar(255, 0, 0), 2);//Z
                    CvInvoke.Line(mats[2], new Point(0, ind_ct_image_sagit), new Point(mats[2].Width, ind_ct_image_sagit), new MCvScalar(0, 255, 0), 2);//X
                }
                if (vision_navig_ct_start_frame)
                {
                    mats[2] = draw_navig_frame(mats[2], main_frame_len, 1);
                }
                if (targets.Count > 0 && vision_navig_target_traj)
                {
                    mats[2] = draw_navig_targets(mats[2], targets, 1);
                }
                CvInvoke.Rotate(mats[2], mats[2], RotateFlags.Rotate90Clockwise);
                CvInvoke.Flip(mats[2], mats[2], FlipType.Horizontal);
                imageBox_navig_coronal.Image = mats[2];
            }

        }

        public Mat draw_limits_lines(Mat mat,  Point px, Point py)
        {
            var color_lines_area = new MCvScalar(0, 110, 255);
            var color_filling_area = new MCvScalar(0, 20, 40);
            if (mat == null) { Console.WriteLine("mat null"); return null; }

            var mat_rect = new Mat(mat.Size.Height, mat.Size.Width, DepthType.Cv8U, 3);
            mat_rect.SetTo(new MCvScalar(0, 0, 0));
            CvInvoke.Rectangle(mat_rect, new Rectangle(px.X, py.X, px.Y - px.X, py.Y - py.X), color_filling_area, -1);

            CvInvoke.Rectangle(mat_rect, new Rectangle(px.X, py.X, px.Y - px.X, py.Y - py.X), color_lines_area, 1);

            mat += mat_rect;
            return mat;
        }
        List<string> models_name = new List<string>();
        public Matrix<double> M_obj_to_camera1 = UtilMatr.eye_matr(4);
        public Matrix<double> M_self = UtilMatr.eye_matr(4);
        public void init_draw_model_3d(GraphicGL graphic, NavigSys.SceneType cur_scene)
        {
            models_name.Add(generate_model_name);
        }
        public void draw_model_3d(GraphicGL graphic, NavigSys.SceneType cur_scene,bool visible)
        {
            if(update_model)
            {
                voxel_model = new VoxelToStlGpu(graphic, orig_model.GetLength(0), orig_model.GetLength(1), orig_model.GetLength(2));
                voxel_model.SetVoxelData(orig_model, graphic, ct_gauss_size, (float)ctSliceFull.pix_xy);
                var gen_mesh = voxel_model.GenerateMesh_isoline(ct_bin_lvl);
                generate_model_name = graphic.add_buff_gl(gen_mesh[0], Color3d_GL.gray(), gen_mesh[1], PrimitiveType.Triangles, generate_model_name, false, false); models_name.Add(generate_model_name);
                update_model = false;
                
            }

            if(cur_scene==NavigSys.SceneType.camera1)  graphic.buffersGl.setMatrobj(generate_model_name, 0, UtilMatr.to_matrix(M_obj_to_camera1* M_self));
            else if(cur_scene == NavigSys.SceneType.model3d) graphic.buffersGl.setMatrobj(generate_model_name, 0, UtilMatr.to_matrix( M_self));
            for (int i = 0; i < models_name.Count; i++)
            {
                graphic.buffersGl.setVisibleobj(models_name[i], visible);
            }
        }
    }

    public class NavigTarget
    {



        public NavigTarget()
        {
            update_intrisic_param();
        }
        public float[] color = new float[3] {0.1f,0.1f,0.1f};

        #region prop
        [Description("Цвет")]
        [Category("Название")]
        [DisplayName("Цвет")]
        public float[] Color
        {
            get { return color; }
            set { color = value; }
        }

        public string name;
        [Description("Название")]
        [Category("Название")]
        [DisplayName("Name")]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }


        public double x;
        [Description("Координата X")]
        [Category("Положение")]
        [DisplayName("X")]
        [Editor(typeof(DoubleUpDownEditor), typeof(UITypeEditor))]
        public double X 
        {
            get { return x; }
            set { x = value; update_intrisic_param(); }
        }

        public double y;
        [Description("Координата Y")]
        [Category("Положение")]
        [DisplayName("Y")]
        [Editor(typeof(DoubleUpDownEditor), typeof(UITypeEditor))]
        public double Y
        {
            get { return y; }
            set { y = value; update_intrisic_param(); }
        }

        public double z;
        [Description("Координата Z")]
        [Category("Положение")]
        [DisplayName("Z")]
        [Editor(typeof(DoubleUpDownEditor), typeof(UITypeEditor))]
        public double Z
        {
            get { return z; }
            set { z = value; update_intrisic_param(); }
        }

        public double a;
        [Description("Координата A")]
        [Category("Положение")]
        [DisplayName("A")]
        [Editor(typeof(DoubleUpDownEditorAngle), typeof(UITypeEditor))]
        public double A
        {
            get { return a; }
            set { a = value; update_intrisic_param(); }
        }

        public double b;
        [Description("Координата B")]
        [Category("Положение")]
        [DisplayName("B")]
        public double B
        {
            get { return b; }
            set { b = value; update_intrisic_param(); }
        }

        public double c;
        [Description("Координата C")]
        [Category("Положение")]
        [DisplayName("C")]
        public double C
        {
            get { return c; }
            set { c = value; update_intrisic_param(); }
        }

        public double d = 3;
        [Description("размер D")]
        [Category("Размер")]
        [DisplayName("D")]
        public double D
        {
            get { return d; }
            set { d = value; }
        }

        public double l = 50;
        [Description("размер L")]
        [Category("Размер")]
        [DisplayName("L")]
        public double L
        {
            get { return l; }
            set { l = value; update_intrisic_param(); }
        }
        #endregion
        public Matrix<double> M_self = UtilMatr.eye_matr(4);
        public Matrix<double> M_ct_to_obj = UtilMatr.eye_matr(4);
        public Matrix<double> M_obj_to_camera1 = UtilMatr.eye_matr(4);

        public Point3d_GL p1;
        public Point3d_GL p2;

        public void update_intrisic_param()
        {
            p1 = new Point3d_GL(x, y, z);   
            p2 = new Point3d_GL(0, 0, l);
            M_self = UtilMatr.matrix_cv(new Point3d_GL(x,y,z),new Point3d_GL(a,b,c));
            p2 = M_self * p2;

        }
        public override string ToString()
        {
            return Name; 
        }
        List<string> models_name = new List<string>();
        public bool update_model = false;
        public bool create_model = true;
        public void init_draw_model_3d(GraphicGL graphic, NavigSys.SceneType cur_scene)
        {

        }
        public float[] cilindr_mesh(float rad, int count, float height)
        {
            var mesh = new List<float>();
            var angle = 2 * Math.PI / count;
            var cur_angle = 0d;
            for (int i = 0; i < count; i++)
            {
                var p1x = (float)(rad * Math.Cos(cur_angle));
                var p1y = (float)(rad * Math.Sin(cur_angle));
                var p1 = new float[] { p1x, p1y, 0 };
                cur_angle += angle;
                var p2x = (float)(rad * Math.Cos(cur_angle));
                var p2y = (float)(rad * Math.Sin(cur_angle));
                var p2 = new float[] { p2x, p2y, 0 };
                var p3 = new float[] { 0, 0, 0 };
                mesh.AddRange(p1);
                mesh.AddRange(p2);
                mesh.AddRange(p3);




                var p1b = new float[] { p1x, p1y, height };
                var p2b = new float[] { p2x, p2y, height };
                var p3b = new float[] { 0, 0, height };
                mesh.AddRange(p1b);
                mesh.AddRange(p2b);
                mesh.AddRange(p3b);

                mesh.AddRange(p2b);
                mesh.AddRange(p1b);
                mesh.AddRange(p2);

                mesh.AddRange(p1);
                mesh.AddRange(p2);
                mesh.AddRange(p1b);


            }
            return mesh.ToArray();
        }
        public void draw_model_3d(GraphicGL graphic, NavigSys.SceneType cur_scene, bool visible )
        {
            if(update_model)
            {
                var cyl_mesh = cilindr_mesh((float)d / 2, 30, (float)l);
                name = graphic.add_buff_gl(cyl_mesh, new Color3d_GL(color[0], color[1], color[2]), null, PrimitiveType.Triangles, name, true, create_model);
                graphic.buffersGl.setlight(name, false);
                graphic.buffersGl.setTranspobj(name, 0.8f);
                create_model = false;
                update_model = false;
            }

            if (cur_scene == NavigSys.SceneType.camera1) graphic.buffersGl.setMatrobj(name, 0, UtilMatr.to_matrix(M_obj_to_camera1 * M_self));
            else if (cur_scene == NavigSys.SceneType.model3d) graphic.buffersGl.setMatrobj(name, 0, UtilMatr.to_matrix(M_self));


            for (int i = 0; i < models_name.Count; i++)
            {
                graphic.buffersGl.setVisibleobj(models_name[i], visible);
            }
        }
    }
    public class NavigRobotClient : TcpClientWrapper
    {

        public string _latestFrame;
        public object _lock = new object();
        public System.Windows.Forms.Timer _uiTimer;
        public int current_robot_turn = 0;
        public RobotFrame.RobotType robot_navig_type = RobotFrame.RobotType.RC5;


        public event Action<string> FrameUpdated;  // теперь имя события отражает суть
        string cur_pos = "";
        private string _currentFrame;
        public string CurrentFrame => _currentFrame;
        public int _messageCounter = 0;
        private DateTime _lastStatsTime = DateTime.UtcNow;
        public RobotFrame.RobotType robotType = RobotFrame.RobotType.RC5;

        public Matrix<double> M_base_in_world = UtilMatr.eye_matr(4) ;
        public Matrix<double> M_world_in_base = UtilMatr.eye_matr(4);
        
        public int cur_turn = 0;

        public string[] real_robot_models_names;
        public string[] virt_robot_models_names;

        public Label label_navig_robot_status_pose;
        protected override void OnMessageReceived(string message)
        {
            //Interlocked.Increment(ref _messageCounter);
            if (message != null)
            {
                _currentFrame = message;
                FrameUpdated?.Invoke(message);

            }
            _currentFrame = message;// Console.WriteLine($"Получено: {message}");

           
           /* try
            {
                var frame = JsonSerializer.Deserialize<RobotFrame>(message);
                if (frame != null)
                {
                    _currentFrame = frame;
                    FrameUpdated?.Invoke(frame);
                }
            }
            catch {  }*/
        }

        private void OnFrameUpdated(string frame)
        {
            lock (_lock) { _latestFrame = frame; }
        }

        private void OnDisconnected()
        {
            /*this.Invoke((MethodInvoker)(() =>
            {

                label_navig_robot_status.Text = "Отключён";
            }));*/
        }
        async public Task connect_start()
        {
            _uiTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _uiTimer.Tick += UiTimer_Tick;
           _uiTimer.Start();

            FrameUpdated += OnFrameUpdated;
            Disconnected += OnDisconnected;

            try
            {
                //label_navig_robot_status.Text = "Подключение...";

                string host = "localhost";
                var port = 30006;

                await ConnectAsync(host, port);
                //label_navig_robot_status.Text = "Подключено";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка подключения: {ex.Message}");
                //label_navig_robot_status.Text = "Ошибка";
            }
        }
        public async Task SendFrameAsync(RobotFrame frame)
        {
            string json = JsonSerializer.Serialize(frame);
            await SendAsync(json);
        }

        private void UiTimer_Tick(object sender, EventArgs e)
        {
            string frame;
            lock (_lock) { frame = _latestFrame; }
            if (frame != null)
            {
                if (frame.Count(',') == 5) label_navig_robot_status_pose.Text = frame;
                else { Console.WriteLine(frame.Count(',')); };
            }
        }
        Matrix4x4f[] ms = new Matrix4x4f[8];
        List<string> models_name = new List<string>();

        List<string> virt_robot_name = new List<string>();
        List<string> real_robot_name = new List<string>();
        public void init_draw_model_3d(GraphicGL graphic, NavigSys.SceneType cur_scene)
        {

            var color_arm = Color3d_GL.black();
            var color_end = Color3d_GL.white();
            for (int i = 0; i <= 7; i++)
            {
                {
                    var k_size = 1;
                    if(i==0) k_size = 1000;
                    var model_ax = new Model3d("models\\rc5\\" + i + ".stl", false, k_size);
                    var ax_name1 = graphic.add_buff_gl(model_ax, PrimitiveType.Triangles, real_robot_models + i); models_name.Add(ax_name1); real_robot_name.Add(ax_name1);
                    var ax_name2 = graphic.add_buff_gl(model_ax, PrimitiveType.Triangles, virt_robot_models + i); models_name.Add(ax_name2); virt_robot_name.Add(ax_name2);
                    graphic.buffersGl.setTranspobj(ax_name2, 0.5f);
                }

            }

            //var tool_name1 = graphic.add_buff_gl(new Model3d("models\\rc5\\t2b.stl", false, 1), color_end, PrimitiveType.Triangles, "t2");
           // models_name.Add(tool_name1);
            var L21 = 156;
            var L31 = -148;

            var L1 = 172.5;
            var L2 = 405;
            var L3 = 370;
            var L4 = 139.8 + L21 + L31;
            var L5 = 139.8;
            var L6 = 141;


            ms[0] = UtilMatr.matrix(new Point3d_GL(0,0,20), new Point3d_GL(90, 90));
            ms[1] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(90, 0));
            ms[2] = UtilMatr.matrix(new Point3d_GL(0 , -L1), new Point3d_GL(0, 180, 0));
            ms[3] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 0, 90)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 180, 0));
            ms[4] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 0, 90)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 180, 0));
            ms[5] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(90)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3, L4), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));
            ms[6] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 180, 180)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3 - L5, L4), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));
            ms[7] = UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(0, 0, -90)) * UtilMatr.matrix(new Point3d_GL(0), new Point3d_GL(180, 0, 0)) * UtilMatr.matrix(new Point3d_GL(0, -L1 - L2 - L3 - L5, L4 + L6), new Point3d_GL(0)) * UtilMatr.matrix(new Point3d_GL(), new Point3d_GL(0, 0, 0));

            for (int i = 0; i < 8; i++)
            {
                graphic.buffersGl.setMatrobj(real_robot_name[i], 0, ms[i]);
                graphic.buffersGl.setMatrobj(virt_robot_name[i], 0, ms[i]);
            }
            //graphic.buffersGl.setMatrobj(tool_name1, 0, ms[7]);

            set_conf_robot_pulse(new double[] { 0, 0, 0, 0, 0, 0 },true,false);
            set_conf_robot_pulse(new double[] { 0, 0, 0, 0, 0, 0 }, true, true);
            //GL1.buffersGl.setTranspobj("t2", 0);

        }
        public void draw_model_3d(GraphicGL graphic, NavigSys.SceneType cur_scene, bool visible)
        {
            for (int i = 0; i <= 7; i++)
            {
                if(cur_scene == NavigSys.SceneType.camera1) 
                {
                    graphic.buffersGl.setMatrobj(virt_robot_name[i], 0, UtilMatr.to_matrix(M_base_in_world)* qms_virt[i] );
                    graphic.buffersGl.setMatrobj(real_robot_name[i], 0, UtilMatr.to_matrix(M_base_in_world)* qms[i] );
                }
                else
                {
                    graphic.buffersGl.setMatrobj(virt_robot_name[i], 0, qms_virt[i]);
                    graphic.buffersGl.setMatrobj(real_robot_name[i], 0, qms[i]);
                }    
            }

            for (int i = 0; i < models_name.Count; i++)
            {
                graphic.buffersGl.setVisibleobj(models_name[i], visible);
            }
        }
        Matrix4x4f[] qms_virt = new Matrix4x4f[8];
        Matrix4x4f[] qms = new Matrix4x4f[8];
        string real_robot_models = "ax_";
        string virt_robot_models = "ax_virt_";
        public void set_conf_robot_pulse(double[] q, bool rad = true,bool virt = false)
        {
            //Matrix<double> M_base_in_world = null,
           //var q = new double[6];
           if(q==null || q.Length <6) return;
           if(!virt)
            {
                cur_ang = q;
            }
           Matrix4x4f base_matrix = Matrix4x4f.Identity;
            if (M_base_in_world != null)
            {
                base_matrix = UtilMatr.to_matrix(M_base_in_world);
            }
            for (int i = 0; i <= 7; i++)
            {
                var mq = Matrix4x4f.Identity;
                if (i >= 2)
                {
                    var j = i - 1;
                    var fr = RobotFrame.comp_forv_kinem(q, j, rad, robotType, false);
                    mq = UtilMatr.matrix(fr.position, fr.rotation.toDegree());
                }
                if (virt)
                {
                    qms_virt[i] = mq * ms[i];
                }
                else
                {
                    qms[i] =  mq * ms[i];
                }

            }
        }

        public async void send_navig_robot(string text)
        {
            try
            {
                if (!IsConnected)
                {
                    MessageBox.Show("Нет соединения.");
                    return;
                }

                await SendAsync(text);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка отправки: {ex.Message}");
            }
        }

        public string[] gen_poses_for_cal(double[] qs, Matrix<double> marker_frame, Matrix<double> prop_M_flange_in_marker)
        {
            var posrob = new RobotFrame(new Pose(qs, false), robotType);
            //test_handeye(marker_frame, posrob.getMatrix());
            cur_turn = RobotFrame.get_current_turn(qs, robotType, false);
            if(cur_turn<0 ) return null;
            //var prop_M_flange_in_marker = new RobotFrame(55, 55, -30, 0.0, 0.0, 1.57).getMatrix();
            var prop_M_base_in_world = NavigSys.set_prop_pos_robot(marker_frame, posrob.getMatrix(), prop_M_flange_in_marker);
            M_base_in_world = prop_M_base_in_world;
            var prop_M_world_in_base = UtilOpenCV.inv(prop_M_base_in_world);
            var gen_Ms_frames_in_world = gen_frames_v2(marker_frame, 20, 0.4, 7);
            var gen_Ms_flange_in_base = new Matrix<double>[gen_Ms_frames_in_world.Length];

            var poses_str = new string[gen_Ms_frames_in_world.Length];

            for (int i = 0; i < gen_Ms_frames_in_world.Length; i++)
            {
                gen_Ms_flange_in_base[i] = prop_M_world_in_base * gen_Ms_frames_in_world[i] * prop_M_flange_in_marker;
                var pose = RobotFrame.comp_inv_kinem_priv_rc5_real(new RobotFrame(gen_Ms_flange_in_base[i], robotType).frame, cur_turn);
                poses_str[i] = pose_to_str(pose,true);
            }
            return poses_str;
        }

        public void get_prop_pos(double[] qs, Matrix<double> marker_frame)
        {
            var posrob = new RobotFrame(new Pose(qs, false), robotType);
            cur_turn = RobotFrame.get_current_turn(qs, robotType, false);
            var prop_M_flange_in_marker = new RobotFrame(55, 55, -30, 0.0, 0.0, 1.57).getMatrix();
            var prop_M_base_in_world = NavigSys.set_prop_pos_robot(marker_frame, posrob.getMatrix(), prop_M_flange_in_marker);
            M_base_in_world = prop_M_base_in_world;

        }
        public static double[] parse_pose(string pose_str)
        {
            var pose = new double[6];
            if (!pose_str.Contains(',')) return null;
            var pose_l = pose_str.Split(',');
            if (pose_l.Length < 6) return null;
            for (int i = 0; i < 6; i++)
            {
                pose[i] = Convert.ToDouble(pose_l[i]);
            }
            return pose;
        }
        public static string pose_to_str(double[] pose, bool rad = true)
        {
            var pose_str = "";
            var pose_d = pose;
            if (rad)
            {
                pose_d = to_degree(pose);
            }
            for (int i = 0; i < pose.Length; i++)
            {
                if (i != 5) pose_str += Math.Round(pose_d[i], 4).ToString() + ",";
                else pose_str += Math.Round(pose_d[i], 4).ToString();
            }
            return pose_str;
        }

        static double[] to_degree(double[] vals)
        {
            if (vals == null) return null;
            if (vals.Length == 0) return null;
            var degree = new double[vals.Length];
            for (int i = 0; i < degree.Length; i++)
            {
                degree[i] = 180 * vals[i] / Math.PI;
            }
            return degree;
        }
        public void test_handeye(Matrix<double> start_M_marker_in_world, Matrix<double> start_M_flange_in_base)
        {
            var Ms_marker_in_world = gen_frames_v2(start_M_marker_in_world, 0.001, 0.5, 5);



            var real_M_flange_in_marker = new RobotFrame(52.3, 58.2, -28.4, 0.51, 0.82, -0.003).getMatrix();
            var prop_M_flange_in_marker = new RobotFrame(55, 55, -30, 0.5, 0.8).getMatrix();
            // -271.569051425043 - 471.1574661903 98.040136843397 51
            //- 271.694355768418 - 471.127457653968 98.0743107217747 52
            //- 271.824159505325 - 471.10296082707 98.0922115609049 53
            // var fr_p_flange_in_marker = new RobotFrame( -54.293, -32.924, 52.978, 2.652, -0.011, -3.131);

            var prop_M_marker_in_flange = UtilOpenCV.inv(prop_M_flange_in_marker);
            var real_M_marker_in_flange = UtilOpenCV.inv(real_M_flange_in_marker);
            var start_M_base_in_flange = UtilOpenCV.inv(start_M_flange_in_base);
            var start_M_world_in_marker = UtilOpenCV.inv(start_M_marker_in_world);
            var prop_M_base_in_world = start_M_marker_in_world * prop_M_flange_in_marker * start_M_base_in_flange;
            var prop_M_world_in_base = UtilOpenCV.inv(prop_M_base_in_world);
            var real_M_base_in_world = start_M_marker_in_world * real_M_flange_in_marker * start_M_base_in_flange;

            var prop_Ms_flange_in_base = new Matrix<double>[Ms_marker_in_world.Length];
            var real_Ms_marker_in_world = new Matrix<double>[Ms_marker_in_world.Length];

            var prop_Ms_base_in_flange = new Matrix<double>[Ms_marker_in_world.Length];
            var real_Ms_world_in_marker = new Matrix<double>[Ms_marker_in_world.Length];

            //UtilOpenCV.noise_matr();
            for (int i = 0; i < Ms_marker_in_world.Length; i++)
            {

                prop_Ms_flange_in_base[i] = prop_M_world_in_base * Ms_marker_in_world[i] * prop_M_flange_in_marker;
                real_Ms_marker_in_world[i] = real_M_base_in_world * prop_Ms_flange_in_base[i] * real_M_marker_in_flange;

                real_Ms_marker_in_world[i] = UtilOpenCV.noise_matr_transf_right_3p(real_Ms_marker_in_world[i], 100, 0, 0.2);

                prop_Ms_base_in_flange[i] = UtilOpenCV.inv(prop_Ms_flange_in_base[i]);
                real_Ms_world_in_marker[i] = UtilOpenCV.inv(real_Ms_marker_in_world[i]);
            }
            //StereoCamera.calibrate_stereo_rob_handeye_navig(real_Ms_marker_in_world, Ms_flange_in_base, real_M_marker_in_flange);

            StereoCamera.calibrate_stereo_rob_handeye_navig(prop_Ms_base_in_flange, real_Ms_marker_in_world, real_M_flange_in_marker);
            prin.t("fr_marker_in_flange_inv");
            prin.t(new RobotFrame(real_M_flange_in_marker).ToString());
        }

        static public Matrix<double>[] gen_frames_v2(Matrix<double> start_frame, double xyz_delt, double abc_delt, int count_one_axis = 5)
        {
            var fr_p = new RobotFrame(start_frame);

            var matrixes_gen = new List<Matrix<double>>();
            var xyz_delt_cur = -xyz_delt;
            var xyz_incr = 2 * xyz_delt / count_one_axis;

            var abc_delt_cur = -abc_delt;
            var abc_incr = 2 * abc_delt / count_one_axis;

            xyz_delt_cur = -xyz_delt;
            xyz_incr = 2 * xyz_delt / count_one_axis;

            abc_delt_cur = -abc_delt;
            abc_incr = 2 * abc_delt / count_one_axis;

            for (int i = 0; i < count_one_axis; i++)
            {
                var fp_gen = fr_p.Clone();
                fp_gen.X += xyz_delt_cur;
                fp_gen.A += abc_delt_cur;
                matrixes_gen.Add(fp_gen.getMatrix());
                xyz_delt_cur += xyz_incr;
                abc_delt_cur += abc_incr;
            }

            xyz_delt_cur = -xyz_delt;
            xyz_incr = 2 * xyz_delt / count_one_axis;

            abc_delt_cur = -abc_delt;
            abc_incr = 2 * abc_delt / count_one_axis;

            for (int i = 0; i < count_one_axis; i++)
            {
                var fp_gen = fr_p.Clone();
                fp_gen.Y += xyz_delt_cur;
                fp_gen.B += abc_delt_cur;
                matrixes_gen.Add(fp_gen.getMatrix());
                xyz_delt_cur += xyz_incr;
                abc_delt_cur += abc_incr;
            }

            xyz_delt_cur = -xyz_delt;
            xyz_incr = 2 * xyz_delt / count_one_axis;

            abc_delt_cur = -abc_delt;
            abc_incr = 2 * abc_delt / count_one_axis;

            for (int i = 0; i < count_one_axis; i++)
            {
                var fp_gen = fr_p.Clone();
                fp_gen.Z += xyz_delt_cur;
                fp_gen.C += abc_delt_cur;
                matrixes_gen.Add(fp_gen.getMatrix());
                xyz_delt_cur += xyz_incr;
            }



            return matrixes_gen.ToArray();
        }

        public double[] pose_dest = new double[6];
        double angle_vel = 0.1;//degr/s
        public bool move = false;
        double pose_err = 0.1;
        public double[] cur_ang;
        public double[] cur_ang_dest = new double[6];
        public void handler_move()
        {
            //Console.WriteLine("handler");
            // if(connect_start)
            if (move)
            {
                Console.WriteLine("move");
                var dist_dest = angles_dist(cur_ang, pose_dest);
                if (dist_dest < 0)
                {
                    move = false;
                    Console.WriteLine("handler_move dist<0");
                }
                if (dist_dest<pose_err)
                {
                    move = false;
                    Console.WriteLine("dist_dest<pose_err");
                }
                else
                {
                    var delt = comp_delt_ang(cur_ang_dest, pose_dest, angle_vel);
                    cur_ang_dest = angles_sum(cur_ang_dest, delt);
                    if (cur_ang_dest == null)
                    {
                        Console.WriteLine("cur_ang_dest == null");
                        move = false;
                    }
                        

                    if (move)
                    {

                        //Console.WriteLine(" posec " + pose_to_str(cur_ang_dest,false));
                       // Console.WriteLine(" posed " + pose_to_str(pose_dest, false));
                        send_navig_robot(" pose " + pose_to_str(cur_ang_dest,false));


                    }
                    
                }
                
            }
            if(!move)
            {
                cur_ang_dest = cur_ang;
            }

        }

        public static double[] angles_sum(double[] pose1, double[] pose2)
        {
            if(pose1== null || pose2 == null) return null;
            if (pose1.Length != pose2.Length) return null;
            var pose_sum = new double[pose1.Length];
            for(int i=0; i< pose1.Length; i++)
            {
                pose_sum[i]= pose1[i] + pose2[i];
            }
            return pose_sum;
        }
        public static double angles_dist(double[] pose1, double[] pose2)
        {
            var dist = 0d;
            if (pose1 == null || pose2 == null) return -1;
            if (pose1.Length != pose2.Length) return -1;
            for (int i = 0; i < pose1.Length; i++)
            {
                dist += Math.Abs(pose1[i] - pose2[i]);
            }
            return dist;
        }
        public static double[] comp_delt_ang(double[] pose_cur, double[] pose_dest, double max_delt)
        {
            if (pose_cur == null || pose_dest == null) return null;
            if (pose_cur.Length != pose_dest.Length) return null;
            if(max_delt <= 0) return null;

            var delt_full = new double[pose_cur.Length];
            double max_d = 0;
            for (int i = 0; i < pose_cur.Length; i++)
            {
                delt_full[i] = pose_dest[i] - pose_cur[i];
                var delt_cur = Math.Abs(delt_full[i]);
                if (delt_cur > max_d) max_d = delt_cur;

            }
            if (max_d > max_delt){
                var k = max_delt / max_d;
                for (int i = 0; i < delt_full.Length; i++)
                {
                    delt_full[i] = k* delt_full[i];
                }

            }

            return delt_full;
        }


    }


    public class NavigMarker
    {
        public int ind = -1;
        public double marker_size_mm = -1;
        public double near_dist = 1;
        public double far_dist = 1;
        public int size_circle = 1;
        public Point3d_GL[] ps_near;
        public Point3d_GL[] ps_far;
        
        static public  NavigMarker get_marker_p1v2(int ind)
        {
            var marker = new NavigMarker();

            marker.ind = ind;
            marker.marker_size_mm = 10.8;
            marker.near_dist = 13.6;
            marker.far_dist = 100;
            marker.size_circle = 50;

            return marker;
        }

    }


    public class NavigTool
    {
        public NavigMarker marker;
        public enum ToolType { tp4_v1, tp1_v1 };
        public enum ToolFuncType { Registr, Model , Other};
        public Point3d_GL[] ps;
        public List<Point3d_GL> trace_tcp = new List<Point3d_GL>();
        int[] inds;
        int[] ind_aruco;
        int aruco_corner_num=4;
        int aruco_number = 0;
        public ToolFuncType toolfunc = ToolFuncType.Other;

        Point3d_GL tcp;
        Point3d_GL rotate;

        public Matrix<double> matrix_frame = UtilMatr.eye_matr(4);
        public Matrix<double> matrix_frame_inv;
        public Matrix<double> matrix_model;
        public Matrix<double> matrix_model_inv;

        public Matrix<double> matrix_model_debug = UtilMatr.eye_matr(4);

        public string name_3d_model = "new_tool";
        public string path_3d_model = null;
        public string name_3d_model_debug = "new_debug_tool";
        public string path_3d_model_debug = null;
        public string name_3d_model_trace_tcp = "new_trace_tcp";
        
        public Matrix<double> matrix_tcp = UtilMatr.eye_matr(4);
        public ToolType tool_type;
        public List<List<Point3d_GL>> ps_for_registr = new List<List<Point3d_GL>>();

        /*
         * x down
         * y right 
         * 
         * y-> 
         * p1----------p0
         * 
         * x
         * ||    
         * \/
         * 
         * p2-----------p3

         * */

        public NavigTool(int[] _inds, ToolType _tool_type,string _name_3d_model=null, string _path_3d_model = null, Matrix<double> _matrix_model_debug = null, string _path_3d_model_debug = null) 
        {
            if (_inds == null) { Console.WriteLine("NavigTool _inds == null"); return; }
            aruco_number = _inds.Length;
            inds = new int[aruco_number];
            for (int i =0; i<_inds.Length;i++)
            {
                inds[i] = _inds[i];
            }
            tool_type = _tool_type;
            if (_tool_type == ToolType.tp4_v1)
            {
                if (aruco_number != 4)  Console.WriteLine("NavigTool inds.Length != 4 for ToolType.tp4_v1");
            }
            if (_tool_type == ToolType.tp1_v1)
            {
                if (aruco_number != 1) Console.WriteLine("NavigTool inds.Length != 1 for ToolType.tp1_v1");
            }
            if (_name_3d_model != null)
            {
                name_3d_model = _name_3d_model;
            }
            if (_path_3d_model != null)
            {
                path_3d_model = _path_3d_model;
            }
            if (_matrix_model_debug != null && _path_3d_model_debug != null)
            {
                matrix_model_debug = _matrix_model_debug;
                path_3d_model_debug = _path_3d_model_debug;
            }
            ind_aruco = comp_ind_table(inds);
        }
        public NavigTool(NavigMarker nmarker, ToolType _tool_type, string _name_3d_model = null, string _path_3d_model = null, Matrix<double> _matrix_model_debug = null, string _path_3d_model_debug = null)
        {
            if (nmarker == null) { Console.WriteLine("nmarker == null"); return; }

            marker = nmarker;
            aruco_number = 1;
            inds = new int[1];
            inds[0] = nmarker.ind;
            tool_type = _tool_type;
            if (_tool_type == ToolType.tp4_v1)
            {
                if (aruco_number != 4) Console.WriteLine("NavigTool inds.Length != 4 for ToolType.tp4_v1");
            }
            if (_tool_type == ToolType.tp1_v1)
            {
                if (aruco_number != 1) Console.WriteLine("NavigTool inds.Length != 1 for ToolType.tp1_v1");
            }
            if (_name_3d_model != null)
            {
                name_3d_model = _name_3d_model;
            }
            if (_path_3d_model != null)
            {
                path_3d_model = _path_3d_model;
            }
            if (_matrix_model_debug != null && _path_3d_model_debug != null)
            {
                matrix_model_debug = _matrix_model_debug;
                path_3d_model_debug = _path_3d_model_debug;
            }
            ind_aruco = comp_ind_table(inds);
        }
        static int[] comp_ind_table(int[] inds)
        {
            var max_ind = int.MinValue;
            for (int i = 0; i < inds.Length; i++)
            {
                max_ind = Math.Max(max_ind, inds[i]);
            }
            var table = new int [max_ind+1];
            for (int i = 0; i < inds.Length; i++)
            {
                table[inds[i]] = i;
            }
            return table;
        }
        public double calibrate_markers(Point3d_GL[][][] ps_cal)//detect markerspos relative p1  //ps not filterd
        {
            var ps = filter_ps_array(ps_cal);
            if(ps == null ) return -1;
            if(ps.Length <4 ) return -1;


            return 0;
        }
        public Point3d_GL calibrate_tcp_4p(Point3d_GL[][][] ps_cal)//ps filterd
        {
            var ps = ps_cal;
            if (ps == null) return Point3d_GL.notExistP();
            if (ps.Length < 5) return Point3d_GL.notExistP();
            var ms = new List<Matrix<double>>();
            var ps_calc = new List<Point3d_GL>();
            for (int i = 0; i < 5; i++)
            {
                var m = get_frame(ps[i]);
                ms.Add(m);
                ps_calc.Add(new Point3d_GL(m[0, 3], m[1, 3], m[2, 3]));
            }


            var p_cenr = find_center_sphere_4p(ps_calc)[0];

            var p_cenr_2 = find_center_sphere_4p(ps_calc.GetRange(1,4))[0];


            //Console.WriteLine("p_cenr: " + p_cenr);
            //Console.WriteLine("p_cenr2: " + p_cenr_2);

            var tcp_aver = new Matrix<double>(new double[,] {
                {0,0,0,0 },
                {0,0,0,0 },
                {0,0,0,0 },
                {0,0,0,0 }});
            int ps_count = 5;

            var tcp_list = new List<Matrix<double>>();
            for (int i = 0; i < ps_count; i++)
            {
                var m_inv = ms[i].Clone();

                CvInvoke.Invert(m_inv, m_inv, DecompMethod.Svd);

                var m_test = m_inv * ms[i];
                var tcp = new Matrix<double>(new double[,] {
                {1,0,0,p_cenr.x },
                {0,1,0,p_cenr.y },
                {0,0,1,p_cenr.z },
                {0,0,0,1 }});
                var m_tcp = m_inv * tcp;
                tcp_list.Add(m_tcp);
                tcp_aver += m_tcp;// m_inv* tcp
                //Console.WriteLine("m_inv * tcp");
                //prin.t(m_inv * tcp);

            }
            tcp_aver = tcp_aver / ps_count;
            var p_err = new Point3d_GL();

            for (int i = 0;i < ps_count;i++)
            {
                var p_i = new Point3d_GL(
                    Math.Abs(tcp_aver[0, 3] - tcp_list[i][0, 3]),
                    Math.Abs(tcp_aver[1, 3] - tcp_list[i][1, 3]),
                    Math.Abs(tcp_aver[2, 3] - tcp_list[i][2, 3]));
                p_err += p_i;
            }


            Console.WriteLine("Navig_calib p_err x y z: " + p_err/ps_count);



            this.matrix_tcp = tcp_aver;
            //Console.WriteLine("tcp_aver");
            //prin.t(tcp_aver);

            this.tcp = new Point3d_GL(tcp_aver[0, 3], tcp_aver[1, 3], tcp_aver[2, 3]);

            return this.tcp;
        }
        public Point3d_GL calibrate_tool_tcp_4p(Point3d_GL[][][] ps_cal)//ps filterd
        {
            var ps = ps_cal;
            if (ps == null) return Point3d_GL.notExistP();
            if (ps.Length < 5) return Point3d_GL.notExistP();
            var ms = new List<Matrix<double>>();
            var ps_calc = new List<Point3d_GL>();
            for (int i = 0; i < 5; i++)
            {
                var m = get_frame(ps[i]);
                if (m == null) continue;
                ms.Add(m);
                ps_calc.Add(new Point3d_GL(m[0, 3], m[1, 3], m[2, 3]));
            }


            var p_cenr = find_center_sphere_4p(ps_calc)[0];

            var p_cenr_2 = find_center_sphere_4p(ps_calc.GetRange(1, 4))[0];


            //Console.WriteLine("p_cenr: " + p_cenr);
            //Console.WriteLine("p_cenr2: " + p_cenr_2);

            var tcp_aver = new Matrix<double>(new double[,] {
                {0,0,0,0 },
                {0,0,0,0 },
                {0,0,0,0 },
                {0,0,0,0 }});
            int ps_count = 5;

            var tcp_list = new List<Matrix<double>>();
            for (int i = 0; i < ps_count; i++)
            {
                var m_inv = ms[i].Clone();

                CvInvoke.Invert(m_inv, m_inv, DecompMethod.LU);

                var m_test = m_inv * ms[i];
                var tcp = new Matrix<double>(new double[,] {
                {1,0,0,p_cenr.x },
                {0,1,0,p_cenr.y },
                {0,0,1,p_cenr.z },
                {0,0,0,1 }});
                var m_tcp = m_inv * tcp;
                tcp_list.Add(m_tcp);
                tcp_aver += m_tcp;// m_inv* tcp
                //Console.WriteLine("m_inv * tcp");
                //prin.t(m_inv * tcp);

            }
            tcp_aver = tcp_aver / ps_count;
            var p_err = new Point3d_GL();

            for (int i = 0; i < ps_count; i++)
            {
                var p_i = new Point3d_GL(
                    Math.Abs(tcp_aver[0, 3] - tcp_list[i][0, 3]),
                    Math.Abs(tcp_aver[1, 3] - tcp_list[i][1, 3]),
                    Math.Abs(tcp_aver[2, 3] - tcp_list[i][2, 3]));
                p_err += p_i;
            }


            Console.WriteLine("Navig_calib p_err x y z: " + p_err / ps_count);



            this.matrix_tcp = tcp_aver;
            //Console.WriteLine("tcp_aver");
            //prin.t(tcp_aver);
            this.tcp = new Point3d_GL(tcp_aver[0, 3], tcp_aver[1, 3], tcp_aver[2, 3]);
            this.matrix_tcp = new Matrix<double>(new double[,] {
                {1,0,0,tcp.x },
                {0,1,0,tcp.y },
                {0,0,1,tcp.z },
                {0,0,0,1 }});
            this.tcp = new Point3d_GL(tcp_aver[0, 3], tcp_aver[1, 3], tcp_aver[2, 3]);

            return this.tcp;
        }
        public static Point3d_GL[] find_center_sphere_4p(List<Point3d_GL> points)
        {
            if (points == null || points.Count < 4)
                throw new ArgumentException("Необходимо минимум 4 точки.");

            double x1 = points[0].x, y1 = points[0].y, z1 = points[0].z;
            double x2 = points[1].x, y2 = points[1].y, z2 = points[1].z;
            double x3 = points[2].x, y3 = points[2].y, z3 = points[2].z;
            double x4 = points[3].x, y4 = points[3].y, z4 = points[3].z;

            // Вспомогательные величины U, V, W
            double U = (z1 - z2) * (x3 * y4 - x4 * y3) - (z2 - z3) * (x4 * y1 - x1 * y4);
            double V = (z3 - z4) * (x1 * y2 - x2 * y1) - (z4 - z1) * (x2 * y3 - x3 * y2);
            double W = (z1 - z3) * (x4 * y2 - x2 * y4) - (z2 - z4) * (x1 * y3 - x3 * y1);

            double denominator = U + V + W;
            if (Math.Abs(denominator) < 1e-12)
                throw new InvalidOperationException("Точки компланарны или сфера не определена.");

            // Вычисление компонент центра (Ax, Bx, Cx, Dx) и т.д.
            double squared1 = x1 * x1 + y1 * y1 + z1 * z1;
            double squared2 = x2 * x2 + y2 * y2 + z2 * z2;
            double squared3 = x3 * x3 + y3 * y3 + z3 * z3;
            double squared4 = x4 * x4 + y4 * y4 + z4 * z4;

            double Ax = squared1 * (y2 * (z3 - z4) + y3 * (z4 - z2) + y4 * (z2 - z3));
            double Bx = squared2 * (y3 * (z4 - z1) + y4 * (z1 - z3) + y1 * (z3 - z4));
            double Cx = squared3 * (y4 * (z1 - z2) + y1 * (z2 - z4) + y2 * (z4 - z1));
            double Dx = squared4 * (y1 * (z2 - z3) + y2 * (z3 - z1) + y3 * (z1 - z2));

            double Ay = squared1 * (x2 * (z3 - z4) + x3 * (z4 - z2) + x4 * (z2 - z3));
            double By = squared2 * (x3 * (z4 - z1) + x4 * (z1 - z3) + x1 * (z3 - z4));
            double Cy = squared3 * (x4 * (z1 - z2) + x1 * (z2 - z4) + x2 * (z4 - z1));
            double Dy = squared4 * (x1 * (z2 - z3) + x2 * (z3 - z1) + x3 * (z1 - z2));

            double Az = squared1 * (x2 * (y3 - y4) + x3 * (y4 - y2) + x4 * (y2 - y3));
            double Bz = squared2 * (x3 * (y4 - y1) + x4 * (y1 - y3) + x1 * (y3 - y4));
            double Cz = squared3 * (x4 * (y1 - y2) + x1 * (y2 - y4) + x2 * (y4 - y1));
            double Dz = squared4 * (x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2));

            double x0 = 0.5 * (Ax - Bx + Cx - Dx) / denominator;
            double y0 = -0.5 * (Ay - By + Cy - Dy) / denominator;
            double z0 = 0.5 * (Az - Bz + Cz - Dz) / denominator;

            double radius = Math.Sqrt((x1 - x0) * (x1 - x0) +
                                      (y1 - y0) * (y1 - y0) +
                                      (z1 - z0) * (z1 - z0));

            return new Point3d_GL[]
            {
            new Point3d_GL(x0, y0, z0),
            new Point3d_GL(radius, radius, radius)
            };
        }

        public double calibrate_rot(Point3d_GL[][][] ps_cal)//ps not filterd
        {
            var ps = filter_ps_array(ps_cal);

            return 0;
        }

        public Matrix<double> get_frame(Point3d_GL[][] ps_finded,bool filtered = false)//ps not filterd
        {
            var ps = filter_ps( ps_finded);
            if (!check_aruko_ps(ps)) { return null; } 
            if (tool_type == ToolType.tp4_v1)
            {
                return get_frame_tr4_v1(ps);
            }
            else if (tool_type == ToolType.tp1_v1)
            {
                return get_frame_tr1_v1(ps, filtered);
            }
            return null;
        }
        public Matrix<double> get_frame_tcp(Point3d_GL[][] ps_finded)//ps not filterd
        {
            var ps = filter_ps(ps_finded);
            if (!check_aruko_ps(ps)) { return null; }
            if (tool_type == ToolType.tp4_v1)
            {
                get_frame_tr4_v1(ps);
                return  matrix_model;
            }
            if (tool_type == ToolType.tp1_v1)
            {
                get_frame_tr1_v1(ps);
                return matrix_model;
            }
            return null;
        }
        /*
         * x down
         * y right 
         * 
         * x - true axis
         * y-> 
         * p1----------p0
         * 
         * x
         * ||    
         * \/
         * 
         * p2-----------p3
         * */
        public Matrix<double> get_frame_tr4_v1(Point3d_GL[][] ps_finded)//ps filterd
        {
            var p0 = Point3d_GL.centr_mass(ps_finded[0]);
            var p1 = Point3d_GL.centr_mass(ps_finded[1]);
            var p2 = Point3d_GL.centr_mass(ps_finded[2]);
            var p3 = Point3d_GL.centr_mass(ps_finded[3]);

            var vx = (p2 - p1).normalize();
            var vyx = (p0 - p1).normalize();

            var vz = (vx | vyx).normalize();
            var vy = (vz | vx).normalize();
            matrix_frame = RobotFrame.matrix_basis_from_ps(new Point3d_GL[] { p0, p1, p2 });
            matrix_frame_inv = matrix_frame.Clone();
            CvInvoke.Invert(matrix_frame, matrix_frame_inv, DecompMethod.LU);

            if (tool_type == ToolType.tp4_v1)
            {
                matrix_model = matrix_frame * matrix_tcp ;
            }
            matrix_model_inv = matrix_model.Clone();
            CvInvoke.Invert(matrix_model, matrix_model_inv, DecompMethod.LU);

            ps = new Point3d_GL[] { p0, p1, p2, p3,new Point3d_GL(matrix_model[0,3], matrix_model[1, 3], matrix_model[2, 3]) };
            return matrix_frame;
        }

        int problem_counter = 0;
        int problem_max_count = 10;
        public Matrix<double> get_frame_tr1_v1(Point3d_GL[][] ps_finded,bool filtered = false)//ps filterd
        {
            var p0 = ps_finded[0][0];
            var p1 = ps_finded[0][3];
            var p2 = ps_finded[0][2];
            var p3 = ps_finded[0][1];

            var vx = (p2 - p1).normalize();
            var vyx = (p0 - p1).normalize();

            var vz = (vx | vyx).normalize();
            var vy = (vz | vx).normalize();
            var matrix_cur = RobotFrame.matrix_basis_from_ps(new Point3d_GL[] { p0, p1, p2 });
            if (matrix_frame!=null && filtered && ps!=null && ps.Length>4)
            {
                var matrix_prev = matrix_frame.Clone();               
                var p_cur = new Point3d_GL(matrix_cur[0, 3], matrix_cur[1, 3], matrix_cur[2, 3]);

                var dist_prev_cur = (p_cur - ps[5]).magnitude();
                //Console.WriteLine(dist_prev_cur);
                //Console.WriteLine(problem_counter);
                if (dist_prev_cur < 20 && problem_counter < problem_max_count)
                {
                    matrix_frame = matrix_frame + 0.5 * (matrix_cur - matrix_frame);
                    problem_counter = 0;

                }
                else
                {

                    problem_counter++;
                    if (problem_counter > problem_max_count)
                    {
                        problem_counter = 0;
                        matrix_frame = matrix_cur;

                    }
                       
                }
            }
            else
            {
                matrix_frame = matrix_cur;
            }
            
           
            matrix_frame_inv = matrix_frame.Clone();
            CvInvoke.Invert(matrix_frame, matrix_frame_inv, DecompMethod.LU); 
            matrix_model = matrix_frame * matrix_tcp;
            matrix_model_inv = matrix_model.Clone();
            CvInvoke.Invert(matrix_model, matrix_model_inv, DecompMethod.LU);
           
            ps = new Point3d_GL[] { p0, p1, p2, p3,
                new Point3d_GL(matrix_model[0, 3], matrix_model[1, 3], matrix_model[2, 3]),
                new Point3d_GL(matrix_frame[0, 3], matrix_frame[1, 3], matrix_frame[2, 3]),
                new Point3d_GL((p0 - p1).magnitude(), (p0 - p2).magnitude(), (p2 - p1).magnitude())
            };

            /*var ps_ext = gen_3d_ps_other_corners(new Point3d_GL(9, 9, 0), 50, 100, matrix_frame);
            var ps_l = ps.ToList();
            ps_l.AddRange(ps_ext);
            ps = ps_l.ToArray();*/

            

            return matrix_frame;
        }

        static Point3d_GL[] gen_3d_ps_other_corners(Point3d_GL pc, double marker_size, double corner_lin_dist, Matrix<double> matrix_frame)
        {
            var ps_marker = new Point3d_GL[] { new Point3d_GL(0, 0, 0), new Point3d_GL(marker_size, 0, 0), new Point3d_GL(marker_size, marker_size, 0), new Point3d_GL(0, marker_size, 0) };
            var pc_marker = new Point3d_GL(marker_size/2, marker_size / 2, 0);
            var ps_corn = Point3d_GL.add_arr(ps_marker, pc - pc_marker);

            var ps_corn_x = Point3d_GL.add_arr(ps_corn, new Point3d_GL(corner_lin_dist,0,0));
            var ps_corn_y = Point3d_GL.add_arr(ps_corn, new Point3d_GL( 0, corner_lin_dist, 0));
            var ps_corn_xy = Point3d_GL.add_arr(ps_corn, new Point3d_GL(corner_lin_dist, corner_lin_dist, 0));

            var ps_l = new List<Point3d_GL>();
            ps_l.AddRange(ps_corn_x);
            ps_l.AddRange(ps_corn_y);
            ps_l.AddRange(ps_corn_xy);
            var ps_arr = Point3d_GL.multMatr( ps_l.ToArray() , matrix_frame);
            return ps_arr;
        }

        Point3d_GL[][][] filter_ps_array(Point3d_GL[][][] ps)
         {
            var ps_filtr = new List<Point3d_GL[][]>();
            for(int i = 0; i < ps.Length; i++)
            {
                var frame_ps_filtr = filter_ps(ps[i]);

                if(check_aruko_ps(frame_ps_filtr))
                {
                    ps_filtr.Add(frame_ps_filtr);
                }                
            }
            return ps_filtr.ToArray();
        }

        Point3d_GL[][] filter_ps(Point3d_GL[][] ps)
        {
            if(ps==null) return null;
            var ps_filtr = new Point3d_GL[aruco_number][];
            for (int i = 0; i < ps_filtr.Length; i++)
            {
                ps_filtr[i] = new Point3d_GL[aruco_corner_num];
                for (int j = 0; j < ps_filtr[i].Length; j++)
                {

                    ps_filtr[i][j] = Point3d_GL.notExistP();
                }

            }

            for (int i = 0; i< ps.Length; i++)
            {
                if (ps[i]!=null && ps[i].Length!=0)
                {
                    var cur_aruco_ind = ps[i][0].ind;
                    if (inds.Contains(cur_aruco_ind))
                    {
                        for (int j = 0; j < ps[i].Length; j++)
                        {
                            if (ps[i][j].ind_sec < aruco_corner_num)
                            {
                                ps_filtr[ind_aruco[cur_aruco_ind]][ps[i][j].ind_sec] = ps[i][j];
                            }
                        }
                    }
                }
                
            }



            return ps_filtr;
        }

        static bool check_aruko_ps(Point3d_GL[][] ps)
        {
            if(ps==null) return false;
            bool check_done = true;
            for (int i = 0; i < ps.Length; i++)
            {
                if(ps[i]!=null)
                {
                    for (int j = 0; j < ps[i].Length; j++)
                    {

                        if (!ps[i][j].exist) check_done = false;
                    }
                }
                else
                {
                    check_done = false;
                }

            }

            return check_done;
        }


        public void add_point_for_registr(int ind_p,Point3d_GL p)
        {
            if(ind_p<ps_for_registr.Count)
            {
                ps_for_registr[ind_p].Add(p * matrix_frame_inv);
            }
        }

        public void init_points_for_registr(int count_p)
        {
            ps_for_registr = new List<List<Point3d_GL>>();
            for (int i = 0; i < count_p; i++)
            {
                ps_for_registr.Add(new List<Point3d_GL>());
            }
        }

        public Point3d_GL[] get_points_for_registr_draw()
        {
          return Point3d_GL.multMatr(Point3d_GL.unifPoints2d(ps_for_registr).ToArray(), matrix_model);
        }
        List<string> models_name = new List<string>();
        public void init_draw_model_3d(GraphicGL graphic, NavigSys.SceneType cur_scene)
        {

            if (path_3d_model != null) { name_3d_model = graphic.add_buff_gl(new Model3d(path_3d_model), PrimitiveType.Triangles, name_3d_model); models_name.Add(name_3d_model); }
            if (path_3d_model_debug != null) { name_3d_model_debug = graphic.add_buff_gl(new Model3d(path_3d_model_debug), PrimitiveType.Triangles, name_3d_model_debug); models_name.Add(name_3d_model_debug); }
        }
        public void draw_model_3d(GraphicGL graphic, NavigSys.SceneType cur_scene,bool visible)
        {

            if (matrix_model != null && name_3d_model != null) graphic.buffersGl.setMatrobj(name_3d_model, 0, UtilMatr.to_matrix(matrix_model));
            if (matrix_frame != null && matrix_model_debug != null && path_3d_model_debug != null) graphic.buffersGl.setMatrobj(name_3d_model_debug, 0, UtilMatr.to_matrix(matrix_frame * matrix_model_debug));
            //Console.WriteLine("\n\n" + j);

            if (toolfunc == ToolFuncType.Registr)
            {
                var ps_all = get_points_for_registr_draw();
                if (ps_all != null)
                    if (ps_all.Length != 0)
                        graphic.addPointMesh(ps_all, Color3d_GL.red(), name_3d_model_trace_tcp, false);
            }

            for (int i = 0; i < models_name.Count; i++)
            {
                graphic.buffersGl.setVisibleobj(models_name[i], visible);
            }

            /*var ps_name = "ps_test" + j;
            glControl1.Invoke((MethodInvoker)(() => GL1.buffersGl.removeObj(ps_name)));
            glControl1.Invoke((MethodInvoker)(() => GL1.addFrame(navig_system.tools[j].matrix_frame, 50, ps_name)));*/
        }
    }


    public class NavigSys
    {
        public enum SceneType {  model3d, camera1 };
        int aruko_max_ind = 1;

        //objs-------------------------------------------------
        public NavigTool[] tools;
        public List<NavigTarget> targets;
        public NavigRobotClient robot;
        public NavigModel model;
        public Scanner stereo;


        //registr-------------------------------------------------

        public int ind_model_instrument;
        public int ind_registr_instrument;

        //draw-------------------------------------------------
        public bool tools_vision = false;
        public bool targets_vision = false;
        public bool robot_vision = false;
        public bool model_vision = false;
        public bool camera_vision = false;
        public SceneType sceneType = SceneType.model3d;

        //cams-------------------------------------------------
       /* public int cam1_ind = 1;
        public int cam2_ind = 0;
        public int cam3_ind = 2;*/

        public int cam1_ind = 1;
        public int cam2_ind = 0;
        public int cam3_ind = 2;
        public static int cam_numbers = 3;
        public System.Drawing.PointF[][][] points2d_cams = new System.Drawing.PointF[cam_numbers][][];
        //-------------------------------------------------

        public NavigSys(Scanner _stereo, int _aruko_max_ind)
        {
            targets = new List<NavigTarget> ();
            stereo = _stereo;
            aruko_max_ind = _aruko_max_ind;
        }

        static public bool refine_marker(NavigMarker marker, ref System.Drawing.PointF[][] ps,ref Mat im)
        {
            //im is gray
            var psf = PointF.toPointF(ps[marker.ind]);
            var ps_near = WarpWithSurroundingsPs_v2(ref im, psf,marker.marker_size_mm, marker.near_dist , marker.size_circle,  true);
            var p0 = PointF.centr_mass(ps_near);
            if (ps_near == null) { return false; }
            var ps_far = WarpWithSurroundingsPs_v2(ref im, ps_near, marker.near_dist*2, marker.far_dist,(int)(3* marker.near_dist * marker.size_circle/ marker.near_dist), false);
            if (ps_far == null) { return false; }
            var ps_refine = new PointF[]
            {      p0 ,
                ps_far[2],
                ps_far[1],
                ps_far[0],
                
            };
            ps[marker.ind] = PointF.toSystemPoint(ps_refine) ;
           /* if(im.NumberOfChannels!=3)
            {
                CvInvoke.CvtColor(im, im, ColorConversion.Gray2Bgr);
            }
            UtilOpenCV.drawPoints(im, PointF.toPoint(ps[marker.ind]), 255, 255, 0, 5, true);*/
            /* 
             CvInvoke.Imshow("asd", im);
             CvInvoke.WaitKey();*/
            return true;
        }

        public System.Drawing.PointF[][] navigation_processing_get_points2d_3cam(ref Mat _mat,int cam_ind )
        {

            var points2d = new System.Drawing.PointF[aruko_max_ind][];
            var mat_orig = stereo.stereoCamera.cameraCVs[cam_ind].undist(_mat.Clone());
            var _mat_out = get_aruco_info(mat_orig, ref points2d);
            
            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i].marker != null)
                {
                    refine_marker(tools[i].marker, ref points2d, ref mat_orig);
                }
            }
            _mat = mat_orig;
            return points2d;
        }

        public Point3d_GL[][] navigation_processing_get_points3d_3cam(System.Drawing.PointF[][] points2d_1, System.Drawing.PointF[][] points2d_2, System.Drawing.PointF[][] points2d_3,Mat mat1 = null, Mat mat2 = null, Mat mat3 = null)
        {
            if (points2d_1 == null || points2d_2 == null || points2d_3 == null) return null;
            if (points2d_1.Length != points2d_2.Length || points2d_1.Length != points2d_3.Length) return null;
            var points3d = new Point3d_GL[points2d_1.Length][];
            for (int i = 0; i < points2d_1.Length; i++)
            {
                if (points2d_1[i] != null && points2d_2[i] != null && points2d_3[i] != null)
                {
                    if (points2d_1[i].Length != 0 && points2d_2[i].Length != 0 && points2d_3[i].Length != 0 )
                    {

                        
                        points3d[i] = stereo.stereoCamera.comp_points_3d_3cam(points2d_1[i], points2d_2[i], points2d_3[i], i);
                        

                        if(i==11)
                        {
                            //Console.WriteLine(" " + points3d[i][2].x + " " + points3d[i][2].y + " " + points3d[i][2].z + " ");
                            //UtilOpenCV.drawPoints(_mat_out1, PointF.toPoint(points_aruco1[i]), 255, 255, 0, 7, true);
                            //UtilOpenCV.drawPoints(_mat_out2, PointF.toPoint(points_aruco2[i]), 255, 255, 0, 7, true);

                            //Console.WriteLine(points_aruco1[i][2].X + " " +points_aruco1[i][2].Y + " " + points_aruco1[i][0].X + " " + points_aruco1[i][0].Y + " " + points_aruco1[i][3].X + " " + points_aruco1[i][3].Y + " " );
                            /* if(mat1!= null && mat2!= null && mat3 != null)
                             {
                                 mat1 = draw_points(points3d[i], mat1, 0, new MCvScalar(255, 0, 0));
                                 mat2 = draw_points(points3d[i], mat2, 1, new MCvScalar(255, 0, 0));
                                 mat3 = draw_points(points3d[i], mat3, 2, new MCvScalar(255, 0, 0));
                                 CvInvoke.Imshow("_mat_out1", mat1);
                                 CvInvoke.Imshow("_mat_out2", mat2);
                                 CvInvoke.Imshow("_mat_out3", mat3);
                                 // CvInvoke.Imshow("_mat_out2", _mat_out2);
                                 CvInvoke.WaitKey();
                             }*/

                        }
                    }
                    else
                    {
                        points3d[i] = null;
                    }
                }
                else
                {
                    points3d[i] = null;
                }
            }

            
            return points3d;
        }

        public Point3d_GL[][] navigation_processing_get_points3d(Mat _mat1, Mat _mat2, out Mat _mat_out1, out Mat _mat_out2)
        {

            System.Drawing.PointF[][] points_aruco1 = new System.Drawing.PointF[aruko_max_ind][];
            System.Drawing.PointF[][] points_aruco2 = new System.Drawing.PointF[aruko_max_ind][];

            Point3d_GL[][] points3d_aruco = new Point3d_GL[aruko_max_ind][];
            var mat1_orig = stereo.stereoCamera.cameraCVs[0].undist(_mat1.Clone());
            var mat2_orig = stereo.stereoCamera.cameraCVs[1].undist(_mat2.Clone());

            _mat_out1 = get_aruco_info(mat1_orig, ref points_aruco1);
            _mat_out2 = get_aruco_info(mat2_orig, ref points_aruco2);



            for (int i = 0; i < tools.Length; i++)
            {
                if (tools[i].marker != null)
                {
                    refine_marker(tools[i].marker, ref points_aruco1, ref mat1_orig);
                    refine_marker(tools[i].marker, ref points_aruco2, ref mat2_orig);
                }

            }


            for (int i = 0; i < points_aruco1.Length; i++)
            {
                if (points_aruco1[i] != null && points_aruco2[i] != null)
                {
                    if (points_aruco1[i].Length != 0 && points_aruco2[i].Length != 0)
                    {


                        points3d_aruco[i] = stereo.stereoCamera.comp_points_3d(points_aruco1[i], points_aruco2[i], i);
                        //Console.WriteLine(i + " " + points3d_aruco[i][0].x + " " + points3d_aruco[i][0].z + " ");

                        /*if(i==11)
                        {
                            
                            UtilOpenCV.drawPoints(_mat_out1, PointF.toPoint(points_aruco1[i]), 255, 255, 0, 7, true);
                            UtilOpenCV.drawPoints(_mat_out2, PointF.toPoint(points_aruco2[i]), 255, 255, 0, 7, true);

                            _mat_out1 = draw_points(points3d_aruco[i], _mat_out1, 0, new MCvScalar(255, 0, 0));
                             _mat_out2 = draw_points(points3d_aruco[i], _mat_out2, 1, new MCvScalar(255, 0, 0));

                            CvInvoke.Imshow("_mat_out1", _mat_out1);
                            CvInvoke.Imshow("_mat_out2", _mat_out2);
                            CvInvoke.WaitKey();
                        }*/
                    }
                    else
                    {
                        points3d_aruco[i] = null;
                    }
                }
                else
                {
                    points3d_aruco[i] = null;
                }
            }


            return points3d_aruco;
        }

        public static Mat WarpArucoArucoWithSurroundings(Mat image, int markerId, int markerOutputSize = 100,double marginFactor = 10,Dictionary.PredefinedDictionaryName dictionary = Dictionary.PredefinedDictionaryName.Dict4X4_50,Gray backgroundColor = default)
        {
            if (marginFactor < 0)
                throw new ArgumentException("marginFactor должно быть >= 0.", nameof(marginFactor));

           // if (backgroundColor == default)
                backgroundColor = new Gray(0d);

            // 1. Детектируем маркеры
            var arucoDict = new Dictionary(dictionary);
            var detectorParams = DetectorParameters.GetDefault();

            var corners = new VectorOfVectorOfPointF();
            var ids = new VectorOfInt();
            var rejected = new VectorOfVectorOfPointF();

            ArucoInvoke.DetectMarkers(image, arucoDict, corners, ids, detectorParams, rejected);

            // 2. Ищем нужный ID
            int index = -1;
            for (int i = 0; i < ids.Size; i++)
            {
                if (ids[i] == markerId)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                throw new Exception($"Маркер с ID {markerId} не найден.");

            System.Drawing.PointF[] srcPoints = corners[index].ToArray();

            // 3. Вычисляем размер выходного изображения и координаты маркера в центре
            int outputSize = (int)Math.Round(markerOutputSize * (1 + 2 * marginFactor));
            int offset = (int)Math.Round(marginFactor * markerOutputSize);
            int markerSize = markerOutputSize; // маркер будет точно этого размера

            System.Drawing.PointF[] dstPoints = new System.Drawing.PointF[]
            {
            new System.Drawing.PointF(offset, offset),
            new System.Drawing.PointF(offset + markerSize - 1, offset),
            new System.Drawing.PointF(offset + markerSize - 1, offset + markerSize - 1),
            new System.Drawing.PointF(offset, offset + markerSize - 1)
            };

            // 4. Вычисляем гомографию и применяем перспективное преобразование
            Mat homography = CvInvoke.FindHomography(srcPoints, dstPoints, RobustEstimationAlgorithm.Ransac);

            Mat warpedMat = new Mat();
            MCvScalar borderColor = new MCvScalar(backgroundColor.Intensity);
            CvInvoke.WarpPerspective(image, warpedMat, homography, new Size(outputSize, outputSize),
                Inter.Linear, Warp.Default, BorderType.Constant, borderColor);

            return warpedMat;
        }
        
        public static Mat WarpWithSurroundings(Mat image, System.Drawing.PointF[] srcPoints, int markerOutputSize = 100, double marginFactor = 10, Dictionary.PredefinedDictionaryName dictionary = Dictionary.PredefinedDictionaryName.Dict4X4_50, Gray backgroundColor = default)
        {
            // 3. Вычисляем размер выходного изображения и координаты маркера в центре
            int outputSize = (int)Math.Round(markerOutputSize * (1 + 2 * marginFactor));
            int offset = (int)Math.Round(marginFactor * markerOutputSize);
            int markerSize = markerOutputSize; // маркер будет точно этого размера

            System.Drawing.PointF[] dstPoints = new System.Drawing.PointF[]
            {
            new System.Drawing.PointF(offset, offset),
            new System.Drawing.PointF(offset + markerSize - 1, offset),
            new System.Drawing.PointF(offset + markerSize - 1, offset + markerSize - 1),
            new System.Drawing.PointF(offset, offset + markerSize - 1)
            };

            Mat homography = CvInvoke.FindHomography(srcPoints, dstPoints, RobustEstimationAlgorithm.Ransac);
            Mat warpedMat = new Mat();
            MCvScalar borderColor = new MCvScalar(backgroundColor.Intensity);
            CvInvoke.WarpPerspective(image, warpedMat, homography, new Size(outputSize, outputSize),
                Inter.Linear, Warp.Default, BorderType.Constant, borderColor);

            return warpedMat;
        }

        public static PointF[] WarpWithSurroundingsPs(ref Mat image, PointF[] srcPoints,double internal_size, double external_size, int markerOutputSize, bool negativ = true, Dictionary.PredefinedDictionaryName dictionary = Dictionary.PredefinedDictionaryName.Dict4X4_50, Gray backgroundColor = default)
        {
            if (srcPoints == null) return null;
            if (srcPoints.Length!=4) return null;
            //var orig_image = image.Clone();
            if (image.NumberOfChannels == 3)
            {
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
            }
            if (internal_size == 0) return null;
            var k_pix = markerOutputSize/internal_size ;
            int board = (int)((external_size+ internal_size /2)* k_pix);


            // 3. Вычисляем размер выходного изображения и координаты маркера в центре
            int outputSize = markerOutputSize + 2*board;
            //var size_warp = 

            System.Drawing.PointF[] dstPoints = new System.Drawing.PointF[]
            {
                new System.Drawing.PointF(board, board),
                new System.Drawing.PointF(board + markerOutputSize , board),
                new System.Drawing.PointF(board + markerOutputSize, board + markerOutputSize ),
                new System.Drawing.PointF(board, board + markerOutputSize)
            };


            var pc = new PointF(outputSize / 2, outputSize / 2);
            Mat homography = CvInvoke.FindHomography(PointF.toSystemPoint( srcPoints), dstPoints, RobustEstimationAlgorithm.Ransac);
            Mat warpedMat = new Mat();
            MCvScalar borderColor = new MCvScalar(backgroundColor.Intensity);
            var homography_inv = homography.Clone();
            CvInvoke.Invert(homography_inv, homography_inv, DecompMethod.LU);
            CvInvoke.WarpPerspective(image, warpedMat, homography, new Size(outputSize, outputSize),
                Inter.Linear, Warp.Default, BorderType.Constant, borderColor);


            var im1 = warpedMat;
            //var warp_orig = warpedMat.Clone();

            CvInvoke.GaussianBlur(im1, im1, new Size(13, 13), -1);
            CvInvoke.AdaptiveThreshold(im1, im1, 255, AdaptiveThresholdType.MeanC, ThresholdType.BinaryInv, 13, 13);
            
            
            var x_pred = (external_size ) * k_pix;
            var y_pred = (external_size ) * k_pix; 
            var ps2d_predict = new PointF[]
            {
                pc + new PointF((float)x_pred,(float)y_pred),
                pc + new PointF((float)-x_pred,(float)y_pred),
                pc + new PointF((float)-x_pred,(float)-y_pred),
                pc + new PointF((float)x_pred,(float)-y_pred),
            };
            if(!negativ)
            {
                ps2d_predict = new PointF[] { 
                    pc + new PointF(-(float)x_pred, 0),
                    pc + new PointF(-(float)x_pred, (float)y_pred),
                    pc + new PointF(0, (float)y_pred),
                    
                };
                
            }
            

            var ps_near = refine_contours(ps2d_predict, im1);

            //CvInvoke.CvtColor(warp_orig, warp_orig, ColorConversion.Gray2Bgr);
            //UtilOpenCV.drawPointsF(warp_orig, ps_near, 255, 0, 0, 3, true);
            if(!negativ)
            {
               // image = warp_orig;
            }
            //CvInvoke.Imshow("asd" + negativ, im1);
            //CvInvoke.WaitKey();
            if (ps_near == null) return null;
            var vpf_warp = new VectorOfPointF(PointF.toSystemPoint(ps_near));

            ps_near = PointF.toPointF( CvInvoke.PerspectiveTransform( PointF.toSystemPoint(ps_near), homography_inv));
           
            /* CvInvoke.WarpPerspective(vpf_warp, vpf_warp, homography_inv, new Size(outputSize, outputSize),
                 Inter.Linear, Warp.Default, BorderType.Constant, borderColor);*/





            return  ps_near;
        }

        public static PointF[] WarpWithSurroundingsPs_v2(ref Mat image, PointF[] srcPoints, double internal_size, double external_size, int markerOutputSize, bool negativ = true, Dictionary.PredefinedDictionaryName dictionary = Dictionary.PredefinedDictionaryName.Dict4X4_50, Gray backgroundColor = default)
        {
            if (srcPoints == null) return null;
            if (srcPoints.Length != 4) return null;
            //var orig_image = image.Clone();
            if (image.NumberOfChannels == 3)
            {
                CvInvoke.CvtColor(image, image, ColorConversion.Bgr2Gray);
            }
            if (internal_size == 0) return null;
            var k_pix = markerOutputSize / internal_size;
            int board = (int)((external_size + internal_size / 2) * k_pix);


            // 3. Вычисляем размер выходного изображения и координаты маркера в центре
            int outputSize = markerOutputSize + 2 * board;
            //var size_warp = 

            System.Drawing.PointF[] dstPoints = new System.Drawing.PointF[]
            {
                new System.Drawing.PointF(board, board),
                new System.Drawing.PointF(board + markerOutputSize , board),
                new System.Drawing.PointF(board + markerOutputSize, board + markerOutputSize ),
                new System.Drawing.PointF(board, board + markerOutputSize)
            };


            var pc = new PointF(outputSize / 2, outputSize / 2);
            Mat homography = CvInvoke.FindHomography(PointF.toSystemPoint(srcPoints), dstPoints, RobustEstimationAlgorithm.Ransac);
            Mat warpedMat = new Mat();
            MCvScalar borderColor = new MCvScalar(backgroundColor.Intensity);
            var homography_inv = homography.Clone();
            if(homography_inv == null || homography_inv.GetData()==null) return null;
            CvInvoke.Invert(homography_inv, homography_inv, DecompMethod.LU);
            CvInvoke.WarpPerspective(image, warpedMat, homography, new Size(outputSize, outputSize),
                Inter.Linear, Warp.Default, BorderType.Constant, borderColor);


            var im1 = warpedMat;
            var warp_orig = warpedMat.Clone();

            //CvInvoke.GaussianBlur(im1, im1, new Size(13, 13), -1);
            //CvInvoke.AdaptiveThreshold(im1, im1, 255, AdaptiveThresholdType.MeanC, ThresholdType.BinaryInv, 13, 13);


            var x_pred = (external_size) * k_pix;
            var y_pred = (external_size) * k_pix;
            var ps2d_predict = new PointF[]
            {
                pc + new PointF((float)x_pred,(float)y_pred),
                pc + new PointF((float)-x_pred,(float)y_pred),
                pc + new PointF((float)-x_pred,(float)-y_pred),
                pc + new PointF((float)x_pred,(float)-y_pred),
            };
            if (!negativ)
            {
                ps2d_predict = new PointF[] {
                    pc + new PointF(-(float)x_pred, 0),
                    pc + new PointF(-(float)x_pred, (float)y_pred),
                    pc + new PointF(0, (float)y_pred),

                };

            }


            var ps_near = refine_contours_v2(ps2d_predict, im1);

           /* CvInvoke.CvtColor(warp_orig, warp_orig, ColorConversion.Gray2Bgr);
            UtilOpenCV.drawPointsF(warp_orig, ps_near, 255, 0, 0, 3, true);
            if (!negativ)
            {
                 image = warp_orig;
            }
            CvInvoke.Imshow("asd" + negativ, image);
            CvInvoke.WaitKey();*/
            if (ps_near == null) return null;
            var vpf_warp = new VectorOfPointF(PointF.toSystemPoint(ps_near));

            ps_near = PointF.toPointF(CvInvoke.PerspectiveTransform(PointF.toSystemPoint(ps_near), homography_inv));

            /* CvInvoke.WarpPerspective(vpf_warp, vpf_warp, homography_inv, new Size(outputSize, outputSize),
                 Inter.Linear, Warp.Default, BorderType.Constant, borderColor);*/





            return ps_near;
        }

        public static PointF[] refine_contours_v2(PointF[] pcs_predict, Mat _mat_out1n)
        {
            var ps = new System.Drawing.PointF[pcs_predict.Length];

            FindCircles.findCircles_v2(_mat_out1n,ref ps, new Size(5, 5), false);
            var psc = PointF.toPointF(ps);
            var ps2_predict = pcs_predict;
            if (psc == null) return null;
            if (psc.Length == 0) return null;

            if (ps2_predict == null) return null;
            if (ps2_predict.Length == 0) return null;

            //UtilOpenCV.drawPointsF(_mat_out1, PointF.toSystemPoint(ps2_predict), 128, 128, 128, 3);
            //UtilOpenCV.drawPointsF(_mat_out1, PointF.toSystemPoint(psc), 255, 255, 0, 3);

            for (int i = 0; i < ps2_predict.Length; i++)
            {
                var j_min = 0;
                var dist_min = double.MaxValue;
                for (int j = 0; j < psc.Length; j++)
                {
                    var cur_dist = (ps2_predict[i] - psc[j]).norm;
                    if (cur_dist < dist_min)
                    {
                        j_min = j;
                        dist_min = cur_dist;
                    }

                }
                ps2_predict[i] = psc[j_min];
            }


            //var ps_result = PointF.toSystemPoint(ps2_predict);

            /* UtilOpenCV.drawPointsF(_mat_out1n, ps2_predict, 255, 255, 255, 3, true);
             CvInvoke.Imshow("asd", _mat_out1n);
             CvInvoke.WaitKey();*/
            return ps2_predict;
        }

        public static PointF[] refine_contours(PointF[] pcs_predict, Mat _mat_out1n)
        {

            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(_mat_out1n, contours, hier, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);
            //var conts = sameContours_cv(contours);
            // CvInvoke.CvtColor(_mat_out1, _mat_out1, ColorConversion.Gray2Bgr);
            contours = FindCircles.size_filter(contours, 3, 300);
            /*contours = FindCircles.only_same_centres_illum(contours, 2);
            contours = FindCircles.sameContours(contours, 0.3, 1);
            contours = FindCircles.illum_same_centres(contours, 3);*/
            var psc = PointF.toPointF(FindCircles.get_pc_conts(contours));
            //draw_conts_with_charachts(_mat_out1n.Clone(), contours);
            var ps2_predict = pcs_predict;
            if (psc == null) return null;
            if (psc.Length == 0) return null;

            if (ps2_predict == null) return null;
            if (ps2_predict.Length == 0) return null;

            //UtilOpenCV.drawPointsF(_mat_out1, PointF.toSystemPoint(ps2_predict), 128, 128, 128, 3);
            //UtilOpenCV.drawPointsF(_mat_out1, PointF.toSystemPoint(psc), 255, 255, 0, 3);

            for (int i = 0; i < ps2_predict.Length; i++)
            {
                var j_min = 0;
                var dist_min = double.MaxValue;
                for (int j = 0; j < psc.Length; j++)
                {
                    var cur_dist = (ps2_predict[i] - psc[j]).norm;
                    if (cur_dist < dist_min)
                    {
                        j_min = j;
                        dist_min = cur_dist;
                    }

                }
                ps2_predict[i] = psc[j_min];
            }


            //var ps_result = PointF.toSystemPoint(ps2_predict);

           /* UtilOpenCV.drawPointsF(_mat_out1n, ps2_predict, 255, 255, 255, 3, true);
            CvInvoke.Imshow("asd", _mat_out1n);
            CvInvoke.WaitKey();*/
            return ps2_predict;
        }

        public void navigation_processing_draw_points3d(Point3d_GL[] ps, ref Mat _mat_out1, ref Mat _mat_out2)
        {
            

        }

        public NavigTool precise_frame_comp( NavigTool tool, ref Mat _mat_out1,ref Mat _mat_out2)
        {
            if (tool.tool_type != NavigTool.ToolType.tp1_v1) return tool;
            var ps1 = precise_points_circles(tool, _mat_out1, 0);
            var ps2 = precise_points_circles(tool, _mat_out2, 1);

            var ps3d  = stereo.stereoCamera.comp_points_3d(ps1, ps2);
            var ps_for_frame = new Point3d_GL[1][];
            if (ps3d == null) { return tool; }
            if(ps3d.Length != 3) { return tool; }
            var p4 = Point3d_GL.centr_mass(new Point3d_GL[] { tool.ps[0], tool.ps[1], tool.ps[2], tool.ps[3] });
            ps_for_frame[0] = new Point3d_GL[] { ps3d[1], ps3d[2],  ps3d[0], p4  };

            _mat_out1 = draw_points(ps_for_frame[0], _mat_out1, 0, new MCvScalar(255, 0, 0));
           // _mat_out2 = draw_points(ps_for_frame[0], _mat_out2, 1, new MCvScalar(0, 255, 0));
            tool.get_frame_tr1_v1(ps_for_frame);


            return tool;
        }
        public Mat draw_points(Point3d_GL[] ps, Mat _mat_out1, int ind, MCvScalar color)
        {
            if(_mat_out1.NumberOfChannels != 3) { CvInvoke.CvtColor(_mat_out1, _mat_out1, ColorConversion.Gray2Bgr); }
            var mat_orig = _mat_out1.Clone();
            if (ps == null || ps.Length == 0) return null;
            var ps_draw = new PointF[ps.Length];
            for (int i = 0; i < ps.Length; i++)
            {

                var p3d_cam_0 = stereo.stereoCamera.cameraCVs[0].matrixSC * ps[i];
                if (ind == 1)
                {
                    p3d_cam_0 = stereo.stereoCamera.R_inv * p3d_cam_0;
                }
                if (ind == 2)
                {
                    p3d_cam_0 = stereo.stereoCamera.R2_inv * p3d_cam_0;
                }
                var p2d_0 = stereo.stereoCamera.cameraCVs[ind].point2DfromCam(p3d_cam_0);
                ps_draw[i] = new PointF(p2d_0.X, p2d_0.Y);
                

            }
            _mat_out1 = UtilOpenCV.drawPointsF(_mat_out1, ps_draw, (int)color.V2, (int)color.V1, (int)color.V0, 4, true);
            return _mat_out1;
        }
        public System.Drawing.PointF[] precise_points_circles(NavigTool tool, Mat _mat_out1, int ind)
        {
            var mat_orig = _mat_out1.Clone();
            var ps = tool.ps;
            if (ps == null || ps.Length == 0) return null;
            if (ps.Length != 17) return null;
            var ps2di_0 = new System.Drawing.Point[12];


            for (int i = 0; i < ps.Length; i++)
            {

                var p3d_cam_0 = stereo.stereoCamera.cameraCVs[0].matrixSC * ps[i];
                if(ind==1)
                {
                    p3d_cam_0 = stereo.stereoCamera.R_inv * p3d_cam_0;
                }
                var p2d_0 = stereo.stereoCamera.cameraCVs[ind].point2DfromCam(p3d_cam_0);
                var p2di_0 = new System.Drawing.Point((int)p2d_0.X, (int)p2d_0.Y);
                //CvInvoke.DrawMarker(_mat_out1, p2di_0, new MCvScalar(0, 0, 255), MarkerTypes.Cross, 5);

                if (i > 4)
                {
                    ps2di_0[i - 5] = p2di_0;
                }

            }

            var ps2di_0_arr = new System.Drawing.Point[3][];
            ps2di_0_arr[0] = new System.Drawing.Point[4] { ps2di_0[0], ps2di_0[1], ps2di_0[2], ps2di_0[3] };
            ps2di_0_arr[1] = new System.Drawing.Point[4] { ps2di_0[4], ps2di_0[5], ps2di_0[6], ps2di_0[7] };
            ps2di_0_arr[2] = new System.Drawing.Point[4] { ps2di_0[8], ps2di_0[9], ps2di_0[10], ps2di_0[11] };

            var ps2di_0_arr_c = new System.Drawing.PointF[] {
                new System.Drawing.PointF((ps2di_0_arr[0][0].X+ ps2di_0_arr[0][2].X)/2,( ps2di_0_arr[0][0].Y + ps2di_0_arr[0][2].Y)/2),
                new System.Drawing.PointF((ps2di_0_arr[1][0].X+ ps2di_0_arr[1][2].X)/2,( ps2di_0_arr[1][0].Y + ps2di_0_arr[1][2].Y)/2),
                new System.Drawing.PointF((ps2di_0_arr[2][0].X+ ps2di_0_arr[2][2].X)/2,( ps2di_0_arr[2][0].Y + ps2di_0_arr[2][2].Y)/2),};

            var _mat_out1n = KeepInsidePolygonsAndFillOutside(mat_orig, ps2di_0_arr, new Bgr(0, 0, 0));

            CvInvoke.CvtColor(_mat_out1n, _mat_out1n, ColorConversion.Bgr2Gray);
            CvInvoke.GaussianBlur(_mat_out1n, _mat_out1n, new Size(5, 5), -1);
            CvInvoke.AdaptiveThreshold(_mat_out1n, _mat_out1n, 255, AdaptiveThresholdType.MeanC, ThresholdType.BinaryInv, 7, 7);


            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            Mat hier = new Mat();
            CvInvoke.FindContours(_mat_out1n, contours, hier, RetrType.Ccomp, ChainApproxMethod.ChainApproxSimple);
            //var conts = sameContours_cv(contours);

           // CvInvoke.CvtColor(_mat_out1, _mat_out1, ColorConversion.Gray2Bgr);

            contours = FindCircles.size_filter(contours, 20, 3800);
            contours = FindCircles.only_same_centres_illum(contours, 2);

            var conts = FindCircles.sameContours(contours, 0.3, 1);
            conts = FindCircles.illum_same_centres(conts, 3);


            var psc = PointF.toPointF(FindCircles.get_pc_conts(conts));


            var ps2_predict = PointF.toPointF(ps2di_0_arr_c);
            if (psc == null) return null;
            if (psc.Length==0) return null;

            if (ps2_predict == null) return null;
            if (ps2_predict.Length == 0) return null;

            //UtilOpenCV.drawPointsF(_mat_out1, PointF.toSystemPoint(ps2_predict), 128, 128, 128, 3);

            //UtilOpenCV.drawPointsF(_mat_out1, PointF.toSystemPoint(psc), 255, 255, 0, 3);

            for (int i = 0; i < ps2_predict.Length; i++)
            {
                var j_min = 0;
                var dist_min = double.MaxValue;
                for (int j = 0; j < psc.Length; j++)
                {
                    var cur_dist = (ps2_predict[i] - psc[j]).norm;
                    if (cur_dist < dist_min)
                    {
                        j_min = j;
                        dist_min = cur_dist;
                    }
                   
                }
                ps2_predict[i] = psc[j_min];
            }


            var ps_result = PointF.toSystemPoint(ps2_predict);

            //UtilOpenCV.drawPointsF(_mat_out1, ps_result, 255, 255, 255, 3, true);

            return ps_result;
        }

        public static Mat draw_conts_with_charachts(Mat mat, VectorOfVectorOfPoint conts)
        {
            CvInvoke.CvtColor(mat, mat, ColorConversion.Gray2Bgr);
            Random random = new Random();
            if (conts == null) return mat;
            if (conts != null) CvInvoke.DrawContours(mat, conts, -1, new MCvScalar(0, 255, 0), 1, LineType.EightConnected);
            Console.WriteLine("__________");
            for (int i = 0; i<conts.Size;i++)
            {
                var area = CvInvoke.ContourArea(conts[i]);
                var cont_pc = FindCircles.findCentrCont(conts[i]);
                var cont_fig = FindCircles.sumHuMom(conts[i]);
                var cont_line = CvInvoke.ArcLength(conts[i],true);
                var f2 = area/ cont_line;
                //if(cont_fig < 0.17 && cont_fig > 0.15)
                {
                    var text = " f: " + Math.Round(10 * cont_fig, 2) + " f2: " + Math.Round(10 * f2, 2) + " area: " + Math.Round( area, 2);
                    CvInvoke.PutText(mat, text, new System.Drawing.Point((int)cont_pc.X, (int)cont_pc.Y), FontFace.HersheyTriplex, 0.5, new MCvScalar(0, 0, 255));
                    CvInvoke.DrawMarker(mat, new System.Drawing.Point((int)cont_pc.X, (int)cont_pc.Y), new MCvScalar(255, 255, 0), MarkerTypes.Cross, 10);
                    Console.WriteLine(text);
                }
                
            }
            CvInvoke.PutText(mat, conts.Size.ToString(), new System.Drawing.Point(20, 30), FontFace.HersheyTriplex, 0.5, new MCvScalar(0, 0, 255));
            return mat;
        }

        public static  Mat KeepInsidePolygonsAndFillOutside(Mat image, System.Drawing.Point[][] polygons, Bgr bgColor)
        {
            if (image == null)
                return image;
            if (image.Depth != DepthType.Cv8U || image.NumberOfChannels != 3)
                return image;

            // Если многоугольников нет — закрашиваем всё изображение
            if (polygons == null || polygons.Length == 0)
            {
                image.SetTo(new MCvScalar(bgColor.Blue, bgColor.Green, bgColor.Red));
                return image;
            }

            int width = image.Width;
            int height = image.Height;

            using (Mat mask = new Mat(height, width, DepthType.Cv8U, 1))
            {
                mask.SetTo(new MCvScalar(0));

                // Создаём обёртку для массива контуров
                using (VectorOfVectorOfPoint vvop = new VectorOfVectorOfPoint(polygons))
                {
                    // Заливаем многоугольники белым (255) на маске
                    CvInvoke.FillPoly(mask, vvop, new MCvScalar(255));
                }

                // Создаём фоновое изображение, полностью заполненное заданным цветом
                using (Mat background = new Mat(height, width, image.Depth, image.NumberOfChannels))
                {
                    background.SetTo(new MCvScalar(bgColor.Blue, bgColor.Green, bgColor.Red));

                    // Инвертируем маску – выделяем области СНАРУЖИ многоугольников
                    using (Mat maskInv = new Mat())
                    {
                        CvInvoke.BitwiseNot(mask, maskInv);

                        // Копируем фон в исходное изображение только там, где maskInv != 0
                        background.CopyTo(image, maskInv);
                    }
                }
            }
            return image; 
        }


        public int number_registr_point_current = 0;
        public int current_registration_instrument = 0;
        public int current_model_instrument = 1;
        public bool registration_done = false;
        public int model_instrument = 1;
        public bool writing_registr_pos = false;
        public NavigSys navigation_processing_get_scene(Point3d_GL[][] ps)
        {


            for (int i = 0;i< tools.Length; i++)
            {
                tools[i].get_frame(ps,true);

                if (tools[i].toolfunc == NavigTool.ToolFuncType.Registr)
                {
                    if (writing_registr_pos && ps != null)
                    {
                        tools[i].add_point_for_registr(number_registr_point_current, tools[current_registration_instrument].ps[4]);
                    }
                    
                }
            }
            update_matrix_model_to_camera1();
            update_ms_targets();
            update_ms_model();
            update_ms_robot();
            robot?.handler_move();
            return this;
        }

        public Matrix<double> matrix_model_to_camera1 = UtilMatr.eye_matr(4);
        //public Matrix<double> matrix_model_to_camera1 = UtilMatr.eye_matr(4);
        public void update_matrix_model_to_camera1()
        {
            if(registration_done)
            {
                var check_inds = current_model_instrument < tools.Length && current_model_instrument >= 0 && current_registration_instrument < tools.Length && current_registration_instrument >= 0;
                if (!check_inds) { Console.WriteLine("update_matrix_model_to_camera1 failed"); return; }
                matrix_model_to_camera1 = tools[current_model_instrument].matrix_frame * tools[current_model_instrument].matrix_tcp;
            }
        }
        public void update_ms_targets()
        {
            for (int i = 0; i < targets.Count; i++)
            {
                targets[i].M_obj_to_camera1 = matrix_model_to_camera1;
            }
        }

        public void update_ms_model()
        {
            model.M_obj_to_camera1 = matrix_model_to_camera1;
        }
        public void update_ms_robot()
        {

        }
        public int change_navig_current_model_instrument()
        {
            current_model_instrument++;
            if (tools != null)
                if (current_model_instrument >= tools.Length)
                {
                    current_model_instrument = 0;
                }
            return current_model_instrument;
        }

        public int change_navig_current_registration_instrument()
        {

            current_registration_instrument++;

            if (tools != null)
                if (current_registration_instrument >= tools.Length)
                {
                    current_registration_instrument = 0;
                }
            return current_registration_instrument;
        }

        public bool write_pos_registr_point_enable()
        {
            if(tools[current_model_instrument].ps_for_registr.Count==0)  tools[current_model_instrument].init_points_for_registr(model.registr_ps.Length);
            writing_registr_pos = !writing_registr_pos;
            return writing_registr_pos;
        }

        public int increment_number_registr_ps()
        {
            number_registr_point_current++;
            if (model.ctSliceFull != null)
                if (number_registr_point_current >= model.registr_ps.Length)
                {
                    number_registr_point_current = 0;
                }
            
            return number_registr_point_current;
        }
        public void registr_model()
        {
            var check_inds = current_model_instrument < tools.Length && current_model_instrument >= 0 && current_registration_instrument <tools.Length && current_registration_instrument >= 0;
            if (!check_inds) { Console.WriteLine("navig_registr_model check failed"); return; }
            current_model_instrument = 1;

            if (tools != null && model.generate_model_name != null)
            {

                var ps_reg = new Point3d_GL[] {
                new Point3d_GL(70,-71,-48),
                new Point3d_GL(70,-71,-26),
                new Point3d_GL(70,-19,-26),
                new Point3d_GL(70,-19,-48)};
                /*var ps_reg = new Point3d_GL[] {
                    Point3d_GL.centr_mass(navig_system.tools[current_model_instrument].ps_for_registr[0].ToArray()),
                    Point3d_GL.centr_mass(navig_system.tools[current_model_instrument].ps_for_registr[1].ToArray()),
                    Point3d_GL.centr_mass(navig_system.tools[current_model_instrument].ps_for_registr[2].ToArray()),
                    Point3d_GL.centr_mass(navig_system.tools[current_model_instrument].ps_for_registr[3].ToArray()),
                };*/
                var b1 = RobotFrame.matrix_basis_from_ps(model.registr_ps);
                var b2 = RobotFrame.matrix_basis_from_ps(ps_reg);
                var b1_inv = b1.Clone();
                CvInvoke.Invert(b1, b1_inv, DecompMethod.LU);

                //model_frame_matrix = UtilMatr.matrix_cv(new Point3d_GL(40, 40, 40), new Point3d_GL());
                var model_frame_matrix = b2 * b1_inv;
                prin.t("model_frame_matrix");
                prin.t(model_frame_matrix);
                //tools[current_model_instrument].name_3d_model = model.generate_model_name;
                tools[current_model_instrument].matrix_tcp = model_frame_matrix;
                model_instrument = current_model_instrument;
                registration_done = true;
            }
        }

        public static Mat get_aruco_info(Mat image, ref System.Drawing.PointF[][] points)
        {
            var dictionary = new Dictionary(Dictionary.PredefinedDictionaryName.Dict4X4_50);
            var detectorParams = DetectorParameters.GetDefault();
            detectorParams.CornerRefinementMethod = DetectorParameters.RefinementMethod.Contour;
            detectorParams.CornerRefinementMaxIterations = 30;
            detectorParams.CornerRefinementMinAccuracy = 0.1;
            Mat gray = new Mat();
            CvInvoke.CvtColor(image, gray, ColorConversion.Rgb2Gray);
            var corners = new VectorOfVectorOfPointF();
            var ids = new VectorOfInt();
            var rejectedPoints = new VectorOfVectorOfPointF();
            if (gray.IsEmpty) return null;
            ArucoInvoke.DetectMarkers(gray, dictionary, corners, ids, detectorParams, rejectedPoints);
            List<System.Drawing.PointF[]> cornersList = corners.ToArrayOfArray().ToList();
            List<System.Drawing.PointF[]> cornersList_2 = new List<System.Drawing.PointF[]>();
            foreach (var markerCorners in cornersList)
            {
                VectorOfPointF vec = new VectorOfPointF(markerCorners);
                CvInvoke.CornerSubPix(gray, vec, new System.Drawing.Size(3, 3), new System.Drawing.Size(-1, -1), new MCvTermCriteria(30, 0.01));
                cornersList_2.Add(vec.ToArray());
            }
            corners = new VectorOfVectorOfPointF(cornersList_2.ToArray());// cornersList.ToArray();
            if (ids.Size > 0)
            {
                // Отрисовка границ и ID маркеров на изображении
                /*ArucoInvoke.DrawDetectedMarkers(image, corners, ids, new Bgr(Color.Green).MCvScalar);
                CvInvoke.Imshow("asd", image);
                CvInvoke.WaitKey();*/
                for (int i = 0; i < ids.Size; i++)
                {
                    int id = ids[i];
                    System.Drawing.PointF[] cornersArray = corners[i].ToArray();
                    if (points.Length > id)
                    {
                        points[id] = new System.Drawing.PointF[cornersArray.Length];
                        for (int j = 0; j < cornersArray.Length; j++)
                        {
                            points[id][j] = new System.Drawing.PointF(cornersArray[j].X, cornersArray[j].Y);
                            var color = new MCvScalar(128,128,128);//bgr
                            /*if(j == 0) color = new MCvScalar(0,255,0);
                            if (j == 1) color = new MCvScalar(0, 0, 0);
                            if (j == 2) color = new MCvScalar(0, 0, 255);
                            if (j == 3) color = new MCvScalar(128, 128, 128);
                            CvInvoke.DrawMarker(image, new System.Drawing.Point((int) cornersArray[j].X, (int)cornersArray[j].Y), color, MarkerTypes.Cross, 3);*/
                        }
                    }
                    //Console.WriteLine("----------");
                }


            }
            else
            {
            }


            return image;
        }



       

        static public Matrix<double> set_prop_pos_robot(Matrix<double> start_M_marker_in_world, Matrix<double> start_M_flange_in_base, Matrix<double> prop_M_flange_in_marker)
        {
            var prop_M_marker_in_flange = UtilOpenCV.inv(prop_M_flange_in_marker);
            var start_M_base_in_flange = UtilOpenCV.inv(start_M_flange_in_base);
            var prop_M_base_in_world = start_M_marker_in_world * prop_M_flange_in_marker * start_M_base_in_flange;
            return prop_M_base_in_world;

        }

        public void handeye_calibr_navig_frames(Frame[] frames,int ind_tool)//, Matrix<double> prop_M_flange_in_marker)
        {
            Matrix<double>[] real_Ms_marker_in_world = new Matrix<double>[frames.Length];
            Matrix<double>[] prop_Ms_flange_in_base = new Matrix<double>[frames.Length];

            for (int i = 0; i < frames.Length; i++)
            {
                var p2d_1 = navigation_processing_get_points2d_3cam(ref frames[i].im, 0);
                var p2d_2 = navigation_processing_get_points2d_3cam(ref frames[i].im_sec, 1);
                var p2d_3 = navigation_processing_get_points2d_3cam(ref frames[i].im_third, 2);
                var ps3d = navigation_processing_get_points3d_3cam(p2d_1, p2d_2, p2d_3, frames[i].im, frames[i].im_sec, frames[i].im_third);
                //ps_calib.Add(ps3d);
                real_Ms_marker_in_world[i] = tools[ind_tool].matrix_frame.Clone();
                prop_Ms_flange_in_base[i] = new RobotFrame(new Pose(frames[i].name),robot.robotType).getMatrix();
            }

            handeye_calibr(real_Ms_marker_in_world, prop_Ms_flange_in_base);
        }

        public void handeye_calibr( Matrix<double>[] real_Ms_marker_in_world, Matrix<double>[] prop_Ms_flange_in_base)
        {

            var prop_Ms_base_in_flange = new Matrix<double>[real_Ms_marker_in_world.Length];
            for (int i = 0; i < real_Ms_marker_in_world.Length; i++)
            {
                prop_Ms_base_in_flange[i] = UtilOpenCV.inv(prop_Ms_flange_in_base[i]);
            }

            var real_M_flange_in_marker = StereoCamera.calibrate_stereo_rob_handeye_navig(prop_Ms_base_in_flange, real_Ms_marker_in_world, null);
            prin.t("fr_marker_in_flange_inv");
           // prin.t(new RobotFrame(real_M_flange_in_marker).ToString());
        }
        static public Matrix<double>[] gen_frames(Matrix<double> start_frame, double xyz_delt, double abc_delt,int count_one_axis = 5)
        {
            var fr_p = new RobotFrame(start_frame);

            var matrixes_gen = new List<Matrix<double>>();
            var xyz_delt_cur = -xyz_delt;
            var xyz_incr = 2 * xyz_delt / count_one_axis;

            xyz_delt_cur = -xyz_delt;
            xyz_incr = 2*xyz_delt/count_one_axis;
            for (int i = 0; i<count_one_axis;i++)
            {
                var fp_gen = fr_p.Clone();
                fp_gen.X += xyz_delt_cur;
                matrixes_gen.Add(fp_gen.getMatrix());
                xyz_delt_cur += xyz_incr;
            }

            xyz_delt_cur = -xyz_delt;
            xyz_incr = 2 * xyz_delt / count_one_axis;
            for (int i = 0; i < count_one_axis; i++)
            {
                var fp_gen = fr_p.Clone();
                fp_gen.Y += xyz_delt_cur;
                matrixes_gen.Add(fp_gen.getMatrix());
                xyz_delt_cur += xyz_incr;
            }

            xyz_delt_cur = -xyz_delt;
            xyz_incr = 2 * xyz_delt / count_one_axis;
            for (int i = 0; i < count_one_axis; i++)
            {
                var fp_gen = fr_p.Clone();
                fp_gen.Z += xyz_delt_cur;
                matrixes_gen.Add(fp_gen.getMatrix());
                xyz_delt_cur += xyz_incr;
            }

            var abc_delt_cur = -abc_delt;
            var abc_incr = 2 * abc_delt / count_one_axis;

            abc_delt_cur = -abc_delt;
            abc_incr = 2 * abc_delt / count_one_axis;
            for (int i = 0; i < count_one_axis; i++)
            {
                var fp_gen = fr_p.Clone();
                fp_gen.A += abc_delt_cur;
                matrixes_gen.Add(fp_gen.getMatrix());
                abc_delt_cur += abc_incr;
            }

            abc_delt_cur = -abc_delt;
            abc_incr = 2 * abc_delt / count_one_axis;
            for (int i = 0; i < count_one_axis; i++)
            {
                var fp_gen = fr_p.Clone();
                fp_gen.B += abc_delt_cur;
                matrixes_gen.Add(fp_gen.getMatrix());
                abc_delt_cur += abc_incr;
            }

            abc_delt_cur = -abc_delt;
            abc_incr = 2 * abc_delt / count_one_axis;
            for (int i = 0; i < count_one_axis; i++)
            {
                var fp_gen = fr_p.Clone();
                fp_gen.C += abc_delt_cur;
                matrixes_gen.Add(fp_gen.getMatrix());
                abc_delt_cur += abc_incr;
            }

            return matrixes_gen.ToArray();
        }


        

        public void init_draw_navig_systems(GraphicGL graphic, SceneType cur_scene)
        {
            var cam_model_path = @"models\nav\cam.stl";
            var cam1_model = graphic.add_buff_gl(new Model3d(cam_model_path), PrimitiveType.Triangles, "cam1");
            var cam2_model = graphic.add_buff_gl(new Model3d(cam_model_path), PrimitiveType.Triangles, "cam2");
            var cam3_model = graphic.add_buff_gl(new Model3d(cam_model_path), PrimitiveType.Triangles, "cam3");
            graphic.buffersGl.setMatrobj(cam1_model, 0, Matrix4x4f.RotatedY(90)); //navig_system.stereo.stereoCamera.cameraCVs[0].matrixCS
            graphic.buffersGl.setMatrobj(cam2_model, 0, UtilMatr.to_matrix(stereo.stereoCamera.R) * Matrix4x4f.RotatedY(90));//navig_system.stereo.stereoCamera.cameraCVs[1].matrixCS *
            graphic.buffersGl.setMatrobj(cam3_model, 0, UtilMatr.to_matrix(stereo.stereoCamera.R2) * Matrix4x4f.RotatedY(90));//navig_system.stereo.stereoCamera.cameraCVs[2].matrixCS *

            models_name.Add(cam1_model);
            models_name.Add(cam2_model);
            models_name.Add(cam3_model);
            //------------------------------------------------------------------------------------------------------------------------
            for (int i = 0; i < tools.Length; i++)  tools[i].init_draw_model_3d(graphic, cur_scene);                       
            for (int i = 0; i < targets.Count; i++)  targets[i].init_draw_model_3d(graphic, cur_scene);         
            robot.init_draw_model_3d(graphic, cur_scene);
            model.init_draw_model_3d(graphic, cur_scene);

        }
        List<string> models_name = new List<string>();
        public void draw_model_3d(GraphicGL graphic, SceneType cur_scene, bool visible)
        {
            for (int i = 0; i < models_name.Count; i++)
            {
                graphic.buffersGl.setVisibleobj(models_name[i], visible);
            }
        }



        public void draw_navig_systems(GraphicGL graphic)
        {
            for (int i = 0; i < tools.Length; i++) tools[i].draw_model_3d(graphic, sceneType, tools_vision);            
            for (int i = 0; i < targets.Count; i++) targets[i].draw_model_3d(graphic, sceneType, targets_vision);           
            robot.draw_model_3d(graphic, sceneType, robot_vision);
            model.draw_model_3d(graphic, sceneType, model_vision);
            draw_model_3d(graphic, sceneType, camera_vision);
        }
       public void calibrate_navig_tool(string tool_cal_path, int tool_ind)
        {
            //var tool_cal_path_orig = textBox_tool_calibr.Text;
            //var tool_cal_path = "tool1_2906_1a";
            var frms_stereo = FrameLoader.loadImages_stereoCV3(@"cam1\" + tool_cal_path, @"cam2\" + tool_cal_path, @"cam3\" + tool_cal_path, FrameType.Test, false);
            Console.WriteLine("load ims_done");
            var ps_calib = new List<Point3d_GL[][]>();
            var ms_calib = new List<Matrix<double>>();
            for (int i = 0; i < frms_stereo.Length; i++)
            {
                var p2d_1 = navigation_processing_get_points2d_3cam(ref frms_stereo[i].im, 0);
                var p2d_2 = navigation_processing_get_points2d_3cam(ref frms_stereo[i].im_sec, 1);
                var p2d_3 = navigation_processing_get_points2d_3cam(ref frms_stereo[i].im_third, 2);
                var ps3d = navigation_processing_get_points3d_3cam(p2d_1, p2d_2, p2d_3, frms_stereo[i].im, frms_stereo[i].im_sec, frms_stereo[i].im_third);

                ps_calib.Add(ps3d);
                tools[tool_ind].get_frame(ps3d);
                ms_calib.Add(tools[tool_ind].matrix_frame);
            }

            var cur_rob = RobotFrame.RobotType.RC5;
           // var qs = test_rob_pos1;
           // var posrob = new RobotFrame(RobotFrame.comp_forv_kinem(qs, 6, false, cur_rob), 0, 0, 0, RobotFrame.RobotType.RC5);



            // RobotFrame.comp_inv_kinem_priv_rc5_real()
            //set_conf_robot_pulse(qs, navig_system.robot.robotType, false, prop_M_base_in_world);
            //--------------------------------------------------------------
            var tcp_cal = tools[tool_ind].calibrate_tool_tcp_4p(ps_calib.ToArray());

        }
    }
}
