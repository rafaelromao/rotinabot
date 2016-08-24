using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Serialization;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Listener;

namespace RotinaBot
{
    public class Startup : IStartable
    {
        private readonly IMessagingHubSender _sender;
        private readonly Settings _settings;

        public Startup(IMessagingHubSender sender, Settings settings)
        {
            _sender = sender;
            _settings = settings;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TypeUtil.RegisterDocument<Routine>();

            return Task.CompletedTask;
        }
    }
}
