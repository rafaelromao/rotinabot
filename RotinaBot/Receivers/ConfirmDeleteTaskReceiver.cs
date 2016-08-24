using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class ConfirmDeleteTaskReceiver : BaseMessageReceiver
    {
        public ConfirmDeleteTaskReceiver(IMessagingHubSender sender, IBucketExtension bucket, Settings settings) : base(sender, bucket, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(message.From, cancellationToken);
            routine.Tasks = routine.Tasks.Take(routine.Tasks.Length - 1).ToArray();
            await SetRoutineAsync(message.From, routine, cancellationToken);

            await Sender.SendMessageAsync(Settings.Phraseology.TheTaskWasRemoved, message.From, cancellationToken);
        }
    }
}