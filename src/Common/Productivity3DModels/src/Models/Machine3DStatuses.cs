using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Models.Models
{
  public class Machine3DStatuses : ContractExecutionResult
  {
    public Machine3DStatuses()
    {
      MachineStatuses = new List<MachineStatus>();
    }
    public List<MachineStatus> MachineStatuses { get; set; }
  }
}
