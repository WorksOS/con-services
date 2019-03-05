using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.DI;
using VSS.TRex.Executors;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.SiteModels.Interfaces;

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
      var extents = DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID))?.SiteModelExtent;
      
      if (extents != null)
        return BoundingBox3DGrid.CreatBoundingBox3DGrid(

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
    public ProjectStatisticsResult GetStatistics([FromBody]ProjectStatisticsTRexRequest projectStatisticsTRexRequest)
    {
      projectStatisticsTRexRequest.Validate();

      ISiteModel siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(projectStatisticsTRexRequest.ProjectUid);
      if (siteModel == null)
        return new ProjectStatisticsResult(ContractExecutionStatesEnum.ValidationError);

      var extents = ProjectExtents.ProductionDataAndSurveyedSurfaces(projectStatisticsTRexRequest.ProjectUid, projectStatisticsTRexRequest.ExcludedSurveyedSurfaceUids);

      var result = new ProjectStatisticsResult();
      if (extents != null)
        result.extents = BoundingBox3DGrid.CreatBoundingBox3DGrid(
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
    /// <returns></returns>
    [HttpGet("{siteModelID}/machines")]
    public MachineExecutionResult GetMachines(string siteModelID)
    {
      var machines = DIContext.Obtain<ISiteModels>().GetSiteModel(Guid.Parse(siteModelID))?.Machines.ToList();

      var result = MachineExecutionResult.CreateMachineExecutionResult(new MachineStatus[0]);
      if (machines != null)
        result.MachineStatuses = machines.Select(machine =>
          AutoMapperUtility.Automapper.Map<MachineStatus>(machine)).ToArray();

      return result;
    }
  }
}
