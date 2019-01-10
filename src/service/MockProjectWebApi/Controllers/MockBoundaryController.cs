using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Utils;
using System;
using System.Collections.Generic;
using VSS.MasterData.Models.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockBoundaryController : Controller
  {
    [Route("api/v1/mock/boundaries/{projectUid}")]//must match same base route as mock filters and same route suffix as real api
    [HttpGet]
    public GeofenceListData GetMockBoundary(string projectUid)
    {
      Console.WriteLine($"GetMockBoundariesForProject: projectUid={projectUid}");

      return new GeofenceListData {GeofenceData = GetMockBoundaries(projectUid)};
    }

    private List<GeofenceData> GetMockBoundaries(string projectUid)
    {
      switch (projectUid)
      {
        case ConstantsUtil.GOLDEN_DATA_DIMENSIONS_PROJECT_UID_1:
        case ConstantsUtil.DIMENSIONS_PROJECT_UID:
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
      },
      new GeofenceData
      {
        GeofenceName = "Dimensions lower right",
        GeofenceUID = Guid.Parse("7f2fb9ec-2384-420e-b2e3-72b9cea939a3"),
        GeometryWKT = "POLYGON((-115.018690 36.206897,-115.018701 36.206795,-115.018264 36.206823,-115.018280 36.206977,-115.018690 36.206897))"
      },
      new GeofenceData
      {
        GeofenceName = "Dimensions boundary machine filter",
        GeofenceUID = Guid.Parse("ca9c91c3-513b-4082-b2d7-0568899e56d5"),
        GeometryWKT = "POLYGON((-115.018480 36.207118,-115.018394 36.207334,-115.019604 36.207492,-115.019641 36.207215,-115.018480 36.207118))"
      },
      new GeofenceData
      {
        GeofenceName = "Dimensions boundary mdp",
        GeofenceUID = Guid.Parse("318f0103-a0c3-4b50-88d4-d4fa12370a63"),
        GeometryWKT = "POLYGON((-115.018943 36.207659,-115.018926 36.207265,-115.018471 36.207412,-115.018943 36.207659))"
      },
      new GeofenceData
      {
        GeofenceName = "Dimensions boundary CMV",
        GeofenceUID = Guid.Parse("c910d127-5e3c-453f-82c3-e235848ac20e"),
        GeometryWKT = "POLYGON((-115.020509 36.207183,-115.020187 36.206862,-115.019731 36.207174,-115.020509 36.207183))"
      },
    };
  }
}
