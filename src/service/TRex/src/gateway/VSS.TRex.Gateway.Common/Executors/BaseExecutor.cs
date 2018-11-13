using System;
using System.Linq;
using System.Net;
using Microsoft.Extensions.Logging;
using VSS.Common.Exceptions;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Filters.Interfaces;
using VSS.TRex.Gateway.Common.Converters;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;

namespace VSS.TRex.Gateway.Common.Executors
{
  public abstract class BaseExecutor : RequestExecutorContainer
  {
    private const string ERROR_MESSAGE = "Failed to get/update data requested by {0}";
    private const string ERROR_MESSAGE_EX = "{0} with error: {1}";

    protected BaseExecutor()
    {
    }

    protected BaseExecutor(IConfigurationStore configurationStore, ILoggerFactory logger, IServiceExceptionHandler exceptionHandler) 
      : base(configurationStore, logger, exceptionHandler)
    {
    }

    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      throw new NotImplementedException();
    }

    protected ISiteModel GetSiteModel(Guid? ID)
    {
      ISiteModel siteModel = ID.HasValue ? DIContext.Obtain<ISiteModels>().GetSiteModel(ID.Value) : null;

      if (siteModel == null)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
            $"Site model {ID} is unavailable"));
      }

      return siteModel;
    }

    protected ICombinedFilter ConvertFilter(FilterResult filter, ISiteModel siteModel)
    {
      if (filter == null)
        return new CombinedFilter();//TRex doesn't like null filter

      var combinedFilter = AutoMapperUtility.Automapper.Map<FilterResult, CombinedFilter>(filter);
      // TODO Map the excluded surveyed surfaces from the filter.SurveyedSurfaceExclusionList to the ones that are in the TRex database
      bool includeSurveyedSurfaces = filter.SurveyedSurfaceExclusionList == null || filter.SurveyedSurfaceExclusionList.Count == 0;
      var excludedIds = siteModel.SurveyedSurfaces == null || includeSurveyedSurfaces ? new Guid[0] : siteModel.SurveyedSurfaces.Select(x => x.ID).ToArray();
      combinedFilter.AttributeFilter.SurveyedSurfaceExclusionList = excludedIds;
      return combinedFilter;
    }

    protected void ThrowRequestTypeCastException<T>()
    {
      throw new ServiceException(
        HttpStatusCode.InternalServerError,
        new ContractExecutionResult(ContractExecutionStatesEnum.InternalProcessingError,
          $"{typeof(T).Name} cast failed."));
    }

    protected sealed override void ProcessErrorCodes()
    {
      ContractExecutionStates.DynamicAddwithOffset("OK", (int)RequestErrorStatus.OK);
      ContractExecutionStates.DynamicAddwithOffset("Unknown error", (int)RequestErrorStatus.Unknown);
      ContractExecutionStates.DynamicAddwithOffset("Exception occurred", (int)RequestErrorStatus.Exception);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported coordinate system definition file type",
        (int)RequestErrorStatus.UnsupportedCSDFileType);
      ContractExecutionStates.DynamicAddwithOffset("Could not convert coordinate system definition file",
        (int)RequestErrorStatus.CouldNotConvertCSDFile);
      ContractExecutionStates.DynamicAddwithOffset("CToolBox failed to complete",
        (int)RequestErrorStatus.CToolBoxFailedToComplete);
      ContractExecutionStates.DynamicAddwithOffset("Failed to write coordinate system definition stream",
        (int)RequestErrorStatus.FailedToWriteCSDStream);
      ContractExecutionStates.DynamicAddwithOffset("Failed on profile request",
        (int)RequestErrorStatus.FailedOnRequestProfile);
      ContractExecutionStates.DynamicAddwithOffset("No such data model",
        (int)RequestErrorStatus.NoSuchDataModel);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported display type",
        (int)RequestErrorStatus.UnsupportedDisplayType);
      ContractExecutionStates.DynamicAddwithOffset("Failed on request of colour graduated profile",
        (int)RequestErrorStatus.FailedOnRequestColourGraduatedProfile);
      ContractExecutionStates.DynamicAddwithOffset("Failed to convert client WGS84 coordinates",
        (int)RequestErrorStatus.FailedToConvertClientWGSCoords);
      ContractExecutionStates.DynamicAddwithOffset("Failed to request sub-grid existence map",
        (int)RequestErrorStatus.FailedToRequestSubgridExistenceMap);
      ContractExecutionStates.DynamicAddwithOffset("Invalid coordinate range",
        (int)RequestErrorStatus.InvalidCoordinateRange);
      ContractExecutionStates.DynamicAddwithOffset("Failed to request data model statistics",
        (int)RequestErrorStatus.FailedToRequestDatamodelStatistics);
      ContractExecutionStates.DynamicAddwithOffset("Failed to request coordinate system projection file",
        (int)RequestErrorStatus.FailedOnRequestCoordinateSystemProjectionFile);
      ContractExecutionStates.DynamicAddwithOffset("Coordinate system is empty",
        (int)RequestErrorStatus.EmptyCoordinateSystem);
      ContractExecutionStates.DynamicAddwithOffset("Request has been aborted due to pipeline timeout",
        (int)RequestErrorStatus.AbortedDueToPipelineTimeout);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported filter attribute",
        (int)RequestErrorStatus.UnsupportedFilterAttribute);
      ContractExecutionStates.DynamicAddwithOffset("Service stopped", (int)RequestErrorStatus.ServiceStopped);
      ContractExecutionStates.DynamicAddwithOffset("Schedule load is too high",
        (int)RequestErrorStatus.RequestScheduleLoadTooHigh);
      ContractExecutionStates.DynamicAddwithOffset("Schedule failure",
        (int)RequestErrorStatus.RequestScheduleFailure);
      ContractExecutionStates.DynamicAddwithOffset("Schedule timeout",
        (int)RequestErrorStatus.RequestScheduleTimeout);
      ContractExecutionStates.DynamicAddwithOffset("Request has been cancelled",
        (int)RequestErrorStatus.RequestHasBeenCancelled);
      ContractExecutionStates.DynamicAddwithOffset("Failed to obtain coordinate system interlock",
        (int)RequestErrorStatus.FailedToObtainCoordinateSystemInterlock);
      ContractExecutionStates.DynamicAddwithOffset(
        "Failed to request coordinate system horizontal adjustment file",
        (int)RequestErrorStatus.FailedOnRequestCoordinateSystemHorizontalAdjustmentFile);
      ContractExecutionStates.DynamicAddwithOffset("No connection to server",
        (int)RequestErrorStatus.NoConnectionToServer);
      ContractExecutionStates.DynamicAddwithOffset("Invalid response code",
        (int)RequestErrorStatus.InvalidResponseCode);
      ContractExecutionStates.DynamicAddwithOffset("No result has been returned",
        (int)RequestErrorStatus.NoResultReturned);
      ContractExecutionStates.DynamicAddwithOffset("Failed to notify that coordinate system was changed",
        (int)RequestErrorStatus.FailedToNotifyCSChange);
      ContractExecutionStates.DynamicAddwithOffset("Failed to create DCtoIRecord converter",
        (int)RequestErrorStatus.FailedToCreateDCToIRecordConverter);
      ContractExecutionStates.DynamicAddwithOffset("Failed to get coordinate systems settings",
        (int)RequestErrorStatus.FailedToGetCSSettings);
      ContractExecutionStates.DynamicAddwithOffset("Coordinate system is incomplete",
        (int)RequestErrorStatus.DCToIRecIncompleteCS);
      ContractExecutionStates.DynamicAddwithOffset("Failed to create CSIB",
        (int)RequestErrorStatus.DCToIRecFailedCreateCSIB);
      ContractExecutionStates.DynamicAddwithOffset("Failed to get geoid information",
        (int)RequestErrorStatus.DCToIRecFailedToGetGeoidInfo);
      ContractExecutionStates.DynamicAddwithOffset("Unable to retrieve zone parameters",
        (int)RequestErrorStatus.DCToIRecFailedToGetZoneParams);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB constant separation geoid",
        (int)RequestErrorStatus.DCToIRecFailedToCreateConstGeoid);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB datum grid file",
        (int)RequestErrorStatus.DCToIRecFailedToCreateDatumGrid);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB ellipsoid",
        (int)RequestErrorStatus.DCToIRecFailedToCreateEllipsoid);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Grid Geoid",
        (int)RequestErrorStatus.DCToIRecFailedToCreateGridGeoid);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Molodensky datum",
        (int)RequestErrorStatus.DCToIRecFailedToCreateMolodenskyDatum);
      ContractExecutionStates.DynamicAddwithOffset(
        "Failed to instantiate CSIB Multiple Regression Parameter datum",
        (int)RequestErrorStatus.DCToIRecFailedToCreateMultiRegressionDatum);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Seven Parameter datum",
        (int)RequestErrorStatus.DCToIRecFailedToCreateSevenParamsDatum);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB WGS84 datum",
        (int)RequestErrorStatus.DCToIRecFailedToCreateWGS84Datum);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Zone Group",
        (int)RequestErrorStatus.DCToIRecFailedToCreateZoneGroup);
      ContractExecutionStates.DynamicAddwithOffset("Failed to instantiate CSIB Zone Based Site",
        (int)RequestErrorStatus.DCToIRecFailedToCreateZoneBasedSite);
      ContractExecutionStates.DynamicAddwithOffset("Failed to create an IAZIParameters object",
        (int)RequestErrorStatus.DCToIRecFailedToCreateAZIParamsObject);
      ContractExecutionStates.DynamicAddwithOffset("Unable to create an ICSIB object",
        (int)RequestErrorStatus.DCToIRecFailedToCreateCSIBObject);
      ContractExecutionStates.DynamicAddwithOffset("Failed to open Calibration reader",
        (int)RequestErrorStatus.DCToIRecFailedToOpenCalibrationReader);
      ContractExecutionStates.DynamicAddwithOffset("Unable to set zone parameters",
        (int)RequestErrorStatus.DCToIRecFailedToSetZoneParams);
      ContractExecutionStates.DynamicAddwithOffset("Failed to read CSIB",
        (int)RequestErrorStatus.DCToIRecFailedToReadCSIB);
      ContractExecutionStates.DynamicAddwithOffset("Failed to read in CSIB",
        (int)RequestErrorStatus.DCToIRecFailedToReadInCSIB);
      ContractExecutionStates.DynamicAddwithOffset("Failed to read the ZoneBased site",
        (int)RequestErrorStatus.DCToIRecFailedToReadZoneBasedSite);
      ContractExecutionStates.DynamicAddwithOffset("Failed to read the zone",
        (int)RequestErrorStatus.DCToIRecFailedToReadZone);
      ContractExecutionStates.DynamicAddwithOffset("Failed to write datum",
        (int)RequestErrorStatus.DCToIRecFailedToWriteDatum);
      ContractExecutionStates.DynamicAddwithOffset("Failed to write geoid",
        (int)RequestErrorStatus.DCToIRecFailedToWriteGeoid);
      ContractExecutionStates.DynamicAddwithOffset("Failed to write CSIB",
        (int)RequestErrorStatus.DCToIRecFailedToWriteCSIB);
      ContractExecutionStates.DynamicAddwithOffset("Failed to set zone info",
        (int)RequestErrorStatus.DCToIRecFailedToSetZoneInfo);
      ContractExecutionStates.DynamicAddwithOffset("Infinite adjustment slope value",
        (int)RequestErrorStatus.DCToIRecInfiniteAdjustmentSlopeValue);
      ContractExecutionStates.DynamicAddwithOffset("Invalid ellipsoid",
        (int)RequestErrorStatus.DCToIRecInvalidEllipsoid);
      ContractExecutionStates.DynamicAddwithOffset("The datum CSIB failed to load",
        (int)RequestErrorStatus.DCToIRecDatumFailedToLoad);
      ContractExecutionStates.DynamicAddwithOffset("Failed to load CSIB",
        (int)RequestErrorStatus.DCToIRecFailedToLoadCSIB);
      ContractExecutionStates.DynamicAddwithOffset("Not WGS84 ellipsoid",
        (int)RequestErrorStatus.DCToIRecNotWGS84Ellipsoid);
      ContractExecutionStates.DynamicAddwithOffset("Not WGS84 ellipsoid in datum record",
        (int)RequestErrorStatus.DCToIRecNotWGS84EllipsoidSameAsProj);
      ContractExecutionStates.DynamicAddwithOffset("Current projection should be scaled",
        (int)RequestErrorStatus.DCToIRecScaleOnlyProj);
      ContractExecutionStates.DynamicAddwithOffset("Unknown coordinate system type",
        (int)RequestErrorStatus.DCToIRecUnknownCSType);
      ContractExecutionStates.DynamicAddwithOffset("Unknown datum adjustment was encountered and ignored",
        (int)RequestErrorStatus.DCToIRecUnknownDatumModel);
      ContractExecutionStates.DynamicAddwithOffset("Unknown geoid model was encountered and ignored",
        (int)RequestErrorStatus.DCToIRecUnknownGeoidModel);
      ContractExecutionStates.DynamicAddwithOffset("Unknown projection type",
        (int)RequestErrorStatus.DCToIRecUnknownProjType);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported datum",
        (int)RequestErrorStatus.DCToIRecUnsupportedDatum);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported geoid",
        (int)RequestErrorStatus.DCToIRecUnsupportedGeoid);
      ContractExecutionStates.DynamicAddwithOffset("Unsupported zone orientation",
        (int)RequestErrorStatus.DCToIRecUnsupportedZoneOrientation);
      ContractExecutionStates.DynamicAddwithOffset("Failed to request file from TCC",
        (int)RequestErrorStatus.FailedToRequestFileFromTCC);
      ContractExecutionStates.DynamicAddwithOffset("Failed to read linework boundary file",
        (int)RequestErrorStatus.FailedToReadLineworkBoundaryFile);
      ContractExecutionStates.DynamicAddwithOffset("No boundaries in linework file",
        (int)RequestErrorStatus.NoBoundariesInLineworkFile);
      ContractExecutionStates.DynamicAddwithOffset("Failed to perform coordinate conversion",
        (int)RequestErrorStatus.FailedToPerformCoordinateConversion);
      ContractExecutionStates.DynamicAddwithOffset("No production data found",
        (int)RequestErrorStatus.NoProductionDataFound);
      ContractExecutionStates.DynamicAddwithOffset("Invalid plan extents",
        (int)RequestErrorStatus.InvalidPlanExtents);
      ContractExecutionStates.DynamicAddwithOffset("No design provided",
        (int)RequestErrorStatus.NoDesignProvided);
      ContractExecutionStates.DynamicAddwithOffset("No data on production data export",
        (int)RequestErrorStatus.ExportNoData);
      ContractExecutionStates.DynamicAddwithOffset("Production data export timeout",
        (int)RequestErrorStatus.ExportTimeOut);
      ContractExecutionStates.DynamicAddwithOffset("Production data export cancelled",
        (int)RequestErrorStatus.ExportCancelled);
      ContractExecutionStates.DynamicAddwithOffset("Production data export limit reached",
        (int)RequestErrorStatus.ExportLimitReached);
      ContractExecutionStates.DynamicAddwithOffset("Invalid data range on production data export",
        (int)RequestErrorStatus.ExportInvalidDateRange);
      ContractExecutionStates.DynamicAddwithOffset("No overlap on production data export ranges",
        (int)RequestErrorStatus.ExportDateRangesNoOverlap);
      ContractExecutionStates.DynamicAddwithOffset(
        "Invalid page size or number for patch request. Try reducing the area being requested.",
        (int)RequestErrorStatus.InvalidArgument);
      ContractExecutionStates.DynamicAddwithOffset("Failed to create coordinate transformer.",
        (int)RequestErrorStatus.FailedToConfigureInternalPipeline);
      ContractExecutionStates.DynamicAddwithOffset("Failed to retrieve design file from storage.",
        (int)RequestErrorStatus.DesignImportUnableToRetrieveFromS3);
    }

    protected ServiceException CreateServiceException<T>(RequestErrorStatus resultStatus = RequestErrorStatus.OK)
    {
      var errorMessage = string.Format(ERROR_MESSAGE, typeof(T).Name);

      if (resultStatus != RequestErrorStatus.OK)
        errorMessage = string.Format(ERROR_MESSAGE_EX, errorMessage, ContractExecutionStates.FirstNameWithOffset((int) resultStatus));

      return new ServiceException(HttpStatusCode.BadRequest, new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults, errorMessage));
    }
  }
}
