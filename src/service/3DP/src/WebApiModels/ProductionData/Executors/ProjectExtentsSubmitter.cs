using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Exexcutes GetProjectsExtents
  /// </summary>
  public class ProjectExtentsSubmitter : RequestExecutorContainer
  {
    /// <summary>
    /// Calls raptor to get project extents
    /// </summary>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      try
      {
        var request = CastRequestObjectTo<ExtentRequest>(item);
        bool success;
        BoundingBox3DGrid bbExtents = null;

        if (UseTRexGateway("ENABLE_TREX_GATEWAY_TILES"))
        {
          bbExtents = trexCompactionDataProxy.SendProjectExtentsRequest(request.ProjectUid.ToString(), customHeaders).Result;
          success = bbExtents != null;
        }
        else
        {
          success = raptorClient.GetDataModelExtents(request.ProjectId ?? -1,
            RaptorConverters.convertSurveyedSurfaceExlusionList(request.excludedSurveyedSurfaceIds),
            out var extents);

          bbExtents = RaptorConverters.ConvertExtents(extents);
        }

        if (success)
          return ProjectExtentsResult.CreateProjectExtentsResult(bbExtents);

        throw CreateServiceException<ProjectExtentsSubmitter>();
      }
      finally
      {
        ContractExecutionStates.ClearDynamic(); // clear memory
      }
    }
  }
}
