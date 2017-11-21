using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using VSS.MasterData.Models.Models;

namespace MockProjectWebApi.Controllers
{
  public class MockGeofenceController : Controller
  {
    [Route("api/v1/mock/geofences")]
    [HttpGet]
    public List<GeofenceData> GetMockGeofences()
    {
      Console.WriteLine("GetMockGeofences");
      var geofences = new List<GeofenceData>
      {
        //Copied from MockBoundaryController
        new GeofenceData
        {
          GeofenceName = "Dimensions boundary CMV",
          GeofenceUID = Guid.Parse("c910d127-5e3c-453f-82c3-e235848ac20e"),
          GeometryWKT = "POLYGON((-115.020509 36.207183,-115.020187 36.206862,-115.019731 36.207174,-115.020509 36.207183))"         
        }
      };
      return geofences;
    }
  }
}
