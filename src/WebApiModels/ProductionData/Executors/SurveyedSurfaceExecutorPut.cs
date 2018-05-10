using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Proxies;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  /// <summary>
  /// Executes PUT method on Surveyed Surfaces resource.
  /// </summary>
  /// 
  public class SurveyedSurfaceExecutorPut : SurveyedSurfaceExecutor
  {
    /// <summary>
    /// Sends a PUT request to Production Data Server (PDS) client.
    /// </summary>
    /// <param name="item">PUT request description.</param>
    /// <param name="surveyedSurfaces">Returned list of Surveyed Surfaces.</param>
    /// <returns>True if the processed request from PDS was successful, false - otherwise.</returns>
    /// 
    protected override bool SendRequestToPdsClient(object item, out TSurveyedSurfaceDetails[] surveyedSurfaces)
    {
      surveyedSurfaces = null;

      SurveyedSurfaceRequest request = item as SurveyedSurfaceRequest;

      ASNode.GroundSurface.RPC.TASNodeServiceRPCVerb_GroundSurface_Args args = ASNode.GroundSurface.RPC.__Global
        .Construct_GroundSurface_Args(
          request.projectId ?? -1,
          request.SurveyedSurface.id,
          request.SurveyedUtc,
          RaptorConverters.DesignDescriptor(request.SurveyedSurface)
        );

      return raptorClient.UpdateGroundSurfaceFile(args);
    }

    /// <summary>
    /// Returns an instance of the ContractExecutionResult class as PUT method execution result.
    /// </summary>
    /// <returns>An instance of the ContractExecutionResult class.</returns>
    protected override ContractExecutionResult ExecutionResult(SurveyedSurfaceDetails[] surveyedSurfaces)
    {
      return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Surveyed Surface data successfully updated.");
    }
  }
}
