using Takenet.MessagingHub.Client.Tester;

namespace RotinaBot.Tests.AcceptanceTests.Base
{
    public class TestServiceProvider : ApplicationTesterServiceProvider
    {
        public static void RegisterTestService<TInterface, TClass>()
            where TInterface : class
            where TClass : class, TInterface
        {
            // Your ServiceProvider must have an event named BeforeGetFirstService of type EventHandler<Container>.
			// Such event must be fired just before the first call to GetService and will be used by the TestServiceProvider
			// to inject the mock services into the original container
            ((ServiceProvider)ApplicationTester.ApplicationServiceProvider).BeforeGetFirstService += (sender, container) =>
            {
                container.RegisterSingleton<TInterface, TClass>();
            };
        }
    }
}
