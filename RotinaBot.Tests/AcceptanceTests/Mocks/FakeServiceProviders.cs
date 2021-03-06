using RotinaBot.Domain;
using RotinaBot.Tests.AcceptanceTests.Base;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Scheduler;

namespace RotinaBot.Tests.AcceptanceTests.Mocks
{
    public class FakeServiceProviderWithFakeBucketAndNoScheduler : TestServiceProvider
    {
        static FakeServiceProviderWithFakeBucketAndNoScheduler()
        {
            RegisterTestService<IBucketExtension, FakeBucketExtension>();
            RegisterTestService<ISchedulerExtension, NoSchedulerExtension>();
            RegisterTestService<ReschedulerTask, NoReschedulerTask>();
            RegisterTestService<ISMSAuthenticator, FakeSMSAuthenticator>();
        }
    }

    public class FakeServiceProviderWithFakeBucketAndScheduler : TestServiceProvider
    {
        static FakeServiceProviderWithFakeBucketAndScheduler()
        {
            RegisterTestService<IBucketExtension, FakeBucketExtension>();
            RegisterTestService<ISchedulerExtension, FakeSchedulerExtension>();
            RegisterTestService<ReschedulerTask, FakeReschedulerTask>();
            RegisterTestService<ISMSAuthenticator, FakeSMSAuthenticator>();
        }
    }
}