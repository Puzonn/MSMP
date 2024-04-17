using System;

namespace Msmp.Server.Packets.Customers
{
    [Serializable]
    internal class OutCustomerTakeProductPacket
    {
        public Guid NetworkId { get; set; }
        public int ProductId { get; set; }
        public int DisplaySlotId { get; set; }
    }
}
