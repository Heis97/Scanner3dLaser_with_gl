
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Accord.IO;
using Emgu.CV.PpfMatch3d;
using jakaApi;
using jkType;



namespace opengl3
{
    class RobotJaka
    {
        int handle = -1;
        List<JKTYPE.CartesianPose> tcp_positions = new List<JKTYPE.CartesianPose>();
        public RobotJaka() 
        { 

        }
        public void on()
        {
            int result = jakaAPI.create_handler("10.5.5.100", ref handle);
            jakaAPI.power_on(ref handle);
            jakaAPI.enable_robot(ref handle);
        }
        public void off() 
        {
            jakaAPI.disable_robot(ref handle);
            jakaAPI.power_off(ref handle);
            jakaAPI.destory_handler(ref handle); ;
        }
        public void stop()
        {
            jakaAPI.motion_abort(ref handle);
        }

        public bool is_move()
        {
            bool is_move = false;
            jakaAPI.is_in_pos(ref handle, ref is_move);
            return !is_move;
        }
        public string get_cur_pos()
        {
            JKTYPE.CartesianPose tcp_pos = new JKTYPE.CartesianPose();
            jakaAPI.get_tcp_position(ref handle, ref tcp_pos);
            JKTYPE.JointValue j_pos = new JKTYPE.JointValue();
            jakaAPI.get_joint_position(ref handle, ref j_pos);
            return
                "tcp: " + tcp_pos.tran.x + "; " + tcp_pos.tran.y + "; " + tcp_pos.tran.z + ";\n " +
                " " + tcp_pos.rpy.rx + "; " + tcp_pos.rpy.ry + "; " + tcp_pos.rpy.rz + "; ";
        }
        public JKTYPE.CartesianPose get_cur_tcp_pos()
        {
            JKTYPE.CartesianPose tcp_pos = new JKTYPE.CartesianPose();
            jakaAPI.get_tcp_position(ref handle, ref tcp_pos);
            return tcp_pos;
        }

        public void move_lin(double x = 0, double y = 0, double z = 0, double rx = 0, double ry = 0, double rz = 0, double vel= 10)
        {
            JKTYPE.CartesianPose tcp_pos = new JKTYPE.CartesianPose();
            tcp_pos.tran.x = x; tcp_pos.tran.y = y; tcp_pos.tran.z = z; 
            tcp_pos.rpy.rx = rx; tcp_pos.rpy.ry = ry; tcp_pos.rpy.rz = rz;
            jakaAPI.linear_move(ref handle, ref tcp_pos, JKTYPE.MoveMode.ABS, false, vel);
        }

        public void move_lin_rel(double x = 0, double y = 0, double z = 0, double rx  = 0, double ry = 0, double rz = 0, double vel = 20)
        {
            JKTYPE.CartesianPose tcp_pos = new JKTYPE.CartesianPose();
            tcp_pos.tran.x = x; tcp_pos.tran.y = y; tcp_pos.tran.z = z; 
            tcp_pos.rpy.rx = rx; tcp_pos.rpy.ry = ry; tcp_pos.rpy.rz = rz;
            JKTYPE.OptionalCond cond = new JKTYPE.OptionalCond();
            var acs = vel * 2;
            jakaAPI.linear_move_extend_ori(ref handle, ref tcp_pos, JKTYPE.MoveMode.INCR, false, vel,acs,0,ref cond,1,1);
             

        }

