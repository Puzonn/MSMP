using System;

namespace MSMP.Server.Packets
{
    [Serializable]
    internal class InMarketShoppingCartPurchasePacket
    {
        public int[] ProductsIds { get; set; }
        public int[] FurnituresIds { get; set; } 
    }
}
