using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using KafkaNet.Common;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using VSS.VisionLink.Landfill.Common.Interfaces;
using VSS.VisionLink.Landfill.Common.Models;

namespace VSS.VisionLink.Landfill.Repositories
{

  public class ProjectRepository : RepositoryBase, IProjectRepository
  {
    private static readonly AsyncLock Locker = new AsyncLock();

    public ProjectRepository(string connectionString)
      : base(connectionString)
    {
    }


    public async Task<int> StoreProject(IProjectEvent evt)
    {
      var upsertedCount = 0;
      var project = new Project();
      string eventType = "Unknown"; 
      if (evt is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent)evt;
        project.projectId = projectEvent.ProjectID;
        project.name = projectEvent.ProjectName;
        project.timeZone = projectEvent.ProjectTimezone;
        project.projectUid = projectEvent.ProjectUID.ToString();
        project.projectEndDate = projectEvent.ProjectEndDate;
        project.lastActionedUtc = projectEvent.ActionUTC;
        project.projectStartDate = projectEvent.ProjectStartDate;
        project.projectType = projectEvent.ProjectType;
        eventType = "CreateProjectEvent";
      }
      else if (evt is UpdateProjectEvent)
      {
        var projectEvent = (UpdateProjectEvent)evt;
        project.projectUid = projectEvent.ProjectUID.ToString();
        project.name = projectEvent.ProjectName;
        project.projectEndDate = projectEvent.ProjectEndDate;
        project.lastActionedUtc = projectEvent.ActionUTC;
        project.projectType = projectEvent.ProjectType;
        eventType = "UpdateProjectEvent";
      }
      else if (evt is DeleteProjectEvent)
      {
        var projectEvent = (DeleteProjectEvent)evt;
        project.projectUid = projectEvent.ProjectUID.ToString();
        project.lastActionedUtc = projectEvent.ActionUTC;
        eventType = "DeleteProjectEvent";
      }
      else if (evt is AssociateProjectCustomer)
      {
        var projectEvent = (AssociateProjectCustomer)evt;
        project.projectUid = projectEvent.ProjectUID.ToString();
        project.customerUid = projectEvent.CustomerUID.ToString();
        project.lastActionedUtc = projectEvent.ActionUTC;
        eventType = "AssociateProjectCustomerEvent";
      }
      else if (evt is DissociateProjectCustomer)
      {
        //TODO Do we realy need to support this?
        var projectEvent = (DissociateProjectCustomer)evt;
        project.projectUid = projectEvent.ProjectUID.ToString();
        project.customerUid = projectEvent.CustomerUID.ToString();
        project.lastActionedUtc = projectEvent.ActionUTC;
        eventType = "DissociateProjectCustomerEvent";
      }
      
      upsertedCount = await UpsertProjectDetail(project, eventType);
      PerhapsCloseConnection();
      return upsertedCount;
    }

