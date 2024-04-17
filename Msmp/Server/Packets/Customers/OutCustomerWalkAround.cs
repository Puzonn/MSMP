using System;

namespace Msmp.Server.Packets.Customers
{
    [Serializable]
    internal class OutCustomerWalkAround
    {
        public Guid NetworkId { get; set; } 
        public int DisplaySlotId { get; set; }  
    }
}
