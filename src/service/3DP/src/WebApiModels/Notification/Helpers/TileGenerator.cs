using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.WebApi.Models.MapHandling;
using VSS.TCCFileAccess;
using Point = VSS.MasterData.Models.Models.Point;

namespace VSS.Productivity3D.WebApi.Models.Notification.Helpers
{
  /// <summary>
  /// Generates tiles for a DXF file using Global Mapper in TCC.
  /// </summary>
  [Obsolete("Migrated to the Scheduler")]
  public class TileGenerator : ITileGenerator
  {
    /// <summary>
    /// Logger for logging
    /// </summary>
    private readonly ILogger log;
  
    /// <summary>
    /// Used to talk to TCC
    /// </summary>
    private readonly IFileRepository fileRepo;

    /// <summary>
    /// Where to get environment variables, connection string etc. from
    /// </summary>
    private IConfigurationStore configStore;

    /// <summary>
    /// Constructor with injection
    /// </summary>
    /// <param name="logger">Logger</param>
    /// <param name="fileRepo">Imported file repository</param>
    /// <param name="configStore">Configuration store for environment variables</param>
    public TileGenerator(ILoggerFactory logger, IFileRepository fileRepo, IConfigurationStore configStore)
    {
      this.log = logger.CreateLogger<TileGenerator>();
      this.fileRepo = fileRepo;
      this.configStore = configStore;
    }

    /// <summary>
    /// Creates DXF tiles.
    /// </summary>
    /// <param name="projectId">The project ID to which the file belongs</param>
    /// <param name="fileDescr">The file for which to create the tiles. If it is an alignment file then a DXF file of the center line has been created
    /// and the tiles are generated for this. If it is a design file then a DXF file of the surface boundary has been created and the tiles are
    /// generated for this.</param>
    /// <param name="suffix">The suffix to apply to a file name for the generated associated DXF file</param>
    /// <param name="zoomResult">The zoom level range for the file calculated from the DXF extents</param>
    /// <param name="regenerate">True indicates tiles should always be generated even if present</param>
    public async Task<bool> CreateDxfTiles(long projectId, FileDescriptor fileDescr, string suffix, ZoomRangeResult zoomResult, bool regenerate)
    {
      //TODO: How are we going to handle not repeating tile generation
      //In CG this was min/max/lat/lng in database as a flag
      bool success = true;

      string generatedName = FileUtils.GeneratedFileName(fileDescr.fileName, suffix, FileUtils.DXF_FILE_EXTENSION);
      if (regenerate)
      {
        // Delete previously generated .DXF tiles
        success = await DeleteDxfTiles(projectId, generatedName, fileDescr);
        //Do we care if this fails?
      }

      var fullGeneratedName = string.Format("{0}/{1}", fileDescr.path, generatedName);
      success = zoomResult.success;
      if (success)
      {
        //Now do the rendering
        string tilePath = FileUtils.TilePath(fileDescr.path, generatedName);
        var timeout = GetJobTimeoutConfig();

        // Generate .DXF file tiles and place the tiles in TCC...
        if (regenerate || ! await fileRepo.FolderExists(fileDescr.filespaceId, tilePath))
        {
          for (int zoomLevel = zoomResult.minZoom; zoomLevel <= zoomResult.maxZoom; zoomLevel++)
            success = success && await GenerateDxfTiles(projectId, fullGeneratedName, tilePath, fileDescr.filespaceId, zoomLevel, timeout);
        }
        else
        {
          for (int zoomLevel = zoomResult.minZoom; zoomLevel <= zoomResult.maxZoom; zoomLevel++)
          {
            string zoomPath = FileUtils.ZoomPath(tilePath, zoomLevel);

            if (! await fileRepo.FolderExists(fileDescr.filespaceId, zoomPath))
              success = success && await GenerateDxfTiles(projectId, fullGeneratedName, tilePath, fileDescr.filespaceId, zoomLevel, timeout);
          }
        }
      }
      return success;
    }

