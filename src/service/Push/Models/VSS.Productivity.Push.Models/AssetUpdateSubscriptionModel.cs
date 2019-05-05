using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.Http;

namespace VSS.Productivity.Push.Models
{
  public class AssetUpdateSubscriptionModel
  {
    public Guid ProjectUid { get; set; }

    public Guid CustomerUid { get; set; }

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
