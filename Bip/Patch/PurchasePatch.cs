using Msmp.Client;
using Msmp.Server;
using Msmp.Server.Packets;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

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

            MarketShoppingCartPurchase marketShoppingCartPurchase = new MarketShoppingCartPurchase()
            {
                FurnituresIds = furnituresIds,
                ProductIds = productsIds,
            };

            Packet packet = new Packet(PacketType.PurchaseEvent, marketShoppingCartPurchase);

            client.SendPayload(packet);
        }
    }
}