    /// <summary>
    /// Calculate the zoom range for which to generate tiles. 
    /// </summary>
    /// <param name="filespaceId">The filespace ID in TCC where the DXF file is located</param>
    /// <param name="fullGeneratedName">The full DXF file name, including path, which is a generated associated name for design and alignment files</param>
    /// <returns>Zoom range and success or failure</returns>
    public async Task<ZoomRangeResult> CalculateTileZoomRange(string filespaceId, string fullGeneratedName)
    {
      var result = new ZoomRangeResult { success = true };

      log.LogDebug("CalculateTileZoomRange for file {0}", fullGeneratedName);

      //Get TCC to work out the DXF file extents for us so we can calculate zoom levels to generate tiles for

      const int MIN_PIXELS_SQUARED = 100;
      const int MIN_MAP_ZOOM_LEVEL = 5;
      const int MAX_MAP_ZOOM_LEVEL = 24;

      var timeout = GetJobTimeoutConfig();

      int waitInterval;
      int maxZoomLevel;
      int maxZoomRange;
      GetRenderConfig(out maxZoomLevel, out maxZoomRange, out waitInterval);

      log.LogDebug("Before CreateFileJob: {0}", fullGeneratedName);
      string jobId = await fileRepo.CreateFileJob(filespaceId, fullGeneratedName);
      if (string.IsNullOrEmpty(jobId))
      {
        result.success = false;
        log.LogDebug("CreateFileJob failed: {0}", fullGeneratedName);
      }
      else
      {
        bool done = false;
        string fileId = null;
        DateTime now = DateTime.Now;
        DateTime endJob = now + timeout;
        while (!done && now <= endJob)
        {
          if (waitInterval > 0) await Task.Delay(waitInterval);
          log.LogDebug("Before CheckFileJobStatus: JobId={0}", jobId);
          var checkStatusResult = await fileRepo.CheckFileJobStatus(jobId);
          log.LogDebug("After CheckFileJobStatus: Status={0}",
            checkStatusResult == null ? "null" : checkStatusResult.status);
          done = checkStatusResult == null || !checkStatusResult.success || checkStatusResult.status == "COMPLETED";
          if (done)
          {
            if (checkStatusResult == null || !checkStatusResult.success)
            {
              result.success = false;
              log.LogDebug("CheckFileJobStatus failed");
            }
            else
            {
              log.LogDebug("Before RenderOutputInfo.Count={0}", checkStatusResult.renderOutputInfo.Count);
              try
              {
                fileId = checkStatusResult.renderOutputInfo[0].fileId;
              }
              catch (Exception e)
              {
                log.LogDebug("checkStatusResult.RenderOutputInfo[0].fileId Failure {0}", e.Message);

              }
              log.LogDebug("After RenderOutputInfo.Count fileID={0}", fileId);
            }
          }
          else
          {
            now = DateTime.Now;
            if (now > endJob)
            {
              result.success = false;
              log.LogDebug("Timeout CheckFileJobStatus: Status={0}",
                checkStatusResult == null ? "null" : checkStatusResult.status);
            }
          }
        }

        if (result.success)
        {
          log.LogDebug("Before GetFileJobResult: fileId={0}", fileId);
          var getJobResultResult = await fileRepo.GetFileJobResult(fileId);

          if (getJobResultResult == null || getJobResultResult.extents == null)
          {
            int minZoomLevel = maxZoomLevel - maxZoomRange;
            log.LogDebug("No extents determined for fileId={0}; setting zoom range to extremes Z{1}-Z{2}", fileId,
              minZoomLevel, maxZoomLevel);
            result.minZoom = minZoomLevel;
            result.maxZoom = maxZoomLevel;
          }
          else
          {
            var extents = getJobResultResult.extents;
            double minLat = Math.Min(extents.latitude1, extents.latitude2);
            double minLon = Math.Min(extents.longitude1, extents.longitude2);
            double maxLat = Math.Max(extents.latitude1, extents.latitude2);
            double maxLon = Math.Max(extents.longitude1, extents.longitude2);
            Point latLng1 = new Point(minLat, minLon);
            Point latLng2 = new Point(maxLat, maxLon);
            log.LogDebug("{0} has extents {1},{2} - {3},{4}", fullGeneratedName, latLng1.Latitude, latLng1.Longitude,
              latLng2.Latitude, latLng2.Longitude);

            result.minZoom = 0;
            result.maxZoom = 0;

            for (int zoomLevel = MIN_MAP_ZOOM_LEVEL;
              zoomLevel <= MAX_MAP_ZOOM_LEVEL && result.maxZoom == 0;
              zoomLevel++)
            {
              int numTiles = TileServiceUtils.NumberOfTiles(zoomLevel);
              Point pixel1 = WebMercatorProjection.LatLngToPixel(latLng1, numTiles);
              Point pixel2 = WebMercatorProjection.LatLngToPixel(latLng2, numTiles);
              double width = Math.Abs(pixel2.x - pixel1.x);
              double height = Math.Abs(pixel2.y - pixel1.y);
              double squaredLength = width * width + height * height;
              if (result.minZoom == 0 && squaredLength > MIN_PIXELS_SQUARED)
              {
                result.minZoom = zoomLevel;
                result.maxZoom = Math.Min(maxZoomLevel, zoomLevel + maxZoomRange);
              }
            }

          }
        }
      }
      log.LogInformation("DXF TILE GENERATION: file={0}, zoom range=Z{1}-Z{2}, success={3}", fullGeneratedName, result.minZoom, result.maxZoom, result.success);
      return result;
    }