        public void move_lin_rel_or(double x = 0, double y = 0, double z = 0, double rx = 0, double ry = 0, double rz = 0,
            double vel = 0.6,double draif_dist = 0.4)
        {
            move_lin_rel(draif_dist, 0, 0,rx, ry, rz, vel);
            move_lin_rel(-draif_dist, 0, 0, 0, 0, 0, vel);
        }
        public void move_lin_abs_from_list(int i = 0, double vel = 20,bool waiting = false)
        {
            if (i == 0) { move_lin_abs(tcp_positions[i], vel); return; }

            if(
                tcp_positions[i].tran.x - tcp_positions[i - 1].tran.x != 0 || 
                tcp_positions[i].tran.y - tcp_positions[i - 1].tran.y != 0 || 
                tcp_positions[i].tran.z - tcp_positions[i - 1].tran.z != 0)
            {
                move_lin_abs(tcp_positions[i], vel);
            }
            else
            {
                double vel_or = 0.8;
                double draif_dist = 0.4;
                var pos = tcp_positions[i];
                pos.tran.x += draif_dist;
                move_lin_abs(pos, vel_or);
                move_lin_abs(tcp_positions[i], vel_or);
            }
        }
        void move_lin_abs(JKTYPE.CartesianPose tcp, double vel = 10)
        {
            JKTYPE.OptionalCond cond = new JKTYPE.OptionalCond();
            var acs = vel * 2;
            jakaAPI.linear_move_extend_ori(ref handle, ref tcp, JKTYPE.MoveMode.ABS, false, vel, acs, 0, ref cond, 1, 1);
        }
        public void move_joint(JKTYPE.JointValue j_pos)
        {
            jakaAPI.joint_move(ref handle, ref j_pos, JKTYPE.MoveMode.ABS, false, 0.1);
        }

        public void move_home()
        {
            JKTYPE.JointValue j_pos = new JKTYPE.JointValue();
            j_pos.jVal = new double[6];
            j_pos.jVal[0] = -1.57063212457686;
            j_pos.jVal[1] = -0.0140405350859115;
            j_pos.jVal[2] = 2.66448463450521;
            j_pos.jVal[3] = 2.09757353830799;
            j_pos.jVal[4] = 3.09867903153056E-05;
            j_pos.jVal[5] = 0.000118478433665371;
            jakaAPI.joint_move(ref handle, ref j_pos, JKTYPE.MoveMode.ABS, false, 0.1);

        }

        public void move_work()
        {
            JKTYPE.JointValue j_pos = new JKTYPE.JointValue();
            j_pos.jVal = new double[6];
            j_pos.jVal[0] = -1.57063212457686;
            j_pos.jVal[1] = 1.41449105349;
            j_pos.jVal[2] = 1.687350859828521;
            j_pos.jVal[3] = 3.18994761932799;
            j_pos.jVal[4] = 1.54737418072;
            j_pos.jVal[5] = 0.000118478433665371;
            jakaAPI.joint_move(ref handle, ref j_pos, JKTYPE.MoveMode.ABS, false, 0.1);
        }

        
        public void example_user_frame()
        {
            /*
             *@brief Set the parameter of specified user coordinate system
             *@param handle Robot control handle
            * @param id The value range of the user coordinate system number is [1,10]
            * @param user_frame Offset value of user coordinate frame
            * @param name Alias of user coordinate frame
            * @return ERR_SUCC Error or Success
            */

            int id_ret = 0;
            int id_set = 2;

            JKTYPE.CartesianPose tcp_ret = new JKTYPE.CartesianPose();
            JKTYPE.CartesianPose tcp_set = new JKTYPE.CartesianPose();
            char[] name = new char[50];
            name = "test".ToCharArray();
          
     
            
            //Interrogate user coordinate system ID currently in use
            jakaAPI.get_user_frame_id(ref handle, ref id_ret);


            //Get user coordinate system info currently in use
            jakaAPI.get_user_frame_data(ref handle, id_ret, ref tcp_ret);
            Console.WriteLine("id_using={0} \nx={1}, y={2}, z={3}\n", id_ret, tcp_ret.tran.x, tcp_ret.tran.y, tcp_ret.tran.y);
            Console.WriteLine("rx={0}, ry={1}, rz={2}\n", tcp_ret.rpy.rx, tcp_ret.rpy.ry, tcp_ret.rpy.rz);
            //Initialize user coordinate system
            tcp_set.tran.x = 0; tcp_set.tran.y = 0; tcp_set.tran.z = 0;
            tcp_set.rpy.rx = 0; tcp_set.rpy.ry =  Math.PI /2; tcp_set.rpy.rz = 0;
            //Set user coordinate system info
            jakaAPI.set_user_frame_data(ref handle, id_set, ref tcp_set, name);
            //Switch the coordinates of the currently used user coordinate system
            jakaAPI.set_user_frame_id(ref handle, id_set);
            System.Threading.Thread.Sleep(1000);
            //Interrogate user coordinate system ID currently in use
            jakaAPI.get_user_frame_id(ref handle, ref id_ret);
            //Get user coordinate system info
            jakaAPI.get_user_frame_data(ref handle, id_ret, ref tcp_ret);
            Console.WriteLine("id_using={0} \nx={1}, y={2}, z={3}\n", id_ret, tcp_ret.tran.x, tcp_ret.tran.y, tcp_ret.tran.y);
            Console.WriteLine("rx={0}, ry={1}, rz={2}\n", tcp_ret.rpy.rx, tcp_ret.rpy.ry, tcp_ret.rpy.rz);

        }

