using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
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
  enum ExecutionResult
  {
    Unknown = -1,
    Success = 0,
    Failed = 1
  }

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
    private readonly bool _isDebugMode;

    private const string ProjectWebApiKey = "PROJECT_API_URL";
    private const string ImportedFileWebApiKey = "IMPORTED_FILE_API_URL";
    private const string UploadFileApiKey = "IMPORTED_FILE_API_URL2";
    private const string TccFilespaceKey = "TCCFILESPACEID";
    private const string TemporaryFolderKey = "TEMPORARY_FOLDER";

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
      FileSpaceId = GetEnvironmentVariable(TccFilespaceKey, 48);
      ProjectApiUrl = GetEnvironmentVariable(ProjectWebApiKey, 1);
      UploadFileApiUrl = GetEnvironmentVariable(UploadFileApiKey, 1);
      ImportedFileApiUrl = GetEnvironmentVariable(ImportedFileWebApiKey, 3);
      TemporaryFolder = GetEnvironmentVariable(TemporaryFolderKey, 2);
      _isDebugMode = ConfigStore.GetValueBool("MIGRATION_MODE_IS_DEBUG", defaultValue: true);
      _migrationDb = liteDbAgent;
    }

    public async Task<bool> MigrateFilesForAllActiveProjects()
    {
      // TODO (Aaron) Remove if part of recovery phase.
      Log.LogInformation("Cleaning database, dropping collections");
      _migrationDb.DropTables(new[]
      {
          LiteDbAgent.Table.Projects,
          LiteDbAgent.Table.Files,
          LiteDbAgent.Table.Errors
      });

      Log.LogInformation($"Fetching projects from: '{ProjectApiUrl}'");
      var projects = (await ProjectRepo.GetActiveProjects()).ToList();
      Log.LogInformation($"Found {projects.Count} projects");

      // TODO (Aaron) Convert to dictionary and store project UID.
      var projectTasks = new List<Task<bool>>(projects.Count);

      var project = projects.First(p => p.ProjectUID == "67a52e4f-faa2-e511-80e5-0050568821e6");
      _migrationDb.WriteRecord(LiteDbAgent.Table.Projects, project);

      await MigrateProject(project);


      //foreach (var project in projects)
      //{
      //  projectTasks.Add(MigrateProject(project));
      //}

      //await Task.WhenAll(projectTasks);

      var result = projectTasks.All(t => t.Result);
      Log.LogInformation($"Overall migration result {result}");

      return result;
    }

    /// <summary>
    /// Migrate all elgible files for a given project.
    /// </summary>
    private async Task<bool> MigrateProject(Project project)
    {
      Log.LogInformation($"{nameof(MigrateProject)} [{project.ProjectUID}]: Migrating project '{project.Name}'");
      _migrationDb.SetMigrationState(LiteDbAgent.Table.Projects, project, LiteDbAgent.MigrationState.InProgress);

      var coordinateSystemFileMigrationResult = false;

      if (!string.IsNullOrEmpty(project.CoordinateSystemFileName))
      {
        var coordSystemFileContent = await DownloadCoordinateSystemFileFromTCC(project);

        if (coordSystemFileContent != null && coordSystemFileContent.Length > 0)
        {
          var updateProjectResult = WebApiUtils.UpdateProjectCoordinateSystemFile(ProjectApiUrl, project, coordSystemFileContent);

          coordinateSystemFileMigrationResult = updateProjectResult.Code == (int)ExecutionResult.Success;

          Log.LogInformation($"{nameof(MigrateProject)} [{project.ProjectUID}]: Update result {updateProjectResult.Message} ({updateProjectResult.Code})");
        }
      }

      //Get list of imported files for project from project web api
      Log.LogInformation($"PUID: {project.ProjectUID} | Getting files");
      var importedFilesResult = false;
      var filesResult = ImportFile.GetImportedFilesFromWebApi($"{ImportedFileApiUrl}?projectUid={project.ProjectUID}", project.CustomerUID);
      var filesList = filesResult.ImportedFileDescriptors;
      if (filesList?.Count > 0)
      {
        var selectedFiles =
          filesResult.ImportedFileDescriptors.Where(f => MigrationFileTypes.Contains(f.ImportedFileType)).ToList();
        Log.LogInformation($"PUID: {project.ProjectUID} | {selectedFiles.Count} out of {filesList.Count} files to migrate");
        var fileTasks = new List<Task<(bool, FileDataSingleResult)>>();

        foreach (var file in selectedFiles)
        {
          _migrationDb.WriteRecord(LiteDbAgent.Table.Files, file);
          var migrationResult = MigrateFile(file);

          fileTasks.Add(migrationResult);
        }

        await Task.WhenAll(fileTasks);

        // Todo (Aaron) In time it might be better to not return a tuple, and simply a null file. Making the following simpler.
        importedFilesResult = fileTasks.All(t => t.Result.Item2.Code == (int)ExecutionResult.Success);
      }
      else
      {
        Log.LogInformation($"PUID: {project.ProjectUID} | No files found");
      }

      var result = coordinateSystemFileMigrationResult && importedFilesResult;
      Log.LogInformation($"PUID: {project.ProjectUID} | Migration result: {result}");

      _migrationDb.SetMigrationState(LiteDbAgent.Table.Projects, project, result ? LiteDbAgent.MigrationState.Completed : LiteDbAgent.MigrationState.Failed);

      return result;
    }

    private async Task<byte[]> DownloadCoordinateSystemFileFromTCC(Project project)
    {
      Log.LogInformation($"{nameof(DownloadCoordinateSystemFileFromTCC)} [{project.ProjectUID}]: Downloading coord system file '{project.CoordinateSystemFileName}'");

      Stream memStream = null;
      byte[] coordSystemFileContent = null;
      var numBytesRead = 0;

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
          ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError,
            80, $" isAbleToRead: {memStream != null && memStream.CanRead} bytesReturned: {memStream?.Length ?? 0}");
        }
      }
      catch (Exception e)
      {
        ServiceExceptionHandler.ThrowServiceException(HttpStatusCode.InternalServerError, 79, e.Message);
      }
      finally
      {
        memStream?.Dispose();
      }

      Log.LogInformation(
        $"Coord system file for project {project.ProjectUID}: numBytesRead: {numBytesRead} coordSystemFileContent.Length {coordSystemFileContent?.Length ?? 0}");
      return coordSystemFileContent;

    }

    /// <summary>
    /// Downloads the file from TCC and if successful uploads it through the Project service.
    /// </summary>
    private async Task<(bool success, FileDataSingleResult file)> MigrateFile(FileData file)
    {
      Log.LogInformation($"Migrating file: Name: {file.Name}, Uid: {file.ImportedFileUid}");

      string tempFileName;

      using (var fileContents = await FileRepo.GetFile(FileSpaceId, $"{file.Path}/{file.Name}"))
      {
        _migrationDb.SetMigrationState(LiteDbAgent.Table.Files, file, LiteDbAgent.MigrationState.InProgress);

        if (fileContents == null)
        {
          Log.LogError($"Failed to fetch file '{file.Name}' ({file.LegacyFileId})");
          _migrationDb.SetMigrationState(LiteDbAgent.Table.Files, file, LiteDbAgent.MigrationState.Failed);
          _migrationDb.WriteError($"Failed to fetch file '{file.Name}' ({file.LegacyFileId})");

          return (false, null);
        }

        var tempPath = Path.Combine(TemporaryFolder, "DataOceanMigrationTmp", file.CustomerUid, file.ProjectUid, file.ImportedFileUid);
        Directory.CreateDirectory(tempPath);

        tempFileName = Path.Combine(tempPath, file.Name);

        Log.LogInformation($"Creating temporary file {tempFileName} for file {file.ImportedFileUid}");

        using (var tempFile = new FileStream(tempFileName, FileMode.Create))
        {
          fileContents.CopyTo(tempFile);
        }
      }

      Log.LogInformation($"Uploading file {file.ImportedFileUid}");

      var result = _isDebugMode
        ? new FileDataSingleResult()
        : ImportFile.SendRequestToFileImportV4(UploadFileApiUrl, file, tempFileName, new ImportOptions(HttpMethod.Post));

      Log.LogInformation($"File {file.ImportedFileUid} update result {result.Code} {result.Message}");
      _migrationDb.SetMigrationState(LiteDbAgent.Table.Files, file, LiteDbAgent.MigrationState.Completed);

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
