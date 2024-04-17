using Msmp.Server.Models;
using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class OutSpawnCustomerVector : OutSpawnCustomer
    {
        public SerializableVector3 Position;
    }
}
