using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using jakaApi;
using jkType;

namespace opengl3
{
    class RobotJaka
    {
        int handle = -1;
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

        public void get_cur_pos()
        {
            JKTYPE.CartesianPose tcp_pos = new JKTYPE.CartesianPose();
            jakaAPI.get_tcp_position(ref handle, ref tcp_pos);
            
        }

        public void move_lin(RobotFrame frame)
        {
            JKTYPE.CartesianPose tcp_pos = new JKTYPE.CartesianPose();
            jakaAPI.linear_move(ref handle, ref tcp_pos, JKTYPE.MoveMode.ABS, false, 0.1);

        }


    }
}
