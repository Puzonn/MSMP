using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class OutSpawnCustomer
    {
        public int SpawnTransformIndex { get; set; }
        public int PrefabIndex { get; set; }   
    }
}
