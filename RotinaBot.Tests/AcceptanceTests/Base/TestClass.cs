using System;
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

        //[OneTimeSetUp]
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

        //[OneTimeTearDown]
        protected override void TearDown()
        {
            base.TearDown();
        }
    }
}