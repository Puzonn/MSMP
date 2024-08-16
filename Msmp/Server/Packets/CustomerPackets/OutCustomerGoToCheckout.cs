using System;

namespace Msmp.Server.Packets.CustomerPackets
{
    [Serializable]
    internal class OutCustomerGoToCheckout
    {
        public Guid NetworkId { get; set; } 
    }
}
