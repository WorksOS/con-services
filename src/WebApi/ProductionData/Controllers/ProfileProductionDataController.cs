using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Raptor.Service.Common.Filters.Authentication;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.WebApiModels.ProductionData.Contracts;
using VSS.Raptor.Service.WebApiModels.ProductionData.Executors;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;


namespace VSS.Raptor.Service.WebApi.ProductionData.Controllers
{
    /// <summary>
    /// Controller for the ProfileProductionData resource.
    /// </summary>
    /// 
    public class ProfileProductionDataController : Controller, IProfileProductionDataContract
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
    public ProfileProductionDataController(IASNodeClient raptorClient, ILoggerFactory logger)
    {
      this.raptorClient = raptorClient;
      this.logger = logger;
      this.log = logger.CreateLogger<ProfileProductionDataController>();
    }


    /// <summary>
    /// Posts a profile production data request to a Raptor's data model/project.
    /// </summary>
    /// <param name="request">Profile production data request structure.></param>
    /// <returns>
    /// Returns JSON structure wtih operation result as profile calculations. {"Code":0,"Message":"User-friendly"}
    /// List of codes:
    ///     OK = 0,
    ///     Incorrect Requested Data = -1,
    ///     Validation Error = -2
    ///     InternalProcessingError = -3;
    ///     FailedToGetResults = -4;
    /// </returns>
    /// <executor>ProfileProductionDataExecutor</executor> 
    [ProjectIdVerifier]
      [NotLandFillProjectVerifier]
      [ProjectUidVerifier]
      [NotLandFillProjectWithUIDVerifier]
      [Route("api/v1/profiles/productiondata")]
      [HttpPost]
      public ProfileResult Post([FromBody]ProfileProductionDataRequest request)
      {
        request.Validate();
        return RequestExecutorContainer.Build<ProfileProductionDataExecutor>(logger, raptorClient, null).Process(request) as ProfileResult;
      }
    }
}
