using System;

namespace Msmp.Server.Models.Sync
{
    [Serializable]
    internal class SyncCustomerModel
    {
        public SerializableVector3 Position { get; set; }

        public bool WithVector { get; set; }    
        public int PrefabIndex { get; set; }
        public int SpawnTransformIndex { get; set; }   
    }
}
