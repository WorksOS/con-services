using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.TRex.Gateway.Common.Executors;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting production data profiles.
  /// </summary>
  public class ProductionDataProfileController : BaseController
  {
    /// <summary>
    /// Default constructor
    /// </summary>
    public ProductionDataProfileController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<ProductionDataProfileController>(), serviceExceptionHandler, configStore)
    { }

    /// <summary>
    /// Get the summary volumes profile report for two surfaces.
    /// </summary>
    [HttpPost("api/v1/productiondata/profile")]
    public Task<ContractExecutionResult> PostProductionDataProfile([FromBody] ProductionDataProfileDataRequest productionDataProfileRequest)
    {
      Log.LogInformation($"{nameof(PostProductionDataProfile)}: {Request.QueryString}");

      productionDataProfileRequest.Validate();
      ValidateFilterMachines(nameof(PostProductionDataProfile), productionDataProfileRequest.ProjectUid, productionDataProfileRequest.Filter);

      return WithServiceExceptionTryExecuteAsync(() =>
        RequestExecutorContainer
          .Build<ProductionDataProfileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .ProcessAsync(productionDataProfileRequest));
    }
  }
}
