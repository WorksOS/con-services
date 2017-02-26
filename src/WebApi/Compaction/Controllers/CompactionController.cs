

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Interfaces;


// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace VSS.Raptor.Service.WebApi.Compaction.Controllers
{
    public class CompactionController : Controller
    {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injected raptor client and logger
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    public CompactionController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CompactionController>();
    }

    //Follow this pattern
    /// <summary>
    ///   Gets xxx 
    /// </summary>
    /// <returns>xxxResult</returns>
    /// <executor>xxxExecutor</executor>
    [Route("api/v1/compaction/xxx")]
    [HttpGet]
    public /*xxxResult*/void Getxxx()
    {
      log.LogInformation("Getxxx");

      //1. Set up request model instance
      //2. request.Validate();
      /*
      var result =
          (RequestExecutorContainer.Build<xxxExecutor>(logger, raptorClient, null, null).Process(request) as xxxResult);

      return result;
      */
    }
  }
}
