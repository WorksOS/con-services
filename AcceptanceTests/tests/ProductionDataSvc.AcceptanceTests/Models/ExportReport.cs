using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The representation of a pass counts request
  /// </summary>
  public class ExportReportRequest : RequestBase
  {
    /// <summary>
    /// The project to process the CS definition file into.
    /// </summary>
    public long projectId { get; set; }

    /// <summary>
    /// An identifier from the caller. 
    /// </summary>
    public Guid? callId { get; set; }

    public ExportTypes exportType { get; set; }

    /// <summary>
    /// Sets the custom caller identifier.
    /// </summary>
    public string callerId { get; set; }

    /// <summary>
    /// The filter instance to use in the request
    /// Value may be null.
    /// </summary>
    public FilterResult filter { get; set; }

    /// <summary>
    /// The filter ID to used in the request.
    /// May be null.
    /// </summary>
    public long filterID { get; set; }

    /// <summary>
    /// A collection of parameters and configuration information relating to analysis and determination of material layers.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }

    public bool timeStampRequired { get; set; }

    public bool cellSizeRequired { get; set; }

    public bool rawData { get; set; }

    public bool restrictSize { get; set; }

    public double tolerance { get; set; }

    public bool includeSurveydSurface { get; set; }

    public bool precheckonly { get; set; }

    public string filename { get; set; }

    public TMachine[] machineList { get; set; }

    public CoordTypes coordType { get; set; }

    public OutputTypes outputType { get; set; }

    public DateTime dateFromUTC { get; set; }

    public DateTime dateToUTC { get; set; }

    //public T3DBoundingWorldExtent projectExtents { get; protected set; }

    //public TTranslation[] translations { get; private set; }

    //public TASNodeUserPreferences userPrefs { get; private set; } 
  }

  public enum ExportTypes
  {
    kSurfaceExport = 1,
    kPassCountExport = 2,
    kVedaExport = 3
  }
  public enum CoordTypes
  {
    ptNORTHEAST = 0,
    ptLATLONG = 1
  }
  public enum OutputTypes
  {
    etPassCountLastPass,
    etPassCountAllPasses,
    etVedaFinalPass,
    etVedaAllPasses
  }
  public struct TMachine
  {
    public long AssetID;
    public string MachineName;
    public string SerialNo;
  }
}
