using Msmp.Server.Models;
using System;
using System.Collections.Generic;

namespace Msmp.Server.Packets.Customers
{
    [Serializable]
    internal class OutCustomerStartShopping
    {
        public ItemQuantity ShoppingList { get; set; }
        public Guid NetworkId { get; set; }
        public List<ProcessedProduct> ProcessedProducts { get; set; }
        public int WalkRandomDisplaySlot { get; set; }
        public int WalkRandomDisplay { get; set; }
    }
}
