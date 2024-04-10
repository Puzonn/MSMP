using HarmonyLib;
using MyBox;
using System;
using UnityEngine;
using System.Reflection;
using System.Collections.Generic;
using Lean.Pool;
using Random = UnityEngine.Random;
using Msmp.Server.Packets;
using Msmp.Server;
using Msmp.Client;

namespace Msmp.Patch.Customers
{
    [HarmonyPatch(typeof(CustomerManager))]
    [HarmonyPatch("SpawnCustomer", new Type[] {})]
    internal class SpawnCustomerPatch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            if(MsmpClient.Instance == null || !MsmpClient.Instance.IsServer)
            {
                return false;
            }

            return false;

            Console.WriteLine($"[Client] [{nameof(SpawnCustomerPatch)}] Spawning customer without vector data");

            CustomerGenerator customerGenerator = Singleton<CustomerGenerator>.Instance;    

            List<Customer> m_CustomerPrefabs = (List<Customer>)customerGenerator.GetType()
                .GetField("m_CustomerPrefabs", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(customerGenerator);

            int spawnTransform = Random.Range(0, customerGenerator.SpawningTransforms.Length);
            int prefab = Random.Range(0, m_CustomerPrefabs.Count);

            OutSpawnCustomer outSpawnCustomer = new OutSpawnCustomer()
            {
                PrefabIndex = prefab,
                SpawnTransformIndex = spawnTransform,
            };

            Packet packet = new Packet(PacketType.SpawnCustomer, outSpawnCustomer);

            MsmpClient.Instance.SendPayload(packet);

            return false;
        }

        public static Customer SpawnCustomer(int prefabIndex, int transformIndex, Vector3 position = default)
        {
            Console.WriteLine($"[Client] [{PacketType.SpawnCustomer}] Spawning customer p: {prefabIndex} t: {transformIndex}");

            CustomerGenerator customerGenerator = Singleton<CustomerGenerator>.Instance;
            CustomerManager customerManager = Singleton<CustomerManager>.Instance;

            List<Customer> m_CustomerPrefabs = (List<Customer>)customerGenerator.GetType()
                .GetField("m_CustomerPrefabs", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(customerGenerator);

            List<Customer> m_ActiveCustomers = (List<Customer>)customerManager.GetType()
                 .GetField("m_ActiveCustomers", BindingFlags.NonPublic | BindingFlags.Instance)
                 .GetValue(customerManager);

            Transform m_StoreDoor = (Transform)customerManager.GetType()
                .GetField("m_StoreDoor", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(customerManager);

            Vector3 spawnPosition;
            Quaternion spawnRotation;

            if(position == default)
            {
                spawnRotation = customerGenerator.SpawningTransforms[transformIndex].rotation;
                spawnPosition = customerGenerator.SpawningTransforms[transformIndex].position;
            }
            else
            {
                spawnRotation = Quaternion.identity;
                spawnPosition = position;
            }

            Customer customer =  LeanPool.Spawn(m_CustomerPrefabs[prefabIndex], spawnPosition, spawnRotation, customerGenerator.transform);

            customer.GoToStore(m_StoreDoor.position);

            m_ActiveCustomers.Add(customer);

            return customer;
        }
    }
}
