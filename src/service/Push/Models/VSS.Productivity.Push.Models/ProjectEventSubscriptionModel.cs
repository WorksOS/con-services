using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.Http;

namespace VSS.Productivity.Push.Models
{

  /// <summary>
  /// A list of clients which have connected for events, and been authorized using the below criteria.
  /// ProjectEvents differs from AssetUpdate in that here there can be multiple projects per customer.
  /// ... todoJeannie perhaps we want a list of events which are allowable?
  /// 
  /// I dislike using the term 'subscription' here as it could be confused with application role subscriptions or entitlements.
  /// will leave for now as may want to merge this with AssetUpdateSubscriptionModel
  /// </summary>
  public class ProjectEventSubscriptionModel
  {
    public Guid CustomerUid { get; set; }
    
    public Guid ProjectUid { get; set; }

    public string AuthorizationHeader { get; set; }

    public string JWTAssertion { get; set; }

    public IDictionary<string, string> Headers()
    {
      return new Dictionary<string, string>()
      {
        {HeaderConstants.X_VISION_LINK_CUSTOMER_UID, CustomerUid.ToString()}, 
        {HeaderConstants.AUTHORIZATION, AuthorizationHeader},
        {HeaderConstants.X_JWT_ASSERTION, JWTAssertion}
      };
    }
  }
}
