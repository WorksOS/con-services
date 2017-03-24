using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Repositories;
using WebApiModels.Executors;
using WebApiModels.Models;
using WebApiModels.ResultHandling;

namespace WebApi.Controllers
{
  public class NotificationController : Controller
  {
    private readonly IRepositoryFactory factory;    
    private readonly ILogger log;

    public NotificationController(IRepositoryFactory factory, ILogger<NotificationController> logger)
    {
      this.factory = factory;
      this.log = logger;
    }

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
      log.LogDebug("PostTagFileProcessingError: request:{0}", JsonConvert.SerializeObject(request));
      request.Validate();

      return RequestExecutorContainer.Build<TagFileProcessingErrorExecutor>(factory, log).Process(request) as TagFileProcessingErrorResult;
    }
  }
}