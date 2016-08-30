using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot
{
    internal class SMSAuthenticator : ISMSAuthenticator
    {
        private readonly IMessagingHubSender _sender;

        public SMSAuthenticator(IMessagingHubSender sender)
        {
            _sender = sender;
        }

        public string GenerateAuthenticationCode()
        {
            var ticks = DateTime.Now.Ticks.ToString();
            ticks = ticks.Substring(ticks.Length - 4);
            return ticks;
        }

        public async Task SendSMSAsync(Routine routine, CancellationToken cancellationToken)
        {
            var authenticationCodeText = $"{nameof(RotinaBot)}: {routine.AuthenticationCode}";
            var phoneAddress = Node.Parse($"{routine.PhoneNumber}@tangram.com.br");
            await _sender.SendMessageAsync(authenticationCodeText, phoneAddress, cancellationToken);
        }
    }
}