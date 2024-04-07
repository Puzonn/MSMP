using System;
using System.Collections.Generic;
using Msmp.Server.Models;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class InUserConnected
    {
        public List<ClientModel> ConnectedClients { get; set; }
        public Guid UserId { get; set; }
    }
}
