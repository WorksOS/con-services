namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CellPassesRequest : RequestBase
  {
    /// <summary>
    /// Project id
    /// </summary>
    public long? projectId { get; set; }

    /// <summary>
    /// Location of the cell in the form of cartesian cell index address. 
    /// May be null.
    /// </summary>       
    public CellAddress cellAddress { get; set; }

    /// <summary>
    /// Location of the cell in the form of a grid position within it. 
    /// May be null.
    /// </summary>   
    public Point probePositionGrid { get; set; }

    /// <summary>
    /// Location of the cell in the form of a WGS84 position within it. 
    /// May be null.
    /// </summary>       
    public WGSPoint probePositionLL { get; set; }

    /// <summary>
    /// The lift/layer build settings to be used.
    /// May be null.
    /// </summary>
    public LiftBuildSettings liftBuildSettings { get; set; }

    /// <summary>
    /// The type of data being requested for the processed passes and layers to represent.
    /// Defined types are as follows:
    ///  icdtAll = $00000000;
    ///  icdtCCV = $00000001;
    ///  icdtHeight = $00000002;
    ///  icdtLatency = $00000003;
    ///  icdtPassCount = $00000004;
    ///  icdtFrequency = $00000005;
    ///  icdtAmplitude = $00000006;
    ///  icdtMoisture = $00000007;
    ///  icdtTemperature = $00000008;
    ///  icdtRMV = $00000009;
    ///  icdtCCVPercent = $0000000B;
    ///  icdtGPSMode = $0000000A;
    ///  icdtSimpleVolumeOverlay = $0000000C;
    ///  icdtHeightAndTime = $0000000D;
    ///  icdtCompositeHeights = $0000000E;
    ///  icdtMDP = $0000000F;
    ///  icdtMDPPercent = $00000010;
    ///  icdtCellProfile = $00000011;
    ///  icdtCellPasses = $00000012;
    /// </summary>
    public int gridDataType { get; set; }

    /// <summary>
    /// The ID of the filter to be used. 
    /// May be null.
    /// </summary>
    public long? filterId { get; set; }

    /// <summary>
    /// The lift/layer build settings to be used.
    /// May be null.
    /// </summary>
    public FilterResult filter { get; set; }
  }
}
