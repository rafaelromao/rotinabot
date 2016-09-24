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
    public class MarkTaskAsCompleted : BaseMessageReceiver
    {
        public MarkTaskAsCompleted(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                if (await MarkTaskAsCompletedAsync(message.From, message.Content, cancellationToken))
                {
                    await InformTheTaskWasCompletedAsync(message.From, cancellationToken);
                    StateManager.SetState(message.From, Settings.States.Default);
                }
                else
                {
                    await InformTheTaskWasNotFoundAsync(message.From, cancellationToken);
                }
            }
            catch (Exception)
            {
                await InformAnOptionShallBeChosenAsync(message.From, cancellationToken);
            }
        }

        private async Task<bool> MarkTaskAsCompletedAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var externalTaskId = RoutineTask.ExtractTaskIdFromCompleteCommand(((PlainText)content)?.Text);
            long taskId;
            long.TryParse(externalTaskId, out taskId);
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return false;

            task.LastTime = DateTimeOffset.Now;
            await SetRoutineAsync(routine, cancellationToken);

            return true;
        }

        private async Task InformTheTaskWasCompletedAsync(Node owner, CancellationToken cancellationToken)
        {
            if (!await SendNextTasksAsync(owner, false, Settings.Phraseology.Congratulations, cancellationToken))
            {
                await InformThereIsNoPendingTaskForTheMomentAsync(owner, cancellationToken);
            }
        }

        private async Task InformThereIsNoPendingTaskForTheMomentAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.CongratulationsNoOtherPendingTask, owner, cancellationToken);
        }
    }
}