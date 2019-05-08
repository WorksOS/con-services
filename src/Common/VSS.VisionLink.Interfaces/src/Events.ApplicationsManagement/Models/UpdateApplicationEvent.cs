using System;

namespace VSS.VisionLink.Interfaces.Events.ApplicationsManagement.Models
{
  public class UpdateApplicationEvent
  {
      public string appUID { get; set; }
      public bool liveStatusInd { get; set; }
      public DateTime eventUtc { get; set; }
  }
}
