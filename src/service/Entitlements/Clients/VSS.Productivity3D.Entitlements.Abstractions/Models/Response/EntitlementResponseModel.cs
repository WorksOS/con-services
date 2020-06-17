using System;
using System.Collections.Generic;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Entitlements.Abstractions.Models.Response
{
  public class EntitlementResponseModel : IMasterDataModel
  {
    /// <summary>
    /// An identifier to represent the Organization
    /// Example is a CustomerUID in WorksOS
    /// </summary>
    public string OrganizationIdentifier { get; set; }

    /// <summary>
    /// The Users email requesting the entitlement
    /// </summary>
    public string UserEmail { get; set; }

    /// <summary>
    /// The feature the Entitlement represents, e.g WorksOS-Basic, WorksOS-Advanced etc - free text as can be extending external to our service.
    /// </summary>
    public string Feature { get; set; }

    /// <summary>
    /// Is the user entitled to the feature described.
    /// </summary>
    public bool IsEntitled { get; set; }

    public List<string> GetIdentifiers()
    {
      return new List<string>
      {
        OrganizationIdentifier, UserEmail
      };
    }
  }
}
