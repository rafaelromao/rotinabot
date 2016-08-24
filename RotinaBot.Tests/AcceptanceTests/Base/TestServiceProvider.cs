using Takenet.MessagingHub.Client.Tester;

namespace RotinaBot.Tests.AcceptanceTests.Base
{
    public class TestServiceProvider : ApplicationTesterServiceProvider
    {
        public static void RegisterTestService<TInterface, TClass>()
            where TInterface : class
            where TClass : class, TInterface
        {
            // Application Service Provider
            //((ServiceProvider)ApplicationTester.ApplicationServiceProvider).Container.RegisterSingleton<TInterface, TClass>();
            ((ServiceProvider)ApplicationTester.ApplicationServiceProvider).BeforeGetFirstService += (sender, container) =>
            {
                container.RegisterSingleton<TInterface, TClass>();
            };
        }
    }
}
