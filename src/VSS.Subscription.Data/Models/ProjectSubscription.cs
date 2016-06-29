using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.Subscription.Data.Models
{
  public class ProjectSubscription
  {
    public string ProjectUID { get; set; }
    public string SubscriptionUID { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }

}
