using HarmonyLib;
using Msmp.Client;
using Msmp.Mono;
using Msmp.Server;
using MSMP.Server.Packets.Customers;
using MyBox;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Remoting.Messaging;

namespace MSMP.Patch.CustomerPatch
{
    [HarmonyPatch(typeof(Customer))]
    [HarmonyPatch("TakeProduct")]
    internal class CustomerTakeProductPatch
    {
        [HarmonyPrefix]
        static void Prefix(Customer __instance, DisplaySlot displaySlot, int productID)
        {
            NetworkedCustomer networkedCustomer = __instance.GetComponent<NetworkedCustomer>();

            if (networkedCustomer == null)
            {
                Console.WriteLine($"[Client] Customer dose not have any {nameof(NetworkedCustomer)}");

                return;
            }

            DisplaySlot[] slots = (DisplaySlot[])displaySlot.Display.GetType().GetField("m_DisplaySlots", BindingFlags.NonPublic | BindingFlags.Instance)
               .GetValue(displaySlot.Display);

            if (slots == null)
            {
                return;
            }

            List<Display> displays = (List<Display>)(Singleton<DisplayManager>.Instance.GetType()
                    .GetField("m_Displays", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(Singleton<DisplayManager>.Instance));

            int displaySlotId = slots.FirstIndex(x => x == displaySlot);
            int displayId = displays.FindIndex(x => x == displaySlot.Display);

            OutCustomerTakeProductPacket outCustomerTakeProductPacket = new OutCustomerTakeProductPacket()
            {
                DisplayId = displayId,
                DisplaySlotId = displaySlotId,
                ProductId = productID
            };

            Packet packet = new Packet(PacketType.CustomerTakeProduct, outCustomerTakeProductPacket);

            MsmpClient.Instance.SendPayload(packet);
        }
    }
}
