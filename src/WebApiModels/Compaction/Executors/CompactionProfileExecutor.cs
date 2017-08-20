using Microsoft.Extensions.Logging;
using SVOICOptionsDecls;
using SVOICProfileCell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.Utilities;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Velociraptor.PDSInterface;

namespace VSS.Productivity3D.WebApiModels.Compaction.Executors
{
  /// <summary>
  /// Get production data profile calculations executor.
  /// </summary>
  public class CompactionProfileExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      try
      {
        MemoryStream memoryStream;

        ProfileProductionDataRequest request = item as ProfileProductionDataRequest;
        var filter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId);
        var designDescriptor = RaptorConverters.DesignDescriptor(request.alignmentDesign);
        var liftBuildSettings =
          RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic);
        if (!RaptorConverters.DesignDescriptor(request.alignmentDesign).IsNull())
        {
          ASNode.RequestAlignmentProfile.RPC.TASNodeServiceRPCVerb_RequestAlignmentProfile_Args args
            = ASNode.RequestAlignmentProfile.RPC.__Global.Construct_RequestAlignmentProfile_Args
            (request.projectId ?? -1,
              -1, // don't care
              request.startStation ?? ValidationConstants.MIN_STATION,
              request.endStation ?? ValidationConstants.MIN_STATION,
              designDescriptor,
              filter,
              liftBuildSettings,
              designDescriptor,
              request.returnAllPassesAndLayers);

          memoryStream = raptorClient.GetAlignmentProfile(args);
        }
        else
        {
          VLPDDecls.TWGS84Point startPt, endPt;
          bool positionsAreGrid;
          ProfilesHelper.convertProfileEndPositions(request.gridPoints, request.wgs84Points, out startPt, out endPt, out positionsAreGrid);

          ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args args
            = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args
            (request.projectId ?? -1,
              -1, // don't care
              positionsAreGrid,
              startPt,
              endPt,
              filter,
              liftBuildSettings,
              designDescriptor,
              request.returnAllPassesAndLayers);

          memoryStream = raptorClient.GetProfile(args);
        }

        if (memoryStream != null)
        {
          var profileResult = ConvertProfileResult(memoryStream, request.liftBuildSettings);
          AddMidPoints(profileResult);
          InterpolateEdges(profileResult);
          result = profileResult;
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest,
            new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
              "Failed to get requested slicer profile"));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
      return result;
    }

    /// <summary>
    /// Convert Raptor data to the data to return from the Web API
    /// </summary>
    /// <param name="ms">Memory stream of data from Raptor</param>
    /// <param name="liftBuildSettings">Lift build settings from project settings used in the Raptor profile calculations</param>
    /// <returns>The profile data</returns>
    private CompactionProfileResult ConvertProfileResult(MemoryStream ms, LiftBuildSettings liftBuildSettings)
    {
      log.LogDebug("Converting profile result");

      var profile = new CompactionProfileResult();
 
      PDSProfile pdsiProfile = new PDSProfile();
      TICProfileCellListPackager packager = new TICProfileCellListPackager();
      packager.CellList = new TICProfileCellList();
      packager.ReadFromStream(ms);
      pdsiProfile.Assign(packager.CellList);
      pdsiProfile.GridDistanceBetweenProfilePoints = packager.GridDistanceBetweenProfilePoints;

      profile.cells = new List<CompactionProfileCell>();
      VSS.Velociraptor.PDSInterface.ProfileCell prevCell = null;
      foreach (VSS.Velociraptor.PDSInterface.ProfileCell currCell in pdsiProfile.cells)
      {
        double prevStationIntercept = prevCell == null ? 0.0 : prevCell.station + prevCell.interceptLength;
        bool gap = prevCell == null
          ? false
          : Math.Abs(currCell.station - prevStationIntercept) > 0.001;
        if (gap)
        {
          var gapCell = new CompactionProfileCell(GapCell);
          gapCell.station = prevStationIntercept;
          profile.cells.Add(gapCell);
        }

        bool noCCVValue = currCell.TargetCCV == 0 || currCell.TargetCCV == NO_CCV || currCell.CCV == NO_CCV;
        bool noCCElevation = currCell.CCVElev == NULL_SINGLE || noCCVValue;
        bool noMDPValue = currCell.TargetMDP == 0 || currCell.TargetMDP == NO_MDP || currCell.MDP == NO_MDP;
        bool noMDPElevation = currCell.MDPElev == NULL_SINGLE || noMDPValue;
        bool noTemperatureValue = currCell.materialTemperature == NO_TEMPERATURE;
        bool noTemperatureElevation = currCell.materialTemperatureElev == NULL_SINGLE || noTemperatureValue;
        bool noPassCountValue = currCell.topLayerPassCount == NO_PASSCOUNT;
        bool noSpeedValue = true;//TODO: *****

        var cmvPercent = noCCVValue
          ? float.NaN
          : (float) currCell.CCV / (float) currCell.TargetCCV * 100.0F;

        var mdpPercent = noMDPValue
          ? float.NaN
          : (float) currCell.MDP / (float) currCell.TargetMDP * 100.0F;

        var speed = 0;//TODO: *****

        profile.cells.Add(new CompactionProfileCell
        {
          cellType = prevCell == null ? CompactionProfileCell.ProfileCellType.MidPoint : CompactionProfileCell.ProfileCellType.Edge,

          station = currCell.station,

          firstPassHeight = currCell.firstPassHeight == NULL_SINGLE ? float.NaN : currCell.firstPassHeight,
          highestPassHeight = currCell.highestPassHeight == NULL_SINGLE ? float.NaN : currCell.highestPassHeight,
          lastPassHeight = currCell.lastPassHeight == NULL_SINGLE ? float.NaN : currCell.lastPassHeight,
          lowestPassHeight = currCell.lowestPassHeight == NULL_SINGLE ? float.NaN : currCell.lowestPassHeight,

          lastCompositeHeight = currCell.compositeLastPassHeight == NULL_SINGLE ? float.NaN : currCell.compositeLastPassHeight,
          designHeight = currCell.designHeight == NULL_SINGLE ? float.NaN : currCell.designHeight,

          cmvPercent = cmvPercent,
          cmvHeight = noCCElevation ? float.NaN : currCell.CCVElev,

          mdpPercent = mdpPercent,
          mdpHeight = noMDPElevation ? float.NaN : currCell.MDPElev,

          temperature = noTemperatureValue ? float.NaN : currCell.materialTemperature / 10.0F,// As temperature is reported in 10th...
          temperatureHeight = noTemperatureElevation ? float.NaN : currCell.materialTemperatureElev,

          topLayerPassCount = noPassCountValue ? -1 : currCell.topLayerPassCount,

          cmvPercentChange = currCell.CCV == NO_CCV ? float.NaN :
            (currCell.PrevCCV == NO_CCV ? 100.0f :
              (float)Math.Abs(currCell.CCV - currCell.PrevCCV) / (float)currCell.PrevCCV * 100.0f),

          speed = speed,
          //TODO: Do we need speedHeight ???

          passCountIndex = noPassCountValue ? CompactionProfileCell.ValueTargetType.NoData :
            (currCell.topLayerPassCount < currCell.topLayerPassCountTargetRange.Min ? CompactionProfileCell.ValueTargetType.BelowTarget :
              (currCell.topLayerPassCount > currCell.topLayerPassCountTargetRange.Max ? CompactionProfileCell.ValueTargetType.AboveTarget : 
              CompactionProfileCell.ValueTargetType.OnTarget)),

          temperatureIndex = noTemperatureValue ? CompactionProfileCell.ValueTargetType.NoData :
            (currCell.materialTemperature < currCell.materialTemperatureWarnMin ? CompactionProfileCell.ValueTargetType.BelowTarget :
              (currCell.materialTemperature > currCell.materialTemperatureWarnMax ? CompactionProfileCell.ValueTargetType.AboveTarget : 
              CompactionProfileCell.ValueTargetType.OnTarget)),

          cmvIndex = noCCVValue ? CompactionProfileCell.ValueTargetType.NoData :
            (cmvPercent < liftBuildSettings.cCVRange.min ? CompactionProfileCell.ValueTargetType.BelowTarget :
              (cmvPercent > liftBuildSettings.cCVRange.max ? CompactionProfileCell.ValueTargetType.AboveTarget :
                CompactionProfileCell.ValueTargetType.OnTarget)),

          mdpIndex = noMDPValue ? CompactionProfileCell.ValueTargetType.NoData :
            (mdpPercent < liftBuildSettings.mDPRange.min ? CompactionProfileCell.ValueTargetType.BelowTarget :
              (mdpPercent > liftBuildSettings.mDPRange.max ? CompactionProfileCell.ValueTargetType.AboveTarget :
                CompactionProfileCell.ValueTargetType.OnTarget)),

          speedIndex = noSpeedValue ? CompactionProfileCell.ValueTargetType.NoData :
            (speed < liftBuildSettings.machineSpeedTarget.MinTargetMachineSpeed ? CompactionProfileCell.ValueTargetType.BelowTarget :
              (speed > liftBuildSettings.machineSpeedTarget.MaxTargetMachineSpeed ? CompactionProfileCell.ValueTargetType.AboveTarget :
                CompactionProfileCell.ValueTargetType.OnTarget)),
        });

        prevCell = currCell;
      }
      //Add a last point at the intercept length of the last cell so profiles are drawn correctly
      if (prevCell != null)
      {
        var lastCell = new CompactionProfileCell(profile.cells[profile.cells.Count - 1]);
        lastCell.cellType = CompactionProfileCell.ProfileCellType.MidPoint;
        lastCell.station = prevCell.station + prevCell.interceptLength;
        profile.cells.Add((lastCell));
      }
      ms.Close();

      profile.gridDistanceBetweenProfilePoints = pdsiProfile.GridDistanceBetweenProfilePoints;

      string message = string.Empty;
      foreach (var cell in profile.cells)
      {
        message = $"{message},{cell.cellType}";
      }
      log.LogDebug($"After profile conversion: {profile.cells.Count}{message}");
      return profile;
    }

    /// <summary>
    /// Add mid points between the cell edge intersections. This is because the profile line is plotted using these points.
    /// The cell edges are retained as this is where the color changes on the graph or chart.
    /// </summary>
    /// <param name="profileResult">The profile containing the list of cell edge points from Raptor</param>
    /// <returns>The complete list of interspersed edges and  mid points.</returns>
    private void AddMidPoints(CompactionProfileResult profileResult)
    {
      log.LogDebug("Adding midpoints");
      if (profileResult.cells.Count > 3)
      {
        //No mid point for first and last segments since only partial across the cell.
        //We have already added them as mid points.
        List<CompactionProfileCell> cells = new List<CompactionProfileCell>();
        cells.Add(profileResult.cells[0]);
        for (int i = 1; i < profileResult.cells.Count - 2; i++)
        {
          cells.Add(profileResult.cells[i]);
          if (profileResult.cells[i].cellType != CompactionProfileCell.ProfileCellType.Gap)
          {
            var midPoint = new CompactionProfileCell(profileResult.cells[i]);
            midPoint.cellType = CompactionProfileCell.ProfileCellType.MidPoint;
            midPoint.station = profileResult.cells[i].station +
                               (profileResult.cells[i + 1].station - profileResult.cells[i].station) / 2;
            cells.Add(midPoint);
          }
        }
        cells.Add(profileResult.cells[profileResult.cells.Count - 2]);
        cells.Add(profileResult.cells[profileResult.cells.Count - 1]);
        profileResult.cells = cells;
      }

      string message = string.Empty;
      foreach (var cell in profileResult.cells)
      {
        message = $"{message},{cell.cellType}";
      }
      log.LogDebug($"After adding midpoints: {profileResult.cells.Count}{message}");
    }

    /// <summary>
    /// Since the profile line will be drawn between line segment mid points we need to interpolate the cell edge points to lie on these line segments.
    /// </summary>
    /// <param name="profileResult">The profile containing the list of line segment points, both edges and mid points.</param>
    private void InterpolateEdges(CompactionProfileResult profileResult)
    {
      log.LogDebug("Interpolating edges");
      if (profileResult.cells.Count > 3)
      {
        //First and last points are not gaps or edges. They're always the start and end of the profile line.
        for (int i = 1; i < profileResult.cells.Count - 1; i++)
        {
          if (profileResult.cells[i].cellType == CompactionProfileCell.ProfileCellType.Edge)
          {
            //Interpolate i edge for line between mid points at i-1 and i+1
            var startIndex = i - 1;
            var endIndex = i + 1;
            //Gap is between i-1 and i. Interpolate i edge for line between mid points at i-2 and i+1
            if (profileResult.cells[i - 1].cellType == CompactionProfileCell.ProfileCellType.Gap)
            {
              startIndex--;
              //Also adjust the gap point
              InterpolateElevations(profileResult.cells[i - 1], profileResult.cells[startIndex],
                profileResult.cells[endIndex]);
            }
            InterpolateElevations(profileResult.cells[i], profileResult.cells[startIndex],
              profileResult.cells[endIndex]);
          }
        }
      }
      log.LogDebug("After interpolation");
    }

    /// <summary>
    /// Interpolate elevations for the specified point (cell) on the line segment from startCell to endCell
    /// </summary>
    /// <param name="cell">The point to interpolate</param>
    /// <param name="startCell">The start of the line segment</param>
    /// <param name="endCell">The end of the line segment</param>
    private void InterpolateElevations(CompactionProfileCell cell, CompactionProfileCell startCell, CompactionProfileCell endCell)
    {
      var proportion = (cell.station - startCell.station) / (endCell.station - startCell.station);

      cell.firstPassHeight = InterpolateElevation(proportion, startCell.firstPassHeight, endCell.firstPassHeight);
      cell.highestPassHeight = InterpolateElevation(proportion, startCell.highestPassHeight, endCell.highestPassHeight);
      cell.lastPassHeight = InterpolateElevation(proportion, startCell.lastPassHeight, endCell.lastPassHeight);
      cell.lowestPassHeight = InterpolateElevation(proportion, startCell.lowestPassHeight, endCell.lowestPassHeight);
      cell.lastCompositeHeight = InterpolateElevation(proportion, startCell.lastCompositeHeight, endCell.lastCompositeHeight);
      cell.designHeight = InterpolateElevation(proportion, startCell.designHeight, endCell.designHeight); //TODO: Should this be interpolated?
      cell.cmvHeight = InterpolateElevation(proportion, startCell.cmvHeight, endCell.cmvHeight);
      cell.mdpHeight = InterpolateElevation(proportion, startCell.mdpHeight, endCell.mdpHeight);
      cell.temperatureHeight = InterpolateElevation(proportion, startCell.temperatureHeight, endCell.temperatureHeight);
      //TODO: Speed ?
      log.LogDebug($"Interpolated point {cell.station} of type {cell.cellType}");
    }

    /// <summary>
    /// Interpolate an elevation
    /// </summary>
    /// <param name="proportion">The proportion of the elevation to use</param>
    /// <param name="startElevation">The elevation at the start of the line segment to be used for interpolation</param>
    /// <param name="endElevation">The elevation at the end of the line segment to be used for interpolation</param>
    /// <returns></returns>
    private float InterpolateElevation(double proportion, float startElevation, float endElevation)
    {
      //Check for no elevation data before trying to interpolate
      return float.IsNaN(startElevation) || float.IsNaN(endElevation) ? float.NaN : 
        startElevation + (float)proportion * (endElevation - startElevation);
    }

    /// <summary>
    /// Representation of a profile cell edge at the start of a gap (no data)
    /// </summary>
    private readonly CompactionProfileCell GapCell = new CompactionProfileCell
    {
      cellType = CompactionProfileCell.ProfileCellType.Gap,
      station = 0,//Will be set for individual gap cells
      firstPassHeight = float.NaN,
      highestPassHeight = float.NaN,
      lastPassHeight = float.NaN,
      lowestPassHeight = float.NaN,
      lastCompositeHeight = float.NaN,
      designHeight = float.NaN,
      cmvPercent = float.NaN,
      cmvHeight = float.NaN,
      mdpPercent = float.NaN,
      mdpHeight = float.NaN,
      temperature = float.NaN,
      temperatureHeight = float.NaN,
      topLayerPassCount = -1,
      cmvPercentChange = float.NaN,
      speed = float.NaN,
      passCountIndex = CompactionProfileCell.ValueTargetType.NoData,
      temperatureIndex = CompactionProfileCell.ValueTargetType.NoData,
      cmvIndex = CompactionProfileCell.ValueTargetType.NoData,
      mdpIndex = CompactionProfileCell.ValueTargetType.NoData,
      speedIndex = CompactionProfileCell.ValueTargetType.NoData,
    };

    private const double NO_HEIGHT = 1E9;
    private const int NO_CCV = SVOICDecls.__Global.kICNullCCVValue;
    private const int NO_MDP = SVOICDecls.__Global.kICNullMDPValue;
    private const int NO_TEMPERATURE = SVOICDecls.__Global.kICNullMaterialTempValue;
    private const int NO_PASSCOUNT = SVOICDecls.__Global.kICNullPassCountValue;
    private const float NULL_SINGLE = DTXModelDecls.__Global.NullSingle;
  }
}