    /// <summary>
    /// Get rendering configuration parameters
    /// </summary>
    /// <param name="maxZoomLevel">The maximum allowable zoom level that can be rendered</param>
    /// <param name="maxZoomRange">The maximum zoom range (total zoom levels) that can be rendered</param>
    /// <param name="waitInterval">The time in milliseconds to wait between TCC job status requests</param>
    private void GetRenderConfig(out int maxZoomLevel, out int maxZoomRange, out int waitInterval)
    {
      maxZoomLevel = configStore.GetValueInt("TILE_RENDER_MAX_ZOOM_LEVEL");
      maxZoomRange = configStore.GetValueInt("TILE_RENDER_MAX_ZOOM_RANGE");
      waitInterval = configStore.GetValueInt("TILE_RENDER_WAIT_INTERVAL");

      if (maxZoomLevel <= 0 || maxZoomRange <= 0 || waitInterval <= 0)
      {
        var errorString = "Your application is missing an environment variable TILE_RENDER_MAX_ZOOM_LEVEL or TILE_RENDER_MAX_ZOOM_RANGE or TILE_RENDER_WAIT_INTERVAL";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }
    }

    /// <summary>
    /// Get job timeout configuration parameter
    /// </summary>
    private TimeSpan GetJobTimeoutConfig()
    {
      const string DEFAULT_TIMESPAN_MESSAGE = "Using default 10 secs.";
      const string TIMEOUT_KEY = "TILE_RENDER_JOB_TIMEOUT";

      string timeout = configStore.GetValueString(TIMEOUT_KEY);
      log.LogInformation($"JOB_TIMEOUT: {timeout}");

      if (string.IsNullOrEmpty(timeout))
      {
        log.LogWarning($"Your application is missing an environment variable {TIMEOUT_KEY}. {DEFAULT_TIMESPAN_MESSAGE}");
        timeout = "00:00:10";
      }

      TimeSpan result;
      if (!TimeSpan.TryParse(timeout, out result))
      {
        log.LogWarning($"Invalid timespan for environment variable {TIMEOUT_KEY}. {DEFAULT_TIMESPAN_MESSAGE}");
        result = new TimeSpan(0, 0, 10);
      }
      return result;
    }

