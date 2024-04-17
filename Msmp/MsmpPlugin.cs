using Msmp.Client;
using BepInEx;
using BepInEx.Logging;
using Msmp.Server;
using UnityEngine;
using System.Threading;
using Msmp.Patch.Shop;
using Msmp.Client.Controllers;
using HarmonyLib;
using Msmp.Server.Packets;
using Msmp.Patch.Traffic;
using Msmp.Patch.BoxPatch;
using Msmp.Patch.CustomerPatch;
using System;
using MSMP.Patch.Traffic;
using MSMP.Patch.CustomerPatch;
using Msmp.Server.Models;
using MyBox;
using System.Collections.Generic;
using System.Linq;
using Lean.Pool;
using System.Reflection;

namespace Msmp
{
    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class MsmpPlugin : BaseUnityPlugin
    {
        public static MsmpPlugin Instance;
        private MsmpServer server;

        private readonly MsmpClient client;
        private readonly ClientManager clientManager;

        private readonly MovementController movementController;
        private readonly MoneyController moneyController;
        private readonly CheckoutController checkoutController;

        private const string MyGUID = "com.Puzonne.Msmp";
        private const string PluginName = "Msmp.";
        private const string VersionString = "1.0.0";

        public static ManualLogSource Log = new ManualLogSource(PluginName);

        public MsmpPlugin()
        {
            UnityDispatcher.UnitySyncContext = SynchronizationContext.Current;

            Log = Logger;

            clientManager = new ClientManager();
            client = new MsmpClient(Log, clientManager);

            movementController = new MovementController(Log, client);
            moneyController = new MoneyController(Log, client);
            checkoutController = new CheckoutController(Log);

            Instance = this;
        }

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(PurchasePatch));
            Harmony.CreateAndPatchAll(typeof(DeliveryPatch));
            Harmony.CreateAndPatchAll(typeof(BoxPickupPatch));
            Harmony.CreateAndPatchAll(typeof(BoxDropPatch));
            Harmony.CreateAndPatchAll(typeof(PlaceProductToDisplayPatch));
            Harmony.CreateAndPatchAll(typeof(OpenBoxPatch));
            Harmony.CreateAndPatchAll(typeof(NpcTrafficManagerSpawnPatch));
            Harmony.CreateAndPatchAll(typeof(WaypointNavigatorPatch));
            Harmony.CreateAndPatchAll(typeof(CustomerManagerSpawnPatch));
            Harmony.CreateAndPatchAll(typeof(CustomermanagerSpawnVectorPatch));
            Harmony.CreateAndPatchAll(typeof(NpcTrafficManagerDespawnPatch));
            Harmony.CreateAndPatchAll(typeof(CustomerStartShoppingPatch));  
        }

        private void Update()
        {
            movementController.OnUpdate();
            moneyController.OnUpdate();
            checkoutController.OnUpdate();

            Time.timeScale = 1.0f;
            Application.runInBackground = true;

            if (Input.GetKeyDown(KeyCode.F6) && !client.Connected)
            {
                server = new MsmpServer(Log);
                client.BuildClient(true);
                MsmpClient.Instance = client;
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.F1))
            {
                var r = new OutSpawnTrafficNpcPacket()
                {
                    Enterence = 0,
                    Forward = true,
                    NetworkId = new Guid(),
                    Prefab = 0,
                    Speed = 0,
                    WaypointTravelCount = 1
                };
                NpcTrafficManagerSpawnPatch.SpawnTraffic(r);

                NpcTrafficManagerDespawnPatch.RemoveTrafficNPC(r.NetworkId);
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                Vector3 position = Singleton<PlayerController>.Instance.transform.position;

                OutSpawnCustomerVector outSpawnCustomer = new OutSpawnCustomerVector()
                {
                    NetworkId = Guid.NewGuid(),
                    PrefabIndex = 0,
                    SpawnTransformIndex = 0,
                    Position = new SerializableVector3(position)
                };

                Packet packet = new Packet(PacketType.SpawnCustomerVector, outSpawnCustomer);

                client.SendPayload(packet);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                int[] r = Singleton<DisplayManager>.Instance.DisplayedProducts.Keys.ToArray();

                foreach(var a in r)
                {
                    List<DisplaySlot> slot = Singleton<DisplayManager>.Instance.GetDisplaySlots(a, true);
                    foreach(var b in slot)
                    {
                        List<Product> m_Products = (List<Product>)b.GetType().GetField("m_Products", BindingFlags.NonPublic | BindingFlags.Instance)
                            .GetValue(b);

                        if(m_Products.Count > 0)
                        {
                            var v = b.TakeProductFromDisplay();
                            m_Products.Remove(v);
                            LeanPool.Despawn(v);
                        }
                    }
                }
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.F2))
            {
                MsmpSynchronizationContext SyncContext = client.SyncContext;

                OutSyncAllPacket outSyncAllPacket = new OutSyncAllPacket()
                {
                    Money = 0,
                    TrafficNPCs = SyncContext.GetSyncTrafficNPCs(),
                };

                Console.WriteLine($"[Client] [{nameof(PacketType.OnConnection)}] SyncTraffic c:{outSyncAllPacket.TrafficNPCs.Count}");

                Packet packet = new Packet(PacketType.SyncAll, outSyncAllPacket);

                Console.WriteLine($"PacketType before [{(PacketType)packet._type}]");

                client.SendPayload(packet);
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.F3))
            {
                InMarketShoppingCartPurchasePacket marketShoppingCartPurchase = new InMarketShoppingCartPurchasePacket()
                {
                    FurnituresIds = new int[0],
                    ProductsIds = new int[] { 70 },
                };

                Packet packet = new Packet(PacketType.PurchaseEvent, marketShoppingCartPurchase);

                MsmpClient.Instance.SendPayload(packet);
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.F5) && !client.Connected)
            {
                client.BuildClient(false);
                MsmpClient.Instance = client;
            }
        }

        private void OnDestroy()
        {
            Logger.LogError("Plugin Disposed");
        }
    }
}
