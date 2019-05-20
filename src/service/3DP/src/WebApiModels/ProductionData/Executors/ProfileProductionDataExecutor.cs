using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
#if RAPTOR
using SVOICOptionsDecls;
using SVOICProfileCell;
using VLPDDecls;
using VSS.Velociraptor.PDSInterface;
#endif
using VSS.Common.Exceptions;
using VSS.MasterData.Models.Converters;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.Models.Profiling;
using VSS.Productivity3D.Models.ResultHandling.Profiling;
using VSS.Productivity3D.Models.Utilities;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using ProfileCell = VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling.ProfileCell;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Get production data profile calculations executor.
  /// </summary>
  public class ProfileProductionDataExecutor : RequestExecutorContainer
  {
    private ProfileResult PerformTRexProductionDataProfilePost(ProfileProductionDataRequest request)
    {
      if (request.IsAlignmentDesign)
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "TRex unsupported request"));

      var productionDataProfileDataRequest = new ProductionDataProfileDataRequest(
        request.ProjectUid ?? Guid.Empty,
        request.Filter,
        request.AlignmentDesign?.FileUid,
        request.AlignmentDesign?.Offset,
        request.GridPoints != null,
        request.GridPoints?.x1 ?? request.WGS84Points.lon1,
        request.GridPoints?.x2 ?? request.WGS84Points.lon2,
        request.GridPoints?.y1 ?? request.WGS84Points.lat1,
        request.GridPoints?.y2 ?? request.WGS84Points.lat2,
        request.ReturnAllPassesAndLayers
      );

      var trexResult = trexCompactionDataProxy.SendDataPostRequest<ProfileDataResult<ProfileCellData>, ProductionDataProfileDataRequest>(productionDataProfileDataRequest, "/productiondata/profile", customHeaders).Result;

      return trexResult != null ? ConvertTRexProfileResult(trexResult) : null;
    }

    private static ProfileResult ConvertTRexProfileResult(ProfileDataResult<ProfileCellData> trexResult)
    {
      var profile = ProcessProfileCells(trexResult.ProfileCells, trexResult.GridDistanceBetweenProfilePoints, Guid.Empty);

      CheckForProductionDataPresence(profile);

      return profile;
    }

#if RAPTOR    
    private ProfileResult PerformProductionDataProfilePost(ProfileProductionDataRequest request)
    {
      MemoryStream memoryStream;

      if (request.IsAlignmentDesign)
      {
        var args = ASNode.RequestAlignmentProfile.RPC.__Global.Construct_RequestAlignmentProfile_Args(
          request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          ProfilesHelper.PROFILE_TYPE_NOT_REQUIRED,
          request.StartStation ?? ValidationConstants3D.MIN_STATION,
          request.EndStation ?? ValidationConstants3D.MIN_STATION,
          RaptorConverters.DesignDescriptor(request.AlignmentDesign),
          RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient),
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmAutomatic),
          RaptorConverters.DesignDescriptor(request.AlignmentDesign),
          request.ReturnAllPassesAndLayers);

        memoryStream = raptorClient.GetAlignmentProfile(args);
      }
      else
      {
        ProfilesHelper.ConvertProfileEndPositions(
          request.GridPoints,
          request.WGS84Points,
          out VLPDDecls.TWGS84Point startPt,
          out VLPDDecls.TWGS84Point endPt,
          out bool positionsAreGrid);

        var args = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args(
          request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
          ProfilesHelper.PROFILE_TYPE_NOT_REQUIRED,
          positionsAreGrid,
          startPt,
          endPt,
          RaptorConverters.ConvertFilter(request.Filter, request.ProjectId, raptorClient),
          RaptorConverters.ConvertLift(request.LiftBuildSettings, TFilterLayerMethod.flmAutomatic),
          RaptorConverters.DesignDescriptor(request.AlignmentDesign),
          request.ReturnAllPassesAndLayers);

        memoryStream = raptorClient.GetProfile(args);
      }

      return memoryStream != null
        ? ConvertProfileResult(memoryStream, request.CallId ?? Guid.NewGuid())
        : null; // TODO: return appropriate result
    }
