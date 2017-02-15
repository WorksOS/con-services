using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Web.Http;
using VSS.TagFileAuth.Service.Executors;
using VSS.TagFileAuth.Service.Interfaces;
using VSS.TagFileAuth.Service.Models.RaptorServicesCommon;
using VSS.TagFileAuth.Service.ResultHandling;
using VSS.TagFileAuth.Service.WebApi.Interfaces;

namespace VSS.TagFileAuth.Service.Controllers
{
  public class NotificationController : ApiController
  {
    /// <summary>
    /// Repository factory for use by executor
    /// </summary>
    private readonly IRepositoryFactory factory;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger logger;


    /// <summary>
    /// Constructor with injected repository factory and logger
    /// </summary>
    /// <param name="factory">Repository factory</param>
    /// <param name="logger">Logger</param>
    public NotificationController(IRepositoryFactory factory, ILogger<NotificationController> logger)
    {
      this.factory = factory;
      this.logger = logger;
    }


    /// <summary>
    /// Raises an application alarm. 
    /// </summary>
    /// <param name="request">Details of the alarm including the level, message and exception</param>
    /// <returns>
    /// True for success and false for failure.
    /// </returns>
    /// <executor>AppAlarmExecutor</executor>
    [Route("api/v1/notification/appAlarm")]
    public AppAlarmResult PostAppAlarm([FromBody]AppAlarmRequest request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<AppAlarmExecutor>(factory).Process(request) as AppAlarmResult;
    }

    /// <summary>
    /// Sends an alert if required for the given tag file processing error. 
    /// </summary>
    /// <param name="request">Details of the error including the asset id, the tag file and the type of error</param>
    /// <returns>
    /// True for success and false for failure.
    /// </returns>
    /// <executor>TagFileProcessingErrorExecutor</executor>
    [Route("api/v1/notification/tagFileProcessingError")]
    public TagFileProcessingErrorResult PostTagFileProcessingError([FromBody]TagFileProcessingErrorRequest request)
    {
      request.Validate();
      return RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory).Process(request) as TagFileProcessingErrorResult;
    }
  }
}