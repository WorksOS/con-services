using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using VSS.ConfigurationStore;
using VSS.MasterData.Models.Models;
using VSS.MasterData.Proxies.Interfaces;
using VSS.Productivity3D.Models.Enums;
using VSS.Tile.Service.Common.Authentication;
using VSS.Tile.Service.Common.Extensions;
using VSS.Tile.Service.Common.Helpers;
using VSS.Tile.Service.Common.ResultHandling;
using VSS.Tile.Service.Common.Services;

namespace VSS.Tile.Service.WebApi.Controllers
{
  public class ThumbnailController : BaseController<ThumbnailController>
  {
    private readonly TileOverlayType[] DEFAULT_PROJECT_THUMBNAIL_OVERLAYS =
    {
      TileOverlayType.BaseMap,
      TileOverlayType.ProjectBoundary
    };

    private readonly TileOverlayType[] DEFAULT_GEOFENCE_THUMBNAIL_OVERLAYS =
    {
      TileOverlayType.BaseMap,
      TileOverlayType.GeofenceBoundary
    };

    private const int DEFAULT_THUMBNAIL_WIDTH = 220;
    private const int DEFAULT_THUMBNAIL_HEIGHT = 182;

    /// <summary>
    /// Default constructor.
    /// </summary>
    public ThumbnailController(IRaptorProxy raptorProxy, IPreferenceProxy prefProxy, IFileListProxy fileListProxy, IMapTileGenerator tileGenerator, 
      IGeofenceProxy geofenceProxy, IMemoryCache cache, IConfigurationStore configStore)
      : base(raptorProxy, prefProxy, fileListProxy, tileGenerator, geofenceProxy, cache, configStore)
    {
    }

