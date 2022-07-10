using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace opengl3
{
    public class DeviceMarlin : DeviceArduino
    {
        string port;
        int baudrate = 115200;
        int cur_com = 1;

        
        public DeviceMarlin(string _port)
        {
            port = _port;
            connect(port, baudrate);
            serialPort.RtsEnable = true;
            serialPort.DtrEnable = true;
        }

        int calcSum(string command)
        {
            int sum = 0;
            foreach (var symb in command) sum ^= symb;
            return sum;
        }


        void sendCommand(string com,string[] vars, object[] vals)
        {
            if (serialPort==null)
            {
                return;
            }
            if (!serialPort.IsOpen)
            {
                return;
            }
            if (vars.Length != vals.Length)
            {
                return;
            }
            var command = "N"+ cur_com.ToString()+ " "+ com;
            for(int i=0; i<vars.Length;i++)
            {
                command += " " + vars[i] + vals[i].ToString();
            }
            command += "*" + calcSum(command).ToString()+"\n";
            Console.WriteLine(command);
            serialPort.Write(command);
            cur_com++;
            var res_all =reseav();
            Console.WriteLine(res_all);
            try
            {
                var res_all_arr = res_all.Split('\n');
                foreach(var res in res_all_arr)
                {
                    if (res.Contains("Resend"))
                    {
                        var ind_err = res.IndexOf("Resend");
                        var res_sub = res.Substring(ind_err);
                        var res_spl = res_sub.Split(':');
                        
                        var err = Convert.ToInt32(res_spl[1].Trim());
                        cur_com = err;
                    }
                }                
            }
            catch
            {

            }
            
        }

        void sendXpos(double pos, double vel_st_min = 4000)
        {
            sendCommand("M92", new string[] { "X"}, new object[] { 1 });
            sendCommand("G1",new string[] { "X","F" }, new object[] { pos, vel_st_min });
        }


        public bool connectStart()
        {
            return connect(port, baudrate);
        }
        public void connectStop()
        {
            close();
        }

        public void setShvpPos(double _pos,double _vel = 4000)
        {
            sendXpos(_pos,_vel);
        }

    }
}
