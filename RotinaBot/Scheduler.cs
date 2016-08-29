using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;

namespace RotinaBot
{
    public class Scheduler
    {
        private readonly Application _application;
        private readonly ISchedulerExtension _scheduler;
        private readonly IDelegationExtension _delegation;

        public Scheduler(Application application, ISchedulerExtension scheduler, IDelegationExtension delegation)
        {
            _application = application;
            _scheduler = scheduler;
            _delegation = delegation;
        }

        public async Task ConfigureScheduleAsync(Routine routine, Node from, RoutineTaskTimeValue time, bool forceSchedule, CancellationToken cancellationToken)
        {
            // Will send a message to itself, the next day only, reminding it to send a message with the routine for the given days and time for each client
            var isFirstScheduleForTheTimeOrTheDay = !routine.Schedules.Any(t => t == time);
            if (forceSchedule || isFirstScheduleForTheTimeOrTheDay)
            {
                await _delegation.DelegateAsync(Identity.Parse("postmaster@scheduler.msging.net"), new [] { EnvelopeType.Message }, cancellationToken);

                if (isFirstScheduleForTheTimeOrTheDay)
                {
                    routine.Schedules = routine.Schedules.Concat(new[] {time}).ToArray();
                }

                var identity = new Node(_application.Identifier, _application.Domain, null);
                var schedule = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    To = identity,
                    Content = new IdentityDocument(from.ToIdentity().ToString())
                };
                var isBeforeMorning = DateTime.Now.Hour < 6;
                var isBeforeAfternoon = DateTime.Now.Hour < 12;
                var isBeforeEvening = DateTime.Now.Hour < 18;
                var firstMorningSchedule = isBeforeMorning 
                    ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0 , 0) 
                    : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 6, 0, 0);
                var firstAfternoonSchedule = isBeforeAfternoon
                    ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 12, 0, 0)
                    : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 12, 0, 0);
                var firstEveningSchedule = isBeforeEvening
                    ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 18, 0, 0)
                    : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day + 1, 18, 0, 0);

                switch (time)
                {
                    default:
                    case RoutineTaskTimeValue.Morning:
                        await _scheduler.ScheduleMessageAsync(schedule, firstMorningSchedule, cancellationToken);
                        break;
                    case RoutineTaskTimeValue.Afternoon:
                        await _scheduler.ScheduleMessageAsync(schedule, firstAfternoonSchedule, cancellationToken);
                        break;
                    case RoutineTaskTimeValue.Evening:
                        await _scheduler.ScheduleMessageAsync(schedule, firstEveningSchedule, cancellationToken);
                        break;
                }
            }
        }
    }
}