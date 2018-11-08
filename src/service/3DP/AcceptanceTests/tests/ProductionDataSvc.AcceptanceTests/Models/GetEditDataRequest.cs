namespace ProductionDataSvc.AcceptanceTests.Models
{
  class GetEditDataRequest : RequestBase
  {
    public long projectId { get; set; }
    public long? assetId { get; set; }
  }
}
