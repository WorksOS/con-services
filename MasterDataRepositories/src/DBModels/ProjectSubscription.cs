using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.DBModels
{
  public class ProjectSubscription
  {
    public string ProjectUID { get; set; }
    public string SubscriptionUID { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime LastActionedUTC { get; set; }
  }

}
