using Msmp.Client;

namespace MSMP.Client
{
    internal class MsmpNPCTrafficManager
    {
        private readonly MsmpClient _client;

        public MsmpNPCTrafficManager(MsmpClient client)
        {
            _client = client;
        }
    }
}
