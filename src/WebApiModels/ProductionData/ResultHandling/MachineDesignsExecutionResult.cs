using System.Collections.Generic;
using Newtonsoft.Json;
using VLPDDecls;
using VSS.Common.ResultsHandling;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling
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

        
    public static ContractExecutionResult CreateMachineExecutionResult(IEnumerable<TDesignName> designNames)
    {
        var result  = new MachineDesignsExecutionResult { Designs = new List<DesignNames>()};
        foreach (var name in designNames)
        {
            result.Designs.Add(DesignNames.CreateDesignNames(name.FName, name.FID));
        }
        return result;
    }


    /// <summary>
    /// Create example instance of MachineDesignsExecutionResult to display in Help documentation.
    /// </summary>
    public static MachineDesignsExecutionResult HelpSample
    {
        get
        {
            return new MachineDesignsExecutionResult
            {
              Designs = new List<DesignNames> {DesignNames.HelpSample, DesignNames.HelpSample}
            };
        }
    }
  }
}