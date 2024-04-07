using System;

namespace Msmp.Server
{
    [Flags]
    public enum PacketType : byte
    {
        ClientAuth = 0x0f,
        OnConnection = 0x1f,
        CreateFakeClient = 0x2f,
        PlayerMovement = 0x3f,
        MoneyChanged = 0x4f,
        PlayerRotate = 0x5,
        PurchaseEvent = 0x6
    }
}