using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CCSS.Geometry;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VSS.Common.Abstractions.Configuration;
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
      if (evt is CreateImportedFileEvent)
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
              ( ProjectUID, CustomerUID, ShortRaptorProjectID, Name,
                fk_ProjectTypeID, ProjectTimeZone, ProjectTimeZoneIana, Boundary,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC, 
                IsArchived, LastActionedUTC)
              SELECT 
                  ProjectUID, CustomerUID, ShortRaptorProjectID, Name, fk_ProjectTypeID,
                  ProjectTimeZone, ProjectTimeZoneIana, Boundary,
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

    #endregion geofenceForFilters

  }
}
