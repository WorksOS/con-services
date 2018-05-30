using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.TRex.DI;
using VSS.TRex.Profiling.Factories;
using VSS.TRex.Profiling.Interfaces;
using VSS.TRex.Servers.Compute;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Storage;
using VSS.TRex.Storage.Interfaces;

namespace VSS.TRex.Server.PSNode
{
  class Program
  {
    private static void DependencyInjection()
    {
      DIBuilder
        .New()
        .AddLogging()
        .Add(x => x.AddSingleton<IStorageProxyFactory>(new StorageProxyFactory()))
        .Add(x => x.AddSingleton<ISiteModels>(new SiteModels.SiteModels()))
        .Add(x => x.AddSingleton<IProfilerBuilderFactory>(new ProfilerBuilderFactory()))
        .Complete();
    }

    static void Main(string[] args)
    { 
      DependencyInjection();

      var server = new SubGridProcessingServer();
      Console.WriteLine("Press anykey to exit");
      Console.ReadLine();
    }
  }
}
