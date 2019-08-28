using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
#if RAPTOR
using SVOICFiltersDecls;
using SVOICGridCell;
using SVOICProfileCell;
#endif
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors.CellPass
{
  public class CellPassesV2Executor : RequestExecutorContainer 
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CellPassesRequest>(item);
      CellPassesV2Result result = null;
#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_CELL_PASSES"))
      {
#endif
        result = await GetTRexCellPasses(request);
        if (result != null)
          return result;
        throw CreateServiceException<CellPassesV2Executor>();

#if RAPTOR
      }

      result = GetRaptorResult(request);
      if (result != null)
        return result;
      throw CreateServiceException<CellPassesV2Executor>();
#endif 
    }

#if RAPTOR 
    #region Raptor Conversion Code

    private CellPassesV2Result GetRaptorResult(CellPassesRequest request)
    {
      bool isGridCoord = request.probePositionGrid != null;
      bool isLatLgCoord = request.probePositionLL != null;
      double probeX = isGridCoord ? request.probePositionGrid.x : (isLatLgCoord ? request.probePositionLL.Lon : 0);
      double probeY = isGridCoord ? request.probePositionGrid.y : (isLatLgCoord ? request.probePositionLL.Lat : 0);

      var raptorFilter = RaptorConverters.ConvertFilter(request.filter, request.ProjectId, raptorClient, overrideAssetIds: new List<long>());

      int code = raptorClient.RequestCellProfile
      (request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
        RaptorConverters.convertCellAddress(request.cellAddress ?? new CellAddress()),
        probeX, probeY,
        isGridCoord,
        RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
        request.gridDataType,
        raptorFilter,
        out var profile);


      if (code == 1)//TICServerRequestResult.icsrrNoError
        return ConvertRaptorResult(profile);

      return null;
    }

    private CellPassesV2Result ConvertRaptorResult(TICProfileCell profile)
    {
      if (profile == null)
        return null;

      return new CellPassesV2Result
      {
        Layers = ConvertCellLayers(profile.Layers, ConvertFilteredPassData(profile.Passes))
      };
    }

     private CellPassesV2Result.ProfileLayer ConvertCellLayerItem(TICProfileLayer layer, CellPassesV2Result.FilteredPassData[] layerPasses)
    {
      return new CellPassesV2Result.ProfileLayer
      {
        Amplitude = layer.Amplitude,
        Ccv = layer.CCV,
        CcvElev = layer.CCV_Elev,
        CcvMachineId = layer.CCV_MachineID,
        CcvTime = layer.CCV_Time,
        FilteredHalfPassCount = layer.FilteredHalfPassCount,
        FilteredPassCount = layer.FilteredPassCount,
        FirstPassHeight = layer.FirstPassHeight,
        Frequency = layer.Frequency,
        Height = layer.Height,
        LastLayerPassTime = layer.LastLayerPassTime,
        LastPassHeight = layer.LastPassHeight,
        MachineId = layer.MachineID,
        MaterialTemperature = layer.MaterialTemperature,
        MaterialTemperatureElev = layer.MaterialTemperature_Elev,
        MaterialTemperatureMachineId = layer.MaterialTemperature_MachineID,
        MaterialTemperatureTime = layer.MaterialTemperature_Time,
        MaximumPassHeight = layer.MaximumPassHeight,
        MaxThickness = layer.MaxThickness,
        Mdp = layer.MDP,
        MdpElev = layer.MDP_Elev,
        MdpMachineId = layer.MDP_MachineID,
        MdpTime = layer.MDP_Time,
        MinimumPassHeight = layer.MinimumPassHeight,
        RadioLatency = layer.RadioLatency,
        Rmv = layer.RMV,
        TargetCcv = layer.TargetCCV,
        TargetMdp = layer.TargetMDP,
        TargetPassCount = layer.TargetPassCount,
        TargetThickness = layer.TargetThickness,
        Thickness = layer.Thickness,
        PassData = layerPasses
      };
    }

    private CellPassesV2Result.ProfileLayer[] ConvertCellLayers(TICProfileLayers layers, CellPassesV2Result.FilteredPassData[] allPasses)
    {
      CellPassesV2Result.ProfileLayer[] result;
      if (layers.Count() == 0)
      {
        result = new CellPassesV2Result.ProfileLayer[1];
        result[0] = ConvertCellLayerItem(new TICProfileLayer(), allPasses);
        return result;
      }

      result = new CellPassesV2Result.ProfileLayer[layers.Count()];

      var count = 0;
      foreach (TICProfileLayer layer in layers)
      {
        var layerPasses = allPasses.Skip(layer.StartCellPassIdx).Take(layer.EndCellPassIdx - layer.StartCellPassIdx + 1).ToArray();
        result[count++] = ConvertCellLayerItem(layer, layerPasses);
      }

      return result;
    }

    private CellPassesV2Result.CellEventsValue ConvertCellPassEvents(TICCellEventsValue events)
    {
      return new CellPassesV2Result.CellEventsValue
      {
        EventAutoVibrationState = RaptorConverters.convertAutoStateType(events.EventAutoVibrationState),
        EventDesignNameId = events.EventDesignNameID,
        EventIcFlags = events.EventICFlags,
        EventInAvoidZoneState = events.EventInAvoidZoneState,
        EventMachineAutomatics = RaptorConverters.convertGCSAutomaticsModeType(events.EventMachineAutomatics),
        EventMachineGear = RaptorConverters.convertMachineGearType(events.EventMachineGear),
        EventMachineRmvThreshold = events.EventMachineRMVThreshold,
        EventMinElevMapping = (byte)events.EventMinElevMapping,
        EventOnGroundState = RaptorConverters.convertOnGroundStateType(events.EventOnGroundState),
        EventVibrationState = RaptorConverters.convertVibrationStateType(events.EventVibrationState),
        GpsAccuracy = RaptorConverters.convertGPSAccuracyType(events.GPSAccuracy),
        GpsTolerance = events.GPSTolerance,
        LayerId = events.LayerID,
        MapResetDesignNameId = events.MapReset_DesignNameID,
        MapResetPriorDate = events.MapReset_PriorDate,
        PositioningTech = RaptorConverters.convertPositioningTechType(events.PositioningTech)
      };
    }

    private CellPassesV2Result.CellPassValue ConvertCellPass(TICCellPassValue pass)
    {
      return new CellPassesV2Result.CellPassValue
      {
        Amplitude = pass.Amplitude,
        Ccv = pass.CCV,
        Frequency = pass.Frequency,
        GpsModeStore = pass.GPSModeStore,
        Height = pass.Height,
        MachineId = pass.MachineID,
        MachineSpeed = pass.MachineSpeed,
        MaterialTemperature = pass.MaterialTemperature,
        Mdp = pass.MDP,
        RadioLatency = pass.RadioLatency,
        Rmv = pass.RMV,
        Time = pass.Time
      };
    }

    private CellPassesV2Result.CellTargetsValue ConvertCellPassTargets(TICCellTargetsValue targets)
    {
      return new CellPassesV2Result.CellTargetsValue
      {
        TargetCcv = targets.TargetCCV,
        TargetMdp = targets.TargetMDP,
        TargetPassCount = targets.TargetPassCount,
        TargetThickness = targets.TargetThickness,
        TempWarningLevelMax = targets.TempWarningLevelMax,
        TempWarningLevelMin = targets.TempWarningLevelMin
      };
    }

    private CellPassesV2Result.FilteredPassData ConvertFilteredPassDataItem(TICFilteredPassData pass)
    {
      return new CellPassesV2Result.FilteredPassData
      {
        EventsValue = ConvertCellPassEvents(pass.EventValues),
        FilteredPass = ConvertCellPass(pass.FilteredPass),
        TargetsValue = ConvertCellPassTargets(pass.TargetValues)
      };
    }

    private CellPassesV2Result.FilteredPassData[] ConvertFilteredPassData(TICFilteredMultiplePassInfo passes)
    {
      return passes.FilteredPassData != null
        ? Array.ConvertAll(passes.FilteredPassData, ConvertFilteredPassDataItem)
        : null;
    }

    #endregion
#endif

    private Task<CellPassesV2Result> GetTRexCellPasses(CellPassesRequest request)
    {
      var overrides = AutoMapperUtility.Automapper.Map<OverridingTargets>(request.liftBuildSettings);
      var liftSettings = AutoMapperUtility.Automapper.Map<LiftSettings>(request.liftBuildSettings);
      CellPassesTRexRequest tRexRequest;
      if (request.probePositionGrid != null)
      {
        tRexRequest = new CellPassesTRexRequest(request.ProjectUid.Value,
          request.probePositionGrid,
          request.filter,
          overrides,
          liftSettings);
      }
      else
      {
        tRexRequest = new CellPassesTRexRequest(request.ProjectUid.Value,
          request.probePositionLL,
          request.filter,
          overrides,
          liftSettings);
      }

      return trexCompactionDataProxy.SendDataPostRequest<CellPassesV2Result, CellPassesTRexRequest>(tRexRequest, "/cells/passes", customHeaders);
    }

  }
}
