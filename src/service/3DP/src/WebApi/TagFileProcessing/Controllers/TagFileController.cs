using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Clients.CWS.Enums;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Authentication.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Compaction.Utilities;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Executors;
using VSS.Productivity3D.WebApi.Models.TagfileProcessing.Models;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.TagFileProcessing.Controllers
{
  /// <summary>
  /// For handling manual import of Tag files from the UI.
  ///    Tag files from other sources e.g. GCS900 (TCC) and Direct (earthworks or Marine) go via SQS and the TagFileGateway service
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class TagFileController : Controller
  {
    private readonly ILogger _log;
    private readonly ILoggerFactory _logger;
    private readonly IConfigurationStore _configStore;
    private readonly ITRexTagFileProxy _tRexTagFileProxy;
    private IHeaderDictionary CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TagFileController(
      ILoggerFactory logger, ITRexTagFileProxy tRexTagFileProxy, IConfigurationStore configStore)
    {
      _logger = logger;
      _log = logger.CreateLogger<TagFileController>();
      _tRexTagFileProxy = tRexTagFileProxy;
      _configStore = configStore;
    }

    /// <summary>
    /// For accepting and loading manually submitted tag files.
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v2/tagfiles")]
    [HttpPost]
    public async Task<IActionResult> PostTagFileManualImport([FromBody] CompactionTagFileRequest request)
    {
      var serializedRequest = JsonUtilities.SerializeObjectIgnoringProperties(request, "Data");
      _log.LogDebug($"{nameof(PostTagFileManualImport)}: request {serializedRequest}");

      await ValidateRequest(request.ProjectUid);

      //Now submit tag file to TRex
      var requestExt = CompactionTagFileRequestExtended.CreateCompactionTagFileRequestExtended(request, null);

      var responseObj = await RequestExecutorContainerFactory
        .Build<TagFileNonDirectSubmissionExecutor>(_logger,
          _configStore, tRexTagFileProxy: _tRexTagFileProxy, customHeaders: CustomHeaders).ProcessAsync(requestExt);

      return responseObj.Code == 0
        ? (IActionResult) Ok(responseObj)
        : BadRequest(responseObj);
    }


    private async Task ValidateRequest(Guid? projectUid)
    {
      if (projectUid == null || projectUid.Value == Guid.Empty)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Request must include the ProjectUid."));

      //Check it's a 3dp project
      var projectData = await ((RaptorPrincipal) User).GetProject(projectUid.Value);
      if (projectData != null && projectData.ProjectType.HasFlag(CwsProjectType.AcceptsTagFiles) == false)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "The project is standard and does not accept tag files."));

      if (projectData?.IsArchived == true)
        throw new ServiceException(HttpStatusCode.BadRequest,

          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "The project has been archived and this function is not allowed."));

      await GetProjectBoundary(projectUid.Value);
    }

    private async Task<WGS84Fence> GetProjectBoundary(Guid projectUid)
    {
      var projectData = await ((RaptorPrincipal) User).GetProject(projectUid);
      var result = GeofenceValidation.ValidateWKT(projectData.ProjectGeofenceWKT);
      if (string.CompareOrdinal(result, GeofenceValidation.ValidationOk) != 0)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            $"{nameof(GetProjectBoundary)}: The project has an invalid boundary ({result})."));

      return new WGS84Fence(CommonConverters.GeometryToPoints(projectData.ProjectGeofenceWKT).ToArray());
    }
  }
}
