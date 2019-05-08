using System;

namespace VSS.VisionLink.Interfaces.Events.ApplicationsManagement.Models
{
  public class CreateApplicationEvent
  {
      public string appName { get; set; }
      public string appUID { get; set; }
      public string appDescription { get; set; }
      public string appURL { get; set; }
      public string appMarketURL { get; set; }
      public string appICONURL { get; set; }
      public int displayOrder { get; set; }
      public string tpaasAppName { get; set; }
      public long tpaasAppID { get; set; }
      public bool liveStatusInd { get; set; }
      public int appOwnerID { get; set; }
      public string createdBy { get; set; }
      public int[] serviceTypeIds { get; set; }
      public DateTime eventUtc { get; set; }

  }
}
