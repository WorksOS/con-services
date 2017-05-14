using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using VSS.VisionLink.Interfaces.Events.MasterData.Interfaces;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using VSS.GenericConfiguration;
using Repositories.DBModels;

namespace Repositories
{
  public class ProjectRepository : RepositoryBase, IRepository<IProjectEvent>
  {
    private readonly ILogger log;

    public ProjectRepository(IConfigurationStore _connectionString, ILoggerFactory logger) : base(_connectionString)
    {
      log = logger.CreateLogger<ProjectRepository>();
    }

    #region store
    public async Task<int> StoreEvent(IProjectEvent evt)
    {
      const string polygonStr = "POLYGON";
      var upsertedCount = 0;
      if (evt is CreateProjectEvent)
      {
        var projectEvent = (CreateProjectEvent)evt;
        var project = new Project();
        project.LegacyProjectID = projectEvent.ProjectID;
        project.Description = projectEvent.Description;
        project.Name = projectEvent.ProjectName;
        project.ProjectTimeZone = projectEvent.ProjectTimezone;
        project.LandfillTimeZone = TimeZone.WindowsToIana(projectEvent.ProjectTimezone);
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.EndDate = projectEvent.ProjectEndDate.Date;
        project.LastActionedUTC = projectEvent.ActionUTC;
        project.StartDate = projectEvent.ProjectStartDate.Date;
        project.ProjectType = projectEvent.ProjectType;

        if (!string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = projectEvent.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = projectEvent.ActionUTC;
        }

        //Don't write if there is no boundary defined
        if (!String.IsNullOrEmpty(projectEvent.ProjectBoundary))
        {
          // Check whether the ProjectBoundary is in WKT format. Convert to the WKT format if it is not. 
          if (!projectEvent.ProjectBoundary.Contains(polygonStr))
          {
            projectEvent.ProjectBoundary =
                projectEvent.ProjectBoundary.Replace(",", " ").Replace(";", ",").TrimEnd(',');
            projectEvent.ProjectBoundary = String.Concat(polygonStr + "((", projectEvent.ProjectBoundary, "))");
          }

          project.GeometryWKT = projectEvent.ProjectBoundary;
          upsertedCount = await UpsertProjectDetail(project, "CreateProjectEvent");
        }
      }
      else if (evt is UpdateProjectEvent)
      {
        var projectEvent = (UpdateProjectEvent)evt;

        var project = new Project();
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.Name = projectEvent.ProjectName;
        project.Description = projectEvent.Description;
        project.EndDate = projectEvent.ProjectEndDate.Date;
        project.LastActionedUTC = projectEvent.ActionUTC;
        project.ProjectType = projectEvent.ProjectType;

        if (!string.IsNullOrEmpty(projectEvent.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = projectEvent.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = projectEvent.ActionUTC;
        }
        upsertedCount = await UpsertProjectDetail(project, "UpdateProjectEvent");
      }
      else if (evt is DeleteProjectEvent)
      {
        var projectEvent = (DeleteProjectEvent)evt;
        var project = new Project();
        project.ProjectUID = projectEvent.ProjectUID.ToString();
        project.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = await UpsertProjectDetail(project, "DeleteProjectEvent");
      }
      else if (evt is AssociateProjectCustomer)
      {
        var projectEvent = (AssociateProjectCustomer)evt;
        var customerProject = new CustomerProject();
        customerProject.ProjectUID = projectEvent.ProjectUID.ToString();
        customerProject.CustomerUID = projectEvent.CustomerUID.ToString();
        customerProject.LegacyCustomerID = projectEvent.LegacyCustomerID;
        customerProject.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = await UpsertCustomerProjectDetail(customerProject, "AssociateProjectCustomerEvent");
      }
      else if (evt is AssociateProjectGeofence)
      {
        var projectEvent = (AssociateProjectGeofence)evt;
        var projectGeofence = new ProjectGeofence();
        projectGeofence.ProjectUID = projectEvent.ProjectUID.ToString();
        projectGeofence.GeofenceUID = projectEvent.GeofenceUID.ToString();
        projectGeofence.LastActionedUTC = projectEvent.ActionUTC;
        upsertedCount = await UpsertProjectGeofenceDetail(projectGeofence, "AssociateProjectGeofenceEvent");
      }
      else if (evt is DissociateProjectCustomer)
      {
        throw new NotImplementedException("Dissociating projects from customers is not supported");
      }
      else if (evt is CreateImportedFileEvent)
      {
        var projectEvent = (CreateImportedFileEvent)evt;
        var importedFile = new ImportedFile
        {
          ProjectUid = projectEvent.ProjectUID.ToString(),
          ImportedFileUid = projectEvent.ImportedFileUID.ToString(),
          CustomerUid = projectEvent.CustomerUID.ToString(),
          ImportedFileType = projectEvent.ImportedFileType,
          Name = projectEvent.Name,
          FileDescriptor = projectEvent.FileDescriptor,
          FileCreatedUtc = projectEvent.FileCreatedUtc,
          FileUpdatedUtc = projectEvent.FileUpdatedUtc,
          ImportedBy = projectEvent.ImportedBy,
          SurveyedUtc = projectEvent.SurveyedUTC,
          IsDeleted = false,
          LastActionedUtc = projectEvent.ActionUTC
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
          LastActionedUtc = projectEvent.ActionUTC
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
        upsertedCount = await UpsertImportedFile(importedFile, "DeleteImportedFileEvent");
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
    private async Task<int> UpsertProjectDetail(Project project, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      var existing = (await Connection.QueryAsync<Project>
          (@"SELECT 
                ProjectUID, Description, LegacyProjectID, Name, fk_ProjectTypeID AS ProjectType, IsDeleted,
                ProjectTimeZone, LandfillTimeZone, 
                LastActionedUTC, StartDate, EndDate, GeometryWKT,
                CoordinateSystemFileName, CoordinateSystemLastActionedUTC
              FROM Project
              WHERE ProjectUID = @projectUid
                OR LegacyProjectId = @legacyProjectId", new { projectUid = project.ProjectUID, legacyProjectId = project.LegacyProjectID }
           )).FirstOrDefault();

      if (eventType == "CreateProjectEvent")
      {
        upsertedCount = await CreateProject(project, existing);
      }

      if (eventType == "UpdateProjectEvent")
      {
        upsertedCount = await UpdateProject(project, existing);
      }

      if (eventType == "DeleteProjectEvent")
      {
        upsertedCount = await DeleteProject(project, existing);
      }

      PerhapsCloseConnection();
      return upsertedCount;
    }

    private async Task<int> CreateProject(Project project, Project existing)
    {
      var upsertedCount = 0;
      if (project.StartDate > project.EndDate)
      {
        log.LogDebug("Project " + project.ProjectUID + "Will not be created, startDate > endDate");
        return upsertedCount;
      }

      else if (existing == null)
      {
        log.LogDebug("ProjectRepository/CreateProject: going to create project={0}))')", JsonConvert.SerializeObject(project));
        var formattedPolygon = string.Format("ST_GeomFromText('{0}')", project.GeometryWKT);

        string insert = null;
        if (project.LegacyProjectID <= 0) // allow db autoincrement on legacyProjectID
          insert = string.Format(
              "INSERT Project " +
              "    (ProjectUID, Name, Description, fk_ProjectTypeID, IsDeleted, ProjectTimeZone, LandfillTimeZone, LastActionedUTC, StartDate, EndDate, GeometryWKT, PolygonST, CoordinateSystemFileName, CoordinateSystemLastActionedUTC) " +
              "  VALUES " +
              "    (@ProjectUID, @Name, @Description, @ProjectType, @IsDeleted, @ProjectTimeZone, @LandfillTimeZone, @LastActionedUTC, @StartDate, @EndDate, @GeometryWKT, {0}, @CoordinateSystemFileName, @CoordinateSystemLastActionedUTC)"
                , formattedPolygon);
        else
          insert = string.Format(
              "INSERT Project " +
              "    (ProjectUID, LegacyProjectID, Name, Description, fk_ProjectTypeID, IsDeleted, ProjectTimeZone, LandfillTimeZone, LastActionedUTC, StartDate, EndDate, GeometryWKT, PolygonST, CoordinateSystemFileName, CoordinateSystemLastActionedUTC ) " +
              "  VALUES " +
              "    (@ProjectUID, @LegacyProjectID, @Name, @Description, @ProjectType, @IsDeleted, @ProjectTimeZone, @LandfillTimeZone, @LastActionedUTC, @StartDate, @EndDate, @GeometryWKT, {0}, @CoordinateSystemFileName, @CoordinateSystemLastActionedUTC)"
                , formattedPolygon);
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(insert, project);
          log.LogDebug("ProjectRepository/CreateProject: (insert): upserted {0} rows (1=insert, 2=update) for: projectUid:{1}", upsertedCount, project.ProjectUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }
      else if (string.IsNullOrEmpty(existing.Name))
      {
        log.LogDebug("ProjectRepository/CreateProject: going to update a dummy project={0}", JsonConvert.SerializeObject(project));

        // this code comes from landfill, however in MD, no dummy is created. 
        //   is this obsolete?
        const string update =
            @"UPDATE Project                
                SET LegacyProjectID = @LegacyProjectID,
                  Name = @Name,
                  Description = @Description,
                  fk_ProjectTypeID = @ProjectType,
                  IsDeleted = @IsDeleted,
                  ProjectTimeZone = @ProjectTimeZone,
                  LandfillTimeZone = @LandfillTimeZone,
                  StartDate = @StartDate,
                  EndDate = @EndDate,
                  LastActionedUTC = @LastActionedUTC,
                  GeometryWKT = @GeometryWKT,
                  CoordinateSystemFileName = @CoordinateSystemFileName,
                  CoordinateSystemLastActionedUTC = @CoordinateSystemLastActionedUTC
                WHERE ProjectUID = @ProjectUID";
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(update, project);
          log.LogDebug("ProjectRepository/CreateProject: (update): upserted {0} rows (1=insert, 2=update) for: projectUid:{1}", upsertedCount, project.ProjectUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }
      else if (existing.LastActionedUTC >= project.LastActionedUTC)
      {
        log.LogDebug("ProjectRepository/CreateProject: create arrived after an update so inserting project={0}", JsonConvert.SerializeObject(project));

        // must be a later update was applied before the create arrived
        // leave the more recent EndDate, Name, Description, ProjectType and actionUTC alone

        // a more recent cs exists, leave it
        if (!string.IsNullOrEmpty(existing.CoordinateSystemFileName))
        {
          project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
          project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
        }

        const string update =
            @"UPDATE Project                
                SET LegacyProjectID = @LegacyProjectID,                  
                  ProjectTimeZone = @ProjectTimeZone,
                  LandfillTimeZone = @LandfillTimeZone,
                  StartDate = @StartDate,
                  GeometryWKT = @GeometryWKT,
                  CoordinateSystemFileName = @CoordinateSystemFileName,
                  CoordinateSystemLastActionedUTC = @CoordinateSystemLastActionedUTC
                WHERE ProjectUID = @ProjectUID";
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(update, project);
          log.LogDebug("ProjectRepository/CreateProject: (updateExisting): upserted {0} rows (1=insert, 2=update) for: projectUid:{1}", upsertedCount, project.ProjectUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }

      log.LogDebug("ProjectRepository/CreateProject: can't create as already exists project {0}.", JsonConvert.SerializeObject(project));
      return upsertedCount;
    }

    private async Task<int> DeleteProject(Project project, Project existing)
    {
      var upsertedCount = 0;
      if (existing != null)
      {
        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          log.LogDebug("ProjectRepository/DeleteProject: updating project={0}", JsonConvert.SerializeObject(project));

          const string update =
            @"UPDATE Project                
                SET IsDeleted = 1,
                  LastActionedUTC = @LastActionedUTC
                WHERE ProjectUID = @ProjectUID";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(update, project);
            log.LogDebug("ProjectRepository/DeleteProject: upserted {0} rows (1=insert, 2=update) for: projectUid:{1}", upsertedCount, project.ProjectUID);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
          });
        }
        else
        {
          log.LogDebug("ProjectRepository/DeleteProject: old delete event ignored project={0}", JsonConvert.SerializeObject(project));
        }
      }
      else
      {
        log.LogDebug("ProjectRepository/DeleteProject: can't delete as none existing ignored project={0}", JsonConvert.SerializeObject(project));
      }
      return upsertedCount;
    }

    private async Task<int> UpdateProject(Project project, Project existing)
    {
      var upsertedCount = 0;
      if (project.EndDate < existing.StartDate)
      {
        log.LogDebug("ProjectRepository/UpdateProject: failed to update project={0} EndDate < StartDate", JsonConvert.SerializeObject(project));
        return upsertedCount;
      }
      else if (existing != null)
      {
        if (project.LastActionedUTC >= existing.LastActionedUTC)
        {
          project.Name = project.Name == null ? existing.Name : project.Name;
          project.Description = project.Description == null ? existing.Description : project.Description;
          project.ProjectTimeZone = project.ProjectTimeZone == null ? existing.ProjectTimeZone : project.ProjectTimeZone;
          if (string.IsNullOrEmpty(project.CoordinateSystemFileName))
          {
            project.CoordinateSystemFileName = existing.CoordinateSystemFileName;
            project.CoordinateSystemLastActionedUTC = existing.CoordinateSystemLastActionedUTC;
          }
          log.LogDebug("ProjectRepository/UpdateProject: updating project={0}", JsonConvert.SerializeObject(project));

          const string update =
            @"UPDATE Project                
                SET Name = @Name,
                  Description = @Description,
                  LastActionedUTC = @LastActionedUTC,
                  EndDate = @EndDate, 
                  fk_ProjectTypeID = @ProjectType,
                  CoordinateSystemFileName = @CoordinateSystemFileName,
                  CoordinateSystemLastActionedUTC = @CoordinateSystemLastActionedUTC
                WHERE ProjectUID = @ProjectUID";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(update, project);
            log.LogDebug("ProjectRepository/UpdateProject: upserted {0} rows (1=insert, 2=update) for: projectUid:{1}", upsertedCount, project.ProjectUID);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
          });
        }
        else
        {
          log.LogDebug("ProjectRepository/UpdateProject: old update event ignored project={0}", JsonConvert.SerializeObject(project));
        }
      }
      else
      {
        log.LogDebug("ProjectRepository/UpdateProject: can't update as none existing project={0}", JsonConvert.SerializeObject(project));
      }
      return upsertedCount;
    }

    private async Task<int> UpsertCustomerProjectDetail(CustomerProject customerProject, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      var existing = (await Connection.QueryAsync<CustomerProject>
          (@"SELECT 
                fk_CustomerUID AS CustomerUID, LegacyCustomerID, fk_ProjectUID AS ProjectUID, LastActionedUTC
              FROM CustomerProject
              WHERE fk_CustomerUID = @customerUID AND fk_ProjectUID = @projectUID",
          new { customerUID = customerProject.CustomerUID, projectUID = customerProject.ProjectUID }
          )).FirstOrDefault();

      if (eventType == "AssociateProjectCustomerEvent")
      {
        upsertedCount = await AssociateProjectCustomer(customerProject, existing);
      }

      PerhapsCloseConnection();
      return upsertedCount;
    }

    #endregion store


    #region associate
    private async Task<int> AssociateProjectCustomer(CustomerProject customerProject, CustomerProject existing)
    {
      var upsertedCount = 0;

      log.LogDebug("ProjectRepository/AssociateProjectCustomer: can't update as none existing customerProject={0}", JsonConvert.SerializeObject(customerProject));
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
      return await dbAsyncPolicy.ExecuteAsync(async () =>
      {
        upsertedCount = await Connection.ExecuteAsync(insert, customerProject);
        log.LogDebug("ProjectRepository/AssociateProjectCustomer: upserted {0} rows (1=insert, 2=update) for: customerProjectUid:{1}", upsertedCount, customerProject.CustomerUID);
        return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
      });

    }

    private async Task<int> UpsertProjectGeofenceDetail(ProjectGeofence projectGeofence, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      //    Log.DebugFormat("ProjectRepository: Upserting eventType={0} ProjectUid={1}, GeofenceUid={2}",
      //        eventType, projectGeofence.ProjectUID, projectGeofence.GeofenceUID);

      var existing = (await Connection.QueryAsync<ProjectGeofence>
        (@"SELECT 
              fk_GeofenceUID AS GeofenceUID, fk_ProjectUID AS ProjectUID, LastActionedUTC
            FROM ProjectGeofence
            WHERE fk_ProjectUID = @projectUID AND fk_GeofenceUID = @geofenceUID",
         new { projectUID = projectGeofence.ProjectUID, geofenceUID = projectGeofence.GeofenceUID }
         )).FirstOrDefault();

      if (eventType == "AssociateProjectGeofenceEvent")
      {
        upsertedCount = await AssociateProjectGeofence(projectGeofence, existing);
      }

      PerhapsCloseConnection();
      return upsertedCount;
    }

    private async Task<int> AssociateProjectGeofence(ProjectGeofence projectGeofence, ProjectGeofence existing)
    {
      var upsertedCount = 0;
      if (existing == null)
      {
        log.LogDebug("ProjectRepository/AssociateProjectGeofence: can't update as none existing projectGeofence={0}", JsonConvert.SerializeObject(projectGeofence));

        const string insert =
          @"INSERT ProjectGeofence
                (fk_GeofenceUID, fk_ProjectUID, LastActionedUTC)
              VALUES
                (@GeofenceUID, @ProjectUID, @LastActionedUTC)";

        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(insert, projectGeofence);
          log.LogDebug("ProjectRepository/AssociateProjectGeofence: upserted {0} rows (1=insert, 2=update) for: projectUid:{1} geofenceUid:{2}", upsertedCount, projectGeofence.ProjectUID, projectGeofence.GeofenceUID);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }

      log.LogDebug("ProjectRepository/AssociateProjectGeofence: can't create as already exists projectGeofence={0}", JsonConvert.SerializeObject(projectGeofence));
      return upsertedCount;
    }
    #endregion associate


    #region importedFiles
    private async Task<int> UpsertImportedFile(ImportedFile importedFile, string eventType)
    {
      int upsertedCount = 0;

      await PerhapsOpenConnection();

      var existing = (await Connection.QueryAsync<ImportedFile>
      (@"SELECT 
              fk_ProjectUID as ProjectUID, ImportedFileUID, fk_CustomerUID as CustomerUID, 
              fk_ImportedFileTypeID as ImportedFileType, Name, 
              FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, IsDeleted,
              LastActionedUTC
            FROM ImportedFile
            WHERE ImportedFileUID = @importedFileUid", new { importedFileUid = importedFile.ImportedFileUid }
      )).FirstOrDefault();

      if (eventType == "CreateImportedFileEvent")
      {
        upsertedCount = await CreateImportedFile(importedFile, existing);
      }

      if (eventType == "UpdateImportedFileEvent")
      {
        upsertedCount = await UpdateImportedFile(importedFile, existing);
      }

      if (eventType == "DeleteImportedFileEvent")
      {
        upsertedCount = await DeleteImportedFile(importedFile, existing);
      }

      PerhapsCloseConnection();
      return upsertedCount;
    }

    private async Task<int> CreateImportedFile(ImportedFile importedFile, ImportedFile existing)
    {
      var upsertedCount = 0;

      if (existing == null)
      {
        log.LogDebug("ProjectRepository/CreateImportedFile: going to create importedFile={0}))')", JsonConvert.SerializeObject(importedFile));
        
        string insert = string.Format(
            "INSERT ImportedFile " +
            "    (fk_ProjectUID, ImportedFileUID, fk_CustomerUID, fk_ImportedFileTypeID, Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, IsDeleted, LastActionedUTC) " +
            "  VALUES " +
            "    (@ProjectUid, @ImportedFileUid, @CustomerUid, @ImportedFileType, @Name, @FileDescriptor, @FileCreatedUTC, @FileUpdatedUTC, @ImportedBy, @SurveyedUtc, 0, @LastActionedUtc)");
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(insert, importedFile);
          log.LogDebug("ProjectRepository/CreateImportedFile: (insert): upserted {0} rows (1=insert, 2=update) for: projectUid:{1} importedFileUid: {2}", upsertedCount, importedFile.ProjectUid, importedFile.ImportedFileUid);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }
      else if (existing.LastActionedUtc >= importedFile.LastActionedUtc)
      {
        log.LogDebug("ProjectRepository/CreateImportedFile: create arrived after an update so inserting importedFile={0}", JsonConvert.SerializeObject(importedFile));

        // must be a later update was applied before the create arrived.
        // The only thing which can be updated is a) the file content, and the LastActionedUtc. A file cannot be moved between projects/customers.
        // We don't store (a), and leave actionUTC as the more recent. 
        const string update =
          @"UPDATE ImportedFile                
                SET fk_ProjectUID = @projectUID,                  
                  fk_CustomerUID = @customerUID,
                  fk_ImportedFileTypeID = @importedFileType,
                  Name = @name,
                  FileDescriptor = @fileDescriptor,
                  FileCreatedUTC = @fileCreatedUTC,
                  FileUpdatedUTC = @fileUpdatedUTC,
                  ImportedBy = @importedBy, 
                  SurveyedUTC = @surveyedUTC
                WHERE ImportedFileUID = @ImportedFileUid";
        return await dbAsyncPolicy.ExecuteAsync(async () =>
        {
          upsertedCount = await Connection.ExecuteAsync(update, importedFile);
          log.LogDebug("ProjectRepository/CreateImportedFile: (updateExisting): upserted {0} rows (1=insert, 2=update) for: projectUid:{1} importedFileUid: {2}", upsertedCount, importedFile.ProjectUid, importedFile.ImportedFileUid);
          return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
        });
      }

      log.LogDebug("ProjectRepository/CreateImportedFile: can't create as already exists importedFile {0}.", JsonConvert.SerializeObject(importedFile));
      return upsertedCount;
    }

    private async Task<int> UpdateImportedFile(ImportedFile importedFile, ImportedFile existing)
    {
      // The only thing which can be updated is a) the file content, and the LastActionedUtc. A file cannot be moved between projects/customers.
      // We don't store (a), and leave actionUTC as the more recent. 
      var upsertedCount = 0;
      if (existing != null)
      {
        if (importedFile.LastActionedUtc > existing.LastActionedUtc)
        {
          const string update =
            @"UPDATE ImportedFile                
                SET 
                  FileDescriptor = @fileDescriptor,
                  FileCreatedUTC = @fileCreatedUtc,
                  FileUpdatedUTC = @fileUpdatedUtc,
                  ImportedBy = @importedBy, 
                  SurveyedUTC = @surveyedUTC,
                  LastActionedUTC = @LastActionedUTC
                WHERE ImportedFileUID = @ImportedFileUid";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(update, importedFile);
            log.LogDebug("ProjectRepository/UpdateImportedFile: upserted {0} rows (1=insert, 2=update) for: projectUid:{1} importedFileUid: {2}", upsertedCount, importedFile.ProjectUid, importedFile.ImportedFileUid);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
          });
        }
        else
        {
          log.LogDebug("ProjectRepository/UpdateImportedFile: old update event ignored importedFile {0}", JsonConvert.SerializeObject(importedFile));
        }
      }
      else
      {
        log.LogDebug("ProjectRepository/UpdateImportedFile: can't update as none existing importedFile {0}", JsonConvert.SerializeObject(importedFile));
      }
      return upsertedCount;
    }

