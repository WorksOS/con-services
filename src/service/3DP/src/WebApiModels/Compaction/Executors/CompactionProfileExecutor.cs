using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using SVOICOptionsDecls;
using SVOICProfileCell;
using SVOICSummaryVolumesProfileCell;
using SVOICVolumeCalculationsDecls;
using VLPDDecls;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Utilities;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using VSS.Velociraptor.PDSInterface;

namespace VSS.Productivity3D.WebApi.Models.Compaction.Executors
{
  /// <summary>
  /// Get production data profile calculations executor.
  /// </summary>
  public class CompactionProfileExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = item as CompactionProfileProductionDataRequest;

        if (request == null)
          ThrowRequestTypeCastException<CompactionProfileProductionDataRequest>();

        var totalResult = ProcessProductionData(request);
        var summaryVolumesResult = ProcessSummaryVolumes(request, totalResult);
        if (summaryVolumesResult != null)
        {
          totalResult.results.Add(summaryVolumesResult);
        }
    
        profileResultHelper.RemoveRepeatedNoData(totalResult, request.volumeCalcType);
        profileResultHelper.AddMidPoints(totalResult);
        profileResultHelper.InterpolateEdges(totalResult, request.volumeCalcType);

        return totalResult;
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

    #region Production Data

    /// <summary>
    /// Process the profile request to get production data profiles
    /// </summary>
    /// <param name="request">Profile request</param>
    /// <returns>Profile for each production data type except summary volumes</returns>
    private CompactionProfileResult<CompactionProfileDataResult> ProcessProductionData(
      CompactionProfileProductionDataRequest request)
    {
      MemoryStream memoryStream;

      var filter = RaptorConverters.ConvertFilter(request.Filter);
      var designDescriptor = RaptorConverters.DesignDescriptor(request.cutFillDesignDescriptor);
      var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.alignmentDesign);
      var liftBuildSettings =
        RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmNone);
      ProfilesHelper.ConvertProfileEndPositions(request.gridPoints, request.wgs84Points, out var startPt, out var endPt,
        out var positionsAreGrid);

      CompactionProfileResult<CompactionProfileDataResult> totalResult = null;
      if (request.IsAlignmentDesign)
      {
        var args
          = ASNode.RequestAlignmentProfile.RPC.__Global.Construct_RequestAlignmentProfile_Args
          (request.ProjectId ?? -1,
            ProfilesHelper.PROFILE_TYPE_NOT_REQUIRED,
            request.startStation ?? ValidationConstants3D.MIN_STATION,
            request.endStation ?? ValidationConstants3D.MIN_STATION,
            alignmentDescriptor,
            filter,
            liftBuildSettings,
            designDescriptor,
            request.returnAllPassesAndLayers);

        memoryStream = raptorClient.GetAlignmentProfile(args);
      }
      else
      {
        var args
          = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args
          (request.ProjectId ?? -1,
            ProfilesHelper.PROFILE_TYPE_HEIGHT,
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
        totalResult = profileResultHelper.RearrangeProfileResult(profileResult);
      }
      else
      {
        //For convenience return empty list rather than null for easier manipulation
        totalResult = new CompactionProfileResult<CompactionProfileDataResult>
        {
          results = new List<CompactionProfileDataResult>()
        };
      }
      return totalResult;
    }

    /// <summary>
    /// Convert Raptor data to the data to return from the Web API
    /// </summary>
    /// <param name="ms">Memory stream of data from Raptor</param>
    /// <param name="liftBuildSettings">Lift build settings from project settings used in the Raptor profile calculations</param>
    /// <returns>The profile data</returns>
    private CompactionProfileResult<CompactionProfileCell> ConvertProfileResult(MemoryStream ms,
      LiftBuildSettings liftBuildSettings)
    {
      log.LogDebug("Converting profile result");

      var profile = new CompactionProfileResult<CompactionProfileCell>();

      var pdsiProfile = new PDSProfile();
      var packager = new TICProfileCellListPackager();
      packager.CellList = new TICProfileCellList();
      packager.ReadFromStream(ms);
      pdsiProfile.Assign(packager.CellList);

      pdsiProfile.GridDistanceBetweenProfilePoints = packager.GridDistanceBetweenProfilePoints;

      profile.results = new List<CompactionProfileCell>();
      ProfileCell prevCell = null;
      foreach (var currCell in pdsiProfile.cells)
      {
        var gapExists = ProfilesHelper.CellGapExists(prevCell, currCell, out double prevStationIntercept);

        if (gapExists)
        {
          var gapCell = new CompactionProfileCell(GapCell);
          gapCell.station = prevStationIntercept;
          profile.results.Add(gapCell);
        }

        var lastPassHeight = currCell.lastPassHeight == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.lastPassHeight;
        var lastCompositeHeight = currCell.compositeLastPassHeight == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.compositeLastPassHeight;

        var designHeight = currCell.designHeight == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.designHeight;
        bool noCCVValue = currCell.TargetCCV == 0 || currCell.TargetCCV == VelociraptorConstants.NO_CCV ||
                          currCell.CCV == VelociraptorConstants.NO_CCV;
        bool noCCVElevation = currCell.CCVElev == VelociraptorConstants.NULL_SINGLE || noCCVValue;
        bool noMDPValue = currCell.TargetMDP == 0 || currCell.TargetMDP == VelociraptorConstants.NO_MDP ||
                          currCell.MDP == VelociraptorConstants.NO_MDP;
        bool noMDPElevation = currCell.MDPElev == VelociraptorConstants.NULL_SINGLE || noMDPValue;
        bool noTemperatureValue = currCell.materialTemperature == VelociraptorConstants.NO_TEMPERATURE;
        bool noTemperatureElevation = currCell.materialTemperatureElev == VelociraptorConstants.NULL_SINGLE ||
                                      noTemperatureValue;
        bool noPassCountValue = currCell.topLayerPassCount == VelociraptorConstants.NO_PASSCOUNT;

        //Either have none or both speed values
        var noSpeedValue = currCell.cellMaxSpeed == VelociraptorConstants.NO_SPEED;
        var speedMin = noSpeedValue ? float.NaN : (float) (currCell.cellMinSpeed / ConversionConstants.KM_HR_TO_CM_SEC);
        var speedMax = noSpeedValue ? float.NaN : (float) (currCell.cellMaxSpeed / ConversionConstants.KM_HR_TO_CM_SEC);

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
        var temperature =
          noTemperatureValue
            ? float.NaN
            : currCell.materialTemperature / 10.0F; // As temperature is reported in 10th...
        var temperatureHeight = noTemperatureElevation ? float.NaN : currCell.materialTemperatureElev;
        var topLayerPassCount = noPassCountValue ? -1 : currCell.topLayerPassCount;
        var cmvPercentChange = currCell.CCV == VelociraptorConstants.NO_CCV
          ? float.NaN
          : (currCell.PrevCCV == VelociraptorConstants.NO_CCV
            ? 100.0f
            : (float) (currCell.CCV - currCell.PrevCCV) / (float) currCell.PrevCCV * 100.0f);

        var passCountIndex = noPassCountValue || float.IsNaN(lastPassHeight)
          ? ValueTargetType.NoData
          : (currCell.topLayerPassCount < currCell.topLayerPassCountTargetRange.Min
            ? ValueTargetType.BelowTarget
            : (currCell.topLayerPassCount > currCell.topLayerPassCountTargetRange.Max
              ? ValueTargetType.AboveTarget
              : ValueTargetType.OnTarget));

        var temperatureIndex = noTemperatureValue || noTemperatureElevation
          ? ValueTargetType.NoData
          : (currCell.materialTemperature < currCell.materialTemperatureWarnMin
            ? ValueTargetType.BelowTarget
            : (currCell.materialTemperature > currCell.materialTemperatureWarnMax
              ? ValueTargetType.AboveTarget
              : ValueTargetType.OnTarget));

        var cmvIndex = noCCVValue || noCCVElevation
          ? ValueTargetType.NoData
          : (cmvPercent < liftBuildSettings.CCVRange.Min
            ? ValueTargetType.BelowTarget
            : (cmvPercent > liftBuildSettings.CCVRange.Max ? ValueTargetType.AboveTarget : ValueTargetType.OnTarget));

        var mdpIndex = noMDPValue || noMDPElevation
          ? ValueTargetType.NoData
          : (mdpPercent < liftBuildSettings.MDPRange.Min
            ? ValueTargetType.BelowTarget
            : (mdpPercent > liftBuildSettings.MDPRange.Max ? ValueTargetType.AboveTarget : ValueTargetType.OnTarget));

        var speedIndex = noSpeedValue || float.IsNaN(lastPassHeight)
          ? ValueTargetType.NoData
          : (currCell.cellMaxSpeed > liftBuildSettings.MachineSpeedTarget.MaxTargetMachineSpeed
            ? ValueTargetType.AboveTarget
            : (currCell.cellMinSpeed < liftBuildSettings.MachineSpeedTarget.MinTargetMachineSpeed &&
               currCell.cellMaxSpeed < liftBuildSettings.MachineSpeedTarget.MinTargetMachineSpeed
              ? ValueTargetType.BelowTarget
              : ValueTargetType.OnTarget));

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

          passCountIndex = passCountIndex,
          temperatureIndex = temperatureIndex,
          cmvIndex = cmvIndex,
          mdpIndex = mdpIndex,
          speedIndex = speedIndex
        });

        prevCell = currCell;
      }

      //Add a last point at the intercept length of the last cell so profiles are drawn correctly
      if (prevCell != null && prevCell.interceptLength > ProfilesHelper.ONE_MM)
      {
        var lastCell = new CompactionProfileCell(profile.results[profile.results.Count - 1])
        {
          station = prevCell.station + prevCell.interceptLength
        };

        profile.results.Add(lastCell);
      }
      profile.results[profile.results.Count-1].cellType = ProfileCellType.MidPoint;


      ms.Close();

      profile.gridDistanceBetweenProfilePoints = pdsiProfile.GridDistanceBetweenProfilePoints;

      var sb = new StringBuilder();
      sb.Append($"After profile conversion: {profile.results.Count}");
      foreach (var cell in profile.results)
      {
        sb.Append($",{cell.cellType}");
      }

      log.LogDebug(sb.ToString());
      return profile;
    }

    /// <summary>
    /// Representation of a profile cell edge at the start of a gap (no data)
    /// </summary>
    private readonly CompactionProfileCell GapCell = new CompactionProfileCell
    {
      cellType = ProfileCellType.Gap,
      station = 0, //Will be set for individual gap vertices
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
    #endregion

    #region Summary Volumes
    /// <summary>
    /// Process profile request to get summary volumes profile
    /// </summary>
    /// <param name="request">Profile request</param>
    /// <param name="totalResult">Results for other production data profile types</param>
    /// <returns>Summary volumes profile</returns>
    private CompactionProfileDataResult ProcessSummaryVolumes(CompactionProfileProductionDataRequest request, CompactionProfileResult<CompactionProfileDataResult> totalResult)
    {
      var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.alignmentDesign);
      var liftBuildSettings =
        RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmNone);
      var baseFilter = RaptorConverters.ConvertFilter(request.baseFilter);
      var topFilter = RaptorConverters.ConvertFilter(request.topFilter);
      var volumeDesignDescriptor = RaptorConverters.DesignDescriptor(request.volumeDesignDescriptor);
      ProfilesHelper.ConvertProfileEndPositions(request.gridPoints, request.wgs84Points, out var startPt, out var endPt,
        out var positionsAreGrid);

      CompactionProfileResult<CompactionSummaryVolumesProfileCell> volumesResult = null;
      if (request.volumeCalcType.HasValue && request.volumeCalcType.Value != VolumeCalcType.None)
      {
        var volCalcType = (TComputeICVolumesType) request.volumeCalcType.Value;
        if (volCalcType == TComputeICVolumesType.ic_cvtBetween2Filters)
        {
          RaptorConverters.AdjustFilterToFilter(ref baseFilter, topFilter);
        }

        MemoryStream memoryStream;
        if (request.IsAlignmentDesign)
        {
          var args
              = ASNode.RequestSummaryVolumesAlignmentProfile.RPC.__Global
                .Construct_RequestSummaryVolumesAlignmentProfile_Args
                (request.ProjectId ?? -1,
                  ProfilesHelper.PROFILE_TYPE_NOT_REQUIRED,
                  volCalcType,
                  request.startStation ?? ValidationConstants3D.MIN_STATION,
                  request.endStation ?? ValidationConstants3D.MIN_STATION,
                  alignmentDescriptor,
                  baseFilter,
                  topFilter,
                  liftBuildSettings,
                  volumeDesignDescriptor);

          memoryStream = raptorClient.GetSummaryVolumesAlignmentProfile(args);
        }
        else
        {
          var args
            = ASNode.RequestSummaryVolumesProfile.RPC.__Global.Construct_RequestSummaryVolumesProfile_Args(
              (request.ProjectId ?? -1),
              ProfilesHelper.PROFILE_TYPE_HEIGHT,
              volCalcType,
              startPt,
              endPt,
              positionsAreGrid,
              baseFilter,
              topFilter,
              liftBuildSettings,
              volumeDesignDescriptor);
          memoryStream = raptorClient.GetSummaryVolumesProfile(args);
        }
        if (memoryStream != null)
        {
          volumesResult = ConvertSummaryVolumesProfileResult(memoryStream, request.volumeCalcType.Value);
          //If we have no other profile results apart from summary volumes, set the total grid distance
          if (totalResult.results.Count == 0 && volumesResult != null)
          {
            totalResult.gridDistanceBetweenProfilePoints = volumesResult.gridDistanceBetweenProfilePoints;
          }
        }
      }
      //If we have other profile types but no summary volumes, add summary volumes with just slicer end points
      if (volumesResult == null && totalResult.results.Count > 0)
      {
        var startSlicer = new CompactionSummaryVolumesProfileCell(SumVolGapCell);
        var endSlicer = new CompactionSummaryVolumesProfileCell(SumVolGapCell);
        endSlicer.station = totalResult.gridDistanceBetweenProfilePoints;
        volumesResult =         
            new CompactionProfileResult<CompactionSummaryVolumesProfileCell>
            {
              gridDistanceBetweenProfilePoints = totalResult.gridDistanceBetweenProfilePoints,
              results = new List<CompactionSummaryVolumesProfileCell>
              {
                startSlicer,
                endSlicer
              }
            };
      }
      return profileResultHelper.RearrangeProfileResult(volumesResult, request.volumeCalcType); 
    }

    /// <summary>
    /// Convert Raptor data to the data to return from the Web API
    /// </summary>
    /// <param name="ms">Memory stream of data from Raptor</param>
    /// <returns>The profile data</returns>
    private CompactionProfileResult<CompactionSummaryVolumesProfileCell> ConvertSummaryVolumesProfileResult(MemoryStream ms, VolumeCalcType calcType)
    {
      log.LogDebug("Converting summary volumes profile result");

      var profile = new CompactionProfileResult<CompactionSummaryVolumesProfileCell>();

      var pdsiProfile = new PDSSummaryVolumesProfile();
      var packager = new TICSummaryVolumesProfileCellListPackager();
      packager.CellList = new TICSummaryVolumesProfileCellList();
      packager.ReadFromStream(ms);
      pdsiProfile.Assign(packager.CellList);

      pdsiProfile.GridDistanceBetweenProfilePoints = packager.GridDistanceBetweenProfilePoints;

      profile.results = new List<CompactionSummaryVolumesProfileCell>();
      SummaryVolumesProfileCell prevCell = null;

      foreach (var currCell in pdsiProfile.Cells)
      {
        var gapExists = ProfilesHelper.CellGapExists(prevCell, currCell, out double prevStationIntercept);

        if (gapExists)
        {
          var gapCell = new CompactionSummaryVolumesProfileCell(SumVolGapCell);
          gapCell.station = prevStationIntercept;
          profile.results.Add(gapCell);
        }

        var lastPassHeight1 = currCell.lastCellPassElevation1 == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.lastCellPassElevation1;

        var lastPassHeight2 = currCell.lastCellPassElevation2 == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.lastCellPassElevation2;

        var designHeight = currCell.designElevation == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.designElevation;

        float cutFill = float.NaN;
        switch (calcType)
        {
           case VolumeCalcType.GroundToGround:
            cutFill = float.IsNaN(lastPassHeight1) || float.IsNaN(lastPassHeight2)
              ? float.NaN
              : lastPassHeight2 - lastPassHeight1;
            break;
          case VolumeCalcType.GroundToDesign:
            cutFill = float.IsNaN(lastPassHeight1) || float.IsNaN(designHeight)
              ? float.NaN
              : designHeight - lastPassHeight1;
            break;
          case VolumeCalcType.DesignToGround:
            cutFill = float.IsNaN(designHeight) || float.IsNaN(lastPassHeight2)
              ? float.NaN
              : lastPassHeight2 - designHeight;
            break;
        }

        profile.results.Add(new CompactionSummaryVolumesProfileCell
        {
          cellType = prevCell == null ? ProfileCellType.MidPoint : ProfileCellType.Edge,

          station = currCell.station,

          lastPassHeight1 = lastPassHeight1,
          lastPassHeight2 = lastPassHeight2,
          designHeight = designHeight,
          cutFill = cutFill
        });

        prevCell = currCell;
      }

      //Add a last point at the intercept length of the last cell so profiles are drawn correctly
      if (prevCell != null && prevCell.interceptLength > ProfilesHelper.ONE_MM)
      {
        var lastCell = new CompactionSummaryVolumesProfileCell(profile.results[profile.results.Count - 1])
        {
          station = prevCell.station + prevCell.interceptLength
        };

        profile.results.Add(lastCell);
      }
      profile.results[profile.results.Count-1].cellType = ProfileCellType.MidPoint;

      ms.Close();

      profile.gridDistanceBetweenProfilePoints = pdsiProfile.GridDistanceBetweenProfilePoints;

      var sb = new StringBuilder();
      sb.Append($"After summary volumes profile conversion: {profile.results.Count}");
      foreach (var cell in profile.results)
      {
        sb.Append($",{cell.cellType}");
      }

      log.LogDebug(sb.ToString());
      return profile;
    }

    /// <summary>
    /// Representation of a profile cell edge at the start of a gap (no data) for summary volumes
    /// </summary>
    private readonly CompactionSummaryVolumesProfileCell SumVolGapCell = new CompactionSummaryVolumesProfileCell
    {
      cellType = ProfileCellType.Gap,
      station = 0, //Will be set for individual gap vertices
      designHeight = float.NaN,
      lastPassHeight1 = float.NaN,
      lastPassHeight2 = float.NaN,
      cutFill = float.NaN
    };

    #endregion
  }
}
