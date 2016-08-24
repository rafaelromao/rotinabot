using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class TaskSelectionReceiver : BaseMessageReceiver
    {
        public TaskSelectionReceiver(IMessagingHubSender sender, IBucketExtension bucket, Settings settings) : base(sender, bucket, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            try
            {
                int taskId;
                int.TryParse(((PlainText) message.Content)?.Text, out taskId);
                var routine = await GetRoutineAsync(message.From, cancellationToken);
                var task = routine.Tasks.FirstOrDefault(t => t.Id == taskId);
                if (task != null)
                {
                    task.LastTime = DateTimeOffset.Now;
                    await SetRoutineAsync(message.From, routine, cancellationToken);
                    await
                        Sender.SendMessageAsync("Parabéns! Continue cumprindo as tarefas da sua rotina!", message.From,
                            cancellationToken);
                    StateManager.Instance.SetState(message.From, "default");
                }
                else
                {
                    await Sender.SendMessageAsync("Quando tiver concluído alguma tarefa da sua rotina, basta me chamar!", message.From, cancellationToken);
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