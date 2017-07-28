using Newtonsoft.Json;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling
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

    /// <summary>
    /// Create example instance of MachineLayerIdsExecutionResult to display in Help documentation.
    /// </summary>
    public static MachineLayerIdsExecutionResult HelpSample => new MachineLayerIdsExecutionResult
    {
      MachineLiftDetails = new[] { Models.MachineLiftDetails.HelpSample }
    };
  }
}