using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using VSS.Common.Abstractions.MasterData.Interfaces;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.Productivity3D.Models.ProductionData
{
  public class Machine3DStatuses : ContractExecutionResult, IMasterDataModel
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

    public List<string> GetIdentifiers()
    {
      return MachineStatuses?
        .Where(p => p.AssetUid.HasValue)
        .Select(p => p.AssetUid.Value.ToString())
        .ToList();
    }
  }
}
