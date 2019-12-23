using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client
{
    class SocketClientTCP
    {
        private static Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private IPAddress address;
        private IPHostEntry entry;
        private EndPoint endPoint;
        private int port;
        private static byte[] buffer = new byte[1024];
        public bool connected = false;

        public SocketClientTCP(int aPort = 100)
        {
            address = IPAddress.Parse("127.0.0.1");
            port = aPort;


            entry = Dns.GetHostEntry("localhost");
            endPoint = new IPEndPoint(address, port);//entry.AddressList[0];
            
        }



        public void AttemptConnection()
        {
            try
            {
                clientSocket.BeginConnect("localhost", port, new AsyncCallback(AttemptConnectionCallback), clientSocket); 

            }catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
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
                clientSocket.BeginConnect("localhost", port, new AsyncCallback(AttemptConnectionCallback), clientSocket);
            }
        }


        private void Receive()
        {
            try
            {
                clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch(SocketException e)
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
                    Console.WriteLine("Text received: " + text);
                    clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), null);
            }
            catch (SocketException e)
            {
                Console.WriteLine(e.Message);

                Console.WriteLine("Disconnected.. trying to reconnect");
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
                Console.WriteLine("Client Sending: " + aMessage);
            }catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
            
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                clientSocket.EndSend(ar);
                
            }
            catch(SocketException)
            {

            }
        }
    }
}
