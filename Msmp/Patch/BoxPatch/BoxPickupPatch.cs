using HarmonyLib;
using Msmp.Mono;
using UnityEngine;

/*
 TODO: Before picking up check if other client is not picking the same box on server
*/
namespace Msmp.Patch.BoxPatch
{
    [HarmonyPatch(typeof(PlayerObjectHolder))]
    [HarmonyPatch("HoldObject")]
    internal class BoxPickupPatch
    {
        [HarmonyPostfix]
        static void Postfix(PlayerObjectHolder __instance, GameObject item)
        {
            item.GetComponent<NetworkedBox>()
                .SetPickedUp(true, true);
        }
    }
}
