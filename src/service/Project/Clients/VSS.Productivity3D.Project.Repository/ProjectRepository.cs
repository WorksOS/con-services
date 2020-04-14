using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Models.Internal;
using VSS.MasterData.Models.Utilities;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.MasterData.Repositories.ExtendedModels;
using VSS.MasterData.Repositories.Extensions;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Visionlink.Interfaces.Events.MasterData.Interfaces;
using VSS.Visionlink.Interfaces.Events.MasterData.Models;
using ProjectDataModel = VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels.Project;

namespace VSS.Productivity3D.Project.Repository
{
  public class ProjectRepository : RepositoryBase, IRepository<IProjectEvent>, IProjectRepository
  {
    public ProjectRepository(IConfigurationStore configurationStore, ILoggerFactory logger) : base(configurationStore,
      logger)
    {
      Log = logger.CreateLogger<ProjectRepository>();
    }

    #region projectstore

    public async Task<int> StoreEvent(IProjectEvent evt)
    {
      var upsertedCount = 0;
      if (evt == null)
      {
        Log.LogWarning("Unsupported project event type");
        return 0;
      }

      Log.LogDebug($"Event type is {evt.GetType()}");
      if (evt is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent)evt;
        var project = new ProjectDataModel
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          CustomerUID = projectEvent.CustomerUID.ToString(),
          ShortRaptorProjectId = projectEvent.ShortRaptorProjectId,
          Name = projectEvent.ProjectName,
          Description = projectEvent.Description,
          ProjectType = projectEvent.ProjectType,
          StartDate = projectEvent.ProjectStartDate.Date,
          EndDate = projectEvent.ProjectEndDate.Date,          
          ProjectTimeZone = projectEvent.ProjectTimezone,
          ProjectTimeZoneIana = PreferencesTimeZones.WindowsToIana(projectEvent.ProjectTimezone),
          LastActionedUTC = projectEvent.ActionUTC
        };

        if (!string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = projectEvent.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = projectEvent.ActionUTC;
        }

