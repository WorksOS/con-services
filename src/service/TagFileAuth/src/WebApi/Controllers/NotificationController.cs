using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.Models.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Notification controller for tag file errors and misc alarms
  /// </summary>
  public class NotificationController : BaseController<NotificationController>
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public NotificationController()
    { }

    /// <summary>
    /// Writes to the log for the given tag file processing error. 
    /// </summary>
    /// <param name="request">Details of the error including the asset id, the tag file and the type of error</param>
    /// <returns>
    /// True for success and false for failure.
    /// </returns>
    /// <executor>TagFileProcessingErrorV1Executor</executor>
    [Obsolete("obsolete", false)]
    [Route("api/v1/notification/tagFileProcessingError")]  // for Raptor, soon obsolete.
    [HttpPost]
    public TagFileProcessingErrorResult PostTagFileProcessingError([FromBody] TagFileProcessingErrorV1Request request)
    {
      Logger.LogDebug("PostTagFileProcessingErrorV1: request:{0}", JsonConvert.SerializeObject(request));
      request.Validate();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(Logger, ConfigStore, Authorization, CwsAccountClient, ProjectProxy, DeviceProxy, RequestCustomHeaders)
        .Process(request) as TagFileProcessingErrorResult;

      Logger.LogDebug("PostTagFileProcessingErrorV1: result:{0}", JsonConvert.SerializeObject(result));
      return result;
    }


    /// <summary>
    /// Writes the given tag file processing error to the log. 
    /// </summary>
    /// <param name="request">Details of the error including the customerUid, the tag file and the type of error</param>
    /// <returns>
    /// True for success and false for failure.
    ///   20180116 Raptor has inadvertently been ported to use v2 which isn't supported.
    ///   v2 has been changes to implement v1 for the time being as it is the path of least resistance.
    /// </returns>
    /// <executor>TagFileProcessingErrorV2Executor</executor>
    [Route("api/v2/notification/tagFileProcessingError")] // for Raptor, soon obsolete.
    [HttpPost]
    public TagFileProcessingErrorResult PostTagFileProcessingError([FromBody] TagFileProcessingErrorV2Request request)
    {
      Logger.LogDebug("PostTagFileProcessingErrorV2: request:{0}", JsonConvert.SerializeObject(request));

      if (!request.assetId.HasValue || request.assetId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 9));
      }

      var v1Request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(request.assetId.Value, request.tagFileName,
          (int)request.error);
      Logger.LogDebug("PostTagFileProcessingErrorV2: v1Request:{0}", JsonConvert.SerializeObject(v1Request));
      v1Request.Validate();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(Logger, ConfigStore, Authorization, CwsAccountClient, ProjectProxy, DeviceProxy, RequestCustomHeaders)
        .Process(v1Request) as TagFileProcessingErrorResult;

      Logger.LogDebug("PostTagFileProcessingErrorV2: v1result:{0}", JsonConvert.SerializeObject(result));
      return result;
    }

    /// <summary>
    /// Posts the application alarm.
    /// </summary>
    [Route("api/v1/notification/appAlarm")]  // for Raptor, soon obsolete.
    [HttpPost]
    public ContractExecutionResult PostAppAlarm([FromBody] AppAlarmMessage request)
    {
      Logger.LogWarning("PostAppAlarm: request:{0}", JsonConvert.SerializeObject(request));
      request.Validate();
      return new ContractExecutionResult();
    }
  }
}
