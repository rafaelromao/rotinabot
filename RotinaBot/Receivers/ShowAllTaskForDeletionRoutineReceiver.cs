using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public class ShowAllTaskForDeletionRoutineReceiver : BaseMessageReceiver
    {
        public ShowAllTaskForDeletionRoutineReceiver(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await Bot.SendTasksThatCanBeDeletedAsync(message.From, cancellationToken))
            {
                StateManager.Instance.SetState(message.From, Bot.Settings.States.WaitingDeleteTaskSelection);
            }
            else
            {
                await Bot.InformThereIsNoTaskRegisteredAsync(message.From, cancellationToken);
                StateManager.Instance.SetState(message.From, Bot.Settings.States.Default);
            }
        }
    }
}