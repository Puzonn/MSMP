using HarmonyLib;
using Lean.Pool;
using Msmp.Client;
using Msmp.Mono;
using Msmp.Server;
using MyBox;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MSMP.Patch.Traffic
{
    [HarmonyPatch(typeof(NPCTrafficManager))]
    [HarmonyPatch("RemoveNPC")]
    internal class NpcTrafficManagerDespawnPatch
    {
        [HarmonyPrefix]
        static bool Prefix(WaypointNavigator npc)
        {
            MsmpClient client = MsmpClient.Instance;

            if(client == null || !client.IsServer)
            {
                return false;
            }

            Guid networkId = npc.GetComponent<NetworkedTrafficNPC>().NetworkId;

            Packet packet = new Packet(PacketType.DespawnTraffic, networkId.ToByteArray());

            client.SendPayload(packet);

            return false;
        }

        public static void RemoveTrafficNPC(Guid networkId)
        {
            MsmpClient client = MsmpClient.Instance;

            if (client.SyncContext.NpcTrafficContainer.Remove(networkId))
            {
                var traffic = Array.Find(UnityEngine.Object.FindObjectsOfType<NetworkedTrafficNPC>(), x => x.NetworkId == networkId);

                var manager = Singleton<NPCTrafficManager>.Instance;
                var navigator = traffic.GetComponent<WaypointNavigator>();

                LeanPool.Despawn(navigator);

                List<WaypointNavigator> m_ActiveNPCs = (List<WaypointNavigator>)manager.GetType()
                    .GetField("m_ActiveNPCs", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(manager);

                m_ActiveNPCs.Remove(navigator);
            }
        }
    }
}
