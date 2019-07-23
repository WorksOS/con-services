using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.Common;
using VSS.TRex.Common.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Designs.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;
using VSS.TRex.Profiling;
using VSS.TRex.Profiling.GridFabric.Arguments;
using VSS.TRex.Profiling.GridFabric.Requests;
using VSS.TRex.Profiling.Models;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/profiles")]
  public class ProfilesController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger<ProfilesController>();
    const double KM_HR_TO_CM_SEC = 27.77777778; //1.0 / 3600 * 100000;
    const int ABOVE_TARGET = 0;
    const int ON_TARGET = 1;
    const int BELOW_TARGET = 2;
    const int NO_INDEX = -1;

    /// <summary>
    /// Gets a profile between two points across a design in a project
    /// </summary>
    [HttpGet("design/{siteModelID}/{designID}")]
    public async Task<JsonResult> ComputeDesignProfile(string siteModelID, string designID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY,
      [FromQuery] double? offset)
    {
      var siteModelUid = Guid.Parse(siteModelID);
      var siteModel = DIContext.Obtain<ISiteModels>().GetSiteModel(siteModelUid);
      var design = siteModel?.Designs?.Locate(Guid.Parse(designID));

      if (design == null)
        return new JsonResult($"Unable to locate design {designID} in project {siteModelID}");

      var result = await design.ComputeProfile(siteModelUid, new[] { new XYZ(startX, startY, 0), new XYZ(endX, endY, 0) }, siteModel.CellSize, offset ?? 0);

      return new JsonResult(result.profile);
    }

    /// <summary>
    /// Gets a profile between two points across a design in a project
    /// </summary>
    [HttpGet("compositeelevations/{siteModelID}")]
    public async Task<JsonResult> ComputeCompositeElevationProfile(string siteModelID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      var siteModelUid = Guid.Parse(siteModelID);

      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = siteModelUid,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.CellPasses,
        PositionsAreGrid = true,
        Filters = new FilterSet(new[] { new CombinedFilter() }),
        ReferenceDesign = new DesignOffset(),
        StartPoint = new WGS84Point(lon: startX, lat: startY),
        EndPoint = new WGS84Point(lon: endX, lat: endY),
        ReturnAllPassesAndLayers = false,
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      var request = new ProfileRequest_ApplicationService_ProfileCell();
      var response = await request.ExecuteAsync(arg);

      if (response == null)
        return new JsonResult(@"Profile response is null");

      if (response.ProfileCells == null)
        return new JsonResult(@"Profile response contains no profile cells");

      //var nonNulls = Response.ProfileCells.Where(x => !x.IsNull()).ToArray();
      return new JsonResult(response.ProfileCells.Select(x => new
      {
        station = x.Station,
        cellLowestElev = x.CellLowestElev,
        cellHighestElev = x.CellHighestElev,
        cellLastElev = x.CellLastElev,
        cellFirstElev = x.CellFirstElev,
        cellLowestCompositeElev = x.CellLowestCompositeElev,
        cellHighestCompositeElev = x.CellHighestCompositeElev,
        cellLastCompositeElev = x.CellLastCompositeElev,
        cellFirstCompositeElev = x.CellFirstCompositeElev
      }));
    }

    /// <summary>
    /// Gets a profile between two points across a design in a project
    /// </summary>
    [HttpPost("productiondata/{siteModelID}")]
    public async Task<JsonResult> ComputeProductionDataProfile(string siteModelID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY,
      [FromQuery] int displayMode,
      [FromQuery] Guid? cutFillDesignUid,
      [FromQuery] double? offset,
      [FromBody] OverrideParameters overrides)
    {
      var siteModelUid = Guid.Parse(siteModelID);

      /*
      //Use default values for now
      var overrides = new OverrideParameters
      {
        CMVRange = new CMVRangePercentageRecord(80, 130),
        MDPRange = new MDPRangePercentageRecord(80, 130),
        TargetMachineSpeed = new MachineSpeedExtendedRecord((ushort) (5 * KM_HR_TO_CM_SEC), (ushort) (10 * KM_HR_TO_CM_SEC))
      };
      */

      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = siteModelUid,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.CellPasses,
        PositionsAreGrid = true,
        Filters = new FilterSet(new[] { new CombinedFilter() }),
        ReferenceDesign = new DesignOffset(cutFillDesignUid ?? Guid.Empty, offset ?? 0.0),
        StartPoint = new WGS84Point(lon: startX, lat: startY),
        EndPoint = new WGS84Point(lon: endX, lat: endY),
        ReturnAllPassesAndLayers = false,
        Overrides = overrides
      };

      // Compute a profile from the bottom left of the screen extents to the top right 
      var request = new ProfileRequest_ApplicationService_ProfileCell();
      var response = await request.ExecuteAsync(arg);

      if (response == null)
        return new JsonResult(@"Profile response is null");

      if (response.ProfileCells == null)
        return new JsonResult(@"Profile response contains no profile cells");

      var results = (from x in response.ProfileCells
        let v = ProfileValue(displayMode, x, overrides)
        select new
        {
          station = x.Station,
          elevation = ProfileElevation(displayMode, x),
          index = v.index,
          value = v.value,
          value2 = v.value2
        });
      return new JsonResult(results);
    }

    /// <summary>
    /// Get the profile value of this cell for the mode. 
    /// </summary>
    private (int index, double value, double value2) ProfileValue(int mode, ProfileCell cell, OverrideParameters overrides)
    {
      var NULL_VALUE = (NO_INDEX, double.NaN, double.NaN);

      double value;
      int index;

      switch ((DisplayMode) mode)
      {
        case DisplayMode.CCV:
          if (cell.CellTargetCCV == 0 || cell.CellTargetCCV == CellPassConsts.NullCCV ||
              cell.CellCCV == CellPassConsts.NullCCV)
            return NULL_VALUE;
          return (NO_INDEX, cell.CellCCV / 10.0, 0);
        case DisplayMode.CCVPercentSummary:
          if (cell.CellTargetCCV == 0 || cell.CellTargetCCV == CellPassConsts.NullCCV ||
              cell.CellCCV == CellPassConsts.NullCCV)
            return NULL_VALUE;
          value = (double) cell.CellCCV / (double) cell.CellTargetCCV * 100;
          index = value < overrides.CMVRange.Min ? BELOW_TARGET : value > overrides.CMVRange.Max ? ABOVE_TARGET : ON_TARGET;
          return (index, value, 0);
        case DisplayMode.CMVChange:
          if (cell.CellCCV == CellPassConsts.NullCCV)
            return NULL_VALUE;
          value = cell.CellPreviousMeasuredCCV == CellPassConsts.NullCCV ? 100 : 
            (double)(cell.CellCCV - cell.CellPreviousMeasuredCCV) / (double)cell.CellPreviousMeasuredCCV * 100;
          return (NO_INDEX, value, 0);
        case DisplayMode.PassCount:
          if (cell.TopLayerPassCount == CellPassConsts.NullPassCountValue)
            return NULL_VALUE;
          return (NO_INDEX, cell.TopLayerPassCount, 0);
        case DisplayMode.PassCountSummary:
          if (cell.TopLayerPassCount == CellPassConsts.NullPassCountValue || cell.CellLastElev == CellPassConsts.NullHeight)
            return NULL_VALUE;
          index = cell.TopLayerPassCount < cell.TopLayerPassCountTargetRangeMin ? BELOW_TARGET :
            cell.TopLayerPassCount > cell.TopLayerPassCountTargetRangeMax ? ABOVE_TARGET : ON_TARGET;
          return (index, 0, 0);
        case DisplayMode.CutFill:
          if (cell.CellLastCompositeElev == CellPassConsts.NullHeight || cell.DesignElev == CellPassConsts.NullHeight)
            return NULL_VALUE;
          value = cell.CellLastCompositeElev - cell.DesignElev;
          return (NO_INDEX, value, 0);
        case DisplayMode.TemperatureSummary:
          if (cell.CellMaterialTemperature == CellPassConsts.NullMaterialTemperatureValue || cell.CellMaterialTemperatureElev == CellPassConsts.NullHeight)
            return NULL_VALUE;
          index = cell.CellMaterialTemperature < cell.CellMaterialTemperatureWarnMin ? BELOW_TARGET : 
            cell.CellMaterialTemperature > cell.CellMaterialTemperatureWarnMax ? ABOVE_TARGET : ON_TARGET;
          return (index, 0, 0);
        case DisplayMode.TemperatureDetail:
          if (cell.CellMaterialTemperature == CellPassConsts.NullMaterialTemperatureValue)
            return NULL_VALUE;
          return (NO_INDEX, cell.CellMaterialTemperature / 10.0, 0);
        case DisplayMode.MDPPercentSummary:
          if (cell.CellTargetMDP == 0 || cell.CellTargetMDP == CellPassConsts.NullMDP ||
              cell.CellMDP == CellPassConsts.NullMDP)
            return NULL_VALUE;
          value = (double) cell.CellMDP / (double) cell.CellTargetMDP * 100;
          index = value < overrides.MDPRange.Min ? BELOW_TARGET : value > overrides.MDPRange.Max ? ABOVE_TARGET : ON_TARGET;
          return (index, value, 0);
        case DisplayMode.TargetSpeedSummary:
          if (cell.CellMaxSpeed == CellPassConsts.NullMachineSpeed || cell.CellLastElev == CellPassConsts.NullHeight)
            return NULL_VALUE;
          index = cell.CellMaxSpeed > overrides.TargetMachineSpeed.Max ? ABOVE_TARGET :
            //cell.CellMinSpeed < overrides.TargetMachineSpeed.Min &&
            cell.CellMaxSpeed < overrides.TargetMachineSpeed.Min ? BELOW_TARGET : ON_TARGET;
          return (index, cell.CellMinSpeed/KM_HR_TO_CM_SEC, cell.CellMaxSpeed/KM_HR_TO_CM_SEC);
        case DisplayMode.Height:
        default:
          if (cell.CellLastElev == CellPassConsts.NullHeight)
            return NULL_VALUE;
          return (NO_INDEX, cell.CellLastElev, 0);
      }
    }

    /// <summary>
    /// Get the profile elevation of the cell for the mode
    /// </summary>
    private double ProfileElevation(int mode, ProfileCell cell)
    {
      var elevation = 0.0;
      switch ((DisplayMode)mode)
      {
        case DisplayMode.CCV:
        case DisplayMode.CCVPercentSummary:
        case DisplayMode.CMVChange:
          elevation = cell.CellCCVElev;
          break;
        case DisplayMode.TemperatureSummary:
        case DisplayMode.TemperatureDetail:
          elevation = cell.CellMaterialTemperatureElev;
          break;
        case DisplayMode.MDPPercentSummary:
          elevation = cell.CellMDPElev;
          break;
        case DisplayMode.CutFill:
          elevation = cell.CellLastCompositeElev;
          break;
        case DisplayMode.PassCount:
        case DisplayMode.PassCountSummary:
        case DisplayMode.TargetSpeedSummary:
        case DisplayMode.Height:
        default:
          elevation = cell.CellLastElev;
          break;
      }

      return elevation;
    }

    [HttpGet("volumes/{siteModelID}")]
    public async Task<JsonResult> ComputeSummaryVolumesProfile(string siteModelID,
      [FromQuery] double startX,
      [FromQuery] double startY,
      [FromQuery] double endX,
      [FromQuery] double endY)
    {
      //TODO: can add design to ground and ground to design by passing the cutFillDesignUid

      var siteModelUid = Guid.Parse(siteModelID);

      var arg = new ProfileRequestArgument_ApplicationService
      {
        ProjectID = siteModelUid,
        ProfileTypeRequired = GridDataType.Height,
        ProfileStyle = ProfileStyle.SummaryVolume,
        PositionsAreGrid = true,
        Filters = new FilterSet(new CombinedFilter(), new CombinedFilter()),
        StartPoint = new WGS84Point(lon: startX, lat: startY),
        EndPoint = new WGS84Point(lon: endX, lat: endY),
        ReturnAllPassesAndLayers = false,
        VolumeType = VolumeComputationType.Between2Filters
      };

      // This is a simple earliest filter to latest filter test
      arg.Filters.Filters[0].AttributeFilter.ReturnEarliestFilteredCellPass = true;
      arg.Filters.Filters[1].AttributeFilter.ReturnEarliestFilteredCellPass = false;

      // Compute a profile from the bottom left of the screen extents to the top right 
      var request = new ProfileRequest_ApplicationService_SummaryVolumeProfileCell();

      var response = await request.ExecuteAsync(arg);
      if (response == null)
        return new JsonResult(@"Profile response is null");

      if (response.ProfileCells == null)
        return new JsonResult(@"Profile response contains no profile cells");

      return new JsonResult(response.ProfileCells.Select(x => new XYZS(0, 0, x.LastCellPassElevation2 - x.LastCellPassElevation1, x.Station, -1)));
    }

    /// <summary>
    /// Retrieves the list of available summary types
    /// </summary>
    [HttpGet("summarytypes")]
    public JsonResult GetModes()
    {
      return new JsonResult(new List<(int Index, string Name)>
      {
        (NO_INDEX, string.Empty),
        (ABOVE_TARGET, "Above target"),
        (BELOW_TARGET, "Below target"),
        (ON_TARGET, "On target")
      });
    }
  }
}
