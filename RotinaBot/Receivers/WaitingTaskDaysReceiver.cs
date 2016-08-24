using System;
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
    public class WaitingTaskDaysReceiver : BaseMessageReceiver
    {
        public WaitingTaskDaysReceiver(IMessagingHubSender sender, IBucketExtension bucket, Settings settings) : base(sender, bucket, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                var daysText = ((PlainText)message.Content)?.Text;
                var daysValue = (RoutineTaskDaysValue)Enum.Parse(typeof(RoutineTaskDaysValue), daysText);
                var taskDays = new RoutineTaskDays { Value = daysValue };

                var routine = await GetRoutineAsync(message.From, cancellationToken);
                var task = routine.Tasks.Last();
                task.Days = taskDays;
                await SetRoutineAsync(message.From, routine, cancellationToken);

                var select = new Select
                {
                    Text = "E qual o melhor momento do dia para esta tarefa?",
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
                await Sender.SendMessageAsync(select, message.From, cancellationToken);
                StateManager.Instance.SetState(message.From, "waitingTaskTime");
            }
            catch (Exception e)
            {
                await Sender.SendMessageAsync("Desculpe, por favor responda com uma das opções apresentadas!", message.From, cancellationToken);
            }
        }
    }
}