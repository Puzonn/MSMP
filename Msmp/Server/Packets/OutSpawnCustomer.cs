using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class OutSpawnCustomer
    {
        public Guid NetworkId { get; set; } 
        public int SpawnTransformIndex { get; set; }
        public int PrefabIndex { get; set; }   
    }
}