    private async Task<int> DeleteImportedFile(ImportedFile importedFile, ImportedFile existing)
    {
      var upsertedCount = 0;
      if (existing != null)
      {
        if (importedFile.LastActionedUtc >= existing.LastActionedUtc)
        {
          log.LogDebug("ProjectRepository/DeleteImportedFile: deleting importedFile {0}", JsonConvert.SerializeObject(importedFile));

          const string update =
            @"Update ImportedFile                               
                SET IsDeleted = 1,
                    LastActionedUTC = @LastActionedUTC
                WHERE ImportedFileUID = @ImportedFileUid";
          return await dbAsyncPolicy.ExecuteAsync(async () =>
          {
            upsertedCount = await Connection.ExecuteAsync(update, importedFile);
            log.LogDebug("ProjectRepository/DeleteImportedFile: upserted {0} rows (1=insert, 2=update) for: projectUid:{1} importedFileUid: {2}", upsertedCount, importedFile.ProjectUid, importedFile.ImportedFileUid);
            return upsertedCount == 2 ? 1 : upsertedCount; // 2=1RowUpdated; 1=1RowInserted; 0=noRowsInserted       
          });
        }
        else
        {
          log.LogDebug("ProjectRepository/DeleteImportedFile: old delete event ignored importedFile={0}", JsonConvert.SerializeObject(importedFile));
        }
      }
      else
      {
        log.LogDebug("ProjectRepository/DeleteImportedFile: can't delete as none existing ignored importedFile={0}", JsonConvert.SerializeObject(importedFile));
      }
      return upsertedCount;
    }

