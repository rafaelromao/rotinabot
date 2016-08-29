using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class SendInitialMenu : BaseMessageReceiver
    {
        public SendInitialMenu(RotinaBot bot) : base(bot) { }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await Bot.IsPhoneNumberRegisteredAsync(message.From, cancellationToken))
            {
                await Bot.SendInitialMenuAsync(message.From, cancellationToken);
            }
            else
            {
                await Bot.OfferPhoneNumberRegistrationAsync(message.From, cancellationToken);
                Bot.StateManager.SetState(message.From, Bot.Settings.States.WaitingPhoneNumber);
            }
        }
    }
}
