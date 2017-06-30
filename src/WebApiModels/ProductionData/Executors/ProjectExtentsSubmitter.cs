using System.Net;
using BoundingExtents;
using Microsoft.Extensions.Logging;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.Common.ResultHandling;
using VSS.Productivity3D.WebApiModels.ProductionData.Models;
using VSS.Productivity3D.WebApiModels.ProductionData.ResultHandling;

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
    /// this allows us to Mock raptorClient
    /// </summary>
    /// <param name="raptorClient"></param>
    public ProjectExtentsSubmitter(ILoggerFactory logger, IASNodeClient raptorClient) : base(logger, raptorClient)
    {
    }

    /// <summary>
    /// Default constructor for RequestExecutorContainer.Build
    /// </summary>
    public ProjectExtentsSubmitter()
    {
    }

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

    protected override void ProcessErrorCodes()
    {
   //   throw new NotImplementedException();
    }
  }
}