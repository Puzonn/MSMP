using System;

namespace Msmp.Server.Packets.Customers
{
    [Serializable]
    internal class OutCustomerStartShopping
    {
        public ItemQuantity ShoppingList { get; set; }    
        public Guid NetworkId { get; set; } 
    }
}
