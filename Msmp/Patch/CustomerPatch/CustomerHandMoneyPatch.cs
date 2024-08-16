using HarmonyLib;

namespace Msmp.Patch.CustomerPatch
{
    [HarmonyPatch(typeof(Customer))]
    [HarmonyPatch("HandMoney")]
    internal class CustomerHandMoneyPatch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            return true;
        }
    }
}
