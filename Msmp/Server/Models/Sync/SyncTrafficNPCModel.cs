using System;

namespace Msmp.Server.Models.Sync
{
    [Serializable]
    public class SyncTrafficNPCModel
    {
        public SerializableVector3 Position { get; set; }
        public SerializableVector3 NextWaypointPosition { get; set; }   

        public Guid NetworkId { get; set; }

        public int Prefab { get; set; }
        public int Enterence { get; set; }
        public float Speed { get; set; }
        public int WaypointTravelCount { get; set; }
        public bool Forward { get; set; }
    }
}
