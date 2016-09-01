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
    public class SendInitialMenu : BaseMessageReceiver
    {
        public SendInitialMenu(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation,
            IStateManager stateManager, Application application, Settings settings)
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await IsPhoneNumberRegisteredAsync(message.From, cancellationToken))
            {
                await SendInitialMenuAsync(message.From, cancellationToken);
            }
            else
            {
                await OfferPhoneNumberRegistrationAsync(message.From, cancellationToken);
                StateManager.SetState(message.From, Settings.States.WaitingPhoneNumber);
            }
        }

        private async Task<bool> IsPhoneNumberRegisteredAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            return routine.PhoneNumberRegistrationStatus != PhoneNumberRegistrationStatus.Pending;
        }
    }
}
