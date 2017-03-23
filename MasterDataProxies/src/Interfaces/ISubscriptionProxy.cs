using System;
using System.Collections.Generic;

namespace VSS.Raptor.Service.Common.Interfaces
{
    public interface ISubscriptionProxy
    {
        void AssociateProjectSubscription(Guid subscriptionUid, Guid projectUid,
            IDictionary<string, string> customHeaders = null);
    }
}

