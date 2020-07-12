using System.Threading.Tasks;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
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
    protected override async Task<ContractExecutionResult> ProcessAsyncEx<T>(T item)
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

          bbExtents = await trexCompactionDataProxy.SendDataGetRequest<BoundingBox3DGrid>(siteModelId, $"/sitemodels/{siteModelId}/extents", customHeaders);
          success = bbExtents != null;
#if RAPTOR
        }
        else
        {
          success = raptorClient.GetDataModelExtents(request.ProjectId ?? VelociraptorConstants.NO_PROJECT_ID,
            RaptorConverters.convertSurveyedSurfaceExlusionList(request.excludedSurveyedSurfaceIds),
            out var extents);

          bbExtents = RaptorConverters.ConvertExtents(extents);
        }
#endif
        if (success)
        {
          //Project without any tagfile data will have these values for extents from TRex
          const double MIN_RANGE = -1E100;
          const double MAX_RANGE = 1E100;
          var noExtents = bbExtents.MinX == MAX_RANGE && bbExtents.MaxX == MIN_RANGE &&
                          bbExtents.MinY == MAX_RANGE && bbExtents.MaxY == MIN_RANGE &&
                          bbExtents.MinZ == MAX_RANGE && bbExtents.MaxZ == MIN_RANGE;
          if (!noExtents)
          {
            return ProjectExtentsResult.CreateProjectExtentsResult(bbExtents);
          }
        }

        throw CreateServiceException<ProjectExtentsSubmitter>();
      }
      finally
      {
        ContractExecutionStates.ClearDynamic(); // clear memory
      }
    }
  }
}
