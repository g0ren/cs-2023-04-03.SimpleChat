namespace ChatServer
{
    class Program { 
        public static void Main()
        {
            Random random = new Random();
            int myPort = random.Next(12346, 65535);
            Console.WriteLine("Starting new server on port " + myPort);
            using (ChatServer chatServer = new ChatServer(myPort))
            {
                chatServer.Start();                
                chatServer.Dispose();
            }
        }
    }
}