using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using SVOICFiltersDecls;
using SVOICFilterSettings;
using SVOICGridCell;
using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling;
using VSS.Raptor.Service.Common.Contracts;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.Common.Proxies;
using VSS.Raptor.Service.Common.ResultHandling;
using SVOICProfileCell;


namespace VSS.Raptor.Service.WebApiModels.ProductionData.Executors
{
  public class CellPassesExecutor : RequestExecutorContainer
  {
        /// <summary>
        /// This constructor allows us to mock RaptorClient
        /// </summary>
        /// <param name="raptorClient"></param>
        public CellPassesExecutor(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
        {
        }

        /// <summary>
        /// Default constructor for RequestExecutorContainer.Build
        /// </summary>
        public CellPassesExecutor()
        {
        }
        protected override ContractExecutionResult ProcessEx<T>(T item)
        {
          ContractExecutionResult result = null;
          try
          {
            CellPassesRequest request = item as CellPassesRequest;
            if (request == null)
               throw new ServiceException(HttpStatusCode.InternalServerError,  
                    new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError, "Missing CellPassesRequest"));

              TICProfileCell profile;
            bool isGridCoord = request.probePositionGrid != null;
            bool isLatLgCoord = request.probePositionLL != null;
            double probeX = isGridCoord ? request.probePositionGrid.x : (isLatLgCoord ? request.probePositionLL.Lon : 0);
            double probeY = isGridCoord ? request.probePositionGrid.y : (isLatLgCoord ? request.probePositionLL.Lat : 0);

            TICFilterSettings raptorFilter = RaptorConverters.ConvertFilter(request.filterId, request.filter, request.projectId, null, null,
                new List<long>());
            int code = raptorClient.RequestCellProfile
                            (request.projectId ?? -1,
                             RaptorConverters.convertCellAddress(request.cellAddress == null ? new CellAddress() : request.cellAddress),
                             probeX, probeY,
                             isGridCoord,
                             RaptorConverters.ConvertLift(request.liftBuildSettings, raptorFilter.LayerMethod),
                             request.gridDataType,
                             raptorFilter,
                             out profile);

            if (code == 1)//TICServerRequestResult.icsrrNoError
            {
              result = ConvertResult(profile);
            }
            else
            {
              throw new ServiceException(HttpStatusCode.BadRequest,  new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                            "Failed to get cell profile details"));
            }
          }
          finally
          {
            
          }
          return result;

        }

        protected override void ProcessErrorCodes()
        {

          //TODO: These are the error codes returned by Raptor. TICServerRequestResult is not currently exposed.
          /*
            icsrrUnknownError = $00000000;
            icsrrNoError = $00000001;
            icsrrClientNotRegistered = $00000002;
            icsrrCellNotFound = $00000003;
            icsrrUnknownCellPassSelectionMethod = $00000004;
            icsrrNoFilteredCellValue = $00000005;
            icsrrFailedToProcessFile = $00000006;
            icsrrTimedOutWaitingForProcessing = $00000007;
            icsrrSubGridNotFound = $00000008;
            icsrrNoSelectedSiteModel = $00000009;
            icsrrServerUnavailable = $0000000A;
            icsrrNoOutstandingEvents = $0000000B;
            icsrrNameAlreadyExists = $0000000C;
            icsrrEventNotFound = $0000000D;
            icsrrMaxNoOfMachinesReached = $0000000E;
            icsrrDataAdminDeleteOpActive = $0000000F;
            icsrrDataAdminArchiveOpActive = $00000010;
            icsrrServerBusyNoDBWriteLockAcquired = $00000011;
            icsrrServerNotReady = $00000012;
            icsrrFailedToReadSubgridSegment = $00000013;
            icsrrOperationAbortedByCaller = $00000014;
            icsrrProductionDataRequiresUpgrade = $00000015;
            icsrrNoConnectionToServer = $00000016;
            icsrrFailedToConvertClientWGSCoords = $00000017;
            icsrrUnableToPrepareFilterForUse = $00000018;
            icsrrServiceStopped = $00000019;
            icsrrFailedToRequestSubgridExistenceMap = $0000001A;
            icsrrFailedToComputeDesignBoundary = $0000001B;
            icsrrFailedToComputeDesignFilterBoundary = $0000001C;
            icsrrNoResponseDataInResponse = $0000001D;
            icsrrFailedToConvertServerNEECoords = $0000001E;
            icsrrFailedToBuildLiftsForCell = $0000001F;
            icsrrFailedToLock = $00000020;
            icsrrCancelled = $00000021;
            icsrrMissingInputParameters = $00000022;
            icsrrFilterInitialisationFailure = $00000023;
            icsrrInvokeError_rpcirFailed = $00000024;
            icsrrInvokeError_rpcirEncodeFailure = $00000025;
            icsrrInvokeError_rpcirDecodeFailure = $00000026;
            icsrrInvokeError_rpcirNotConnected = $00000027;
            icsrrFailedToComputeDesignFilterPatch = $00000028;
            icsrrDataModelDoesNotHaveValidPlanExtents = $00000029;
            icsrrDataModelHasInvalidZeroCellSize = $0000002A;
           */

        }

