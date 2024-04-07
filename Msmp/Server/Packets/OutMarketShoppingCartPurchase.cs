using MSMP.Server.Models;
using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class OutMarketShoppingCartPurchase
    {
        public MarketShoppingCartPurcheItem[] Products { get; set; }
        public MarketShoppingCartPurcheItem[] Furnitures { get; set; }    
    }
}
