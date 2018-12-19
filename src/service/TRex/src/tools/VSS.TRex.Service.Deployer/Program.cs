using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.Logging;
using VSS.TRex.TAGFiles.GridFabric.Services;
using VSS.TRex.Servers.Client;

namespace VSS.TRex.Service.Deployer
{
  /// <summary>
  /// This command line tool deploys all grid deployed services into the appropriate mutable and immutable
  /// data grids in TRex.
  /// </summary>
  public class Program
  {
    private static ILogger Log;

    private static void DependencyInjection()
    {
      DIBuilder.New()
        .AddLogging()
        .Add(x => x.AddSingleton<ITRexGridFactory>(new TRexGridFactory()))
        .Build()
        .Add(x => x.AddSingleton(new MutableClientServer("ServiceDeployer")))
        .Add(x => x.AddSingleton(new ImmutableClientServer("ServiceDeployer")))
        .Complete();
    }

    static void Main(string[] args)
    {
      DependencyInjection();

      Log = Logger.CreateLogger<Program>();

      Log.LogInformation("Obtaining proxy for TAG file buffer queue service");

      // Ensure the continuous query service is installed that supports TAG file processing
      var tagBufferFileQueueProxy = new TAGFileBufferQueueServiceProxy();
      try
      {
        Log.LogInformation("Deploying TAG file buffer queue service");
        tagBufferFileQueueProxy.Deploy();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred deploying service:");
      }

      Log.LogInformation("Completed service deployment for TAG file buffer queue service");

      var segmentRetirementProxyMutable = new SegmentRetirementQueueServiceProxyMutable();
      try
      {
        Log.LogInformation("Deploying segment retirement queue service to the mutable grid");
        segmentRetirementProxyMutable.Deploy();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred deploying service:");
      }

      Log.LogInformation("Completed service deployment for mutable segment retirement queue service");

      // Deploy the retirement service to the immutable grid
      var segmentRetirementProxyImmutable = new SegmentRetirementQueueServiceProxyImmutable();
      try
      {
        Log.LogInformation("Deploying segment retirement queue service to the immutable grid");
        segmentRetirementProxyImmutable.Deploy();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred deploying service:");
      }

      Log.LogInformation("Completed service deployment for immutable segment retirement queue service");
    }
  }
}
