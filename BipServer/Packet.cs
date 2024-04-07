using System.Text;

namespace BipServer
{
    public class Packet
    {
        public readonly byte[] _data;
        public readonly byte _type;

        public Packet(PacketType type, object data)
        {
            _type = (byte)type;

            /* TODO: Check if data is string*/
            _data = Encoding.UTF8.GetBytes((string)data);
        }
    }
}
