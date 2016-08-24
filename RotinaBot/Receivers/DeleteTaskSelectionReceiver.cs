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
                        Text = $"Confirma a exclusão da tarefa '{task.Name} durante a {task.Time.GetValueOrDefault().Name().ToLower()} {task.Days.GetValueOrDefault().Name().ToLower()}'?",
                        Options = new[]
                        {
                        new SelectOption
                        {
                            Text = "Confirmar",
                            Value = new PlainText {Text = "/confirmdeletetask"}
                        },
                        new SelectOption
                        {
                            Text = "Cancelar",
                            Value = new PlainText {Text = "/canceldeletetask"}
                        }
                    }
                    };
                    await Sender.SendMessageAsync(select, message.From, cancellationToken);
                    StateManager.Instance.SetState(message.From, "waitingDeleteTaskConfirmation");
                }
                else
                {
                    await Sender.SendMessageAsync("A tarefa não foi encontrada para se excluída!", message.From, cancellationToken);
                    StateManager.Instance.SetState(message.From, "default");
                }
            }
            catch (Exception e)
            {
                await Sender.SendMessageAsync("Desculpe, por favor responda com uma das opções apresentadas!", message.From, cancellationToken);
            }
        }
    }
}