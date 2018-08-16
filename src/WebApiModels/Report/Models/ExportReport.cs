using System;
using System.Net;
using ASNode.ExportProductionDataCSV.RPC;
using ASNode.UserPreferences;
using BoundingExtents;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.WebApi.Models.Report.Models
{
  /// <summary>
  /// The representation of a pass counts request
  /// </summary>
  public class ExportReport : ProjectID, IValidatable
  {
    /// <summary>
    /// An identifier from the caller. 
    /// </summary>
    [JsonProperty(PropertyName = "callId", Required = Required.Default)]
    public Guid? callId { get; protected set; }

    [JsonProperty(PropertyName = "exportType", Required = Required.Default)]
    public ExportTypes exportType { get; protected set; }

    /// <summary>
    /// Sets the custom caller identifier.
    /// </summary>
    [JsonProperty(PropertyName = "callerId", Required = Required.Default)]
    public string callerId { get; protected set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult filter { get; protected set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterID", Required = Required.Default)]
    public long filterID { get; protected set; }

    /// <summary>
    /// A collection of parameters and configuration information relating to analysis and determination of material layers.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings liftBuildSettings { get; protected set; }

    [JsonProperty(PropertyName = "timeStampRequired", Required = Required.Default)]
    public bool timeStampRequired { get; protected set; }

    [JsonProperty(PropertyName = "cellSizeRequired", Required = Required.Default)]
    public bool cellSizeRequired { get; protected set; }

    [JsonProperty(PropertyName = "rawData", Required = Required.Default)]
    public bool rawData { get; protected set; }

    [JsonProperty(PropertyName = "restrictSize", Required = Required.Default)]
    public bool restrictSize { get; protected set; }

    [JsonProperty(PropertyName = "tolerance", Required = Required.Default)]
    public double tolerance { get; protected set; }

    [JsonProperty(PropertyName = "includeSurveydSurface", Required = Required.Default)]
    public bool includeSurveydSurface { get; protected set; }

    [JsonProperty(PropertyName = "precheckonly", Required = Required.Default)]
    public bool precheckonly { get; protected set; }

    [JsonProperty(PropertyName = "filename", Required = Required.Default)]
    public string filename { get; protected set; }

    [JsonProperty(PropertyName = "machineList", Required = Required.Default)]
    public TMachine[] machineList { get; protected set; }

    [JsonProperty(PropertyName = "coordType", Required = Required.Default)]
    public CoordType coordType { get; protected set; }

    [JsonProperty(PropertyName = "outputType", Required = Required.Default)]
    public OutputTypes outputType { get; protected set; }

    [JsonProperty(PropertyName = "dateFromUTC", Required = Required.Default)]
    public DateTime dateFromUTC { get; protected set; }

    [JsonProperty(PropertyName = "dateToUTC", Required = Required.Default)]
    public DateTime dateToUTC { get; protected set; }

    [JsonProperty(PropertyName = "projectExtents", Required = Required.Default)]
    public T3DBoundingWorldExtent projectExtents { get; protected set; }

    public TTranslation[] translations { get; private set; }

    public TASNodeUserPreferences userPrefs { get; private set; }

    protected ExportReport()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public static ExportReport CreateExportReportRequest(long projectId, LiftBuildSettings liftBuildSettings, FilterResult filter, long filterID, Guid? callid, bool cellSizeRq, string callerID, CoordType coordtype,
        DateTime DateFromUTC, DateTime DateToUTC, bool ZipFile, double Tolerance, bool TimeStampRequired, bool RestrictSize, bool RawData, T3DBoundingWorldExtent PrjExtents, bool PrecheckOnly, OutputTypes OutpuType,
        TMachine[] MachineList, bool IncludeSrvSurface, string FileName, ExportTypes ExportType, TASNodeUserPreferences UserPrefs)
    {
      return new ExportReport
      {
        ProjectId = projectId,
        liftBuildSettings = liftBuildSettings,
        filter = filter,
        filterID = filterID,
        callId = callid,
        cellSizeRequired = cellSizeRq,
        callerId = callerID,
        coordType = coordtype,
        dateFromUTC = DateFromUTC,
        dateToUTC = DateToUTC,
        exportType = ExportType,
        filename = FileName,
        includeSurveydSurface = IncludeSrvSurface,
        machineList = MachineList,
        outputType = OutpuType,
        precheckonly = PrecheckOnly,
        projectExtents = PrjExtents,
        rawData = RawData,
        restrictSize = RestrictSize,
        timeStampRequired = TimeStampRequired,
        tolerance = Tolerance,
        userPrefs = UserPrefs
      };
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (coordType != CoordType.Northeast && coordType != CoordType.LatLon)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid coordinates type for export report"));
      }

      if (outputType < OutputTypes.PassCountLastPass || outputType > OutputTypes.VedaAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for export report"));
      }

      if (exportType == ExportTypes.PassCountExport && outputType != OutputTypes.PassCountLastPass &&
          outputType != OutputTypes.PassCountAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for machine passes export report"));
      }

      if (exportType == ExportTypes.VedaExport && outputType != OutputTypes.VedaFinalPass &&
          outputType != OutputTypes.VedaAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for machine passes export report for VETA"));
      }

      if (machineList == null)
      {
        machineList = new TMachine[2];

        machineList[0] = new TMachine
        {
          AssetID = 1,
          MachineName = "Asset 1 Name",
          SerialNo = "Asset 1 SN"
        };

        machineList[1] = new TMachine
        {
          AssetID = 3517551388324974,
          MachineName = "Asset 3517551388324974 Name",
          SerialNo = "Asset 3517551388324974 SN"
        };
      }

      translations = new TTranslation[6];
      translations[0].ID = 0;
      translations[0].Translation = "Problem occured processing export.";
      translations[1].ID = 1;
      translations[1].Translation = "No data found";
      translations[2].ID = 2;
      translations[2].Translation = "Timed out";
      translations[3].ID = 3;
      translations[3].Translation = "Unexpected error";
      translations[4].ID = 4;
      translations[4].Translation = "Request Canceled";
      translations[5].ID = 5;
      translations[5].Translation = "Maxmium records reached";

      if (userPrefs.Equals(Preferences.EmptyUserPreferences()))
      {
        userPrefs = ASNode.UserPreferences.__Global.Construct_TASNodeUserPreferences(
          "NZ",
          Preferences.DefaultDateSeparator,
          Preferences.DefaultTimeSeparator,
          Preferences.DefaultThousandsSeparator,
          Preferences.DefaultDecimalSeparator,
          0.0,
          (int)LanguageEnum.enUS,
          (int)UnitsTypeEnum.Metric,
          Preferences.DefaultDateTimeFormat,
          Preferences.DefaultNumberFormat,
          Preferences.DefaultTemperatureUnit,
          Preferences.DefaultAssetLabelTypeId);
      }

      if (string.IsNullOrEmpty(filename))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing export file name"));
      }
    }
  }
}
