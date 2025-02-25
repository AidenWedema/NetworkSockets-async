using Networking;

namespace Networking_Commands
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.Write("Server or Client? (s/c): ");
            string input = (Console.ReadLine() ?? "").ToLower();
            while (input != "s" && input != "c")
            {
                Console.Clear();
                Console.Write("Server or Client? (s/c): ");
                input = Console.ReadLine() ?? "";
            }

            if (input == "s")
            {
                SocketServerTCP server = new SocketServerTCP(true);
                Console.ReadLine();
                while (true)
                {
                    input = Console.ReadLine() ?? "";
                    if (input == "") continue;
                    server.SendAll(input);
                }
            }
            else if (input == "c")
            {
                SocketClientTCP client = new SocketClientTCP();
                client.AttemptConnection();
                while (true)
                {
                    if (client.connected)
                    {
                        input = Console.ReadLine() ?? "";
                        if (input == "") continue;
                        client.Send(input);
                    } 
                }
            }
        }
    }
}
