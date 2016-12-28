using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol.Serialization;
using RotinaBot.Documents;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client.Listener;

namespace RotinaBot
{
    public class Startup : IStartable
    {
        private readonly IMessagingHubSender _sender;
        private readonly Settings _settings;
        private readonly ReschedulerTask _reschedulerTask;

        public Startup(IMessagingHubSender sender, Settings settings, ReschedulerTask reschedulerTask)
        {
            _sender = sender;
            _settings = settings;
            _reschedulerTask = reschedulerTask;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Trace.Listeners.Add(new ConsoleTraceListener());

            TypeUtil.RegisterDocument<Routine>();
            TypeUtil.RegisterDocument<PhoneNumber>();

            _reschedulerTask.Start();

            return Task.CompletedTask;
        }
    }
}
