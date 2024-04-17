using HarmonyLib;
using Msmp.Client;
using Msmp.Server;
using Msmp.Mono;
using Msmp.Server.Packets;
using MyBox;
using System;
using System.Collections.Generic;
using System.Reflection;
using Msmp.Client.SynchronizationContainers;
using System.Linq;
using Lean.Pool;

namespace Msmp.Patch.Shop
{
    [HarmonyPatch(typeof(BoxInteraction))]
    [HarmonyPatch("PlaceProductToDisplay")]
    internal class PlaceProductToDisplayPatch
    {
        [HarmonyPrefix]
        static void Prefix(BoxInteraction __instance)
        {
            DisplaySlot currentDisplaySlot = (DisplaySlot)__instance.GetType()
            .GetField("m_CurrentDisplaySlot", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if(currentDisplaySlot == null || currentDisplaySlot.Display == null)
            {
                return;
            }

            DisplaySlot[] slots = (DisplaySlot[])currentDisplaySlot.Display.GetType().GetField("m_DisplaySlots", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(currentDisplaySlot.Display);

            if(slots == null)
            {
                return;
            }

            List<Display> displays = (List<Display>)(Singleton<DisplayManager>.Instance.GetType()
                                        .GetField("m_Displays", BindingFlags.NonPublic | BindingFlags.Instance)
                                        .GetValue(Singleton<DisplayManager>.Instance));

            int displaySlotId = slots.FirstIndex(x => x == currentDisplaySlot);

            if (currentDisplaySlot == null)
            {
                return;
            }

            Box currentBox = (Box)__instance.GetType()
               .GetField("m_Box", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if (currentBox == null)
            {
                return;
            }

            int displayId = displays.FindIndex(x => x == currentDisplaySlot.Display);    

            OutProductToDisplayPacket outProductToDisplayPacket = new OutProductToDisplayPacket()
            {
                DisplayId = displayId,
                DisplaySlotId = displaySlotId,
                BoxNetworkId = currentBox.GetComponent<NetworkedBox>().NetworkId,
                ProductId = currentBox.Product.ID
            };

            Console.WriteLine($"DisplayId: {outProductToDisplayPacket.DisplayId}  SlotId: {outProductToDisplayPacket.DisplaySlotId}" +
                $"  NetworkId: {outProductToDisplayPacket.BoxNetworkId}  ProductId: {outProductToDisplayPacket.ProductId}");

            Packet packet = new Packet(PacketType.ProductToDisplayEvent, outProductToDisplayPacket);

            MsmpClient.Instance.SendPayload(packet);

            if (MsmpClient.Instance.IsServer)
            {
                MsmpClient.Instance.SyncContext.DisplayContainer.AddProduct(displayId, displaySlotId, currentBox.Product.ID);
            }

            return;
        }

        public static void AddProductToDisplay(List<SyncDisplayContainer.DisplaySyncModel> packet)
        {
            List<Display> displays = (List<Display>)Singleton<DisplayManager>.Instance.GetType()
                .GetField("m_Displays", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(Singleton<DisplayManager>.Instance);

            List<SyncDisplayContainer.DisplaySyncModel> syncDisplays =
                packet.FindAll(x => x.Slots.Any(y => y.ProductId != 0));

            foreach (var syncDisplay in syncDisplays)
            {
                Display display = displays[syncDisplay.DisplayId];

                DisplaySlot[] displaySlots = (DisplaySlot[])display.GetType()
                    .GetField("m_DisplaySlots", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(display);

                foreach (var syncSlot in syncDisplay.Slots)
                {
                    DisplaySlot slot = displaySlots[syncSlot.DisplaySlotId];

                    List<Product> m_Products = (List<Product>)slot.GetType().GetField("m_Products", BindingFlags.NonPublic | BindingFlags.Instance)
                        .GetValue(slot);

                    foreach(var product in m_Products.ToArray())
                    {
                        LeanPool.Despawn(product);
                    }

                    slot.Data = new ItemQuantity();

                    if (syncSlot.ProductId == 0 || syncSlot.ProductCount == 0)
                    {
                        continue;
                    }

                    for(int i = 0; i<syncSlot.ProductCount; i++)
                    {
                        displaySlots[syncSlot.DisplaySlotId].AddProduct(syncSlot.ProductId,
                            LeanPool.Spawn(Singleton<IDManager>.Instance.ProductSO(syncSlot.ProductId).ProductPrefab, display.transform));
                    }
                }
            }
        }

        public static void AddProductToDisplay(OutProductToDisplayPacket packet)
        {
            Console.WriteLine("add");
            ClientManager clientManager = MsmpClient.Instance._clientManager;
            Box box = clientManager.GetBox(packet.BoxNetworkId);

            Product product = box.GetProductFromBox();

            List<Display> displays = (List<Display>)Singleton<DisplayManager>.Instance.GetType()
                .GetField("m_Displays", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(Singleton<DisplayManager>.Instance);

            if (displays == null)
            {
                return;
            }

            DisplaySlot[] slots = (DisplaySlot[])displays[packet.DisplayId].GetType()
             .GetField("m_DisplaySlots", BindingFlags.NonPublic | BindingFlags.Instance)
             .GetValue(displays[packet.DisplayId]);

            if (slots == null)
            {
                return;
            }

            slots[packet.DisplaySlotId].AddProduct(packet.ProductId, product);
        }
    }
}
