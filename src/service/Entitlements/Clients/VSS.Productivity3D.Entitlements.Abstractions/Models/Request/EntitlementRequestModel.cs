using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Entitlements.Abstractions.Models.Request
{
  public class EntitlementRequestModel
  {
    /// <summary>
    /// An identifier to represent the Organization
    /// Example is a CustomerUID in WorksOS
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public string OrganizationIdentifier { get; set; }

    /// <summary>
    /// The identifier of the user
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public string UserUid { get; set; }

    /// <summary>
    /// The email address of the user. 
    /// </summary>
    [JsonProperty(Required = Required.Default)]
    public string UserEmail { get; set; }

    /// <summary>
    /// The feature the Entitlement represents, e.g WorksOS-Basic, WorksOS-Advanced etc - free text as can be extending external to our service.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public string Feature { get; set; }

    /// <summary>
    /// The SKU (Stock Keeping Unit) or product code the Entitlement represents.
    /// </summary>
    [JsonProperty(Required = Required.Always)]
    [Required]
    public string Sku { get; set; }

    /// <summary>
    /// Validates the model
    /// </summary>
    public ContractExecutionResult Validate(string jwtUserUid)
    {
      if (string.IsNullOrEmpty(jwtUserUid))
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "JWT uuid is empty.");
      }

      if (string.Compare(jwtUserUid, UserUid, StringComparison.InvariantCultureIgnoreCase) != 0)
      {
        return new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError, "Provided uuid does not match JWT.");
      }
      return new ContractExecutionResult();
    }
  }
}
