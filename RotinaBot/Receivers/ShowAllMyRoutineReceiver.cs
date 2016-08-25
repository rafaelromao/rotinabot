using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;

namespace RotinaBot.Receivers
{
    public class ShowAllMyRoutineReceiver : BaseMessageReceiver
    {
        public ShowAllMyRoutineReceiver(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (!await Bot.SendTasksForTheWeekAsync(message.From, cancellationToken))
            {
                await Bot.InformThereIsNoTaskRegisteredAsync(message.From, cancellationToken);
            }
        }
    }
}