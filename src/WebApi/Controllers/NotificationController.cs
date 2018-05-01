using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.ConfigurationStore;
using VSS.KafkaConsumer.Kafka;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using ContractExecutionStatesEnum = VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling.ContractExecutionStatesEnum;

namespace VSS.Productivity3D.TagFileAuth.WebAPI.Controllers
{
  /// <summary>
  /// Notification controller for tag file errors and misc alarms
  /// </summary>
  public class NotificationController : BaseController
  {
    private readonly ILogger log;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public NotificationController(ILoggerFactory logger, IConfigurationStore configStore, 
      IRepository<IAssetEvent> assetRepository, IRepository<IDeviceEvent> deviceRepository,
      IRepository<ICustomerEvent> customerRepository, IRepository<IProjectEvent> projectRepository,
      IRepository<ISubscriptionEvent> subscriptionsRepository, IKafka producer)
      :base(logger, configStore, assetRepository, deviceRepository,
        customerRepository, projectRepository,
        subscriptionsRepository, producer)
    {
      log = logger.CreateLogger<NotificationController>();
    }
    
    /// <summary>
    /// Writes to the log for the given tag file processing error. 
    /// </summary>
    /// <param name="request">Details of the error including the asset id, the tag file and the type of error</param>
    /// <returns>
    /// True for success and false for failure.
    /// </returns>
    /// <executor>TagFileProcessingErrorV1Executor</executor>
    [Route("api/v1/notification/tagFileProcessingError")]
    [HttpPost]
    public TagFileProcessingErrorResult PostTagFileProcessingError([FromBody] TagFileProcessingErrorV1Request request)
    {
      log.LogDebug("PostTagFileProcessingErrorV1: request:{0}", JsonConvert.SerializeObject(request));
      request.Validate();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(log, configStore, assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository)
        .Process(request) as TagFileProcessingErrorResult;
      
      log.LogDebug("PostTagFileProcessingErrorV1: result:{0}", JsonConvert.SerializeObject(result));
      return result;
    }


    /// <summary>
    /// Writes a Kafka event for the given tag file processing error. 
    /// </summary>
    /// <param name="request">Details of the error including the customerUid, the tag file and the type of error</param>
    /// <returns>
    /// True for success and false for failure.
    ///   20180116 Raptor has inadvertantly been ported to use v2 which isn't supported.
    ///   v2 has been changes to implement v1 for the time being as it is the path of least resistance.
    /// </returns>
    /// <executor>TagFileProcessingErrorV2Executor</executor>
    [Route("api/v2/notification/tagFileProcessingError")]
    [HttpPost]
    public TagFileProcessingErrorResult PostTagFileProcessingError([FromBody] TagFileProcessingErrorV2Request request)
    {
      log.LogDebug("PostTagFileProcessingErrorV2: request:{0}", JsonConvert.SerializeObject(request));

      if (!request.assetId.HasValue || request.assetId <= 0)
      {
        throw new ServiceException(System.Net.HttpStatusCode.BadRequest,
          TagFileProcessingErrorResult.CreateTagFileProcessingErrorResult(false,
            ContractExecutionStatesEnum.ValidationError, 9));
      }

      var v1Request = TagFileProcessingErrorV1Request.CreateTagFileProcessingErrorRequest(request.assetId.Value, request.tagFileName,
          (int)request.error);
      log.LogDebug("PostTagFileProcessingErrorV2: v1Request:{0}", JsonConvert.SerializeObject(v1Request));
      v1Request.Validate();

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(log, configStore, assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository)
        .Process(v1Request) as TagFileProcessingErrorResult;

      log.LogDebug("PostTagFileProcessingErrorV2: v1result:{0}", JsonConvert.SerializeObject(result));
      return result;
    }

    /// the following is for AlertNotification. This will not be completed until after the June2018-all-hands-to-the-pump
    /// Please LEAVE!
    /// <summary>
    /// Writes a Kafka event for the given tag file processing error. 
    /// </summary>
    /// <param name="request">Details of the error including the customerUid, the tag file and the type of error</param>
    /// <returns>
    /// True for success and false for failure.
    /// </returns>
    /// <executor>TagFileProcessingErrorV2Executor</executor>
    //[Route("api/v2/notification/tagFileProcessingError")]
    //[HttpPost]
    //public async Task<TagFileProcessingErrorResult> PostTagFileProcessingError([FromBody] TagFileProcessingErrorV2Request request)
    //{
    //  log.LogDebug("PostTagFileProcessingErrorV2: request:{0}", JsonConvert.SerializeObject(request));
    //  request.Validate();

    //  var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(log, configStore, assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository, producer, kafkaTopicName);
    //  var result = await executor.ProcessAsync(request) as TagFileProcessingErrorResult;

    //  log.LogDebug("PostTagFileProcessingErrorV2: result:{0}", JsonConvert.SerializeObject(result));
    //  return result;
    //}

    /// <summary>
    /// Posts the application alarm.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns></returns>
    [Route("api/v1/notification/appAlarm")]
    [HttpPost]
    public ContractExecutionResult PostAppAlarm([FromBody] AppAlarmMessage request)
    {
      log.LogWarning("PostAppAlarm: request:{0}", JsonConvert.SerializeObject(request));
      request.Validate();
      return new ContractExecutionResult();
    }
  }
}
