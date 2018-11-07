namespace ProductionDataSvc.AcceptanceTests.Models
{
  /// <summary>
  /// The request to identify the cell, display information and other configuration information to determine a datum value for the cell.
  /// One of llPoint or gridPoint must be defined.
  /// </summary>
  public class CellDatumRequest : RequestBase
  {
    /// <summary>
    /// Project ID. Required.
    /// </summary>
    public long? projectId;

    /// <summary>
    /// The datum type to return (eg: height, CMV, Temperature etc). 
    /// Required.
    /// </summary>
    public DisplayMode displayMode;

    /// <summary>
    /// If defined, the WGS84 LL position to identify the cell from. 
    /// May be null.
    /// </summary>
    public WGSPoint llPoint;

    /// <summary>
    /// If defined, the grid point in the project coordinate system to identify the cell from.
    /// May be null.
    /// </summary>
    public Point gridPoint;

    /// <summary>
    /// The filter to be used to govern selection of the cell/cell pass. 
    /// May be null.
    /// </summary>
    public FilterResult filter;

    /// <summary>
    /// The ID of the filter to be used.
    /// May be null.
    /// </summary>
    public long filterId;

    /// <summary>
    /// The lift/layer build settings to be used.
    /// May be null.
    /// </summary>
    public LiftBuildSettings liftBuildSettings;

    /// <summary>
    /// The descriptor identifyig the surface design to be used.
    /// May be null.
    /// </summary>
    public DesignDescriptor design;

    public CellDatumRequest()
    { }

    public CellDatumRequest(long? projectId, DisplayMode displayMode, Point gridPoint,
        FilterResult filter = null, long filterId = -1, LiftBuildSettings liftBuildSettings = null, DesignDescriptor design = null)
    {
      this.projectId = projectId;
      this.displayMode = displayMode;
      this.gridPoint = gridPoint;
      llPoint = null;
      this.filter = filter;
      this.filterId = filterId;
      this.liftBuildSettings = liftBuildSettings;
      this.design = design;
    }
  }
}
