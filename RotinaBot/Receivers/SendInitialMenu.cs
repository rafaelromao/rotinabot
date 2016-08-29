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
            await Bot.SendInitialMenuAsync(message.From, cancellationToken);
        }
    }
}