using HarmonyLib;
using System;
using UnityEngine;

namespace MSMP.Patch.Customer
{
    [HarmonyPatch(typeof(CustomerManager))]
    [HarmonyPatch("SpawnCustomer", new Type[] {})]
    internal class CustomerManagerPatch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            Console.WriteLine("Spawning customer"); 
            return false;
        }
    }
}
