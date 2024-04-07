using System;

namespace BipServer
{
    [Flags]
    public enum PacketType : byte
    {
        PlayerMovement = 0x1f,
        CreateFakeClient = 0x2f
    }
}