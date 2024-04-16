using Msmp.Mono;
using Msmp.Server.Models.Sync;
using System;
using System.Collections.Generic;
using Msmp.Server.Models;
using BepInEx.Logging;

namespace Msmp.Client.SynchronizationContainers
{
    internal class SyncBoxConstainer
    {
        public readonly List<SyncBox> SyncBoxes = new List<SyncBox>();

        private readonly ManualLogSource _logger;

        public SyncBoxConstainer(ManualLogSource logger)
        {
            _logger = logger;   
        }

        public void AddBox(SyncBox box)
        {
            _logger.LogInfo("Box added");
            SyncBoxes.Add(box); 
        }

        public void Remove(Guid networkId)
        {
            SyncBox box = SyncBoxes.Find(x => x.BoxReference.NetworkId == networkId); 

            if(box == null)
            {
                return;
            } 
            
            SyncBoxes.Remove(box);
        }

        public void SetSpawned(Guid networkId)
        {
            SyncBox box = SyncBoxes.Find(x => x.BoxReference.NetworkId == networkId);

            if (box == null)
            {
                return;
            }

            box.BoxReference.Spawned = true;
        }

        public List<SyncBoxModel> GetModels()
        {
            List<SyncBoxModel> models = new List<SyncBoxModel>();

            foreach (SyncBox syncBox in SyncBoxes)
            {
                Box box = syncBox.BoxReference.gameObject.GetComponent<Box>();

                int productCount = box.Data.ProductCount;
                int productId = box.Data.ProductID;
                bool isOpen = box.IsOpen;

                models.Add(new SyncBoxModel()
                {
                    NetworkId = syncBox.BoxReference.NetworkId,
                    OwnerNetworkId = syncBox.BoxReference.OwnerNetworkId,
                    Position = new SerializableVector3(syncBox.BoxReference.gameObject.transform.position),
                    ProductCount = productCount,
                    ProductId = productId,
                    IsOpen = isOpen
                });
            }

            return models;
        }

        public class SyncBox
        {
            public NetworkedBox BoxReference { get; set; }
        }
    }
}
