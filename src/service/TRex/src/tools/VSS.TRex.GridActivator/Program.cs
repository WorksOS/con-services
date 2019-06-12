using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Interfaces;
using VSS.TRex.Logging;

namespace VSS.TRex.GridActivator
{
  class Program
  {
    private static ILogger Log;

    private static void DependencyInjection()
    {
      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(TRexGridFactory.AddGridFactoriesToDI)
        .Complete();
    }

    static void Main(string[] args)
    {
      DependencyInjection();

      try
      {
        Log = Logger.CreateLogger<Program>();

        Log.LogInformation("Activating Grids");

        Log.LogInformation(
          "About to call ActivatePersistentGridServer.Instance().SetGridActive() for Mutable TRex grid");
        bool result2 = DIContext.Obtain<IActivatePersistentGridServer>().SetGridActive(TRexGrids.MutableGridName());

        Log.LogInformation(
          "About to call ActivatePersistentGridServer.Instance().SetGridActive() for Immutable TRex grid");
        bool result1 = DIContext.Obtain<IActivatePersistentGridServer>().SetGridActive(TRexGrids.ImmutableGridName());

        Log.LogInformation($"Immutable Grid Active: {result1}");
        if (!result1)
        {
          Log.LogCritical("Immutable Grid failed to activate");
        }

        Log.LogInformation($"Mutable Grid Active: {result2}");
        if (!result2)
        {
          Log.LogCritical("Mutable Grid failed to activate");
        }
      }
      finally
      {
        DIContext.Obtain<ITRexGridFactory>()?.StopGrids();
      }
    }
  }
}
