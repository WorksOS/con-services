using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Repositories;
using VSS.TagFileAuth.Service.WebApiModels.Executors;
using VSS.TagFileAuth.Service.WebApiModels.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.WebApiModels.ResultHandling;

namespace VSS.TagFileAuth.Service.Controllers
{
  public class NotificationController : Controller
  {
    /// <summary>
    /// Repository factory for use by executor
    /// </summary>
    private readonly IRepositoryFactory factory;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;


    /// <summary>
    /// Constructor with injected repository factory and logger
    /// </summary>
    /// <param name="factory">Repository factory</param>
    /// <param name="logger">Logger</param>
    public NotificationController(IRepositoryFactory factory, ILogger<NotificationController> logger)
    {
      this.factory = factory;
      this.log = logger;
    }


    ///// <summary>
    ///// Raises an application alarm. Obsolete
    ///// </summary>
    ///// <param name="request">Details of the alarm including the level, message and exception</param>
    ///// <returns>
    ///// True for success and false for failure.
    ///// </returns>
    ///// <executor>AppAlarmExecutor</executor>
    //[Route("api/v1/notification/appAlarm")]
    //public AppAlarmResult PostAppAlarm([FromBody]AppAlarmRequest request)
    //{
    //  request.Validate();
    //  return RequestExecutorContainer.Build<AppAlarmExecutor>(factory).Process(request) as AppAlarmResult;
    //}

    /// <summary>
    /// Writes to the log for the given tag file processing error. 
    /// </summary>
    /// <param name="request">Details of the error including the asset id, the tag file and the type of error</param>
    /// <returns>
    /// True for success and false for failure.
    /// </returns>
    /// <executor>TagFileProcessingErrorExecutor</executor>
    [Route("api/v1/notification/tagFileProcessingError")]
    [HttpPost]
    public TagFileProcessingErrorResult PostTagFileProcessingError([FromBody]TagFileProcessingErrorRequest request)
    {
      log.LogInformation("PostTagFileProcessingError: {0}", Request.QueryString);
      
      request.Validate();
      var result = RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, log).Process(request) as TagFileProcessingErrorResult;
      
      if (result.result) 
      {
        var infoMessage = string.Format("TAG file was processed successfully. File name: {0}, asset ID: {1}", request.tagFileName, request.assetId);
        log.LogInformation(infoMessage);
      }
      else
      {
        var errorMessage = string.Format("TAG file failed to be processed. File name: {0}, asset ID: {1}, error: {2}", request.tagFileName, request.assetId, request.error);
        log.LogError(errorMessage);
      }
      
      return result;
    }
  }
}