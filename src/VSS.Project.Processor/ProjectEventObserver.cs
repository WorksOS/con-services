using System.Linq;
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

      //Old way of associating through name match
      if (evt is AssociateProjectCustomer)
      {
        //Now we have the customerUID, check for geofence for this project 
        //and if it exists and is unassigned then assign it to this project
        //and also assign relevant unassigned Landfill geofences.
        var associateEvent = evt as AssociateProjectCustomer;
        string projectUID = associateEvent.ProjectUID.ToString();
        var project = _projectService.GetProject(associateEvent.ProjectUID.ToString());        
        var geofences = _geofenceService.GetProjectGeofences(project.CustomerUID);
        var geofence = (from g in geofences where g.Name == project.Name select g).FirstOrDefault();
        AssignGeofencesToProject(geofence, projectUID);
      }
      //New way of associating through event
      else if (evt is AssociateProjectGeofence)
      {
        updatedCount = 1; //StoreProject does nothing; handling event here

        var associateEvent = evt as AssociateProjectGeofence;
        string projectUID = associateEvent.ProjectUID.ToString();
        Geofence.Data.Models.Geofence geofence = _geofenceService.GetGeofence(associateEvent.GeofenceUID.ToString());
        AssignGeofencesToProject(geofence, projectUID);
      }
      return updatedCount == 1;
    }

    private void AssignGeofencesToProject(Geofence.Data.Models.Geofence geofence, string projectUID)
    {
      if (geofence != null)
      {
        if (string.IsNullOrEmpty(geofence.ProjectUID))
        {
          //Assign project geofence to project
          int result = _geofenceService.AssignGeofenceToProject(geofence.GeofenceUID, projectUID);
          if (result > 0)
          {
            //Assign landfill geofences to project where applicable
            _geofenceService.AssignApplicableLandfillGeofencesToProject(geofence.GeometryWKT, geofence.CustomerUID,
                geofence.ProjectUID);
          }
        }
        else if (projectUID != geofence.ProjectUID)
        {
          Log.WarnFormat("ProjectEventObserver: Mismatch of assigned project for geofence {0}, {1}", projectUID, geofence.ProjectUID);
        }
      }
    }

  }
}
