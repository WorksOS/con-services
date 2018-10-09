using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.ResultHandling.Abstractions;
using VSS.Productivity3D.WebApi.Models.ProductionData.Models;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Contracts
{
  /// <summary>
  /// Data contract representing an edit applied to production data for a project. 
  /// </summary>
  public interface IEditDataContract
  {
    /// <summary>
    /// Gets a list of edits or overrides of the production data for a project and machine.
    /// </summary>
    /// <param name="request">The request representation for the operation.</param>
    /// <returns>A list of the edits applied to the production data for the project and machine.</returns>
    /// <executor>GetEditDataExecutor</executor> 
    EditDataResult PostEditDataAcquire([FromBody] GetEditDataRequest request);

    /// <summary>
    /// Applies an edit to production data to correct data that has been recorded wrongly in Machines by Operator.
    /// </summary>
    /// <param name="request">The request representation for the operation</param>
    /// <returns></returns>
    /// <executor>EditDataExecutor</executor> 
    ContractExecutionResult Post([FromBody] EditDataRequest request);
  }
}
