using System.Collections.Generic;
using Newtonsoft.Json;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class LayerIdsExecutionResult : ContractExecutionResult
  {
    [JsonProperty(PropertyName = "LayerIdDetailsArray")]
    public List<LayerIdDetails> Layers { get; private set; }

    /// <summary>
    /// Private constructor
    /// </summary>
    private LayerIdsExecutionResult()
    {
    }

    public LayerIdsExecutionResult(List<LayerIdDetails> layers)
    {
      Layers = layers;
    }

  }
}
