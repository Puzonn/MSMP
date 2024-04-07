using HarmonyLib;
using MSMP.Mono;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Msmp.Patch
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
            Console.WriteLine("not gol");
            networkedBox.SetPickedUp(false, false);
        }
    }
}
