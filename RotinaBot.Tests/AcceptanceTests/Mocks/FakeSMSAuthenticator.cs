using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Documents;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Tests.AcceptanceTests.Mocks
{
    internal class FakeSMSAuthenticator : ISMSAuthenticator
    {
        private readonly IMessagingHubSender _sender;
        private readonly Application _application;

        public FakeSMSAuthenticator(IMessagingHubSender sender, Application application)
        {
            _sender = sender;
            _application = application;
        }

        public string GenerateAuthenticationCode()
        {
            return "5555";
        }

        public Task SendSMSAsync(Routine routine, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}