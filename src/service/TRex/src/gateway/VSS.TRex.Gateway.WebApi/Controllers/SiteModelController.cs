using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Gateway.Common.Executors;
using VSS.TRex.Gateway.Common.Helpers;
using VSS.TRex.Gateway.WebApi.ActionServices;

namespace VSS.TRex.Gateway.WebApi.Controllers
{
  /// <summary>
  /// Controller for getting site model statistics.
  /// </summary>
  [Route("api/v1/sitemodels")]
  public class SiteModelController : BaseController
  {
    public SiteModelController(ILoggerFactory loggerFactory, IServiceExceptionHandler serviceExceptionHandler, IConfigurationStore configStore)
      : base(loggerFactory, loggerFactory.CreateLogger<SiteModelController>(), serviceExceptionHandler, configStore)
    {
    }

    /// <summary>
    /// Returns project extents for a site model.
    /// </summary>
    /// <param name="siteModelID">Site model identifier.</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/extents")]
    public BoundingBox3DGrid GetExtents(string siteModelID)
    {
      Log.LogInformation($"{nameof(GetExtents)}: siteModelID: {siteModelID}");

      var extents = GatewayHelper.ValidateAndGetSiteModel(nameof(GetExtents), siteModelID).SiteModelExtent;
      if (extents != null)
        return new BoundingBox3DGrid(
          extents.MinX,
          extents.MinY,
          extents.MinZ,
          extents.MaxX,
          extents.MaxY,
          extents.MaxZ
        );

      return null;
    }

    /// <summary>
    /// Returns project statistics for a site model.
    /// </summary>
    /// <param name="projectStatisticsTRexRequest"></param>
    /// <returns></returns>
    [HttpPost("statistics")]
    public ProjectStatisticsResult GetStatistics([FromBody] ProjectStatisticsTRexRequest projectStatisticsTRexRequest)
    {
      Log.LogInformation($"#In# {nameof(GetStatistics)}: projectStatisticsTRexRequest: {JsonConvert.SerializeObject(projectStatisticsTRexRequest)}");

      try
      {
        projectStatisticsTRexRequest.Validate();
        GatewayHelper.ValidateAndGetSiteModel(nameof(GetStatistics), projectStatisticsTRexRequest.ProjectUid);

        return WithServiceExceptionTryExecute(() =>
          RequestExecutorContainer
            .Build<SiteModelStatisticsExecutor>(ConfigStore, LoggerFactory, ServiceExceptionHandler)
            .Process(projectStatisticsTRexRequest) as ProjectStatisticsResult);
      }
      finally
      {
        Log.LogInformation($"#Out# {nameof(GetStatistics)}: projectStatisticsTRexRequest: {JsonConvert.SerializeObject(projectStatisticsTRexRequest)}");
      }
    }

    
    /// <summary>
    /// Returns list of machines which have contributed to a site model.
    /// </summary>
    /// <param name="siteModelID">Site model identifier.</param>
    /// <param name="coordinateServiceUtility"></param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/machines")]
    public async Task<MachineExecutionResult> GetMachines(string siteModelID,
      [FromServices] ICoordinateServiceUtility coordinateServiceUtility)
    {
      Log.LogInformation($"{nameof(GetMachines)}: siteModelID: {siteModelID}");

      var siteModel = GatewayHelper.ValidateAndGetSiteModel(nameof(GetMachines), siteModelID);
      var CSIB = siteModel.CSIB();
      if (string.IsNullOrEmpty(CSIB))
      {
        Log.LogError($"{nameof(GetMachines)}: siteModel has no CSIB");
        throw new ServiceException(System.Net.HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "The SiteModel has no CSIB."));
      }

      var machines = siteModel.Machines.ToList();
      var result = new MachineExecutionResult(new List<MachineStatus>(machines.Count));

      if (machines.Any())
      {
        var resultMachines = machines.Select(machine => AutoMapperUtility.Automapper.Map<MachineStatus>(machine)).ToList();
        var response = await coordinateServiceUtility.PatchLLH(siteModel.CSIB(), resultMachines);
        result.MachineStatuses = resultMachines;

        // todo once corex is implemented, we will have a better idea why patching fails
        //if (response == ContractExecutionStatesEnum.ExecutedSuccessfully)
        //  result.MachineStatuses = resultMachines;
        //else
        //  return (MachineExecutionResult) new ContractExecutionResult(response);
      }

      return result;
    }

    /// <summary>
    /// Returns list of design/machines which have contributed to a site model.
    /// </summary>
    /// <param name="siteModelID">Site model identifier.</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/machinedesigns")]
    public MachineDesignsExecutionResult GetAssetOnDesignPeriods(string siteModelID)
    {
      Log.LogInformation($"{nameof(GetAssetOnDesignPeriods)}: siteModelID: {siteModelID}");

      var siteModel = GatewayHelper.ValidateAndGetSiteModel(nameof(GetAssetOnDesignPeriods), siteModelID);
      return new MachineDesignsExecutionResult(siteModel.GetAssetOnDesignPeriods());
    }

    /// <summary>
    /// Returns list of design layers/machines which have contributed to a site model.
    /// </summary>
    /// <param name="siteModelID">Site model identifier.</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/machinelayers")]
    public AssetOnDesignLayerPeriodsExecutionResult GetMachineLayers(string siteModelID)
    {
      Log.LogInformation($"{nameof(GetMachineLayers)}: siteModelID: {siteModelID}");

      var siteModel = GatewayHelper.ValidateAndGetSiteModel(nameof(GetMachineLayers), siteModelID);
      return new AssetOnDesignLayerPeriodsExecutionResult(siteModel.GetAssetOnDesignLayerPeriods());
    }

  }
}
