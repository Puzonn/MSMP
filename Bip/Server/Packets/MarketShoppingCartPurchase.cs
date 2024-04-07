using System;

namespace Msmp.Server.Packets
{
    [Serializable]
    internal class MarketShoppingCartPurchase
    {
        public int[] ProductIds { get; set; }
        public int[] FurnituresIds { get; set; }    
    }
}
