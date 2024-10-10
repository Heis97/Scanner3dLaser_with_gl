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

        public string get_cur_pos()
        {
            JKTYPE.CartesianPose tcp_pos = new JKTYPE.CartesianPose();
            jakaAPI.get_tcp_position(ref handle, ref tcp_pos);
            JKTYPE.JointValue j_pos = new JKTYPE.JointValue();
            jakaAPI.get_joint_position(ref handle, ref j_pos);
            return
                "tcp: " + tcp_pos.tran.x + "; " + tcp_pos.tran.y + "; " + tcp_pos.tran.z + ";\n " +
                " " + tcp_pos.rpy.rx + "; " + tcp_pos.rpy.ry + "; " + tcp_pos.rpy.rz + "; " +
                "joint: " + j_pos.jVal[0] + "; " + j_pos.jVal[1] + "; " + j_pos.jVal[2] + "; " +
                j_pos.jVal[3] + "; " + j_pos.jVal[4] + "; " + j_pos.jVal[5] + "; ";
        }

        public void move_lin(double x = 0, double y = 0, double z = 0, double rx = 0, double ry = 0, double rz = 0, double vel= 1)
        {
            JKTYPE.CartesianPose tcp_pos = new JKTYPE.CartesianPose();
            tcp_pos.tran.x = x; tcp_pos.tran.y = y; tcp_pos.tran.z = z; 
            tcp_pos.rpy.rx = rx; tcp_pos.rpy.ry = ry; tcp_pos.rpy.rz = rz;
            jakaAPI.linear_move(ref handle, ref tcp_pos, JKTYPE.MoveMode.ABS, false, vel);
        }
        public void move_lin_rel(double x = 0, double y = 0, double z = 0, double rx  = 0, double ry = 0, double rz = 0, double vel = 1)
        {
            JKTYPE.CartesianPose tcp_pos = new JKTYPE.CartesianPose();
            tcp_pos.tran.x = x; tcp_pos.tran.y = y; tcp_pos.tran.z = z; 
            tcp_pos.rpy.rx = rx; tcp_pos.rpy.ry = ry; tcp_pos.rpy.rz = rz;
            jakaAPI.linear_move(ref handle, ref tcp_pos, JKTYPE.MoveMode.INCR, false, vel);
        }
        public void move_joint(JKTYPE.JointValue j_pos)
        {
            jakaAPI.joint_move(ref handle, ref j_pos, JKTYPE.MoveMode.ABS, false, 0.1);

        }

        public void move_home()
        {
            JKTYPE.JointValue j_pos = new JKTYPE.JointValue();
            //j_pos.jVal[0] = 
            jakaAPI.joint_move(ref handle, ref j_pos, JKTYPE.MoveMode.ABS, false, 0.1);

        }

    }
}
