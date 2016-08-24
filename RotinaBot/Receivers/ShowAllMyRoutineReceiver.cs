using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class ShowAllMyRoutineReceiver : BaseMessageReceiver
    {
        public ShowAllMyRoutineReceiver(IMessagingHubSender sender, IBucketExtension bucket, Settings settings) : base(sender, bucket, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(message.From, cancellationToken);
            var todaysTasks = SortRoutineTasks(routine.Tasks.Where(
                t => t.IsActive
            ));

            if (!todaysTasks.Any())
            {
                await Sender.SendMessageAsync(Settings.Phraseology.NoTask, message.From, cancellationToken);
            }
            else
            {
                var text = new StringBuilder();
                text.AppendLine(Settings.Phraseology.HereAreYourTasksForTheWeek);
                text.AppendLine();
                todaysTasks.ForEach(task => text.AppendLine($"- {task.Name} " +
                                                            $"{Settings.Phraseology.During} " +
                                                            $"{task.Time.GetValueOrDefault().Name().ToLower()} " +
                                                            $"{task.Days.GetValueOrDefault().Name().ToLower()}."));

                await Sender.SendMessageAsync(text.ToString(), message.From, cancellationToken);
            }
        }
    }
}