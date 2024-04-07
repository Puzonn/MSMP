using System;
using UnityEngine;

namespace MSMP.Mono
{
    internal class NetworkedBox : MonoBehaviour
    {
        public Guid BoxNetworkId { get; set; }   
        public bool PickedUp { get; set; }

        public void Update()
        {
            if (PickedUp)
            {
                Console.WriteLine("Picked");
            }
        }

        public void Awake()
        {
            Console.WriteLine("Box initalized!");
        }
    }
}
