using BepInEx.Logging;
using MyBox;

namespace Msmp.Client.Controllers
{
    internal class CheckoutController
    {
        private CheckoutManager _checkoutManager
        {
            get
            {
                return Singleton<CheckoutManager>.Instance;
            } 
        }

        private readonly ManualLogSource _logger;

        private bool _checkoutInitalized = false;

        public CheckoutController(ManualLogSource logger)
        {
            _logger = logger;
        }

        public void OnUpdate()
        {
            if(_checkoutManager == null)
            {
                return;
            }

            if (!_checkoutInitalized)
            {
                _checkoutManager.onCheckoutCompleted += () =>
                {
                    _logger.LogInfo("Checkout ..");
                };

                _checkoutInitalized = true; 
            }
        }
    }
}
