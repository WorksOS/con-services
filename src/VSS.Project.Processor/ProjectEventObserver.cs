using ikvm.extensions;
using VSS.Geofence.Data.Interfaces;
using VSS.Landfill.Common.JsonConverters;
using VSS.Landfill.Common.Processor;
using VSS.Project.Data.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Project.Processor
{
  public class ProjectEventObserver : EventObserverBase<IProjectEvent, ProjectEventConverter>
  {
    private IProjectService _projectService;
    private IGeofenceService _geofenceService;

    public ProjectEventObserver(IProjectService projectService, IGeofenceService geofenceService)
    {
      _projectService = projectService;
      _geofenceService = geofenceService;
      EventName = "Project";
    }

    protected override bool ProcessEvent(IProjectEvent evt)
    {
      int updatedCount = _projectService.StoreProject(evt);

      if (evt is AssociateProjectCustomer)
      {
        //Now we have the customerUID, check for geofence for this project 
        //and if it exists and is unassigned then assign it to this project
        //and also assign relevant unassigned Landfill geofences.
        var associateEvent = evt as AssociateProjectCustomer;
        var project = _projectService.GetProject(associateEvent.ProjectUID.toString());
        var geofence = _geofenceService.GetGeofenceByName(project.CustomerUID, project.Name);
        if (geofence != null && string.IsNullOrEmpty(geofence.ProjectUID))
        {
          int result = _geofenceService.AssignGeofenceToProject(geofence.GeofenceUID, project.ProjectUID);
          if (result > 0)
          {
            _geofenceService.AssignApplicableLandfillGeofencesToProject(geofence.GeometryWKT, geofence.CustomerUID, geofence.ProjectUID);
          }
        }

      }
      return updatedCount == 1;

    }

  }
}
