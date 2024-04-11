using HarmonyLib;
using Msmp.Client;
using Msmp.Server;
using Msmp.Mono;
using Msmp.Server.Packets;
using MyBox;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Msmp.Patch.Traffic
{
    [HarmonyPatch(typeof(WaypointNavigator))]
    [HarmonyPatch("ReachedWaypoint")]
    internal class WaypointNavigatorPatch
    {
        [HarmonyPostfix]
        static void Postfix(WaypointNavigator __instance)
        {
            if (MsmpClient.Instance == null || !MsmpClient.Instance.IsServer)
            {
                return;
            }

            Waypoint nextWaypoint = (Waypoint)__instance.GetType()
                .GetField("m_CurrentWaypoint", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            NetworkedTrafficNPC networkedTrafficNPC = __instance.GetComponent<NetworkedTrafficNPC>();

            OutTrafficNpcSetDestinationPacket outTrafficNpcSetDestinationPacket = new OutTrafficNpcSetDestinationPacket()
            {
                NetworkId = networkedTrafficNPC.NetworkId,
                x = nextWaypoint.GetPosition.x,
                y = nextWaypoint.GetPosition.y,
                z = nextWaypoint.GetPosition.z
            };

            Packet packet = new Packet(PacketType.TrafficNpcSetDestination, outTrafficNpcSetDestinationPacket);

            MsmpClient.Instance.SendPayload(packet);
        }

        /* TODO: Cache all npcs */
        public static void SetDestination(OutTrafficNpcSetDestinationPacket packet)
        {
            NPCTrafficManager manager = Singleton<NPCTrafficManager>.Instance;

            List<WaypointNavigator> m_ActiveNPCs = (List<WaypointNavigator>)manager.GetType()
             .GetField("m_ActiveNPCs", BindingFlags.NonPublic | BindingFlags.Instance)
             .GetValue(manager);

            WaypointNavigator navigator = m_ActiveNPCs.Find(x => x.GetComponent<NetworkedTrafficNPC>().NetworkId == packet.NetworkId);

            if(navigator == null)
            {
                Console.WriteLine($"[Client] [{nameof(WaypointNavigatorPatch)}] NPC dose not exist with current navigator id: {packet.NetworkId}");
                return;
            }

            NPC npc = (NPC)navigator.GetType().GetField("m_Npc", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(navigator);

            npc.SetDestination(new Vector3(packet.x, packet.y, packet.z));
        }
    }
}
