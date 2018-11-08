using System;

namespace ProductionDataSvc.AcceptanceTests.Models
{
  public class CompactionTagFilePostParameter : RequestBase
  {
    public string fileName { get; set; }
    public byte[] data { get; set; }
    public Guid? projectUid { get; set; }
    public long? orgId { get; set; }
  }
}
