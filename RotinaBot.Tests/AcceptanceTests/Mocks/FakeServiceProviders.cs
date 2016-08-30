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
        }
    }

    public class FakeServiceProviderWithFakeBucketAndScheduler : TestServiceProvider
    {
        static FakeServiceProviderWithFakeBucketAndScheduler()
        {
            RegisterTestService<IBucketExtension, FakeBucketExtension>();
            RegisterTestService<ISchedulerExtension, FakeSchedulerExtension>();
        }
    }

    public class FakeServiceProviderWithFakeBucketNoSchedulerAndFakeSMSSender : TestServiceProvider
    {
        static FakeServiceProviderWithFakeBucketNoSchedulerAndFakeSMSSender()
        {
            RegisterTestService<IBucketExtension, FakeBucketExtension>();
            RegisterTestService<ISchedulerExtension, NoSchedulerExtension>();
            RegisterTestService<ISMSAuthenticator, FakeSMSAuthenticator>();
        }
    }

}