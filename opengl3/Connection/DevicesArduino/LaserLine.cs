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
        int baudrate = 500000;
        int laser = 1, power = 2;
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

    }
}
