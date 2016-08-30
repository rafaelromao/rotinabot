
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class SavePhoneNumber : BaseMessageReceiver
    {
        public SavePhoneNumber(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await Bot.SavePhoneNumberAsync(message.From, message.Content, cancellationToken))
            {

                Bot.StateManager.SetState(message.From, Bot.Settings.States.WaitingSMSCode);

                await Bot.SendPhoneNumberAuthenticationCodeAsync(message.From, cancellationToken);
            }
            else
            {
                await Bot.InformPhoneNumberIsWrongAsync(message.From, cancellationToken);
            }
        }
    }
}