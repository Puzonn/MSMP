using BepInEx.Logging;
using Msmp.Server;
using MyBox;
using System;

namespace Msmp.Client.Controllers
{
    /* TODO: Create this with observe pattern */
    internal class MoneyController
    {
        public static MoneyController Instance { get; private set; }

        private readonly MsmpClient _client;
        private readonly ManualLogSource _logger;

        private MoneyManager _moneyManager
        {
            get
            {
                return Singleton<MoneyManager>.Instance;    
            }
        }

        private float LastMoney = 0f;

        private bool _ready
        {
            get
            {
                return _moneyManager != null;
            }
        }

        public MoneyController(ManualLogSource logger, MsmpClient client) 
        {
            _logger = logger;
            _client = client;

            Instance = this;
        }

        public void OnUpdate()
        {
            /* TODO: Check if needed */
            if (!_client.Connected || ! _client.IsServer || !_ready)
            {
                return;
            }

            float currentMoney = _moneyManager.Money;

            if (LastMoney != currentMoney) 
            {
                byte[] data = new byte[4];
                Buffer.BlockCopy(BitConverter.GetBytes(currentMoney), 0, data, 0, 4);

                Packet packet = new Packet(PacketType.MoneyChanged, data);
                _client.SendPayload(packet);

                LastMoney = currentMoney;

                _logger.LogInfo($"[Client] [{nameof(MoneyController)}] Syncing money");
            }
        }

        public void MoneyChanged(float value)
        {
            UnityDispatcher.UnitySyncContext.Post(_ =>
            {
                _moneyManager.Money = value;
                _moneyManager.onMoneyTransition?.Invoke(value, MoneyManager.TransitionType.NONE);
            }, null);
        }
    }
}
