using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace BipServer.Models
{
    internal class CreateFakeClientModel
    {
        public Vector3 SpawnPosition { get; set; }
        public Guid ClientGuid { get; set; }
    }
}