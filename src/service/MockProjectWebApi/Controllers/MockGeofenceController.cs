using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Services;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockGeofenceController : Controller
  {
    private readonly GeofenceService geofenceService;

    public MockGeofenceController(IGeofenceservice geofenceService)
    {
      this.geofenceService = (GeofenceService) geofenceService;
    }

    [Route("api/v1/mock/geofences")]
    [HttpGet]
    public GeofenceDataResult GetMockGeofences(long[] geofenceTypeIds)
    {
      Console.WriteLine($"GetMockGeofences: {JsonConvert.SerializeObject(geofenceTypeIds)}");
      var allGeofences = new List<GeofenceData>();
      allGeofences.AddRange(geofenceService.Standard);
      allGeofences.AddRange(geofenceService.Favorites);
      allGeofences.AddRange(geofenceService.Associated);
      return new GeofenceDataResult { Geofences = allGeofences.Distinct().ToList() };
    }

    [Route("api/v1/mock/geofences/favorite")]
    [HttpGet]
    public GeofenceDataResult GetMockFavoriteGeofences()
    {
      Console.WriteLine("GetMockFavoriteGeofences");
      return new GeofenceDataResult { Geofences = geofenceService.Favorites };
    }

    [Route("api/v1/mock/geofences")]
    [HttpPost]
    public GeofenceCreateResult CreateMockGeofence([FromBody] GeofenceData geofenceData)
    {
      Console.WriteLine("CreateMockGeofence");
      //GeofenceData.Add(geofenceData);
      return new GeofenceCreateResult() { geofenceUID = geofenceData.GeofenceUID.ToString() };
    }

    [Route("api/v1/mock/geofences")]
    [HttpPut]
    public OkResult UdpateMockGeofence([FromBody] GeofenceData geofenceData)
    {
      Console.WriteLine("UdpateMockGeofence");
      //GeofenceData.Remove(GeofenceData.FirstOrDefault(g => g.GeofenceUID == geofenceData.GeofenceUID));
      //GeofenceData.Add(geofenceData);
      return Ok();
    }
  }
}
