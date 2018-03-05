using Newtonsoft.Json;
using System.Linq;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class MachineExecutionResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "machineStatuses")]
    public MachineStatus[] MachineStatuses { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private MachineExecutionResult()
    {}

    /// <summary>
    /// Create instance of MachineExecutionResult
    /// </summary>
    public static MachineExecutionResult CreateMachineExecutionResult(MachineStatus[] machineDetails)
    {
      return new MachineExecutionResult
      {
        MachineStatuses = machineDetails,
      };
    }


    public void FilterByMachineId(long machineId)
    {
      MachineStatuses = MachineStatuses.Where(m => m.assetID == machineId).ToArray();
    }
  }
}