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

        void sendCommand(string[] vars, object[] vals)
        {
            if (vars.Length != vals.Length)
            {
                return;
            }
            var command = "N"+ cur_com.ToString()+ " G1";
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

        void sendXpos(double pos)
        {            
            sendCommand(new string[] { "X","F" }, new object[] { pos,6 });
        }


        public bool connectStart()
        {
            return connect(port, baudrate);
        }
        public void connectStop()
        {
            close();
        }

        public void setShvpPos(double _pos)
        {
            sendXpos(_pos);
        }

    }
}
