using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class OfferPhoneNumberRegistration : BaseMessageReceiver
    {
        public OfferPhoneNumberRegistration(RotinaBot bot) : base(bot) { }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await Bot.OfferPhoneNumberRegistrationAsync(message.From, cancellationToken);
            Bot.StateManager.SetState(message.From, Bot.Settings.States.WaitingPhoneNumber);
        }
    }
}
