using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Ports;
using System.IO;
using System.Windows.Forms;

namespace opengl3
{
    public class DeviceArduino
    {
        protected SerialPort serialPort;
        protected bool isConnected = false;
        public int adr = -1;
        static protected StringBuilder response;
        protected DeviceArduino()
        {
            serialPort = new SerialPort();
        }
        protected bool connect(string port, int baudrate)
        {
            serialPort = new SerialPort();
            try
            {

                serialPort.PortName = port;
                serialPort.BaudRate = baudrate;
                serialPort.RtsEnable = true;
                serialPort.DtrEnable = true;
                Console.WriteLine("try " + port + " " + baudrate);
                serialPort.Open();
                isConnected = true;
                Console.WriteLine("open " + port + " " + baudrate);
                return true;
            }
            catch (Exception ex) 
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

       public static string find_ports(ComboBox comboBox, bool last = false)//not work
        {
            //comboBox_portsArd.Items.Add("COM3");
            // comboBox_portsArd.Items.Clear();
            string port = "";
            comboBox.BeginInvoke((MethodInvoker)(() => comboBox.Items.Clear()));
            // Получаем список COM портов доступных в системе
            string[] portnames = SerialPort.GetPortNames();
            // Проверяем есть ли доступные
            if (portnames.Length == 0)
            {
                MessageBox.Show("COM PORT not found");
            }
            foreach (string portName in portnames)
            {
                //добавляем доступные COM порты в список           
                comboBox.BeginInvoke((MethodInvoker)(() => comboBox.Items.Add(portName)));
                //Console.WriteLine(portnames.Length);
                if (portnames[0] != null)
                {
                    comboBox.BeginInvoke((MethodInvoker)(() => comboBox.SelectedItem = portnames[0]));
                    port = portnames[0];
                    if (last)
                    {
                        comboBox.BeginInvoke((MethodInvoker)(() => comboBox.SelectedItem = portnames[portnames.Length - 1]));
                        port = portnames[portnames.Length - 1];

                    }
                }
               
            }
            return port;
        }

        public void set_adr(int adr)
        {
            this.adr = adr;
        }
        protected void close()
        {
            if(serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }
        public void send(int val, int var)
        {

            //Console.WriteLine("send: "+val+" " + var);
           
                
            string Mes1 = "0";
            string Mes2 = "0";
            string Mes3 = "0";
            // try
            {
                if (adr < 0) adr = 0;
                
                Mes1 = Convert.ToString(val);
                Mes2 = Convert.ToString(var);
                Mes3 = Convert.ToString(adr);
                
            }
            //catch
            {
                // Console.WriteLine("except convert");
            }
            
            //Console.WriteLine("Mes12.Length: " + Mes1.Length + " " + Mes2.Length);
            Console.WriteLine(Mes1 + " " + Mes2 + " " + Mes3 + " ");
            if (Mes1.Length <= 4 && Mes2.Length <= 2)
            {
                while (Mes1.Length < 4)
                {
                    Mes1 = "0" + Mes1;
                }
                while (Mes2.Length < 2)
                {
                    Mes2 = "0" + Mes2;
                }
                while (Mes3.Length < 2)
                {
                    Mes3 = "0" + Mes3;
                }
                Mes1 = "b" + Mes1 + Mes2;
                if(adr>=0)
                {
                    Mes1 += Mes3;
                }
                //try
                {
                 Console.WriteLine("Out: " + Mes1);
                if(this.serialPort.IsOpen)
                {
                    this.serialPort.WriteLine(Mes1);
                    Thread.Sleep(5);
                    var resp = reseav();
                    Console.WriteLine(resp);
                }
                            

                    // Console.WriteLine("resp:\n"+resp);
                }
                // catch
                {
                    // Console.WriteLine("except sending");
                }

            }
            else
            {
                Console.WriteLine("message lengh too long");
            }
               
            
        }
        
        public  string reseav()
        {
            /*var res = "";
            while(serialPort.BytesToRead>0)
            {
                res += serialPort.ReadLine();
            }*/
            //return res;
            if (response == null) response = new StringBuilder();
            //response.Clear();
            var res = serialPort.ReadExisting();
            //serialPort.
            if(res!=null) response.Append(res);

            return res;
            
        }

        private async void func(int val,int var)
        {
            while (isConnected == true)
            {
                await Task.Delay(20);
                if (val != 0 && var != 0)
                {
                    string Mes1 = "0";
                    string Mes2 = "0";
                    try
                    {
                        Mes1 = Convert.ToString(val);
                        Mes2 = Convert.ToString(var);
                    }
                    catch
                    {
                        Console.WriteLine("except convert");
                    }
                    if (Mes1.Length <= 3 && Mes2.Length <= 2)
                    {



                        while (Mes1.Length < 3)
                        {
                            Mes1 = "0" + Mes1;
                        }
                        while  (Mes2.Length < 2)
                        {
                            Mes2 = "0" + Mes2;
                        }
                        Mes1 = "b" + Mes1 + Mes2;
                        try
                        {
                            Console.WriteLine("Out: " + Mes1);
                            this.serialPort.WriteLine(Mes1);
                        }
                        catch
                        {
                            Console.WriteLine("except sending");
                        }

                    }
                    else
                    {
                        Console.WriteLine("message lengh too long");
                    }
                    val = 0;
                    var = 0;
                }

            }
        }

        
    }
}
