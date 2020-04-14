using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Productivity3D.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public abstract class FilterExecutorBase : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    protected FilterExecutorBase(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy,
      IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification, IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction,
      IFileImportProxy fileImportProxy,
      RepositoryBase repository, RepositoryBase auxRepository /*, IGeofenceProxy geofenceProxy, IUnifiedProductivityProxy unifiedProductivityProxy */)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction, fileImportProxy, repository, auxRepository /*, geofenceProxy, unifiedProductivityProxy*/)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    protected FilterExecutorBase()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Store the filter in the database and notify Raptor of a filter change
    /// </summary>
    protected async Task<T> StoreFilterAndNotifyRaptor<T>(FilterRequestFull filterRequest, int[] errorCodes) where T : IFilterEvent
    {
      var filterEvent = default(T);

      try
      {
        filterEvent = AutoMapperUtility.Automapper.Map<T>(filterRequest);
        filterEvent.ActionUTC = DateTime.UtcNow;

        var count = await ((IFilterRepository)Repository).StoreEvent(filterEvent);
        if (count == 0)
        {
          serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, errorCodes[0]);
        }
        else
        {
          // It's not necessary to invalidate the Raptor services proxy filter cache when a filter is created, or if it's transient.
          if (filterRequest.FilterType == FilterType.Transient || filterEvent is CreateFilterEvent)
          {
            return filterEvent;
          }

          _ = Task.Run(() => NotifyProductivity3D(filterRequest));
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, errorCodes[1], e.Message);
      }

      return filterEvent;
    }

    /// <summary>
    /// Notify 3dpm service that a filter has been added/updated/deleted.
    /// </summary>
    private async Task NotifyProductivity3D(FilterRequestFull filterRequest)
    {
      BaseMasterDataResult notificationResult = null;

      try
      {
        notificationResult = await Productivity3dV2ProxyNotification.NotifyFilterChange(new Guid(filterRequest.FilterUid),
          new Guid(filterRequest.ProjectUid), filterRequest.CustomHeaders);
      }
      catch (ServiceException se)
      {
        log.LogError(se, $"FilterExecutorBase: RaptorServices failed with service exception. FilterUid:{filterRequest.FilterUid}.");
        //rethrow this to surface it
        throw;
      }
      catch (Exception e)
      {
        log.LogError(e, $"FilterExecutorBase: RaptorServices failed with exception. FilterUid:{filterRequest.FilterUid}.");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 30, "Productivity3dV2ProxyNotification.NotifyFilterChange", e.Message);
      }

      log.LogDebug(
        $"FilterExecutorBase: NotifyFilterChange in RaptorServices returned code: {notificationResult?.Code ?? -1} Message {notificationResult?.Message ?? "notificationResult == null"}.");

      if (notificationResult != null && notificationResult.Code != 0)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 29, notificationResult.Code.ToString(), notificationResult.Message);
      }
    }
  }
}
