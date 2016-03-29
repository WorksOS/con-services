using System.Collections.Generic;
using System.Linq;
using Dapper;
using VSS.VisionLink.Landfill.Common.Interfaces;
using VSS.VisionLink.Landfill.Common.Models;

namespace VSS.VisionLink.Landfill.Repositories
{

  public class ProjectRepository : RepositoryBase, IProjectRepository
  {
    public ProjectRepository(string connectionString)
      : base(connectionString)
    {
    }

    public IEnumerable<Project> GetProjectsBySubcription(string subscriptionUid)
    {
      PerhapsOpenConnection();
      var projects = Connection.Query<Project>
        (@"SELECT 
                 projectUid, name, projectId, timeZone, customerUid, subscriptionUid, 
                  daysToSubscriptionExpiry, lastActionedUtc, IsDeleted
              FROM projects
              WHERE subscriptionUid = @subscriptionUid AND IsDeleted=0"
        );
      PerhapsCloseConnection();
      return projects;
    }

    public IEnumerable<Project> GetProjects()
    {
      PerhapsOpenConnection();
      var project = Connection.Query<Project>
        (@"SELECT 
                 projectUid, name, projectId, timeZone, customerUid, subscriptionUid, 
                  daysToSubscriptionExpiry, lastActionedUtc, IsDeleted, projectStartDate, projectEndDate, projectType
              FROM projects
              WHERE  IsDeleted=0");
      PerhapsCloseConnection();
      return project;
    }
  }
}