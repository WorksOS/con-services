
using VSS.MasterData.Common.JsonConverters;
using VSS.MasterData.Common.Processor;
using VSS.Project.Data.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Project.Processor
{
  public class ProjectEventObserver : EventObserverBase<IProjectEvent, ProjectEventConverter>
  {
    private IProjectService _projectService;

    public ProjectEventObserver(IProjectService projectService)
    {
      _projectService = projectService;
      EventName = "Project";
    }

    protected override bool ProcessEvent(IProjectEvent evt)
    {
      int updatedCount = _projectService.StoreProject(evt);
      return updatedCount == 1;
    }

 

  }
}
