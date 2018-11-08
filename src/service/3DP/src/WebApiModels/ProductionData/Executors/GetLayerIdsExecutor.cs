using System.Net;
using VLPDDecls;
using VSS.Common.Exceptions;
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
      var request = item as ProjectID;

      if (request == null)
        ThrowRequestTypeCastException<ProjectID>();

      TDesignLayer[] layerlist;

      if (raptorClient.GetOnMachineLayers(request.ProjectId ?? -1, out layerlist) >= 0 && layerlist != null)
        return LayerIdsExecutionResult.CreateLayerIdsExecutionResult(layerlist);

      throw CreateServiceException<GetLayerIdsExecutor>();
    }
  }
}
