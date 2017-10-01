using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Utils;
using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockBoundaryController : Controller
  {
    [Route("api/v4/mockboundary/{projectUid}")]
    [HttpGet]
    public IEnumerable<GeofenceData> GetMockFilter(string projectUid)
    {
      Console.WriteLine("GetMockBoundariesForProject: projectUid={0}");

      return GetFilters(projectUid);
    }

    private IEnumerable<GeofenceData> GetFilters(string projectUid)
    {
      switch (projectUid)
      {
        case ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1:
          {
            return _goldenDataProjectFilterBoundaryList;
          }
        default:
          return null;
      }
    }

    /// <summary>
    /// <see cref="GeofenceData"/> objects don't know about their Filter object or Project so to mock this we need separate collections per project.
    /// </summary>
    private readonly List<GeofenceData> _goldenDataProjectFilterBoundaryList = new List<GeofenceData>
    {
      new GeofenceData
      {
        GeofenceName = null,
        GeofenceUID = Guid.Parse("94dc0ec6-32ef-4b54-a7f8-e8e3eee65642"),
        GeometryWKT = "POLYGON((-121.347189366818 38.8361907402694,-121.349260032177 38.8361656688414,-121.349217116833 38.8387897637231,-121.347275197506 38.8387145521594,-121.347189366818 38.8361907402694,-121.347189366818 38.8361907402694))"
      }
    };
  }
}