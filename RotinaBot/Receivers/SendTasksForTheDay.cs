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
    public class SendTasksForTheDay : BaseMessageReceiver
    {
        public SendTasksForTheDay(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation,
            IStateManager stateManager, Application application, Settings settings)
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await SendTasksForTheDayAsync(message.From, cancellationToken))
            {
                StateManager.SetState(message.From, Settings.States.WaitingTaskSelection);
            }
            else
            {
                await InformThereIsNoTaskForTodayAsync(message.From, cancellationToken);
                StateManager.SetState(message.From, Settings.States.Default);
            }
        }

        private async Task<bool> SendTasksForTheDayAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var isWorkDay = DateTime.Today.DayOfWeek != DayOfWeek.Saturday &&
                            DateTime.Today.DayOfWeek != DayOfWeek.Sunday;
            var tasks = SortRoutineTasks(routine.Tasks.Where(
                t => t.IsActive && t.LastTime.Date != DateTime.Today &&
                     ((t.Days.Value == RoutineTaskDaysValue.EveryDay) ||
                      (t.Days.Value == RoutineTaskDaysValue.WorkDays && isWorkDay) ||
                      (t.Days.Value == RoutineTaskDaysValue.WeekEnds && !isWorkDay))
            ));

            if (!tasks.Any())
                return false;

            var select = new Select
            {
                Text = Settings.Phraseology.HereAreYouTasksForToday
            };
            select.Options = select.Options = BuildTaskSelectionOptions(tasks, RoutineTask.CreateCompleteCommand);
            await Sender.SendMessageAsync(select, owner, cancellationToken);

            return true;
        }

        public async Task InformThereIsNoTaskForTodayAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.NoTaskForToday, owner, cancellationToken);
        }
    }
}