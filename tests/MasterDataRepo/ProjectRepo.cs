using System.Linq;
using System.Reflection;
using Common.Models;
using Dapper;
using log4net;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;


namespace MasterDataRepo
{

  public class ProjectRepo : RepositoryBase
  {
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

    public int StoreProject(IProjectEvent evt)
    {
      var upsertedCount = 0;
      if (evt is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent)evt;

        var project = new Common.Models.Project
        {
          EndDate = projectEvent.ProjectEndDate,
          StartDate = projectEvent.ProjectStartDate,
          ProjectTimeZone = projectEvent.ProjectTimezone,
          LandfillTimeZone = TimeZone.WindowsToIana(projectEvent.ProjectTimezone),
          Name = projectEvent.ProjectName,
          Description = projectEvent.Description,
          ProjectType = projectEvent.ProjectType,
          GeometryWKT = projectEvent.ProjectBoundary,
          ProjectUID = projectEvent.ProjectUID.ToString(),
          LegacyProjectID = projectEvent.ProjectID,
          LastActionedUTC = projectEvent.ActionUTC,
          
        };
        upsertedCount = UpsertProjectDetail(project, "CreateProjectEvent");
      }
      else if (evt is AssociateProjectCustomer)
      {
        var projectEvent = (AssociateProjectCustomer)evt;
        var customerProject = new CustomerProject();
        customerProject.ProjectUID = projectEvent.ProjectUID.ToString();
        customerProject.CustomerUID = projectEvent.CustomerUID.ToString();
        customerProject.LegacyCustomerID = projectEvent.LegacyCustomerID;
        customerProject.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = UpsertCustomerProjectDetail(customerProject, "AssociateProjectCustomerEvent");
      }
      else if (evt is AssociateProjectGeofence)
      {
        var projectEvent = (AssociateProjectGeofence)evt;
        var projectGeofence = new ProjectGeofence();
        projectGeofence.ProjectUID = projectEvent.ProjectUID.ToString();
        projectGeofence.GeofenceUID = projectEvent.GeofenceUID.ToString();
        projectGeofence.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = UpsertProjectGeofenceDetail(projectGeofence, "AssociateProjectGeofenceEvent");
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
    private int UpsertProjectDetail(Common.Models.Project project, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      Log.DebugFormat("ProjectRepository: Upserting eventType={0} projectUid={1}", eventType, project.ProjectUID);

      var existing = Connection.Query<Project>
      (@"SELECT 
              ProjectUID, LegacyProjectID, Name, fk_ProjectTypeID AS ProjectType, Description, 
              ProjectTimeZone, LandfillTimeZone, GeometryWKT,
              StartDate, EndDate,  IsDeleted, LastActionedUTC 
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

    private int CreateProject(Project project, Project existing)
    {
      if (existing == null)
      {
        const string insert =
          @"INSERT Project
                (LegacyProjectID, Name, ProjectTimeZone, LandfillTimeZone, ProjectUID, LastActionedUTC, StartDate, EndDate, fk_ProjectTypeID, Description, GeometryWKT)
            VALUES
                (@LegacyProjectID, @Name, @ProjectTimeZone, @LandfillTimeZone, @ProjectUID, @LastActionedUTC, @StartDate, @EndDate, @ProjectType, @Description, @GeometryWKT)";
        return Connection.Execute(insert, project);
      }

      Log.DebugFormat("ProjectRepository: can't create as already exists newActionedUTC {0}.", project.LastActionedUTC);
      return 0;
    }

    private int DeleteProject(Project project, Project existing)
    {
      if (existing != null)
      {
          const string update =
            @"UPDATE Project                
                SET IsDeleted = 1,
                  LastActionedUTC = @LastActionedUTC
              WHERE ProjectUID = @ProjectUID";
          return Connection.Execute(update, project);
        }
      return 0;
    }

    private int UpdateProject(Project project, Project existing)
    {
      if (existing != null)
      {

        const string update =
          @"UPDATE Project                
                SET Name = @Name,
                  LastActionedUTC = @LastActionedUTC,
                  EndDate = @EndDate, 
                  fk_ProjectTypeID = @ProjectType,
                  Description = @Description, 
                  GeometryWKT = @GeometryWKT
              WHERE ProjectUID = @ProjectUID";
        return Connection.Execute(update, project);
      }

      return 0;
    }

    private int UpsertCustomerProjectDetail(CustomerProject customerProject, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      Log.DebugFormat("ProjectRepository: Upserting eventType={0} CustomerUid={1}, ProjectUid={2}",
        eventType, customerProject.CustomerUID, customerProject.ProjectUID);

      var existing = Connection.Query<CustomerProject>
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

    private int AssociateProjectCustomer(CustomerProject customerProject, CustomerProject existing)
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

    private int UpsertProjectGeofenceDetail(ProjectGeofence projectGeofence, string eventType)
    {
      int upsertedCount = 0;

      PerhapsOpenConnection();

      Log.DebugFormat("ProjectRepository: Upserting eventType={0} ProjectUid={1}, GeofenceUid={2}",
        eventType, projectGeofence.ProjectUID, projectGeofence.GeofenceUID);

      var existing = Connection.Query<ProjectGeofence>
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

    private int AssociateProjectGeofence(ProjectGeofence projectGeofence, ProjectGeofence existing)
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
  }
}
