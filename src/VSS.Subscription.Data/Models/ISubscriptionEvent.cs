using System;
using System.Collections.Generic;

namespace VSS.Subscription.Model.Interfaces
{
	public interface ISubscriptionEvent
	{
        Guid SubscriptionUID { get; set; }
        DateTime ActionUTC { get; set; }
        DateTime ReceivedUTC { get; set; }
	}
}
