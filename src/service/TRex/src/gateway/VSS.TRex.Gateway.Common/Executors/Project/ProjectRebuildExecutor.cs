using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Projects;
using VSS.TRex.DI;
using VSS.TRex.SiteModels.Interfaces.Executors;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors.Project
{
  public class ProjectRebuildExecutor : BaseExecutor
  {
    /// <summary>
    /// TagFileExecutor
    /// </summary>
    public ProjectRebuildExecutor(IConfigurationStore configStore,
        ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configStore, logger, exceptionHandler)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ProjectRebuildExecutor()
    {
    }

    /// <summary>
    /// Process rebuild project request
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectRebuildRequest>(item);

      try
      {
        log.LogInformation($"#In#: ProjectRebuildExecutor. Project:{request.ProjectUid}, Archive Tag Files: {request.ArchiveTagFiles}, Data Origin:{request.DataOrigin}");

        var rebuildResult = DIContext.Obtain<ISiteModelRebuilderManager>().Rebuild(request.ProjectUid, request.ArchiveTagFiles, request.DataOrigin);

        if (!rebuildResult)
        {
          log.LogError($"#Out# ProjectRebuildExecutor. Rebuild request failed for Project:{request.ProjectUid}, Archive Tag Files: {request.ArchiveTagFiles}, Data Origin:{request.DataOrigin}");
          throw CreateServiceException<ProjectRebuildExecutor>
            (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
              RequestErrorStatus.Unknown); // Todo: May want to enrich the return over a simple bool...
        }

        log.LogInformation($"#Out# ProjectRebuildExecutor. Rebuild request for Project:{request.ProjectUid}, Archive Tag Files: {request.ArchiveTagFiles}, Data Origin:{request.DataOrigin}");
      }
      catch (Exception e)
      {
        log.LogError(e, $"#Out# ProjectRebuildExecutor. Rebuild request failed for Project:{request.ProjectUid}, Archive Tag Files: {request.ArchiveTagFiles}, Data Origin:{request.DataOrigin}");
        serviceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, (int)RequestErrorStatus.Unknown, e.Message); // Todo: Enrich return enum from rebuild request
        throw CreateServiceException<ProjectRebuildExecutor>
          (HttpStatusCode.InternalServerError, ContractExecutionStatesEnum.InternalProcessingError,
            RequestErrorStatus.Unknown, e.Message);
      }

      return new ContractExecutionResult();
    }

    /// <summary>
    /// Processes the request asynchronously.
    /// </summary>
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      throw new NotImplementedException();
    }
  }
}
