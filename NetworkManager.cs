using System.Net;
using System.Diagnostics;

namespace Networking
{
    public class NetworkManager
    {
        private static NetworkManager instance;
        private static readonly object lockObject = new object();

        private int lastId = -1;

        private SocketServerTCP server;
        private SocketClientTCP client;
        private SynchronizationContext syncContext;

        public bool gameStarted = false;
        public bool isOnline = false;
        public bool isHost = false;
        public string address = "127.0.0.1";
        public int port = 8080;

        private NetworkManager() { syncContext = SynchronizationContext.Current; }

        public static NetworkManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = new NetworkManager();
                        }
                    }
                }
                return instance;
            }
        }

        /// <returns>the IPv4 address IPAddress of the machine</returns>
        public static IPAddress GetLocalIPAddress()
        {
            try
            {
                foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        return ip;
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error fetching local IP: {ex.Message}");
            }
            return IPAddress.Parse("127.0.0.1");
        }

        public int GetServerClientAmount() { return server.GetClients().Count; }
    }

    [System.Serializable]
    public class Command
    {
        public enum Type { SEND_PRIVATE, SEND_ALL, DISCONNECT };  // How should the recipient handle this command?
        public int Id { get; private set; }                       // The unique identifier of the command
        public Type type = Type.SEND_PRIVATE;                     // The type of command
        public string data;                                       // The data of the command
        public string delimiter = "~";                            // The delimiter used to seperate the data

        public Command()
        {
            Id = System.DateTime.Now.Millisecond;
        }

        public override string ToString() { return ToString(this); }

        public static string ToString(Command c) { return $"{c.Id}<->{(int)c.type}<->{c.delimiter}<->{c.data}|!|"; }

        public static Command FromString(string aCommand)
        {
            string[] parts = aCommand.Split("<->");
            return new Command { Id = int.Parse(parts[0]), type = (Type)int.Parse(parts[1]), delimiter = parts[2], data = parts[3] };
        }
    }
}