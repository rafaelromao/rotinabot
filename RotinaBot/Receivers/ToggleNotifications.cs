using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public class ToggleNotifications : BaseMessageReceiver
    {
        public ToggleNotifications(
            IMessagingHubSender sender, IStateManager stateManager, 
            Settings settings, RoutineRepository routineRepository, ReschedulerTask reschedulerTask)
            : base(sender, stateManager, settings, routineRepository, reschedulerTask)
        {
        }

        public override async Task ReceiveAsync(Message message, CancellationToken cancellationToken)
        {
            if (await DisableNotificationsAsync(message.From, cancellationToken))
            {
                await InformNotificationsWhereDisabledAsync(message.From, cancellationToken);
            }
            else
            {
                await InformNotificationsWhereEnabledAsync(message.From, cancellationToken);
            }
        }

        private async Task<bool> DisableNotificationsAsync(Node owner, CancellationToken cancellationToken)
        {
            var routine = await GetRoutineAsync(owner, true, cancellationToken);
            var result = !routine.DisableNotifications;
            routine.DisableNotifications = !routine.DisableNotifications;
            await SetRoutineAsync(routine, cancellationToken);
            return result;
        }

        public async Task InformNotificationsWhereDisabledAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.YouWillNoLongerReceiveNotifications, owner, cancellationToken);
        }

        public async Task InformNotificationsWhereEnabledAsync(Node owner, CancellationToken cancellationToken)
        {
            await Sender.SendMessageAsync(Settings.Phraseology.YouWillNowReceiveNotifications, owner, cancellationToken);
        }
    }
}