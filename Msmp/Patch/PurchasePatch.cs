using Msmp.Client;
using Msmp.Server;
using HarmonyLib;
using System.Reflection;
using System.Linq;
using MSMP.Server.Packets;

namespace Msmp.Patch
{
    [HarmonyPatch(typeof(MarketShoppingCart))]
    [HarmonyPatch("Purchase")]
    internal class PurchasePatch
    {
        [HarmonyPrefix]
        static void PreFix(MarketShoppingCart __instance)
        {
            MarketShoppingCart.CartData m_CartData = (MarketShoppingCart.CartData)__instance.GetType()
                .GetField("m_CartData", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            int[] productsIds = m_CartData.ProductInCarts.Select(x => x.FirstItemID).ToArray();
            int[] furnituresIds = m_CartData.FurnituresInCarts.Select(x => x.FirstItemID).ToArray();

            MsmpClient client = MsmpClient.Instance;

            InMarketShoppingCartPurchase marketShoppingCartPurchase = new InMarketShoppingCartPurchase()
            {
                FurnituresIds = furnituresIds,
                ProductsIds = productsIds,
            };

            Packet packet = new Packet(PacketType.PurchaseEvent, marketShoppingCartPurchase);

            client.SendPayload(packet);
        }
    }
}
