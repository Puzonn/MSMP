using HarmonyLib;
using Msmp.Client;
using Msmp.Server;
using Msmp.Server.Packets.CheckoutPackets;
using Msmp.Utility;

namespace Msmp.Patch.CheckoutPatch
{
    [HarmonyPatch(typeof(Checkout))]
    [HarmonyPatch("TryFinishingCardPayment")]
    internal class CheckoutTryFinishingCardPayment
    {
        private static bool _allow = false;

        [HarmonyPrefix  ]
        static bool Prefix(float posTotal, Checkout __instance)
        {
            if(_allow)
            {
                return true;
            }

            OutCheckoutCardPayment outCheckoutCardPayment = new OutCheckoutCardPayment()
            {
                Total = posTotal,
                CheckoutId = CheckoutUtility.GetCheckoutId(__instance)
            };
            
            Packet packet = new Packet(PacketType.CheckoutTryFinishingCardPayment, outCheckoutCardPayment);

            MsmpClient.Instance.SendPayload(packet);

            _allow = false;

            return false;
        }

        public static void SyncTryFinishingCardPayment(float total, int checkoutId)
        {
            Checkout checkout = CheckoutUtility.GetCheckouts()[checkoutId];

            checkout.TryFinishingCardPayment(total);

            _allow = true;
        }
    }
}
