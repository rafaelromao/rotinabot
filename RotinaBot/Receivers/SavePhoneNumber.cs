
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class SavePhoneNumber : BaseMessageReceiver
    {
        private readonly ISMSAuthenticator _smsAuthenticator;

        public SavePhoneNumber(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation,
            IStateManager stateManager, ISMSAuthenticator smsAuthenticator, Application application, Settings settings) 
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
            _smsAuthenticator = smsAuthenticator;
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await SavePhoneNumberAsync(message.From, message.Content, cancellationToken))
            {
                StateManager.SetState(message.From, Settings.States.WaitingSMSCode);

                await SendPhoneNumberAuthenticationCodeAsync(message.From, cancellationToken);
            }
            else
            {
                await InformPhoneNumberIsWrongAsync(message.From, cancellationToken);
            }
        }

        private async Task<bool> SavePhoneNumberAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            long phoneNumber;
            if (!long.TryParse(content.ToString().Replace("(", "").Replace(")", "").Replace("-", ""), out phoneNumber))
                return false;

            if (phoneNumber.ToString().Length < 10 || phoneNumber.ToString().Length > 13)
                return false;

            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            routine.PhoneNumber = phoneNumber.ToString();
            routine.AuthenticationCode = _smsAuthenticator.GenerateAuthenticationCode();
            await SetRoutineAsync(routine, cancellationToken);
            return true;
        }

        private async Task SendPhoneNumberAuthenticationCodeAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            await _smsAuthenticator.SendSMSAsync(routine, cancellationToken);
            await Sender.SendMessageAsync(Settings.Phraseology.InformSMSCode, owner, cancellationToken);
        }

        public async Task InformPhoneNumberIsWrongAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.ThisIsNotAValidPhoneNumber, owner, cancellationToken);
        }
    }
}