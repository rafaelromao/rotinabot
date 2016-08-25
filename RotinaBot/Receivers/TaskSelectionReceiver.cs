using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public class TaskSelectionReceiver : BaseMessageReceiver
    {
        public TaskSelectionReceiver(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                if (await Bot.MarkTaskAsCompletedAsync(message.From, message.Content, cancellationToken))
                {
                    await Bot.InformTheTaskWasCompletedAsync(message.From, cancellationToken);
                    StateManager.Instance.SetState(message.From, Bot.Settings.States.Default);
                }
                else
                {
                    await Bot.InformTheTaskWasNotFoundAsync(message.From, cancellationToken);
                }
            }
            catch (Exception)
            {
                await Bot.InformAnOptionShallBeChosenAsync(message.From, cancellationToken);
            }
        }
    }
}