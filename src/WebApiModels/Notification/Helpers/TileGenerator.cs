using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using TCCFileAccess;
using VSS.GenericConfiguration;
using VSS.Raptor.Service.Common.Models;
using VSS.Raptor.Service.WebApiModels.Notification.Models;
using WebApiModels.Interfaces;

namespace WebApiModels.Notification.Helpers
{
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
    public TileGenerator(ILoggerFactory logger, IFileRepository fileRepo, IConfigurationStore configStore)
    {
      this.log = logger.CreateLogger<TileGenerator>();
      this.fileRepo = fileRepo;
      this.configStore = configStore;
    }

    public void CreateDxfTiles(long projectId, FileDescriptor fileDescr, bool regenerate)
    {
      //TODO: How are we going to handle not repeating tile generation
      //In CG this was min/max/lat/lng in database as a flag

      ImportedFileTypeEnum fileType = FileUtils.GetFileType(fileDescr.fileName);
      var suffix = FileUtils.GeneratedFileSuffix(fileType);

 
      if (regenerate)
      {
        // Delete previously generated .DXF tiles
        DeleteDxfTiles(projectId, fileDescr, suffix);
      }

      int minZoom;
      int maxZoom;
      CalculateTileZoomRange(fileDescr, out minZoom, out maxZoom);

      //Now do the rendering
      string tilePath = TilePath(fileDescr, suffix);

      // Generate .DXF file tiles and place the tiles in TCC...
      if (regenerate || !fileRepo.FolderExists(fileDescr.filespaceId, tilePath).Result)
      {
        for (int zoomLevel = minZoom; zoomLevel <= maxZoom; zoomLevel++)
          GenerateDxfTiles(fileDescr, suffix, zoomLevel);
      }
      else
      {
        for (int zoomLevel = minZoom; zoomLevel <= maxZoom; zoomLevel++)
        {
          string zoomFolder = ZoomFolder(zoomLevel);
          string zoomPath = string.Format("{0}/{1}", tilePath, zoomFolder);

          if (!fileRepo.FolderExists(fileDescr.filespaceId, zoomPath).Result)
            GenerateDxfTiles(fileDescr, suffix, zoomLevel);
        }
      }
    }


