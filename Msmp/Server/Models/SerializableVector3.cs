using System;
using UnityEngine;

namespace Msmp.Server.Models
{
    [Serializable]
    public class SerializableVector3
    {
        public float X; 
        public float Y; 
        public float Z; 

        public SerializableVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public SerializableVector3(Vector3 vector)
        {
            X = vector.x;
            Y = vector.y;
            Z = vector.z;
        }

        public Vector3 ToVector3() 
            => new Vector3(X, Y, Z);
    }
}
