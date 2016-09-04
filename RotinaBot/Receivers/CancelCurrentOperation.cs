using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class CancelCurrentOperation : BaseMessageReceiver
    {
        public CancelCurrentOperation(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
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