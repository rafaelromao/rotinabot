using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Listener;
using Takenet.MessagingHub.Client.Sender;

namespace RotinaBot.Receivers
{
    public abstract class BaseMessageReceiver : IMessageReceiver
    {
        protected Settings Settings { get; }
        protected IMessagingHubSender Sender { get; }
        protected IBucketExtension Bucket { get; }

        protected BaseMessageReceiver(IMessagingHubSender sender, IBucketExtension bucket, Settings settings)
        {
            Settings = settings;
            Sender = sender;
            Bucket = bucket;
        }

        public abstract Task ReceiveAsync(Message message, CancellationToken cancellationToken);

        protected async Task<Routine> GetRoutineAsync(Node owner, CancellationToken cancellationToken)
        {
            try
            {
                return await Bucket.GetAsync<Routine>(owner.ToIdentity().ToString(), cancellationToken) ?? new Routine { Owner = owner };
            }
            catch
            {
                return new Routine { Owner = owner };
            }
        }

        protected async Task SetRoutineAsync(Node owner, Routine routine, CancellationToken cancellationToken)
        {
            await Bucket.SetAsync(owner.ToIdentity().ToString(), routine, TimeSpan.FromDays(36500), cancellationToken);
            var proof = await Bucket.GetAsync<Routine>(owner.ToIdentity().ToString(), cancellationToken);
            if (proof.Tasks.Length != routine.Tasks.Length)
                throw new Exception();
        }

        protected RoutineTask[] SortRoutineTasks(IEnumerable<RoutineTask> tasks)
        {
            return tasks.OrderBy(t => t.Time.GetValueOrDefault()).ThenBy(t => t.Days.GetValueOrDefault()).ThenBy(t => t.Name).ToArray();
        }
    }
}
