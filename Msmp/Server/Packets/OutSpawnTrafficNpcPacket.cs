using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class OutSpawnTrafficNpcPacket
    {
        public int Prefab {  get; set; }
        public int Enterence { get; set; }
        public float Speed { get; set; }
        public int WaypointTravelCount { get; set; }
        public bool Forward { get; set; }
        public Guid NetworkId { get; set; }
    }
}
