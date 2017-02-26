using System;
using Microsoft.AspNetCore.Mvc;
using VSS.Raptor.Service.WebApiModels.Coord.Models;
using VSS.Raptor.Service.WebApiModels.Coord.ResultHandling;

namespace VSS.Raptor.Service.WebApiModels.Coord.Contracts
{
    /// <summary>
    /// Data contract representing a coordinate system definition...
    /// </summary>
    public interface ICoordinateSystemFileContract
	  {
    /// <summary>
    /// Posts Coordinate System settings to a Raptor's data model.
    /// </summary>
    /// <param name="request">>Description of the Coordinate System settings request.</param>
    /// <returns>Execution result with Coordinate System settings.</returns>
    /// 
    CoordinateSystemSettings Post([FromBody]CoordinateSystemFile request);
    
    /// <summary>
    /// Gets Coordinate System settings from a Raptor's data model.
    /// </summary>
    /// <param name="projectId">The model/project identifier.</param>
    /// <returns>Execution result with Coordinate System settings.</returns>
    /// 
    CoordinateSystemSettings Get([FromRoute] long projectId);
    
    /// <summary>
    /// Gets Coordinate System settings from a Raptor's data model with a unique identifier.
    /// </summary>
    /// <param name="projectUid">The model/project unique identifier.</param>
    /// <returns>Execution result with Coordinate System settings.</returns>
    /// 
    CoordinateSystemSettings Get([FromRoute] Guid projectUid);

    /// <summary>
    /// Posts a list of coordinates to a Raptor's data model for conversion.
    /// </summary>
    /// <param name="request">>Description of the coordinate conversion request.</param>
    /// <returns>Execution result with a list of converted coordinates.</returns>
    /// 
    CoordinateConversionResult Post([FromBody]CoordinateConversionRequest request);
  }
}