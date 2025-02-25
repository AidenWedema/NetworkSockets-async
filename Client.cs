using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Networking
{
    class SocketClientTCP
    {
        private static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPAddress address;
        private int port;
        private static byte[] buffer = new byte[2048];
        public bool connected = false;

        public SocketClientTCP(string anAddress = "127.0.0.1", int aPort = 8080)
        {
            if (clientSocket != null) StopClient();
            clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            address = IPAddress.Parse(anAddress);
            port = aPort;
        }

        public void AttemptConnection()
        {
            try
            {
                clientSocket.BeginConnect(address, port, new AsyncCallback(AttemptConnectionCallback), clientSocket);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public void AttemptConnection(string anAddress = "127.0.0.1", int aPort = 8080)
        {
            address = IPAddress.Parse(anAddress);
            port = aPort;
            AttemptConnection();
        }

        private int attempts = 0;
        private void AttemptConnectionCallback(IAsyncResult ar)
        {
            try
            {
                attempts++;
                clientSocket.EndConnect(ar);
                Receive();
                Console.Clear();
                Console.WriteLine("Connected");
                connected = true;
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
                clientSocket.BeginConnect(address, port, new AsyncCallback(AttemptConnectionCallback), clientSocket);
            }
        }

        private void Receive()
        {
            try
            {
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                int received = clientSocket.EndReceive(ar);

                byte[] tempBuffer = new byte[received];
                Array.Copy(buffer, tempBuffer, received);
                string text = Encoding.ASCII.GetString(tempBuffer);
                Console.WriteLine(text);
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);

                Console.WriteLine("Disconnected.. trying to reconnect", 2);
                clientSocket.Dispose();
                clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                AttemptConnection();
            }
        }

        public void Send(string aMessage)
        {
            try
            {
                byte[] buffer = Encoding.ASCII.GetBytes(aMessage);
                clientSocket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString(), 2);
            }

        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
            }
            catch (SocketException)
            {

            }
        }

        public void StopClient()
        {
            if (clientSocket == null) return;
            try
            {
                if (clientSocket.Connected)
                {
                    clientSocket.Shutdown(SocketShutdown.Both);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                clientSocket.Close();
                clientSocket = null;
                connected = false;
            }
        }
    }
}