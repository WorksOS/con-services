using System;
using VSS.MasterData.Project.WebAPI.Common.Models;

namespace VSS.MasterData.Project.WebAPI.Common.Helpers
{
  public interface IProjectSettingsRequestHelper
  {
    ProjectSettingsRequest CreateProjectSettingsRequest(string projectUid, string settings);
  }
}
