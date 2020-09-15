using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace VSS.Productivity3D.Entitlements.Abstractions.Models.Request
{
  /// <summary>
  /// Model for external entitlement requests i.e. from WorksOS UI
  /// </summary>
  public class ExternalEntitlementRequestModel
  {
    /// <summary>
    /// An identifier to represent the Organization
    /// Example is a CustomerUID in WorksOS
    /// </summary>
    [JsonProperty(PropertyName = "organizationIdentifier", Required = Required.Always)]
    [Required]
    public string OrganizationIdentifier { get; set; }

    /// <summary>
    /// The application name used to map the feature requested
    /// </summary>
    [JsonProperty(PropertyName="feature", Required = Required.Always)]
    [Required]
    public string ApplicationName { get; set; }

    /// <summary>
    /// The email address of the user. 
    /// </summary>
    [JsonProperty(PropertyName = "userEmail", Required = Required.Default)]
    public string UserEmail { get; set; }

  }
}
