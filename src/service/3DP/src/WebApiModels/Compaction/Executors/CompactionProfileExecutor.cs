using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
#if RAPTOR
using SVOICOptionsDecls;
using SVOICProfileCell;
using SVOICSummaryVolumesProfileCell;
using SVOICVolumeCalculationsDecls;
using VSS.Velociraptor.PDSInterface;
#endif
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
using VSS.Productivity3D.Models.Utilities;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.Compaction.Models;
using SummaryVolumeProfileCell = VSS.Productivity3D.Models.ResultHandling.Profiling.SummaryVolumesProfileCell;

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
        var request = CastRequestObjectTo<CompactionProfileProductionDataRequest>(item);
        var totalResult = ProcessProductionData(request);
        var summaryVolumesResult = ProcessSummaryVolumes(request, totalResult);

        if (summaryVolumesResult != null)
        {
          totalResult.results.Add(summaryVolumesResult);
        }

        profileResultHelper.RemoveRepeatedNoData(totalResult, request.VolumeCalcType);
        profileResultHelper.AddMidPoints(totalResult);
        profileResultHelper.InterpolateEdges(totalResult, request.VolumeCalcType);

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
    private CompactionProfileResult<CompactionProfileDataResult> ProcessProductionData(CompactionProfileProductionDataRequest request)
    {
      CompactionProfileResult<CompactionProfileDataResult> totalResult;

      var productionDataProfileResult =
#if RAPTOR
          UseTRexGateway("ENABLE_TREX_GATEWAY_PROFILING") ?
#endif
        ProcessProductionDataWithTRexGateway(request)
#if RAPTOR
            : ProcessProductionDataWithRaptor(request)
#endif
        ;

      if (productionDataProfileResult != null)
        totalResult = profileResultHelper.RearrangeProfileResult(productionDataProfileResult);
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

#if RAPTOR
    /// <summary>
    /// Convert Raptor data to the data to return from the Web API
    /// </summary>
    /// <param name="ms">Memory stream of data from Raptor</param>
    /// <param name="liftBuildSettings">Lift build settings from project settings used in the Raptor profile calculations</param>
    /// <returns>The profile data</returns>
    private CompactionProfileResult<CompactionProfileCell> ConvertProfileResult(MemoryStream ms, LiftBuildSettings liftBuildSettings)
    {
      log.LogDebug("Converting profile result");

      var pdsiProfile = new PDSProfile();
      var packager = new TICProfileCellListPackager();
      packager.CellList = new TICProfileCellList();

      packager.ReadFromStream(ms);

      ms.Close();

      pdsiProfile.Assign(packager.CellList);

      pdsiProfile.GridDistanceBetweenProfilePoints = packager.GridDistanceBetweenProfilePoints;

      var profileCells = pdsiProfile.cells.Select(c => new ProfileCellData(
        c.station,
        c.interceptLength,
        c.firstPassHeight,
        c.lastPassHeight,
        c.lowestPassHeight,
        c.highestPassHeight,
        c.compositeFirstPassHeight,
        c.compositeLastPassHeight,
        c.compositeLowestPassHeight,
        c.compositeHighestPassHeight,
        c.designHeight,
        c.CCV,
        c.TargetCCV,
        c.CCVElev,
        c.PrevCCV,
        c.PrevTargetCCV,
        c.MDP,
        c.TargetMDP,
        c.MDPElev,
        c.materialTemperature,
        c.materialTemperatureWarnMin,
        c.materialTemperatureWarnMax,
        c.materialTemperatureElev,
        c.topLayerThickness,
        c.topLayerPassCount,
        c.topLayerPassCountTargetRange.Min,
        c.topLayerPassCountTargetRange.Max,
        c.cellMinSpeed,
        c.cellMaxSpeed
      )).ToList();

      return ProcessProductionDataProfileCells(profileCells, pdsiProfile.GridDistanceBetweenProfilePoints, liftBuildSettings);
    }
#endif

    private CompactionProfileResult<CompactionProfileCell> ProcessProductionDataWithTRexGateway(CompactionProfileProductionDataRequest request)
    {
      if (request.IsAlignmentDesign)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));

      var productionDataProfileDataRequest = new ProductionDataProfileDataRequest(
        request.ProjectUid ?? Guid.Empty,
        request.BaseFilter,
        request.VolumeDesignDescriptor?.FileUid,
        request.GridPoints != null,
        request.GridPoints?.x1 ?? (request.WGS84Points?.lon1 ?? 0.0),
        request.GridPoints?.y1 ?? (request.WGS84Points?.lat1 ?? 0.0),
        request.GridPoints?.x2 ?? (request.WGS84Points?.lon2 ?? 0.0),
        request.GridPoints?.y2 ?? (request.WGS84Points?.lat2 ?? 0.0),
        request.ReturnAllPassesAndLayers
      );

      var trexResult = trexCompactionDataProxy.SendDataPostRequest<ProfileDataResult<ProfileCellData>, ProductionDataProfileDataRequest>(productionDataProfileDataRequest, "/productiondata/profile", customHeaders).Result;

      return trexResult != null && trexResult.HasData() ? ConvertTRexProductioDataProfileResult(trexResult, request.LiftBuildSettings) : null;
    }

