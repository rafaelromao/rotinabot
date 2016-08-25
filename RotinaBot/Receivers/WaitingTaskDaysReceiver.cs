using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public class WaitingTaskDaysReceiver : BaseMessageReceiver
    {
        public WaitingTaskDaysReceiver(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                await Bot.SetDaysForNewTaskAsync(message.From, message.Content, cancellationToken);
                await Bot.SendTaskTimeRequestAsync(message.From, cancellationToken);
                StateManager.Instance.SetState(message.From, Bot.Settings.States.WaitingTaskTime);
            }
            catch (Exception e)
            {
                await Bot.InformAnOptionShallBeChosenAsync(message.From, cancellationToken);
            }
        }
    }
}