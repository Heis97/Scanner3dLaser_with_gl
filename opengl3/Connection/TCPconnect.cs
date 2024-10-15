using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace opengl3
{

    class TCPclient
    {
        private  int _port;
        private static string _server;
        private static TcpClient _client = new TcpClient();
        private static StringBuilder _response;
        private static NetworkStream _stream;
        public TCPclient()
        {

        }
        public TCPclient(int port, string server)
        {
            _port = port;
            _server = server;
        }
        public void Connection(int port, string server)
        {
            try
            {
                Console.WriteLine("connect");
                _client.Connect(server, port);
                Console.WriteLine("con");
                _response = new StringBuilder();
                Console.WriteLine("get");
                _stream = _client.GetStream();
            }
            catch
            {

            }
            
        }
        public string reseav()
        {
            byte[] data = new byte[1024];
            if(_response == null) _response = new StringBuilder();
            _response.Clear();
            if(_stream.DataAvailable)
            {
                do
                {
                    int bytes = _stream.Read(data, 0, data.Length);
                    _response.Append(Encoding.UTF8.GetString(data, 0, bytes));

                }
                while (_stream.DataAvailable);
                
                return _response.ToString();
            }
            else
            {
                return null;
            }
        }
        public void send_mes(string send_prog)
        {            
            byte[] send = System.Text.Encoding.UTF8.GetBytes(send_prog);
            _stream?.Write(send, 0, send.Length);
            Console.WriteLine(send_prog);
        }
        public void close_con()
        {
            if(_client.Connected)
            {
                _stream.Close();
                _client.Close();
            }            
        }

        public bool is_connect()
        {
            return _client.Connected;
        }


        public void connect_leg(string send_prog)
        {
            try
            {
                TcpClient client = new TcpClient();
                client.Connect(_server, _port);

                byte[] data = new byte[256];
                StringBuilder response = new StringBuilder();
                NetworkStream stream = client.GetStream();

                do
                {
                    int bytes = stream.Read(data, 0, data.Length);
                    response.Append(Encoding.UTF8.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable); // пока данные есть в потоке
                byte[] send = System.Text.Encoding.UTF8.GetBytes(send_prog);
                stream.Write(send, 0, send.Length);
                Console.WriteLine(response.ToString());

                // Закрываем потоки
                //stream.Close();
                //client.Close();
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: {0}", e.Message);
            }

            Console.WriteLine("Запрос завершен...");
            Console.Read();
        }
    }

    public class TCPserver
    {
        int port; // порт для прослушивания подключений
        string buffer_in;
        string buffer_out;
        private static StringBuilder _response;
        private static NetworkStream _stream;
        public TCPserver(int _port)
        {
            port = _port;
            buffer_in = "";
            buffer_out = "";
        }
        public string getBuffer()
        {
            var ret = buffer_in;
            buffer_in = "";
            return ret;
        }

        public void pushBuffer(string data)
        {
            buffer_out += data;
        }
        public void send_mes(string send_prog)
        {

            byte[] send = System.Text.Encoding.UTF8.GetBytes(send_prog);
            _stream.Write(send, 0, send.Length);
        }
        public string reseav()
        {
            byte[] data = new byte[1024];
            _response.Clear();
            if (_stream.DataAvailable)
            {
                do
                {
                    int bytes = _stream.Read(data, 0, data.Length);
                    _response.Append(Encoding.UTF8.GetString(data, 0, bytes));

                }
                while (_stream.DataAvailable);

                return _response.ToString();
            }
            else
            {
                return null;
            }
        }
        public void startServer()
        {
            TcpListener server = null;
            try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);
                server.Start();
                Console.WriteLine("start server");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("client connected");
                _stream = client.GetStream();
                _response = new StringBuilder();
                while (true)
                {
                    buffer_in += reseav();
                    if(buffer_out.Length>0)
                    {
                        send_mes(buffer_out);
                        buffer_out = "";
                    }
                   
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                if (server != null)
                    server.Stop();
            }
        }
    }
}
