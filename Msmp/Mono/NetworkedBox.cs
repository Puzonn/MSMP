using Msmp.Client;
using Msmp.Mono;
using Msmp.Server;
using Msmp.Server.Packets;
using System;
using UnityEngine;

namespace MSMP.Mono
{
    internal class NetworkedBox : MonoBehaviour
    {
        public Guid BoxNetworkId { get; set; }  
        
        public Guid OwnerNetworkId { get; private set; }

        public bool PickedUp { get; private set; }

        private readonly MsmpClient _client = MsmpClient.Instance;

        private GameObject _Owner;

        public void Update()
        {
            if (!PickedUp || _Owner == null)
            {
                return;
            }

            Vector3 ownerPosition = _Owner.transform.position;

            Console.WriteLine($"Box transform to {ownerPosition}");

            transform.position = new Vector3(ownerPosition.x, ownerPosition.y + 0.65f, ownerPosition.z);
        }

        /// <summary>
        /// Called on all clients when some clinet picks up a box
        /// </summary>
        public void SetPickedUp(Guid owner)
        {
            OwnerNetworkId = owner;

            NetworkedPlayer networkedPlayer = Array.Find(FindObjectsOfType<NetworkedPlayer>(), x => x.NetworkId == owner);

            if(networkedPlayer == null)
            {
                throw new Exception($"Player dose not exist as {nameof(NetworkedPlayer)}");
            }

            _Owner = networkedPlayer.gameObject;

            GetComponent<Rigidbody>().isKinematic = false;
        }

        /// <summary>
        /// Checks if can be picked up and picks up boxes
        /// </summary>
        /// <param name="value"></param>
        public void SetPickedUp(bool value)
        {
            PickedUp = value;
            /* TODO: @see Patch.PickupPatch */

            BoxPickedupPacket boxPickedupPacket = new BoxPickedupPacket()
            {
                BoxNetworkId = this.BoxNetworkId,
                BoxOwner = _client.LocalClientNetworkId,
            };

            Packet packet = new Packet(PacketType.PickupEvent, boxPickedupPacket);

            _client.SendPayload(packet);

            if(!value)
            {
                _Owner = null;
                OwnerNetworkId = default;
            }
        }
    }
}
