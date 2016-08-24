using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class InitialInteractionReceiver : BaseMessageReceiver
    {
        public InitialInteractionReceiver(IMessagingHubSender sender, IBucketExtension bucket, Settings settings) : base(sender, bucket, settings) { }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = Settings.Phraseology.InitialMessage,
                Options = new[]
                {
                    new SelectOption
                    {
                        Text = Settings.Phraseology.WhatIHaveForToday,
                        Value = new PlainText { Text = Settings.Commands.ShowMyRoutine }
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.WhatIHaveForTheWeek,
                        Value = new PlainText { Text = Settings.Commands.ShowAllMyRoutine }
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.IncludeATaskInMyRoutine,
                        Value = new PlainText { Text = Settings.Commands.NewTask }
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.ExcludeATaskFromMyRoutine,
                        Value = new PlainText { Text = Settings.Commands.DeleteTask }
                    }
                }
            };
            await Sender.SendMessageAsync(select, message.From, cancellationToken);
        }
    }
}
