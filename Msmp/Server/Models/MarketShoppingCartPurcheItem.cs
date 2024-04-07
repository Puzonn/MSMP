using System;

namespace MSMP.Server.Models
{
    [Serializable]
    internal class MarketShoppingCartPurcheItem
    {
        public int ItemId { get; set; } 
        public Guid NetworkItemId { get; set; }
    }
}
