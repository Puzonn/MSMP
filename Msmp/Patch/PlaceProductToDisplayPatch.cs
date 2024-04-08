using HarmonyLib;
using Msmp.Client;
using Msmp.Server;
using MSMP.Mono;
using MSMP.Server.Packets;
using MyBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MSMP.Patch
{
    [HarmonyPatch(typeof(BoxInteraction))]
    [HarmonyPatch("PlaceProductToDisplay")]
    internal class PlaceProductToDisplayPatch
    {
        private static DisplaySlot _currentDisplaySlot;

        [HarmonyPostfix]
        static void PostFix(BoxInteraction __instance)
        {
            DisplaySlot currentDisplaySlot = (DisplaySlot)__instance.GetType()
            .GetField("m_CurrentDisplaySlot", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            DisplaySlot[] slots = (DisplaySlot[])currentDisplaySlot.Display.GetType().GetField("m_DisplaySlots", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(currentDisplaySlot.Display);

            int index = slots.FirstIndex(x => x == currentDisplaySlot);

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

            int displayIndex = -1;

            foreach (var a in Singleton<DisplayManager>.Instance.DisplayedProducts)
            {
                displayIndex = a.Value.FindIndex(x => x == currentDisplaySlot);
            }

            OutProductToDisplayPacket outProductToDisplayPacket = new OutProductToDisplayPacket()
            {
                DisplayId = currentDisplaySlot.Display.ID,
                DisplaySlotId = index,
                BoxNetworkId = currentBox.GetComponent<NetworkedBox>().BoxNetworkId,
                ProductId = currentBox.Product.ID
            };

            Console.WriteLine($"DisplayId: {outProductToDisplayPacket.DisplayId}  SlotId: {outProductToDisplayPacket.DisplaySlotId}" +
                $"  NetworkId: {outProductToDisplayPacket.BoxNetworkId}  ProductId: {outProductToDisplayPacket.ProductId}");

            Packet packet = new Packet(PacketType.ProductToDisplayEvent, outProductToDisplayPacket);

            MsmpClient.Instance.SendPayload(packet);
        }
    }
}
