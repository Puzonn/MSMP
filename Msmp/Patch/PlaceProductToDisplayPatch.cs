using HarmonyLib;
using Msmp.Client;
using Msmp.Server;
using MSMP.Mono;
using MSMP.Server.Packets;
using MyBox;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace MSMP.Patch
{
    [HarmonyPatch(typeof(BoxInteraction))]
    [HarmonyPatch("PlaceProductToDisplay")]
    internal class PlaceProductToDisplayPatch
    {
        [HarmonyPostfix]
        static void PostFix(BoxInteraction __instance)
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

            int index = slots.FirstIndex(x => x == currentDisplaySlot);

            if (currentDisplaySlot == null)
            {
                return;
            }

            Box currentBox = (Box)__instance.GetType()
               .GetField("m_Box", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            if (currentBox == null || !currentBox.HasProducts || currentBox.Product == null)
            {
                return;
            }

            int displayIndex = displays.FindIndex(x => x == currentDisplaySlot.Display);    

            OutProductToDisplayPacket outProductToDisplayPacket = new OutProductToDisplayPacket()
            {
                DisplayId = displayIndex,
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
