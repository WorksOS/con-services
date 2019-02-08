using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
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
    private readonly MigrationSettings MigrationSettings;
    private readonly string FileSpaceId;
    private readonly string ProjectApiUrl;
    private readonly string ImportedFileApiUrl;
    private readonly string TemporaryFolder;

    private const string ProjectWebApiKey = "PROJECT_API_URL";
    private const string ImportedFileWebApiKey = "IMPORTED_FILE_API_URL";
    private const string TccFilespaceKey = "TCCFILESPACEID";
    private const string TemporaryFolderKey = "TEMPORARY_FOLDER";

    private readonly List<ImportedFileType> MigrationFileTypes = new List<ImportedFileType>
    {
      ImportedFileType.Linework,
      ImportedFileType.DesignSurface,
      ImportedFileType.SurveyedSurface,
      ImportedFileType.Alignment
    };

    public Migrator(IProjectRepository projectRepository, IConfigurationStore configStore,
      IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo, IWebApiUtils webApiUtils, IImportFile importFile, IMigrationSettings migrationSettings)
    {
      ProjectRepo = projectRepository;
      ConfigStore = configStore;
      FileRepo = fileRepo;
      ServiceExceptionHandler = serviceExceptionHandler;
      WebApiUtils = webApiUtils;
      ImportFile = importFile;
      FileSpaceId = GetEnvironmentVariable(TccFilespaceKey, 48);
      ProjectApiUrl = GetEnvironmentVariable(ProjectWebApiKey, 1);
      ImportedFileApiUrl = GetEnvironmentVariable(ImportedFileWebApiKey, 3);
      TemporaryFolder = GetEnvironmentVariable(TemporaryFolderKey, 2);
      MigrationSettings = (MigrationSettings)migrationSettings;
    }

    public async Task<bool> MigrateFilesForAllActiveProjects()
    {
      Log.Information($"Fetching projects from: '{ProjectApiUrl}'");
      var projects = (await ProjectRepo.GetActiveProjects()).ToList();
      Log.Information($"Found {projects.Count} projects");

      // TODO (Aaron) Convert to dictionary and store project UID.
      var projectTasks = new List<Task<bool>>(projects.Count);

      foreach (var project in projects)
      {
        projectTasks.Add(MigrateProject(project));
      }

      await Task.WhenAll(projectTasks);

      var result = projectTasks.All(t => t.Result);
      Log.Information($"Overall migration result {result}");

      return result;
    }

    /// <summary>
    /// Migrate all elgible files for a given project.
    /// </summary>
    private async Task<bool> MigrateProject(Project project)
    {
      Log.Information($"PUID: {project.ProjectUID} | Migrating project '{project.Name}'");

      var coordinateSystemFileMigrationResult = false;

      if (!string.IsNullOrEmpty(project.CoordinateSystemFileName))
      {
        var coordSystemFileContent = await DownloadCoordinateSystemFileFromTCC(project);
        var updateProjectResult = WebApiUtils.UpdateProjectCoordinateSystemFile(ProjectApiUrl, project, coordSystemFileContent);

        coordinateSystemFileMigrationResult = updateProjectResult.Code == (int)ExecutionResult.Success;

        Log.Information($"PUID: {project.ProjectUID} | Update result code: {updateProjectResult.Code}, {updateProjectResult.Message}");
      }

      //Get list of imported files for project from project web api
      Log.Information($"PUID: {project.ProjectUID} | Getting files");
      var importedFilesResult = false;
      var filesResult = ImportFile.GetImportedFilesFromWebApi($"{ImportedFileApiUrl}?projectUid={project.ProjectUID}", project.CustomerUID);
      var filesList = filesResult.ImportedFileDescriptors;
      if (filesList?.Count > 0)
      {
        var selectedFiles =
          filesResult.ImportedFileDescriptors.Where(f => MigrationFileTypes.Contains(f.ImportedFileType)).ToList();
        Log.Information($"PUID: {project.ProjectUID} | {selectedFiles.Count} out of {filesList.Count} files to migrate");
        var fileTasks = new List<Task<FileDataSingleResult>>();

        foreach (var file in selectedFiles)
        {
          fileTasks.Add(MigrateFile(file));
        }

        await Task.WhenAll(fileTasks);

        importedFilesResult = fileTasks.All(t => t.Result.Code == (int)ExecutionResult.Success);
      }
      else
      {
        Log.Information($"PUID: {project.ProjectUID} | No files found");
      }

      var result = coordinateSystemFileMigrationResult && importedFilesResult;
      Log.Information($"PUID: {project.ProjectUID} | Migration result: {result}");

      return result;
    }

    private async Task<byte[]> DownloadCoordinateSystemFileFromTCC(Project project)
    {
      Log.Information($"PUID: {project.ProjectUID} | Downloading coord system file '{project.CoordinateSystemFileName}'");

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

      Log.Information(
        $"Coord system file for project {project.ProjectUID}: numBytesRead: {numBytesRead} coordSystemFileContent.Length {coordSystemFileContent?.Length ?? 0}");
      return coordSystemFileContent;

    }
    private async Task<FileDataSingleResult> MigrateFile(FileData file)
    {
      Log.Information($"Migrating file: Name: {file.Name}, Uid: {file.ImportedFileUid}");

      string tempFileName;

      using (var fileContents = await FileRepo.GetFile(FileSpaceId, $"{file.Path}/{file.Name}"))
      {
        var tempPath = Path.Combine(TemporaryFolder, file.ImportedFileUid);
        tempFileName = Path.Combine(tempPath, file.Name);

        Directory.CreateDirectory(tempPath);

        Log.Information($"Creating temporary file {tempFileName} for file {file.ImportedFileUid}");

        using (var tempFile = new FileStream(tempFileName, FileMode.Create))
        {
          fileContents.CopyTo(tempFile);
        }
      }

      Log.Information($"Uploading file {file.ImportedFileUid}");

      var result = MigrationSettings.IsDebug
        ? new FileDataSingleResult()
        : ImportFile.SendRequestToFileImportV4(ImportedFileApiUrl, file, tempFileName, new ImportOptions(HttpMethod.Post));

      Log.Information($"File {file.ImportedFileUid} update result {result.Code} {result.Message}");

      return result;
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
