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

        public Settings Settings { get; }

        public RotinaBot(IMessagingHubSender sender, IBucketExtension bucket, Scheduler scheduler, Settings settings)
        {
            _sender = sender;
            _scheduler = scheduler;
            _bucket = bucket;
            Settings = settings;
        }

        private async Task<Routine> GetRoutineAsync(Node owner, CancellationToken cancellationToken)
        {
            try
            {
                return await _bucket.GetAsync<Routine>(owner.ToIdentity().ToString(), cancellationToken) ??
                       new Routine { Owner = owner };
            }
            catch
            {
                return new Routine { Owner = owner };
            }
        }

        private async Task SetRoutineAsync(Node owner, Routine routine, CancellationToken cancellationToken)
        {
            await _bucket.SetAsync(owner.ToIdentity().ToString(), routine, TimeSpan.FromDays(36500), cancellationToken);
            var proof = await _bucket.GetAsync<Routine>(owner.ToIdentity().ToString(), cancellationToken);
            if (proof.Tasks.Length != routine.Tasks.Length)
                throw new Exception();
        }

        private static RoutineTask[] SortRoutineTasks(IEnumerable<RoutineTask> tasks)
        {
            return
                tasks.OrderBy(t => t.Days.GetValueOrDefault())
                    .ThenBy(t => t.Time.GetValueOrDefault())
                    .ThenBy(t => t.Name)
                    .ToArray();
        }

        public async Task CancelCurrentOperationAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, cancellationToken);
            if (!routine.Tasks.Last().IsActive)
            {
                routine.Tasks = routine.Tasks.Take(routine.Tasks.Length - 1).ToArray();
            }
            await SetRoutineAsync(owner, routine, cancellationToken);
        }

        public async Task SendAtYourServiceMessageAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.WheneverYouNeed, owner, cancellationToken);
        }

        public async Task FinishTaskDeletionAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, cancellationToken);
            routine.Tasks = routine.Tasks.Take(routine.Tasks.Length - 1).ToArray();
            await SetRoutineAsync(owner, routine, cancellationToken);
        }

        public async Task InformTheTaskWasRemovedAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.TheTaskWasRemoved, owner, cancellationToken);
        }

        public async Task FinishTaskCreationAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, cancellationToken);
            var task = routine.Tasks.Last();
            task.IsActive = true;
            await _scheduler.ConfigureScheduleAsync(routine, owner, task.Time.GetValueOrDefault(), cancellationToken);
            await SetRoutineAsync(owner, routine, cancellationToken);
        }

        public async Task InformTheTaskWasCreatedAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.TheTaskWasRegistered, owner, cancellationToken);
        }

        public async Task<RoutineTask> PrepareTaskToBeDeletedAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            int taskId;
            int.TryParse(((PlainText)content)?.Text, out taskId);
            var routine = await GetRoutineAsync(owner, cancellationToken);
            var task = routine.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                // Put the task to be deleted at the end of the array
                routine.Tasks = routine.Tasks.Where(t => t != task).Concat(new[] { task }).ToArray();

                task.LastTime = DateTimeOffset.Now;
                await SetRoutineAsync(owner, routine, cancellationToken);

                return task;
            }
            return null;
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
                                Value = new PlainText {Text = Settings.Commands.ConfirmDeleteTask}
                            },
                            new SelectOption
                            {
                                Text = Settings.Phraseology.Cancel,
                                Value = new PlainText {Text = Settings.Commands.Cancel}
                            }
                        }
            };
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }

        public async Task InformTheTaskWasNotFoundAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.TheTaskWasNotFound, owner, cancellationToken);
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
                        Value = new PlainText { Text = Settings.Commands.ShowMyRoutine }
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.WhatIHaveForTheWeek,
                        Value = new PlainText { Text = Settings.Commands.ShowAllMyRoutine }
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.IncludeATaskInMyRoutine,
                        Value = new PlainText { Text = Settings.Commands.NewTask }
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.ExcludeATaskFromMyRoutine,
                        Value = new PlainText { Text = Settings.Commands.DeleteTask }
                    }
                }
            };
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }

        public async Task RequestTaskNameAsync(Node owner, CancellationToken cancellationToken)
        {
            await _sender.SendMessageAsync(Settings.Phraseology.WhatIsTheTaskName, owner, cancellationToken);
        }

        public async Task<bool> SendNextTasksAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var identity = content as IdentityDocument;
            if (identity == null)
                return false;

            var routine = await GetRoutineAsync(Node.Parse(identity.ToString()), cancellationToken);
            var isWorkDay = DateTime.Today.DayOfWeek != DayOfWeek.Saturday &&
                            DateTime.Today.DayOfWeek != DayOfWeek.Sunday;
            var todaysTasks = SortRoutineTasks(routine.Tasks.Where(
                t => t.IsActive && t.LastTime.Date != DateTime.Today &&
                     ((t.Days.GetValueOrDefault() == RoutineTaskDaysValue.EveryDay) ||
                      (t.Days.GetValueOrDefault() == RoutineTaskDaysValue.WorkDays && isWorkDay) ||
                      (t.Days.GetValueOrDefault() == RoutineTaskDaysValue.WeekEnds && !isWorkDay))
                )
                .Where(
                    t =>
                        t.Time.GetValueOrDefault() == RoutineTaskTimeValue.Evening && DateTime.Now.Hour > 18 ||
                        t.Time.GetValueOrDefault() == RoutineTaskTimeValue.Afternoon &&
                        DateTime.Now.Hour > 12 ||
                        t.Time.GetValueOrDefault() == RoutineTaskTimeValue.Morning && DateTime.Now.Hour < 12));

            var time = DateTime.Now.Hour > 18
                ? RoutineTaskTimeValue.Evening
                : (DateTime.Now.Hour > 12 ? RoutineTaskTimeValue.Afternoon : RoutineTaskTimeValue.Morning);

            await _scheduler.ConfigureScheduleAsync(routine, owner, time, cancellationToken);

            if (!todaysTasks.Any())
                return false;

            var select = new Select
            {
                Text = Settings.Phraseology.HereAreYourNextTask
            };
            var options = todaysTasks.Select(task => new SelectOption
            {
                Text = task.Name,
                Value = new PlainText { Text = task.Id.ToString() }
            }).ToList();
            options.Add(new SelectOption
            {
                Text = Settings.Phraseology.Cancel,
                Value = new PlainText { Text = Settings.Commands.Cancel }
            });
            select.Options = options.ToArray();
            await _sender.SendMessageAsync(select, owner, cancellationToken);

            return true;
        }

        public async Task<bool> SendTasksForTheWeekAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, cancellationToken);
            var todaysTasks = SortRoutineTasks(routine.Tasks.Where(
                t => t.IsActive
            ));

            if (!todaysTasks.Any())
                return false;

            var text = new StringBuilder();
            text.AppendLine(Settings.Phraseology.HereAreYourTasksForTheWeek);
            text.AppendLine();
            todaysTasks.ForEach(task => text.AppendLine($"- {task.Name} " +
                                                        $"{Settings.Phraseology.During} " +
                                                        $"{task.Time.GetValueOrDefault().Name().ToLower()} " +
                                                        $"{task.Days.GetValueOrDefault().Name().ToLower()}."));

            await _sender.SendMessageAsync(text.ToString(), owner, cancellationToken);

            return true;
        }

        public async Task<bool> SendTasksThatCanBeDeletedAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, cancellationToken);
            var todaysTasks = SortRoutineTasks(routine.Tasks);

            if (!todaysTasks.Any())
            {
                return false;
            }

            var select = new Select
            {
                Text = Settings.Phraseology.ChooseATaskToBeDeleted
            };
            var options = todaysTasks.Select(task => new SelectOption
            {
                Text = $"{task.Name} " +
                       $"{Settings.Phraseology.During} " +
                       $"{task.Time.GetValueOrDefault().Name().ToLower()} " +
                       $"{task.Days.GetValueOrDefault().Name().ToLower()}",
                Value = new PlainText { Text = task.Id.ToString() }
            }).ToList();
            options.Add(new SelectOption
            {
                Text = Settings.Phraseology.Cancel,
                Value = new PlainText { Text = Settings.Commands.Cancel }
            });
            select.Options = options.ToArray();
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
            var routine = await GetRoutineAsync(owner, cancellationToken);
            var isWorkDay = DateTime.Today.DayOfWeek != DayOfWeek.Saturday &&
                            DateTime.Today.DayOfWeek != DayOfWeek.Sunday;
            var todaysTasks = SortRoutineTasks(routine.Tasks.Where(
                t => t.IsActive && t.LastTime.Date != DateTime.Today &&
                     ((t.Days.Value == RoutineTaskDaysValue.EveryDay) ||
                      (t.Days.Value == RoutineTaskDaysValue.WorkDays && isWorkDay) ||
                      (t.Days.Value == RoutineTaskDaysValue.WeekEnds && !isWorkDay))
            ));

            if (!todaysTasks.Any())
                return false;

            var select = new Select
            {
                Text = Settings.Phraseology.HereAreYouTasksForToday
            };
            var options = todaysTasks.Select(task => new SelectOption
            {
                Text = $"{task.Name} " +
                       $"{Settings.Phraseology.During} " +
                       $"{task.Time.GetValueOrDefault().Name().ToLower()}",
                Value = new PlainText { Text = task.Id.ToString() }
            }).ToList();
            options.Add(new SelectOption
            {
                Text = Settings.Phraseology.Cancel,
                Order = options.Count,
                Value = new PlainText { Text = Settings.Commands.Cancel }
            });
            select.Options = options.ToArray();
            await _sender.SendMessageAsync(select, owner, cancellationToken);

            return true;
        }

        public async Task MarkTaskAsCompletedAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            int taskId;
            int.TryParse(((PlainText)content)?.Text, out taskId);
            var routine = await GetRoutineAsync(owner, cancellationToken);
            var task = routine.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                task.LastTime = DateTimeOffset.Now;
                await SetRoutineAsync(owner, routine, cancellationToken);
                await _sender.SendMessageAsync(Settings.Phraseology.KeepGoing, owner, cancellationToken);
            }
            else
            {
                await _sender.SendMessageAsync(Settings.Phraseology.CallMeWhenYouFinishATask, owner, cancellationToken);
            }
        }

        public async Task SetDaysForNewTaskAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var daysText = ((PlainText)content)?.Text;
            var daysValue = (RoutineTaskDaysValue)Enum.Parse(typeof(RoutineTaskDaysValue), daysText);
            var taskDays = new RoutineTaskDays { Value = daysValue };

            var routine = await GetRoutineAsync(owner, cancellationToken);
            var task = routine.Tasks.Last();
            task.Days = taskDays;
            await SetRoutineAsync(owner, routine, cancellationToken);
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
                        }
                    }
            };
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }

        public async Task SetNameForNewTaskAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var taskName = ((PlainText)content).Text;
            var routine = await GetRoutineAsync(owner, cancellationToken);
            routine.Tasks = routine.Tasks ?? new RoutineTask[0];
            routine.Tasks = routine.Tasks.Concat(new[]
            {
                new RoutineTask
                {
                    Id = DateTime.Now.Ticks,
                    Name = taskName
                }
            }).ToArray();
            await SetRoutineAsync(owner, routine, cancellationToken);
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

            var routine = await GetRoutineAsync(owner, cancellationToken);
            var task = routine.Tasks.Last();
            task.Time = taskTime;
            await SetRoutineAsync(owner, routine, cancellationToken);

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
                            Value = new PlainText {Text = Settings.Commands.ConfirmNewTask}
                        },
                        new SelectOption
                        {
                            Text = Settings.Phraseology.Cancel,
                            Value = new PlainText {Text = Settings.Phraseology.Cancel}
                        }
                    }
            };
            await _sender.SendMessageAsync(select, owner, cancellationToken);
        }
    }
}
