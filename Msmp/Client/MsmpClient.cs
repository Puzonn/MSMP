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
using System.Reflection;
using MSMP.Server.Packets;
using System.Collections.Generic;
using MSMP.Patch;
using MSMP.Patch.Traffic;

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
                                        if (client.ClientId == LocalClientNetworkId)
                                        {
                                            Singleton<PlayerController>.Instance.GetOrAddComponent<NetworkedPlayer>()
                                            .NetworkId = client.ClientId;

                                            continue;
                                        }

                                        CustomerGenerator customerGenerator = Singleton<CustomerGenerator>.Instance;
                                        Customer customer = customerGenerator.Spawn();
                                        GameObject ad = GameObject.Instantiate(customer.gameObject);

                                        ad.AddComponent<NetworkedPlayer>()
                                        .NetworkId = client.ClientId;

                                        customerGenerator.DeSpawn(customer);

                                        foreach (var comp in ad.GetComponents<Component>())
                                        {
                                            if (comp.GetType().Name == "Customer" || comp.GetType().Name == "Renderer")
                                            {
                                                UnityEngine.Object.Destroy(comp);
                                                continue;
                                            }

                                            _logger.LogInfo(comp.GetType().Name);
                                        }

                                        ad.GetComponent<Collider>().enabled = true;

                                        ad.name = "_";

                                        ad.GetComponent<NavMeshAgent>().isStopped = true;
                                        ad.GetComponent<NavMeshAgent>().enabled = false;

                                        ad.transform.position = new Vector3(client.x, client.y, client.z);
                                        _clientManager.AddOrUpdateClient(client.ClientId, ad);

                                        _logger.LogInfo($"Added {client.ClientId} as unity GameObject");
                                    }

                                    _logger.LogInfo($"[Client] {nameof(PacketType.OnConnection)} You're connected as {conn.NetworkId}");
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

                                    _clientManager.Move(clientId, new Vector3((float)x, (float)y, (float)z));
                                }
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

                                        box.GetOrAddComponent<NetworkedBox>().NetworkId = product.NetworkItemId;

                                        _clientManager.AddBox(product.NetworkItemId, box);
                                    }

                                    foreach(var furniture in items.Furnitures)
                                    {
                                        Box box = Singleton<BoxGenerator>.Instance.SpawnBox(Singleton<IDManager>.Instance.ProductSO(furniture.ItemId),
                                           new Vector3(4, 1, 4), Quaternion.identity);

                                        box.Setup(furniture.ItemId, true);

                                        box.GetOrAddComponent<NetworkedBox>().NetworkId = furniture.NetworkItemId;

                                        _clientManager.AddBox(furniture.NetworkItemId, box);
                                    }
                                }
                                break;
                            case PacketType.BoxPickupEvent:
                                {
                                    /* TODO: Cache all boxes */
                                    BoxPickedupPacket boxPickedupPacket = Packet.Deserialize<BoxPickedupPacket>(buffer);

                                    _clientManager.GetBox(boxPickedupPacket.BoxNetworkId).GetComponent<NetworkedBox>()
                                   .SetPickedUp(boxPickedupPacket.BoxOwner);
                                }
                                break;
                            case PacketType.BoxDropEvent:
                                {
                                    OutBoxDropPacket boxDropPacket = Packet.Deserialize<OutBoxDropPacket>(buffer);

                                    _clientManager.GetBox(boxDropPacket.BoxNetworkId).GetComponent<NetworkedBox>()
                                    .BoxDropped(new Vector3(boxDropPacket.x, boxDropPacket.y, boxDropPacket.z));
                                }
                                break;
                            case PacketType.ProductToDisplayEvent:
                                {
                                    OutProductToDisplayPacket outProductToDisplayPacket = Packet.Deserialize<OutProductToDisplayPacket>(buffer);
                                    Box box = _clientManager.GetBox(outProductToDisplayPacket.BoxNetworkId);

                                    Product product = box.GetProductFromBox();

                                    List<Display> displays = (List<Display>)(Singleton<DisplayManager>.Instance.GetType()
                                        .GetField("m_Displays", BindingFlags.NonPublic | BindingFlags.Instance)
                                        .GetValue(Singleton<DisplayManager>.Instance));

                                    if(displays == null) 
                                    {
                                        _logger.LogError($"[Client] {nameof(displays)} was null at {PacketType.ProductToDisplayEvent}");

                                        return;
                                    }

                                    DisplaySlot[] slots = (DisplaySlot[])displays[0].GetType()
                                     .GetField("m_DisplaySlots", BindingFlags.NonPublic | BindingFlags.Instance)
                                     .GetValue(displays[outProductToDisplayPacket.DisplayId]);

                                    if (slots == null)
                                    {
                                        _logger.LogError($"[Client] {nameof(slots)} was null at {PacketType.ProductToDisplayEvent}");
                                        return;
                                    }

                                    slots[outProductToDisplayPacket.DisplaySlotId].AddProduct(outProductToDisplayPacket.ProductId, product);
                                    Singleton<InventoryManager>.Instance.AddProductToDisplay(new ItemQuantity
                                    {
                                        Products = new Dictionary<int, int>
                                        {
                                            {
                                                 outProductToDisplayPacket.ProductId,
                                                 1
                                            }
                                        }
                                    });
                                }
                                break;
                            case PacketType.OpenBoxEvent:
                                {
                                    OutOpenBoxPacket outOpenBoxPacket = Packet.Deserialize<OutOpenBoxPacket>(buffer);

                                    if(outOpenBoxPacket.State)
                                    {
                                        _clientManager.GetBox(outOpenBoxPacket.BoxNetworkId).OpenBox();
                                    }
                                    else
                                    {
                                        _clientManager.GetBox(outOpenBoxPacket.BoxNetworkId).CloseBox();
                                    }
                                }
                                break;
                            case PacketType.SpawnTrafficNpc:
                                {
                                    OutSpawnTrafficNpcPacket spawnTrafficNpcPacket = Packet.Deserialize<OutSpawnTrafficNpcPacket>(buffer);
                                    NpcTrafficManagerPatch.SpawnNpc(spawnTrafficNpcPacket);
                                }
                                break;
                            case PacketType.TrafficNpcSetDestination:
                                {
                                    OutTrafficNpcSetDestinationPacket outTrafficNpcSetDestinationPacket 
                                        = Packet.Deserialize<OutTrafficNpcSetDestinationPacket>(buffer);
                                    WaypointNavigatorPatch.SetDestination(outTrafficNpcSetDestinationPacket);
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
