using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using Dapper;
using MySql.Data.MySqlClient;
using log4net;
using VSS.Project.Data.Interfaces;
using VSS.Project.Data.Models;

namespace VSS.Project.Data
{

  public class MySqlProjectRepository : IProjectService
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
    private readonly string _connectionString;

    public MySqlProjectRepository()
    {
      _connectionString = ConfigurationManager.ConnectionStrings["MySql.Connection"].ConnectionString;
    }


    public int StoreProject(IProjectEvent evt)
    {
      var upsertedCount = 0;
      var project = new Models.Project();
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

      upsertedCount = UpsertProjectDetail(project, eventType);
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
    private int UpsertProjectDetail(Models.Project project, string eventType)
    {
      int upsertedCount = 0;
      using (var connection = new MySqlConnection(_connectionString))
      {
        Log.DebugFormat("ProjectRepository: Upserting eventType{0} projectUid={1}", eventType, project.projectUid);

        connection.Open();
        var existing = connection.Query<Models.Project>
          (@"SELECT 
                  projectUid, name, projectId, timeZone, customerUid, subscriptionUid, 
                  daysToSubscriptionExpiry, lastActionedUtc
                FROM projects
                WHERE projectUid = @projectUid", new {project.projectUid}).FirstOrDefault();

        if (eventType == "CreateProjectEvent")
        {
          upsertedCount = CreateProject(connection, project, existing);
        }

        if (eventType == "UpdateProjectEvent")
        {
          upsertedCount = UpdateProject(connection, project, existing);
        }

        if (eventType == "DeleteProjectEvent")
        {
          upsertedCount = DeleteProject(connection, project, existing);
        }

        if (eventType == "AssociateProjectCustomerEvent")
        {
          upsertedCount = AssociateProject(connection, project, existing);
        }

        Log.DebugFormat("ProjectRepository: upserted {0} rows", upsertedCount);
        connection.Close();
      }
      return upsertedCount;
    }

    private int AssociateProject(MySqlConnection connection, Models.Project project, Models.Project existing)
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
          return connection.Execute(update, project);
        }
        else
        {
          Log.DebugFormat("ProjectRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
            existing.lastActionedUtc, project.lastActionedUtc);
        }
      }
      else
      {
        Log.DebugFormat("ProjectRepository: can't update as none existing newActionedUTC {0}",
          project.lastActionedUtc);
      }
      return 0;
    }

    private int CreateProject(MySqlConnection connection, Models.Project project, Models.Project existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT projects
                (projectId, name, timeZone, projectUid, lastActionedUtc, projectStartDate, projectEndDate, projectType)
                VALUES
                (@projectId, @name, @timeZone, @projectUid, @lastActionedUtc, @projectStartDate, @projectEndDate, @projectType)";
        return connection.Execute(insert, project);
      }
      else
      {
        Log.DebugFormat("ProjectRepository: can't create as already exists newActionedUTC {0}", project.lastActionedUtc);
      }
      return 0;
    }

    private int DeleteProject(MySqlConnection connection, Models.Project project, Models.Project existing)
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
          return connection.Execute(update, project);
        }
        else
        {
          Log.DebugFormat("ProjectRepository: old delete event ignored currentActionedUTC{0} newActionedUTC{1}",
            existing.lastActionedUtc, project.lastActionedUtc);
        }
      }
      else
      {
        Log.DebugFormat("ProjectRepository: can't delete as none existing newActionedUTC {0}",
          project.lastActionedUtc);
      }
      return 0;
    }

    private int UpdateProject(MySqlConnection connection, Models.Project project, Models.Project existing)
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
          return connection.Execute(update, project);
        }
        else
        {
          Log.DebugFormat("ProjectRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
            existing.lastActionedUtc, project.lastActionedUtc);
        }
      }
      else
      {
        Log.DebugFormat("ProjectRepository: can't update as none existing newActionedUTC {0}",
          project.lastActionedUtc);
      }
      return 0;
    }

    public Models.Project GetProject(string projectUid)
    {
      Models.Project project;
      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();
        project = connection.Query<Models.Project>
          (@"SELECT 
                   projectUid, name, projectId, timeZone, customerUid, subscriptionUid, 
                    daysToSubscriptionExpiry, lastActionedUtc, IsDeleted
                FROM projects
                WHERE projectUid = @projectUid AND IsDeleted=0"
            , new {projectUid}
          ).FirstOrDefault();
        connection.Close();
      }
      return project;
    }

    public IEnumerable<Models.Project> GetProjectsBySubcription(string subscriptionUid)
    {
      IEnumerable<Models.Project> projects;
      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();
        projects = connection.Query<Models.Project>
          (@"SELECT 
                   projectUid, name, projectId, timeZone, customerUid, subscriptionUid, 
                    daysToSubscriptionExpiry, lastActionedUtc, IsDeleted
                FROM projects
                WHERE subscriptionUid = @subscriptionUid AND IsDeleted=0"
          );
        connection.Close();
      }
      return projects;
    }

    public IEnumerable<Models.Project> GetProjects()
    {
      IEnumerable<Models.Project> projects;
      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();
        projects = connection.Query<Models.Project>
          (@"SELECT 
                   projectUid, name, projectId, timeZone, customerUid, subscriptionUid, 
                    daysToSubscriptionExpiry, lastActionedUtc, IsDeleted, projectStartDate, projectEndDate, projectType
                FROM projects
                WHERE  IsDeleted=0");
        connection.Close();
      }
      return projects;
    }
  }
}