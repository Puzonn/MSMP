using BepInEx.Logging;
using Msmp.Server;
using System;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;
using MyBox;
using Msmp.Client.Controllers;
using UnityEngine.AI;
using Msmp.Server.Packets;
using MSMP.Mono;
using Msmp.Mono;

namespace Msmp.Client
{
    internal class MsmpClient
    {
        private static MsmpClient _instance;
        public static MsmpClient Instance
        {
            get
            {
                return _instance;
            }
            set
            {
                _instance = value;
            }
        }

        public bool Connected => _client.Connected;
        
        private readonly TcpClient _client;
        private readonly ClientManager _clientManager;

        public Guid LocalClientNetworkId { get; private set; }

        public bool IsServer { get; private set; }

        private NetworkStream GetStream()
            => _client.GetStream();

        private Thread _responseThread;

        private readonly ManualLogSource _logger;

        public MsmpClient(ManualLogSource logger, ClientManager clientManager)
        {
            _logger = logger;
            _logger.LogInfo("Starting Msmp client");
            _clientManager = clientManager;
            _client = new TcpClient();  
        }

        public void BuildClient(bool isServer)
        {
            IsServer = isServer;
            _client.Connect("localhost", 35555);
            _responseThread = new Thread(WaitForResponse);
            _responseThread.Start();
        }

