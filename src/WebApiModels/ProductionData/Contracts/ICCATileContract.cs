using System;
using Microsoft.AspNetCore.Mvc;

namespace VSS.Productivity3D.WebApi.Models.ProductionData.Contracts
{
  /// <summary>
  ///  Data contract representing CCA tiles of rendered overlays from Raptor
  /// </summary>
  public interface ICCATileContract
  {
    /// <summary>
    /// Gets tiles of rendered overlays for a CCA data set from Raptor.
    /// </summary>
    /// <param name="projectId">Raptor's data model/project identifier.</param>
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="machineName">Raptor's machine name.</param>
    /// <param name="isJohnDoe">IsJohnDoe flag.</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="bbox">Bounding box, as a comma separated string, that represents a WGS84 latitude/longitude coordinate area.</param>    
    /// <param name="width">Width of the requested CCA data tile.</param>
    /// <param name="height">Height of the requested CCA data tile.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <param name="geofenceUid">Geofence boundary unique identifier.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds. If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 (number of cells across a subgrid) * 0.34 (default width in meters of a single cell)</returns>
    /// 
    FileResult Get([FromQuery] long projectId,
                   [FromQuery] long assetId,
                   [FromQuery] string machineName,
                   [FromQuery] bool isJohnDoe,
                   [FromQuery] DateTime startUtc,
                   [FromQuery] DateTime endUtc,
                   [FromQuery] string bbox,
                   [FromQuery] ushort width,
                   [FromQuery] ushort height,
                   [FromQuery] int? liftId = null,
                   [FromQuery] Guid? geofenceUid = null);

    /// <summary>
    /// Gets tiles of rendered overlays for a CCA data set from Raptor.
    /// </summary>
    /// <param name="projectUid">Raptor's data model/project unique identifier.</param>
    /// <param name="assetId">Raptor's machine identifier.</param>
    /// <param name="machineName">Raptor's machine name.</param>
    /// <param name="isJohnDoe">IsJohnDoe flag.</param>
    /// <param name="startUtc">Start date of the requeted CCA data in UTC.</param>
    /// <param name="endUtc">End date of the requested CCA data in UTC.</param>
    /// <param name="bbox">Bounding box, as a comma separated string, that represents a WGS84 latitude/longitude coordinate area.</param>    
    /// <param name="width">Width of the requested CCA data tile.</param>
    /// <param name="height">Height of the requested CCA data tile.</param>
    /// <param name="liftId">Lift identifier of the requested CCA data.</param>
    /// <param name="geofenceUid">Geofence boundary unique identifier.</param>
    /// <returns>An HTTP response containing an error code is there is a failure, or a PNG image if the request suceeds. If the size of a pixel in the rendered tile coveres more than 10.88 meters in width or height, then the pixel will be rendered in a 'representational style' where black (currently, but there is a work item to allow this to be configurable) is used to indicate the presense of data. Representational style rendering performs no filtering what so ever on the data.10.88 meters is 32 (number of cells across a subgrid) * 0.34 (default width in meters of a single cell)</returns>
    /// 
    FileResult Get([FromQuery] Guid projectUid,
                   [FromQuery] long assetId,
                   [FromQuery] string machineName,
                   [FromQuery] bool isJohnDoe,
                   [FromQuery] DateTime startUtc,
                   [FromQuery] DateTime endUtc,
                   [FromQuery] string bbox,
                   [FromQuery] ushort width,
                   [FromQuery] ushort height,
                   [FromQuery] int? liftId = null,
                   [FromQuery] Guid? geofenceUid = null);
  }
}