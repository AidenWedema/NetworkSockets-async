using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Networking
{
    class SocketServerTCP
    {
        private static byte[] buffer = new byte[2048];
        private static List<Socket> clientSockets = new List<Socket>();

        private static Socket serverSocket = null;

        public SocketServerTCP(bool local = false, int aPort = 8080)
        {
            if (serverSocket != null) StopServer();
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            SetupServer(local, aPort);
        }

        public void SetupServer(bool local = false, int aPort = 8080)
        {
            try
            {
                IPAddress address = local ? IPAddress.Parse("127.0.0.1") : NetworkManager.GetLocalIPAddress();
                serverSocket.Bind(new IPEndPoint(address, aPort));
                serverSocket.Listen(1);
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                Console.WriteLine("Server accepting people on ip " + address.ToString() + " on port " + aPort.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            try
            {
                if (NetworkManager.Instance.gameStarted)
                {
                    Console.WriteLine("Ignored incoming connection");
                    return;
                }

                Socket aClient = serverSocket.EndAccept(ar);
                clientSockets.Add(aClient);
                aClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), aClient);
                serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
                Console.WriteLine("accepted: " + aClient.RemoteEndPoint);
            }
            catch (Exception e)
            {
                if (e is ObjectDisposedException || e is SocketException) return;
                Console.WriteLine(e.Message);
            }
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket aClient = ar.AsyncState as Socket;
                if (aClient == null) return;
                int received = aClient.EndReceive(ar);

                byte[] tempBuffer = new byte[received];
                Array.Copy(buffer, tempBuffer, received);
                string text = Encoding.ASCII.GetString(tempBuffer);
                Console.WriteLine(text);
                aClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), aClient);
            }
            catch (Exception e)
            {
                if (e is ObjectDisposedException || e is SocketException) return;
                Console.WriteLine(e.Message);
            }
        }

        public void SendAll(string aMessage)
        {
            List<Socket> clientsToRemove = new List<Socket>();
            foreach (Socket aClient in clientSockets)
            {
                try
                {
                    if (aClient.Connected)
                    {
                        byte[] data = Encoding.ASCII.GetBytes(aMessage);
                        aClient.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), aClient);
                    }
                    else
                    {
                        clientsToRemove.Add(aClient);
                        Console.WriteLine("removed client", 2);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

            }
            foreach (Socket client in clientsToRemove)
                clientSockets.Remove(client);
            clientsToRemove.Clear();
        }

        public void Send(string aMessage, Socket aClient)
        {
            if (!aClient.Connected) return;

            byte[] data = Encoding.ASCII.GetBytes(aMessage);
            aClient.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), aClient);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket aClient = ar.AsyncState as Socket;
                if (aClient != null)
                    aClient.EndSend(ar);
                else
                    Console.WriteLine("Client is null", 2);
            }
            catch (Exception e)
            {
                if (e is ObjectDisposedException || e is SocketException) return;
                Console.WriteLine(e.Message);
            }
        }

        public void StopServer()
        {
            if (serverSocket == null) return;
            Command command = new Command();
            command.type = Command.Type.DISCONNECT;
            SendAll(command.ToString());
            foreach (Socket aClient in clientSockets)
            {
                try
                {
                    if (aClient.Connected)
                    {
                        aClient.Shutdown(SocketShutdown.Both);
                    }
                }
                catch (Exception e)
                {
                    if (e is not ObjectDisposedException || e is not SocketException)
                        Console.WriteLine(e.Message);
                }
                finally
                {
                    aClient.Dispose();
                    aClient.Close();
                }
            }
            clientSockets.Clear();
            try
            {
                serverSocket.Shutdown(SocketShutdown.Both);
            }
            finally
            {
                serverSocket.Close();
                serverSocket = null;
            }
        }

        public List<Socket> GetClients() { return clientSockets; }
    }
}