using System.Collections.Generic;
using Newtonsoft.Json;

namespace VSS.MasterData.Project.WebAPI.Common.Models
{
  public class AccountHierarchy
  {
    [JsonProperty("UserUID")]
    public string UserUid { get; set; }

    [JsonProperty("Customers")]
    public List<AccountHierarchyCustomer> Customers { get; set; }
  }
}
