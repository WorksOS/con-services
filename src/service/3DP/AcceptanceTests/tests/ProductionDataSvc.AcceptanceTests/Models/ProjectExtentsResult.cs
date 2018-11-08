namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class ProjectExtentsResult : ResponseBase
  {
    public BoundingBox3DGrid ProjectExtents { get; set; }

    public ProjectExtentsResult()
      : base("success")
    { }

    public ProjectExtentsResult(BoundingBox3DGrid extents, int code, string message = "")
      : base(code, message)
    {
      ProjectExtents = extents;
    }
  }
}
