
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
#if RAPTOR
using SVOICFiltersDecls;
using SVOICGridCell;
using SVOICProfileCell;
#endif
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.Productivity3D.WebApi.Models.Compaction.AutoMapper;
using VSS.Productivity3D.Productivity3D.Models.Compaction;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors.CellPass
{
  public class CellPassesExecutor : RequestExecutorContainer
  {
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = CastRequestObjectTo<CellPassesRequest>(item);
#if RAPTOR
      if (UseTRexGateway("ENABLE_TREX_GATEWAY_CELL_PASSES"))
      {
#endif
        return await GetTRexCellPasses(request);
#if RAPTOR
      }

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
        return ConvertResult(profile);

      return null;
#endif
    }

#if RAPTOR
    protected CellPassesResult ConvertResult(TICProfileCell profile)
    {
      return new CellPassesResult(
        profile.CellCCV,
        profile.CellCCVElev,
        profile.CellFirstCompositeElev,
        profile.CellFirstElev,
        profile.CellHighestCompositeElev,
        profile.CellHighestElev,
        profile.CellLastCompositeElev,
        profile.CellLastElev,
        profile.CellLowestCompositeElev,
        profile.CellLowestElev,
        profile.CellMaterialTemperature,
        profile.CellMaterialTemperatureElev,
        profile.CellMaterialTemperatureWarnMax,
        profile.CellMaterialTemperatureWarnMin,
        profile.FilteredHalfPassCount,
        profile.FilteredPassCount,
        profile.CellMDP,
        profile.CellMDPElev,
        profile.CellTargetCCV,
        profile.CellTargetMDP,
        profile.CellTopLayerThickness,
        profile.DesignElev,
        profile.IncludesProductionData,
        profile.InterceptLength,
        profile.OTGCellX,
        profile.OTGCellY,
        profile.Station,
        profile.TopLayerPassCount,
        new TargetPassCountRange(profile.TopLayerPassCountTargetRangeMin, profile.TopLayerPassCountTargetRangeMax),
        ConvertCellLayers(profile.Layers, ConvertFilteredPassData(profile.Passes))
      );
    }

    private CellPassesResult.ProfileLayer ConvertCellLayerItem(TICProfileLayer layer, CellPassesResult.FilteredPassData[] layerPasses)
    {
      return new CellPassesResult.ProfileLayer
      {
        Amplitude = layer.Amplitude,
        CCV = layer.CCV,
        CCV_Elev = layer.CCV_Elev,
        CCV_MachineID = layer.CCV_MachineID,
        CCV_Time = layer.CCV_Time,
        FilteredHalfPassCount = layer.FilteredHalfPassCount,
        FilteredPassCount = layer.FilteredPassCount,
        FirstPassHeight = layer.FirstPassHeight,
        Frequency = layer.Frequency,
        Height = layer.Height,
        LastLayerPassTime = layer.LastLayerPassTime,
        LastPassHeight = layer.LastPassHeight,
        MachineID = layer.MachineID,
        MaterialTemperature = layer.MaterialTemperature,
        MaterialTemperature_Elev = layer.MaterialTemperature_Elev,
        MaterialTemperature_MachineID = layer.MaterialTemperature_MachineID,
        MaterialTemperature_Time = layer.MaterialTemperature_Time,
        MaximumPassHeight = layer.MaximumPassHeight,
        MaxThickness = layer.MaxThickness,
        MDP = layer.MDP,
        MDP_Elev = layer.MDP_Elev,
        MDP_MachineID = layer.MDP_MachineID,
        MDP_Time = layer.MDP_Time,
        MinimumPassHeight = layer.MinimumPassHeight,
        RadioLatency = layer.RadioLatency,
        RMV = layer.RMV,
        TargetCCV = layer.TargetCCV,
        TargetMDP = layer.TargetMDP,
        TargetPassCount = layer.TargetPassCount,
        TargetThickness = layer.TargetThickness,
        Thickness = layer.Thickness,
        FilteredPassData = layerPasses
      };
    }


    private CellPassesResult.ProfileLayer[] ConvertCellLayers(TICProfileLayers layers, CellPassesResult.FilteredPassData[] allPasses)
    {
      CellPassesResult.ProfileLayer[] result;
      if (layers.Count() == 0)
      {
        result = new CellPassesResult.ProfileLayer[1];
        result[0] = ConvertCellLayerItem(new TICProfileLayer(), allPasses);
        return result;
      }

      result = new CellPassesResult.ProfileLayer[layers.Count()];

      int count = 0;
      foreach (TICProfileLayer layer in layers)
      {
        var layerPasses = allPasses.Skip(layer.StartCellPassIdx).Take(layer.EndCellPassIdx - layer.StartCellPassIdx + 1).ToArray();
        result[count++] = ConvertCellLayerItem(layer, layerPasses);
      }

      return result;
    }

    private CellPassesResult.CellEventsValue ConvertCellPassEvents(TICCellEventsValue events)
    {
      return new CellPassesResult.CellEventsValue
      {
        EventAutoVibrationState = RaptorConverters.convertAutoStateType(events.EventAutoVibrationState),
        EventDesignNameID = events.EventDesignNameID,
        EventICFlags = events.EventICFlags,
        EventInAvoidZoneState = events.EventInAvoidZoneState,
        EventMachineAutomatics = RaptorConverters.convertGCSAutomaticsModeType(events.EventMachineAutomatics),
        EventMachineGear = RaptorConverters.convertMachineGearType(events.EventMachineGear),
        EventMachineRMVThreshold = events.EventMachineRMVThreshold,
        EventMinElevMapping = events.EventMinElevMapping,
        EventOnGroundState = RaptorConverters.convertOnGroundStateType(events.EventOnGroundState),
        EventVibrationState = RaptorConverters.convertVibrationStateType(events.EventVibrationState),
        GPSAccuracy = RaptorConverters.convertGPSAccuracyType(events.GPSAccuracy),
        GPSTolerance = events.GPSTolerance,
        LayerID = events.LayerID,
        MapReset_DesignNameID = events.MapReset_DesignNameID,
        MapReset_PriorDate = events.MapReset_PriorDate,
        PositioningTech = RaptorConverters.convertPositioningTechType(events.PositioningTech)
      };
    }

    private CellPassesResult.CellPassValue ConvertCellPass(TICCellPassValue pass)
    {
      return new CellPassesResult.CellPassValue
      {
        Amplitude = pass.Amplitude,
        CCV = pass.CCV,
        Frequency = pass.Frequency,
        GPSModeStore = pass.GPSModeStore,
        Height = pass.Height,
        MachineID = pass.MachineID,
        MachineSpeed = pass.MachineSpeed,
        MaterialTemperature = pass.MaterialTemperature,
        MDP = pass.MDP,
        RadioLatency = pass.RadioLatency,
        RMV = pass.RMV,
        Time = pass.Time
      };
    }

    private CellPassesResult.CellTargetsValue ConvertCellPassTargets(TICCellTargetsValue targets)
    {
      return new CellPassesResult.CellTargetsValue
      {
        TargetCCV = targets.TargetCCV,
        TargetMDP = targets.TargetMDP,
        TargetPassCount = targets.TargetPassCount,
        TargetThickness = targets.TargetThickness,
        TempWarningLevelMax = targets.TempWarningLevelMax,
        TempWarningLevelMin = targets.TempWarningLevelMin
      };
    }

    private CellPassesResult.FilteredPassData ConvertFilteredPassDataItem(TICFilteredPassData pass)
    {
      return new CellPassesResult.FilteredPassData
      {
        EventsValue = ConvertCellPassEvents(pass.EventValues),
        FilteredPass = ConvertCellPass(pass.FilteredPass),
        TargetsValue = ConvertCellPassTargets(pass.TargetValues)
      };
    }

    private CellPassesResult.FilteredPassData[] ConvertFilteredPassData(TICFilteredMultiplePassInfo passes)
    {
      return passes.FilteredPassData != null
        ? Array.ConvertAll(passes.FilteredPassData, ConvertFilteredPassDataItem)
        : null;
    }
#endif

    private async Task<CellPassesResult> GetTRexCellPasses(CellPassesRequest request)
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

      var trexResult = await trexCompactionDataProxy.SendDataPostRequest<CellPassesV2Result, CellPassesTRexRequest>(tRexRequest, "/cells/passes", customHeaders);

      if (trexResult != null)
        return ConvertTRexResult(trexResult);

      return null;
    }

    private CellPassesResult ConvertTRexResult(CellPassesV2Result result)
    {
      // Convert layers...
      var layers = new CellPassesResult.ProfileLayer[result.Layers.Length];

      for (var i = 0; i < result.Layers.Length; i++)
        layers[i] = AutoMapperUtility.Automapper.Map<CellPassesResult.ProfileLayer>(result.Layers[i]);

      return new CellPassesResult() {Layers = layers};
    }
  }
}
