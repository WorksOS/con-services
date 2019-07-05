using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.ConfigurationStore;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.GridFabric.Servers.Client;
using VSS.TRex.Logging;
using VSS.TRex.SiteModels.GridFabric.Services;
using VSS.TRex.TAGFiles.GridFabric.Services;

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
        .Add(x => x.AddSingleton<IConfigurationStore, GenericConfiguration>())
        .Add(TRexGridFactory.AddGridFactoriesToDI)
        .Build()
        .Add(x => x.AddSingleton(new MutableClientServer("ServiceDeployer")))
        .Add(x => x.AddSingleton(new ImmutableClientServer("ServiceDeployer")))
        .Complete();
    }

    private static void DeployTAGFileBufferQueueService()
    {
      Log.LogInformation("Obtaining proxy for TAG file buffer queue service");

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
    }

    private static void DeploySegmentRetirementQueueService()
    {
      Log.LogInformation("Obtaining proxy for segment retirement queue service");

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
    }

    private static void DeploySiteModelChangeProcessorService()
    {
      Log.LogInformation("Obtaining proxy for site model change processor service");

      var siteModelChangeProcessorService = new SiteModelChangeProcessorServiceProxy();
      try
      {
        Log.LogInformation("Deploying segment retirement queue service to the mutable grid");
        siteModelChangeProcessorService.Deploy();
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception occurred deploying service:");
      }

      Log.LogInformation("Completed service deployment for mutable segment retirement queue service");
    }
  
    static void Main(string[] args)
    {
      DependencyInjection();

      try
      {
        Log = Logger.CreateLogger<Program>();

        // Ensure the continuous query service is installed that supports TAG file processing
        DeployTAGFileBufferQueueService();

        // Ensure the service responsible for retiring segments updated by TAG file ingest is installed
        DeploySegmentRetirementQueueService();

        // Ensure the site model spatial data change tracking service is deployed.
        DeploySiteModelChangeProcessorService();
      }
      finally
      {
        DIContext.Obtain<ITRexGridFactory>()?.StopGrids();
      }
    }
  }
}
