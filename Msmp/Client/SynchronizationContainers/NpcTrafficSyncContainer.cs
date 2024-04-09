using BepInEx.Logging;
using Msmp.Server.Models;
using Msmp.Server.Models.Sync;
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;

namespace Msmp.Client.SynchronizationContainers
{
    public class NpcTrafficSyncContainer
    {
        /* TODO: Possibly cache reference to GameObjects */
        public readonly List<SyncTrafficNPC> SyncTrafficNPCs = new List<SyncTrafficNPC>();

        private readonly ManualLogSource _logger;

        public NpcTrafficSyncContainer(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void Add(SyncTrafficNPC npc)
        {
            SyncTrafficNPCs.Add(npc);
        }

        public List<SyncTrafficNPCModel> Get()
        {
            List<SyncTrafficNPCModel> updated = new List<SyncTrafficNPCModel>();

            foreach(SyncTrafficNPC npc in SyncTrafficNPCs)
            {
                int m_TripLength = (int)npc.Navigator.GetType()
                    .GetField("m_TripLength", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(npc.Navigator);

                Waypoint m_CurrentWaypoint = (Waypoint)npc.Navigator.GetType()
                   .GetField("m_CurrentWaypoint", BindingFlags.NonPublic | BindingFlags.Instance)
                   .GetValue(npc.Navigator);

                Vector3 waypointPosition = m_CurrentWaypoint.GetPosition;
                Vector3 navigatorPosition = npc.Navigator.transform.position;

                updated.Add(new SyncTrafficNPCModel()
                {
                    Enterence = npc.Enterence,
                    Forward = npc.Forward,
                    NetworkId = npc.NetworkId,
                    NextWaypointPosition = new SerializableVector3(waypointPosition),
                    Position = new SerializableVector3(navigatorPosition),
                    Prefab = npc.Prefab,
                    Speed = npc.Speed,
                    WaypointTravelCount = m_TripLength
                });
            }

            return updated;
        }

        public void Remove(Guid networkId)
        {
            SyncTrafficNPC syncNpc = SyncTrafficNPCs.Find(x => x.NetworkId == networkId);   

            if(syncNpc == null)
            {
                _logger.LogWarning($"[Client] [{nameof(NpcTrafficSyncContainer)}] Tried remove traffic npc {networkId} that doesnt exist");

                return;
            }

            SyncTrafficNPCs.Remove(syncNpc);
        }

        public class SyncTrafficNPC
        {
            public WaypointNavigator Navigator;

            public Guid NetworkId { get; set; }

            public int Prefab { get; set; }
            public int Enterence { get; set; }
            public float Speed { get; set; }
            public bool Forward { get; set; }
        }
    }
}
