
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
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }

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
      return CellPassesResult.CreateCellPassesResult(
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
        amplitude = layer.Amplitude,
        cCV = layer.CCV,
        cCV_Elev = layer.CCV_Elev,
        cCV_MachineID = layer.CCV_MachineID,
        cCV_Time = layer.CCV_Time,
        filteredHalfPassCount = layer.FilteredHalfPassCount,
        filteredPassCount = layer.FilteredPassCount,
        firstPassHeight = layer.FirstPassHeight,
        frequency = layer.Frequency,
        height = layer.Height,
        lastLayerPassTime = layer.LastLayerPassTime,
        lastPassHeight = layer.LastPassHeight,
        machineID = layer.MachineID,
        materialTemperature = layer.MaterialTemperature,
        materialTemperature_Elev = layer.MaterialTemperature_Elev,
        materialTemperature_MachineID = layer.MaterialTemperature_MachineID,
        materialTemperature_Time = layer.MaterialTemperature_Time,
        maximumPassHeight = layer.MaximumPassHeight,
        maxThickness = layer.MaxThickness,
        mDP = layer.MDP,
        mDP_Elev = layer.MDP_Elev,
        mDP_MachineID = layer.MDP_MachineID,
        mDP_Time = layer.MDP_Time,
        minimumPassHeight = layer.MinimumPassHeight,
        radioLatency = layer.RadioLatency,
        rMV = layer.RMV,
        targetCCV = layer.TargetCCV,
        targetMDP = layer.TargetMDP,
        targetPassCount = layer.TargetPassCount,
        targetThickness = layer.TargetThickness,
        thickness = layer.Thickness,
        filteredPassData = layerPasses
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
        eventAutoVibrationState = RaptorConverters.convertAutoStateType(events.EventAutoVibrationState),
        eventDesignNameID = events.EventDesignNameID,
        eventICFlags = events.EventICFlags,
        EventInAvoidZoneState = events.EventInAvoidZoneState,
        eventMachineAutomatics = RaptorConverters.convertGCSAutomaticsModeType(events.EventMachineAutomatics),
        eventMachineGear = RaptorConverters.convertMachineGearType(events.EventMachineGear),
        eventMachineRMVThreshold = events.EventMachineRMVThreshold,
        EventMinElevMapping = events.EventMinElevMapping,
        eventOnGroundState = RaptorConverters.convertOnGroundStateType(events.EventOnGroundState),
        eventVibrationState = RaptorConverters.convertVibrationStateType(events.EventVibrationState),
        gPSAccuracy = RaptorConverters.convertGPSAccuracyType(events.GPSAccuracy),
        gPSTolerance = events.GPSTolerance,
        layerID = events.LayerID,
        mapReset_DesignNameID = events.MapReset_DesignNameID,
        mapReset_PriorDate = events.MapReset_PriorDate,
        positioningTech = RaptorConverters.convertPositioningTechType(events.PositioningTech)
      };
    }

    private CellPassesResult.CellPassValue ConvertCellPass(TICCellPassValue pass)
    {
      return new CellPassesResult.CellPassValue
      {
        amplitude = pass.Amplitude,
        cCV = pass.CCV,
        frequency = pass.Frequency,
        gPSModeStore = pass.GPSModeStore,
        height = pass.Height,
        machineID = pass.MachineID,
        machineSpeed = pass.MachineSpeed,
        materialTemperature = pass.MaterialTemperature,
        mDP = pass.MDP,
        radioLatency = pass.RadioLatency,
        rMV = pass.RMV,
        time = pass.Time
      };
    }

    private CellPassesResult.CellTargetsValue ConvertCellPassTargets(TICCellTargetsValue targets)
    {
      return new CellPassesResult.CellTargetsValue
      {
        targetCCV = targets.TargetCCV,
        targetMDP = targets.TargetMDP,
        targetPassCount = targets.TargetPassCount,
        targetThickness = targets.TargetThickness,
        tempWarningLevelMax = targets.TempWarningLevelMax,
        tempWarningLevelMin = targets.TempWarningLevelMin
      };
    }

    private CellPassesResult.FilteredPassData ConvertFilteredPassDataItem(TICFilteredPassData pass)
    {
      return new CellPassesResult.FilteredPassData
      {
        eventsValue = ConvertCellPassEvents(pass.EventValues),
        filteredPass = ConvertCellPass(pass.FilteredPass),
        targetsValue = ConvertCellPassTargets(pass.TargetValues)
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

      return new CellPassesResult() {layers = layers};
    }
  }
}
