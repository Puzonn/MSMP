using System;

namespace Msmp.Server
{
    [Flags]
    public enum PacketType : ushort 
    {
        ClientAuth = 0x0,
        OnConnection = 0x1,
        CreateFakeClient = 0x2,
        PlayerMovement = 0x3,
        MoneyChanged = 0x4,
        PlayerRotate = 0x5,
        PurchaseEvent = 0x6,
        BoxPickupEvent = 0x7,
        BoxDropEvent = 0x8,
        ProductToDisplayEvent = 0x9,
        OpenBoxEvent = 0x10,
        SpawnTrafficNpc = 0x11,
        TrafficNpcSetDestination = 0x12,
        SpawnCustomer = 0x13,
        SpawnCustomerVector = 0x14,
        /* Acutally can be send */
        SyncAll = 0x15,
        DespawnTraffic = 0x16
    }
}