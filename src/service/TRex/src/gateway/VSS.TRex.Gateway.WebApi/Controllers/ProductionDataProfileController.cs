using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
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
    /// <param name="loggerFactory"></param>
    /// <param name="serviceExceptionHandler"></param>
    /// <param name="configStore"></param>
    public ProductionDataProfileController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore) 
      : base(loggerFactory, loggerFactory.CreateLogger<ProductionDataProfileController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Get the summary volumes profile report for two surfaces.
    /// </summary>
    /// <param name="productionDataProfileRequest"></param>
    /// <returns></returns>
    [Route("api/v1/productiondata/profile")]
    [HttpPost]
    public ProfileDataResult<ProfileCellData> PostProductionDataProfile([FromBody] ProductionDataProfileDataRequest productionDataProfileRequest)
    {
      Log.LogInformation($"{nameof(PostProductionDataProfile)}: {Request.QueryString}");

      productionDataProfileRequest.Validate();
      ValidateFilterMachines(nameof(PostProductionDataProfile), productionDataProfileRequest.ProjectUid, productionDataProfileRequest.BaseFilter);

      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<ProductionDataProfileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(productionDataProfileRequest) as ProfileDataResult<ProfileCellData>);
    }
  }
}
