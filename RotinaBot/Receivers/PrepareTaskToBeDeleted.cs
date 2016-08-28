using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public class PrepareTaskToBeDeleted : BaseMessageReceiver
    {
        public PrepareTaskToBeDeleted(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                var task = await Bot.PrepareTaskToBeDeletedAsync(message.From, message.Content, cancellationToken);
                if (task != null)
                {
                    await Bot.SendDeleteConfirmationRequestAsync(message.From, task, cancellationToken);
                    StateManager.Instance.SetState(message.From, Bot.Settings.States.WaitingDeleteTaskConfirmation);
                }
                else
                {
                    await Bot.InformTheTaskWasNotFoundAsync(message.From, cancellationToken);
                    StateManager.Instance.SetState(message.From, Bot.Settings.States.Default);
                }
            }
            catch (Exception)
            {
                await Bot.InformAnOptionShallBeChosenAsync(message.From, cancellationToken);
            }
        }
    }
}