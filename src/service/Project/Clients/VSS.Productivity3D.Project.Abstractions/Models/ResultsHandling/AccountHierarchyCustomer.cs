using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class AccountHierarchyCustomer
  {
    [JsonProperty("CustomerCode")]
    public string CustomerCode { get; set; }

    [JsonProperty("CustomerType")]
    public string CustomerType { get; set; }

    [JsonProperty("CustomerUID")]
    public string CustomerUid { get; set; }

    [JsonProperty("DisplayName")]
    public string DisplayName { get; set; }

    [JsonProperty("Name")]
    public string Name { get; set; }

    [JsonProperty("Children")]
    public List<AccountHierarchyCustomer> Children { get; set; } = new List<AccountHierarchyCustomer>();
  }
}
