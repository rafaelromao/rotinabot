using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot
{
    internal class SMSSender : ISMSSender
    {
        private readonly IMessagingHubSender _sender;

        public SMSSender(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public async Task SendSMSAsync(Routine routine, CancellationToken cancellationToken)
        {
            var authenticationCodeText = $"{nameof(RotinaBot)}: {routine.AuthenticationCode}";
            var phoneAddress = Node.Parse($"{routine.PhoneNumber}@tangram.com.br");
            await _sender.SendMessageAsync(authenticationCodeText, phoneAddress, cancellationToken);
        }
    }
}