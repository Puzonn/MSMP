using System.IO;
using System;
using System.Net.Sockets;
using System.Collections.Generic;

namespace BipServer
{

    internal class PayloadManager
    {
        public Dictionary<Guid, NetworkStream> Clients = new Dictionary<Guid, NetworkStream>();

        public void AddClient(Guid networkId, NetworkStream stream)
        {
            /* Check if client exist */
            Clients.Add(networkId, stream);
        }

        public void SendPayload(Packet payload)
        {
            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (payload.GetType() != typeof(Packet))
            {
                throw new NotImplementedException("Payload have to be packet type");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] buffer = new byte[1 + payload._data.Length];
                buffer[0] = payload._type;

                Array.Copy(payload._data, 0, buffer, 1, payload._data.Length);

                foreach (var client in Clients)
                {
                    client.Value.Write(buffer, 0, buffer.Length);
                }
            }
        }
    }
}