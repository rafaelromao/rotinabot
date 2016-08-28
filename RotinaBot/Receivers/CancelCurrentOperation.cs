using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class CancelCurrentOperation : BaseMessageReceiver
    {
        public CancelCurrentOperation(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await Bot.CancelCurrentOperationAsync(message.From, cancellationToken);
            await Bot.SendAtYourServiceMessageAsync(message.From, cancellationToken);
        }
    }
}