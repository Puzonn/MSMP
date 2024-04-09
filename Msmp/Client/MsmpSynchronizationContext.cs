using Msmp.Client;
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

        public MsmpSynchronizationContext(ManualLogSource logger, MsmpClient client) 
        {
            _client = client;
            _logger = logger;

            NpcTrafficContainer = new NpcTrafficSyncContainer(logger);
        }

        public List<SyncTrafficNPCModel> GetSyncTrafficNPCs()
        {
            return NpcTrafficContainer.Get();
        }
    }
}
