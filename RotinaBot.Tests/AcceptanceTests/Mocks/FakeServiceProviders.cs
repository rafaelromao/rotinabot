using RotinaBot.Tests.AcceptanceTests.Base;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Extensions.Scheduler;

namespace RotinaBot.Tests.AcceptanceTests.Mocks
{
    public class FakeServiceProviderWithFakeBucket : TestServiceProvider
    {
        static FakeServiceProviderWithFakeBucket()
        {
            RegisterTestService<IBucketExtension, FakeBucketExtension>();
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
}