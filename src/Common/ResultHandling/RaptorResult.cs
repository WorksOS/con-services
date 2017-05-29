using ASNodeDecls;
using DesignProfilerDecls;
using TAGProcServiceDecls;
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.Common.ResultHandling
{
    public class RaptorResult
    {
        public static void AddErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
        {
          AddErrorMessages(contractExecutionStates, contractExecutionStates.DefaultDynamicOffset);
        }

        public static void AddErrorMessages(ContractExecutionStatesEnum contractExecutionStates, int offset)
        {
            contractExecutionStates.DynamicAddwithOffset("OK", (int) TASNodeErrorStatus.asneOK, offset);
            contractExecutionStates.DynamicAddwithOffset("Unknown error", (int) TASNodeErrorStatus.asneUnknown, offset);
            contractExecutionStates.DynamicAddwithOffset("Exception occurred", (int) TASNodeErrorStatus.asneException, offset);
            contractExecutionStates.DynamicAddwithOffset("Unsupported coordinate system definition file type",
                    (int) TASNodeErrorStatus.asneUnsupportedCSDFileType, offset);
            contractExecutionStates.DynamicAddwithOffset("Could not convert coordinate system definition file",
                    (int) TASNodeErrorStatus.asneCouldNotConvertCSDFile, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to write coordinate system definition stream",
                    (int) TASNodeErrorStatus.asneFailedToWriteCSDStream, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed on profile request",
                    (int) TASNodeErrorStatus.asneFailedOnRequestProfile, offset);
            contractExecutionStates.DynamicAddwithOffset("No such data model",
                    (int) TASNodeErrorStatus.asneNoSuchDataModel, offset);
            contractExecutionStates.DynamicAddwithOffset("Unsupported display type",
                    (int) TASNodeErrorStatus.asneUnsupportedDisplayType, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed on request of colour graduated profilee",
                    (int) TASNodeErrorStatus.asneFailedOnRequestColourGraduatedProfile, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to convert client WGS84 coordinates",
                    (int) TASNodeErrorStatus.asneFailedToConvertClientWGSCoords, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to request sub-grid existence map",
                    (int) TASNodeErrorStatus.asneFailedToRequestSubgridExistenceMap, offset);
            contractExecutionStates.DynamicAddwithOffset("Invalid coordinate range",
                    (int) TASNodeErrorStatus.asneInvalidCoordinateRange, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to request data model statistics",
                    (int) TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to request coordinate system projection file",
                    (int) TASNodeErrorStatus.asneFailedOnRequestCoordinateSystemProjectionFile, offset);
            contractExecutionStates.DynamicAddwithOffset("Coordinate system is empty",
                    (int) TASNodeErrorStatus.asneEmptyCoordinateSystem, offset);
            contractExecutionStates.DynamicAddwithOffset("Request has been aborted due to pipeline timeout",
                    (int) TASNodeErrorStatus.asneAbortedDueToPipelineTimeout, offset);
            contractExecutionStates.DynamicAddwithOffset("Unsupported filter attribute",
                    (int) TASNodeErrorStatus.asneUnsupportedFilterAttribute, offset);
            contractExecutionStates.DynamicAddwithOffset("Service stopped", (int) TASNodeErrorStatus.asneServiceStopped, offset);
            contractExecutionStates.DynamicAddwithOffset("Schedule load is too high",
                    (int) TASNodeErrorStatus.asneRequestScheduleLoadTooHigh, offset);
            contractExecutionStates.DynamicAddwithOffset("Schedule failure",
                    (int) TASNodeErrorStatus.asneRequestScheduleFailure, offset);
            contractExecutionStates.DynamicAddwithOffset("Schedule timeout",
                    (int) TASNodeErrorStatus.asneRequestScheduleTimeout, offset);
            contractExecutionStates.DynamicAddwithOffset("Request has been cancelled",
                    (int) TASNodeErrorStatus.asneRequestHasBeenCancelled, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to obtain coordinate system interlock",
                    (int) TASNodeErrorStatus.asneFailedToObtainCoordinateSystemInterlock, offset);
            contractExecutionStates.DynamicAddwithOffset(
                    "Failed to request coordinate system horizontal adjustment file",
                    (int) TASNodeErrorStatus.asneFailedOnRequestCoordinateSystemHorizontalAdjustmentFile, offset);
            contractExecutionStates.DynamicAddwithOffset("No connection to server",
                    (int) TASNodeErrorStatus.asneNoConnectionToServer, offset);
            contractExecutionStates.DynamicAddwithOffset("Invalid response code",
                    (int) TASNodeErrorStatus.asneInvalidResponseCode, offset);
            contractExecutionStates.DynamicAddwithOffset("No result has been returned",
                    (int) TASNodeErrorStatus.asneNoResultReturned, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to notify that coordinate system was changed",
                    (int) TASNodeErrorStatus.asneFailedToNotifyCSChange, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to create DCtoIRecord converter",
                    (int) TASNodeErrorStatus.asneFailedToCreateDCToIRecordConverter, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to get coordinate systems settings",
                    (int) TASNodeErrorStatus.asneFailedToGetCSSettings, offset);
            contractExecutionStates.DynamicAddwithOffset("Coordinate system is incomplete",
                    (int) TASNodeErrorStatus.asneDCToIRecIncompleteCS, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to create CSIB",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedCreateCSIB, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to get geoid information",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToGetGeoidInfo, offset);
            contractExecutionStates.DynamicAddwithOffset("Unable to retrieve zone parameters",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToGetZoneParams, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB constant separation geoid",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateConstGeoid, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB datum grid file",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateDatumGrid, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB ellipsoid",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateEllipsoid, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Grid Geoid",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateGridGeoid, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Molodensky datum",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateMolodenskyDatum, offset);
            contractExecutionStates.DynamicAddwithOffset(
                    "Failed to instantiate CSIB Multiple Regression Parameter datum",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateMultiRegressionDatum, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Seven Parameter datum",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateSevenParamsDatum, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB WGS84 datum",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateWGS84Datum, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Zone Group",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateZoneGroup, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Zone Based Site",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateZoneBasedSite, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to create an IAZIParameters object",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateAZIParamsObject, offset);
            contractExecutionStates.DynamicAddwithOffset("Unable to create an ICSIB object",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateCSIBObject, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to open Calibration reader",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToOpenCalibrationReader, offset);
            contractExecutionStates.DynamicAddwithOffset("Unable to set zone parameters",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToSetZoneParams, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to read CSIB",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToReadCSIB, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to read in CSIB",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToReadInCSIB, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to read the ZoneBased site",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToReadZoneBasedSite, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to read the zone",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToReadZone, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to write datum",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToWriteDatum, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to write geoid",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToWriteGeoid, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to write CSIB",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToWriteCSIB, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to set zone info",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToSetZoneInfo, offset);
            contractExecutionStates.DynamicAddwithOffset("Inifinite adjustment slope value",
                    (int) TASNodeErrorStatus.asneDCToIRecInifiniteAdjustmentSlopeValue, offset);
            contractExecutionStates.DynamicAddwithOffset("Invalid ellipsoid",
                    (int) TASNodeErrorStatus.asneDCToIRecInvalidEllipsoid, offset);
            contractExecutionStates.DynamicAddwithOffset("The datum CSIB failed to load",
                    (int) TASNodeErrorStatus.asneDCToIRecDatumFailedToLoad, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to load CSIB",
                    (int) TASNodeErrorStatus.asneDCToIRecFailedToLoadCSIB, offset);
            contractExecutionStates.DynamicAddwithOffset("Not WGS84 ellipsoid",
                    (int) TASNodeErrorStatus.asneDCToIRecNotWGS84Ellipsoid, offset);
            contractExecutionStates.DynamicAddwithOffset("Not WGS84 ellipsoid in datum record",
                    (int) TASNodeErrorStatus.asneDCToIRecNotWGS84EllipsoidSameAsProj, offset);
            contractExecutionStates.DynamicAddwithOffset("Current projection should be scaled",
                    (int) TASNodeErrorStatus.asneDCToIRecScaleOnlyProj, offset);
            contractExecutionStates.DynamicAddwithOffset("Unknown coordinate system type",
                    (int) TASNodeErrorStatus.asneDCToIRecUnknownCSType, offset);
            contractExecutionStates.DynamicAddwithOffset("Unknown datum adjustment was encountered and ignored",
                    (int) TASNodeErrorStatus.asneDCToIRecUnknownDatumModel, offset);
            contractExecutionStates.DynamicAddwithOffset("Unknown geoid model was encountered and ignored",
                    (int) TASNodeErrorStatus.asneDCToIRecUnknownGeoidModel, offset);
            contractExecutionStates.DynamicAddwithOffset("Unknown projection type",
                    (int) TASNodeErrorStatus.asneDCToIRecUnknownProjType, offset);
            contractExecutionStates.DynamicAddwithOffset("Unsupported datum",
                    (int) TASNodeErrorStatus.asneDCToIRecUnsupportedDatum, offset);
            contractExecutionStates.DynamicAddwithOffset("Unsupported geoid",
                    (int) TASNodeErrorStatus.asneDCToIRecUnsupportedGeoid, offset);
            contractExecutionStates.DynamicAddwithOffset("Unsupported zone orientation",
                    (int) TASNodeErrorStatus.asneDCToIRecUnsupportedZoneOrientation, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to request file from TCC",
                    (int) TASNodeErrorStatus.asneFailedToRequestFileFromTCC, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to read linework boundary file",
                    (int) TASNodeErrorStatus.asneFailedToReadLineworkBoundaryFile, offset);
            contractExecutionStates.DynamicAddwithOffset("No boundaries in linework file",
                    (int) TASNodeErrorStatus.asneNoBoundariesInLineworkFile, offset);
            contractExecutionStates.DynamicAddwithOffset("Failed to perform coordinate conversion",
                    (int) TASNodeErrorStatus.asneFailedToPerformCoordinateConversion, offset);
            contractExecutionStates.DynamicAddwithOffset("No production data found",
                    (int) TASNodeErrorStatus.asneNoProductionDataFound, offset);
            contractExecutionStates.DynamicAddwithOffset("Invalid plan extents",
                    (int) TASNodeErrorStatus.asneInvalidPlanExtents, offset);
            contractExecutionStates.DynamicAddwithOffset("No design provided",
                    (int) TASNodeErrorStatus.asneNoDesignProvided, offset);
            contractExecutionStates.DynamicAddwithOffset("No data on production data export",
                    (int) TASNodeErrorStatus.asneExportNoData, offset);
            contractExecutionStates.DynamicAddwithOffset("Production data export timeout",
                    (int) TASNodeErrorStatus.asneExportTimeOut, offset);
            contractExecutionStates.DynamicAddwithOffset("Production data export cancelled",
                    (int) TASNodeErrorStatus.asneExportCancelled, offset);
            contractExecutionStates.DynamicAddwithOffset("Production data export limit reached",
                    (int) TASNodeErrorStatus.asneExportLimitReached, offset);
            contractExecutionStates.DynamicAddwithOffset("Invalid data range on production data export",
                    (int) TASNodeErrorStatus.asneExportInvalidDateRange, offset);
            contractExecutionStates.DynamicAddwithOffset("No overlap on production data export ranges",
                    (int) TASNodeErrorStatus.asneExportDateRangesNoOverlap, offset);
            contractExecutionStates.DynamicAddwithOffset("Invalid page size or number for patch request. Try reducing the area being requested.",
                    (int) TASNodeErrorStatus.asneInvalidArgument, offset);
        }

      public static void AddTagProcessorErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
      {
        AddTagProcessorErrorMessages(contractExecutionStates, contractExecutionStates.DefaultDynamicOffset);
      }

      public static void AddTagProcessorErrorMessages(ContractExecutionStatesEnum contractExecutionStates, int offset)
      {
        contractExecutionStates.DynamicAddwithOffset("OK", (int)TTAGProcServerProcessResult.tpsprOK, offset);
        contractExecutionStates.DynamicAddwithOffset("Unknown error", (int)TTAGProcServerProcessResult.tpsprUnknown, offset);
        contractExecutionStates.DynamicAddwithOffset("OnSubmissionBase. Connection Failure.", (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure, offset);
        contractExecutionStates.DynamicAddwithOffset("OnSubmissionVerb. Connection Failure.", (int)TTAGProcServerProcessResult.tpsprOnSubmissionVerbConnectionFailure, offset);
        contractExecutionStates.DynamicAddwithOffset("OnSubmissionResult. ConnectionFailure.", (int)TTAGProcServerProcessResult.tpsprOnSubmissionResultConnectionFailure, offset);
        contractExecutionStates.DynamicAddwithOffset("The TAG file was found to be corrupted on its pre-processing scan.", (int)TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData, offset);
        contractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Unknown Machine AssetID.", (int)TTAGProcServerProcessResult.tpsprOnChooseMachineUnknownMachine, offset);
        contractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Invalid TagFile on selecting machine AssetID.", (int)TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidTagFile, offset);
        contractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Machine Subscriptions Invalid.", (int)TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions, offset);
        contractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Unable To Determine Machine.", (int)TTAGProcServerProcessResult.tpsprOnChooseMachineUnableToDetermineMachine, offset);
        contractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. Unable To Determine DataModel.", (int)TTAGProcServerProcessResult.tpsprOnChooseDataModelUnableToDetermineDataModel, offset);
        contractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. Could Not Convert DataModel Boundary To Grid.", (int)TTAGProcServerProcessResult.tpsprOnChooseDataModelCouldNotConvertDataModelBoundaryToGrid, offset);
        contractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. No GridEpochs Found In TAGFile.", (int)TTAGProcServerProcessResult.tpsprOnChooseDataModelNoGridEpochsFoundInTAGFile, offset);
        contractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. Supplied DataModel Boundary Contains Insufficeint Vertices.", (int)TTAGProcServerProcessResult.tpsprOnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices, offset);
        contractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. First Epoch Blade Position Does Not Lie Within Project Boundary.", (int)TTAGProcServerProcessResult.tpsprOnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary, offset);

      }

      public static void AddDesignProfileErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
      {
        AddDesignProfileErrorMessages(contractExecutionStates, contractExecutionStates.DefaultDynamicOffset);
      }
      public static void AddDesignProfileErrorMessages(ContractExecutionStatesEnum contractExecutionStates, int offset)
      {
        contractExecutionStates.DynamicAddwithOffset("OK", (int)TDesignProfilerRequestResult.dppiOK, offset);
        contractExecutionStates.DynamicAddwithOffset("Unknown Error", (int)TDesignProfilerRequestResult.dppiUnknownError, offset);
        contractExecutionStates.DynamicAddwithOffset("Could Not Connect To Server", (int)TDesignProfilerRequestResult.dppiCouldNotConnectToServer, offset);
        contractExecutionStates.DynamicAddwithOffset("Failed To Convert Client WGS Coords", (int)TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords, offset);
        contractExecutionStates.DynamicAddwithOffset("Failed To Load Design File", (int)TDesignProfilerRequestResult.dppiFailedToLoadDesignFile, offset);
        contractExecutionStates.DynamicAddwithOffset("Profile Generation Failure", (int)TDesignProfilerRequestResult.dppiProfileGenerationFailure, offset);
        contractExecutionStates.DynamicAddwithOffset("Failed To Result From Response Verb", (int)TDesignProfilerRequestResult.dppiFailedToResultFromResponseVerb, offset);
        contractExecutionStates.DynamicAddwithOffset("Unsupported Design Type", (int)TDesignProfilerRequestResult.dppiUnsupportedDesignType, offset);
        contractExecutionStates.DynamicAddwithOffset("Failed To Save Intermediary Result", (int)TDesignProfilerRequestResult.dppiFailedToSaveIntermediaryResult, offset);
        contractExecutionStates.DynamicAddwithOffset("Failed To Load Intermediary Result", (int)TDesignProfilerRequestResult.dppiFailedToLoadIntermediaryResult, offset);
        contractExecutionStates.DynamicAddwithOffset("No Elevations In Requested Patch", (int)TDesignProfilerRequestResult.dppiNoElevationsInRequestedPatch, offset);
        contractExecutionStates.DynamicAddwithOffset("Service Stopped", (int)TDesignProfilerRequestResult.dppiServiceStopped, offset);
        contractExecutionStates.DynamicAddwithOffset("Design Does Not Support Subgrid Overlay Index", (int)TDesignProfilerRequestResult.dppiDesignDoesNotSupportSubgridOverlayIndex, offset);
        contractExecutionStates.DynamicAddwithOffset("Failed To Save Subgrid Overlay Index To Stream", (int)TDesignProfilerRequestResult.dppiFailedToSaveSubgridOverlayIndexToStream, offset);
        contractExecutionStates.DynamicAddwithOffset("Alignment Contains No Elements", (int)TDesignProfilerRequestResult.dppiAlignmentContainsNoElements, offset);
        contractExecutionStates.DynamicAddwithOffset("Alignment Contains No Stationing", (int)TDesignProfilerRequestResult.dppiAlignmentContainsNoStationing, offset);
        contractExecutionStates.DynamicAddwithOffset("Alignment Contains Invalid Stationing", (int)TDesignProfilerRequestResult.dppiAlignmentContainsInvalidStationing, offset);
        contractExecutionStates.DynamicAddwithOffset("Invalid Station Values", (int)TDesignProfilerRequestResult.dppiInvalidStationValues, offset);
        contractExecutionStates.DynamicAddwithOffset("No Selected Site Model", (int)TDesignProfilerRequestResult.dppiNoSelectedSiteModel, offset);
        contractExecutionStates.DynamicAddwithOffset("Failed To Compute Alignment Vertices", (int)TDesignProfilerRequestResult.dppiFailedToComputeAlignmentVertices, offset);
        contractExecutionStates.DynamicAddwithOffset("Failed To Add Item To Cache", (int)TDesignProfilerRequestResult.dppiFailedToAddItemToCache, offset);
        contractExecutionStates.DynamicAddwithOffset("Failed To Update Cache", (int)TDesignProfilerRequestResult.dppiFailedToUpdateCache, offset);
        contractExecutionStates.DynamicAddwithOffset("Failed Get Data Model Spatial Extents", (int)TDesignProfilerRequestResult.dppiFailedGetDataModelSpatialExtents, offset);
        contractExecutionStates.DynamicAddwithOffset("No Alignments Found", (int)TDesignProfilerRequestResult.dppiNoAlignmentsFound, offset);
        contractExecutionStates.DynamicAddwithOffset("Invalid Response Code", (int)TDesignProfilerRequestResult.dppiInvalidResponseCode, offset);  
    }
  }
}
