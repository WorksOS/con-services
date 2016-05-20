using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dapper;
using MySql.Data.MySqlClient;
using log4net;
using VSS.Geofence.Data.Interfaces;
using VSS.Landfill.Common.Repositories;
using VSS.Project.Data.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Project.Data
{

  public class MySqlProjectRepository : RepositoryBase, IProjectService
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public int StoreProject(IProjectEvent evt, IGeofenceService geofenceService)
    {
      var upsertedCount = 0;
      var project = new Models.Project();
      string eventType = "Unknown"; 
      if (evt is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent)evt;
        project.ProjectID = projectEvent.ProjectID;
        project.Name = projectEvent.ProjectName;
        project.ProjectTimeZone = projectEvent.ProjectTimezone;
        project.LandfillTimeZone = TimeZone.WindowsToIana(projectEvent.ProjectTimezone);
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.ProjectEndDate = projectEvent.ProjectEndDate;
        project.LastActionedUTC = projectEvent.ActionUTC;
        project.ProjectStartDate = projectEvent.ProjectStartDate;
        project.ProjectType = projectEvent.ProjectType;
        eventType = "CreateProjectEvent";
      }
      else if (evt is UpdateProjectEvent)
      {
        var projectEvent = (UpdateProjectEvent)evt;
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.Name = projectEvent.ProjectName;
        project.ProjectEndDate = projectEvent.ProjectEndDate;
        project.LastActionedUTC = projectEvent.ActionUTC;
        project.ProjectType = projectEvent.ProjectType;
        eventType = "UpdateProjectEvent";
      }
      else if (evt is DeleteProjectEvent)
      {
        var projectEvent = (DeleteProjectEvent)evt;
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.LastActionedUTC = projectEvent.ActionUTC;
        eventType = "DeleteProjectEvent";
      }
      else if (evt is AssociateProjectCustomer)
      {
        var projectEvent = (AssociateProjectCustomer)evt;
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.CustomerUID = projectEvent.CustomerUID.ToString();
        project.LastActionedUTC = projectEvent.ActionUTC;
        eventType = "AssociateProjectCustomerEvent";   
      }
      else if (evt is DissociateProjectCustomer)
      {
        //TODO Do we realy need to support this?
        var projectEvent = (DissociateProjectCustomer)evt;
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.CustomerUID = projectEvent.CustomerUID.ToString();
        project.LastActionedUTC = projectEvent.ActionUTC;
        eventType = "DissociateProjectCustomerEvent";
      }

      upsertedCount = UpsertProjectDetail(project, eventType);

      if (evt is AssociateProjectCustomer)
      {
        //Now we have the customerUID, check for geofence for this project 
        //and if it exists and is unassigned then assign it to this project
        //and also assign relevant unassigned Landfill geofences.
        var geofence = geofenceService.GetGeofenceByName(project.CustomerUID, project.Name);
        if (geofence != null && string.IsNullOrEmpty(geofence.ProjectUID))
        {
          int result = geofenceService.AssignGeofenceToProject(geofence.GeofenceUID, project.ProjectUID);
          if (result > 0)
          {
            geofenceService.AssignApplicableLandfillGeofencesToProject(geofence.GeometryWKT, geofence.CustomerUID, geofence.ProjectUID);
          }
        }
     
      }
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
      
      PerhapsOpenConnection();
      
      Log.DebugFormat("ProjectRepository: Upserting eventType{0} projectUid={1}", eventType, project.ProjectUID);

      var existing = Connection.Query<Models.Project>
        (@"SELECT 
                ProjectUID, Name, ProjectID, ProjectTimeZone, LandfillTimeZone, CustomerUID, SubscriptionUID, 
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

      if (eventType == "AssociateProjectCustomerEvent")
      {
        upsertedCount = AssociateProject(project, existing);
      }

      Log.DebugFormat("ProjectRepository: upserted {0} rows", upsertedCount);
      
      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int AssociateProject(Models.Project project, Models.Project existing)
    {
      if (existing != null)
      {
        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string update =
            @"UPDATE Project                
                SET CustomerUID = @CustomerUID,
                  LastActionedUTC = @LastActionedUTC
              WHERE ProjectUID = @ProjectUID";
          return Connection.Execute(update, project);
        }
        else
        {
          Log.DebugFormat("ProjectRepository: old update event ignored currentActionedUTC{0} newActionedUTC{1}",
            existing.LastActionedUTC, project.LastActionedUTC);
        }
      }
      else
      {
        Log.DebugFormat("ProjectRepository: can't update as none existing newActionedUTC {0}",
          project.LastActionedUTC);
      }
      return 0;
    }

    private int CreateProject(Models.Project project, Models.Project existing)
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

      Log.DebugFormat("ProjectRepository: can't create as already exists newActionedUTC {0}. So, the existing entry should be updated.", project.LastActionedUTC);

      return UpdateProject(project, existing);
    }

    private int DeleteProject(Models.Project project, Models.Project existing)
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

    private int UpdateProject(Models.Project project, Models.Project existing)
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

    public Models.Project GetProject(string projectUid)
    {
      PerhapsOpenConnection();

      var project = Connection.Query<Models.Project>
        (@"SELECT 
                  ProjectUID, Name, ProjectID, ProjectTimeZone, LandfillTimeZone, CustomerUID, SubscriptionUID, 
                  LastActionedUTC, IsDeleted, StartDate AS ProjectStartDate, EndDate AS ProjectEndDate, fk_ProjectTypeID as ProjectType
              FROM Project
              WHERE ProjectUID = @projectUid AND IsDeleted = 0"
          , new {projectUid}
        ).FirstOrDefault();

      PerhapsCloseConnection();

      return project;
    }

    public IEnumerable<Models.Project> GetProjectsBySubcription(string subscriptionUid)
    {
      PerhapsOpenConnection();

      var projects = Connection.Query<Models.Project>
          (@"SELECT 
                   ProjectUID, Name, ProjectID, ProjectTimeZone, LandfillTimeZone, CustomerUID, SubscriptionUID, 
                    LastActionedUTC, IsDeleted, StartDate AS ProjectStartDate, EndDate AS ProjectEndDate, fk_ProjectTypeID as ProjectType
                FROM Project
                WHERE SubscriptionUID = @subscriptionUid AND IsDeleted = 0"
          );

      PerhapsCloseConnection();

      return projects;
    }

    public IEnumerable<Models.Project> GetProjects()
    {
      PerhapsOpenConnection();

      var projects = Connection.Query<Models.Project>
         (@"SELECT 
                   ProjectUID, Name, ProjectID, ProjectTimeZone, LandfillTimeZone, CustomerUID, SubscriptionUID, 
                   LastActionedUTC, IsDeleted, StartDate AS ProjectStartDate, EndDate AS ProjectEndDate, fk_ProjectTypeID as ProjectType
                FROM Project
                WHERE IsDeleted = 0"
         );

      PerhapsCloseConnection();

      return projects;
    }

    public IEnumerable<Models.Project> GetProjectsForUser(string userUid)
    {
      PerhapsOpenConnection();

      var projects = Connection.Query<Models.Project>
         (@"SELECT 
                   p.ProjectUID, p.Name, p.ProjectID, p.ProjectTimeZone, p.LandfillTimeZone, p.CustomerUID, p.SubscriptionUID, 
                   p.LastActionedUTC, p.IsDeleted, p.StartDate AS ProjectStartDate, p.EndDate AS ProjectEndDate, 
                   p.fk_ProjectTypeID AS ProjectType, s.EndDate AS SubEndDate
                FROM Project p
                JOIN Subscription s on p.SubscriptionUID = s.SubscriptionUID
                JOIN CustomerUser cu on p.CustomerUID = cu.fk_CustomerUID
                WHERE cu.fk_userUID = @userUid and p.IsDeleted = 0" , 
         new { userUid }
         );

      PerhapsCloseConnection();

      return projects;
    }

    

  }
}