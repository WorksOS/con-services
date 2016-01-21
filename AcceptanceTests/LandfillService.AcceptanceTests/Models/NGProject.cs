using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LandfillService.AcceptanceTests.Models
{
    public class ProjectNg
    {
        public DateTime lastActionedUtc { get; set; }
        public int projectId { get; set; }
        public string name { get; set; }
        public string timeZone { get; set; }
        public DateTime retrievalStartedAt { get; set; }
        public int daysToSubscriptionExpiry { get; set; }
        public string projectUid { get; set; }
        public string customerUid { get; set; }
        public string subscriptionUid { get; set; }
    }
}
