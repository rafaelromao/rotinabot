using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Listener;

namespace RotinaBot.Receivers
{
    public abstract class BaseMessageReceiver : IMessageReceiver
    {
        protected RotinaBot Bot { get; }

        protected BaseMessageReceiver(RotinaBot bot)
        {
            Bot = bot;
        }

        public abstract Task ReceiveAsync(Message message, CancellationToken cancellationToken);
    }
}