        project.Boundary = RepositoryHelper.GetPolygonWKT(projectEvent.ProjectBoundary);
        if (!string.IsNullOrEmpty(project.Boundary))
        {
          upsertedCount = await UpsertProjectDetail(project, "CreateProjectEvent");
        }
        else
        {
          Log.LogWarning(
            $"ProjectRepository/CreateProject: Unable to createProject as Boundary is missing. Project: {JsonConvert.SerializeObject(project)}))')");
        }
      }
      else if (evt is UpdateProjectEvent)
      {
        var projectEvent = (UpdateProjectEvent)evt;

        var project = new ProjectDataModel
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),          
          Name = projectEvent.ProjectName,
          Description = projectEvent.Description,
          ProjectType = projectEvent.ProjectType,
          EndDate = projectEvent.ProjectEndDate.Date,          
          ProjectTimeZone = projectEvent.ProjectTimezone,
          ProjectTimeZoneIana = PreferencesTimeZones.WindowsToIana(projectEvent.ProjectTimezone),
          LastActionedUTC = projectEvent.ActionUTC
        };

        if (!string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = projectEvent.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = projectEvent.ActionUTC;
        }

        project.Boundary = RepositoryHelper.GetPolygonWKT(projectEvent.ProjectBoundary);
        upsertedCount = await UpsertProjectDetail(project, "UpdateProjectEvent");
      }
      else if (evt is DeleteProjectEvent)
      {
        var projectEvent = (DeleteProjectEvent)evt;
        var project = new ProjectDataModel
        {
          ProjectUID = projectEvent.ProjectUID.ToString(),
          LastActionedUTC = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectDetail(project, "DeleteProjectEvent", projectEvent.DeletePermanently);
      }
       
      
      else if (evt is CreateImportedFileEvent)
      {
        var projectEvent = (CreateImportedFileEvent)evt;
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
          LastActionedUtc = projectEvent.ActionUTC,
          ParentUid = projectEvent.ParentUID?.ToString(),
          Offset = projectEvent.Offset
        };
        upsertedCount = await UpsertImportedFile(importedFile, "CreateImportedFileEvent");
      }
      else if (evt is UpdateImportedFileEvent)
      {
        var projectEvent = (UpdateImportedFileEvent)evt;
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
          LastActionedUtc = projectEvent.ActionUTC,
          Offset = projectEvent.Offset
        };
        upsertedCount = await UpsertImportedFile(importedFile, "UpdateImportedFileEvent");
      }
      else if (evt is DeleteImportedFileEvent)
      {
        var projectEvent = (DeleteImportedFileEvent)evt;
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
        var projectEvent = (UndeleteImportedFileEvent)evt;
        var importedFile = new ImportedFile
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ImportedFileUid = projectEvent.ImportedFileUID.ToString(),
          LastActionedUtc = projectEvent.ActionUTC
        };
        upsertedCount = await UpsertImportedFile(importedFile, "UndeleteImportedFileEvent");
      }

      else if (evt is UpdateProjectSettingsEvent projectEvent)
      {
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

      // this geofence code is used by FilterSvc and refer to tables solely in the FilterSvc database (not the ProjectSvc one).
      else if (evt is AssociateProjectGeofence)
      {
        var projectGeofenceEvent = (AssociateProjectGeofence)evt;
        var projectGeofence = new ProjectGeofence
        {
          ProjectUID = projectGeofenceEvent.ProjectUID.ToString(),
          GeofenceUID = projectGeofenceEvent.GeofenceUID.ToString(),
          LastActionedUTC = projectGeofenceEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectGeofenceDetail(projectGeofence, "AssociateProjectGeofenceEvent");
      }
      else if (evt is DissociateProjectGeofence)
      {
        var projectGeofenceEvent = (DissociateProjectGeofence)evt;
        var projectGeofence = new ProjectGeofence
        {
          ProjectUID = projectGeofenceEvent.ProjectUID.ToString(),
          GeofenceUID = projectGeofenceEvent.GeofenceUID.ToString(),
          LastActionedUTC = projectGeofenceEvent.ActionUTC
        };
        upsertedCount = await UpsertProjectGeofenceDetail(projectGeofence, "DissociateProjectGeofenceEvent");
      }

      return upsertedCount;
    }

    /// <summary>
    ///     All detail-related columns can be inserted,
    ///     but only certain columns can be updated.
    ///     on deletion, a flag will be set.
    /// </summary>
    /// <param name="project"></param>
    /// <param name="eventType"></param>
    /// <param name="isDeletePermanently"></param>
    /// <returns></returns>
    private async Task<int> UpsertProjectDetail(ProjectDataModel project, string eventType, bool isDeletePermanently = false)
    {
      var upsertedCount = 0;
      var existing = (await QueryWithAsyncPolicy<ProjectDataModel>
      (@"SELECT 
                ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, 
                fk_ProjectTypeID AS ProjectType, StartDate, EndDate, 
                ProjectTimeZone, ProjectTimeZoneIana, 
                ST_ASWKT(Boundary) AS Boundary,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                IsArchived, LastActionedUTC
              FROM Project
              WHERE ProjectUID = @ProjectUID
                OR ShortRaptorProjectID = @ShortRaptorProjectId",
        new { project.ProjectUID, project.ShortRaptorProjectId }
      )).FirstOrDefault();

      if (eventType == "CreateProjectEvent")
        upsertedCount = await CreateProject(project, existing);

      if (eventType == "UpdateProjectEvent")
        upsertedCount = await UpdateProject(project, existing);

      if (eventType == "DeleteProjectEvent")
        upsertedCount = await DeleteProject(project, existing, isDeletePermanently);
      return upsertedCount;
    }

    private async Task<int> CreateProject(ProjectDataModel project, ProjectDataModel existing)
    {
      var upsertedCount = 0;
      Log.LogDebug($"ProjectRepository/CreateProject: project={JsonConvert.SerializeObject(project)}))')");

      if (project.StartDate > project.EndDate)
      {
        Log.LogDebug("Project will not be created as startDate > endDate");
        return upsertedCount;
      }

      if (existing == null)
      {
        string insert = BuildProjectInsertString(project);

        upsertedCount = await ExecuteWithAsyncPolicy(insert, project);
        Log.LogDebug($"ProjectRepository/CreateProject: (insert): inserted {upsertedCount} rows");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
          await UpsertProjectTypeGeofence("CreatedProject", project);
        }

        return upsertedCount;
      }

      // a delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
      //       update everything but ActionUTC from the create
      if ((existing.LastActionedUTC >= project.LastActionedUTC) && existing.IsArchived == true)
      {
        project.IsArchived = true;

        // this create could have the legit legacyProjectId
        project.ShortRaptorProjectId = existing.ShortRaptorProjectId;
        project.CustomerUID = string.IsNullOrEmpty(existing.CustomerUID) ? project.CustomerUID : existing.CustomerUID;

        // leave more recent values
        project.Name = string.IsNullOrEmpty(existing.Name) ? project.Name : existing.Name;
        project.Description = string.IsNullOrEmpty(existing.Description) ? project.Description : existing.Description;
        project.ProjectTimeZone = string.IsNullOrEmpty(existing.ProjectTimeZone)
          ? project.ProjectTimeZone
          : existing.ProjectTimeZone;
        project.ProjectTimeZoneIana = string.IsNullOrEmpty(existing.ProjectTimeZoneIana)
          ? project.ProjectTimeZoneIana
          : existing.ProjectTimeZoneIana;
        project.StartDate = existing.StartDate == DateTime.MinValue ? project.StartDate : existing.StartDate;
        project.EndDate = existing.EndDate == DateTime.MinValue ? project.EndDate : existing.EndDate;
        project.LastActionedUTC = existing.LastActionedUTC;

        if (!string.IsNullOrEmpty(existing.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
        }

        project.Boundary = string.IsNullOrEmpty(existing.Boundary) ? project.Boundary : existing.Boundary;

        string update = BuildProjectUpdateString(project);
        Log.LogDebug("ProjectRepository/CreateProject: going to update a dummy project");

        upsertedCount = await ExecuteWithAsyncPolicy(update, project);
        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
        }

        Log.LogDebug($"ProjectRepository/CreateProject: (update): updated {upsertedCount} rows ");
        return upsertedCount;
      }

      // an update was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
      // leave the more recent EndDate, Name, Description, ProjectType and actionUTC alone
      if (existing.LastActionedUTC >= project.LastActionedUTC)
      {
        Log.LogDebug("ProjectRepository/CreateProject: create arrived after an update so updating project");

        // this create could have the legit legacyProjectId
        project.ShortRaptorProjectId = existing.ShortRaptorProjectId;
        project.CustomerUID = string.IsNullOrEmpty(existing.CustomerUID) ? project.CustomerUID : existing.CustomerUID;

        // leave more recent values
        project.Name = string.IsNullOrEmpty(existing.Name) ? project.Name : existing.Name;
        project.Description = string.IsNullOrEmpty(existing.Description) ? project.Description : existing.Description;
        project.ProjectTimeZone = string.IsNullOrEmpty(existing.ProjectTimeZone)
          ? project.ProjectTimeZone
          : existing.ProjectTimeZone;
        project.ProjectTimeZoneIana = string.IsNullOrEmpty(existing.ProjectTimeZoneIana)
          ? project.ProjectTimeZoneIana
          : existing.ProjectTimeZoneIana;
        project.StartDate = existing.StartDate == DateTime.MinValue ? project.StartDate : existing.StartDate;
        project.EndDate = existing.EndDate == DateTime.MinValue ? project.EndDate : existing.EndDate;
        project.LastActionedUTC = existing.LastActionedUTC;

        if (!string.IsNullOrEmpty(existing.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
        }

        project.Boundary = string.IsNullOrEmpty(existing.Boundary) ? project.Boundary : existing.Boundary;

        string update = BuildProjectUpdateString(project);
        upsertedCount = await ExecuteWithAsyncPolicy(update, project);
        Log.LogDebug($"ProjectRepository/CreateProject: (updateExisting): updated {upsertedCount} rows");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
          await UpsertProjectTypeGeofence("UpdatedProject", project);
        }

        return upsertedCount;
      }

      Log.LogDebug("ProjectRepository/CreateProject: No action as project already exists.");
      return upsertedCount;
    }

    private async Task<int> UpdateProject(ProjectDataModel project, ProjectDataModel existing)
    {
      Log.LogDebug($"ProjectRepository/UpdateProject: project={JsonConvert.SerializeObject(project)}))')");

      var upsertedCount = 0;
      if (existing != null)
      {
        if (project.EndDate < existing.StartDate)
        {
          Log.LogDebug(
            $"ProjectRepository/UpdateProject: failed to update project={project.ProjectUID} EndDate < StartDate");
          return upsertedCount;
        }

        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          project.ShortRaptorProjectId = existing.ShortRaptorProjectId;
          project.CustomerUID = string.IsNullOrEmpty(project.CustomerUID) ? existing.CustomerUID : project.CustomerUID;
          project.Name = string.IsNullOrEmpty(project.Name) ? existing.Name : project.Name;
          project.Description = string.IsNullOrEmpty(project.Description) ? existing.Description : project.Description;
          project.ProjectTimeZone = string.IsNullOrEmpty(project.ProjectTimeZone)
            ? existing.ProjectTimeZone
            : project.ProjectTimeZone;
          project.ProjectTimeZoneIana = string.IsNullOrEmpty(project.ProjectTimeZoneIana)
            ? existing.ProjectTimeZoneIana
            : project.ProjectTimeZoneIana;
          project.StartDate = project.StartDate == DateTime.MinValue ? existing.StartDate : project.StartDate;

          if (string.IsNullOrEmpty(project.CoordinateSystemFileName))
          {
            project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
            project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
          }

          project.Boundary = string.IsNullOrEmpty(project.Boundary) ? existing.Boundary : project.Boundary;

          Log.LogDebug($"ProjectRepository/UpdateProject: updating project={project.ProjectUID}");

          string update = BuildProjectUpdateString(project);
          upsertedCount = await ExecuteWithAsyncPolicy(update, project);
          Log.LogDebug(
            $"ProjectRepository/UpdateProject: upserted {upsertedCount} rows for: projectUid:{project.ProjectUID}");

          if (upsertedCount > 0)
          {
            upsertedCount = await InsertProjectHistory(project);
            await UpsertProjectTypeGeofence("UpdatedProject", project);
          }

          return upsertedCount;
        }

        Log.LogDebug($"ProjectRepository/UpdateProject: old update event ignored project={project.ProjectUID}");
      }
      else
      {
        // an update was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        Log.LogDebug(
          $"ProjectRepository/UpdateProject: project doesn't already exist, creating one. project={project.ProjectUID}");
        if (string.IsNullOrEmpty(project.ProjectTimeZone))
          project.ProjectTimeZone = "";

        string insert = BuildProjectInsertString(project);
        upsertedCount = await ExecuteWithAsyncPolicy(insert, project);
        Log.LogDebug($"ProjectRepository/UpdateProject: (insert): inserted {upsertedCount} rows");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
          await UpsertProjectTypeGeofence("CreatedProject", project);
        }

        return upsertedCount;
      }

      return upsertedCount;
    }

    private async Task<int> DeleteProject(ProjectDataModel project, ProjectDataModel existing, bool isDeletePermanently)
    {
      Log.LogDebug(
        $"ProjectRepository/DeleteProject: project={JsonConvert.SerializeObject(project)} permanently: {isDeletePermanently}))')");

      var upsertedCount = 0;
      if (existing != null)
      {
        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          // this is for internal use only to roll-back after failed series of steps
          if (isDeletePermanently)
          {
            Log.LogDebug(
              $"ProjectRepository/DeleteProject: deleting a project permanently: {JsonConvert.SerializeObject(project)}");
            const string delete =
              @"DELETE FROM Project
                    WHERE ProjectUID = @ProjectUID";
            upsertedCount = await ExecuteWithAsyncPolicy(delete, project);
            Log.LogDebug(
              $"ProjectRepository/DeleteProject: deleted {upsertedCount} rows for: projectUid:{project.ProjectUID}");

            return upsertedCount;
          }
          else
          {
            Log.LogDebug($"ProjectRepository/DeleteProject: updating project={project.ProjectUID}");

            // on deletion, the projects endDate will be set to now, in its local time.
            var localEndDate = project.LastActionedUTC.ToLocalDateTime(existing.ProjectTimeZoneIana);
            if (localEndDate != null)
            {
              project.EndDate = localEndDate.Value.Date;
              const string update =
                @"UPDATE Project                
                  SET IsArchived = 1,
                    EndDate = @EndDate,
                    LastActionedUTC = @LastActionedUTC
                  WHERE ProjectUID = @ProjectUID";
              upsertedCount = await ExecuteWithAsyncPolicy(update, project);
              Log.LogDebug(
                $"ProjectRepository/DeleteProject: upserted {upsertedCount} rows for: projectUid:{project.ProjectUID} new endDate: {project.EndDate}");
            }
            else
            {
              Log.LogError($"ProjectRepository/DeleteProject: Unable to convert current Utc date to local. Unknown timeZone: {existing.ProjectTimeZoneIana}");
            }

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
        Log.LogDebug(
          $"ProjectRepository/DeleteProject: delete event where no project exists, creating one. project={project.ProjectUID}");
        project.Name = "";
        project.ProjectTimeZone = "";
        project.ProjectTimeZoneIana = "";
        project.ProjectType = ProjectType.Standard;

        const string delete =
          "INSERT Project " +
          "    (ProjectUID, Name, fk_ProjectTypeID, IsArchived, ProjectTimeZone, ProjectTimeZoneIana, LastActionedUTC)" +
          "  VALUES " +
          "    (@ProjectUID, @Name, @ProjectType, 1, @ProjectTimeZone, @ProjectTimeZoneIana, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(delete, project);
        Log.LogDebug(
          $"ProjectRepository/DeleteProject: inserted {upsertedCount} rows for: projectUid:{project.ProjectUID}");

        if (upsertedCount > 0)
        {
          upsertedCount = await InsertProjectHistory(project);
        }

        return upsertedCount;
      }

      return upsertedCount;
    }

    private string BuildProjectInsertString(ProjectDataModel project)
    {
      var formattedPolygon = RepositoryHelper.WKTToSpatial(project.Boundary);

      if (project.ShortRaptorProjectId <= 0) // allow db autoincrement on legacyProjectID
      {
        return "INSERT Project " +
          "    (ProjectUID, CustomerUID, Name, Description, fk_ProjectTypeID, StartDate, EndDate, ProjectTimeZone, ProjectTimeZoneIana, Boundary, CoordinateSystemFileName, CoordinateSystemLastActionedUTC, IsArchived, LastActionedUTC) " +
          "  VALUES " +
          $"    (@ProjectUID, @CustomerUID, @Name, @Description, @ProjectType, @StartDate, @EndDate, @ProjectTimeZone, @ProjectTimeZoneIana, {formattedPolygon}, @CoordinateSystemFileName, @CoordinateSystemLastActionedUTC, @IsArchived, @LastActionedUTC)";
      }

      return "INSERT Project " +
        "    (ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, fk_ProjectTypeID, StartDate, EndDate, ProjectTimeZone, ProjectTimeZoneIana, Boundary, CoordinateSystemFileName, CoordinateSystemLastActionedUTC, IsArchived, LastActionedUTC) " +
        "  VALUES " +
        $"    (@ProjectUID, @CustomerUID, @ShortRaptorProjectID, @Name, @Description, @ProjectType, @StartDate, @EndDate, @ProjectTimeZone, @ProjectTimeZoneIana, {formattedPolygon}, @CoordinateSystemFileName, @CoordinateSystemLastActionedUTC, @IsArchived, @LastActionedUTC)";
    }

    private string BuildProjectUpdateString(ProjectDataModel project)
    {
      var formattedPolygon = RepositoryHelper.WKTToSpatial(project.Boundary);

      if (project.ShortRaptorProjectId <= 0) // allow db autoincrement on legacyProjectID
      {
        return $@"UPDATE Project
                SET CustomerUID = @CustomerUID,
                  Name = @Name, Description = @Description, fk_ProjectTypeID = @ProjectType,
                  StartDate = @StartDate, EndDate = @EndDate, 
                  ProjectTimeZone = @ProjectTimeZone, ProjectTimeZoneIana = @ProjectTimeZoneIana,
                  Boundary = {formattedPolygon},
                  CoordinateSystemFileName = @CoordinateSystemFileName,
                  CoordinateSystemLastActionedUTC = @CoordinateSystemLastActionedUTC,
                  IsArchived = @IsArchived, LastActionedUTC = @LastActionedUTC
                WHERE ProjectUID = @ProjectUID";
      }

      return $@"UPDATE Project
                SET ShortRaptorProjectId = @ShortRaptorProjectId, CustomerUID = @CustomerUID,
                  Name = @Name, Description = @Description, fk_ProjectTypeID = @ProjectType,
                  StartDate = @StartDate, EndDate = @EndDate, 
                  ProjectTimeZone = @ProjectTimeZone, ProjectTimeZoneIana = @ProjectTimeZoneIana,
                  Boundary = {formattedPolygon},
                  CoordinateSystemFileName = @CoordinateSystemFileName,
                  CoordinateSystemLastActionedUTC = @CoordinateSystemLastActionedUTC,
                  IsArchived = @IsArchived, LastActionedUTC = @LastActionedUTC
                WHERE ProjectUID = @ProjectUID";
    }

    #endregion projectstore


    #region projectSettingsStore

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
      Log.LogDebug(
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
      Log.LogDebug(
        $"ProjectRepository/UpsertProjectSettings: upserted {upsertedCount} rows (1=insert, 2=update) for: projectSettingsProjectUid:{projectSettings.ProjectUid}");
      return upsertedCount.CalculateUpsertCount();
    }

    #endregion projectSettingsStore


    #region importedFilesStore

    private async Task<int> UpsertImportedFile(ImportedFile importedFile, string eventType,
      bool isDeletePermanently = false)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<ImportedFile>
      (@"SELECT 
              fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID as CustomerUID,
              fk_ImportedFileTypeID as ImportedFileType, Name, 
              FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, 
              fk_DXFUnitsTypeID as DxfUnitsType, MinZoomLevel, MaxZoomLevel, Offset, fk_ReferenceImportedFileUID as ParentUID,
              IsDeleted, LastActionedUTC
            FROM ImportedFile
            WHERE ImportedFileUID = @ImportedFileUid", new { importedFile.ImportedFileUid }
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
        Log.LogDebug(
          $"ProjectRepository/CreateImportedFile: going to create importedFile={JsonConvert.SerializeObject(importedFile)}");

        var insert = string.Format(
          "INSERT ImportedFile " +
          "    (fk_ProjectUID, ImportedFileUID, ImportedFileID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID, MinZoomLevel, MaxZoomLevel, IsDeleted, LastActionedUTC, Offset, fk_ReferenceImportedFileUID) " +
          "  VALUES " +
          "    (@ProjectUid, @ImportedFileUid, @ImportedFileId, @CustomerUID, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUtc, @FileUpdatedUtc, @ImportedBy, @SurveyedUtc, @DxfUnitsType, @MinZoomLevel, @MaxZoomLevel, 0, @LastActionedUtc, @Offset, @ParentUid)");

        upsertedCount = await ExecuteWithAsyncPolicy(insert, importedFile);
        Log.LogDebug(
          $"ProjectRepository/CreateImportedFile: (insert): inserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");

        if (upsertedCount > 0)
          upsertedCount = await UpsertImportedFileHistory(importedFile);
      }
      else if (existing.LastActionedUtc >= importedFile.LastActionedUtc)
      {
        // an update/delete was processed before the create, even though it's actionUTC is later (due to kafka partioning issue)
        // The only thing which can be updated is a) the file content, and the LastActionedUtc. A file cannot be moved between projects/customers.
        // We don't store (a), and leave actionUTC as the more recent. 

        Log.LogDebug(
          $"ProjectRepository/CreateImportedFile: create arrived after an update so inserting importedFile={importedFile.ImportedFileUid}");

        const string update =
          @"UPDATE ImportedFile
              SET fk_ProjectUID = @ProjectUid, 
                ImportedFileID = @ImportedFileId,
                fk_CustomerUID = @CustomerUID,
                fk_ImportedFileTypeID = @ImportedFileType,
                Name = @Name,
                FileDescriptor = @FileDescriptor,
                FileCreatedUTC = @FileCreatedUtc,
                FileUpdatedUTC = @FileUpdatedUtc,
                ImportedBy = @ImportedBy, 
                SurveyedUTC = @SurveyedUtc,
                MinZoomLevel = @MinZoomLevel,
                MaxZoomLevel = @MaxZoomLevel,
                fk_DXFUnitsTypeID = @DxfUnitsType,
                Offset = @Offset,
                fk_ReferenceImportedFileUID  = @ParentUid
              WHERE ImportedFileUID = @ImportedFileUid";

        upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
        Log.LogDebug(
          $"ProjectRepository/CreateImportedFile: (updateExisting): upserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");

        // don't really care if this didn't pass as may already exist for create/update utc
        if (upsertedCount > 0)
          await UpsertImportedFileHistory(importedFile);
      }
      else
      {

        Log.LogDebug(
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
                  Offset = @Offset,
                  LastActionedUTC = @LastActionedUtc
                WHERE ImportedFileUID = @ImportedFileUid";

          upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
          Log.LogDebug(
            $"ProjectRepository/UpdateImportedFile: updated {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");

          // don't really care if this didn't pass as may already exist for create/update utc
          if (upsertedCount > 0)
            await UpsertImportedFileHistory(importedFile);
        }
        else
        {
          Log.LogDebug(
            $"ProjectRepository/UpdateImportedFile: old update event ignored importedFile {importedFile.ImportedFileUid}");
        }
      }
      else
      {
        // can't create as don't know fk_ImportedFileTypeID, fk_DXFUnitsTypeID or customerUID
        Log.LogDebug(
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

    private async Task<int> InsertProjectHistory(ProjectDataModel project)
    {
      const string insert = @"INSERT INTO ProjectHistory
              ( ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, 
                fk_ProjectTypeID, StartDate, EndDate, 
                ProjectTimeZone, ProjectTimeZoneIana, 
                Boundary,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                IsArchived, LastActionedUTC)
              SELECT 
                  ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, 
                  fk_ProjectTypeID, StartDate, EndDate, 
                  ProjectTimeZone, ProjectTimeZoneIana, 
                  Boundary,
                  CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                  IsArchived, LastActionedUTC
                FROM Project
                WHERE ProjectUID = @ProjectUID;";
      var insertedCount = await ExecuteWithAsyncPolicy(insert, project);
      Log.LogDebug($"ProjectRepository/CreateProjectHistory: inserted {insertedCount} rows");
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
        new { importedFile.ImportedFileUid }
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

        Log.LogDebug(
          $"ProjectRepository/UpsertImportedFileHistory: inserted {insertedCount} rows for: ImportedFileUid:{importedFile.ImportedFileUid} FileCreatedUTC: {importedFile.FileCreatedUtc} FileUpdatedUTC: {importedFile.FileUpdatedUtc}");
      }
      else
      {
        Log.LogDebug(
          $"ProjectRepository/UpsertImportedFileHistory: History already exists ImportedFileUid:{importedFile.ImportedFileUid} FileCreatedUTC: {importedFile.FileCreatedUtc} FileUpdatedUTC: {importedFile.FileUpdatedUtc}");
      }

      return insertedCount;
    }

    private async Task<int> DeleteImportedFile(ImportedFile importedFile, ImportedFile existing,
      bool isDeletePermanently)
    {
      Log.LogDebug(
        $"ProjectRepository/DeleteImportedFile: deleting importedFile: {JsonConvert.SerializeObject(importedFile)} permanent flag:{isDeletePermanently}");
      var upsertedCount = 0;
      if (existing != null)
      {
        if (importedFile.LastActionedUtc >= existing.LastActionedUtc)
        {
          if (isDeletePermanently)
          {
            Log.LogDebug(
              $"ProjectRepository/DeleteImportedFile: deleting importedFile permanently: {importedFile.ImportedFileUid}");
            const string delete =
              @"DELETE FROM ImportedFile
                  WHERE ImportedFileUID = @ImportedFileUid";
            upsertedCount = await ExecuteWithAsyncPolicy(delete, importedFile);
            Log.LogDebug(
              $"ProjectRepository/DeleteImportedFile: deleted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");
            return upsertedCount;
          }
          else
          {
            Log.LogDebug($"ProjectRepository/DeleteImportedFile: deleting importedFile {importedFile.ImportedFileUid}");

            const string update =
              @"UPDATE ImportedFile                               
                SET IsDeleted = 1,
                    LastActionedUTC = @LastActionedUtc
                WHERE ImportedFileUID = @ImportedFileUid";

            upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
            Log.LogDebug(
              $"ProjectRepository/DeleteImportedFile: upserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");
            return upsertedCount;
          }
        }
      }
      else
      {
        Log.LogDebug(
          $"ProjectRepository/DeleteImportedFile: can't delete as none existing, ignored. importedFile={importedFile.ImportedFileUid}. Can't create one as don't have enough info e.g.customerUID / type.");
      }

      return upsertedCount;
    }

    private async Task<int> UndeleteImportedFile(ImportedFile importedFile, ImportedFile existing)
    {
      // this is an interfaces extension model used solely by ProjectMDM to allow a rollback of a DeleteImportedFile
      Log.LogDebug(
        $"ProjectRepository/UndeleteImportedFile: undeleting importedFile: {JsonConvert.SerializeObject(importedFile)}.");
      var upsertedCount = 0;

      if (existing != null)
      {
        Log.LogDebug($"ProjectRepository/UndeleteImportedFile: undeleting importedFile {importedFile.ImportedFileUid}");

        const string update =
          @"UPDATE ImportedFile                               
                SET IsDeleted = 0
              WHERE ImportedFileUID = @ImportedFileUid";

        upsertedCount = await ExecuteWithAsyncPolicy(update, importedFile);
        Log.LogDebug(
          $"ProjectRepository/UndeleteImportedFile: upserted {upsertedCount} rows for: projectUid:{importedFile.ProjectUid} importedFileUid: {importedFile.ImportedFileUid}");
        return upsertedCount;
      }

      Log.LogDebug(
        $"ProjectRepository/UndeleteImportedFile: can't undelete as none existing ignored importedFile={importedFile.ImportedFileUid}.");
      return upsertedCount;
    }

    #endregion importedFilesStore


    #region projectGeofenceStore  // this geofence code is used by FilterSvc and refer to tables solely in the FilterSvc database (not the ProjectSvc one).

    private async Task<int> UpsertProjectGeofenceDetail(ProjectGeofence projectGeofence, string eventType)
    {
      var upsertedCount = 0;

      var existing = (await QueryWithAsyncPolicy<ProjectGeofence>
      (@"SELECT 
              fk_GeofenceUID AS GeofenceUID, fk_ProjectUID AS ProjectUID, LastActionedUTC
            FROM ProjectGeofence
            WHERE fk_ProjectUID = @ProjectUID AND fk_GeofenceUID = @GeofenceUID",
        new { projectGeofence.ProjectUID, projectGeofence.GeofenceUID }
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
        Log.LogDebug(
          $"ProjectRepository/AssociateProjectGeofence: projectGeofence={JsonConvert.SerializeObject(projectGeofence)}");

        const string insert =
          @"INSERT ProjectGeofence
                (fk_GeofenceUID, fk_ProjectUID, LastActionedUTC)
              VALUES
                (@GeofenceUID, @ProjectUID, @LastActionedUTC)";

        upsertedCount = await ExecuteWithAsyncPolicy(insert, projectGeofence);
        Log.LogDebug(
          $"ProjectRepository/AssociateProjectGeofence: inserted {upsertedCount} rows for: projectUid:{projectGeofence.ProjectUID} geofenceUid:{projectGeofence.GeofenceUID}");

        return upsertedCount;
      }

      Log.LogDebug(
        $"ProjectRepository/AssociateProjectGeofence: can't create as already exists projectGeofence={JsonConvert.SerializeObject(projectGeofence)}");
      return upsertedCount;
    }

    private async Task<int> DissociateProjectGeofence(ProjectGeofence projectGeofence, ProjectGeofence existing)
    {
      var upsertedCount = 0;

      Log.LogDebug(
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
          Log.LogDebug(
            $"ProjectRepository/DissociateProjectGeofence: upserted {upsertedCount} rows for: geofenceUid:{projectGeofence.GeofenceUID}");
          return upsertedCount;
        }

        // may have been associated again since, so don't delete
        Log.LogDebug("ProjectRepository/DissociateProjectGeofence: old delete event ignored");
      }
      else
      {
        Log.LogDebug("ProjectRepository/DissociateProjectGeofence: can't delete as none existing");
      }

      return upsertedCount;
    }

    private async Task UpsertProjectTypeGeofence(string upsertType, ProjectDataModel project)
    {
      if (string.IsNullOrEmpty(project.Boundary))
      {
        Log.LogInformation(
          $"ProjectRepository/UpsertProjectTypeGeofence: Unable to Upsert GeofenceBoundary as boundary not available. UpsertType {upsertType}. project={project.ProjectUID}.");
        return;
      }

      // may be an existing one if this create comes from a replay of kafka que.
      var select = "SELECT GeofenceUID, Name, fk_GeofenceTypeID AS GeofenceType, ST_ASWKT(PolygonST) AS GeometryWKT, " +
                   "     FillColor, IsTransparent, IsDeleted, Description, fk_CustomerUID AS CustomerUID, UserUID, " +
                   "     AreaSqMeters, g.LastActionedUTC " +
                   "  FROM ProjectGeofence pg " +
                   "   INNER JOIN Geofence g ON g.GeofenceUID = pg.fk_GeofenceUID " +
                   $" WHERE fk_ProjectUID = '{project.ProjectUID}' " +
                   $"  AND fk_GeofenceTypeID = {(int)GeofenceType.Project}; ";
      var existingGeofence = (await QueryWithAsyncPolicy<Geofence>(select)).FirstOrDefault();

      Log.LogDebug(
        $"ProjectRepository/UpsertProjectTypeGeofence: going to upsert. upsertType {upsertType}. project={project.ProjectUID} existingGeofence? {existingGeofence}");

      if (existingGeofence == null)
        await CreateGeofenceAndAssociation(project);
      else
        await UpdateGeofence(project, existingGeofence);
    }

    private async Task<int> CreateGeofenceAndAssociation(ProjectDataModel project)
    {
      var geofence = new Geofence().Setup();
      geofence.GeofenceUID = Guid.NewGuid().ToString();
      geofence.Name = project.Name;
      geofence.GeofenceType = GeofenceType.Project;
      geofence.GeometryWKT = project.Boundary;
      geofence.CustomerUID = project.CustomerUID;
      var area = GeofenceValidation.CalculateAreaSqMeters(project.Boundary);
      geofence.AreaSqMeters = area > 1000000000 ? 0 : area;
      geofence.IsDeleted = false;
      geofence.LastActionedUTC = DateTime.UtcNow;

      string formattedPolygon = RepositoryHelper.WKTToSpatial(project.Boundary);

      string insert = string.Format(
         "INSERT Geofence " +
         "     (GeofenceUID, Name, Description, PolygonST, FillColor, IsTransparent, IsDeleted, fk_CustomerUID, UserUID, LastActionedUTC, fk_GeofenceTypeID, AreaSqMeters) " +
         " VALUES " +
         "     (@GeofenceUID, @Name, @Description, {0}, @FillColor, @IsTransparent, @IsDeleted, @CustomerUID, @UserUID, @LastActionedUTC, @GeofenceType, @AreaSqMeters)", formattedPolygon);

      var upsertedCount = await ExecuteWithAsyncPolicy(insert, geofence);
      Log.LogDebug(
        $"ProjectRepository/UpsertGeofence inserted. upsertedCount {upsertedCount} rows for: geofenceUid:{geofence.GeofenceUID}");

      if (upsertedCount == 1)
      {
        var projectGeofence = new ProjectGeofence()
        {
          ProjectUID = project.ProjectUID,
          GeofenceUID = geofence.GeofenceUID,
          LastActionedUTC = DateTime.UtcNow
        };
        await AssociateProjectGeofence(projectGeofence, null);
        return upsertedCount;
      }

      return 0;
    }

    private async Task<int> UpdateGeofence(ProjectDataModel project, Geofence existingGeofence)
    {
      string formattedPolygon = RepositoryHelper.WKTToSpatial(project.Boundary);

      var update = "UPDATE Geofence " +
                   $" SET PolygonST = {formattedPolygon} " +
                   $" WHERE GeofenceUID = '{existingGeofence.GeofenceUID}' " +
                   $"  AND fk_GeofenceTypeID = {(int)GeofenceType.Project}; ";
      var upsertedCount = await ExecuteWithAsyncPolicy(update);
      Log.LogDebug(
        $"ProjectRepository/UpsertGeofence updated. upsertedCount {upsertedCount} rows for: geofenceUid:{existingGeofence.GeofenceUID}");

      return upsertedCount;
    }

    #endregion projectGeofenceStore


    #region projects

    /// <summary>
    ///    Gets a project by Uid, only if it is not archived
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<ProjectDataModel> GetProject(string projectUid)
    {
      var project = (await QueryWithAsyncPolicy<ProjectDataModel>(@"SELECT 
                ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, 
                fk_ProjectTypeID AS ProjectType, StartDate, EndDate, 
                ProjectTimeZone, ProjectTimeZoneIana, 
                ST_ASWKT(Boundary) AS Boundary,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                IsArchived, LastActionedUTC
              FROM Project 
              WHERE ProjectUID = @ProjectUID 
                AND IsArchived = 0",
        new { ProjectUID = projectUid })).FirstOrDefault();
      return project;
    }

    /// <summary>
    ///     Gets by shortRaptorProjectId, only if it is not archived
    /// </summary>
    /// <returns></returns>
    public async Task<ProjectDataModel> GetProject(long shortRaptorProjectId) 
    {
      var project = await QueryWithAsyncPolicy<ProjectDataModel>(@"SELECT
                ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, 
                fk_ProjectTypeID AS ProjectType, StartDate, EndDate, 
                ProjectTimeZone, ProjectTimeZoneIana, 
                ST_ASWKT(Boundary) AS Boundary,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                IsArchived, LastActionedUTC
              FROM Project 
              WHERE ShortRaptorProjectId = @ShortRaptorProjectId 
                AND IsArchived = 0",
        new { ShortRaptorProjectId = shortRaptorProjectId });
      return project.FirstOrDefault();
    }

    /// <summary>
    ///     Gets the specified project even if archived
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>The project</returns>
    public async Task<ProjectDataModel> GetProjectOnly(string projectUid)
    {
      var project = (await QueryWithAsyncPolicy<ProjectDataModel>
      (@"SELECT
                ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, 
                fk_ProjectTypeID AS ProjectType, StartDate, EndDate, 
                ProjectTimeZone, ProjectTimeZoneIana, 
                ST_ASWKT(Boundary) AS Boundary,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                IsArchived, LastActionedUTC
              FROM Project 
              WHERE ProjectUID = @ProjectUID",
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

    public async Task<IEnumerable<ProjectDataModel>> GetProjectsForCustomer(string customerUid) 
    {
      return await QueryWithAsyncPolicy<ProjectDataModel>
      (@"SELECT 
                ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, 
                fk_ProjectTypeID AS ProjectType, StartDate, EndDate, 
                ProjectTimeZone, ProjectTimeZoneIana, 
                ST_ASWKT(Boundary) AS Boundary,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                IsArchived, LastActionedUTC
            FROM Project  
            WHERE CustomerUID = @CustomerUID",
        new { CustomerUID = customerUid }
      );
    }

    /// <summary>
    ///    Gets a project by Uid, even if it is not archived
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<ProjectDataModel> GetProject_UnitTests(string projectUid)
    {
      var project = (await QueryWithAsyncPolicy<ProjectDataModel>(@"SELECT 
                ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, 
                fk_ProjectTypeID AS ProjectType, StartDate, EndDate, 
                ProjectTimeZone, ProjectTimeZoneIana, 
                ST_ASWKT(Boundary) AS Boundary,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                IsArchived, LastActionedUTC
              FROM Project 
              WHERE ProjectUID = @ProjectUID",
        new { ProjectUID = projectUid })).FirstOrDefault();
      return project;
    }

    public async Task<IEnumerable<ProjectDataModel>> GetProjectHistory_UnitTests(string projectUid)
    {
      return await QueryWithAsyncPolicy<ProjectDataModel>
      (@"SELECT 
                ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, 
                fk_ProjectTypeID AS ProjectType, StartDate, EndDate, 
                ProjectTimeZone, ProjectTimeZoneIana, 
                ST_ASWKT(Boundary) AS Boundary,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                IsArchived, LastActionedUTC
            FROM ProjectHistory 
            WHERE ProjectUID = @ProjectUID",
        new { ProjectUID = projectUid }
      );
    }

#endregion projects


#region projectSpatial

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
      string polygonToCheck = RepositoryHelper.WKTToSpatial(geometryWkt);

      var select = $@"SELECT DISTINCT
                          ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, 
                          fk_ProjectTypeID AS ProjectType, StartDate, EndDate, 
                          ProjectTimeZone, ProjectTimeZoneIana, 
                          ST_ASWKT(Boundary) AS Boundary,
                          CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                          IsArchived, LastActionedUTC
                        FROM Project
                        WHERE IsArchived = 0
                          AND @StartDate <= EndDate
                          AND @EndDate >= StartDate
                          AND CustomerUID = @CustomerUID
                          AND ProjectUid != @excludeProjectUid
                          AND st_Intersects({polygonToCheck}, Boundary) = 1";

      return (await QueryWithAsyncPolicy<ProjectDataModel>(select,
          new { CustomerUID = customerUid, StartDate = startDate.Date, EndDate = endDate.Date, excludeProjectUid }))
        .Any();
    }

    /// <summary>
    ///     Gets active projects for the customer
    ///     which the lat/long is within
    ///       optionally can check for within time 
    ///       note that projectTypes are only standard at present
    /// </summary>
    public Task<IEnumerable<ProjectDataModel>> GetIntersectingProjects(string customerUid,
      double latitude, double longitude, DateTime? timeOfPosition) 
    {
      var point = $"ST_GeomFromText('POINT({longitude} {latitude})')";

      var timeRangeString = string.Empty;
      if (timeOfPosition != null)
      {
        var formattedDate = (timeOfPosition.Value.Date.ToString("yyyy-MM-dd"));
        timeRangeString = $"  AND '{formattedDate}' BETWEEN StartDate AND EndDate ";
      }

      var select = "SELECT DISTINCT " +
                   "     ProjectUID, CustomerUID, ShortRaptorProjectID, Name, Description, "+
                   "     fk_ProjectTypeID AS ProjectType, StartDate, EndDate, "+
                   "     ProjectTimeZone, ProjectTimeZoneIana, "+
                   "     ST_ASWKT(Boundary) AS Boundary, " +
                   "     CoordinateSystemFileName, CoordinateSystemLastActionedUTC, "+
                   "     IsArchived, LastActionedUTC "+
                   "   FROM Project " +
                   "      WHERE IsArchived = 0 " +
                   $"        AND CustomerUID = '{customerUid}' " +
                   $"       {timeRangeString} " +
                   $"        AND st_Intersects({point}, Boundary) = 1";

      return QueryWithAsyncPolicy<ProjectDataModel>(select);
    }
    #endregion projectSpatial


    #region projectSettings

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
      return (await QueryWithAsyncPolicy<ProjectSettings>(@"SELECT 
                fk_ProjectUID AS ProjectUid, fk_ProjectSettingsTypeID AS ProjectSettingsType, Settings, UserID, LastActionedUTC
              FROM ProjectSettings
              WHERE fk_ProjectUID = @ProjectUid
                AND UserID = @UserID
                AND fk_ProjectSettingsTypeID = @ProjectSettingsType
              ORDER BY fk_ProjectUID, UserID, fk_ProjectSettingsTypeID",
          new { ProjectUid = projectUid, UserID = userId, ProjectSettingsType = projectSettingsType }))
        .FirstOrDefault();
    }

    /// <summary>
    /// At this stage 2 types, user must eval result
    /// </summary>
    /// <param name="projectUid"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public Task<IEnumerable<ProjectSettings>> GetProjectSettings(string projectUid, string userId)
    {
      return QueryWithAsyncPolicy<ProjectSettings>
      (@"SELECT 
                fk_ProjectUID AS ProjectUid, fk_ProjectSettingsTypeID AS ProjectSettingsType, Settings, UserID, LastActionedUTC
              FROM ProjectSettings
              WHERE fk_ProjectUID = @ProjectUid
                AND UserID = @UserID",
        new { ProjectUID = projectUid, UserID = userId }
      );
    }

    #endregion projectSettings


    #region importedFiles

    public async Task<ImportedFile> GetImportedFile(string importedFileUid)
    {
      var importedFile = (await QueryWithAsyncPolicy<ImportedFile>
      (@"SELECT 
            fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, LegacyImportedFileID, fk_CustomerUID as CustomerUID, fk_ImportedFileTypeID as ImportedFileType, 
            Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID as DxfUnitsType, 
            MinZoomLevel, MaxZoomLevel, IsDeleted, LastActionedUTC, Offset, fk_ReferenceImportedFileUID as ParentUID
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
    
    public async Task<IEnumerable<ImportedFile>> GetReferencedImportedFiles(string importedFileUid)
    {
      var importedFileList = (await QueryWithAsyncPolicy<ImportedFile>
      (@"SELECT 
            fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, LegacyImportedFileID, fk_CustomerUID as CustomerUID, fk_ImportedFileTypeID as ImportedFileType, 
            Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID as DxfUnitsType,
            MinZoomLevel, MaxZoomLevel, IsDeleted, LastActionedUTC, Offset, fk_ReferenceImportedFileUID as ParentUID 
          FROM ImportedFile
            WHERE fk_ReferenceImportedFileUID = @ImportedFileUid
              AND IsDeleted = 0",
        new { ImportedFileUid = importedFileUid }
      )).ToList();

      if (importedFileList.Count > 0)
      {
        //They will all belong to the same project
        var historyAllFiles = await GetImportedFileHistory(importedFileList[0].ProjectUid);
        foreach (var importedFile in importedFileList)
        {
          var historyOne = historyAllFiles.FindAll(x => x.ImportedFileUid == importedFile.ImportedFileUid);
          if (historyOne.Any())
          {
            importedFile.ImportedFileHistory = new ImportedFileHistory(historyOne);
          }
        }
      }

      return importedFileList;
    }

    public async Task<IEnumerable<ImportedFile>> GetImportedFiles(string projectUid)
    {
      var importedFileList = (await QueryWithAsyncPolicy<ImportedFile>
      (@"SELECT 
            fk_ProjectUID as ProjectUID, ImportedFileUID, ImportedFileID, LegacyImportedFileID, fk_CustomerUID as CustomerUID, fk_ImportedFileTypeID as ImportedFileType, 
            Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, fk_DXFUnitsTypeID as DxfUnitsType,
            MinZoomLevel, MaxZoomLevel, IsDeleted, LastActionedUTC, Offset, fk_ReferenceImportedFileUID as ParentUID 
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
        new { projectUid, ImportedFileUid = importedFileUid }
      )).ToList();
    }

    #endregion importedFiles


    // this geofence code is used by projectSvc and FilterSvc and wll refer to database within the service
    #region geofenceForFilters 

    /// <summary>
    /// Gets the list of geofence UIDs associated wih the specified project
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>List of associations</returns>
    public Task<IEnumerable<ProjectGeofence>> GetAssociatedGeofences(string projectUid)
    {
      return QueryWithAsyncPolicy<ProjectGeofence>
      (@"SELECT 
                fk_GeofenceUID AS GeofenceUID, fk_ProjectUID AS ProjectUID, pg.LastActionedUTC, g.fk_GeofenceTypeID AS GeofenceType 
              FROM ProjectGeofence pg
                LEFT OUTER JOIN Geofence g on g.GeofenceUID = pg.fk_GeofenceUID
              WHERE fk_ProjectUID = @ProjectUID",
        new { ProjectUID = projectUid }
      );
    }


    /// <summary>
    /// Determines which, if any, of the given geofence polygons intersect the project polygon
    /// </summary>
    public async Task<IEnumerable<bool>> DoPolygonsOverlap(string projectGeometryWkt, IEnumerable<string> geometryWkts)
    {
      if (geometryWkts == null || !geometryWkts.Any())
        return new List<bool>();

      var tasks = new List<Task<bool>>();
      var list = geometryWkts.ToList();

      for (var i = 0; i < list.Count; i++)
      {
        tasks.Add(DoesPolygonOverlap(projectGeometryWkt, list[i]));
      }
      await Task.WhenAll(tasks);
      return tasks.Select(t => t.Result);
    }

    /// <summary>
    /// Determines if the given geofence polygon intersects the project polygon
    /// </summary>
    private async Task<bool> DoesPolygonOverlap(string projectGeometryWkt, string geometryWkt)
    {
      //Do some basic checking first to avoid filling the logs with lots of exceptions
      if (string.IsNullOrEmpty(geometryWkt) || !geometryWkt.StartsWith("POLYGON"))
      {
        Log.LogDebug($"DoesPolygonOverlap: No geofence polygon to test {geometryWkt}");
        return false;
      }

      string polygonToCheck = RepositoryHelper.WKTToSpatial(geometryWkt);
      var select = $@"SELECT st_IsValid({polygonToCheck})";
      var result = (await QueryWithAsyncPolicy<int>(select)).FirstOrDefault();
      if (result == 0)
      {
        Log.LogDebug($"DoesPolygonOverlap: Invalid geofence to test {geometryWkt}");
        return false;
      }

      string projectPolygon = RepositoryHelper.WKTToSpatial(projectGeometryWkt);

      select = $@"SELECT st_Intersects({polygonToCheck}, {projectPolygon})";
      result = (await QueryWithAsyncPolicy<int>(select)).FirstOrDefault();

      return result == 1;
    }

    #endregion geofenceForFilters
      
  }
}
