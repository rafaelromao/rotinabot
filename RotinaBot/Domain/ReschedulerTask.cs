using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;

namespace RotinaBot.Domain
{
    public class ReschedulerTask : IDisposable
    {
        private readonly ISchedulerExtension _scheduler;
        private readonly IDelegationExtension _delegation;
        private readonly RoutineRepository _routineRepository;
        private readonly Application _application;
        private readonly Settings _settings;

        public ReschedulerTask(ISchedulerExtension scheduler, IDelegationExtension delegation,
               RoutineRepository routineRepository, Application application, Settings settings)
        {
            _scheduler = scheduler;
            _delegation = delegation;
            _routineRepository = routineRepository;
            _application = application;
            _settings = settings;
        }

        private CancellationTokenSource _configureSchedulesCancellationTokenSource;

        private readonly HashSet<Identity> _routinesToUpdateSchedule = new HashSet<Identity>();

        public virtual void Start()
        {
            if (_configureSchedulesCancellationTokenSource == null)
            {
                _configureSchedulesCancellationTokenSource = new CancellationTokenSource();
                Task.Factory.StartNew(async () =>
                {
                    while (true)
                    {
                        if (_configureSchedulesCancellationTokenSource.IsCancellationRequested)
                            return;

                        await DelayBeforeReschedule();

                        Identity[] routinesToUpdateSchedule;
                        lock (_routinesToUpdateSchedule)
                        {
                            routinesToUpdateSchedule = _routinesToUpdateSchedule.ToArray();
                            _routinesToUpdateSchedule.Clear();
                        }
                        var time = DateTime.Now.Hour >= 18
                            ? RoutineTaskTimeValue.Evening
                            : DateTime.Now.Hour >= 12
                                ? RoutineTaskTimeValue.Afternoon
                                : RoutineTaskTimeValue.Morning;
                        foreach (var identity in routinesToUpdateSchedule)
                        {
                            await ConfigureScheduleAsync(identity, time, _configureSchedulesCancellationTokenSource.Token);
                        }
                    }
                }, _configureSchedulesCancellationTokenSource.Token, TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);
            }
        }

        protected virtual async Task DelayBeforeReschedule()
        {
            await Task.Delay(TimeSpan.FromSeconds(_settings.SchedulerDelayInSeconds));
        }

        public void ConfigureSchedule(Identity owner, CancellationToken cancellationToken)
        {
            lock (_routinesToUpdateSchedule)
            {
                if (!_routinesToUpdateSchedule.Contains(owner))
                {
                    _routinesToUpdateSchedule.Add(owner);
                }
            }
        }

        private async Task ConfigureScheduleAsync(Identity owner, RoutineTaskTimeValue time, CancellationToken cancellationToken)
        {
            Trace.WriteLine($"Start scheduling reminder for {owner} at {time}!");
            try
            {
                var routine = await _routineRepository.GetRoutineAsync(owner, false, cancellationToken);

                // Will send a message to itself, the next day only, reminding it to send a message with the routine for the given days and time for each client

                var currentTime = GetCurrentTime();

                var isBeforeMorning = currentTime.Hour < 6;
                var isBeforeAfternoon = currentTime.Hour < 12;
                var isBeforeEvening = currentTime.Hour < 18;

                var today = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, 0, 0, 0);

                var nextMorningSchedule = isBeforeMorning ? today.AddHours(6) : today.AddHours(6).AddDays(1);
                var nextAfternoonSchedule = isBeforeAfternoon ? today.AddHours(12) : today.AddHours(12).AddDays(1);
                var nextEveningSchedule = isBeforeEvening ? today.AddHours(18) : today.AddHours(18).AddDays(1);

                var shouldScheduleAtMorning = time == RoutineTaskTimeValue.Morning &&
                                              routine.LastMorningReminder < nextMorningSchedule;
                var shouldScheduleAtAfternoon = time == RoutineTaskTimeValue.Afternoon &&
                                                routine.LastAfternoonReminder < nextAfternoonSchedule;
                var shouldScheduleAtEvening = time == RoutineTaskTimeValue.Evening &&
                                              routine.LastEveningReminder < nextEveningSchedule;

                if (shouldScheduleAtMorning || shouldScheduleAtAfternoon || shouldScheduleAtEvening)
                {
                    await
                        _delegation.DelegateAsync(Identity.Parse("postmaster@scheduler.msging.net"),
                            new[] {EnvelopeType.Message}, cancellationToken);

                    var identity = new Node(_application.Identifier, _application.Domain, null);
                    var schedule = new Message
                    {
                        Id = Guid.NewGuid().ToString(),
                        To = identity,
                        Content = new IdentityDocument(routine.Owner.ToString())
                    };

                    if (shouldScheduleAtMorning)
                        routine.LastMorningReminder = nextMorningSchedule;
                    if (shouldScheduleAtAfternoon)
                        routine.LastAfternoonReminder = nextAfternoonSchedule;
                    if (shouldScheduleAtEvening)
                        routine.LastEveningReminder = nextEveningSchedule;

                    switch (time)
                    {
                        case RoutineTaskTimeValue.Morning:
                            await _scheduler.ScheduleMessageAsync(schedule, nextMorningSchedule, cancellationToken);
                            break;
                        case RoutineTaskTimeValue.Afternoon:
                            await _scheduler.ScheduleMessageAsync(schedule, nextAfternoonSchedule, cancellationToken);
                            break;
                        case RoutineTaskTimeValue.Evening:
                            await _scheduler.ScheduleMessageAsync(schedule, nextEveningSchedule, cancellationToken);
                            break;
                    }

                    await _routineRepository.SetRoutineAsync(routine, cancellationToken);
                }
                Trace.WriteLine($"Finished scheduling reminder for {owner} at {time}!");
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Failed scheduling reminder for {owner} at {time}: {e}");
                throw;
            }
        }

        protected virtual DateTime GetCurrentTime()
        {
            return DateTime.Now;
        }

        public void Dispose()
        {
            _configureSchedulesCancellationTokenSource?.Cancel(false);
        }
    }
}