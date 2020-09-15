using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using VSS.Common.Abstractions.MasterData.Interfaces;

namespace VSS.Productivity3D.Entitlements.Abstractions.Models.Response
{
  public class EntitlementResponseModel : IMasterDataModel
  {
    /// <summary>
    /// All entitlements are tagged by this key in cache, and can be cleared and refreshed by invalidating the cache
    /// </summary>
    public const string EntitlementCacheTag = "ENTITLEMENT";

    /// <summary>
    /// An identifier to represent the Organization
    /// Example is a CustomerUID in WorksOS
    /// </summary>
    public string OrganizationIdentifier { get; set; }

    /// <summary>
    /// The identifier of the user
    /// </summary>
    public string UserUid { get; set; }

    /// <summary>
    /// The email address of the user
    /// </summary>
    public string UserEmail { get; set; }

    /// <summary>
    /// The feature the Entitlement represents, e.g WorksOS-Basic, WorksOS-Advanced etc - free text as can be extending external to our service.
    /// </summary>
    public string Feature { get; set; }

    /// <summary>
    /// The SKU (Stock Keeping Unit) or product code the Entitlement represents.
    /// </summary>
    public string Sku { get; set; }

    /// <summary>
    /// Is the user entitled to the feature described.
    /// </summary>
    [JsonProperty(PropertyName = "isEntitled")]
    public bool IsEntitled { get; set; }

    public List<string> GetIdentifiers()
    {
      return new List<string>
      {
        OrganizationIdentifier, UserUid, EntitlementCacheTag
      };
    }
  }
}
