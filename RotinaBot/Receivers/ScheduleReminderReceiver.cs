using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class ScheduleReminderReceiver : BaseMessageReceiver
    {
        private readonly Scheduler _scheduler;

        public ScheduleReminderReceiver(IMessagingHubSender sender, IBucketExtension bucket, Scheduler scheduler, Settings settings) : base(sender, bucket, settings)
        {
            _scheduler = scheduler;
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var identity = message.Content as IdentityDocument;
            if (identity != null)
            {
                var routine = await GetRoutineAsync(Node.Parse(identity.ToString()), cancellationToken);
                var isWorkDay = DateTime.Today.DayOfWeek != DayOfWeek.Saturday &&
                                DateTime.Today.DayOfWeek != DayOfWeek.Sunday;
                var todaysTasks = SortRoutineTasks(routine.Tasks.Where(
                    t => t.IsActive && t.LastTime.Date != DateTime.Today &&
                         ((t.Days.GetValueOrDefault() == RoutineTaskDaysValue.EveryDay) ||
                          (t.Days.GetValueOrDefault() == RoutineTaskDaysValue.WorkDays && isWorkDay) ||
                          (t.Days.GetValueOrDefault() == RoutineTaskDaysValue.WeekEnds && !isWorkDay))
                )
                .Where(t => t.Time.GetValueOrDefault() == RoutineTaskTimeValue.Evening && DateTime.Now.Hour > 18 ||
                            t.Time.GetValueOrDefault() == RoutineTaskTimeValue.Afternoon && DateTime.Now.Hour > 12 ||
                            t.Time.GetValueOrDefault() == RoutineTaskTimeValue.Morning && DateTime.Now.Hour < 12));

                var time = DateTime.Now.Hour > 18
                    ? RoutineTaskTimeValue.Evening
                    : (DateTime.Now.Hour > 12 ? RoutineTaskTimeValue.Afternoon : RoutineTaskTimeValue.Morning);

                if (todaysTasks.Any())
                {
                    var select = new Select
                    {
                        Text = "Olá! Aqui estão as próximas tarefas da sua rotina:"
                    };
                    var options = todaysTasks.Select(task => new SelectOption
                    {
                        Text = $"{task.Name}.",
                    }).ToList();
                    options.Add(new SelectOption
                    {
                        Text = "Cancelar"
                    });
                    select.Options = options.ToArray();
                    await Sender.SendMessageAsync(select, message.From, cancellationToken);
                    StateManager.Instance.SetState(message.From, "waitingTaskSelection");
                }

                // Schedule next day
                await _scheduler.ConfigureScheduleAsync(routine, message.From, time, cancellationToken);

                await SetRoutineAsync(Node.Parse(identity.ToString()), routine, cancellationToken);
            }
        }
    }
}