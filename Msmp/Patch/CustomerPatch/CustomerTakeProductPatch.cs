using HarmonyLib;
using Msmp.Mono;
using MyBox;
using System;
using System.Collections.Generic;
using System.Reflection;

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

            Console.WriteLine($"[Client] ds: {displaySlotId} d: {displayId}");
        }
    }
}
