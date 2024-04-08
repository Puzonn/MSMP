using HarmonyLib;
using System;
using UnityEngine;
using System.Reflection;
using Random = UnityEngine.Random;
using Lean.Pool;
using System.Collections.Generic;
using MSMP.Mono;
using MSMP.Server.Packets;
using MyBox;
using Msmp.Server;
using Msmp.Client;

namespace MSMP.Patch
{
    [HarmonyPatch(typeof(NPCTrafficManager))]
    [HarmonyPatch("SpawnNPC")]
    internal class NpcTrafficManagerPatch
    {
        [HarmonyPrefix]
        static bool Prefix(NPCTrafficManager __instance)
        {
            if (!MsmpClient.Instance.IsServer)
            {
                return false;
            }

            if(MsmpClient.Instance == null || !MsmpClient.Instance.Connected)
            {
                return false;
            }

            WaypointNavigator[] m_NPCPrefabs = (WaypointNavigator[])__instance.GetType()
                .GetField("m_NPCPrefabs", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(__instance);

            BuildingEnterence[] m_BuildingEnterences = (BuildingEnterence[])__instance.GetType()
               .GetField("m_BuildingEnterences", BindingFlags.NonPublic | BindingFlags.Instance)
               .GetValue(__instance);

            Vector2Int m_TripLengthRange = (Vector2Int)__instance.GetType()
               .GetField("m_TripLengthRange", BindingFlags.NonPublic | BindingFlags.Instance)
               .GetValue(__instance);

            Vector2 m_SpeedRange = (Vector2)__instance.GetType()
               .GetField("m_SpeedRange", BindingFlags.NonPublic | BindingFlags.Instance)
               .GetValue(__instance);

            int randomPrefab = Random.Range(0, m_NPCPrefabs.Length);
            int randomEnterence = Random.Range(0, m_BuildingEnterences.Length);
            float speed = Random.Range(m_SpeedRange.x, m_SpeedRange.y);
            int waypointTravelCount = Random.Range(m_TripLengthRange.x, m_TripLengthRange.y);
            bool forward = (float)Mathf.RoundToInt(Random.value) == 0f;
            Guid networkId = Guid.NewGuid();

            OutSpawnTrafficNpcPacket outSpawnTrafficNpcPacket = new OutSpawnTrafficNpcPacket()
            {
                Enterence = randomEnterence,
                Prefab = randomPrefab,
                Speed = speed,
                WaypointTravelCount = waypointTravelCount,
                Forward = forward,
                NetworkId = networkId
            };

            Packet packet = new Packet(PacketType.SpawnTrafficNpc, outSpawnTrafficNpcPacket);

            MsmpClient.Instance.SendPayload(packet);

            return false;
        }

        public static void SpawnNpc(OutSpawnTrafficNpcPacket packet)
        {
            NPCTrafficManager manager = Singleton<NPCTrafficManager>.Instance;

            WaypointNavigator[] m_NPCPrefabs = (WaypointNavigator[])manager.GetType()
               .GetField("m_NPCPrefabs", BindingFlags.NonPublic | BindingFlags.Instance)
               .GetValue(manager);

            BuildingEnterence[] m_BuildingEnterences = (BuildingEnterence[])manager.GetType()
               .GetField("m_BuildingEnterences", BindingFlags.NonPublic | BindingFlags.Instance)
               .GetValue(manager);

            List<WaypointNavigator> m_ActiveNPCs = (List<WaypointNavigator>)manager.GetType()
              .GetField("m_ActiveNPCs", BindingFlags.NonPublic | BindingFlags.Instance)
              .GetValue(manager);

            WaypointNavigator prefab = m_NPCPrefabs[packet.Prefab];
            BuildingEnterence buildingEnterence = m_BuildingEnterences[packet.Enterence];
            WaypointNavigator waypointNavigator = LeanPool.Spawn<WaypointNavigator>
                (prefab, buildingEnterence.transform.position, buildingEnterence.transform.rotation, manager.transform);
            waypointNavigator.SetupTravel(buildingEnterence.GetWaypoint(packet.Forward), packet.Forward, packet.WaypointTravelCount, packet.Speed);
            waypointNavigator.gameObject.AddComponent<NetworkedTrafficNPC>().NetworkId = packet.NetworkId;

            m_ActiveNPCs.Add(waypointNavigator);

            Console.WriteLine($"[Client] {PacketType.SpawnTrafficNpc} spawned id: {packet.NetworkId}");
        }
    }
}
