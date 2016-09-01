using System;
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
    public class IgnorePhoneNumberRegistration : BaseMessageReceiver
    {
        public IgnorePhoneNumberRegistration(
            IMessagingHubSender sender, IBucketExtension bucket, ISchedulerExtension scheduler, IDelegationExtension delegation,
            IStateManager stateManager, Application application, Settings settings)
            : base(sender, bucket, scheduler, delegation, stateManager, application, settings)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            await IgnorePhoneNumberRegistrationAsync(message.From, cancellationToken);
            await InformPhoneNumberRegistrationCommandAsync(message.From, cancellationToken);
            StateManager.SetState(message.From, Settings.States.Default);
        }

        private async Task IgnorePhoneNumberRegistrationAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            routine.PhoneNumberRegistrationStatus = PhoneNumberRegistrationStatus.Ignored;
            await SetRoutineAsync(routine, cancellationToken);
        }

        private async Task InformPhoneNumberRegistrationCommandAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.InformRegisterPhoneCommand, owner, cancellationToken);
        }
    }
}