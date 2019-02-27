using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Models;
using TCCToDataOcean.Types;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Repositories;
using VSS.MasterData.Repositories.DBModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean
{
  public class Migrator : IMigrator
  {
    private readonly IProjectRepository ProjectRepo;
    private readonly IConfigurationStore ConfigStore;
    private readonly IServiceExceptionHandler ServiceExceptionHandler;
    private readonly IFileRepository FileRepo;
    private readonly IWebApiUtils WebApiUtils;
    private readonly IImportFile ImportFile;
    private readonly ILogger Log;
    private readonly ILiteDbAgent _migrationDb;
    private readonly string FileSpaceId;
    private readonly string ProjectApiUrl;
    private readonly string UploadFileApiUrl;
    private readonly string ImportedFileApiUrl;
    private readonly string TemporaryFolder;

    private readonly bool _resumeModeEnabled;

    // Diagnostic settings
    private readonly bool _isDebugMode;
    private readonly bool _updateProjectCoordinateSystemFile;

    private readonly List<ImportedFileType> MigrationFileTypes = new List<ImportedFileType>
    {
      ImportedFileType.Linework,
      ImportedFileType.DesignSurface,
      ImportedFileType.SurveyedSurface,
      ImportedFileType.Alignment
    };

    public Migrator(ILoggerFactory logger, IProjectRepository projectRepository, IConfigurationStore configStore, ILiteDbAgent liteDbAgent,
      IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo, IWebApiUtils webApiUtils, IImportFile importFile)
    {
      Log = logger.CreateLogger<Migrator>();
      ProjectRepo = projectRepository;
      ConfigStore = configStore;
      FileRepo = fileRepo;
      ServiceExceptionHandler = serviceExceptionHandler;
      WebApiUtils = webApiUtils;
      ImportFile = importFile;
      _migrationDb = liteDbAgent;

      FileSpaceId = GetEnvironmentVariable("TCCFILESPACEID", 48);
      ProjectApiUrl = GetEnvironmentVariable("PROJECT_API_URL", 1);
      UploadFileApiUrl = GetEnvironmentVariable("IMPORTED_FILE_API_URL2", 1);
      ImportedFileApiUrl = GetEnvironmentVariable("IMPORTED_FILE_API_URL", 3);
      TemporaryFolder = GetEnvironmentVariable("TEMPORARY_FOLDER", 2);

      _resumeModeEnabled = ConfigStore.GetValueBool("RESUME_MODE_ENABLED", defaultValue: false);
      // Diagnostic settings
      _isDebugMode = ConfigStore.GetValueBool("MIGRATION_MODE_IS_DEBUG", defaultValue: true);
      _updateProjectCoordinateSystemFile = ConfigStore.GetValueBool("UPDATE_PROJECT_COORDINATE_SYSTEM_FILE", defaultValue: false);
    }

    public async Task<bool> MigrateFilesForAllActiveProjects()
    {
      Log.LogInformation($"## In ## {nameof(MigrateFilesForAllActiveProjects)}");

      // TODO (Aaron) Remove if part of recovery phase.
      Log.LogInformation($"{nameof(MigrateFilesForAllActiveProjects)} | Cleaning database, dropping collections");
      _migrationDb.DropTables(new[]
      {
          Table.Projects,
          Table.Files,
          Table.Errors
      });

      var workingTmpDir = Path.Combine(TemporaryFolder, "DataOceanMigrationTmp");
      if (Directory.Exists(workingTmpDir))
      {
        Directory.Delete(workingTmpDir, recursive: true);
      }
      // TODO (Aaron) Tidy up complete.

      Log.LogInformation($"{nameof(MigrateFilesForAllActiveProjects)} | Fetching projects from: '{ProjectApiUrl}'");
      var projects = (await ProjectRepo.GetActiveProjects()).ToList();
      Log.LogInformation($"{nameof(MigrateFilesForAllActiveProjects)} | Found {projects.Count} projects");

      var projectTasks = new List<Task<bool>>(projects.Count);

      foreach (var project in projects)
      {
        _migrationDb.WriteRecord(Table.Projects, project);

        projectTasks.Add(MigrateProject(project));
      }

      await Task.WhenAll(projectTasks);

      var result = projectTasks.All(t => t.Result);

      Log.LogInformation($"## Out ## {nameof(MigrateFilesForAllActiveProjects)} | Migration {(result ? "succeeded" : "failed")}");

      return result;
    }

    /// <summary>
    /// Migrate all elgible files for a given project.
    /// </summary>
    private async Task<bool> MigrateProject(Project project)
    {
      Log.LogInformation($"## In ## {nameof(MigrateProject)} | Migrating project {project.ProjectUID}, Name: '{project.Name}'");
      _migrationDb.SetMigrationState(Table.Projects, project, MigrationState.InProgress);

      var coordinateSystemFileMigrationResult = false;

      if (!string.IsNullOrEmpty(project.CoordinateSystemFileName))
      {
        var coordSystemFileContent = await DownloadCoordinateSystemFileFromTCC(project);

        if (coordSystemFileContent != null && coordSystemFileContent.Length > 0)
        {
          if (_updateProjectCoordinateSystemFile)
          {
            var updateProjectResult = WebApiUtils.UpdateProjectCoordinateSystemFile(ProjectApiUrl, project, coordSystemFileContent);

            coordinateSystemFileMigrationResult = updateProjectResult.Code == (int)ExecutionResult.Success;

            Log.LogInformation($"{nameof(MigrateProject)} | Update result {updateProjectResult.Message} ({updateProjectResult.Code})");
          }
          else
          {
            Log.LogDebug($"## DEBUG MODE ## {nameof(MigrateProject)} | Skipping updating project coordinate system file step");
          }
        }
        else
        {
          _migrationDb.SetProjectCoordinateSystemDetails(Table.Projects, project, false);
        }
      }

      var importedFilesResult = false;
      var filesResult = ImportFile.GetImportedFilesFromWebApi($"{ImportedFileApiUrl}?projectUid={project.ProjectUID}", project);

      if (filesResult.ImportedFileDescriptors?.Count > 0)
      {
        var selectedFiles = filesResult.ImportedFileDescriptors.Where(f => MigrationFileTypes.Contains(f.ImportedFileType)).ToList();
        _migrationDb.SetProjectFilesDetails(Table.Projects, project, filesResult.ImportedFileDescriptors.Count, selectedFiles.Count);
        
        Log.LogInformation($"{nameof(MigrateProject)} | Found {selectedFiles.Count} out of {filesResult.ImportedFileDescriptors.Count} files to migrate for {project.ProjectUID}");

        var fileTasks = new List<Task<(bool, FileDataSingleResult)>>();

        foreach (var file in selectedFiles)
        {
          _migrationDb.WriteRecord(Table.Files, file);
          var migrationResult = MigrateFile(file);

          fileTasks.Add(migrationResult);
        }

        await Task.WhenAll(fileTasks);

        importedFilesResult = fileTasks.All(t => t.Result.Item1);
      }
      else
      {
        Log.LogInformation($"{nameof(MigrateProject)} | No files found for {project.ProjectUID}");
      }

      var result = (coordinateSystemFileMigrationResult && importedFilesResult) || filesResult.ImportedFileDescriptors?.Count == 0;

      _migrationDb.SetMigrationState(Table.Projects, project, result ? MigrationState.Completed : MigrationState.Failed);

      Log.LogInformation($"## Out ## {nameof(MigrateProject)} | Migration {(result ? "succeeded" : "failed")}");

      return result;
    }

    /// <summary>
    /// Downloads the coordinate system file for a given project.
    /// </summary>
    private async Task<byte[]> DownloadCoordinateSystemFileFromTCC(Project project)
    {
      Log.LogInformation($"## In ## {nameof(DownloadCoordinateSystemFileFromTCC)} | Downloading coord system file '{project.CoordinateSystemFileName}'");

      Stream memStream = null;
      byte[] coordSystemFileContent;
      int numBytesRead;

      try
      {
        memStream = await FileRepo.GetFile(FileSpaceId, $"/{project.CustomerUID}/{project.ProjectUID}/{project.CoordinateSystemFileName}");
        if (memStream != null && memStream.CanRead && memStream.Length > 0)
        {
          coordSystemFileContent = new byte[memStream.Length];
          int numBytesToRead = (int)memStream.Length;
          numBytesRead = memStream.Read(coordSystemFileContent, 0, numBytesToRead);
        }
        else
        {
          _migrationDb.WriteError(project.ProjectUID, $"Failed to fetch coordinate system file '{project.CustomerUID}/{project.ProjectUID}/{project.CoordinateSystemFileName}'. isAbleToRead: {memStream != null && memStream.CanRead}, bytesReturned: {memStream?.Length ?? 0}");

          return null;
        }
      }
      finally
      {
        memStream?.Dispose();
      }

      Log.LogInformation(
        $"Coord system file for project {project.ProjectUID}: numBytesRead: {numBytesRead} coordSystemFileContent.Length {coordSystemFileContent?.Length ?? 0}");

      Log.LogInformation($"## Out ## {nameof(DownloadCoordinateSystemFileFromTCC)}");

      return coordSystemFileContent;
    }

    /// <summary>
    /// Downloads the file from TCC and if successful uploads it through the Project service.
    /// </summary>
    private async Task<(bool success, FileDataSingleResult file)> MigrateFile(FileData file)
    {
      Log.LogInformation($"## In ## {nameof(MigrateFile)} | Migrating file '{file.Name}', Uid: {file.ImportedFileUid}");

      string tempFileName;

      using (var fileContents = await FileRepo.GetFile(FileSpaceId, $"{file.Path}/{file.Name}"))
      {
        _migrationDb.SetMigrationState(Table.Files, file, MigrationState.InProgress);

        if (fileContents == null)
        {
          string errorMessage = $"Failed to fetch file '{file.Name}' ({file.LegacyFileId}), not found";
          _migrationDb.SetMigrationState(Table.Files, file, MigrationState.FileNotFound);
          _migrationDb.WriteError(file.ProjectUid, errorMessage);

          Log.LogError($"## Out ## {nameof(MigrateFile)} | {errorMessage}");

          return (false, null);
        }

        var tempPath = Path.Combine(TemporaryFolder, "DataOceanMigrationTmp", file.CustomerUid, file.ProjectUid, file.ImportedFileUid);
        Directory.CreateDirectory(tempPath);

        tempFileName = Path.Combine(tempPath, file.Name);

        Log.LogInformation($"{nameof(MigrateFile)} | Creating temporary file '{tempFileName}' for file {file.ImportedFileUid}");

        using (var tempFile = new FileStream(tempFileName, FileMode.Create))
        {
          fileContents.CopyTo(tempFile);
        }
      }

      var result = new FileDataSingleResult();

      if (!_isDebugMode)
      {
        Log.LogInformation($"{nameof(MigrateFile)} | Uploading file {file.ImportedFileUid}");
        result = ImportFile.SendRequestToFileImportV4(UploadFileApiUrl, file, tempFileName, new ImportOptions(HttpMethod.Post));

        _migrationDb.SetMigrationState(Table.Files, file, MigrationState.Completed);
      }
      else
      {
        Log.LogDebug($"## DEBUG MODE ## {nameof(MigrateFile)} | Skipped uploading file '{tempFileName}' to project service");
      }

      Log.LogInformation($"## Out ## {nameof(MigrateFile)} | File {file.ImportedFileUid} update result {result.Code} {result.Message}");

      return (true, result);
    }

    private string GetEnvironmentVariable(string key, int errorNumber)
    {
      var value = ConfigStore.GetValueString(key);
      if (string.IsNullOrEmpty(value))
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, errorNumber,
          $"Missing environment variable {key}");
      }

      return value;
    }
  }
}
