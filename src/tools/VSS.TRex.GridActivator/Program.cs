﻿using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Logging;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.GridActivator
{
  class Program
  {
    private static ILogger Log;

    private static void DependencyInjection()
    {
      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Complete();
    }

    static void Main(string[] args)
    {
      DependencyInjection();
      Log = Logger.CreateLogger<Program>();

      Log.LogInformation("Activating Grids");


      //TODO: Work out how we want to activate the grid in netcore. For now do it here directly.
      //Log.LogInformation("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Immutable TRex grid");

      //Log.LogInformation($"Activation process completed: Immutable = {result1}");

      //Log.LogInformation("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Mutable TRex grid");
      bool result2 = ActivatePersistentGridServer.Instance().SetGridActive(TRexGrids.MutableGridName());
      //Log.LogInformation($"Activation process completed: Mutable = {result2}");

      bool result1 = ActivatePersistentGridServer.Instance().SetGridActive(TRexGrids.ImmutableGridName());

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
  }
}
