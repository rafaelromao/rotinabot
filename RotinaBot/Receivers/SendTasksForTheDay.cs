using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using RotinaBot.Domain;
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
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
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
            var isSaturday = DateTime.Today.DayOfWeek == DayOfWeek.Saturday;
            var isSunday = DateTime.Today.DayOfWeek == DayOfWeek.Saturday;

            RoutineTask[] tasks;
            if (isSaturday)
            {
                tasks = SortRoutineTasks(routine.Tasks.Where(
                    t => t.IsActive && 
                         t.LastTime.Date != DateTime.Today &&
                         t.Days.Value != RoutineTaskDaysValue.WorkDays
                    ));
            }
            else if (isSunday)
            {
                tasks = SortRoutineTasks(routine.Tasks.Where(
                    t => t.IsActive && 
                         t.LastTime.Date != DateTime.Today &&
                         t.LastTime.Date != DateTime.Today.AddDays(-1) &&
                         t.Days.Value != RoutineTaskDaysValue.WorkDays
                    ));
            }
            else
            {
                tasks = SortRoutineTasks(routine.Tasks.Where(
                    t => t.IsActive && 
                         t.LastTime.Date != DateTime.Today &&
                         t.Days.Value != RoutineTaskDaysValue.WeekEnds
                    ));
            }

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