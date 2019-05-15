using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  public class Machine3DStatuses : ContractExecutionResult
  {
    public Machine3DStatuses()
    {
      MachineStatuses = new List<MachineStatus>();
    }

    public Machine3DStatuses(int code) : base(code)
    {
      MachineStatuses = new List<MachineStatus>();
    }

    [JsonProperty(Required = Required.Default)]
    public List<MachineStatus> MachineStatuses { get; set; }
  }
}
