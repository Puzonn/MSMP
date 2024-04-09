using System;

namespace Msmp.Server.Models
{
    [Serializable]
    public class ClientModel
    {
        public Guid ClientId { get; set; }

        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
    }
}
