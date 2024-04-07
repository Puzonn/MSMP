namespace BipServer
{
    internal class ClientManager
    {
        private readonly PayloadManager context;

        public ClientManager(PayloadManager _context)
        {
            context = _context;
        }

        public void OnPacket(PacketType type, byte[] data)
        {
            switch(type)
            {
                case PacketType.PlayerMovement:
                    OnMovement(data); break;    
            }
        }

        private void OnMovement(byte[] data)
        {

        }
    }
}