using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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
    public class SendTasksForTheWeek : BaseMessageReceiver
    {
        public SendTasksForTheWeek(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation,
            IStateManager stateManager, Application application, Settings settings)
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (!await SendTasksForTheWeekAsync(message.From, cancellationToken))
            {
                await InformThereIsNoTaskRegisteredAsync(message.From, cancellationToken);
            }
        }

        private async Task<bool> SendTasksForTheWeekAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var tasks = SortRoutineTasks(routine.Tasks.Where(
                t => t.IsActive
            ));

            if (!tasks.Any())
                return false;

            var text = new StringBuilder();
            text.AppendLine(Settings.Phraseology.HereAreYourTasksForTheWeek);

            PrintTasksForDaysAndTime(text, RoutineTaskDaysValue.WorkDays, RoutineTaskTimeValue.Morning, tasks);
            PrintTasksForDaysAndTime(text, RoutineTaskDaysValue.WorkDays, RoutineTaskTimeValue.Afternoon, tasks);
            PrintTasksForDaysAndTime(text, RoutineTaskDaysValue.WorkDays, RoutineTaskTimeValue.Evening, tasks);

            PrintTasksForDaysAndTime(text, RoutineTaskDaysValue.WeekEnds, RoutineTaskTimeValue.Morning, tasks);
            PrintTasksForDaysAndTime(text, RoutineTaskDaysValue.WeekEnds, RoutineTaskTimeValue.Afternoon, tasks);
            PrintTasksForDaysAndTime(text, RoutineTaskDaysValue.WeekEnds, RoutineTaskTimeValue.Evening, tasks);

            await Sender.SendMessageAsync(text.ToString(), owner, cancellationToken);

            return true;
        }

        private void PrintTasksForDaysAndTime(StringBuilder text, RoutineTaskDaysValue days, RoutineTaskTimeValue time,
            IEnumerable<RoutineTask> tasks)
        {
            tasks = tasks.Where(
                task => (task.Days.GetValueOrDefault() == days || task.Days.GetValueOrDefault() == RoutineTaskDaysValue.EveryDay) &&
                         task.Time.GetValueOrDefault() == time
            ).ToArray();
            if (!tasks.Any())
                return;

            text.AppendLine();
            text.AppendLine($"{days.Name()} {Settings.Phraseology.During} {time.Name().ToLower()}:");
            tasks.ForEach(task => text.AppendLine($"- {task.Name}"));
        }
    }
}