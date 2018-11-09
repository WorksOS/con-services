using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockGeofenceController : Controller
  {
    private static List<GeofenceData> GeofenceData = null;

    public MockGeofenceController()
    {
      if (GeofenceData == null)
      {
        GeofenceData = new List<GeofenceData>
        {
          //Copied from MockBoundaryController
          new GeofenceData
          {
            GeofenceName = "Dimensions boundary CMV",
            GeofenceUID = Guid.Parse("c910d127-5e3c-453f-82c3-e235848ac20e"),
            GeometryWKT =
              "POLYGON((-115.020509 36.207183,-115.020187 36.206862,-115.019731 36.207174,-115.020509 36.207183))"
          },
          new GeofenceData
          {
            GeofenceName = "Inside Dimensions project",
            GeofenceUID = Guid.Parse("d4edddc9-d07f-4d56-ad50-5e9671631f70"),
            GeometryWKT = "POLYGON((-115.020 36.207,-115.021 36.2075,-115.023 36.208,-115.020 36.207))",
            FillColor = 16711680, //red
            IsTransparent = false
          },
          new GeofenceData
          {
            GeofenceName = "Dimensions project boundary",
            GeofenceUID = Guid.Parse("eee23e91-5682-45ec-a4a7-9dfe0d6b7a64"),
            GeometryWKT = "POLYGON((-115.025723657623 36.2101347890754,-115.026281557098 36.2056332151707,-115.018041811005 36.205460072542,-115.017698488251 36.2102040420362, -115.025723657623 36.2101347890754))",
            FillColor = 65280, //green
            IsTransparent = false
          },
          new GeofenceData
          {
            GeofenceName = "Zero area boundary ie a point",
            GeofenceUID = Guid.Parse("ba35221d-cc46-48ce-970c-8b1509a0c737"),
            GeometryWKT = "POLYGON((6.94613249999989 24.95555531306,6.94613249999989 24.95555531306,6.94613249999989 24.95555531306,6.94613249999989 24.95555531306,6.94613249999989 24.95555531306))",
            FillColor = 16711680, //red
            IsTransparent = false
          }
        };
      }
    }

    [Route("api/v1/mock/geofences")]
    [HttpGet]
    public GeofenceDataResult GetMockGeofences(long[] geofenceTypeIds)
    {
      Console.WriteLine($"GetMockGeofences: {JsonConvert.SerializeObject(geofenceTypeIds)}");
      var geofences = GeofenceData;
      return new GeofenceDataResult { Geofences = geofences };
    }

    [Route("api/v1/mock/geofences")]
    [HttpPost]
    public GeofenceCreateResult CreateMockGeofence([FromBody] GeofenceData geofenceData)
    {
      Console.WriteLine("GetMockGeofences");
      GeofenceData.Add(geofenceData);
      return new GeofenceCreateResult(){geofenceUID = geofenceData.GeofenceUID.ToString()};
    }

    [Route("api/v1/mock/geofences")]
    [HttpPut]
    public OkResult UdpateMockGeofence([FromBody] GeofenceData geofenceData)
    {
      Console.WriteLine("GetMockGeofences");
      GeofenceData.Remove(GeofenceData.FirstOrDefault(g => g.GeofenceUID == geofenceData.GeofenceUID));
      GeofenceData.Add(geofenceData);
      return Ok();
    }
  }
}
