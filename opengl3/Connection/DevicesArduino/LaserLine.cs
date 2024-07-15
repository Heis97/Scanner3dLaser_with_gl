using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace opengl3
{
    public class LaserLine : DeviceArduino
    {
        string port;
        int baudrate = 250000;
        int laser = 1, 
            power = 2,
            posit = 3,
            moveshvp = 4, 
            shvp_vel = 5,
            laser_cur = 6,
            laser_dest = 7,
            laser_sensor = 8,
            k_p_p = 9,
            k_v_p = 10,
            send_pos_las = 11,
            div_disp = 12,
            dir_disp = 13,
            home_laser = 14,
            div_z = 15,
            posit_z = 16,
            home_z = 17;

        int on = 1, off = 2;
        public LaserLine(string _port)
        {
            port = _port;
            connect(port, baudrate);
        }
        public bool connectStart()
        {
            return connect(port, baudrate);
        }
        public void connectStop()
        {
            close();
        }
        public void laserOn()
        {
            send(on, laser);
        }
        public void laserOff()
        {
            send(off, laser);
        }
        public void setPower(int _power)
        {
            send(_power, power);
        }

        public void setShvpPos(int _pos)
        {
            send(_pos+5000, posit);
        }

        public void setShvpVel(double _vel)
        {
            if(_vel>0) send(comp_vel_div(_vel), shvp_vel);
        }


        public void setLaserCur(int _pos)
        {
            send(_pos, laser_cur);
        }
        public void setLaserDest(int _pos)
        {
            send(_pos, laser_dest);
        }
        public void onLaserSensor()
        {
            send(on, laser_sensor);
        }
        public void offLaserSensor()
        {
            send(off, laser_sensor);
        }
        public void setK_p_p(int _pos)
        {
            send(_pos, k_p_p);
        }
        public void setK_v_p(int _pos)
        {
            send(_pos, k_v_p);
        }

        public void get_las_pos_send()
        {
            send(0, send_pos_las);
        }

        public int get_las_pos_res()
        {

            var res= reseav().Split('\n');

            for(int k=res.Length-1;k>=0;k--)
            {
                //Console.WriteLine("k"+k+":"+res[k]);
                for (int i = 0; i < res[k].Length; i++)
                {
                    if (res[k].Contains( "lp"))
                    {
                        
                        var res_sp = res[k].Split(' ');
                        //Console.WriteLine(res_sp);
                        try
                        {
                            //Console.WriteLine(res_sp[1]);
                            return Convert.ToInt32(res_sp[1]);
                        }
                        catch
                        {
                            return 0;
                        }
                    }
                }
            }
            
            return 0;
        }

        public string get_las_pos_res_time()
        {

            var res = reseav().Split('\n');

            for (int k = res.Length - 1; k >= 0; k--)
            {
                Console.WriteLine("k"+k+":"+res[k]);
                for (int i = 0; i < res[k].Length; i++)
                {
                    if (res[k].Contains("lp") && res[k].Contains("end"))
                    {

                        var res_sp = res[k].Split(' ');
                        if(res_sp.Length>2)
                        {
                            return res_sp[1] + " " + res_sp[2];
                        }
                        
                    }
                }
            }

            return "0 0";
        }
        public int get_las_pos()
        {
            get_las_pos_send();
            //Thread.Sleep(1);
            return get_las_pos_res();
        }

        public string get_las_pos_time()
        {
            get_las_pos_send();
           // Thread.Sleep(10);
            return get_las_pos_res_time();
        }

        static public string get_las_pos_time(LaserLine laserLine)
        {
            if(laserLine == null)
            {
                return "0 0";
            }
            else
            {
                laserLine.get_las_pos_send();
                return laserLine.get_las_pos_res_time();
            }
        }
        static int comp_vel_div(double vel)
        {
            return Math.Abs((int)(5000 / vel));
        }

        public void set_dir_disp(int _dir)
        {
            send(_dir+1, dir_disp);
        }
        public void set_div_disp(int _vel)
        {
            if (_vel >= 0) send(_vel, div_disp);
        }
        public void set_home_laser()
        {
            send(0, home_laser);
        }
        public void set_z_pos(int _pos)
        {
            send(_pos + 5000, posit_z);
        }
        public void set_z_div(int _vel)
        {
            if (_vel >= 0) send(_vel, div_z);
        }
        public void set_home_z()
        {
            send(0, home_z);
        }

        public static int vel_div(double vel_nos, double d_nos, double d_syr)
        {
            double vel = (vel_nos * d_nos * d_nos) / (d_syr * d_syr);//vel pist
            return vel_pist_to_ard(vel);
        }

        public static int vel_pist_to_ard(double vel)
        {
            double nT = 5000;  //  #timer freq
            double p = 0.8;//     #step mm
            double rev = 100 * 16 * (60d / 16d); //# - reduct steps per revol
            int st = (int)((nT * p) / (vel * rev));
            //vel = (nT * p) / (st * rev);
            return st;
        }
    }
}
