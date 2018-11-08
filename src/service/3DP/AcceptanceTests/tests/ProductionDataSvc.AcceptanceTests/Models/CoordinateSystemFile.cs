namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CoordinateSystemFile : RequestBase
  {
    public long? projectId { get; set; }
    public byte[] csFileContent { get; set; }
    public string csFileName { get; set; }
  }
}
