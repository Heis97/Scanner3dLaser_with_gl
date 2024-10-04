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
            this.SuspendLayout();
            // 
            // glControl_main
            // 
            this.glControl_main.Animation = true;
            this.glControl_main.AnimationTime = 60;
            this.glControl_main.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.glControl_main.ColorBits = ((uint)(24u));
            this.glControl_main.DepthBits = ((uint)(24u));
            this.glControl_main.Location = new System.Drawing.Point(10, 11);
            this.glControl_main.MultisampleBits = ((uint)(8u));
            this.glControl_main.Name = "glControl_main";
            this.glControl_main.Size = new System.Drawing.Size(900, 812);
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
            this.treeView_models.Location = new System.Drawing.Point(915, 11);
            this.treeView_models.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.treeView_models.Name = "treeView_models";
            this.treeView_models.Size = new System.Drawing.Size(265, 375);
            this.treeView_models.TabIndex = 1;
            // 
            // label_gl
            // 
            this.label_gl.AutoSize = true;
            this.label_gl.Location = new System.Drawing.Point(926, 397);
            this.label_gl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_gl.Name = "label_gl";
            this.label_gl.Size = new System.Drawing.Size(43, 13);
            this.label_gl.TabIndex = 2;
            this.label_gl.Text = "label_gl";
            // 
            // but_scan_con
            // 
            this.but_scan_con.Location = new System.Drawing.Point(1184, 11);
            this.but_scan_con.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.but_scan_con.Name = "but_scan_con";
            this.but_scan_con.Size = new System.Drawing.Size(128, 54);
            this.but_scan_con.TabIndex = 3;
            this.but_scan_con.Text = "Подключиться";
            this.but_scan_con.UseVisualStyleBackColor = true;
            this.but_scan_con.Click += new System.EventHandler(this.but_scan_con_Click);
            // 
            // but_scan_discon
            // 
            this.but_scan_discon.Location = new System.Drawing.Point(1184, 70);
            this.but_scan_discon.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.but_scan_discon.Name = "but_scan_discon";
            this.but_scan_discon.Size = new System.Drawing.Size(128, 54);
            this.but_scan_discon.TabIndex = 4;
            this.but_scan_discon.Text = "Отключиться";
            this.but_scan_discon.UseVisualStyleBackColor = true;
            this.but_scan_discon.Click += new System.EventHandler(this.but_scan_discon_Click);
            // 
            // but_scan_make_scan
            // 
            this.but_scan_make_scan.Location = new System.Drawing.Point(1184, 169);
            this.but_scan_make_scan.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.but_scan_make_scan.Name = "but_scan_make_scan";
            this.but_scan_make_scan.Size = new System.Drawing.Size(128, 54);
            this.but_scan_make_scan.TabIndex = 5;
            this.but_scan_make_scan.Text = "Сделать скан";
            this.but_scan_make_scan.UseVisualStyleBackColor = true;
            this.but_scan_make_scan.Click += new System.EventHandler(this.but_scan_make_scan_Click);
            // 
            // but_scan_clear_scan
            // 
            this.but_scan_clear_scan.Location = new System.Drawing.Point(1184, 228);
            this.but_scan_clear_scan.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.but_scan_clear_scan.Name = "but_scan_clear_scan";
            this.but_scan_clear_scan.Size = new System.Drawing.Size(128, 54);
            this.but_scan_clear_scan.TabIndex = 6;
            this.but_scan_clear_scan.Text = "Очистить сканы";
            this.but_scan_clear_scan.UseVisualStyleBackColor = true;
            this.but_scan_clear_scan.Click += new System.EventHandler(this.but_scan_clear_scan_Click);
            // 
            // but_scan_make_model
            // 
            this.but_scan_make_model.Location = new System.Drawing.Point(1184, 288);
            this.but_scan_make_model.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.but_scan_make_model.Name = "but_scan_make_model";
            this.but_scan_make_model.Size = new System.Drawing.Size(128, 54);
            this.but_scan_make_model.TabIndex = 7;
            this.but_scan_make_model.Text = "Сделать модель";
            this.but_scan_make_model.UseVisualStyleBackColor = true;
            this.but_scan_make_model.Click += new System.EventHandler(this.but_scan_make_model_Click);
            // 
            // but_save_stl
            // 
            this.but_save_stl.Location = new System.Drawing.Point(1184, 347);
            this.but_save_stl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.but_save_stl.Name = "but_save_stl";
            this.but_save_stl.Size = new System.Drawing.Size(128, 54);
            this.but_save_stl.TabIndex = 8;
            this.but_save_stl.Text = "Сохранить stl";
            this.but_save_stl.UseVisualStyleBackColor = true;
            this.but_save_stl.Click += new System.EventHandler(this.but_save_stl_Click);
            // 
            // RobotScanner
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1426, 839);
            this.Controls.Add(this.but_save_stl);
            this.Controls.Add(this.but_scan_make_model);
            this.Controls.Add(this.but_scan_clear_scan);
            this.Controls.Add(this.but_scan_make_scan);
            this.Controls.Add(this.but_scan_discon);
            this.Controls.Add(this.but_scan_con);
            this.Controls.Add(this.label_gl);
            this.Controls.Add(this.treeView_models);
            this.Controls.Add(this.glControl_main);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
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
    }
}