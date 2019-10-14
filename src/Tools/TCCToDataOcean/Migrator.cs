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
    private readonly bool _updateProjectCoordinateSystemFile;
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
      _updateProjectCoordinateSystemFile = configStore.GetValueBool("UPDATE_PROJECT_COORDINATE_SYSTEM_FILE", defaultValue: false);
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

        Log.LogInformation($"{Method.Info()} Cleaning database, dropping collections");
        DropTables();

        projects = tmpProjects;
      }
      else
      {
        Log.LogInformation($"{Method.Info()} Cleaning database, dropping collections");
        DropTables();

        if (Directory.Exists(TemporaryFolder))
        {
          Directory.Delete(TemporaryFolder, recursive: true);
        }
      }

      _migrationDb.InitDatabase();

      var projectTasks = new List<Task<bool>>(projects.Count);
      _migrationDb.SetMigationInfo_SetProjectCount(projects.Count);

      foreach (var project in projects)
      {
        _migrationDb.WriteRecord(Table.Projects, project);

        var job = new MigrationJob { Project = project };
        projectTasks.Add(MigrateProject(job));

        if (projectTasks.Count != THROTTLE_ASYNC_PROJECT_JOBS) continue;

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
      _migrationDb.DropTables(new[]
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
        _migrationDb.SetMigrationState(Table.Projects, job, MigrationState.InProgress, null);

        var result = await ResolveProjectCoordinateSystemFile(job);
        if (!result)
        {
          migrationStateMessage = "Unable to resolve coordinate system file";
          return false;
        }

        var filesResult = await ImportFile.GetImportedFilesFromWebApi($"{ImportedFileApiUrl}?projectUid={job.Project.ProjectUID}", job.Project);

        if (filesResult == null)
        {
          Log.LogInformation($"{Method.Info()} Failed to fetch imported files for project {job.Project.ProjectUID}, aborting project migration");
          _migrationDb.SetMigrationState(Table.Projects, job, MigrationState.Failed, "Failed to fetch imported file list");

          migrationStateMessage = "Failed to fetch imported file list";
          return false;
        }

        if (filesResult.ImportedFileDescriptors == null || filesResult.ImportedFileDescriptors.Count == 0)
        {
          Log.LogInformation($"{Method.Info()} Project {job.Project.ProjectUID} contains no imported files, aborting project migration");
          _migrationDb.SetMigrationState(Table.Projects, job, MigrationState.Failed, "No imported files");

          migrationStateMessage = "Project contains no imported files";
          migrationResult = true;
        }
        else
        {
          var selectedFiles = filesResult.ImportedFileDescriptors
                                         .Where(f => MigrationFileTypes.Contains(f.ImportedFileType))
                                         .ToList();

          _migrationDb.SetProjectFilesDetails(Table.Projects, job.Project, filesResult.ImportedFileDescriptors.Count, selectedFiles.Count);

          Log.LogInformation($"{Method.Info()} Found {selectedFiles.Count} eligible files out of {filesResult.ImportedFileDescriptors.Count} total to migrate for {job.Project.ProjectUID}");

          if (selectedFiles.Count == 0)
          {
            Log.LogInformation($"{Method.Info()} Project {job.Project.ProjectUID} contains no eligible files, aborting project migration");

            migrationStateMessage = "Project contains no eligible files";
            migrationResult = true;
          }
          
          var fileTasks = new List<Task<(bool, FileDataSingleResult)>>();

          foreach (var file in selectedFiles)
          {
            _migrationDb.WriteRecord(Table.Files, file);
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
        _migrationDb.SetMigrationState(Table.Projects, job, migrationResult ? MigrationState.Completed : MigrationState.Failed, migrationStateMessage);
        _migrationDb.SetMigationInfo_IncrementProjectsProcessed();
      }

      return false;
    }

    /// <summary>
    /// Resolve the coordinate system file from either TCC or what we know of it from Raptor.
    /// </summary>
    private async Task<bool> ResolveProjectCoordinateSystemFile(MigrationJob job)
    {
      if (string.IsNullOrEmpty(job.Project.CoordinateSystemFileName))
      {
        Log.LogDebug($"Project '{job.Project.ProjectUID}' contains NULL CoordinateSystemFileName field.");
        if (!await ResolveCoordinateSystemFromRaptor(job))
        {
          return false;
        }
      }
      else
      {
        var fileDownloadResult = await DownloadCoordinateSystemFileFromTCC(job);
        if (!fileDownloadResult)
        {
          if (!await ResolveCoordinateSystemFromRaptor(job))
          {
            return false;
          }
        }
      }

      _migrationDb.SetProjectCoordinateSystemDetails(Table.Projects, job.Project);

      // DIAGNOSTIC RUNTIME SWITCH
      if (_updateProjectCoordinateSystemFile)
      {
        var updateProjectResult = await WebApiUtils.UpdateProjectCoordinateSystemFile(ProjectApiUrl, job);

        if (updateProjectResult.Code != 0)
        {
          Log.LogError($"{Method.Info()} Error: Unable to update project coordinate system file; '{updateProjectResult.Message}' ({updateProjectResult.Code})");

          return false;
        }

        Log.LogInformation($"{Method.Info()} Update result '{updateProjectResult.Message}' ({updateProjectResult.Code})");

        return true;
      }

      Log.LogDebug($"{Method.Info("DEBUG")} Skipping updating project coordinate system file step");

      return true;
    }

    private async Task<bool> ResolveCoordinateSystemFromRaptor(MigrationJob job)
    {
      // Failed to download coordinate system file from TCC, try and resolve it using what we know from Raptor.
      var logMessage = $"Failed to fetch coordinate system file '{job.Project.CustomerUID}/{job.Project.ProjectUID}/{job.Project.CoordinateSystemFileName}' from TCC.";
      _migrationDb.WriteWarning(job.Project.ProjectUID, logMessage);
      Log.LogWarning(logMessage);

      // Get the the CSIB for the project from Raptor.
      var csibResponse = await _csibAgent.GetCSIBForProject(job.Project);
      var csib = csibResponse.CSIB;

      if (csibResponse.Code != 0)
      {
        _migrationDb.SetResolveCSIBMessage(Table.Projects, job.Project.ProjectUID, csib);
        _migrationDb.WriteError(job.Project.ProjectUID, $"Failed to resolve CSIB for project: {job.Project.ProjectUID}");

        return false;
      }

      _migrationDb.SetProjectCSIB(Table.Projects, job.Project.ProjectUID, csib);

      var coordSysInfo = await _csibAgent.GetCoordSysInfoFromCSIB64(job.Project, csib);
      var dcFileContent = await _csibAgent.GetCalibrationFileForCoordSysId(job.Project, coordSysInfo["coordinateSystem"]["id"].ToString());

      var coordSystemFileContent = Encoding.UTF8.GetBytes(dcFileContent);

      using (var stream = new MemoryStream(coordSystemFileContent))
      {
        return SaveDCFileToDisk(job, stream);
      }
    }

    /// <summary>
    /// Downloads the coordinate system file for a given project.
    /// </summary>
    private async Task<bool> DownloadCoordinateSystemFileFromTCC(MigrationJob job)
    {
      Log.LogInformation($"{Method.In()} Downloading coord system file '{job.Project.CoordinateSystemFileName}' from TCC");

      Stream memStream = null;

      try
      {
        memStream = await FileRepo.GetFile(FileSpaceId, $"/{job.Project.CustomerUID}/{job.Project.ProjectUID}/{job.Project.CoordinateSystemFileName}");

        return SaveDCFileToDisk(job, memStream);
      }
      finally
      {
        memStream?.Dispose();
      }
    }

    /// <summary>
    /// Saves the DC file content to disk; for testing purposes only so we can eyeball the content.
    /// </summary>
    private bool SaveDCFileToDisk(MigrationJob job, Stream dcFileContent)
    {
      Log.LogDebug($"{Method.In()} Writing coordinate system file for project {job.Project.ProjectUID}");

      if (dcFileContent == null || dcFileContent.Length <= 0)
      {
        Log.LogDebug($"{Method.Info()} Error: Null stream provided for dcFileContent for project '{job.Project.ProjectUID}'");
        return false;
      }

      using (var memoryStream = new MemoryStream())
      {
        dcFileContent.CopyTo(memoryStream);
        var dcFileArray = memoryStream.ToArray();
        var dxfUnitsType = new CalibrationFileHelper(dcFileArray).GetDxfUnitsType();
        job.CoordinateSystemFileBytes = dcFileArray;

        Log.LogDebug($"{Method.Info()} Coordinate system file for project {job.Project.ProjectUID} uses {dxfUnitsType} units.");
        _migrationDb.SetProjectDxfUnitsType(Table.Projects, job.Project, dxfUnitsType);
      }

      try
      {
        var dcFilePath = Path.Combine(TemporaryFolder, job.Project.CustomerUID, job.Project.ProjectUID);

        if (!Directory.Exists(dcFilePath))
        {
          Directory.CreateDirectory(dcFilePath);
        }

        var coordinateSystemFilename = job.Project.CoordinateSystemFileName;

        if (string.IsNullOrEmpty(coordinateSystemFilename) ||
            coordinateSystemFilename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
          coordinateSystemFilename = "ProjectCalibrationFile.dc";
        }

        var tempFileName = Path.Combine(dcFilePath, coordinateSystemFilename);

        using (var fileStream = File.Create(tempFileName))
        {
          dcFileContent.Seek(0, SeekOrigin.Begin);
          dcFileContent.CopyTo(fileStream);

          Log.LogInformation($"{Method.Info()} Completed writing DC file '{tempFileName}' for project {job.Project.ProjectUID}");

          return true;
        }
      }
      catch (Exception exception)
      {
        Log.LogError(exception, $"{Method.Info()} Error writing DC file for project {job.Project.ProjectUID}");
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
        _migrationDb.SetMigrationState(Table.Files, file, MigrationState.InProgress);

        if (fileContents == null)
        {
          var message = $"Failed to fetch file '{file.Name}' ({file.LegacyFileId}), not found";
          _migrationDb.SetMigrationState(Table.Files, file, MigrationState.FileNotFound);
          _migrationDb.WriteWarning(file.ProjectUid, message);

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

            _migrationDb.SetFileSize(Table.Files, file, tempFile.Length);
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

        _migrationDb.SetMigrationState(Table.Files, file, MigrationState.Completed);
        _migrationDb.IncrementProjectFilesUploaded(Table.Projects, project, 1);
      }
      else
      {
        _migrationDb.SetMigrationState(Table.Files, file, MigrationState.Skipped);
        Log.LogDebug($"{Method.Info()} Skipped uploading file {file.ImportedFileUid}");
      }

      Log.LogInformation($"{Method.Out()} File {file.ImportedFileUid} update result {result.Code} {result.Message}");

      return (success: true, file: result);
    }
  }
}
