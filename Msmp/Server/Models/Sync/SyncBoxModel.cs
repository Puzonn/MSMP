using System;

namespace Msmp.Server.Models.Sync
{
    [Serializable]
    internal class SyncBoxModel
    {
        public Guid NetworkId { get; set; }
        public Guid OwnerNetworkId { get; set; }
        public SerializableVector3 Position { get; set; }

        public int ProductId { get; set; }  
        public int ProductCount { get; set; }
        public bool IsOpen { get; set; }    
        public bool Spawned { get; set; }
    }
}
