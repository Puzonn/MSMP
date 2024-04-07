using System;
using System.Numerics;

namespace Msmp.Server.Models
{
    internal class InCreateFakeClientModel
    {
        public Vector3 SpawnPosition { get; set; }
        public Guid ClientGuid { get; set; }
    }
}