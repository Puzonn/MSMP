using HarmonyLib;
using System;

namespace Msmp.Patch.CheckoutPatch
{
    [HarmonyPatch(typeof(Checkout))]
    [HarmonyPatch("CashierCompletedCheckout")]
    internal class CheckoutCashierCompletedCheckoutPatch
    {
        [HarmonyPrefix]
        static void Prefix()
        {
            Console.WriteLine("Shop done");
        }
    }
}
