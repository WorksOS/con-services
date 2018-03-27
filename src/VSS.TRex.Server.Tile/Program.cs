using System;
using Microsoft.Extensions.DependencyInjection;
using RaptorClassLibrary.netstandard.DI;
using VSS.TRex.Rendering.Abstractions;
using VSS.TRex.Rendering.Implementations.Core2;
using VSS.VisionLink.Raptor;
using VSS.VisionLink.Raptor.Servers.Client;

namespace VSS.TRex.Server.Application
{
    class Program
    {
        private static void CreateDependencyInjection()
        {
            DIImplementation DI = new DIImplementation(
                collection =>
                {
                    collection.AddSingleton<IRenderingFactory>(new RenderingFactory());
                });

            DIContext.Inject(DI.ServiceProvider);
        }

        static void Main(string[] args)
        {
            CreateDependencyInjection();

            var server = new RaptorApplicationServiceServer();
            Console.WriteLine($"Spatial Division {RaptorServerConfig.Instance().SpatialSubdivisionDescriptor}");
            Console.WriteLine("Press anykey to exit");
            Console.ReadLine();
        }
    }
}
