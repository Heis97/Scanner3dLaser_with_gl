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

namespace opengl3
{
    public partial class RobotScanner : Form
    {
        GraphicGL GL1 = new GraphicGL();
        TCPclient scanner_client = new TCPclient();
        string scanner_ip = "";
        int scanner_port = 10000;
        string robot_ip = "";
        int robot_port = 10000;
        public RobotScanner()
        {
            InitializeComponent();
            var test_com = new ScannerCommand("Scan", "Create", null);
            Console.WriteLine(test_com.toStr());
        }

        private void glControl1_Resize(object sender, EventArgs e)
        {
            GL1.resize(sender, e);
        }

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

        private void but_scan_con_Click(object sender, EventArgs e)
        {
            scanner_client.Connection(scanner_port,scanner_ip);
        }

        private void but_scan_discon_Click(object sender, EventArgs e)
        {
            scanner_client.close_con();
        }

        private void but_scan_make_scan_Click(object sender, EventArgs e)
        {

            scanner_client.send_mes("");
        }

        private void but_scan_clear_scan_Click(object sender, EventArgs e)
        {

        }

        private void but_scan_make_model_Click(object sender, EventArgs e)
        {

        }

        private void but_save_stl_Click(object sender, EventArgs e)
        {

        }


        public class ScannerCommand
        {
            public string section {get;set;}
            public string command { get; set; }
            public string[] value { get; set; }
            public ScannerCommand(string _section = "",string _command = "",string[] _value = null)
            {
                section = _section; 
                command = _command;
                value = _value;
            }
            public string toStr()
            {
                var json = JsonSerializer.Serialize(this);
                return json;
            }
        }
    }
}
