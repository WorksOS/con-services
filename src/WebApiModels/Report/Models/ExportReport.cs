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
    public Guid? CallId { get; protected set; }

    [JsonProperty(PropertyName = "exportType", Required = Required.Default)]
    public ExportTypes ExportType { get; protected set; }

    /// <summary>
    /// Sets the custom caller identifier.
    /// </summary>
    [JsonProperty(PropertyName = "callerId", Required = Required.Default)]
    public string CallerId { get; protected set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    [JsonProperty(PropertyName = "filter", Required = Required.Default)]
    public FilterResult Filter { get; protected set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// May be null.
    /// </summary>
    [JsonProperty(PropertyName = "filterID", Required = Required.Default)]
    public long FilterID { get; protected set; }

    /// <summary>
    /// A collection of parameters and configuration information relating to analysis and determination of material layers.
    /// </summary>
    [JsonProperty(PropertyName = "liftBuildSettings", Required = Required.Default)]
    public LiftBuildSettings LiftBuildSettings { get; protected set; }

    [JsonProperty(PropertyName = "timeStampRequired", Required = Required.Default)]
    public bool TimeStampRequired { get; protected set; }

    [JsonProperty(PropertyName = "cellSizeRequired", Required = Required.Default)]
    public bool CellSizeRequired { get; protected set; }

    [JsonProperty(PropertyName = "rawData", Required = Required.Default)]
    public bool RawData { get; protected set; }

    [JsonProperty(PropertyName = "restrictSize", Required = Required.Default)]
    public bool RestrictSize { get; protected set; }

    [JsonProperty(PropertyName = "tolerance", Required = Required.Default)]
    public double Tolerance { get; protected set; }

    [JsonProperty(PropertyName = "includeSurveydSurface", Required = Required.Default)]
    public bool IncludeSurveydSurface { get; protected set; }

    [JsonProperty(PropertyName = "precheckonly", Required = Required.Default)]
    public bool Precheckonly { get; protected set; }

    [JsonProperty(PropertyName = "filename", Required = Required.Default)]
    public string Filename { get; protected set; }

    [JsonProperty(PropertyName = "machineList", Required = Required.Default)]
    public TMachine[] MachineList { get; protected set; }

    [JsonProperty(PropertyName = "coordType", Required = Required.Default)]
    public CoordType CoordType { get; protected set; }

    [JsonProperty(PropertyName = "outputType", Required = Required.Default)]
    public OutputTypes OutputType { get; protected set; }

    [JsonProperty(PropertyName = "dateFromUTC", Required = Required.Default)]
    public DateTime DateFromUTC { get; protected set; }

    [JsonProperty(PropertyName = "dateToUTC", Required = Required.Default)]
    public DateTime DateToUTC { get; protected set; }

    [JsonProperty(PropertyName = "projectExtents", Required = Required.Default)]
    public T3DBoundingWorldExtent ProjectExtents { get; protected set; }

    public TTranslation[] Translations { get; private set; }

    public TASNodeUserPreferences UserPrefs { get; private set; }

    /// <summary>
    /// Default protected constructor.
    /// </summary>
    protected ExportReport()
    { }

    /// <summary>
    /// Overload constructor with parameters.
    /// </summary>>
    /// <param name="projectId"></param>
    /// <param name="projectUid"></param>
    /// <param name="liftBuildSettings"></param>
    /// <param name="filter"></param>
    /// <param name="filterID"></param>
    /// <param name="callid"></param>
    /// <param name="cellSizeRq"></param>
    /// <param name="callerID"></param>
    /// <param name="coordtype"></param>
    /// <param name="dateFromUTC"></param>
    /// <param name="dateToUTC"></param>
    /// <param name="tolerance"></param>
    /// <param name="timeStampRequired"></param>
    /// <param name="restrictSize"></param>
    /// <param name="rawData"></param>
    /// <param name="prjExtents"></param>
    /// <param name="precheckOnly"></param>
    /// <param name="outpuType"></param>
    /// <param name="machineList"></param>
    /// <param name="includeSrvSurface"></param>
    /// <param name="fileName"></param>
    /// <param name="exportType"></param>
    /// <param name="userPrefs"></param>
    public ExportReport(long projectId, Guid? projectUid, LiftBuildSettings liftBuildSettings, FilterResult filter, long filterID, Guid? callid, bool cellSizeRq, string callerID, CoordType coordtype,
        DateTime dateFromUTC, DateTime dateToUTC, double tolerance, bool timeStampRequired, bool restrictSize, bool rawData, T3DBoundingWorldExtent prjExtents, bool precheckOnly, OutputTypes outpuType,
        TMachine[] machineList, bool includeSrvSurface, string fileName, ExportTypes exportType, TASNodeUserPreferences userPrefs)
    {
      ProjectId = projectId;
      ProjectUid = projectUid;
      LiftBuildSettings = liftBuildSettings;
      Filter = filter;
      FilterID = filterID;
      CallId = callid;
      CellSizeRequired = cellSizeRq;
      CallerId = callerID;
      CoordType = coordtype;
      DateFromUTC = dateFromUTC;
      DateToUTC = dateToUTC;
      ExportType = exportType;
      Filename = fileName;
      IncludeSurveydSurface = includeSrvSurface;
      MachineList = machineList;
      OutputType = outpuType;
      Precheckonly = precheckOnly;
      ProjectExtents = prjExtents;
      RawData = rawData;
      RestrictSize = restrictSize;
      TimeStampRequired = timeStampRequired;
      Tolerance = tolerance;
      UserPrefs = userPrefs;
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (CoordType != CoordType.Northeast && CoordType != CoordType.LatLon)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid coordinates type for export report"));
      }

      if (OutputType < OutputTypes.PassCountLastPass || OutputType > OutputTypes.VedaAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for export report"));
      }

      if (ExportType == ExportTypes.PassCountExport && OutputType != OutputTypes.PassCountLastPass &&
          OutputType != OutputTypes.PassCountAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for machine passes export report"));
      }

      if (ExportType == ExportTypes.VedaExport && OutputType != OutputTypes.VedaFinalPass &&
          OutputType != OutputTypes.VedaAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for machine passes export report for VETA"));
      }

      if (MachineList == null)
      {
        MachineList = new TMachine[2];

        MachineList[0] = new TMachine
        {
          AssetID = 1,
          MachineName = "Asset 1 Name",
          SerialNo = "Asset 1 SN"
        };

        MachineList[1] = new TMachine
        {
          AssetID = 3517551388324974,
          MachineName = "Asset 3517551388324974 Name",
          SerialNo = "Asset 3517551388324974 SN"
        };
      }

      Translations = new TTranslation[6];
      Translations[0].ID = 0;
      Translations[0].Translation = "Problem occured processing export.";
      Translations[1].ID = 1;
      Translations[1].Translation = "No data found";
      Translations[2].ID = 2;
      Translations[2].Translation = "Timed out";
      Translations[3].ID = 3;
      Translations[3].Translation = "Unexpected error";
      Translations[4].ID = 4;
      Translations[4].Translation = "Request Canceled";
      Translations[5].ID = 5;
      Translations[5].Translation = "Maxmium records reached";

      if (UserPrefs.Equals(Preferences.EmptyUserPreferences()))
      {
        UserPrefs = ASNode.UserPreferences.__Global.Construct_TASNodeUserPreferences(
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

      if (string.IsNullOrEmpty(Filename))
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Missing export file name"));
      }
    }
  }
}
