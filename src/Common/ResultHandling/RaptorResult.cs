using ASNodeDecls;
using DesignProfilerDecls;
using TAGProcServiceDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Common.ResultHandling
{
  public class RaptorResult : ContractExecutionStatesEnum
  {
    public static void AddErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
    {
      contractExecutionStates.DynamicAddwithOffset("OK", (int) TASNodeErrorStatus.asneOK);
      contractExecutionStates.DynamicAddwithOffset("Unknown error", (int) TASNodeErrorStatus.asneUnknown);
      contractExecutionStates.DynamicAddwithOffset("Exception occurred", (int) TASNodeErrorStatus.asneException);
      contractExecutionStates.DynamicAddwithOffset("Unsupported coordinate system definition file type",
        (int) TASNodeErrorStatus.asneUnsupportedCSDFileType);
      contractExecutionStates.DynamicAddwithOffset("Could not convert coordinate system definition file",
        (int) TASNodeErrorStatus.asneCouldNotConvertCSDFile);
      contractExecutionStates.DynamicAddwithOffset("Failed to write coordinate system definition stream",
        (int) TASNodeErrorStatus.asneFailedToWriteCSDStream);
      contractExecutionStates.DynamicAddwithOffset("Failed on profile request",
        (int) TASNodeErrorStatus.asneFailedOnRequestProfile);
      contractExecutionStates.DynamicAddwithOffset("No such data model",
        (int) TASNodeErrorStatus.asneNoSuchDataModel);
      contractExecutionStates.DynamicAddwithOffset("Unsupported display type",
        (int) TASNodeErrorStatus.asneUnsupportedDisplayType);
      contractExecutionStates.DynamicAddwithOffset("Failed on request of colour graduated profilee",
        (int) TASNodeErrorStatus.asneFailedOnRequestColourGraduatedProfile);
      contractExecutionStates.DynamicAddwithOffset("Failed to convert client WGS84 coordinates",
        (int) TASNodeErrorStatus.asneFailedToConvertClientWGSCoords);
      contractExecutionStates.DynamicAddwithOffset("Failed to request sub-grid existence map",
        (int) TASNodeErrorStatus.asneFailedToRequestSubgridExistenceMap);
      contractExecutionStates.DynamicAddwithOffset("Invalid coordinate range",
        (int) TASNodeErrorStatus.asneInvalidCoordinateRange);
      contractExecutionStates.DynamicAddwithOffset("Failed to request data model statistics",
        (int) TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics);
      contractExecutionStates.DynamicAddwithOffset("Failed to request coordinate system projection file",
        (int) TASNodeErrorStatus.asneFailedOnRequestCoordinateSystemProjectionFile);
      contractExecutionStates.DynamicAddwithOffset("Coordinate system is empty",
        (int) TASNodeErrorStatus.asneEmptyCoordinateSystem);
      contractExecutionStates.DynamicAddwithOffset("Request has been aborted due to pipeline timeout",
        (int) TASNodeErrorStatus.asneAbortedDueToPipelineTimeout);
      contractExecutionStates.DynamicAddwithOffset("Unsupported filter attribute",
        (int) TASNodeErrorStatus.asneUnsupportedFilterAttribute);
      contractExecutionStates.DynamicAddwithOffset("Service stopped", (int) TASNodeErrorStatus.asneServiceStopped);
      contractExecutionStates.DynamicAddwithOffset("Schedule load is too high",
        (int) TASNodeErrorStatus.asneRequestScheduleLoadTooHigh);
      contractExecutionStates.DynamicAddwithOffset("Schedule failure",
        (int) TASNodeErrorStatus.asneRequestScheduleFailure);
      contractExecutionStates.DynamicAddwithOffset("Schedule timeout",
        (int) TASNodeErrorStatus.asneRequestScheduleTimeout);
      contractExecutionStates.DynamicAddwithOffset("Request has been cancelled",
        (int) TASNodeErrorStatus.asneRequestHasBeenCancelled);
      contractExecutionStates.DynamicAddwithOffset("Failed to obtain coordinate system interlock",
        (int) TASNodeErrorStatus.asneFailedToObtainCoordinateSystemInterlock);
      contractExecutionStates.DynamicAddwithOffset(
        "Failed to request coordinate system horizontal adjustment file",
        (int) TASNodeErrorStatus.asneFailedOnRequestCoordinateSystemHorizontalAdjustmentFile);
      contractExecutionStates.DynamicAddwithOffset("No connection to server",
        (int) TASNodeErrorStatus.asneNoConnectionToServer);
      contractExecutionStates.DynamicAddwithOffset("Invalid response code",
        (int) TASNodeErrorStatus.asneInvalidResponseCode);
      contractExecutionStates.DynamicAddwithOffset("No result has been returned",
        (int) TASNodeErrorStatus.asneNoResultReturned);
      contractExecutionStates.DynamicAddwithOffset("Failed to notify that coordinate system was changed",
        (int) TASNodeErrorStatus.asneFailedToNotifyCSChange);
      contractExecutionStates.DynamicAddwithOffset("Failed to create DCtoIRecord converter",
        (int) TASNodeErrorStatus.asneFailedToCreateDCToIRecordConverter);
      contractExecutionStates.DynamicAddwithOffset("Failed to get coordinate systems settings",
        (int) TASNodeErrorStatus.asneFailedToGetCSSettings);
      contractExecutionStates.DynamicAddwithOffset("Coordinate system is incomplete",
        (int) TASNodeErrorStatus.asneDCToIRecIncompleteCS);
      contractExecutionStates.DynamicAddwithOffset("Failed to create CSIB",
        (int) TASNodeErrorStatus.asneDCToIRecFailedCreateCSIB);
      contractExecutionStates.DynamicAddwithOffset("Failed to get geoid information",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToGetGeoidInfo);
      contractExecutionStates.DynamicAddwithOffset("Unable to retrieve zone parameters",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToGetZoneParams);
      contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB constant separation geoid",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateConstGeoid);
      contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB datum grid file",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateDatumGrid);
      contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB ellipsoid",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateEllipsoid);
      contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Grid Geoid",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateGridGeoid);
      contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Molodensky datum",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateMolodenskyDatum);
      contractExecutionStates.DynamicAddwithOffset(
        "Failed to instantiate CSIB Multiple Regression Parameter datum",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateMultiRegressionDatum);
      contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Seven Parameter datum",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateSevenParamsDatum);
      contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB WGS84 datum",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateWGS84Datum);
      contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Zone Group",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateZoneGroup);
      contractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Zone Based Site",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateZoneBasedSite);
      contractExecutionStates.DynamicAddwithOffset("Failed to create an IAZIParameters object",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateAZIParamsObject);
      contractExecutionStates.DynamicAddwithOffset("Unable to create an ICSIB object",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToCreateCSIBObject);
      contractExecutionStates.DynamicAddwithOffset("Failed to open Calibration reader",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToOpenCalibrationReader);
      contractExecutionStates.DynamicAddwithOffset("Unable to set zone parameters",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToSetZoneParams);
      contractExecutionStates.DynamicAddwithOffset("Failed to read CSIB",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToReadCSIB);
      contractExecutionStates.DynamicAddwithOffset("Failed to read in CSIB",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToReadInCSIB);
      contractExecutionStates.DynamicAddwithOffset("Failed to read the ZoneBased site",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToReadZoneBasedSite);
      contractExecutionStates.DynamicAddwithOffset("Failed to read the zone",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToReadZone);
      contractExecutionStates.DynamicAddwithOffset("Failed to write datum",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToWriteDatum);
      contractExecutionStates.DynamicAddwithOffset("Failed to write geoid",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToWriteGeoid);
      contractExecutionStates.DynamicAddwithOffset("Failed to write CSIB",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToWriteCSIB);
      contractExecutionStates.DynamicAddwithOffset("Failed to set zone info",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToSetZoneInfo);
      contractExecutionStates.DynamicAddwithOffset("Inifinite adjustment slope value",
        (int) TASNodeErrorStatus.asneDCToIRecInifiniteAdjustmentSlopeValue);
      contractExecutionStates.DynamicAddwithOffset("Invalid ellipsoid",
        (int) TASNodeErrorStatus.asneDCToIRecInvalidEllipsoid);
      contractExecutionStates.DynamicAddwithOffset("The datum CSIB failed to load",
        (int) TASNodeErrorStatus.asneDCToIRecDatumFailedToLoad);
      contractExecutionStates.DynamicAddwithOffset("Failed to load CSIB",
        (int) TASNodeErrorStatus.asneDCToIRecFailedToLoadCSIB);
      contractExecutionStates.DynamicAddwithOffset("Not WGS84 ellipsoid",
        (int) TASNodeErrorStatus.asneDCToIRecNotWGS84Ellipsoid);
      contractExecutionStates.DynamicAddwithOffset("Not WGS84 ellipsoid in datum record",
        (int) TASNodeErrorStatus.asneDCToIRecNotWGS84EllipsoidSameAsProj);
      contractExecutionStates.DynamicAddwithOffset("Current projection should be scaled",
        (int) TASNodeErrorStatus.asneDCToIRecScaleOnlyProj);
      contractExecutionStates.DynamicAddwithOffset("Unknown coordinate system type",
        (int) TASNodeErrorStatus.asneDCToIRecUnknownCSType);
      contractExecutionStates.DynamicAddwithOffset("Unknown datum adjustment was encountered and ignored",
        (int) TASNodeErrorStatus.asneDCToIRecUnknownDatumModel);
      contractExecutionStates.DynamicAddwithOffset("Unknown geoid model was encountered and ignored",
        (int) TASNodeErrorStatus.asneDCToIRecUnknownGeoidModel);
      contractExecutionStates.DynamicAddwithOffset("Unknown projection type",
        (int) TASNodeErrorStatus.asneDCToIRecUnknownProjType);
      contractExecutionStates.DynamicAddwithOffset("Unsupported datum",
        (int) TASNodeErrorStatus.asneDCToIRecUnsupportedDatum);
      contractExecutionStates.DynamicAddwithOffset("Unsupported geoid",
        (int) TASNodeErrorStatus.asneDCToIRecUnsupportedGeoid);
      contractExecutionStates.DynamicAddwithOffset("Unsupported zone orientation",
        (int) TASNodeErrorStatus.asneDCToIRecUnsupportedZoneOrientation);
      contractExecutionStates.DynamicAddwithOffset("Failed to request file from TCC",
        (int) TASNodeErrorStatus.asneFailedToRequestFileFromTCC);
      contractExecutionStates.DynamicAddwithOffset("Failed to read linework boundary file",
        (int) TASNodeErrorStatus.asneFailedToReadLineworkBoundaryFile);
      contractExecutionStates.DynamicAddwithOffset("No boundaries in linework file",
        (int) TASNodeErrorStatus.asneNoBoundariesInLineworkFile);
      contractExecutionStates.DynamicAddwithOffset("Failed to perform coordinate conversion",
        (int) TASNodeErrorStatus.asneFailedToPerformCoordinateConversion);
      contractExecutionStates.DynamicAddwithOffset("No production data found",
        (int) TASNodeErrorStatus.asneNoProductionDataFound);
      contractExecutionStates.DynamicAddwithOffset("Invalid plan extents",
        (int) TASNodeErrorStatus.asneInvalidPlanExtents);
      contractExecutionStates.DynamicAddwithOffset("No design provided",
        (int) TASNodeErrorStatus.asneNoDesignProvided);
      contractExecutionStates.DynamicAddwithOffset("No data on production data export",
        (int) TASNodeErrorStatus.asneExportNoData);
      contractExecutionStates.DynamicAddwithOffset("Production data export timeout",
        (int) TASNodeErrorStatus.asneExportTimeOut);
      contractExecutionStates.DynamicAddwithOffset("Production data export cancelled",
        (int) TASNodeErrorStatus.asneExportCancelled);
      contractExecutionStates.DynamicAddwithOffset("Production data export limit reached",
        (int) TASNodeErrorStatus.asneExportLimitReached);
      contractExecutionStates.DynamicAddwithOffset("Invalid data range on production data export",
        (int) TASNodeErrorStatus.asneExportInvalidDateRange);
      contractExecutionStates.DynamicAddwithOffset("No overlap on production data export ranges",
        (int) TASNodeErrorStatus.asneExportDateRangesNoOverlap);
      contractExecutionStates.DynamicAddwithOffset(
        "Invalid page size or number for patch request. Try reducing the area being requested.",
        (int) TASNodeErrorStatus.asneInvalidArgument);
      contractExecutionStates.DynamicAddwithOffset("No coordinate system assigned to project.",
        (int) TASNodeErrorStatus.asneNoCoordinateSystem);
      contractExecutionStates.DynamicAddwithOffset(
        "Failed to load coordinate system data from project's data model file.",
        (int) TASNodeErrorStatus.asneFailedToLoadCoordinateSystem);
      contractExecutionStates.DynamicAddwithOffset("Failed to create coordinate transformer.",
        (int) TASNodeErrorStatus.asneFailedToCreateCoordinateTransformer);
    }


    public static void AddTagProcessorErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
    {
      contractExecutionStates.DynamicAddwithOffset("Tagfile OK", (int) TTAGProcServerProcessResult.tpsprOK);
      contractExecutionStates.DynamicAddwithOffset("Tagfile Unknown error",
        (int) TTAGProcServerProcessResult.tpsprUnknown);
      contractExecutionStates.DynamicAddwithOffset("OnSubmissionBase. Connection Failure.",
        (int) TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure);
      contractExecutionStates.DynamicAddwithOffset("OnSubmissionVerb. Connection Failure.",
        (int) TTAGProcServerProcessResult.tpsprOnSubmissionVerbConnectionFailure);
      contractExecutionStates.DynamicAddwithOffset("OnSubmissionResult. ConnectionFailure.",
        (int) TTAGProcServerProcessResult.tpsprOnSubmissionResultConnectionFailure);
      contractExecutionStates.DynamicAddwithOffset("The TAG file was found to be corrupted on its pre-processing scan.",
        (int) TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData);
      contractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Unknown Machine AssetID.",
        (int) TTAGProcServerProcessResult.tpsprOnChooseMachineUnknownMachine);
      contractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Invalid TagFile on selecting machine AssetID.",
        (int) TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidTagFile);
      contractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Machine Subscriptions Invalid.",
        (int) TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions);
      contractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Unable To Determine Machine.",
        (int) TTAGProcServerProcessResult.tpsprOnChooseMachineUnableToDetermineMachine);
      contractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. Unable To Determine DataModel.",
        (int) TTAGProcServerProcessResult.tpsprOnChooseDataModelUnableToDetermineDataModel);
      contractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. Could Not Convert DataModel Boundary To Grid.",
        (int) TTAGProcServerProcessResult.tpsprOnChooseDataModelCouldNotConvertDataModelBoundaryToGrid);
      contractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. No GridEpochs Found In TAGFile.",
        (int) TTAGProcServerProcessResult.tpsprOnChooseDataModelNoGridEpochsFoundInTAGFile);
      contractExecutionStates.DynamicAddwithOffset(
        "OnChooseDataModel. Supplied DataModel Boundary Contains Insufficeint Vertices.",
        (int) TTAGProcServerProcessResult.tpsprOnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices);
      contractExecutionStates.DynamicAddwithOffset(
        "OnChooseDataModel. First Epoch Blade Position Does Not Lie Within Project Boundary.",
        (int) TTAGProcServerProcessResult.tpsprOnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary);
      contractExecutionStates.DynamicAddwithOffset("OnOverrideEvent. Failed on event's date validation.",
        (int) TTAGProcServerProcessResult.tpsprFailedEventDateValidation);
      contractExecutionStates.DynamicAddwithOffset("OnProcessTAGFile. Invalid tag file submission message type.",
        (int) TTAGProcServerProcessResult.tpsprInvalidTagFileSubmissionMessageType);
      contractExecutionStates.DynamicAddwithOffset(
        "OnProcessTAGFile. TAG file already exists in data model's processing folder.",
        (int) TTAGProcServerProcessResult.tpsprTAGFileAlreadyExistsInProcessingFolderForDataModel);
      contractExecutionStates.DynamicAddwithOffset(
        "OnProcessTAGFile. TAG file already exists in data model's processing archival queue.",
        (int) TTAGProcServerProcessResult.tpsprTAGFileAlreadyExistsInProcessingArchivalQueueForDataModel);
      contractExecutionStates.DynamicAddwithOffset("OnProcessTAGFile. Service has been stopped.",
        (int) TTAGProcServerProcessResult.tpsprServiceStopped);
      contractExecutionStates.DynamicAddwithOffset("OnOverrideEvent. Failed on target data validation.",
        (int) TTAGProcServerProcessResult.tpsprFailedValidation);
      contractExecutionStates.DynamicAddwithOffset("TFA service error. Cannot request Project or Asset from TFA.",
        (int)TTAGProcServerProcessResult.tpsprTFAServiceError);

    }

    public static void AddDesignProfileErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
    {
      contractExecutionStates.DynamicAddwithOffset("Design Profiler OK", (int) TDesignProfilerRequestResult.dppiOK);
      contractExecutionStates.DynamicAddwithOffset("Design Profile Unknown Error",
        (int) TDesignProfilerRequestResult.dppiUnknownError);
      contractExecutionStates.DynamicAddwithOffset("Could Not Connect To Server",
        (int) TDesignProfilerRequestResult.dppiCouldNotConnectToServer);
      contractExecutionStates.DynamicAddwithOffset("Failed To Convert Client WGS Coords",
        (int) TDesignProfilerRequestResult.dppiFailedToConvertClientWGSCoords);
      contractExecutionStates.DynamicAddwithOffset("Failed To Load Design File",
        (int) TDesignProfilerRequestResult.dppiFailedToLoadDesignFile);
      contractExecutionStates.DynamicAddwithOffset("Profile Generation Failure",
        (int) TDesignProfilerRequestResult.dppiProfileGenerationFailure);
      contractExecutionStates.DynamicAddwithOffset("Failed To Result From Response Verb",
        (int) TDesignProfilerRequestResult.dppiFailedToResultFromResponseVerb);
      contractExecutionStates.DynamicAddwithOffset("Unsupported Design Type",
        (int) TDesignProfilerRequestResult.dppiUnsupportedDesignType);
      contractExecutionStates.DynamicAddwithOffset("Failed To Save Intermediary Result",
        (int) TDesignProfilerRequestResult.dppiFailedToSaveIntermediaryResult);
      contractExecutionStates.DynamicAddwithOffset("Failed To Load Intermediary Result",
        (int) TDesignProfilerRequestResult.dppiFailedToLoadIntermediaryResult);
      contractExecutionStates.DynamicAddwithOffset("No Elevations In Requested Patch",
        (int) TDesignProfilerRequestResult.dppiNoElevationsInRequestedPatch);
      contractExecutionStates.DynamicAddwithOffset("Service Stopped",
        (int) TDesignProfilerRequestResult.dppiServiceStopped);
      contractExecutionStates.DynamicAddwithOffset("Design Does Not Support Subgrid Overlay Index",
        (int) TDesignProfilerRequestResult.dppiDesignDoesNotSupportSubgridOverlayIndex);
      contractExecutionStates.DynamicAddwithOffset("Failed To Save Subgrid Overlay Index To Stream",
        (int) TDesignProfilerRequestResult.dppiFailedToSaveSubgridOverlayIndexToStream);
      contractExecutionStates.DynamicAddwithOffset("Alignment Contains No Elements",
        (int) TDesignProfilerRequestResult.dppiAlignmentContainsNoElements);
      contractExecutionStates.DynamicAddwithOffset("Alignment Contains No Stationing",
        (int) TDesignProfilerRequestResult.dppiAlignmentContainsNoStationing);
      contractExecutionStates.DynamicAddwithOffset("Alignment Contains Invalid Stationing",
        (int) TDesignProfilerRequestResult.dppiAlignmentContainsInvalidStationing);
      contractExecutionStates.DynamicAddwithOffset("Invalid Station Values",
        (int) TDesignProfilerRequestResult.dppiInvalidStationValues);
      contractExecutionStates.DynamicAddwithOffset("No Selected Site Model",
        (int) TDesignProfilerRequestResult.dppiNoSelectedSiteModel);
      contractExecutionStates.DynamicAddwithOffset("Failed To Compute Alignment Vertices",
        (int) TDesignProfilerRequestResult.dppiFailedToComputeAlignmentVertices);
      contractExecutionStates.DynamicAddwithOffset("Failed To Add Item To Cache",
        (int) TDesignProfilerRequestResult.dppiFailedToAddItemToCache);
      contractExecutionStates.DynamicAddwithOffset("Failed To Update Cache",
        (int) TDesignProfilerRequestResult.dppiFailedToUpdateCache);
      contractExecutionStates.DynamicAddwithOffset("Failed Get Data Model Spatial Extents",
        (int) TDesignProfilerRequestResult.dppiFailedGetDataModelSpatialExtents);
      contractExecutionStates.DynamicAddwithOffset("No Alignments Found",
        (int) TDesignProfilerRequestResult.dppiNoAlignmentsFound);
      contractExecutionStates.DynamicAddwithOffset("Invalid Response Code",
        (int) TDesignProfilerRequestResult.dppiInvalidResponseCode);
    }

    public static void AddExportErrorMessages(ContractExecutionStatesEnum contractExecutionStates)
    {
      contractExecutionStates.DynamicAddwithOffset("Export OK",
        (int)TASNodeExportStatus.asnesOK);
      contractExecutionStates.DynamicAddwithOffset("Export Unknown Error",
        (int)TASNodeExportStatus.asnesUnknown);
      contractExecutionStates.DynamicAddwithOffset("No data for export",
        (int)TASNodeExportStatus.asnesNoData);
      contractExecutionStates.DynamicAddwithOffset("Export timeout",
        (int)TASNodeExportStatus.asnesTimeOut);
      contractExecutionStates.DynamicAddwithOffset("Export cancelled",
        (int)TASNodeExportStatus.asnesCancelled);
      contractExecutionStates.DynamicAddwithOffset("Export limit reached",
        (int)TASNodeExportStatus.asnesLimitReached);
      contractExecutionStates.DynamicAddwithOffset("Invalid date range for export",
        (int)TASNodeExportStatus.asnesInvalidDateRange);
      contractExecutionStates.DynamicAddwithOffset("No overlap for export date ranges",
        (int)TASNodeExportStatus.asnesDateRangesDoNotOverlap);
    }
  }
}
