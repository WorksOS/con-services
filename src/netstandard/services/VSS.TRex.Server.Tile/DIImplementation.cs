using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace VSS.TRex.Server.Application
{
    public class DIImplementation
    {
        public IServiceProvider ServiceProvider { get; internal set; }

        public DIImplementation(Action<IServiceCollection> configureDI)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            configureDI(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }
    }
}
