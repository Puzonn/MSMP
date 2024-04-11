using Msmp.Server.Models.Sync;
using System.Collections.Generic;
using BepInEx.Logging;
using Msmp.Client.SynchronizationContainers;

namespace Msmp.Client
{
    internal class MsmpSynchronizationContext
    {
        private readonly MsmpClient _client;
        private readonly ManualLogSource _logger;

        public readonly NpcTrafficSyncContainer NpcTrafficContainer;
        public readonly CustomerSyncContainer CustomerContainer;

        public MsmpSynchronizationContext(ManualLogSource logger, MsmpClient client) 
        {
            _client = client;
            _logger = logger;

            NpcTrafficContainer = new NpcTrafficSyncContainer(logger);
            CustomerContainer = new CustomerSyncContainer(logger);
        }

        public List<SyncTrafficNPCModel> GetSyncTrafficNPCs()
        {
            return NpcTrafficContainer.GetModels();
        }

        public List<SyncCustomerModel> GetSyncCustomers()
        {
            return CustomerContainer.Get(); 
        }
    }
}