#endif

    private static ProfileResult ProcessProfileCells(List<ProfileCellData> profileCells, double gridDistanceBetweenProfilePoints, Guid callerId)
    {
      var profile = new ProfileResult()
      {
        callId = callerId,
        cells = null
      };

      profile.cells = new List<ProfileCell>();
      ProfileCellData prevCell = null;

      foreach (var currCell in profileCells)
      {
        var gapExists = ProfilesHelper.CellGapExists(prevCell, currCell, out double prevStationIntercept);

        if (gapExists)
        {
          profile.cells.Add(new ProfileCell()
          {
            station = prevStationIntercept,
            firstPassHeight = float.NaN,
            highestPassHeight = float.NaN,
            lastPassHeight = float.NaN,
            lowestPassHeight = float.NaN,
            firstCompositeHeight = float.NaN,
            highestCompositeHeight = float.NaN,
            lastCompositeHeight = float.NaN,
            lowestCompositeHeight = float.NaN,
            designHeight = float.NaN,
            cmvPercent = float.NaN,
            cmvHeight = float.NaN,
            previousCmvPercent = float.NaN,
            mdpPercent = float.NaN,
            mdpHeight = float.NaN,
            temperature = float.NaN,
            temperatureHeight = float.NaN,
            temperatureLevel = -1,
            topLayerPassCount = -1,
            topLayerPassCountTargetRange = new TargetPassCountRange(VelociraptorConstants.NO_PASSCOUNT, VelociraptorConstants.NO_PASSCOUNT),
            passCountIndex = -1,
            topLayerThickness = float.NaN
          });
        }

        bool noCCVValue = currCell.TargetCCV == 0 || currCell.TargetCCV == VelociraptorConstants.NO_CCV || currCell.CCV == VelociraptorConstants.NO_CCV;
        bool noCCElevation = currCell.CCVElev == VelociraptorConstants.NULL_SINGLE || noCCVValue;
        bool noMDPValue = currCell.TargetMDP == 0 || currCell.TargetMDP == VelociraptorConstants.NO_MDP || currCell.MDP == VelociraptorConstants.NO_MDP;
        bool noMDPElevation = currCell.MDPElev == VelociraptorConstants.NULL_SINGLE || noMDPValue;
        bool noTemperatureValue = currCell.MaterialTemperature == VelociraptorConstants.NO_TEMPERATURE;
        bool noTemperatureElevation = currCell.MaterialTemperatureElev == VelociraptorConstants.NULL_SINGLE || noTemperatureValue;
        bool noPassCountValue = currCell.TopLayerPassCount == VelociraptorConstants.NO_PASSCOUNT;

        profile.cells.Add(new ProfileCell
        {
          station = currCell.Station,

          firstPassHeight = currCell.FirstPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.FirstPassHeight,
          highestPassHeight = currCell.HighestPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.HighestPassHeight,
          lastPassHeight = currCell.LastPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.LastPassHeight,
          lowestPassHeight = currCell.LowestPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.LowestPassHeight,

          firstCompositeHeight = currCell.CompositeFirstPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.CompositeFirstPassHeight,
          highestCompositeHeight = currCell.CompositeHighestPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.CompositeHighestPassHeight,
          lastCompositeHeight = currCell.CompositeLastPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.CompositeLastPassHeight,
          lowestCompositeHeight = currCell.CompositeLowestPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.CompositeLowestPassHeight,
          designHeight = currCell.DesignHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.DesignHeight,

          cmvPercent = noCCVValue
            ? float.NaN : (float)currCell.CCV / (float)currCell.TargetCCV * 100.0F,
          cmvHeight = noCCElevation ? float.NaN : currCell.CCVElev,
          previousCmvPercent = noCCVValue
            ? float.NaN : (float)currCell.PrevCCV / (float)currCell.PrevTargetCCV * 100.0F,

          mdpPercent = noMDPValue
            ? float.NaN : (float)currCell.MDP / (float)currCell.TargetMDP * 100.0F,
          mdpHeight = noMDPElevation ? float.NaN : currCell.MDPElev,

          temperature = noTemperatureValue ? float.NaN : currCell.MaterialTemperature / 10.0F,// As temperature is reported in 10th...
          temperatureHeight = noTemperatureElevation ? float.NaN : currCell.MaterialTemperatureElev,
          temperatureLevel = noTemperatureValue ? -1 :
            (currCell.MaterialTemperature < currCell.MaterialTemperatureWarnMin ? 2 :
              (currCell.MaterialTemperature > currCell.MaterialTemperatureWarnMax ? 0 : 1)),

          topLayerPassCount = noPassCountValue ? -1 : currCell.TopLayerPassCount,
          topLayerPassCountTargetRange = new TargetPassCountRange(currCell.TopLayerPassCountTargetRangeMin, currCell.TopLayerPassCountTargetRangeMax),

          passCountIndex = noPassCountValue ? -1 :
            (currCell.TopLayerPassCount < currCell.TopLayerPassCountTargetRangeMin ? 2 :
              (currCell.TopLayerPassCount > currCell.TopLayerPassCountTargetRangeMax ? 0 : 1)),

          topLayerThickness = currCell.TopLayerThickness == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.TopLayerThickness
        });

        prevCell = currCell;
      }
      //Add a last point at the intercept length of the last cell so profiles are drawn correctly
      if (prevCell != null)
      {
        ProfileCell lastCell = profile.cells[profile.cells.Count - 1];
        profile.cells.Add(new ProfileCell()
        {
          station = prevCell.Station + prevCell.InterceptLength,
          firstPassHeight = lastCell.firstPassHeight,
          highestPassHeight = lastCell.highestPassHeight,
          lastPassHeight = lastCell.lastPassHeight,
          lowestPassHeight = lastCell.lowestPassHeight,
          firstCompositeHeight = lastCell.firstCompositeHeight,
          highestCompositeHeight = lastCell.highestCompositeHeight,
          lastCompositeHeight = lastCell.lastCompositeHeight,
          lowestCompositeHeight = lastCell.lowestCompositeHeight,
          designHeight = lastCell.designHeight,
          cmvPercent = lastCell.cmvPercent,
          cmvHeight = lastCell.cmvHeight,
          mdpPercent = lastCell.mdpPercent,
          mdpHeight = lastCell.mdpHeight,
          temperature = lastCell.temperature,
          temperatureHeight = lastCell.temperatureHeight,
          temperatureLevel = lastCell.temperatureLevel,
          topLayerPassCount = lastCell.topLayerPassCount,
          topLayerPassCountTargetRange = lastCell.topLayerPassCountTargetRange,
          passCountIndex = lastCell.passCountIndex,
          topLayerThickness = lastCell.topLayerThickness,
          previousCmvPercent = lastCell.previousCmvPercent
        });
      }

      profile.gridDistanceBetweenProfilePoints = gridDistanceBetweenProfilePoints;

      return profile;
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<ProfileProductionDataRequest>(item);

        var profileResult =
#if RAPTOR
          UseTRexGateway("ENABLE_TREX_GATEWAY_PROFILING") ?
#endif
            PerformTRexProductionDataProfilePost(request)
#if RAPTOR
            : PerformProductionDataProfilePost(request)
#endif
          ;

        if (profileResult != null)
          return profileResult;

        throw CreateServiceException<ProfileProductionDataExecutor>();
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }
    }

