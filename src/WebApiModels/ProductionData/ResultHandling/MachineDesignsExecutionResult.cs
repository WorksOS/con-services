using Newtonsoft.Json;
using System.Collections.Generic;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling
{
  public class MachineDesignsExecutionResult : ContractExecutionResult
  {
    /// <summary>
    /// The list of the on-machine designs available for the project.
    /// </summary>
    /// <value>
    /// The designs.
    /// </value>
    [JsonProperty(PropertyName = "designs")]
    public List<DesignNames> Designs { get; private set; }

    /// <summary>
    /// The design details - used by the controller for filtering by machine and date range
    /// </summary>
    [JsonIgnore]
    public TDesignName[] DesignDetails { get; private set; }

    public static ContractExecutionResult CreateMachineExecutionResult(TDesignName[] designNames)
    {
      var result  = new MachineDesignsExecutionResult { Designs = new List<DesignNames>()};
      foreach (var name in designNames)
      {
        result.Designs.Add(DesignNames.CreateDesignNames(name.FName, name.FID));
      }
      result.DesignDetails = designNames;
      return result;
    }
  }
}
