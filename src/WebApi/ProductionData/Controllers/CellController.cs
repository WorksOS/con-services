using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.Executors;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// CellController
  /// </summary>
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CellController : Controller
  {
    /// <summary>
    /// Raptor client for use by executor
    /// </summary>
    private readonly IASNodeClient raptorClient;

    /// <summary>
    /// Logger factory for use by executor
    /// </summary>
    private readonly ILoggerFactory logger;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="raptorClient">Raptor client</param>
    /// <param name="logger">Logger</param>
    public CellController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
    }

    // POST: api/Cells
    /// <summary>
    /// Retrieve passes for a single cell and process them according to the provided filter and layer analysis parameters
    /// </summary>
    /// <param name="request">The request representation for the operation</param>
    /// <returns>A representation of the cell that contains summary information relative to the cell as a whole, a collection of layers derived from layer analysis and the collection of cell passes that met the filter conditions.</returns>
    /// <executor>CellPassesExecutor</executor> 
    /// 
    [PostRequestVerifier]
    [ProjectIdVerifier]
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [Route("api/v1/productiondata/cells/passes")]
    [HttpPost]
    public CellPassesResult Post([FromBody]CellPassesRequest request)
    {
      return RequestExecutorContainerFactory.Build<CellPassesExecutor>(logger, raptorClient).Process(request) as CellPassesResult;
    }

    // POST: api/Cells
    /// <summary>
    /// Requests a single thematic datum value from a single cell. Examples are elevation, compaction. temperature etc. The request body contains all necessary parameters.
    /// The cell may be identified by either WGS84 lat/long coordinates or by project grid coordinates.
    /// </summary>
    /// <param name="request">The request body parameters for the request.</param>
    /// <returns>The requested thematic value expressed as a floating point number. Interpretation is dependant on the thematic domain.</returns>
    /// <executor>CellDatumExecutor</executor> 
    [PostRequestVerifier]
    [ProjectIdVerifier(AllowLandfillProjects = true)]
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [Route("api/v1/productiondata/cells/datum")]
    [HttpPost]
    public CellDatumResponse Post([FromBody]CellDatumRequest request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<CellDatumExecutor>(logger, raptorClient).Process(request) as CellDatumResponse;
    }

    // POST: api/Patches
    /// <summary>
    /// Requests cell passes information in patches (raw Raptor data output)
    /// </summary>
    /// <param name="request">The request body.</param>
    [PostRequestVerifier]
    [ProjectIdVerifier(AllowLandfillProjects = true)]
    [ProjectUidVerifier(AllowLandfillProjects = true)]
    [Route("api/v1/productiondata/patches")]
    [HttpPost]
    public ContractExecutionResult Post([FromBody]PatchRequest request)
    {
      request.Validate();
      return RequestExecutorContainerFactory.Build<PatchExecutor>(logger, raptorClient).Process(request);
    }

  }
}
