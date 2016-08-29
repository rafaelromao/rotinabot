using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public class SendTasksForTheDay : BaseMessageReceiver
    {
        public SendTasksForTheDay(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await Bot.SendTasksForTheDayAsync(message.From, cancellationToken))
            {
                Bot.StateManager.SetState(message.From, Bot.Settings.States.WaitingTaskSelection);
            }
            else
            {
                await Bot.InformThereIsNoTaskForTodayAsync(message.From, cancellationToken);
                Bot.StateManager.SetState(message.From, Bot.Settings.States.Default);
            }
        }
    }
}