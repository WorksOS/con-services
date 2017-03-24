using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Interfaces;
using VSS.Raptor.Service.Common.Proxies.Models;

namespace VSS.Raptor.Service.Common.Proxies
{
    public class SubscriptionProxy : BaseProxy<SubscriptionData>, ISubscriptionProxy
    {
        public SubscriptionProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) : base(configurationStore, logger, cache)
        {
        }

        /// <summary>
        /// Associates the project subscription.
        /// </summary>
        /// <param name="subscriptionUid">The subscription uid.</param>
        /// <param name="projectUid">The project uid.</param>
        /// <param name="customHeaders">The custom headers.</param>
        public async Task AssociateProjectSubscription(Guid subscriptionUid, Guid projectUid, IDictionary<string, string> customHeaders = null)
        {
            var payLoadToSend = new AssociateProjectSubscriptionData() {EffectiveDate = DateTime.UtcNow.Date, ProjectUID = projectUid, SubscriptionUID = subscriptionUid};
            await SendRequest("ASSOCIATESUBSPROJECT_API_URL", JsonConvert.SerializeObject(payLoadToSend), customHeaders);
        }
    }
}
