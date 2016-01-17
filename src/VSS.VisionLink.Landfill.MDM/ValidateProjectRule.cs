using System;
using VSS.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Landfill.MDM.Interfaces;

namespace VSS.VisionLink.Landfill.MDM
{
  public class ValidateProjectRule : IMDMRule<IProjectEvent>
  {
    public IProjectEvent ExecuteRule(IProjectEvent incoming)
    {
      var projectEvent = incoming as CreateProjectEvent;
      if (projectEvent != null)
        if (String.IsNullOrEmpty(projectEvent.ProjectName))
          return null;
      return incoming;
    }
  }
}