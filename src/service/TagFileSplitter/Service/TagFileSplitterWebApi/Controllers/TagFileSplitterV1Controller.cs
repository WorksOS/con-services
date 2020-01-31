using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using CCSS.TagFileSplitter.Models;
using CCSS.TagFileSplitter.WebAPI.Common.Helpers;
using CCSS.TagFileSplitter.WebAPI.Common.Models;
using CCSS.TagFileSplitter.WebAPI.Utilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Abstractions.ServiceDiscovery.Constants;
using VSS.Common.Abstractions.ServiceDiscovery.Enums;
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
    private readonly string _TMCApplicationName;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public TagFileSplitterV1Controller(IConfigurationStore configStore)
      : base(configStore)
    {
      /* todojeannie for jenkins config:
       TAGFILE_TARGET_SERVICES
       TAGFILE_AUTO_TIMEOUT_SECONDS
       TAGFILE_TMC_APPLICATION_NAME -- different for each platform?
       see also appsettings for apiService names. 
       */
      _targetServices.SetServices(ConfigStore.GetValueString("TAGFILE_TARGET_SERVICES", string.Empty));
      _targetServices.Validate();

      var configuredTimeoutSeconds = ConfigStore.GetValueInt("TAGFILE_AUTO_TIMEOUT_SECONDS");
      if (configuredTimeoutSeconds != Int32.MinValue)
        _timeoutSeconds = configuredTimeoutSeconds;

      _TMCApplicationName = ConfigStore.GetValueString("TAGFILE_TMC_APPLICATION_NAME", string.Empty);
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
      
      // setup tasks for each service in _targetServices
      // wait for all targets (within a reasonable response time)
      // return response from all targets, including their applicationIds 
      // TFH will call this endpoint, and process all target responses, archiving to appropriate directories in TCC

      var tasks = new List<Task<TargetServiceResponse>>();
      foreach (var targetService in _targetServices.Services)
        tasks.Add(TargetServiceHelper.SendTagFileTo3dPmService(request, ServiceResolution, GenericHttpProxy, 
          targetService.ServiceName, targetService.TargetApiVersion, targetService.AutoRoute, 
          Logger, CustomHeaders, _timeoutSeconds));
      await Task.WhenAll(tasks);

      var vssResults = tasks.Select(t => t.Result).ToArray();
      if (vssResults == null || vssResults.Count() != _targetServices.Services.Count)
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Incorrect number of result sets gathered. Expected: {_targetServices.Services.Count} got: {vssResults.Count()}"));

      var result = new TagFileSplitterAutoResponse(vssResults.ToList());
      Logger.LogDebug($":{nameof(SplitAutoSubmission)} response: {result}");

      return Ok(result);
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
      Logger.LogDebug($"{nameof(SplitTagFileDirectSubmission)}: request {serializedRequest}, userEmailAdress: {UserEmailAddress}, tmcApplicationName: {_TMCApplicationName}");
      request.Validate();
      
      // setup tasks for appropriate services in _targetServices.
      //     TMC request should go ONLY to Productivity3D service (not VSS)
      // wait for all targets (within a reasonable response time)
      // return response from all targets, including their applicationIds 
      // TFH to be changed to call this, and process all target responses to enable it to archive to appropriate directories in TCC
      var isTmcDevice = string.Compare(UserEmailAddress, _TMCApplicationName, StringComparison.OrdinalIgnoreCase) == 0;
      var tasks = new List<Task<TargetServiceResponse>>();
      foreach (var targetService in _targetServices.Services)
      {
        if (!isTmcDevice || (isTmcDevice && targetService.ServiceName == ServiceNameConstants.PRODUCTIVITY3D_SERVICE))
          tasks.Add(TargetServiceHelper.SendTagFileTo3dPmService(request, ServiceResolution, GenericHttpProxy,
            targetService.ServiceName, targetService.TargetApiVersion, targetService.DirectRoute,
            Logger, CustomHeaders, _timeoutSeconds));
      }
      await Task.WhenAll(tasks);

      var vssResults = tasks.Select(t => t.Result).ToArray();
      if (vssResults == null || (isTmcDevice && vssResults.Length > 1) || (!isTmcDevice && vssResults.Length != _targetServices.Services.Count))
        throw new ServiceException(HttpStatusCode.InternalServerError,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Incorrect number of result sets gathered. Expected: {_targetServices.Services.Count} got: {vssResults.Count()}"));

      // return the first !success response if available
      var result = new ContractExecutionResult();
      var failedResult = vssResults.First(r => r.StatusCode != HttpStatusCode.OK);
      result = failedResult != null 
        ? new ContractExecutionResult(failedResult.Code, failedResult.Message) 
        : new ContractExecutionResult(vssResults[0].Code, vssResults[0].Message);
      Logger.LogDebug($":{nameof(SplitTagFileDirectSubmission)} response: {result} fullResults: {vssResults}");

      return failedResult == null
        ? StatusCode((int) HttpStatusCode.OK, result)
        : StatusCode((int) HttpStatusCode.BadRequest, result);
    }
  }
}
