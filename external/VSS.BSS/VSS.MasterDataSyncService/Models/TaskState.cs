using System;

namespace VSS.Nighthawk.MasterDataSync.Models
{
  public class TaskState
  {
    public string TaskName { get; set; }
    public long? lastProcessedId { get; set; }
    public DateTime? InsertUtc { get; set; }
  }
}