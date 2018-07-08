using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.ConfigurationStore;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.MasterData.Repositories.Extensions;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace VSS.MasterData.Repositories
{
  public class ProjectRepository : RepositoryBase, IRepository<IProjectEvent>, IProjectRepository
  {
    private const int LegacyProjectIdCutoff = 2000000;

    public ProjectRepository(IConfigurationStore connectionString, ILoggerFactory logger) : base(connectionString,
      logger)
    {
      log = logger.CreateLogger<ProjectRepository>();
    }

    #region store

    public async Task<int> StoreEvent(IProjectEvent evt)
    {
      var upsertedCount = 0;
      if (evt == null)
      {
        log.LogWarning("Unsupported project event type");
        return 0;
      }

      log.LogDebug($"Event type is {evt.GetType()}");
      if (evt is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent) evt;
        var project = new Project
        {
          LegacyProjectID = projectEvent.ProjectID,
          Description = projectEvent.Description,
          Name = projectEvent.ProjectName,
          ProjectTimeZone = projectEvent.ProjectTimezone,
          LandfillTimeZone = PreferencesTimeZones.WindowsToIana(projectEvent.ProjectTimezone),
          ProjectUID = projectEvent.ProjectUID.ToString(),
          EndDate = projectEvent.ProjectEndDate.Date,
          LastActionedUTC = projectEvent.ActionUTC,
          StartDate = projectEvent.ProjectStartDate.Date,
          ProjectType = projectEvent.ProjectType
        };

        if (!string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = projectEvent.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = projectEvent.ActionUTC;
        }

        project.GeometryWKT = GetPolygonWKT(projectEvent.ProjectBoundary);
        if (!string.IsNullOrEmpty(project.GeometryWKT))
        {
          upsertedCount = await UpsertProjectDetail(project, "CreateProjectEvent");
        }
      }
      else if (evt is UpdateProjectEvent)
      {
        var projectEvent = (UpdateProjectEvent) evt;

        var project = new Project
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          Name = projectEvent.ProjectName,
          Description = projectEvent.Description,
          EndDate = projectEvent.ProjectEndDate.Date,
          LastActionedUTC = projectEvent.ActionUTC,
          ProjectType = projectEvent.ProjectType,
          ProjectTimeZone = projectEvent.ProjectTimezone,
          LandfillTimeZone = PreferencesTimeZones.WindowsToIana(projectEvent.ProjectTimezone)
        };

        if (!string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = projectEvent.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = projectEvent.ActionUTC;
        }

        project.GeometryWKT = GetPolygonWKT(projectEvent.ProjectBoundary);
        upsertedCount = await UpsertProjectDetail(project, "UpdateProjectEvent");
      }
      else if (evt is DeleteProjectEvent)
      {
        var projectEvent = (DeleteProjectEvent) evt;
        var project = new Project
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          LastActionedUTC = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectDetail(project, "DeleteProjectEvent", projectEvent.DeletePermanently);
      }
      else if (evt is AssociateProjectCustomer)
      {
        var projectEvent = (AssociateProjectCustomer) evt;
        var customerProject = new CustomerProject();
        customerProject.ProjectUID = projectEvent.ProjectUID.ToString();
        customerProject.CustomerUID = projectEvent.CustomerUID.ToString();
        customerProject.LegacyCustomerID = projectEvent.LegacyCustomerID;
        customerProject.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = await UpsertCustomerProjectDetail(customerProject, "AssociateProjectCustomerEvent");
      }
      else if (evt is DissociateProjectCustomer)
      {
        var projectEvent = (DissociateProjectCustomer) evt;
        var customerProject = new CustomerProject
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          CustomerUID = projectEvent.CustomerUID.ToString(),
          LastActionedUTC = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertCustomerProjectDetail(customerProject, "DissociateProjectCustomerEvent");
      }
      else if (evt is AssociateProjectGeofence)
      {
        var projectEvent = (AssociateProjectGeofence) evt;
        var projectGeofence = new ProjectGeofence
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          GeofenceUID = projectEvent.GeofenceUID.ToString(),
          LastActionedUTC = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectGeofenceDetail(projectGeofence, "AssociateProjectGeofenceEvent");
      }
      else if (evt is DissociateProjectGeofence )
      {
        var projectEvent = (DissociateProjectGeofence)evt;
        var projectGeofence = new ProjectGeofence
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          GeofenceUID = projectEvent.GeofenceUID.ToString(),
          LastActionedUTC = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectGeofenceDetail(projectGeofence, "DissociateProjectGeofenceEvent");
      }
      else if (evt is CreateImportedFileEvent)
      {
        var projectEvent = (CreateImportedFileEvent) evt;
        var importedFile = new ImportedFile
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ImportedFileUid = projectEvent.ImportedFileUID.ToString(),
          ImportedFileId = projectEvent.ImportedFileID,
          CustomerUid = projectEvent.CustomerUID.ToString(),
          ImportedFileType = projectEvent.ImportedFileType,
          Name = projectEvent.Name,
          FileDescriptor = projectEvent.FileDescriptor,
          FileCreatedUtc = projectEvent.FileCreatedUtc,
          FileUpdatedUtc = projectEvent.FileUpdatedUtc,
          ImportedBy = projectEvent.ImportedBy,
          SurveyedUtc = projectEvent.SurveyedUTC,
          DxfUnitsType = projectEvent.DxfUnitsType,
          MinZoomLevel = projectEvent.MinZoomLevel,
          MaxZoomLevel = projectEvent.MaxZoomLevel,
          LastActionedUtc = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertImportedFile(importedFile, "CreateImportedFileEvent");
      }
      else if (evt is UpdateImportedFileEvent)
      {
        var projectEvent = (UpdateImportedFileEvent) evt;
        var importedFile = new ImportedFile
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ImportedFileUid = projectEvent.ImportedFileUID.ToString(),
          FileDescriptor = projectEvent.FileDescriptor,
          FileCreatedUtc = projectEvent.FileCreatedUtc,
          FileUpdatedUtc = projectEvent.FileUpdatedUtc,
          ImportedBy = projectEvent.ImportedBy,
          SurveyedUtc = projectEvent.SurveyedUtc,
          MinZoomLevel = projectEvent.MinZoomLevel,
          MaxZoomLevel = projectEvent.MaxZoomLevel,
          LastActionedUtc = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertImportedFile(importedFile, "UpdateImportedFileEvent");
      }
      else if (evt is DeleteImportedFileEvent)
      {
        var projectEvent = (DeleteImportedFileEvent) evt;
        var importedFile = new ImportedFile
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ImportedFileUid = projectEvent.ImportedFileUID.ToString(),
          LastActionedUtc = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertImportedFile(importedFile, "DeleteImportedFileEvent",
          projectEvent.DeletePermanently);
      }
      else if (evt is UndeleteImportedFileEvent)
      {
        var projectEvent = (UndeleteImportedFileEvent) evt;
        var importedFile = new ImportedFile
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ImportedFileUid = projectEvent.ImportedFileUID.ToString(),
          LastActionedUtc = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertImportedFile(importedFile, "UndeleteImportedFileEvent");
      }
      else if (evt is UpdateProjectSettingsEvent)
      {
        var projectEvent = (UpdateProjectSettingsEvent) evt;
        var projectSettings = new ProjectSettings
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ProjectSettingsType = projectEvent.ProjectSettingsType,
          Settings = projectEvent.Settings,
          UserID = projectEvent.UserID,
          LastActionedUtc = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectSettings(projectSettings);
      }

      return upsertedCount;
    }

    #endregion store


    #region project

    private string GetPolygonWKT(string boundary)
    {
      const string polygonStr = "POLYGON";
      var boundaryWkt = string.Empty;

      if (!string.IsNullOrEmpty(boundary))
      {
        // Check whether the ProjectBoundary is in WKT format. Convert to the WKT format if it is not. 
        if (!boundary.Contains(polygonStr))
        {
          boundary =
            boundary.Replace(",", " ").Replace(";", ",").TrimEnd(',');
          boundary =
            string.Concat(polygonStr + "((", boundary, "))");
        }
        //Polygon must start and end with the same point

        boundaryWkt = boundary.ParseGeometryData().ClosePolygonIfRequired()
          .ToPolygonWKT();
      }

      return boundaryWkt;
    }

    /// <summary>
    ///     All detail-related columns can be inserted,
    ///     but only certain columns can be updated.
    ///     on deletion, a flag will be set.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="eventType"></param>
    /// <returns></returns>
    private async Task<int> UpsertProjectDetail(Project project, string eventType, bool isDeletePermanently = false)
    {
      var upsertedCount = 0;
      var existing = (await QueryWithAsyncPolicy<Project>
      (@"SELECT 
                ProjectUID, Description, LegacyProjectID, Name, fk_ProjectTypeID AS ProjectType, IsDeleted,
                ProjectTimeZone, LandfillTimeZone, 
                LastActionedUTC, StartDate, EndDate, GeometryWKT,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC
              FROM Project
              WHERE ProjectUID = @ProjectUID
                OR LegacyProjectId = @LegacyProjectID",
        new { ProjectUID = project.ProjectUID, LegacyProjectID = project.LegacyProjectID}
      )).FirstOrDefault();

      if (eventType == "CreateProjectEvent")
        upsertedCount = await CreateProject(project, existing);

      if (eventType == "UpdateProjectEvent")
        upsertedCount = await UpdateProject(project, existing);

      if (eventType == "DeleteProjectEvent")
        upsertedCount = await DeleteProject(project, existing, isDeletePermanently);
      return upsertedCount;
    }

    private async Task<int> CreateProject(Project project, Project existing)
    {
      var upsertedCount = 0;
      log.LogDebug($"ProjectRepository/CreateProject: project={JsonConvert.SerializeObject(project)}))')");

      if (project.StartDate > project.EndDate)
      {
        log.LogDebug("Project will not be created as startDate > endDate");
        return upsertedCount;
      }

      if (existing == null)
      {
        string insert = BuildProjectInsertString(project);

        upsertedCount = await ExecuteWithAsyncPolicy(insert, project);
        log.LogDebug($"ProjectRepository/CreateProject: (insert): inserted {upsertedCount} rows");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
        }

        return upsertedCount;
      }

      // a delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
      //       update everything but ActionUTC from the create
      if ((existing.LastActionedUTC >= project.LastActionedUTC) && existing.IsDeleted == true)
      {
        project.IsDeleted = true;

        // this create could have the legit legacyProjectId
        project.LegacyProjectID =
          project.LegacyProjectID > 0 && project.LegacyProjectID < LegacyProjectIdCutoff ? project.LegacyProjectID : existing.LegacyProjectID;

        // leave more recent values
        project.Name = string.IsNullOrEmpty(existing.Name) ? project.Name : existing.Name;
        project.Description = string.IsNullOrEmpty(existing.Description) ? project.Description : existing.Description;
        project.ProjectTimeZone = string.IsNullOrEmpty(existing.ProjectTimeZone) ? project.ProjectTimeZone : existing.ProjectTimeZone;
        project.LandfillTimeZone = string.IsNullOrEmpty(existing.LandfillTimeZone) ? project.LandfillTimeZone : existing.LandfillTimeZone;
        project.StartDate = existing.StartDate == DateTime.MinValue ? project.StartDate : existing.StartDate;
        project.EndDate = existing.EndDate == DateTime.MinValue ? project.EndDate : existing.EndDate;
        project.LastActionedUTC = existing.LastActionedUTC;

        if (!string.IsNullOrEmpty(existing.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
        }
        project.GeometryWKT = string.IsNullOrEmpty(existing.GeometryWKT) ? project.GeometryWKT : existing.GeometryWKT;
        
        string update = BuildProjectUpdateString(project);
        log.LogDebug("ProjectRepository/CreateProject: going to update a dummy project");

        upsertedCount = await ExecuteWithAsyncPolicy(update, project);
        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
        }

        log.LogDebug($"ProjectRepository/CreateProject: (update): updated {upsertedCount} rows ");
        return upsertedCount;
      }

      // an update was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
      // leave the more recent EndDate, Name, Description, ProjectType and actionUTC alone
      if (existing.LastActionedUTC >= project.LastActionedUTC)
      {
        log.LogDebug("ProjectRepository/CreateProject: create arrived after an update so updating project");

        // this create could have the legit legacyProjectId
        project.LegacyProjectID =
          project.LegacyProjectID > 0 && project.LegacyProjectID < LegacyProjectIdCutoff ? project.LegacyProjectID : existing.LegacyProjectID;

        // leave more recent values
        project.Name = string.IsNullOrEmpty(existing.Name) ? project.Name : existing.Name;
        project.Description = string.IsNullOrEmpty(existing.Description) ? project.Description : existing.Description;
        project.ProjectTimeZone = string.IsNullOrEmpty(existing.ProjectTimeZone) ? project.ProjectTimeZone : existing.ProjectTimeZone;
        project.LandfillTimeZone = string.IsNullOrEmpty(existing.LandfillTimeZone) ? project.LandfillTimeZone : existing.LandfillTimeZone;
        project.StartDate = existing.StartDate == DateTime.MinValue ? project.StartDate : existing.StartDate;
        project.EndDate = existing.EndDate == DateTime.MinValue ? project.EndDate : existing.EndDate;
        project.LastActionedUTC = existing.LastActionedUTC;

        if (!string.IsNullOrEmpty(existing.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
        }
        project.GeometryWKT = string.IsNullOrEmpty(existing.GeometryWKT) ? project.GeometryWKT : existing.GeometryWKT;

        string update = BuildProjectUpdateString(project);
        upsertedCount = await ExecuteWithAsyncPolicy(update, project);
        log.LogDebug($"ProjectRepository/CreateProject: (updateExisting): updated {upsertedCount} rows");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
        }

        return upsertedCount;
      }

      log.LogDebug("ProjectRepository/CreateProject: No action as project already exists.");
      return upsertedCount;
    }

    private async Task<int> UpdateProject(Project project, Project existing)
    {
      log.LogDebug($"ProjectRepository/UpdateProject: project={JsonConvert.SerializeObject(project)}))')");

      var upsertedCount = 0;
      if (existing != null)
      {
        if (project.EndDate < existing.StartDate)
        {
          log.LogDebug(
            $"ProjectRepository/UpdateProject: failed to update project={project.ProjectUID} EndDate < StartDate");
          return upsertedCount;
        }

        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          project.LegacyProjectID = existing.LegacyProjectID;
          project.Name = string.IsNullOrEmpty(project.Name) ? existing.Name : project.Name;
          project.Description = string.IsNullOrEmpty(project.Description) ? existing.Description : project.Description;
          project.ProjectTimeZone = string.IsNullOrEmpty(project.ProjectTimeZone) ? existing.ProjectTimeZone : project.ProjectTimeZone;
          project.LandfillTimeZone = string.IsNullOrEmpty(project.LandfillTimeZone) ? existing.LandfillTimeZone : project.LandfillTimeZone;
          project.StartDate = project.StartDate == DateTime.MinValue ? existing.StartDate : project.StartDate;

          if (string.IsNullOrEmpty(project.CoordinateSystemFileName))
          {
            project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
            project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
          }
          project.GeometryWKT = string.IsNullOrEmpty(project.GeometryWKT) ? existing.GeometryWKT : project.GeometryWKT;

          log.LogDebug($"ProjectRepository/UpdateProject: updating project={project.ProjectUID}");

          string update = BuildProjectUpdateString(project);
          upsertedCount = await ExecuteWithAsyncPolicy(update, project);
          log.LogDebug(
            $"ProjectRepository/UpdateProject: upserted {upsertedCount} rows for: projectUid:{project.ProjectUID}");

          if (upsertedCount > 0)
          {
            upsertedCount = await InsertProjectHistory(project);
          }
          return upsertedCount;
        }

        log.LogDebug($"ProjectRepository/UpdateProject: old update event ignored project={project.ProjectUID}");
      }
      else
      {
        // an update was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        log.LogDebug(
          $"ProjectRepository/UpdateProject: project doesn't already exist, creating one. project={project.ProjectUID}");
        if (String.IsNullOrEmpty(project.ProjectTimeZone))
          project.ProjectTimeZone = "";
        
        string insert = BuildProjectInsertString(project);
        upsertedCount = await ExecuteWithAsyncPolicy(insert, project);
        log.LogDebug($"ProjectRepository/UpdateProject: (insert): inserted {upsertedCount} rows");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
        }

        return upsertedCount;
      }

      return upsertedCount;
    }

    private async Task<int> DeleteProject(Project project, Project existing, bool isDeletePermanently)
    {
      log.LogDebug(
        $"ProjectRepository/DeleteProject: project={JsonConvert.SerializeObject(project)} permanently: {isDeletePermanently}))')");

      var upsertedCount = 0;
      if (existing != null)
      {
        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          // this is for internal use only to roll-back after failed series of steps
          if (isDeletePermanently)
          {
            log.LogDebug(
              $"ProjectRepository/DeleteProject: deleting a project permanently: {JsonConvert.SerializeObject(project)}");
            const string delete =
              @"DELETE FROM Project
                    WHERE ProjectUID = @ProjectUID";
            upsertedCount = await ExecuteWithAsyncPolicy(delete, project);
            log.LogDebug(
              $"ProjectRepository/DeleteProject: deleted {upsertedCount} rows for: projectUid:{project.ProjectUID}");

            return upsertedCount;
          }
          else
          {
            log.LogDebug($"ProjectRepository/DeleteProject: updating project={project.ProjectUID}");

            const string update =
              @"UPDATE Project                
                  SET IsDeleted = 1,
                    LastActionedUTC = @LastActionedUTC
                  WHERE ProjectUID = @ProjectUID";
            upsertedCount = await ExecuteWithAsyncPolicy(update, project);
            log.LogDebug(
              $"ProjectRepository/DeleteProject: upserted {upsertedCount} rows for: projectUid:{project.ProjectUID}");

            if (upsertedCount > 0)
            {
              upsertedCount = await InsertProjectHistory(project);
            }
            return upsertedCount;
          }
        }
      }
      else
      {
        // a delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        log.LogDebug(
          $"ProjectRepository/DeleteProject: delete event where no project exists, creating one. project={project.ProjectUID}");
        project.Name = "";
        project.ProjectTimeZone = "";
        project.LandfillTimeZone = "";
        project.ProjectType = ProjectType.Standard;

        const string delete =
          "INSERT Project " +
          "    (ProjectUID, Name, fk_ProjectTypeID, IsDeleted, ProjectTimeZone, LandfillTimeZone, LastActionedUTC)" +
          "  VALUES " +
          "    (@ProjectUID, @Name, @ProjectType, 1, @ProjectTimeZone, @LandfillTimeZone, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(delete, project);
        log.LogDebug(
          $"ProjectRepository/DeleteProject: inserted {upsertedCount} rows for: projectUid:{project.ProjectUID}");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
        }
        return upsertedCount;
      }

      return upsertedCount;
    }


    private string BuildProjectInsertString(Project project)
    {
      string formattedPolygon = null;
      if (!string.IsNullOrEmpty(project.GeometryWKT))
        formattedPolygon = $"ST_GeomFromText('{project.GeometryWKT}')";

      string insert = null;
      if (project.LegacyProjectID <= 0) // allow db autoincrement on legacyProjectID
        insert = string.Format(
          "INSERT Project " +
          "    (ProjectUID, Name, Description, fk_ProjectTypeID, IsDeleted, ProjectTimeZone, LandfillTimeZone, LastActionedUTC, StartDate, EndDate, GeometryWKT, PolygonST, CoordinateSystemFileName, CoordinateSystemLastActionedUTC) " +
          "  VALUES " +
          "    (@ProjectUID, @Name, @Description, @ProjectType, @IsDeleted, @ProjectTimeZone, @LandfillTimeZone, @LastActionedUTC, @StartDate, @EndDate, @GeometryWKT, {0}, @CoordinateSystemFileName, @CoordinateSystemLastActionedUTC)"
          , formattedPolygon ?? "null");
      else
        insert = string.Format(
          "INSERT Project " +
          "    (ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID, IsDeleted, ProjectTimeZone, LandfillTimeZone, LastActionedUTC, StartDate, EndDate, GeometryWKT, PolygonST, CoordinateSystemFileName, CoordinateSystemLastActionedUTC ) " +
          "  VALUES " +
          "    (@ProjectUID, @LegacyProjectID, @Name, @Description, @ProjectType, @IsDeleted, @ProjectTimeZone, @LandfillTimeZone, @LastActionedUTC, @StartDate, @EndDate, @GeometryWKT, {0}, @CoordinateSystemFileName, @CoordinateSystemLastActionedUTC)"
          , formattedPolygon ?? "null");
      return insert;
    }

    private string BuildProjectUpdateString(Project project)
    {
      string formattedPolygon = null;
      if (!string.IsNullOrEmpty(project.GeometryWKT))
        formattedPolygon = $"ST_GeomFromText('{project.GeometryWKT}')";

      string update = null;
      if (project.LegacyProjectID <= 0) // allow db autoincrement on legacyProjectID
      {
        update = string.Format(
          @"UPDATE Project
                SET 
                  Name = @Name, Description = @Description, fk_ProjectTypeID = @ProjectType,
                  IsDeleted = @IsDeleted,
                  ProjectTimeZone = @ProjectTimeZone, LandfillTimeZone = @LandfillTimeZone,
                  LastActionedUTC = @LastActionedUTC,
                  StartDate = @StartDate, EndDate = @EndDate,   
                  CoordinateSystemFileName = @CoordinateSystemFileName,
                  CoordinateSystemLastActionedUTC = @CoordinateSystemLastActionedUTC,
                  GeometryWKT = '{0}', PolygonST = {1}
                WHERE ProjectUID = @ProjectUID"
          , project.GeometryWKT, formattedPolygon ?? "null");
      }
      else
      {
        update = string.Format(
          @"UPDATE Project
                SET LegacyProjectID = @LegacyProjectID, 
                  Name = @Name, Description = @Description, fk_ProjectTypeID = @ProjectType,
                  IsDeleted = @IsDeleted,
                  ProjectTimeZone = @ProjectTimeZone, LandfillTimeZone = @LandfillTimeZone,
                  LastActionedUTC = @LastActionedUTC,
                  StartDate = @StartDate, EndDate = @EndDate,   
                  CoordinateSystemFileName = @CoordinateSystemFileName,
                  CoordinateSystemLastActionedUTC = @CoordinateSystemLastActionedUTC,
                  GeometryWKT = '{0}', PolygonST = {1}
                WHERE ProjectUID = @ProjectUID"
          , project.GeometryWKT, formattedPolygon ?? "null");
      }
      return update;
    }

    #endregion project


    #region associate

    private async Task<int> UpsertCustomerProjectDetail(CustomerProject customerProject, string eventType)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<CustomerProject>
      (@"SELECT 
                fk_CustomerUID AS CustomerUID, LegacyCustomerID, fk_ProjectUID AS ProjectUID, LastActionedUTC
              FROM CustomerProject
              WHERE fk_CustomerUID = @CustomerUID 
                AND fk_ProjectUID = @ProjectUID",
        new { CustomerUID = customerProject.CustomerUID, ProjectUID = customerProject.ProjectUID}
      )).FirstOrDefault();

      if (eventType == "AssociateProjectCustomerEvent")
        upsertedCount = await AssociateProjectCustomer(customerProject, existing);
      if (eventType == "DissociateProjectCustomerEvent")
        upsertedCount = await DissociateProjectCustomer(customerProject, existing);
      return upsertedCount;
    }

    private async Task<int> AssociateProjectCustomer(CustomerProject customerProject, CustomerProject existing)
    {
      log.LogDebug(
        $"ProjectRepository/AssociateProjectCustomer: customerProject={JsonConvert.SerializeObject(customerProject)}");

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

      var upsertedCount = await ExecuteWithAsyncPolicy(insert, customerProject);
      log.LogDebug(
        $"ProjectRepository/AssociateProjectCustomer: upserted {upsertedCount} rows (1=insert, 2=update) for: customerProjectUid:{customerProject.CustomerUID}");
      return upsertedCount.CalculateUpsertCount();
    }

    private async Task<int> DissociateProjectCustomer(CustomerProject customerProject, CustomerProject existing)
    {
      var upsertedCount = 0;

      log.LogDebug(
        $"ProjectRepository/DissociateProjectCustomer: customerProject={JsonConvert.SerializeObject(customerProject)} existing={JsonConvert.SerializeObject(existing)}");

      if (existing != null)
      {
        if (customerProject.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string delete =
            @"DELETE FROM CustomerProject
                WHERE fk_CustomerUID = @CustomerUID 
                  AND fk_ProjectUID = @ProjectUID";
          upsertedCount = await ExecuteWithAsyncPolicy(delete, customerProject);
          log.LogDebug(
            $"ProjectRepository/DissociateProjectCustomer: upserted {upsertedCount} rows for: customerUid:{customerProject.CustomerUID}");
          return upsertedCount;
        }

        // may have been associated again since, so don't delete
        log.LogDebug("ProjectRepository/DissociateProjectCustomer: old delete event ignored");
      }
      else
      {
        log.LogDebug("ProjectRepository/DissociateProjectCustomer: can't delete as none existing");
      }

      return upsertedCount;
    }

    private async Task<int> UpsertProjectGeofenceDetail(ProjectGeofence projectGeofence, string eventType)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<ProjectGeofence>
      (@"SELECT 
              fk_GeofenceUID AS GeofenceUID, fk_ProjectUID AS ProjectUID, LastActionedUTC
            FROM ProjectGeofence
            WHERE fk_ProjectUID = @ProjectUID AND fk_GeofenceUID = @GeofenceUID",
        new { ProjectUID = projectGeofence.ProjectUID, GeofenceUID = projectGeofence.GeofenceUID}
      )).FirstOrDefault();

      if (eventType == "AssociateProjectGeofenceEvent")
        upsertedCount = await AssociateProjectGeofence(projectGeofence, existing);
      if (eventType == "DissociateProjectGeofenceEvent")
        upsertedCount = await DissociateProjectGeofence(projectGeofence, existing);

      return upsertedCount;
    }

    private async Task<int> AssociateProjectGeofence(ProjectGeofence projectGeofence, ProjectGeofence existing)
    {
      var upsertedCount = 0;
      if (existing == null)
      {
        log.LogDebug(
          $"ProjectRepository/AssociateProjectGeofence: projectGeofence={JsonConvert.SerializeObject(projectGeofence)}");

        const string insert =
          @"INSERT ProjectGeofence
                (fk_GeofenceUID, fk_ProjectUID, LastActionedUTC)
              VALUES
                (@GeofenceUID, @ProjectUID, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, projectGeofence);
        log.LogDebug(
          $"ProjectRepository/AssociateProjectGeofence: inserted {upsertedCount} rows for: projectUid:{projectGeofence.ProjectUID} geofenceUid:{projectGeofence.GeofenceUID}");

        return upsertedCount;
      }

      log.LogDebug(
        $"ProjectRepository/AssociateProjectGeofence: can't create as already exists projectGeofence={JsonConvert.SerializeObject(projectGeofence)}");
      return upsertedCount;
    }

    private async Task<int> DissociateProjectGeofence(ProjectGeofence projectGeofence, ProjectGeofence existing)
    {
      var upsertedCount = 0;

      log.LogDebug(
        $"ProjectRepository/DissociateProjectGeofence: projectGeofence={JsonConvert.SerializeObject(projectGeofence)} existing={JsonConvert.SerializeObject(existing)}");

      if (existing != null)
      {
        if (projectGeofence.LastActionedUTC >= existing.LastActionedUTC)
        {
          const string delete =
            @"DELETE FROM ProjectGeofence
                WHERE fk_GeofenceUID = @GeofenceUID 
                  AND fk_ProjectUID = @ProjectUID";
          upsertedCount = await ExecuteWithAsyncPolicy(delete, projectGeofence);
          log.LogDebug(
            $"ProjectRepository/DissociateProjectGeofence: upserted {upsertedCount} rows for: geofenceUid:{projectGeofence.GeofenceUID}");
          return upsertedCount;
        }

        // may have been associated again since, so don't delete
        log.LogDebug("ProjectRepository/DissociateProjectGeofence: old delete event ignored");
      }
      else
      {
        log.LogDebug("ProjectRepository/DissociateProjectGeofence: can't delete as none existing");
      }

      return upsertedCount;
    }

    #endregion associate


    #region importedFiles

    private async Task<int> UpsertImportedFile(ImportedFile importedFile, string eventType,
      bool isDeletePermanently = false)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<ImportedFile>
      (@"SELECT 
              fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID as CustomerUID,
              fk_ImportedFileTypeID as ImportedFileType, Name, 
              FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, 
              fk_DXFUnitsTypeID as DxfUnitsType, MinZoomLevel, MaxZoomLevel,
              IsDeleted, LastActionedUTC
            FROM ImportedFile
            WHERE ImportedFileUID = @ImportedFileUid", new { ImportedFileUid = importedFile.ImportedFileUid}
      )).FirstOrDefault();

      if (eventType == "CreateImportedFileEvent")
        upsertedCount = await CreateImportedFile(importedFile, existing);

      if (eventType == "UpdateImportedFileEvent")
        upsertedCount = await UpdateImportedFile(importedFile, existing);

      if (eventType == "DeleteImportedFileEvent")
        upsertedCount = await DeleteImportedFile(importedFile, existing, isDeletePermanently);

      if (eventType == "UndeleteImportedFileEvent")
        upsertedCount = await UndeleteImportedFile(importedFile, existing);

      return upsertedCount;
    }

    private async Task<int> CreateImportedFile(ImportedFile importedFile, ImportedFile existing)
    {
      var upsertedCount = 0;

      if (existing == null)
      {
        log.LogDebug(
          $"ProjectRepository/CreateImportedFile: going to create importedFile={JsonConvert.SerializeObject(importedFile)}");

        var insert = string.Format(
          "INSERT ImportedFile " +
          "    (fk_ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID, MinZoomLevel, MaxZoomLevel, IsDeleted, LastActionedUTC) " +
          "  VALUES " +
          "    (@ProjectUid, @ImportedFileUid, @ImportedFileId, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUtc, @FileUpdatedUtc, @ImportedBy, @SurveyedUtc, @DxfUnitsType, @MinZoomLevel, @MaxZoomLevel, 0, @LastActionedUtc)");

        upsertedCount = await ExecuteWithAsyncPolicy(insert, importedFile);
        log.LogDebug(
          $"ProjectRepository/CreateImportedFile: (insert): inserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");

        if (upsertedCount > 0)
          upsertedCount = await UpsertImportedFileHistory(importedFile);
      }
      else if (existing.LastActionedUtc >= importedFile.LastActionedUtc)
      {
        // an update/delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        // The only thing which can be updated is a) the file content, and the LastActionedUtc. A file cannot be moved between projects/customers.
        // We don't store (a), and leave actionUTC as the more recent. 

        log.LogDebug(
          $"ProjectRepository/CreateImportedFile: create arrived after an update so inserting importedFile={importedFile.ImportedFileUid}");

        const string update =
          @"UPDATE ImportedFile
              SET fk_ProjectUID = @ProjectUid, 
                ImportedFileID = @ImportedFileId,
                fk_CustomerUID = @CustomerUid,
                fk_ImportedFileTypeID = @ImportedFileType,
                Name = @Name,
                FileDescriptor = @FileDescriptor,
                FileCreatedUTC = @FileCreatedUtc,
                FileUpdatedUTC = @FileUpdatedUtc,
                ImportedBy = @ImportedBy, 
                SurveyedUTC = @SurveyedUtc,
                MinZoomLevel = @MinZoomLevel,
                MaxZoomLevel = @MaxZoomLevel,
                fk_DXFUnitsTypeID = @DxfUnitsType
              WHERE ImportedFileUID = @ImportedFileUid";

        upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
        log.LogDebug(
          $"ProjectRepository/CreateImportedFile: (updateExisting): upserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");

        // don't really care if this didn't pass as may already exist for create/update utc
        if (upsertedCount > 0)
          await UpsertImportedFileHistory(importedFile);
      }
      else
      {

        log.LogDebug(
          $"ProjectRepository/CreateImportedFile: can't create as older actioned importedFile already exists: {importedFile.ImportedFileUid}.");
      }

      return upsertedCount;
    }

    private async Task<int> UpdateImportedFile(ImportedFile importedFile, ImportedFile existing)
    {
      // The only thing which can be updated is a) the file content, and the LastActionedUtc. A file cannot be moved between projects/customers.
      // We don't store (a), and leave actionUTC as the more recent. 
      var upsertedCount = 0;
      if (existing != null)
      {
        if (importedFile.LastActionedUtc >= existing.LastActionedUtc)
        {
          const string update =
            @"UPDATE ImportedFile
                SET 
                  FileDescriptor = @FileDescriptor,
                  FileCreatedUTC = @FileCreatedUtc,
                  FileUpdatedUTC = @FileUpdatedUtc,
                  ImportedBy = @ImportedBy, 
                  SurveyedUTC = @SurveyedUtc,
                  MinZoomLevel = @MinZoomLevel,
                  MaxZoomLevel = @MaxZoomLevel,
                  LastActionedUTC = @LastActionedUtc
                WHERE ImportedFileUID = @ImportedFileUid";

          upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
          log.LogDebug(
            $"ProjectRepository/UpdateImportedFile: updated {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");

          // don't really care if this didn't pass as may already exist for create/update utc
          if (upsertedCount > 0)
            await UpsertImportedFileHistory(importedFile);
        }

        log.LogDebug(
          $"ProjectRepository/UpdateImportedFile: old update event ignored importedFile {importedFile.ImportedFileUid}");
      }
      else
      {
        // can't create as don't know fk_ImportedFileTypeID, fk_DXFUnitsTypeID or customerUID
        log.LogDebug(
          $"ProjectRepository/UpdateImportedFile: No ImportedFile exists {importedFile.ImportedFileUid}. Can't create one as don't have enough info e.g. customerUID/type");
      }

      return upsertedCount;
    }

    /// <summary>
    /// Round date time to nearest second
    /// </summary>
    private DateTime RoundDateTimeToSeconds(DateTime dateTime)
    {
      return DateTime.Parse(dateTime.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    private async Task<int> InsertProjectHistory(Project project)
    {
      var insertedCount = 0;
      var insert = string.Format(
        @"INSERT INTO ProjectHistory
              (ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID,
                IsDeleted, ProjectTimeZone, LandfillTimeZone, StartDate, EndDate,
                GeometryWKT, PolygonST,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC,
                LastActionedUTC)
              SELECT 
                  ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID,
                  IsDeleted, ProjectTimeZone, LandfillTimeZone, StartDate, EndDate,
                  GeometryWKT, PolygonST,
                  CoordinateSystemFileName, CoordinateSystemLastActionedUTC,
                  LastActionedUTC
                FROM Project
                WHERE ProjectUID = @ProjectUID;");
      insertedCount = await ExecuteWithAsyncPolicy(insert, project);
      log.LogDebug($"ProjectRepository/CreateProjectHistory: inserted {insertedCount} rows");
      return insertedCount;
    }

    private async Task<int> UpsertImportedFileHistory(ImportedFile importedFile)
    {
      var insertedCount = 0;
      var importedFileHistoryExisting = (await QueryWithAsyncPolicy<ImportedFileHistoryItem>
      (@"SELECT 
            fk_ImportedFileUID AS ImportedFileUid, FileCreatedUTC, FileUpdatedUTC, ImportedBy
          FROM ImportedFileHistory
            WHERE fk_ImportedFileUID = @ImportedFileUid",
        new { ImportedFileUid = importedFile.ImportedFileUid}
      )).ToList();

      bool alreadyExists = false;
      // comparing sql dateTimes to c# doesn't work
      if (importedFileHistoryExisting.Any())
      {
        var newCreatedUtcRounded = RoundDateTimeToSeconds(importedFile.FileCreatedUtc);
        var newUpdatedUtcRounded = RoundDateTimeToSeconds(importedFile.FileUpdatedUtc);

        alreadyExists = importedFileHistoryExisting
          .Any(h => RoundDateTimeToSeconds(h.FileCreatedUtc) == newCreatedUtcRounded &&
                    RoundDateTimeToSeconds(h.FileUpdatedUtc) == newUpdatedUtcRounded);
      }

      if (!alreadyExists)
      {
        const string insert =
          @"INSERT ImportedFileHistory
                 (fk_ImportedFileUID, FileCreatedUtc, FileUpdatedUtc, ImportedBy)
            VALUES
              (@ImportedFileUid, @FileCreatedUtc, @FileUpdatedUtc, @ImportedBy)";

        insertedCount = await ExecuteWithAsyncPolicy(insert, importedFile);

        log.LogDebug(
          $"ProjectRepository/UpsertImportedFileHistory: inserted {insertedCount} rows for: ImportedFileUid:{importedFile.ImportedFileUid} FileCreatedUTC: {importedFile.FileCreatedUtc} FileUpdatedUTC: {importedFile.FileUpdatedUtc}");
      }
      else
      {
        log.LogDebug(
          $"ProjectRepository/UpsertImportedFileHistory: History already exists ImportedFileUid:{importedFile.ImportedFileUid} FileCreatedUTC: {importedFile.FileCreatedUtc} FileUpdatedUTC: {importedFile.FileUpdatedUtc}");
      }

      return insertedCount;
    }

    private async Task<int> DeleteImportedFile(ImportedFile importedFile, ImportedFile existing,
      bool isDeletePermanently)
    {
      log.LogDebug(
        $"ProjectRepository/DeleteImportedFile: deleting importedFile: {JsonConvert.SerializeObject(importedFile)} permanent flag:{isDeletePermanently}");
      var upsertedCount = 0;
      if (existing != null)
      {
        if (importedFile.LastActionedUtc >= existing.LastActionedUtc)
        {
          if (isDeletePermanently)
          {
            log.LogDebug(
              $"ProjectRepository/DeleteImportedFile: deleting importedFile permanently: {importedFile.ImportedFileUid}");
            const string delete =
              @"DELETE FROM ImportedFile
                  WHERE ImportedFileUID = @ImportedFileUid";
            upsertedCount = await ExecuteWithAsyncPolicy(delete, importedFile);
            log.LogDebug(
              $"ProjectRepository/DeleteImportedFile: deleted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");
            return upsertedCount;
          }
          else
          {
            log.LogDebug($"ProjectRepository/DeleteImportedFile: deleting importedFile {importedFile.ImportedFileUid}");

            const string update =
              @"UPDATE ImportedFile                               
                SET IsDeleted = 1,
                    LastActionedUTC = @LastActionedUtc
                WHERE ImportedFileUID = @ImportedFileUid";

            upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
            log.LogDebug(
              $"ProjectRepository/DeleteImportedFile: upserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");
            return upsertedCount;
          }
        }
      }
      else
      {
        log.LogDebug(
          $"ProjectRepository/DeleteImportedFile: can't delete as none existing, ignored. importedFile={importedFile.ImportedFileUid}. Can't create one as don't have enough info e.g.customerUID / type.");
      }

      return upsertedCount;
    }

    private async Task<int> UndeleteImportedFile(ImportedFile importedFile, ImportedFile existing)
    {
      // this is an interfaces extension model used solely by ProjectMDM to allow a rollback of a DeleteImportedFile
      log.LogDebug(
        $"ProjectRepository/UndeleteImportedFile: undeleting importedFile: {JsonConvert.SerializeObject(importedFile)}.");
      var upsertedCount = 0;

      if (existing != null)
      {
        log.LogDebug($"ProjectRepository/UndeleteImportedFile: undeleting importedFile {importedFile.ImportedFileUid}");

        const string update =
          @"UPDATE ImportedFile                               
                SET IsDeleted = 0
              WHERE ImportedFileUID = @ImportedFileUid";

        upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
        log.LogDebug(
          $"ProjectRepository/UndeleteImportedFile: upserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");
        return upsertedCount;
      }

      log.LogDebug(
        $"ProjectRepository/UndeleteImportedFile: can't undelete as none existing ignored importedFile={importedFile.ImportedFileUid}.");
      return upsertedCount;
    }

    #endregion importedFiles


    #region projectSettings

    /// <summary>
    ///     Only an upsert is implemented.
    /// 1) because as that is the only endpoint in ProjectMDM
    /// 2) because create and Update have to cover both scenarios anyway
    /// can't update the type or UserID, only the Settings
    /// </summary>
    /// <param name="projectSettings"></param>
    /// <returns></returns>
    private async Task<int> UpsertProjectSettings(ProjectSettings projectSettings)
    {
      log.LogDebug(
        $"ProjectRepository/UpsertProjectSettings: projectSettings={JsonConvert.SerializeObject(projectSettings)}))')");

      const string upsert =
        @"INSERT ProjectSettings
                 (fk_ProjectUID, fk_ProjectSettingsTypeID, Settings, UserID, LastActionedUTC)
            VALUES
              (@ProjectUid, @ProjectSettingsType, @Settings, @UserID, @LastActionedUtc)
            ON DUPLICATE KEY UPDATE
              LastActionedUTC =
                IF ( VALUES(LastActionedUtc) >= LastActionedUTC, 
                    VALUES(LastActionedUtc), LastActionedUTC),
              Settings =
                IF ( VALUES(LastActionedUtc) >= LastActionedUTC, 
                    VALUES(Settings), Settings)";

      var upsertedCount = await ExecuteWithAsyncPolicy(upsert, projectSettings);
      log.LogDebug(
        $"ProjectRepository/UpsertProjectSettings: upserted {upsertedCount} rows (1=insert, 2=update) for: projectSettingsProjectUid:{projectSettings.ProjectUid}");
      return upsertedCount.CalculateUpsertCount();
    }

    #endregion projectSettings


    #region gettersProject

    /// <summary>
    ///     There may be 0 or n subscriptions for this project. None/many may be current.
    ///     This method just gets ANY one of these or no subs (SubscriptionUID == null)
    ///     We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<Project> GetProject(string projectUid)
    {
      var project = (await QueryWithAsyncPolicy<Project>(@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE p.ProjectUID = @ProjectUID 
                AND p.IsDeleted = 0",
        new { ProjectUID = projectUid })).FirstOrDefault();
      return project;
    }

    /// <summary>
    ///     Gets by legacyProjectID. No subs
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<Project> GetProject(long legacyProjectID)
    {
      var project = await QueryWithAsyncPolicy<Project>(@"SELECT
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
              WHERE p.LegacyProjectID = @LegacyProjectID 
                AND p.IsDeleted = 0",
        new { LegacyProjectID = legacyProjectID });
      return project.FirstOrDefault();
    }


    /// <summary>
    ///     There may be 0 or n subscriptions for this project. None/many may be current.
    ///     This method just gets ANY one of these or no subs (SubscriptionUID == null)
    ///     We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Project>> GetProjectAndSubscriptions(long legacyProjectID, DateTime validAtDate)
    {
      var projectSubList = await QueryWithAsyncPolicy<Project>
      (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID,
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID
              WHERE p.LegacyProjectID = @LegacyProjectID 
                AND p.IsDeleted = 0
                AND @validAtDate BETWEEN s.StartDate AND s.EndDate",
        new { LegacyProjectID = legacyProjectID, validAtDate = validAtDate.Date}
      );


      return projectSubList;
    }

    /// <summary>
    ///     There should be 1 or more per ProjectUID
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Project>> GetProjectHistory(string projectUid)
    {
      var projectList = await QueryWithAsyncPolicy<Project>(@"SELECT 
                ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID as ProjectType, 
                IsDeleted, ProjectTimeZone, LandfillTimeZone, StartDate, EndDate, 
                GeometryWKT,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC,
                LastActionedUTC 
              FROM ProjectHistory             
              WHERE ProjectUID = @ProjectUID",
        new { ProjectUID = projectUid });
      return projectList;
    }

    /// <summary>
    ///     gets only 1 row for a particular sub. only 1 projectUID and be associated with a sub
    /// </summary>
    /// <param name="subscriptionUid"></param>
    /// <returns></returns>
    public async Task<Project> GetProjectBySubcription(string subscriptionUid)
    {
      var projects = (await QueryWithAsyncPolicy<Project>
      (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE ps.fk_SubscriptionUID = @SubscriptionUID 
                AND p.IsDeleted = 0",
        new { SubscriptionUID = subscriptionUid }
      )).FirstOrDefault();
      ;


      return projects;
    }


    /// <summary>
    ///     There may be 0 or n subscriptions for each project. None/many may be current.
    ///     This method just gets ANY one of these or no subs (SubscriptionUID == null)
    ///     We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="userUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Project>> GetProjectsForUser(string userUid)
    {
      var projects = await QueryWithAsyncPolicy<Project>
      (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID,
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                JOIN CustomerUser cu on cu.fk_CustomerUID = c.CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE cu.UserUID = @userUid 
                AND p.IsDeleted = 0",
        new {userUid}
      );


      return projects;
    }

    /// <summary>
    ///     There may be 0 or n subscriptions for each project. None/many may be current.
    ///     This method just gets ANY one of these or no subs (SubscriptionUID == null)
    ///     We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="userUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Project>> GetProjectsForCustomerUser(string customerUid, string userUid)
    {
      var projects = await QueryWithAsyncPolicy<Project>
      (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                JOIN CustomerUser cu ON cu.fk_CustomerUID = c.CustomerUID
                LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE cp.fk_CustomerUID = @CustomerUID 
                AND cu.UserUID = @userUid 
                AND p.IsDeleted = 0",
        new { CustomerUID = customerUid, userUid}
      );


      return projects;
    }

    /// <summary>
    ///     There may be 0 or n subscriptions for each project. None/many may be current.
    ///     This method gets the latest EndDate so at most 1 sub per project
    ///     Also returns the GeofenceWRK. List returned includes archived projects.
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="userUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Project>> GetProjectsForCustomer(string customerUid)
    {
      // mysql doesn't have any nice mssql features like rowNumber/paritionBy, so quicker to do in c#
      var projects = await QueryWithAsyncPolicy<Project>
      (@"SELECT 
              c.CustomerUID, cp.LegacyCustomerID, 
              p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
              p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
              p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
              ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
            FROM Customer c  
              JOIN CustomerProject cp ON cp.fk_CustomerUID = c.CustomerUID 
              JOIN Project p on p.ProjectUID = cp.fk_ProjectUID           
              LEFT OUTER JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
              LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
            WHERE c.CustomerUID = @CustomerUID",
        new { CustomerUID = customerUid }
      );


      // need to get the row with the later SubscriptionEndDate if there are duplicates
      // Also if there are >1 projectGeofences.. hmm.. it will just return either
      return projects.OrderByDescending(proj => proj.SubscriptionEndDate).GroupBy(d => d.ProjectUID)
        .Select(g => g.First()).ToList();
    }

    /// <summary>
    ///     Gets the specified project without linked data like customer and subscription.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>The project</returns>
    public async Task<Project> GetProjectOnly(string projectUid)
    {
      var project = (await QueryWithAsyncPolicy<Project>
      (@"SELECT
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC
              FROM Project p 
              WHERE p.ProjectUID = @ProjectUID",
        new { ProjectUID = projectUid }
      )).FirstOrDefault();


      return project;
    }

    /// <summary>
    ///     Checks if a project with the specified projectUid exists.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>true if project exists or false otherwise</returns>
    public async Task<bool> ProjectExists(string projectUid)
    {
      var uid = (await QueryWithAsyncPolicy<string>
      (@"SELECT p.ProjectUID
              FROM Project p 
              WHERE p.ProjectUID = @ProjectUID",
        new { ProjectUID = projectUid }
      )).FirstOrDefault();


      return !string.IsNullOrEmpty(uid);
    }

    /// <summary>
    ///     Checks if a project with the specified projectUid is associated with a customer.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>true if project is associated with a customer or false otherwise</returns>
    public async Task<bool> CustomerProjectExists(string projectUid)
    {
      var uid = (await QueryWithAsyncPolicy<string>
      (@"SELECT cp.fk_ProjectUID
              FROM CustomerProject cp 
              WHERE cp.fk_ProjectUID = @ProjectUID",
        new { ProjectUID = projectUid }
      )).FirstOrDefault();


      return !string.IsNullOrEmpty(uid);
    }

    /// <summary>
    ///     for unit tests - so we don't have to create everything (associations) for a test
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<Project> GetProject_UnitTest(string projectUid)
    {
      var project = (await QueryWithAsyncPolicy<Project>
      (@"SELECT 
                  p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                  p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                  p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                  cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                  ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p 
                LEFT JOIN CustomerProject cp ON p.ProjectUID = cp.fk_ProjectUID
                LEFT JOIN Customer c ON c.CustomerUID = cp.fk_CustomerUID
                LEFT JOIN ProjectSubscription ps on p.ProjectUID = ps.fk_ProjectUID
                LEFT OUTER JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE p.ProjectUID = @ProjectUID",
        new { ProjectUID = projectUid }
      )).FirstOrDefault();


      return project;
    }

    /// <summary>
    /// Gets the list of geofence UIDs associated wih the specified project
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>List of associations</returns>
    public async Task<IEnumerable<ProjectGeofence>> GetAssociatedGeofences(string projectUid)
    {
      return await QueryWithAsyncPolicy<ProjectGeofence>
      (@"SELECT 
                fk_GeofenceUID AS GeofenceUID, fk_ProjectUID AS ProjectUID, pg.LastActionedUTC, g.fk_GeofenceTypeID AS GeofenceType 
              FROM ProjectGeofence pg
                LEFT OUTER JOIN Geofence g on g.GeofenceUID = pg.fk_GeofenceUID
              WHERE fk_ProjectUID = @ProjectUID",
        new { ProjectUID = projectUid }
      );
    }

    /// <summary>
    /// Gets the list of geofence UIDs for the customer, along with any potential projectUid association
    /// </summary>
    /// <param name="customerUid"></param>
    /// <returns>List of geofences and potential ProjectUid</returns>
    public async Task<IEnumerable<GeofenceWithAssociation>> GetCustomerGeofences(string customerUid)
    {
      return await QueryWithAsyncPolicy<GeofenceWithAssociation>
      (@"SELECT 
                g.GeofenceUID, g.Name, g.fk_GeofenceTypeID AS GeofenceType, g.GeometryWKT, g.FillColor, g.IsTransparent,
                g.IsDeleted, g.Description, g.fk_CustomerUID AS CustomerUID, g.UserUID, g.AreaSqMeters,
                g.LastActionedUTC, pg.fk_ProjectUID AS ProjectUID 
              FROM Geofence g 
                LEFT OUTER JOIN ProjectGeofence pg on pg.fk_GeofenceUID = g.GeofenceUID 
              WHERE fk_CustomerUID = @CustomerUID 
                AND g.IsDeleted = 0",
        new { CustomerUID = customerUid }
      );
    }

    #endregion gettersProject


    #region gettersProjectSettings

    /// <summary>
    /// At this stage 2 types
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="userId"></param>
    /// <param name="projectSettingsType"></param>
    /// <returns></returns>
    public async Task<ProjectSettings> GetProjectSettings(string projectUid, string userId,
      ProjectSettingsType projectSettingsType)
    {
      var projectSettings = (await QueryWithAsyncPolicy<ProjectSettings>(@"SELECT 
                fk_ProjectUID AS ProjectUid, fk_ProjectSettingsTypeID AS ProjectSettingsType, Settings, UserID, LastActionedUTC
              FROM ProjectSettings
              WHERE fk_ProjectUID = @ProjectUid
                AND UserID = @UserID
                AND fk_ProjectSettingsTypeID = @ProjectSettingsType
              ORDER BY fk_ProjectUID, UserID, fk_ProjectSettingsTypeID",
        new { ProjectUid = projectUid, UserID = userId, ProjectSettingsType = projectSettingsType })).FirstOrDefault();
      return projectSettings;
    }

    /// <summary>
    /// At this stage 2 types, user must eval result
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<IEnumerable<ProjectSettings>> GetProjectSettings(string projectUid, string userId)
    {
      var projectSettingsList = await QueryWithAsyncPolicy<ProjectSettings>
      (@"SELECT 
                fk_ProjectUID AS ProjectUid, fk_ProjectSettingsTypeID AS ProjectSettingsType, Settings, UserID, LastActionedUTC
              FROM ProjectSettings
              WHERE fk_ProjectUID = @ProjectUid
                AND UserID = @UserID",
        new { ProjectUid = projectUid, UserID = userId }
      );
      return projectSettingsList;
    }

    #endregion gettersProjectSettings


    #region gettersImportedFiles

    public async Task<IEnumerable<ImportedFile>> GetImportedFiles(string projectUid)
    {
      var importedFileList = (await QueryWithAsyncPolicy<ImportedFile>
      (@"SELECT 
            fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, LegacyImportedFileID, fk_CustomerUID as CustomerUID, fk_ImportedFileTypeID as ImportedFileType, 
            Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID as DxfUnitsType,
            MinZoomLevel, MaxZoomLevel, IsDeleted, LastActionedUTC
          FROM ImportedFile
            WHERE fk_ProjectUID = @ProjectUid
              AND IsDeleted = 0",
        new { ProjectUid = projectUid }
      )).ToList();

      var historyAllFiles = await GetImportedFileHistory(projectUid);
      foreach (var importedFile in importedFileList)
      {
        var historyOne = historyAllFiles.FindAll(x => x.ImportedFileUid == importedFile.ImportedFileUid);
        if (historyOne.Any())
        {
          importedFile.ImportedFileHistory = new ImportedFileHistory(historyOne);
        }
      }

      return importedFileList;
    }

    public async Task<ImportedFile> GetImportedFile(string importedFileUid)
    {
      var importedFile = (await QueryWithAsyncPolicy<ImportedFile>
      (@"SELECT 
            fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, LegacyImportedFileID, fk_CustomerUID as CustomerUID, fk_ImportedFileTypeID as ImportedFileType, 
            Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID as DxfUnitsType, 
            MinZoomLevel, MaxZoomLevel, IsDeleted, LastActionedUTC
          FROM ImportedFile
            WHERE importedFileUID = @ImportedFileUid",
        new { ImportedFileUid = importedFileUid }
      )).FirstOrDefault();

      if (importedFile != null)
      {
        var historyAllFiles = await GetImportedFileHistory(importedFile.ProjectUid, importedFileUid);
        if (historyAllFiles.Any())
        {
          importedFile.ImportedFileHistory = new ImportedFileHistory(historyAllFiles);
        }
      }

      return importedFile;
    }

    private async Task<List<ImportedFileHistoryItem>> GetImportedFileHistory(string projectUid,
      string importedFileUid = null)
    {
      return (await QueryWithAsyncPolicy<ImportedFileHistoryItem>
      (@"SELECT 
              ImportedFileUID, ifh.FileCreatedUTC, ifh.FileUpdatedUTC, ifh.ImportedBy
            FROM ImportedFile iff
              INNER JOIN ImportedFileHistory ifh ON ifh.fk_ImportedFileUID = iff.ImportedFileUID
            WHERE fk_ProjectUID = @projectUid
              AND IsDeleted = 0
              AND (@ImportedFileUid IS NULL OR ImportedFileUID = @ImportedFileUid)
            ORDER BY ImportedFileUID, ifh.FileUpdatedUTC",
        new {projectUid, ImportedFileUid = importedFileUid }
      )).ToList();
    }

    #endregion gettersImportedFiles


    #region gettersSpatial

    /// <summary>
    ///     Gets any standard project which the lat/long is within,
    ///     which satisfies all conditions for the asset
    /// </summary>
    /// <param name="customerUID"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="timeOfPosition"></param>
    /// <returns>The project</returns>
    public async Task<IEnumerable<Project>> GetStandardProject(string customerUID, double latitude,
      double longitude, DateTime timeOfPosition)
    {
      var point = string.Format("ST_GeomFromText('POINT({0} {1})')", longitude, latitude);
      var select = string.Format(
        "SELECT DISTINCT " +
        "        p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone, " +
        "        p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT, " +
        "        p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC, " +
        "        cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID " +
        "      FROM Project p " +
        "        INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID " +
        "      WHERE p.fk_ProjectTypeID = 0 " +
        "        AND p.IsDeleted = 0 " +
        "        AND @timeOfPosition BETWEEN p.StartDate AND p.EndDate " +
        "        AND cp.fk_CustomerUID = @CustomerUID " +
        "        AND st_Intersects({0}, PolygonST) = 1"
        , point);

      var projects =
        await QueryWithAsyncPolicy<Project>(select, new { CustomerUID = customerUID, timeOfPosition = timeOfPosition.Date});


      return projects;
    }

    /// <summary>
    ///     Gets any ProjectMonitoring or Landfill (as requested) project which the lat/long is within,
    ///     which satisfies all conditions for the tccOrgid
    ///     note that project can be backfilled i.e.set to a date earlier than the serviceView
    /// </summary>
    /// <param name="customerUID"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="timeOfPosition"></param>
    /// <returns>The project</returns>
    public async Task<IEnumerable<Project>> GetProjectMonitoringProject(string customerUID,
      double latitude, double longitude, DateTime timeOfPosition, int projectType, int serviceType)
    {
      var point = string.Format("ST_GeomFromText('POINT({0} {1})')", longitude, latitude);
      var select = string.Format(
        "SELECT DISTINCT " +
        "        p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone, " +
        "        p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT, " +
        "        p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC, " +
        "        cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, " +
        "        ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID " +
        "      FROM Project p " +
        "        INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID " +
        "        INNER JOIN ProjectSubscription ps ON ps.fk_ProjectUID = cp.fk_ProjectUID " +
        "        INNER JOIN Subscription s ON s.SubscriptionUID = ps.fk_SubscriptionUID " +
        "      WHERE p.fk_ProjectTypeID = @ProjectType " +
        "        AND p.IsDeleted = 0 " +
        "        AND @timeOfPosition BETWEEN p.StartDate AND p.EndDate " +
        "        AND @timeOfPosition <= s.EndDate " +
        "        AND s.fk_ServiceTypeID = @serviceType " +
        "        AND cp.fk_CustomerUID = @CustomerUID " +
        "        AND st_Intersects({0}, PolygonST) = 1"
        , point);

      var projects = await QueryWithAsyncPolicy<Project>(select,
        new { CustomerUID = customerUID, timeOfPosition = timeOfPosition.Date, ProjectType = projectType, serviceType});


      return projects;
    }

    /// <summary>
    ///     Gets any project which
    ///     1) for this Customer
    ///     2) is active at the time
    ///     3) the lat/long is within,
    ///     4) but ignore the project if it's an update
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="geometryWkt"></param>
    /// <param name="startDate"></param>
    /// <param name="endDate"></param>
    /// <param name="excludeProjectUid"></param>
    /// <returns>The project</returns>
    public async Task<bool> DoesPolygonOverlap(string customerUid, string geometryWkt, DateTime startDate,
      DateTime endDate, string excludeProjectUid = "")
    {
      var polygonToCheck = $"ST_GeomFromText('{geometryWkt}')";
      var select = $@"SELECT DISTINCT
                          p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                          p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                          p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                          cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID
                        FROM Project p 
                          INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                        WHERE p.IsDeleted = 0
                          AND @StartDate <= p.EndDate
                          AND @EndDate >= p.StartDate
                          AND cp.fk_CustomerUID = @CustomerUID
                          AND p.ProjectUid != @excludeProjectUid
                          AND st_Intersects({ polygonToCheck}, PolygonST) = 1";

      var projects = await QueryWithAsyncPolicy<Project>(select,
        new { CustomerUID = customerUid, StartDate = startDate.Date, EndDate = endDate.Date, excludeProjectUid });

      return projects.Any();
    }

    public async Task<IEnumerable<Project>> GetProjects_UnitTests()
    {
      var projects = await QueryWithAsyncPolicy<Project>
      (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID, 
                ps.fk_SubscriptionUID AS SubscriptionUID, s.StartDate AS SubscriptionStartDate, s.EndDate AS SubscriptionEndDate, fk_ServiceTypeID AS ServiceTypeID
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID
                JOIN ProjectSubscription ps on ps.fk_ProjectUID = p.ProjectUID
                JOIN Subscription s on s.SubscriptionUID = ps.fk_SubscriptionUID 
              WHERE p.IsDeleted = 0"
      );


      return projects;
    }

    #endregion gettersSpatial
  }

  internal class Point
  {
    public double X;
    public double Y;
    public string WKTSubstring => $"{X} {Y}";

    public override bool Equals(object obj)
    {
      var source = (Point) obj;
      return (source.X == X) && (source.Y == Y);
    }

    public override int GetHashCode()
    {
      return 0;
    }
  }

  internal static class ExtensionString
  {
    private static readonly Dictionary<string, string> _replacements = new Dictionary<string, string>();

    static ExtensionString()
    {
      _replacements["LINESTRING"] = "";
      _replacements["CIRCLE"] = "";
      _replacements["POLYGON"] = "";
      _replacements["POINT"] = "";
      _replacements["("] = "";
      _replacements[")"] = "";
    }

    public static List<Point> ClosePolygonIfRequired(this List<Point> s)
    {
      if (Equals(s.First(), s.Last()))
        return s;
      s.Add(s.First());
      return s;
    }

    public static string ToPolygonWKT(this List<Point> s)
    {
      var internalString = s.Select(p => p.WKTSubstring).Aggregate((i, j) => $"{i},{j}");
      return $"POLYGON(({internalString}))";
    }

    public static List<Point> ParseGeometryData(this string s)
    {
      var points = new List<Point>();

      foreach (string to_replace in _replacements.Keys)
      {
        s = s.Replace(to_replace, _replacements[to_replace]);
      }

      string[] pointsArray = s.Split(',').Select(str => str.Trim()).ToArray();

      IEnumerable<string[]> coordinates;

      //gets x and y coordinates split by space, trims whitespace at pos 0, converts to double array
      coordinates = pointsArray.Select(point => point.Trim().Split(null)
        .Where(v => !string.IsNullOrWhiteSpace(v)).ToArray());
      points = coordinates.Select(p => new Point() {X = double.Parse(p[0]), Y = double.Parse(p[1])}).ToList();

      return points;
    }
  }
}