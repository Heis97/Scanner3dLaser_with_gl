
namespace opengl3
{
    partial class MainScanningForm
    {
        /// <summary>
        /// Обязательная переменная конструктора.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Освободить все используемые ресурсы.
        /// </summary>
        /// <param name="disposing">истинно, если управляемый ресурс должен быть удален; иначе ложно.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Код, автоматически созданный конструктором форм Windows

        /// <summary>
        /// Требуемый метод для поддержки конструктора — не изменяйте 
        /// содержимое этого метода с помощью редактора кода.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.comboImages = new System.Windows.Forms.ComboBox();
            this.tabCalibMonit = new System.Windows.Forms.TabPage();
            this.lab_pos_mouse = new System.Windows.Forms.Label();
            this.but_set_wind = new System.Windows.Forms.Button();
            this.but_ph_1 = new System.Windows.Forms.Button();
            this.but_calib_Start = new System.Windows.Forms.Button();
            this.textBoxK_8 = new System.Windows.Forms.TextBox();
            this.textBoxK_7 = new System.Windows.Forms.TextBox();
            this.textBoxK_6 = new System.Windows.Forms.TextBox();
            this.textBoxK_5 = new System.Windows.Forms.TextBox();
            this.textBoxK_4 = new System.Windows.Forms.TextBox();
            this.textBoxK_3 = new System.Windows.Forms.TextBox();
            this.textBoxK_2 = new System.Windows.Forms.TextBox();
            this.textBoxK_1 = new System.Windows.Forms.TextBox();
            this.textBoxK_0 = new System.Windows.Forms.TextBox();
            this.trackBar27 = new System.Windows.Forms.TrackBar();
            this.trackBar28 = new System.Windows.Forms.TrackBar();
            this.trackBar29 = new System.Windows.Forms.TrackBar();
            this.trackBar24 = new System.Windows.Forms.TrackBar();
            this.trackBar25 = new System.Windows.Forms.TrackBar();
            this.trackBar26 = new System.Windows.Forms.TrackBar();
            this.trackBar23 = new System.Windows.Forms.TrackBar();
            this.trackBar22 = new System.Windows.Forms.TrackBar();
            this.trackBar21 = new System.Windows.Forms.TrackBar();
            this.imBox_input_2 = new Emgu.CV.UI.ImageBox();
            this.imBox_input_1 = new Emgu.CV.UI.ImageBox();
            this.imBox_pattern = new Emgu.CV.UI.ImageBox();
            this.tabDistort = new System.Windows.Forms.TabPage();
            this.imBox_debug2 = new Emgu.CV.UI.ImageBox();
            this.imBox_debug1 = new Emgu.CV.UI.ImageBox();
            this.label_corPic = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.textBox_P2deg = new System.Windows.Forms.TextBox();
            this.textBox_P1deg = new System.Windows.Forms.TextBox();
            this.textBox_K3deg = new System.Windows.Forms.TextBox();
            this.textBox_K2deg = new System.Windows.Forms.TextBox();
            this.textBox_K1deg = new System.Windows.Forms.TextBox();
            this.textBox_P2 = new System.Windows.Forms.TextBox();
            this.textBox_P1 = new System.Windows.Forms.TextBox();
            this.textBox_K3 = new System.Windows.Forms.TextBox();
            this.textBox_K2 = new System.Windows.Forms.TextBox();
            this.textBox_K1 = new System.Windows.Forms.TextBox();
            this.but_comp_dist = new System.Windows.Forms.Button();
            this.label32 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.imageBox_cameraDist = new Emgu.CV.UI.ImageBox();
            this.tabDebug = new System.Windows.Forms.TabPage();
            this.imageBox6 = new Emgu.CV.UI.ImageBox();
            this.imageBox5 = new Emgu.CV.UI.ImageBox();
            this.imageBox3 = new Emgu.CV.UI.ImageBox();
            this.imageBox7 = new Emgu.CV.UI.ImageBox();
            this.imageBox4 = new Emgu.CV.UI.ImageBox();
            this.imageBox8 = new Emgu.CV.UI.ImageBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.histogramBox1 = new Emgu.CV.UI.HistogramBox();
            this.tabOpenGl = new System.Windows.Forms.TabPage();
            this.but_dist_same_ps = new System.Windows.Forms.Button();
            this.but_stereo_3dp = new System.Windows.Forms.Button();
            this.but_comp_basis = new System.Windows.Forms.Button();
            this.but_ps_cal_save = new System.Windows.Forms.Button();
            this.but_set_model_matr = new System.Windows.Forms.Button();
            this.ch_b_im_s = new System.Windows.Forms.CheckBox();
            this.ch_b_dist = new System.Windows.Forms.CheckBox();
            this.ch_b_sync = new System.Windows.Forms.CheckBox();
            this.but_del_obj3d = new System.Windows.Forms.Button();
            this.but_remesh_test = new System.Windows.Forms.Button();
            this.but_intersec_obj = new System.Windows.Forms.Button();
            this.prop_grid_model = new System.Windows.Forms.PropertyGrid();
            this.tree_models = new System.Windows.Forms.TreeView();
            this.but_scan_stereo_rob = new System.Windows.Forms.Button();
            this.but_rob_traj_pulse = new System.Windows.Forms.Button();
            this.but_rob_traj_kuka = new System.Windows.Forms.Button();
            this.but_rob_start_sc = new System.Windows.Forms.Button();
            this.but_rob_clear_sc = new System.Windows.Forms.Button();
            this.but_rob_manual_sc = new System.Windows.Forms.Button();
            this.but_rob_auto_sc = new System.Windows.Forms.Button();
            this.tb_rob_pos_sc = new System.Windows.Forms.TextBox();
            this.but_rob_res_sc = new System.Windows.Forms.Button();
            this.but_rob_con_sc = new System.Windows.Forms.Button();
            this.but_rob_discon_sc = new System.Windows.Forms.Button();
            this.but_rob_send_sc = new System.Windows.Forms.Button();
            this.but_reconstruc_area = new System.Windows.Forms.Button();
            this.but_keep_area = new System.Windows.Forms.Button();
            this.but_delete_area = new System.Windows.Forms.Button();
            this.but_load_sing_calib = new System.Windows.Forms.Button();
            this.but_scan_load_sing = new System.Windows.Forms.Button();
            this.but_load_stl = new System.Windows.Forms.Button();
            this.but_save_stl = new System.Windows.Forms.Button();
            this.but_cross_flat = new System.Windows.Forms.Button();
            this.but_traj_clear = new System.Windows.Forms.Button();
            this.tp_smooth_scan = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.but_gl_light = new System.Windows.Forms.Button();
            this.but_load_fr_cal = new System.Windows.Forms.Button();
            this.tb_strip_scan = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.but_im_to_3d_im1 = new System.Windows.Forms.Button();
            this.but_calibr_Bfs = new System.Windows.Forms.Button();
            this.propGrid_traj = new System.Windows.Forms.PropertyGrid();
            this.but_gl_clear = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.but_scan_path = new System.Windows.Forms.Button();
            this.textB_scan_path = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.but_stereo_cal_path = new System.Windows.Forms.Button();
            this.textB_stereo_cal_path = new System.Windows.Forms.TextBox();
            this.but_load_conf_cam2 = new System.Windows.Forms.Button();
            this.but_load_conf_cam1 = new System.Windows.Forms.Button();
            this.textB_cam2_conf = new System.Windows.Forms.TextBox();
            this.textB_cam1_conf = new System.Windows.Forms.TextBox();
            this.but_scan_load_ex = new System.Windows.Forms.Button();
            this.but_send_traj = new System.Windows.Forms.Button();
            this.but_end_cont = new System.Windows.Forms.Button();
            this.but_point_type = new System.Windows.Forms.Button();
            this.but_text_vis = new System.Windows.Forms.Button();
            this.lab_TRZ = new System.Windows.Forms.Label();
            this.debugBox = new System.Windows.Forms.RichTextBox();
            this.textBox_monitor_id = new System.Windows.Forms.TextBox();
            this.glControl1 = new OpenGL.GlControl();
            this.but_gl_cam_calib = new System.Windows.Forms.Button();
            this.imBox_3dDebug = new Emgu.CV.UI.ImageBox();
            this.but_SubpixPrec = new System.Windows.Forms.Button();
            this.label44 = new System.Windows.Forms.Label();
            this.trackBar10 = new System.Windows.Forms.TrackBar();
            this.label43 = new System.Windows.Forms.Label();
            this.trackBar9 = new System.Windows.Forms.TrackBar();
            this.label42 = new System.Windows.Forms.Label();
            this.trackBar8 = new System.Windows.Forms.TrackBar();
            this.label41 = new System.Windows.Forms.Label();
            this.trackBar7 = new System.Windows.Forms.TrackBar();
            this.label40 = new System.Windows.Forms.Label();
            this.trackBar6 = new System.Windows.Forms.TrackBar();
            this.label39 = new System.Windows.Forms.Label();
            this.trackBar5 = new System.Windows.Forms.TrackBar();
            this.label38 = new System.Windows.Forms.Label();
            this.trackBar4 = new System.Windows.Forms.TrackBar();
            this.label37 = new System.Windows.Forms.Label();
            this.trackBar3 = new System.Windows.Forms.TrackBar();
            this.label36 = new System.Windows.Forms.Label();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.label35 = new System.Windows.Forms.Label();
            this.label34 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.imBox_disparity = new Emgu.CV.UI.ImageBox();
            this.but_imGen = new System.Windows.Forms.Button();
            this.imBox_mark2 = new Emgu.CV.UI.ImageBox();
            this.imBox_mark1 = new Emgu.CV.UI.ImageBox();
            this.lab_check = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.but_swapMonit = new System.Windows.Forms.Button();
            this.lab_curCor = new System.Windows.Forms.Label();
            this.lab_kor = new System.Windows.Forms.Label();
            this.but_modeV = new System.Windows.Forms.Button();
            this.butt_plane_Ozx = new System.Windows.Forms.Button();
            this.but_plane_Oyz = new System.Windows.Forms.Button();
            this.but_plane_Oxy = new System.Windows.Forms.Button();
            this.but_ProjV = new System.Windows.Forms.Button();
            this.label27 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.trackOz = new System.Windows.Forms.TrackBar();
            this.butSaveOpenGlIm = new System.Windows.Forms.Button();
            this.trackY_light = new System.Windows.Forms.TrackBar();
            this.trackZ_light = new System.Windows.Forms.TrackBar();
            this.trackOx = new System.Windows.Forms.TrackBar();
            this.trackX_light = new System.Windows.Forms.TrackBar();
            this.trackOy = new System.Windows.Forms.TrackBar();
            this.tabMain = new System.Windows.Forms.TabPage();
            this.combo_robot_ch = new System.Windows.Forms.ComboBox();
            this.tb_port_tcp = new System.Windows.Forms.TextBox();
            this.lab_fps_cam1 = new System.Windows.Forms.Label();
            this.tB_fps_scan = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.but_scan_sing_las = new System.Windows.Forms.Button();
            this.label_timer = new System.Windows.Forms.Label();
            this.but_load_scan = new System.Windows.Forms.Button();
            this.but_scan_stereolas = new System.Windows.Forms.Button();
            this.but_scan_marl = new System.Windows.Forms.Button();
            this.but_scan_def = new System.Windows.Forms.Button();
            this.but_scan_start_laser = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.combo_improc = new System.Windows.Forms.ComboBox();
            this.label56 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.but_extr_st = new System.Windows.Forms.Button();
            this.label57 = new System.Windows.Forms.Label();
            this.tb_print_syr_d = new System.Windows.Forms.TextBox();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.tb_print_vel = new System.Windows.Forms.TextBox();
            this.tb_print_nozzle_d = new System.Windows.Forms.TextBox();
            this.but_dir_disp = new System.Windows.Forms.Button();
            this.tb_dir_disp = new System.Windows.Forms.TextBox();
            this.but_home_las = new System.Windows.Forms.Button();
            this.but_div_disp = new System.Windows.Forms.Button();
            this.tb_div_disp = new System.Windows.Forms.TextBox();
            this.but_las_enc = new System.Windows.Forms.Button();
            this.label20 = new System.Windows.Forms.Label();
            this.butset_kvp = new System.Windows.Forms.Button();
            this.textBox_set_kvp = new System.Windows.Forms.TextBox();
            this.but_set_kpp = new System.Windows.Forms.Button();
            this.textBox_set_kpp = new System.Windows.Forms.TextBox();
            this.but_laser_dest = new System.Windows.Forms.Button();
            this.textBox_laser_dest = new System.Windows.Forms.TextBox();
            this.but_setShvpVel = new System.Windows.Forms.Button();
            this.textBox_shvpVel = new System.Windows.Forms.TextBox();
            this.but_marl_receav = new System.Windows.Forms.Button();
            this.but_marl_close = new System.Windows.Forms.Button();
            this.but_marl_open = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_marl_shcpPos = new System.Windows.Forms.TextBox();
            this.but_marl_setShvpPos = new System.Windows.Forms.Button();
            this.but_setShvpPos = new System.Windows.Forms.Button();
            this.textBox_shvpPos = new System.Windows.Forms.TextBox();
            this.but_open = new System.Windows.Forms.Button();
            this.but_close = new System.Windows.Forms.Button();
            this.but_laserOn = new System.Windows.Forms.Button();
            this.but_laserOff = new System.Windows.Forms.Button();
            this.but_setPower = new System.Windows.Forms.Button();
            this.comboBox_portsArd = new System.Windows.Forms.ComboBox();
            this.but_find_ports = new System.Windows.Forms.Button();
            this.textBox_powerLaser = new System.Windows.Forms.TextBox();
            this.label55 = new System.Windows.Forms.Label();
            this.txBx_photoName = new System.Windows.Forms.TextBox();
            this.nameC2 = new System.Windows.Forms.TextBox();
            this.nameB2 = new System.Windows.Forms.TextBox();
            this.textNimVid = new System.Windows.Forms.TextBox();
            this.nameA2 = new System.Windows.Forms.TextBox();
            this.nameX = new System.Windows.Forms.TextBox();
            this.nameY = new System.Windows.Forms.TextBox();
            this.nameC_in = new System.Windows.Forms.TextBox();
            this.nameZ = new System.Windows.Forms.TextBox();
            this.nameB_in = new System.Windows.Forms.TextBox();
            this.nameA_in = new System.Windows.Forms.TextBox();
            this.nameX2 = new System.Windows.Forms.TextBox();
            this.nameZ_in = new System.Windows.Forms.TextBox();
            this.nameY2 = new System.Windows.Forms.TextBox();
            this.nameY_in = new System.Windows.Forms.TextBox();
            this.nameZ2 = new System.Windows.Forms.TextBox();
            this.nameX_in = new System.Windows.Forms.TextBox();
            this.boxN = new System.Windows.Forms.TextBox();
            this.nameC = new System.Windows.Forms.TextBox();
            this.nameB = new System.Windows.Forms.TextBox();
            this.nameA = new System.Windows.Forms.TextBox();
            this.box_scanFolder = new System.Windows.Forms.TextBox();
            this.box_photoFolder = new System.Windows.Forms.TextBox();
            this.imBox_base_2 = new Emgu.CV.UI.ImageBox();
            this.imBox_base_1 = new Emgu.CV.UI.ImageBox();
            this.but_ph = new System.Windows.Forms.Button();
            this.label45 = new System.Windows.Forms.Label();
            this.trackBar11 = new System.Windows.Forms.TrackBar();
            this.label46 = new System.Windows.Forms.Label();
            this.trackBar12 = new System.Windows.Forms.TrackBar();
            this.label47 = new System.Windows.Forms.Label();
            this.trackBar13 = new System.Windows.Forms.TrackBar();
            this.label48 = new System.Windows.Forms.Label();
            this.trackBar14 = new System.Windows.Forms.TrackBar();
            this.label49 = new System.Windows.Forms.Label();
            this.trackBar15 = new System.Windows.Forms.TrackBar();
            this.label50 = new System.Windows.Forms.Label();
            this.trackBar16 = new System.Windows.Forms.TrackBar();
            this.label51 = new System.Windows.Forms.Label();
            this.trackBar17 = new System.Windows.Forms.TrackBar();
            this.label52 = new System.Windows.Forms.Label();
            this.trackBar18 = new System.Windows.Forms.TrackBar();
            this.label53 = new System.Windows.Forms.Label();
            this.trackBar19 = new System.Windows.Forms.TrackBar();
            this.label54 = new System.Windows.Forms.Label();
            this.trackBar20 = new System.Windows.Forms.TrackBar();
            this.but_addBufRob = new System.Windows.Forms.Button();
            this.but_robMod = new System.Windows.Forms.Button();
            this.label24 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.imageBox1 = new Emgu.CV.UI.ImageBox();
            this.butStop = new System.Windows.Forms.Button();
            this.label26 = new System.Windows.Forms.Label();
            this.butStart = new System.Windows.Forms.Button();
            this.imageBox2 = new Emgu.CV.UI.ImageBox();
            this.videoCapt = new System.Windows.Forms.Button();
            this.rob_res = new System.Windows.Forms.Button();
            this.rob_con = new System.Windows.Forms.Button();
            this.but_res_pos_2 = new System.Windows.Forms.Button();
            this.disc_rob = new System.Windows.Forms.Button();
            this.but_res_pos1 = new System.Windows.Forms.Button();
            this.send_rob = new System.Windows.Forms.Button();
            this.bet_res_pos = new System.Windows.Forms.Button();
            this.but_scan_start = new System.Windows.Forms.Button();
            this.label17 = new System.Windows.Forms.Label();
            this.but_photo = new System.Windows.Forms.Button();
            this.butCalcIm = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.comboVideo = new System.Windows.Forms.ComboBox();
            this.comboNumber = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.imBox_base = new Emgu.CV.UI.ImageBox();
            this.but_resize = new System.Windows.Forms.Button();
            this.windowsTabs = new System.Windows.Forms.TabControl();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.graphicGLBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.tabCalibMonit.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar27)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar28)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar29)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar24)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar25)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar26)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar23)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar22)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar21)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_input_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_input_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_pattern)).BeginInit();
            this.tabDistort.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_debug2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_debug1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox_cameraDist)).BeginInit();
            this.tabDebug.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.histogramBox1)).BeginInit();
            this.tabOpenGl.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_3dDebug)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_disparity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_mark2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_mark1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOz)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackY_light)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackZ_light)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOx)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackX_light)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOy)).BeginInit();
            this.tabMain.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_base_2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_base_1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar13)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar14)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar15)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar16)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar17)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar18)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar19)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar20)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_base)).BeginInit();
            this.windowsTabs.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.graphicGLBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // comboImages
            // 
            this.comboImages.FormattingEnabled = true;
            this.comboImages.Location = new System.Drawing.Point(646, -2);
            this.comboImages.Name = "comboImages";
            this.comboImages.Size = new System.Drawing.Size(431, 21);
            this.comboImages.TabIndex = 24;
            this.comboImages.SelectionChangeCommitted += new System.EventHandler(this.comboImages_SelectionChangeCommitted);
            // 
            // tabCalibMonit
            // 
            this.tabCalibMonit.AutoScroll = true;
            this.tabCalibMonit.Controls.Add(this.lab_pos_mouse);
            this.tabCalibMonit.Controls.Add(this.but_set_wind);
            this.tabCalibMonit.Controls.Add(this.but_ph_1);
            this.tabCalibMonit.Controls.Add(this.but_calib_Start);
            this.tabCalibMonit.Controls.Add(this.textBoxK_8);
            this.tabCalibMonit.Controls.Add(this.textBoxK_7);
            this.tabCalibMonit.Controls.Add(this.textBoxK_6);
            this.tabCalibMonit.Controls.Add(this.textBoxK_5);
            this.tabCalibMonit.Controls.Add(this.textBoxK_4);
            this.tabCalibMonit.Controls.Add(this.textBoxK_3);
            this.tabCalibMonit.Controls.Add(this.textBoxK_2);
            this.tabCalibMonit.Controls.Add(this.textBoxK_1);
            this.tabCalibMonit.Controls.Add(this.textBoxK_0);
            this.tabCalibMonit.Controls.Add(this.trackBar27);
            this.tabCalibMonit.Controls.Add(this.trackBar28);
            this.tabCalibMonit.Controls.Add(this.trackBar29);
            this.tabCalibMonit.Controls.Add(this.trackBar24);
            this.tabCalibMonit.Controls.Add(this.trackBar25);
            this.tabCalibMonit.Controls.Add(this.trackBar26);
            this.tabCalibMonit.Controls.Add(this.trackBar23);
            this.tabCalibMonit.Controls.Add(this.trackBar22);
            this.tabCalibMonit.Controls.Add(this.trackBar21);
            this.tabCalibMonit.Controls.Add(this.imBox_input_2);
            this.tabCalibMonit.Controls.Add(this.imBox_input_1);
            this.tabCalibMonit.Controls.Add(this.imBox_pattern);
            this.tabCalibMonit.Location = new System.Drawing.Point(4, 22);
            this.tabCalibMonit.Name = "tabCalibMonit";
            this.tabCalibMonit.Padding = new System.Windows.Forms.Padding(3);
            this.tabCalibMonit.Size = new System.Drawing.Size(1882, 1003);
            this.tabCalibMonit.TabIndex = 5;
            this.tabCalibMonit.Text = "Калибровка";
            this.tabCalibMonit.UseVisualStyleBackColor = true;
            this.tabCalibMonit.Paint += new System.Windows.Forms.PaintEventHandler(this.tabCalibMonit_Paint);
            this.tabCalibMonit.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabCalibMonit_MouseDown);
            this.tabCalibMonit.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tabCalibMonit_MouseMove);
            this.tabCalibMonit.MouseUp += new System.Windows.Forms.MouseEventHandler(this.tabCalibMonit_MouseUp);
            // 
            // lab_pos_mouse
            // 
            this.lab_pos_mouse.AutoSize = true;
            this.lab_pos_mouse.Location = new System.Drawing.Point(1519, 494);
            this.lab_pos_mouse.Name = "lab_pos_mouse";
            this.lab_pos_mouse.Size = new System.Drawing.Size(81, 13);
            this.lab_pos_mouse.TabIndex = 120;
            this.lab_pos_mouse.Text = "lab_pos_mouse";
            // 
            // but_set_wind
            // 
            this.but_set_wind.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_set_wind.Location = new System.Drawing.Point(1749, 591);
            this.but_set_wind.Name = "but_set_wind";
            this.but_set_wind.Size = new System.Drawing.Size(127, 55);
            this.but_set_wind.TabIndex = 119;
            this.but_set_wind.Text = "Установить окно";
            this.but_set_wind.UseVisualStyleBackColor = true;
            this.but_set_wind.Click += new System.EventHandler(this.but_set_wind_Click);
            // 
            // but_ph_1
            // 
            this.but_ph_1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_ph_1.Location = new System.Drawing.Point(1749, 552);
            this.but_ph_1.Name = "but_ph_1";
            this.but_ph_1.Size = new System.Drawing.Size(127, 33);
            this.but_ph_1.TabIndex = 117;
            this.but_ph_1.Text = "photo";
            this.but_ph_1.UseVisualStyleBackColor = true;
            this.but_ph_1.Click += new System.EventHandler(this.butSaveIm_Click);
            // 
            // but_calib_Start
            // 
            this.but_calib_Start.Location = new System.Drawing.Point(1776, 479);
            this.but_calib_Start.Name = "but_calib_Start";
            this.but_calib_Start.Size = new System.Drawing.Size(100, 67);
            this.but_calib_Start.TabIndex = 23;
            this.but_calib_Start.Text = "Начать калибровку";
            this.but_calib_Start.UseVisualStyleBackColor = true;
            this.but_calib_Start.Click += new System.EventHandler(this.but_calib_Start_Click);
            // 
            // textBoxK_8
            // 
            this.textBoxK_8.Location = new System.Drawing.Point(1749, 439);
            this.textBoxK_8.Name = "textBoxK_8";
            this.textBoxK_8.Size = new System.Drawing.Size(100, 20);
            this.textBoxK_8.TabIndex = 22;
            this.textBoxK_8.Text = "1";
            // 
            // textBoxK_7
            // 
            this.textBoxK_7.Location = new System.Drawing.Point(1749, 388);
            this.textBoxK_7.Name = "textBoxK_7";
            this.textBoxK_7.Size = new System.Drawing.Size(100, 20);
            this.textBoxK_7.TabIndex = 20;
            this.textBoxK_7.Text = "1";
            // 
            // textBoxK_6
            // 
            this.textBoxK_6.Location = new System.Drawing.Point(1749, 337);
            this.textBoxK_6.Name = "textBoxK_6";
            this.textBoxK_6.Size = new System.Drawing.Size(100, 20);
            this.textBoxK_6.TabIndex = 18;
            this.textBoxK_6.Text = "1";
            // 
            // textBoxK_5
            // 
            this.textBoxK_5.Location = new System.Drawing.Point(1749, 286);
            this.textBoxK_5.Name = "textBoxK_5";
            this.textBoxK_5.Size = new System.Drawing.Size(100, 20);
            this.textBoxK_5.TabIndex = 16;
            this.textBoxK_5.Text = "0.01";
            // 
            // textBoxK_4
            // 
            this.textBoxK_4.Location = new System.Drawing.Point(1749, 235);
            this.textBoxK_4.Name = "textBoxK_4";
            this.textBoxK_4.Size = new System.Drawing.Size(100, 20);
            this.textBoxK_4.TabIndex = 14;
            this.textBoxK_4.Text = "1";
            // 
            // textBoxK_3
            // 
            this.textBoxK_3.Location = new System.Drawing.Point(1749, 184);
            this.textBoxK_3.Name = "textBoxK_3";
            this.textBoxK_3.Size = new System.Drawing.Size(100, 20);
            this.textBoxK_3.TabIndex = 12;
            this.textBoxK_3.Text = "1";
            // 
            // textBoxK_2
            // 
            this.textBoxK_2.Location = new System.Drawing.Point(1749, 133);
            this.textBoxK_2.Name = "textBoxK_2";
            this.textBoxK_2.Size = new System.Drawing.Size(100, 20);
            this.textBoxK_2.TabIndex = 10;
            this.textBoxK_2.Text = "0.01";
            // 
            // textBoxK_1
            // 
            this.textBoxK_1.Location = new System.Drawing.Point(1749, 82);
            this.textBoxK_1.Name = "textBoxK_1";
            this.textBoxK_1.Size = new System.Drawing.Size(100, 20);
            this.textBoxK_1.TabIndex = 8;
            this.textBoxK_1.Text = "1";
            // 
            // textBoxK_0
            // 
            this.textBoxK_0.Location = new System.Drawing.Point(1749, 31);
            this.textBoxK_0.Name = "textBoxK_0";
            this.textBoxK_0.Size = new System.Drawing.Size(100, 20);
            this.textBoxK_0.TabIndex = 6;
            this.textBoxK_0.Text = "1";
            // 
            // trackBar27
            // 
            this.trackBar27.AccessibleName = "8";
            this.trackBar27.Location = new System.Drawing.Point(1514, 427);
            this.trackBar27.Maximum = 100;
            this.trackBar27.Minimum = -100;
            this.trackBar27.Name = "trackBar27";
            this.trackBar27.Size = new System.Drawing.Size(229, 45);
            this.trackBar27.TabIndex = 21;
            this.trackBar27.Value = 100;
            this.trackBar27.Scroll += new System.EventHandler(this.tr_Persp_Scroll);
            // 
            // trackBar28
            // 
            this.trackBar28.AccessibleName = "7";
            this.trackBar28.Location = new System.Drawing.Point(1514, 376);
            this.trackBar28.Maximum = 100;
            this.trackBar28.Minimum = -100;
            this.trackBar28.Name = "trackBar28";
            this.trackBar28.Size = new System.Drawing.Size(229, 45);
            this.trackBar28.TabIndex = 19;
            this.trackBar28.Scroll += new System.EventHandler(this.tr_Persp_Scroll);
            // 
            // trackBar29
            // 
            this.trackBar29.AccessibleName = "6";
            this.trackBar29.Location = new System.Drawing.Point(1514, 325);
            this.trackBar29.Maximum = 100;
            this.trackBar29.Minimum = -100;
            this.trackBar29.Name = "trackBar29";
            this.trackBar29.Size = new System.Drawing.Size(229, 45);
            this.trackBar29.TabIndex = 17;
            this.trackBar29.Scroll += new System.EventHandler(this.tr_Persp_Scroll);
            // 
            // trackBar24
            // 
            this.trackBar24.AccessibleName = "5";
            this.trackBar24.Location = new System.Drawing.Point(1514, 274);
            this.trackBar24.Maximum = 100;
            this.trackBar24.Minimum = -100;
            this.trackBar24.Name = "trackBar24";
            this.trackBar24.Size = new System.Drawing.Size(229, 45);
            this.trackBar24.TabIndex = 15;
            this.trackBar24.Scroll += new System.EventHandler(this.tr_Persp_Scroll);
            // 
            // trackBar25
            // 
            this.trackBar25.AccessibleName = "4";
            this.trackBar25.Location = new System.Drawing.Point(1514, 223);
            this.trackBar25.Maximum = 100;
            this.trackBar25.Minimum = -100;
            this.trackBar25.Name = "trackBar25";
            this.trackBar25.Size = new System.Drawing.Size(229, 45);
            this.trackBar25.TabIndex = 13;
            this.trackBar25.Value = 100;
            this.trackBar25.Scroll += new System.EventHandler(this.tr_Persp_Scroll);
            // 
            // trackBar26
            // 
            this.trackBar26.AccessibleName = "3";
            this.trackBar26.Location = new System.Drawing.Point(1514, 172);
            this.trackBar26.Maximum = 100;
            this.trackBar26.Minimum = -100;
            this.trackBar26.Name = "trackBar26";
            this.trackBar26.Size = new System.Drawing.Size(229, 45);
            this.trackBar26.TabIndex = 11;
            this.trackBar26.Scroll += new System.EventHandler(this.tr_Persp_Scroll);
            // 
            // trackBar23
            // 
            this.trackBar23.AccessibleName = "2";
            this.trackBar23.Location = new System.Drawing.Point(1514, 121);
            this.trackBar23.Maximum = 100;
            this.trackBar23.Minimum = -100;
            this.trackBar23.Name = "trackBar23";
            this.trackBar23.Size = new System.Drawing.Size(229, 45);
            this.trackBar23.TabIndex = 9;
            this.trackBar23.Scroll += new System.EventHandler(this.tr_Persp_Scroll);
            // 
            // trackBar22
            // 
            this.trackBar22.AccessibleName = "1";
            this.trackBar22.Location = new System.Drawing.Point(1514, 70);
            this.trackBar22.Maximum = 100;
            this.trackBar22.Minimum = -100;
            this.trackBar22.Name = "trackBar22";
            this.trackBar22.Size = new System.Drawing.Size(229, 45);
            this.trackBar22.TabIndex = 7;
            this.trackBar22.Scroll += new System.EventHandler(this.tr_Persp_Scroll);
            // 
            // trackBar21
            // 
            this.trackBar21.AccessibleName = "0";
            this.trackBar21.Location = new System.Drawing.Point(1514, 19);
            this.trackBar21.Maximum = 100;
            this.trackBar21.Minimum = -100;
            this.trackBar21.Name = "trackBar21";
            this.trackBar21.Size = new System.Drawing.Size(229, 45);
            this.trackBar21.TabIndex = 5;
            this.trackBar21.Value = 100;
            this.trackBar21.Scroll += new System.EventHandler(this.tr_Persp_Scroll);
            // 
            // imBox_input_2
            // 
            this.imBox_input_2.Location = new System.Drawing.Point(1522, 735);
            this.imBox_input_2.Name = "imBox_input_2";
            this.imBox_input_2.Size = new System.Drawing.Size(354, 265);
            this.imBox_input_2.TabIndex = 4;
            this.imBox_input_2.TabStop = false;
            // 
            // imBox_input_1
            // 
            this.imBox_input_1.Location = new System.Drawing.Point(1154, 735);
            this.imBox_input_1.Name = "imBox_input_1";
            this.imBox_input_1.Size = new System.Drawing.Size(362, 262);
            this.imBox_input_1.TabIndex = 3;
            this.imBox_input_1.TabStop = false;
            // 
            // imBox_pattern
            // 
            this.imBox_pattern.Location = new System.Drawing.Point(543, 413);
            this.imBox_pattern.Name = "imBox_pattern";
            this.imBox_pattern.Size = new System.Drawing.Size(537, 271);
            this.imBox_pattern.TabIndex = 2;
            this.imBox_pattern.TabStop = false;
            // 
            // tabDistort
            // 
            this.tabDistort.Controls.Add(this.imBox_debug2);
            this.tabDistort.Controls.Add(this.imBox_debug1);
            this.tabDistort.Controls.Add(this.label_corPic);
            this.tabDistort.Controls.Add(this.button1);
            this.tabDistort.Controls.Add(this.textBox_P2deg);
            this.tabDistort.Controls.Add(this.textBox_P1deg);
            this.tabDistort.Controls.Add(this.textBox_K3deg);
            this.tabDistort.Controls.Add(this.textBox_K2deg);
            this.tabDistort.Controls.Add(this.textBox_K1deg);
            this.tabDistort.Controls.Add(this.textBox_P2);
            this.tabDistort.Controls.Add(this.textBox_P1);
            this.tabDistort.Controls.Add(this.textBox_K3);
            this.tabDistort.Controls.Add(this.textBox_K2);
            this.tabDistort.Controls.Add(this.textBox_K1);
            this.tabDistort.Controls.Add(this.but_comp_dist);
            this.tabDistort.Controls.Add(this.label32);
            this.tabDistort.Controls.Add(this.label31);
            this.tabDistort.Controls.Add(this.label30);
            this.tabDistort.Controls.Add(this.label29);
            this.tabDistort.Controls.Add(this.label28);
            this.tabDistort.Controls.Add(this.imageBox_cameraDist);
            this.tabDistort.Location = new System.Drawing.Point(4, 22);
            this.tabDistort.Name = "tabDistort";
            this.tabDistort.Padding = new System.Windows.Forms.Padding(3);
            this.tabDistort.Size = new System.Drawing.Size(1882, 1003);
            this.tabDistort.TabIndex = 4;
            this.tabDistort.Text = "Камера";
            this.tabDistort.UseVisualStyleBackColor = true;
            // 
            // imBox_debug2
            // 
            this.imBox_debug2.Location = new System.Drawing.Point(655, 492);
            this.imBox_debug2.Name = "imBox_debug2";
            this.imBox_debug2.Size = new System.Drawing.Size(640, 480);
            this.imBox_debug2.TabIndex = 22;
            this.imBox_debug2.TabStop = false;
            // 
            // imBox_debug1
            // 
            this.imBox_debug1.Location = new System.Drawing.Point(6, 492);
            this.imBox_debug1.Name = "imBox_debug1";
            this.imBox_debug1.Size = new System.Drawing.Size(640, 480);
            this.imBox_debug1.TabIndex = 21;
            this.imBox_debug1.TabStop = false;
            // 
            // label_corPic
            // 
            this.label_corPic.AutoSize = true;
            this.label_corPic.Location = new System.Drawing.Point(1294, 162);
            this.label_corPic.Name = "label_corPic";
            this.label_corPic.Size = new System.Drawing.Size(22, 13);
            this.label_corPic.TabIndex = 19;
            this.label_corPic.Text = "cor";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(1297, 215);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(123, 31);
            this.button1.TabIndex = 20;
            this.button1.Text = "Comp undist pic";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // textBox_P2deg
            // 
            this.textBox_P2deg.Location = new System.Drawing.Point(1447, 122);
            this.textBox_P2deg.Name = "textBox_P2deg";
            this.textBox_P2deg.Size = new System.Drawing.Size(100, 20);
            this.textBox_P2deg.TabIndex = 18;
            this.textBox_P2deg.Text = "0";
            // 
            // textBox_P1deg
            // 
            this.textBox_P1deg.Location = new System.Drawing.Point(1447, 96);
            this.textBox_P1deg.Name = "textBox_P1deg";
            this.textBox_P1deg.Size = new System.Drawing.Size(100, 20);
            this.textBox_P1deg.TabIndex = 17;
            this.textBox_P1deg.Text = "0";
            // 
            // textBox_K3deg
            // 
            this.textBox_K3deg.Location = new System.Drawing.Point(1447, 70);
            this.textBox_K3deg.Name = "textBox_K3deg";
            this.textBox_K3deg.Size = new System.Drawing.Size(100, 20);
            this.textBox_K3deg.TabIndex = 16;
            this.textBox_K3deg.Text = "0";
            // 
            // textBox_K2deg
            // 
            this.textBox_K2deg.Location = new System.Drawing.Point(1447, 44);
            this.textBox_K2deg.Name = "textBox_K2deg";
            this.textBox_K2deg.Size = new System.Drawing.Size(100, 20);
            this.textBox_K2deg.TabIndex = 15;
            this.textBox_K2deg.Text = "0";
            // 
            // textBox_K1deg
            // 
            this.textBox_K1deg.Location = new System.Drawing.Point(1447, 18);
            this.textBox_K1deg.Name = "textBox_K1deg";
            this.textBox_K1deg.Size = new System.Drawing.Size(100, 20);
            this.textBox_K1deg.TabIndex = 14;
            this.textBox_K1deg.Text = "0";
            // 
            // textBox_P2
            // 
            this.textBox_P2.Location = new System.Drawing.Point(1320, 122);
            this.textBox_P2.Name = "textBox_P2";
            this.textBox_P2.Size = new System.Drawing.Size(100, 20);
            this.textBox_P2.TabIndex = 12;
            this.textBox_P2.Text = "0";
            // 
            // textBox_P1
            // 
            this.textBox_P1.Location = new System.Drawing.Point(1320, 96);
            this.textBox_P1.Name = "textBox_P1";
            this.textBox_P1.Size = new System.Drawing.Size(100, 20);
            this.textBox_P1.TabIndex = 10;
            this.textBox_P1.Text = "0";
            // 
            // textBox_K3
            // 
            this.textBox_K3.Location = new System.Drawing.Point(1320, 70);
            this.textBox_K3.Name = "textBox_K3";
            this.textBox_K3.Size = new System.Drawing.Size(100, 20);
            this.textBox_K3.TabIndex = 8;
            this.textBox_K3.Text = "0";
            // 
            // textBox_K2
            // 
            this.textBox_K2.Location = new System.Drawing.Point(1320, 44);
            this.textBox_K2.Name = "textBox_K2";
            this.textBox_K2.Size = new System.Drawing.Size(100, 20);
            this.textBox_K2.TabIndex = 6;
            this.textBox_K2.Text = "0";
            // 
            // textBox_K1
            // 
            this.textBox_K1.Location = new System.Drawing.Point(1320, 18);
            this.textBox_K1.Name = "textBox_K1";
            this.textBox_K1.Size = new System.Drawing.Size(100, 20);
            this.textBox_K1.TabIndex = 4;
            this.textBox_K1.Text = "-1";
            // 
            // but_comp_dist
            // 
            this.but_comp_dist.Location = new System.Drawing.Point(1297, 178);
            this.but_comp_dist.Name = "but_comp_dist";
            this.but_comp_dist.Size = new System.Drawing.Size(123, 31);
            this.but_comp_dist.TabIndex = 13;
            this.but_comp_dist.Text = "Comp dist pic";
            this.but_comp_dist.UseVisualStyleBackColor = true;
            this.but_comp_dist.Click += new System.EventHandler(this.but_comDist_Click);
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(1294, 125);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(20, 13);
            this.label32.TabIndex = 11;
            this.label32.Text = "P2";
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(1294, 99);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(20, 13);
            this.label31.TabIndex = 9;
            this.label31.Text = "P1";
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(1294, 73);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(20, 13);
            this.label30.TabIndex = 7;
            this.label30.Text = "K3";
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(1294, 47);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(20, 13);
            this.label29.TabIndex = 5;
            this.label29.Text = "K2";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(1294, 21);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(20, 13);
            this.label28.TabIndex = 3;
            this.label28.Text = "K1";
            // 
            // imageBox_cameraDist
            // 
            this.imageBox_cameraDist.Location = new System.Drawing.Point(6, 6);
            this.imageBox_cameraDist.Name = "imageBox_cameraDist";
            this.imageBox_cameraDist.Size = new System.Drawing.Size(1269, 480);
            this.imageBox_cameraDist.TabIndex = 2;
            this.imageBox_cameraDist.TabStop = false;
            this.imageBox_cameraDist.MouseMove += new System.Windows.Forms.MouseEventHandler(this.imageBox_cameraDist_MouseMove);
            // 
            // tabDebug
            // 
            this.tabDebug.Controls.Add(this.imageBox6);
            this.tabDebug.Controls.Add(this.imageBox5);
            this.tabDebug.Controls.Add(this.imageBox3);
            this.tabDebug.Controls.Add(this.imageBox7);
            this.tabDebug.Controls.Add(this.imageBox4);
            this.tabDebug.Controls.Add(this.imageBox8);
            this.tabDebug.Controls.Add(this.pictureBox1);
            this.tabDebug.Controls.Add(this.histogramBox1);
            this.tabDebug.Location = new System.Drawing.Point(4, 22);
            this.tabDebug.Name = "tabDebug";
            this.tabDebug.Padding = new System.Windows.Forms.Padding(3);
            this.tabDebug.Size = new System.Drawing.Size(1882, 1003);
            this.tabDebug.TabIndex = 3;
            this.tabDebug.Text = "Отладка";
            this.tabDebug.UseVisualStyleBackColor = true;
            // 
            // imageBox6
            // 
            this.imageBox6.Location = new System.Drawing.Point(6, 492);
            this.imageBox6.Name = "imageBox6";
            this.imageBox6.Size = new System.Drawing.Size(640, 480);
            this.imageBox6.TabIndex = 23;
            this.imageBox6.TabStop = false;
            // 
            // imageBox5
            // 
            this.imageBox5.Location = new System.Drawing.Point(1236, 6);
            this.imageBox5.Name = "imageBox5";
            this.imageBox5.Size = new System.Drawing.Size(640, 480);
            this.imageBox5.TabIndex = 22;
            this.imageBox5.TabStop = false;
            // 
            // imageBox3
            // 
            this.imageBox3.Location = new System.Drawing.Point(6, 6);
            this.imageBox3.Name = "imageBox3";
            this.imageBox3.Size = new System.Drawing.Size(640, 480);
            this.imageBox3.TabIndex = 19;
            this.imageBox3.TabStop = false;
            // 
            // imageBox7
            // 
            this.imageBox7.Location = new System.Drawing.Point(652, 492);
            this.imageBox7.Name = "imageBox7";
            this.imageBox7.Size = new System.Drawing.Size(640, 480);
            this.imageBox7.TabIndex = 46;
            this.imageBox7.TabStop = false;
            // 
            // imageBox4
            // 
            this.imageBox4.Location = new System.Drawing.Point(652, 6);
            this.imageBox4.Name = "imageBox4";
            this.imageBox4.Size = new System.Drawing.Size(640, 480);
            this.imageBox4.TabIndex = 21;
            this.imageBox4.TabStop = false;
            // 
            // imageBox8
            // 
            this.imageBox8.Location = new System.Drawing.Point(1236, 492);
            this.imageBox8.Name = "imageBox8";
            this.imageBox8.Size = new System.Drawing.Size(640, 480);
            this.imageBox8.TabIndex = 23;
            this.imageBox8.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(1631, 456);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(295, 208);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            // 
            // histogramBox1
            // 
            this.histogramBox1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.histogramBox1.Location = new System.Drawing.Point(1618, 725);
            this.histogramBox1.Name = "histogramBox1";
            this.histogramBox1.Size = new System.Drawing.Size(268, 180);
            this.histogramBox1.TabIndex = 2;
            this.histogramBox1.TabStop = false;
            // 
            // tabOpenGl
            // 
            this.tabOpenGl.Controls.Add(this.but_dist_same_ps);
            this.tabOpenGl.Controls.Add(this.but_stereo_3dp);
            this.tabOpenGl.Controls.Add(this.but_comp_basis);
            this.tabOpenGl.Controls.Add(this.but_ps_cal_save);
            this.tabOpenGl.Controls.Add(this.but_set_model_matr);
            this.tabOpenGl.Controls.Add(this.ch_b_im_s);
            this.tabOpenGl.Controls.Add(this.ch_b_dist);
            this.tabOpenGl.Controls.Add(this.ch_b_sync);
            this.tabOpenGl.Controls.Add(this.but_del_obj3d);
            this.tabOpenGl.Controls.Add(this.but_remesh_test);
            this.tabOpenGl.Controls.Add(this.but_intersec_obj);
            this.tabOpenGl.Controls.Add(this.prop_grid_model);
            this.tabOpenGl.Controls.Add(this.tree_models);
            this.tabOpenGl.Controls.Add(this.but_scan_stereo_rob);
            this.tabOpenGl.Controls.Add(this.but_rob_traj_pulse);
            this.tabOpenGl.Controls.Add(this.but_rob_traj_kuka);
            this.tabOpenGl.Controls.Add(this.but_rob_start_sc);
            this.tabOpenGl.Controls.Add(this.but_rob_clear_sc);
            this.tabOpenGl.Controls.Add(this.but_rob_manual_sc);
            this.tabOpenGl.Controls.Add(this.but_rob_auto_sc);
            this.tabOpenGl.Controls.Add(this.tb_rob_pos_sc);
            this.tabOpenGl.Controls.Add(this.but_rob_res_sc);
            this.tabOpenGl.Controls.Add(this.but_rob_con_sc);
            this.tabOpenGl.Controls.Add(this.but_rob_discon_sc);
            this.tabOpenGl.Controls.Add(this.but_rob_send_sc);
            this.tabOpenGl.Controls.Add(this.but_reconstruc_area);
            this.tabOpenGl.Controls.Add(this.but_keep_area);
            this.tabOpenGl.Controls.Add(this.but_delete_area);
            this.tabOpenGl.Controls.Add(this.but_load_sing_calib);
            this.tabOpenGl.Controls.Add(this.but_scan_load_sing);
            this.tabOpenGl.Controls.Add(this.but_load_stl);
            this.tabOpenGl.Controls.Add(this.but_save_stl);
            this.tabOpenGl.Controls.Add(this.but_cross_flat);
            this.tabOpenGl.Controls.Add(this.but_traj_clear);
            this.tabOpenGl.Controls.Add(this.tp_smooth_scan);
            this.tabOpenGl.Controls.Add(this.label19);
            this.tabOpenGl.Controls.Add(this.but_gl_light);
            this.tabOpenGl.Controls.Add(this.but_load_fr_cal);
            this.tabOpenGl.Controls.Add(this.tb_strip_scan);
            this.tabOpenGl.Controls.Add(this.label18);
            this.tabOpenGl.Controls.Add(this.but_im_to_3d_im1);
            this.tabOpenGl.Controls.Add(this.but_calibr_Bfs);
            this.tabOpenGl.Controls.Add(this.propGrid_traj);
            this.tabOpenGl.Controls.Add(this.but_gl_clear);
            this.tabOpenGl.Controls.Add(this.label13);
            this.tabOpenGl.Controls.Add(this.but_scan_path);
            this.tabOpenGl.Controls.Add(this.textB_scan_path);
            this.tabOpenGl.Controls.Add(this.label12);
            this.tabOpenGl.Controls.Add(this.label11);
            this.tabOpenGl.Controls.Add(this.label3);
            this.tabOpenGl.Controls.Add(this.but_stereo_cal_path);
            this.tabOpenGl.Controls.Add(this.textB_stereo_cal_path);
            this.tabOpenGl.Controls.Add(this.but_load_conf_cam2);
            this.tabOpenGl.Controls.Add(this.but_load_conf_cam1);
            this.tabOpenGl.Controls.Add(this.textB_cam2_conf);
            this.tabOpenGl.Controls.Add(this.textB_cam1_conf);
            this.tabOpenGl.Controls.Add(this.but_scan_load_ex);
            this.tabOpenGl.Controls.Add(this.but_send_traj);
            this.tabOpenGl.Controls.Add(this.but_end_cont);
            this.tabOpenGl.Controls.Add(this.but_point_type);
            this.tabOpenGl.Controls.Add(this.but_text_vis);
            this.tabOpenGl.Controls.Add(this.lab_TRZ);
            this.tabOpenGl.Controls.Add(this.debugBox);
            this.tabOpenGl.Controls.Add(this.textBox_monitor_id);
            this.tabOpenGl.Controls.Add(this.glControl1);
            this.tabOpenGl.Controls.Add(this.but_gl_cam_calib);
            this.tabOpenGl.Controls.Add(this.imBox_3dDebug);
            this.tabOpenGl.Controls.Add(this.but_SubpixPrec);
            this.tabOpenGl.Controls.Add(this.label44);
            this.tabOpenGl.Controls.Add(this.trackBar10);
            this.tabOpenGl.Controls.Add(this.label43);
            this.tabOpenGl.Controls.Add(this.trackBar9);
            this.tabOpenGl.Controls.Add(this.label42);
            this.tabOpenGl.Controls.Add(this.trackBar8);
            this.tabOpenGl.Controls.Add(this.label41);
            this.tabOpenGl.Controls.Add(this.trackBar7);
            this.tabOpenGl.Controls.Add(this.label40);
            this.tabOpenGl.Controls.Add(this.trackBar6);
            this.tabOpenGl.Controls.Add(this.label39);
            this.tabOpenGl.Controls.Add(this.trackBar5);
            this.tabOpenGl.Controls.Add(this.label38);
            this.tabOpenGl.Controls.Add(this.trackBar4);
            this.tabOpenGl.Controls.Add(this.label37);
            this.tabOpenGl.Controls.Add(this.trackBar3);
            this.tabOpenGl.Controls.Add(this.label36);
            this.tabOpenGl.Controls.Add(this.trackBar2);
            this.tabOpenGl.Controls.Add(this.label35);
            this.tabOpenGl.Controls.Add(this.label34);
            this.tabOpenGl.Controls.Add(this.trackBar1);
            this.tabOpenGl.Controls.Add(this.imBox_disparity);
            this.tabOpenGl.Controls.Add(this.but_imGen);
            this.tabOpenGl.Controls.Add(this.imBox_mark2);
            this.tabOpenGl.Controls.Add(this.imBox_mark1);
            this.tabOpenGl.Controls.Add(this.lab_check);
            this.tabOpenGl.Controls.Add(this.label33);
            this.tabOpenGl.Controls.Add(this.but_swapMonit);
            this.tabOpenGl.Controls.Add(this.lab_curCor);
            this.tabOpenGl.Controls.Add(this.lab_kor);
            this.tabOpenGl.Controls.Add(this.but_modeV);
            this.tabOpenGl.Controls.Add(this.butt_plane_Ozx);
            this.tabOpenGl.Controls.Add(this.but_plane_Oyz);
            this.tabOpenGl.Controls.Add(this.but_plane_Oxy);
            this.tabOpenGl.Controls.Add(this.but_ProjV);
            this.tabOpenGl.Controls.Add(this.label27);
            this.tabOpenGl.Controls.Add(this.label4);
            this.tabOpenGl.Controls.Add(this.trackOz);
            this.tabOpenGl.Controls.Add(this.butSaveOpenGlIm);
            this.tabOpenGl.Controls.Add(this.trackY_light);
            this.tabOpenGl.Controls.Add(this.trackZ_light);
            this.tabOpenGl.Controls.Add(this.trackOx);
            this.tabOpenGl.Controls.Add(this.trackX_light);
            this.tabOpenGl.Controls.Add(this.trackOy);
            this.tabOpenGl.Location = new System.Drawing.Point(4, 22);
            this.tabOpenGl.Name = "tabOpenGl";
            this.tabOpenGl.Padding = new System.Windows.Forms.Padding(3);
            this.tabOpenGl.Size = new System.Drawing.Size(1882, 1003);
            this.tabOpenGl.TabIndex = 2;
            this.tabOpenGl.Text = "3Д";
            this.tabOpenGl.UseVisualStyleBackColor = true;
            // 
            // but_dist_same_ps
            // 
            this.but_dist_same_ps.Location = new System.Drawing.Point(996, 575);
            this.but_dist_same_ps.Name = "but_dist_same_ps";
            this.but_dist_same_ps.Size = new System.Drawing.Size(99, 42);
            this.but_dist_same_ps.TabIndex = 161;
            this.but_dist_same_ps.Text = "Расстояние между точками";
            this.but_dist_same_ps.UseVisualStyleBackColor = true;
            this.but_dist_same_ps.Click += new System.EventHandler(this.but_dist_same_ps_Click);
            // 
            // but_stereo_3dp
            // 
            this.but_stereo_3dp.Location = new System.Drawing.Point(1101, 575);
            this.but_stereo_3dp.Name = "but_stereo_3dp";
            this.but_stereo_3dp.Size = new System.Drawing.Size(111, 42);
            this.but_stereo_3dp.TabIndex = 160;
            this.but_stereo_3dp.Text = "Посчитать 3д точки";
            this.but_stereo_3dp.UseVisualStyleBackColor = true;
            this.but_stereo_3dp.Click += new System.EventHandler(this.but_stereo_3dp_Click);
            // 
            // but_comp_basis
            // 
            this.but_comp_basis.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_comp_basis.Location = new System.Drawing.Point(1076, 468);
            this.but_comp_basis.Name = "but_comp_basis";
            this.but_comp_basis.Size = new System.Drawing.Size(136, 48);
            this.but_comp_basis.TabIndex = 159;
            this.but_comp_basis.Text = "Посчитать матрицу";
            this.but_comp_basis.UseVisualStyleBackColor = true;
            this.but_comp_basis.Click += new System.EventHandler(this.but_comp_basis_Click);
            // 
            // but_ps_cal_save
            // 
            this.but_ps_cal_save.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_ps_cal_save.Location = new System.Drawing.Point(1076, 522);
            this.but_ps_cal_save.Name = "but_ps_cal_save";
            this.but_ps_cal_save.Size = new System.Drawing.Size(136, 48);
            this.but_ps_cal_save.TabIndex = 158;
            this.but_ps_cal_save.Text = "Проецировать точки";
            this.but_ps_cal_save.UseVisualStyleBackColor = true;
            this.but_ps_cal_save.Click += new System.EventHandler(this.but_ps_cal_save_Click);
            // 
            // but_set_model_matr
            // 
            this.but_set_model_matr.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_set_model_matr.Location = new System.Drawing.Point(1076, 417);
            this.but_set_model_matr.Name = "but_set_model_matr";
            this.but_set_model_matr.Size = new System.Drawing.Size(136, 48);
            this.but_set_model_matr.TabIndex = 157;
            this.but_set_model_matr.Text = "Установить матрицу";
            this.but_set_model_matr.UseVisualStyleBackColor = true;
            this.but_set_model_matr.Click += new System.EventHandler(this.but_set_model_matr_Click);
            // 
            // ch_b_im_s
            // 
            this.ch_b_im_s.AutoSize = true;
            this.ch_b_im_s.Location = new System.Drawing.Point(1244, 850);
            this.ch_b_im_s.Name = "ch_b_im_s";
            this.ch_b_im_s.Size = new System.Drawing.Size(150, 17);
            this.ch_b_im_s.TabIndex = 156;
            this.ch_b_im_s.Text = "Сохранять изображения";
            this.ch_b_im_s.UseVisualStyleBackColor = true;
            // 
            // ch_b_dist
            // 
            this.ch_b_dist.AutoSize = true;
            this.ch_b_dist.Checked = true;
            this.ch_b_dist.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ch_b_dist.Location = new System.Drawing.Point(1244, 832);
            this.ch_b_dist.Name = "ch_b_dist";
            this.ch_b_dist.Size = new System.Drawing.Size(82, 17);
            this.ch_b_dist.TabIndex = 155;
            this.ch_b_dist.Text = "Дисторсия";
            this.ch_b_dist.UseVisualStyleBackColor = true;
            // 
            // ch_b_sync
            // 
            this.ch_b_sync.AutoSize = true;
            this.ch_b_sync.Checked = true;
            this.ch_b_sync.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ch_b_sync.Location = new System.Drawing.Point(1244, 814);
            this.ch_b_sync.Name = "ch_b_sync";
            this.ch_b_sync.Size = new System.Drawing.Size(104, 17);
            this.ch_b_sync.TabIndex = 154;
            this.ch_b_sync.Text = "Синхронизация";
            this.ch_b_sync.UseVisualStyleBackColor = true;
            // 
            // but_del_obj3d
            // 
            this.but_del_obj3d.Location = new System.Drawing.Point(776, 922);
            this.but_del_obj3d.Name = "but_del_obj3d";
            this.but_del_obj3d.Size = new System.Drawing.Size(132, 24);
            this.but_del_obj3d.TabIndex = 153;
            this.but_del_obj3d.Text = "Удалить объект";
            this.but_del_obj3d.UseVisualStyleBackColor = true;
            this.but_del_obj3d.Click += new System.EventHandler(this.but_del_obj3d_Click);
            // 
            // but_remesh_test
            // 
            this.but_remesh_test.Location = new System.Drawing.Point(662, 820);
            this.but_remesh_test.Name = "but_remesh_test";
            this.but_remesh_test.Size = new System.Drawing.Size(96, 34);
            this.but_remesh_test.TabIndex = 152;
            this.but_remesh_test.Text = "Сгладить";
            this.but_remesh_test.UseVisualStyleBackColor = true;
            this.but_remesh_test.Click += new System.EventHandler(this.but_remesh_test_Click);
            // 
            // but_intersec_obj
            // 
            this.but_intersec_obj.Location = new System.Drawing.Point(674, 933);
            this.but_intersec_obj.Name = "but_intersec_obj";
            this.but_intersec_obj.Size = new System.Drawing.Size(96, 34);
            this.but_intersec_obj.TabIndex = 151;
            this.but_intersec_obj.Text = "Линия пересеч. объектов";
            this.but_intersec_obj.UseVisualStyleBackColor = true;
            this.but_intersec_obj.Click += new System.EventHandler(this.but_intersec_obj_Click);
            // 
            // prop_grid_model
            // 
            this.prop_grid_model.Location = new System.Drawing.Point(996, 6);
            this.prop_grid_model.Name = "prop_grid_model";
            this.prop_grid_model.Size = new System.Drawing.Size(183, 400);
            this.prop_grid_model.TabIndex = 150;
            // 
            // tree_models
            // 
            this.tree_models.BackColor = System.Drawing.SystemColors.Window;
            this.tree_models.Location = new System.Drawing.Point(813, 6);
            this.tree_models.Name = "tree_models";
            this.tree_models.Size = new System.Drawing.Size(177, 271);
            this.tree_models.TabIndex = 149;
            this.tree_models.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.tree_models_AfterCheck);
            this.tree_models.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tree_models_AfterSelect);
            this.tree_models.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tree_models_MouseDown);
            // 
            // but_scan_stereo_rob
            // 
            this.but_scan_stereo_rob.Location = new System.Drawing.Point(1720, 897);
            this.but_scan_stereo_rob.Name = "but_scan_stereo_rob";
            this.but_scan_stereo_rob.Size = new System.Drawing.Size(111, 42);
            this.but_scan_stereo_rob.TabIndex = 148;
            this.but_scan_stereo_rob.Text = "Загрузить скан стерео робот";
            this.but_scan_stereo_rob.UseVisualStyleBackColor = true;
            this.but_scan_stereo_rob.Click += new System.EventHandler(this.but_scan_stereo_rob_Click);
            // 
            // but_rob_traj_pulse
            // 
            this.but_rob_traj_pulse.Location = new System.Drawing.Point(813, 632);
            this.but_rob_traj_pulse.Name = "but_rob_traj_pulse";
            this.but_rob_traj_pulse.Size = new System.Drawing.Size(106, 34);
            this.but_rob_traj_pulse.TabIndex = 147;
            this.but_rob_traj_pulse.Text = "Траектория Pulse";
            this.but_rob_traj_pulse.UseVisualStyleBackColor = true;
            this.but_rob_traj_pulse.Click += new System.EventHandler(this.but_rob_traj_pulse_Click);
            // 
            // but_rob_traj_kuka
            // 
            this.but_rob_traj_kuka.Location = new System.Drawing.Point(813, 668);
            this.but_rob_traj_kuka.Name = "but_rob_traj_kuka";
            this.but_rob_traj_kuka.Size = new System.Drawing.Size(106, 34);
            this.but_rob_traj_kuka.TabIndex = 146;
            this.but_rob_traj_kuka.Text = "Траектория Kuka";
            this.but_rob_traj_kuka.UseVisualStyleBackColor = true;
            this.but_rob_traj_kuka.Click += new System.EventHandler(this.but_rob_traj_kuka_Click);
            // 
            // but_rob_start_sc
            // 
            this.but_rob_start_sc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_rob_start_sc.Location = new System.Drawing.Point(1080, 757);
            this.but_rob_start_sc.Name = "but_rob_start_sc";
            this.but_rob_start_sc.Size = new System.Drawing.Size(106, 33);
            this.but_rob_start_sc.TabIndex = 145;
            this.but_rob_start_sc.Text = "Start";
            this.but_rob_start_sc.UseVisualStyleBackColor = true;
            this.but_rob_start_sc.Click += new System.EventHandler(this.but_rob_start_sc_Click);
            // 
            // but_rob_clear_sc
            // 
            this.but_rob_clear_sc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_rob_clear_sc.Location = new System.Drawing.Point(1080, 713);
            this.but_rob_clear_sc.Name = "but_rob_clear_sc";
            this.but_rob_clear_sc.Size = new System.Drawing.Size(106, 33);
            this.but_rob_clear_sc.TabIndex = 144;
            this.but_rob_clear_sc.Text = "Clear buf";
            this.but_rob_clear_sc.UseVisualStyleBackColor = true;
            this.but_rob_clear_sc.Click += new System.EventHandler(this.but_rob_clear_sc_Click);
            // 
            // but_rob_manual_sc
            // 
            this.but_rob_manual_sc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_rob_manual_sc.Location = new System.Drawing.Point(1080, 669);
            this.but_rob_manual_sc.Name = "but_rob_manual_sc";
            this.but_rob_manual_sc.Size = new System.Drawing.Size(106, 33);
            this.but_rob_manual_sc.TabIndex = 143;
            this.but_rob_manual_sc.Text = "Manual";
            this.but_rob_manual_sc.UseVisualStyleBackColor = true;
            this.but_rob_manual_sc.Click += new System.EventHandler(this.but_rob_manual_sc_Click);
            // 
            // but_rob_auto_sc
            // 
            this.but_rob_auto_sc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_rob_auto_sc.Location = new System.Drawing.Point(1080, 625);
            this.but_rob_auto_sc.Name = "but_rob_auto_sc";
            this.but_rob_auto_sc.Size = new System.Drawing.Size(106, 33);
            this.but_rob_auto_sc.TabIndex = 142;
            this.but_rob_auto_sc.Text = "Auto";
            this.but_rob_auto_sc.UseVisualStyleBackColor = true;
            this.but_rob_auto_sc.Click += new System.EventHandler(this.but_rob_auto_sc_Click);
            // 
            // tb_rob_pos_sc
            // 
            this.tb_rob_pos_sc.Location = new System.Drawing.Point(965, 838);
            this.tb_rob_pos_sc.Name = "tb_rob_pos_sc";
            this.tb_rob_pos_sc.Size = new System.Drawing.Size(247, 20);
            this.tb_rob_pos_sc.TabIndex = 141;
            this.tb_rob_pos_sc.Text = "0";
            // 
            // but_rob_res_sc
            // 
            this.but_rob_res_sc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_rob_res_sc.Location = new System.Drawing.Point(968, 757);
            this.but_rob_res_sc.Name = "but_rob_res_sc";
            this.but_rob_res_sc.Size = new System.Drawing.Size(106, 33);
            this.but_rob_res_sc.TabIndex = 140;
            this.but_rob_res_sc.Text = "Reseive robot";
            this.but_rob_res_sc.UseVisualStyleBackColor = true;
            this.but_rob_res_sc.Click += new System.EventHandler(this.but_rob_res_sc_Click);
            // 
            // but_rob_con_sc
            // 
            this.but_rob_con_sc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_rob_con_sc.Location = new System.Drawing.Point(968, 625);
            this.but_rob_con_sc.Name = "but_rob_con_sc";
            this.but_rob_con_sc.Size = new System.Drawing.Size(106, 33);
            this.but_rob_con_sc.TabIndex = 137;
            this.but_rob_con_sc.Text = "Connect robot";
            this.but_rob_con_sc.UseVisualStyleBackColor = true;
            this.but_rob_con_sc.Click += new System.EventHandler(this.but_rob_con_sc_Click);
            // 
            // but_rob_discon_sc
            // 
            this.but_rob_discon_sc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_rob_discon_sc.Location = new System.Drawing.Point(968, 669);
            this.but_rob_discon_sc.Name = "but_rob_discon_sc";
            this.but_rob_discon_sc.Size = new System.Drawing.Size(106, 33);
            this.but_rob_discon_sc.TabIndex = 138;
            this.but_rob_discon_sc.Text = "Disconnect robot";
            this.but_rob_discon_sc.UseVisualStyleBackColor = true;
            this.but_rob_discon_sc.Click += new System.EventHandler(this.but_rob_discon_sc_Click);
            // 
            // but_rob_send_sc
            // 
            this.but_rob_send_sc.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_rob_send_sc.Location = new System.Drawing.Point(968, 713);
            this.but_rob_send_sc.Name = "but_rob_send_sc";
            this.but_rob_send_sc.Size = new System.Drawing.Size(106, 33);
            this.but_rob_send_sc.TabIndex = 139;
            this.but_rob_send_sc.Text = "Send robot";
            this.but_rob_send_sc.UseVisualStyleBackColor = true;
            this.but_rob_send_sc.Click += new System.EventHandler(this.but_rob_send_sc_Click);
            // 
            // but_reconstruc_area
            // 
            this.but_reconstruc_area.Location = new System.Drawing.Point(572, 933);
            this.but_reconstruc_area.Name = "but_reconstruc_area";
            this.but_reconstruc_area.Size = new System.Drawing.Size(96, 34);
            this.but_reconstruc_area.TabIndex = 136;
            this.but_reconstruc_area.Text = "Восстановить область";
            this.but_reconstruc_area.UseVisualStyleBackColor = true;
            this.but_reconstruc_area.Click += new System.EventHandler(this.but_reconstruc_area_Click);
            // 
            // but_keep_area
            // 
            this.but_keep_area.Location = new System.Drawing.Point(528, 856);
            this.but_keep_area.Name = "but_keep_area";
            this.but_keep_area.Size = new System.Drawing.Size(96, 34);
            this.but_keep_area.TabIndex = 135;
            this.but_keep_area.Text = "Оставить область";
            this.but_keep_area.UseVisualStyleBackColor = true;
            this.but_keep_area.Click += new System.EventHandler(this.but_keep_area_Click);
            // 
            // but_delete_area
            // 
            this.but_delete_area.Location = new System.Drawing.Point(528, 893);
            this.but_delete_area.Name = "but_delete_area";
            this.but_delete_area.Size = new System.Drawing.Size(96, 34);
            this.but_delete_area.TabIndex = 134;
            this.but_delete_area.Text = "Удалить область";
            this.but_delete_area.UseVisualStyleBackColor = true;
            this.but_delete_area.Click += new System.EventHandler(this.but_delete_area_Click);
            // 
            // but_load_sing_calib
            // 
            this.but_load_sing_calib.Location = new System.Drawing.Point(1721, 945);
            this.but_load_sing_calib.Name = "but_load_sing_calib";
            this.but_load_sing_calib.Size = new System.Drawing.Size(111, 42);
            this.but_load_sing_calib.TabIndex = 133;
            this.but_load_sing_calib.Text = "Загрузить калиб одиноч";
            this.but_load_sing_calib.UseVisualStyleBackColor = true;
            this.but_load_sing_calib.Click += new System.EventHandler(this.but_load_sing_calib_Click);
            // 
            // but_scan_load_sing
            // 
            this.but_scan_load_sing.Location = new System.Drawing.Point(1604, 944);
            this.but_scan_load_sing.Name = "but_scan_load_sing";
            this.but_scan_load_sing.Size = new System.Drawing.Size(111, 42);
            this.but_scan_load_sing.TabIndex = 132;
            this.but_scan_load_sing.Text = "Загрузить скан одиноч";
            this.but_scan_load_sing.UseVisualStyleBackColor = true;
            this.but_scan_load_sing.Click += new System.EventHandler(this.but_scan_load_sing_Click);
            // 
            // but_load_stl
            // 
            this.but_load_stl.Location = new System.Drawing.Point(776, 884);
            this.but_load_stl.Name = "but_load_stl";
            this.but_load_stl.Size = new System.Drawing.Size(132, 24);
            this.but_load_stl.TabIndex = 131;
            this.but_load_stl.Text = "Загрузить модель";
            this.but_load_stl.UseVisualStyleBackColor = true;
            this.but_load_stl.Click += new System.EventHandler(this.but_load_stl_Click);
            // 
            // but_save_stl
            // 
            this.but_save_stl.Location = new System.Drawing.Point(776, 854);
            this.but_save_stl.Name = "but_save_stl";
            this.but_save_stl.Size = new System.Drawing.Size(132, 24);
            this.but_save_stl.TabIndex = 130;
            this.but_save_stl.Text = "Сохранить модель";
            this.but_save_stl.UseVisualStyleBackColor = true;
            this.but_save_stl.Click += new System.EventHandler(this.but_save_stl_Click);
            // 
            // but_cross_flat
            // 
            this.but_cross_flat.Location = new System.Drawing.Point(431, 907);
            this.but_cross_flat.Name = "but_cross_flat";
            this.but_cross_flat.Size = new System.Drawing.Size(75, 23);
            this.but_cross_flat.TabIndex = 129;
            this.but_cross_flat.Text = "Пересеч";
            this.but_cross_flat.UseVisualStyleBackColor = true;
            this.but_cross_flat.Click += new System.EventHandler(this.but_cross_flat_Click);
            // 
            // but_traj_clear
            // 
            this.but_traj_clear.Location = new System.Drawing.Point(247, 979);
            this.but_traj_clear.Name = "but_traj_clear";
            this.but_traj_clear.Size = new System.Drawing.Size(96, 24);
            this.but_traj_clear.TabIndex = 128;
            this.but_traj_clear.Text = "Очистить траек";
            this.but_traj_clear.UseVisualStyleBackColor = true;
            this.but_traj_clear.Click += new System.EventHandler(this.but_traj_clear_Click);
            // 
            // tp_smooth_scan
            // 
            this.tp_smooth_scan.Location = new System.Drawing.Point(1118, 864);
            this.tp_smooth_scan.Name = "tp_smooth_scan";
            this.tp_smooth_scan.Size = new System.Drawing.Size(68, 20);
            this.tp_smooth_scan.TabIndex = 127;
            this.tp_smooth_scan.Text = "-1";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(1049, 867);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(63, 13);
            this.label19.TabIndex = 126;
            this.label19.Text = "smooth, mm";
            // 
            // but_gl_light
            // 
            this.but_gl_light.Location = new System.Drawing.Point(431, 958);
            this.but_gl_light.Name = "but_gl_light";
            this.but_gl_light.Size = new System.Drawing.Size(96, 49);
            this.but_gl_light.TabIndex = 125;
            this.but_gl_light.Text = "Отобразить освещение";
            this.but_gl_light.UseVisualStyleBackColor = true;
            this.but_gl_light.Click += new System.EventHandler(this.but_gl_light_Click);
            // 
            // but_load_fr_cal
            // 
            this.but_load_fr_cal.Location = new System.Drawing.Point(1526, 864);
            this.but_load_fr_cal.Name = "but_load_fr_cal";
            this.but_load_fr_cal.Size = new System.Drawing.Size(149, 20);
            this.but_load_fr_cal.TabIndex = 124;
            this.but_load_fr_cal.Text = "Загрузить калиборовку";
            this.but_load_fr_cal.UseVisualStyleBackColor = true;
            this.but_load_fr_cal.Click += new System.EventHandler(this.but_load_fr_cal_Click);
            // 
            // tb_strip_scan
            // 
            this.tb_strip_scan.Location = new System.Drawing.Point(965, 864);
            this.tb_strip_scan.Name = "tb_strip_scan";
            this.tb_strip_scan.Size = new System.Drawing.Size(68, 20);
            this.tb_strip_scan.TabIndex = 123;
            this.tb_strip_scan.Text = "5";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(929, 867);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(26, 13);
            this.label18.TabIndex = 122;
            this.label18.Text = "strip";
            // 
            // but_im_to_3d_im1
            // 
            this.but_im_to_3d_im1.Location = new System.Drawing.Point(14, 872);
            this.but_im_to_3d_im1.Name = "but_im_to_3d_im1";
            this.but_im_to_3d_im1.Size = new System.Drawing.Size(96, 49);
            this.but_im_to_3d_im1.TabIndex = 121;
            this.but_im_to_3d_im1.Text = "3д модель изобр im1";
            this.but_im_to_3d_im1.UseVisualStyleBackColor = true;
            this.but_im_to_3d_im1.Click += new System.EventHandler(this.but_im_to_3d_im1_Click);
            // 
            // but_calibr_Bfs
            // 
            this.but_calibr_Bfs.Location = new System.Drawing.Point(1424, 849);
            this.but_calibr_Bfs.Name = "but_calibr_Bfs";
            this.but_calibr_Bfs.Size = new System.Drawing.Size(96, 34);
            this.but_calibr_Bfs.TabIndex = 120;
            this.but_calibr_Bfs.Text = "Калибровка относит фланца";
            this.but_calibr_Bfs.UseVisualStyleBackColor = true;
            this.but_calibr_Bfs.Click += new System.EventHandler(this.but_calibr_Bfs_Click);
            // 
            // propGrid_traj
            // 
            this.propGrid_traj.Location = new System.Drawing.Point(807, 283);
            this.propGrid_traj.Name = "propGrid_traj";
            this.propGrid_traj.Size = new System.Drawing.Size(183, 343);
            this.propGrid_traj.TabIndex = 119;
            // 
            // but_gl_clear
            // 
            this.but_gl_clear.Location = new System.Drawing.Point(243, 955);
            this.but_gl_clear.Name = "but_gl_clear";
            this.but_gl_clear.Size = new System.Drawing.Size(96, 28);
            this.but_gl_clear.TabIndex = 118;
            this.but_gl_clear.Text = "Очистить";
            this.but_gl_clear.UseVisualStyleBackColor = true;
            this.but_gl_clear.Click += new System.EventHandler(this.but_gl_clear_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(929, 970);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(30, 13);
            this.label13.TabIndex = 117;
            this.label13.Text = "scan";
            // 
            // but_scan_path
            // 
            this.but_scan_path.Location = new System.Drawing.Point(1526, 965);
            this.but_scan_path.Name = "but_scan_path";
            this.but_scan_path.Size = new System.Drawing.Size(72, 20);
            this.but_scan_path.TabIndex = 116;
            this.but_scan_path.Text = "Выбрать";
            this.but_scan_path.UseVisualStyleBackColor = true;
            this.but_scan_path.Click += new System.EventHandler(this.but_scan_path_Click);
            // 
            // textB_scan_path
            // 
            this.textB_scan_path.Location = new System.Drawing.Point(965, 967);
            this.textB_scan_path.Name = "textB_scan_path";
            this.textB_scan_path.Size = new System.Drawing.Size(555, 20);
            this.textB_scan_path.TabIndex = 115;
            this.textB_scan_path.Text = "\"\"";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(903, 944);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(60, 13);
            this.label12.TabIndex = 114;
            this.label12.Text = "cam2_conf";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(903, 919);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(60, 13);
            this.label11.TabIndex = 113;
            this.label11.Text = "cam1_conf";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(903, 893);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 13);
            this.label3.TabIndex = 112;
            this.label3.Text = "stereo_cal";
            // 
            // but_stereo_cal_path
            // 
            this.but_stereo_cal_path.Location = new System.Drawing.Point(1526, 888);
            this.but_stereo_cal_path.Name = "but_stereo_cal_path";
            this.but_stereo_cal_path.Size = new System.Drawing.Size(72, 20);
            this.but_stereo_cal_path.TabIndex = 111;
            this.but_stereo_cal_path.Text = "Выбрать";
            this.but_stereo_cal_path.UseVisualStyleBackColor = true;
            this.but_stereo_cal_path.Click += new System.EventHandler(this.but_stereo_cal_path_Click);
            // 
            // textB_stereo_cal_path
            // 
            this.textB_stereo_cal_path.Location = new System.Drawing.Point(965, 889);
            this.textB_stereo_cal_path.Name = "textB_stereo_cal_path";
            this.textB_stereo_cal_path.Size = new System.Drawing.Size(555, 20);
            this.textB_stereo_cal_path.TabIndex = 110;
            this.textB_stereo_cal_path.Text = "\"\"";
            // 
            // but_load_conf_cam2
            // 
            this.but_load_conf_cam2.Location = new System.Drawing.Point(1526, 939);
            this.but_load_conf_cam2.Name = "but_load_conf_cam2";
            this.but_load_conf_cam2.Size = new System.Drawing.Size(72, 20);
            this.but_load_conf_cam2.TabIndex = 109;
            this.but_load_conf_cam2.Text = "Выбрать";
            this.but_load_conf_cam2.UseVisualStyleBackColor = true;
            this.but_load_conf_cam2.Click += new System.EventHandler(this.but_load_conf_cam2_Click);
            // 
            // but_load_conf_cam1
            // 
            this.but_load_conf_cam1.Location = new System.Drawing.Point(1526, 914);
            this.but_load_conf_cam1.Name = "but_load_conf_cam1";
            this.but_load_conf_cam1.Size = new System.Drawing.Size(72, 20);
            this.but_load_conf_cam1.TabIndex = 108;
            this.but_load_conf_cam1.Text = "Выбрать";
            this.but_load_conf_cam1.UseVisualStyleBackColor = true;
            this.but_load_conf_cam1.Click += new System.EventHandler(this.but_load_conf_cam1_Click);
            // 
            // textB_cam2_conf
            // 
            this.textB_cam2_conf.Location = new System.Drawing.Point(965, 941);
            this.textB_cam2_conf.Name = "textB_cam2_conf";
            this.textB_cam2_conf.Size = new System.Drawing.Size(555, 20);
            this.textB_cam2_conf.TabIndex = 107;
            this.textB_cam2_conf.Text = "\"\"";
            // 
            // textB_cam1_conf
            // 
            this.textB_cam1_conf.Location = new System.Drawing.Point(965, 915);
            this.textB_cam1_conf.Name = "textB_cam1_conf";
            this.textB_cam1_conf.Size = new System.Drawing.Size(555, 20);
            this.textB_cam1_conf.TabIndex = 106;
            this.textB_cam1_conf.Text = "\"\"";
            // 
            // but_scan_load_ex
            // 
            this.but_scan_load_ex.Location = new System.Drawing.Point(1604, 896);
            this.but_scan_load_ex.Name = "but_scan_load_ex";
            this.but_scan_load_ex.Size = new System.Drawing.Size(111, 42);
            this.but_scan_load_ex.TabIndex = 105;
            this.but_scan_load_ex.Text = "Загрузить скан стерео";
            this.but_scan_load_ex.UseVisualStyleBackColor = true;
            this.but_scan_load_ex.Click += new System.EventHandler(this.but_scan_load_ex_Click);
            // 
            // but_send_traj
            // 
            this.but_send_traj.Location = new System.Drawing.Point(807, 967);
            this.but_send_traj.Name = "but_send_traj";
            this.but_send_traj.Size = new System.Drawing.Size(96, 34);
            this.but_send_traj.TabIndex = 104;
            this.but_send_traj.Text = "Отправить траекторию";
            this.but_send_traj.UseVisualStyleBackColor = true;
            this.but_send_traj.Click += new System.EventHandler(this.but_send_traj_Click);
            // 
            // but_end_cont
            // 
            this.but_end_cont.Location = new System.Drawing.Point(528, 820);
            this.but_end_cont.Name = "but_end_cont";
            this.but_end_cont.Size = new System.Drawing.Size(96, 34);
            this.but_end_cont.TabIndex = 103;
            this.but_end_cont.Text = "Сохранить контур";
            this.but_end_cont.UseVisualStyleBackColor = true;
            this.but_end_cont.Click += new System.EventHandler(this.but_end_cont_Click);
            // 
            // but_point_type
            // 
            this.but_point_type.Location = new System.Drawing.Point(361, 812);
            this.but_point_type.Name = "but_point_type";
            this.but_point_type.Size = new System.Drawing.Size(96, 50);
            this.but_point_type.TabIndex = 101;
            this.but_point_type.Text = "Точки";
            this.but_point_type.UseVisualStyleBackColor = true;
            this.but_point_type.Click += new System.EventHandler(this.but_point_type_Click);
            // 
            // but_text_vis
            // 
            this.but_text_vis.Location = new System.Drawing.Point(470, 951);
            this.but_text_vis.Name = "but_text_vis";
            this.but_text_vis.Size = new System.Drawing.Size(96, 50);
            this.but_text_vis.TabIndex = 100;
            this.but_text_vis.Text = "Отобразить текстуру";
            this.but_text_vis.UseVisualStyleBackColor = true;
            this.but_text_vis.Click += new System.EventHandler(this.but_text_vis_Click);
            // 
            // lab_TRZ
            // 
            this.lab_TRZ.AutoSize = true;
            this.lab_TRZ.Location = new System.Drawing.Point(156, 890);
            this.lab_TRZ.Name = "lab_TRZ";
            this.lab_TRZ.Size = new System.Drawing.Size(22, 13);
            this.lab_TRZ.TabIndex = 99;
            this.lab_TRZ.Text = "cor";
            // 
            // debugBox
            // 
            this.debugBox.Location = new System.Drawing.Point(1218, 6);
            this.debugBox.Name = "debugBox";
            this.debugBox.Size = new System.Drawing.Size(402, 784);
            this.debugBox.TabIndex = 62;
            this.debugBox.Text = "";
            // 
            // textBox_monitor_id
            // 
            this.textBox_monitor_id.Location = new System.Drawing.Point(89, 812);
            this.textBox_monitor_id.Name = "textBox_monitor_id";
            this.textBox_monitor_id.Size = new System.Drawing.Size(100, 20);
            this.textBox_monitor_id.TabIndex = 68;
            this.textBox_monitor_id.Text = "0 1";
            // 
            // glControl1
            // 
            this.glControl1.AccessibleName = "1";
            this.glControl1.Animation = true;
            this.glControl1.AnimationTime = 60;
            this.glControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.glControl1.ColorBits = ((uint)(24u));
            this.glControl1.DepthBits = ((uint)(24u));
            this.glControl1.Location = new System.Drawing.Point(6, 6);
            this.glControl1.Margin = new System.Windows.Forms.Padding(4);
            this.glControl1.MultisampleBits = ((uint)(8u));
            this.glControl1.Name = "glControl1";
            this.glControl1.Size = new System.Drawing.Size(800, 800);
            this.glControl1.StencilBits = ((uint)(0u));
            this.glControl1.TabIndex = 65;
            this.glControl1.ContextCreated += new System.EventHandler<OpenGL.GlControlEventArgs>(this.glControl1_ContextCreated);
            this.glControl1.Render += new System.EventHandler<OpenGL.GlControlEventArgs>(this.glControl1_Render);
            this.glControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseMove);
            this.glControl1.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Form1_mousewheel);
            this.glControl1.Resize += new System.EventHandler(this.glControl1_Resize);
            // 
            // but_gl_cam_calib
            // 
            this.but_gl_cam_calib.Location = new System.Drawing.Point(390, 951);
            this.but_gl_cam_calib.Name = "but_gl_cam_calib";
            this.but_gl_cam_calib.Size = new System.Drawing.Size(96, 56);
            this.but_gl_cam_calib.TabIndex = 98;
            this.but_gl_cam_calib.Text = "Калибровка камеры";
            this.but_gl_cam_calib.UseVisualStyleBackColor = true;
            this.but_gl_cam_calib.Click += new System.EventHandler(this.but_gl_cam_calib_Click);
            // 
            // imBox_3dDebug
            // 
            this.imBox_3dDebug.Location = new System.Drawing.Point(1218, 411);
            this.imBox_3dDebug.Name = "imBox_3dDebug";
            this.imBox_3dDebug.Size = new System.Drawing.Size(400, 400);
            this.imBox_3dDebug.TabIndex = 97;
            this.imBox_3dDebug.TabStop = false;
            // 
            // but_SubpixPrec
            // 
            this.but_SubpixPrec.Location = new System.Drawing.Point(812, 818);
            this.but_SubpixPrec.Name = "but_SubpixPrec";
            this.but_SubpixPrec.Size = new System.Drawing.Size(75, 23);
            this.but_SubpixPrec.TabIndex = 96;
            this.but_SubpixPrec.Text = "SubpixelPrec";
            this.but_SubpixPrec.UseVisualStyleBackColor = true;
            this.but_SubpixPrec.Click += new System.EventHandler(this.but_SubpixPrec_Click);
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(1638, 417);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(44, 13);
            this.label44.TabIndex = 95;
            this.label44.Text = "minDisp";
            // 
            // trackBar10
            // 
            this.trackBar10.AccessibleName = "10";
            this.trackBar10.Location = new System.Drawing.Point(1688, 869);
            this.trackBar10.Maximum = 20;
            this.trackBar10.Name = "trackBar10";
            this.trackBar10.Size = new System.Drawing.Size(188, 45);
            this.trackBar10.TabIndex = 94;
            this.trackBar10.Value = 10;
            this.trackBar10.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(1809, 922);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(70, 13);
            this.label43.TabIndex = 93;
            this.label43.Text = "specleRange";
            // 
            // trackBar9
            // 
            this.trackBar9.AccessibleName = "9";
            this.trackBar9.Location = new System.Drawing.Point(1688, 818);
            this.trackBar9.Maximum = 20;
            this.trackBar9.Name = "trackBar9";
            this.trackBar9.Size = new System.Drawing.Size(188, 45);
            this.trackBar9.TabIndex = 92;
            this.trackBar9.Value = 10;
            this.trackBar9.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(1626, 832);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(56, 13);
            this.label42.TabIndex = 91;
            this.label42.Text = "specleWS";
            // 
            // trackBar8
            // 
            this.trackBar8.AccessibleName = "8";
            this.trackBar8.Location = new System.Drawing.Point(1688, 766);
            this.trackBar8.Maximum = 20;
            this.trackBar8.Name = "trackBar8";
            this.trackBar8.Size = new System.Drawing.Size(188, 45);
            this.trackBar8.TabIndex = 90;
            this.trackBar8.Value = 10;
            this.trackBar8.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(1638, 793);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(37, 13);
            this.label41.TabIndex = 89;
            this.label41.Text = "unuqe";
            // 
            // trackBar7
            // 
            this.trackBar7.AccessibleName = "7";
            this.trackBar7.Location = new System.Drawing.Point(1688, 715);
            this.trackBar7.Maximum = 20;
            this.trackBar7.Name = "trackBar7";
            this.trackBar7.Size = new System.Drawing.Size(188, 45);
            this.trackBar7.TabIndex = 88;
            this.trackBar7.Value = 10;
            this.trackBar7.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(1645, 734);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(41, 13);
            this.label40.TabIndex = 87;
            this.label40.Text = "prefilter";
            // 
            // trackBar6
            // 
            this.trackBar6.AccessibleName = "6";
            this.trackBar6.Location = new System.Drawing.Point(1688, 664);
            this.trackBar6.Maximum = 20;
            this.trackBar6.Name = "trackBar6";
            this.trackBar6.Size = new System.Drawing.Size(188, 45);
            this.trackBar6.TabIndex = 86;
            this.trackBar6.Value = 10;
            this.trackBar6.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(1613, 684);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(73, 13);
            this.label39.TabIndex = 85;
            this.label39.Text = "Disp12maxdiff";
            // 
            // trackBar5
            // 
            this.trackBar5.AccessibleName = "5";
            this.trackBar5.Location = new System.Drawing.Point(1688, 613);
            this.trackBar5.Maximum = 60;
            this.trackBar5.Name = "trackBar5";
            this.trackBar5.Size = new System.Drawing.Size(188, 45);
            this.trackBar5.TabIndex = 84;
            this.trackBar5.Value = 10;
            this.trackBar5.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(1656, 629);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(19, 13);
            this.label38.TabIndex = 83;
            this.label38.Text = "p2";
            // 
            // trackBar4
            // 
            this.trackBar4.AccessibleName = "4";
            this.trackBar4.Location = new System.Drawing.Point(1688, 562);
            this.trackBar4.Maximum = 60;
            this.trackBar4.Name = "trackBar4";
            this.trackBar4.Size = new System.Drawing.Size(188, 45);
            this.trackBar4.TabIndex = 82;
            this.trackBar4.Value = 10;
            this.trackBar4.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(1656, 575);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(19, 13);
            this.label37.TabIndex = 81;
            this.label37.Text = "p1";
            // 
            // trackBar3
            // 
            this.trackBar3.AccessibleName = "3";
            this.trackBar3.Location = new System.Drawing.Point(1688, 511);
            this.trackBar3.Maximum = 20;
            this.trackBar3.Name = "trackBar3";
            this.trackBar3.Size = new System.Drawing.Size(188, 45);
            this.trackBar3.TabIndex = 80;
            this.trackBar3.Value = 10;
            this.trackBar3.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(1629, 530);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(53, 13);
            this.label36.TabIndex = 79;
            this.label36.Text = "blockSize";
            // 
            // trackBar2
            // 
            this.trackBar2.AccessibleName = "2";
            this.trackBar2.Location = new System.Drawing.Point(1688, 460);
            this.trackBar2.Maximum = 20;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(188, 45);
            this.trackBar2.TabIndex = 78;
            this.trackBar2.Value = 2;
            this.trackBar2.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(1638, 468);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(48, 13);
            this.label35.TabIndex = 77;
            this.label35.Text = "manDisp";
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(1717, 363);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(96, 13);
            this.label34.TabIndex = 75;
            this.label34.Text = "Настройки SGBM";
            // 
            // trackBar1
            // 
            this.trackBar1.AccessibleName = "1";
            this.trackBar1.Location = new System.Drawing.Point(1688, 409);
            this.trackBar1.Maximum = 20;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(188, 45);
            this.trackBar1.TabIndex = 74;
            this.trackBar1.Value = 1;
            this.trackBar1.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // imBox_disparity
            // 
            this.imBox_disparity.Location = new System.Drawing.Point(812, 412);
            this.imBox_disparity.Name = "imBox_disparity";
            this.imBox_disparity.Size = new System.Drawing.Size(400, 400);
            this.imBox_disparity.TabIndex = 73;
            this.imBox_disparity.TabStop = false;
            // 
            // but_imGen
            // 
            this.but_imGen.Location = new System.Drawing.Point(349, 947);
            this.but_imGen.Name = "but_imGen";
            this.but_imGen.Size = new System.Drawing.Size(96, 56);
            this.but_imGen.TabIndex = 72;
            this.but_imGen.Text = "Начать генерацию";
            this.but_imGen.UseVisualStyleBackColor = true;
            this.but_imGen.Click += new System.EventHandler(this.but_imGen_Click);
            // 
            // imBox_mark2
            // 
            this.imBox_mark2.Location = new System.Drawing.Point(1218, 6);
            this.imBox_mark2.Name = "imBox_mark2";
            this.imBox_mark2.Size = new System.Drawing.Size(400, 400);
            this.imBox_mark2.TabIndex = 71;
            this.imBox_mark2.TabStop = false;
            // 
            // imBox_mark1
            // 
            this.imBox_mark1.Location = new System.Drawing.Point(812, 6);
            this.imBox_mark1.Name = "imBox_mark1";
            this.imBox_mark1.Size = new System.Drawing.Size(400, 400);
            this.imBox_mark1.TabIndex = 2;
            this.imBox_mark1.TabStop = false;
            // 
            // lab_check
            // 
            this.lab_check.AutoSize = true;
            this.lab_check.Location = new System.Drawing.Point(195, 815);
            this.lab_check.Name = "lab_check";
            this.lab_check.Size = new System.Drawing.Size(38, 13);
            this.lab_check.TabIndex = 70;
            this.lab_check.Text = "curCor";
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(11, 815);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(58, 13);
            this.label33.TabIndex = 69;
            this.label33.Text = "ID_monitor";
            // 
            // but_swapMonit
            // 
            this.but_swapMonit.Location = new System.Drawing.Point(309, 944);
            this.but_swapMonit.Name = "but_swapMonit";
            this.but_swapMonit.Size = new System.Drawing.Size(96, 56);
            this.but_swapMonit.TabIndex = 67;
            this.but_swapMonit.Text = "Поменять местами";
            this.but_swapMonit.UseVisualStyleBackColor = true;
            this.but_swapMonit.Click += new System.EventHandler(this.but_swapMonit_Click);
            // 
            // lab_curCor
            // 
            this.lab_curCor.AutoSize = true;
            this.lab_curCor.Location = new System.Drawing.Point(122, 850);
            this.lab_curCor.Name = "lab_curCor";
            this.lab_curCor.Size = new System.Drawing.Size(38, 13);
            this.lab_curCor.TabIndex = 66;
            this.lab_curCor.Text = "curCor";
            // 
            // lab_kor
            // 
            this.lab_kor.AutoSize = true;
            this.lab_kor.Location = new System.Drawing.Point(15, 850);
            this.lab_kor.Name = "lab_kor";
            this.lab_kor.Size = new System.Drawing.Size(22, 13);
            this.lab_kor.TabIndex = 64;
            this.lab_kor.Text = "cor";
            // 
            // but_modeV
            // 
            this.but_modeV.Location = new System.Drawing.Point(259, 810);
            this.but_modeV.Name = "but_modeV";
            this.but_modeV.Size = new System.Drawing.Size(96, 56);
            this.but_modeV.TabIndex = 63;
            this.but_modeV.Text = "Рисование(текущая: обозреватель)";
            this.but_modeV.UseVisualStyleBackColor = true;
            this.but_modeV.Click += new System.EventHandler(this.but_modeV_Click);
            // 
            // butt_plane_Ozx
            // 
            this.butt_plane_Ozx.CausesValidation = false;
            this.butt_plane_Ozx.Location = new System.Drawing.Point(379, 872);
            this.butt_plane_Ozx.Name = "butt_plane_Ozx";
            this.butt_plane_Ozx.Size = new System.Drawing.Size(54, 31);
            this.butt_plane_Ozx.TabIndex = 61;
            this.butt_plane_Ozx.Text = "Ozx";
            this.butt_plane_Ozx.UseVisualStyleBackColor = true;
            this.butt_plane_Ozx.Click += new System.EventHandler(this.butt_plane_Ozx_Click);
            // 
            // but_plane_Oyz
            // 
            this.but_plane_Oyz.Location = new System.Drawing.Point(319, 872);
            this.but_plane_Oyz.Name = "but_plane_Oyz";
            this.but_plane_Oyz.Size = new System.Drawing.Size(54, 31);
            this.but_plane_Oyz.TabIndex = 60;
            this.but_plane_Oyz.Text = "Oyz";
            this.but_plane_Oyz.UseVisualStyleBackColor = true;
            this.but_plane_Oyz.Click += new System.EventHandler(this.but_plane_Oyz_Click);
            // 
            // but_plane_Oxy
            // 
            this.but_plane_Oxy.Location = new System.Drawing.Point(259, 872);
            this.but_plane_Oxy.Name = "but_plane_Oxy";
            this.but_plane_Oxy.Size = new System.Drawing.Size(54, 31);
            this.but_plane_Oxy.TabIndex = 59;
            this.but_plane_Oxy.Text = "Oxy";
            this.but_plane_Oxy.UseVisualStyleBackColor = true;
            this.but_plane_Oxy.Click += new System.EventHandler(this.but_plane_Oxy_Click);
            // 
            // but_ProjV
            // 
            this.but_ProjV.Location = new System.Drawing.Point(141, 931);
            this.but_ProjV.Name = "but_ProjV";
            this.but_ProjV.Size = new System.Drawing.Size(96, 56);
            this.but_ProjV.TabIndex = 58;
            this.but_ProjV.Text = "Проецирование(текущая:ортоганальная)";
            this.but_ProjV.UseVisualStyleBackColor = true;
            this.but_ProjV.Click += new System.EventHandler(this.but_ProjV_Click);
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(1685, 186);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(152, 13);
            this.label27.TabIndex = 57;
            this.label27.Text = "Положение источника света";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1702, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 13);
            this.label4.TabIndex = 56;
            this.label4.Text = "Ориентация камеры";
            // 
            // trackOz
            // 
            this.trackOz.Location = new System.Drawing.Point(1624, 76);
            this.trackOz.Maximum = 300;
            this.trackOz.Minimum = -300;
            this.trackOz.Name = "trackOz";
            this.trackOz.Size = new System.Drawing.Size(252, 45);
            this.trackOz.TabIndex = 55;
            this.trackOz.Value = 12;
            this.trackOz.Scroll += new System.EventHandler(this.trackOz_Scroll);
            // 
            // butSaveOpenGlIm
            // 
            this.butSaveOpenGlIm.Location = new System.Drawing.Point(14, 929);
            this.butSaveOpenGlIm.Name = "butSaveOpenGlIm";
            this.butSaveOpenGlIm.Size = new System.Drawing.Size(121, 42);
            this.butSaveOpenGlIm.TabIndex = 20;
            this.butSaveOpenGlIm.Text = "Сохранить изображение";
            this.butSaveOpenGlIm.UseVisualStyleBackColor = true;
            this.butSaveOpenGlIm.Click += new System.EventHandler(this.butFinPointFs_Click);
            // 
            // trackY_light
            // 
            this.trackY_light.Location = new System.Drawing.Point(1624, 264);
            this.trackY_light.Maximum = 100;
            this.trackY_light.Minimum = -100;
            this.trackY_light.Name = "trackY_light";
            this.trackY_light.Size = new System.Drawing.Size(252, 45);
            this.trackY_light.TabIndex = 50;
            this.trackY_light.Scroll += new System.EventHandler(this.trackY_light_Scroll);
            // 
            // trackZ_light
            // 
            this.trackZ_light.Location = new System.Drawing.Point(1624, 315);
            this.trackZ_light.Maximum = 100;
            this.trackZ_light.Minimum = -100;
            this.trackZ_light.Name = "trackZ_light";
            this.trackZ_light.Size = new System.Drawing.Size(252, 45);
            this.trackZ_light.TabIndex = 51;
            this.trackZ_light.Value = 12;
            this.trackZ_light.Scroll += new System.EventHandler(this.trackZ_light_Scroll);
            // 
            // trackOx
            // 
            this.trackOx.Location = new System.Drawing.Point(1624, 25);
            this.trackOx.Maximum = 300;
            this.trackOx.Minimum = -300;
            this.trackOx.Name = "trackOx";
            this.trackOx.Size = new System.Drawing.Size(252, 45);
            this.trackOx.TabIndex = 53;
            this.trackOx.Value = 12;
            this.trackOx.Scroll += new System.EventHandler(this.trackOx_Scroll);
            // 
            // trackX_light
            // 
            this.trackX_light.Location = new System.Drawing.Point(1624, 213);
            this.trackX_light.Maximum = 100;
            this.trackX_light.Minimum = -100;
            this.trackX_light.Name = "trackX_light";
            this.trackX_light.Size = new System.Drawing.Size(252, 45);
            this.trackX_light.TabIndex = 49;
            this.trackX_light.Value = 60;
            this.trackX_light.Scroll += new System.EventHandler(this.trackX_light_Scroll);
            // 
            // trackOy
            // 
            this.trackOy.Location = new System.Drawing.Point(1624, 127);
            this.trackOy.Maximum = 300;
            this.trackOy.Minimum = -300;
            this.trackOy.Name = "trackOy";
            this.trackOy.Size = new System.Drawing.Size(252, 45);
            this.trackOy.TabIndex = 54;
            this.trackOy.Value = 12;
            this.trackOy.Scroll += new System.EventHandler(this.trackOy_Scroll);
            // 
            // tabMain
            // 
            this.tabMain.Controls.Add(this.combo_robot_ch);
            this.tabMain.Controls.Add(this.tb_port_tcp);
            this.tabMain.Controls.Add(this.lab_fps_cam1);
            this.tabMain.Controls.Add(this.tB_fps_scan);
            this.tabMain.Controls.Add(this.label21);
            this.tabMain.Controls.Add(this.but_scan_sing_las);
            this.tabMain.Controls.Add(this.label_timer);
            this.tabMain.Controls.Add(this.but_load_scan);
            this.tabMain.Controls.Add(this.but_scan_stereolas);
            this.tabMain.Controls.Add(this.but_scan_marl);
            this.tabMain.Controls.Add(this.but_scan_def);
            this.tabMain.Controls.Add(this.but_scan_start_laser);
            this.tabMain.Controls.Add(this.label1);
            this.tabMain.Controls.Add(this.combo_improc);
            this.tabMain.Controls.Add(this.label56);
            this.tabMain.Controls.Add(this.groupBox1);
            this.tabMain.Controls.Add(this.label55);
            this.tabMain.Controls.Add(this.txBx_photoName);
            this.tabMain.Controls.Add(this.nameC2);
            this.tabMain.Controls.Add(this.nameB2);
            this.tabMain.Controls.Add(this.textNimVid);
            this.tabMain.Controls.Add(this.nameA2);
            this.tabMain.Controls.Add(this.nameX);
            this.tabMain.Controls.Add(this.nameY);
            this.tabMain.Controls.Add(this.nameC_in);
            this.tabMain.Controls.Add(this.nameZ);
            this.tabMain.Controls.Add(this.nameB_in);
            this.tabMain.Controls.Add(this.nameA_in);
            this.tabMain.Controls.Add(this.nameX2);
            this.tabMain.Controls.Add(this.nameZ_in);
            this.tabMain.Controls.Add(this.nameY2);
            this.tabMain.Controls.Add(this.nameY_in);
            this.tabMain.Controls.Add(this.nameZ2);
            this.tabMain.Controls.Add(this.nameX_in);
            this.tabMain.Controls.Add(this.boxN);
            this.tabMain.Controls.Add(this.nameC);
            this.tabMain.Controls.Add(this.nameB);
            this.tabMain.Controls.Add(this.nameA);
            this.tabMain.Controls.Add(this.box_scanFolder);
            this.tabMain.Controls.Add(this.box_photoFolder);
            this.tabMain.Controls.Add(this.imBox_base_2);
            this.tabMain.Controls.Add(this.imBox_base_1);
            this.tabMain.Controls.Add(this.but_ph);
            this.tabMain.Controls.Add(this.label45);
            this.tabMain.Controls.Add(this.trackBar11);
            this.tabMain.Controls.Add(this.label46);
            this.tabMain.Controls.Add(this.trackBar12);
            this.tabMain.Controls.Add(this.label47);
            this.tabMain.Controls.Add(this.trackBar13);
            this.tabMain.Controls.Add(this.label48);
            this.tabMain.Controls.Add(this.trackBar14);
            this.tabMain.Controls.Add(this.label49);
            this.tabMain.Controls.Add(this.trackBar15);
            this.tabMain.Controls.Add(this.label50);
            this.tabMain.Controls.Add(this.trackBar16);
            this.tabMain.Controls.Add(this.label51);
            this.tabMain.Controls.Add(this.trackBar17);
            this.tabMain.Controls.Add(this.label52);
            this.tabMain.Controls.Add(this.trackBar18);
            this.tabMain.Controls.Add(this.label53);
            this.tabMain.Controls.Add(this.trackBar19);
            this.tabMain.Controls.Add(this.label54);
            this.tabMain.Controls.Add(this.trackBar20);
            this.tabMain.Controls.Add(this.but_addBufRob);
            this.tabMain.Controls.Add(this.but_robMod);
            this.tabMain.Controls.Add(this.label24);
            this.tabMain.Controls.Add(this.label25);
            this.tabMain.Controls.Add(this.imageBox1);
            this.tabMain.Controls.Add(this.butStop);
            this.tabMain.Controls.Add(this.label26);
            this.tabMain.Controls.Add(this.butStart);
            this.tabMain.Controls.Add(this.imageBox2);
            this.tabMain.Controls.Add(this.videoCapt);
            this.tabMain.Controls.Add(this.rob_res);
            this.tabMain.Controls.Add(this.rob_con);
            this.tabMain.Controls.Add(this.but_res_pos_2);
            this.tabMain.Controls.Add(this.disc_rob);
            this.tabMain.Controls.Add(this.but_res_pos1);
            this.tabMain.Controls.Add(this.send_rob);
            this.tabMain.Controls.Add(this.bet_res_pos);
            this.tabMain.Controls.Add(this.but_scan_start);
            this.tabMain.Controls.Add(this.label17);
            this.tabMain.Controls.Add(this.but_photo);
            this.tabMain.Controls.Add(this.butCalcIm);
            this.tabMain.Controls.Add(this.label7);
            this.tabMain.Controls.Add(this.label16);
            this.tabMain.Controls.Add(this.label6);
            this.tabMain.Controls.Add(this.label15);
            this.tabMain.Controls.Add(this.label5);
            this.tabMain.Controls.Add(this.label14);
            this.tabMain.Controls.Add(this.label8);
            this.tabMain.Controls.Add(this.comboVideo);
            this.tabMain.Controls.Add(this.comboNumber);
            this.tabMain.Controls.Add(this.label10);
            this.tabMain.Controls.Add(this.label9);
            this.tabMain.Controls.Add(this.imBox_base);
            this.tabMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.tabMain.Location = new System.Drawing.Point(4, 22);
            this.tabMain.Name = "tabMain";
            this.tabMain.Padding = new System.Windows.Forms.Padding(3);
            this.tabMain.Size = new System.Drawing.Size(1882, 1003);
            this.tabMain.TabIndex = 1;
            this.tabMain.Text = "Основное";
            this.tabMain.UseVisualStyleBackColor = true;
            // 
            // combo_robot_ch
            // 
            this.combo_robot_ch.FormattingEnabled = true;
            this.combo_robot_ch.Location = new System.Drawing.Point(1407, 7);
            this.combo_robot_ch.Name = "combo_robot_ch";
            this.combo_robot_ch.Size = new System.Drawing.Size(121, 28);
            this.combo_robot_ch.TabIndex = 137;
            // 
            // tb_port_tcp
            // 
            this.tb_port_tcp.Location = new System.Drawing.Point(1296, 9);
            this.tb_port_tcp.Name = "tb_port_tcp";
            this.tb_port_tcp.Size = new System.Drawing.Size(100, 26);
            this.tb_port_tcp.TabIndex = 136;
            this.tb_port_tcp.Text = "30006";
            // 
            // lab_fps_cam1
            // 
            this.lab_fps_cam1.Location = new System.Drawing.Point(6, 9);
            this.lab_fps_cam1.Name = "lab_fps_cam1";
            this.lab_fps_cam1.Size = new System.Drawing.Size(100, 23);
            this.lab_fps_cam1.TabIndex = 0;
            // 
            // tB_fps_scan
            // 
            this.tB_fps_scan.Location = new System.Drawing.Point(1425, 475);
            this.tB_fps_scan.Name = "tB_fps_scan";
            this.tB_fps_scan.Size = new System.Drawing.Size(69, 26);
            this.tB_fps_scan.TabIndex = 133;
            this.tB_fps_scan.Text = "30";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label21.Location = new System.Drawing.Point(1364, 478);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(54, 20);
            this.label21.TabIndex = 135;
            this.label21.Text = "Fps ->";
            // 
            // but_scan_sing_las
            // 
            this.but_scan_sing_las.AccessibleName = "1";
            this.but_scan_sing_las.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_scan_sing_las.Location = new System.Drawing.Point(1407, 214);
            this.but_scan_sing_las.Name = "but_scan_sing_las";
            this.but_scan_sing_las.Size = new System.Drawing.Size(148, 33);
            this.but_scan_sing_las.TabIndex = 132;
            this.but_scan_sing_las.Text = "Scan SingLaser";
            this.but_scan_sing_las.UseVisualStyleBackColor = true;
            this.but_scan_sing_las.Click += new System.EventHandler(this.but_scan_sing_las_Click);
            // 
            // label_timer
            // 
            this.label_timer.AutoSize = true;
            this.label_timer.Font = new System.Drawing.Font("Microsoft Sans Serif", 24F);
            this.label_timer.Location = new System.Drawing.Point(3, 584);
            this.label_timer.Name = "label_timer";
            this.label_timer.Size = new System.Drawing.Size(118, 37);
            this.label_timer.TabIndex = 131;
            this.label_timer.Text = "label19";
            // 
            // but_load_scan
            // 
            this.but_load_scan.Location = new System.Drawing.Point(1110, 616);
            this.but_load_scan.Name = "but_load_scan";
            this.but_load_scan.Size = new System.Drawing.Size(106, 38);
            this.but_load_scan.TabIndex = 129;
            this.but_load_scan.Text = "Загрузить";
            this.but_load_scan.UseVisualStyleBackColor = true;
            this.but_load_scan.Click += new System.EventHandler(this.but_scan_load_ex_Click);
            // 
            // but_scan_stereolas
            // 
            this.but_scan_stereolas.AccessibleName = "1";
            this.but_scan_stereolas.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_scan_stereolas.Location = new System.Drawing.Point(1407, 175);
            this.but_scan_stereolas.Name = "but_scan_stereolas";
            this.but_scan_stereolas.Size = new System.Drawing.Size(148, 33);
            this.but_scan_stereolas.TabIndex = 128;
            this.but_scan_stereolas.Text = "Scan StereoLaser";
            this.but_scan_stereolas.UseVisualStyleBackColor = true;
            this.but_scan_stereolas.Click += new System.EventHandler(this.but_scan_stereolas_Click);
            // 
            // but_scan_marl
            // 
            this.but_scan_marl.AccessibleName = "2";
            this.but_scan_marl.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_scan_marl.Location = new System.Drawing.Point(1407, 41);
            this.but_scan_marl.Name = "but_scan_marl";
            this.but_scan_marl.Size = new System.Drawing.Size(148, 33);
            this.but_scan_marl.TabIndex = 127;
            this.but_scan_marl.Text = "Scan Laser marl";
            this.but_scan_marl.UseVisualStyleBackColor = true;
            this.but_scan_marl.Click += new System.EventHandler(this.but_scan_marl_Click);
            // 
            // but_scan_def
            // 
            this.but_scan_def.AccessibleName = "0";
            this.but_scan_def.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_scan_def.Location = new System.Drawing.Point(1407, 85);
            this.but_scan_def.Name = "but_scan_def";
            this.but_scan_def.Size = new System.Drawing.Size(148, 33);
            this.but_scan_def.TabIndex = 126;
            this.but_scan_def.Text = "Scan Laser def";
            this.but_scan_def.UseVisualStyleBackColor = true;
            this.but_scan_def.Click += new System.EventHandler(this.but_scan_start_laser_Click);
            // 
            // but_scan_start_laser
            // 
            this.but_scan_start_laser.AccessibleName = "1";
            this.but_scan_start_laser.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_scan_start_laser.Location = new System.Drawing.Point(1407, 129);
            this.but_scan_start_laser.Name = "but_scan_start_laser";
            this.but_scan_start_laser.Size = new System.Drawing.Size(148, 33);
            this.but_scan_start_laser.TabIndex = 125;
            this.but_scan_start_laser.Text = "Scan Laser";
            this.but_scan_start_laser.UseVisualStyleBackColor = true;
            this.but_scan_start_laser.Click += new System.EventHandler(this.but_scan_start_laser_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1544, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 20);
            this.label1.TabIndex = 124;
            this.label1.Text = "Camera:";
            // 
            // combo_improc
            // 
            this.combo_improc.FormattingEnabled = true;
            this.combo_improc.Location = new System.Drawing.Point(1407, 329);
            this.combo_improc.Name = "combo_improc";
            this.combo_improc.Size = new System.Drawing.Size(121, 28);
            this.combo_improc.TabIndex = 123;
            this.combo_improc.SelectedIndexChanged += new System.EventHandler(this.combo_improc_SelectedIndexChanged);
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(1412, 305);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(63, 20);
            this.label56.TabIndex = 122;
            this.label56.Text = "ImProc:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.but_extr_st);
            this.groupBox1.Controls.Add(this.label57);
            this.groupBox1.Controls.Add(this.tb_print_syr_d);
            this.groupBox1.Controls.Add(this.label23);
            this.groupBox1.Controls.Add(this.label22);
            this.groupBox1.Controls.Add(this.tb_print_vel);
            this.groupBox1.Controls.Add(this.tb_print_nozzle_d);
            this.groupBox1.Controls.Add(this.but_dir_disp);
            this.groupBox1.Controls.Add(this.tb_dir_disp);
            this.groupBox1.Controls.Add(this.but_home_las);
            this.groupBox1.Controls.Add(this.but_div_disp);
            this.groupBox1.Controls.Add(this.tb_div_disp);
            this.groupBox1.Controls.Add(this.but_las_enc);
            this.groupBox1.Controls.Add(this.label20);
            this.groupBox1.Controls.Add(this.butset_kvp);
            this.groupBox1.Controls.Add(this.textBox_set_kvp);
            this.groupBox1.Controls.Add(this.but_set_kpp);
            this.groupBox1.Controls.Add(this.textBox_set_kpp);
            this.groupBox1.Controls.Add(this.but_laser_dest);
            this.groupBox1.Controls.Add(this.textBox_laser_dest);
            this.groupBox1.Controls.Add(this.but_setShvpVel);
            this.groupBox1.Controls.Add(this.textBox_shvpVel);
            this.groupBox1.Controls.Add(this.but_marl_receav);
            this.groupBox1.Controls.Add(this.but_marl_close);
            this.groupBox1.Controls.Add(this.but_marl_open);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_marl_shcpPos);
            this.groupBox1.Controls.Add(this.but_marl_setShvpPos);
            this.groupBox1.Controls.Add(this.but_setShvpPos);
            this.groupBox1.Controls.Add(this.textBox_shvpPos);
            this.groupBox1.Controls.Add(this.but_open);
            this.groupBox1.Controls.Add(this.but_close);
            this.groupBox1.Controls.Add(this.but_laserOn);
            this.groupBox1.Controls.Add(this.but_laserOff);
            this.groupBox1.Controls.Add(this.but_setPower);
            this.groupBox1.Controls.Add(this.comboBox_portsArd);
            this.groupBox1.Controls.Add(this.but_find_ports);
            this.groupBox1.Controls.Add(this.textBox_powerLaser);
            this.groupBox1.Location = new System.Drawing.Point(1222, 529);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(654, 468);
            this.groupBox1.TabIndex = 121;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Scaner Manual Control";
            // 
            // but_extr_st
            // 
            this.but_extr_st.Location = new System.Drawing.Point(538, 319);
            this.but_extr_st.Name = "but_extr_st";
            this.but_extr_st.Size = new System.Drawing.Size(110, 55);
            this.but_extr_st.TabIndex = 144;
            this.but_extr_st.Text = "Выдавливание";
            this.but_extr_st.UseVisualStyleBackColor = true;
            this.but_extr_st.Click += new System.EventHandler(this.but_extr_st_Click);
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label57.Location = new System.Drawing.Point(397, 290);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(139, 20);
            this.label57.TabIndex = 143;
            this.label57.Text = "Диаметр шприца";
            // 
            // tb_print_syr_d
            // 
            this.tb_print_syr_d.Location = new System.Drawing.Point(538, 287);
            this.tb_print_syr_d.Name = "tb_print_syr_d";
            this.tb_print_syr_d.Size = new System.Drawing.Size(110, 26);
            this.tb_print_syr_d.TabIndex = 142;
            this.tb_print_syr_d.Text = "12";
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label23.Location = new System.Drawing.Point(396, 258);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(139, 20);
            this.label23.TabIndex = 141;
            this.label23.Text = "Скорость печати";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label22.Location = new System.Drawing.Point(409, 226);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(126, 20);
            this.label22.TabIndex = 138;
            this.label22.Text = "Толщина сопла";
            // 
            // tb_print_vel
            // 
            this.tb_print_vel.Location = new System.Drawing.Point(538, 255);
            this.tb_print_vel.Name = "tb_print_vel";
            this.tb_print_vel.Size = new System.Drawing.Size(110, 26);
            this.tb_print_vel.TabIndex = 140;
            this.tb_print_vel.Text = "5";
            // 
            // tb_print_nozzle_d
            // 
            this.tb_print_nozzle_d.Location = new System.Drawing.Point(538, 223);
            this.tb_print_nozzle_d.Name = "tb_print_nozzle_d";
            this.tb_print_nozzle_d.Size = new System.Drawing.Size(110, 26);
            this.tb_print_nozzle_d.TabIndex = 139;
            this.tb_print_nozzle_d.Text = "1";
            // 
            // but_dir_disp
            // 
            this.but_dir_disp.Location = new System.Drawing.Point(451, 147);
            this.but_dir_disp.Name = "but_dir_disp";
            this.but_dir_disp.Size = new System.Drawing.Size(110, 55);
            this.but_dir_disp.TabIndex = 138;
            this.but_dir_disp.Text = "Установить направ.";
            this.but_dir_disp.UseVisualStyleBackColor = true;
            this.but_dir_disp.Click += new System.EventHandler(this.but_dir_disp_Click);
            // 
            // tb_dir_disp
            // 
            this.tb_dir_disp.Location = new System.Drawing.Point(562, 161);
            this.tb_dir_disp.Name = "tb_dir_disp";
            this.tb_dir_disp.Size = new System.Drawing.Size(71, 26);
            this.tb_dir_disp.TabIndex = 137;
            this.tb_dir_disp.Text = "0";
            // 
            // but_home_las
            // 
            this.but_home_las.Location = new System.Drawing.Point(401, 32);
            this.but_home_las.Name = "but_home_las";
            this.but_home_las.Size = new System.Drawing.Size(110, 46);
            this.but_home_las.TabIndex = 136;
            this.but_home_las.Text = "Ноль";
            this.but_home_las.UseVisualStyleBackColor = true;
            this.but_home_las.Click += new System.EventHandler(this.but_home_las_Click);
            // 
            // but_div_disp
            // 
            this.but_div_disp.Location = new System.Drawing.Point(451, 87);
            this.but_div_disp.Name = "but_div_disp";
            this.but_div_disp.Size = new System.Drawing.Size(110, 55);
            this.but_div_disp.TabIndex = 135;
            this.but_div_disp.Text = "Установить скорость";
            this.but_div_disp.UseVisualStyleBackColor = true;
            this.but_div_disp.Click += new System.EventHandler(this.but_div_disp_Click);
            // 
            // tb_div_disp
            // 
            this.tb_div_disp.Location = new System.Drawing.Point(562, 101);
            this.tb_div_disp.Name = "tb_div_disp";
            this.tb_div_disp.Size = new System.Drawing.Size(71, 26);
            this.tb_div_disp.TabIndex = 134;
            this.tb_div_disp.Text = "0";
            // 
            // but_las_enc
            // 
            this.but_las_enc.Location = new System.Drawing.Point(171, 29);
            this.but_las_enc.Name = "but_las_enc";
            this.but_las_enc.Size = new System.Drawing.Size(110, 46);
            this.but_las_enc.TabIndex = 133;
            this.but_las_enc.Text = "Позиция";
            this.but_las_enc.UseVisualStyleBackColor = true;
            this.but_las_enc.Click += new System.EventHandler(this.but_las_enc_Click);
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(295, 3);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(49, 20);
            this.label20.TabIndex = 132;
            this.label20.Text = "Laser";
            // 
            // butset_kvp
            // 
            this.butset_kvp.Location = new System.Drawing.Point(22, 302);
            this.butset_kvp.Name = "butset_kvp";
            this.butset_kvp.Size = new System.Drawing.Size(110, 55);
            this.butset_kvp.TabIndex = 125;
            this.butset_kvp.Text = "Установить kvp";
            this.butset_kvp.UseVisualStyleBackColor = true;
            this.butset_kvp.Click += new System.EventHandler(this.butset_kvp_Click);
            // 
            // textBox_set_kvp
            // 
            this.textBox_set_kvp.Location = new System.Drawing.Point(138, 316);
            this.textBox_set_kvp.Name = "textBox_set_kvp";
            this.textBox_set_kvp.Size = new System.Drawing.Size(70, 26);
            this.textBox_set_kvp.TabIndex = 124;
            this.textBox_set_kvp.Text = "100";
            // 
            // but_set_kpp
            // 
            this.but_set_kpp.Location = new System.Drawing.Point(22, 223);
            this.but_set_kpp.Name = "but_set_kpp";
            this.but_set_kpp.Size = new System.Drawing.Size(110, 55);
            this.but_set_kpp.TabIndex = 123;
            this.but_set_kpp.Text = "Установить kpp";
            this.but_set_kpp.UseVisualStyleBackColor = true;
            this.but_set_kpp.Click += new System.EventHandler(this.but_set_kpp_Click);
            // 
            // textBox_set_kpp
            // 
            this.textBox_set_kpp.Location = new System.Drawing.Point(138, 237);
            this.textBox_set_kpp.Name = "textBox_set_kpp";
            this.textBox_set_kpp.Size = new System.Drawing.Size(70, 26);
            this.textBox_set_kpp.TabIndex = 122;
            this.textBox_set_kpp.Text = "20";
            // 
            // but_laser_dest
            // 
            this.but_laser_dest.Location = new System.Drawing.Point(211, 223);
            this.but_laser_dest.Name = "but_laser_dest";
            this.but_laser_dest.Size = new System.Drawing.Size(110, 55);
            this.but_laser_dest.TabIndex = 121;
            this.but_laser_dest.Text = "Установить дистанцию";
            this.but_laser_dest.UseVisualStyleBackColor = true;
            this.but_laser_dest.Click += new System.EventHandler(this.but_laser_dest_Click);
            // 
            // textBox_laser_dest
            // 
            this.textBox_laser_dest.Location = new System.Drawing.Point(327, 237);
            this.textBox_laser_dest.Name = "textBox_laser_dest";
            this.textBox_laser_dest.Size = new System.Drawing.Size(70, 26);
            this.textBox_laser_dest.TabIndex = 120;
            this.textBox_laser_dest.Text = "3500";
            // 
            // but_setShvpVel
            // 
            this.but_setShvpVel.Location = new System.Drawing.Point(216, 302);
            this.but_setShvpVel.Name = "but_setShvpVel";
            this.but_setShvpVel.Size = new System.Drawing.Size(110, 55);
            this.but_setShvpVel.TabIndex = 119;
            this.but_setShvpVel.Text = "Установить скорость";
            this.but_setShvpVel.UseVisualStyleBackColor = true;
            this.but_setShvpVel.Click += new System.EventHandler(this.but_setShvpVel_Click);
            // 
            // textBox_shvpVel
            // 
            this.textBox_shvpVel.Location = new System.Drawing.Point(327, 316);
            this.textBox_shvpVel.Name = "textBox_shvpVel";
            this.textBox_shvpVel.Size = new System.Drawing.Size(71, 26);
            this.textBox_shvpVel.TabIndex = 118;
            this.textBox_shvpVel.Text = "0";
            // 
            // but_marl_receav
            // 
            this.but_marl_receav.Location = new System.Drawing.Point(139, 379);
            this.but_marl_receav.Name = "but_marl_receav";
            this.but_marl_receav.Size = new System.Drawing.Size(116, 38);
            this.but_marl_receav.TabIndex = 117;
            this.but_marl_receav.Text = "Принять";
            this.but_marl_receav.UseVisualStyleBackColor = true;
            this.but_marl_receav.Click += new System.EventHandler(this.but_marl_receav_Click);
            // 
            // but_marl_close
            // 
            this.but_marl_close.Location = new System.Drawing.Point(261, 423);
            this.but_marl_close.Name = "but_marl_close";
            this.but_marl_close.Size = new System.Drawing.Size(143, 38);
            this.but_marl_close.TabIndex = 116;
            this.but_marl_close.Text = "Отключиться";
            this.but_marl_close.UseVisualStyleBackColor = true;
            this.but_marl_close.Click += new System.EventHandler(this.but_marl_close_Click);
            // 
            // but_marl_open
            // 
            this.but_marl_open.Location = new System.Drawing.Point(261, 379);
            this.but_marl_open.Name = "but_marl_open";
            this.but_marl_open.Size = new System.Drawing.Size(143, 38);
            this.but_marl_open.TabIndex = 115;
            this.but_marl_open.Text = "Подключиться";
            this.but_marl_open.UseVisualStyleBackColor = true;
            this.but_marl_open.Click += new System.EventHandler(this.but_marl_open_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 379);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 20);
            this.label2.TabIndex = 114;
            this.label2.Text = "Marlin";
            // 
            // textBox_marl_shcpPos
            // 
            this.textBox_marl_shcpPos.Location = new System.Drawing.Point(138, 429);
            this.textBox_marl_shcpPos.Name = "textBox_marl_shcpPos";
            this.textBox_marl_shcpPos.Size = new System.Drawing.Size(110, 26);
            this.textBox_marl_shcpPos.TabIndex = 11;
            this.textBox_marl_shcpPos.Text = "0";
            // 
            // but_marl_setShvpPos
            // 
            this.but_marl_setShvpPos.Location = new System.Drawing.Point(22, 406);
            this.but_marl_setShvpPos.Name = "but_marl_setShvpPos";
            this.but_marl_setShvpPos.Size = new System.Drawing.Size(110, 55);
            this.but_marl_setShvpPos.TabIndex = 10;
            this.but_marl_setShvpPos.Text = "Установить положение";
            this.but_marl_setShvpPos.UseVisualStyleBackColor = true;
            this.but_marl_setShvpPos.Click += new System.EventHandler(this.but_marl_setShvpPos_Click);
            // 
            // but_setShvpPos
            // 
            this.but_setShvpPos.Location = new System.Drawing.Point(171, 152);
            this.but_setShvpPos.Name = "but_setShvpPos";
            this.but_setShvpPos.Size = new System.Drawing.Size(110, 55);
            this.but_setShvpPos.TabIndex = 9;
            this.but_setShvpPos.Text = "Установить положение";
            this.but_setShvpPos.UseVisualStyleBackColor = true;
            this.but_setShvpPos.Click += new System.EventHandler(this.but_setShvpPos_Click);
            // 
            // textBox_shvpPos
            // 
            this.textBox_shvpPos.Location = new System.Drawing.Point(287, 166);
            this.textBox_shvpPos.Name = "textBox_shvpPos";
            this.textBox_shvpPos.Size = new System.Drawing.Size(110, 26);
            this.textBox_shvpPos.TabIndex = 8;
            this.textBox_shvpPos.Text = "0";
            // 
            // but_open
            // 
            this.but_open.Location = new System.Drawing.Point(22, 122);
            this.but_open.Name = "but_open";
            this.but_open.Size = new System.Drawing.Size(143, 38);
            this.but_open.TabIndex = 0;
            this.but_open.Text = "Подключиться";
            this.but_open.UseVisualStyleBackColor = true;
            this.but_open.Click += new System.EventHandler(this.but_open_Click);
            // 
            // but_close
            // 
            this.but_close.Location = new System.Drawing.Point(22, 166);
            this.but_close.Name = "but_close";
            this.but_close.Size = new System.Drawing.Size(143, 36);
            this.but_close.TabIndex = 1;
            this.but_close.Text = "Отключиться";
            this.but_close.UseVisualStyleBackColor = true;
            this.but_close.Click += new System.EventHandler(this.but_close_Click);
            // 
            // but_laserOn
            // 
            this.but_laserOn.Location = new System.Drawing.Point(287, 26);
            this.but_laserOn.Name = "but_laserOn";
            this.but_laserOn.Size = new System.Drawing.Size(110, 26);
            this.but_laserOn.TabIndex = 2;
            this.but_laserOn.Text = "Включить лазер";
            this.but_laserOn.UseVisualStyleBackColor = true;
            this.but_laserOn.Click += new System.EventHandler(this.but_laserOn_Click);
            // 
            // but_laserOff
            // 
            this.but_laserOff.Location = new System.Drawing.Point(287, 55);
            this.but_laserOff.Name = "but_laserOff";
            this.but_laserOff.Size = new System.Drawing.Size(110, 26);
            this.but_laserOff.TabIndex = 3;
            this.but_laserOff.Text = "Выключить лазер";
            this.but_laserOff.UseVisualStyleBackColor = true;
            this.but_laserOff.Click += new System.EventHandler(this.but_laserOff_Click);
            // 
            // but_setPower
            // 
            this.but_setPower.Location = new System.Drawing.Point(171, 87);
            this.but_setPower.Name = "but_setPower";
            this.but_setPower.Size = new System.Drawing.Size(110, 50);
            this.but_setPower.TabIndex = 4;
            this.but_setPower.Text = "Установить мощность";
            this.but_setPower.UseVisualStyleBackColor = true;
            this.but_setPower.Click += new System.EventHandler(this.but_setPower_Click);
            // 
            // comboBox_portsArd
            // 
            this.comboBox_portsArd.FormattingEnabled = true;
            this.comboBox_portsArd.Location = new System.Drawing.Point(22, 74);
            this.comboBox_portsArd.Name = "comboBox_portsArd";
            this.comboBox_portsArd.Size = new System.Drawing.Size(143, 28);
            this.comboBox_portsArd.TabIndex = 6;
            this.comboBox_portsArd.SelectedIndexChanged += new System.EventHandler(this.comboBox_portsArd_SelectedIndexChanged);
            // 
            // but_find_ports
            // 
            this.but_find_ports.ImageAlign = System.Drawing.ContentAlignment.TopCenter;
            this.but_find_ports.Location = new System.Drawing.Point(22, 32);
            this.but_find_ports.Name = "but_find_ports";
            this.but_find_ports.Size = new System.Drawing.Size(143, 36);
            this.but_find_ports.TabIndex = 7;
            this.but_find_ports.Text = "Найти порты";
            this.but_find_ports.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.but_find_ports.UseVisualStyleBackColor = true;
            this.but_find_ports.Click += new System.EventHandler(this.but_find_ports_Click);
            // 
            // textBox_powerLaser
            // 
            this.textBox_powerLaser.Location = new System.Drawing.Point(288, 101);
            this.textBox_powerLaser.Name = "textBox_powerLaser";
            this.textBox_powerLaser.Size = new System.Drawing.Size(110, 26);
            this.textBox_powerLaser.TabIndex = 5;
            this.textBox_powerLaser.Text = "0";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label55.Location = new System.Drawing.Point(1296, 504);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(115, 20);
            this.label55.TabIndex = 120;
            this.label55.Text = "Name Photo ->";
            // 
            // txBx_photoName
            // 
            this.txBx_photoName.Location = new System.Drawing.Point(1425, 501);
            this.txBx_photoName.Name = "txBx_photoName";
            this.txBx_photoName.Size = new System.Drawing.Size(103, 26);
            this.txBx_photoName.TabIndex = 119;
            this.txBx_photoName.Text = "photo_13";
            // 
            // nameC2
            // 
            this.nameC2.Location = new System.Drawing.Point(1004, 555);
            this.nameC2.Name = "nameC2";
            this.nameC2.Size = new System.Drawing.Size(69, 26);
            this.nameC2.TabIndex = 88;
            this.nameC2.Text = "230";
            // 
            // nameB2
            // 
            this.nameB2.Location = new System.Drawing.Point(929, 555);
            this.nameB2.Name = "nameB2";
            this.nameB2.Size = new System.Drawing.Size(69, 26);
            this.nameB2.TabIndex = 87;
            this.nameB2.Text = "100";
            // 
            // textNimVid
            // 
            this.textNimVid.Location = new System.Drawing.Point(1619, 0);
            this.textNimVid.Name = "textNimVid";
            this.textNimVid.Size = new System.Drawing.Size(45, 26);
            this.textNimVid.TabIndex = 40;
            this.textNimVid.Text = "1";
            this.textNimVid.DoubleClick += new System.EventHandler(this.videoStart_Click);
            // 
            // nameA2
            // 
            this.nameA2.Location = new System.Drawing.Point(854, 555);
            this.nameA2.Name = "nameA2";
            this.nameA2.Size = new System.Drawing.Size(69, 26);
            this.nameA2.TabIndex = 86;
            this.nameA2.Text = "550";
            // 
            // nameX
            // 
            this.nameX.Location = new System.Drawing.Point(630, 522);
            this.nameX.Name = "nameX";
            this.nameX.Size = new System.Drawing.Size(69, 26);
            this.nameX.TabIndex = 11;
            this.nameX.Text = "0";
            // 
            // nameY
            // 
            this.nameY.Location = new System.Drawing.Point(705, 522);
            this.nameY.Name = "nameY";
            this.nameY.Size = new System.Drawing.Size(69, 26);
            this.nameY.TabIndex = 12;
            this.nameY.Text = "100";
            // 
            // nameC_in
            // 
            this.nameC_in.Location = new System.Drawing.Point(1004, 493);
            this.nameC_in.Name = "nameC_in";
            this.nameC_in.Size = new System.Drawing.Size(69, 26);
            this.nameC_in.TabIndex = 78;
            this.nameC_in.Text = "230";
            // 
            // nameZ
            // 
            this.nameZ.Location = new System.Drawing.Point(780, 522);
            this.nameZ.Name = "nameZ";
            this.nameZ.Size = new System.Drawing.Size(69, 26);
            this.nameZ.TabIndex = 13;
            this.nameZ.Text = "230";
            // 
            // nameB_in
            // 
            this.nameB_in.Location = new System.Drawing.Point(929, 493);
            this.nameB_in.Name = "nameB_in";
            this.nameB_in.Size = new System.Drawing.Size(69, 26);
            this.nameB_in.TabIndex = 77;
            this.nameB_in.Text = "100";
            // 
            // nameA_in
            // 
            this.nameA_in.Location = new System.Drawing.Point(854, 493);
            this.nameA_in.Name = "nameA_in";
            this.nameA_in.Size = new System.Drawing.Size(69, 26);
            this.nameA_in.TabIndex = 76;
            this.nameA_in.Text = "550";
            // 
            // nameX2
            // 
            this.nameX2.Location = new System.Drawing.Point(630, 555);
            this.nameX2.Name = "nameX2";
            this.nameX2.Size = new System.Drawing.Size(69, 26);
            this.nameX2.TabIndex = 25;
            this.nameX2.Text = "1";
            // 
            // nameZ_in
            // 
            this.nameZ_in.Location = new System.Drawing.Point(780, 493);
            this.nameZ_in.Name = "nameZ_in";
            this.nameZ_in.Size = new System.Drawing.Size(69, 26);
            this.nameZ_in.TabIndex = 72;
            this.nameZ_in.Text = "230";
            // 
            // nameY2
            // 
            this.nameY2.Location = new System.Drawing.Point(705, 555);
            this.nameY2.Name = "nameY2";
            this.nameY2.Size = new System.Drawing.Size(69, 26);
            this.nameY2.TabIndex = 26;
            this.nameY2.Text = "100";
            // 
            // nameY_in
            // 
            this.nameY_in.Location = new System.Drawing.Point(705, 493);
            this.nameY_in.Name = "nameY_in";
            this.nameY_in.Size = new System.Drawing.Size(69, 26);
            this.nameY_in.TabIndex = 71;
            this.nameY_in.Text = "100";
            // 
            // nameZ2
            // 
            this.nameZ2.Location = new System.Drawing.Point(780, 555);
            this.nameZ2.Name = "nameZ2";
            this.nameZ2.Size = new System.Drawing.Size(69, 26);
            this.nameZ2.TabIndex = 27;
            this.nameZ2.Text = "230";
            // 
            // nameX_in
            // 
            this.nameX_in.Location = new System.Drawing.Point(630, 493);
            this.nameX_in.Name = "nameX_in";
            this.nameX_in.Size = new System.Drawing.Size(69, 26);
            this.nameX_in.TabIndex = 70;
            this.nameX_in.Text = "550";
            // 
            // boxN
            // 
            this.boxN.Location = new System.Drawing.Point(1425, 448);
            this.boxN.Name = "boxN";
            this.boxN.Size = new System.Drawing.Size(69, 26);
            this.boxN.TabIndex = 31;
            this.boxN.Text = "10";
            // 
            // nameC
            // 
            this.nameC.Location = new System.Drawing.Point(1004, 522);
            this.nameC.Name = "nameC";
            this.nameC.Size = new System.Drawing.Size(69, 26);
            this.nameC.TabIndex = 63;
            this.nameC.Text = "230";
            // 
            // nameB
            // 
            this.nameB.Location = new System.Drawing.Point(929, 522);
            this.nameB.Name = "nameB";
            this.nameB.Size = new System.Drawing.Size(69, 26);
            this.nameB.TabIndex = 62;
            this.nameB.Text = "100";
            // 
            // nameA
            // 
            this.nameA.Location = new System.Drawing.Point(854, 522);
            this.nameA.Name = "nameA";
            this.nameA.Size = new System.Drawing.Size(69, 26);
            this.nameA.TabIndex = 61;
            this.nameA.Text = "550";
            // 
            // box_scanFolder
            // 
            this.box_scanFolder.Location = new System.Drawing.Point(1425, 372);
            this.box_scanFolder.Name = "box_scanFolder";
            this.box_scanFolder.Size = new System.Drawing.Size(130, 26);
            this.box_scanFolder.TabIndex = 57;
            this.box_scanFolder.Text = "test";
            // 
            // box_photoFolder
            // 
            this.box_photoFolder.Location = new System.Drawing.Point(1425, 403);
            this.box_photoFolder.Name = "box_photoFolder";
            this.box_photoFolder.Size = new System.Drawing.Size(130, 26);
            this.box_photoFolder.TabIndex = 59;
            this.box_photoFolder.Text = "test";
            // 
            // imBox_base_2
            // 
            this.imBox_base_2.Location = new System.Drawing.Point(604, 607);
            this.imBox_base_2.Name = "imBox_base_2";
            this.imBox_base_2.Size = new System.Drawing.Size(480, 360);
            this.imBox_base_2.TabIndex = 118;
            this.imBox_base_2.TabStop = false;
            // 
            // imBox_base_1
            // 
            this.imBox_base_1.Location = new System.Drawing.Point(118, 607);
            this.imBox_base_1.Name = "imBox_base_1";
            this.imBox_base_1.Size = new System.Drawing.Size(480, 360);
            this.imBox_base_1.TabIndex = 117;
            this.imBox_base_1.TabStop = false;
            // 
            // but_ph
            // 
            this.but_ph.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_ph.Location = new System.Drawing.Point(20, 483);
            this.but_ph.Name = "but_ph";
            this.but_ph.Size = new System.Drawing.Size(106, 33);
            this.but_ph.TabIndex = 116;
            this.but_ph.Text = "photo";
            this.but_ph.UseVisualStyleBackColor = true;
            this.but_ph.Click += new System.EventHandler(this.butSaveIm_Click);
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(1619, 21);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(66, 20);
            this.label45.TabIndex = 115;
            this.label45.Text = "minDisp";
            // 
            // trackBar11
            // 
            this.trackBar11.AccessibleName = "10";
            this.trackBar11.Location = new System.Drawing.Point(1688, 475);
            this.trackBar11.Maximum = 20;
            this.trackBar11.Name = "trackBar11";
            this.trackBar11.Size = new System.Drawing.Size(188, 45);
            this.trackBar11.TabIndex = 114;
            this.trackBar11.Value = 10;
            this.trackBar11.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(1587, 489);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(103, 20);
            this.label46.TabIndex = 113;
            this.label46.Text = "specleRange";
            // 
            // trackBar12
            // 
            this.trackBar12.AccessibleName = "9";
            this.trackBar12.Location = new System.Drawing.Point(1688, 424);
            this.trackBar12.Maximum = 20;
            this.trackBar12.Name = "trackBar12";
            this.trackBar12.Size = new System.Drawing.Size(188, 45);
            this.trackBar12.TabIndex = 112;
            this.trackBar12.Value = 10;
            this.trackBar12.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(1601, 437);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(81, 20);
            this.label47.TabIndex = 111;
            this.label47.Text = "specleWS";
            // 
            // trackBar13
            // 
            this.trackBar13.AccessibleName = "8";
            this.trackBar13.Location = new System.Drawing.Point(1688, 372);
            this.trackBar13.Maximum = 20;
            this.trackBar13.Name = "trackBar13";
            this.trackBar13.Size = new System.Drawing.Size(188, 45);
            this.trackBar13.TabIndex = 110;
            this.trackBar13.Value = 10;
            this.trackBar13.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label48
            // 
            this.label48.AutoSize = true;
            this.label48.Location = new System.Drawing.Point(1631, 385);
            this.label48.Name = "label48";
            this.label48.Size = new System.Drawing.Size(54, 20);
            this.label48.TabIndex = 109;
            this.label48.Text = "unuqe";
            // 
            // trackBar14
            // 
            this.trackBar14.AccessibleName = "7";
            this.trackBar14.Location = new System.Drawing.Point(1688, 321);
            this.trackBar14.Maximum = 20;
            this.trackBar14.Name = "trackBar14";
            this.trackBar14.Size = new System.Drawing.Size(188, 45);
            this.trackBar14.TabIndex = 108;
            this.trackBar14.Value = 10;
            this.trackBar14.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(1621, 332);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(62, 20);
            this.label49.TabIndex = 107;
            this.label49.Text = "prefilter";
            // 
            // trackBar15
            // 
            this.trackBar15.AccessibleName = "6";
            this.trackBar15.Location = new System.Drawing.Point(1688, 270);
            this.trackBar15.Maximum = 20;
            this.trackBar15.Name = "trackBar15";
            this.trackBar15.Size = new System.Drawing.Size(188, 45);
            this.trackBar15.TabIndex = 106;
            this.trackBar15.Value = 10;
            this.trackBar15.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(1575, 282);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(110, 20);
            this.label50.TabIndex = 105;
            this.label50.Text = "Disp12maxdiff";
            // 
            // trackBar16
            // 
            this.trackBar16.AccessibleName = "5";
            this.trackBar16.Location = new System.Drawing.Point(1688, 219);
            this.trackBar16.Maximum = 60;
            this.trackBar16.Name = "trackBar16";
            this.trackBar16.Size = new System.Drawing.Size(188, 45);
            this.trackBar16.TabIndex = 104;
            this.trackBar16.Value = 10;
            this.trackBar16.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(1656, 235);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(27, 20);
            this.label51.TabIndex = 103;
            this.label51.Text = "p2";
            // 
            // trackBar17
            // 
            this.trackBar17.AccessibleName = "4";
            this.trackBar17.Location = new System.Drawing.Point(1688, 168);
            this.trackBar17.Maximum = 60;
            this.trackBar17.Name = "trackBar17";
            this.trackBar17.Size = new System.Drawing.Size(188, 45);
            this.trackBar17.TabIndex = 102;
            this.trackBar17.Value = 10;
            this.trackBar17.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Location = new System.Drawing.Point(1656, 181);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(27, 20);
            this.label52.TabIndex = 101;
            this.label52.Text = "p1";
            // 
            // trackBar18
            // 
            this.trackBar18.AccessibleName = "3";
            this.trackBar18.Location = new System.Drawing.Point(1688, 117);
            this.trackBar18.Maximum = 20;
            this.trackBar18.Name = "trackBar18";
            this.trackBar18.Size = new System.Drawing.Size(188, 45);
            this.trackBar18.TabIndex = 100;
            this.trackBar18.Value = 10;
            this.trackBar18.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Location = new System.Drawing.Point(1613, 135);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(77, 20);
            this.label53.TabIndex = 99;
            this.label53.Text = "blockSize";
            // 
            // trackBar19
            // 
            this.trackBar19.AccessibleName = "2";
            this.trackBar19.Location = new System.Drawing.Point(1688, 66);
            this.trackBar19.Maximum = 20;
            this.trackBar19.Name = "trackBar19";
            this.trackBar19.Size = new System.Drawing.Size(188, 45);
            this.trackBar19.TabIndex = 98;
            this.trackBar19.Value = 2;
            this.trackBar19.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(1613, 80);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(72, 20);
            this.label54.TabIndex = 97;
            this.label54.Text = "manDisp";
            // 
            // trackBar20
            // 
            this.trackBar20.AccessibleName = "1";
            this.trackBar20.Location = new System.Drawing.Point(1688, 15);
            this.trackBar20.Maximum = 20;
            this.trackBar20.Name = "trackBar20";
            this.trackBar20.Size = new System.Drawing.Size(188, 45);
            this.trackBar20.TabIndex = 96;
            this.trackBar20.Value = 1;
            this.trackBar20.Scroll += new System.EventHandler(this.trB_SGBM_Scroll);
            // 
            // but_addBufRob
            // 
            this.but_addBufRob.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_addBufRob.Location = new System.Drawing.Point(1550, 67);
            this.but_addBufRob.Name = "but_addBufRob";
            this.but_addBufRob.Size = new System.Drawing.Size(106, 33);
            this.but_addBufRob.TabIndex = 93;
            this.but_addBufRob.Text = "add buf robot";
            this.but_addBufRob.UseVisualStyleBackColor = true;
            this.but_addBufRob.Click += new System.EventHandler(this.but_addBufRob_Click);
            // 
            // but_robMod
            // 
            this.but_robMod.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_robMod.Location = new System.Drawing.Point(1550, 27);
            this.but_robMod.Name = "but_robMod";
            this.but_robMod.Size = new System.Drawing.Size(106, 33);
            this.but_robMod.TabIndex = 92;
            this.but_robMod.Text = "Start virtual robot";
            this.but_robMod.UseVisualStyleBackColor = true;
            this.but_robMod.Click += new System.EventHandler(this.but_robMod_Click);
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(1022, 584);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(20, 20);
            this.label24.TabIndex = 91;
            this.label24.Text = "C";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(950, 584);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(20, 20);
            this.label25.TabIndex = 90;
            this.label25.Text = "B";
            // 
            // imageBox1
            // 
            this.imageBox1.Location = new System.Drawing.Point(3, 3);
            this.imageBox1.Name = "imageBox1";
            this.imageBox1.Size = new System.Drawing.Size(640, 480);
            this.imageBox1.TabIndex = 2;
            this.imageBox1.TabStop = false;
            // 
            // butStop
            // 
            this.butStop.Location = new System.Drawing.Point(996, 973);
            this.butStop.Name = "butStop";
            this.butStop.Size = new System.Drawing.Size(80, 42);
            this.butStop.TabIndex = 9;
            this.butStop.Text = "Закончить запись";
            this.butStop.UseVisualStyleBackColor = true;
            this.butStop.Click += new System.EventHandler(this.butStop_Click);
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(872, 584);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(20, 20);
            this.label26.TabIndex = 89;
            this.label26.Text = "A";
            // 
            // butStart
            // 
            this.butStart.Location = new System.Drawing.Point(914, 973);
            this.butStart.Name = "butStart";
            this.butStart.Size = new System.Drawing.Size(76, 42);
            this.butStart.TabIndex = 8;
            this.butStart.Text = "Начать запись";
            this.butStart.UseVisualStyleBackColor = true;
            this.butStart.Click += new System.EventHandler(this.butStart_Click);
            // 
            // imageBox2
            // 
            this.imageBox2.Location = new System.Drawing.Point(649, 3);
            this.imageBox2.Name = "imageBox2";
            this.imageBox2.Size = new System.Drawing.Size(640, 480);
            this.imageBox2.TabIndex = 18;
            this.imageBox2.TabStop = false;
            // 
            // videoCapt
            // 
            this.videoCapt.Location = new System.Drawing.Point(802, 973);
            this.videoCapt.Name = "videoCapt";
            this.videoCapt.Size = new System.Drawing.Size(106, 38);
            this.videoCapt.TabIndex = 1;
            this.videoCapt.Text = "съёмка";
            this.videoCapt.UseVisualStyleBackColor = true;
            this.videoCapt.Click += new System.EventHandler(this.videoCapt_Click);
            // 
            // rob_res
            // 
            this.rob_res.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rob_res.Location = new System.Drawing.Point(1295, 173);
            this.rob_res.Name = "rob_res";
            this.rob_res.Size = new System.Drawing.Size(106, 33);
            this.rob_res.TabIndex = 44;
            this.rob_res.Text = "Reseive robot";
            this.rob_res.UseVisualStyleBackColor = true;
            this.rob_res.Click += new System.EventHandler(this.rob_res_Click);
            // 
            // rob_con
            // 
            this.rob_con.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.rob_con.Location = new System.Drawing.Point(1295, 41);
            this.rob_con.Name = "rob_con";
            this.rob_con.Size = new System.Drawing.Size(106, 33);
            this.rob_con.TabIndex = 41;
            this.rob_con.Text = "Connect robot";
            this.rob_con.UseVisualStyleBackColor = true;
            this.rob_con.Click += new System.EventHandler(this.rob_con_Click);
            // 
            // but_res_pos_2
            // 
            this.but_res_pos_2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_res_pos_2.Location = new System.Drawing.Point(1082, 554);
            this.but_res_pos_2.Name = "but_res_pos_2";
            this.but_res_pos_2.Size = new System.Drawing.Size(106, 27);
            this.but_res_pos_2.TabIndex = 85;
            this.but_res_pos_2.Text = "Reseive";
            this.but_res_pos_2.UseVisualStyleBackColor = true;
            this.but_res_pos_2.Click += new System.EventHandler(this.but_res_pos_2_Click);
            // 
            // disc_rob
            // 
            this.disc_rob.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.disc_rob.Location = new System.Drawing.Point(1295, 85);
            this.disc_rob.Name = "disc_rob";
            this.disc_rob.Size = new System.Drawing.Size(106, 33);
            this.disc_rob.TabIndex = 42;
            this.disc_rob.Text = "Disconnect robot";
            this.disc_rob.UseVisualStyleBackColor = true;
            this.disc_rob.Click += new System.EventHandler(this.rob_discon_Click);
            // 
            // but_res_pos1
            // 
            this.but_res_pos1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_res_pos1.Location = new System.Drawing.Point(1082, 522);
            this.but_res_pos1.Name = "but_res_pos1";
            this.but_res_pos1.Size = new System.Drawing.Size(106, 26);
            this.but_res_pos1.TabIndex = 84;
            this.but_res_pos1.Text = "Reseive";
            this.but_res_pos1.UseVisualStyleBackColor = true;
            this.but_res_pos1.Click += new System.EventHandler(this.but_res_pos1_Click);
            // 
            // send_rob
            // 
            this.send_rob.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.send_rob.Location = new System.Drawing.Point(1295, 129);
            this.send_rob.Name = "send_rob";
            this.send_rob.Size = new System.Drawing.Size(106, 33);
            this.send_rob.TabIndex = 43;
            this.send_rob.Text = "Send robot";
            this.send_rob.UseVisualStyleBackColor = true;
            this.send_rob.Click += new System.EventHandler(this.rob_send_Click);
            // 
            // bet_res_pos
            // 
            this.bet_res_pos.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.bet_res_pos.Location = new System.Drawing.Point(1082, 493);
            this.bet_res_pos.Name = "bet_res_pos";
            this.bet_res_pos.Size = new System.Drawing.Size(106, 26);
            this.bet_res_pos.TabIndex = 83;
            this.bet_res_pos.Text = "Reseive";
            this.bet_res_pos.UseVisualStyleBackColor = true;
            this.bet_res_pos.Click += new System.EventHandler(this.bet_res_pos_Click);
            // 
            // but_scan_start
            // 
            this.but_scan_start.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_scan_start.Location = new System.Drawing.Point(1295, 217);
            this.but_scan_start.Name = "but_scan_start";
            this.but_scan_start.Size = new System.Drawing.Size(106, 33);
            this.but_scan_start.TabIndex = 45;
            this.but_scan_start.Text = "Scan rob";
            this.but_scan_start.UseVisualStyleBackColor = true;
            this.but_scan_start.Click += new System.EventHandler(this.startScan);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label17.Location = new System.Drawing.Point(481, 496);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(104, 20);
            this.label17.TabIndex = 82;
            this.label17.Text = "Input Point ->";
            // 
            // but_photo
            // 
            this.but_photo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_photo.Location = new System.Drawing.Point(1295, 256);
            this.but_photo.Name = "but_photo";
            this.but_photo.Size = new System.Drawing.Size(106, 33);
            this.but_photo.TabIndex = 56;
            this.but_photo.Text = "Robot photo";
            this.but_photo.UseVisualStyleBackColor = true;
            this.but_photo.Click += new System.EventHandler(this.but_photo_Click);
            // 
            // butCalcIm
            // 
            this.butCalcIm.Location = new System.Drawing.Point(1082, 977);
            this.butCalcIm.Name = "butCalcIm";
            this.butCalcIm.Size = new System.Drawing.Size(106, 38);
            this.butCalcIm.TabIndex = 35;
            this.butCalcIm.Text = "Расчёт";
            this.butCalcIm.UseVisualStyleBackColor = true;
            this.butCalcIm.Click += new System.EventHandler(this.butCalcIm_Click);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(648, 584);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(20, 20);
            this.label7.TabIndex = 28;
            this.label7.Text = "X";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label16.Location = new System.Drawing.Point(1296, 451);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(123, 20);
            this.label16.TabIndex = 69;
            this.label16.Text = "Number Point ->";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(726, 584);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(20, 20);
            this.label6.TabIndex = 29;
            this.label6.Text = "Y";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label15.Location = new System.Drawing.Point(481, 558);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(142, 20);
            this.label15.TabIndex = 68;
            this.label15.Text = "Stop Scan Point ->";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(798, 584);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(19, 20);
            this.label5.TabIndex = 30;
            this.label5.Text = "Z";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label14.Location = new System.Drawing.Point(481, 525);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(143, 20);
            this.label14.TabIndex = 67;
            this.label14.Text = "Start Scan Point ->";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(1500, 451);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(20, 20);
            this.label8.TabIndex = 32;
            this.label8.Text = "N";
            // 
            // comboVideo
            // 
            this.comboVideo.FormattingEnabled = true;
            this.comboVideo.Location = new System.Drawing.Point(20, 519);
            this.comboVideo.Name = "comboVideo";
            this.comboVideo.Size = new System.Drawing.Size(431, 28);
            this.comboVideo.TabIndex = 36;
            this.comboVideo.SelectedIndexChanged += new System.EventHandler(this.comboVideo_SelectedIndexChanged);
            // 
            // comboNumber
            // 
            this.comboNumber.FormattingEnabled = true;
            this.comboNumber.Location = new System.Drawing.Point(20, 546);
            this.comboNumber.Name = "comboNumber";
            this.comboNumber.Size = new System.Drawing.Size(431, 28);
            this.comboNumber.TabIndex = 37;
            this.comboNumber.SelectedIndexChanged += new System.EventHandler(this.comboNumber_SelectedIndexChanged);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label10.Location = new System.Drawing.Point(1300, 406);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(118, 20);
            this.label10.TabIndex = 60;
            this.label10.Text = "Photo Folder ->";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.label9.Location = new System.Drawing.Point(1300, 375);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(113, 20);
            this.label9.TabIndex = 58;
            this.label9.Text = "Scan Folder ->";
            // 
            // imBox_base
            // 
            this.imBox_base.Location = new System.Drawing.Point(6, 607);
            this.imBox_base.Name = "imBox_base";
            this.imBox_base.Size = new System.Drawing.Size(480, 360);
            this.imBox_base.TabIndex = 94;
            this.imBox_base.TabStop = false;
            // 
            // but_resize
            // 
            this.but_resize.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.but_resize.Location = new System.Drawing.Point(508, -11);
            this.but_resize.Name = "but_resize";
            this.but_resize.Size = new System.Drawing.Size(106, 33);
            this.but_resize.TabIndex = 136;
            this.but_resize.Text = "Resize";
            this.but_resize.UseVisualStyleBackColor = true;
            this.but_resize.Click += new System.EventHandler(this.but_resize_Click);
            // 
            // windowsTabs
            // 
            this.windowsTabs.Controls.Add(this.tabMain);
            this.windowsTabs.Controls.Add(this.tabOpenGl);
            this.windowsTabs.Controls.Add(this.tabDebug);
            this.windowsTabs.Controls.Add(this.tabDistort);
            this.windowsTabs.Controls.Add(this.tabCalibMonit);
            this.windowsTabs.Location = new System.Drawing.Point(12, 0);
            this.windowsTabs.Name = "windowsTabs";
            this.windowsTabs.SelectedIndex = 0;
            this.windowsTabs.Size = new System.Drawing.Size(1890, 1029);
            this.windowsTabs.TabIndex = 92;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 10;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // MainScanningForm
            // 
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.but_resize);
            this.Controls.Add(this.comboImages);
            this.Controls.Add(this.windowsTabs);
            this.Name = "MainScanningForm";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.MainScanningForm_Load);
            this.tabCalibMonit.ResumeLayout(false);
            this.tabCalibMonit.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar27)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar28)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar29)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar24)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar25)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar26)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar23)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar22)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar21)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_input_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_input_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_pattern)).EndInit();
            this.tabDistort.ResumeLayout(false);
            this.tabDistort.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_debug2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_debug1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox_cameraDist)).EndInit();
            this.tabDebug.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.imageBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.histogramBox1)).EndInit();
            this.tabOpenGl.ResumeLayout(false);
            this.tabOpenGl.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_3dDebug)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_disparity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_mark2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_mark1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOz)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackY_light)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackZ_light)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOx)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackX_light)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackOy)).EndInit();
            this.tabMain.ResumeLayout(false);
            this.tabMain.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_base_2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_base_1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar13)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar14)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar15)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar16)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar17)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar18)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar19)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar20)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.imBox_base)).EndInit();
            this.windowsTabs.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.graphicGLBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.ComboBox comboImages;
        private System.Windows.Forms.BindingSource graphicGLBindingSource;
        private System.Windows.Forms.TabPage tabCalibMonit;
        private System.Windows.Forms.Label lab_pos_mouse;
        private System.Windows.Forms.Button but_set_wind;
        private System.Windows.Forms.Button but_ph_1;
        private System.Windows.Forms.Button but_calib_Start;
        private System.Windows.Forms.TextBox textBoxK_8;
        private System.Windows.Forms.TextBox textBoxK_7;
        private System.Windows.Forms.TextBox textBoxK_6;
        private System.Windows.Forms.TextBox textBoxK_5;
        private System.Windows.Forms.TextBox textBoxK_4;
        private System.Windows.Forms.TextBox textBoxK_3;
        private System.Windows.Forms.TextBox textBoxK_2;
        private System.Windows.Forms.TextBox textBoxK_1;
        private System.Windows.Forms.TextBox textBoxK_0;
        private System.Windows.Forms.TrackBar trackBar27;
        private System.Windows.Forms.TrackBar trackBar28;
        private System.Windows.Forms.TrackBar trackBar29;
        private System.Windows.Forms.TrackBar trackBar24;
        private System.Windows.Forms.TrackBar trackBar25;
        private System.Windows.Forms.TrackBar trackBar26;
        private System.Windows.Forms.TrackBar trackBar23;
        private System.Windows.Forms.TrackBar trackBar22;
        private System.Windows.Forms.TrackBar trackBar21;
        private Emgu.CV.UI.ImageBox imBox_input_2;
        private Emgu.CV.UI.ImageBox imBox_input_1;
        private Emgu.CV.UI.ImageBox imBox_pattern;
        private System.Windows.Forms.TabPage tabDistort;
        private Emgu.CV.UI.ImageBox imBox_debug2;
        private Emgu.CV.UI.ImageBox imBox_debug1;
        private System.Windows.Forms.Label label_corPic;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.TextBox textBox_P2deg;
        private System.Windows.Forms.TextBox textBox_P1deg;
        private System.Windows.Forms.TextBox textBox_K3deg;
        private System.Windows.Forms.TextBox textBox_K2deg;
        private System.Windows.Forms.TextBox textBox_K1deg;
        private System.Windows.Forms.TextBox textBox_P2;
        private System.Windows.Forms.TextBox textBox_P1;
        private System.Windows.Forms.TextBox textBox_K3;
        private System.Windows.Forms.TextBox textBox_K2;
        private System.Windows.Forms.TextBox textBox_K1;
        private System.Windows.Forms.Button but_comp_dist;
        private System.Windows.Forms.Label label32;
        private System.Windows.Forms.Label label31;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label28;
        private Emgu.CV.UI.ImageBox imageBox_cameraDist;
        private System.Windows.Forms.TabPage tabDebug;
        private Emgu.CV.UI.ImageBox imageBox6;
        private Emgu.CV.UI.ImageBox imageBox5;
        private Emgu.CV.UI.ImageBox imageBox3;
        private Emgu.CV.UI.ImageBox imageBox7;
        private Emgu.CV.UI.ImageBox imageBox4;
        private Emgu.CV.UI.ImageBox imageBox8;
        private System.Windows.Forms.PictureBox pictureBox1;
        private Emgu.CV.UI.HistogramBox histogramBox1;
        private System.Windows.Forms.TabPage tabOpenGl;
        private System.Windows.Forms.Label lab_TRZ;
        private System.Windows.Forms.RichTextBox debugBox;
        private System.Windows.Forms.TextBox textBox_monitor_id;
        private OpenGL.GlControl glControl1;
        private System.Windows.Forms.Button but_gl_cam_calib;
        private Emgu.CV.UI.ImageBox imBox_3dDebug;
        private System.Windows.Forms.Button but_SubpixPrec;
        private System.Windows.Forms.Label label44;
        private System.Windows.Forms.TrackBar trackBar10;
        private System.Windows.Forms.Label label43;
        private System.Windows.Forms.TrackBar trackBar9;
        private System.Windows.Forms.Label label42;
        private System.Windows.Forms.TrackBar trackBar8;
        private System.Windows.Forms.Label label41;
        private System.Windows.Forms.TrackBar trackBar7;
        private System.Windows.Forms.Label label40;
        private System.Windows.Forms.TrackBar trackBar6;
        private System.Windows.Forms.Label label39;
        private System.Windows.Forms.TrackBar trackBar5;
        private System.Windows.Forms.Label label38;
        private System.Windows.Forms.TrackBar trackBar4;
        private System.Windows.Forms.Label label37;
        private System.Windows.Forms.TrackBar trackBar3;
        private System.Windows.Forms.Label label36;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.Label label35;
        private System.Windows.Forms.Label label34;
        private System.Windows.Forms.TrackBar trackBar1;
        private Emgu.CV.UI.ImageBox imBox_disparity;
        private System.Windows.Forms.Button but_imGen;
        private Emgu.CV.UI.ImageBox imBox_mark2;
        private Emgu.CV.UI.ImageBox imBox_mark1;
        private System.Windows.Forms.Label lab_check;
        private System.Windows.Forms.Label label33;
        private System.Windows.Forms.Button but_swapMonit;
        private System.Windows.Forms.Label lab_curCor;
        private System.Windows.Forms.Label lab_kor;
        private System.Windows.Forms.Button but_modeV;
        private System.Windows.Forms.Button butt_plane_Ozx;
        private System.Windows.Forms.Button but_plane_Oyz;
        private System.Windows.Forms.Button but_plane_Oxy;
        private System.Windows.Forms.Button but_ProjV;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TrackBar trackOz;
        private System.Windows.Forms.Button butSaveOpenGlIm;
        private System.Windows.Forms.TrackBar trackY_light;
        private System.Windows.Forms.TrackBar trackZ_light;
        private System.Windows.Forms.TrackBar trackOx;
        private System.Windows.Forms.TrackBar trackX_light;
        private System.Windows.Forms.TrackBar trackOy;
        private System.Windows.Forms.TabPage tabMain;
        private System.Windows.Forms.Label label55;
        private System.Windows.Forms.TextBox txBx_photoName;
        private System.Windows.Forms.TextBox nameC2;
        private System.Windows.Forms.TextBox nameB2;
        private System.Windows.Forms.TextBox textNimVid;
        private System.Windows.Forms.TextBox nameA2;
        private System.Windows.Forms.TextBox nameX;
        private System.Windows.Forms.TextBox nameY;
        private System.Windows.Forms.TextBox nameC_in;
        private System.Windows.Forms.TextBox nameZ;
        private System.Windows.Forms.TextBox nameB_in;
        private System.Windows.Forms.TextBox nameA_in;
        private System.Windows.Forms.TextBox nameX2;
        private System.Windows.Forms.TextBox nameZ_in;
        private System.Windows.Forms.TextBox nameY2;
        private System.Windows.Forms.TextBox nameY_in;
        private System.Windows.Forms.TextBox nameZ2;
        private System.Windows.Forms.TextBox nameX_in;
        private System.Windows.Forms.TextBox boxN;
        private System.Windows.Forms.TextBox nameC;
        private System.Windows.Forms.TextBox nameB;
        private System.Windows.Forms.TextBox nameA;
        private System.Windows.Forms.TextBox box_scanFolder;
        private System.Windows.Forms.TextBox box_photoFolder;
        private System.Windows.Forms.TextBox textBox_powerLaser;
        private Emgu.CV.UI.ImageBox imBox_base_2;
        private Emgu.CV.UI.ImageBox imBox_base_1;
        private Emgu.CV.UI.ImageBox imBox_base;
        private System.Windows.Forms.Button but_ph;
        private System.Windows.Forms.Label label45;
        private System.Windows.Forms.TrackBar trackBar11;
        private System.Windows.Forms.Label label46;
        private System.Windows.Forms.TrackBar trackBar12;
        private System.Windows.Forms.Label label47;
        private System.Windows.Forms.TrackBar trackBar13;
        private System.Windows.Forms.Label label48;
        private System.Windows.Forms.TrackBar trackBar14;
        private System.Windows.Forms.Label label49;
        private System.Windows.Forms.TrackBar trackBar15;
        private System.Windows.Forms.Label label50;
        private System.Windows.Forms.TrackBar trackBar16;
        private System.Windows.Forms.Label label51;
        private System.Windows.Forms.TrackBar trackBar17;
        private System.Windows.Forms.Label label52;
        private System.Windows.Forms.TrackBar trackBar18;
        private System.Windows.Forms.Label label53;
        private System.Windows.Forms.TrackBar trackBar19;
        private System.Windows.Forms.Label label54;
        private System.Windows.Forms.TrackBar trackBar20;
        private System.Windows.Forms.Button but_addBufRob;
        private System.Windows.Forms.Button but_robMod;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label25;
        private Emgu.CV.UI.ImageBox imageBox1;
        private System.Windows.Forms.Button butStop;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Button butStart;
        private Emgu.CV.UI.ImageBox imageBox2;
        private System.Windows.Forms.Button videoCapt;
        private System.Windows.Forms.Button rob_res;
        private System.Windows.Forms.Button rob_con;
        private System.Windows.Forms.Button but_res_pos_2;
        private System.Windows.Forms.Button disc_rob;
        private System.Windows.Forms.Button but_res_pos1;
        private System.Windows.Forms.Button send_rob;
        private System.Windows.Forms.Button bet_res_pos;
        private System.Windows.Forms.Button but_scan_start;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Button but_photo;
        private System.Windows.Forms.Button butCalcIm;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox comboVideo;
        private System.Windows.Forms.ComboBox comboNumber;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button but_find_ports;
        private System.Windows.Forms.ComboBox comboBox_portsArd;
        private System.Windows.Forms.Button but_setPower;
        private System.Windows.Forms.Button but_laserOff;
        private System.Windows.Forms.Button but_laserOn;
        private System.Windows.Forms.Button but_close;
        private System.Windows.Forms.Button but_open;
        private System.Windows.Forms.TabControl windowsTabs;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button but_setShvpPos;
        private System.Windows.Forms.TextBox textBox_shvpPos;
        private System.Windows.Forms.Label label56;
        private System.Windows.Forms.ComboBox combo_improc;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button but_scan_start_laser;
        private System.Windows.Forms.Button but_scan_def;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_marl_shcpPos;
        private System.Windows.Forms.Button but_marl_setShvpPos;
        private System.Windows.Forms.Button but_marl_close;
        private System.Windows.Forms.Button but_marl_open;
        private System.Windows.Forms.Button but_marl_receav;
        private System.Windows.Forms.Button but_scan_marl;
        private System.Windows.Forms.Button but_scan_stereolas;
        private System.Windows.Forms.Button but_load_scan;
        private System.Windows.Forms.Button but_text_vis;
        private System.Windows.Forms.Button but_point_type;
        private System.Windows.Forms.Button but_end_cont;
        private System.Windows.Forms.Button but_send_traj;
        private System.Windows.Forms.Button but_scan_load_ex;
        private System.Windows.Forms.Button but_load_conf_cam2;
        private System.Windows.Forms.Button but_load_conf_cam1;
        private System.Windows.Forms.TextBox textB_cam2_conf;
        private System.Windows.Forms.TextBox textB_cam1_conf;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button but_scan_path;
        private System.Windows.Forms.TextBox textB_scan_path;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button but_stereo_cal_path;
        private System.Windows.Forms.TextBox textB_stereo_cal_path;
        private System.Windows.Forms.Button but_setShvpVel;
        private System.Windows.Forms.TextBox textBox_shvpVel;
        private System.Windows.Forms.Button but_gl_clear;
        private System.Windows.Forms.PropertyGrid propGrid_traj;
        private System.Windows.Forms.Label label_timer;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button but_laser_dest;
        private System.Windows.Forms.TextBox textBox_laser_dest;
        private System.Windows.Forms.Button but_set_kpp;
        private System.Windows.Forms.TextBox textBox_set_kpp;
        private System.Windows.Forms.Button butset_kvp;
        private System.Windows.Forms.TextBox textBox_set_kvp;
        private System.Windows.Forms.Button but_calibr_Bfs;
        private System.Windows.Forms.Button but_im_to_3d_im1;
        private System.Windows.Forms.TextBox tb_strip_scan;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Button but_load_fr_cal;
        private System.Windows.Forms.Button but_gl_light;
        private System.Windows.Forms.TextBox tp_smooth_scan;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Button but_traj_clear;
        private System.Windows.Forms.Button but_cross_flat;
        private System.Windows.Forms.Button but_las_enc;
        private System.Windows.Forms.Label label20;
        private System.Windows.Forms.Button but_save_stl;
        private System.Windows.Forms.Button but_load_stl;
        private System.Windows.Forms.Button but_scan_load_sing;
        private System.Windows.Forms.Button but_load_sing_calib;
        private System.Windows.Forms.Button but_scan_sing_las;
        private System.Windows.Forms.TextBox tB_fps_scan;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Button but_keep_area;
        private System.Windows.Forms.Button but_delete_area;
        private System.Windows.Forms.Button but_reconstruc_area;
        private System.Windows.Forms.Button but_rob_res_sc;
        private System.Windows.Forms.Button but_rob_con_sc;
        private System.Windows.Forms.Button but_rob_discon_sc;
        private System.Windows.Forms.Button but_rob_send_sc;
        private System.Windows.Forms.TextBox tb_rob_pos_sc;
        private System.Windows.Forms.Button but_rob_clear_sc;
        private System.Windows.Forms.Button but_rob_manual_sc;
        private System.Windows.Forms.Button but_rob_auto_sc;
        private System.Windows.Forms.Button but_rob_start_sc;
        private System.Windows.Forms.Button but_rob_traj_kuka;
        private System.Windows.Forms.Button but_rob_traj_pulse;
        private System.Windows.Forms.Button but_resize;
        private System.Windows.Forms.Button but_scan_stereo_rob;
        private System.Windows.Forms.TreeView tree_models;
        private System.Windows.Forms.PropertyGrid prop_grid_model;
        private System.Windows.Forms.Button but_intersec_obj;
        private System.Windows.Forms.Button but_remesh_test;
        private System.Windows.Forms.Label lab_fps_cam1;
        private System.Windows.Forms.TextBox tb_port_tcp;
        private System.Windows.Forms.Button but_dir_disp;
        private System.Windows.Forms.TextBox tb_dir_disp;
        private System.Windows.Forms.Button but_home_las;
        private System.Windows.Forms.Button but_div_disp;
        private System.Windows.Forms.TextBox tb_div_disp;
        private System.Windows.Forms.ComboBox combo_robot_ch;
        private System.Windows.Forms.Button but_del_obj3d;
        private System.Windows.Forms.CheckBox ch_b_dist;
        private System.Windows.Forms.CheckBox ch_b_sync;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.TextBox tb_print_vel;
        private System.Windows.Forms.TextBox tb_print_nozzle_d;
        private System.Windows.Forms.Label label57;
        private System.Windows.Forms.TextBox tb_print_syr_d;
        private System.Windows.Forms.Button but_extr_st;
        private System.Windows.Forms.CheckBox ch_b_im_s;
        private System.Windows.Forms.Button but_set_model_matr;
        private System.Windows.Forms.Button but_ps_cal_save;
        private System.Windows.Forms.Button but_comp_basis;
        private System.Windows.Forms.Button but_stereo_3dp;
        private System.Windows.Forms.Button but_dist_same_ps;
    }
}



