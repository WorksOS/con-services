using System;
using Microsoft.Extensions.DependencyInjection;

namespace TRexTileServer
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
