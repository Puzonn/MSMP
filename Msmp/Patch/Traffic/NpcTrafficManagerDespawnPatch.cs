using HarmonyLib;
using Msmp.Client;
using Msmp.Mono;
using Msmp.Server;
using MyBox;
using System;

namespace MSMP.Patch.Traffic
{
    [HarmonyPatch(typeof(NPCTrafficManager))]
    [HarmonyPatch("RemoveNPC")]
    internal class NpcTrafficManagerDespawnPatch
    {
        [HarmonyPrefix]
        public bool Prefix(WaypointNavigator npc)
        {
            MsmpClient client = MsmpClient.Instance;

            if(client == null || !client.IsServer)
            {
                return false;
            }

            Guid networkId = npc.GetComponent<NetworkedTrafficNPC>().NetworkId;

            Console.WriteLine($"[Client] [{nameof(PacketType.DespawnTraffic)}] Despawning {networkId}");

            Packet packet = new Packet(PacketType.DespawnTraffic, networkId.ToByteArray());

            client.SendPayload(packet);

            return false;
        }

        public static void RemoveTrafficNPC(Guid networkId)
        {
            MsmpClient client = MsmpClient.Instance;

            client.SyncContext.NpcTrafficContainer
                .Remove(networkId);

            var traffic = Array.Find(UnityEngine.Object.FindObjectsOfType<NetworkedTrafficNPC>(), x => x.NetworkId == networkId);

            var navigator = traffic.GetComponent<WaypointNavigator>();

            Singleton<NPCTrafficManager>.Instance.RemoveNPC(navigator);
        }
    }
}
