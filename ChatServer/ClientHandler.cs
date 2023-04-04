using System.IO;
using System.Net.Sockets;

namespace ChatServer
{
    class ClientHandler
    {
        Socket _clientSocket;
        string _nick;
        NetworkStream _networkStream;
        StreamWriter _streamwriter;
        StreamReader _streamreader;
        public bool Dead=false;

        public string Nick { 
            get { return _nick; }
            set { _nick = value; }
        }
        public ClientHandler(Socket clientSocket, string nick)
        {
            _clientSocket = clientSocket;
            _nick = nick;
            _networkStream = new NetworkStream(clientSocket);
            _streamwriter = new StreamWriter(_networkStream);
            _streamreader = new StreamReader(_networkStream);
        }
        public ClientHandler(Socket clientSocket) : 
            this(clientSocket, clientSocket.RemoteEndPoint.ToString())
        {}

        public void SendMessage(string message)
        {
            _streamwriter.WriteLine(message);
            _streamwriter.Flush();
        }

        public string ReceiveMessage()
        {
            return _streamreader.ReadLine();
        }
    }
}