using System;
using System.Collections.Generic;
using ClipperLib;
using ikvm.extensions;
using VSS.Geofence.Data.Interfaces;
using VSS.Geofence.Data.Models;
using VSS.Landfill.Common.Helpers;
using VSS.Landfill.Common.JsonConverters;
using VSS.Landfill.Common.Processor;
using VSS.Project.Data.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Geofence.Processor
{
  public class GeofenceEventObserver : EventObserverBase<IGeofenceEvent, GeofenceEventConverter>
  {
    private IGeofenceService _geofenceService;
    private IProjectService _projectService;

    public GeofenceEventObserver(IGeofenceService geofenceService)
    {
      _geofenceService = geofenceService;
      EventName = "Geofence";
    }

    protected override bool ProcessEvent(IGeofenceEvent evt)
    {
      int updatedCount = _geofenceService.StoreGeofence(evt);
      if (updatedCount > 0)
      {
        if (evt is CreateGeofenceEvent)
        {
          var createEvent = evt as CreateGeofenceEvent;
          string projectUid = null;
          GeofenceType geofenceType = _geofenceService.GetGeofenceType(evt);

          if (geofenceType == GeofenceType.Landfill)
          {
            projectUid = FindAssociatedProjectUidForLandfillGeofence(createEvent.CustomerUID.toString(), createEvent.GeometryWKT);
          }
          else if (geofenceType == GeofenceType.Project)
          {
            projectUid = _projectService.GetProjectUidForName(createEvent.CustomerUID.toString(), createEvent.GeofenceName);
          }

          if (!string.IsNullOrEmpty(projectUid))
          {
            _geofenceService.AssignGeofenceToProject(createEvent.GeofenceUID.toString(), projectUid);
            if (geofenceType == GeofenceType.Project)
            {
              _geofenceService.AssignApplicableLandfillGeofencesToProject(createEvent.GeometryWKT, createEvent.CustomerUID.toString(), projectUid);
            }
          }
        }
      }
      return updatedCount == 1;    
    }

    private string FindAssociatedProjectUidForLandfillGeofence(string customerUid, string geofenceGeometry)
    {
      List<IntPoint> geofencePolygon = Geometry.ClipperPolygon(geofenceGeometry);
      IEnumerable<Data.Models.Geofence> projectGeofences = _geofenceService.GetProjectGeofences(customerUid);
      foreach (var projectGeofence in projectGeofences)
      {
        if (Geometry.GeofencesOverlap(projectGeofence.GeometryWKT, geofencePolygon))
        {
          if (string.IsNullOrEmpty(projectGeofence.ProjectUID))
          {
            projectGeofence.ProjectUID = _projectService.GetProjectUidForName(customerUid, projectGeofence.Name);
            if (!string.IsNullOrEmpty(projectGeofence.ProjectUID))
            {
              _geofenceService.AssignGeofenceToProject(projectGeofence.GeofenceUID, projectGeofence.ProjectUID);
            }
          }
          return projectGeofence.ProjectUID;
        }
      }
      return null;
    }

  }
}
