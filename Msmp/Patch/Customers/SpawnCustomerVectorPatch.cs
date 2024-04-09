using HarmonyLib;
using System;
using UnityEngine;

namespace Msmp.Patch.Customers
{
    [HarmonyPatch(typeof(CustomerManager))]
    [HarmonyPatch("SpawnCustomer", new Type[] { typeof(Vector3)})]
    internal class SpawnCustomerVectorPatch
    {
        [HarmonyPrefix]
        static bool Prefix(CustomerManager __instance, Vector3 position )
        {
            Console.WriteLine($"[Client] [{nameof(SpawnCustomerPatch)}] Canceling spawning customer with vector data");

            return false;
        }
    }
}
