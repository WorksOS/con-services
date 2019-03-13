using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Common.Executors;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Cell and cell patches controller.
  /// </summary>
  [ProjectVerifier]
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CellController
  {
#if RAPTOR
    private readonly IASNodeClient raptorClient;
#endif
    private readonly ILoggerFactory logger;
    private readonly IConfigurationStore configStore;
    private readonly ITRexCompactionDataProxy trexCompactionDataProxy;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public CellController(
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
    /// Retrieve passes for a single cell and process them according to the provided filter and layer analysis parameters
    /// </summary>
    /// <param name="request">The request representation for the operation</param>
    /// <returns>A representation of the cell that contains summary information relative to the cell as a whole, a collection of layers derived from layer analysis and the collection of cell passes that met the filter conditions.</returns>
    /// <executor>CellPassesExecutor</executor>
    [PostRequestVerifier]
    [Route("api/v1/productiondata/cells/passes")]
    [HttpPost]
    public CellPassesResult Post([FromBody]CellPassesRequest request)
    {
#if RAPTOR
      return RequestExecutorContainerFactory.Build<CellPassesExecutor>(logger, raptorClient).Process(request) as CellPassesResult;
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    /// <summary>
    /// Requests a single thematic datum value from a single cell. Examples are elevation, compaction. temperature etc. The request body contains all necessary parameters.
    /// The cell may be identified by either WGS84 lat/long coordinates or by project grid coordinates.
    /// </summary>
    /// <param name="request">The request body parameters for the request.</param>
    /// <returns>The requested thematic value expressed as a floating point number. Interpretation is dependant on the thematic domain.</returns>
    [PostRequestVerifier]
    [Route("api/v1/productiondata/cells/datum")]
    [HttpPost]
    public async Task<CellDatumResponse> Post([FromBody]CellDatumRequest request)
    {
      request.Validate();
#if RAPTOR
      return await RequestExecutorContainerFactory.Build<CellDatumExecutor>(logger, raptorClient, configStore: configStore, trexCompactionDataProxy: trexCompactionDataProxy).ProcessAsync(request) as CellDatumResponse;
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    /// <summary>
    /// Requests cell passes information in patches (raw Raptor data output)
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v1/productiondata/patches")]
    [HttpPost]
    public ContractExecutionResult Post([FromBody]PatchRequest request)
    {
      request.Validate();
#if RAPTOR
      return RequestExecutorContainerFactory.Build<PatchExecutor>(logger, raptorClient).Process(request);
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }

    /// <summary>
    /// Requests cell passes information in patches but returning co-ordinates relative to the world origin rather than cell origins.
    /// </summary>
    [PostRequestVerifier]
    [Route("api/v1/productiondata/patches/worldorigin")]
    [HttpPost]
    public ContractExecutionResult GetSubGridPatchesAsWorldOrigins([FromBody]PatchRequest request)
    {
      request.Validate();
#if RAPTOR
      return RequestExecutorContainerFactory.Build<CompactionPatchExecutor>(logger, raptorClient).Process(request);
#else
      throw new ServiceException(HttpStatusCode.BadRequest,
        new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));
#endif
    }
  }
}
