using RotinaBot.Tests.AcceptanceTests.Base;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Tester;

namespace RotinaBot.Tests.AcceptanceTests.Mocks
{
    public class FakeServiceProvider : TestServiceProvider
    {
        static FakeServiceProvider()
        {
            RegisterTestService<IBucketExtension, FakeBucketExtension>();
        }
    }
}