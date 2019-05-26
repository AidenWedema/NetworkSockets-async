using System;


namespace Server
{
    class Program
    {
        

        static void Main(string[] args)
        {
            SocketServerTCP serverTCP = new SocketServerTCP();
            while (true)
            {
                serverTCP.SendAll(Console.ReadLine());
            }
        }
    }
}
