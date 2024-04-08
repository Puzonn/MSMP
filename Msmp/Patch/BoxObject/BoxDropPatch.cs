using HarmonyLib;
using Msmp.Client;
using Msmp.Server;
using Msmp.Server.Packets;
using MSMP.Mono;
using System;
using System.Reflection;
using UnityEngine;

namespace MSMP.Patch.BoxObject
{
    [HarmonyPatch(typeof(PlayerObjectHolder))]
    [HarmonyPatch("DropObject")]
    internal class BoxDropPatch
    {
        [HarmonyPrefix]
        static void Prefix(PlayerObjectHolder __instance)
        {
            GameObject currentObject = (GameObject)__instance.GetType()
             .GetField("m_CurrentObject", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);

            Guid boxNetworkId = currentObject.GetComponent<NetworkedBox>().NetworkId;

            OutBoxDropPacket outBoxDropPacket = new OutBoxDropPacket()
            {
                x = currentObject.transform.position.x,
                y = currentObject.transform.position.y,
                z = currentObject.transform.position.z,
                BoxNetworkId = boxNetworkId
            };

            Packet packet = new Packet(PacketType.BoxDropEvent, outBoxDropPacket);
            MsmpClient.Instance.SendPayload(packet);
        }
    }
}
