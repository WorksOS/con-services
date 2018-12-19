using System;
using Microsoft.Extensions.DependencyInjection;
using VSS.ConfigurationStore;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Factories;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.SubGridTrees.Client;

namespace VSS.TRex.Tests.TestFixtures
{
  public class DILoggingFixture : IDisposable
  {
    private static object Lock = new object();

    public DILoggingFixture()
    {
      lock (Lock)
      {
        DIBuilder
          .New()
          .AddLogging()
          .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
          .Add(x => x.AddSingleton(ClientLeafSubgridFactoryFactory.CreateClientSubGridFactory()))
          .Add(x => x.AddSingleton<ISubGridSpatialAffinityKeyFactory>(new SubGridSpatialAffinityKeyFactory()))
          .Complete();
      }
    }

    public void Dispose()
    {
      DIBuilder.Continue().Eject();
    }
  }
}
