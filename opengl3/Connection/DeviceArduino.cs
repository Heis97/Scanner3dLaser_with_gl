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

        protected DeviceArduino()
        {
            serialPort = new SerialPort();
        }
        protected bool connect(string port, int baudrate)
        {
            try
            {
                serialPort.PortName = port;
                serialPort.BaudRate = baudrate;
                serialPort.Open();
                isConnected = true;
                Console.WriteLine("open " + port + " " + baudrate);
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected void close()
        {
            if(serialPort.IsOpen)
            {
                serialPort.Close();
            }
        }
        protected void send(int val, int var)
        {
            if (isConnected)
            {
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
                        Mes1 = "b" + Mes1 + Mes2;
                        try
                        {
                            Console.WriteLine("Out: " + Mes1);
                            this.serialPort.WriteLine(Mes1);
                           // Thread.Sleep(5);
                           // Console.WriteLine(reseav());
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
                }
            }
        }
        public  string reseav()
        {
            return serialPort.ReadExisting();
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
                        while (Mes2.Length < 2)
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
        private void arduinoButton_Click(object sender, EventArgs e)
        {
            /*comboBox.Items.Clear();
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
                 comboBox.Items.Add(portName);
                 //Console.WriteLine(portnames.Length);
                 if (portnames[0] != null)
                 {
                     comboBox.SelectedItem = portnames[0];
                 }
             }*/
        }

        /*async void func()
        {
            string a = "";
            while (true)
            {
                if (isConnected == true)
                {
                    await Task.Delay(20);
                    //Console.WriteLine(Mes[2]);
                    int rows = Mes.GetUpperBound(0) + 1;
                    for (int i1 = 0; i1 < rows; i1++)
                    {
                        if (Mes[i1, 2] == 1)
                        {
                            string Mes1 = Convert.ToString(Mes[i1, 0]);
                            string Mes2 = Convert.ToString(Mes[i1, 1]);
                            while (Mes1.Length < 3)
                            {
                                Mes1 = "0" + Mes1;
                            }
                            while (Mes2.Length < 2)
                            {
                                Mes2 = "0" + Mes2;
                            }
                            Mes1 = "b" + Mes1 + Mes2;
                            Console.WriteLine("Out: " + Mes1);
                            this.serialPort.WriteLine(Mes1);
                            try
                            {
                                await Task.Delay(2);
                                string a1 = this.serialPort.ReadLine();
                                int a2 = Convert.ToInt32(a1);
                                if (a2 != 9)
                                {
                                    len_t--;
                                    Mes[i1, 2] = 0;
                                    Mes[i1, 1] = 0;
                                    Mes[i1, 0] = 0;
                                }
                            }
                            catch
                            {

                                Console.WriteLine("Catch_1");
                            }
                        }
                    }
                    try
                    {
                        a = this.serialPort.ReadLine();
                        Console.WriteLine("In: " + a);
                        mes_out = Convert.ToInt32(a);
                        Invoke(delegate_Gui);
                    }
                    catch
                    {
                        Console.WriteLine("Catch_2");
                    }
                }
            }
        }
        */
    }
}
