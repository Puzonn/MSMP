using HarmonyLib;
using System;
using UnityEngine;
using System.Reflection;
using Random = UnityEngine.Random;
using Lean.Pool;
using System.Collections.Generic;
using Msmp.Mono;
using Msmp.Server.Packets;
using MyBox;
using Msmp.Server;
using Msmp.Client;
using Msmp.Client.SynchronizationContainers;
using Msmp.Server.Models.Sync;

namespace Msmp.Patch.Traffic
{
    [HarmonyPatch(typeof(NPCTrafficManager))]
    [HarmonyPatch("SpawnNPC")]
    internal class NpcTrafficManagerPatch
    {
        [HarmonyPrefix]
        static bool Prefix(NPCTrafficManager __instance)
        {
            if (MsmpClient.Instance == null || !MsmpClient.Instance.IsServer)
            {
                Console.WriteLine($"[Client] [{nameof(NpcTrafficManagerPatch)}] You're not server");
                return false;
            }

            if (!MsmpClient.Instance.Connected)
            {
                Console.WriteLine($"[Client] [{nameof(NpcTrafficManagerPatch)}] You're not connected to any server");
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
            bool forward = Mathf.RoundToInt(Random.value) == 0f;

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

            Console.WriteLine($"[Client] [{nameof(NpcTrafficManagerPatch)}] [{PacketType.SpawnTrafficNpc}] id: {outSpawnTrafficNpcPacket.NetworkId}");

            return false;
        }

        public static void SpawnTraffic(OutSpawnTrafficNpcPacket packet)
        {
            SpawnTraffic(packet.Prefab, packet.Enterence, packet.Forward, packet.WaypointTravelCount,packet.Speed, packet.NetworkId);

            Console.WriteLine($"[Client] {PacketType.SpawnTrafficNpc} spawned id: {packet.NetworkId}");
        }

        public static WaypointNavigator SpawnTraffic(int prefab, int enterance,
            bool forward, int travelCount, float speed, Guid networkId)
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

            WaypointNavigator waypointNavigator = LeanPool.Spawn
               (m_NPCPrefabs[prefab], m_BuildingEnterences[enterance].transform.position, 
               m_BuildingEnterences[enterance].transform.rotation, manager.transform);

            waypointNavigator.SetupTravel(m_BuildingEnterences[enterance].GetWaypoint(forward), forward, travelCount, speed);
            waypointNavigator.gameObject.AddComponent<NetworkedTrafficNPC>().NetworkId = networkId;

            m_ActiveNPCs.Add(waypointNavigator);

            if (MsmpClient.Instance.IsServer)
            {
                MsmpClient.Instance.SyncContext.NpcTrafficContainer.Add(new NpcTrafficSyncContainer.SyncTrafficNPC()
                {
                    Enterence = enterance,
                    Forward = forward,
                    NetworkId = networkId,
                    Prefab = prefab,
                    Speed = speed,
                    Navigator = waypointNavigator
                });
            }

            return waypointNavigator;
        }

        public static WaypointNavigator SpawnTraffic(int prefab, int enterance,
           bool forward, int travelCount, float speed, Guid networkId, Vector3 waypoint, Vector3 position)
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

            WaypointNavigator waypointNavigator = LeanPool.Spawn
               (m_NPCPrefabs[prefab], m_BuildingEnterences[enterance].transform.position,
               m_BuildingEnterences[enterance].transform.rotation, manager.transform);
            
            waypointNavigator.SetupTravel(m_BuildingEnterences[enterance].GetWaypoint(forward), forward, travelCount, speed);
            waypointNavigator.gameObject.AddComponent<NetworkedTrafficNPC>().NetworkId = networkId;

            waypointNavigator.GetComponent<NPC>().SetDestination(waypoint);

            m_ActiveNPCs.Add(waypointNavigator);

            if (MsmpClient.Instance.IsServer)
            {
                MsmpClient.Instance.SyncContext.NpcTrafficContainer.Add(new NpcTrafficSyncContainer.SyncTrafficNPC()
                {
                    Enterence = enterance,
                    Forward = forward,
                    NetworkId = networkId,
                    Prefab = prefab,
                    Speed = speed,
                    Navigator = waypointNavigator
                });
            }

            waypointNavigator.transform.position = position;

            return waypointNavigator;
        }

        public static void SyncTraffic(List<SyncTrafficNPCModel> packet) 
        {
            NPCTrafficManager manager = Singleton<NPCTrafficManager>.Instance;

            List<WaypointNavigator> m_ActiveNPCs = (List<WaypointNavigator>)manager.GetType()
                .GetField("m_ActiveNPCs", BindingFlags.NonPublic | BindingFlags.Instance)  
                .GetValue(manager);

            Console.WriteLine($"[Client] [{nameof(PacketType.SyncAll)}] Despawning all current traffic");

            foreach (var trafficNpc in m_ActiveNPCs)
            {
                LeanPool.Despawn(trafficNpc);
            }

            foreach(var npc in packet)
            {
                SpawnTraffic(npc.Prefab, npc.Enterence, npc.Forward, npc.WaypointTravelCount,
                    npc.Speed, npc.NetworkId, npc.NextWaypointPosition.ToVector3(), npc.Position.ToVector3());

                Console.WriteLine($"Spawning traffic npc {npc.NetworkId}");
            }
        }
    }
}
