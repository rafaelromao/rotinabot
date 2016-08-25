using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Tests.AcceptanceTests.Mocks
{
    internal class FakeSchedulerExtension : ISchedulerExtension
    {
        private readonly Application _application;
        private readonly IMessagingHubSender _sender;

        public FakeSchedulerExtension(Application application, IMessagingHubSender sender)
        {
            _application = application;
            _sender = sender;
        }

        public async Task ScheduleMessageAsync(Message message, DateTimeOffset when,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var identity = new Node(_application.Identifier, _application.Domain, null);
            var schedule = new Message
            {
                Id = Guid.NewGuid().ToString(),
                To = identity,
                Content = new IdentityDocument(message.From.ToIdentity().ToString())
            };

            await _sender.SendMessageAsync(schedule, cancellationToken);
        }
    }
}