using System;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.GridActivator
{
    class Program
    {
        static void Main(string[] args)
        {
      //TODO: Work out how we want to activate the grid in netcore. For now do it here directly.
          //Log.LogInformation("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Immutable TRex grid");
          bool result1 = ActivatePersistentGridServer.Instance().SetGridActive(TRexGrids.ImmutableGridName());
          //Log.LogInformation($"Activation process completed: Immutable = {result1}");

          //Log.LogInformation("About to call ActivatePersistentGridServer.Instance().SetGridActive() for Mutable TRex grid");
          bool result2 = ActivatePersistentGridServer.Instance().SetGridActive(TRexGrids.MutableGridName());
          //Log.LogInformation($"Activation process completed: Mutable = {result2}");
    }
  }
}