    #endregion importedFiles


    #region getters

    /// <summary>
    /// There may be 0 or n subscriptions for this project. None/many may be current. 
    /// This method just gets ANY one of these or no subs (SubscriptionUID == null)
    /// We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<Project> GetProject(string projectUid)
    {
      await PerhapsOpenConnection();

      var project = (await Connection.QueryAsync<Project>
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
              WHERE p.ProjectUID = @projectUid AND p.IsDeleted = 0",
            new { projectUid }
          )).FirstOrDefault();

      PerhapsCloseConnection();
      return project;
    }

    /// <summary>
    /// Gets by legacyProjectID. No subs
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<Project> GetProject(long legacyProjectID)
    {
      await PerhapsOpenConnection();

      var project = (await Connection.QueryAsync<Project>
          (@"SELECT 
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,                     
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC,
                cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID               
              FROM Project p 
                JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID
                JOIN Customer c on c.CustomerUID = cp.fk_CustomerUID                
              WHERE p.LegacyProjectID = @legacyProjectID 
                AND p.IsDeleted = 0",
            new { legacyProjectID }
          )).FirstOrDefault();

      PerhapsCloseConnection();
      return project;
    }


    /// <summary>
    /// There may be 0 or n subscriptions for this project. None/many may be current. 
    /// This method just gets ANY one of these or no subs (SubscriptionUID == null)
    /// We don't care, up to the calling code to decipher.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Project>> GetProjectAndSubscriptions(long legacyProjectID, DateTime validAtDate)
    {
      await PerhapsOpenConnection();

      var projectSubList = (await Connection.QueryAsync<Project>
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
              WHERE p.LegacyProjectID = @legacyProjectID 
                AND p.IsDeleted = 0
                AND @validAtDate BETWEEN s.StartDate AND s.EndDate",
            new { legacyProjectID, validAtDate = validAtDate.Date }
          ));