    /// <summary>
    /// Delete the DXF tiles associated with the specified DXF file
    /// </summary>
    /// <param name="projectId">The id of the project to which the DXF file belongs</param>
    /// <param name="generatedName">The DXF file name, generated for design and alignment files</param>
    /// <param name="fileDescr">The original file name</param>
    public async Task<bool> DeleteDxfTiles(long projectId, string generatedName, FileDescriptor fileDescr)
    {
      bool success = true;
      string tilePath = FileUtils.TilePath(fileDescr.path, generatedName);
      log.LogDebug("Deleting DXF tiles {0}", tilePath);
      if (await fileRepo.FolderExists(fileDescr.filespaceId, tilePath))
      {
        success = await fileRepo.DeleteFolder(fileDescr.filespaceId, tilePath);
        if (!success)
        {
          log.LogWarning("Failed to delete tiles {0} for project {1}", tilePath, projectId);
          //TODO: Do we want to throw an exception here?
        }
      }
      return success;
    }



    /// <summary>
    /// Generates DXF tiles
    /// </summary>
    /// <param name="projectId">The project ID to which the file belongs</param>
    /// <param name="generatedName">The DXF file name which is generated for an alignment or design file</param>
    /// <param name="tilePath">The full folder name of where the tiles are stored</param>
    /// <param name="filespaceId">The filespace ID in TCC where the DXF file is located</param>
    /// <param name="zoomLevel">The zoom level for which to generate the tiles</param>
    /// <param name="timeout">Maximum time to wait for the completion of the TCC job</param>
    /// <param name="waitInterval">The time in milliseconds to wait between TCC job status requests</param>
    private async Task<bool> GenerateDxfTiles(long projectId, string generatedName, string tilePath, string filespaceId, int zoomLevel, TimeSpan timeout, int waitInterval = -1)
    {
      bool success = true;

      //Check if tiles already exist
      if (! await fileRepo.FolderExists(filespaceId, tilePath))
      {
        success = await fileRepo.MakeFolder(filespaceId, tilePath);
        if (!success)
        {
          log.LogWarning("Failed to create tiles folder {0} for project {1}", tilePath, projectId);
          //TODO: Do we want to throw an exception here?
        }
      }
      if (success)
      {
        string zoomPath = FileUtils.ZoomPath(tilePath, zoomLevel);
        if (!await fileRepo.FolderExists(filespaceId, zoomPath))
        {
          //Generate tiles for this file at this zoom level.
          //Note: This could be made asynchronous to speed it up if required
          string dstPath = string.Format("{0}.html", zoomPath);

          log.LogDebug("Before ExportToWebFormat: srcPath={0}, dstPath={1}, zoomLevel={2}", generatedName, dstPath,
            zoomLevel);
          var exportJobId =
            await fileRepo.ExportToWebFormat(filespaceId, generatedName, filespaceId, dstPath, zoomLevel);
          if (string.IsNullOrEmpty(exportJobId))
          {
            success = false;
            log.LogDebug("ExportToWebFormat failed");
          }
          else
          {
            if (waitInterval > -1)
            {
              bool done = false;
              DateTime now = DateTime.Now;
              DateTime endJob = now + timeout;

              while (!done && now <= endJob)
              {
                if (waitInterval > 0) await Task.Delay(waitInterval);
                log.LogDebug("Before CheckExportJob: JobId={0}", exportJobId);
                var jobStatus = await fileRepo.CheckExportJob(exportJobId);
                now = DateTime.Now;
                var failed = string.IsNullOrEmpty(jobStatus) || now > endJob;
                done = failed || jobStatus == "COMPLETED";
                if (failed)
                {
                  success = false;
                  log.LogDebug($"Timeout CheckExportJob: Status={jobStatus}");
                }
              }
            }
          } 
        }
      }
      return success;
    }
  }
}
