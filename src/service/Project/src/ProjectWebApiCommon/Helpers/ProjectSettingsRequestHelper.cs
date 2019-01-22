using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  /// <summary>
  /// The request representation for a linear or alignment based profile request for all thematic types other than summary volumes.
  /// Model represents a production data profile
  /// </summary>
  public class ProjectSettingsRequestHelper : DataRequestBase, IProjectSettingsRequestHelper
  {
    public ProjectSettingsRequestHelper()
    { }

    public ProjectSettingsRequestHelper(ILoggerFactory logger)
     {
      log = logger.CreateLogger<ProjectSettingsRequestHelper>();
    }

    /// <summary>
    /// Creates an instance of the ProjectSettingsRequest class and populate it.   
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="settings"></param>
    /// <param name="projectSettingsType"></param>
    /// <returns>An instance of the ProjectSettingsRequest class.</returns>
    public ProjectSettingsRequest CreateProjectSettingsRequest(string projectUid, string settings, ProjectSettingsType projectSettingsType)
    {
      return ProjectSettingsRequest.CreateProjectSettingsRequest(projectUid, settings, projectSettingsType);
    }
  }
}
