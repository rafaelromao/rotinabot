using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client;

namespace RotinaBot.Receivers
{
    public abstract class BaseMessageReceiver : IMessageReceiver
    {
        private readonly RoutineRepository _routineRepository;
        private readonly ReschedulerTask _reschedulerTask;

        public IStateManager StateManager { get; }
        public IMessagingHubSender Sender { get; }
        public Settings Settings { get; }

        protected BaseMessageReceiver(
            IMessagingHubSender sender, IStateManager stateManager, 
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
        {
            _routineRepository = routineRepository;
            _reschedulerTask = reschedulerTask;
            StateManager = stateManager;
            Sender = sender;
            Settings = settings;
        }

        public abstract Task ReceiveAsync(Message message, CancellationToken cancellationToken);

        protected async Task SetRoutineAsync(Routine routine, CancellationToken cancellationToken)
        {
            await _routineRepository.SetRoutineAsync(routine, cancellationToken);
        }

        protected async Task<Routine> GetRoutineAsync(Identity owner, bool allowSlaveRoutine, CancellationToken cancellationToken)
        {
            return await _routineRepository.GetRoutineAsync(owner, allowSlaveRoutine, cancellationToken);
        }

        public void ConfigureSchedule(Identity owner, CancellationToken cancellationToken)
        {
            _reschedulerTask.ConfigureSchedule(owner, cancellationToken);
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
                        Text = Settings.Phraseology.WhatAreMyNextTasks,
                        Value = new PlainText { Text = Settings.Commands.Next },
                        Order = 1
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.WhatIHaveForToday,
                        Value = new PlainText { Text = Settings.Commands.Day },
                        Order = 2
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.WhatIHaveForTheWeek,
                        Value = new PlainText { Text = Settings.Commands.Week },
                        Order = 3
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.IncludeATaskInMyRoutine,
                        Value = new PlainText { Text = Settings.Commands.New },
                        Order = 4
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.ExcludeATaskFromMyRoutine,
                        Value = new PlainText { Text = Settings.Commands.Delete },
                        Order = 5
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.Cancel,
                        Value = new PlainText { Text = Settings.Commands.Cancel },
                        Order = 6
                    }
                }
            };
            await Sender.SendMessageAsync(select, owner, cancellationToken);
        }

        protected static IEnumerable<RoutineTask> GetTasksForWeekEnds(Routine routine)
        {
            var isSaturday = DateTime.Today.DayOfWeek == DayOfWeek.Saturday;
            var isSunday = DateTime.Today.DayOfWeek == DayOfWeek.Sunday;
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
            return tasks;
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
