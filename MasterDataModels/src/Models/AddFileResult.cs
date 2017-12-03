namespace VSS.MasterData.Models.Models
{
  public class AddFileResult : BaseDataResult
  {
    /// <summary>
    /// The minimum zoom level that DXF tiles have been generated for.
    /// </summary>
    public int MinZoomLevel;
    /// <summary>
    /// The maximum zoom level that DXF tiles have been generated for.
    /// </summary>
    public int MaxZoomLevel;
  }
}
