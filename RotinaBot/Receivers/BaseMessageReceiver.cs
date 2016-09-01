using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;

namespace RotinaBot.Receivers
{
    public abstract class BaseMessageReceiver : IMessageReceiver
    {
        private readonly ISchedulerExtension _scheduler;
        private readonly IDelegationExtension _delegation;
        private readonly Application _application;
        public IStateManager StateManager { get; }
        public IMessagingHubSender Sender { get; }
        public IBucketExtension Bucket { get; }
        public Settings Settings { get; }

        protected BaseMessageReceiver(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation, 
            IStateManager stateManager, Application application, Settings settings)
        {
            _scheduler = scheduler;
            _delegation = delegation;
            _application = application;
            StateManager = stateManager;
            Sender = sender;
            Bucket = bucket;
            Settings = settings;
        }

        public abstract Task ReceiveAsync(Message message, CancellationToken cancellationToken);

        protected async Task<Routine> GetRoutineAsync(Identity owner, bool allowSlaveRoutine, CancellationToken cancellationToken)
        {
            try
            {
                owner = owner.ToNode().ToIdentity();

                var routine = await Bucket.GetAsync<Routine>(owner.ToString(), cancellationToken);
                if (routine == null)
                {
                    routine = new Routine { Owner = owner };
                    await Bucket.SetAsync(owner.ToString(), routine, TimeSpan.FromDays(short.MaxValue), cancellationToken);
                }

                if (routine.PhoneNumberRegistrationStatus != PhoneNumberRegistrationStatus.Confirmed || allowSlaveRoutine)
                    return routine;

                var phoneNumber = await Bucket.GetAsync<PhoneNumber>(routine.PhoneNumber, cancellationToken);
                if (phoneNumber == null)
                    return routine;

                var ownerRoutine = await Bucket.GetAsync<Routine>(phoneNumber.Owner, cancellationToken);
                if (!ownerRoutine.Owner.Equals(routine.Owner) && routine.Tasks?.Length > 0)
                {
                    ownerRoutine.Tasks = ownerRoutine.Tasks.Concat(routine.Tasks).ToArray();
                    routine.Tasks = new RoutineTask[0];
                    await SetRoutineAsync(routine, cancellationToken);
                    await SetRoutineAsync(ownerRoutine, cancellationToken);
                }
                routine = ownerRoutine;

                return routine;
            }
            catch (Exception e)
            {
                return new Routine { Owner = owner };
            }
        }

        protected async Task SetRoutineAsync(Routine routine, CancellationToken cancellationToken)
        {
            await Bucket.SetAsync(routine.Owner.ToString(), routine, TimeSpan.FromDays(short.MaxValue), cancellationToken);

            if (routine.PhoneNumberRegistrationStatus != PhoneNumberRegistrationStatus.Confirmed)
                return;

            var phoneNumber = await Bucket.GetAsync<PhoneNumber>(routine.PhoneNumber, cancellationToken);
            if (phoneNumber != null)
                return;

            await Bucket.SetAsync(
                routine.PhoneNumber,
                new PhoneNumber
                {
                    Owner = routine.Owner.ToString(),
                    Value = routine.PhoneNumber
                },
                TimeSpan.FromDays(short.MaxValue),
                cancellationToken
            );
        }

        protected static RoutineTask[] SortRoutineTasks(IEnumerable<RoutineTask> tasks)
        {
            return
                tasks.OrderBy(t => t.Time.GetValueOrDefault())
                    .ThenBy(t => t.Name)
                    .ToArray();
        }

        protected async Task SendAtYourServiceMessageAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.WheneverYouNeed, owner, cancellationToken);
        }

        protected async Task ConfigureScheduleAsync(Routine routine, RoutineTaskTimeValue time, CancellationToken cancellationToken)
        {
            // Will send a message to itself, the next day only, reminding it to send a message with the routine for the given days and time for each client
            var shouldScheduleAtMorning = time == RoutineTaskTimeValue.Morning &&
                                          routine.LastMorningReminder != DateTime.Today;
            var shouldScheduleAtAfternoon = time == RoutineTaskTimeValue.Afternoon &&
                                          routine.LastAfternoonReminder != DateTime.Today;
            var shouldScheduleAtEvening = time == RoutineTaskTimeValue.Evening &&
                                          routine.LastEveningReminder != DateTime.Today;

            if (shouldScheduleAtMorning || shouldScheduleAtAfternoon || shouldScheduleAtEvening)
            {
                await _delegation.DelegateAsync(Identity.Parse("postmaster@scheduler.msging.net"), new[] { EnvelopeType.Message }, cancellationToken);

                if (shouldScheduleAtMorning)
                    routine.LastMorningReminder = DateTime.Today;
                if (shouldScheduleAtAfternoon)
                    routine.LastAfternoonReminder = DateTime.Today;
                if (shouldScheduleAtEvening)
                    routine.LastEveningReminder = DateTime.Today;

                var identity = new Node(_application.Identifier, _application.Domain, null);
                var schedule = new Message
                {
                    Id = Guid.NewGuid().ToString(),
                    To = identity,
                    Content = new IdentityDocument(routine.Owner.ToString())
                };
                var isBeforeMorning = DateTime.Now.Hour < 6;
                var isBeforeAfternoon = DateTime.Now.Hour < 12;
                var isBeforeEvening = DateTime.Now.Hour < 18;

                var today = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);

                var firstMorningSchedule = isBeforeMorning ? today.AddHours(6) : today.AddHours(6).AddDays(1);
                var firstAfternoonSchedule = isBeforeAfternoon ? today.AddHours(12) : today.AddHours(12).AddDays(1);
                var firstEveningSchedule = isBeforeEvening ? today.AddHours(18) : today.AddHours(18).AddDays(1);

                switch (time)
                {
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

        protected async Task InformTheTaskWasNotFoundAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.TheTaskWasNotFound, owner, cancellationToken);
        }

        protected async Task InformAnOptionShallBeChosenAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.SorryYouNeedToChooseAnOption, owner, cancellationToken);
        }

        protected async Task InformAProblemHasOcurredAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.SorryICannotHelpYouRightNow, owner, cancellationToken);
        }

        protected async Task InformThereIsNoTaskRegisteredAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.NoTask, owner, cancellationToken);
        }

        protected async Task OfferPhoneNumberRegistrationAsync(Node owner, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = Settings.Phraseology.PhoneNumberRegistrationOffer,
                Options = new[]
                {
                    new SelectOption
                    {
                        Text = Settings.Phraseology.IDoNotWant,
                        Value = new PlainText { Text = Settings.Commands.Ignore },
                        Order = 1
                    }
                }
            };
            await Sender.SendMessageAsync(select, owner, cancellationToken);
        }

        protected async Task SendInitialMenuAsync(Node owner, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = Settings.Phraseology.InitialMessage,
                Options = new[]
                {
                    new SelectOption
                    {
                        Text = Settings.Phraseology.WhatIHaveForToday,
                        Value = new PlainText { Text = Settings.Commands.Day },
                        Order = 1
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.WhatIHaveForTheWeek,
                        Value = new PlainText { Text = Settings.Commands.Week },
                        Order = 2
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.IncludeATaskInMyRoutine,
                        Value = new PlainText { Text = Settings.Commands.New },
                        Order = 3
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.ExcludeATaskFromMyRoutine,
                        Value = new PlainText { Text = Settings.Commands.Delete },
                        Order = 4
                    }
                }
            };
            await Sender.SendMessageAsync(select, owner, cancellationToken);
        }

        protected SelectOption[] BuildTaskSelectionOptions(IEnumerable<RoutineTask> tasks, Func<string, string> buildCommand)
        {
            var options = tasks.Select((task, i) => new SelectOption
            {
                Text = $"{task.Name} {Settings.Phraseology.During} {task.Time.GetValueOrDefault().Name().ToLower()}",
                Value = new PlainText { Text = buildCommand(task.Id.ToString()) },
                Order = i
            }).ToList();
            options.Add(new SelectOption
            {
                Text = Settings.Phraseology.Cancel,
                Value = new PlainText { Text = Settings.Commands.Cancel },
                Order = options.Count
            });
            return options.ToArray();
        }
    }
}
