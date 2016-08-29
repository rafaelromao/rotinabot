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
            var owner = ((IdentityDocument) message.Content).Value.ToNode();
            if (await Bot.SendNextTasksAsync(owner, message.Content, cancellationToken))
            {
                Bot.StateManager.SetState(owner, Bot.Settings.States.WaitingTaskSelection);
            }
        }
    }
}