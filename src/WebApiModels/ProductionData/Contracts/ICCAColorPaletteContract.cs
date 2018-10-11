using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.WebApi.Models.ProductionData.ResultHandling;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Contracts
{
  /// <summary>
  /// The data contract represents CCA data colour palettes requested from Raptor for a single machene.
  /// </summary>
  /// 
  public interface ICCAColorPaletteContract
  {
    /// <summary>
    /// Gets CCA data colour palette requested from Raptor with a project identifier.
    /// </summary>
    /// <param name="projectId">Raptor's data model/project identifier.</param>
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <returns>Execution result with a list of CCA data colour palettes.</returns>
    CCAColorPaletteResult Get([FromQuery] long projectId,
                              [FromQuery] long assetId,
                              [FromQuery] DateTime? startUtc = null,
                              [FromQuery] DateTime? endUtc = null,
                              [FromQuery] int? liftId = null);

    /// <summary>
    /// Gets CCA data colour palette requested from Raptor with a project unique identifier.
    /// </summary>
    /// <param name="projectUid">Raptor's data model/project unique identifier.</param>
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <returns>Execution result with a list of CCA data colour palettes.</returns>
    Task<CCAColorPaletteResult> Get([FromQuery] Guid projectUid,
                              [FromQuery] long assetId,
                              [FromQuery] DateTime? startUtc = null,
                              [FromQuery] DateTime? endUtc = null,
                              [FromQuery] int? liftId = null);
  }
}
