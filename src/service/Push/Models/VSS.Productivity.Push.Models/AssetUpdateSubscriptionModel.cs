using System;
using Microsoft.AspNetCore.Http;
using VSS.Common.Abstractions.Http;

namespace VSS.Productivity.Push.Models
{
  public class AssetUpdateSubscriptionModel
  {
    public Guid ProjectUid { get; set; }

    public Guid CustomerUid { get; set; }

    public string AuthorizationHeader { get; set; }

    public string JWTAssertion { get; set; }

    public IHeaderDictionary Headers() => new HeaderDictionary
      {
        {HeaderConstants.X_VISION_LINK_CUSTOMER_UID, CustomerUid.ToString()},
        {HeaderConstants.AUTHORIZATION, AuthorizationHeader},
        {HeaderConstants.X_JWT_ASSERTION, JWTAssertion}
      };
  }
}