        public void set_user_frame()
        {
            JKTYPE.CartesianPose tcp_set = new JKTYPE.CartesianPose();
            JKTYPE.CartesianPose tcp_ret = new JKTYPE.CartesianPose();

            jakaAPI.get_tcp_position(ref handle, ref tcp_set);
            var str_pos = "tcp: " + tcp_set.tran.x + "; " + tcp_set.tran.y + "; " + tcp_set.tran.z + ";\n " +
                " " + tcp_set.rpy.rx + "; " + tcp_set.rpy.ry + "; " + tcp_set.rpy.rz + "; "; 
            //Console.WriteLine("user_fr: \n "+ str_pos);

            int id_set = 2;
            //JKTYPE.CartesianPose tcp_set = new JKTYPE.CartesianPose();
            char[] name = new char[50];
            name = "test".ToCharArray();

            //tcp_set.tran.x = 0; tcp_set.tran.y = 0; tcp_set.tran.z = 0;
            //tcp_set.rpy.rx = 0; tcp_set.rpy.ry = 0; tcp_set.rpy.rz = 0;
            jakaAPI.set_user_frame_data(ref handle, id_set, ref tcp_set, name);
            jakaAPI.set_user_frame_id(ref handle, id_set);
            System.Threading.Thread.Sleep(1000);

        }

        public void set_zero_frame()
        {
            JKTYPE.CartesianPose tcp_set = new JKTYPE.CartesianPose();
            JKTYPE.CartesianPose tcp_ret = new JKTYPE.CartesianPose();

            /* jakaAPI.get_tcp_position(ref handle, ref tcp_set);
             var str_pos = "tcp: " + tcp_set.tran.x + "; " + tcp_set.tran.y + "; " + tcp_set.tran.z + ";\n " +
                 " " + tcp_set.rpy.rx + "; " + tcp_set.rpy.ry + "; " + tcp_set.rpy.rz + "; ";
             Console.WriteLine("user_fr: \n " + str_pos);

             int id_set = 2;*/
            int id_set = 2;
            //JKTYPE.CartesianPose tcp_set = new JKTYPE.CartesianPose();
            char[] name = new char[50];
            name = "test".ToCharArray();

            tcp_set.tran.x = 0; tcp_set.tran.y = 0; tcp_set.tran.z = 0;
            tcp_set.rpy.rx = 0; tcp_set.rpy.ry = 0; tcp_set.rpy.rz = 0;
            jakaAPI.set_user_frame_data(ref handle, id_set, ref tcp_set, name);
            jakaAPI.set_user_frame_id(ref handle, id_set);
            System.Threading.Thread.Sleep(1000);

        }

