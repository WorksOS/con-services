using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;

namespace VSS.Productivity3D.Models.ResultHandling
{
  public class LayerIdsExecutionResult : ContractExecutionResult
  {
    public LayerIdDetails[] LayerIdDetailsArray { get; private set; }

    public LayerIdsExecutionResult(LayerIdDetails[] layerlist)
    {
      LayerIdDetailsArray = layerlist;
    }

  }
}
