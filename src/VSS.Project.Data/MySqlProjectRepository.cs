using System;
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
        project.projectTimeZone = projectEvent.ProjectTimezone;
        project.landfillTimeZone = TimeZone.WindowsToIana(projectEvent.ProjectTimezone);
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
                  ProjectUID, Name, ProjectID, ProjectTimeZone, LandfillTimeZone, CustomerUID, SubscriptionUID, 
                  LastActionedUTC, StartDate, EndDate, fk_ProjectTypeID AS ProjectType, IsDeleted
                FROM Project
                WHERE ProjectUID = @projectUid", new { project.projectUid }).FirstOrDefault();

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
            @"UPDATE Project                
                SET customerUID = @customerUid,
                  LastActionedUTC = @lastActionedUtc
              WHERE ProjectUID = @projectUid";
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
          @"INSERT Project
                (ProjectID, Name, ProjectTimeZone, LandfillTimeZone, ProjectUID, LastActionedUTC, StartDate, EndDate, fk_ProjectTypeID)
            VALUES
                (@projectId, @name, @projectTimeZone, @landfillTimeZone, @projectUid, @lastActionedUtc, @projectStartDate, @projectEndDate, @projectType)";
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
            @"UPDATE Project                
                SET IsDeleted = 1,
                  LastActionedUTC = @lastActionedUtc
              WHERE ProjectUID = @projectUid";
          return connection.Execute(update, project);
        }
        else
        {
          Log.DebugFormat("ProjectRepository: old delete event ignored currentActionedUTC={0} newActionedUTC={1}",
            existing.lastActionedUtc, project.lastActionedUtc);
        }
      }
      else
      {
        Log.DebugFormat("ProjectRepository: can't delete as none existing newActionedUTC={0}",
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
            @"UPDATE Project                
                SET Name = @name,
                  LastActionedUTC = @lastActionedUtc,
                  EndDate = @projectEndDate, 
                  fk_ProjectTypeID = @projectType
              WHERE ProjectUID = @projectUid";
          return connection.Execute(update, project);
        }
        else
        {
          Log.DebugFormat("ProjectRepository: old update event ignored currentActionedUTC={0} newActionedUTC={1}",
            existing.lastActionedUtc, project.lastActionedUtc);
        }
      }
      else
      {
        Log.DebugFormat("ProjectRepository: can't update as none existing newActionedUTC={0}",
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
                   ProjectUID, Name, ProjectID, ProjectTimeZone, LandfillTimeZone, CustomerUID, SubscriptionUID, 
                    LastActionedUTC, IsDeleted, StartDate, EndDate, fk_ProjectTypeID as ProjectType
                FROM Project
                WHERE ProjectUID = @projectUid AND IsDeleted = 0"
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
                   ProjectUID, Name, ProjectID, ProjectTimeZone, LandfillTimeZone, CustomerUID, SubscriptionUID, 
                    LastActionedUTC, IsDeleted, StartDate, EndDate, fk_ProjectTypeID as ProjectType
                FROM Project
                WHERE SubscriptionUID = @subscriptionUid AND IsDeleted = 0"
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
                   ProjectUID, Name, ProjectID, ProjectTimeZone, LandfillTimeZone, CustomerUID, SubscriptionUID, 
                    LastActionedUTC, IsDeleted, StartDate, EndDate, fk_ProjectTypeID as ProjectType
                FROM Project
                WHERE IsDeleted = 0"
         );
        connection.Close();
      }
      return projects;
    }

    public IEnumerable<Models.Project> GetProjectsForUser(string userUid)
    {
      IEnumerable<Models.Project> projects;
      using (var connection = new MySqlConnection(_connectionString))
      {
        connection.Open();
        projects = connection.Query<Models.Project>
         (@"SELECT 
                   p.ProjectUID, p.Name, p.ProjectID, p.ProjectTimeZone, p.LandfillTimeZone, p.CustomerUID, p.SubscriptionUID, 
                   p.LastActionedUTC, p.IsDeleted, p.StartDate AS ProjectStartDate, p.EndDate AS ProjectEndDate, 
                   p.fk_ProjectTypeID as ProjectType, s.EndDate AS SubEndDate
                FROM Project p
                JOIN Subscription s on p.SubscriptionUID = s.SubscriptionUID
                JOIN CustomerUser cu on p.CustomerUID = cu.fk_CustomerUID
                WHERE cu.fk_userUID = @userUid", 
         new { userUid }
         );
        connection.Close();
      }
      return projects;
    }

  }
}