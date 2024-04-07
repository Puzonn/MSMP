using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class OutBoxDropPacket
    {
        /// <summary>
        /// Position where box was dropped
        /// </summary>
        public float x, y, z;

        public Guid BoxNetworkId {  get; set; } 
    }
}
