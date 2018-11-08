namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class ProjectSettingsRequest : RequestBase
  {
    public string projectUid { get; set; }
    public string Settings { get; set; }
    public int ProjectSettingsType { get; set; }
  }
}
