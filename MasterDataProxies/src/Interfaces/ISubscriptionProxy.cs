using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VSS.Productivity3D.MasterDataProxies.Interfaces
{
  public interface ISubscriptionProxy
  {
    Task AssociateProjectSubscription(Guid subscriptionUid, Guid projectUid,
      IDictionary<string, string> customHeaders = null);

    Task DissociateProjectSubscription(Guid subscriptionUid, Guid projectUid,
      IDictionary<string, string> customHeaders = null);
  }
}

