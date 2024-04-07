using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading;

namespace BipServer
{
    public class Server
    {
        private readonly Thread serverThread;
        private readonly TcpListener serverListener;

        public Server()
        {
            serverListener = new TcpListener(IPAddress.Any, 35555);
            serverThread = new Thread(Listen);
        }

        private void Listen()
        {
            while (true)
            {
                TcpClient client = serverListener.AcceptTcpClient();

                NetworkStream ns = client.GetStream();

                while (client.Connected)
                {
                    byte[] msg = new byte[1024];

                    if (ns.Read(msg, 0, msg.Length) > 0)
                    {
                        if (msg[0] == (byte)PacketType.PlayerMovement)
                        {
                            Console.WriteLine("Moving ...");
                        }
                    }
                }
            }
        }
    }
}
