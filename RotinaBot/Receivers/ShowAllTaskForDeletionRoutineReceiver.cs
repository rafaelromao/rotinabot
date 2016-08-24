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
    public class ShowAllTaskForDeletionRoutineReceiver : BaseMessageReceiver
    {
        public ShowAllTaskForDeletionRoutineReceiver(IMessagingHubSender sender, IBucketExtension bucket, Settings settings) : base(sender, bucket, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(message.From, cancellationToken);
            var todaysTasks = SortRoutineTasks(routine.Tasks);

            if (!todaysTasks.Any())
            {
                await Sender.SendMessageAsync(Settings.Phraseology.NoTask, message.From, cancellationToken);
                StateManager.Instance.SetState(message.From, Settings.States.Default);
            }
            else
            {
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
                await Sender.SendMessageAsync(select, message.From, cancellationToken);
                StateManager.Instance.SetState(message.From, Settings.States.WaitingDeleteTaskSelection);
            }
        }
    }
}