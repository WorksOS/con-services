using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories.DBModels;

namespace MockProjectWebApi.Controllers
{
  public class MockGeofenceController : Controller
  {
    private static List<GeofenceData> GeofenceData = null;
    private static List<GeofenceData> Favorites = null;

    public MockGeofenceController()
    {
      InitGeofenceData();
      InitFavorites();
    }

    [Route("api/v1/mock/geofences")]
    [HttpGet]
    public GeofenceDataResult GetMockGeofences(long[] geofenceTypeIds)
    {
      Console.WriteLine($"GetMockGeofences: {JsonConvert.SerializeObject(geofenceTypeIds)}");
      return new GeofenceDataResult { Geofences = GeofenceData };
    }


    [Route("api/v1/mock/geofences/favorite")]
    [HttpGet]
    public GeofenceDataResult GetMockFavoriteGeofences()
    {
      Console.WriteLine("GetMockFavoriteGeofences");
      return new GeofenceDataResult { Geofences = Favorites };
    }

    [Route("api/v1/mock/geofences")]
    [HttpPost]
    public GeofenceCreateResult CreateMockGeofence([FromBody] GeofenceData geofenceData)
    {
      Console.WriteLine("CreateMockGeofence");
      GeofenceData.Add(geofenceData);
      return new GeofenceCreateResult() { geofenceUID = geofenceData.GeofenceUID.ToString() };
    }

    [Route("api/v1/mock/geofences")]
    [HttpPut]
    public OkResult UdpateMockGeofence([FromBody] GeofenceData geofenceData)
    {
      Console.WriteLine("UdpateMockGeofence");
      GeofenceData.Remove(GeofenceData.FirstOrDefault(g => g.GeofenceUID == geofenceData.GeofenceUID));
      GeofenceData.Add(geofenceData);
      return Ok();
    }

    private void InitGeofenceData()
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

    private void InitFavorites()
    {
      if (Favorites == null)
      {
        Favorites = new List<GeofenceData>
        {
          new GeofenceData
          {
            GeofenceName = "Southern Motorway",
            GeofenceUID = Guid.Parse("ffdabc61-7ee9-4054-a3e1-f182dd1abec9"),
            GeometryWKT =
              "POLYGON((172.525733009204 -43.5613699555099,172.527964607104 -43.5572026751871,172.539980903491 -43.5602504159773,172.553370490893 -43.5555232419366,172.571652427539 -43.5466276854031,172.566760078295 -43.542086090904,172.571652427539 -43.5402195830085,172.583067909106 -43.5438281128051,172.594998374804 -43.5441391828477,172.621777549609 -43.5459433574412,172.621949210986 -43.5494271279847,172.611220374926 -43.5504846613456,172.597916618212 -43.548929458806,172.588217750415 -43.5476852678816,172.585556999072 -43.5501114163959,172.568133369311 -43.5580112745029,172.563412681445 -43.5617431307312,172.552254691943 -43.5703255228523,172.544444099292 -43.5696414639818,172.53328610979 -43.567091721564,172.525733009204 -43.5613699555099))",
            FillColor = 16744448,
            IsTransparent = true,
            GeofenceType = GeofenceType.Generic.ToString()
          },
          new GeofenceData
          {
            GeofenceName = "Walnut Creek",
            GeofenceUID = Guid.Parse("09097669-34e7-4b34-b921-680018388505"),
            GeometryWKT = "POLYGON((-96.6672726884766 31.102645603119,-96.6748257890625 31.0986769551086,-96.6792889848633 31.1014697247005,-96.6943951860352 31.0992649134258,-96.7016049638672 31.0975010275558,-96.7031499162598 31.0933851664818,-96.7012616411133 31.0894161315677,-96.6955968156738 31.0850058983233,-96.6897603288574 31.0817715971976,-96.6768857255859 31.0798603675266,-96.6676160112305 31.0766258913085,-96.6602345720215 31.0742734758456,-96.654741407959 31.0738323914664,-96.6495915666504 31.0735383340767,-96.6418668046875 31.0711858422196,-96.6410084978027 31.0692743997297,-96.6339703813477 31.0626575713579,-96.6286488786621 31.0614811981213,-96.5977498308106 31.0619223397898,-96.5987797990723 31.0819186132729,-96.5986081376953 31.1022046504182,-96.6672726884766 31.102645603119))",
            FillColor = 16777011,
            IsTransparent = true,
            GeofenceType = GeofenceType.Generic.ToString()
          },
          new GeofenceData
          {
            GeofenceName = "Ziegler Bloomington",
            GeofenceUID = Guid.Parse("69de1f67-1b2a-413a-8936-659892379fd9"),
            GeometryWKT = "POLYGON((-93.293819224884 44.8334583624534,-93.2899139285583 44.8334887962539,-93.2899139285583 44.8316170876231,-93.2889054179687 44.8316323050115,-93.2887981296082 44.8314344786485,-93.2920382380981 44.8295474853646,-93.2928536296387 44.8308714306678,-93.2930682063599 44.8313431739442,-93.293819224884 44.8334583624534))",
            FillColor = 65535,
            IsTransparent = true,
            GeofenceType = GeofenceType.Generic.ToString()
          }
        };
      }
    }
  }
}
