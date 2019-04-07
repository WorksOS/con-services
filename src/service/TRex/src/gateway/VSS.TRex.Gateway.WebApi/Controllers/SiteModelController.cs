﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.Executors;
using VSS.TRex.Gateway.Common.Converters;
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

      var extents = GatewayHelper.ValidateAndGetSiteModel(siteModelID, nameof(GetExtents)).SiteModelExtent;
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
      Log.LogInformation($"{nameof(GetStatistics)}: projectStatisticsTRexRequest: {JsonConvert.SerializeObject(projectStatisticsTRexRequest)}");
      projectStatisticsTRexRequest.Validate();

      var siteModel = GatewayHelper.ValidateAndGetSiteModel(projectStatisticsTRexRequest.ProjectUid, nameof(GetStatistics));
      var extents = ProjectExtents.ProductionDataAndSurveyedSurfaces(projectStatisticsTRexRequest.ProjectUid, projectStatisticsTRexRequest.ExcludedSurveyedSurfaceUids);

      var result = new ProjectStatisticsResult();
      if (extents != null)
        result.extents = new BoundingBox3DGrid(
          extents.MinX, extents.MinY, extents.MinZ,
          extents.MaxX, extents.MaxY, extents.MaxZ
        );

      var startEndDates = siteModel.GetDateRange();
      result.startTime = startEndDates.startUtc;
      result.endTime = startEndDates.endUtc;

      result.cellSize = siteModel.Grid.CellSize;
      result.indexOriginOffset = (int) siteModel.Grid.IndexOriginOffset;
      return result;
    }

    
    /// <summary>
    /// Returns list of machines which have contributed to a site model.
    /// </summary>
    /// <param name="siteModelID">Site model identifier.</param>
    /// <param name="coordinateServiceUtility"></param>
    /// <returns></returns>
    [HttpGet("{siteModelID}/machines")]
    public MachineExecutionResult GetMachines(string siteModelID,
      [FromServices] ICoordinateServiceUtility coordinateServiceUtility)
    {
      Log.LogInformation($"{nameof(GetMachines)}: siteModelID: {siteModelID}");

      var siteModel = GatewayHelper.ValidateAndGetSiteModel(siteModelID, nameof(GetMachines));
      if (string.IsNullOrEmpty(siteModel.CSIB()))
      {
        Log.LogError($"{nameof(GetMachines)}: siteModel has no CSIB");
        throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "The SiteModel has no CSIB."));
      }

      var machines = siteModel.Machines.ToList();
      var result = new MachineExecutionResult(new List<MachineStatus>(machines.Count));

      if (machines.Any())
      {
        List<MachineStatus> resultMachines = machines.Select(machine => AutoMapperUtility.Automapper.Map<MachineStatus>(machine)).ToList();
        var response = coordinateServiceUtility.PatchLLH(siteModel.CSIB(), resultMachines);
        if (response == ContractExecutionStatesEnum.ExecutedSuccessfully)
          result.MachineStatuses = resultMachines;
        else
          throw new ServiceException(HttpStatusCode.InternalServerError, new ContractExecutionResult(response, $"Unable to convert last known machine locations to LLH. machineLocations: {JsonConvert.SerializeObject(machines)}. CSIB: {siteModel.CSIB()}"));
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

      var siteModel = GatewayHelper.ValidateAndGetSiteModel(siteModelID, nameof(GetAssetOnDesignPeriods));
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

      var siteModel = GatewayHelper.ValidateAndGetSiteModel(siteModelID, nameof(GetMachineLayers));
      return new AssetOnDesignLayerPeriodsExecutionResult(siteModel.GetAssetOnDesignLayerPeriods());
    }

  }
}
