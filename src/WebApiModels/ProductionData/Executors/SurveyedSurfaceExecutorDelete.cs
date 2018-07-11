using System;
using VLPDDecls;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Models.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.Executors;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Executors
{
  /// <summary>
  /// Executes DELETE method on Surveyed Surfaces resource.
  /// </summary>
  public class SurveyedSurfaceExecutorDelete : SurveyedSurfaceExecutor
  {
    /// <summary>
    /// Sends a DELETE request to Production Data Server (PDS) client.
    /// </summary>
    /// <param name="item">DELETE request description.</param>
    /// <param name="surveyedSurfaces">Returned list of Surveyed Surfaces.</param>
    /// <returns>True if the processed request from PDS was successful, false - otherwise.</returns>
    /// 
    protected override bool SendRequestToPdsClient(object item, out TSurveyedSurfaceDetails[] surveyedSurfaces)
    {
      surveyedSurfaces = null;

      ProjectID projectId = (item as Tuple<ProjectID, DataID>).Item1;
      DataID surveyedSurfaceId= (item as Tuple<ProjectID, DataID>).Item2;

      return raptorClient.DiscardGroundSurfaceFileDetails(projectId.ProjectId ?? -1, surveyedSurfaceId.dataId);
    }

    /// <summary>
    /// Returns an instance of the ContractExecutionResult class as DELETE method execution result.
    /// </summary>
    /// <returns>An instance of the ContractExecutionResult class.</returns>
    /// 
    protected override ContractExecutionResult ExecutionResult(SurveyedSurfaceDetails[] surveyedSurfaces)
    {
      return new ContractExecutionResult(ContractExecutionStatesEnum.ExecutedSuccessfully, "Surveyed Surface data successfully deleted.");
    }
  }
}
