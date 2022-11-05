using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            laser_sensor = 8;
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
            send(_pos+500, posit);
        }

        public void setShvpVel(double _vel)
        {
            send(comp_vel_div(_vel), shvp_vel);
        }
        public void setLaserCur(int _pos)
        {
            send(_pos, laser_cur);
        }
        public void onLaserSensor()
        {
            send(on, laser_sensor);
        }
        public void offLaserSensor()
        {
            send(off, laser_sensor);
        }
        static int comp_vel_div(double vel)
        {
            return Math.Abs((int)(1000 / vel));
        }
    }
}