    /// <summary>
    /// All detail-related columns can be inserted, 
    ///    but only certain columns can be updated.
    ///    on deletion, a flag will be set.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertProjectDetail(Project project, string eventType)
    {
      using (await Locker.LockAsync())
      {
        PerhapsOpenConnection();
        log.DebugFormat("ProjectRepository: Upserting eventType{0} projectUid={1}", eventType, project.projectUid);
        var upsertedCount = 0;

        var existing = (await Connection.QueryAsync<Project>
          (@"SELECT 
                  projectUid, name, projectId, timeZone, customerUid, subscriptionUid, 
                  daysToSubscriptionExpiry, lastActionedUtc
                FROM projects
                WHERE projectUid = @projectUid", new {project.projectUid})).FirstOrDefault();

        if (existing == null && eventType == "CreateProjectEvent")
        {
          upsertedCount = await CreateProject(project);
        }

        if (eventType == "UpdateProjectEvent")
        {
          upsertedCount = await UpdateProject(project, existing);
        }

        if (eventType == "DeleteProjectEvent")
        {
          upsertedCount = await DeleteProject(project, existing);
        }

        if (eventType == "AssociateProjectCustomerEvent")
        {
          upsertedCount = await AssociateProject(project, existing);
        }

        log.DebugFormat("ProjectRepository: upserted {0} rows", upsertedCount);
        PerhapsCloseConnection();
        return upsertedCount;
      }
    }

    private async Task<int> AssociateProject(Project project, Project existing)
    {
      if (existing != null)
      {
        if (project.lastActionedUtc >= existing.lastActionedUtc)
        {
          const string update =
            @"UPDATE projects                
                SET customerUid = @customerUid,
                  lastActionedUTC = @lastActionedUtc
              WHERE projectUid = @projectUid";
          return await Connection.ExecuteAsync(update, project);
        }
        else
        {
          log.DebugFormat("ProjectRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
            existing.lastActionedUtc, project.lastActionedUtc);
        }
      }
      else
      {
        log.DebugFormat("ProjectRepository: can't update as none existing newActionedUTC {0}",
          project.lastActionedUtc);
      }
      return await Task.FromResult(0);
    }

    private async Task<int> CreateProject(Project project)
    {
      const string insert =
        @"INSERT projects
                (projectId, name, timeZone, projectUid, lastActionedUtc, projectStartDate, projectEndDate, projectType)
                VALUES
                (@projectId, @name, @timeZone, @projectUid, @lastActionedUtc, @projectStartDate, @projectEndDate, @projectType)";
      return await Connection.ExecuteAsync(insert, project);
    }

    private async Task<int> DeleteProject(Project project, Project existing)
    {
      if (existing != null)
      {
        if (project.lastActionedUtc >= existing.lastActionedUtc)
        {
          const string update =
            @"UPDATE projects                
                SET IsDeleted = 1,
                  lastActionedUtc = @lastActionedUtc
              WHERE projectUid = @projectUid";
          return await Connection.ExecuteAsync(update, project);
        }
        else
        {
          log.DebugFormat("ProjectRepository: old delete event ignored currentActionedUTC{0} newActionedUTC{1}",
            existing.lastActionedUtc, project.lastActionedUtc);
        }
      }
      else
      {
        log.DebugFormat("ProjectRepository: can't delete as none existing newActionedUTC {0}",
          project.lastActionedUtc);
      }
      return await Task.FromResult(0);
    }

    private async Task<int> UpdateProject(Project project, Project existing)
    {
      if (existing != null)
      {
        if (project.lastActionedUtc >= existing.lastActionedUtc)
        {
          const string update =
            @"UPDATE projects                
                SET name = @name,
                  lastActionedUTC = @lastActionedUtc,
                  projectEndDate=@projectEndDate, 
                  projectType=@projectType
              WHERE projectUid = @projectUid";
          return await Connection.ExecuteAsync(update, project);
        }
        else
        {
          log.DebugFormat("ProjectRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
            existing.lastActionedUtc, project.lastActionedUtc);
        }
      }
      else
      {
        log.DebugFormat("ProjectRepository: can't update as none existing newActionedUTC {0}",
          project.lastActionedUtc);
      }
      return await Task.FromResult(0);
    }

    public Project GetProject(string projectUid)
    {
      PerhapsOpenConnection();
      var project = Connection.Query<Project>
        (@"SELECT 
                 projectUid, name, projectId, timeZone, customerUid, subscriptionUid, 
                  daysToSubscriptionExpiry, lastActionedUtc, IsDeleted
              FROM projects
              WHERE projectUid = @projectUid AND IsDeleted=0"
          , new { projectUid }
        ).FirstOrDefault();
      PerhapsCloseConnection();
      return project;
    }

    public IEnumerable<Project> GetProjects()
    {
      PerhapsOpenConnection();
      var project = Connection.Query<Project>
        (@"SELECT 
                 projectUid, name, projectId, timeZone, customerUid, subscriptionUid, 
                  daysToSubscriptionExpiry, lastActionedUtc, IsDeleted
              FROM projects
              WHERE  IsDeleted=0");
      PerhapsCloseConnection();
      return project;
    }
  }
}