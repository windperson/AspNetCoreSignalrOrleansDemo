using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans;
using OrleansInterface;

namespace HelloGrain
{
    public class MessageServerGrain : Grain, IMsgServer
    {
        private readonly ILogger<MessageServerGrain> _logger;
        private ObserverSubscriptionManager<IMsgClient> _subManager;

        public MessageServerGrain(ILogger<MessageServerGrain> logger)
        {
            _logger = logger;
        }

        public override Task OnActivateAsync()
        {
            _subManager = new ObserverSubscriptionManager<IMsgClient>();
            return base.OnActivateAsync();
        }

        public Task Subscribe(IMsgClient msgClient)
        {
            if (!_subManager.IsSubscribed(msgClient))
            {
                _subManager.Subscribe(msgClient);
                _logger.LogInformation("client connected");
            }

            return Task.CompletedTask;
        }

        public Task UnSubscribe(IMsgClient msgClient)
        {
            if (_subManager.IsSubscribed(msgClient))
            {
                _subManager.Unsubscribe(msgClient);
                _logger.LogInformation("client disconnected");
            }

            return Task.CompletedTask;
        }

        public Task BroadcastMessage(string greetings)
        {
            _logger.LogInformation("Server receive : {@1}", greetings);
            var broadcastMsg = $"Broadcast: Hello {greetings}!";
            _subManager.Notify(s => s.ReceiveMessage(broadcastMsg));
            return Task.CompletedTask;
        }
    }
}