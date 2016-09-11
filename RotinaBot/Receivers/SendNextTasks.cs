using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class SendNextTasks : BaseMessageReceiver
    {
        public SendNextTasks(
            IMessagingHubSender sender, IStateManager stateManager, 
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask) 
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var owner = (message.Content as IdentityDocument)?.Value?.ToNode();
            var reschedule = true;
            if (owner == null)
            {
                owner = message.From;
                reschedule = false;
            }
            if (await SendNextTasksAsync(owner, reschedule, cancellationToken))
            {
                StateManager.SetState(owner, Settings.States.WaitingTaskSelection);
            }
        }

        private async Task<bool> SendNextTasksAsync(Node owner, bool reschedule, CancellationToken cancellationToken)
        {
            var time = DateTime.Now.Hour >= 18
                ? RoutineTaskTimeValue.Evening
                : DateTime.Now.Hour >= 12
                    ? RoutineTaskTimeValue.Afternoon
                    : RoutineTaskTimeValue.Morning;

            var routine = await GetRoutineAsync(owner, false, cancellationToken);

            var tasks = GetTasksForWeekEnds(routine).Where(t => t.Time.GetValueOrDefault() == time).ToArray();

            if (reschedule)
            {
                ConfigureSchedule(routine.Owner, cancellationToken);
            }

            if (!tasks.Any())
                return false;

            var select = new Select
            {
                Text = Settings.Phraseology.HereAreYourNextTasks
            };
            select.Options = BuildTaskSelectionOptions(tasks, RoutineTask.CreateCompleteCommand);
            await Sender.SendMessageAsync(select, owner, cancellationToken);

            return true;
        }
    }
}