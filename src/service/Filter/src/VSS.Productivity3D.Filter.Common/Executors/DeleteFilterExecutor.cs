using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class DeleteFilterExecutor : FilterExecutorBase
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public DeleteFilterExecutor(IConfigurationStore configStore, ILoggerFactory logger, IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy,
      IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification, IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction,
      IFileImportProxy fileImportProxy,
      RepositoryBase repository)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction, fileImportProxy, repository, null, null, null)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public DeleteFilterExecutor()
    { }

    /// <summary>
    /// Processes the DeleteFilter Request
    /// </summary>
    /// <param name="item"></param>
    /// <returns>a FiltersResult if successful</returns>     
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<FilterRequestFull>(item, 37);
      if (request == null) return null;

      var filter =
        (await ((IFilterRepository)Repository).GetFiltersForProjectUser(request.CustomerUid, request.ProjectUid,
          request.UserId, true).ConfigureAwait(false))
        .SingleOrDefault(f => string.Equals(f.FilterUid, request.FilterUid, StringComparison.OrdinalIgnoreCase));

      if (filter == null)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.BadRequest, 11);
      }
      log.LogDebug($"DeleteFilter retrieved filter {JsonConvert.SerializeObject(filter)}");

      var deleteEvent = await StoreFilterAndNotifyRaptor<DeleteFilterEvent>(request, new[] { 12, 13 });

      return new ContractExecutionResult();
    }
  }
}
