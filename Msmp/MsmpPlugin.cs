using Msmp.Client;
using BepInEx;
using BepInEx.Logging;
using Msmp.Server;
using MyBox;
using UnityEngine;
using System.Threading;
using Msmp.Client.Controllers;
using HarmonyLib;
using Msmp.Patch;
using MSMP.Patch;

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
        private const string PluginName = "Msmp";
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
            Harmony.CreateAndPatchAll(typeof(PickupPatch));
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

            if (UnityInput.Current.GetKey(KeyCode.F3))
            {
                CheckoutInteraction i = UnityEngine.Object.FindObjectsOfType<CheckoutInteraction>()[0];

                i.onCheckoutClosed += (checkout) =>
                {
                    Log.LogInfo($"Checkut ! {checkout}");
                };

                i.onCheckoutBoxed += (checkout) =>
                {
                    Log.LogInfo($"Checkout ! boxed");
                };
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.F1))
            {
                Singleton<MoneyManager>.Instance.MoneyTransition(500, MoneyManager.TransitionType.BILLS);
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
