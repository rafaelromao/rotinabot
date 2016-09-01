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
    public class SendNextTasks : BaseMessageReceiver
    {
        public SendNextTasks(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation,
            IStateManager stateManager, Application application, Settings settings)
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var owner = (message.Content as IdentityDocument)?.Value?.ToNode() ?? message.From;
            if (await SendNextTasksAsync(owner, cancellationToken))
            {
                StateManager.SetState(owner, Settings.States.WaitingTaskSelection);
            }
        }

        public async Task<bool> SendNextTasksAsync(Node owner, CancellationToken cancellationToken)
        {
            var time = DateTime.Now.Hour >= 18
                ? RoutineTaskTimeValue.Evening
                : DateTime.Now.Hour >= 12
                  ? RoutineTaskTimeValue.Afternoon
                  : RoutineTaskTimeValue.Morning;

            var routine = await GetRoutineAsync(owner, false, cancellationToken);

            var isWorkDay = DateTime.Today.DayOfWeek != DayOfWeek.Saturday &&
                            DateTime.Today.DayOfWeek != DayOfWeek.Sunday;

            var tasks = SortRoutineTasks(routine.Tasks.Where(
                t => t.IsActive && t.LastTime.Date != DateTime.Today &&
                     ((t.Days.GetValueOrDefault() == RoutineTaskDaysValue.EveryDay) ||
                      (t.Days.GetValueOrDefault() == RoutineTaskDaysValue.WorkDays && isWorkDay) ||
                      (t.Days.GetValueOrDefault() == RoutineTaskDaysValue.WeekEnds && !isWorkDay))
                )
                .Where(t => t.Time.GetValueOrDefault() == time));

            await ConfigureScheduleAsync(routine, time, cancellationToken);

            await SetRoutineAsync(routine, cancellationToken);

            if (!tasks.Any())
                return false;

            var select = new Select
            {
                Text = Settings.Phraseology.HereAreYourNextTasks
            };
            select.Options = BuildTaskSelectionOptions(tasks, RoutineTask.CreateCompleteCommand);
            await Sender.SendMessageAsync(select, owner, cancellationToken);

            return true;
        }
    }
}