using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Executors
{
  /// <summary>
  /// Executes GET method on Surveyed Surfaces resource.
  /// </summary>
  public class SurveyedSurfaceExecutorGet : SurveyedSurfaceExecutor
  {
    /// <summary>
    /// Sends a GET request to Production Data Server (PDS) client.
    /// </summary>
    /// <param name="item">GET request description.</param>
    /// <param name="surveyedSurfaces">Returned list of Surveyed Surfaces.</param>
    /// <returns>True if the processed request from PDS was successful, false - otherwise.</returns>
    protected override bool SendRequestToPdsClient(object item, out TSurveyedSurfaceDetails[] surveyedSurfaces)
    {
      var request = CastRequestObjectTo<ProjectID>(item);

      return raptorClient.GetKnownGroundSurfaceFileDetails(request.ProjectId ?? -1, out surveyedSurfaces);
    }

    /// <summary>
    /// Returns an instance of the ContractExecutionResult class as GET method execution result.
    /// </summary>
    /// <returns>An instance of the ContractExecutionResult class.</returns>
    protected override ContractExecutionResult ExecutionResult(SurveyedSurfaceDetails[] surveyedSurfaces)
    {
      return SurveyedSurfaceResult.CreateSurveyedSurfaceResult(surveyedSurfaces);
    }
  }
} 
