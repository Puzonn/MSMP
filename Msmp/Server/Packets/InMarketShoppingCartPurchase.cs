using System;

namespace MSMP.Server.Packets
{
    [Serializable]
    internal class InMarketShoppingCartPurchase
    {
        public int[] ProductsIds { get; set; }
        public int[] FurnituresIds { get; set; } 
    }
}
