using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public class ScheduleReminderReceiver : BaseMessageReceiver
    {
        public ScheduleReminderReceiver(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await Bot.SendNextTasksAsync(message.From, message.Content, cancellationToken))
            {
                StateManager.Instance.SetState(message.From, Bot.Settings.States.WaitingTaskSelection);
            }
        }
    }
}