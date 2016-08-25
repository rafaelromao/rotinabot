using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public class WaitingTaskNameReceiver : BaseMessageReceiver
    {
        public WaitingTaskNameReceiver(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                await Bot.SetNameForNewTaskAsync(message.From, message.Content, cancellationToken);
                await Bot.SendTaskDaysRequestAsync(message.From, cancellationToken);
                StateManager.Instance.SetState(message.From, Bot.Settings.States.WaitingTaskDays);
            }
            catch (Exception)
            {
                await Bot.InformAProblemHasOcurredAsync(message.From, cancellationToken);
            }
        }
    }
}