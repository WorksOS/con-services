﻿using Microsoft.Extensions.Logging;
using SVOICOptionsDecls;
using SVOICProfileCell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.Utilities;
using VSS.Productivity3D.Common.Filters.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Productivity3D.WebApi.Models.Compaction.ResultHandling;
using VSS.Productivity3D.WebApi.Models.ProductionData.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
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

        CompactionProfileProductionDataRequest request = item as CompactionProfileProductionDataRequest;
        var filter = RaptorConverters.ConvertFilter(request.filterID, request.filter, request.projectId);
        var designDescriptor = RaptorConverters.DesignDescriptor(request.cutFillDesignDescriptor);
        var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.alignmentDesign);
        var liftBuildSettings =
          RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmNone);
        if (request.IsAlignmentDesign)
        {
          ASNode.RequestAlignmentProfile.RPC.TASNodeServiceRPCVerb_RequestAlignmentProfile_Args args
            = ASNode.RequestAlignmentProfile.RPC.__Global.Construct_RequestAlignmentProfile_Args
            (request.projectId ?? -1,
              ProfilesHelper.PROFILE_TYPE_NOT_REQUIRED,
              request.startStation ?? ValidationConstants.MIN_STATION,
              request.endStation ?? ValidationConstants.MIN_STATION,
              alignmentDescriptor,
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
          ProfilesHelper.ConvertProfileEndPositions(request.gridPoints, request.wgs84Points, out startPt, out endPt, out positionsAreGrid);

          ASNode.RequestProfile.RPC.TASNodeServiceRPCVerb_RequestProfile_Args args
            = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args
            (request.projectId ?? -1,
              ProfilesHelper.PROFILE_TYPE_NOT_REQUIRED,
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
          //For convenience return empty list rather than null for easier manipulation
          result = new CompactionProfileResult<CompactionProfileCell>{results = new List<CompactionProfileCell>()};
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
    private CompactionProfileResult<CompactionProfileCell> ConvertProfileResult(MemoryStream ms, LiftBuildSettings liftBuildSettings)
    {
      log.LogDebug("Converting profile result");

      var profile = new CompactionProfileResult<CompactionProfileCell>();
 
      PDSProfile pdsiProfile = new PDSProfile();
      TICProfileCellListPackager packager = new TICProfileCellListPackager();
      packager.CellList = new TICProfileCellList();
      packager.ReadFromStream(ms);
      pdsiProfile.Assign(packager.CellList);

      pdsiProfile.GridDistanceBetweenProfilePoints = packager.GridDistanceBetweenProfilePoints;

      profile.results = new List<CompactionProfileCell>();
      VSS.Velociraptor.PDSInterface.ProfileCell prevCell = null;
      foreach (VSS.Velociraptor.PDSInterface.ProfileCell currCell in pdsiProfile.cells)
      {
        var gapExists = ProfilesHelper.CellGapExists(prevCell, currCell, out double prevStationIntercept);

        if (gapExists)
        {
          var gapCell = new CompactionProfileCell(GapCell);
          gapCell.station = prevStationIntercept;
          profile.results.Add(gapCell);
        }

        bool noCCVValue = currCell.TargetCCV == 0 || currCell.TargetCCV == VelociraptorConstants.NO_CCV || currCell.CCV == VelociraptorConstants.NO_CCV;
        bool noCCVElevation = currCell.CCVElev == VelociraptorConstants.NULL_SINGLE || noCCVValue;
        bool noMDPValue = currCell.TargetMDP == 0 || currCell.TargetMDP == VelociraptorConstants.NO_MDP || currCell.MDP == VelociraptorConstants.NO_MDP;
        bool noMDPElevation = currCell.MDPElev == VelociraptorConstants.NULL_SINGLE || noMDPValue;
        bool noTemperatureValue = currCell.materialTemperature == VelociraptorConstants.NO_TEMPERATURE;
        bool noTemperatureElevation = currCell.materialTemperatureElev == VelociraptorConstants.NULL_SINGLE || noTemperatureValue;
        bool noPassCountValue = currCell.topLayerPassCount == VelociraptorConstants.NO_PASSCOUNT;
 
        var lastPassHeight = currCell.lastPassHeight == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.lastPassHeight;
        var lastCompositeHeight = currCell.compositeLastPassHeight == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.compositeLastPassHeight;
        var designHeight = currCell.designHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.designHeight;

        //Either have none or both speed values
        var noSpeedValue = currCell.cellMaxSpeed == VelociraptorConstants.NO_SPEED;
        var speedMin = noSpeedValue ? float.NaN : (float)(currCell.cellMinSpeed / ConversionConstants.KM_HR_TO_CM_SEC);
        var speedMax = noSpeedValue ? float.NaN : (float)(currCell.cellMaxSpeed / ConversionConstants.KM_HR_TO_CM_SEC);

        var cmvPercent = noCCVValue
          ? float.NaN
          : (float) currCell.CCV / (float) currCell.TargetCCV * 100.0F;

        var mdpPercent = noMDPValue
          ? float.NaN
          : (float) currCell.MDP / (float) currCell.TargetMDP * 100.0F;

        var firstPassHeight = currCell.firstPassHeight == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.firstPassHeight;

        var highestPassHeight = currCell.highestPassHeight == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.highestPassHeight;

        var lowestPassHeight = currCell.lowestPassHeight == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.lowestPassHeight;

        var cutFill = float.IsNaN(lastCompositeHeight) || float.IsNaN(designHeight)
          ? float.NaN
          : lastCompositeHeight - designHeight;

        var cmv = noCCVValue ? float.NaN : currCell.CCV / 10.0F;
        var cmvHeight = noCCVElevation ? float.NaN : currCell.CCVElev;
        var mdpHeight = noMDPElevation ? float.NaN : currCell.MDPElev;
        var temperature = noTemperatureValue ? float.NaN : currCell.materialTemperature / 10.0F;// As temperature is reported in 10th...
        var temperatureHeight = noTemperatureElevation ? float.NaN : currCell.materialTemperatureElev;
        var topLayerPassCount = noPassCountValue ? -1 : currCell.topLayerPassCount;
        var cmvPercentChange = currCell.CCV == VelociraptorConstants.NO_CCV ? float.NaN :
          (currCell.PrevCCV == VelociraptorConstants.NO_CCV ? 100.0f :
            (float)Math.Abs(currCell.CCV - currCell.PrevCCV) / (float)currCell.PrevCCV * 100.0f);

        var passCountIndex = noPassCountValue ? ValueTargetType.NoData :
          (currCell.topLayerPassCount < currCell.topLayerPassCountTargetRange.Min ? ValueTargetType.BelowTarget :
            (currCell.topLayerPassCount > currCell.topLayerPassCountTargetRange.Max ? ValueTargetType.AboveTarget :
              ValueTargetType.OnTarget));

        var temperatureIndex = noTemperatureValue ? ValueTargetType.NoData :
          (currCell.materialTemperature < currCell.materialTemperatureWarnMin ? ValueTargetType.BelowTarget :
            (currCell.materialTemperature > currCell.materialTemperatureWarnMax ? ValueTargetType.AboveTarget :
              ValueTargetType.OnTarget));

        var cmvIndex = noCCVValue ? ValueTargetType.NoData :
          (cmvPercent < liftBuildSettings.cCVRange.min ? ValueTargetType.BelowTarget :
            (cmvPercent > liftBuildSettings.cCVRange.max ? ValueTargetType.AboveTarget :
              ValueTargetType.OnTarget));

        var mdpIndex = noMDPValue ? ValueTargetType.NoData :
          (mdpPercent < liftBuildSettings.mDPRange.min ? ValueTargetType.BelowTarget :
            (mdpPercent > liftBuildSettings.mDPRange.max ? ValueTargetType.AboveTarget :
              ValueTargetType.OnTarget));

        var speedIndex = noSpeedValue ? ValueTargetType.NoData :
          (speedMax > liftBuildSettings.machineSpeedTarget.MaxTargetMachineSpeed ? ValueTargetType.AboveTarget :
            (speedMin < liftBuildSettings.machineSpeedTarget.MinTargetMachineSpeed &&
             speedMax < liftBuildSettings.machineSpeedTarget.MinTargetMachineSpeed ? ValueTargetType.BelowTarget :
              ValueTargetType.OnTarget));

        profile.results.Add(new CompactionProfileCell
        {
          cellType = prevCell == null ? ProfileCellType.MidPoint : ProfileCellType.Edge,

          station = currCell.station,

          firstPassHeight = firstPassHeight,
          highestPassHeight = highestPassHeight,
          lastPassHeight = lastPassHeight,
          lowestPassHeight = lowestPassHeight,

          lastCompositeHeight = lastCompositeHeight,
          designHeight = designHeight,

          cutFill = cutFill,
          cutFillHeight = float.NaN,//will be set later using the cut-fill design

          cmv = cmv,
          cmvPercent = cmvPercent,
          cmvHeight = cmvHeight,

          mdpPercent = mdpPercent,
          mdpHeight = mdpHeight,

          temperature = temperature,
          temperatureHeight = temperatureHeight,

          topLayerPassCount = topLayerPassCount,

          cmvPercentChange = cmvPercentChange,

          minSpeed = speedMin,
          maxSpeed = speedMax,
          speedHeight = lastPassHeight,

          passCountIndex = passCountIndex,
          temperatureIndex = temperatureIndex,
          cmvIndex = cmvIndex,
          mdpIndex = mdpIndex,
          speedIndex = speedIndex
        });

        prevCell = currCell;
      }

      //Add a last point at the intercept length of the last cell so profiles are drawn correctly
      if (prevCell != null)
      {
        var lastCell = new CompactionProfileCell(profile.results[profile.results.Count - 1])
        {
          cellType = ProfileCellType.MidPoint,
          station = prevCell.station + prevCell.interceptLength
        };

        profile.results.Add(lastCell);
      }

      ms.Close();

      profile.gridDistanceBetweenProfilePoints = pdsiProfile.GridDistanceBetweenProfilePoints;

      StringBuilder sb = new StringBuilder();
      sb.Append($"After profile conversion: {profile.results.Count}");
      foreach (var cell in profile.results)
      {
        sb.Append($",{cell.cellType}");
      }

      log.LogDebug(sb.ToString());
      return profile;
    }

    /// <summary>
    /// Add mid points between the cell edge intersections. This is because the profile line is plotted using these points.
    /// The cell edges are retained as this is where the color changes on the graph or chart.
    /// </summary>
    /// <param name="profileResult">The profile containing the list of cell edge points from Raptor</param>
    /// <returns>The complete list of interspersed edges and  mid points.</returns>
    private void AddMidPoints(CompactionProfileResult<CompactionProfileCell> profileResult)
    {
      log.LogDebug("Adding midpoints");
      if (profileResult.results.Count > 3)
      {
        //No mid point for first and last segments since only partial across the cell.
        //We have already added them as mid points.
        var cells = new List<CompactionProfileCell>();

        cells.Add(profileResult.results[0]);
        for (int i = 1; i < profileResult.results.Count - 2; i++)
        {
          cells.Add(profileResult.results[i]);
          if (profileResult.results[i].cellType != ProfileCellType.Gap)
          {
            var midPoint = new CompactionProfileCell(profileResult.results[i]);
            midPoint.cellType = ProfileCellType.MidPoint;
            midPoint.station = profileResult.results[i].station +
                               (profileResult.results[i + 1].station - profileResult.results[i].station) / 2;
            cells.Add(midPoint);
          }
        }
        cells.Add(profileResult.results[profileResult.results.Count - 2]);
        cells.Add(profileResult.results[profileResult.results.Count - 1]);
        profileResult.results = cells;
      }

      StringBuilder sb = new StringBuilder();
      sb.Append($"After adding midpoints: {profileResult.results.Count}");
      foreach (var cell in profileResult.results)
      {
        sb.Append($",{cell.cellType}");
      }
      log.LogDebug(sb.ToString());
    }

    /// <summary>
    /// Since the profile line will be drawn between line segment mid points we need to interpolate the cell edge points to lie on these line segments.
    /// </summary>
    /// <param name="profileResult">The profile containing the list of line segment points, both edges and mid points.</param>
    private void InterpolateEdges(CompactionProfileResult<CompactionProfileCell> profileResult)
    {
      log.LogDebug("Interpolating edges");
      if (profileResult.results.Count > 3)
      {
        //First and last points are not gaps or edges. They're always the start and end of the profile line.
        for (int i = 1; i < profileResult.results.Count - 1; i++)
        {
          if (profileResult.results[i].cellType == ProfileCellType.Edge || profileResult.results[i].cellType == ProfileCellType.Gap)
          {
            int startIndx, endIndx;
            var heightTypes = (HeightType[])Enum.GetValues(typeof(HeightType));
            foreach (var heightType in heightTypes)
            {
              FindMidPoints(i, heightType, profileResult.results, out startIndx, out endIndx);
              log.LogDebug($"Edge {i}: Midpoints: {startIndx}, {endIndx} for heightType {heightType}");
              if (startIndx >= 0 && endIndx <= profileResult.results.Count - 1)
              {
                InterpolateElevation(profileResult.results[i], profileResult.results[startIndx],
                  profileResult.results[endIndx], heightType);
              }
              else
              {
                //Special case: If all NaN to the LHS try and find 2 mid points to the RHS and extrapolate
                if (endIndx < profileResult.results.Count - 1)
                {
                  startIndx = endIndx;
                  int startIndx2, endIndx2;
                  FindMidPoints(endIndx, heightType, profileResult.results, out startIndx2, out endIndx2);
                  log.LogDebug($"Special Case Edge {i}: Midpoints: {startIndx}, {endIndx2} for heightType {heightType}");
                  if (endIndx2 <= profileResult.results.Count - 1)
                  {
                    InterpolateElevation(profileResult.results[i], profileResult.results[startIndx],
                      profileResult.results[endIndx2], heightType);
                  }
                }
              }
            }
          }
        }
      }
      log.LogDebug("After interpolation");
    }

    /// <summary>
    /// Finds the mid points each side of an edge to use for interpolation
    /// </summary>
    /// <param name="indx">The index of the edge point</param>
    /// <param name="heightType">The type of height to be interpolated</param>
    /// <param name="cells">The list of cells</param>
    /// <param name="startIndx">The index of the mid point before the edge</param>
    /// <param name="endIndx">The index of the mid point after the edge</param>
    private void FindMidPoints(int indx, HeightType heightType, List<CompactionProfileCell> cells, out int startIndx, out int endIndx)
    {
      startIndx = indx;
      bool found = false;
      while (startIndx >= 0 && !found)
      {
        found = HeightHasValue(HeightType.FirstPass, cells[startIndx]);
        if (!found) startIndx--;
      }
      endIndx = indx;
      found = false;
      while (endIndx < cells.Count && !found)
      {
        found = HeightHasValue(HeightType.FirstPass, cells[endIndx]);
        if (!found) endIndx++;
      }
    }

    /// <summary>
    /// Determine if the current cell has an elevation for the specified type of height
    /// </summary>
    /// <param name="heightType">The type of height to check</param>
    /// <param name="cell">The cell to check</param>
    /// <returns>True if the cell has a non-NaN elevation value for the specified height type</returns>
    private bool HeightHasValue(HeightType heightType, CompactionProfileCell cell)
    {
      if (cell.cellType == ProfileCellType.MidPoint)
      {
        switch (heightType)
        {
          case HeightType.FirstPass:
            return !float.IsNaN(cell.firstPassHeight);
          case HeightType.HighestPass:
            return !float.IsNaN(cell.highestPassHeight);
          case HeightType.LastPass:
            return !float.IsNaN(cell.lastPassHeight); 
          case HeightType.LowestPass:
            return !float.IsNaN(cell.lowestPassHeight); 
          case HeightType.LastComposite:
            return !float.IsNaN(cell.lastCompositeHeight);
          case HeightType.Design:
            return !float.IsNaN(cell.designHeight);
          case HeightType.Cmv:
            return !float.IsNaN(cell.cmvHeight); 
          case HeightType.Mdp:
            return !float.IsNaN(cell.mdpHeight);  
          case HeightType.Temperature:
            return !float.IsNaN(cell.temperatureHeight);
          case HeightType.Speed:
            return !float.IsNaN(cell.speedHeight);    
        }
      }
      return false;
    }

    /// <summary>
    /// Interpolate elevation of the specified height type for the specified point (cell) on the line segment from startCell to endCell
    /// </summary>
    /// <param name="cell">The point to interpolate</param>
    /// <param name="startCell">The start of the line segment</param>
    /// <param name="endCell">The end of the line segment</param>
    /// <param name="heightType">The type of height to interpolate</param>
    private void InterpolateElevation(CompactionProfileCell cell, CompactionProfileCell startCell, CompactionProfileCell endCell, HeightType heightType)
    {
      var proportion = (cell.station - startCell.station) / (endCell.station - startCell.station);

      switch (heightType)
      {
          case HeightType.FirstPass:
          cell.firstPassHeight = InterpolateElevation(proportion, startCell.firstPassHeight, endCell.firstPassHeight);
            break;
        case HeightType.HighestPass:
          cell.highestPassHeight = InterpolateElevation(proportion, startCell.highestPassHeight, endCell.highestPassHeight);
          break;
        case HeightType.LastPass:
          cell.lastPassHeight = InterpolateElevation(proportion, startCell.lastPassHeight, endCell.lastPassHeight);
          break;
        case HeightType.LowestPass:
          cell.lowestPassHeight = InterpolateElevation(proportion, startCell.lowestPassHeight, endCell.lowestPassHeight); break;
        case HeightType.LastComposite:
          cell.lastCompositeHeight = InterpolateElevation(proportion, startCell.lastCompositeHeight, endCell.lastCompositeHeight);
          break;
        case HeightType.Design:
          cell.designHeight = InterpolateElevation(proportion, startCell.designHeight, endCell.designHeight); //TODO: Should this be interpolated?
          break;
        case HeightType.Cmv:
          cell.cmvHeight = InterpolateElevation(proportion, startCell.cmvHeight, endCell.cmvHeight);
          break;
        case HeightType.Mdp:
          cell.mdpHeight = InterpolateElevation(proportion, startCell.mdpHeight, endCell.mdpHeight);
          break;
        case HeightType.Temperature:
          cell.temperatureHeight = InterpolateElevation(proportion, startCell.temperatureHeight, endCell.temperatureHeight);
          break;
        case HeightType.Speed:
          cell.speedHeight = InterpolateElevation(proportion, startCell.speedHeight, endCell.speedHeight);
          break;
      }
      log.LogDebug($"Interpolated station {cell.station} of cell type {cell.cellType} for height type {heightType}");
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
      cellType = ProfileCellType.Gap,
      station = 0,//Will be set for individual gap vertices
      firstPassHeight = float.NaN,
      highestPassHeight = float.NaN,
      lastPassHeight = float.NaN,
      lowestPassHeight = float.NaN,
      lastCompositeHeight = float.NaN,
      designHeight = float.NaN,
      cmv = float.NaN,
      cmvPercent = float.NaN,
      cmvHeight = float.NaN,
      mdpPercent = float.NaN,
      mdpHeight = float.NaN,
      temperature = float.NaN,
      temperatureHeight = float.NaN,
      topLayerPassCount = -1,
      cmvPercentChange = float.NaN,
      minSpeed = float.NaN,
      maxSpeed = float.NaN,
      passCountIndex = ValueTargetType.NoData,
      temperatureIndex = ValueTargetType.NoData,
      cmvIndex = ValueTargetType.NoData,
      mdpIndex = ValueTargetType.NoData,
      speedIndex = ValueTargetType.NoData,
    };

    /// <summary>
    /// Type of height in cell for interpolation
    /// </summary>
    public enum HeightType
    {
      FirstPass,
      HighestPass,
      LastPass,
      LowestPass,
      LastComposite,
      Design,
      Cmv,
      Mdp,
      Temperature,
      Speed
    }

  }
}