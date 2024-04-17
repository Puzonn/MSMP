using HarmonyLib;
using Msmp.Client;
using Msmp.Mono;
using Msmp.Server;
using Msmp.Server.Packets.Customers;
using MyBox;

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

            OutCustomerStartShopping outCustomerStartShopping = new OutCustomerStartShopping()
            {
                ShoppingList = shoppingList,
                NetworkId = __instance.GetComponent<NetworkedCustomer>().NetworkId  
            };

            Packet packet = new Packet(PacketType.CustomerStartShopping, outCustomerStartShopping);

            MsmpClient.Instance.SendPayload(packet);
            
            return false;
        }
    }
}
