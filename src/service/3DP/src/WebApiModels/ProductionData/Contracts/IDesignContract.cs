using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.ResultHandling.Abstractions;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Contracts
{
  /// <summary>
  /// Data contract representing designs...
  /// </summary>
  /// 
  public interface IDesignContract
  {
    /// <summary>
    /// Gets a list of design boundaries in GeoJson format from Raptor.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <param name="tolerance">The spacing interval for the sampled points. Setting to 1.0 will cause points to be spaced 1.0 meters apart.</param>
    /// <returns>Execution result with a list of design boundaries.</returns>
    /// 
    Task<ContractExecutionResult> GetDesignBoundaries([FromQuery] Guid projectUid, [FromQuery] double? tolerance);
  }
}
