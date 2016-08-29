using System;
using NUnit.Framework;
using RotinaBot.Tests.AcceptanceTests.Mocks;
using Takenet.MessagingHub.Client.Extensions.Bucket;
using Takenet.MessagingHub.Client.Tester;

namespace RotinaBot.Tests.AcceptanceTests.Base
{
    public class TestClass<TServiceProvider> : Takenet.MessagingHub.Client.Tester.TestClass<TServiceProvider>
        where TServiceProvider : ApplicationTesterServiceProvider
    {
        protected override ApplicationTesterOptions Options<TTestServiceProvider>()
        {
            var options = base.Options<TTestServiceProvider>();
            options.EnableMutualDelegation = true;
            options.EnableConsoleListener = true;
            return options;
        }

        // Application Settings
        protected Settings Settings { get; set; }

        [OneTimeSetUp]
        protected override void SetUp()
        {
            try
            {
                base.SetUp();
            }
            catch (Exception e)
            {
                throw;
            }
            Settings = Tester.GetService<Settings>();
        }

        [SetUp]
        protected void CleanUp()
        {
            ((FakeBucketExtension)Tester.GetService<IBucketExtension>()).Clear();
            Tester.IgnoreMessageAsync(TimeSpan.FromMilliseconds(100)).Wait();
            Tester.IgnoreMessageAsync(TimeSpan.FromMilliseconds(100)).Wait();
            Tester.IgnoreMessageAsync(TimeSpan.FromMilliseconds(100)).Wait();
        }

        [OneTimeTearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }
    }
}