using System.Text;

namespace Bep.Client
{
    internal class Packet
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
