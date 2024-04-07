using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using BepInEx.Logging;
using System.Linq;
using Msmp.Server.Packets;

namespace Msmp.Server
{
    public class MsmpServer
    {
        /*TODO: Use task pooling or thread pooling */
        private List<Thread> ConnectionThreads = new List<Thread>();

        private readonly Thread serverThread;
        private readonly TcpListener serverListener;
        private readonly ManualLogSource _logger;
        private readonly PayloadManager manager;

        public MsmpServer(ManualLogSource logger)
        {
            manager = new PayloadManager(logger);

            _logger = logger;

            _logger.LogInfo("Starting listener ...");
            serverListener = new TcpListener(IPAddress.Any, 35555);
            serverListener.Start();
            _logger.LogInfo("Listener initalized");
            _logger.LogInfo("Creating server listener thread ...");
            serverThread = new Thread(ListenForConnection);
            serverThread.Start();
        }

        private void ListenForConnection()
        {
            _logger.LogInfo("Server listener thread started");

            while (true)
            {
                TcpClient client = serverListener.AcceptTcpClient();
                Guid clientId = Guid.NewGuid();

                manager.AddClient(clientId, client.GetStream());

                /* Sync all clients that someone connected */

                /* TODO:
                    Sync player prefabs for all connected 
                    Each time client will connect all, all prefabs will be randomized 
                 */

                foreach (var _client in manager.Clients) 
                {
                    UserConnected connected = new UserConnected()
                    {
                        ConnectedClients = manager.Clients.Values.ToList(),
                        UserId = _client.Value.ClientId,
                    };

                    Packet packet = new Packet(PacketType.OnConnection, connected);

                    manager.SendPayload(_client.Key, packet);
                }

                /* Each client have their own thread to listen all payloads */
                Thread _ = new Thread(() => manager.ListenForPayload(client.GetStream()));
                _.Start();
                ConnectionThreads.Add(_);
            }
        }
    }
}
