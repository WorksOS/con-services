﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MasterDataProxies.Interfaces;
using MasterDataProxies.Models;
using MasterDataProxies.ResultHandling;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.GenericConfiguration;

namespace MasterDataProxies
{
  public class SubscriptionProxy : BaseProxy, ISubscriptionProxy
  {
    public SubscriptionProxy(IConfigurationStore configurationStore, ILoggerFactory logger, IMemoryCache cache) 
      : base(configurationStore, logger, cache)
    {
    }

    /// <summary>
    /// Associates the project subscription.
    /// </summary>
    /// <param name="subscriptionUid">The subscription uid.</param>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task AssociateProjectSubscription(Guid subscriptionUid, Guid projectUid,
      IDictionary<string, string> customHeaders = null)
    {
      var payLoadToSend = new AssociateProjectSubscriptionData()
      {
        EffectiveDate = DateTime.UtcNow.Date,
        ProjectUID = projectUid,
        SubscriptionUID = subscriptionUid
      };
      await SendRequest<SubscriptionDataResult>("ASSOCIATESUBSPROJECT_API_URL", JsonConvert.SerializeObject(payLoadToSend),
        customHeaders);
    }

    /// <summary>
    /// Dissociates the project subscription.
    /// </summary>
    /// <param name="subscriptionUid">The subscription uid.</param>
    /// <param name="projectUid">The project uid.</param>
    /// <param name="customHeaders">The custom headers.</param>
    public async Task DissociateProjectSubscription(Guid subscriptionUid, Guid projectUid,
      IDictionary<string, string> customHeaders = null)
    {
      var payLoadToSend = new AssociateProjectSubscriptionData()
      {
        EffectiveDate = DateTime.UtcNow.Date,
        ProjectUID = projectUid,
        SubscriptionUID = subscriptionUid
      };
      await SendRequest<SubscriptionDataResult>("DISSOCIATESUBSPROJECT_API_URL", JsonConvert.SerializeObject(payLoadToSend),
        customHeaders);
    }
  }
}
