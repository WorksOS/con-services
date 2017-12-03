using System.Linq;
using System.Reflection;
using Dapper;
using log4net;
using VSS.MasterData.Common.Repositories;
using VSS.Project.Data.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Project.Data
{

  public class MySqlProjectRepository : RepositoryBase, IProjectService
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public int StoreProject(IProjectEvent evt)
    {
      var upsertedCount = 0;
      if (evt is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent)evt;
        var project = new Common.Models.Project();
        project.ProjectID = projectEvent.ProjectID;
        project.Name = projectEvent.ProjectName;
        project.ProjectTimeZone = projectEvent.ProjectTimezone;
        project.LandfillTimeZone = TimeZone.WindowsToIana(projectEvent.ProjectTimezone);
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.ProjectEndDate = projectEvent.ProjectEndDate;
        project.LastActionedUTC = projectEvent.ActionUTC;
        project.ProjectStartDate = projectEvent.ProjectStartDate;
        project.ProjectType = projectEvent.ProjectType;
        upsertedCount = UpsertProjectDetail(project, "CreateProjectEvent");
      }
      else if (evt is UpdateProjectEvent)
      {
        var projectEvent = (UpdateProjectEvent)evt;
        var project = new Common.Models.Project();
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.Name = projectEvent.ProjectName;
        project.ProjectEndDate = projectEvent.ProjectEndDate;
        project.LastActionedUTC = projectEvent.ActionUTC;
        project.ProjectType = projectEvent.ProjectType;
        upsertedCount = UpsertProjectDetail(project, "UpdateProjectEvent");
      }
      else if (evt is DeleteProjectEvent)
      {
        var projectEvent = (DeleteProjectEvent)evt;
        var project = new Common.Models.Project();
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = UpsertProjectDetail(project, "DeleteProjectEvent");
      }
      else if (evt is AssociateProjectCustomer)
      {
        var projectEvent = (AssociateProjectCustomer)evt;
        var customerProject = new Models.CustomerProject();
        customerProject.ProjectUID = projectEvent.ProjectUID.ToString();
        customerProject.CustomerUID = projectEvent.CustomerUID.ToString();
        customerProject.LegacyCustomerID = projectEvent.LegacyCustomerID;
        customerProject.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = UpsertCustomerProjectDetail(customerProject, "AssociateProjectCustomerEvent");
      }
      else if (evt is AssociateProjectGeofence)
      {
        var projectEvent = (AssociateProjectGeofence)evt;
        var projectGeofence = new Common.Models.ProjectGeofence();
        projectGeofence.ProjectUID = projectEvent.ProjectUID.ToString();
        projectGeofence.GeofenceUID = projectEvent.GeofenceUID.ToString();
        projectGeofence.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = UpsertProjectGeofenceDetail(projectGeofence, "AssociateProjectGeofenceEvent");
      }
      //Ignore DissociateProjectCustomer (not used) 
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
    private int UpsertProjectDetail(Common.Models.Project project, string eventType)
    {
      int upsertedCount = 0;
      
      PerhapsOpenConnection();
      
      Log.DebugFormat("ProjectRepository: Upserting eventType={0} projectUid={1}", eventType, project.ProjectUID);

      var existing = Connection.Query<Common.Models.Project>
        (@"SELECT 
                ProjectUID, Name, ProjectID, ProjectTimeZone, LandfillTimeZone, 
                LastActionedUTC, StartDate, EndDate, fk_ProjectTypeID AS ProjectType, IsDeleted
              FROM Project
              WHERE ProjectUID = @projectUid", new { projectUid = project.ProjectUID }).FirstOrDefault();

      if (eventType == "CreateProjectEvent")
      {
        upsertedCount = CreateProject(project, existing);
      }

      if (eventType == "UpdateProjectEvent")
      {
        upsertedCount = UpdateProject(project, existing);
      }

      if (eventType == "DeleteProjectEvent")
      {
        upsertedCount = DeleteProject(project, existing);
      }

      Log.DebugFormat("ProjectRepository: upserted {0} rows", upsertedCount);
      
      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int CreateProject(Common.Models.Project project, Common.Models.Project existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT Project
                (ProjectID, Name, ProjectTimeZone, LandfillTimeZone, ProjectUID, LastActionedUTC, StartDate, EndDate, fk_ProjectTypeID)
            VALUES
                (@ProjectID, @Name, @ProjectTimeZone, @LandfillTimeZone, @ProjectUID, @LastActionedUTC, @ProjectStartDate, @ProjectEndDate, @ProjectType)";
        return Connection.Execute(insert, project);
      }
      else if (string.IsNullOrEmpty(existing.Name))
      {
        //Dummy one was inserted, so update with actual data
        const string update =
       @"UPDATE Project                
                SET ProjectID = @ProjectID,
                  Name = @Name,
                  ProjectTimeZone = @ProjectTimeZone,
                  LandfillTimeZone = @LandfillTimeZone,
                  StartDate = @ProjectStartDate,
                  EndDate = @ProjectEndDate,
                  LastActionedUTC = @LastActionedUTC,
                  EndDate = @ProjectEndDate, 
                  fk_ProjectTypeID = @ProjectType
              WHERE ProjectUID = @ProjectUID";
        return Connection.Execute(update, project);
      }

      Log.DebugFormat("ProjectRepository: can't create as already exists newActionedUTC {0}.", project.LastActionedUTC);

      return 0;
    }

    private int DeleteProject(Common.Models.Project project, Common.Models.Project existing)
    {
      if (existing != null)
      {
        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string update =
            @"UPDATE Project                
                SET IsDeleted = 1,
                  LastActionedUTC = @LastActionedUTC
              WHERE ProjectUID = @ProjectUID";
          return Connection.Execute(update, project);
        }
        else
        {
          Log.DebugFormat("ProjectRepository: old delete event ignored currentActionedUTC={0} newActionedUTC={1}",
            existing.LastActionedUTC, project.LastActionedUTC);
        }
      }
      else
      {
        Log.DebugFormat("ProjectRepository: can't delete as none existing newActionedUTC={0}",
          project.LastActionedUTC);
      }
      return 0;
    }

    private int UpdateProject(Common.Models.Project project, Common.Models.Project existing)
    {
      if (existing != null)
      {
        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string update =
            @"UPDATE Project                
                SET Name = @Name,
                  LastActionedUTC = @LastActionedUTC,
                  EndDate = @ProjectEndDate, 
                  fk_ProjectTypeID = @ProjectType
              WHERE ProjectUID = @ProjectUID";
          return Connection.Execute(update, project);
        }
        else
        {
          Log.DebugFormat("ProjectRepository: old update event ignored currentActionedUTC={0} newActionedUTC={1}",
            existing.LastActionedUTC, project.LastActionedUTC);
        }
      }
      else
      {
        Log.DebugFormat("ProjectRepository: can't update as none existing newActionedUTC={0}",
          project.LastActionedUTC);
      }
      return 0;
    }

    private int UpsertCustomerProjectDetail(Models.CustomerProject customerProject, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      Log.DebugFormat("ProjectRepository: Upserting eventType={0} CustomerUid={1}, ProjectUid={2}",
        eventType, customerProject.CustomerUID, customerProject.ProjectUID);

      var existing = Connection.Query<Models.CustomerProject>
        (@"SELECT 
            fk_CustomerUID AS CustomerUID, LegacyCustomerID, fk_ProjectUID AS ProjectUID, LastActionedUTC
              FROM CustomerProject
              WHERE fk_CustomerUID = @customerUID AND fk_ProjectUID = @projectUID", 
            new { customerUID = customerProject.CustomerUID, projectUID = customerProject.ProjectUID }).FirstOrDefault();

      if (eventType == "AssociateProjectCustomerEvent")
      {
        upsertedCount = AssociateProjectCustomer(customerProject, existing);
      }

      Log.DebugFormat("ProjectRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int AssociateProjectCustomer(Models.CustomerProject customerProject, Models.CustomerProject existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT CustomerProject
            (fk_ProjectUID, fk_CustomerUID, LegacyCustomerID, LastActionedUTC)
            VALUES
            (@ProjectUID, @CustomerUID, @LegacyCustomerID, @LastActionedUTC)";

        return Connection.Execute(insert, customerProject);
      }

      Log.DebugFormat("ProjectRepository: can't create as already exists newActionedUTC={0}", customerProject.LastActionedUTC);
      return 0;
    }

    private int UpsertProjectGeofenceDetail(Common.Models.ProjectGeofence projectGeofence, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      Log.DebugFormat("ProjectRepository: Upserting eventType={0} ProjectUid={1}, GeofenceUid={2}",
        eventType, projectGeofence.ProjectUID, projectGeofence.GeofenceUID);

      var existing = Connection.Query<Common.Models.ProjectGeofence>
        (@"SELECT 
            fk_GeofenceUID AS GeofenceUID, fk_ProjectUID AS ProjectUID, LastActionedUTC
              FROM ProjectGeofence
              WHERE fk_ProjectUID = @projectUID AND fk_GeofenceUID = @geofenceUID", 
         new { projectUID = projectGeofence.ProjectUID, geofenceUID = projectGeofence.GeofenceUID }).FirstOrDefault();

      if (eventType == "AssociateProjectGeofenceEvent")
      {
        upsertedCount = AssociateProjectGeofence(projectGeofence, existing);
      }

      Log.DebugFormat("ProjectRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int AssociateProjectGeofence(Common.Models.ProjectGeofence projectGeofence, Common.Models.ProjectGeofence existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT ProjectGeofence
            (fk_GeofenceUID, fk_ProjectUID, LastActionedUTC)
            VALUES
            (@GeofenceUID, @ProjectUID, @LastActionedUTC)";

        return Connection.Execute(insert, projectGeofence);
      }

      Log.DebugFormat("ProjectRepository: can't create as already exists newActionedUTC={0}", projectGeofence.LastActionedUTC);
      return 0;
    }


    //public IEnumerable<Models.Project> GetProjectsForUser(string userUid)
    //{
    //  PerhapsOpenConnection();

    //  var projects = Connection.Query<Models.Project>
    //     (@"SELECT 
    //          p.ProjectUID, p.Name, p.ProjectID, p.ProjectTimeZone, p.LandfillTimeZone, 
    //          cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, s.SubscriptionUID, 
    //          p.LastActionedUTC, p.IsDeleted, p.StartDate AS ProjectStartDate, p.EndDate AS ProjectEndDate, 
    //          p.fk_ProjectTypeID AS ProjectType, s.EndDate AS SubEndDate
    //      FROM Project p
    //      JOIN ProjectSubscription ps ON p.ProjectUID = ps.fk_ProjectUID
    //      JOIN Subscription s on ps.fk_SubscriptionUID = s.SubscriptionUID
    //      JOIN CustomerProject cp on p.ProjectUID = cp.fk_ProjectUID
    //      JOIN CustomerUser cu on cp.fk_CustomerUID = cu.fk_CustomerUID
    //      WHERE cu.fk_userUID = @userUid and p.IsDeleted = 0",
    //     new { userUid }
    //     );

    //  PerhapsCloseConnection();

    //  return projects;
    //}

    //public IEnumerable<Models.Project> GetLandfillProjectsForUser(string userUid)
    //{
    //  PerhapsOpenConnection();

    //  var projects = Connection.Query<Models.Project>
    //     (@"SELECT 
    //          p.ProjectUID, p.Name, p.ProjectID, p.ProjectTimeZone, p.LandfillTimeZone, 
    //          cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, s.SubscriptionUID, 
    //          p.LastActionedUTC, p.IsDeleted, p.StartDate AS ProjectStartDate, p.EndDate AS ProjectEndDate, 
    //          p.fk_ProjectTypeID AS ProjectType, s.EndDate AS SubEndDate
    //      FROM Project p
    //      JOIN ProjectSubscription ps ON p.ProjectUID = ps.fk_ProjectUID
    //      JOIN Subscription s on ps.fk_SubscriptionUID = s.SubscriptionUID
    //      JOIN CustomerProject cp on p.ProjectUID = cp.fk_ProjectUID
    //      JOIN CustomerUser cu on cp.fk_CustomerUID = cu.fk_CustomerUID
    //      WHERE cu.fk_userUID = @userUid and p.IsDeleted = 0 AND p.fk_ProjectTypeID = 1",
    //     new { userUid }
    //     );

    //  PerhapsCloseConnection();

    //  return projects;
    //}


    //for unit tests - so we don't have to create everything (associations) for a test
    public Common.Models.Project GetProject_UnitTest(string projectUid)
    {
      PerhapsOpenConnection();

      var project = Connection.Query<Common.Models.Project>
         (@"SELECT 
                p.ProjectUID, p.Name, p.ProjectID, p.ProjectTimeZone, p.LandfillTimeZone, 
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, ps.fk_SubscriptionUID AS SubscriptionUID, 
                p.LastActionedUTC, p.IsDeleted, p.StartDate AS ProjectStartDate, p.EndDate AS ProjectEndDate, 
                p.fk_ProjectTypeID as ProjectType
              FROM Project p LEFT JOIN CustomerProject cp ON p.ProjectUID = cp.fk_ProjectUID
                   LEFT JOIN ProjectSubscription ps on p.ProjectUID = ps.fk_ProjectUID
              WHERE p.ProjectUID = @projectUid AND p.IsDeleted = 0"
           , new { projectUid }
         ).FirstOrDefault();

      PerhapsCloseConnection();

      return project;
    }

  }
}