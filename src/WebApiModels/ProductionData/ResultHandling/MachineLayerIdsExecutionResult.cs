using VSS.Raptor.Service.WebApiModels.ProductionData.Models;
using VSS.Raptor.Service.Common.Contracts;

namespace VSS.Raptor.Service.WebApiModels.ProductionData.ResultHandling
{
  public class MachineLayerIdsExecutionResult : ContractExecutionResult
  {

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
    public static MachineLayerIdsExecutionResult HelpSample
    {
      get
      {
        return new MachineLayerIdsExecutionResult()
        {
          MachineLiftDetails = new[] { Models.MachineLiftDetails.HelpSample }
        };
      }
    }

  }
}