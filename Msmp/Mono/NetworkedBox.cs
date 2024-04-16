using Msmp.Client;
using Msmp.Server;
using Msmp.Server.Packets;
using System;
using UnityEngine;

namespace Msmp.Mono
{
    internal class NetworkedBox : MonoBehaviour
    {
        public Guid NetworkId { get; set; }

        public Guid OwnerNetworkId { get; private set; } = default;

        public bool PickedUp { get; private set; }
        public bool Spawned { get; set; } = false;

        private readonly MsmpClient _client = MsmpClient.Instance;

        private GameObject _Owner;

        public bool HasOwner => _Owner != null;

        public void Update()
        {
            /* Play normal pickup animation for local client */
            if (!PickedUp || _Owner == null || OwnerNetworkId == _client.LocalClientNetworkId)
            {
                return;
            }

            Vector3 ownerPosition = _Owner.transform.position;

            transform.position = new Vector3(ownerPosition.x, ownerPosition.y + 0.65f, ownerPosition.z);
        }

        /// <summary>
        /// Called on all clients when some client picks up a box
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

            PickedUp = true;
        }

        /// <summary>
        /// Checks if can be picked up and picks up boxes
        /// </summary>
        /// <param name="value"></param>
        public void SetPickedUp(bool value, bool inform)
        {
            PickedUp = value;
            /* TODO: @see Patch.PickupPatch */

            OutBoxPickedupPacket boxPickedupPacket = new OutBoxPickedupPacket()
            {
                BoxNetworkId = this.NetworkId,
                BoxOwner = _client.LocalClientNetworkId,
            };

            if (inform)
            {
                Packet packet = new Packet(PacketType.BoxPickupEvent, boxPickedupPacket);

                _client.SendPayload(packet);
            }

            if(!value)
            {
                _Owner = null;
                OwnerNetworkId = default;
            }
        }

        public void BoxDropped(Vector3 dropPosition)
        {
            transform.position = dropPosition;
            _Owner = null;
            OwnerNetworkId = default;
            PickedUp = false;
        }
    }
}