    private void CalculateTileZoomRange(FileDescriptor fileDescr, out int minZoom, out int maxZoom)
    {
      log.LogDebug("CalculateTileZoomRange for file {0}/{1}", fileDescr.path, fileDescr.fileName);

      //Get TCC to work out the DXF file extents for us so we can calculate zoom levels to generate tiles for

      const int MIN_PIXELS_SQUARED = 100;
      const int MIN_MAP_ZOOM_LEVEL = 5;
      const int MAX_MAP_ZOOM_LEVEL = 24;

      int waitInterval;
      int maxZoomLevel;
      int maxZoomRange;
      GetRenderConfig(out maxZoomLevel, out maxZoomRange, out waitInterval);

      ImportedFileTypeEnum fileType = FileUtils.GetFileType(fileDescr.fileName);
      var suffix = FileUtils.GeneratedFileSuffix(fileType);
      string generatedName = FileUtils.GeneratedFileName(fileDescr.fileName, suffix, FileUtils.DXF_FILE_EXTENSION);

      log.LogDebug("Before CreateFileJob: {0}", generatedName);
      string jobId = fileRepo.CreateFileJob(fileDescr.filespaceId, generatedName).Result;

      bool done = false;
      string fileId = null;
      while (!done)
      {
        Thread.Sleep(waitInterval);
        log.LogDebug("Before CheckFileJobStatus: JobId={0}", jobId);
        var checkStatusResult = fileRepo.CheckFileJobStatus(jobId).Result;
        log.LogDebug("After CheckFileJobStatus: Status={0}", checkStatusResult == null ? "null" : checkStatusResult.status);
        done = checkStatusResult != null && checkStatusResult.status == "COMPLETED";
        if (done)
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

      log.LogDebug("Before GetFileJobResult: fileId={0}", fileId);
      var getJobResultResult = fileRepo.GetFileJobResult(fileId).Result;

      if (getJobResultResult == null || getJobResultResult.extents == null)
      {
        int minZoomLevel = maxZoomLevel - maxZoomRange;
        log.LogDebug("No extents determined for fileId={0}; setting zoom range to extremes Z{1}-Z{2}", fileId, minZoomLevel, maxZoomLevel);
        minZoom = minZoomLevel;
        maxZoom = maxZoomLevel;
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
        log.LogDebug("{0} has extents {1},{2} - {3},{4}", generatedName, latLng1.Latitude, latLng1.Longitude, latLng2.Latitude, latLng2.Longitude);

        minZoom = 0;
        maxZoom = 0;

        for (int zoomLevel = MIN_MAP_ZOOM_LEVEL; zoomLevel <= MAX_MAP_ZOOM_LEVEL && maxZoom == 0; zoomLevel++)
        {
          int numTiles = 1 << zoomLevel;
          Point pixel1 = WebMercatorProjection.LatLngToPixel(latLng1, numTiles);
          Point pixel2 = WebMercatorProjection.LatLngToPixel(latLng2, numTiles);
          double width = Math.Abs(pixel2.x - pixel1.x);
          double height = Math.Abs(pixel2.y - pixel1.y);
          double squaredLength = width * width + height * height;
          if (minZoom == 0 && squaredLength > MIN_PIXELS_SQUARED)
          {
            minZoom = zoomLevel;
            maxZoom = Math.Min(maxZoomLevel, zoomLevel + maxZoomRange);
          }
        }

        log.LogInformation("DXF TILE GENERATION: file={0}, zoom range=Z{1}-Z{2}", generatedName, minZoom, maxZoom);
      }
  
    }

    private void GetRenderConfig(out int maxZoomLevel, out int maxZoomRange, out int waitInterval)
    {
      maxZoomLevel = configStore.GetValueInt("TILE_RENDER_MAX_ZOOM_LEVEL");
      maxZoomRange = configStore.GetValueInt("TILE_RENDER_MAX_ZOOM_RANGE");
      waitInterval = configStore.GetValueInt("TILE_RENDER_WAIT_INTERVAL");

      if (maxZoomLevel == 0 || maxZoomRange == 0 || waitInterval == 0)
      {
        var errorString = "Your application is missing an environment variable TILE_RENDER_MAX_ZOOM_LEVEL or TILE_RENDER_MAX_ZOOM_RANGE or TILE_RENDER_WAIT_INTERVAL";
        log.LogError(errorString);
        throw new InvalidOperationException(errorString);
      }
    }

    /// <summary>
    /// Delete the DXF tiles associated with the specified DXF file
    /// </summary>
    /// <param name="projectId">The id of the project to which the DXF file belongs</param>
    /// <param name="fileDescr">THe DXF file name</param>
    /// <param name="suffix">The suffix applied to the file name to get the generated file name</param>
    public void DeleteDxfTiles(long projectId, FileDescriptor fileDescr, string suffix)
    {
      string tilePath = TilePath(fileDescr, suffix);
      log.LogDebug("Deleting DXF tiles {0}", tilePath);
      if (fileRepo.FolderExists(fileDescr.filespaceId, tilePath).Result)
      {
        bool success = fileRepo.DeleteFolder(fileDescr.filespaceId, tilePath).Result;
        if (!success)
        {
          log.LogWarning("Failed to delete tiles {0} for project {1}", tilePath, projectId);
          //TODO: Do we want to throw an exception here?
        }
      }
    }

    private string TilePath(FileDescriptor fileDescr, string suffix)
    {
      string generatedName = FileUtils.GeneratedFileName(fileDescr.fileName, suffix, FileUtils.DXF_FILE_EXTENSION);
      string tileFolder = FileUtils.TilesFolderWithSuffix(generatedName);
      return string.Format("{0}/{1}", fileDescr.path, tileFolder);
    }

    private string ZoomFolder(int zoomLevel)
    {
      return string.Format("Z{0}", zoomLevel);
    }

    private void GenerateDxfTiles(FileDescriptor fileDescr, string suffix, int zoomLevel, int waitInterval = -1)
    {
      //Check if tiles already exist
      string tilePath = TilePath(fileDescr, suffix);
      if (!fileRepo.FolderExists(fileDescr.filespaceId, tilePath).Result)
      {
        var success = fileRepo.MakeFolder(fileDescr.filespaceId, tilePath).Result;
      }
      string zoomPath = string.Format("{0}/{1}", tilePath, ZoomFolder(zoomLevel));
      if (!fileRepo.FolderExists(fileDescr.filespaceId, zoomPath).Result)
      { 
        //Generate tiles for this file at this zoom level.
        //Note: This could be made asynchronous to speed it up if required
        string dstPath = string.Format("{0}.html", zoomPath);
        string generatedName = FileUtils.GeneratedFileName(fileDescr.fileName, suffix, FileUtils.DXF_FILE_EXTENSION);

        //TODO: clean up suffix, generated name stuff everywhere

        log.LogDebug("Before ExportToWebFormat: srcPath={0}, dstPath={1}, zoomLevel={2}", generatedName, dstPath, zoomLevel);
        var exportJobId = fileRepo.ExportToWebFormat(fileDescr.filespaceId, generatedName, fileDescr.filespaceId, dstPath, zoomLevel).Result;

        if (waitInterval > -1)
        {
          bool done = false;

          while (!done)
          {
            Thread.Sleep(waitInterval);
            log.LogDebug("Before CheckExportJob: JobId={0}", exportJobId);
            var jobStatus = fileRepo.CheckExportJob(exportJobId).Result;
            done = jobStatus == "COMPLETED";
          }
        }
     
      }
    }

  }
}
