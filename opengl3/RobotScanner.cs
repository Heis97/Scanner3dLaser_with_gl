using System.Text.Json;
using OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using jakaApi;

namespace opengl3
{

    public partial class RobotScanner : Form
    {
        GraphicGL GL1 = new GraphicGL();
        RobotJaka robot = new RobotJaka();
        TCPclient scanner_client = new TCPclient();
        Thread robot_thread = null;
        string scanner_ip = "localhost";
        string path_model_global = "C:\\Users\\User\\Documents\\RV 3D Studio Scans\\test_robot_0410_folder\\Processing\\Models";
        int scanner_port = 31000;
        int jaka_handle;
        double tcp_incr_lin = 1;
        double k_ori = 0.01;
        string res_scan = "";
        bool scanning = false;
        bool modeling = false;
        string scan_name = "";
        public RobotScanner()
        {
            InitializeComponent();
            var test_com = new ScannerCommand(ScannerCommand.Module.Scan, ScannerCommand.Command.Create, null);
            var str_j = test_com.toStr();
            Console.WriteLine(test_com.toStr());
        }
        private void glControl1_Resize(object sender, EventArgs e)
        {
            GL1.resize(sender, e);
        }
        void recieve_tcp(object obj)
        {
            var con = (TCPclient)obj;

            while (con.is_connect())
            {
                var res = con.reseav();

                Thread.Sleep(10);
                if (res != null)
                    if (res.Length > 3)
                    {
                        res = res.Substring(2);
                        if (res.Contains("scan"))
                        {
                            label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.Text = "Сканирование завершено"));
                            label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.ForeColor = Color.ForestGreen));
                            scanning = false;
                        }
                        if (res.Contains("built"))
                        {
                            label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.Text = "Создание модели завершено"));
                            label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.ForeColor = Color.ForestGreen));
                            modeling = false;
                        }
                        if (res.Contains("model_"))
                        {
                            scan_name = res;
                        }
                        Console.WriteLine(res);
                    }

            }
        }
        #region event gl
        private void glControl1_ContextCreated(object sender, GlControlEventArgs e)
        {
            var send = (Control)sender;
            GL1.glControl_ContextCreated(sender, e);
            var w = send.Width;
            var h = send.Height;
            var d = 100;
            var fr = GL1.addFrame(new Point3d_GL(0, 0, 0), new Point3d_GL(d, 0, 0), new Point3d_GL(0, d, 0), new Point3d_GL(0, 0, d));
            GL1.buffersGl.setTranspobj(fr, 0.4f);
            // generateImage3D_BOARD_solid(chess_size.Height, chess_size.Width, markSize, PatternType.Mesh);

            // generateImage3D_BOARD_solid(chess_size.Height, chess_size.Width, markSize, PatternType.Chess);
            // GL1.addFlat3d_XY_zero_s(-0.01f, new Color3d_GL(135,117,103,1,255)*1.4);
            //GL1.SortObj();
            int monitor_num = 1;
            if (monitor_num == 4)
            {
                GL1.addMonitor(new Rectangle(w / 2, 0, w / 2, h / 2), 0);
                GL1.addMonitor(new Rectangle(0, 0, w / 2, h / 2), 1, new Vertex3d(0, 60, 0), new Vertex3d(100, 0, -60), 0);
                GL1.addMonitor(new Rectangle(w / 2, h / 2, w / 2, h / 2), 2);
                GL1.addMonitor(new Rectangle(0, h / 2, w / 2, h / 2), 3);
            }
            else if (monitor_num == 2)
            {
                GL1.addMonitor(new Rectangle(0, 0, w, h / 2), 0);
                GL1.addMonitor(new Rectangle(0, h / 2, w, h / 2), 1, new Vertex3d(0, 60, 0), new Vertex3d(100, 0, -60), 0);
                GL1.transRotZooms[0].viewType_ = viewType.Perspective;
                GL1.transRotZooms[1].viewType_ = viewType.Perspective;
            }
            else
            {
                GL1.addMonitor(new Rectangle(0, 0, w, h), 0);
            }

            /* */
            //GL1.transRotZooms[1].xRot = 33;
            //GL1.transRotZooms[1].off_x = -25;
            //GL1.transRotZooms[1].off_y = 31;
            //GL1.transRotZooms[1].zoom = 2.6699;

            GL1.add_TreeView(treeView_models);
            GL1.add_Label(label_gl, label_gl, label_gl);
        }

        private void glControl1_Render(object sender, GlControlEventArgs e)
        {

            GL1.glControl_Render(sender, e);
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            GL1.glControl_MouseMove(sender, e);
        }
        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            GL1.glControl_MouseDown(sender, e);
        }

        private void Form1_mousewheel(object sender, MouseEventArgs e)
        {
            GL1.Form1_mousewheel(sender, e);
        }
        #endregion
        private void but_scan_con_Click(object sender, EventArgs e)
        {

            scanner_client = new TCPclient();
            scanner_client.Connection(scanner_port, scanner_ip);
            Thread tcp_thread = new Thread(recieve_tcp);
            tcp_thread.Start(scanner_client);
            label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.Text = "Сканер подключён"));
            label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.ForeColor = Color.ForestGreen));
        }

        private void but_scan_discon_Click(object sender, EventArgs e)
        {
            scanner_client?.close_con();
            scanner_client = null;
        }

        private void but_scan_make_scan_Click(object sender, EventArgs e)
        {
            make_scan();
        }
        private void make_scan()
        {
            wait_scanner();
            var com = new ScannerCommand(ScannerCommand.Module.Scan, ScannerCommand.Command.Create, null);
            send_command(com);
            label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.Text = "Сканирование запущено..."));
            label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.ForeColor = Color.Firebrick));
            scanning = true;
        }
        private void build_model()
        {
            wait_scanner();
            var com = new ScannerCommand(ScannerCommand.Module.Processing, ScannerCommand.Command.BuildModel, null);
            send_command(com);
            label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.Text = "Создание модели..."));
            label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.ForeColor = Color.Firebrick));
            modeling = true;
            //wait
            Thread.Sleep(100);
            wait_scanner();
            Thread.Sleep(500);
            
            var last_folder = GetLatestCreatedFolder(path_model_global);
            var path_model = Path.Combine(last_folder, "Raw", "body");
            var ps_ob = (Polygon3d_GL[])Model3d.parsing_raw_binary( path_model)[1];
             GL1.addMesh(Polygon3d_GL.toMesh(ps_ob)[0], PrimitiveType.Triangles,null,"model");
        }

        private void wait_scanner()
        {
            while (scanning || modeling) 
            {
                Console.WriteLine(scanning+ " " + modeling);
                Thread.Sleep(100);
            };
        }

        private void but_scan_clear_scan_Click(object sender, EventArgs e)
        {
            var com = new ScannerCommand(ScannerCommand.Module.General, ScannerCommand.Command.CheckFeasibility, "clear");
            send_command(com);
        }

        private void but_scan_make_model_Click(object sender, EventArgs e)
        {
            build_model();
        }

        private void but_save_stl_Click(object sender, EventArgs e)
        {
           

        }

        static string GetLatestCreatedFolder(string directoryPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);
            DirectoryInfo[] directories = directoryInfo.GetDirectories();

            if (directories.Length == 0)
            {
                return null;
            }

            DirectoryInfo latestDirectory = directories[0];

            foreach (var dir in directories)
            {
                if (dir.CreationTime > latestDirectory.CreationTime)
                {
                    latestDirectory = dir;
                }
            }

            return latestDirectory.FullName;
        
        }
        string send_command(ScannerCommand command)
        {

            if (!scanner_client.is_connect())
            {
                label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.Text = "Сканер не подключён"));
                label_cur_status.BeginInvoke((MethodInvoker)(() => label_cur_status.ForeColor = Color.Firebrick));
            }
            scanner_client.send_mes("00" + command.toStr());
            Thread.Sleep(100);
            return scanner_client.reseav();
        }
        #region buts_jog
        //__________TRANSLATION_____________
        private void but_x_p_Click(object sender, EventArgs e)
        {
            move(tcp_incr_lin);
        }

        private void but_x_m_Click(object sender, EventArgs e)
        {
            move(-tcp_incr_lin);
        }

        private void but_y_p_Click(object sender, EventArgs e)
        {
            move(0, tcp_incr_lin);
        }

        private void but_y_m_Click(object sender, EventArgs e)
        {
            move(0, -tcp_incr_lin);
        }

        private void but_z_p_Click(object sender, EventArgs e)
        {
            move(0, 0, tcp_incr_lin);
        }

        private void but_z_m_Click(object sender, EventArgs e)
        {
            move(0, 0, -tcp_incr_lin);
        }
        //__________ROTATION_____________
        private void but_rx_p_Click(object sender, EventArgs e)
        {
            move(0, 0, 0, k_ori * tcp_incr_lin);
        }

        private void but_rx_m_Click(object sender, EventArgs e)
        {
            move(0, 0, 0, -k_ori * tcp_incr_lin);
        }

        private void but_ry_p_Click(object sender, EventArgs e)
        {
            move(0, 0, 0, 0, k_ori * tcp_incr_lin);
        }

        private void but_ry_m_Click(object sender, EventArgs e)
        {
            move(0, 0, 0, 0, -k_ori * tcp_incr_lin);
        }

        private void but_rz_p_Click(object sender, EventArgs e)
        {
            move(0, 0, 0, 0, 0, k_ori * tcp_incr_lin);
        }

        private void but_rz_m_Click(object sender, EventArgs e)
        {
            move(0, 0, 0, 0, 0, -k_ori * tcp_incr_lin);
        }

        private void but_rob_con_Click(object sender, EventArgs e)
        {
            robot.on();
            robot.set_tool();
            //robot.set_user_frame();
        }

        private void but_rob_discon_Click(object sender, EventArgs e)
        {
            robot.off();
        }

        #endregion

        private void but_rob_cur_pos_Click(object sender, EventArgs e)
        {
            Console.WriteLine(robot.get_cur_pos());
        }

        private void but_rob_home_Click(object sender, EventArgs e)
        {
            robot.move_home();
        }

        private void but_robscan_scan_Click(object sender, EventArgs e)
        {
            robot_thread = new Thread(scan_area);
            robot_thread.Start();
        }
        private void scan_area()
        {
            
            var len = robot.get_list_len();
            if(len<=1)
            {
                scan_area_or();
            }
            else
            {
                robot.set_zero_frame();
                for (int i = 0; i < len; i++)
                {
                    move_and_scan_abs(i);
                }
            }
        }
        private void scan_area_or()
        {
            
            robot.set_zero_frame();
            Console.WriteLine("begin: " + robot.get_cur_pos());
            Console.WriteLine("_____________");
            robot.set_user_frame();
           
            var rx_amp = 0.1;
            var ry_amp = 0.1;
            var nx = 3;
            var ny = 3;
            var rx_delt = rx_amp / nx;
            var ry_delt = ry_amp / ny;

            move(0, 0, 0, -rx_amp, 0, 0);
            for (int i = -nx; i <  nx; i++)
            {
                move_and_scan(0, 0, 0, rx_delt, 0, 0);
            }
            move(0, 0, 0, -rx_amp, 0, 0);
            //----------------------------------
            move(0, 0, 0, -ry_amp, 0, 0);
            for (int i = -ny; i < ny; i++)
            {
                move_and_scan(0, 0, 0,0, ry_delt,  0);
            }
            move(0, 0, 0,0, -ry_amp, 0);
            robot.set_zero_frame();
            //move(1, 0, 0, 0, 0, 0);
            Thread.Sleep(500);
            Console.WriteLine("end: " + robot.get_cur_pos());
            Console.WriteLine("_____________");
            /* move(0, 0, 0, 0, -ry_amp, 0);
             for (int i = -ny; i < 2 * ny; i++)
             {
                 move_and_scan(0, 0, 0, 0, ry_delt, 0);
             }
             move(0, 0, 0, 0, -ry_amp, 0);*/
        }
        private void move_and_scan(double x = 0, double y = 0, double z = 0, double rx = 0, double ry = 0, double rz=0)
        {
            move(x,y,z,rx,ry,rz);
            Thread.Sleep(400);
            //make_scan();
            Thread.Sleep(100);
            //wait_scanner();

           
        }


        private void move(double x = 0, double y = 0, double z = 0, double rx = 0, double ry = 0, double rz = 0)
        {
            //robot.set_zero_frame();
            //robot.set_user_frame();
            
           // int id_set = 2;
            //robot.set_tool();
            if (x == 0 && y == 0 && z == 0)
            {
                robot.move_lin_rel_or(x, y, z, rx, ry, rz);
            }
            else
            {
                robot.move_lin_rel(x, y, z, rx, ry, rz);
            }
           // Thread.Sleep(1000);
           //Console.WriteLine(robot.get_cur_pos());
        }
        private void move_and_scan_abs(int i)
        {
            robot.move_lin_abs_from_list(i);
            Thread.Sleep(400);
            //make_scan();
            Thread.Sleep(100);
            //wait_scanner();


        }
        private void move_abs(int i)
        {
            robot.move_lin_abs_from_list(i);
        }

        private void but_rob_work_pos_Click(object sender, EventArgs e)
        {
            robot.move_work();
        }

        private void but_rob_stop_Click(object sender, EventArgs e)
        {
            robot.stop();
            robot.set_zero_frame();
            if (robot_thread!=null)
            {
                robot_thread.Suspend();
                robot_thread = null;
            }
            
        }
        private void radioButton_mm_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton checkBox = (RadioButton)sender; // приводим отправителя к элементу типа CheckBox
            if (checkBox.Checked == true)
            {
                tcp_incr_lin = Convert.ToDouble(checkBox.Text);
            }
        }

        private void but_model_save_stl_Click(object sender, EventArgs e)
        {
            var stl_name = MainScanningForm.save_file_name(Directory.GetCurrentDirectory(), treeView_models.SelectedNode.Text, "stl");
            //var scan_stl = Polygon3d_GL.toMesh(mesh);
            if (stl_name == null) return;
            STLmodel.saveMesh(GL1.buffersGl.objs[treeView_models.SelectedNode.Text].vertex_buffer_data, stl_name);
        }

        private void but_save_point_Click(object sender, EventArgs e)
        {
            robot.add_to_list();
            label_points_cur.BeginInvoke((MethodInvoker)(() => label_points_cur.Text = "Точек: "+ robot.get_list_len()));
        }

        private void but_clear_points_Click(object sender, EventArgs e)
        {
            robot.clean_list();
            label_points_cur.BeginInvoke((MethodInvoker)(() => label_points_cur.Text = "Точки не добавлены" + robot.get_list_len()));
        }
    }

    public class ScannerCommand
    {
        public enum Module { Scan, General, Table, Processing };
        public enum Command { CheckFeasibility, Create, Rotate, RemoveNoise, RegisterGlobally, BuildModel };
        public string module { get; set; }
        public string command { get; set; }
        public string value { get; set; }
        public ScannerCommand(Module _module, Command _command, string _value = null)
        {
            module = _module.ToString();
            command = _command.ToString();
            if (_value == null)
            {
                value ="asd" ;
            }
            else
            {
                value = _value;
            }

        }
        public string toStr()
        {
            JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
            //jsonSerializerOptions.MaxDepth = 0;
            var json = JsonSerializer.Serialize(this);
            //var json2 = json.Replace(@"\\", "");
            return json;
        }
    }
}
