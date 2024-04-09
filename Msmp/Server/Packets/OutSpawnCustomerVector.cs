using System;
using UnityEngine;

namespace MSMP.Server.Packets
{
    [Serializable]
    internal class OutSpawnCustomerVector : OutSpawnCustomer
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Vector3 GetVector()
            => new Vector3(x, y, z);
    }
}
