using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Models;
using TCCToDataOcean.Types;
using TCCToDataOcean.Utils;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Models.ResultHandling;
using VSS.Productivity3D.Project.Abstractions.Interfaces.Repository;
using VSS.Productivity3D.Project.Abstractions.Models.DatabaseModels;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean
{
  public class Migrator : IMigrator
  {
    private readonly IProjectRepository ProjectRepo;
    private readonly IFileRepository FileRepo;
    private readonly IWebApiUtils WebApiUtils;
    private readonly IImportFile ImportFile;
    private readonly ILogger Log;
    private readonly ILiteDbAgent _migrationDb;
    private readonly ICSIBAgent _csibAgent;

    private readonly string FileSpaceId;
    private readonly string ProjectApiUrl;
    private readonly string UploadFileApiUrl;
    private readonly string ImportedFileApiUrl;
    private readonly string TemporaryFolder;

    private readonly bool _resumeModeEnabled;

    // Diagnostic settings
    private readonly bool _downloadProjectFiles;
    private readonly bool _uploadProjectFiles;
    private readonly bool _downloadProjectCoordinateSystemFile;
    private readonly bool _updateProjectCoordinateSystemFile;
    private readonly bool _saveCoordinateSystemFile;

    private readonly List<ImportedFileType> MigrationFileTypes = new List<ImportedFileType>
    {
      ImportedFileType.Linework,
      ImportedFileType.DesignSurface,
      ImportedFileType.SurveyedSurface,
      ImportedFileType.Alignment
    };

    public Migrator(ILoggerFactory logger, IProjectRepository projectRepository, IConfigurationStore configStore,
                    ILiteDbAgent liteDbAgent, IFileRepository fileRepo, IWebApiUtils webApiUtils, IImportFile importFile,
                    IEnvironmentHelper environmentHelper, ICSIBAgent csibAgent)
    {
      Log = logger.CreateLogger<Migrator>();
      ProjectRepo = projectRepository;
      var configStore1 = configStore;
      FileRepo = fileRepo;
      WebApiUtils = webApiUtils;
      ImportFile = importFile;
      _migrationDb = liteDbAgent;
      _csibAgent = csibAgent;

      FileSpaceId = environmentHelper.GetVariable("TCCFILESPACEID", 48);
      ProjectApiUrl = environmentHelper.GetVariable("PROJECT_API_URL", 1);
      UploadFileApiUrl = environmentHelper.GetVariable("IMPORTED_FILE_API_URL2", 1);
      ImportedFileApiUrl = environmentHelper.GetVariable("IMPORTED_FILE_API_URL", 3);
      TemporaryFolder = Path.Combine(environmentHelper.GetVariable("TEMPORARY_FOLDER", 2), "DataOceanMigrationTmp");

      _resumeModeEnabled = configStore1.GetValueBool("RESUME_MODE_ENABLED", defaultValue: false);
      // Diagnostic settings
      _downloadProjectFiles = configStore1.GetValueBool("DOWNLOAD_PROJECT_FILES", defaultValue: false);
      _uploadProjectFiles = configStore1.GetValueBool("UPLOAD_PROJECT_FILES", defaultValue: false);
      _downloadProjectCoordinateSystemFile = configStore1.GetValueBool("DOWNLOAD_PROJECT_COORDINATE_SYSTEM_FILE", defaultValue: false);
      _updateProjectCoordinateSystemFile = configStore1.GetValueBool("UPDATE_PROJECT_COORDINATE_SYSTEM_FILE", defaultValue: false);
      _saveCoordinateSystemFile = configStore1.GetValueBool("SAVE_COORDIANTE_SYSTEM_FILE", defaultValue: false);
    }

    public async Task MigrateFilesForAllActiveProjects()
    {
      Log.LogInformation(Method.In());

      // TODO (Aaron) Remove if part of recovery phase.
      Log.LogInformation($"{Method.Info()} | Cleaning database, dropping collections");
      _migrationDb.DropTables(new[]
      {
          Table.MigrationInfo,
          Table.Projects,
          Table.Files,
          Table.Errors
      });

      if (Directory.Exists(TemporaryFolder))
      {
        Directory.Delete(TemporaryFolder, recursive: true);
      }
      // TODO (Aaron) Tidy up complete.

      _migrationDb.InitDatabase();

      Log.LogInformation($"{Method.Info()} | Fetching projects from: '{ProjectApiUrl}'");
      var projects = (await ProjectRepo.GetActiveProjects()).ToList();
      Log.LogInformation($"{Method.Info()} | Found {projects.Count} projects");

      var projectTasks = new List<Task<bool>>(projects.Count);
      _migrationDb.SetMigationInfo_SetProjectCount(projects.Count);

      //var project = projects.First(p => p.ProjectUID == "6470f6f5-77d7-e511-80dc-a0369f4c5117");
      //_migrationDb.WriteRecord(Table.Projects, project);

      //await MigrateProject(project);

      foreach (var project in projects)
      {
        _migrationDb.WriteRecord(Table.Projects, project);

        projectTasks.Add(MigrateProject(project));
      }

      await Task.WhenAll(projectTasks);
    }

    /// <summary>
    /// Migrate all elgible files for a given project.
    /// </summary>
    private async Task<bool> MigrateProject(Project project)
    {
      Log.LogInformation($"{Method.In()} | Migrating project {project.ProjectUID}, Name: '{project.Name}'");
      _migrationDb.SetMigrationState(Table.Projects, project, MigrationState.InProgress);

      var coordinateSystemFileMigrationResult = false;

      if (!string.IsNullOrEmpty(project.CoordinateSystemFileName))
      {
        // GET CSIB
        var csibResponse = await _csibAgent.GetCSIBForProject(project);

        _migrationDb.SetCanResolveCSIB(Table.Projects, project.ProjectUID, csibResponse.Code == 0);

        if (csibResponse.Code != 0)
        {
          // We couldn't resolve a CSIB for the project, so try using the DC file if one exists.
          var coordSystemFileContent = await DownloadCoordinateSystemFileFromTCC(project);

          if (coordSystemFileContent != null && coordSystemFileContent.Length > 0)
          {
            // DIAGNOSTIC RUNTIME SWITCH
            if (_updateProjectCoordinateSystemFile)
            {
              var updateProjectResult = await WebApiUtils.UpdateProjectCoordinateSystemFile(ProjectApiUrl, project, coordSystemFileContent);

              coordinateSystemFileMigrationResult = updateProjectResult.Code == (int)ExecutionResult.Success;

              Log.LogInformation($"{Method.Info()} | Update result {updateProjectResult.Message} ({updateProjectResult.Code})");
            }
            else
            {
              Log.LogDebug($"{Method.Info("DEBUG")} | Skipping updating project coordinate system file step");
            }
          }
          else
          {
            _migrationDb.SetProjectCoordinateSystemDetails(Table.Projects, project, false);
          }
        }
        else
        {
          _migrationDb.SetProjectCSIB(Table.Projects, project.ProjectUID, csibResponse.Message);

          var coordSysInfo = await _csibAgent.GetCoordSysInfoFromCSIB64(project, csibResponse.Message);
          var a = coordSysInfo["coordinateSystem"]["id"].ToString();
          var dcFileContent = await _csibAgent.GetCalibrationFileForCoordSysId(project, coordSysInfo["coordinateSystem"]["id"].ToString());

          var dcFilePath = Path.Combine(TemporaryFolder, project.CustomerUID, project.ProjectUID);
          if (!Directory.Exists(dcFilePath)) Directory.CreateDirectory(dcFilePath);

          var tempFileName = Path.Combine(dcFilePath, "project.dc");

          Log.LogInformation($"{Method.Info()} | Creating DC file '{tempFileName}' for project {project.ProjectUID}");

          File.WriteAllText(tempFileName, dcFileContent);
        }
      }

      var importedFilesResult = false;
      var filesResult = await ImportFile.GetImportedFilesFromWebApi($"{ImportedFileApiUrl}?projectUid={project.ProjectUID}", project);

      if (filesResult.ImportedFileDescriptors?.Count > 0)
      {
        var selectedFiles = filesResult.ImportedFileDescriptors.Where(f => MigrationFileTypes.Contains(f.ImportedFileType)).ToList();
        _migrationDb.SetProjectFilesDetails(Table.Projects, project, filesResult.ImportedFileDescriptors.Count, selectedFiles.Count);

        Log.LogInformation($"{Method.Info()} | Found {selectedFiles.Count} out of {filesResult.ImportedFileDescriptors.Count} files to migrate for {project.ProjectUID}");

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
        Log.LogInformation($"{Method.Info()} | No files found for {project.ProjectUID}");
      }

      var result = (coordinateSystemFileMigrationResult && importedFilesResult) || filesResult.ImportedFileDescriptors?.Count == 0;

      _migrationDb.SetMigrationState(Table.Projects, project, result ? MigrationState.Completed : MigrationState.Failed);

      Log.LogInformation($"{Method.Out()} | Project '{project.Name}' ({project.ProjectUID}) {(result ? "succeeded" : "failed")}");

      _migrationDb.SetMigationInfo_IncrementProjectsProcessed();

      return result;
    }

    /// <summary>
    /// Downloads the coordinate system file for a given project.
    /// </summary>
    private async Task<byte[]> DownloadCoordinateSystemFileFromTCC(Project project)
    {
      Log.LogInformation($"{Method.In()} | Downloading coord system file '{project.CoordinateSystemFileName}'");

      // DIAGNOSTIC RUNTIME SWITCH
      if (!_downloadProjectCoordinateSystemFile)
      {
        Log.LogDebug($"{Method.Info("DEBUG")} | Skipped downloading coordinate system file '{project.CoordinateSystemFileName}' for project {project.ProjectUID}");
        return null;
      }

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

          // DIAGNOSTIC RUNTIME SWITCH
          if (_saveCoordinateSystemFile)
          {
            var tempPath = Path.Combine(TemporaryFolder, project.CustomerUID, project.ProjectUID);
            Directory.CreateDirectory(tempPath);

            var tempFileName = Path.Combine(tempPath, project.CoordinateSystemFileName);

            Log.LogInformation($"{Method.Info()} | Creating temporary file '{tempFileName}' for project {project.ProjectUID}");

            using (var tempFile = new FileStream(tempFileName, FileMode.Create))
            {
              memStream.Position = 0;
              memStream.CopyTo(tempFile);
            }
          }
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

      Log.LogInformation(Method.Out());

      return coordSystemFileContent;
    }

    /// <summary>
    /// Downloads the file from TCC and if successful uploads it through the Project service.
    /// </summary>
    private async Task<(bool success, FileDataSingleResult file)> MigrateFile(FileData file)
    {
      Log.LogInformation($"{Method.In()} | Migrating file '{file.Name}', Uid: {file.ImportedFileUid}");

      string tempFileName;

      using (var fileContents = await FileRepo.GetFile(FileSpaceId, $"{file.Path}/{file.Name}"))
      {
        _migrationDb.SetMigrationState(Table.Files, file, MigrationState.InProgress);

        if (fileContents == null)
        {
          string errorMessage = $"Failed to fetch file '{file.Name}' ({file.LegacyFileId}), not found";
          _migrationDb.SetMigrationState(Table.Files, file, MigrationState.FileNotFound);
          _migrationDb.WriteError(file.ProjectUid, errorMessage);

          Log.LogError($"{Method.Out()} | {errorMessage}");

          return (false, null);
        }

        var tempPath = Path.Combine(TemporaryFolder, file.CustomerUid, file.ProjectUid, file.ImportedFileUid);
        Directory.CreateDirectory(tempPath);

        tempFileName = Path.Combine(tempPath, file.Name);

        Log.LogInformation($"{Method.Info()} | Creating temporary file '{tempFileName}' for file {file.ImportedFileUid}");

        // DIAGNOSTIC RUNTIME SWITCH
        if (_downloadProjectFiles)
        {
          using (var tempFile = new FileStream(tempFileName, FileMode.Create))
          {
            fileContents.CopyTo(tempFile);
          }
        }
        else
        {
          Log.LogDebug($"{Method.Info("DEBUG")} | Skipped downloading file '{tempFileName}' from TCC");
        }
      }

      var result = new FileDataSingleResult();

      // DIAGNOSTIC RUNTIME SWITCH
      if (_downloadProjectFiles && _uploadProjectFiles)
      {
        Log.LogInformation($"{Method.Info()} | Uploading file {file.ImportedFileUid}");
        result = ImportFile.SendRequestToFileImportV4(UploadFileApiUrl, file, tempFileName, new ImportOptions(HttpMethod.Post));

        _migrationDb.SetMigrationState(Table.Files, file, MigrationState.Completed);
      }
      else
      {
        Log.LogDebug($"{Method.Info("DEBUG")} | Skipped uploading file '{tempFileName}' to project service");
      }

      Log.LogInformation($"{Method.Out()} | File {file.ImportedFileUid} update result {result.Code} {result.Message}");

      return (true, result);
    }
  }
}
