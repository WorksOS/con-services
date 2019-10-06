using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Enums;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.Models.ResultHandling;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.Requests;
using VSS.TRex.Types.CellPasses;
using VSS.TRex.Common.Models;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Client.Types;
using VSS.TRex.Types;
using GPSAccuracy = VSS.TRex.Types.GPSAccuracy;

namespace VSS.TRex.Gateway.Common.Executors
{
  public class CellPassesExecutor : BaseExecutor
  {
    public CellPassesExecutor()
    {
    }

    public CellPassesExecutor(IConfigurationStore configurationStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) : base(configurationStore, logger, exceptionHandler)
    {
    }

    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
    {
      var request = item as CellPassesTRexRequest;

      if (request == null)
        ThrowRequestTypeCastException<CellPassesTRexRequest>();

      var siteModel = GetSiteModel(request.ProjectUid);
      var filter = ConvertFilter(request.Filter, siteModel);
      var coords = request.CoordsAreGrid
        ? AutoMapperUtility.Automapper.Map<XYZ>(request.GridPoint)
        : AutoMapperUtility.Automapper.Map<XYZ>(request.LLPoint);

      var cellPassesApplicationService = new CellPassesRequest_ApplicationService();
      var response = await cellPassesApplicationService.ExecuteAsync(new CellPassesRequestArgument_ApplicationService
      {
        ProjectID = siteModel.ID,
        Filters =  new FilterSet(filter),
        CoordsAreGrid = request.CoordsAreGrid,
        Point = coords,
        LiftParams = ConvertLift(request.LiftSettings, request.Filter?.LayerType),
        //NOTE: Currently cell passes is raw data so does not use overriding targets
        Overrides = AutoMapperUtility.Automapper.Map<OverrideParameters>(request.Overrides)
      });

      if(response.ReturnCode != CellPassesReturnCode.DataFound)
        return new CellPassesV2Result((int)response.ReturnCode);

      var cellPasses = new List<CellPassesV2Result.FilteredPassData>();
      foreach (var cellPass in response.CellPasses)
        cellPasses.Add(ConvertCellPass(cellPass));

      var layer = new CellPassesV2Result.ProfileLayer
      {
        PassData = cellPasses.ToArray()
      };

      // Convert the response
      return new CellPassesV2Result((int)response.ReturnCode)
      {
        Layers = new[]
        {
          layer
        }
      };
    }

    private CellPassesV2Result.FilteredPassData ConvertCellPass(ClientCellProfileLeafSubgridRecord cellPass)
    {
      var result = new CellPassesV2Result.FilteredPassData()
      {
        FilteredPass = new CellPassesV2Result.CellPassValue()
        {
          Time = cellPass.LastPassTime,
          Amplitude = cellPass.LastPassValidAmp,
          Height = cellPass.Height,
          Ccv = cellPass.LastPassValidCCV,
          Mdp = cellPass.LastPassValidMDP,
          Rmv = cellPass.LastPassValidRMV,
          MachineSpeed = cellPass.MachineSpeed,
          Frequency = cellPass.LastPassValidFreq,
          GpsModeStore = (byte)cellPass.LastPassValidGPSMode,
          MachineId = cellPass.InternalSiteModelMachineIndex,
          MaterialTemperature = ushort.MaxValue, // Not present in cell pass
          RadioLatency = byte.MaxValue, // Not present in cell pass
          
        },
        TargetsValue = new CellPassesV2Result.CellTargetsValue()
        {
          TargetCcv = cellPass.TargetCCV,
          TargetMdp = cellPass.TargetMDP,
          TargetPassCount = (ushort)cellPass.TargetPassCount,
          TargetThickness = cellPass.TargetThickness,
          TempWarningLevelMax = CellPassConsts.MaxMaterialTempValue, //Not present in cell pass
          TempWarningLevelMin = CellPassConsts.MinMaterialTempValue//Not present in cell pass
        },
        EventsValue = new CellPassesV2Result.CellEventsValue()
        {
          EventAutoVibrationState = AutoStateType.Unknown, // Not present in cell pass
          EventDesignNameId = cellPass.EventDesignNameID,
          EventIcFlags = CellPassConsts.NullEventIcFlags, // Not present in cell pass
          EventInAvoidZoneState = CellPassConsts.NullEventAvoidZoneState, // Not present in cell pass
          EventVibrationState = ConvertVibState(cellPass.EventVibrationState),
          EventOnGroundState = OnGroundStateType.Unknown, // Not present in cell pass
          GpsAccuracy = ConvertGpsAccuracy(cellPass.GPSAccuracy),
          GpsTolerance = cellPass.GPSTolerance,
          LayerId = CellPassConsts.NullLayerID, // Not present in cell pass
          MapResetDesignNameId = CellPassConsts.NullEventMapResetDesignNameId, // Not present in cell pass
          MapResetPriorDate = CellPassConsts.NullTime, // Not present in cell pass
          PositioningTech = PositioningTechType.Unknown // Not present in cell pass
        }
      };

      return result;
    }

    private static GPSAccuracyType ConvertGpsAccuracy(GPSAccuracy cellPassGpsAccuracy)
    {
      switch (cellPassGpsAccuracy)
      {
        case GPSAccuracy.Fine: return GPSAccuracyType.Fine;
        case GPSAccuracy.Medium: return GPSAccuracyType.Medium;
        case GPSAccuracy.Coarse: return GPSAccuracyType.Coarse;
        case GPSAccuracy.Unknown: return GPSAccuracyType.Unknown;
        default: throw new ArgumentException($"Unknown GPSAccuracy type: {cellPassGpsAccuracy}");
      }
    }

    private static VibrationStateType ConvertVibState(VibrationState cellPassEventVibrationState)
    {
      switch (cellPassEventVibrationState)
      {
        case VibrationState.Off: return VibrationStateType.Off;
        case VibrationState.On: return VibrationStateType.On;
        case VibrationState.Invalid: return VibrationStateType.Invalid;
        default: throw new ArgumentException($"Unknown TICVibrationState type: {cellPassEventVibrationState}");
      }
    }

    /// <summary>
    /// Processes the tile request synchronously.
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException("Use the asynchronous form of this method");
    }
  }
}
