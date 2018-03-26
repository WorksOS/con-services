using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Report.ResultHandling;
using VSS.Productivity3D.WebApiModels.Report.Executors;

namespace VSS.Productivity3D.WebApi.Report.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class ConfigController : Controller
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">LoggerFactory</param>
    public ConfigController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
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
      return RequestExecutorContainerFactory.Build<ConfigExecutor>(logger, raptorClient).Process(new object()) as ConfigResult;
    }
  }
}