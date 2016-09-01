using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class CancelCurrentOperation : BaseMessageReceiver
    {
        public CancelCurrentOperation(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation,
            IStateManager stateManager, Application application, Settings settings)
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await CancelCurrentOperationAsync(message.From, cancellationToken);
            await SendAtYourServiceMessageAsync(message.From, cancellationToken);
        }

        private async Task CancelCurrentOperationAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            if (!routine.Tasks.Last().IsActive)
            {
                routine.Tasks = routine.Tasks.Take(routine.Tasks.Length - 1).ToArray();
            }
            await SetRoutineAsync(routine, cancellationToken);
        }
    }
}