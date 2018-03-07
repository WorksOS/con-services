using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class MachineLayerIdsExecutionResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "machineLiftDetails")]
    public MachineLiftDetails[] MachineLiftDetails { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private MachineLayerIdsExecutionResult()
    { }

    /// <summary>
    /// Create instance of MachineLayerIdsExecutionResult
    /// </summary>
    public static MachineLayerIdsExecutionResult CreateMachineLayerIdsExecutionResult(MachineLiftDetails[] machineLiftDetails)
    {
      return new MachineLayerIdsExecutionResult
      {
        MachineLiftDetails = machineLiftDetails,
      };
    }
  }
}