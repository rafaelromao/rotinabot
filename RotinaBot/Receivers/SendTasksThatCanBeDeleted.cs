using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public class SendTasksThatCanBeDeleted : BaseMessageReceiver
    {
        public SendTasksThatCanBeDeleted(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await Bot.SendTasksThatCanBeDeletedAsync(message.From, cancellationToken))
            {
                Bot.StateManager.SetState(message.From, Bot.Settings.States.WaitingDeleteTaskSelection);
            }
            else
            {
                await Bot.InformThereIsNoTaskRegisteredAsync(message.From, cancellationToken);
                Bot.StateManager.SetState(message.From, Bot.Settings.States.Default);
            }
        }
    }
}