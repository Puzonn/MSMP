using System;

namespace Msmp.Server.Models
{
    [Serializable]
    internal class MarketShoppingCartPurcheItem
    {
        public Guid NetworkItemId { get; set; }

        public int ItemId { get; set; } 
    }
}
