using HarmonyLib;
using Msmp.Client;
using Msmp.Mono;
using Msmp.Server;
using Msmp.Server.Models;
using Msmp.Server.Packets.Customers;
using MyBox;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using System.Linq;
using UnityEngine;
using Msmp.Utility;
using Msmp.Utility.Models;

namespace Msmp.Patch.CustomerPatch
{
    [HarmonyPatch(typeof(Customer))]
    [HarmonyPatch("StartShopping")]
    internal class CustomerStartShoppingPatch
    {
        [HarmonyPrefix]
        static bool Prefix(Customer __instance)
        {
            if (!MsmpClient.Instance.IsServer)
            {
                return false;
            }

            ItemQuantity shoppingList = Singleton<CustomerManager>.Instance.CreateShoppingList();

            RandomDisplay randomDisplay = DisplayUtility.GetRandomDisplay();    

            OutCustomerStartShopping outCustomerStartShopping = new OutCustomerStartShopping()
            {
                ProcessedProducts = CreateSyncProcessProduct(__instance,shoppingList),
                ShoppingList = shoppingList,
                NetworkId = __instance.GetComponent<NetworkedCustomer>().NetworkId,
                WalkRandomDisplay = randomDisplay.DisplayId,
                WalkRandomDisplaySlot = randomDisplay.DisplaySlotId
            };

            Packet packet = new Packet(PacketType.CustomerStartShopping, outCustomerStartShopping);

            MsmpClient.Instance.SendPayload(packet);
            
            return false;
        }


        private static List<ProcessedProduct> CreateSyncProcessProduct(Customer customer, ItemQuantity shoppingList)
        {
            List<ProcessedProduct> processedProducts = new List<ProcessedProduct>();

            foreach (int productId in shoppingList.Products.Keys.ToList())
            {
                List<DisplaySlot> displaySlots = Singleton<DisplayManager>.Instance.GetDisplaySlots(productId, true);

                if (displaySlots == null || displaySlots.Count == 0)
                {
                    Console.WriteLine("Product is not displayed");
                    continue;
                }

                int randomDisplayIndex = Random.Range(0, displaySlots.Count);

                Vector2 m_ExtraPurchaseAmount = customer.GetType().GetPrivateField<Vector2>("m_ExtraPurchaseAmount", customer);
                int purchaseChance = (int)Math.Floor(Singleton<PriceEvaluationManager>.Instance.PurchaseChance(productId));

                processedProducts.Add(new ProcessedProduct()
                {
                    DisplaySlotId = randomDisplayIndex,
                    IsProductDisplayed = true,
                    PurchaseChance = purchaseChance,
                    ExpensiveRandom = Random.Range(0, 100),
                    RandomMultiplier = (int)Math.Floor(Random.Range(m_ExtraPurchaseAmount.x, m_ExtraPurchaseAmount.y) * purchaseChance),
                    ProductCount = shoppingList.Products[productId],
                    ProductId = productId,
                });
            }

            return processedProducts;
        }
    }
}
