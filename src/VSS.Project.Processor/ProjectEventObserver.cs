using VSS.Geofence.Data.Interfaces;
using VSS.Landfill.Common.JsonConverters;
using VSS.Landfill.Common.Processor;
using VSS.Project.Data.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

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
      int updatedCount = _projectService.StoreProject(evt, _geofenceService);
      return updatedCount == 1;    
    }

  }
}
