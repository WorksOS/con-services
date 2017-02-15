using System;

namespace VSS.Subscription.Data.Models
{
  public class Subscription
  {
    public string SubscriptionUID { get; set; }
    public string CustomerUID { get; set; }
    public int ServiceTypeID { get; set; }

    // start, end and Effective are actually only date with no time component. However C# has no date-only.
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; } = DateTime.MaxValue.Date;

    public DateTime LastActionedUTC { get; set; }
  }

  public class ServiceType
  {
    public int ID { get; set; }
    public string Name { get; set; }
    public int ServiceTypeFamilyID { get; set; }
    public string ServiceTypeFamilyName { get; set; }
  }
}
