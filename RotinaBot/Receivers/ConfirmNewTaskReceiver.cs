using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class ConfirmNewTaskReceiver : BaseMessageReceiver
    {
        private readonly Scheduler _scheduler;

        public ConfirmNewTaskReceiver(Application application, IMessagingHubSender sender, IBucketExtension bucket,
            Scheduler scheduler, Settings settings) : base(sender, bucket, settings)
        {
            _scheduler = scheduler;
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(message.From, cancellationToken);
            var task = routine.Tasks.Last();
            task.IsActive = true;
            await _scheduler.ConfigureScheduleAsync(routine, message.From, task.Time.GetValueOrDefault(), cancellationToken);
            await SetRoutineAsync(message.From, routine, cancellationToken);
            await
                Sender.SendMessageAsync("Parabéns! Essa tarefa foi incluída em sua rotina!", message.From,
                    cancellationToken);
        }
    }
}