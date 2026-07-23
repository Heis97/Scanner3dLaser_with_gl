using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Windows.Forms;

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
                _client =  new TcpClient();
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
        string buffer_in = "";
        string buffer_out = "";
        private static StringBuilder _response;
        private static NetworkStream _stream;
        public bool connected = false;
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
            //Console.Write(data);
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

        public void handle()
        {
            var res = reseav();
            if (res != null)
            {

                if (res.Length > 3)
                {
                    //Console.WriteLine(res);
                    buffer_in += res;
                }
            }

            if (buffer_out.Length > 0)
            {
                send_mes(buffer_out);
                buffer_out = "";
            }
        }


        public void startServer()
        {
            TcpListener server = null;
            //try
            {
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);
                server.Start();
                Console.WriteLine("start server");
                TcpClient client = server.AcceptTcpClient();
                Console.WriteLine("client connected");
                connected = true;
                _stream = client.GetStream();
                _response = new StringBuilder();
                while (true)
                {


                    handle();


                }
            }
            //catch (Exception e)
            {
               // Console.WriteLine(e.Message);
            }
            //finally
            {
               // if (server != null)
                    //server.Stop();
            }
        }
    }



    public class TcpClientWrapper : IDisposable
    {
        private TcpClient _client;
        private NetworkStream _stream;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _readerTask;
        private readonly byte[] _readBuffer = new byte[4096];
        private readonly StringBuilder _messageBuilder = new StringBuilder();

        public event Action<string> MessageReceived;
        public event Action Disconnected;
        public bool IsConnected => _client?.Connected == true;

        public async Task ConnectAsync(string host, int port)
        {
            if (IsConnected)
                throw new InvalidOperationException("Клиент уже подключён.");

            _client = new TcpClient();
            await _client.ConnectAsync(host, port);
            _stream = _client.GetStream();
            _readerTask = Task.Run(ReaderLoopAsync);
        }

        public async Task SendAsync(string message)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Клиент не подключён.");

            byte[] data = Encoding.UTF8.GetBytes(message + "\n ");
            await _stream.WriteAsync(data, 0, data.Length);
        }

        public async Task DisconnectAsync()
        {
            if (!IsConnected) return;

            _cts.Cancel();
            if (_readerTask != null)
                await _readerTask;

            _stream?.Close();
            _client?.Close();
            Disconnected?.Invoke();
        }
        private async Task ReaderLoopAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested && IsConnected)
                {
                    int bytesRead = await _stream.ReadAsync(_readBuffer, 0, _readBuffer.Length, _cts.Token);
                    if (bytesRead == 0) break;

                    string chunk = Encoding.UTF8.GetString(_readBuffer, 0, bytesRead);
                    try
                    {
                        OnMessageReceived(chunk);
                    }
                    catch (Exception ex)
                    {
                        // Логируем ошибку обработки, но НЕ закрываем соединение
                        System.Diagnostics.Debug.WriteLine($"Ошибка в обработчике сообщения: {ex.Message}");
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка чтения: {ex.Message}");
            }
            finally
            {
                if (IsConnected)
                {
                    _client.Close();
                    Disconnected?.Invoke();
                }
            }
        }
        private async Task ReaderLoopAsync_v1()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested && IsConnected)
                {
                    int bytesRead = await _stream.ReadAsync(_readBuffer, 0, _readBuffer.Length, _cts.Token);
                    if (bytesRead == 0) break;

                    string chunk = Encoding.UTF8.GetString(_readBuffer, 0, bytesRead);
                    OnMessageReceived(chunk);

                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                // Логируйте при необходимости
                System.Diagnostics.Debug.WriteLine($"Ошибка чтения: {ex.Message}");
            }
            finally
            {
                if (IsConnected)
                {
                    _client.Close();
                    Disconnected?.Invoke();
                }
            }
        }

        /*private int _totalBytes = 0;
        private int _packetCount = 0;
        private DateTime _lastLog = DateTime.UtcNow;
        private async Task ReaderLoopAsync()
        {
            try
            {
                while (!_cts.Token.IsCancellationRequested && IsConnected)
                {
                    int bytesRead = await _stream.ReadAsync(_readBuffer, 0, _readBuffer.Length, _cts.Token);
                    if (bytesRead == 0) break;
                    Interlocked.Add(ref _totalBytes, bytesRead);
                    Interlocked.Increment(ref _packetCount);

                    // Раз в секунду выводим статистику
                    if ((DateTime.UtcNow - _lastLog).TotalSeconds >= 1)
                    {
                        int packets = Interlocked.Exchange(ref _packetCount, 0);
                        int bytes = Interlocked.Exchange(ref _totalBytes, 0);
                        Console.WriteLine($"Пакетов/с: {packets}, байт/с: {bytes}");
                        _lastLog = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception ex)
            {
                // Логируйте при необходимости
                System.Diagnostics.Debug.WriteLine($"Ошибка чтения: {ex.Message}");
            }
            finally
            {
                if (IsConnected)
                {
                    _client.Close();
                    Disconnected?.Invoke();
                }
            }
            
        }*/

        protected virtual void OnMessageReceived(string message)
        {
            MessageReceived?.Invoke(message);
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _stream?.Dispose();
            _client?.Dispose();
        }
    }
}
