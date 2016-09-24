using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class SendNextTasks : BaseMessageReceiver
    {
        public SendNextTasks(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var owner = (message.Content as IdentityDocument)?.Value?.ToNode();
            var isScheduledRequest = true;
            if (owner == null)
            {
                owner = message.From;
                isScheduledRequest = false;
            }
            if (await SendNextTasksAsync(owner, isScheduledRequest, Settings.Phraseology.Hi, cancellationToken))
            {
                StateManager.SetState(owner, Settings.States.WaitingTaskSelection);
            }
            else if (!isScheduledRequest)
            {
                await InformThereIsNoPendingTasksAsync(owner, cancellationToken);
            }
        }

        public async Task InformThereIsNoPendingTasksAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.CongratulationsNoOtherPendingTask, owner, cancellationToken);
        }
    }
}