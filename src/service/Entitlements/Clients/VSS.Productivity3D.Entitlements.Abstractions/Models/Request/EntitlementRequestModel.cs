using System;

namespace VSS.Productivity3D.Entitlements.Abstractions.Models.Request
{
  public class EntitlementRequestModel
  {
    /// <summary>
    /// An identifier to represent the Organization
    /// Example is a CustomerUID in WorksOS
    /// </summary>
    public string OrganizationIdentifier { get; set; }

    /// <summary>
    /// The email of the User requesting the entitlement
    /// </summary>
    public string UserEmail { get; set; }

    /// <summary>
    /// The identifier of the user
    /// </summary>
    public string UserUid { get; set; }

    /// <summary>
    /// The feature the Entitlement represents, e.g WorksOS-Basic, WorksOS-Advanced etc - free text as can be extending external to our service.
    /// </summary>
    public string Feature { get; set; }

    /// <summary>
    /// The SKU (Stock Keeping Unit) or product code the Entitlement represents.
    /// </summary>
    public string Sku { get; set; }
  }
}
