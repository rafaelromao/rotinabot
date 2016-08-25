using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class ConfirmNewTaskReceiver : BaseMessageReceiver
    {
        public ConfirmNewTaskReceiver(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await Bot.FinishTaskCreationAsync(message.From, cancellationToken);
            await Bot.InformTheTaskWasCreatedAsync(message.From, cancellationToken);
        }
    }
}