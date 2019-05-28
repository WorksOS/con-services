using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using MockProjectWebApi.Services;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;

namespace MockProjectWebApi.Controllers
{
  public class MockUnifiedProductivityController : Controller
  {
    private static List<GeofenceWithTargetsData> AssociatedGeofenceData = null;
    private readonly GeofenceService geofenceService;


    public MockUnifiedProductivityController(IGeofenceservice geofenceService)
    {
      this.geofenceService = (GeofenceService)geofenceService;
      InitAssociatedGeofenceData();
    }

    [Route("api/v1/composite/projects/{projectUid}/sitewithtargets/asgeofence")]
    [HttpGet]
    public GeofenceWithTargetsResult GetMockAssociatedGeofences([FromRoute] Guid projectUid)
    {
      Console.WriteLine($"GetMockAssociatedGeofences: {projectUid}");
      return new GeofenceWithTargetsResult { Results = AssociatedGeofenceData };
    }

    private void InitAssociatedGeofenceData()
    {
      if (AssociatedGeofenceData == null)
      {
        AssociatedGeofenceData = new List<GeofenceWithTargetsData>();
        foreach (var geofence in geofenceService.Associated)
        {
          AssociatedGeofenceData.Add(new GeofenceWithTargetsData { Geofence = geofence});
        }  
      }
    }

  }
}
