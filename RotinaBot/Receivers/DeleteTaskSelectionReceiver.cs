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
    public class DeleteTaskSelectionReceiver : BaseMessageReceiver
    {
        public DeleteTaskSelectionReceiver(IMessagingHubSender sender, IBucketExtension bucket, Settings settings) : base(sender, bucket, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                int taskId;
                int.TryParse(((PlainText)message.Content)?.Text, out taskId);
                var routine = await GetRoutineAsync(message.From, cancellationToken);
                var task = routine.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    // Put the task to be deleted at the end of the array
                    routine.Tasks = routine.Tasks.Where(t => t != task).Concat(new[] { task }).ToArray();

                    task.LastTime = DateTimeOffset.Now;
                    await SetRoutineAsync(message.From, routine, cancellationToken);

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
                            Value = new PlainText {Text = Settings.Commands.ConfirmDeleteTask }
                        },
                        new SelectOption
                        {
                            Text = Settings.Phraseology.Cancel,
                            Value = new PlainText {Text = Settings.Commands.Cancel }
                        }
                    }
                    };
                    await Sender.SendMessageAsync(select, message.From, cancellationToken);
                    StateManager.Instance.SetState(message.From, Settings.States.WaitingDeleteTaskConfirmation);
                }
                else
                {
                    await Sender.SendMessageAsync(Settings.Phraseology.TheTaskWasNotFound, message.From, cancellationToken);
                    StateManager.Instance.SetState(message.From, Settings.States.Default);
                }
            }
            catch (Exception e)
            {
                await Sender.SendMessageAsync(Settings.Phraseology.SorryYouNeedToChooseAnOption, message.From, cancellationToken);
            }
        }
    }
}