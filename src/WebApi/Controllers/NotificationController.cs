using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Repositories;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Executors;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Models;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.ResultHandling;

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
    /// <param name="logger">Service implementation of ILogger</param>
    /// <param name="assetRepository"></param>
    /// <param name="deviceRepository"></param>
    /// <param name="customerRepository"></param>
    /// <param name="projectRepository"></param>
    /// <param name="subscriptionsRepository"></param>
    public NotificationController(ILogger logger, IAssetRepository assetRepository, IDeviceRepository deviceRepository,
      ICustomerRepository customerRepository, IProjectRepository projectRepository,
      ISubscriptionRepository subscriptionsRepository)
      :base(logger, assetRepository, deviceRepository,
        customerRepository, projectRepository,
        subscriptionsRepository)
    {
      this.log = logger;
    }
    
    /// <summary>
    /// Writes to the log for the given tag file processing errorEnum. 
    /// </summary>
    /// <param name="request">Details of the errorEnum including the asset id, the tag file and the type of errorEnum</param>
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

      var result = RequestExecutorContainer.Build<TagFileProcessingErrorV1Executor>(log, assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository)
        .Process(request) as TagFileProcessingErrorResult;
      
      log.LogDebug("PostTagFileProcessingErrorV2: result:{0}", JsonConvert.SerializeObject(result));
      return result;
    }

    /// <summary>
    /// Writes a Kafka event for the given tag file processing errorEnum. 
    /// </summary>
    /// <param name="request">Details of the errorEnum including the customerUid, the tag file and the type of errorEnum</param>
    /// <returns>
    /// True for success and false for failure.
    /// </returns>
    /// <executor>TagFileProcessingErrorV2Executor</executor>
    [Route("api/v2/notification/tagFileProcessingError")]
    [HttpPost]
    public async Task<TagFileProcessingErrorResult> PostTagFileProcessingError([FromBody] TagFileProcessingErrorV2Request request)
    {
      log.LogDebug("PostTagFileProcessingErrorV2: request:{0}", JsonConvert.SerializeObject(request));
      request.Validate();

      var executor = RequestExecutorContainer.Build<TagFileProcessingErrorV2Executor>(log, assetRepository, deviceRepository, customerRepository, projectRepository, subscriptionsRepository);
      var result = await executor.ProcessAsync(request) as TagFileProcessingErrorResult;

      log.LogDebug("PostTagFileProcessingErrorV2: result:{0}", JsonConvert.SerializeObject(result));
      return result;
    }

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