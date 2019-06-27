using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class MachineExecutionResult : ContractExecutionResult, IMasterDataModel
  {
    [JsonProperty(PropertyName = "machineStatuses")]
    public List<MachineStatus> MachineStatuses { get; set; }

    /// <summary>
    /// Default private constructor.
    /// </summary>
    private MachineExecutionResult()
    { }

    /// <summary>
    /// Static constructor.
    /// </summary>
    public MachineExecutionResult(List<MachineStatus> machineDetails)
    {
      MachineStatuses = machineDetails;
    }

    public void FilterByMachineId(long machineId)
    {
      MachineStatuses = MachineStatuses.Where(m => m.AssetId == machineId).ToList();
    }

    public void FilterByMachineUid(Guid machineUid)
    {
      MachineStatuses = MachineStatuses.Where(m => m.AssetUid.HasValue && m.AssetUid == machineUid).ToList();
    }

    public List<string> GetIdentifiers()
    {
      return MachineStatuses?
        .Where(p => p.AssetUid.HasValue)
        .Select(p => p.AssetUid.Value.ToString())
        .ToList();
    }
  }
}
