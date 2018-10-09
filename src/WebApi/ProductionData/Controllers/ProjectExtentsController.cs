using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Contracts;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// 
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class ProjectExtentsController : IProjectExtentsContract
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
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    protected IConfigurationStore configStore;

    /// <summary>
    /// For requesting data from TRex database.
    /// </summary>
    protected ITRexCompactionDataProxy trexCompactionDataProxy;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">LoggerFactory</param>
    /// <param name="configStore">Configuration Store</param>
    /// <param name="trexCompactionDataProxy">Trex Gateway production data proxy</param>
    public ProjectExtentsController(IASNodeClient raptorClient, ILoggerFactory logger, IConfigurationStore configStore, ITRexCompactionDataProxy trexCompactionDataProxy)
    {
      this.raptorClient = raptorClient;
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
    /// 
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/projectextents")]
    [HttpPost]

    public ProjectExtentsResult Post([FromBody] ExtentRequest request)
    {
      return RequestExecutorContainerFactory.Build<ProjectExtentsSubmitter>(logger, raptorClient, configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy).Process(request) as ProjectExtentsResult;
    }
  }
}
