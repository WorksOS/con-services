using System;
using Microsoft.Extensions.DependencyInjection;

namespace RaptorTileServer
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
