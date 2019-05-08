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

    // Diagnostic settings
    private readonly bool _downloadProjectFiles;
    private readonly bool _uploadProjectFiles;
    private readonly bool _downloadProjectCoordinateSystemFile;
    private readonly bool _updateProjectCoordinateSystemFile;
    private readonly bool _saveCoordinateSystemFile;
    private readonly bool _saveFailedProjects;

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

      // Diagnostic settings
      _downloadProjectFiles = configStore.GetValueBool("DOWNLOAD_PROJECT_FILES", defaultValue: false);
      _uploadProjectFiles = configStore.GetValueBool("UPLOAD_PROJECT_FILES", defaultValue: false);
      _downloadProjectCoordinateSystemFile = configStore.GetValueBool("DOWNLOAD_PROJECT_COORDINATE_SYSTEM_FILE", defaultValue: false);
      _updateProjectCoordinateSystemFile = configStore.GetValueBool("UPDATE_PROJECT_COORDINATE_SYSTEM_FILE", defaultValue: false);
      _saveCoordinateSystemFile = configStore.GetValueBool("SAVE_COORDIANTE_SYSTEM_FILE", defaultValue: false);
      _saveFailedProjects = configStore.GetValueBool("SAVE_FAILED_PROJECT_IDS", defaultValue: true);
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

        Log.LogInformation($"{Method.Info()} Cleaning database, dropping collections");
        _migrationDb.DropTables(new[]
        {
          Table.MigrationInfo,
          Table.Projects,
          Table.Files,
          Table.Errors
        });

        projects = tmpProjects;
      }
      else
      {
        Log.LogInformation($"{Method.Info()} Cleaning database, dropping collections");
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
      }

      _migrationDb.InitDatabase();

      var projectTasks = new List<Task<bool>>(projects.Count);
      _migrationDb.SetMigationInfo_SetProjectCount(projects.Count);

      // Throttle the async jobs or on we could attempt to run thousands or projection migrations
      // currently; which results in file stream errors in production.
      const int THROTTLE_ASYNC_JOBS = 10;

      foreach (var project in projects)
      {
        _migrationDb.WriteRecord(Table.Projects, project);

        projectTasks.Add(MigrateProject(project));

        if (projectTasks.Count != THROTTLE_ASYNC_JOBS) continue;

        var completed = await Task.WhenAny(projectTasks);
        projectTasks.Remove(completed);
      }

      await Task.WhenAll(projectTasks);

      // DIAGNOSTIC RUNTIME SWITCH
      if (_saveFailedProjects)
      {
        // Create a recovery file of project uids for re processing
        var processedProjects = _migrationDb.GetTable<MigrationProject>(Table.Projects);
        var logFilename = Path.Combine(TemporaryFolder, $"MigrationRecovery_{DateTime.Now.Date.ToShortDateString().Replace('/', '-')}_{DateTime.Now.Hour}{DateTime.Now.Minute}{DateTime.Now.Second}.log");

        using (TextWriter tw = new StreamWriter(logFilename))
        {
          foreach (var project in processedProjects)
          {
            if (project.MigrationState != MigrationState.Completed)
            {
              tw.WriteLine(project.ProjectUid);
            }
          }

          tw.Close();
        }
      }
    }

    /// <summary>
    /// Migrate all elgible files for a given project.
    /// </summary>
    private async Task<bool> MigrateProject(Project project)
    {
      try
      {
        Log.LogInformation($"{Method.In()} Migrating project {project.ProjectUID}, Name: '{project.Name}'");
        _migrationDb.SetMigrationState(Table.Projects, project, MigrationState.InProgress);

        var coordinateSystemFileMigrationResult = false;

        // DIAGNOSTIC RUNTIME SWITCH
        if (_downloadProjectCoordinateSystemFile)
        {
          // Get the real CSIB for this project, ignoring what's attached to the project in the database.
          var csibResponse = await _csibAgent.GetCSIBForProject(project);

          _migrationDb.SetCanResolveCSIB(Table.Projects, project.ProjectUID, csibResponse.Code == 0);

          byte[] coordSystemFileContent;

          if (csibResponse.Code != 0)
          {
            _migrationDb.SetResolveCSIBMessage(Table.Projects, project.ProjectUID, csibResponse.Message);

            // We couldn't resolve a CSIB for the project, so try using the DC file if one exists.
            coordSystemFileContent = await DownloadCoordinateSystemFileFromTCC(project);
          }
          else
          {
            _migrationDb.SetProjectCSIB(Table.Projects, project.ProjectUID, csibResponse.Message);

            var coordSysInfo = await _csibAgent.GetCoordSysInfoFromCSIB64(project, csibResponse.Message);
            var dcFileContent = await _csibAgent.GetCalibrationFileForCoordSysId(project, coordSysInfo["coordinateSystem"]["id"].ToString());

            coordSystemFileContent = Encoding.UTF8.GetBytes(dcFileContent);
          }

          if (coordSystemFileContent != null && coordSystemFileContent.Length > 0)
          {
            _migrationDb.SetProjectCoordinateSystemDetails(Table.Projects, project, true);

            // DIAGNOSTIC RUNTIME SWITCH
            if (_updateProjectCoordinateSystemFile)
            {
              var updateProjectResult = await WebApiUtils.UpdateProjectCoordinateSystemFile(ProjectApiUrl, project, coordSystemFileContent);

              coordinateSystemFileMigrationResult = updateProjectResult.Code == (int)ExecutionResult.Success;

              Log.LogInformation($"{Method.Info()} Update result {updateProjectResult.Message} ({updateProjectResult.Code})");
            }
            else
            {
              Log.LogDebug($"{Method.Info("DEBUG")} Skipping updating project coordinate system file step");
            }

            // DIAGNOSTIC STEP: To validate content of DC file is good.
            SaveDCFileToDisk(project, coordSystemFileContent);
          }
        }
        else
        {
          _migrationDb.SetProjectCoordinateSystemDetails(Table.Projects, project, false);
        }

        var importedFilesResult = false;
        var filesResult = await ImportFile.GetImportedFilesFromWebApi($"{ImportedFileApiUrl}?projectUid={project.ProjectUID}", project);

        if (filesResult == null)
        {
          Log.LogInformation($"{Method.Info()} Failed to fetch imported files for project {project.ProjectUID}, aborting project migration");
          _migrationDb.SetMigrationState(Table.Projects, project, MigrationState.Failed);

          return false;
        }

        if (filesResult.ImportedFileDescriptors?.Count > 0)
        {
          var selectedFiles = filesResult.ImportedFileDescriptors.Where(f => MigrationFileTypes.Contains(f.ImportedFileType)).ToList();
          _migrationDb.SetProjectFilesDetails(Table.Projects, project, filesResult.ImportedFileDescriptors.Count, selectedFiles.Count);

          Log.LogInformation($"{Method.Info()} Found {selectedFiles.Count} eligible files out of {filesResult.ImportedFileDescriptors.Count} total to migrate for {project.ProjectUID}");

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
          Log.LogInformation($"{Method.Info()} No files found for {project.ProjectUID}");
        }

        var result = (coordinateSystemFileMigrationResult && importedFilesResult) || filesResult.ImportedFileDescriptors?.Count == 0;

        _migrationDb.SetMigrationState(Table.Projects, project, result ? MigrationState.Completed : MigrationState.Failed);

        Log.LogInformation($"{Method.Out()} Project '{project.Name}' ({project.ProjectUID}) {(result ? "succeeded" : "failed")}");

        _migrationDb.SetMigationInfo_IncrementProjectsProcessed();

        return result;
      }
      catch (Exception exception)
      {
        Log.LogError($"{Method.Info()} Error processing project {project.ProjectUID}");
        Log.LogError(exception.GetBaseException().Message);
      }

      return false;
    }

    /// <summary>
    /// Saves the DC file content to disk; for testing purposes only so we can eyeball the content.
    /// </summary>
    private void SaveDCFileToDisk(Project project, byte[] dcFileContent)
    {
      Log.LogDebug($"{Method.In()} Writing coordinate system file for project {project.ProjectUID}");

      var dcFilePath = Path.Combine(TemporaryFolder, project.CustomerUID, project.ProjectUID);

      if (!Directory.Exists(dcFilePath)) Directory.CreateDirectory(dcFilePath);

      var tempFileName = Path.Combine(dcFilePath, "CoordSystemFile.dc");

      Log.LogInformation($"{Method.Info()} Creating DC file '{tempFileName}' for project {project.ProjectUID}");

      File.WriteAllBytes(tempFileName, dcFileContent);
    }

    /// <summary>
    /// Downloads the coordinate system file for a given project.
    /// </summary>
    private async Task<byte[]> DownloadCoordinateSystemFileFromTCC(Project project)
    {
      Log.LogInformation($"{Method.In()} Downloading coord system file '{project.CoordinateSystemFileName}'");

      // DIAGNOSTIC RUNTIME SWITCH
      if (!_downloadProjectCoordinateSystemFile)
      {
        Log.LogDebug($"{Method.Info("DEBUG")} Skipped downloading coordinate system file '{project.CoordinateSystemFileName}' for project {project.ProjectUID}");
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

            Log.LogInformation($"{Method.Info()} Creating temporary file '{tempFileName}' for project {project.ProjectUID}");

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

      return coordSystemFileContent;
    }

    /// <summary>
    /// Downloads the file from TCC and if successful uploads it through the Project service.
    /// </summary>
    private async Task<(bool success, FileDataSingleResult file)> MigrateFile(FileData file)
    {
      Log.LogInformation($"{Method.In()} Migrating file '{file.Name}', Uid: {file.ImportedFileUid}");

      string tempFileName;

      using (var fileContents = await FileRepo.GetFile(FileSpaceId, $"{file.Path}/{file.Name}"))
      {
        _migrationDb.SetMigrationState(Table.Files, file, MigrationState.InProgress);

        if (fileContents == null)
        {
          var message = $"Failed to fetch file '{file.Name}' ({file.LegacyFileId}), not found";
          _migrationDb.SetMigrationState(Table.Files, file, MigrationState.FileNotFound);
          _migrationDb.WriteError(file.ProjectUid, message);

          Log.LogWarning($"{Method.Out()} {message}");

          return (false, null);
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

            _migrationDb.SetFileSize(Table.Files, file, tempFile.Length);
          }
        }
      }

      var result = new FileDataSingleResult();

      // DIAGNOSTIC RUNTIME SWITCH
      if (_downloadProjectFiles && _uploadProjectFiles)
      {
        Log.LogInformation($"{Method.Info()} Uploading file {file.ImportedFileUid}");
        result = ImportFile.SendRequestToFileImportV4(UploadFileApiUrl, file, tempFileName, new ImportOptions(HttpMethod.Post));

        _migrationDb.SetMigrationState(Table.Files, file, MigrationState.Completed);
      }

      Log.LogInformation($"{Method.Out()} File {file.ImportedFileUid} update result {result.Code} {result.Message}");

      return (true, result);
    }
  }
}
