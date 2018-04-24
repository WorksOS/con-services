using BoundingExtents;
using System.Net;
using VSS.Common.Exceptions;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;
  
namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  /// <summary>
  /// Exexcutes GetProjectsExtents
  /// </summary>
  public class ProjectExtentsSubmitter : RequestExecutorContainer
  {
    /// <summary>
    /// Project extents returnd by Raptor
    /// </summary>
    private T3DBoundingWorldExtent extents;
    
    /// <summary>
    /// Calls raptor to get project extents
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="item">ExtentRequest</param>
    /// <returns>ContractExecutionResult</returns>
    protected override ContractExecutionResult ProcessEx<T>(T item)
    {
      ContractExecutionResult result;
      try
      {
        // get request parameters
        ExtentRequest request = item as ExtentRequest;

        bool success = raptorClient.GetDataModelExtents(request.projectId ?? -1,
              RaptorConverters.convertSurveyedSurfaceExlusionList(request.excludedSurveyedSurfaceIds),
              out extents);


        if (success)
          result = ProjectExtentsResult.CreateProjectExtentsResult(RaptorConverters.ConvertExtents(extents));
        else
          throw new ServiceException(HttpStatusCode.BadRequest,
                  new ContractExecutionResult(ContractExecutionStatesEnum.FailedToGetResults,
                          "Failed to get project extents"));
      }
      finally
      {
        ContractExecutionStates.ClearDynamic(); // clear memory
      }
      return result;
    }
  }
}
