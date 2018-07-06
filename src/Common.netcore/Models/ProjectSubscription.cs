using System;

namespace Common.Models
{
  public class ProjectSubscription
  {
    public string ProjectUID { get; set; }
    public string SubscriptionUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }
}