using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace ChatServer
{
    internal class ChatServer : IDisposable
    {
        const int MAX_CLIENTS = 10;
        int _serverPort;
        Socket _serverSocket;
        List<string> _chatLog = new List<string>();
        List<ClientHandler> _clients = new List<ClientHandler>();

        public int ServerPort {
            get
            {
                return _serverPort;
            }
            set
            {
                _serverPort = value;
            }
        }

        public ChatServer(int serverPort)
        {
            _serverPort = serverPort;
        }

        public void SendMessageToChat(string message)
        {
            if (_clients.Any())
            {
                foreach(var client in _clients)
                {
                    if (!client.Dead)
                    {
                        client.SendMessage(message);
                    }
                }
            }
            Console.WriteLine(message);
        }

        void WaitForBroadcast()
        {
            UdpClient server = new UdpClient(12345);
            IPEndPoint remoteIp = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                byte[] data = server.Receive(ref remoteIp); 
                string message = Encoding.ASCII.GetString(data);
                Console.WriteLine("Received broadcast message: " + message);
                string myIP = Dns
                    .GetHostEntry(Dns.GetHostName())
                    .AddressList
                    .First(x => x.AddressFamily == AddressFamily.InterNetwork)
                    .ToString();
                string myEndpoint = myIP +":"+_serverPort;
                byte[] response = Encoding.ASCII.GetBytes(myEndpoint);
                server.Send(response, response.Length, remoteIp);
                Console.WriteLine("Sent endpoint to client: " + myEndpoint);
            }
        }
        
        void Handle(ClientHandler handler)
        {
            bool status = true;
            SendMessageToChat("Client " + handler.Nick + " connected");
            foreach(var line in _chatLog)
            {
                handler.SendMessage(line);
            }
            string message = "";
            while (status)
            {
                message = handler.ReceiveMessage();
                if (message == "/quit")
                {
                    handler.SendMessage("/quit");
                    handler.Dead = true;
                    SendMessageToChat(handler.Nick + " left the chat");
                    return;
                }
                string chatLine = handler.Nick + ": " + message;
                _chatLog.Add(chatLine);
                SendMessageToChat(chatLine);
                if (message.StartsWith("/nick "))
                {
                    string newNick = Regex.Replace(message, @"^/nick\s*", "");
                    SendMessageToChat(handler.Nick + " is now known as " + newNick);
                    handler.Nick = newNick;
                }
            }
        }
  
        public void Start()
        {
            _serverSocket = new Socket(SocketType.Stream, ProtocolType.IP);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Loopback, _serverPort));
            _serverSocket.Listen(MAX_CLIENTS);
            Console.WriteLine("Waiting for a connection...");
            Task.Factory.StartNew(WaitForBroadcast);
            while (true)
            {
                Socket client = _serverSocket.Accept();
                ClientHandler handler = new ClientHandler(client);
                _clients.Add(handler);
                Task.Factory.StartNew(() => Handle(handler));
            }
        }

        public void Stop()
        {
            _serverSocket?.Close();
        }

        public void Dispose()
        {
            Stop();
            _serverSocket?.Dispose();
        }
    }
}