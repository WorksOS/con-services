using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TCCToDataOcean.DatabaseAgent;
using TCCToDataOcean.Interfaces;
using TCCToDataOcean.Models;
using TCCToDataOcean.Types;
using VSS.Common.Abstractions.Configuration;
using VSS.DataOcean.Client.Models;
using VSS.TCCFileAccess;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace TCCToDataOcean.Utils
{
  public class CalibrationFileAgent : ICalibrationFileAgent
  {
    private const string COORDINATE_SYSTEM_FILES_KEY = "coordfiles";
    private readonly ILogger _log;
    private readonly ILiteDbAgent _migrationDb;
    private readonly ICSIBAgent _csibAgent;
    private readonly IDataOceanAgent _dataOceanAgent;
    private readonly IFileRepository _fileRepo;
    private readonly IWebApiUtils _webApiUtils;
    private readonly IMemoryCache _cache;

    private readonly string _tempFolder;

    private readonly string _projectApiUrl;
    private readonly string _fileSpaceId;
    private readonly bool _updateProjectCoordinateSystemFile;

    public CalibrationFileAgent(ILoggerFactory loggerFactory, ILiteDbAgent liteDbAgent, IConfigurationStore configStore, IEnvironmentHelper environmentHelper, IFileRepository fileRepo, IWebApiUtils webApiUtils, ICSIBAgent csibAgent, IDataOceanAgent dataOceanAgent, IMemoryCache memoryCache)
    {
      _log = loggerFactory.CreateLogger<CalibrationFileAgent>();
      _log.LogInformation(Method.In());

      _migrationDb = liteDbAgent;
      _webApiUtils = webApiUtils;
      _csibAgent = csibAgent;
      _dataOceanAgent = dataOceanAgent;
      _fileRepo = fileRepo;
      _cache = memoryCache;
      _cache.Set(COORDINATE_SYSTEM_FILES_KEY, new List<string>());

      _projectApiUrl = environmentHelper.GetVariable("PROJECT_API_URL", 1);
      _fileSpaceId = environmentHelper.GetVariable("TCCFILESPACEID", 48);
      _tempFolder = Path.Combine(
        environmentHelper.GetVariable("TEMPORARY_FOLDER", 2),
        "DataOceanMigrationTmp",
        environmentHelper.GetVariable("MIGRATION_ENVIRONMENT", 2));

      _updateProjectCoordinateSystemFile = configStore.GetValueBool("UPDATE_PROJECT_COORDINATE_SYSTEM_FILE", defaultValue: false);
    }

    /// <summary>
    /// Resolves the Distance Unit from a coordinate system file.
    /// </summary>
    private static DxfUnitsType GetDxfUnitsType(byte[] coordSystemFileContent)
    {
      var dxfUnitsType = Encoding.UTF8.GetString(coordSystemFileContent).Substring(41, 1);
      int.TryParse(dxfUnitsType, out var dxfUnits);

      // DXF Unit types in the .dc file are 1 based.
      return (DxfUnitsType)dxfUnits - 1;
    }

    /// <summary>
    /// Resolves the Projection Type Code from a coordinate system file.
    /// </summary>
    private static (string id, string name) GetProjectionTypeCode(MigrationJob job, byte[] dcFileArray)
    {
      const string projectionKey = "64";
      var fs = new MemoryStream(dcFileArray);

      using (var sr = new StreamReader(fs, Encoding.UTF8))
      {
        string line;

        while ((line = sr.ReadLine()) != null)
        {
          if (!line.StartsWith(projectionKey)) { continue; }

          var projectionTypeCode = line[4].ToString();

          return (projectionTypeCode, Projection.GetProjectionName(projectionTypeCode));
        }
      }

      throw new Exception($"Calibration file for project {job.Project.ProjectUID} doesn't contain Projection data");
    }

    /// <summary>
    /// Resolve the coordinate system file from either TCC or what we know of it from Raptor.
    /// </summary>
    public async Task<bool> ResolveProjectCoordinateSystemFile(MigrationJob job)
    {
      if (await ResolveCoordinateSystemFromDataOcean(job)) { return true; }

      if (string.IsNullOrEmpty(job.Project.CoordinateSystemFileName))
      {
        _log.LogDebug($"Project '{job.Project.ProjectUID}' contains NULL CoordinateSystemFileName field.");

        if (!await ResolveCoordinateSystemFromRaptor(job)) { return false; }
      }
      else
      {
        var fileDownloadResult = await DownloadCoordinateSystemFileFromTCC(job);

        if (!fileDownloadResult)
        {
          if (!await ResolveCoordinateSystemFromRaptor(job)) { return false; }
        }
      }

      _log.LogInformation("Successfully resolved coordinate system information from Raptor");

      // Push the file information back to the Project service and by proxy DataOcean.
      var projectUpdateResult = await UpdateProjectCoordinateSystemInfo(job);

      if (!projectUpdateResult)
      {
        _log.LogError($"Unable to update Project database with new coordinate system file data for project {job.Project.ProjectUID}.");
        return false;
      }

      _migrationDb.SetProjectCoordinateSystemDetails(job.Project);

      // Wait for the coordinate system file to be pushed to DataOcean, then recheck it's present.
      await Task.Delay(2000);
      return await ResolveCoordinateSystemFromDataOcean(job);
    }

    private async Task<bool> ResolveCoordinateSystemFromDataOcean(MigrationJob job)
    {
      _log.LogInformation($"{Method.In()} Resolving project {job.Project.ProjectUID} coordination file from DataOcean.");

      _cache.TryGetValue(COORDINATE_SYSTEM_FILES_KEY, out List<string> fileList);

      if (fileList.Contains(job.Project.ProjectUID))
      {
        _log.LogDebug($"Resolving DataOcean calibration file from cache for project {job.Project.ProjectUID}.");
        return true;
      }

      // Resolve the customer id from DataOcean using the .Name, our CustomerUid value.
      if (!_cache.TryGetValue(job.Project.CustomerUID, out DataOceanDirectory customer))
      {
        var directoryResponse = await _dataOceanAgent.GetCustomerByName(job.Project.CustomerUID);

        if (!directoryResponse.Directories.Any())
        {
          _log.LogWarning($"Unable to resolve DataOcean customer {job.Project.CustomerUID}, project {job.Project.ProjectUID}");
          return false;
        }

        _cache.Set(job.Project.CustomerUID, customer);
        customer = directoryResponse.Directories[0];
      }

      // Resolve the project folder's id from DataOcean using the .Name, our ProjectUid value.
      if (!_cache.TryGetValue(job.Project.ProjectUID, out DataOceanDirectory project))
      {
        var dirProjectResponse = await _dataOceanAgent.GetProjectForCustomerById(customer.Id.ToString(), job.Project.ProjectUID);

        if (!dirProjectResponse.Directories.Any())
        {
          _log.LogError($"Unable to resolve DataOcean project folder {job.Project.ProjectUID} for customer {job.Project.CustomerUID}.");
          return false;
        }

        _cache.Set(job.Project.ProjectUID, project);
        project = dirProjectResponse.Directories[0];
      }

      // Iterate all files, 25 at a time, until we find a .DC or .CAL file.
      long metaKeyOffset = -1;

      do
      {
        var dirFilesResponse = await _dataOceanAgent.GetFilesForProjectById(project.Id.ToString(), metaKeyOffset);

        if (dirFilesResponse.Files.Length == 0) { return false; }

        foreach (var file in dirFilesResponse.Files)
        {
          if (!file.Path.EndsWith(job.Project.ProjectUID + ".dc") && !file.Path.EndsWith(job.Project.ProjectUID + ".cal"))
          {
            continue;
          }

          fileList.Add(job.Project.ProjectUID);

          return true;
        }

        if (dirFilesResponse.Files.Length < 25) { return false; }

        // Setup the Key_Offset for the next DataOcean request; moving us to the next page of results.
        metaKeyOffset = dirFilesResponse.Meta.Key_Offset;

      } while (true);
    }

    private async Task<bool> ResolveCoordinateSystemFromRaptor(MigrationJob job)
    {
      _log.LogInformation($"{Method.In()} Resolving project {job.Project.ProjectUID} CSIB from Raptor");
      var logMessage = $"Failed to fetch coordinate system file '{job.Project.CustomerUID}/{job.Project.ProjectUID}/{job.Project.CoordinateSystemFileName}' from TCC.";

      _migrationDb.Insert(new MigrationMessage(job.Project.ProjectUID, logMessage), Table.Warnings);
      _log.LogWarning(logMessage);

      // Get the the CSIB for the project from Raptor.
      var csibResponse = await _csibAgent.GetCSIBForProject(job.Project);
      var csib = csibResponse.CSIB;

      if (csibResponse.Code != 0)
      {
        const string errorMessage = "Failed to resolve CSIB from Raptor";
        _migrationDb.SetResolveCSIBMessage(Table.Projects, job.Project.ProjectUID, csib);
        _migrationDb.Insert(new MigrationMessage(job.Project.ProjectUID, errorMessage), Table.Errors);

        _log.LogError(errorMessage);

        return false;
      }

      _migrationDb.SetProjectCSIB(Table.Projects, job.Project.ProjectUID, csib);

      var coordSysInfo = await _csibAgent.GetCoordSysInfoFromCSIB64(job.Project, csib);
      var dcFileContent = await _csibAgent.GetCalibrationFileForCoordSysId(job.Project, coordSysInfo["coordinateSystem"]["id"].ToString());

      var coordSystemFileContent = Encoding.UTF8.GetBytes(dcFileContent);

      using (var stream = new MemoryStream(coordSystemFileContent))
      {
        if (SaveDCFileToDisk(job, stream)) { return true; }
      }

      _log.LogError("Failed to resolve coordinate system information from Raptor");
      return false;
    }

    /// <summary>
    /// Update the Project with new coordinate system info file.
    /// </summary>
    private async Task<bool> UpdateProjectCoordinateSystemInfo(MigrationJob job)
    {
      if (!_updateProjectCoordinateSystemFile)
      {
        _log.LogDebug($"{Method.Info("DEBUG")} Skipping updating project coordinate system file step");

        return true;
      }

      var updateProjectResult = await _webApiUtils.UpdateProjectCoordinateSystemFile(_projectApiUrl, job);

      if (updateProjectResult.Code != 0)
      {
        _log.LogError($"{Method.Info()} Error: Unable to update project coordinate system file; '{updateProjectResult.Message}' ({updateProjectResult.Code})");

        return false;
      }

      _log.LogInformation($"{Method.Info()} Update result '{updateProjectResult.Message}' ({updateProjectResult.Code})");

      return true;
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
        memStream = await DownloadFile(job, job.Project.CoordinateSystemFileName);

        if (memStream == null) { memStream = await DownloadFile(job, job.Project.LegacyCustomerID + ".dc"); }
        if (memStream == null) { memStream = await DownloadFile(job, job.Project.LegacyProjectID + ".dc"); }
        if (memStream == null) { return false; }

        return SaveDCFileToDisk(job, memStream);
      }
      catch (Exception exception)
      {
        _log.LogError(exception, $"Unexpected error processing calibration file for project {job.Project.ProjectUID}");

        return false;
      }
      finally
      {
        memStream?.Dispose();
      }
    }

    private async Task<Stream> DownloadFile(MigrationJob job, string filename)
    {
      var memStream = await _fileRepo.GetFile(_fileSpaceId, $"/{job.Project.CustomerUID}/{job.Project.ProjectUID}/{filename}");

      if (memStream == null)
      {
        _log.LogWarning($"{Method.Info()} Unable to download '{filename}' from TCC, unexpected error.");
      }

      return memStream;
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
        var projectionType = GetProjectionTypeCode(job, dcFileArray);

        var coordinateSystemInfo = new MigrationCoordinateSystemInfo
        {
          ProjectUid = job.Project.ProjectUID,
          DxfUnitsType = GetDxfUnitsType(dcFileArray),
          ProjectionTypeCode = projectionType.id,
          ProjectionName = projectionType.name
        };

        job.CoordinateSystemFileBytes = dcFileArray;

        _migrationDb.Update(
          job.Project.LegacyProjectID, (MigrationProject x) =>
          {
            x.CoordinateSystemInfo = coordinateSystemInfo;
            x.CalibrationFile = new CalibrationFile { Content = Encoding.Default.GetString(dcFileArray) };
          },
          tableName: Table.Projects);

        _migrationDb.Insert(coordinateSystemInfo);
      }

      try
      {
        var dcFilePath = Path.Combine(_tempFolder, job.Project.CustomerUID, job.Project.ProjectUID);

        Directory.CreateDirectory(dcFilePath);

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
