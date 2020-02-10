using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCSS.TagFileSplitter.Models;
using CCSS.TagFileSplitter.WebAPI.Common.Executors;
using CCSS.TagFileSplitter.WebAPI.Common.Models;
using CCSS.TagFileSplitter.WebAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace CCSS.TagFileSplitter.WebAPI.Controllers
{

  /// <summary>
  /// Tag File Splitter service enables tag files to be shared between VSS and CCSS systems.
  ///
  /// There are 3 sources of tag files:
  /// Auto: From GCS devices via TCC and then TagFileHarvester.
  ///       TFH will hit this TFSS,
  ///       which will call the endpoint for each configured target service,
  ///       returns a response which includes the responses from each target.
  ///           this allows TFH to archive in locations specific to the target
  /// Direct: From EC5nn and TMC devices
  ///       These will now hit the TFSS service
  ///       which will call the endpoint for each configured target service,
  ///       returning the most negative response to the device
  ///       tag file will be archived in S3 using locations specific to the target
  ///             note: will no longer be archived in TCC
  /// Manual: From the VSS or CCSS UI
  ///        UI will go directly to the appropriate 3dpm service, skipping TFSS
  ///        other systems are not interested in these, and they are archived only by the user.
  /// </summary>
  public class TagFileSplitterV1Controller : TagFileSplitterBaseController<TagFileSplitterV1Controller>
  {
    private readonly TargetServices _targetServices = new TargetServices();
    private readonly int? _timeoutSeconds = null;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TagFileSplitterV1Controller(IConfigurationStore configStore)
      : base(configStore)
    {
      _targetServices.SetServices(ConfigStore.GetValueString("TAGFILE_TARGET_SERVICES", string.Empty));
      _targetServices.Validate();

      var configuredTimeoutSeconds = ConfigStore.GetValueInt("TAGFILE_AUTO_TIMEOUT_SECONDS");
      if (configuredTimeoutSeconds != Int32.MinValue)
        _timeoutSeconds = configuredTimeoutSeconds;
    }


    /// <summary>
    /// For accepting and loading tag files from tagFileHarvester
    ///     These need to be applied to both VSS and CCSS services and response from both returned to TFH.
    /// </summary>
    [Route("api/v2/tagfiles/auto")]
    [HttpPost]
    public async Task<IActionResult> SplitAutoSubmission([FromBody] CompactionTagFileRequest request)
    {
      var serializedRequest = JsonUtilities.SerializeObjectIgnoringProperties(request, "Data");
      Logger.LogDebug($"{nameof(SplitAutoSubmission)}: request {serializedRequest}");
      request.Validate();
      if (!_targetServices.Services.Exists(r => r.ServiceName == ServiceNameConstants.PRODUCTIVITY3D_VSS_SERVICE))
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Auto submission requires a VSS target service as this determines archiving"));

      var response = await RequestExecutorContainerFactory
        .Build<AutoSubmissionExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
          ServiceResolution, GenericHttpProxy, CustomHeaders, _targetServices, _timeoutSeconds)
        .ProcessAsync(request) as TagFileSplitterAutoResponse;

      Logger.LogDebug($":{nameof(SplitAutoSubmission)} response: {response}");
      return Ok(response);
    }


    /// <summary>
    /// For the direct submission of tag files from GNSS capable machines.
    ///   TMC device requests (identified by applicationName), will be sent to CCSS only
    /// </summary>
    [Route("api/v2/tagfiles/direct")]
    [HttpPost]
    public async Task<ObjectResult> SplitTagFileDirectSubmission([FromBody] CompactionTagFileRequest request)
    {
      var serializedRequest = JsonUtilities.SerializeObjectIgnoringProperties(request, "Data");
      Logger.LogDebug($"{nameof(SplitTagFileDirectSubmission)}: request {serializedRequest}, userEmailAdress: {UserEmailAddress}");
      request.Validate();

      var response = await RequestExecutorContainerFactory
        .Build<DirectSubmissionExecutor>(LoggerFactory, ConfigStore, ServiceExceptionHandler,
          ServiceResolution, GenericHttpProxy, CustomHeaders, _targetServices, _timeoutSeconds, UserEmailAddress)
        .ProcessAsync(request) as TagFileSplitterAutoResponse;
      Logger.LogDebug($":{nameof(SplitTagFileDirectSubmission)} response: {response}");

      return response.Code == 0
        ? StatusCode((int) HttpStatusCode.OK, response)
        : StatusCode((int) HttpStatusCode.BadRequest, response);
    }
  }
}
