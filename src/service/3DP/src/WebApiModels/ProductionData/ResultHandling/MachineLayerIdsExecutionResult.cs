using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class MachineLayerIdsExecutionResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "machineLiftDetails")]
    public List<MachineLiftDetails> MachineLiftDetails { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private MachineLayerIdsExecutionResult()
    {
    }

    /// <summary>
    /// Create instance of MachineLayerIdsExecutionResult
    /// </summary>
    public MachineLayerIdsExecutionResult(List<MachineLiftDetails> machineLiftDetails)
    {
      MachineLiftDetails = machineLiftDetails;
    }
  }
}
