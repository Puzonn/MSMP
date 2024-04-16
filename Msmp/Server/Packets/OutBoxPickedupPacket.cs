using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class OutBoxPickedupPacket
    {
        public Guid BoxOwner { get; set; }
        public Guid BoxNetworkId { get; set; }  
    }
}
