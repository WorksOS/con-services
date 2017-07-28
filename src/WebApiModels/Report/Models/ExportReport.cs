using System;
using System.Net;
using ASNode.ExportProductionDataCSV.RPC;
using ASNode.UserPreferences;
using BoundingExtents;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Common.Exceptions;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.Common.Contracts;

namespace VSS.Productivity3D.WebApiModels.Report.Models
{

  public enum ExportTypes
  {
    kSurfaceExport = 1,
    kPassCountExport = 2,
    kVedaExport = 3
  }

  public enum CoordTypes
  {
    ptNORTHEAST=0,
    ptLATLONG=1
  }

  public enum OutputTypes
  {
    etPassCountLastPass,
    etPassCountAllPasses,
    etVedaFinalPass,
    etVedaAllPasses
  }

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
    public Filter filter { get; protected set; }


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

 //   [JsonProperty(PropertyName = "zipFile", Required = Required.Default)]
//    public bool zipFile { get; protected set; }

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
    public CoordTypes coordType { get; protected set; }

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
    /// Create instance of CreatePassCountsRequest
    /// </summary>
    public static ExportReport CreateExportReportRequest(long projectId, LiftBuildSettings liftBuildSettings,
        Filter filter, long filterID, Guid? callid, bool cellSizeRq, string callerID, CoordTypes coordtype,
        DateTime DateFromUTC, DateTime DateToUTC, bool ZipFile, double Tolerance, bool TimeStampRequired,
        bool RestrictSize, bool RawData, T3DBoundingWorldExtent PrjExtents, bool PrecheckOnly, OutputTypes OutpuType,
        TMachine[] MachineList, bool IncludeSrvSurface, string FileName, ExportTypes ExportType)
    {
      return new ExportReport
             {
                 projectId = projectId,
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
             };
    }

    /// <summary>
    /// Create example instance of PassCounts to display in Help documentation.
    /// </summary>
    public new static ExportReport HelpSample
    {
      get
      {
        return new ExportReport
        {
                   projectId = 34,
                   liftBuildSettings = LiftBuildSettings.HelpSample,
                   filter = Filter.HelpSample,
                   filterID = 0,
                   machineList = new TMachine[2] {new TMachine(), new TMachine()},
                   projectExtents = new T3DBoundingWorldExtent(),
                   restrictSize = true,
                   tolerance = 0.1,
                   includeSurveydSurface = true,
                   cellSizeRequired = true,
                   rawData = true,
                   filename = "GoToSomwhere.csv",
                   coordType = CoordTypes.ptNORTHEAST,
                   exportType = ExportTypes.kPassCountExport,
                   outputType = OutputTypes.etVedaAllPasses,
                   precheckonly = false,
                   timeStampRequired = true,
                   dateToUTC = DateTime.UtcNow,
                   dateFromUTC = DateTime.MinValue,
                   callId = new Guid(),
                   callerId = "Myself"
               };
      }
    }


    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();

      if (coordType != CoordTypes.ptNORTHEAST && coordType != CoordTypes.ptLATLONG)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid coordinates type for export report"));
      }

      if (outputType < OutputTypes.etPassCountLastPass || outputType > OutputTypes.etVedaAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for export report"));
      }

      if (exportType == ExportTypes.kPassCountExport && outputType != OutputTypes.etPassCountLastPass && outputType != OutputTypes.etPassCountAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for machine passes export report"));
      }

      if (exportType == ExportTypes.kVedaExport && outputType != OutputTypes.etVedaFinalPass && outputType != OutputTypes.etVedaAllPasses)
      {
        throw new ServiceException(HttpStatusCode.BadRequest,
          new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
            "Invalid output type for machine passes export report for VETA"));
      }

      if (machineList == null)
        {
          machineList = new TMachine[2];

          machineList[0] = new TMachine();
          machineList[0].AssetID = 1;
          machineList[0].MachineName = "Asset 1 Name";
          machineList[0].SerialNo = "Asset 1 SN";

          machineList[1] = new TMachine();
          machineList[1].AssetID = 3517551388324974;
          machineList[1].MachineName = "Asset 3517551388324974 Name";
          machineList[1].SerialNo = "Asset 3517551388324974 SN";
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
 
      userPrefs = 
             ASNode.UserPreferences.__Global.Construct_TASNodeUserPreferences("NZ", "/", ":", ",", ".", 0.0, 0, 1, 0, 0, 1, 3); 


    }


  }
}