using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCCToDataOcean.Interfaces;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Handlers;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.MasterData.Models.ResultHandling.Abstractions;
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
    private string FileSpaceId;
    private readonly string ProjectApiUrl;
    private readonly string ImportedFileApiUrl;
    private readonly string TemporaryFolder;

    private const string ProjectWebApiKey = "PROJECT_API_URL";
    private const string ImportedFileWebApiKey = "IMPORTED_FILE_API_URL";
    private const string TccFilespaceKey = "TCCFILESPACEID";
    private const string TemporaryFolderKey = "TEMPORARY_FOLDER";

    private List<ImportedFileType> MigrationFileTypes = new List<ImportedFileType>
    {
      ImportedFileType.Linework,
      ImportedFileType.DesignSurface,
      ImportedFileType.SurveyedSurface,
      ImportedFileType.Alignment
    };

    public Migrator(IProjectRepository projectRepository, IConfigurationStore configStore, 
      IServiceExceptionHandler serviceExceptionHandler, IFileRepository fileRepo,
      ILoggerFactory loggerFactory, IWebApiUtils webApiUtils, IImportFile importFile)
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
      Log = loggerFactory.CreateLogger<Migrator>();
    }

    public async Task<bool> MigrateFilesForAllActiveProjects()
    {
      //Get list of all active projects for all customers fronm project repo
      Log.LogInformation($"Getting list of projects from project repo");
      var projects = (await ProjectRepo.GetActiveProjects()).ToList();
      Log.LogInformation($"Got {projects.Count} projects");
      var projectTasks = new List<Task<bool>>();
      foreach (var project in projects)
      {
        projectTasks.Add(MigrateProject(project));      
      }
      await Task.WhenAll(projectTasks);
      var result = projectTasks.All(t => t.Result);
      Log.LogInformation($"Overall migration result {result}");
      return result;
    }

    private async Task<bool> MigrateProject(Project project)
    {
      Log.LogInformation($"Migrating project {project.Name} {project.ProjectUID}");
      //Migrate coordinate system file if applicable
      var coordFileResult = true;
      if (!string.IsNullOrEmpty(project.CoordinateSystemFileName))
      {
        //Download coordinate system file from TCC
        var coordSystemFileContent = await GetCoordSystemFileContent(project);
        //Update project in project web api to get coordinate system file migrated
        var updateProjectResult = WebApiUtils.UpdateProjectViaWebApi(ProjectApiUrl, project, coordSystemFileContent);
        coordFileResult = updateProjectResult.Code == ContractExecutionStatesEnum.ExecutedSuccessfully;
        Log.LogInformation($"Project {project.ProjectUID} update result {updateProjectResult.Code} {updateProjectResult.Message}");
      }
      //Get list of imported files for project from project web api
      Log.LogInformation($"Getting list of files for project {project.ProjectUID} from {ImportedFileApiUrl}");
      var importedFilesResult = true;
      var filesResult = ImportFile.GetImportedFilesFromWebApi($"{ImportedFileApiUrl}?projectUid={project.ProjectUID}", project.CustomerUID);
      var filesList = filesResult.ImportedFileDescriptors;
      if (filesList?.Count > 0)
      {
        var selectedFiles =
          filesResult.ImportedFileDescriptors.Where(f => MigrationFileTypes.Contains(f.ImportedFileType)).ToList();
        Log.LogInformation($"Found {selectedFiles.Count} out of {filesList.Count} files to migrate for project {project.ProjectUID}");
        var fileTasks = new List<Task<FileDataSingleResult>>();
        foreach (var file in selectedFiles)
        {
          fileTasks.Add(MigrateFile(file));
        }
        await Task.WhenAll(fileTasks);
        importedFilesResult = fileTasks.All(t => t.Result.Code == ContractExecutionStatesEnum.ExecutedSuccessfully);
      }
      else
      {
        Log.LogInformation($"No files found for project {project.ProjectUID}");
      }
      var result = coordFileResult && importedFilesResult;
      Log.LogInformation($"Project {project.ProjectUID} migration result {result}");
      return result;
    }

    private async Task<byte[]> GetCoordSystemFileContent(Project project)
    {
      Log.LogInformation($"Downloading coord system file {project.CoordinateSystemFileName} for project {project.ProjectUID}");

      Stream memStream = null;
      byte[] coordSystemFileContent = null;
      int numBytesRead = 0;

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
    private async Task<FileDataSingleResult> MigrateFile(FileData file)
    {
      Log.LogInformation($"Migrating file {file.Name} {file.ImportedFileUid}");
      //Download file from TCC
      Log.LogInformation($"Downloading file {file.ImportedFileUid}");
      var fileContents = await FileRepo.GetFile(FileSpaceId, $"{file.Path}/{file.Name}");
      string tempPath = $"{TemporaryFolder}/{file.ImportedFileUid}";
      Directory.CreateDirectory(tempPath);
      var tempFileName = $"{tempPath}/{file.Name}";
      Log.LogInformation($"Creating temporary file {tempFileName} for file {file.ImportedFileUid}");
      using (FileStream tempFile = new FileStream(tempFileName, FileMode.Create))
      {
        fileContents.CopyTo(tempFile);
      }
      fileContents?.Dispose();
      //Upload file to project web api to migrate
      Log.LogInformation($"Uploading file {file.ImportedFileUid}");
      var result = ImportFile.SendRequestToFileImportV4(ImportedFileApiUrl, file, tempFileName, new ImportOptions(HttpMethod.Post));
      Log.LogInformation($"File {file.ImportedFileUid} update result {result.Code} {result.Message}");
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
