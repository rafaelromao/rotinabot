using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using RotinaBot.Receivers;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Sender;
using Takenet.MessagingHub.Client;

namespace RotinaBot
{
    public class RotinaBot
    {
        private readonly IMessagingHubSender _sender;
        private readonly Scheduler _scheduler;
        private readonly IBucketExtension _bucket;
        private readonly ISMSAuthenticator _ismsAuthenticator;
        public IStateManager StateManager { get; }

        public Settings Settings { get; }

        public RotinaBot(IMessagingHubSender sender, IBucketExtension bucket, IStateManager stateManager, ISMSAuthenticator ismsAuthenticator, Scheduler scheduler, Settings settings)
        {
            _sender = sender;
            _scheduler = scheduler;
            _bucket = bucket;
            _ismsAuthenticator = ismsAuthenticator;
            StateManager = stateManager;
            Settings = settings;
        }

        #region Generic

        private async Task<Routine> GetRoutineAsync(Identity owner, bool allowSlaveRoutine, CancellationToken cancellationToken)
        {
            try
            {
                owner = owner.ToNode().ToIdentity();

                var routine = await _bucket.GetAsync<Routine>(owner.ToString(), cancellationToken);
                if (routine == null)
                {
                    routine = new Routine { Owner = owner };
                    await _bucket.SetAsync(owner.ToString(), routine, TimeSpan.FromDays(short.MaxValue), cancellationToken);
                }

                if (routine.PhoneNumberRegistrationStatus != PhoneNumberRegistrationStatus.Confirmed || allowSlaveRoutine)
                    return routine;

                var phoneNumber = await _bucket.GetAsync<PhoneNumber>(routine.PhoneNumber, cancellationToken);
                if (phoneNumber == null)
                    return routine;

                var ownerRoutine = await _bucket.GetAsync<Routine>(phoneNumber.Owner, cancellationToken);
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

        private async Task SetRoutineAsync(Routine routine, CancellationToken cancellationToken)
        {
            await _bucket.SetAsync(routine.Owner.ToString(), routine, TimeSpan.FromDays(short.MaxValue), cancellationToken);

            if (routine.PhoneNumberRegistrationStatus != PhoneNumberRegistrationStatus.Confirmed)
                return;

            var phoneNumber = await _bucket.GetAsync<PhoneNumber>(routine.PhoneNumber, cancellationToken);
            if (phoneNumber != null)
                return;

            await _bucket.SetAsync(
                routine.PhoneNumber, 
                new PhoneNumber
                {
                    Owner = routine.Owner.ToString(), Value = routine.PhoneNumber
                }, 
                TimeSpan.FromDays(short.MaxValue), 
                cancellationToken
            );
        }

        private static RoutineTask[] SortRoutineTasks(IEnumerable<RoutineTask> tasks)
        {
            return
                tasks.OrderBy(t => t.Time.GetValueOrDefault())
                    .ThenBy(t => t.Name)
                    .ToArray();
        }

        public async Task CancelCurrentOperationAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            if (!routine.Tasks.Last().IsActive)
            {
                routine.Tasks = routine.Tasks.Take(routine.Tasks.Length - 1).ToArray();
            }
            await SetRoutineAsync(routine, cancellationToken);
        }

        public async Task SendAtYourServiceMessageAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.WheneverYouNeed, owner, cancellationToken);
        }

        public async Task InformAnOptionShallBeChosenAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.SorryYouNeedToChooseAnOption, owner, cancellationToken);
        }

        public async Task SendInitialMenuAsync(Node owner, CancellationToken cancellationToken)
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
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }

        public async Task InformAProblemHasOcurredAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.SorryICannotHelpYouRightNow, owner, cancellationToken);
        }

        #endregion

        #region Registration

        public async Task<bool> IsPhoneNumberRegisteredAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            return routine.PhoneNumberRegistrationStatus != PhoneNumberRegistrationStatus.Pending;
        }

        public async Task OfferPhoneNumberRegistrationAsync(Node owner, CancellationToken cancellationToken)
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
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }

        public async Task IgnorePhoneNumberRegistrationAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            routine.PhoneNumberRegistrationStatus = PhoneNumberRegistrationStatus.Ignored;
            await SetRoutineAsync(routine, cancellationToken);
        }

        public async Task<bool> SavePhoneNumberAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            long phoneNumber;
            if (!long.TryParse(content.ToString().Replace("(", "").Replace(")", "").Replace("-", ""), out phoneNumber))
                return false;

            if (phoneNumber.ToString().Length < 10 || phoneNumber.ToString().Length > 13)
                return false;

            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            routine.PhoneNumber = phoneNumber.ToString();
            routine.AuthenticationCode = _ismsAuthenticator.GenerateAuthenticationCode();
            await SetRoutineAsync(routine, cancellationToken);
            return true;
        }

        public async Task SendPhoneNumberAuthenticationCodeAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            await _ismsAuthenticator.SendSMSAsync(routine, cancellationToken);
            await _sender.SendMessageAsync(Settings.Phraseology.InformSMSCode, owner, cancellationToken);
        }

        public async Task<bool> ConfirmPhoneNumberAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            if (content.ToString() != routine.AuthenticationCode)
                return false;

            routine.PhoneNumberRegistrationStatus = PhoneNumberRegistrationStatus.Confirmed;
            await SetRoutineAsync(routine, cancellationToken);
            return true;
        }

        public async Task InformPhoneNumberRegistrationCommandAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.InformRegisterPhoneCommand, owner, cancellationToken);
        }

        public async Task InformPhoneNumberRegistrationSucceededAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.RegistrationOkay, owner, cancellationToken);
        }

        public async Task InformPhoneNumberIsWrongAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.ThisIsNotAValidPhoneNumber, owner, cancellationToken);
        }

        public async Task InformPhoneNumberRegistrationFailedAsync(Node owner, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = Settings.Phraseology.RegistrationFailed,
                Options = new[] {
                    new SelectOption
                    {
                        Text = Settings.Phraseology.Yes,
                        Value = new PlainText { Text = Settings.Commands.Register },
                        Order = 1
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.No,
                        Value = new PlainText { Text = Settings.Commands.Cancel },
                        Order = 2
                    }
                }
            };
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }

        #endregion

        #region New

        public async Task SetDaysForNewTaskAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var daysText = ((PlainText)content)?.Text;
            var daysValue = (RoutineTaskDaysValue)Enum.Parse(typeof(RoutineTaskDaysValue), daysText);
            var taskDays = new RoutineTaskDays { Value = daysValue };

            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.Last();
            task.Days = taskDays;
            await SetRoutineAsync(routine, cancellationToken);
        }

        public async Task SendTaskTimeRequestAsync(Node owner, CancellationToken cancellationToken)
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
                            Value = new PlainText { Text = Settings.Commands.Cancel }
                        }
                    }
            };
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }

        public async Task SetNameForNewTaskAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var taskName = ((PlainText)content).Text;
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            routine.Tasks = routine.Tasks ?? new RoutineTask[0];
            routine.Tasks = routine.Tasks.Concat(new[]
            {
                new RoutineTask
                {
                    Id = DateTime.Now.Ticks,
                    Name = taskName
                }
            }).ToArray();
            await SetRoutineAsync(routine, cancellationToken);
        }

        public async Task SendTaskDaysRequestAsync(Node owner, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = Settings.Phraseology.WhichDaysShallThisTaskBePerformed,
                Options = new[]
                {
                        new SelectOption
                        {
                            Text = RoutineTaskDaysValue.EveryDay.Name(),
                            Order = (int)RoutineTaskDaysValue.EveryDay
                        },
                        new SelectOption
                        {
                            Text = RoutineTaskDaysValue.WorkDays.Name(),
                            Order = (int)RoutineTaskDaysValue.WorkDays
                        },
                        new SelectOption
                        {
                            Text = RoutineTaskDaysValue.WeekEnds.Name(),
                            Order = (int)RoutineTaskDaysValue.WeekEnds
                        },
                        new SelectOption
                        {
                            Text = Settings.Phraseology.Cancel,
                            Value = new PlainText { Text = Settings.Commands.Cancel }
                        }
                    }
            };
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }

        public async Task<RoutineTask> SetTimeForNewTaskAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var timeText = ((PlainText)content)?.Text;
            var timeValue = (RoutineTaskTimeValue)Enum.Parse(typeof(RoutineTaskDaysValue), timeText);
            var taskTime = new RoutineTaskTime { Value = timeValue };

            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.Last();
            task.Time = taskTime;
            await SetRoutineAsync(routine, cancellationToken);

            return task;
        }

        public async Task SendTaskConfirmationRequestAsync(Node owner, RoutineTask task, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = $"{task.Name} " +
                       $"{Settings.Phraseology.During} " +
                       $"{task.Time.GetValueOrDefault().Name().ToLower()} " +
                       $"{task.Days.GetValueOrDefault().Name().ToLower()}!",
                Options = new[]
                {
                        new SelectOption
                        {
                            Text = Settings.Phraseology.Confirm,
                            Value = new PlainText {Text = Settings.Commands.Confirm},
                            Order = 1
                        },
                        new SelectOption
                        {
                            Text = Settings.Phraseology.Cancel,
                            Value = new PlainText {Text = Settings.Phraseology.Cancel},
                            Order = 2
                        }
                    }
            };
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }

        public async Task RequestTaskNameAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.WhatIsTheTaskName, owner, cancellationToken);
        }

        public async Task FinishTaskCreationAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.Last();
            task.IsActive = true;
            await _scheduler.ConfigureScheduleAsync(routine, task.Time.GetValueOrDefault(), cancellationToken);
            await SetRoutineAsync(routine, cancellationToken);
        }

        public async Task InformTheTaskWasCreatedAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.TheTaskWasRegistered, owner, cancellationToken);
        }

        #endregion

        #region Delete

        public async Task<RoutineTask> PrepareTaskToBeDeletedAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var externalTaskId = RoutineTask.ExtractTaskIdFromDeleteCommand(((PlainText)content)?.Text);
            long taskId;
            long.TryParse(externalTaskId, out taskId);
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                // Put the task to be deleted at the end of the array
                routine.Tasks = routine.Tasks.Where(t => t != task).Concat(new[] { task }).ToArray();

                task.LastTime = DateTimeOffset.Now;
                await SetRoutineAsync(routine, cancellationToken);

                return task;
            }
            return null;
        }

        public async Task FinishTaskDeletionAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            routine.Tasks = routine.Tasks.Take(routine.Tasks.Length - 1).ToArray();
            await SetRoutineAsync(routine, cancellationToken);
        }

        public async Task SendDeleteConfirmationRequestAsync(Node owner, RoutineTask task, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = $"{Settings.Phraseology.ConfirmDelete} '" +
                       $"{task.Name} " +
                       $"{Settings.Phraseology.During} " +
                       $"{task.Time.GetValueOrDefault().Name().ToLower()} " +
                       $"{task.Days.GetValueOrDefault().Name().ToLower()}'?",
                Options = new[]
                {
                            new SelectOption
                            {
                                Text = Settings.Phraseology.Confirm,
                                Value = new PlainText {Text = Settings.Commands.Confirm},
                                Order = 1
                            },
                            new SelectOption
                            {
                                Text = Settings.Phraseology.Cancel,
                                Value = new PlainText {Text = Settings.Commands.Cancel},
                                Order = 2
                            }
                        }
            };
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }

        public async Task InformTheTaskWasRemovedAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.TheTaskWasRemoved, owner, cancellationToken);
        }

        public async Task InformTheTaskWasNotFoundAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.TheTaskWasNotFound, owner, cancellationToken);
        }

        #endregion

        #region Show

        private SelectOption[] BuildTaskSelectionOptions(IEnumerable<RoutineTask> tasks, Func<string, string> buildCommand)
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

        public async Task<bool> SendNextTasksAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var identity = content as IdentityDocument;
            if (identity == null)
                return false;

            var time = DateTime.Now.Hour >= 18
                ? RoutineTaskTimeValue.Evening
                : DateTime.Now.Hour >= 12 
                  ? RoutineTaskTimeValue.Afternoon 
                  : RoutineTaskTimeValue.Morning;

            var routine = await GetRoutineAsync(Node.Parse(identity.ToString()), false, cancellationToken);

            var isWorkDay = DateTime.Today.DayOfWeek != DayOfWeek.Saturday &&
                            DateTime.Today.DayOfWeek != DayOfWeek.Sunday;


            var tasks = SortRoutineTasks(routine.Tasks.Where(
                t => t.IsActive && t.LastTime.Date != DateTime.Today &&
                     ((t.Days.GetValueOrDefault() == RoutineTaskDaysValue.EveryDay) ||
                      (t.Days.GetValueOrDefault() == RoutineTaskDaysValue.WorkDays && isWorkDay) ||
                      (t.Days.GetValueOrDefault() == RoutineTaskDaysValue.WeekEnds && !isWorkDay))
                )
                .Where(t => t.Time.GetValueOrDefault() == time));

            await _scheduler.ConfigureScheduleAsync(routine, time, cancellationToken);

            await SetRoutineAsync(routine, cancellationToken);

            if (!tasks.Any())
                return false;

            var select = new Select
            {
                Text = Settings.Phraseology.HereAreYourNextTasks
            };
            select.Options = BuildTaskSelectionOptions(tasks, RoutineTask.CreateCompleteCommand);
            await _sender.SendMessageAsync(select, owner, cancellationToken);

            return true;
        }

        public async Task<bool> SendTasksForTheWeekAsync(Node owner, CancellationToken cancellationToken)
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

            await _sender.SendMessageAsync(text.ToString(), owner, cancellationToken);

            return true;
        }

        public async Task<bool> SendTasksThatCanBeDeletedAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var tasks = SortRoutineTasks(routine.Tasks);

            if (!tasks.Any())
            {
                return false;
            }

            var select = new Select
            {
                Text = Settings.Phraseology.ChooseATaskToBeDeleted
            };
            select.Options = select.Options = BuildTaskSelectionOptions(tasks, RoutineTask.CreateDeleteCommand);
            await _sender.SendMessageAsync(select, owner, cancellationToken);

            return true;
        }

        public async Task InformThereIsNoTaskRegisteredAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.NoTask, owner, cancellationToken);
        }

        public async Task InformThereIsNoTaskForTodayAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.NoTaskForToday, owner, cancellationToken);
        }

        public async Task<bool> SendTasksForTheDayAsync(Node owner, CancellationToken cancellationToken)
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
            await _sender.SendMessageAsync(select, owner, cancellationToken);

            return true;
        }

        public async Task<bool> MarkTaskAsCompletedAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var externalTaskId = RoutineTask.ExtractTaskIdFromCompleteCommand(((PlainText)content)?.Text);
            long taskId;
            long.TryParse(externalTaskId, out taskId);
            var routine = await GetRoutineAsync(owner, false, cancellationToken);
            var task = routine.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task == null)
                return false;

            task.LastTime = DateTimeOffset.Now;
            await SetRoutineAsync(routine, cancellationToken);

            return true;
        }

        public async Task InformTheTaskWasCompletedAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.KeepGoing, owner, cancellationToken);
        }

        #endregion
    }
}
