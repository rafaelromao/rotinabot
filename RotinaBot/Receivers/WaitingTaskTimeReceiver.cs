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
    public class WaitingTaskTimeReceiver : BaseMessageReceiver
    {
        public WaitingTaskTimeReceiver(IMessagingHubSender sender, IBucketExtension bucket, Settings settings) : base(sender, bucket, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                var timeText = ((PlainText)message.Content)?.Text;
                var timeValue = (RoutineTaskTimeValue)Enum.Parse(typeof(RoutineTaskDaysValue), timeText);
                var taskTime = new RoutineTaskTime { Value = timeValue };

                var routine = await GetRoutineAsync(message.From, cancellationToken);
                var task = routine.Tasks.Last();
                task.Time = taskTime;
                await SetRoutineAsync(message.From, routine, cancellationToken);

                var select = new Select
                {
                    Text = $"{task.Name} durante a {task.Time.GetValueOrDefault().Name().ToLower()} {task.Days.GetValueOrDefault().Name().ToLower()}!",
                    Options = new[]
                    {
                        new SelectOption
                        {
                            Text = "Confirmar",
                            Value = new PlainText {Text = "/confirmnewtask"}
                        },
                        new SelectOption
                        {
                            Text = "Cancelar",
                            Value = new PlainText {Text = "/cancelnewtask"}
                        }
                    }
                };
                await Sender.SendMessageAsync(select, message.From, cancellationToken);
                StateManager.Instance.SetState(message.From, "waitingTaskConfirmation");
            }
            catch (Exception e)
            {
                await Sender.SendMessageAsync("Desculpe, por favor responda com uma das opções apresentadas!", message.From, cancellationToken);
            }
        }
    }
}