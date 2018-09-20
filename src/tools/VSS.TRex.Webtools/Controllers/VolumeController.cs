using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.TRex.DI;
using VSS.TRex.Filters;
using VSS.TRex.Gateway.Common.ResultHandling;
using VSS.TRex.Geometry;
using VSS.TRex.Logging;
using VSS.TRex.Rendering.GridFabric.Arguments;
using VSS.TRex.Rendering.GridFabric.Requests;
using VSS.TRex.Rendering.Implementations.Core2.GridFabric.Responses;
using VSS.TRex.SiteModels.Interfaces;
using VSS.TRex.Types;
using VSS.TRex.Volumes;
using VSS.TRex.Volumes.GridFabric.Arguments;
using VSS.TRex.Volumes.GridFabric.Requests;

namespace VSS.TRex.Webtools.Controllers
{
  [Route("api/volumes")]
  public class VolumeController : Controller
  {
    private static readonly ILogger Log = Logging.Logger.CreateLogger(MethodBase.GetCurrentMethod().DeclaringType.Name);

    /// <summary>
    /// Generates a tile...
    /// </summary>
    /// <param name="siteModelID">Grid to return status for</param>
    /// <returns></returns>
    [HttpGet("{siteModelID}")]
    public JsonResult GetVolume(string siteModelID,
    [FromQuery] string filter)
    {
      var filterJson = Encoding.ASCII.GetString(Convert.FromBase64String(filter));

      var baseFilter = JsonConvert.DeserializeObject<CombinedFilter>(filterJson);
      baseFilter.AttributeFilter.ReturnEarliestFilteredCellPass = true;
      baseFilter.SpatialFilter.Fence?.UpdateExtents();
      var topFilter = JsonConvert.DeserializeObject<CombinedFilter>(filterJson);
      topFilter.SpatialFilter.Fence?.UpdateExtents();

      var request = new SimpleVolumesRequest_ApplicationService();
       var response = request.Execute(new SimpleVolumesRequestArgument
       {
         SiteModelID = Guid.Parse(siteModelID),
         VolumeType = VolumeComputationType.Between2Filters,
         BaseFilter = baseFilter,
         TopFilter = topFilter
      });

      return new JsonResult(response);
    }
  }
}
