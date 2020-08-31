using System.Collections.Generic;
using VSS.Productivity3D.Entitlements.Abstractions.Models.Request;
using VSS.Productivity3D.Entitlements.Common.Authentication;

namespace VSS.Productivity3D.Entitlements.Common.Models
{
  public class GetEntitlementsRequest
  {
    public EntitlementUserClaim User { get; set; }
    public EntitlementRequestModel Request { get; set; }
    public List<string> AcceptedEmails { get; set; }
    public bool EnableEntitlementCheck { get; set; }
  }
}
