using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CCARequest : RequestBase
  {
    public long projectID { get; set; }
    public Guid? callId { get; set; }
    public LiftBuildSettings liftBuildSettings { get; set; }
    public FilterResult filter { get; set; }
    public long filterID { get; set; }
  }
}
