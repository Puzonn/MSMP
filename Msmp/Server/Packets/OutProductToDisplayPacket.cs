using System;

namespace MSMP.Server.Packets
{
    [Serializable]
    internal class OutProductToDisplayPacket
    {
        public Guid BoxNetworkId { get; set; }  
        public int ProductId { get; set; }
        public int DisplayId { get; set; }  
        public int DisplaySlotId { get; set; }
    }
}
