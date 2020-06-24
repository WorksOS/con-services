using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.AWS.TransferProxy;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Projects;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Executors.Project;
using VSS.TRex.Gateway.Common.Helpers;

namespace VSS.TRex.Mutable.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller to manage projects (eg: rebuild).
  ///     HttpGet endpoints use the immutable endpoint (at present VSS.TRex.Gateway.WebApi)
  ///     If ProjectUid doesn't exist then it gets created
  /// </summary>
  public class ProjectController : BaseController
  {
    /// <inheritdoc />
    public ProjectController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<ProjectController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Rebuilds a project from the TAG files present in the processed TAG files archive
    /// </summary>
    [Route("api/v1/project/rebuild")]
    [HttpPost]
    public ContractExecutionResult RebuildProject([FromBody] ProjectRebuildRequest rebuildRequest)
    {
      Log.LogInformation($"{nameof(RebuildProject)}: {JsonConvert.SerializeObject(rebuildRequest)}");
      rebuildRequest.Validate();
      GatewayHelper.ValidateAndGetSiteModel(nameof(RebuildProject), rebuildRequest.ProjectUid, true);

      if (rebuildRequest.DataOrigin == TransferProxyType.TAGFiles)
      {
        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<ProjectRebuildExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(rebuildRequest));
      }

      throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Data origin must be ''TAGFiles'' (enum TransferProxyType.TAGFiles)"));
    }
  }
}
