namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class TileResult : ResponseBase
  {
    public byte[] TileData { get; set; }
    public bool TileOutsideProjectExtents { get; set; }

    public TileResult() :
        base("success")
    { }
  }
}
