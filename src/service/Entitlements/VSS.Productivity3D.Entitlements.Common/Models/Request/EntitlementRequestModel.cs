namespace VSS.Productivity3D.Entitlements.Common.Models.Request
{
  public class EntitlementRequestModel
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
  }
}
