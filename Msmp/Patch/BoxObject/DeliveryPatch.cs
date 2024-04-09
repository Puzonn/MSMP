using HarmonyLib;

namespace Msmp.Patch.BoxObject
{
    [HarmonyPatch(typeof(DeliveryManager))]
    [HarmonyPatch("Delivery")]
    internal class DeliveryPatch
    {
        /* Do not create any boxes by this function. We need to create boxes and assign guid to them by server */
        [HarmonyPrefix]
        static bool Prefix()
        {
            return false;
        }
    }
}
