using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class BoxPickedupPacket
    {
        public Guid BoxOwner { get; set; }
        public Guid BoxNetworkId { get; set; }  
    }
}