        public void set_tool()
        {
            int id_set = 0;
            id_set = 2;
            jakaAPI.set_tool_id(ref handle, id_set);

            JKTYPE.CartesianPose tcp_get = new JKTYPE.CartesianPose();
            JKTYPE.CartesianPose tcp_set = new JKTYPE.CartesianPose();

            var str_pos = "tcp: " + tcp_set.tran.x + "; " + tcp_set.tran.y + "; " + tcp_set.tran.z + ";\n " +
                " " + tcp_set.rpy.rx + "; " + tcp_set.rpy.ry + "; " + tcp_set.rpy.rz + "; ";
            //Console.WriteLine("user_before: \n " + str_pos);

            char[] name = new char[50]; 
            name = "test".ToCharArray(); // Instantiate the bot and switch the ip to your own ip
            //Initialize tool coordinates
            tcp_set.tran.x = 0; tcp_set.tran.y = -390; tcp_set.tran.z = 19; 
            tcp_set.rpy.rx = Math.PI/2; tcp_set.rpy.ry = 0; tcp_set.rpy.rz = 0;

           // tcp_set.tran.x = 0; tcp_set.tran.y = 0; tcp_set.tran.z = 0;
            //tcp_set.rpy.rx =0; tcp_set.rpy.ry = 0; tcp_set.rpy.rz = 0;
            //Set tool data 28.
            jakaAPI.set_tool_data(ref handle, id_set, ref tcp_set, name); 
            //Switch the coordinates of the currently used tool
            jakaAPI.set_tool_id( ref handle, id_set);

            jakaAPI.get_tcp_position(ref handle, ref tcp_get);

           /* str_pos = "tcp: " + tcp_get.tran.x + "; " + tcp_get.tran.y + "; " + tcp_get.tran.z + ";\n " +
                " " + tcp_get.rpy.rx + "; " + tcp_get.rpy.ry + "; " + tcp_get.rpy.rz + "; ";*/
           // Console.WriteLine("user_after: \n " + str_pos);
        }

        public void clean_list()
        {
            tcp_positions = new List<JKTYPE.CartesianPose>();
        }

        public void add_to_list()
        {
            tcp_positions.Add(get_cur_tcp_pos());
        }

        public void divide_list(double div_lin = 0.5,double div_or = 0.02)
        {
            var list_ext = new List<JKTYPE.CartesianPose>();
            list_ext.Add(tcp_positions[0]);
            for (int i=1; i< tcp_positions.Count;i++)
            {
                var dist_cur = dist(tcp_positions[i], tcp_positions[i - 1]);
                var dist_cur_or = dist_or(tcp_positions[i], tcp_positions[i - 1]);
                if (dist_cur > div_lin)
                {
                    var numb = (int)(dist_cur / div_lin);
                    list_ext.AddRange(div_betw_lin(tcp_positions[i-1], tcp_positions[i], numb));
                }
                
                else if (dist_cur_or > div_or)
                {
                    var numb = (int)(dist_cur / div_or);
                    list_ext.AddRange(div_betw_lin(tcp_positions[i-1], tcp_positions[i], numb));
                }
                else
                {
                    list_ext.Add(tcp_positions[i]);
                }
            }
            list_ext.Add(tcp_positions[tcp_positions.Count-1]);
            Console.WriteLine("tcps:");
           
            tcp_positions = list_ext;
            for (int i = 0; i < tcp_positions.Count; i++)
            {
                Console.WriteLine(tcp_to_str(tcp_positions[i]));
            }
            Console.WriteLine("end");
        }

        double dist(JKTYPE.CartesianPose p1, JKTYPE.CartesianPose p2)
        {
            var x = p1.tran.x - p2.tran.x;
            var y = p1.tran.y - p2.tran.y;
            var z = p1.tran.z - p2.tran.z;
            return Math.Sqrt(x*x + y*y + z*z);
        }

