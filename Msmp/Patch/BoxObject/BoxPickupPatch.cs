using System;
using HarmonyLib;
using MSMP.Mono;
using MyBox;
using UnityEngine;

/*
 TODO: Before picking up check if other client is not picking the same box on server
*/
namespace MSMP.Patch.BoxObject
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
