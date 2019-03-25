using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class MachineDesignDetailsExecutionResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "machineDesignDetails")]
    public List<MachineDesignDetails> MachineDesignDetails { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private MachineDesignDetailsExecutionResult()
    { }

    /// <summary>
    /// Create instance of MachineDesignDetailsExecutionResult
    /// </summary>
    public MachineDesignDetailsExecutionResult(List<MachineDesignDetails> machineDesignDetails)
    {
      MachineDesignDetails = machineDesignDetails;
    }
  }
}
