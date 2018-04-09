using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Contracts
{
  /// <summary>
  /// Data contract representing surveyed surfaces...
  /// </summary>
  /// 
  public interface ISurveyedSurfaceContract
  {
    /// <summary>
    /// Posts a Surveyed Surface to Raptor.
    /// </summary>
    /// <param name="request">Description of the Surveyed Surface request.</param>
    /// <returns>Execution result.</returns>
    /// 
    ContractExecutionResult Post([FromBody]SurveyedSurfaceRequest request);

    /// <summary>
    /// Deletes a Surveyed Surface form Raptor's list of surveyed surfaces.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <param name="surveyedSurfaceId">The Surveyed Surface identifier.</param>
    /// <returns></returns>
    /// 
    ContractExecutionResult GetDel([FromRoute] long projectId, [FromRoute] long surveyedSurfaceId);

    /// <summary>
    /// Deletes a Surveyed Surface form Raptor's list of surveyed surfaces.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <param name="surveyedSurfaceId">The Surveyed Surface identifier.</param>
    /// <returns></returns>
    /// 
    Task<ContractExecutionResult> GetDel([FromRoute] Guid projectUid, [FromRoute] long surveyedSurfaceId);

    /// <summary>
    /// Gets a Surveyed Surface list from Raptor.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <returns>Execution result with a list of Surveyed Surfaces.</returns>
    /// 
    SurveyedSurfaceResult Get([FromRoute] long projectId);

    /// <summary>
    /// Gets a Surveyed Surface list from Raptor.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <returns>Execution result with a list of Surveyed Surfaces.</returns>
    /// 
    Task<SurveyedSurfaceResult> Get([FromRoute] Guid projectUid);

    /// <summary>
    /// Updates an existing Surveyed Surface data in a Raptor's list of surveyed surfaces if the target
    /// exists, otherwise - adds a new Surveyed Surface to the list.
    /// </summary>
    /// <param name="request">Description of the Surveyed Surface request.</param>
    /// <returns>Execution result.</returns>
    /// 
    ContractExecutionResult PostPut([FromBody] SurveyedSurfaceRequest request);

    /// <summary>
    /// Removes specified Design File from DesignProfiler cache.
    /// </summary>
    /// <param name="request">Descriptor of the Design File (filename).</param>
    /// <returns>Execution result.</returns>
    ContractExecutionResult PostDelete([FromBody] DesignNameRequest request);
  }


}