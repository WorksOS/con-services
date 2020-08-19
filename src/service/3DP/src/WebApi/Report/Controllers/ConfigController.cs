using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Report.Contracts;
using VSS.Productivity3D.WebApi.Models.Report.Executors;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.Report.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class ConfigController : Controller, IReportSvc
  {
    private readonly ILoggerFactory _logger;
    private readonly ILogger _log;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore _configStore;

    /// <summary>
    /// The TRex Gateway proxy for use by executor.
    /// </summary>
    private readonly ITRexCompactionDataProxy _tRexCompactionDataProxy;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    private IHeaderDictionary CustomHeaders => Request.Headers.GetCustomHeaders();


    /// <summary>
    /// Default constructor.
    /// </summary>
    public ConfigController(ILoggerFactory logger, IConfigurationStore configStore, ITRexCompactionDataProxy tRexCompactionDataProxy)
    {
      _logger = logger;
      _log = logger.CreateLogger<ConfigController>();
      _configStore = configStore;
      _tRexCompactionDataProxy = tRexCompactionDataProxy;
    }

    /// <summary>
    /// Called by TBC only.
    ///    For now, just call the Trex endpoint which returns 'OK'
    /// </summary>
    [Route("api/v1/configuration")]
    [HttpGet]
    public async Task<ConfigResult> GetConfigTBC()
    {
      _log.LogDebug($"{nameof(GetConfigTBC)}");
      return await RequestExecutorContainerFactory.Build<ConfigExecutor>(_logger,
        _configStore, trexCompactionDataProxy: _tRexCompactionDataProxy, customHeaders: CustomHeaders).ProcessAsync(string.Empty) as ConfigResult;
    }
  }
}
