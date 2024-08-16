using Msmp.Server.Models;
using System;

namespace Msmp.Server.Packets.CustomerPackets
{
    [Serializable]
    internal class OutCustomerMoveTo
    {
        public Guid NetworkId { get; set; }
        public SerializableVector3 Target { get; set; }
    }
}