        private void WaitForResponse()
        {
            _logger.LogInfo("Client connected, waiting for response");

            NetworkStream _stream = GetStream();

            while (true)
            {
                // Adjust buffer length if buffer is somehow cut off
                byte[] buffer = new byte[1024];

                int bytesRead = _stream.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    UnityDispatcher.UnitySyncContext.Post(_ =>
                    {
                        PacketType packetType = (PacketType)buffer[0];

                        switch (packetType)
                        {
                            case PacketType.OnConnection:
                                {
                                    UnityDispatcher.UnitySyncContext.Post(_p =>
                                    {
                                        UserConnected conn = Packet.Deserialize<UserConnected>(buffer.Take(bytesRead).ToArray());

                                        LocalClientNetworkId = conn.NetworkId;

                                        /* Clear buffer. @see Msmp.Server.PayloadManager */
                                        foreach (var client in _clientManager.Clients.ToList() /* Prevent { Collection was modified }*/)
                                        {
                                            GameObject gm = _clientManager.Clients[client.Key];
                                            /* Check if needed. @see Msmp.Server.PayloadManager */
                                            UnityEngine.Object.Destroy(gm);

                                            _clientManager.Clients[client.Key] = null;
                                        }
                                        foreach (var client in conn.ConnectedClients)
                                        {
                                            CustomerGenerator customerGenerator = Singleton<CustomerGenerator>.Instance;
                                            Customer customer = customerGenerator.Spawn();
                                            GameObject ad = GameObject.Instantiate(customer.gameObject);

                                            ad.AddComponent<NetworkedPlayer>()
                                            .NetworkId = client.ClientId;

                                            customerGenerator.DeSpawn(customer);

                                            foreach (var comp in ad.GetComponents<Component>())
                                            {
                                                if (comp.GetType().Name == "Customer")
                                                {
                                                    UnityEngine.Object.Destroy(comp);
                                                    continue;
                                                }

                                                ad.tag = "_";
                                                ad.name = "_";
                                            }

                                            ad.GetComponent<NavMeshAgent>().isStopped = true;
                                            ad.GetComponent<NavMeshAgent>().enabled = false;

                                            ad.transform.position = new Vector3(client.x, client.y, client.z);
                                            _clientManager.AddOrUpdateClient(client.ClientId, ad);

                                            _logger.LogInfo($"Added {client.ClientId} as unity GameObject");
                                        }

                                        _logger.LogInfo($"[Client] {nameof(PacketType.OnConnection)} You're connected as {conn.NetworkId}");
                                    }, null);

                                }
                                break;
                            case PacketType.PlayerMovement:
                                {
                                    Guid clientId = new Guid(buffer.Skip(1).Take(bytesRead).Take(16).ToArray());
                                    int intX = BitConverter.ToInt32(buffer, 17);
                                    int intY = BitConverter.ToInt32(buffer, 21);
                                    int intZ = BitConverter.ToInt32(buffer, 25);

                                    double x = (double)(intX / 32.0);
                                    double y = (double)(intY / 32.0);
                                    double z = (double)(intZ / 32.0);

                                    if (_clientManager.Clients.ContainsKey(clientId))
                                    {
                                        GameObject r = _clientManager.Clients[clientId];

                                        if (r == null)
                                        {
                                            return;
                                        }

                                        _clientManager.Clients[clientId].transform.position = new Vector3((float)x, (float)y, (float)z);
                                    }

                                }
                                //_logger.LogInfo($"[Client] [{nameof(PacketType.PlayerMovement)}] {clientId} x:{x} y:{y} z:{z}");
                                break;
                            case PacketType.MoneyChanged:
                                {
                                    float changedMoney = BitConverter.ToSingle(buffer, 1);
                                    MoneyController.Instance.MoneyChanged(changedMoney);
                                }
                                break;
                            case PacketType.PlayerRotate:
                                {
                                    Guid clientId = new Guid(buffer.Skip(1).Take(bytesRead).Take(16).ToArray());
                                    float euler = (float)BitConverter.ToInt32(buffer, 16 + 1) / 32;
                                    Quaternion rotation = Quaternion.Euler(0f, euler, 0f);
                                    _clientManager.Clients[clientId].transform.rotation = rotation; 
                                }
                                break;
                            case PacketType.PurchaseEvent:
                                {
                                    OutMarketShoppingCartPurchasePacket items = Packet.Deserialize<OutMarketShoppingCartPurchasePacket>(buffer);

                                    foreach(var product in items.Products)
                                    {
                                        Box box = Singleton<BoxGenerator>.Instance.SpawnBox(Singleton<IDManager>.Instance.ProductSO(product.ItemId),
                                            new Vector3(4, 1, 4), Quaternion.identity);

                                        box.Setup(product.ItemId, true);

                                        box.GetOrAddComponent<NetworkedBox>().BoxNetworkId = product.NetworkItemId;
                                    }

                                    foreach(var furniture in items.Furnitures)
                                    {
                                        Box box = Singleton<BoxGenerator>.Instance.SpawnBox(Singleton<IDManager>.Instance.ProductSO(furniture.ItemId),
                                           new Vector3(4, 1, 4), Quaternion.identity);

                                        box.Setup(furniture.ItemId, true);

                                        box.GetOrAddComponent<NetworkedBox>().BoxNetworkId = furniture.NetworkItemId;
                                    }
                                }
                                break;
                            case PacketType.PickupEvent:
                                {
                                    /* TODO: Cache all boxes */
                                    _logger.LogInfo("Picking up");
                                    BoxPickedupPacket boxPickedupPacket = Packet.Deserialize<BoxPickedupPacket>(buffer);

                                    Array.Find(UnityEngine.Object.FindObjectsOfType<NetworkedBox>(),
                                        x => x.BoxNetworkId == boxPickedupPacket.BoxNetworkId)
                                    .SetPickedUp(boxPickedupPacket.BoxOwner);

                                    _logger.LogInfo("Picked");
                                }
                                break;
                        }
                    }, null);
                }
            }
        }

        public void SendPayload(Packet payload)
        {
            if(payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if(payload.GetType() != typeof(Packet))
            {
                throw new NotImplementedException("Payload have to be string type");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] packetBytes = new byte[1 + payload._data.Length];

                packetBytes[0] = payload._type;

                Array.Copy(payload._data, 0, packetBytes, 1, payload._data.Length);

                GetStream().Write(packetBytes, 0, packetBytes.Length);
            }
        }
    }
}
