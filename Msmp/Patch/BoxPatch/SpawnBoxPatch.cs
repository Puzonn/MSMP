using Msmp.Mono;
using Msmp.Server.Models.Sync;
using MyBox;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace Msmp.Patch.BoxPatch
{
    internal class SpawnBoxPatch
    {
        public static void SpawnBox(List<SyncBoxModel> packet)
        {
            foreach (var syncBox in packet)
            {

                int productId = syncBox.ProductId;
                int productCount = syncBox.ProductCount;

                Guid networkId = syncBox.NetworkId;
                Vector3 position = syncBox.Position.ToVector3();

                Box box = Singleton<BoxGenerator>.Instance.SpawnBox(Singleton<IDManager>.Instance.ProductSO(productId),
                    position, Quaternion.identity);

                box.Setup(productId, true);

                NetworkedBox networkedBox = box.gameObject.AddComponent<NetworkedBox>();
                networkedBox.NetworkId = networkId;

                box.Data.ProductCount = productCount;

                if (syncBox.Spawned)
                {
                    box.GetType().GetMethod("SpawnProducts", BindingFlags.NonPublic | BindingFlags.Instance)
                        .Invoke(box, new object[] { false });
                    box.Data.IsOpen = true;
                }

                if (syncBox.IsOpen)
                {
                    box.OpenBox();
                }

                if (syncBox.OwnerNetworkId != default)
                {
                    networkedBox.SetPickedUp(syncBox.NetworkId);
                }
            }
        }
    }
}
