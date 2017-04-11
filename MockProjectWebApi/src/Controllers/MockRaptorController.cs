using System.Net;
using MasterDataProxies.Interfaces;
using MasterDataProxies.Models;
using Microsoft.AspNetCore.Mvc;
using MasterDataProxies.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockRaptorController : Controller
  {

    /// <summary>
    /// Dummies the post.
    /// </summary>
    [Route("api/v1/mock/coordsystem/validation")]
    [HttpPost]
    public CoordinateSystemSettings DummyCoordsystemValidationPost([FromBody]CoordinateSystemFileValidationRequest request)
    {
        var cs = CoordinateSystemSettings.CreateCoordinateSystemSettings
        (
          csName: "DatumGrid and Geoid.cal",
          csGroup: "Projection from Data Collector",
          csib: new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 },
          datumName: "Datum Grid",
          siteCalibration: false,
          geoidFileName: "NZ2009.GGF",
          geoidName: "New Zealand Geoid 2009",
          isDatumGrid: true,
          latitudeDatumGridFileName: "NZNATlat.DGF",
          longitudeDatumGridFileName: "NZNATlon.DGF",
          heightDatumGridFileName: null,
          shiftGridName: null,
          snakeGridName: null,
          verticalDatumName: null,
          unsupportedProjection: false
        );
        return cs;
    }

    /// <summary>
    /// Dummies the post.
    /// </summary>
    [Route("api/v1/mock/coordsystem")]
    [HttpPost]
    public CoordinateSystemSettings DummyCoordsystemPost([FromBody]CoordinateSystemFile request)
    {

        var cs = CoordinateSystemSettings.CreateCoordinateSystemSettings
        (
          csName: "DatumGrid and Geoid.cal",
          csGroup: "Projection from Data Collector",
          csib: new byte[] { 0, 1, 2, 3, 4, 5, 6, 7 },
          datumName: "Datum Grid",
          siteCalibration: false,
          geoidFileName: "NZ2009.GGF",
          geoidName: "New Zealand Geoid 2009",
          isDatumGrid: true,
          latitudeDatumGridFileName: "NZNATlat.DGF",
          longitudeDatumGridFileName: "NZNATlon.DGF",
          heightDatumGridFileName: null,
          shiftGridName: null,
          snakeGridName: null,
          verticalDatumName: null,
          unsupportedProjection: false
        );
        return cs;


    }
  }
}
