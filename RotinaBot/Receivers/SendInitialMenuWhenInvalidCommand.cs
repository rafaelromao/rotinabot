
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class SendInitialMenuWhenInvalidCommand : BaseMessageReceiver
    {
        public SendInitialMenuWhenInvalidCommand(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await Bot.SendInitialMenuAsync(message.From, cancellationToken);
        }
    }
}