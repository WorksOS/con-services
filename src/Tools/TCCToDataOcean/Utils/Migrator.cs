using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Models;
using VSS.Common.Abstractions.Configuration;
using VSS.MasterData.Project.WebAPI.Common.Models;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.Productivity3D.Project.Abstractions.Models.ResultsHandling;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.Utils
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
    private const int THROTTLE_ASYNC_FILE_UPLOAD_JOBS = 5;

    private long _migrationInfoId = -1;

    private readonly IProjectRepository ProjectRepo;
    private readonly IFileRepository FileRepo;
    private readonly IImportFile ImportFile;
    private readonly ILogger Log;
    private readonly ILiteDbAgent _database;
    private readonly ICalibrationFileAgent DcFileAgent;
    private readonly IConfiguration _appSettings;

    private readonly bool _resumeMigration;
    private readonly bool _reProcessFailedProjects;
    private readonly bool _reProcessSkippedFiles;
    private readonly string _fileSpaceId;
    private readonly string _uploadFileApiUrl;
    private readonly string _importedFileApiUrl;
    private readonly string _tempFolder;
    private readonly int _capMigrationCount;
    private string[] _ignoredFiles;

    // Diagnostic settings
    private readonly bool _downloadProjectFiles;
    private readonly bool _uploadProjectFiles;
    private readonly bool _saveFailedProjects;

    private readonly List<ImportedFileType> MigrationFileTypes = new List<ImportedFileType>
    {
      ImportedFileType.Linework, // DXF
      ImportedFileType.Alignment // SVL
    };

    public Migrator(ILoggerFactory logger, IProjectRepository projectRepository, IConfiguration configuration, IConfigurationStore configStore,
                    ILiteDbAgent liteDbAgent, IFileRepository fileRepo, IImportFile importFile,
                    IEnvironmentHelper environmentHelper, ICalibrationFileAgent dcFileAgent)
    {
      Log = logger.CreateLogger<Migrator>();
      ProjectRepo = projectRepository;
      FileRepo = fileRepo;
      ImportFile = importFile;
      _database = liteDbAgent;
      DcFileAgent = dcFileAgent;

      _appSettings = configuration;

      _resumeMigration = configStore.GetValueBool("RESUME_MIGRATION", true);
      _reProcessFailedProjects = configStore.GetValueBool("REPROCESS_FAILED_PROJECTS", true);
      _reProcessSkippedFiles = configStore.GetValueBool("REPROCESS_SKIPPED_FILES", true);

      _fileSpaceId = environmentHelper.GetVariable("TCCFILESPACEID", 48);
      _uploadFileApiUrl = environmentHelper.GetVariable("IMPORTED_FILE_API_URL2", 1);
      _importedFileApiUrl = environmentHelper.GetVariable("IMPORTED_FILE_API_URL", 3);
      _capMigrationCount = configStore.GetValueInt("CAP_MIGRATION_COUNT", defaultValue: int.MaxValue);
      _tempFolder = Path.Combine(
        environmentHelper.GetVariable("TEMPORARY_FOLDER", 2),
        "DataOceanMigrationTmp",
        environmentHelper.GetVariable("MIGRATION_ENVIRONMENT", 2));

      // Diagnostic settings
      _downloadProjectFiles = configStore.GetValueBool("DOWNLOAD_PROJECT_FILES", defaultValue: false);
      _uploadProjectFiles = configStore.GetValueBool("UPLOAD_PROJECT_FILES", defaultValue: false);
      _saveFailedProjects = configStore.GetValueBool("SAVE_FAILED_PROJECT_IDS", defaultValue: true);
    }

    private long MigrationInfoId
    {
      get
      {
        if (_migrationInfoId < 0)
        {
          _migrationInfoId = _database.Find<MigrationInfo>(Table.MigrationInfo).Id;
        }

        return _migrationInfoId;
      }

      set { _migrationInfoId = value; }
    }

    public async Task MigrateFilesForAllActiveProjects()
    {
      Log.LogInformation($"{Method.Info()} Fetching projects...");
      var projects = (await ProjectRepo.GetActiveProjects()).ToList();
      Log.LogInformation($"{Method.Info()} Found {projects.Count} projects");

      var inputProjects = _appSettings.GetSection("Projects")
                                      .Get<string[]>();

      var ignoredProjects = _appSettings.GetSection("IgnoredProjects")
                                        .Get<string[]>();

      _ignoredFiles = _appSettings.GetSection("IgnoredFiles")
                                  .Get<string[]>();

      // Are we processing only a subset of projects from the appSettings::Projects array?
      if (inputProjects != null && inputProjects.Any())
      {
        Log.LogInformation($"{Method.Info()} Found {inputProjects.Length} input projects to process.");

        var tmpProjects = new List<Project>(inputProjects.Length);

        foreach (var projectUid in inputProjects)
        {
          var project = projects.Find(x => x.ProjectUID == projectUid);

          if (project != null)
          {
            Log.LogInformation($"{Method.Info()} Adding {project.ProjectUID}");
            tmpProjects.Add(project);
          }
        }

        if (!projects.Any())
        {
          Log.LogInformation($"{Method.Info()} Unable to resolve any projects to process, exiting.");
          return;
        }

        DropTables();

        projects = tmpProjects;
      }
      else
      {
        if (!_resumeMigration)
        {
          DropTables();

          if (Directory.Exists(_tempFolder))
          {
            Log.LogDebug($"{Method.Info()} Removing temporary files from {_tempFolder}");
            Directory.Delete(_tempFolder, recursive: true);
          }
        }
      }

      if (!_resumeMigration)
      {
        MigrationInfoId = _database.Insert(new MigrationInfo());
      }

      var projectCount = Math.Min(projects.Count, _capMigrationCount);
      var projectTasks = new List<Task<bool>>(projectCount);

      _database.Update(MigrationInfoId, (MigrationInfo x) => x.ProjectsTotal = projectCount);

      var projectsProcessed = 0;
      var processedProjects = new List<string>();

      foreach (var project in projects)
      {
        if (ignoredProjects != null && ignoredProjects.Contains(project.ProjectUID))
        {
          Log.LogInformation($"{Method.Info()} Ignoring project {project.ProjectUID}; found in IgnoredProjects list.");
          continue;
        }

        var projectRecord = _database.Find<MigrationProject>(Table.Projects, project.LegacyProjectID);

        if (projectRecord == null)
        {
          Log.LogInformation($"{Method.Info()} Creating new migration record for project {project.ProjectUID}");
          _database.Insert(new MigrationProject(project));
        }
        else
        {
          Log.LogInformation($"{Method.Info()} Found migration record for project {project.ProjectUID}");

          // TODO Check completed=true & eligibleFiles > 0 && uploadedFiles=0; should retry.

          if (projectRecord.MigrationState == MigrationState.Completed && !_reProcessSkippedFiles)
          {
            Log.LogInformation($"{Method.Info()} Skipping project {project.ProjectUID}, marked as COMPLETED");
            continue;
          }

          if (projectRecord.MigrationState != MigrationState.Completed)
          {
            if (!_reProcessFailedProjects)
            {
              Log.LogInformation($"{Method.Info()} Not reprocessing {Enum.GetName(typeof(MigrationState), projectRecord.MigrationState)?.ToUpper()} project {project.ProjectUID}");
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

        if (projectsProcessed <= _capMigrationCount)
        {
          processedProjects.Add(job.Project.ProjectUID);
          projectTasks.Add(MigrateProject(job));
        }

        if (projectTasks.Count <= THROTTLE_ASYNC_PROJECT_JOBS && projectsProcessed < _capMigrationCount - 1) { continue; }

        var completed = await Task.WhenAny(projectTasks);
        projectTasks.Remove(completed);

        _database.IncrementProjectMigrationCounter(project);
        projectsProcessed += 1;

        Log.LogInformation("Migration Progress:");
        Log.LogInformation($"  Processed: {projectsProcessed}");
        Log.LogInformation($"  In Flight: {projectTasks.Count}");
        Log.LogInformation($"  Remaining: {projectCount - projectsProcessed}");

        if (projectsProcessed >= _capMigrationCount)
        {
          Log.LogInformation($"{Method.Info()} Reached maxium number of projects to process, exiting.");
          break;
        }
      }

      await Task.WhenAll(projectTasks);

      // DIAGNOSTIC RUNTIME SWITCH
      if (_saveFailedProjects)
      {
        // Create a recovery file of project uids for re processing
        var failedProjectsLog = Path.Combine(_tempFolder, $"FailedProjects{DateTime.Now.Date.ToShortDateString().Replace('/', '-')}_{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.log");

        var completedProjectsLog = Path.Combine(_tempFolder, $"Completed{DateTime.Now.Date.ToShortDateString().Replace('/', '-')}_{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.log");

        if (!Directory.Exists(_tempFolder)) { Directory.CreateDirectory(_tempFolder); }

        var allProjects = _database.GetTable<MigrationProject>(Table.Projects).ToList();

        using (TextWriter streamWriterFailed = new StreamWriter(failedProjectsLog))
        using (TextWriter streamWriterCompleted = new StreamWriter(completedProjectsLog))
        {
          foreach (var project in processedProjects)
          {
            var migrationProject = allProjects.FirstOrDefault(x => x.ProjectUid == project);

            if (migrationProject == null) { continue; }

            if (migrationProject.MigrationState == MigrationState.Completed)
            {
              streamWriterCompleted.WriteLine($"{migrationProject.ProjectUid}");
              continue;
            }

            var message = string.IsNullOrEmpty(migrationProject.MigrationStateMessage)
              ? null
              : $" // {migrationProject.MigrationStateMessage}";

            streamWriterFailed.WriteLine($"{migrationProject.ProjectUid}{message}");
          }
        }
      }

      // Set the final summary figures.
      var completedCount = _database.Find<MigrationProject>(Table.Projects, x => x.MigrationState == MigrationState.Completed)
                                    .Count();

      _database.Update(MigrationInfoId, (MigrationInfo x) => x.ProjectsSuccessful = completedCount);

      var failedCount = _database.Find<MigrationProject>(Table.Projects, x => x.MigrationState == MigrationState.Failed)
                                 .Count();

      _database.Update(MigrationInfoId, (MigrationInfo x) => x.ProjectsFailed = failedCount);

      Log.LogInformation("Migration processing completed.");
    }

    private void DropTables()
    {
      if (_resumeMigration) { return; }

      Log.LogInformation($"{Method.Info()} Cleaning database, dropping collections");

      _database.DropTables(new[]
      {
        Table.MigrationInfo,
        Table.Projects,
        Table.Files,
        Table.CoordinateSystemInfo,
        Table.Errors,
        Table.Warnings,
        Table.Messages
      });
    }

    /// <summary>
    /// Migrate all elgible files for a given project.
    /// </summary>
    private async Task<bool> MigrateProject(MigrationJob job)
    {
      var migrationResult = MigrationState.Unknown;
      var migrationStateMessage = "";

      try
      {
        Log.LogInformation($"{Method.In()} Migrating project {job.Project.ProjectUID}, Name: '{job.Project.Name}'");
        _database.SetMigrationState(job, MigrationState.InProgress, null);

        // Resolve coordinate system file first; all projects regardless of whether they have files need to have 
        // their calibration file present in DataOcean post migration.
        var result = await DcFileAgent.ResolveProjectCoordinateSystemFile(job);

        if (!result)
        {
          migrationResult = MigrationState.Failed;
          migrationStateMessage = "Unable to resolve coordinate system file";

          Log.LogError($"{Method.Info()} {migrationStateMessage} for project {job.Project.ProjectUID}, aborting project migration");

          return false;
        }

        // Resolve imported files for current project.
        var filesResult = await ImportFile.GetImportedFilesFromWebApi($"{_importedFileApiUrl}?projectUid={job.Project.ProjectUID}", job.Project);

        if (filesResult == null)
        {
          Log.LogInformation($"{Method.Info()} Failed to fetch imported files for project {job.Project.ProjectUID}, aborting project migration");
          _database.SetMigrationState(job, MigrationState.Failed, "Failed to fetch imported file list");

          migrationStateMessage = "Failed to fetch imported file list";
          return false;
        }

        if (filesResult.ImportedFileDescriptors == null || filesResult.ImportedFileDescriptors.Count == 0)
        {
          Log.LogInformation($"{Method.Info()} Project {job.Project.ProjectUID} contains no imported files, aborting project migration");

          _database.SetMigrationState(job, MigrationState.Skipped, "No imported files");
          _database.Update(MigrationInfoId, (MigrationInfo x) => x.ProjectsWithNoFiles += 1);

          migrationStateMessage = "Project contains no imported files";
          migrationResult = MigrationState.Completed;
        }
        else
        {
          // We have files, and a valid coordinate system, continue processing.
          var selectedFiles = filesResult.ImportedFileDescriptors
                                         .Where(f => MigrationFileTypes.Contains(f.ImportedFileType))
                                         .ToList();

          _database.Update(job.Project.LegacyProjectID, (MigrationProject x) =>
          {
            x.TotalFileCount = filesResult.ImportedFileDescriptors.Count;
            x.EligibleFileCount = selectedFiles.Count;
          }, Table.Projects);

          Log.LogInformation($"{Method.Info()} Found {selectedFiles.Count} eligible files out of {filesResult.ImportedFileDescriptors.Count} total to migrate for {job.Project.ProjectUID}");

          if (selectedFiles.Count == 0)
          {
            Log.LogInformation($"{Method.Info()} Project {job.Project.ProjectUID} contains no eligible files, skipping project migration");

            _database.Update(MigrationInfoId, (MigrationInfo x) => x.ProjectsWithNoEligibleFiles += 1);

            migrationStateMessage = "Project contains no eligible files";
            migrationResult = MigrationState.Completed;
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

            var migrationFile = _database.Find<MigrationFile>(Table.Files, file.LegacyFileId);

            if (migrationFile == null)
            {
              _database.Insert(new MigrationFile(file), Table.Files);
            }
            else
            {
              if (migrationFile.MigrationState == MigrationState.Completed ||
                  migrationFile.MigrationState == MigrationState.Skipped && !_reProcessSkippedFiles)
              {
                Log.LogInformation($"{Method.Info()} Skipping file {file.ImportedFileUid}, migrationState={Enum.GetName(typeof(MigrationState), migrationFile.MigrationState)?.ToUpper()} and REPROCESS_SKIPPED_FILES={_reProcessSkippedFiles}");

                continue;
              }
            }
            
            var migrationResultObj = MigrateFile(file, job.Project);

            fileTasks.Add(migrationResultObj);

            if (fileTasks.Count != THROTTLE_ASYNC_FILE_UPLOAD_JOBS) { continue; }

            var completed = await Task.WhenAny(fileTasks);
            fileTasks.Remove(completed);
          }

          await Task.WhenAll(fileTasks);

          var importedFilesResult = fileTasks.All(t => t.Result.Item1);

          migrationResult = importedFilesResult ? MigrationState.Completed : MigrationState.Failed;
          if (!_uploadProjectFiles) { migrationResult = MigrationState.Unknown; }

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
        _database.SetMigrationState(job, migrationResult, migrationStateMessage);

        Action<MigrationInfo> migrationResultAction;

        switch (migrationResult)
        {
          case MigrationState.Skipped: migrationResultAction = x => x.ProjectsSkipped += 1; break;
          case MigrationState.Completed: migrationResultAction = x => x.ProjectsCompleted += 1; break;
          case MigrationState.Failed: migrationResultAction = x => x.ProjectsFailed += 1; break;
          default:
            if (!_uploadProjectFiles)
            {
              migrationResultAction = x => x.ProjectsSkipped += 1;
              break;
            }

            throw new Exception($"Invalid migrationResult state for project {job.Project.ProjectUID}");
        }

        _database.Update(MigrationInfoId, migrationResultAction);
      }

      return false;
    }

    /// <summary>
    /// Downloads the file from TCC and if successful uploads it through the Project service.
    /// </summary>
    private async Task<(bool success, FileDataSingleResult file)> MigrateFile(ImportedFileDescriptor file, Project project)
    {
      if (_ignoredFiles != null && _ignoredFiles.Contains(file.ImportedFileUid))
      {
        Log.LogWarning($"{Method.Info()} Migrating file '{file.Name}', Uid: {file.ImportedFileUid} aborted, found in exclusion list.");

        return (success: false, file: null);
      }

      Log.LogInformation($"{Method.In()} Migrating file '{file.Name}', Uid: {file.ImportedFileUid}");

      string tempFileName;

      using (var fileContents = await FileRepo.GetFile(_fileSpaceId, $"{file.Path}/{file.Name}"))
      {
        _database.Update(file.LegacyFileId, (MigrationFile x) => x.MigrationState = MigrationState.InProgress, Table.Files);

        if (fileContents == null)
        {
          var message = $"Failed to fetch file '{file.Name}' ({file.LegacyFileId}), not found";
          _database.Update(file.LegacyFileId, (MigrationFile x) => x.MigrationState = MigrationState.FileNotFound, Table.Files);
          _database.Insert(new MigrationMessage(file.ProjectUid, message));

          Log.LogWarning($"{Method.Out()} {message}");

          return (success: true, file: null);
        }

        var tempPath = Path.Combine(_tempFolder, file.CustomerUid, file.ProjectUid, file.ImportedFileUid);
        Directory.CreateDirectory(tempPath);

        tempFileName = Path.Combine(tempPath, file.Name);

        Log.LogInformation($"{Method.Info()} Creating temporary file '{tempFileName}' for file {file.ImportedFileUid}");

        if (_downloadProjectFiles)
        {
          using (var tempFile = new FileStream(tempFileName, FileMode.Create))
          {
            fileContents.CopyTo(tempFile);

            _database.Update(
              file.LegacyFileId, (MigrationFile x) =>
              {
                // ReSharper disable once AccessToDisposedClosure
                x.Length = tempFile.Length;
              },
              tableName: Table.Files);
          }
        }
      }

      var result = new FileDataSingleResult();

      if (_downloadProjectFiles && _uploadProjectFiles)
      {
        Log.LogInformation($"{Method.Info()} Uploading file {file.ImportedFileUid}");

        result = ImportFile.SendRequestToFileImportV4(
          _uploadFileApiUrl,
          file,
          tempFileName,
          new ImportOptions(HttpMethod.Put));

        _database.Update(file.LegacyFileId, (MigrationFile x) => x.MigrationState = MigrationState.Completed, Table.Files);
        _database.Update(project.LegacyProjectID, (MigrationProject x) => x.UploadedFileCount += 1, Table.Projects);
      }
      else
      {
        var skippedMessage = $"Skipped because DOWNLOAD_PROJECT_FILES={_downloadProjectFiles} && UPLOAD_PROJECT_FILES={_uploadProjectFiles}";

        _database.Update(file.LegacyFileId, (MigrationFile x) =>
        {
          x.MigrationState = MigrationState.Skipped;
          x.MigrationStateMessage = skippedMessage;
        }, Table.Files);

        Log.LogDebug($"{Method.Info("DEBUG")} {skippedMessage}");
      }

      Log.LogInformation($"{Method.Out()} File {file.ImportedFileUid} update result {result.Code} {result.Message}");

      return (success: true, file: result);
    }
  }
}