#if RAPTOR
    private static ProfileResult ConvertProfileResult(MemoryStream ms, Guid callID)
    {
      List<StationLLPoint> points = null;

      PDSProfile pdsiProfile = new PDSProfile();

      var packager = new TICProfileCellListPackager()
      {
        CellList = new TICProfileCellList()
      };

      packager.ReadFromStream(ms);

      ms.Close();

      pdsiProfile.Assign(packager.CellList);
      pdsiProfile.GridDistanceBetweenProfilePoints = packager.GridDistanceBetweenProfilePoints;

      if (packager.LatLongList != null) // For an alignment profile we return the lat long list to draw the profile line. slicer tool will just be a zero count
      {
        points = packager.LatLongList.ToList().ConvertAll(delegate (TWGS84StationPoint p)
        {
          return new StationLLPoint { station = p.Station, lat = p.Lat * Coordinates.RADIANS_TO_DEGREES, lng = p.Lon * Coordinates.RADIANS_TO_DEGREES };
        });
      }

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

      var profile = ProcessProfileCells(profileCells, pdsiProfile.GridDistanceBetweenProfilePoints, callID);

      CheckForProductionDataPresence(profile, points);

      return profile;
    }
#endif

    private static void CheckForProductionDataPresence(ProfileResult profile, List<StationLLPoint> points = null)
    {
      //Check for no production data at all - Raptor may return cells but all heights will be NaN
      bool gotData = profile.cells != null && profile.cells.Count > 0;
      var heights = gotData ? (from c in profile.cells where !float.IsNaN(c.firstPassHeight) select c).ToList() : null;
      if (heights != null && heights.Count > 0)
      {
        profile.minStation = profile.cells.Min(c => c.station);
        profile.maxStation = profile.cells.Max(c => c.station);
        profile.minHeight = heights.Min(h => h.minHeight);
        profile.maxHeight = heights.Max(h => h.maxHeight);
        profile.alignmentPoints = points;
      }
      else
      {
        profile.minStation = double.NaN;
        profile.maxStation = double.NaN;
        profile.minHeight = double.NaN;
        profile.maxHeight = double.NaN;
        profile.alignmentPoints = points;
      }
    }
  }
}
