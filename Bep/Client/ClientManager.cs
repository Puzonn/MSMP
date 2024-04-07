using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bep.Client
{
    internal class ClientManager
    {
        /* GameObject is reference to PlayerController */
        public Dictionary<Guid, GameObject> Clients = new Dictionary<Guid, GameObject>();

        public void AddClient(Guid guid, GameObject gameObject)
        {
            Clients.Add(guid, gameObject);
        }
        
        public void CreateFakeClient(Guid guid, Vector3 position)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = position;
            cube.transform.localScale = new Vector3(1f, 1f, 1f);
            Clients.Add(guid, cube);
        }

        public void Move(Guid guid, Vector3 position)
        {
            if(!Clients.ContainsKey(guid))
            {
                throw new Exception($"[Client] {guid.ToString()} does not exist");
            }

            Clients[guid].transform.position = position;
        }
    }
}
