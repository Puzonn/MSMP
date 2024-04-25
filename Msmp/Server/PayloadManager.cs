using System.IO;
using System;
using System.Net.Sockets;
using System.Collections.Generic;
using Msmp.Server.Models;
using BepInEx.Logging;
using Msmp.Server.Packets;
using System.Linq;

namespace Msmp.Server
{

    internal class PayloadManager
    {
        public readonly Dictionary<NetworkStream, ClientModel> Clients = new Dictionary<NetworkStream, ClientModel>();

        private readonly ManualLogSource _logger;

        public PayloadManager(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void AddClient(Guid networkId, NetworkStream stream)
        {
            _logger.LogInfo($"[{nameof(PayloadManager)}] Added client");

            /* TODO: Check if client exist */
            Clients.Add(stream, new ClientModel()
            {
                x = 0,
                y = 0,
                z = 0,
                ClientId = networkId,
            });
        }

        public void ListenForPayload(NetworkStream stream)
        {
            while (true)
            {
                byte[] data = new byte[1024 * 50];

                if (stream.Read(data, 0, data.Length) > 0)
                {
                    PacketType type = (PacketType)data[0];

                    switch (type)
                    {
                        case PacketType.PlayerMovement:
                            {
                                /* Cut one byte because of packet type */
                                int intX = BitConverter.ToInt32(data, 1);
                                int intY = BitConverter.ToInt32(data, 5);
                                int intZ = BitConverter.ToInt32(data, 9);

                                float x = (float)(intX / 32.0);
                                float y = (float)(intY / 32.0);
                                float z = (float)(intZ / 32.0);

                                ClientModel client = Clients[stream];

                                client.x = x;
                                client.y = y;
                                client.z = z;

                                // _logger.LogInfo($"[Server] [Client Movement] {client.ClientId} x:{client.x} y:{client.y} z:{client.z}");

                                byte[] clientId = client.ClientId.ToByteArray();
                                byte[] positionData = new byte[12];

                                Buffer.BlockCopy(BitConverter.GetBytes(intX), 0, positionData, 0, 4);
                                Buffer.BlockCopy(BitConverter.GetBytes(intY), 0, positionData, 4, 4);
                                Buffer.BlockCopy(BitConverter.GetBytes(intZ), 0, positionData, 8, 4);

                                byte[] clientMovementOut = new byte[clientId.Length + positionData.Length];
                                Buffer.BlockCopy(clientId, 0, clientMovementOut, 0, clientId.Length);
                                Buffer.BlockCopy(positionData, 0, clientMovementOut, clientId.Length, positionData.Length);

                                SendPayloadExclude(stream, new Packet(PacketType.PlayerMovement, clientMovementOut));
                            }
                            break;
                        case PacketType.PlayerRotate:
                            {
                                byte[] clientId = Clients[stream].ClientId.ToByteArray();
                                byte[] rotationData = new byte[4];
                                Buffer.BlockCopy(data, 1, rotationData, 0, rotationData.Length);

                                byte[] rotationDataOut = new byte[clientId.Length + rotationData.Length];
                                Buffer.BlockCopy(clientId, 0, rotationDataOut, 0, clientId.Length);
                                Buffer.BlockCopy(rotationData, 0, rotationDataOut, clientId.Length, rotationData.Length);

                                SendPayloadExclude(stream, new Packet(PacketType.PlayerRotate, rotationDataOut));
                            }
                            break;
                        case PacketType.PurchaseEvent:
                            {
                                InMarketShoppingCartPurchasePacket inMarketShoppingCartPurchase = Packet.Deserialize<InMarketShoppingCartPurchasePacket>(data);
                           
                                OutMarketShoppingCartPurchasePacket outMarketShoppingCartPurchase = new OutMarketShoppingCartPurchasePacket()
                                {
                                    Furnitures = inMarketShoppingCartPurchase.FurnituresIds.Select(x =>
                                        new MarketShoppingCartPurcheItem()
                                        {
                                            ItemId = x,
                                            NetworkItemId = Guid.NewGuid(),
                                        }
                                    ).ToArray(),
                                    Products = inMarketShoppingCartPurchase.ProductsIds.Select(x =>
                                        new MarketShoppingCartPurcheItem()
                                        {
                                            ItemId = x,
                                            NetworkItemId = Guid.NewGuid(),
                                        }
                                    ).ToArray(),
                                };
                               
                                SendPayload(new Packet(PacketType.PurchaseEvent, outMarketShoppingCartPurchase));
                            }
                            break;
                        case PacketType.BoxPickupEvent:
                            {
                                SendPayload(new Packet(data));
                            }
                            break;
                        case PacketType.BoxDropEvent:
                            {
                                SendPayload(new Packet(data));
                            }
                            break;
                        case PacketType.ProductToDisplayEvent:
                            {
                                SendPayloadExclude(stream, new Packet(data));
                            }
                            break;
                        case PacketType.OpenBoxEvent:
                            {
                                SendPayload(new Packet(data));
                            }
                            break;
                        case PacketType.MoneyChanged:
                            {
                                SendPayload(new Packet(data));
                            }
                            break;
                        case PacketType.SpawnTrafficNpc:
                            {
                                /* TODO: Let server set the networkid for npc */
                                SendPayload(new Packet(data));
                            }
                            break;
                        case PacketType.TrafficNpcSetDestination:
                            {
                                SendPayload(new Packet(data));
                            }
                            break;
                        case PacketType.SpawnCustomer:
                            {
                                SendPayload(new Packet(data));
                            }
                            break;
                        case PacketType.SpawnCustomerVector:
                            {
                                SendPayload(new Packet(data));
                            }
                            break;
                        case PacketType.SyncAll:
                            {
                                SendPayloadExclude(stream, new Packet(data));  
                            }
                            break;
                        case PacketType.DespawnTraffic:
                            {
                                SendPayload(new Packet(data));
                            }
                            break;
                        case PacketType.CustomerGoToCheckout:
                            {
                                SendPayload(new Packet(data));
                            }
                            break;
                        case PacketType.CustomerStartShopping:
                            {
                                SendPayload(new Packet(data));  
                            }
                            break;
                        case PacketType.CustomerTakeProductFromDisplay:
                            {
                                SendPayload(new Packet(data));  
                            }
                            break;
                        case PacketType.CustomerWalkAround:
                            {
                                SendPayload(new Packet(data));
                            }
                            break;
                    }
                }
            }
        }

        public void SendPayloadExclude(NetworkStream exclude, Packet payload)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.GetType() != typeof(Packet))
            {
                throw new NotImplementedException("Payload have to be packet type");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[1 + payload._data.Length];
                buffer[0] = payload._type;

                Array.Copy(payload._data, 0, buffer, 1, payload._data.Length);

                foreach (var client in Clients.Keys)
                {
                    if(client == exclude)
                    {
                        continue;
                    }

                    client.Write(buffer, 0, buffer.Length);
                }
            }
        }

        public void SendPayloadByStream(NetworkStream client, Packet payload)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.GetType() != typeof(Packet))
            {
                throw new NotImplementedException("Payload have to be packet type");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[1 + payload._data.Length];
                buffer[0] = payload._type;

                Array.Copy(payload._data, 0, buffer, 1, payload._data.Length);

                client.Write(buffer, 0, buffer.Length);
            }
        }

        public void SendPayload(Packet payload)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.GetType() != typeof(Packet))
            {
                throw new NotImplementedException("Payload have to be packet type");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[1 + payload._data.Length];
                buffer[0] = payload._type;

                Array.Copy(payload._data, 0, buffer, 1, payload._data.Length);

                foreach (var client in Clients.Keys)
                {
                    client.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}