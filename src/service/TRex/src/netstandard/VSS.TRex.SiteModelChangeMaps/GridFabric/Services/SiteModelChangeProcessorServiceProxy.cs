using System;
using Apache.Ignite.Core.Services;
using Microsoft.Extensions.Logging;
using VSS.TRex.DI;
using VSS.TRex.GridFabric.Grids;
using VSS.TRex.SiteModelChangeMaps.GridFabric.NodeFilters;
using VSS.TRex.SiteModelChangeMaps.Interfaces.GridFabric.Services;
using VSS.TRex.Storage.Models;

namespace VSS.TRex.SiteModelChangeMaps.GridFabric.Services
{
  /// <summary>
  /// Class responsible for deploying the site model change processor service
  /// </summary>
  public class SiteModelChangeProcessorServiceProxy
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<SiteModelChangeProcessorServiceProxy>();

    /// <summary>
    /// The cluster wide name of the deployed service
    /// </summary>
    public const string ServiceName = "SiteModelChangeProcessorService";

    /// <summary>
    /// Services interface for the cluster group projection
    /// </summary>
    private readonly IServices _services;

    /// <summary>
    /// The proxy to the deployed service
    /// </summary>
    private ISiteModelChangeProcessorService _proxy;

    /// <summary>
    /// No-arg constructor that instantiates the Ignite instance, cluster, service and proxy members
    /// </summary>
    public SiteModelChangeProcessorServiceProxy()
    {
      var ignite = DIContext.Obtain<ITRexGridFactory>().Grid(StorageMutability.Immutable);

      // Get an instance of IServices for the cluster group.
      _services = ignite.GetServices();
    }

    /// <summary>
    /// Deploys the TAG file buffer queue service on to each TAG file processor node in the mutable grid.
    /// </summary>
    public void Deploy()
    {
      // Attempt to cancel any previously deployed service
      try
      {
        Log.LogInformation($"Cancelling deployed service {ServiceName}");
        _services.Cancel(ServiceName);
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception thrown while attempting to cancel service");
        throw;
      }

      try
      {
        Log.LogInformation("Deploying new service");

        _services.Deploy(new ServiceConfiguration
        {
          Name = ServiceName,
          Service = new SiteModelChangeProcessorService(),
          TotalCount = 0,
          MaxPerNodeCount = 1,
          NodeFilter = new SiteModelChangeProcessorRoleBasedNodeFilter()
        });
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception thrown while attempting to deploy service");
        throw;
      }

      try
      {
        Log.LogInformation($"Obtaining service proxy for {ServiceName}");
        _proxy = _services.GetServiceProxy<ISiteModelChangeProcessorService>(ServiceName);
      }
      catch (Exception e)
      {
        Log.LogError(e, "Exception thrown while attempting to get service proxy");
        throw;
      }
    }
  }
}
