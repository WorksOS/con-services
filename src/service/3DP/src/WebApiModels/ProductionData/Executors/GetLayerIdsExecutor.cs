using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  public class GetLayerIdsExecutor : RequestExecutorContainer
  {
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      var request = CastRequestObjectTo<ProjectID>(item);

      if (raptorClient.GetOnMachineLayers(request.ProjectId ?? -1, out var layerlist) >= 0 && layerlist != null)
      {
        return LayerIdsExecutionResult.CreateLayerIdsExecutionResult(layerlist);
      }

      throw CreateServiceException<GetLayerIdsExecutor>();
    }
  }
}
