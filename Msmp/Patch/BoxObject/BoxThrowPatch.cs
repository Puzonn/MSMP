using HarmonyLib;
using Msmp.Mono;
using System.Reflection;
using UnityEngine;

namespace Msmp.Patch.BoxObject
{
    [HarmonyPatch(typeof(PlayerObjectHolder))]
    [HarmonyPatch("ThrowObject")]
    internal class BoxThrowPatch
    {
        [HarmonyPrefix]
        static void Prefix(PlayerObjectHolder __instance)
        {
            GameObject currentObject = (GameObject)__instance.GetType()
             .GetField("m_CurrentObject", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if (currentObject == null)
            {
                return;
            }

            NetworkedBox networkedBox = currentObject.GetComponent<NetworkedBox>();

            if (networkedBox == null)
            {
                return;
            }

            networkedBox.SetPickedUp(false, false);
        }
    }
}
