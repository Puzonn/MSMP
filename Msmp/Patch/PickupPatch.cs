using HarmonyLib;
using MSMP.Mono;
using UnityEngine;

/*
 TODO: Before picking up check if other client is not picking the same box on server
*/
namespace Msmp.Patch
{
    [HarmonyPatch(typeof(PlayerObjectHolder))]
    [HarmonyPatch("HoldObject")]
    internal class PickupPatch
    {
        [HarmonyPostfix]
        static void Postfix(GameObject item)
        {
            item.GetComponent<NetworkedBox>()
                .SetPickedUp(true);
        }
    }
}
