using System;

namespace Msmp.Server.Packets.CheckoutPackets
{
    [Serializable]
    internal class OutCheckoutCardPayment
    {
        public float Total { get; set; }
        public int CheckoutId { get; set; }
    }
}
