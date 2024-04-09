using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class OutTrafficNpcSetDestinationPacket
    {
        public float x {  get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public Guid NetworkId { get; set; } 
    }
}
