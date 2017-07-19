using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.MasterData.Proxies.Interfaces
{
  public interface ISubscriptionProxy
  {
    Task AssociateProjectSubscription(Guid subscriptionUid, Guid projectUid,
      IDictionary<string, string> customHeaders = null);

    Task DissociateProjectSubscription(Guid subscriptionUid, Guid projectUid,
      IDictionary<string, string> customHeaders = null);
  }
}

