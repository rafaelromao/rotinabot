using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class FinishTaskDeletion : BaseMessageReceiver
    {
        public FinishTaskDeletion(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await FinishTaskDeletionAsync(message.From, cancellationToken);
            await InformTheTaskWasRemovedAsync(message.From, cancellationToken);
        }

        private async Task FinishTaskDeletionAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            routine.Tasks = routine.Tasks.Take(routine.Tasks.Length - 1).ToArray();
            await SetRoutineAsync(routine, cancellationToken);
        }

        public async Task InformTheTaskWasRemovedAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.TheTaskWasRemoved, owner, cancellationToken);
        }
    }
}