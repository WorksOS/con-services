using System.Linq;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling
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

    /// <summary>
    /// Create example instance of MachineExecutionResult to display in Help documentation.
    /// </summary>
    public static MachineExecutionResult HelpSample
    {
      get
      {
        return new MachineExecutionResult
        {
            MachineStatuses = new []{MachineStatus.HelpSample}
        };
      }
    }

  }
}