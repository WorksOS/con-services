using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Dapper;
using KafkaConsumer;
using VSS.Project.Service.Repositories;
using VSS.Project.Service.Utils;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.Project.Data
{
  public class ProjectRepository : RepositoryBase, IRepository<IProjectEvent>
  {
    //  private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public ProjectRepository(IConfigurationStore _connectionString) : base(_connectionString)
    {
    }

    public async Task<int> StoreEvent(IProjectEvent evt)
    {
      var upsertedCount = 0;
      if (evt is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent)evt;
        var project = new Models.Project();
        project.LegacyProjectID = projectEvent.ProjectID;
        project.Name = projectEvent.ProjectName;
        project.ProjectTimeZone = projectEvent.ProjectTimezone;
        project.LandfillTimeZone = TimeZone.WindowsToIana(projectEvent.ProjectTimezone);
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.EndDate = projectEvent.ProjectEndDate;
        project.LastActionedUTC = projectEvent.ActionUTC;
        project.StartDate = projectEvent.ProjectStartDate;
        project.ProjectType = projectEvent.ProjectType;
        upsertedCount = await UpsertProjectDetail(project, "CreateProjectEvent");
      }
      else if (evt is UpdateProjectEvent)
      {
        var projectEvent = (UpdateProjectEvent)evt;
        var project = new Models.Project();
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.Name = projectEvent.ProjectName;
        project.EndDate = projectEvent.ProjectEndDate;
        project.LastActionedUTC = projectEvent.ActionUTC;
        project.ProjectType = projectEvent.ProjectType;
        upsertedCount = await UpsertProjectDetail(project, "UpdateProjectEvent");
      }
      else if (evt is DeleteProjectEvent)
      {
        var projectEvent = (DeleteProjectEvent)evt;
        var project = new Models.Project();
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = await UpsertProjectDetail(project, "DeleteProjectEvent");
      }
      else if (evt is AssociateProjectCustomer)
      {
        var projectEvent = (AssociateProjectCustomer)evt;
        var customerProject = new Models.CustomerProject();
        customerProject.ProjectUID = projectEvent.ProjectUID.ToString();
        customerProject.CustomerUID = projectEvent.CustomerUID.ToString();
        customerProject.LegacyCustomerID = projectEvent.LegacyCustomerID;
        customerProject.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = await UpsertCustomerProjectDetail(customerProject, "AssociateProjectCustomerEvent");
      }
      else if (evt is AssociateProjectGeofence)
      {
        var projectEvent = (AssociateProjectGeofence)evt;
        var projectGeofence = new Models.ProjectGeofence();
        projectGeofence.ProjectUID = projectEvent.ProjectUID.ToString();
        projectGeofence.GeofenceUID = projectEvent.GeofenceUID.ToString();
        projectGeofence.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = await UpsertProjectGeofenceDetail(projectGeofence, "AssociateProjectGeofenceEvent");
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
    private async Task<int> UpsertProjectDetail(Models.Project project, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      //Log.DebugFormat("ProjectRepository: Upserting eventType={0} projectUid={1}", eventType, project.ProjectUID);

      var existing = Connection.Query<Models.Project>
          (@"SELECT 
                ProjectUID, LegacyProjectID, Name, fk_ProjectTypeID AS ProjectType, IsDeleted,
                ProjectTimeZone, LandfillTimeZone, 
                LastActionedUTC, StartDate, EndDate
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

      //Log.DebugFormat("ProjectRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();

      return upsertedCount;
    }

    private int CreateProject(Models.Project project, Models.Project existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT Project
                (ProjectUID, LegacyProjectID, Name, fk_ProjectTypeID, IsDeleted, ProjectTimeZone, LandfillTimeZone, LastActionedUTC, StartDate, EndDate )
              VALUES
                (@ProjectUID, @LegacyProjectID, @Name, @ProjectType, @IsDeleted, @ProjectTimeZone, @LandfillTimeZone, @LastActionedUTC, @StartDate, @EndDate)";
        return Connection.Execute(insert, project);
      }
      else if (string.IsNullOrEmpty(existing.Name))
      {
        //Dummy one was inserted, so update with actual data
        const string update =
            @"UPDATE Project                
                SET LegacyProjectID = @LegacyProjectID,
                  Name = @Name,
                  fk_ProjectTypeID = @ProjectType,
                  IsDeleted = @IsDeleted,
                  ProjectTimeZone = @ProjectTimeZone,
                  LandfillTimeZone = @LandfillTimeZone,
                  StartDate = @StartDate,
                  EndDate = @EndDate,
                  LastActionedUTC = @LastActionedUTC         
                WHERE ProjectUID = @ProjectUID";
        return Connection.Execute(update, project);
      }

      //            Log.DebugFormat("ProjectRepository: can't create as already exists newActionedUTC {0}.", project.LastActionedUTC);

      return 0;
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
          //                Log.DebugFormat("ProjectRepository: old delete event ignored currentActionedUTC={0} newActionedUTC={1}",
          //                    existing.LastActionedUTC, project.LastActionedUTC);
        }
      }
      else
      {
        //     Log.DebugFormat("ProjectRepository: can't delete as none existing newActionedUTC={0}",
        //         project.LastActionedUTC);
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
                  EndDate = @EndDate, 
                  fk_ProjectTypeID = @ProjectType
                WHERE ProjectUID = @ProjectUID";
          return Connection.Execute(update, project);
        }
        else
        {
          //           Log.DebugFormat("ProjectRepository: old update event ignored currentActionedUTC={0} newActionedUTC={1}",
          //             existing.LastActionedUTC, project.LastActionedUTC);
        }
      }
      else
      {
        //        Log.DebugFormat("ProjectRepository: can't update as none existing newActionedUTC={0}",
        //           project.LastActionedUTC);
      }
      return 0;
    }

    private async Task<int> UpsertCustomerProjectDetail(Models.CustomerProject customerProject, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      //    Log.DebugFormat("ProjectRepository: Upserting eventType={0} CustomerUid={1}, ProjectUid={2}",
      //         eventType, customerProject.CustomerUID, customerProject.ProjectUID);

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

      //      Log.DebugFormat("ProjectRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();
      return upsertedCount;
    }

    private int AssociateProjectCustomer(Models.CustomerProject customerProject, Models.CustomerProject existing)
    {
      const string insert =
        @"INSERT CustomerProject
              (fk_ProjectUID, fk_CustomerUID, LegacyCustomerID, LastActionedUTC)
            VALUES
              (@ProjectUID, @CustomerUID, @LegacyCustomerID, @LastActionedUTC)
            ON DUPLICATE KEY UPDATE              
              LastActionedUTC =
                IF ( VALUES(LastActionedUTC) >= LastActionedUTC, 
                    VALUES(LastActionedUTC), LastActionedUTC),
              LegacyCustomerID =
                IF ( VALUES(LastActionedUTC) >= LastActionedUTC, 
                    VALUES(LegacyCustomerID), LegacyCustomerID)";
      var rowCount = Connection.Execute(insert, customerProject);
      return rowCount == 2 ? 1 : rowCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted

    }

    private async Task<int> UpsertProjectGeofenceDetail(Models.ProjectGeofence projectGeofence, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      //    Log.DebugFormat("ProjectRepository: Upserting eventType={0} ProjectUid={1}, GeofenceUid={2}",
      //        eventType, projectGeofence.ProjectUID, projectGeofence.GeofenceUID);

      var existing = Connection.Query<Models.ProjectGeofence>
        (@"SELECT 
              fk_GeofenceUID AS GeofenceUID, fk_ProjectUID AS ProjectUID, LastActionedUTC
            FROM ProjectGeofence
            WHERE fk_ProjectUID = @projectUID AND fk_GeofenceUID = @geofenceUID",
         new { projectUID = projectGeofence.ProjectUID, geofenceUID = projectGeofence.GeofenceUID }).FirstOrDefault();

      if (eventType == "AssociateProjectGeofenceEvent")
      {
        upsertedCount = AssociateProjectGeofence(projectGeofence, existing);
      }

      //    Log.DebugFormat("ProjectRepository: upserted {0} rows", upsertedCount);

      PerhapsCloseConnection();
      return upsertedCount;
    }

    private int AssociateProjectGeofence(Models.ProjectGeofence projectGeofence, Models.ProjectGeofence existing)
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

      //      Log.DebugFormat("ProjectRepository: can't create as already exists newActionedUTC={0}", projectGeofence.LastActionedUTC);
      return 0;
    }

    /// <summary>
    /// There may be 0 or n subscriptions for this project. None/many may be current. 
    /// This method just gets ANY one of these or no subs (SubscriptionUID == null)
    /// We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<Models.Project> GetProject(string projectUid)
    {
      await PerhapsOpenConnection();

      var project = Connection.Query<Models.Project>
          (@"SELECT 
                p.ProjectUID, p.Name, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,                     
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.EndDate AS SubscriptionEndDate
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE p.ProjectUID = @projectUid AND p.IsDeleted = 0",
            new { projectUid }
          ).FirstOrDefault();

      PerhapsCloseConnection();
      return project;
    }

    /// <summary>
    /// gets only 1 row for a particular sub. only 1 projectUID and be associated with a sub
    /// </summary>
    /// <param name="subscriptionUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Models.Project>> GetProjectsBySubcription(string subscriptionUid)
    {
      await PerhapsOpenConnection();

      var projects = Connection.Query<Models.Project>
          (@"SELECT 
                p.ProjectUID, p.Name, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,                     
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.EndDate AS SubscriptionEndDate
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE ps.fk_SubscriptionUID = @subscriptionUid AND p.IsDeleted = 0"
          );

      PerhapsCloseConnection();
      return projects;
    }


    /// <summary>
    /// There may be 0 or n subscriptions for each project. None/many may be current. 
    /// This method just gets ANY one of these or no subs (SubscriptionUID == null)
    /// We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="userUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Models.Project>> GetProjectsForUser(string userUid)
    {
      await PerhapsOpenConnection();
      var projects = Connection.Query<Models.Project>
          (@"SELECT 
                p.ProjectUID, p.Name, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,                     
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.EndDate AS SubscriptionEndDate
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE cu.fk_userUID = @userUid and p.IsDeleted = 0",
            new { userUid }
          );

      PerhapsCloseConnection();
      return projects;
    }

    /// <summary>
    /// There may be 0 or n subscriptions for each project. None/many may be current. 
    /// This method just gets ANY one of these or no subs (SubscriptionUID == null)
    /// We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="userUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Models.Project>> GetProjectsForCustomerUser(string customerUid, string userUid)
    {
      await PerhapsOpenConnection();

      var projects = Connection.Query<Models.Project>
          (@"SELECT 
                p.ProjectUID, p.Name, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,                     
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.EndDate AS SubscriptionEndDate
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE cp.fk_CustomerUID = @customerUid and cu.fk_userUID = @userUid and p.IsDeleted = 0",
            new { userUid }
          );

      PerhapsCloseConnection();
      return projects;
    }


    /// <summary>
    /// for unit tests - so we don't have to create everything (associations) for a test
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<Models.Project> GetProject_UnitTest(string projectUid)
    {
      await PerhapsOpenConnection();

      var project = Connection.Query<Models.Project>
          (@"SELECT 
                  p.ProjectUID, p.Name, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,                     
                  p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType,
                  cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                  ps.fk_SubscriptionUID AS SubscriptionUID, s.EndDate AS SubscriptionEndDate              
              FROM Project p 
                LEFT JOIN CustomerProject cp ON p.ProjectUID = cp.fk_ProjectUID
                LEFT JOIN Customer c ON c.CustomerUID = cp.fk_CustomerUID
                LEFT JOIN ProjectSubscription ps on p.ProjectUID = ps.fk_ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE p.ProjectUID = @projectUid",
            new { projectUid }
          ).FirstOrDefault();

      PerhapsCloseConnection();
      return project;
    }

    /// <summary>
    ///  this must be a test method
    /// </summary>
    /// <returns></returns>
    public async Task<IEnumerable<Models.Project>> GetProjects()
    {
      await PerhapsOpenConnection();

      var projects = Connection.Query<Models.Project>
          (@"SELECT 
                p.ProjectUID, p.Name, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,                     
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.EndDate AS SubscriptionEndDate
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE p.IsDeleted = 0");

      PerhapsCloseConnection();
      return projects;
    }

  }
}