using System;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class IgnorePhoneNumberRegistration : BaseMessageReceiver
    {
        public IgnorePhoneNumberRegistration(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
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