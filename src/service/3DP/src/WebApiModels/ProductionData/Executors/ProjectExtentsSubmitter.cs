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
#if RAPTOR
        if (UseTRexGateway("ENABLE_TREX_GATEWAY_TILES"))
        {
#endif
          var siteModelId = request.ProjectUid.ToString();

          bbExtents = trexCompactionDataProxy.SendDataGetRequest<BoundingBox3DGrid>(siteModelId, $"/sitemodels/{siteModelId}/extents", customHeaders).Result;
          success = bbExtents != null;
#if RAPTOR
        }
        else
        {
          success = raptorClient.GetDataModelExtents(request.ProjectId ?? -1,
            RaptorConverters.convertSurveyedSurfaceExlusionList(request.excludedSurveyedSurfaceIds),
            out var extents);

          bbExtents = RaptorConverters.ConvertExtents(extents);
        }
#endif
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
