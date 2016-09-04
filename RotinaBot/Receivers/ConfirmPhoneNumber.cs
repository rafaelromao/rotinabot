using System.Threading;
using System.Threading.Tasks;
using Lime.Messaging.Contents;
using Lime.Protocol;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class ConfirmPhoneNumber : BaseMessageReceiver
    {
        public ConfirmPhoneNumber(
            IMessagingHubSender sender, IStateManager stateManager,
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await ConfirmPhoneNumberAsync(message.From, message.Content, cancellationToken))
            {
                await InformPhoneNumberRegistrationSucceededAsync(message.From, cancellationToken);
                StateManager.SetState(message.From, Settings.States.Default);
            }
            else
            {
                await InformPhoneNumberRegistrationFailedAsync(message.From, cancellationToken);
                StateManager.SetState(message.From, Settings.States.Default);
            }
        }

        private async Task<bool> ConfirmPhoneNumberAsync(Node owner, Document content, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            if (content.ToString() != routine.AuthenticationCode)
                return false;

            routine.PhoneNumberRegistrationStatus = PhoneNumberRegistrationStatus.Confirmed;
            await SetRoutineAsync(routine, cancellationToken);
            return true;
        }

        private async Task InformPhoneNumberRegistrationSucceededAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.RegistrationOkay, owner, cancellationToken);
        }

        private async Task InformPhoneNumberRegistrationFailedAsync(Node owner, CancellationToken cancellationToken)
        {
            var select = new Select
            {
                Text = Settings.Phraseology.RegistrationFailed,
                Options = new[] {
                    new SelectOption
                    {
                        Text = Settings.Phraseology.Yes,
                        Value = new PlainText { Text = Settings.Commands.Register },
                        Order = 1
                    },
                    new SelectOption
                    {
                        Text = Settings.Phraseology.No,
                        Value = new PlainText { Text = Settings.Commands.Cancel },
                        Order = 2
                    }
                }
            };
            await Sender.SendMessageAsync(select, owner, cancellationToken);
        }
    }
}