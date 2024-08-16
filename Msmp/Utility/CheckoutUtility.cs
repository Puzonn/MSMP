using MyBox;
using System;
using System.Collections.Generic;

namespace Msmp.Utility
{
    internal static class CheckoutUtility
    {
        public static int GetCheckoutId(Checkout checkout)
        {
            return Array.IndexOf(GetCheckouts().ToArray(), checkout); 
        }

        public static List<Checkout> GetCheckouts()
        {
            CheckoutManager manager = Singleton<CheckoutManager>.Instance;

            return manager.GetType().GetPrivateField<List<Checkout>>("m_Checkouts", manager);
        }
    }
}
