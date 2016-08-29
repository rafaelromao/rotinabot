using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RotinaBot.Receivers;
using SimpleInjector;
using Takenet.MessagingHub.Client.Extensions.Scheduler;
using Takenet.MessagingHub.Client.Host;

namespace RotinaBot
{
    public class ServiceProvider : IServiceContainer
    {
        public Container Container { get; }

        public ServiceProvider()
        {
            Container = new Container();

            Container.Options.AllowOverridingRegistrations = true;

            Container.RegisterSingleton<ISMSSender, SMSSender>();
            Container.RegisterSingleton<Scheduler>();
            Container.RegisterSingleton<RotinaBot>();

            BeforeGetFirstService += delegate { };
        }

        public event EventHandler<Container> BeforeGetFirstService;
        private bool _gotFirstService = false;

        public object GetService(Type serviceType)
        {
            if (!_gotFirstService)
            {
                BeforeGetFirstService(this, Container);
                _gotFirstService = true;
            }

            return Container.GetInstance(serviceType);
        }

        public void RegisterService(Type serviceType, object instance)
        {
            Container.RegisterSingleton(serviceType, instance);
        }
    }
}
