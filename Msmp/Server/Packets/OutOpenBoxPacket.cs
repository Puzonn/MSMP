using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class OutOpenBoxPacket
    {
        public Guid BoxNetworkId { get; set; }

        public bool State { get; set; }
    }
}
