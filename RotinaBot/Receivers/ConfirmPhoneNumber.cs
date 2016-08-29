
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class ConfirmPhoneNumber : BaseMessageReceiver
    {
        public ConfirmPhoneNumber(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await Bot.ConfirmPhoneNumberAsync(message.From, message.Content, cancellationToken))
            {
                await Bot.InformPhoneNumberRegistrationSucceededAsync(message.From, cancellationToken);
                Bot.StateManager.SetState(message.From, Bot.Settings.States.Default);
            }
            else
            {
                await Bot.InformPhoneNumberRegistrationFailedAsync(message.From, cancellationToken);
                Bot.StateManager.SetState(message.From, Bot.Settings.States.Default);
            }
        }
    }
}