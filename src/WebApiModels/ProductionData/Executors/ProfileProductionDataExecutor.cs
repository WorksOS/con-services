using SVOICOptionsDecls;
using SVOICProfileCell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.Utilities;
using VSS.Productivity3D.WebApi.Models.Common;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
using VSS.Velociraptor.PDSInterface;
using ProfileCell = VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling.ProfileCell;
using VSS.Productivity3D.WebApiModels.Compaction.Helpers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.Compaction.Helpers;
using VSS.Productivity3D.Models.Utilities;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Get production data profile calculations executor.
  /// </summary>
  public class ProfileProductionDataExecutor : RequestExecutorContainer
  {
    private ProfileResult PerformProductionDataProfilePost(ProfileProductionDataRequest request)
    {
      MemoryStream memoryStream;

      if (request.IsAlignmentDesign)
      {
        var args = ASNode.RequestAlignmentProfile.RPC.__Global.Construct_RequestAlignmentProfile_Args(
          request.ProjectId ?? -1,
          ProfilesHelper.PROFILE_TYPE_NOT_REQUIRED,
          request.startStation ?? ValidationConstants3D.MIN_STATION,
          request.endStation ?? ValidationConstants3D.MIN_STATION,
          RaptorConverters.DesignDescriptor(request.alignmentDesign),
          RaptorConverters.ConvertFilter(request.filterID, request.filter, request.ProjectId),
          RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
          RaptorConverters.DesignDescriptor(request.alignmentDesign),
          request.returnAllPassesAndLayers);

        memoryStream = raptorClient.GetAlignmentProfile(args);
      }
      else
      {
        ProfilesHelper.ConvertProfileEndPositions(
          request.gridPoints,
          request.wgs84Points, 
          out VLPDDecls.TWGS84Point startPt,
          out VLPDDecls.TWGS84Point endPt,
          out bool positionsAreGrid);

        var args = ASNode.RequestProfile.RPC.__Global.Construct_RequestProfile_Args(
          request.ProjectId ?? -1,
          ProfilesHelper.PROFILE_TYPE_NOT_REQUIRED,
          positionsAreGrid,
          startPt,
          endPt,
          RaptorConverters.ConvertFilter(request.filterID, request.filter, request.ProjectId),
          RaptorConverters.ConvertLift(request.liftBuildSettings, TFilterLayerMethod.flmAutomatic),
          RaptorConverters.DesignDescriptor(request.alignmentDesign),
          request.returnAllPassesAndLayers);

        memoryStream = raptorClient.GetProfile(args);
      }

      return memoryStream != null
        ? ConvertProfileResult(memoryStream, request.callId ?? Guid.NewGuid())
        : null; // TODO: return appropriate result
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      try
      {
        var profileResult = PerformProductionDataProfilePost(item as ProfileProductionDataRequest);

        if (profileResult != null)
        {
          result = profileResult;
        }
        else
        {
          throw new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, "Failed to get requested profile calculations."));
        }
      }
      finally
      {
        ContractExecutionStates.ClearDynamic();
      }

      return result;
    }

    private static ProfileResult ConvertProfileResult(MemoryStream ms, Guid callID)
    {
      List<StationLLPoint> points = null;

      var profile = new ProfileResult()
      {
        callId = callID,
        cells = null,
        success = ms != null
      };

      if (profile.success)
      {
        PDSProfile pdsiProfile = new PDSProfile();

        var packager = new TICProfileCellListPackager()
        {
          CellList = new TICProfileCellList()
        };

        packager.ReadFromStream(ms);

        pdsiProfile.Assign(packager.CellList);
        pdsiProfile.GridDistanceBetweenProfilePoints = packager.GridDistanceBetweenProfilePoints;

        if (packager.LatLongList != null) // For an alignment profile we return the lat long list to draw the profile line. slicer tool will just be a zero count
        {
          points = packager.LatLongList.ToList().ConvertAll(delegate (TWGS84StationPoint p)
          {
            return new StationLLPoint { station = p.Station, lat = p.Lat * 180 / Math.PI, lng = p.Lon * 180 / Math.PI };
          });
        }

        profile.cells = new List<ProfileCell>();
        VSS.Velociraptor.PDSInterface.ProfileCell prevCell = null;
        foreach (Velociraptor.PDSInterface.ProfileCell currCell in pdsiProfile.cells)
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
          bool noTemperatureValue = currCell.materialTemperature == VelociraptorConstants.NO_TEMPERATURE;
          bool noTemperatureElevation = currCell.materialTemperatureElev == VelociraptorConstants.NULL_SINGLE || noTemperatureValue;
          bool noPassCountValue = currCell.topLayerPassCount == VelociraptorConstants.NO_PASSCOUNT;

          profile.cells.Add(new ProfileCell
          {
            station = currCell.station,

            firstPassHeight = currCell.firstPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.firstPassHeight,
            highestPassHeight = currCell.highestPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.highestPassHeight,
            lastPassHeight = currCell.lastPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.lastPassHeight,
            lowestPassHeight = currCell.lowestPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.lowestPassHeight,

            firstCompositeHeight = currCell.compositeFirstPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.compositeFirstPassHeight,
            highestCompositeHeight = currCell.compositeHighestPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.compositeHighestPassHeight,
            lastCompositeHeight = currCell.compositeLastPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.compositeLastPassHeight,
            lowestCompositeHeight = currCell.compositeLowestPassHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.compositeLowestPassHeight,
            designHeight = currCell.designHeight == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.designHeight,

            cmvPercent = noCCVValue
              ? float.NaN : (float)currCell.CCV / (float)currCell.TargetCCV * 100.0F,
            cmvHeight = noCCElevation ? float.NaN : currCell.CCVElev,
            previousCmvPercent = noCCVValue
              ? float.NaN : (float)currCell.PrevCCV / (float)currCell.PrevTargetCCV * 100.0F,

            mdpPercent = noMDPValue
              ? float.NaN : (float)currCell.MDP / (float)currCell.TargetMDP * 100.0F,
            mdpHeight = noMDPElevation ? float.NaN : currCell.MDPElev,

            temperature = noTemperatureValue ? float.NaN : currCell.materialTemperature / 10.0F,// As temperature is reported in 10th...
            temperatureHeight = noTemperatureElevation ? float.NaN : currCell.materialTemperatureElev,
            temperatureLevel = noTemperatureValue ? -1 :
              (currCell.materialTemperature < currCell.materialTemperatureWarnMin ? 2 :
                (currCell.materialTemperature > currCell.materialTemperatureWarnMax ? 0 : 1)),

            topLayerPassCount = noPassCountValue ? -1 : currCell.topLayerPassCount,
            topLayerPassCountTargetRange = new TargetPassCountRange(currCell.topLayerPassCountTargetRange.Min, currCell.topLayerPassCountTargetRange.Max),

            passCountIndex = noPassCountValue ? -1 :
              (currCell.topLayerPassCount < currCell.topLayerPassCountTargetRange.Min ? 2 :
                (currCell.topLayerPassCount > currCell.topLayerPassCountTargetRange.Max ? 0 : 1)),

            topLayerThickness = currCell.topLayerThickness == VelociraptorConstants.NULL_SINGLE ? float.NaN : currCell.topLayerThickness
          });

          prevCell = currCell;
        }
        //Add a last point at the intercept length of the last cell so profiles are drawn correctly
        if (prevCell != null)
        {
          ProfileCell lastCell = profile.cells[profile.cells.Count - 1];
          profile.cells.Add(new ProfileCell()
          {
            station = prevCell.station + prevCell.interceptLength,
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
        ms.Close();

        profile.gridDistanceBetweenProfilePoints = pdsiProfile.GridDistanceBetweenProfilePoints;
      }

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

      return profile;
    }
  }
}
