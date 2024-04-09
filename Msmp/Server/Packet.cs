using System;
using System.CodeDom;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Msmp.Server
{
    public class Packet
    {
        public readonly byte[] _data;
        public readonly byte _type;

        public Packet(PacketType type, string data)
        {
            _type = (byte)type;

            /* TODO: Check if data is string*/
            _data = Encoding.UTF8.GetBytes((string)data);
        }

        public Packet(PacketType type, object packet)
        {
            _type = (byte)type;

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, packet);
                _data = stream.ToArray();
            }
        }

        public Packet(PacketType type, byte[] data)
        {
            _type = (byte)type;
            _data = data;
        }

        public Packet(PacketType type)
        {
            _type = (byte)type;
        }

        /// <summary>
        /// Use only if you want to reuse buffer 
        /// </summary>
        /// <param name="data">Reusable buffer</param>
        public Packet(byte[] data)
        {
            _type = data[0];
            _data = data.Skip(1).ToArray();
        }

        public static T Deserialize<T>(byte[] data)
        {
            /* Cut data buffer from packet type which is one byte `*/
            byte[] _ = new byte[data.Length-1];
            Array.Copy(data, 1, _, 0, _.Length);

            using (MemoryStream stream = new MemoryStream(_))
            {
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
