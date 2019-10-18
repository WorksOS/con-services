using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
using VSS.Common.Abstractions.Configuration;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.Utils
{
  public class CalibrationFileAgent : ICalibrationFileAgent
  {
    private readonly ILogger _log;
    private readonly ILiteDbAgent _migrationDb;
    private readonly ICSIBAgent _csibAgent;
    private readonly IFileRepository FileRepo;
    private readonly IWebApiUtils _webApiUtils;

    private readonly string _tempFolder;

    private readonly string _projectApiUrl;
    private readonly string _fileSpaceId;
    private readonly bool _updateProjectCoordinateSystemFile;

    public CalibrationFileAgent(ILoggerFactory loggerFactory, ILiteDbAgent liteDbAgent, IConfigurationStore configStore, IEnvironmentHelper environmentHelper, IFileRepository fileRepo, IWebApiUtils webApiUtils, ICSIBAgent csibAgent)
    {
      _log = loggerFactory.CreateLogger<CalibrationFileAgent>();
      _log.LogInformation(Method.In());

      _migrationDb = liteDbAgent;
      _webApiUtils = webApiUtils;
      _csibAgent = csibAgent;
      FileRepo = fileRepo;

      _projectApiUrl = environmentHelper.GetVariable("PROJECT_API_URL", 1);
      _fileSpaceId = environmentHelper.GetVariable("TCCFILESPACEID", 48);
      _tempFolder = Path.Combine(environmentHelper.GetVariable("TEMPORARY_FOLDER", 2), "DataOceanMigrationTmp");

      _updateProjectCoordinateSystemFile = configStore.GetValueBool("UPDATE_PROJECT_COORDINATE_SYSTEM_FILE", defaultValue: false);
    }

    /// <summary>
    /// Resolves the Distance Unit from .dc file content.
    /// </summary>
    private DxfUnitsType GetDxfUnitsType(byte[] coordSystemFileContent)
    {
      var dxfUnitsType = Encoding.UTF8.GetString(coordSystemFileContent).Substring(41, 1);
      int.TryParse(dxfUnitsType, out var dxfUnits);

      // DXF Unit types in the .dc file are 1 based.
      return (DxfUnitsType)dxfUnits - 1;
    }

    /// <summary>
    /// Resolve the coordinate system file from either TCC or what we know of it from Raptor.
    /// </summary>
    public async Task<bool> ResolveProjectCoordinateSystemFile(MigrationJob job)
    {
      if (string.IsNullOrEmpty(job.Project.CoordinateSystemFileName))
      {
        _log.LogDebug($"Project '{job.Project.ProjectUID}' contains NULL CoordinateSystemFileName field.");
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

      _migrationDb.SetProjectCoordinateSystemDetails(job.Project);

      return true;
    }

    /// <summary>
    /// Failed to download coordinate system file from TCC, try and resolve it using what we know from Raptor.
    /// </summary>
    private async Task<bool> ResolveCoordinateSystemFromRaptor(MigrationJob job)
    {
      _log.LogInformation($"{Method.In()} Resolving project {job.Project.ProjectUID} CSIB from Raptor");
      var logMessage = $"Failed to fetch coordinate system file '{job.Project.CustomerUID}/{job.Project.ProjectUID}/{job.Project.CoordinateSystemFileName}' from TCC.";

      _migrationDb.WriteWarning(job.Project.ProjectUID, logMessage);
      _log.LogWarning(logMessage);

      // Get the the CSIB for the project from Raptor.
      var csibResponse = await _csibAgent.GetCSIBForProject(job.Project);
      var csib = csibResponse.CSIB;

      if (csibResponse.Code != 0)
      {
        var errorMessage = $"Failed to resolve CSIB for project: {job.Project.ProjectUID}";
        _migrationDb.SetResolveCSIBMessage(Table.Projects, job.Project.ProjectUID, csib);
        _migrationDb.WriteError(job.Project.ProjectUID, errorMessage);

        _log.LogError(errorMessage);

        return false;
      }

      _migrationDb.SetProjectCSIB(Table.Projects, job.Project.ProjectUID, csib);

      var coordSysInfo = await _csibAgent.GetCoordSysInfoFromCSIB64(job.Project, csib);
      var dcFileContent = await _csibAgent.GetCalibrationFileForCoordSysId(job.Project, coordSysInfo["coordinateSystem"]["id"].ToString());

      var coordSystemFileContent = Encoding.UTF8.GetBytes(dcFileContent);
      bool result;

      using (var stream = new MemoryStream(coordSystemFileContent))
      {
        result = SaveDCFileToDisk(job, stream);
      }

      if (result)
      {
        _log.LogInformation("Successfully resolved coordinate system information from Raptor");
        return await UpdateProjectCoordinateSystemInfo(job);
      }

      _log.LogError("Failed to resolve coordinate system information from Raptor");
      return false;
    }

    /// <summary>
    /// Update the Project with new coordinate system info file.
    /// </summary>
    private async Task<bool> UpdateProjectCoordinateSystemInfo(MigrationJob job)
    {
      if (_updateProjectCoordinateSystemFile)
      {
        var updateProjectResult = await _webApiUtils.UpdateProjectCoordinateSystemFile(_projectApiUrl, job);

        if (updateProjectResult.Code != 0)
        {
          _log.LogError($"{Method.Info()} Error: Unable to update project coordinate system file; '{updateProjectResult.Message}' ({updateProjectResult.Code})");

          return false;
        }

        _log.LogInformation($"{Method.Info()} Update result '{updateProjectResult.Message}' ({updateProjectResult.Code})");

        return true;
      }

      _log.LogDebug($"{Method.Info("DEBUG")} Skipping updating project coordinate system file step");

      return false;
    }
    
    /// <summary>
    /// Downloads the coordinate system file for a given project.
    /// </summary>
    private async Task<bool> DownloadCoordinateSystemFileFromTCC(MigrationJob job)
    {
      _log.LogInformation($"{Method.In()} Downloading coord system file '{job.Project.CoordinateSystemFileName}' from TCC");

      Stream memStream = null;

      try
      {
        memStream = await FileRepo.GetFile(_fileSpaceId, $"/{job.Project.CustomerUID}/{job.Project.ProjectUID}/{job.Project.CoordinateSystemFileName}");

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
      _log.LogDebug($"{Method.In()} Writing coordinate system file for project {job.Project.ProjectUID}");

      if (dcFileContent == null || dcFileContent.Length <= 0)
      {
        _log.LogDebug($"{Method.Info()} Error: Null stream provided for dcFileContent for project '{job.Project.ProjectUID}'");
        return false;
      }

      using (var memoryStream = new MemoryStream())
      {
        dcFileContent.CopyTo(memoryStream);
        var dcFileArray = memoryStream.ToArray();
        var dxfUnitsType = GetDxfUnitsType(dcFileArray);
        job.CoordinateSystemFileBytes = dcFileArray;

        _log.LogDebug($"{Method.Info()} Coordinate system file for project {job.Project.ProjectUID} uses {dxfUnitsType} units.");
        _migrationDb.SetProjectDxfUnitsType(Table.Projects, job.Project, dxfUnitsType);
      }

      try
      {
        var dcFilePath = Path.Combine(_tempFolder, job.Project.CustomerUID, job.Project.ProjectUID);

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

          _log.LogInformation($"{Method.Info()} Completed writing DC file '{tempFileName}' for project {job.Project.ProjectUID}");

          return true;
        }
      }
      catch (Exception exception)
      {
        _log.LogError(exception, $"{Method.Info()} Error writing DC file for project {job.Project.ProjectUID}");
      }

      return false;
    }
  }
}
