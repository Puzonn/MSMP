using System;

namespace Msmp.Server.Models
{
    [Serializable]
    public class ClientModel
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Guid ClientId { get; set; }  
    }
}
