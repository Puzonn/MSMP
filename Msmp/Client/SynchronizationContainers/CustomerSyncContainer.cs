using BepInEx.Logging;
using Msmp.Server.Models.Sync;
using System;
using System.Collections.Generic;
using Msmp.Server.Models;

namespace Msmp.Client.SynchronizationContainers
{
    internal class CustomerSyncContainer
    {
        public readonly List<SyncCustomer> SyncCustomers = new List<SyncCustomer>();
        private readonly ManualLogSource _logger;

        public CustomerSyncContainer(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void Add(SyncCustomer npc)
        {
            SyncCustomers.Add(npc);
        }

        public bool Remove(Guid networkId)
        {
            SyncCustomer syncNpc = SyncCustomers.Find(x => x.NetworkId == networkId);

            if (syncNpc == null)
            {
                _logger.LogWarning($"[Client] [{nameof(CustomerSyncContainer)}] Tried remove customer {networkId} that doesnt exist");

                return false;
            }

            return SyncCustomers.Remove(syncNpc);
        }

        public List<SyncCustomerModel> GetModels()
        {
            List<SyncCustomerModel> customers = new List<SyncCustomerModel>();

            foreach (var customer in SyncCustomers)
            {
                customers.Add(new SyncCustomerModel()
                {
                    NetworkId = customer.NetworkId,
                    Position = new SerializableVector3(customer.CustomerReference.transform.position),
                    PrefabIndex = customer.Prefab,
                    SpawnTransformIndex = 0,
                    WithVector = true
                });
            }

            return customers;
        }

        public class SyncCustomer
        {
            public Customer CustomerReference;
            public Guid NetworkId { get; set; }

            public int Prefab { get; set; }
        }
    }
}
