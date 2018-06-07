using Newtonsoft.Json;
using System.Collections.Generic;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class MachineDesignsExecutionResult : ContractExecutionResult
  {
    /// <summary>
    /// The list of the on-machine designs available for the project.
    /// </summary>
    [JsonProperty(PropertyName = "designs")]
    public List<DesignNames> Designs { get; private set; }

    public static MachineDesignsExecutionResult CreateMachineExecutionResult(List<DesignNames> designNames)
    {
      return new MachineDesignsExecutionResult
      {
        Designs = designNames,
      };
    }
  }
}
