
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.WebApiModels.Report.Executors;
using VSS.Raptor.Service.WebApiModels.Report.ResultHandling;

namespace VSS.Raptor.Service.WebApi.Report.Controllers
{
  public class ConfigController : Controller
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
    public ConfigController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<ConfigController>();
    }


    /// <summary>
    /// Gets Raptor Configuration in XML
    /// </summary>
    /// <returns>The current Raptor configuration using the XML representation of the Velociraptor.Config.xml Raptor configuration file. All configuration options are included, not just the non-default setting in the actual configuration file.</returns>
    /// <executor>ConfigExecutor</executor>
    [Route("api/v1/configuration")]
    [HttpGet]

    public ConfigResult Get()
    {
      return RequestExecutorContainer.Build<ConfigExecutor>(logger, raptorClient, null).Process((string)null) as ConfigResult;
    }


  }
}
