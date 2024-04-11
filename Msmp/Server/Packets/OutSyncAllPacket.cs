using Msmp.Server.Models.Sync;
using System;
using System.Collections.Generic;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class OutSyncAllPacket
    {
        public float Money { get; set; }    
        public List<SyncTrafficNPCModel> TrafficNPCs { get; set; }
        public List<SyncCustomerModel> Customer { get; set; }
    }
}
