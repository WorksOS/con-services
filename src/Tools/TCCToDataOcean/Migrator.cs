using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Models;
using TCCToDataOcean.Utils;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean
{
  public class Migrator : IMigrator
  {
    /// <summary>
    /// Throttle the async jobs or we could attempt to run thousands of project migrations concurrently
    /// which results in file stream errors in production. (TCC becomes overwhelmed by the file upload
    /// queue). This should be mitigated by setting UPLOAD_TO_TCC=false.
    /// </summary>
    private const int THROTTLE_ASYNC_PROJECT_JOBS = 5;

    /// <summary>
    /// Throttle the uploading of files per project. Generally set to 1 for development testing.
    /// </summary>
    const int THROTTLE_ASYNC_FILE_UPLOAD_JOBS = 5;

    private readonly IProjectRepository ProjectRepo;
    private readonly IFileRepository FileRepo;
    private readonly IImportFile ImportFile;
    private readonly ILogger Log;
    private readonly ILiteDbAgent Database;
    private readonly ICalibrationFileAgent DcFileAgent;

    private readonly bool ResumeMigration;
    private readonly bool ReProcessFailedProjects;
    private readonly string FileSpaceId;
    private readonly string ProjectApiUrl;
    private readonly string UploadFileApiUrl;
    private readonly string ImportedFileApiUrl;
    private readonly string TemporaryFolder;

    // Diagnostic settings
    private readonly bool _downloadProjectFiles;
    private readonly bool _uploadProjectFiles;
    private readonly bool _saveFailedProjects;
    private readonly bool _uploadToTCC;

    private readonly List<ImportedFileType> MigrationFileTypes = new List<ImportedFileType>
    {
      ImportedFileType.Linework, // DXF
      //ImportedFileType.DesignSurface,
      //ImportedFileType.SurveyedSurface,
      ImportedFileType.Alignment // SVL
    };

    public Migrator(ILoggerFactory logger, IProjectRepository projectRepository, IConfigurationStore configStore,
                    ILiteDbAgent liteDbAgent, IFileRepository fileRepo, IImportFile importFile,
                    IEnvironmentHelper environmentHelper, ICalibrationFileAgent dcFileAgent)
    {
      Log = logger.CreateLogger<Migrator>();
      ProjectRepo = projectRepository;
      FileRepo = fileRepo;
      ImportFile = importFile;
      Database = liteDbAgent;
      DcFileAgent = dcFileAgent;

      ResumeMigration = configStore.GetValueBool("RESUME_MIGRATION", true);
      ReProcessFailedProjects = configStore.GetValueBool("REPROCESS_FAILED_PROJECTS", true);

      FileSpaceId = environmentHelper.GetVariable("TCCFILESPACEID", 48);
      ProjectApiUrl = environmentHelper.GetVariable("PROJECT_API_URL", 1);
      UploadFileApiUrl = environmentHelper.GetVariable("IMPORTED_FILE_API_URL2", 1);
      ImportedFileApiUrl = environmentHelper.GetVariable("IMPORTED_FILE_API_URL", 3);
      TemporaryFolder = Path.Combine(environmentHelper.GetVariable("TEMPORARY_FOLDER", 2), "DataOceanMigrationTmp");

      // Diagnostic settings
      _downloadProjectFiles = configStore.GetValueBool("DOWNLOAD_PROJECT_FILES", defaultValue: false);
      _uploadProjectFiles = configStore.GetValueBool("UPLOAD_PROJECT_FILES", defaultValue: false);
      _saveFailedProjects = configStore.GetValueBool("SAVE_FAILED_PROJECT_IDS", defaultValue: true);
      _uploadToTCC = configStore.GetValueBool("UPLOAD_TO_TCC", defaultValue: false);
    }

    public async Task MigrateFilesForAllActiveProjects()
    {
      var recoveryFile = Path.Combine(TemporaryFolder, "MigrationRecovery.log");

      Log.LogInformation($"{Method.Info()} Fetching projects from: '{ProjectApiUrl}'");
      var projects = (await ProjectRepo.GetActiveProjects()).ToList();
      Log.LogInformation($"{Method.Info()} Found {projects.Count} projects");

      if (File.Exists(recoveryFile))
      {
        Log.LogInformation($"{Method.Info()} Fetching projects from: '{recoveryFile}'");
        var fileContents = File.ReadAllLines(recoveryFile, Encoding.UTF8).ToList();
        Log.LogInformation($"{Method.Info()} Found {fileContents.Count} projects to reprocess");

        var tmpProjects = new List<Project>();

        foreach (var project in projects)
        {
          if (fileContents.Contains(project.ProjectUID)) tmpProjects.Add(project);
        }

        DropTables();

        projects = tmpProjects;
      }
      else
      {
        if (!ResumeMigration)
        {
          DropTables();

          if (Directory.Exists(TemporaryFolder))
          {
            Log.LogDebug($"{Method.Info()} Removing temporary files from {TemporaryFolder}");
            Directory.Delete(TemporaryFolder, recursive: true);
          }
        }
      }

      if (!ResumeMigration)
      {
        Database.InitDatabase();
      }

      var projectTasks = new List<Task<bool>>(projects.Count);
      Database.SetMigationInfo_SetProjectCount(projects.Count);

      // Sort projects by most recently updated, so we process the (likely) most utilized first,
      // in-case of a race condition during production migration with projects receiving new files
      // while the migration is in flight.
      projects = projects.OrderBy(x => x.LastActionedUTC)
                         .Reverse()
                         .ToList();

      var projectCount = projects.Count;
      var projectsProcessed = 0;

      foreach (var project in projects)
      {
        var projectRecord = Database.GetRecord<MigrationProject>(Table.Projects, project.LegacyProjectID);

        if (projectRecord == null)
        {
          Log.LogInformation($"{Method.Info()} Creating new migration record for project {project.ProjectUID}");
          Database.WriteRecord(Table.Projects, project);
        }
        else
        {
          Log.LogInformation($"{Method.Info()} Found migration record for project {project.ProjectUID}");

          if (projectRecord.MigrationState == MigrationState.Completed)
          {
            Log.LogInformation($"{Method.Info()} Skipping project {project.ProjectUID}, marked as COMPLETED");
            continue;
          }

          if (projectRecord.MigrationState == MigrationState.Failed)
          {
            if (!ReProcessFailedProjects)
            {
              Log.LogInformation($"{Method.Info()} Not reprocessing FAILED project {project.ProjectUID}");
              continue;
            }
          }

          Log.LogInformation($"{Method.Info()} Resuming migration for project {project.ProjectUID}, marked as {Enum.GetName(typeof(MigrationState), projectRecord.MigrationState)?.ToUpper()}");
        }

        var job = new MigrationJob
        {
          Project = project,
          IsRetryAttempt = projectRecord != null
        };

        projectTasks.Add(MigrateProject(job));

        if (projectTasks.Count != THROTTLE_ASYNC_PROJECT_JOBS) continue;

        var completed = await Task.WhenAny(projectTasks);
        projectTasks.Remove(completed);

        Database.IncrementProjectMigrationCounter(project);
        projectsProcessed += 1;

        Log.LogInformation("Migration Progress:");
        Log.LogInformation($"  Processed: {projectsProcessed}");
        Log.LogInformation($"  In Flight: {projectTasks.Count}");
        Log.LogInformation($"  Remaining: {projectCount - projectsProcessed}");
      }

      await Task.WhenAll(projectTasks);

      // DIAGNOSTIC RUNTIME SWITCH
      if (_saveFailedProjects)
      {
        // Create a recovery file of project uids for re processing
        var processedProjects = Database.GetTable<MigrationProject>(Table.Projects);
        var logFilename = Path.Combine(TemporaryFolder, $"MigrationRecovery_{DateTime.Now.Date.ToShortDateString().Replace('/', '-')}_{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.log");

        if (!Directory.Exists(TemporaryFolder)) Directory.CreateDirectory(TemporaryFolder);

        using (TextWriter tw = new StreamWriter(logFilename))
        {
          foreach (var project in processedProjects)
          {
            if (project.MigrationState != MigrationState.Completed)
            {
              var message = string.IsNullOrEmpty(project.MigrationStateMessage)
                ? null
                : $" // {project.MigrationStateMessage}";

              tw.WriteLine($"{project.ProjectUid}{message}");
            }
          }

          tw.Close();
        }
      }
    }

    private void DropTables()
    {
      if (ResumeMigration) { return; }

      Log.LogInformation($"{Method.Info()} Cleaning database, dropping collections");

      Database.DropTables(new[]
      {
        Table.MigrationInfo,
        Table.Projects,
        Table.Files,
        Table.Errors,
        Table.Warnings
      });
    }

    /// <summary>
    /// Migrate all elgible files for a given project.
    /// </summary>
    private async Task<bool> MigrateProject(MigrationJob job)
    {
      var migrationResult = false;
      var migrationStateMessage = "";

      try
      {
        Log.LogInformation($"{Method.In()} Migrating project {job.Project.ProjectUID}, Name: '{job.Project.Name}'");
        Database.SetMigrationState(Table.Projects, job, MigrationState.InProgress, null);

        // Resolve imported files for current project.
        var filesResult = await ImportFile.GetImportedFilesFromWebApi($"{ImportedFileApiUrl}?projectUid={job.Project.ProjectUID}", job.Project);

        if (filesResult == null)
        {
          Log.LogInformation($"{Method.Info()} Failed to fetch imported files for project {job.Project.ProjectUID}, aborting project migration");
          Database.SetMigrationState(Table.Projects, job, MigrationState.Failed, "Failed to fetch imported file list");

          migrationStateMessage = "Failed to fetch imported file list";
          return false;
        }

        if (filesResult.ImportedFileDescriptors == null || filesResult.ImportedFileDescriptors.Count == 0)
        {
          Log.LogInformation($"{Method.Info()} Project {job.Project.ProjectUID} contains no imported files, aborting project migration");
          Database.SetMigrationState(Table.Projects, job, MigrationState.Failed, "No imported files");

          migrationStateMessage = "Project contains no imported files";
          migrationResult = true;
        }
        else
        {
          // Resolve coordinate system file for the project.
          var result = await DcFileAgent.ResolveProjectCoordinateSystemFile(job);
          if (!result)
          {
            migrationStateMessage = "Unable to resolve coordinate system file";
            return false;
          }

          // We have files, and a valid coordinate system, continue processing.
          var selectedFiles = filesResult.ImportedFileDescriptors
                                         .Where(f => MigrationFileTypes.Contains(f.ImportedFileType))
                                         .ToList();
          
          Database.SetProjectFilesDetails(Table.Projects, job.Project, filesResult.ImportedFileDescriptors.Count, selectedFiles.Count);

          Log.LogInformation($"{Method.Info()} Found {selectedFiles.Count} eligible files out of {filesResult.ImportedFileDescriptors.Count} total to migrate for {job.Project.ProjectUID}");

          if (selectedFiles.Count == 0)
          {
            Log.LogInformation($"{Method.Info()} Project {job.Project.ProjectUID} contains no eligible files, skipping project migration");

            migrationStateMessage = "Project contains no eligible files";
            migrationResult = true;
          }

          var fileTasks = new List<Task<(bool, FileDataSingleResult)>>();

          foreach (var file in selectedFiles)
          {
            // Check to sort out bad data; found in development database.
            if (file.CustomerUid != job.Project.CustomerUID)
            {
              Log.LogError($"{Method.Info("ERROR")} CustomerUid ({file.CustomerUid}) for ImportedFile ({file.ImportedFileUid}) doesn't match associated project: {job.Project.ProjectUID}");
              continue;
            }

            if (!job.IsRetryAttempt)
            {
              Database.WriteRecord(Table.Files, file);
            }

            var migrationResultObj = MigrateFile(file, job.Project);

            fileTasks.Add(migrationResultObj);

            if (fileTasks.Count != THROTTLE_ASYNC_FILE_UPLOAD_JOBS) { continue; }

            var completed = await Task.WhenAny(fileTasks);
            fileTasks.Remove(completed);
          }

          await Task.WhenAll(fileTasks);

          var importedFilesResult = fileTasks.All(t => t.Result.Item1);

          migrationResult = importedFilesResult;

          Log.LogInformation($"{Method.Out()} Project '{job.Project.Name}' ({job.Project.ProjectUID}) {(importedFilesResult ? "succeeded" : "failed")}");

          migrationStateMessage = importedFilesResult ? "Success" : "failed";
          return importedFilesResult;
        }
      }
      catch (Exception exception)
      {
        Log.LogError(exception, $"{Method.Info()} Error processing project {job.Project.ProjectUID}");
      }
      finally
      {
        Database.SetMigrationState(Table.Projects, job, migrationResult ? MigrationState.Completed : MigrationState.Failed, migrationStateMessage);
        Database.SetMigationInfo_IncrementProjectsProcessed();
      }

      return false;
    }

    /// <summary>
    /// Downloads the file from TCC and if successful uploads it through the Project service.
    /// </summary>
    private async Task<(bool success, FileDataSingleResult file)> MigrateFile(ImportedFileDescriptor file, Project project)
    {
      Log.LogInformation($"{Method.In()} Migrating file '{file.Name}', Uid: {file.ImportedFileUid}");

      string tempFileName;

      using (var fileContents = await FileRepo.GetFile(FileSpaceId, $"{file.Path}/{file.Name}"))
      {
        Database.SetMigrationState(Table.Files, file, MigrationState.InProgress);

        if (fileContents == null)
        {
          var message = $"Failed to fetch file '{file.Name}' ({file.LegacyFileId}), not found";
          Database.SetMigrationState(Table.Files, file, MigrationState.FileNotFound);
          Database.WriteWarning(file.ProjectUid, message);

          Log.LogWarning($"{Method.Out()} {message}");

          return (success: true, file: null);
        }

        var tempPath = Path.Combine(TemporaryFolder, file.CustomerUid, file.ProjectUid, file.ImportedFileUid);
        Directory.CreateDirectory(tempPath);

        tempFileName = Path.Combine(tempPath, file.Name);

        Log.LogInformation($"{Method.Info()} Creating temporary file '{tempFileName}' for file {file.ImportedFileUid}");

        // DIAGNOSTIC RUNTIME SWITCH
        if (_downloadProjectFiles)
        {
          using (var tempFile = new FileStream(tempFileName, FileMode.Create))
          {
            fileContents.CopyTo(tempFile);

            Database.SetFileSize(Table.Files, file, tempFile.Length);
          }
        }
      }

      var result = new FileDataSingleResult();

      // DIAGNOSTIC RUNTIME SWITCH
      if (_downloadProjectFiles && _uploadProjectFiles)
      {
        Log.LogInformation($"{Method.Info()} Uploading file {file.ImportedFileUid}");

        result = ImportFile.SendRequestToFileImportV4(
          UploadFileApiUrl,
          file,
          tempFileName,
          new ImportOptions(HttpMethod.Put),
          _uploadToTCC);

        Database.SetMigrationState(Table.Files, file, MigrationState.Completed);
        Database.IncrementProjectFilesUploaded(project);
      }
      else
      {
        Database.SetMigrationState(Table.Files, file, MigrationState.Skipped);
        Log.LogDebug($"{Method.Info("DEBUG")} Skipped uploading file {file.ImportedFileUid}");
      }

      Log.LogInformation($"{Method.Out()} File {file.ImportedFileUid} update result {result.Code} {result.Message}");

      return (success: true, file: result);
    }
  }
}
