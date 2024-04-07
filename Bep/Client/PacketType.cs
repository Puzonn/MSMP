using System;

namespace Bep.Client
{
    [Flags]
    internal enum PacketType : byte
    {
        PlayerMovement = 0x1f,
        CreateFakeClient = 0x2f
    }
}
