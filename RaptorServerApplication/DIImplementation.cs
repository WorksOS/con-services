using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
