using System.Collections.Generic;

namespace VSS.Project.Data.Interfaces
{
	public interface IProjectService
	{
    Models.Project GetProject(string projectUid);
    int StoreProject(IProjectEvent evt);
    IEnumerable<Models.Project> GetProjects();
    IEnumerable<Models.Project> GetProjectsBySubcription(string subscriptionUid);
	}
}
