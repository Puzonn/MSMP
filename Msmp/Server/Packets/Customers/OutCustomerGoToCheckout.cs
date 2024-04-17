using System;

namespace Msmp.Server.Packets.Customers
{
    [Serializable]
    internal class OutCustomerGoToCheckout
    {
        public Guid NetworkId { get; set; } 
    }
}
