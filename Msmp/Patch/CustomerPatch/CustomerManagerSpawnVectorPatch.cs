using HarmonyLib;
using System;
using UnityEngine;

namespace Msmp.Patch.CustomerPatch
{
    [HarmonyPatch(typeof(CustomerManager))]
    [HarmonyPatch("SpawnCustomer", new Type[] { typeof(Vector3)})]
    internal class CustomermanagerSpawnVectorPatch
    {
        [HarmonyPrefix]
        static bool Prefix(CustomerManager __instance, Vector3 position )
        {
            Console.WriteLine($"[Client] [{nameof(CustomerManagerSpawnPatch)}] Canceling spawning customer with vector data");

            return false;
        }
    }
}
