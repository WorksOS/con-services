using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VSS.Productivity3D.Models.Enums;
using VSS.TRex.CellDatum.GridFabric.Arguments;
using VSS.TRex.CellDatum.GridFabric.Requests;
using VSS.TRex.Filters;
using VSS.TRex.Geometry;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api")]
  public class CellController : Controller
  {
    /// <summary>
    /// Gets cell datum using grid coordinates
    /// </summary>
    [HttpGet("cells/datum")]
    public async Task<JsonResult> GetCellDatum(
      [FromQuery] Guid projectUid,
      [FromQuery] Guid? designUid,
      [FromQuery] int displayMode,
      [FromQuery] double x,
      [FromQuery] double y)
    { 
      var cellDatumRequest = new CellDatumRequest_ApplicationService();
      var response = cellDatumRequest.Execute(new CellDatumRequestArgument_ApplicationService
      {
        ProjectID = projectUid,
        Filters = new FilterSet(new CombinedFilter()),
        Mode = (DisplayMode) displayMode,
        CoordsAreGrid = true,
        Point = new XYZ(x, y),
        ReferenceDesign.DesignID = designUid ?? Guid.Empty
      });

      var result = new
      {
        displayMode = response.DisplayMode,
        returnCode = response.ReturnCode,
        value  = response.Value,
        timestamp = response.TimeStampUTC,
        northing  = response.Northing,
        easting = response.Easting
      };
    
      return new JsonResult(result);
    }
  }
}