    /// <summary>
    /// Gets a project thumbnail image as a raw png.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail/png")]
    [HttpGet]
    public async Task<FileResult> GetProjectThumbnailPng(
      [FromQuery] Guid projectUid, 
      [FromQuery] TileOverlayType[] additionalOverlays)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnailPng)}: {Request.QueryString}");

      var project = await ((TilePrincipal) User).GetProject(projectUid);
      var bbox = GetBoundingBoxFromWKT(project.ProjectGeofenceWKT);

      DisplayMode? mode = null;
      var overlays = DEFAULT_PROJECT_THUMBNAIL_OVERLAYS.ToList();
      if (additionalOverlays?.Length > 0)
      {
        overlays.AddRange(additionalOverlays);
        if (additionalOverlays.Contains(TileOverlayType.ProductionData))
          mode = DisplayMode.Height;
      }

      var tileResult = await GetGeneratedTile(projectUid, null, null, null, null,
        null, overlays.ToArray(), DEFAULT_THUMBNAIL_WIDTH, DEFAULT_THUMBNAIL_HEIGHT, bbox, MapType.MAP,
        mode, null, true);

      // TODO (Aaron) refactor this repeated code
      //Short-circuit cache time for Archived projects
      if (project.IsArchived)
        Response.Headers["Cache-Control"] = "public,max-age=31536000";
      Response.Headers.Add("X-Warning", "false");

      return tileResult;
    }

    /// <summary>
    /// Gets a project thumbnail image as a Base64 encoded string.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail/base64")]
    [HttpGet]
    public async Task<byte[]> GetProjectThumbnailBase64(
      [FromQuery] Guid projectUid,
      [FromQuery] TileOverlayType[] additionalOverlays)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnailBase64)}: {Request.QueryString}");
      var result = await GetProjectThumbnailPng(projectUid, additionalOverlays);
      return GetStreamContents(result);
    }

    /// <summary>
    /// Gets a 3D project thumbnail image as a raw png.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail3d/png")]
    [HttpGet]
    public async Task<FileResult> GetProjectThumbnail3DPng(
      [FromQuery] Guid projectUid)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnail3DPng)}: {Request.QueryString}");

      return await GetProjectThumbnailPng(projectUid, new [] {TileOverlayType.ProductionData});
    }

    /// <summary>
    /// Gets a 3D project thumbnail image as a Base64 encoded string.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail3d/base64")]
    [HttpGet]
    public async Task<byte[]> GetProjectThumbnail3DBase64(
      [FromQuery] Guid projectUid)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnail3DBase64)}: {Request.QueryString}");

      var result = await GetProjectThumbnailPng(projectUid, new[] { TileOverlayType.ProductionData });
      return GetStreamContents(result);
    }

    /// <summary>
    /// Gets a 2D (unified productivity) project thumbnail image as a raw png.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail2d/png")]
    [HttpGet]
    public async Task<FileResult> GetProjectThumbnail2DPng(
      [FromQuery] Guid projectUid)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnail2DPng)}: {Request.QueryString}");

      return await GetProjectThumbnailPng(projectUid, new[] { TileOverlayType.LoadDumpData });
    }

    /// <summary>
    /// Gets a 2D (unified productivity) project thumbnail image as a Base64 encoded string.
    /// </summary>
    [ProjectUidVerifier]
    [Route("api/v1/projectthumbnail2d/base64")]
    [HttpGet]
    public async Task<byte[]> GetProjectThumbnail2DBase64(
      [FromQuery] Guid projectUid)
    {
      Log.LogDebug($"{nameof(GetProjectThumbnail2DBase64)}: {Request.QueryString}");

      var result = await GetProjectThumbnailPng(projectUid, new[] { TileOverlayType.LoadDumpData });
      return GetStreamContents(result);
    }

    /// <summary>
    /// Gets a geofence thumbnail image as a raw png.
    /// </summary>
    [Route("api/v1/geofencethumbnail/png")]
    [HttpGet]
    public async Task<FileResult> GetGeofenceThumbnailPng(
      [FromQuery] Guid geofenceUid)
    {
      Log.LogDebug($"{nameof(GetGeofenceThumbnailPng)}: {Request.QueryString}");

      var geofence = await geofenceProxy.GetGeofenceForCustomer(GetCustomerUid, geofenceUid.ToString(), CustomHeaders);
      if (geofence == null)
      {
        return new FileStreamResult(new MemoryStream(TileServiceUtils.EmptyTile(DEFAULT_THUMBNAIL_WIDTH, DEFAULT_THUMBNAIL_HEIGHT)), "image/png");
      }

      var bbox = GetBoundingBoxFromWKT(geofence.GeometryWKT);

      var tileResult = await GetGeneratedTile(geofence, DEFAULT_GEOFENCE_THUMBNAIL_OVERLAYS, DEFAULT_THUMBNAIL_WIDTH, DEFAULT_THUMBNAIL_HEIGHT, 
        bbox, MapType.MAP, null, true);

      return tileResult;
    }

    /// <summary>
    /// Gets a geofence thumbnail image as a Base64 encoded string.
    /// </summary>
    [Route("api/v1/geofencethumbnail/base64")]
    [HttpGet]
    public async Task<byte[]> GetGeofenceThumbnailBase64(
      [FromQuery] Guid geofenceUid)
    {
      Log.LogDebug($"{nameof(GetGeofenceThumbnailBase64)}: {Request.QueryString}");

      var result = await GetGeofenceThumbnailPng(geofenceUid);
      return GetStreamContents(result);
    }

    /// <summary>
    /// Gets a list of geofence thumbnail images as Base64 encoded strings.
    /// </summary>
    [Route("api/v1/geofencethumbnails/base64")]
    [HttpGet]
    public async Task<MultipleThumbnailsResult> GetGeofenceThumbnailsBase64([FromQuery] Guid[] geofenceUids)
    {
      Log.LogDebug($"{nameof(GetGeofenceThumbnailsBase64)}: {Request.QueryString}");

      object lockObject = new object();

      var thumbnailList = new List<ThumbnailResult>();
      var geofences = await geofenceProxy.GetGeofences(GetCustomerUid, CustomHeaders);
      var selectedGeofences = geofenceUids != null && geofenceUids.Length > 0 ? geofences.Where(g => geofenceUids.Contains(g.GeofenceUID)) : geofences;
      var thumbnailTasks = selectedGeofences.Select(async geofence =>
      {
        var result = await GetGeofenceThumbnailPng(geofence.GeofenceUID);
        lock (lockObject)
        {
          thumbnailList.Add(new ThumbnailResult{Uid = geofence.GeofenceUID, Data = GetStreamContents(result) });
        }
      });

      await Task.WhenAll(thumbnailTasks);

      return new MultipleThumbnailsResult { Thumbnails = thumbnailList };
    }


    /// <summary>
    /// Gets the bounding box of the WKT
    /// </summary>
    private string GetBoundingBoxFromWKT(string wkt)
    {
      var points = wkt.GeometryToPoints().ToList();
      var minLat = points.Min(p => p.Lat).LatRadiansToDegrees();
      var minLng = points.Min(p => p.Lon).LonRadiansToDegrees();
      var maxLat = points.Max(p => p.Lat).LatRadiansToDegrees();
      var maxLng = points.Max(p => p.Lon).LonRadiansToDegrees();
      return $"{minLat},{minLng},{maxLat},{maxLng}";
    }

    /// <summary>
    /// Convert the raw PNG into an array of bytes
    /// </summary>
    private byte[] GetStreamContents(FileResult result)
    {
      using (MemoryStream ms = new MemoryStream())
      {
        (result as FileStreamResult).FileStream.CopyTo(ms);
        return ms.ToArray();
      }
    }

  }
}
