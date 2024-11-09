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
        public double steps_per_unit_z = 800;// 1600 16 micr; 800 8 micr;
        public double steps_per_unit_las = 800;// 1600 16 micr; 800 8 micr;
        public double steps_per_unit_disp = 3200;
        public double steps_per_unit_movm_mash = 1012;//3200:1.58 ||1600:1.58 
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
            move_z = 16,
            home_z = 17,
            push_forw = 18,
            push_back = 19,
            drill_vel = 20,
            drill_dir = 21,

            rel_pos_z = 22,
            rel_pos_disp = 23,

            water_vel = 24,
            cycle_type = 25,
            cycle_ampl = 26,
            cycle_time = 27,
            cycle_time_rel = 28,
            pos_disp = 29,

            temp_control = 30,
            temp_value = 31,
            led_r= 32,
            led_g = 33,
            led_b = 34,

            hyst_temp = 35,
            cool_pwm = 36,
            heat_pwm = 37,

            send_poses = 38,

            valve_val = 39,

            div_las = 40,

            stop_las = 41,
            stop_z = 42,

            pos_las  = 43,
            pos_z = 44,

            move_las = 45
            ;

        int on = 1, off = 0;
        public LaserLine(string _port, int adr=-1)
        {
            port = _port;
            connect(port, baudrate);
            set_adr(adr);
        }
        public double[] parse_pos_z()
        {
            var ret = new double[] { -1, -1 };
            if (response==null) return ret;           
            var resp = response.ToString();
            var lines = resp.Split('\n');                
            for(int i = lines.Length-1; i >=0;i--)
            {
                var l = lines[i];
                l = l.Trim();
                if(l.Contains("cur_pos")&& l.Contains("q"))
                {
                    
                    for(int k=0; k<5; k++)
                    {
                        l = l.Replace("  ", " ");
                    }
                    var resp_vals = l.Split();

                    var pos_str = resp_vals[1].Trim();
                    var pos_int = Convert.ToInt32(pos_str);
                    ret[0] = pos_int;
                    if(resp_vals.Length==4)
                    {
                        var pos_movm_str = resp_vals[2].Trim();
                        var pos_movm_int = Convert.ToDouble(pos_movm_str);
                        ret[1] = pos_movm_int;
                    }
                    response.Clear();
                    return ret;
                }
                    

            }
               
            
            return ret;
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

        public void test()
        {
            send(99, 0);
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
        public void set_rel_pos_z(int rel_pos)
        {
            send(rel_pos + 5000, rel_pos_z);
        }
        public void set_rel_pos_disp(int rel_pos)
        {
            send(rel_pos + 5000, rel_pos_disp);
        }

        public void set_stop_z()
        {
            send(0, stop_z);
        }
        public void set_stop_las()
        {
            send(0, stop_las);
        }
        public void set_pos_las(double pos)
        {
            send((int)pos, pos_las);
        }
        public void set_pos_z(double pos)
        {
            send((int)pos, pos_z);
        }
        public void set_stop_las(int pos)
        {
            send(0, stop_las);
        }
        public void setShvpVel(double _vel)
        {
            if(_vel>0) send(comp_vel_div(_vel), shvp_vel);
        }

        public void setLaserCur(int _pos)
        {
            send(_pos, laser_cur);
        }

        public void set_send_poses(int status)
        {
            send(status, send_poses);
        }
        public void set_temp_control(int _val)//0 or 1
        {
            send( _val,temp_control);
        }
        public void set_temp_value(double _val)//0...37
        {
            send((int)(_val),temp_value );// [degree*10]
        }
        public void set_led_r(int _val)//0 ... 255 pwm
        {
            send(_val,led_r);
        }
        public void set_led_b(int _val)//0 ... 255 pwm
        {
            send(_val,led_b);
        }
        public void set_led_g(int _val)//0 ... 255 pwm
        {
            send(_val,led_g);
        }
        public void set_hyst_temp(int _val)//0 ... 5 degree
        {
            send(_val, hyst_temp);
        }
        public void set_cool_pwm(int _val)//0 ... 255 pwm
        {
            send(_val, cool_pwm);
        }
        public void set_heat_pwm(int _val)//0 ... 255 pwm
        {
            send(_val, heat_pwm);
        }
        //-------------------------------------------
        public void set_comp_cycle_type(int _type)
        {
            send(_type, cycle_type);
        }
        public void set_comp_cycle_ampl(double _ampl)//mm
        {
            int val =(int)(_ampl * 10);
            send(val, cycle_ampl);
        }
        public void set_comp_cycle_time(double _time)//sec
        {
            int val = (int)(_time * 10);
            send(val, cycle_time);
        }
        public void set_comp_cycle_time_rel(double _time)//sec
        {
            int val = (int)(_time * 10);
            send(val, cycle_time_rel);
        }
        public void set_pos_disp(double _pos)//mm
        {
            //int val = (int)(_pos * 10);
            send((int)_pos+5000, pos_disp);
        }

        public void set_valve_pos(int _val)
        {
            send(_val, valve_val);
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

        static public string get_las_pos_time(LaserLine laserLine,bool returned = false)
        {
           
            if(laserLine == null || returned)
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
        public void set_move_z(double _pos)
        {
            send((int)((_pos * steps_per_unit_z+5000) / 10), move_z);
        }
        public void set_move_las(double _pos)
        {
            send((int)((_pos * steps_per_unit_las + 5000) / 10), move_las);
        }

        public void set_drill_dir(int dir)//0 or 1
        {
            send(dir, drill_dir);
        }
        public void set_drill_vel(int vel)//0...255
        {
            send(vel, drill_vel);
        }
        public void set_water_vel(int vel)//0...255
        {
            send(vel, water_vel);
        }
        public void set_z_div(int _vel)
        {
            if (_vel >= 0) send(_vel, div_z);
        }

        public void set_las_div(int _vel)
        {
            if (_vel >= 0) send(_vel, div_las);
        }
        public void set_home_z()
        {
            send(0, home_z);
        }
        public void push_forward()
        {
            send(0, push_forw);
        }
        public void push_back_()
        {
            send(0, push_back);
        }
        public static int vel_div(double vel_nos, double d_nos, double d_syr)
        {
            double vel = (vel_nos * d_nos * d_nos) / (d_syr * d_syr);//vel pist
            return vel_pist_to_ard(vel);
        }

        public static int vel_pist_to_ard(double vel)
        {
            double nT = 5000;  //  #timer freq
            double p = 1;//     #step mm
            double rev = 200 * 16;// * (60d / 16d); //# - reduct steps per revol
            int st = (int)((nT * p) / (vel * rev));
            //vel = (nT * p) / (st * rev);
            return st;
        }

        public static int vel_pist_to_ard(double vel, double nT = 5000,double p = 1,double rev = 200 * 16)//mm/s to div
        {
            int st = (int)((nT * p) / (vel * rev));
            //vel = (nT * p) / (st * rev);
            return st;
        }
    }
}
