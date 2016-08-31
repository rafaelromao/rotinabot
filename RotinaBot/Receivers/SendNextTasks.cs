using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public class SendNextTasks : BaseMessageReceiver
    {
        public SendNextTasks(RotinaBot bot) : base(bot)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var owner = (message.Content as IdentityDocument)?.Value?.ToNode() ?? message.From;
            if (await Bot.SendNextTasksAsync(owner, cancellationToken))
            {
                Bot.StateManager.SetState(owner, Bot.Settings.States.WaitingTaskSelection);
            }
        }
    }
}