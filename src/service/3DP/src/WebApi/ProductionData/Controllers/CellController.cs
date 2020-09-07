using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Filters.Authentication;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors.CellPass;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.ProductionData.Controllers
{
  /// <summary>
  /// Cell and cell patches controller.
  /// </summary>
  [ProjectVerifier]
  [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
  public class CellController : ProductionDataBaseController<CellController>
  {

    /// Called by TBC only
    /// <summary>
    /// Retrieve passes for a single cell and process them according to the provided filter and layer analysis parameters
    /// </summary>
    /// <param name="request">The request representation for the operation</param>
    /// <returns>A representation of the cell that contains summary information relative to the cell as a whole, a collection of layers derived from layer analysis and the collection of cell passes that met the filter conditions.</returns>
    /// <executor>CellPassesExecutor</executor>
    [PostRequestVerifier]
    [HttpPost("api/v1/productiondata/cells/passes")]
    public async Task<CellPassesResult> CellPassesTbc([FromBody] CellPassesRequest request)
    {
      request.Validate();

      return await RequestExecutorContainerFactory.Build<CellPassesExecutor>(
        LoggerFactory,
        configStore: ConfigStore,
        trexCompactionDataProxy: TRexCompactionDataProxy, 
        fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId()
        ).ProcessAsync(request) as CellPassesResult;
    }

    /// Called by TBC only
    /// <summary>
    /// Requests a single thematic datum value from a single cell. Examples are elevation, compaction. temperature etc. The request body contains all necessary parameters.
    /// The cell may be identified by either WGS84 lat/long coordinates or by project grid coordinates.
    /// </summary>
    /// <param name="request">The request body parameters for the request.</param>
    /// <returns>The requested thematic value expressed as a floating point number. Interpretation is dependant on the thematic domain.</returns>
    [PostRequestVerifier]
    [HttpPost("api/v1/productiondata/cells/datum")]
    public async Task<CellDatumResult> PostCellDatumTbc([FromBody] CellDatumRequest request)
    {
      request.Validate();

      return await RequestExecutorContainerFactory.Build<CellDatumExecutor>(
        LoggerFactory,
        configStore: ConfigStore,
        trexCompactionDataProxy: TRexCompactionDataProxy,
        fileImportProxy: FileImportProxy, customHeaders: CustomHeaders, userId: GetUserId()
        ).ProcessAsync(request) as CellDatumResult;
    }

    /// Called by TBC only
    /// <summary>
    /// Requests cell passes information in patches (raw Raptor data output)
    /// </summary>
    [PostRequestVerifier]
    [HttpPost("api/v1/productiondata/patches")]
    public async Task<ContractExecutionResult> PostProductionDataPatchesTbc([FromBody] PatchRequest request)
    {
      request.Validate();

      return await RequestExecutorContainerFactory
        .Build<PatchExecutor>(LoggerFactory,
          ConfigStore, trexCompactionDataProxy: TRexCompactionDataProxy, customHeaders: CustomHeaders)
        .ProcessAsync(request);
    }
  }
}
