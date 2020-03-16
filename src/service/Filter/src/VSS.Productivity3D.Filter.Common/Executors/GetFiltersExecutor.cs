using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.Filter.Abstractions.Models;
using VSS.Productivity3D.Filter.Abstractions.Models.ResultHandling;
using VSS.Productivity3D.Filter.Common.Models;
using VSS.Productivity3D.Filter.Common.Utilities;
using VSS.Productivity3D.Filter.Common.Utilities.AutoMapper;
using VSS.Productivity3D.Productivity3D.Abstractions.Interfaces;
using VSS.Productivity3D.Project.Abstractions.Interfaces;

namespace VSS.Productivity3D.Filter.Common.Executors
{
  public class GetFiltersExecutor : RequestExecutorContainer
  {
    /// <summary>
    /// This constructor allows us to mock raptorClient
    /// </summary>
    public GetFiltersExecutor(IConfigurationStore configStore, ILoggerFactory logger,
      IServiceExceptionHandler serviceExceptionHandler,
      IProjectProxy projectProxy,
      IProductivity3dV2ProxyNotification productivity3dV2ProxyNotification, IProductivity3dV2ProxyCompaction productivity3dV2ProxyCompaction,
      IFileImportProxy fileImportProxy,
      RepositoryBase repository)
      : base(configStore, logger, serviceExceptionHandler, projectProxy, productivity3dV2ProxyNotification, productivity3dV2ProxyCompaction, fileImportProxy, repository, null, null, null)
    { }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public GetFiltersExecutor()
    { }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Gets all filters for the project.
    /// </summary>
    /// <returns>If successful returns a <see cref="FilterDescriptorListResult"/> containing a collection of filters for the project.</returns>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<FilterRequestFull>(item, 9);
      if (request == null) return null;

      List<MasterData.Repositories.DBModels.Filter> filters = null;

      // get all for ProjectUid where !deleted 
      //   must be ok for 
      //      customer /project
      //      and UserUid: If the calling context is == Application, then get all 
      //                     else get only those for the calling UserUid
      try
      {
        if (request.IsApplicationContext)
        {
          filters = (List<MasterData.Repositories.DBModels.Filter>)await ((IFilterRepository)this.Repository)
          .GetFiltersForProject(request.ProjectUid)
          .ConfigureAwait(false);
        }
        else
        {
          filters = (List<MasterData.Repositories.DBModels.Filter>)await ((IFilterRepository)this.Repository)
          .GetFiltersForProjectUser(request.CustomerUid, request.ProjectUid, request.UserId)
          .ConfigureAwait(false);
        }
      }
      catch (Exception e)
      {
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 10, e.Message);
      }

      await FilterJsonHelper.ParseFilterJson(request.ProjectData, filters, Productivity3dV2ProxyCompaction, request.CustomHeaders);

      // may be none, return success and empty list
      return new FilterDescriptorListResult
      {
        FilterDescriptors = filters?
          .Select(filter => AutoMapperUtility.Automapper.Map<FilterDescriptor>(filter))
          .ToImmutableList()
      };
    }
  }
}
