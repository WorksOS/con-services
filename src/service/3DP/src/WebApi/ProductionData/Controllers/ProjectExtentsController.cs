using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Proxies;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.TRex.Gateway.Common.Abstractions;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class ProjectExtentsController : Controller, IProjectExtentsContract
  {
#if RAPTOR
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;
#endif
    /// <summary>
    /// LoggerFactory factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private readonly IConfigurationStore configStore;

    /// <summary>
    /// For requesting data from TRex database.
    /// </summary>
    private readonly ITRexCompactionDataProxy trexCompactionDataProxy;

    /// <summary>
    /// Gets the custom headers for the request.
    /// </summary>
    private IHeaderDictionary CustomHeaders => Request.Headers.GetCustomHeaders();

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="logger">LoggerFactory</param>
    /// <param name="configStore">Configuration Store</param>
    /// <param name="trexCompactionDataProxy">Trex Gateway production data proxy</param>
    public ProjectExtentsController(
#if RAPTOR
      IASNodeClient raptorClient, 
#endif
      ILoggerFactory logger, IConfigurationStore configStore, ITRexCompactionDataProxy trexCompactionDataProxy)
    {
#if RAPTOR
      this.raptorClient = raptorClient;
#endif
      this.logger = logger;
      this.configStore = configStore;
      this.trexCompactionDataProxy = trexCompactionDataProxy;
    }

    /// <summary>
    /// Returns a projects data extents information.
    /// </summary>
    /// <param name="request">Parameters to request project data extents</param>
    /// <returns></returns>
    /// <executor>ProjectExtentsSubmitter</executor>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/projectextents")]
    [HttpPost]

    public async Task<ProjectExtentsResult> Post([FromBody] ExtentRequest request)
    {
      return await RequestExecutorContainerFactory.
        Build<ProjectExtentsSubmitter>(logger,
#if RAPTOR
          raptorClient, 
#endif
          configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy, customHeaders: CustomHeaders)
        .ProcessAsync(request) as ProjectExtentsResult;
    }
  }
}
