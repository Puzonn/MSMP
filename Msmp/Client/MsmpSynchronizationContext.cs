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
        public readonly SyncDisplayContainer DisplayContainer;
        public readonly SyncBoxConstainer BoxContainer;

        public MsmpSynchronizationContext(ManualLogSource logger, MsmpClient client) 
        {
            _client = client;
            _logger = logger;

            NpcTrafficContainer = new NpcTrafficSyncContainer(logger);
            CustomerContainer = new CustomerSyncContainer(logger);
            DisplayContainer = new SyncDisplayContainer(logger);
            BoxContainer = new SyncBoxConstainer(logger);
        }

        public List<SyncTrafficNPCModel> GetSyncTrafficNPCs()
        {
            return NpcTrafficContainer.GetModels();
        }

        public List<SyncCustomerModel> GetSyncCustomers()
        {
            return CustomerContainer.GetModels(); 
        }

        public List<SyncDisplayContainer.DisplaySyncModel> GetSyncDisplays()
        {
            return DisplayContainer.GetModels();
        }

        public List<SyncBoxModel> GetSyncBoxes()
        {
            return BoxContainer.GetModels();
        }
    }
}
