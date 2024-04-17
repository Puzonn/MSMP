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
using Msmp.Client.SynchronizationContainers;
using Msmp.Mono;
using Msmp.Server.Models.Sync;

namespace Msmp.Patch.CustomerPatch  
{
    [HarmonyPatch(typeof(CustomerManager))]
    [HarmonyPatch("SpawnCustomer", new Type[] {})]
    internal class CustomerManagerSpawnPatch
    {
        [HarmonyPrefix]
        static bool Prefix()
        {
            if(MsmpClient.Instance == null || !MsmpClient.Instance.IsServer)
            {
                return false;
            }

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
                NetworkId = Guid.NewGuid()
            };

            Packet packet = new Packet(PacketType.SpawnCustomer, outSpawnCustomer);

            MsmpClient.Instance.SendPayload(packet);

            return false;
        }

        public static Customer SpawnCustomer(Guid networkId, int prefabIndex, int transformIndex, Vector3 position = default)
        {
            Console.WriteLine($"[Client] [{PacketType.SpawnCustomer}] Spawning customer prefab: {prefabIndex} transform: {transformIndex} id: {networkId}");

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

            Customer customer = LeanPool.Spawn(m_CustomerPrefabs[prefabIndex], spawnPosition, spawnRotation, customerGenerator.transform);

            customer.gameObject.AddComponent<NetworkedCustomer>().NetworkId = networkId;

            customer.GoToStore(m_StoreDoor.position);

            m_ActiveCustomers.Add(customer);

            MsmpClient client = MsmpClient.Instance;

            client.SyncContext.CustomerContainer.Add(new CustomerSyncContainer.SyncCustomer()
            {
                CustomerReference = customer,
                NetworkId = networkId,
                Prefab = prefabIndex
            });

            return customer;
        }

        public static void SyncCustomers(List<SyncCustomerModel> customers)
        {
            foreach(SyncCustomerModel customer in customers)
            {
                SpawnCustomer(customer.NetworkId, customer.PrefabIndex, customer.SpawnTransformIndex, customer.Position.ToVector3());
            }
        }
    }
}
