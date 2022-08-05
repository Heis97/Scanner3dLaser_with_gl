using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace opengl3

{
    class FormSettings
    {

        string path_settings = "settings_form.txt";
        string cam1_conf = "";
        string cam2_conf = "";
        string stereo_cal = "";
        string scan_path = "";
        public void save_settings(TextBox tb_cam1_conf, TextBox tb_cam2_conf, TextBox tb_stereo_cal, TextBox tb_scan_path)
        {
            cam1_conf = tb_cam1_conf.Text;
            cam2_conf = tb_cam2_conf.Text;
            stereo_cal = tb_stereo_cal.Text;
            scan_path = tb_scan_path.Text;
            save();
        }

        public void load_settings(TextBox tb_cam1_conf, TextBox tb_cam2_conf, TextBox tb_stereo_cal, TextBox tb_scan_path)
        {
            load();
            tb_cam1_conf.Text = cam1_conf;
            tb_cam2_conf.Text = cam2_conf;
            tb_stereo_cal.Text = stereo_cal;
            tb_scan_path.Text = scan_path;
        }

       void load()
        {
           
            string file = "";
            StreamReader sr;
            try
            {
                sr = new StreamReader(path_settings);
                file = sr.ReadToEnd();
                sr.Close();
            }
            catch(FileNotFoundException)
            {

                save();
            }
            sr = new StreamReader(path_settings);
            file = sr.ReadToEnd();
            file = file.Replace("\r", ""); 
            string[] lines = file.Split(new char[] { '\n' });
            cam1_conf = lines[0];
            cam2_conf = lines[1];
            stereo_cal = lines[2];
            scan_path = lines[3];
            sr.Close();
        }

        void save()
        {
            StreamWriter sw = new StreamWriter(path_settings, false, Encoding.UTF8);           
            sw.Write(cam1_conf+ '\n');
            sw.Write(cam2_conf + '\n');
            sw.Write(stereo_cal + '\n');
            sw.Write(scan_path);
            sw.Close();
            
        }
    }
}