      PerhapsCloseConnection();
      return projectSubList;
    }

    /// <summary>
    /// gets only 1 row for a particular sub. only 1 projectUID and be associated with a sub
    /// </summary>
    /// <param name="subscriptionUid"></param>
    /// <returns></returns>
    public async Task<Project> GetProjectBySubcription(string subscriptionUid)
    {
      await PerhapsOpenConnection();

      var projects = (await Connection.QueryAsync<Project>
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
              WHERE ps.fk_SubscriptionUID = @subscriptionUid AND p.IsDeleted = 0",
              new { subscriptionUid }
          )).FirstOrDefault(); ;

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
    public async Task<IEnumerable<Project>> GetProjectsForUser(string userUid)
    {
      await PerhapsOpenConnection();
      var projects = (await Connection.QueryAsync<Project>
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
              WHERE cu.UserUID = @userUid and p.IsDeleted = 0",
            new { userUid }
          ));

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
    public async Task<IEnumerable<Project>> GetProjectsForCustomerUser(string customerUid, string userUid)
    {
      await PerhapsOpenConnection();

      var projects = (await Connection.QueryAsync<Project>
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
              WHERE cp.fk_CustomerUID = @customerUid and cu.UserUID = @userUid and p.IsDeleted = 0",
            new { customerUid, userUid }
          ));

      PerhapsCloseConnection();
      return projects;
    }

    /// <summary>
    /// There may be 0 or n subscriptions for each project. None/many may be current. 
    /// This method gets the latest EndDate so at most 1 sub per project
    /// Also returns the GeofenceWRK. List returned includes archived projects.
    /// </summary>
    /// <param name="customerUid"></param>
    /// <param name="userUid"></param>
    /// <returns></returns>
    public async Task<IEnumerable<Project>> GetProjectsForCustomer(string customerUid)
    {
      await PerhapsOpenConnection();
      // mysql doesn't have any nice mssql features like rowNumber/paritionBy, so quicker to do in c#
      var projects = (await Connection.QueryAsync<Project>
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
                LEFT OUTER JOIN ProjectGeofence pg on pg.fk_ProjectUID = p.ProjectUID
                LEFT OUTER JOIN Geofence g on g.GeofenceUID = pg.fk_GeofenceUID 
              WHERE c.CustomerUID = @customerUid 
                AND (g.fk_GeofenceTypeID IS NULL 
                      OR (g.IsDeleted = 0 AND g.fk_GeofenceTypeID = 1)
                    )",
            new { customerUid }
          ));

      PerhapsCloseConnection();

      // need to get the row with the later SubscriptionEndDate if there are duplicates
      // Also if there are >1 projectGeofences.. hmm.. it will just return either
      return projects.OrderByDescending(proj => proj.SubscriptionEndDate).GroupBy(d => d.ProjectUID).Select(g => g.First()).ToList();
    }

    /// <summary>
    /// Gets the specified project without linked data like customer and subscription.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>The project</returns>
    public async Task<Project> GetProjectOnly(string projectUid)
    {
      await PerhapsOpenConnection();

      var project = (await Connection.QueryAsync<Project>
          (@"SELECT              
                p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone,                     
                p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT,
                p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC
              FROM Project p 
              WHERE p.ProjectUID = @projectUid",
            new { projectUid }
          )).FirstOrDefault();

      PerhapsCloseConnection();
      return project;
    }

    /// <summary>
    /// Checks if a project with the specified projectUid exists.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>true if project exists or false otherwise</returns>
    public async Task<bool> ProjectExists(string projectUid)
    {
      await PerhapsOpenConnection();

      var uid = (await Connection.QueryAsync<string>
          (@"SELECT p.ProjectUID             
              FROM Project p 
              WHERE p.ProjectUID = @projectUid",
            new { projectUid }
          )).FirstOrDefault();

      PerhapsCloseConnection();
      return !string.IsNullOrEmpty(uid);
    }

    /// <summary>
    /// Checks if a project with the specified projectUid is associated with a customer.
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns>true if project is associated with a customer or false otherwise</returns>
    public async Task<bool> CustomerProjectExists(string projectUid)
    {
      await PerhapsOpenConnection();

      var uid = (await Connection.QueryAsync<string>
          (@"SELECT cp.fk_ProjectUID             
              FROM CustomerProject cp 
              WHERE cp.fk_ProjectUID = @projectUid",
            new { projectUid }
          )).FirstOrDefault();

      PerhapsCloseConnection();
      return !string.IsNullOrEmpty(uid);
    }

    /// <summary>
    /// for unit tests - so we don't have to create everything (associations) for a test
    /// </summary>
    /// <param name="projectUid"></param>
    /// <returns></returns>
    public async Task<Project> GetProject_UnitTest(string projectUid)
    {
      await PerhapsOpenConnection();

      var project = (await Connection.QueryAsync<Project>
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
              WHERE p.ProjectUID = @projectUid",
            new { projectUid }
          )).FirstOrDefault();

      PerhapsCloseConnection();
      return project;
    }

    #endregion getters


    #region gettersImportedFiles

    public async Task<IEnumerable<ImportedFile>> GetImportedFiles(string projectUid)
    {
      await PerhapsOpenConnection();

      var importedFileList = (await Connection.QueryAsync<ImportedFile>
      (@"SELECT 
              fk_ProjectUID as ProjectUID, ImportedFileUID, fk_CustomerUID as CustomerUID, fk_ImportedFileTypeID as ImportedFileType, 
              Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, IsDeleted,
              LastActionedUTC
            FROM ImportedFile
              WHERE fk_ProjectUID = @projectUid
                AND IsDeleted = 0",
        new { projectUid }
      ));

      PerhapsCloseConnection();
      return importedFileList;
    }

    public async Task<ImportedFile> GetImportedFile(string importedFileUid)
    {
      await PerhapsOpenConnection();

      var importedFile = (await Connection.QueryAsync<ImportedFile>
      (@"SELECT 
              fk_ProjectUID as ProjectUID, ImportedFileUID, fk_CustomerUID as CustomerUID, fk_ImportedFileTypeID as ImportedFileType, 
              Name, FileDescriptor, FileCreatedUTC, FileUpdatedUTC, ImportedBy, SurveyedUTC, IsDeleted,
              LastActionedUTC
            FROM ImportedFile
              WHERE importedFileUID = @importedFileUid",
        new { importedFileUid }
      )).FirstOrDefault();

      PerhapsCloseConnection();
      return importedFile;
    }
    #endregion gettersImportedFiles


    #region gettersSpatial
    /// <summary>
    /// Gets any standard project which the lat/long is within,
    ///     which satisfies all conditions for the asset
    /// </summary>
    /// <param name="customerUID"></param>
    /// <param name="latitude"></param>
    /// <param name="longitude"></param>
    /// <param name="timeOfPosition"></param>
    /// <returns>The project</returns>
    public async Task<IEnumerable<Project>> GetStandardProject(string customerUID, double latitude, double longitude, DateTime timeOfPosition)
    {
      await PerhapsOpenConnection();

      string point = string.Format("ST_GeomFromText('POINT({0} {1})')", longitude, latitude);
      string select = string.Format(
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
        "        AND cp.fk_CustomerUID = @customerUID " +
        "        AND st_Intersects({0}, PolygonST) = 1"
            , point);

      var projects = (await Connection.QueryAsync<Project>(select,  new { customerUID, timeOfPosition = timeOfPosition.Date } ));
     
      PerhapsCloseConnection();
      return projects;
    }

    /// <summary>
    /// Gets any ProjectMonitoring or Landfill (as requested) project which the lat/long is within,
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
      await PerhapsOpenConnection();

      string point = string.Format("ST_GeomFromText('POINT({0} {1})')", longitude, latitude);
      string select = string.Format(
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
        "      WHERE p.fk_ProjectTypeID = @projectType " +
        "        AND p.IsDeleted = 0 " +
        "        AND @timeOfPosition BETWEEN p.StartDate AND p.EndDate " +
        "        AND @timeOfPosition <= s.EndDate " +
        "        AND s.fk_ServiceTypeID = @serviceType " +
        "        AND cp.fk_CustomerUID = @customerUID " +
        "        AND st_Intersects({0}, PolygonST) = 1"
            , point);
     
      var projects = (await Connection.QueryAsync<Project>(select, new { customerUID, timeOfPosition = timeOfPosition.Date, projectType, serviceType }));
      
      PerhapsCloseConnection();
      return projects;
    }

    /// <summary>
    /// Gets any project which 
    ///      1) for this Customer
    ///      2) is active at the time
    ///      3) the lat/long is within,
    /// </summary>
    /// <param name="customerUID"></param>
    /// <param name="geometryWKT"></param>
    /// <param name="timeOfPosition"></param>
    /// <returns>The project</returns>
    public async Task<bool> DoesPolygonOverlap(string customerUID, string geometryWKT, DateTime startDate, DateTime endDate)
    {
      // todo does st_intersects detect inside/onpoint/online/overlap/etc?
      await PerhapsOpenConnection();

      string polygonToCheck = string.Format("ST_GeomFromText('{0}')", geometryWKT);
      string select = string.Format(
        "SELECT DISTINCT " +
        "        p.ProjectUID, p.Name, p.Description, p.LegacyProjectID, p.ProjectTimeZone, p.LandfillTimeZone, " +
        "        p.LastActionedUTC, p.IsDeleted, p.StartDate, p.EndDate, p.fk_ProjectTypeID as ProjectType, p.GeometryWKT, " +
        "        p.CoordinateSystemFileName, p.CoordinateSystemLastActionedUTC, " +
        "        cp.fk_CustomerUID AS CustomerUID, cp.LegacyCustomerID " +
        "      FROM Project p " +
        "        INNER JOIN CustomerProject cp ON cp.fk_ProjectUID = p.ProjectUID " +
        "      WHERE p.IsDeleted = 0 " +
        "        AND @startDate <= p.EndDate " +
         "       AND @endDate >= p.StartDate " +
        "        AND cp.fk_CustomerUID = @customerUID " +
        "        AND st_Intersects({0}, PolygonST) = 1"
            , polygonToCheck);

      var projects = (await Connection.QueryAsync<Project>(select, new { customerUID, startDate = startDate.Date, endDate = endDate.Date }));

      PerhapsCloseConnection();
      return projects.Count() > 0;
    }

    public async Task<IEnumerable<Project>> GetProjects_UnitTests()
    {
      await PerhapsOpenConnection();

      var projects = (await Connection.QueryAsync<Project>
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
          ));

      PerhapsCloseConnection();
      return projects;
    }
    #endregion gettersSpatial

  }
}