using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server
{
    class SocketServerTCP
    {
        private static byte[] buffer = new byte[1024];
        private static List<Socket> clientSockets = new List<Socket>();

        private static Socket serverSocket = new Socket
            (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        public SocketServerTCP()
        {
            SetupServer();
        }


        private void SetupServer()
        {
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 100));
            serverSocket.Listen(1);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            Console.WriteLine("Server accepting people");
        }

        private static void AcceptCallback(IAsyncResult ar)
        {
            Socket aClient = serverSocket.EndAccept(ar);
            clientSockets.Add(aClient);
            aClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), aClient);
            serverSocket.BeginAccept(new AsyncCallback(AcceptCallback), null);
            Console.WriteLine("server accepted: " + aClient.RemoteEndPoint);
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                Socket aClient = (Socket)ar.AsyncState;
                int received = aClient.EndReceive(ar);

                byte[] tempBuffer = new byte[received];
                Array.Copy(buffer, tempBuffer, received);
                string text = Encoding.ASCII.GetString(tempBuffer);
                Console.WriteLine("Text received: " + text);
                aClient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), aClient);
            }
            catch (SocketException)
            {

            }
            //logics
            
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
                        Console.WriteLine("removed client");
                    }
                    
                }catch(Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                
            }
            foreach(Socket client in clientsToRemove)
            {
                clientSockets.Remove(client);
            }
            clientsToRemove.Clear();
        }
        public void Send(string aMessage, Socket aClient)
        {
            byte[] data = Encoding.ASCII.GetBytes(aMessage);
            aClient.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), aClient);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket aClient = (Socket)ar.AsyncState;
                aClient.EndSend(ar);
            }
            catch(SocketException)
            {

            }
        }
    }
}
