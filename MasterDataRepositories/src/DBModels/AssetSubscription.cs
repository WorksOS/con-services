using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Repositories.DBModels
{
  public class AssetSubscription
    {
      public string AssetUID { get; set; }
      public string SubscriptionUID { get; set; }
      public DateTime EffectiveDate { get; set; }
      public DateTime LastActionedUTC { get; set; }
  }
}
