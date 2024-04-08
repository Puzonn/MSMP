using HarmonyLib;
using Msmp.Client;
using Msmp.Server;
using MSMP.Mono;
using MSMP.Server.Packets;

namespace MSMP.Patch.BoxObject
{
    [HarmonyPatch(typeof(Box))]
    [HarmonyPatch("OpenBox")]
    internal class OpenBoxPatch
    {
        [HarmonyPrefix]
        static void Prefix(Box __instance)
        {
            NetworkedBox networkedBox = __instance.GetComponent<NetworkedBox>();

            if(__instance.IsOpen || networkedBox.NetworkId == MsmpClient.Instance.LocalClientNetworkId)
            {
                return;
            }

            OutOpenBoxPacket outOpenBoxPacket = new OutOpenBoxPacket()
            {
                BoxNetworkId = networkedBox.NetworkId,
                State = true,
            };

            Packet packet = new Packet(PacketType.OpenBoxEvent, outOpenBoxPacket);

            MsmpClient.Instance.SendPayload(packet);
        }
    }
}