        double dist_or(JKTYPE.CartesianPose p1, JKTYPE.CartesianPose p2)
        {
            var x = p1.rpy.rx - p2.rpy.rx;
            var y = p1.rpy.ry - p2.rpy.ry;
            var z = p1.rpy.rz - p2.rpy.rz;
            return Math.Sqrt(x * x + y * y + z * z);
        }
        List<JKTYPE.CartesianPose> div_betw(JKTYPE.CartesianPose p1, JKTYPE.CartesianPose p2,int count)
        {
           
            var list_div = new List<JKTYPE.CartesianPose>();
            if (count < 2)
            {
                list_div.Add(p1);
                return list_div;
            } 
            var x0 = p1.tran.x;
            var y0 = p1.tran.y;
            var z0 = p1.tran.z;

            var rx0 = p1.rpy.rx;
            var ry0 = p1.rpy.ry;
            var rz0 = p1.rpy.rz;
            //-------------------------
            var dx = p2.tran.x - p1.tran.x;
            var dy = p2.tran.y - p1.tran.y;
            var dz = p2.tran.z - p1.tran.z;

            var drx = p2.rpy.rx - p1.rpy.rx;
            var dry = p2.rpy.ry - p1.rpy.ry;
            var drz = p2.rpy.rz - p1.rpy.rz;
            //-----------------------------

            list_div.Add(p1);
            for(int i = 1; i < count; i++)
            {
                JKTYPE.CartesianPose tcp = new JKTYPE.CartesianPose();
                tcp.tran.x = x0 + dx*i/count; tcp.tran.y = y0 + dy * i / count; tcp.tran.z = z0 + dz * i / count;
                tcp.rpy.rx = rx0 + drx * i / count; tcp.rpy.ry = ry0 + dry * i / count; tcp.rpy.rz = rz0 + drz * i / count;
                list_div.Add(tcp);
                Console.WriteLine(tcp_to_str(tcp));
            }


            return list_div;
        }
        List<JKTYPE.CartesianPose> div_betw_lin(JKTYPE.CartesianPose p1, JKTYPE.CartesianPose p2, int count)
        {

            var list_div = new List<JKTYPE.CartesianPose>();
            if (count < 2)
            {
                list_div.Add(p1);
                return list_div;
            }
            var x0 = p1.tran.x;
            var y0 = p1.tran.y;
            var z0 = p1.tran.z;

            var rx0 = p1.rpy.rx;
            var ry0 = p1.rpy.ry;
            var rz0 = p1.rpy.rz;
            //-------------------------
            var dx = p2.tran.x - p1.tran.x;
            var dy = p2.tran.y - p1.tran.y;
            var dz = p2.tran.z - p1.tran.z;

            var drx = p2.rpy.rx - p1.rpy.rx;
            var dry = p2.rpy.ry - p1.rpy.ry;
            var drz = p2.rpy.rz - p1.rpy.rz;
            //-----------------------------

            list_div.Add(p1);
            for (int i = 1; i < count; i++)
            {
                JKTYPE.CartesianPose tcp = new JKTYPE.CartesianPose();
                tcp.tran.x = x0 + dx * i / count; tcp.tran.y = y0 + dy * i / count; tcp.tran.z = z0 + dz * i / count;
                tcp.rpy.rx = rx0; tcp.rpy.ry = ry0; tcp.rpy.rz = rz0;
                list_div.Add(tcp);
                Console.WriteLine(tcp_to_str(tcp));
            }


            return list_div;
        }
        string tcp_to_str(JKTYPE.CartesianPose tcp)
        {
            var pres = 2;
            return Math.Round( tcp.tran.x, pres) + " " + Math.Round(tcp.tran.y, pres) + " " + Math.Round(tcp.tran.z, pres) + " " +
                    Math.Round(tcp.rpy.rx, pres) + " " + Math.Round(tcp.rpy.ry, pres) + " " + Math.Round(tcp.rpy.rz, pres);
        }
        public int get_list_len()
        {
            return tcp_positions.Count;
        }

    }
}
