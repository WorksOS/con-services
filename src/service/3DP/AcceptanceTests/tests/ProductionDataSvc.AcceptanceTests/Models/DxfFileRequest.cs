namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class DxfFileRequest : RequestBase
  {
    public byte[] FileData { get; set; }
    public int DxfUnits { get; set; }
    public string CoordinateSystemName { get; set; }
  }
}
