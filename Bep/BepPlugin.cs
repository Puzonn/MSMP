using Bep.Client;
using Bep.Patch;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using ES3Types;
using HarmonyLib;
using MyBox;
using System.Reflection;
using UnityEngine;

namespace Bep
{
    // TODO Review this file and update to your own requirements.

    [BepInPlugin(MyGUID, PluginName, VersionString)]
    public class BepPlugin : BaseUnityPlugin
    {
        public static BepPlugin Instance;
        public static MoneyManager money;

        private const string MyGUID = "com.Puzonne.Bep";
        private const string PluginName = "Bep";
        private const string VersionString = "1.0.0";

        private static readonly Harmony Harmony = new Harmony(MyGUID);
        public static ManualLogSource Log = new ManualLogSource(PluginName);

        private BepClient client;

        public BepPlugin()
        {
            Instance = this;
        }

        private void Awake()
        {
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loading...");
            Harmony.PatchAll();
            Logger.LogInfo($"PluginName: {PluginName}, VersionString: {VersionString} is loaded.");
            Log = Logger;
        }

        private static MoneyManager moneyManager = null;
        private static PlayerController playerController = null;
        private Vector3 LastPos = new Vector3();

        private void Update()
        {
            if (true)
            {
                if (moneyManager == null || playerController == null)
                {
                    playerController = Singleton<PlayerController>.Instance;
                    moneyManager = Singleton<MoneyManager>.Instance;
                }
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.F6))
            {
                moneyManager.MoneyTransition(500, MoneyManager.TransitionType.BILLS);
            }

            if (UnityInput.Current.GetKeyDown(KeyCode.F5))
            {
                Logger.LogInfo("Starting client");
                client = new BepClient(Log);
                client.BuildClient();
            }

            if(client.Connected)
            {
                if (playerController != null && LastPos != playerController.transform.position)
                {
                    LastPos = playerController.transform.position;
                    Logger.LogInfo("Sending pos");
                    Packet packet = new Packet(PacketType.PlayerMovement, LastPos.ToString());
                    client.SendPayload(packet);
                }
            }
        }

        private void OnDestroy()
        {
            Logger.LogError("Plugin Disposed");
        }
    }
}
