using System.Collections.Generic;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;

namespace VSS.Project.Data.Interfaces
{
	public interface IProjectService
	{
    Models.Project GetProject(string projectUid);
    int StoreProject(IProjectEvent evt);
	  IEnumerable<Models.Project> GetProjectsForUser(string userUid);
    IEnumerable<Models.Project> GetProjects();
    IEnumerable<Models.Project> GetProjectsBySubcription(string subscriptionUid);
	  string GetProjectUidForName(string customerUid, string name);

	}
}
