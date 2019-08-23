using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MockProjectWebApi.Services;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockUnifiedProductivityController : BaseController
  {
    private static List<GeofenceWithTargetsData> AssociatedGeofenceData;
    private readonly GeofenceService GeofenceService;
    
    public MockUnifiedProductivityController(ILoggerFactory loggerFactory, IGeofenceservice geofenceService)
    : base(loggerFactory)
    {
      GeofenceService = (GeofenceService)geofenceService;
      InitAssociatedGeofenceData();
    }

    [Route("api/v1/composite/projects/{projectUid}/sitewithtargets/asgeofence")]
    [HttpGet]
    public GeofenceWithTargetsResult GetMockAssociatedGeofences([FromRoute] Guid projectUid)
    {
      Logger.LogInformation($"GetMockAssociatedGeofences: {projectUid}");
      return new GeofenceWithTargetsResult { Results = AssociatedGeofenceData };
    }

    private void InitAssociatedGeofenceData()
    {
      if (AssociatedGeofenceData == null)
      {
        AssociatedGeofenceData = new List<GeofenceWithTargetsData>();
        foreach (var geofence in GeofenceService.Associated)
        {
          AssociatedGeofenceData.Add(new GeofenceWithTargetsData { Geofence = geofence });
        }
      }
    }
  }
}
