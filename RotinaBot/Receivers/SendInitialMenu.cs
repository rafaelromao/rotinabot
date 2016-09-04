using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class SendInitialMenu : BaseMessageReceiver
    {
        public SendInitialMenu(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await IsPhoneNumberRegisteredAsync(message.From, cancellationToken))
            {
                await SendInitialMenuAsync(message.From, cancellationToken);
                StateManager.SetState(message.From, Settings.States.WaitingInitialMenuOption);
            }
            else
            {
                await OfferPhoneNumberRegistrationAsync(message.From, cancellationToken);
                StateManager.SetState(message.From, Settings.States.WaitingPhoneNumber);
            }
        }

        private async Task<bool> IsPhoneNumberRegisteredAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            return routine.PhoneNumberRegistrationStatus != PhoneNumberRegistrationStatus.Pending;
        }
    }
}
