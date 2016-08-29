using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class RequestTaskName : BaseMessageReceiver
    {
        public RequestTaskName(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await Bot.RequestTaskNameAsync(message.From, cancellationToken);
        }
    }
}