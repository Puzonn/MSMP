using System;

namespace MSMP.Server.Packets
{
    [Serializable]
    internal class OutCustomerTakeProduct
    {
        public int ProductId { get; set; }  
        public int DisplayId { get; set; }
        public int DisplaySlotId { get; set; }  
    }
}