#if RAPTOR
    private CompactionProfileResult<CompactionProfileCell> ProcessProductionDataWithRaptor(
      CompactionProfileProductionDataRequest request)
    {
      MemoryStream memoryStream;

      var filter = RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient);
      var designDescriptor = RaptorConverters.DesignDescriptor(request.CutFillDesignDescriptor);
      var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.AlignmentDesign);
      var liftBuildSettings =
        RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone);
      ProfilesHelper.ConvertProfileEndPositions(request.GridPoints, request.WGS84Points, out var startPt, out var endPt,
        out var positionsAreGrid);

      CompactionProfileResult<CompactionProfileDataResult> totalResult = null;
      if (request.IsAlignmentDesign)
      {
        var args
          = ASNode.RequestAlignmentProfile.RPC.__Global.Construct_RequestAlignmentProfile_Args
          (request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
            ProfilesHelper.PROFILE_TYPE_NOT_REQUIRED,
            request.StartStation ?? ValidationConstants3D.MIN_STATION,
            request.EndStation ?? ValidationConstants3D.MIN_STATION,
            alignmentDescriptor,
            filter,
            liftBuildSettings,
            designDescriptor,
            request.ReturnAllPassesAndLayers);

        memoryStream = raptorClient.GetAlignmentProfile(args);
      }
      else
      {
        var args
          = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args
          (request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
            ProfilesHelper.PROFILE_TYPE_HEIGHT,
            positionsAreGrid,
            startPt,
            endPt,
            filter,
            liftBuildSettings,
            designDescriptor,
            request.ReturnAllPassesAndLayers);

        memoryStream = raptorClient.GetProfile(args);
      }

      return memoryStream != null ? ConvertProfileResult(memoryStream, request.LiftBuildSettings) : null;
    }
