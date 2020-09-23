using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace CCSS.WorksOS.Reports.Common.Models
{
  public class MachineDesignData : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "designs")]
    public List<DesignDetail> MachineDesigns { get; set; }
  }
  public class DesignDetail
  {
    [JsonProperty(PropertyName = "designId")]
    public string DesignId { get; set; }
    [JsonProperty(PropertyName = "designName")]
    public string DesignName { get; set; }
  }
}
