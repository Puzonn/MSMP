using Msmp.Client;
using BepInEx;
using BepInEx.Logging;
using Msmp.Server;
using MyBox;
using UnityEngine;
using System.Threading;
using Msmp.Patch.Shop;
using Msmp.Client.Controllers;
using HarmonyLib;
using Msmp.Server.Packets;
using Msmp.Patch.Traffic;
using Msmp.Patch.BoxObject;
using Msmp.Patch.Customers;
using System;

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
            Harmony.CreateAndPatchAll(typeof(NpcTrafficManagerPatch));
            Harmony.CreateAndPatchAll(typeof(WaypointNavigatorPatch));
            Harmony.CreateAndPatchAll(typeof(SpawnCustomerPatch));
            Harmony.CreateAndPatchAll(typeof(SpawnCustomerVectorPatch));
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
                Singleton<MoneyManager>.Instance.MoneyTransition(500, MoneyManager.TransitionType.BILLS);
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
