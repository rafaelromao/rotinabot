using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class WaitingTaskNameReceiver : BaseMessageReceiver
    {
        public WaitingTaskNameReceiver(IMessagingHubSender sender, IBucketExtension bucket, Settings settings) : base(sender, bucket, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                var taskName = ((PlainText)message.Content).Text;
                var routine = await GetRoutineAsync(message.From, cancellationToken);
                var task = routine.Tasks?.FirstOrDefault(t => t.Name == taskName);
                if (task == null)
                {
                    routine.Tasks = routine.Tasks ?? new RoutineTask[0];
                    routine.Tasks = routine.Tasks.Concat(new[]
                    {
                        new RoutineTask
                        {
                            Id = DateTime.Now.Ticks,
                            Name = taskName
                        }
                    }).ToArray();
                }
                await SetRoutineAsync(message.From, routine, cancellationToken);

                var select = new Select
                {
                    Text = "E em que dias da semana essa tarefa deve ser realizada?",
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
                await Sender.SendMessageAsync(select, message.From, cancellationToken);
                StateManager.Instance.SetState(message.From, "waitingTaskDays");
            }
            catch (Exception e)
            {
                await Sender.SendMessageAsync("Desculpe, por favor informe o nome da tarefa!", message.From, cancellationToken);
            }
        }
    }
}