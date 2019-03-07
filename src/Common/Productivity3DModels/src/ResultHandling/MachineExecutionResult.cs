using System;
using System.Linq;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class MachineExecutionResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "machineStatuses")]
    public MachineStatus[] MachineStatuses { get; set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private MachineExecutionResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public MachineExecutionResult(MachineStatus[] machineDetails)
    {
      MachineStatuses = machineDetails;
    }

    public void FilterByMachineId(long machineId)
    {
      MachineStatuses = MachineStatuses.Where(m => m.AssetId == machineId).ToArray();
    }

    public void FilterByMachineUid(Guid machineUid)
    {
      MachineStatuses = MachineStatuses.Where(m => m.AssetUid.HasValue && m.AssetUid == machineUid).ToArray();
    }
  }
}
