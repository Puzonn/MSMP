using System;

namespace Msmp.Server.Packets.CustomerPackets
{
    [Serializable]
    internal class OutCustomerWalkAround
    {
        public Guid NetworkId { get; set; } 
        public int DisplaySlotId { get; set; }  
    }
}
