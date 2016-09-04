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
    public class PrepareTaskToBeDeleted : BaseMessageReceiver
    {
        public PrepareTaskToBeDeleted(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                var task = await PrepareTaskToBeDeletedAsync(message.From, message.Content, cancellationToken);
                if (task != null)
                {
                    await SendDeleteConfirmationRequestAsync(message.From, task, cancellationToken);
                    StateManager.SetState(message.From, Settings.States.WaitingDeleteTaskConfirmation);
                }
                else
                {
                    await InformTheTaskWasNotFoundAsync(message.From, cancellationToken);
                    StateManager.SetState(message.From, Settings.States.Default);
                }
            }
            catch (Exception)
            {
                await InformAnOptionShallBeChosenAsync(message.From, cancellationToken);
            }
        }

        private async Task<RoutineTask> PrepareTaskToBeDeletedAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var externalTaskId = RoutineTask.ExtractTaskIdFromDeleteCommand(((PlainText)content)?.Text);
            long taskId;
            long.TryParse(externalTaskId, out taskId);
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return null;

            // Put the task to be deleted at the end of the array
            routine.Tasks = routine.Tasks.Where(t => t != task).Concat(new[] { task }).ToArray();

            task.LastTime = DateTimeOffset.Now;
            await SetRoutineAsync(routine, cancellationToken);

            return task;
        }

        private async Task SendDeleteConfirmationRequestAsync(Node owner, RoutineTask task, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = $"{Settings.Phraseology.ConfirmDelete} '" +
                       $"{task.Name} " +
                       $"{Settings.Phraseology.During} " +
                       $"{task.Time.GetValueOrDefault().Name().ToLower()} " +
                       $"{task.Days.GetValueOrDefault().Name().ToLower()}'?",
                Options = new[]
                {
                            new SelectOption
                            {
                                Text = Settings.Phraseology.Confirm,
                                Value = new PlainText {Text = Settings.Commands.Confirm},
                                Order = 1
                            },
                            new SelectOption
                            {
                                Text = Settings.Phraseology.Cancel,
                                Value = new PlainText {Text = Settings.Commands.Cancel},
                                Order = 2
                            }
                        }
            };
            await Sender.SendMessageAsync(select, owner, cancellationToken);
        }
    }
}