using System;

namespace VSS.VisionLink.Landfill.Common.Models
{
  public class Subscription
  {
    public DateTime lastActionedUtc { get; set; }
    public string subscriptionUid { get; set; }
    public string customerUid { get; set; }
    public DateTime startDate { get; set; }
    public DateTime endDate { get; set; }


    public override bool Equals(object obj)
    {
      var otherProject = obj as Subscription;
      if (otherProject == null) return false;
      return otherProject.subscriptionUid == subscriptionUid && otherProject.customerUid == customerUid &&
             otherProject.startDate == startDate && otherProject.endDate==endDate;
    }
    public override int GetHashCode() { return 0; }
  }
}