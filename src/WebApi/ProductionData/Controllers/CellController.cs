using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.WebApi.Compaction.Controllers;
using VSS.Raptor.Service.WebApiModels.ProductionData.Executors;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;

namespace VSS.Raptor.Service.WebApi.ProductionData.Controllers
{
    /// <summary>
    /// CellController
    /// </summary>
    public class CellController : Controller
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
    public CellController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<CellController>();
    }


    // POST: api/Cells
    /// <summary>
    /// Retrieve passes for a single cell and process them according to the provided filter and layer analysis parameters
    /// </summary>
    /// <param name="request">The request representation for the operation</param>
    /// <returns>A representation of the cell that contains summary information relative to the cell as a whole, a collection of layers derived from layer analysis and the collection of cell passes that met the filter conditions.</returns>
    /// <executor>CellPassesExecutor</executor> 
    [ProjectIdVerifier]
      [NotLandFillProjectVerifier]
      [ProjectUidVerifier]
      [NotLandFillProjectWithUIDVerifier]
      [System.Web.Http.Route("api/v1/productiondata/cells/passes")]
      [System.Web.Http.HttpPost]
      public CellPassesResult Post([System.Web.Http.FromBody]CellPassesRequest request)
      {
          return RequestExecutorContainer.Build<CellPassesExecutor>(logger, raptorClient, null).Process(request) as CellPassesResult;
      }

      // POST: api/Cells
      /// <summary>
      /// Requests a single thematic datum value from a single cell. Examples are elevation, compaction. temperature etc. The request body contains all necessary parameters.
      /// The cell may be identified by either WGS84 lat/long coordinates or by project grid coordinates.
      /// </summary>
      /// <param name="request">The request body parameters for the request.</param>
      /// <returns>The requested thematic value expressed as a floating point number. Interpretation is dependant on the thematic domain.</returns>
      /// <executor>CellDatumExecutor</executor> 
      [ProjectIdVerifier]
      [NotLandFillProjectVerifier]
      [ProjectUidVerifier]
      [NotLandFillProjectWithUIDVerifier]
      [System.Web.Http.Route("api/v1/productiondata/cells/datum")]
      [System.Web.Http.HttpPost]
      public CellDatumResponse Post([System.Web.Http.FromBody]CellDatumRequest request)
      {
          request.Validate();
          return RequestExecutorContainer.Build<CellDatumExecutor>(logger, raptorClient, null).Process(request) as CellDatumResponse;
      }



      // POST: api/Patches
      /// <summary>
      /// Requests cell passes information in patches (raw Raptor data output)
      /// </summary>
      /// <param name="request">The request body.</param>
      /// <returns></returns>
      [ProjectIdVerifier]
      [NotLandFillProjectVerifier]
      [ProjectUidVerifier]
      [NotLandFillProjectWithUIDVerifier]
      [System.Web.Http.Route("api/v1/productiondata/patches")]
      [System.Web.Http.HttpPost]
      public ContractExecutionResult Post([System.Web.Http.FromBody]PatchRequest request)
      {
          request.Validate();
          return RequestExecutorContainer.Build<PatchExecutor>(logger, raptorClient, null).Process(request);
      }

    }
}
