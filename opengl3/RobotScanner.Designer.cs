namespace opengl3
{
    partial class RobotScanner
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.glControl_main = new OpenGL.GlControl();
            this.treeView_models = new System.Windows.Forms.TreeView();
            this.label_gl = new System.Windows.Forms.Label();
            this.but_scan_con = new System.Windows.Forms.Button();
            this.but_scan_discon = new System.Windows.Forms.Button();
            this.but_scan_make_scan = new System.Windows.Forms.Button();
            this.but_scan_clear_scan = new System.Windows.Forms.Button();
            this.but_scan_make_model = new System.Windows.Forms.Button();
            this.but_save_stl = new System.Windows.Forms.Button();
            this.but_rob_discon = new System.Windows.Forms.Button();
            this.but_rob_con = new System.Windows.Forms.Button();
            this.but_x_p = new System.Windows.Forms.Button();
            this.but_y_p = new System.Windows.Forms.Button();
            this.but_z_p = new System.Windows.Forms.Button();
            this.but_x_m = new System.Windows.Forms.Button();
            this.but_y_m = new System.Windows.Forms.Button();
            this.but_z_m = new System.Windows.Forms.Button();
            this.but_rx_p = new System.Windows.Forms.Button();
            this.but_ry_p = new System.Windows.Forms.Button();
            this.but_rx_m = new System.Windows.Forms.Button();
            this.but_ry_m = new System.Windows.Forms.Button();
            this.but_rz_p = new System.Windows.Forms.Button();
            this.but_rz_m = new System.Windows.Forms.Button();
            this.but_rob_cur_pos = new System.Windows.Forms.Button();
            this.but_rob_home = new System.Windows.Forms.Button();
            this.but_robscan_scan = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // glControl_main
            // 
            this.glControl_main.Animation = true;
            this.glControl_main.AnimationTime = 60;
            this.glControl_main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.glControl_main.ColorBits = ((uint)(24u));
            this.glControl_main.DepthBits = ((uint)(24u));
            this.glControl_main.Location = new System.Drawing.Point(13, 14);
            this.glControl_main.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.glControl_main.MultisampleBits = ((uint)(8u));
            this.glControl_main.Name = "glControl_main";
            this.glControl_main.Size = new System.Drawing.Size(1200, 999);
            this.glControl_main.StencilBits = ((uint)(0u));
            this.glControl_main.TabIndex = 0;
            this.glControl_main.ContextCreated += new System.EventHandler<OpenGL.GlControlEventArgs>(this.glControl1_ContextCreated);
            this.glControl_main.Render += new System.EventHandler<OpenGL.GlControlEventArgs>(this.glControl1_Render);
            this.glControl_main.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseDown);
            this.glControl_main.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl1_MouseMove);
            this.glControl_main.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.Form1_mousewheel);
            this.glControl_main.Resize += new System.EventHandler(this.glControl1_Resize);
            // 
            // treeView_models
            // 
            this.treeView_models.Location = new System.Drawing.Point(1244, 589);
            this.treeView_models.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.treeView_models.Name = "treeView_models";
            this.treeView_models.Size = new System.Drawing.Size(352, 424);
            this.treeView_models.TabIndex = 1;
            // 
            // label_gl
            // 
            this.label_gl.AutoSize = true;
            this.label_gl.Location = new System.Drawing.Point(1603, 986);
            this.label_gl.Name = "label_gl";
            this.label_gl.Size = new System.Drawing.Size(55, 16);
            this.label_gl.TabIndex = 2;
            this.label_gl.Text = "label_gl";
            // 
            // but_scan_con
            // 
            this.but_scan_con.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_scan_con.Location = new System.Drawing.Point(1244, 33);
            this.but_scan_con.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_scan_con.Name = "but_scan_con";
            this.but_scan_con.Size = new System.Drawing.Size(171, 66);
            this.but_scan_con.TabIndex = 3;
            this.but_scan_con.Text = "Подключиться";
            this.but_scan_con.UseVisualStyleBackColor = true;
            this.but_scan_con.Click += new System.EventHandler(this.but_scan_con_Click);
            // 
            // but_scan_discon
            // 
            this.but_scan_discon.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_scan_discon.Location = new System.Drawing.Point(1244, 105);
            this.but_scan_discon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_scan_discon.Name = "but_scan_discon";
            this.but_scan_discon.Size = new System.Drawing.Size(171, 66);
            this.but_scan_discon.TabIndex = 4;
            this.but_scan_discon.Text = "Отключиться";
            this.but_scan_discon.UseVisualStyleBackColor = true;
            this.but_scan_discon.Click += new System.EventHandler(this.but_scan_discon_Click);
            // 
            // but_scan_make_scan
            // 
            this.but_scan_make_scan.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_scan_make_scan.Location = new System.Drawing.Point(1244, 204);
            this.but_scan_make_scan.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_scan_make_scan.Name = "but_scan_make_scan";
            this.but_scan_make_scan.Size = new System.Drawing.Size(171, 66);
            this.but_scan_make_scan.TabIndex = 5;
            this.but_scan_make_scan.Text = "Сделать скан";
            this.but_scan_make_scan.UseVisualStyleBackColor = true;
            this.but_scan_make_scan.Click += new System.EventHandler(this.but_scan_make_scan_Click);
            // 
            // but_scan_clear_scan
            // 
            this.but_scan_clear_scan.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_scan_clear_scan.Location = new System.Drawing.Point(1244, 276);
            this.but_scan_clear_scan.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_scan_clear_scan.Name = "but_scan_clear_scan";
            this.but_scan_clear_scan.Size = new System.Drawing.Size(171, 66);
            this.but_scan_clear_scan.TabIndex = 6;
            this.but_scan_clear_scan.Text = "Очистить сканы";
            this.but_scan_clear_scan.UseVisualStyleBackColor = true;
            this.but_scan_clear_scan.Click += new System.EventHandler(this.but_scan_clear_scan_Click);
            // 
            // but_scan_make_model
            // 
            this.but_scan_make_model.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_scan_make_model.Location = new System.Drawing.Point(1244, 347);
            this.but_scan_make_model.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_scan_make_model.Name = "but_scan_make_model";
            this.but_scan_make_model.Size = new System.Drawing.Size(171, 66);
            this.but_scan_make_model.TabIndex = 7;
            this.but_scan_make_model.Text = "Сделать модель";
            this.but_scan_make_model.UseVisualStyleBackColor = true;
            this.but_scan_make_model.Click += new System.EventHandler(this.but_scan_make_model_Click);
            // 
            // but_save_stl
            // 
            this.but_save_stl.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_save_stl.Location = new System.Drawing.Point(1244, 418);
            this.but_save_stl.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_save_stl.Name = "but_save_stl";
            this.but_save_stl.Size = new System.Drawing.Size(171, 66);
            this.but_save_stl.TabIndex = 8;
            this.but_save_stl.Text = "Сохранить stl";
            this.but_save_stl.UseVisualStyleBackColor = true;
            this.but_save_stl.Click += new System.EventHandler(this.but_save_stl_Click);
            // 
            // but_rob_discon
            // 
            this.but_rob_discon.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_rob_discon.Location = new System.Drawing.Point(1479, 105);
            this.but_rob_discon.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_rob_discon.Name = "but_rob_discon";
            this.but_rob_discon.Size = new System.Drawing.Size(171, 66);
            this.but_rob_discon.TabIndex = 10;
            this.but_rob_discon.Text = "Отключиться";
            this.but_rob_discon.UseVisualStyleBackColor = true;
            this.but_rob_discon.Click += new System.EventHandler(this.but_rob_discon_Click);
            // 
            // but_rob_con
            // 
            this.but_rob_con.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_rob_con.Location = new System.Drawing.Point(1479, 33);
            this.but_rob_con.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_rob_con.Name = "but_rob_con";
            this.but_rob_con.Size = new System.Drawing.Size(171, 66);
            this.but_rob_con.TabIndex = 9;
            this.but_rob_con.Text = "Подключиться";
            this.but_rob_con.UseVisualStyleBackColor = true;
            this.but_rob_con.Click += new System.EventHandler(this.but_rob_con_Click);
            // 
            // but_x_p
            // 
            this.but_x_p.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_x_p.Location = new System.Drawing.Point(1479, 204);
            this.but_x_p.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_x_p.Name = "but_x_p";
            this.but_x_p.Size = new System.Drawing.Size(64, 55);
            this.but_x_p.TabIndex = 11;
            this.but_x_p.Text = "+X";
            this.but_x_p.UseVisualStyleBackColor = true;
            this.but_x_p.Click += new System.EventHandler(this.but_x_p_Click);
            // 
            // but_y_p
            // 
            this.but_y_p.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_y_p.Location = new System.Drawing.Point(1548, 204);
            this.but_y_p.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_y_p.Name = "but_y_p";
            this.but_y_p.Size = new System.Drawing.Size(64, 55);
            this.but_y_p.TabIndex = 12;
            this.but_y_p.Text = "+Y";
            this.but_y_p.UseVisualStyleBackColor = true;
            this.but_y_p.Click += new System.EventHandler(this.but_y_p_Click);
            // 
            // but_z_p
            // 
            this.but_z_p.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_z_p.Location = new System.Drawing.Point(1617, 204);
            this.but_z_p.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_z_p.Name = "but_z_p";
            this.but_z_p.Size = new System.Drawing.Size(64, 55);
            this.but_z_p.TabIndex = 13;
            this.but_z_p.Text = "+Z";
            this.but_z_p.UseVisualStyleBackColor = true;
            this.but_z_p.Click += new System.EventHandler(this.but_z_p_Click);
            // 
            // but_x_m
            // 
            this.but_x_m.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_x_m.Location = new System.Drawing.Point(1479, 265);
            this.but_x_m.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_x_m.Name = "but_x_m";
            this.but_x_m.Size = new System.Drawing.Size(64, 55);
            this.but_x_m.TabIndex = 14;
            this.but_x_m.Text = "-X";
            this.but_x_m.UseVisualStyleBackColor = true;
            this.but_x_m.Click += new System.EventHandler(this.but_x_m_Click);
            // 
            // but_y_m
            // 
            this.but_y_m.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_y_m.Location = new System.Drawing.Point(1548, 265);
            this.but_y_m.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_y_m.Name = "but_y_m";
            this.but_y_m.Size = new System.Drawing.Size(64, 55);
            this.but_y_m.TabIndex = 15;
            this.but_y_m.Text = "-Y";
            this.but_y_m.UseVisualStyleBackColor = true;
            this.but_y_m.Click += new System.EventHandler(this.but_y_m_Click);
            // 
            // but_z_m
            // 
            this.but_z_m.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_z_m.Location = new System.Drawing.Point(1617, 265);
            this.but_z_m.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_z_m.Name = "but_z_m";
            this.but_z_m.Size = new System.Drawing.Size(64, 55);
            this.but_z_m.TabIndex = 16;
            this.but_z_m.Text = "-Z";
            this.but_z_m.UseVisualStyleBackColor = true;
            this.but_z_m.Click += new System.EventHandler(this.but_z_m_Click);
            // 
            // but_rx_p
            // 
            this.but_rx_p.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_rx_p.Location = new System.Drawing.Point(1687, 204);
            this.but_rx_p.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_rx_p.Name = "but_rx_p";
            this.but_rx_p.Size = new System.Drawing.Size(64, 55);
            this.but_rx_p.TabIndex = 17;
            this.but_rx_p.Text = "+Rx";
            this.but_rx_p.UseVisualStyleBackColor = true;
            this.but_rx_p.Click += new System.EventHandler(this.but_rx_p_Click);
            // 
            // but_ry_p
            // 
            this.but_ry_p.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_ry_p.Location = new System.Drawing.Point(1756, 204);
            this.but_ry_p.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_ry_p.Name = "but_ry_p";
            this.but_ry_p.Size = new System.Drawing.Size(64, 55);
            this.but_ry_p.TabIndex = 18;
            this.but_ry_p.Text = "+Ry";
            this.but_ry_p.UseVisualStyleBackColor = true;
            this.but_ry_p.Click += new System.EventHandler(this.but_ry_p_Click);
            // 
            // but_rx_m
            // 
            this.but_rx_m.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_rx_m.Location = new System.Drawing.Point(1687, 265);
            this.but_rx_m.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_rx_m.Name = "but_rx_m";
            this.but_rx_m.Size = new System.Drawing.Size(64, 55);
            this.but_rx_m.TabIndex = 19;
            this.but_rx_m.Text = "-Rx";
            this.but_rx_m.UseVisualStyleBackColor = true;
            this.but_rx_m.Click += new System.EventHandler(this.but_rx_m_Click);
            // 
            // but_ry_m
            // 
            this.but_ry_m.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_ry_m.Location = new System.Drawing.Point(1756, 265);
            this.but_ry_m.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_ry_m.Name = "but_ry_m";
            this.but_ry_m.Size = new System.Drawing.Size(64, 55);
            this.but_ry_m.TabIndex = 20;
            this.but_ry_m.Text = "-Ry";
            this.but_ry_m.UseVisualStyleBackColor = true;
            this.but_ry_m.Click += new System.EventHandler(this.but_ry_m_Click);
            // 
            // but_rz_p
            // 
            this.but_rz_p.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_rz_p.Location = new System.Drawing.Point(1825, 204);
            this.but_rz_p.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_rz_p.Name = "but_rz_p";
            this.but_rz_p.Size = new System.Drawing.Size(64, 55);
            this.but_rz_p.TabIndex = 21;
            this.but_rz_p.Text = "+Rz";
            this.but_rz_p.UseVisualStyleBackColor = true;
            this.but_rz_p.Click += new System.EventHandler(this.but_rz_p_Click);
            // 
            // but_rz_m
            // 
            this.but_rz_m.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_rz_m.Location = new System.Drawing.Point(1825, 265);
            this.but_rz_m.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_rz_m.Name = "but_rz_m";
            this.but_rz_m.Size = new System.Drawing.Size(64, 55);
            this.but_rz_m.TabIndex = 22;
            this.but_rz_m.Text = "-Rz";
            this.but_rz_m.UseVisualStyleBackColor = true;
            this.but_rz_m.Click += new System.EventHandler(this.but_rz_m_Click);
            // 
            // but_rob_cur_pos
            // 
            this.but_rob_cur_pos.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_rob_cur_pos.Location = new System.Drawing.Point(1479, 347);
            this.but_rob_cur_pos.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_rob_cur_pos.Name = "but_rob_cur_pos";
            this.but_rob_cur_pos.Size = new System.Drawing.Size(171, 66);
            this.but_rob_cur_pos.TabIndex = 23;
            this.but_rob_cur_pos.Text = "Текущая позиция";
            this.but_rob_cur_pos.UseVisualStyleBackColor = true;
            this.but_rob_cur_pos.Click += new System.EventHandler(this.but_rob_cur_pos_Click);
            // 
            // but_rob_home
            // 
            this.but_rob_home.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_rob_home.Location = new System.Drawing.Point(1479, 418);
            this.but_rob_home.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_rob_home.Name = "but_rob_home";
            this.but_rob_home.Size = new System.Drawing.Size(171, 66);
            this.but_rob_home.TabIndex = 24;
            this.but_rob_home.Text = "Начальное положение";
            this.but_rob_home.UseVisualStyleBackColor = true;
            this.but_rob_home.Click += new System.EventHandler(this.but_rob_home_Click);
            // 
            // but_robscan_scan
            // 
            this.but_robscan_scan.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F);
            this.but_robscan_scan.Location = new System.Drawing.Point(1244, 502);
            this.but_robscan_scan.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.but_robscan_scan.Name = "but_robscan_scan";
            this.but_robscan_scan.Size = new System.Drawing.Size(171, 83);
            this.but_robscan_scan.TabIndex = 25;
            this.but_robscan_scan.Text = "Провести сканирование области";
            this.but_robscan_scan.UseVisualStyleBackColor = true;
            this.but_robscan_scan.Click += new System.EventHandler(this.but_robscan_scan_Click);
            // 
            // RobotScanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1901, 1033);
            this.Controls.Add(this.but_robscan_scan);
            this.Controls.Add(this.but_rob_home);
            this.Controls.Add(this.but_rob_cur_pos);
            this.Controls.Add(this.but_rz_m);
            this.Controls.Add(this.but_rz_p);
            this.Controls.Add(this.but_ry_m);
            this.Controls.Add(this.but_rx_m);
            this.Controls.Add(this.but_ry_p);
            this.Controls.Add(this.but_rx_p);
            this.Controls.Add(this.but_z_m);
            this.Controls.Add(this.but_y_m);
            this.Controls.Add(this.but_x_m);
            this.Controls.Add(this.but_z_p);
            this.Controls.Add(this.but_y_p);
            this.Controls.Add(this.but_x_p);
            this.Controls.Add(this.but_rob_discon);
            this.Controls.Add(this.but_rob_con);
            this.Controls.Add(this.but_save_stl);
            this.Controls.Add(this.but_scan_make_model);
            this.Controls.Add(this.but_scan_clear_scan);
            this.Controls.Add(this.but_scan_make_scan);
            this.Controls.Add(this.but_scan_discon);
            this.Controls.Add(this.but_scan_con);
            this.Controls.Add(this.label_gl);
            this.Controls.Add(this.treeView_models);
            this.Controls.Add(this.glControl_main);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "RobotScanner";
            this.Text = "RobotScanner";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private OpenGL.GlControl glControl_main;
        private System.Windows.Forms.TreeView treeView_models;
        private System.Windows.Forms.Label label_gl;
        private System.Windows.Forms.Button but_scan_con;
        private System.Windows.Forms.Button but_scan_discon;
        private System.Windows.Forms.Button but_scan_make_scan;
        private System.Windows.Forms.Button but_scan_clear_scan;
        private System.Windows.Forms.Button but_scan_make_model;
        private System.Windows.Forms.Button but_save_stl;
        private System.Windows.Forms.Button but_rob_discon;
        private System.Windows.Forms.Button but_rob_con;
        private System.Windows.Forms.Button but_x_p;
        private System.Windows.Forms.Button but_y_p;
        private System.Windows.Forms.Button but_z_p;
        private System.Windows.Forms.Button but_x_m;
        private System.Windows.Forms.Button but_y_m;
        private System.Windows.Forms.Button but_z_m;
        private System.Windows.Forms.Button but_rx_p;
        private System.Windows.Forms.Button but_ry_p;
        private System.Windows.Forms.Button but_rx_m;
        private System.Windows.Forms.Button but_ry_m;
        private System.Windows.Forms.Button but_rz_p;
        private System.Windows.Forms.Button but_rz_m;
        private System.Windows.Forms.Button but_rob_cur_pos;
        private System.Windows.Forms.Button but_rob_home;
        private System.Windows.Forms.Button but_robscan_scan;
    }
}