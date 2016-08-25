using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public class WaitingTaskTimeReceiver : BaseMessageReceiver
    {
        public WaitingTaskTimeReceiver(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                var task = await Bot.SetTimeForNewTaskAsync(message.From, message.Content, cancellationToken);
                if (task != null)
                {
                    await Bot.SendTaskConfirmationRequestAsync(message.From, task, cancellationToken);
                    StateManager.Instance.SetState(message.From, Bot.Settings.States.WaitingTaskConfirmation);
                }
            }
            catch (Exception)
            {
                await Bot.InformAnOptionShallBeChosenAsync(message.From, cancellationToken);
            }
        }
    }
}