using BepInEx.Logging;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Bep.Client
{
    internal class BepClient
    {
        public bool Connected => _client.Connected;

        private readonly TcpClient _client;

        private NetworkStream GetStream()
            => _client.GetStream();

        private Thread _responseThread;

        private readonly ManualLogSource _logger;

        public BepClient(ManualLogSource logger)
        {
            _logger = logger;  
            _client = new TcpClient();  
        }

        public void BuildClient()
        {
            _client.Connect("localhost", 35555);
            _responseThread = new Thread(() => { WaitForResponse(); }); 
        }

        private void WaitForResponse()
        {
            NetworkStream _stream = GetStream();
            while (true)
            {
                byte[] buffer = new byte[1024];
                int bytesRead = _stream.Read(buffer, 0, buffer.Length);

                if(bytesRead > 0)
                {
                    string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    _logger.LogInfo($"Received: {receivedMessage}");
                }
            }
        }

        public void SendPayload(Packet payload)
        {
            if(payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if(payload.GetType() != typeof(Packet))
            {
                throw new NotImplementedException("Payload have to be string type");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                byte[] packetBytes = new byte[1 + payload._data.Length];

                packetBytes[0] = payload._type;

                Array.Copy(payload._data, 0, packetBytes, 1, payload._data.Length);

                GetStream().Write(packetBytes, 0, packetBytes.Length);
            }
        }
    }
}
