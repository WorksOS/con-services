using System;
using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Project.Data.Interfaces
{
	public interface IProjectService
	{
    int StoreProject(IProjectEvent evt);
    Models.Project GetProject(string projectUid);
    IEnumerable<Models.Project> GetProjects();
    IEnumerable<Models.Project> GetProjectsBySubcription(string subscriptionUid);
    IEnumerable<Models.Project> GetProjectsForUser(string userUid);
    IEnumerable<Models.Project> GetLandfillProjectsForUser(string userUid);
	}
}
