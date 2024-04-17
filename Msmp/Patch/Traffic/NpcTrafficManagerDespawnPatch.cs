using HarmonyLib;
using Lean.Pool;
using Msmp.Client;
using Msmp.Mono;
using Msmp.Server;
using MyBox;
using System;
using System.Collections.Generic;
using System.Reflection;
using static Msmp.Client.SynchronizationContainers.NpcTrafficSyncContainer;

namespace Msmp.Patch.Traffic
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

            SyncTrafficNPC npc = client.SyncContext.NpcTrafficContainer.Get().Find(x => x.NetworkId == networkId);

            if(npc == null)
            {
                return;
            }

            LeanPool.Despawn(npc.Navigator, 0f);

            var manager = Singleton<NPCTrafficManager>.Instance;

            List<WaypointNavigator> m_ActiveNPCs = (List<WaypointNavigator>)manager.GetType()
                .GetField("m_ActiveNPCs", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(manager);

            m_ActiveNPCs.Remove(npc.Navigator);

            client.SyncContext.NpcTrafficContainer.Remove(networkId);
        }
    }
}
