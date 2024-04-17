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
using Msmp.Mono;
using Msmp.Patch.Traffic;
using Msmp.Patch.CustomerPatch;
using Msmp.Patch.Shop;
using Msmp.Client.SynchronizationContainers;
using Msmp.Patch.BoxPatch;
using Msmp.Server.Packets.Customers;

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
        public readonly ClientManager _clientManager;

        public Guid LocalClientNetworkId { get; private set; }

        public bool IsServer { get; private set; } = false;

        private NetworkStream GetStream()
            => _client.GetStream();

        private readonly Thread _responseThread;

        private readonly ManualLogSource _logger;

        public readonly MsmpSynchronizationContext SyncContext;

        public MsmpClient(ManualLogSource logger, ClientManager clientManager)
        {
            SyncContext = new MsmpSynchronizationContext(logger, this);
            _logger = logger;
            _logger.LogInfo("Starting client");
            _clientManager = clientManager;
            _client = new TcpClient();
            _responseThread = new Thread(WaitForResponse);
        }

        public void BuildClient(bool isServer)
        {
            IsServer = isServer;
            _client.Connect("localhost", 35555);
            _responseThread.Start();
        }

        private void WaitForResponse()
        {
            _logger.LogInfo("Client connected, waiting for response");

            NetworkStream _stream = GetStream();

            while (true)
            {
                byte[] buffer = new byte[1024 * 50];

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
                                    if(IsServer)
                                    {
                                        SyncContext.DisplayContainer.InitializeDisplays();

                                        Console.WriteLine($"[Client] [{nameof(PacketType.OnConnection)}] New client connected. Syncing all clients");

                                        OutSyncAllPacket outSyncAllPacket = new OutSyncAllPacket()
                                        {
                                            Money = 0,
                                            TrafficNPCs = SyncContext.GetSyncTrafficNPCs(),
                                            Customer = SyncContext.GetSyncCustomers(),
                                            Displays = SyncContext.GetSyncDisplays(),
                                            Boxes = SyncContext.GetSyncBoxes(),
                                        };

                                        Packet packet = new Packet(PacketType.SyncAll, outSyncAllPacket);

                                        SendPayload(packet);
                                    }

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
                                        }

                                        ad.GetComponent<Collider>().enabled = true;

                                        ad.name = "_";

                                        ad.GetComponent<NavMeshAgent>().isStopped = true;
                                        ad.GetComponent<NavMeshAgent>().enabled = false;

                                        ad.transform.position = new Vector3(client.x, client.y, client.z);
                                        _clientManager.AddOrUpdateClient(client.ClientId, ad);

                                        _logger.LogInfo($"Added {client.ClientId} as unity GameObject");
                                    }

                                    _logger.LogInfo($"[Client] [{nameof(PacketType.OnConnection)}] You're connected as {conn.NetworkId}");
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

                                        NetworkedBox networkedBox = box.gameObject.AddComponent<NetworkedBox>();
                                        networkedBox.NetworkId = product.NetworkItemId;

                                        SyncContext.BoxContainer.AddBox(new SyncBoxConstainer.SyncBox()
                                        {
                                            BoxReference = networkedBox,
                                        });

                                        _clientManager.AddBox(product.NetworkItemId, box);
                                    }

                                    foreach(var furniture in items.Furnitures)
                                    {
                                        Box box = Singleton<BoxGenerator>.Instance.SpawnBox(Singleton<IDManager>.Instance.ProductSO(furniture.ItemId),
                                           new Vector3(4, 1, 4), Quaternion.identity);

                                        box.Setup(furniture.ItemId, true);

                                        NetworkedBox networkedBox = box.gameObject.AddComponent<NetworkedBox>();
                                        networkedBox.NetworkId = furniture.NetworkItemId;

                                        SyncContext.BoxContainer.AddBox(new SyncBoxConstainer.SyncBox()
                                        {
                                            BoxReference = networkedBox,
                                        });

                                        _clientManager.AddBox(furniture.NetworkItemId, box);
                                    }
                                }
                                break;
                            case PacketType.BoxPickupEvent:
                                {
                                    /* TODO: Cache all boxes */
                                    OutBoxPickedupPacket boxPickedupPacket = Packet.Deserialize<OutBoxPickedupPacket>(buffer);

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
                                    PlaceProductToDisplayPatch.AddProductToDisplay(outProductToDisplayPacket);  
                                }
                                break;
                            case PacketType.OpenBoxEvent:
                                {
                                    OutOpenBoxPacket outOpenBoxPacket = Packet.Deserialize<OutOpenBoxPacket>(buffer);

                                    if(outOpenBoxPacket.State)
                                    {
                                        _clientManager.GetBox(outOpenBoxPacket.BoxNetworkId).OpenBox();
                                        SyncContext.BoxContainer.SetSpawned(outOpenBoxPacket.BoxNetworkId);
                                    }
                                    else
                                    {
                                        _clientManager.GetBox(outOpenBoxPacket.BoxNetworkId).CloseBox();
                                        SyncContext.BoxContainer.SetSpawned(outOpenBoxPacket.BoxNetworkId);
                                    }
                                }
                                break;
                            case PacketType.SpawnTrafficNpc:
                                {
                                    OutSpawnTrafficNpcPacket spawnTrafficNpcPacket = Packet.Deserialize<OutSpawnTrafficNpcPacket>(buffer);
                                    NpcTrafficManagerSpawnPatch.SpawnTraffic(spawnTrafficNpcPacket);
                                }
                                break;
                            case PacketType.TrafficNpcSetDestination:
                                {
                                    OutTrafficNpcSetDestinationPacket outTrafficNpcSetDestinationPacket 
                                        = Packet.Deserialize<OutTrafficNpcSetDestinationPacket>(buffer);

                                    WaypointNavigatorPatch.SetDestination(outTrafficNpcSetDestinationPacket);
                                }
                                break;
                            case PacketType.SpawnCustomer:
                                {
                                    OutSpawnCustomer outSpawnCustomer = Packet.Deserialize<OutSpawnCustomer>(buffer);

                                    CustomerManagerSpawnPatch.SpawnCustomer(outSpawnCustomer.NetworkId, outSpawnCustomer.PrefabIndex, outSpawnCustomer.SpawnTransformIndex);
                                }
                                break;
                            case PacketType.SpawnCustomerVector:
                                {
                                    OutSpawnCustomerVector outSpawnCustomerVector = Packet.Deserialize<OutSpawnCustomerVector>(buffer);

                                    CustomerManagerSpawnPatch.SpawnCustomer(outSpawnCustomerVector.NetworkId, outSpawnCustomerVector.PrefabIndex,
                                        outSpawnCustomerVector.SpawnTransformIndex, outSpawnCustomerVector.Position.ToVector3());
                                }
                                break;
                            case PacketType.SyncAll:
                                {
                                    OutSyncAllPacket outSyncAllPacket = Packet.Deserialize<OutSyncAllPacket>(buffer);
                                    NpcTrafficManagerSpawnPatch.SyncTraffic(outSyncAllPacket.TrafficNPCs);
                                    CustomerManagerSpawnPatch.SyncCustomers(outSyncAllPacket.Customer);
                                    PlaceProductToDisplayPatch.AddProductToDisplay(outSyncAllPacket.Displays);
                                    SpawnBoxPatch.SpawnBox(outSyncAllPacket.Boxes);
                                }
                                break;
                            case PacketType.DespawnTraffic:
                                {
                                    Guid guid = new Guid(buffer.Skip(1).Take(16).ToArray());
                                    NpcTrafficManagerDespawnPatch.RemoveTrafficNPC(guid);
                                }
                                break;
                            case PacketType.CustomerGoToCheckout:
                                {
                                    OutCustomerGoToCheckout outCustomerGoToCheckout = Packet.Deserialize<OutCustomerGoToCheckout>(buffer);
                                    Customer customer = SyncContext.CustomerContainer.GetCustomer(outCustomerGoToCheckout.NetworkId);
                                    customer.GetComponent<NetworkedCustomer>().GoToCheckout();
                                }
                                break;
                            case PacketType.CustomerStartShopping:
                                {
                                    OutCustomerStartShopping outCustomerStartShopping = Packet.Deserialize<OutCustomerStartShopping>(buffer);
                                    Customer customer = SyncContext.CustomerContainer.GetCustomer(outCustomerStartShopping.NetworkId);
                                    customer.GetComponent<NetworkedCustomer>().SyncStartShopping(outCustomerStartShopping.ShoppingList);
                                }
                                break;
                            case PacketType.CustomerTakeProductFromDisplay:
                                {
                                    OutCustomerTakeProductPacket outCustomerTakeProductPacket = Packet.Deserialize<OutCustomerTakeProductPacket>(buffer);
                                    Customer customer = SyncContext.CustomerContainer.GetCustomer(outCustomerTakeProductPacket.NetworkId);
                                    customer.GetComponent<NetworkedCustomer>().SyncTakeProductFromDisplay(outCustomerTakeProductPacket);
                                }
                                break;
                            case PacketType.CustomerWalkAround:
                                {
                                    OutCustomerWalkAround outCustomerWalkAround = Packet.Deserialize<OutCustomerWalkAround>(buffer);
                                    Customer customer = SyncContext.CustomerContainer.GetCustomer(outCustomerWalkAround.NetworkId);
                                    customer.GetComponent<NetworkedCustomer>().SyncWalkAround(outCustomerWalkAround.DisplaySlotId);

                                }
                                break;
                        }
                    }, null);
                }
                _stream.Flush();
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
                throw new NotImplementedException($"Payload have to be {nameof(Packet)} type");
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