    private CellPassesResult ConvertResult(TICProfileCell profile)
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
                 TargetPassCountRange.CreateTargetPassCountRange(profile.TopLayerPassCountTargetRangeMin, profile.TopLayerPassCountTargetRangeMax),
                 ConvertCellLayers(profile.Layers, ConvertFilteredPassData(profile.Passes))
                 
             );
    }

    #region Converters
    private CellPassesResult.ProfileLayer ConvertCellLayerItem(TICProfileLayer layer, CellPassesResult.FilteredPassData[] layerPasses)
        {
          return new CellPassesResult.ProfileLayer()
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
          CellPassesResult.FilteredPassData[] layerPasses;
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
           layerPasses =
                allPasses.Skip(layer.StartCellPassIdx).Take(layer.EndCellPassIdx-layer.StartCellPassIdx+1).ToArray();
            result[count++] = ConvertCellLayerItem(layer, layerPasses);
          }

          return result;
        }

        private CellPassesResult.CellEventsValue ConvertCellPassEvents(TICCellEventsValue events)
        {
          return new CellPassesResult.CellEventsValue()
          {
            eventAutoVibrationState = events.EventAutoVibrationState,
            eventDesignNameID = events.EventDesignNameID,
            eventICFlags = events.EventICFlags,
            EventInAvoidZoneState = events.EventInAvoidZoneState,
            eventMachineAutomatics = events.EventMachineAutomatics,
            eventMachineGear = events.EventMachineGear,
            eventMachineRMVThreshold = events.EventMachineRMVThreshold,
            EventMinElevMapping = events.EventMinElevMapping,
            eventOnGroundState = events.EventOnGroundState,
            eventVibrationState = events.EventVibrationState,
            gPSAccuracy = events.GPSAccuracy,
            gPSTolerance = events.GPSTolerance,
            layerID = events.LayerID,
            mapReset_DesignNameID = events.MapReset_DesignNameID,
            mapReset_PriorDate = events.MapReset_PriorDate,
            positioningTech = events.PositioningTech
          };
        }


        private CellPassesResult.CellPassValue ConvertCellPass(TICCellPassValue pass)
        {
          return new CellPassesResult.CellPassValue()
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
          return new CellPassesResult.CellTargetsValue()
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
          return new CellPassesResult.FilteredPassData()
          {
            eventsValue = ConvertCellPassEvents(pass.EventValues),
            filteredPass = ConvertCellPass(pass.FilteredPass),
            targetsValue = ConvertCellPassTargets(pass.TargetValues)
          };
        }

        private CellPassesResult.FilteredPassData[] ConvertFilteredPassData(TICFilteredMultiplePassInfo passes)
        {
          if (passes.FilteredPassData != null)
            return Array.ConvertAll<TICFilteredPassData, CellPassesResult.FilteredPassData>(passes.FilteredPassData,
                ConvertFilteredPassDataItem);
          return null;
        }
        #endregion

  }
}