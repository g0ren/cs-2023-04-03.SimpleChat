using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ChatClient
{
    internal class ChatClient : IDisposable
    {
        int _port;
        IPAddress _ipAddress;
        Socket? _socket;
        public int Port { 
            get { return _port; }
            set { _port = value; }
        }
        public ChatClient(IPAddress address,
            int port)
        {
            _ipAddress = address;
            _port = port;
        }

        public void Connect(string? nick=null)
        {
            bool status = true;
            _socket = new Socket(SocketType.Stream, ProtocolType.IP);
            _socket.Connect(new IPEndPoint(IPAddress.Loopback, _port));
            NetworkStream networkStream = new NetworkStream(_socket);
            StreamReader streamreader = new StreamReader(networkStream);
            StreamWriter streamwriter = new StreamWriter(networkStream);
            Task.Factory.StartNew(() =>
            {
                while (_socket.Connected)
                {
                    string? line = streamreader.ReadLine();
                    if (line == "/quit")
                    {
                        status = false;
                        Dispose();
                        return;
                    }
                    Console.WriteLine(line);
                }
            });
            if (nick != null)
            {
                streamwriter.WriteLine("/nick " + nick);
                streamwriter.Flush();
            }
            string message = "";
            while (status)
            {
                message = Console.ReadLine();
                try
                {
                    streamwriter.WriteLine(message);
                    streamwriter.Flush();
                }
                catch(Exception ex)
                {
                    break;
                }
            }
        }

        public void Disconnect()
        {
            if (_socket != null)
            {
                _socket.Close();
            }
        }

        public void Dispose()
        {
            Disconnect();
            if (_socket != null)
            {
                _socket.Dispose();
            }
        }
    }
}