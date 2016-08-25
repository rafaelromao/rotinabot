using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class ConfirmDeleteTaskReceiver : BaseMessageReceiver
    {
        public ConfirmDeleteTaskReceiver(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await Bot.FinishTaskDeletionAsync(message.From, cancellationToken);
            await Bot.InformTheTaskWasRemovedAsync(message.From, cancellationToken);
        }
    }
}