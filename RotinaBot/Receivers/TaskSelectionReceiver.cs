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
                await Bot.MarkTaskAsCompletedAsync(message.From, message.Content, cancellationToken);
                StateManager.Instance.SetState(message.From, Bot.Settings.States.Default);
            }
            catch (Exception)
            {
                await Bot.InformAnOptionShallBeChosenAsync(message.From, cancellationToken);
            }
        }
    }
}