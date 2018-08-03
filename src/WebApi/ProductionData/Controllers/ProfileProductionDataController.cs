using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.Contracts;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Controller for the ProfileProductionData resource.
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class ProfileProductionDataController : Controller, IProfileProductionDataContract
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
    /// Default constructor.
    /// </summary>
    public ProfileProductionDataController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
    }

    /// <summary>
    /// Posts a profile production data request to a Raptor's data model/project.
    /// </summary>
    /// <param name="request">Profile production data request structure.></param>
    /// <returns>
    /// Returns JSON structure wtih operation result as profile calculations./>
    /// </returns>
    [PostRequestVerifier]
    [ProjectVerifier]
    [Route("api/v1/profiles/productiondata")]
    [HttpPost]
    public ProfileResult Post([FromBody] ProfileProductionDataRequest request)
    {
      request.Validate();

      return RequestExecutorContainerFactory
        .Build<ProfileProductionDataExecutor>(logger, raptorClient)
        .Process(request) as ProfileResult;
    }
  }
}
