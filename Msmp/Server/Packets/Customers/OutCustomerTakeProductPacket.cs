using System;

namespace MSMP.Server.Packets.Customers
{
    [Serializable]
    internal class OutCustomerTakeProductPacket
    {
        public int ProductId { get; set; }
        public int DisplayId { get; set; }
        public int DisplaySlotId { get; set; }
    }
}
