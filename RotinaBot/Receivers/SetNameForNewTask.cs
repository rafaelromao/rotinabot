using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class SetNameForNewTask : BaseMessageReceiver
    {
        public SetNameForNewTask(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                if (StateManager.GetState(message.From) == Settings.States.WaitingPhoneNumber)
                    return;
                if (StateManager.GetState(message.From) == Settings.States.WaitingInitialMenuOption)
                    return;

                if (message.Content.ToString().Length < 4)
                {
                    await SendInitialMenuAsync(message.From, cancellationToken);
                    return;
                }
                    
                await SetNameForNewTaskAsync(message.From, message.Content, cancellationToken);
                await SendTaskDaysRequestAsync(message.From, cancellationToken);
                StateManager.SetState(message.From, Settings.States.WaitingTaskDays);
            }
            catch (Exception e)
            {
                await InformAProblemHasOcurredAsync(message.From, cancellationToken);
            }
        }

        private async Task SetNameForNewTaskAsync(Node owner, Document content, CancellationToken cancellationToken)
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

        private async Task SendTaskDaysRequestAsync(Node owner, CancellationToken cancellationToken)
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
                            Value = new PlainText { Text = Settings.Commands.Cancel },
                            Order = 4
                        }
                    }
            };
            await Sender.SendMessageAsync(select, owner, cancellationToken);
        }
    }
}