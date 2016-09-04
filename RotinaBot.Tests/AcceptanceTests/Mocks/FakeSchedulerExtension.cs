using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Tests.AcceptanceTests.Mocks
{
    internal class NoSchedulerExtension : ISchedulerExtension
    {
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

        private bool isFirstCall = true;

        public async Task ScheduleMessageAsync(Message message, DateTimeOffset when,
            CancellationToken cancellationToken = new CancellationToken())
        {
            if (isFirstCall)
            {
                await _sender.SendMessageAsync(message, cancellationToken);
                isFirstCall = false;
            }
        }
    }
}