using System;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Helpers;

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
      if (productionDataProfileRequest.ProjectUid == null || productionDataProfileRequest.ProjectUid == Guid.Empty)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid project UID."));
      }
      var siteModel = GatewayHelper.ValidateAndGetSiteModel(productionDataProfileRequest.ProjectUid.Value, nameof(PostProductionDataProfile));
      if (productionDataProfileRequest.BaseFilter != null && productionDataProfileRequest.BaseFilter.ContributingMachines != null)
        GatewayHelper.ValidateMachines(productionDataProfileRequest.BaseFilter.ContributingMachines.Select(m => m.AssetUid).ToList(), siteModel);


      return WithServiceExceptionTryExecute(() =>
        RequestExecutorContainer
          .Build<ProductionDataProfileExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
          .Process(productionDataProfileRequest) as ProfileDataResult<ProfileCellData>);
    }
  }
}
