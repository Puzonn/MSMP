using System;

namespace MSMP.Server.Packets
{
    [Serializable]
    internal class OutOpenBoxPacket
    {
        public bool State { get; set; }
        public Guid BoxNetworkId { get; set; } 
    }
}
