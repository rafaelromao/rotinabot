using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.Iris.Messaging.Resources;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Tests.AcceptanceTests.Mocks
{
    internal class NoSchedulerExtension : ISchedulerExtension
    {
        public Task<Schedule> GetScheduledMessageAsync(string messageId, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public Task ScheduleMessageAsync(Message message, DateTimeOffset when,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.CompletedTask;
        }
    }

    internal class FakeSchedulerExtension : ISchedulerExtension
    {
        private readonly IMessagingHubSender _sender;

        public FakeSchedulerExtension(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public Task<Schedule> GetScheduledMessageAsync(string messageId,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            throw new NotImplementedException();
        }

        public async Task ScheduleMessageAsync(Message message, DateTimeOffset when,
            CancellationToken cancellationToken = new CancellationToken())
        {
            await _sender.SendMessageAsync(message, cancellationToken);
        }
    }
}