using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using RotinaBot.Documents;
using Takenet.MessagingHub.Client.Extensions.Bucket;

namespace RotinaBot.Domain
{
    public class RoutineRepository
    {
        private readonly IBucketExtension _bucket;

        public RoutineRepository(IBucketExtension bucket)
        {
            _bucket = bucket;
        }

        public async Task<Routine> GetRoutineAsync(Identity owner, bool allowSlaveRoutine, CancellationToken cancellationToken)
        {
            try
            {
                owner = owner.ToNode().ToIdentity();

                var routine = await _bucket.GetAsync<Routine>(owner.ToString(), cancellationToken);
                if (routine == null)
                {
                    routine = new Routine { Owner = owner };
                    await _bucket.SetAsync(owner.ToString(), routine, TimeSpan.FromDays(short.MaxValue), cancellationToken);
                }

                if (routine.PhoneNumberRegistrationStatus != PhoneNumberRegistrationStatus.Confirmed || allowSlaveRoutine)
                    return routine;

                var phoneNumber = await _bucket.GetAsync<PhoneNumber>(routine.PhoneNumber, cancellationToken);
                if (phoneNumber == null)
                    return routine;

                var ownerRoutine = await _bucket.GetAsync<Routine>(phoneNumber.Owner, cancellationToken);
                if (!ownerRoutine.Owner.Equals(routine.Owner) && routine.Tasks?.Length > 0)
                {
                    ownerRoutine.Tasks = ownerRoutine.Tasks.Concat(routine.Tasks).ToArray();
                    routine.Tasks = new RoutineTask[0];
                    await SetRoutineAsync(routine, cancellationToken);
                    await SetRoutineAsync(ownerRoutine, cancellationToken);
                }
                routine = ownerRoutine;

                return routine;
            }
            catch
            {
                return new Routine { Owner = owner };
            }
        }

        public async Task SetRoutineAsync(Routine routine, CancellationToken cancellationToken)
        {
            await _bucket.SetAsync(routine.Owner.ToString(), routine, TimeSpan.FromDays(short.MaxValue), cancellationToken);

            if (routine.PhoneNumberRegistrationStatus != PhoneNumberRegistrationStatus.Confirmed)
                return;

            var phoneNumber = await _bucket.GetAsync<PhoneNumber>(routine.PhoneNumber, cancellationToken);
            if (phoneNumber != null)
                return;

            await _bucket.SetAsync(
                routine.PhoneNumber,
                new PhoneNumber
                {
                    Owner = routine.Owner.ToString(),
                    Value = routine.PhoneNumber
                },
                TimeSpan.FromDays(short.MaxValue),
                cancellationToken
            );
        }
    }
}