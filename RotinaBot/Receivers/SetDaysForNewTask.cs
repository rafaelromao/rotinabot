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
    public class SetDaysForNewTask : BaseMessageReceiver
    {
        public SetDaysForNewTask(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation, 
            IStateManager stateManager, Application application, Settings settings) 
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                await SetDaysForNewTaskAsync(message.From, message.Content, cancellationToken);
                await SendTaskTimeRequestAsync(message.From, cancellationToken);
                StateManager.SetState(message.From, Settings.States.WaitingTaskTime);
            }
            catch (Exception e)
            {
                await InformAnOptionShallBeChosenAsync(message.From, cancellationToken);
            }
        }

        private async Task SetDaysForNewTaskAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var daysText = ((PlainText)content)?.Text;
            var daysValue = (RoutineTaskDaysValue)Enum.Parse(typeof(RoutineTaskDaysValue), daysText);
            var taskDays = new RoutineTaskDays { Value = daysValue };

            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.Last();
            task.Days = taskDays;
            await SetRoutineAsync(routine, cancellationToken);
        }

        private async Task SendTaskTimeRequestAsync(Node owner, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = Settings.Phraseology.WhichTimeShallThisTaskBePerformed,
                Options = new[]
                {
                        new SelectOption
                        {
                            Text = RoutineTaskTimeValue.Morning.Name(),
                            Order = (int)RoutineTaskTimeValue.Morning
                        },
                        new SelectOption
                        {
                            Text = RoutineTaskTimeValue.Afternoon.Name(),
                            Order = (int)RoutineTaskTimeValue.Afternoon
                        },
                        new SelectOption
                        {
                            Text = RoutineTaskTimeValue.Evening.Name(),
                            Order = (int)RoutineTaskTimeValue.Evening
                        },
                        new SelectOption
                        {
                            Text = Settings.Phraseology.Cancel,
                            Value = new PlainText { Text = Settings.Commands.Cancel },
                            Order = 4
                        }
                    }
            };
            await Sender.SendMessageAsync(select, owner, cancellationToken);
        }
    }
}