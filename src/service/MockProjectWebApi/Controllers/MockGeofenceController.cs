using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Services;
using Newtonsoft.Json;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockGeofenceController : BaseController
  {
    private readonly GeofenceService geofenceService;

    public MockGeofenceController(ILoggerFactory loggerFactory, IGeofenceservice geofenceService)
      : base(loggerFactory)
    {
      this.geofenceService = (GeofenceService)geofenceService;
    }

    [Route("api/v1/mock/geofences")]
    [Route("api/v1")]
    [HttpGet]
    public GeofenceDataResult GetMockGeofences(long[] geofenceTypeIds)
    {
      Logger.LogInformation($"GetMockGeofences: {JsonConvert.SerializeObject(geofenceTypeIds)}");

      var allGeofences = new List<GeofenceData>();
      allGeofences.AddRange(geofenceService.Standard);
      allGeofences.AddRange(geofenceService.Favorites);
      allGeofences.AddRange(geofenceService.Associated);

      return new GeofenceDataResult { Geofences = allGeofences.Distinct().ToList() };
    }

    [Route("api/v1/mock/geofences/favorite")]
    [Route("api/v1/favorite")]
    [HttpGet]
    public GeofenceDataResult GetMockFavoriteGeofences()
    {
      Logger.LogInformation("GetMockFavoriteGeofences");
      return new GeofenceDataResult { Geofences = geofenceService.Favorites };
    }

    [Route("api/v1/mock/geofences")]
    [Route("api/v1")]
    [HttpPost]
    public GeofenceCreateResult CreateMockGeofence([FromBody] GeofenceData geofenceData)
    {
      Logger.LogInformation("CreateMockGeofence");
      return new GeofenceCreateResult { geofenceUID = geofenceData.GeofenceUID.ToString() };
    }

    [Route("api/v1/mock/geofences")]
    [Route("api/v1")]
    [HttpPut]
    public OkResult UdpateMockGeofence([FromBody] GeofenceData geofenceData)
    {
      Logger.LogInformation("UdpateMockGeofence");

      return Ok();
    }
  }
}
