using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class MarkTaskAsCompleted : BaseMessageReceiver
    {
        public MarkTaskAsCompleted(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation,
            IStateManager stateManager, Application application, Settings settings)
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                if (await MarkTaskAsCompletedAsync(message.From, message.Content, cancellationToken))
                {
                    await InformTheTaskWasCompletedAsync(message.From, cancellationToken);
                    StateManager.SetState(message.From, Settings.States.Default);
                }
                else
                {
                    await InformTheTaskWasNotFoundAsync(message.From, cancellationToken);
                }
            }
            catch (Exception)
            {
                await InformAnOptionShallBeChosenAsync(message.From, cancellationToken);
            }
        }

        private async Task<bool> MarkTaskAsCompletedAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var externalTaskId = RoutineTask.ExtractTaskIdFromCompleteCommand(((PlainText)content)?.Text);
            long taskId;
            long.TryParse(externalTaskId, out taskId);
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return false;

            task.LastTime = DateTimeOffset.Now;
            await SetRoutineAsync(routine, cancellationToken);

            return true;
        }

        private async Task InformTheTaskWasCompletedAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.KeepGoing, owner, cancellationToken);
        }
    }
}