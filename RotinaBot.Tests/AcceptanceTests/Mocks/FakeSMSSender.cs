using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Tests.AcceptanceTests.Mocks
{
    internal class FakeSMSSender : ISMSSender
    {
        private readonly IMessagingHubSender _sender;
        private readonly Application _application;

        public FakeSMSSender(IMessagingHubSender sender, Application application)
        {
            _sender = sender;
            _application = application;
        }

        public async Task SendSMSAsync(Routine routine, CancellationToken cancellationToken)
        {
            var message = new Message
            {
                From = routine.Owner.ToNode(),
                To = new Node(_application.Identifier, "msging.net", null),
                Content = new PlainText { Text = routine.AuthenticationCode }
            };
            await _sender.SendMessageAsync(message, cancellationToken);
        }
    }
}