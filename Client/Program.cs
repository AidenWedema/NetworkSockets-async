using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            SocketClientTCP client = new SocketClientTCP();
            
                client.AttemptConnection();
            
            while (true)
            {
                if (client.connected)
                {
                    client.Send(Console.ReadLine());
                }
                
            }
            
        }
    }
}