#endif

    /// <summary>
    /// Convert TRex data to the data to return from the Web API
    /// </summary>
    /// <param name="trexResult">Result data from TRex.</param>
    /// <param name="calcType"></param>
    /// <returns>The profile data</returns>
    private CompactionProfileResult<CompactionProfileCell> ConvertTRexProductioDataProfileResult(ProfileDataResult<ProfileCellData> trexResult, LiftBuildSettings liftBuildSettings)
    {
      log.LogDebug("Converting production data profile TRex result");

      return ProcessProductionDataProfileCells(trexResult.ProfileCells, trexResult.GridDistanceBetweenProfilePoints, liftBuildSettings);
    }

    private CompactionProfileResult<CompactionProfileCell> ProcessProductionDataProfileCells(List<ProfileCellData> profileCells, double gridDistanceBetweenProfilePoints, LiftBuildSettings liftBuildSettings)
    {
      var profile = new CompactionProfileResult<CompactionProfileCell>();
      profile.results = new List<CompactionProfileCell>();
      ProfileCellData prevCell = null;

      foreach (var currCell in profileCells)
      {
        var gapExists = ProfilesHelper.CellGapExists(prevCell, currCell, out double prevStationIntercept);

        if (gapExists)
        {
          var gapCell = new CompactionProfileCell(GapCell);
          gapCell.station = prevStationIntercept;
          profile.results.Add(gapCell);
        }

        var lastPassHeight = currCell.LastPassHeight == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.LastPassHeight;
        var lastCompositeHeight = currCell.CompositeLastPassHeight == VelociraptorConstants.NULL_SINGLE
        ? float.NaN
          : currCell.CompositeLastPassHeight;

        var designHeight = currCell.DesignHeight == VelociraptorConstants.NULL_SINGLE
        ? float.NaN
          : currCell.DesignHeight;
        bool noCCVValue = currCell.TargetCCV == 0 || currCell.TargetCCV == VelociraptorConstants.NO_CCV ||
                          currCell.CCV == VelociraptorConstants.NO_CCV;
        bool noCCVElevation = currCell.CCVElev == VelociraptorConstants.NULL_SINGLE || noCCVValue;
        bool noMDPValue = currCell.TargetMDP == 0 || currCell.TargetMDP == VelociraptorConstants.NO_MDP ||
                          currCell.MDP == VelociraptorConstants.NO_MDP;
        bool noMDPElevation = currCell.MDPElev == VelociraptorConstants.NULL_SINGLE || noMDPValue;
        bool noTemperatureValue = currCell.MaterialTemperature == VelociraptorConstants.NO_TEMPERATURE;
        bool noTemperatureElevation = currCell.MaterialTemperatureElev == VelociraptorConstants.NULL_SINGLE ||
                                      noTemperatureValue;
        bool noPassCountValue = currCell.TopLayerPassCount == VelociraptorConstants.NO_PASSCOUNT;

        //Either have none or both speed values
        var noSpeedValue = currCell.CellMaxSpeed == VelociraptorConstants.NO_SPEED;
        var speedMin = noSpeedValue ? float.NaN : (float)(currCell.CellMinSpeed / ConversionConstants.KM_HR_TO_CM_SEC);
        var speedMax = noSpeedValue ? float.NaN : (float)(currCell.CellMaxSpeed / ConversionConstants.KM_HR_TO_CM_SEC);

        var cmvPercent = noCCVValue
          ? float.NaN
          : (float)currCell.CCV / (float)currCell.TargetCCV * 100.0F;

        var mdpPercent = noMDPValue
          ? float.NaN
          : (float)currCell.MDP / (float)currCell.TargetMDP * 100.0F;

        var firstPassHeight = currCell.FirstPassHeight == VelociraptorConstants.NULL_SINGLE
        ? float.NaN
          : currCell.FirstPassHeight;

        var highestPassHeight = currCell.HighestPassHeight == VelociraptorConstants.NULL_SINGLE
        ? float.NaN
          : currCell.HighestPassHeight;

        var lowestPassHeight = currCell.LowestPassHeight == VelociraptorConstants.NULL_SINGLE
        ? float.NaN
          : currCell.LowestPassHeight;

        var cutFill = float.IsNaN(lastCompositeHeight) || float.IsNaN(designHeight)
          ? float.NaN
          : lastCompositeHeight - designHeight;

        var cmv = noCCVValue ? float.NaN : currCell.CCV / 10.0F;
        var cmvHeight = noCCVElevation ? float.NaN : currCell.CCVElev;
        var mdpHeight = noMDPElevation ? float.NaN : currCell.MDPElev;
        var temperature =
          noTemperatureValue
            ? float.NaN
            : currCell.MaterialTemperature / 10.0F; // As temperature is reported in 10th...
        var temperatureHeight = noTemperatureElevation ? float.NaN : currCell.MaterialTemperatureElev;
        var topLayerPassCount = noPassCountValue ? -1 : currCell.TopLayerPassCount;
        var cmvPercentChange = currCell.CCV == VelociraptorConstants.NO_CCV
          ? float.NaN
          : (currCell.PrevCCV == VelociraptorConstants.NO_CCV
            ? 100.0f
            : (float)(currCell.CCV - currCell.PrevCCV) / (float)currCell.PrevCCV * 100.0f);

        var passCountIndex = noPassCountValue || float.IsNaN(lastPassHeight)
          ? ValueTargetType.NoData
          : (currCell.TopLayerPassCount < currCell.TopLayerPassCountTargetRangeMin
            ? ValueTargetType.BelowTarget
            : (currCell.TopLayerPassCount > currCell.TopLayerPassCountTargetRangeMax
              ? ValueTargetType.AboveTarget
              : ValueTargetType.OnTarget));

        var temperatureIndex = noTemperatureValue || noTemperatureElevation
          ? ValueTargetType.NoData
          : (currCell.MaterialTemperature < currCell.MaterialTemperatureWarnMin
            ? ValueTargetType.BelowTarget
            : (currCell.MaterialTemperature > currCell.MaterialTemperatureWarnMax
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
          : (currCell.CellMaxSpeed > liftBuildSettings.MachineSpeedTarget.MaxTargetMachineSpeed
            ? ValueTargetType.AboveTarget
            : (currCell.CellMinSpeed < liftBuildSettings.MachineSpeedTarget.MinTargetMachineSpeed &&
               currCell.CellMaxSpeed < liftBuildSettings.MachineSpeedTarget.MinTargetMachineSpeed
              ? ValueTargetType.BelowTarget
              : ValueTargetType.OnTarget));

        profile.results.Add(new CompactionProfileCell
        {
          cellType = prevCell == null ? ProfileCellType.MidPoint : ProfileCellType.Edge,

          station = currCell.Station,

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
      if (prevCell != null && prevCell.InterceptLength > ProfilesHelper.ONE_MM)
      {
        var lastCell = new CompactionProfileCell(profile.results[profile.results.Count - 1])
        {
          station = prevCell.Station + prevCell.InterceptLength
        };

        profile.results.Add(lastCell);
      }

      if (profile.results.Count > 0)
        profile.results[profile.results.Count - 1].cellType = ProfileCellType.MidPoint;

      profile.gridDistanceBetweenProfilePoints = gridDistanceBetweenProfilePoints;

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
      var volumesResult =
#if RAPTOR
      UseTRexGateway("ENABLE_TREX_GATEWAY_PROFILING") ?
#endif
          ProcessSummaryVolumesWithTRexGateway(request)
#if RAPTOR
          : ProcessSummaryVolumesWithRaptor(request)
#endif
        ;

      //If we have no other profile results apart from summary volumes, set the total grid distance
      if (totalResult.results.Count == 0 && volumesResult != null)
        totalResult.gridDistanceBetweenProfilePoints = volumesResult.gridDistanceBetweenProfilePoints;

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
      return profileResultHelper.RearrangeProfileResult(volumesResult, request.VolumeCalcType);
    }

    private CompactionProfileResult<CompactionSummaryVolumesProfileCell> ProcessSummaryVolumesWithTRexGateway(CompactionProfileProductionDataRequest request)
    {
      if (request.IsAlignmentDesign)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));

      var volumeCalcType = request.VolumeCalcType ?? VolumeCalcType.None;

      var summaryVolumesProfileDataRequest = new SummaryVolumesProfileDataRequest(
        request.ProjectUid ?? Guid.Empty,
        request.BaseFilter,
        request.TopFilter,
        request.VolumeDesignDescriptor?.FileUid,
        ConvertVolumeCalcType(volumeCalcType),
        request.GridPoints != null,
        request.GridPoints?.x1 ?? request.WGS84Points.lon1,
        request.GridPoints?.x2 ?? request.WGS84Points.lon2,
        request.GridPoints?.y1 ?? request.WGS84Points.lat1,
        request.GridPoints?.y2 ?? request.WGS84Points.lat2
      );

      var trexResult = trexCompactionDataProxy.SendDataPostRequest<ProfileDataResult<SummaryVolumeProfileCell>, SummaryVolumesProfileDataRequest>(summaryVolumesProfileDataRequest, "/volumes/summary/profile", customHeaders).Result;

      return trexResult != null ? ConvertTRexSummaryVolumesProfileResult(trexResult, volumeCalcType) : null;
    }
#if RAPTOR
    private CompactionProfileResult<CompactionSummaryVolumesProfileCell> ProcessSummaryVolumesWithRaptor(CompactionProfileProductionDataRequest request)
    {
      var alignmentDescriptor = RaptorConverters.DesignDescriptor(request.AlignmentDesign);
      var liftBuildSettings =
        RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmNone);
      var baseFilter = RaptorConverters.ConvertFilter(request.BaseFilter, request.ProjectId, raptorClient);
      var topFilter = RaptorConverters.ConvertFilter(request.TopFilter, request.ProjectId, raptorClient);
      var volumeDesignDescriptor = RaptorConverters.DesignDescriptor(request.VolumeDesignDescriptor);
      ProfilesHelper.ConvertProfileEndPositions(request.GridPoints, request.WGS84Points, out var startPt, out var endPt,
        out var positionsAreGrid);

      if (request.VolumeCalcType.HasValue && request.VolumeCalcType.Value != VolumeCalcType.None)
      {
        var volCalcType = (TComputeICVolumesType)request.VolumeCalcType.Value;
        if (volCalcType == TComputeICVolumesType.ic_cvtBetween2Filters && !request.ExplicitFilters)
        {
          RaptorConverters.AdjustFilterToFilter(ref baseFilter, topFilter);
        }

        MemoryStream memoryStream;
        if (request.IsAlignmentDesign)
        {
          var args
              = ASNode.RequestSummaryVolumesAlignmentProfile.RPC.__Global
                .Construct_RequestSummaryVolumesAlignmentProfile_Args
                (request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
                  ProfilesHelper.PROFILE_TYPE_NOT_REQUIRED,
                  volCalcType,
                  request.StartStation ?? ValidationConstants3D.MIN_STATION,
                  request.EndStation ?? ValidationConstants3D.MIN_STATION,
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
              request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
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

        return memoryStream != null ? ConvertSummaryVolumesProfileResult(memoryStream, request.VolumeCalcType.Value) : null;
      }

      return null;
    }
#endif

    /// <summary>
    /// Convert TRex data to the data to return from the Web API
    /// </summary>
    /// <param name="trexResult">Result data from TRex.</param>
    /// <param name="calcType"></param>
    /// <returns>The profile data</returns>
    private CompactionProfileResult<CompactionSummaryVolumesProfileCell> ConvertTRexSummaryVolumesProfileResult(ProfileDataResult<SummaryVolumeProfileCell> trexResult, VolumeCalcType calcType)
    {
      log.LogDebug("Converting summary volumes profile TRex result");

      return ProcessSummaryVolumesProfileCells(trexResult.ProfileCells, trexResult.GridDistanceBetweenProfilePoints, calcType); ;
    }
#if RAPTOR
    /// <summary>
    /// Convert Raptor data to the data to return from the Web API
    /// </summary>
    /// <param name="ms">Memory stream of data from Raptor</param>
    /// <param name="calcType">Volume calculation type.</param>
    /// <returns>The profile data</returns>
    private CompactionProfileResult<CompactionSummaryVolumesProfileCell> ConvertSummaryVolumesProfileResult(MemoryStream ms, VolumeCalcType calcType)
    {
      log.LogDebug("Converting summary volumes profile Raptor result");

      var pdsiProfile = new PDSSummaryVolumesProfile();
      var packager = new TICSummaryVolumesProfileCellListPackager();
      packager.CellList = new TICSummaryVolumesProfileCellList();

      packager.ReadFromStream(ms);

      ms.Close();

      pdsiProfile.Assign(packager.CellList);

      pdsiProfile.GridDistanceBetweenProfilePoints = packager.GridDistanceBetweenProfilePoints;

      var profileCells = pdsiProfile.Cells.Select(c => new SummaryVolumeProfileCell(
        c.station,
        c.interceptLength,
        (uint)c.OTGCellX,
        (uint)c.OTGCellY,
        c.designElevation,
        c.lastCellPassElevation1,
        c.lastCellPassElevation2
      )).ToList();

      return ProcessSummaryVolumesProfileCells(profileCells, pdsiProfile.GridDistanceBetweenProfilePoints, calcType);
    }
#endif

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

    private VolumesType ConvertVolumeCalcType(VolumeCalcType volumesType)
    {
      switch (volumesType)
      {
        case VolumeCalcType.None: return VolumesType.None;
        case VolumeCalcType.GroundToGround: return VolumesType.Between2Filters;
        case VolumeCalcType.GroundToDesign: return VolumesType.BetweenFilterAndDesign;
        case VolumeCalcType.DesignToGround: return VolumesType.BetweenDesignAndFilter;
        default: throw new Exception($"Unknown VolumeCalcType {Convert.ToInt16(volumesType)}");
      }
    }

    private CompactionProfileResult<CompactionSummaryVolumesProfileCell> ProcessSummaryVolumesProfileCells(List<SummaryVolumeProfileCell> profileCells, double gridDistanceBetweenProfilePoints, VolumeCalcType calcType)
    {
      var profile = new CompactionProfileResult<CompactionSummaryVolumesProfileCell>();

      profile.results = new List<CompactionSummaryVolumesProfileCell>();
      SummaryVolumeProfileCell prevCell = null;

      foreach (var currCell in profileCells)
      {
        var gapExists = ProfilesHelper.CellGapExists(prevCell, currCell, out var prevStationIntercept);

        if (gapExists)
        {
          var gapCell = new CompactionSummaryVolumesProfileCell(SumVolGapCell);
          gapCell.station = prevStationIntercept;
          profile.results.Add(gapCell);
        }

        var lastPassHeight1 = currCell.LastCellPassElevation1 == VelociraptorConstants.NULL_SINGLE
          ? float.NaN
          : currCell.LastCellPassElevation1;

        var lastPassHeight2 = currCell.LastCellPassElevation2 == VelociraptorConstants.NULL_SINGLE
        ? float.NaN
          : currCell.LastCellPassElevation2;

        var designHeight = currCell.DesignElev == VelociraptorConstants.NULL_SINGLE
        ? float.NaN
          : currCell.DesignElev;

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

          station = currCell.Station,

          lastPassHeight1 = lastPassHeight1,
          lastPassHeight2 = lastPassHeight2,
          designHeight = designHeight,
          cutFill = cutFill
        });

        prevCell = currCell;
      }

      //Add a last point at the intercept length of the last cell so profiles are drawn correctly
      if (prevCell != null && prevCell.InterceptLength > ProfilesHelper.ONE_MM)
      {
        var lastCell = new CompactionSummaryVolumesProfileCell(profile.results[profile.results.Count - 1])
        {
          station = prevCell.Station + prevCell.InterceptLength
        };

        profile.results.Add(lastCell);
      }

      if (profile.results.Count > 0)
        profile.results[profile.results.Count - 1].cellType = ProfileCellType.MidPoint;

      profile.gridDistanceBetweenProfilePoints = gridDistanceBetweenProfilePoints;

      var sb = new StringBuilder();
      sb.Append($"After summary volumes profile conversion: {profile.results.Count}");
      foreach (var cell in profile.results)
      {
        sb.Append($",{cell.cellType}");
      }

      log.LogDebug(sb.ToString());
      return profile;
    }
    #endregion
  }
}
