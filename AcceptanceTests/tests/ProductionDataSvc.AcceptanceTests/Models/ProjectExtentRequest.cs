namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class ProjectExtentRequest : RequestBase
  {
    public long projectId { get; set; }

    public long[] excludedSurveyedSurfaceIds { get; set; }

    public ProjectExtentRequest()
    {
      projectId = -1;
      excludedSurveyedSurfaceIds = new long[] { };
    }
  }
}
