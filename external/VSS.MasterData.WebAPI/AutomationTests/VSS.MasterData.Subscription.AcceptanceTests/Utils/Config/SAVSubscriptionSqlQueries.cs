using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSS.MasterData.Subscription.AcceptanceTests.Utils.Config
{
  class SAVSubscriptionSqlQueries
  {
    public static string SAVSubscriptionByAssetUID = "SELECT hex(fk_CustomerUID),startDate,EndDate,fk_SubscriptionSourceID FROM AssetSubscription where fk_AssetUID=unhex('{0}') and AssetSubscriptionUID=unhex('{1}')";
  }
}
