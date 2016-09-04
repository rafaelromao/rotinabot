using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RotinaBot.Domain;
using Takenet.MessagingHub.Client.Extensions.Delegation;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;

namespace RotinaBot.Tests.AcceptanceTests.Mocks
{
    internal class FakeReschedulerTask : ReschedulerTask
    {
        public FakeReschedulerTask(
            ISchedulerExtension scheduler, IDelegationExtension delegation, 
            RoutineRepository routineRepository, Application application, Settings settings) 
            : base(scheduler, delegation, routineRepository, application, settings)
        {
        }

        protected override async Task DelayBeforeReschedule()
        {
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
    }

    internal class NoReschedulerTask : ReschedulerTask
    {
        public NoReschedulerTask(
            ISchedulerExtension scheduler, IDelegationExtension delegation,
            RoutineRepository routineRepository, Application application, Settings settings)
            : base(scheduler, delegation, routineRepository, application, settings)
        {
        }

        public override void Start()
        {
            //Does nothing
        }
    }
}
