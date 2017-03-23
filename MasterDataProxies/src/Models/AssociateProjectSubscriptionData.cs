using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VSS.Raptor.Service.Common.Proxies.Models
{
    public class AssociateProjectSubscriptionData
    {
        public Guid SubscriptionUID { get; set; }
        public Guid ProjectUID { get; set; }
        public DateTime EffectiveDate { get; set; }
    }
}
