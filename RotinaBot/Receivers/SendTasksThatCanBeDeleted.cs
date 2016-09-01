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
    public class SendTasksThatCanBeDeleted : BaseMessageReceiver
    {
        public SendTasksThatCanBeDeleted(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation, 
            IStateManager stateManager, Application application, Settings settings) 
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await SendTasksThatCanBeDeletedAsync(message.From, cancellationToken))
            {
                StateManager.SetState(message.From, Settings.States.WaitingDeleteTaskSelection);
            }
            else
            {
                await InformThereIsNoTaskRegisteredAsync(message.From, cancellationToken);
                StateManager.SetState(message.From, Settings.States.Default);
            }
        }

        private async Task<bool> SendTasksThatCanBeDeletedAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var tasks = SortRoutineTasks(routine.Tasks);

            if (!tasks.Any())
            {
                return false;
            }

            var select = new Select
            {
                Text = Settings.Phraseology.ChooseATaskToBeDeleted
            };
            select.Options = select.Options = BuildTaskSelectionOptions(tasks, RoutineTask.CreateDeleteCommand);
            await Sender.SendMessageAsync(select, owner, cancellationToken);

            return true;
        }
    }
}