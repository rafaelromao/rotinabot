using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class SetTimeForNewTask : BaseMessageReceiver
    {
        public SetTimeForNewTask(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation, 
            IStateManager stateManager, Application application, Settings settings) 
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                var task = await SetTimeForNewTaskAsync(message.From, message.Content, cancellationToken);
                if (task != null)
                {
                    await SendTaskConfirmationRequestAsync(message.From, task, cancellationToken);
                    StateManager.SetState(message.From, Settings.States.WaitingTaskConfirmation);
                }
            }
            catch (Exception)
            {
                await InformAnOptionShallBeChosenAsync(message.From, cancellationToken);
            }
        }

        private async Task<RoutineTask> SetTimeForNewTaskAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var timeText = ((PlainText)content)?.Text;
            var timeValue = (RoutineTaskTimeValue)Enum.Parse(typeof(RoutineTaskDaysValue), timeText);
            var taskTime = new RoutineTaskTime { Value = timeValue };

            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.Last();
            task.Time = taskTime;
            await SetRoutineAsync(routine, cancellationToken);

            return task;
        }

        private async Task SendTaskConfirmationRequestAsync(Node owner, RoutineTask task, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = $"{task.Name} " +
                       $"{Settings.Phraseology.During} " +
                       $"{task.Time.GetValueOrDefault().Name().ToLower()} " +
                       $"{task.Days.GetValueOrDefault().Name().ToLower()}!",
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
                            Value = new PlainText {Text = Settings.Phraseology.Cancel},
                            Order = 2
                        }
                    }
            };
            await Sender.SendMessageAsync(select, owner, cancellationToken);
        }
    }
}