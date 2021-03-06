using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class OfferPhoneNumberRegistration : BaseMessageReceiver
    {
        public OfferPhoneNumberRegistration(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await OfferPhoneNumberRegistrationAsync(message.From, cancellationToken);
            StateManager.SetState(message.From, Settings.States.WaitingPhoneNumber);
        }
    }
}
