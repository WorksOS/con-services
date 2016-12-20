using ASNodeDecls;
using TAGProcServiceDecls;

namespace VSS.Raptor.Service.WebApiModels.ResultHandling
{
  public class RaptorResult
  {
    public static void AddErrorMessages(ContractExecutionStatesEnum ContractExecutionStates)
    {
      ContractExecutionStates.DynamicAddwithOffset("OK", (int)TASNodeErrorStatus.asneOK);
      ContractExecutionStates.DynamicAddwithOffset("Unknown error", (int)TASNodeErrorStatus.asneUnknown);
      ContractExecutionStates.DynamicAddwithOffset("Exception occurred", (int)TASNodeErrorStatus.asneException);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported coordinate system definition file type",
              (int)TASNodeErrorStatus.asneUnsupportedCSDFileType);
      ContractExecutionStates.DynamicAddwithOffset("Could not convert coordinate system definition file",
              (int)TASNodeErrorStatus.asneCouldNotConvertCSDFile);
      ContractExecutionStates.DynamicAddwithOffset("Failed to write coordinate system definition stream",
              (int)TASNodeErrorStatus.asneFailedToWriteCSDStream);
      ContractExecutionStates.DynamicAddwithOffset("Failed on profile request",
              (int)TASNodeErrorStatus.asneFailedOnRequestProfile);
      ContractExecutionStates.DynamicAddwithOffset("No such data model",
              (int)TASNodeErrorStatus.asneNoSuchDataModel);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported display type",
              (int)TASNodeErrorStatus.asneUnsupportedDisplayType);
      ContractExecutionStates.DynamicAddwithOffset("Failed on request of colour graduated profilee",
              (int)TASNodeErrorStatus.asneFailedOnRequestColourGraduatedProfile);
      ContractExecutionStates.DynamicAddwithOffset("Failed to convert client WGS84 coordinates",
              (int)TASNodeErrorStatus.asneFailedToConvertClientWGSCoords);
      ContractExecutionStates.DynamicAddwithOffset("Failed to request sub-grid existence map",
              (int)TASNodeErrorStatus.asneFailedToRequestSubgridExistenceMap);
      ContractExecutionStates.DynamicAddwithOffset("Invalid coordinate range",
              (int)TASNodeErrorStatus.asneInvalidCoordinateRange);
      ContractExecutionStates.DynamicAddwithOffset("Failed to request data model statistics",
              (int)TASNodeErrorStatus.asneFailedToRequestDatamodelStatistics);
      ContractExecutionStates.DynamicAddwithOffset("Failed to request coordinate system projection file",
              (int)TASNodeErrorStatus.asneFailedOnRequestCoordinateSystemProjectionFile);
      ContractExecutionStates.DynamicAddwithOffset("Coordinate system is empty",
              (int)TASNodeErrorStatus.asneEmptyCoordinateSystem);
      ContractExecutionStates.DynamicAddwithOffset("Request has been aborted due to pipeline timeout",
              (int)TASNodeErrorStatus.asneAbortedDueToPipelineTimeout);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported filter attribute",
              (int)TASNodeErrorStatus.asneUnsupportedFilterAttribute);
      ContractExecutionStates.DynamicAddwithOffset("Service stopped", (int)TASNodeErrorStatus.asneServiceStopped);
      ContractExecutionStates.DynamicAddwithOffset("Schedule load is too high",
              (int)TASNodeErrorStatus.asneRequestScheduleLoadTooHigh);
      ContractExecutionStates.DynamicAddwithOffset("Schedule failure",
              (int)TASNodeErrorStatus.asneRequestScheduleFailure);
      ContractExecutionStates.DynamicAddwithOffset("Schedule timeout",
              (int)TASNodeErrorStatus.asneRequestScheduleTimeout);
      ContractExecutionStates.DynamicAddwithOffset("Request has been cancelled",
              (int)TASNodeErrorStatus.asneRequestHasBeenCancelled);
      ContractExecutionStates.DynamicAddwithOffset("Failed to obtain coordinate system interlock",
              (int)TASNodeErrorStatus.asneFailedToObtainCoordinateSystemInterlock);
      ContractExecutionStates.DynamicAddwithOffset(
              "Failed to request coordinate system horizontal adjustment file",
              (int)TASNodeErrorStatus.asneFailedOnRequestCoordinateSystemHorizontalAdjustmentFile);
      ContractExecutionStates.DynamicAddwithOffset("No connection to server",
              (int)TASNodeErrorStatus.asneNoConnectionToServer);
      ContractExecutionStates.DynamicAddwithOffset("Invalid response code",
              (int)TASNodeErrorStatus.asneInvalidResponseCode);
      ContractExecutionStates.DynamicAddwithOffset("No result has been returned",
              (int)TASNodeErrorStatus.asneNoResultReturned);
      ContractExecutionStates.DynamicAddwithOffset("Failed to notify that coordinate system was changed",
              (int)TASNodeErrorStatus.asneFailedToNotifyCSChange);
      ContractExecutionStates.DynamicAddwithOffset("Failed to create DCtoIRecord converter",
              (int)TASNodeErrorStatus.asneFailedToCreateDCToIRecordConverter);
      ContractExecutionStates.DynamicAddwithOffset("Failed to get coordinate systems settings",
              (int)TASNodeErrorStatus.asneFailedToGetCSSettings);
      ContractExecutionStates.DynamicAddwithOffset("Coordinate system is incomplete",
              (int)TASNodeErrorStatus.asneDCToIRecIncompleteCS);
      ContractExecutionStates.DynamicAddwithOffset("Failed to create CSIB",
              (int)TASNodeErrorStatus.asneDCToIRecFailedCreateCSIB);
      ContractExecutionStates.DynamicAddwithOffset("Failed to get geoid information",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToGetGeoidInfo);
      ContractExecutionStates.DynamicAddwithOffset("Unable to retrieve zone parameters",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToGetZoneParams);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB constant separation geoid",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateConstGeoid);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB datum grid file",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateDatumGrid);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB ellipsoid",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateEllipsoid);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Grid Geoid",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateGridGeoid);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Molodensky datum",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateMolodenskyDatum);
      ContractExecutionStates.DynamicAddwithOffset(
              "Failed to instantiate CSIB Multiple Regression Parameter datum",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateMultiRegressionDatum);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Seven Parameter datum",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateSevenParamsDatum);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB WGS84 datum",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateWGS84Datum);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Zone Group",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateZoneGroup);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Zone Based Site",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateZoneBasedSite);
      ContractExecutionStates.DynamicAddwithOffset("Failed to create an IAZIParameters object",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateAZIParamsObject);
      ContractExecutionStates.DynamicAddwithOffset("Unable to create an ICSIB object",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToCreateCSIBObject);
      ContractExecutionStates.DynamicAddwithOffset("Failed to open Calibration reader",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToOpenCalibrationReader);
      ContractExecutionStates.DynamicAddwithOffset("Unable to set zone parameters",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToSetZoneParams);
      ContractExecutionStates.DynamicAddwithOffset("Failed to read CSIB",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToReadCSIB);
      ContractExecutionStates.DynamicAddwithOffset("Failed to read in CSIB",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToReadInCSIB);
      ContractExecutionStates.DynamicAddwithOffset("Failed to read the ZoneBased site",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToReadZoneBasedSite);
      ContractExecutionStates.DynamicAddwithOffset("Failed to read the zone",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToReadZone);
      ContractExecutionStates.DynamicAddwithOffset("Failed to write datum",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToWriteDatum);
      ContractExecutionStates.DynamicAddwithOffset("Failed to write geoid",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToWriteGeoid);
      ContractExecutionStates.DynamicAddwithOffset("Failed to write CSIB",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToWriteCSIB);
      ContractExecutionStates.DynamicAddwithOffset("Failed to set zone info",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToSetZoneInfo);
      ContractExecutionStates.DynamicAddwithOffset("Inifinite adjustment slope value",
              (int)TASNodeErrorStatus.asneDCToIRecInifiniteAdjustmentSlopeValue);
      ContractExecutionStates.DynamicAddwithOffset("Invalid ellipsoid",
              (int)TASNodeErrorStatus.asneDCToIRecInvalidEllipsoid);
      ContractExecutionStates.DynamicAddwithOffset("The datum CSIB failed to load",
              (int)TASNodeErrorStatus.asneDCToIRecDatumFailedToLoad);
      ContractExecutionStates.DynamicAddwithOffset("Failed to load CSIB",
              (int)TASNodeErrorStatus.asneDCToIRecFailedToLoadCSIB);
      ContractExecutionStates.DynamicAddwithOffset("Not WGS84 ellipsoid",
              (int)TASNodeErrorStatus.asneDCToIRecNotWGS84Ellipsoid);
      ContractExecutionStates.DynamicAddwithOffset("Not WGS84 ellipsoid in datum record",
              (int)TASNodeErrorStatus.asneDCToIRecNotWGS84EllipsoidSameAsProj);
      ContractExecutionStates.DynamicAddwithOffset("Current projection should be scaled",
              (int)TASNodeErrorStatus.asneDCToIRecScaleOnlyProj);
      ContractExecutionStates.DynamicAddwithOffset("Unknown coordinate system type",
              (int)TASNodeErrorStatus.asneDCToIRecUnknownCSType);
      ContractExecutionStates.DynamicAddwithOffset("Unknown datum adjustment was encountered and ignored",
              (int)TASNodeErrorStatus.asneDCToIRecUnknownDatumModel);
      ContractExecutionStates.DynamicAddwithOffset("Unknown geoid model was encountered and ignored",
              (int)TASNodeErrorStatus.asneDCToIRecUnknownGeoidModel);
      ContractExecutionStates.DynamicAddwithOffset("Unknown projection type",
              (int)TASNodeErrorStatus.asneDCToIRecUnknownProjType);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported datum",
              (int)TASNodeErrorStatus.asneDCToIRecUnsupportedDatum);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported geoid",
              (int)TASNodeErrorStatus.asneDCToIRecUnsupportedGeoid);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported zone orientation",
              (int)TASNodeErrorStatus.asneDCToIRecUnsupportedZoneOrientation);
      ContractExecutionStates.DynamicAddwithOffset("Failed to request file from TCC",
              (int)TASNodeErrorStatus.asneFailedToRequestFileFromTCC);
      ContractExecutionStates.DynamicAddwithOffset("Failed to read linework boundary file",
              (int)TASNodeErrorStatus.asneFailedToReadLineworkBoundaryFile);
      ContractExecutionStates.DynamicAddwithOffset("No boundaries in linework file",
              (int)TASNodeErrorStatus.asneNoBoundariesInLineworkFile);
      ContractExecutionStates.DynamicAddwithOffset("Failed to perform coordinate conversion",
              (int)TASNodeErrorStatus.asneFailedToPerformCoordinateConversion);
      ContractExecutionStates.DynamicAddwithOffset("No production data found",
              (int)TASNodeErrorStatus.asneNoProductionDataFound);
      ContractExecutionStates.DynamicAddwithOffset("Invalid plan extents",
              (int)TASNodeErrorStatus.asneInvalidPlanExtents);
      ContractExecutionStates.DynamicAddwithOffset("No design provided",
              (int)TASNodeErrorStatus.asneNoDesignProvided);
      ContractExecutionStates.DynamicAddwithOffset("No data on production data export",
              (int)TASNodeErrorStatus.asneExportNoData);
      ContractExecutionStates.DynamicAddwithOffset("Production data export timeout",
              (int)TASNodeErrorStatus.asneExportTimeOut);
      ContractExecutionStates.DynamicAddwithOffset("Production data export cancelled",
              (int)TASNodeErrorStatus.asneExportCancelled);
      ContractExecutionStates.DynamicAddwithOffset("Production data export limit reached",
              (int)TASNodeErrorStatus.asneExportLimitReached);
      ContractExecutionStates.DynamicAddwithOffset("Invalid data range on production data export",
              (int)TASNodeErrorStatus.asneExportInvalidDateRange);
      ContractExecutionStates.DynamicAddwithOffset("No overlap on production data export ranges",
              (int)TASNodeErrorStatus.asneExportDateRangesNoOverlap);
      ContractExecutionStates.DynamicAddwithOffset("Invalid page size or number for patch request. Try reducing the area being requested.",
              (int)TASNodeErrorStatus.asneInvalidArgument);
    }

    public static void AddTagProcessorErrorMessages(ContractExecutionStatesEnum ContractExecutionStates)
    {
      ContractExecutionStates.DynamicAddwithOffset("OK", (int)TTAGProcServerProcessResult.tpsprOK);
      ContractExecutionStates.DynamicAddwithOffset("Unknown error", (int)TTAGProcServerProcessResult.tpsprUnknown);
      ContractExecutionStates.DynamicAddwithOffset("OnSubmissionBase. Connection Failure.", (int)TTAGProcServerProcessResult.tpsprOnSubmissionBaseConnectionFailure);
      ContractExecutionStates.DynamicAddwithOffset("OnSubmissionVerb. Connection Failure.", (int)TTAGProcServerProcessResult.tpsprOnSubmissionVerbConnectionFailure);
      ContractExecutionStates.DynamicAddwithOffset("OnSubmissionResult. ConnectionFailure.", (int)TTAGProcServerProcessResult.tpsprOnSubmissionResultConnectionFailure);
      ContractExecutionStates.DynamicAddwithOffset("The TAG file was found to be corrupted on its pre-processing scan.", (int)TTAGProcServerProcessResult.tpsprFileReaderCorruptedTAGFileData);
      ContractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Unknown Machine AssetID.", (int)TTAGProcServerProcessResult.tpsprOnChooseMachineUnknownMachine);
      ContractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Invalid TagFile on selecting machine AssetID.", (int)TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidTagFile);
      ContractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Machine Subscriptions Invalid.", (int)TTAGProcServerProcessResult.tpsprOnChooseMachineInvalidSubscriptions);
      ContractExecutionStates.DynamicAddwithOffset("OnChooseMachine. Unable To Determine Machine.", (int)TTAGProcServerProcessResult.tpsprOnChooseMachineUnableToDetermineMachine);
      ContractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. Unable To Determine DataModel.", (int)TTAGProcServerProcessResult.tpsprOnChooseDataModelUnableToDetermineDataModel);
      ContractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. Could Not Convert DataModel Boundary To Grid.", (int)TTAGProcServerProcessResult.tpsprOnChooseDataModelCouldNotConvertDataModelBoundaryToGrid);
      ContractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. No GridEpochs Found In TAGFile.", (int)TTAGProcServerProcessResult.tpsprOnChooseDataModelNoGridEpochsFoundInTAGFile);
      ContractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. Supplied DataModel Boundary Contains Insufficeint Vertices.", (int)TTAGProcServerProcessResult.tpsprOnChooseDataModelSuppliedDataModelBoundaryContainsInsufficeintVertices);
      ContractExecutionStates.DynamicAddwithOffset("OnChooseDataModel. First Epoch Blade Position Does Not Lie Within Project Boundary.", (int)TTAGProcServerProcessResult.tpsprOnChooseDataModelFirstEpochBladePositionDoesNotLieWithinProjectBoundary);

    }
  }
}
