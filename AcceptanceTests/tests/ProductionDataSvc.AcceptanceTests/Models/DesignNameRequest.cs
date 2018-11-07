namespace ProductionDataSvc.AcceptanceTests.Models
{
  class DesignNameRequest : RequestBase
  {
    public long ProjectId { get; set; }
    public string DesignFilename { get; set; }
  }
}
