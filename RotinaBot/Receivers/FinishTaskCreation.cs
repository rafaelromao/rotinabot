using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Documents;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class FinishTaskCreation : BaseMessageReceiver
    {
        public FinishTaskCreation(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await FinishTaskCreationAsync(message.From, cancellationToken);
            await InformTheTaskWasCreatedAsync(message.From, cancellationToken);
        }

        private async Task FinishTaskCreationAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.Last();
            task.IsActive = true;
            await SetRoutineAsync(routine, cancellationToken);

            ConfigureSchedule(routine.Owner, cancellationToken);
        }

        private async Task InformTheTaskWasCreatedAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.TheTaskWasRegistered, owner, cancellationToken);
        }

    }
}